using System;

namespace OmniFrame.Core
{
    /// <summary>
    /// 系统看门狗服务 — 独立心跳线程，监控系统健康状态。
    /// </summary>
    public interface IWatchdogService
    {
        bool IsHealthy { get; }
        event EventHandler<string> WatchdogTriggered;
        event EventHandler<HealthCheckResult> HealthReport;

        void Start(int intervalMs = 1000);
        void Stop();
        void ForceTrigger(string reason);
    }
}
