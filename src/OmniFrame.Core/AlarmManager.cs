using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.DataAccess;

namespace OmniFrame.Core
{
    /// <summary>
    /// 报警级别
        /// </summary>
    public enum AlarmLevel
    {
        Info,       // 信息
        Warning,    // 警告
        Error,      // 错误
        Critical    // 严重
    }

    /// <summary>
    /// 报警信息
        /// </summary>
    public class AlarmInfo
    {
        public int Id { get; set; }
        public string AlarmCode { get; set; }
        public string AlarmMessage { get; set; }
        public AlarmLevel Level { get; set; }
        public string Source { get; set; }
        public DateTime OccurTime { get; set; }
        public bool IsCleared { get; set; }
        public DateTime? ClearTime { get; set; }
        public string ClearUser { get; set; }

        public AlarmInfo()
        {
            OccurTime = DateTime.Now;
            IsCleared = false;
        }
    }

    /// <summary>
    /// 报警管理器 - 管理系统报警的添加、清除、查询
        /// </summary>
    public class AlarmManager : IAlarmManager
    {
        private readonly object _lock = new object();
        private List<AlarmInfo> _activeAlarms;
        private List<AlarmInfo> _alarmHistory;
        private readonly IAlarmDb _alarmDb;
        private Task _monitorTask;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly IAlarmNotification _alarmNotification;

        public bool HasActiveAlarm
        {
            get
            {
                lock (_lock)
                {
                    return _activeAlarms.Count > 0;
                }
            }
        }
        public bool HasCriticalAlarm
        {
            get
            {
                lock (_lock)
                {
                    return _activeAlarms.Any(a => a.Level == AlarmLevel.Critical);
                }
            }
        }
        public int ActiveAlarmCount
        {
            get
            {
                lock (_lock)
                {
                    return _activeAlarms.Count;
                }
            }
        }
        public int TotalAlarmCount
        {
            get
            {
                lock (_lock)
                {
                    return _alarmHistory.Count;
                }
            }
        }

        public event EventHandler<AlarmInfo> AlarmOccurred;
        public event EventHandler<AlarmInfo> AlarmCleared;
        public event EventHandler<AlarmLevel> CriticalAlarmOccurred;

        public AlarmManager(IAlarmNotification alarmNotification, IAlarmDb alarmDb)
        {
            _alarmNotification = alarmNotification;
            _alarmDb = alarmDb;
            _activeAlarms = new List<AlarmInfo>();
            _alarmHistory = new List<AlarmInfo>();
        }

