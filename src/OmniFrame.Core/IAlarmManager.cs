using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    /// <summary>
    /// 报警管理器接口
    /// </summary>
    public interface IAlarmManager : IDisposable
    {
        bool HasActiveAlarm { get; }
        bool HasCriticalAlarm { get; }
        int ActiveAlarmCount { get; }
        int TotalAlarmCount { get; }

        event EventHandler<AlarmInfo> AlarmOccurred;
        event EventHandler<AlarmInfo> AlarmCleared;
        event EventHandler<AlarmLevel> CriticalAlarmOccurred;

        bool Initialize();
        AlarmInfo AddAlarm(string code, string message, AlarmLevel level, string source);
        bool ClearAlarm(int alarmId, string clearUser = null);
        void ClearAllAlarms(string clearUser = null);
        List<AlarmInfo> GetActiveAlarms();
        List<AlarmInfo> GetAlarmHistory(DateTime? startTime = null, DateTime? endTime = null);
        List<AlarmInfo> GetAlarmsByLevel(AlarmLevel level);
        AlarmStatistics GetStatistics(DateTime? startTime = null, DateTime? endTime = null);
    }
}
