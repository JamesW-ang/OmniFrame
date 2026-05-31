using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace OmniFrame.Core
{
    public class HealthCheckResult
    {
        public string Status { get; set; } = "Healthy";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public TimeSpan Uptime { get; set; }
        public int ActiveAlarmCount { get; set; }
        public int DeviceCount { get; set; }
        public int ConnectedDeviceCount { get; set; }
        public string SystemState { get; set; }
        public string CurrentUser { get; set; }
        public long MemoryUsageBytes { get; set; }
        public double CpuUsagePercent { get; set; }
        public int ThreadPoolThreads { get; set; }
        public int ThreadPoolPendingWorkItems { get; set; }
        public int ConnectionCount { get; set; }
        public List<ComponentHealth> ComponentStatus { get; set; } = new List<ComponentHealth>();
    }

    public class ComponentHealth
    {
        public string Name { get; set; }
        public bool IsHealthy { get; set; }
        public string Message { get; set; }
    }

    public class HealthCheckService : IHealthCheckService
    {
        private readonly ISystemManager _systemManager;
        private readonly IUserManager _userManager;
        private readonly IAlarmManager _alarmManager;
        private readonly IDeviceManager _deviceManager;
        private readonly DateTime _startTime = DateTime.UtcNow;
        private readonly Stopwatch _cpuStopwatch = Stopwatch.StartNew();
        private TimeSpan _prevCpuTime = TimeSpan.Zero;

        public HealthCheckService(
            ISystemManager systemManager,
            IUserManager userManager,
            IAlarmManager alarmManager,
            IDeviceManager deviceManager)
        {
            _systemManager = systemManager;
            _userManager = userManager;
            _alarmManager = alarmManager;
            _deviceManager = deviceManager;
        }

        public HealthCheckResult CheckHealth()
        {
            var result = new HealthCheckResult();
            var components = new List<ComponentHealth>();

            result.SystemState = _systemManager.State.ToString();
            result.Uptime = _systemManager.IsRunning
                ? _systemManager.RunningTime
                : TimeSpan.Zero;
            result.CurrentUser = _userManager.CurrentUser?.UserName ?? "未登录";
            result.ActiveAlarmCount = _alarmManager.ActiveAlarmCount;

            using (var process = Process.GetCurrentProcess())
            {
                result.MemoryUsageBytes = process.WorkingSet64;
                result.ThreadPoolThreads = process.Threads.Count;

                // CPU usage since last check
                TimeSpan currentCpu = process.TotalProcessorTime;
                double cpuDeltaMs = (currentCpu - _prevCpuTime).TotalMilliseconds;
                double elapsedDeltaMs = _cpuStopwatch.ElapsedMilliseconds;
                if (elapsedDeltaMs > 0)
                {
                    result.CpuUsagePercent = Math.Round(cpuDeltaMs / elapsedDeltaMs * 100.0 /
                        Environment.ProcessorCount, 1);
                }
                _prevCpuTime = currentCpu;
                _cpuStopwatch.Restart();
            }

            ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
            result.ThreadPoolPendingWorkItems = maxWorkerThreads - workerThreads;

            var deviceSummary = _deviceManager.GetStatusSummary();
            result.DeviceCount = deviceSummary.TotalDeviceCount;
            result.ConnectedDeviceCount = deviceSummary.PlcDeviceStatus.Count(s => s.Value)
                                        + deviceSummary.MotionDeviceStatus.Count(s => s.Value);
            result.ConnectionCount = result.ConnectedDeviceCount;

            components.Add(new ComponentHealth
            {
                Name = "SystemManager",
                IsHealthy = _systemManager.State != SystemState.Error
                         && _systemManager.State != SystemState.EmergencyStop,
                Message = $"State: {_systemManager.State}"
            });

            components.Add(new ComponentHealth
            {
                Name = "DeviceManager",
                IsHealthy = !_deviceManager.HasError,
                Message = _deviceManager.HasError
                    ? $"{_deviceManager.GetErrors().Count} error(s)"
                    : "OK"
            });

            components.Add(new ComponentHealth
            {
                Name = "AlarmManager",
                IsHealthy = !_alarmManager.HasCriticalAlarm,
                Message = _alarmManager.HasCriticalAlarm
                    ? "Has critical alarms"
                    : $"{_alarmManager.ActiveAlarmCount} active alarm(s)"
            });

            components.Add(new ComponentHealth
            {
                Name = "UserManager",
                IsHealthy = true,
                Message = _userManager.IsLoggedIn
                    ? $"Logged in as {_userManager.CurrentUser.UserName}"
                    : "No user logged in"
            });

            int busyThreads = maxWorkerThreads - workerThreads;
            components.Add(new ComponentHealth
            {
                Name = "ThreadPool",
                IsHealthy = busyThreads < maxWorkerThreads * 0.8,
                Message = $"{workerThreads}/{maxWorkerThreads} available, {busyThreads} busy"
            });

            result.ComponentStatus = components;

            if (components.Any(c => !c.IsHealthy))
                result.Status = "Degraded";
            if (_systemManager.State == SystemState.Error
                || _systemManager.State == SystemState.EmergencyStop)
                result.Status = "Unhealthy";

            return result;
        }
    }
}