        public bool Initialize()
        {
            try
            {
                Logger.Info("初始化报警管理器...");

                // 初始化数据库
                string dbPath = "Data/Alarm.db";
                if (!_alarmDb.Open(dbPath))
                {
                    Logger.Error("报警数据库打开失败");
                    return false;
                }

                // 启动监控线程
                _cancellationTokenSource = new CancellationTokenSource();
                _monitorTask = Task.Factory.StartNew(MonitorLoop, TaskCreationOptions.LongRunning);

                Logger.Info("报警管理器初始化完成");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("报警管理器初始化失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 添加报警
        /// </summary>
        public AlarmInfo AddAlarm(string code, string message, AlarmLevel level, string source)
        {
            lock (_lock)
            {
                // 检查是否已存在相同未清除的报警
                var existingAlarm = _activeAlarms.FirstOrDefault(
                    a => a.AlarmCode == code && !a.IsCleared);

                if (existingAlarm != null)
                {
                    return existingAlarm;
                }

                var alarm = new AlarmInfo
                {
                    AlarmCode = code,
                    AlarmMessage = message,
                    Level = level,
                    Source = source,
                    OccurTime = DateTime.Now
                };

                _activeAlarms.Add(alarm);
                _alarmHistory.Add(alarm);

                // 保存到数据库
                try
                {
                    _alarmDb?.AddAlarm(new AlarmRecord
                    {
                        AlarmTime = alarm.OccurTime,
                        AlarmCode = alarm.AlarmCode,
                        AlarmMessage = alarm.AlarmMessage,
                        AlarmLevel = alarm.Level.ToString(),
                        Source = alarm.Source,
                        IsCleared = false
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error("保存报警到数据库失败", ex);
                }

                // 触发事件
                AlarmOccurred?.Invoke(this, alarm);

                if (level == AlarmLevel.Critical)
                {
                    CriticalAlarmOccurred?.Invoke(this, level);
                }

                // 发送告警通知
                Task.Run(async () =>
                {
                    await _alarmNotification.SendNotification(alarm);
                }).ContinueWith(t =>
                {
                    if (t.IsFaulted) Logger.Error("告警通知发送失败", t.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);

                // 记录日志
                string logMessage = $"[{level}] {code}: {message} (来源: {source})";
                switch (level)
                {
                    case AlarmLevel.Info:
                        Logger.Info(logMessage);
                        break;
                    case AlarmLevel.Warning:
                        Logger.Warning(logMessage);
                        break;
                    case AlarmLevel.Error:
                    case AlarmLevel.Critical:
                        Logger.Error(logMessage);
                        break;
                }

                return alarm;
            }
        }

        /// <summary>
        /// 清除报警
        /// </summary>
        public bool ClearAlarm(int alarmId, string clearUser = null)
        {
            lock (_lock)
            {
                var alarm = _activeAlarms.FirstOrDefault(a => a.Id == alarmId);
                if (alarm == null)
                    return false;

                alarm.IsCleared = true;
                alarm.ClearTime = DateTime.Now;
                alarm.ClearUser = clearUser ?? "System";

                _activeAlarms.Remove(alarm);

                // 更新数据库
                try
                {
                    _alarmDb?.ClearAlarm(alarmId, alarm.ClearUser);
                }
                catch (Exception ex)
                {
                    Logger.Error("更新报警清除状态到数据库失败", ex);
                }

                AlarmCleared?.Invoke(this, alarm);
                Logger.Info($"报警已清除: {alarm.AlarmCode} - {alarm.AlarmMessage}");

                return true;
            }
        }

        /// <summary>
        /// 清除所有报警
        /// </summary>
        public void ClearAllAlarms(string clearUser = null)
        {
            lock (_lock)
            {
                var alarmsToClear = _activeAlarms.ToList();
                foreach (var alarm in alarmsToClear)
                {
                    ClearAlarm(alarm.Id, clearUser);
                }
            }
        }

        /// <summary>
        /// 获取活动报警列表
        /// </summary>
        public List<AlarmInfo> GetActiveAlarms()
        {
            lock (_lock)
            {
                return _activeAlarms.OrderByDescending(a => a.OccurTime).ToList();
            }
        }

        /// <summary>
        /// 获取报警历史
        /// </summary>
        public List<AlarmInfo> GetAlarmHistory(DateTime? startTime = null, DateTime? endTime = null)
        {
            lock (_lock)
            {
                var query = _alarmHistory.AsEnumerable();

                if (startTime.HasValue)
                    query = query.Where(a => a.OccurTime >= startTime.Value);

                if (endTime.HasValue)
                    query = query.Where(a => a.OccurTime <= endTime.Value);

                return query.OrderByDescending(a => a.OccurTime).ToList();
            }
        }

        /// <summary>
        /// 获取指定级别的报警
        /// </summary>
        public List<AlarmInfo> GetAlarmsByLevel(AlarmLevel level)
        {
            lock (_lock)
            {
                return _activeAlarms.Where(a => a.Level == level)
                    .OrderByDescending(a => a.OccurTime)
                    .ToList();
            }
        }

        /// <summary>
        /// 获取报警统计
        /// </summary>
        public AlarmStatistics GetStatistics(DateTime? startTime = null, DateTime? endTime = null)
        {
            lock (_lock)
            {
                var query = _alarmHistory.AsEnumerable();

                if (startTime.HasValue)
                    query = query.Where(a => a.OccurTime >= startTime.Value);

                if (endTime.HasValue)
                    query = query.Where(a => a.OccurTime <= endTime.Value);

                var alarms = query.ToList();

                return new AlarmStatistics
                {
                    TotalCount = alarms.Count,
                    InfoCount = alarms.Count(a => a.Level == AlarmLevel.Info),
                    WarningCount = alarms.Count(a => a.Level == AlarmLevel.Warning),
                    ErrorCount = alarms.Count(a => a.Level == AlarmLevel.Error),
                    CriticalCount = alarms.Count(a => a.Level == AlarmLevel.Critical),
                    ActiveCount = alarms.Count(a => !a.IsCleared),
                    ClearedCount = alarms.Count(a => a.IsCleared)
                };
            }
        }

        /// <summary>
        /// 监控循环
        /// </summary>
        private void MonitorLoop()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // 检查报警超时等
                    CheckAlarmTimeout();

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Logger.Error("报警监控线程异常", ex);
                }
            }
        }

        /// <summary>
        /// 检查报警超时 — 超时未清除的报警自动升级或记录警告
        /// </summary>
        private void CheckAlarmTimeout()
        {
            const int warningTimeoutMin = 30;
            const int errorTimeoutMin = 10;
            const int criticalTimeoutMin = 5;

            lock (_lock)
            {
                foreach (var alarm in _activeAlarms.ToList())
                {
                    TimeSpan elapsed = DateTime.Now - alarm.OccurTime;

                    // 根据报警级别判断是否超时
                    bool isTimeout = alarm.Level switch
                    {
                        AlarmLevel.Warning => elapsed.TotalMinutes >= warningTimeoutMin,
                        AlarmLevel.Error => elapsed.TotalMinutes >= errorTimeoutMin,
                        AlarmLevel.Critical => elapsed.TotalMinutes >= criticalTimeoutMin,
                        _ => false
                    };

                    if (isTimeout)
                    {
                        Logger.Warning($"报警超时未清除: [{alarm.Level}] {alarm.AlarmCode} - {alarm.AlarmMessage} (持续 {elapsed.TotalMinutes:F1} 分钟)");
                    }
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _monitorTask?.Wait(1000);
            _cancellationTokenSource?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// 报警统计
        /// </summary>
    public class AlarmStatistics
    {
        public int TotalCount { get; set; }
        public int InfoCount { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public int CriticalCount { get; set; }
        public int ActiveCount { get; set; }
        public int ClearedCount { get; set; }
    }
}
