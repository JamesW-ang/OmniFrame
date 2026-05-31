using System;
using System.IO;
using System.Reflection;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 审计日志记录器
    /// </summary>
    public class AuditLogger : IAuditLogger
    {
        private string _auditLogPath;
        private bool _isDisposed;

        public void Initialize()
        {
            string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _auditLogPath = Path.Combine(appDir, "Log", "Audit");

            if (!Directory.Exists(_auditLogPath))
            {
                Directory.CreateDirectory(_auditLogPath);
            }
        }

        public void LogOperation(string userId, string userName, UserLevel userLevel, string operation, bool success)
        {
            try
            {
                string logFile = Path.Combine(_auditLogPath, $"Audit_{DateTime.Now:yyyy-MM-dd}.log");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{userLevel}] {userName}({userId}) - {operation} - {(success ? "成功" : "失败")}";

                File.AppendAllText(logFile, logEntry + Environment.NewLine);
                Logger.Info(logEntry);
            }
            catch (Exception ex)
            {
                Logger.Error("记录审计日志失败", ex);
            }
        }

        public void LogParameterChange(string userId, string userName, UserLevel userLevel, string operation, string parameters, string oldValue, string newValue)
        {
            try
            {
                string logFile = Path.Combine(_auditLogPath, $"ParameterChange_{DateTime.Now:yyyy-MM-dd}.log");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{userLevel}] {userName}({userId}) - {operation} - 参数: {parameters} - 旧值: {oldValue} - 新值: {newValue}";

                File.AppendAllText(logFile, logEntry + Environment.NewLine);
                Logger.Info(logEntry);
            }
            catch (Exception ex)
            {
                Logger.Error("记录参数变更日志失败", ex);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
