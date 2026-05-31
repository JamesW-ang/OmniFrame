using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    public class ReconnectionService : IReconnectionService, IDisposable
    {
        private readonly IDeviceManager _deviceMgr;
        private readonly IAlarmManager _alarmMgr;
        private readonly IHealthCheckService _healthCheck;

        // 每个设备独立的重连状态
        private readonly ConcurrentDictionary<string, ReconnectState> _states = new();
        private CancellationTokenSource _cts;
        private Task _monitorTask;
        private bool _disposed;

        /// <summary>最大重试间隔（上限），超过后不再增长</summary>
        private const int MaxBackoffMs = 15_000;

        /// <summary>重试次数达到此阈值后触发报警</summary>
        private const int AlarmThreshold = 3;

        public bool IsAnyReconnecting => _states.Values.Any(s => s.IsActive);

        public event EventHandler<string> ReconnectStarted;
        public event EventHandler<string> ReconnectSucceeded;
        public event EventHandler<(string Name, string Message)> ReconnectFailed;

        public ReconnectionService(
            IDeviceManager deviceMgr,
            IAlarmManager alarmMgr,
            IHealthCheckService healthCheck)
        {
            _deviceMgr = deviceMgr ?? throw new ArgumentNullException(nameof(deviceMgr));
            _alarmMgr = alarmMgr ?? throw new ArgumentNullException(nameof(alarmMgr));
            _healthCheck = healthCheck ?? throw new ArgumentNullException(nameof(healthCheck));
        }

        public void Start()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();

            // 监听设备管理器自身的事件
            _deviceMgr.DeviceDisconnected += OnDeviceDisconnected;

            _monitorTask = Task.Factory.StartNew(MonitorLoop, TaskCreationOptions.LongRunning);
            Logger.Info("ReconnectionService 已启动");
        }

        public void Stop()
        {
            _deviceMgr.DeviceDisconnected -= OnDeviceDisconnected;
            _cts?.Cancel();
            try { _monitorTask?.Wait(3000); } catch { }
            _monitorTask = null;
            Logger.Info("ReconnectionService 已停止");
        }

        /// <summary>外部手动触发重连（如操作员点击"重连"按钮）</summary>
        public void TriggerReconnect(string deviceName)
        {
            EnqueueReconnect(deviceName);
        }

        #region 事件处理

        private void OnDeviceDisconnected(object sender, string deviceName)
        {
            Logger.Warning($"设备断连: {deviceName} — 自动重连已排队");
            EnqueueReconnect(deviceName);
        }

        private void EnqueueReconnect(string deviceName)
        {
            var state = _states.GetOrAdd(deviceName, _ => new ReconnectState());
            lock (state.Lock)
            {
                if (state.IsActive) return; // 已在重连中
                state.IsActive = true;
                state.RetryCount = 0;
                state.NextAttemptAt = DateTime.UtcNow;
            }
        }

        #endregion

        #region 重连循环

        private void MonitorLoop()
        {
            var token = _cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    DateTime now = DateTime.UtcNow;

                    foreach (var kvp in _states)
                    {
                        string name = kvp.Key;
                        var state = kvp.Value;

                        if (!state.IsActive) continue;

                        lock (state.Lock)
                        {
                            if (!state.IsActive) continue;
                            if (now < state.NextAttemptAt) continue;

                            // 先检查设备是否已恢复（可能被其他组件手动重连）
                            if (IsDeviceConnected(name))
                            {
                                state.IsActive = false;
                                state.RetryCount = 0;
                                Logger.Info($"设备 {name} 已自动恢复连接");
                                ReconnectSucceeded?.Invoke(this, name);
                                continue;
                            }
                        }

                        // 尝试重连
                        DoReconnect(name, state);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("ReconnectionService 监控循环异常", ex);
                }

                token.WaitHandle.WaitOne(500);
            }
        }

        private bool IsDeviceConnected(string deviceName)
        {
            try
            {
                var device = _deviceMgr.GetDevice<IDevice>(deviceName);
                return device != null && device.IsConnected;
            }
            catch
            {
                return false;
            }
        }

        private void DoReconnect(string name, ReconnectState state)
        {
            IDevice device = null;
            try
            {
                device = _deviceMgr.GetDevice<IDevice>(name);
                if (device == null)
                {
                    Logger.Error($"重连失败: 设备 {name} 不存在");
                    FailReconnect(name, state, "设备不存在");
                    return;
                }

                Logger.Info($"正在重连设备 {name} (第 {state.RetryCount + 1} 次)...");
                ReconnectStarted?.Invoke(this, name);

                bool success = device.Connect();

                if (success)
                {
                    Logger.Info($"设备 {name} 重连成功 (尝试次数: {state.RetryCount + 1})");
                    lock (state.Lock)
                    {
                        state.IsActive = false;
                        state.RetryCount = 0;
                    }
                    ReconnectSucceeded?.Invoke(this, name);
                }
                else
                {
                    ScheduleRetry(name, state, "Connect() 返回 false");
                }
            }
            catch (Exception ex)
            {
                ScheduleRetry(name, state, ex.Message);
            }
        }

        private void ScheduleRetry(string name, ReconnectState state, string errorMsg)
        {
            lock (state.Lock)
            {
                state.RetryCount++;
                int delayMs = CalculateBackoff(state.RetryCount);
                state.NextAttemptAt = DateTime.UtcNow.AddMilliseconds(delayMs);

                Logger.Warning($"设备 {name} 重连失败 (第 {state.RetryCount} 次), " +
                    $"{delayMs}ms 后重试: {errorMsg}");

                // 连续失败超过阈值 → 触发报警
                if (state.RetryCount == AlarmThreshold)
                {
                    _alarmMgr.AddAlarm(
                        "RECONNECT_FAILED",
                        $"设备 {name} 连续 {AlarmThreshold} 次重连失败，请检查硬件",
                        AlarmLevel.Error,
                        "ReconnectionService");
                    ReconnectFailed?.Invoke(this, (name, errorMsg));
                }

                // 超出最大重试 → 标记失败（留给人工干预）
                if (state.RetryCount >= 10)
                {
                    _alarmMgr.AddAlarm(
                        "RECONNECT_EXCEEDED",
                        $"设备 {name} 重连失败次数过多 ({state.RetryCount})，停止自动重连",
                        AlarmLevel.Critical,
                        "ReconnectionService");
                    state.IsActive = false;
                    state.RetryCount = 0;
                }
            }
        }

        private void FailReconnect(string name, ReconnectState state, string reason)
        {
            lock (state.Lock)
            {
                state.IsActive = false;
                state.RetryCount = 0;
            }
            _alarmMgr.AddAlarm("RECONNECT_FATAL",
                $"设备 {name} 重连终止: {reason}", AlarmLevel.Error, "ReconnectionService");
            ReconnectFailed?.Invoke(this, (name, reason));
        }

        /// <summary>指数退避: 1s → 2s → 4s → 8s → 15s(上限) → 15s...</summary>
        private static int CalculateBackoff(int retryCount)
        {
            int delay = (int)Math.Min(
                Math.Pow(2, retryCount) * 1000,
                MaxBackoffMs);
            // 加随机抖动 ±20%，避免多个设备同时重连碰撞
            var jitter = new Random().Next((int)(delay * 0.8), (int)(delay * 1.2));
            return jitter;
        }

        #endregion

        private class ReconnectState
        {
            public bool IsActive;
            public int RetryCount;
            public DateTime NextAttemptAt;
            public readonly object Lock = new();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
            _cts?.Dispose();
        }
    }
}
