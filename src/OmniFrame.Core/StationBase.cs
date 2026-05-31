using System;
using System.Collections.Generic;
using System.Threading;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 工站基类
    /// 所有工站实现的抽象基类，提供工站的基本功能和状态管理
        /// </summary>
    public abstract class StationBase : IDisposable
    {
        /// <summary>
        /// 工站名称
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 工站是否运行中
        /// </summary>
        public bool IsRunning { get; protected set; }
        /// <summary>
        /// 工站是否初始化
        /// </summary>
        public bool IsInitialized { get; protected set; }
        /// <summary>
        /// 工站是否处于错误状态
        /// </summary>
        public bool IsInError { get; protected set; }
        /// <summary>
        /// 连续失败次数（用于自动恢复）
        /// </summary>
        public int ConsecutiveFailCount { get; protected set; }
        /// <summary>
        /// 最大自动重试次数
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// 上次错误信息
        /// </summary>
        public string LastErrorMessage { get; protected set; }

        // 信号管理
        private Dictionary<string, bool> _signals;
        private readonly Dictionary<string, ManualResetEvent> _signalEvents = new Dictionary<string, ManualResetEvent>();
        private readonly object _signalLock = new object();

        /// <summary>
        /// 工站状态变更事件
        /// </summary>
        public event EventHandler<StationEventArgs> StationStatusChanged;

        /// <summary>
        /// 工站错误事件
        /// </summary>
        public event EventHandler<StationErrorEventArgs> StationErrorOccurred;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">工站名称</param>
        public StationBase(string name)
        {
            StationName = name;
            IsRunning = false;
            IsInitialized = false;
            IsInError = false;
            ConsecutiveFailCount = 0;
            _signals = new Dictionary<string, bool>();
        }

        /// <summary>
        /// 等待信号
        /// </summary>
        /// <param name="signalName">信号名称</param>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>信号触发返回true，超时返回false</returns>
        protected bool WaitSignal(string signalName, int timeoutMs)
        {
            ManualResetEvent signalEvent;
            lock (_signalLock)
            {
                if (_signals.TryGetValue(signalName, out bool signalValue) && signalValue)
                {
                    _signals[signalName] = false;
                    return true;
                }
                if (!_signalEvents.TryGetValue(signalName, out signalEvent))
                {
                    signalEvent = new ManualResetEvent(false);
                    _signalEvents[signalName] = signalEvent;
                }
                else
                {
                    signalEvent.Reset();
                }
            }
            bool signaled = signalEvent.WaitOne(timeoutMs);
            if (signaled)
            {
                lock (_signalLock)
                {
                    _signals[signalName] = false;
                }
            }
            return signaled;
        }

        /// <summary>
        /// 设置信号
        /// </summary>
        /// <param name="signalName">信号名称</param>
        protected void SetSignal(string signalName)
        {
            lock (_signalLock)
            {
                _signals[signalName] = true;
                if (_signalEvents.TryGetValue(signalName, out var evt))
                {
                    evt.Set();
                }
            }
        }

        /// <summary>
        /// 重置信号
        /// </summary>
        /// <param name="signalName">信号名称</param>
        protected void ResetSignal(string signalName)
        {
            lock (_signalLock)
            {
                _signals[signalName] = false;
                if (_signalEvents.TryGetValue(signalName, out var evt))
                {
                    evt.Reset();
                }
            }
        }

        /// <summary>
        /// 检查信号状态
        /// </summary>
        /// <param name="signalName">信号名称</param>
        /// <returns>信号状态</returns>
        protected bool CheckSignal(string signalName)
        {
            lock (_signalLock)
            {
                _signals.TryGetValue(signalName, out bool signalValue);
                return signalValue;
            }
        }

        /// <summary>
        /// 初始化工站
        /// </summary>
        /// <returns>初始化成功返回true，失败返回false</returns>
        public virtual bool Initialize()
        {
            if (IsInitialized)
            {
                Logger.Warning($"工站 {StationName} 已经初始化");
                return true;
            }

            try
            {
                // 重置信号状态
                lock (_signalLock)
                {
                    _signals.Clear();
                }

                IsInitialized = true;
                IsInError = false;
                ConsecutiveFailCount = 0;
                LastErrorMessage = null;

                Logger.Info($"工站 {StationName} 初始化完成");
                OnStationStatusChanged(new StationEventArgs { StationName = StationName, Status = "Initialized" });
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"工站 {StationName} 初始化失败", ex);
                IsInitialized = false;
                return false;
            }
        }

        /// <summary>
        /// 启动工站
        /// </summary>
        /// <returns>启动成功返回true，失败返回false</returns>
        public virtual bool Start()
        {
            if (!IsInitialized)
            {
                OnStationStatusChanged(new StationEventArgs { StationName = StationName, Status = "Error: Not initialized" });
                return false;
            }

            if (IsInError)
            {
                Logger.Warning($"工站 {StationName} 处于错误状态，请先复位");
                return false;
            }

            IsRunning = true;
            OnStationStatusChanged(new StationEventArgs { StationName = StationName, Status = "Started" });
            return true;
        }

        /// <summary>
        /// 停止工站
        /// </summary>
        /// <returns>停止成功返回true，失败返回false</returns>
        public virtual bool Stop()
        {
            IsRunning = false;
            OnStationStatusChanged(new StationEventArgs { StationName = StationName, Status = "Stopped" });
            return true;
        }

        /// <summary>
        /// 重置工站（清除错误并重新初始化）
        /// </summary>
        public virtual void Reset()
        {
            IsInError = false;
            ConsecutiveFailCount = 0;
            LastErrorMessage = null;
            Stop();
            Initialize();
            OnStationStatusChanged(new StationEventArgs { StationName = StationName, Status = "Reset" });
        }

        /// <summary>
        /// 执行工站功能（带异常恢复的模板方法）
        /// 子类实现 DoExecute() 而不是直接重写 Execute()
        /// </summary>
        /// <returns>执行成功返回true，失败返回false</returns>
        public bool Execute()
        {
            try
            {
                if (IsInError)
                {
                    Logger.Warning($"工站 {StationName} 处于错误状态，无法执行");
                    return false;
                }

                bool result = DoExecute();

                if (result)
                {
                    ConsecutiveFailCount = 0;
                }
                else
                {
                    ConsecutiveFailCount++;
                    HandleExecutionFailure("工站执行返回失败");
                }

                return result;
            }
            catch (Exception ex)
            {
                ConsecutiveFailCount++;
                HandleExecutionFailure($"工站执行异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 子类实现具体的执行逻辑
        /// </summary>
        protected abstract bool DoExecute();

        /// <summary>
        /// 处理执行失败 — 自动重试或进入错误状态
        /// </summary>
        private void HandleExecutionFailure(string errorMessage)
        {
            LastErrorMessage = errorMessage;

            if (ConsecutiveFailCount >= MaxRetryCount)
            {
                IsInError = true;
                IsRunning = false;
                Logger.Error($"工站 {StationName} 连续失败 {ConsecutiveFailCount} 次，已进入错误状态: {errorMessage}");
                OnStationErrorOccurred(new StationErrorEventArgs
                {
                    StationName = StationName,
                    ErrorMessage = errorMessage,
                    FailCount = ConsecutiveFailCount
                });
            }
            else
            {
                Logger.Warning($"工站 {StationName} 执行失败（第 {ConsecutiveFailCount}/{MaxRetryCount} 次）: {errorMessage}");
            }
        }

        /// <summary>
        /// 触发工站状态变更事件
        /// </summary>
        protected virtual void OnStationStatusChanged(StationEventArgs e)
        {
            StationStatusChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 触发工站错误事件
        /// </summary>
        protected virtual void OnStationErrorOccurred(StationErrorEventArgs e)
        {
            StationErrorOccurred?.Invoke(this, e);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_signalLock)
                {
                    _signals?.Clear();
                    foreach (var evt in _signalEvents.Values)
                    {
                        evt?.Dispose();
                    }
                    _signalEvents.Clear();
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 工站事件参数
        /// </summary>
    public class StationEventArgs : EventArgs
    {
        /// <summary>
        /// 工站名称
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 工站状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 事件时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 工站错误事件参数
        /// </summary>
    public class StationErrorEventArgs : EventArgs
    {
        public string StationName { get; set; }
        public string ErrorMessage { get; set; }
        public int FailCount { get; set; }
    }
}
