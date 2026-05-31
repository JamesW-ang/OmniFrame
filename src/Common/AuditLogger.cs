using System;
using System.IO;
using System.Text;
using Serilog;
using Serilog.Sinks.Async;

namespace OmniFrame.Common
{
    /// <summary>
    /// 审计日志管理器
    /// 记录关键操作（用户登录/登出、配置修改、设备控制指令、配方切换、报警确认）
    /// 写入独立的audit_{date}.log文件，格式为JSON Lines便于后续分析
        /// </summary>
    public class AuditLogger
    {
        private static ILogger _logger;
        private static string _logPath = Path.Combine(AppContext.BaseDirectory, "Log");

        static AuditLogger()
        {
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }

            // 初始化Serilog，使用JSON格式输出
            _logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithMachineName()
                .WriteTo.Async(a => a.File(
                    path: Path.Combine(_logPath, "audit_.log"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 100 * 1024 * 1024, // 100MB
                    retainedFileCountLimit: 30,
                    formatter: new Serilog.Formatting.Json.JsonFormatter(),
                    encoding: Encoding.UTF8
                ))
                .CreateLogger();
        }

        public static void SetLogPath(string path)
        {
            _logPath = path;
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }

            // 重新配置Serilog
            _logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithMachineName()
                .WriteTo.Async(a => a.File(
                    path: Path.Combine(_logPath, "audit_.log"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 100 * 1024 * 1024, // 100MB
                    retainedFileCountLimit: 30,
                    formatter: new Serilog.Formatting.Json.JsonFormatter(),
                    encoding: Encoding.UTF8
                ))
                .CreateLogger();
        }

        /// <summary>
        /// 记录用户登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="success">是否成功</param>
        public static void LogUserLogin(string username, string ipAddress, bool success)
        {
            _logger.Information("User login", new
            {
                Operation = "UserLogin",
                Username = username,
                IpAddress = ipAddress,
                Success = success,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 记录用户登出
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="ipAddress">IP地址</param>
        public static void LogUserLogout(string username, string ipAddress)
        {
            _logger.Information("User logout", new
            {
                Operation = "UserLogout",
                Username = username,
                IpAddress = ipAddress,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 记录配置修改
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="configSection">配置节</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        public static void LogConfigChange(string username, string configSection, string oldValue, string newValue)
        {
            _logger.Information("Config change", new
            {
                Operation = "ConfigChange",
                Username = username,
                ConfigSection = configSection,
                OldValue = oldValue,
                NewValue = newValue,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 记录设备控制指令
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="command">指令</param>
        /// <param name="parameters">参数</param>
        /// <param name="result">结果</param>
        public static void LogDeviceCommand(string username, string deviceId, string command, string parameters, string result)
        {
            _logger.Information("Device command", new
            {
                Operation = "DeviceCommand",
                Username = username,
                DeviceId = deviceId,
                Command = command,
                Parameters = parameters,
                Result = result,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 记录配方切换
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="oldRecipe">旧配方</param>
        /// <param name="newRecipe">新配方</param>
        public static void LogRecipeChange(string username, string oldRecipe, string newRecipe)
        {
            _logger.Information("Recipe change", new
            {
                Operation = "RecipeChange",
                Username = username,
                OldRecipe = oldRecipe,
                NewRecipe = newRecipe,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 记录报警确认
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="alarmId">报警ID</param>
        /// <param name="alarmMessage">报警消息</param>
        public static void LogAlarmAcknowledge(string username, string alarmId, string alarmMessage)
        {
            _logger.Information("Alarm acknowledge", new
            {
                Operation = "AlarmAcknowledge",
                Username = username,
                AlarmId = alarmId,
                AlarmMessage = alarmMessage,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 记录通用审计事件
        /// </summary>
        /// <param name="operation">操作类型</param>
        /// <param name="username">用户名</param>
        /// <param name="details">详细信息</param>
        public static void LogAuditEvent(string operation, string username, object details)
        {
            _logger.Information("Audit event", new
            {
                Operation = operation,
                Username = username,
                Details = details,
                Timestamp = DateTime.Now
            });
        }
    }
}