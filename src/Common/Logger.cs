using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Async;

namespace OmniFrame.Common
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public class LogEntry
    {
        public DateTime Time { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
    }

    public class Logger
    {
        private static readonly object _lock = new object();
        private static readonly object _serilogLock = new object();
        private static string _logPath = Path.Combine(AppContext.BaseDirectory, "Log");
        private static volatile LogLevel _minLevel = LogLevel.Debug;

        private static long _successCount;
        private static long _failureCount;

        public static event Action<LogEntry> OnLogWritten;

        static Logger()
        {
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }

            Log.Logger = CreateLoggerConfig().CreateLogger();
        }

        private static LoggerConfiguration CreateLoggerConfig(LogEventLevel? minLevel = null)
        {
            var config = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithMachineName()
                .WriteTo.Async(a => a.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [T{ThreadId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
                ))
                .WriteTo.Async(a => a.File(
                    path: Path.Combine(_logPath, "log_.txt"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 100 * 1024 * 1024,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [T{ThreadId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    encoding: Encoding.UTF8
                ));

            if (minLevel.HasValue)
                config = config.MinimumLevel.Is(minLevel.Value);
            else
                config = config.MinimumLevel.Debug();

            return config;
        }

        public static void SetLogPath(string path)
        {
            _logPath = path;
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }

            lock (_serilogLock)
            {
                Log.Logger = CreateLoggerConfig().CreateLogger();
            }
        }

        public static void SetMinLevel(LogLevel level)
        {
            _minLevel = level;

            lock (_serilogLock)
            {
                Log.Logger = CreateLoggerConfig(MapLogLevel(level)).CreateLogger();
            }
        }

        private static void FireEvent(LogLevel level, string message)
        {
            var handler = OnLogWritten;
            if (handler != null)
            {
                var entry = new LogEntry { Time = DateTime.Now, Level = level, Message = message, Source = "App" };
                handler(entry);
            }
        }

        public static void Debug(string message)
        {
            if (_minLevel <= LogLevel.Debug)
            {
                Log.Debug(message);
                FireEvent(LogLevel.Debug, message);
            }
        }

        public static void Debug(string message, Exception ex)
        {
            if (_minLevel <= LogLevel.Debug)
            {
                Log.Debug(ex, message);
                FireEvent(LogLevel.Debug, ex != null ? $"{message} | {ex.Message}" : message);
            }
        }

        public static void Info(string message)
        {
            if (_minLevel <= LogLevel.Info)
            {
                Log.Information(message);
                FireEvent(LogLevel.Info, message);
            }
        }

        public static void Warning(string message)
        {
            if (_minLevel <= LogLevel.Warning)
            {
                Log.Warning(message);
                FireEvent(LogLevel.Warning, message);
            }
        }

        public static void Error(string message)
        {
            if (_minLevel <= LogLevel.Error)
            {
                Log.Error(message);
                FireEvent(LogLevel.Error, message);
            }
        }

        public static void Error(string message, Exception ex)
        {
            if (_minLevel <= LogLevel.Error)
            {
                Log.Error(ex, message);
                FireEvent(LogLevel.Error, ex != null ? $"{message} | {ex.Message}" : message);
            }
        }

        public static void Error(string messageTemplate, Exception ex, params object[] args)
        {
            if (_minLevel <= LogLevel.Error)
            {
                Log.Error(ex, messageTemplate, args);
                FireEvent(LogLevel.Error, SafeFormat(messageTemplate, args));
            }
        }

        public static void Error(string messageTemplate, params object[] args)
        {
            if (_minLevel <= LogLevel.Error)
            {
                Log.Error(messageTemplate, args);
                FireEvent(LogLevel.Error, SafeFormat(messageTemplate, args));
            }
        }

        public static void Warning(string messageTemplate, Exception ex, params object[] args)
        {
            if (_minLevel <= LogLevel.Warning)
            {
                Log.Warning(ex, messageTemplate, args);
                FireEvent(LogLevel.Warning, SafeFormat(messageTemplate, args));
            }
        }

        public static void Info(string messageTemplate, params object[] args)
        {
            if (_minLevel <= LogLevel.Info)
            {
                Log.Information(messageTemplate, args);
                FireEvent(LogLevel.Info, SafeFormat(messageTemplate, args));
            }
        }

        public static void Event(int eventId, string message)
        {
            Log.Information("Event {EventId}: {Message}", eventId, message);
            FireEvent(LogLevel.Info, $"Event {eventId}: {message}");
        }

        public static long SuccessCount => Interlocked.Read(ref _successCount);
        public static long FailureCount => Interlocked.Read(ref _failureCount);

        public static void IncrementSuccess() => Interlocked.Increment(ref _successCount);
        public static void IncrementFailure() => Interlocked.Increment(ref _failureCount);

        public static (long success, long failure) ResetCounters()
        {
            var s = Interlocked.Exchange(ref _successCount, 0);
            var f = Interlocked.Exchange(ref _failureCount, 0);
            return (s, f);
        }

        public static void Fatal(string message)
        {
            if (_minLevel <= LogLevel.Fatal)
            {
                Log.Fatal(message);
                FireEvent(LogLevel.Fatal, message);
            }
        }

        private static string SafeFormat(string template, object[] args)
        {
            if (args == null || args.Length == 0) return template;
            try
            {
                return template + " [" + string.Join(", ", args) + "]";
            }
            catch
            {
                return template;
            }
        }

        private static LogEventLevel MapLogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Info:
                    return LogEventLevel.Information;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Fatal:
                    return LogEventLevel.Fatal;
                default:
                    return LogEventLevel.Information;
            }
        }

        public static List<LogEntry> GetLogs()
        {
            List<LogEntry> logs = new List<LogEntry>();
            try
            {
                string todayFile = Path.Combine(_logPath, $"log_{DateTime.Now:yyyyMMdd}.txt");
                if (File.Exists(todayFile))
                {
                    string[] lines = File.ReadAllLines(todayFile, Encoding.UTF8);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("["))
                        {
                            try
                            {
                                int timeEnd = line.IndexOf("] [", 1);
                                int levelEnd = line.IndexOf("] [T", timeEnd + 3);
                                if (timeEnd > 0 && levelEnd > 0)
                                {
                                    string timeStr = line.Substring(1, timeEnd - 1);
                                    string levelStr = line.Substring(timeEnd + 3, levelEnd - timeEnd - 3);
                                    string message = line.Substring(line.IndexOf("] ", levelEnd) + 2);

                                    if (DateTime.TryParse(timeStr, out DateTime time))
                                    {
                                        LogLevel level = MapSerilogLevel(levelStr);
                                        logs.Add(new LogEntry
                                        {
                                            Time = time,
                                            Level = level,
                                            Message = message,
                                            Source = "System"
                                        });
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return logs;
        }

        private static LogLevel MapSerilogLevel(string levelStr)
        {
            switch (levelStr)
            {
                case "DBG":
                    return LogLevel.Debug;
                case "INF":
                    return LogLevel.Info;
                case "WRN":
                    return LogLevel.Warning;
                case "ERR":
                    return LogLevel.Error;
                case "FTL":
                    return LogLevel.Fatal;
                default:
                    return LogLevel.Info;
            }
        }

    }
}
