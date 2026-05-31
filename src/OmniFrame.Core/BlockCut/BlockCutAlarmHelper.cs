using System;
using OmniFrame.Core;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    public static class BlockCutAlarmHelper
    {
        private static IAlarmManager _alarmManager;

        public static void Initialize(IAlarmManager alarmManager)
        {
            _alarmManager = alarmManager;
        }

        public static void ReportInfo(string message)
        {
            ReportAlarm(message, AlarmLevel.Info);
        }

        public static void ReportWarning(string message)
        {
            ReportAlarm(message, AlarmLevel.Warning);
        }

        public static void ReportError(string message)
        {
            ReportAlarm(message, AlarmLevel.Error);
        }

        public static void ReportError(string message, Exception ex)
        {
            Logger.Error(message, ex);
            ReportAlarm($"{message}: {ex.Message}", AlarmLevel.Error);
        }

        public static void ReportAlarm(string message, AlarmLevel level)
        {
            if (_alarmManager == null)
            {
                switch (level)
                {
                    case AlarmLevel.Info:
                        Logger.Info($"[BlockCut] {message}");
                        break;
                    case AlarmLevel.Warning:
                        Logger.Warning($"[BlockCut] {message}");
                        break;
                    case AlarmLevel.Error:
                        Logger.Error($"[BlockCut] {message}");
                        break;
                }
                return;
            }

            var alarmLevel = level switch
            {
                AlarmLevel.Info => OmniFrame.Core.AlarmLevel.Info,
                AlarmLevel.Warning => OmniFrame.Core.AlarmLevel.Warning,
                AlarmLevel.Error => OmniFrame.Core.AlarmLevel.Error,
                _ => OmniFrame.Core.AlarmLevel.Info
            };

            _alarmManager.AddAlarm("BlockCut", message, alarmLevel, "BlockCut");
        }

        public static void ClearAlarm(string message)
        {
            if (_alarmManager != null)
            {
                var alarms = _alarmManager.GetActiveAlarms();
                foreach (var alarm in alarms)
                {
                    if (alarm.AlarmMessage.Contains(message))
                    {
                        _alarmManager.ClearAlarm(alarm.Id);
                    }
                }
            }
        }
    }

    public enum AlarmLevel
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }
}
