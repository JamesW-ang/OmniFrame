using System;
using System.Data;

namespace OmniFrame.DataAccess
{
    public interface IAlarmDb
    {
        bool Open(string dbPath = null);
        void Close();
        bool AddAlarm(AlarmRecord record);
        bool ClearAlarm(int alarmId, string clearUser);
        DataTable GetAlarmHistory(DateTime startTime, DateTime endTime, string alarmLevel = null, bool? isCleared = null);
        DataTable GetActiveAlarms();
        int GetAlarmCount(DateTime startTime, DateTime endTime, string alarmLevel = null);
        bool ClearAllAlarms(string clearUser);
    }
}
