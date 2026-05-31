using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    public class WatchdogService : IWatchdogService, IDisposable
    {
        private readonly ISystemManager _systemMgr;
        private readonly IHealthCheckService _healthCheck;
        private readonly IAlarmManager _alarmMgr;

        private Timer _timer;
        private int _consecutiveFailures;
        private bool _disposed;

        /// <summary>连续多少次健康检查失败后触发看门狗</summary>
        private int _failureThreshold = 5;

        /// <summary>看门狗检查间隔（毫秒）</summary>
        private int _intervalMs = 1000;

        /// <summary>最近一次健康检查结果</summary>
        private HealthCheckResult _lastHealth;

        /// <summary>看门狗日志路径</summary>
        private string _watchdogLogDir;

        public bool IsHealthy => _consecutiveFailures < _failureThreshold;

        public event EventHandler<string> WatchdogTriggered;
        public event EventHandler<HealthCheckResult> HealthReport;

        public WatchdogService(
            ISystemManager systemMgr,
            IHealthCheckService healthCheck,
            IAlarmManager alarmMgr)
        {
            _systemMgr = systemMgr ?? throw new ArgumentNullException(nameof(systemMgr));
            _healthCheck = healthCheck ?? throw new ArgumentNullException(nameof(healthCheck));
            _alarmMgr = alarmMgr ?? throw new ArgumentNullException(nameof(alarmMgr));

            _watchdogLogDir = Path.Combine(AppContext.BaseDirectory, "Logs", "Watchdog");
            Directory.CreateDirectory(_watchdogLogDir);
        }

        public void Start(int intervalMs = 1000)
        {
            _intervalMs = intervalMs;
            _consecutiveFailures = 0;
            _timer = new Timer(OnTick, null, 0, _intervalMs);
            Logger.Info($"WatchdogService 已启动 (间隔={_intervalMs}ms, 阈值={_failureThreshold})");
        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
            Logger.Info("WatchdogService 已停止");
        }

        /// <summary>外部强制触发看门狗（如 UI 检测到严重故障时调用）</summary>
        public void ForceTrigger(string reason)
        {
            Logger.Error($"WatchdogService 被强制触发: {reason}");
            WriteDiagnosticSnapshot("forced", reason);
            _alarmMgr.AddAlarm("WATCHDOG_FORCED",
                $"看门狗强制触发: {reason}", AlarmLevel.Critical, "WatchdogService");
            TriggerSafeShutdown(reason);
        }

        private void OnTick(object state)
        {
            try
            {
                var health = _healthCheck.CheckHealth();
                _lastHealth = health;

                // 发布健康报告（供 UI/监控订阅）
                HealthReport?.Invoke(this, health);

                bool allHealthy = health.ComponentStatus.All(c => c.IsHealthy);

                if (allHealthy)
                {
                    // 恢复正常 → 重置计数器
                    if (_consecutiveFailures > 0)
                    {
                        Logger.Info($"Watchdog: 系统恢复健康 (连续失败={_consecutiveFailures} → 0)");
                        _consecutiveFailures = 0;
                    }
                    return;
                }

                // 有不健康的组件
                _consecutiveFailures++;
                var unhealthy = health.ComponentStatus
                    .Where(c => !c.IsHealthy)
                    .Select(c => $"{c.Name}: {c.Message}");

                Logger.Warning($"Watchdog: 发现不健康组件 ({_consecutiveFailures}/{_failureThreshold}): " +
                    string.Join("; ", unhealthy));

                // 超过阈值 → 触发安全动作
                if (_consecutiveFailures >= _failureThreshold)
                {
                    string reason = string.Join(", ", unhealthy);
                    Logger.Error($"Watchdog 触发! 连续 {_consecutiveFailures} 次健康检查失败: {reason}");

                    WriteDiagnosticSnapshot("watchdog", reason);
                    _alarmMgr.AddAlarm("WATCHDOG_TRIGGERED",
                        $"看门狗触发: 连续{_failureThreshold}次健康检查失败 — {reason}",
                        AlarmLevel.Critical, "WatchdogService");

                    TriggerSafeShutdown(reason);
                }
            }
            catch (Exception ex)
            {
                // 看门狗自身异常也要记录
                _consecutiveFailures++;
                Logger.Error($"WatchdogService 检查异常 ({_consecutiveFailures}/{_failureThreshold})", ex);

                if (_consecutiveFailures >= _failureThreshold)
                {
                    WriteDiagnosticSnapshot("exception", ex.Message);
                    TriggerSafeShutdown($"看门狗自身异常: {ex.Message}");
                }
            }
        }

        /// <summary>触发安全停机 — 先软停，硬停兜底</summary>
        private void TriggerSafeShutdown(string reason)
        {
            try
            {
                Logger.Error($"═════ 看门狗安全停机 ═════\n原因: {reason}");

                // 先尝试紧急停止（发送停机指令给硬件）
                _systemMgr.EmergencyStop();

                // 停止主循环
                _systemMgr.Stop();

                WatchdogTriggered?.Invoke(this, reason);
            }
            catch (Exception ex)
            {
                Logger.Error($"看门狗安全停机过程异常: {ex.Message}", ex);
            }
        }

        /// <summary>写入诊断快照 — 用于事后分析</summary>
        private void WriteDiagnosticSnapshot(string triggerType, string reason)
        {
            try
            {
                string fileName = $"Watchdog_{DateTime.Now:yyyyMMdd_HHmmss}_{triggerType}.log";
                string filePath = Path.Combine(_watchdogLogDir, fileName);

                using var writer = new StreamWriter(filePath, append: false);
                writer.WriteLine($"=== Watchdog Diagnostic Snapshot ===");
                writer.WriteLine($"Trigger: {triggerType}");
                writer.WriteLine($"Reason: {reason}");
                writer.WriteLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                writer.WriteLine($"SystemState: {_systemMgr.State}");
                writer.WriteLine($"Uptime: {_systemMgr.RunningTime}");

                if (_lastHealth != null)
                {
                    writer.WriteLine($"Memory: {_lastHealth.MemoryUsageBytes / 1024 / 1024} MB");
                    writer.WriteLine($"CPU: {_lastHealth.CpuUsagePercent:F1}%");
                    writer.WriteLine($"Components:");
                    foreach (var c in _lastHealth.ComponentStatus)
                    {
                        writer.WriteLine($"  [{ (c.IsHealthy ? "OK" : "FAIL") }] {c.Name}: {c.Message}");
                    }
                }

                // 进程信息
                using var proc = Process.GetCurrentProcess();
                writer.WriteLine($"Process threads: {proc.Threads.Count}");
                writer.WriteLine($"Working set: {proc.WorkingSet64 / 1024 / 1024} MB");
                writer.WriteLine($"Handle count: {proc.HandleCount}");

                Logger.Info($"诊断快照已写入: {filePath}");
            }
            catch (Exception ex)
            {
                Logger.Error($"写入诊断快照失败", ex);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
        }
    }
}
