using System;
using System.Reflection;
using System.Text;
using System.IO;
using OmniFrame.Common;

namespace RemoteMonitor
{
    /// <summary>
    /// 安全审计特性
    /// 用于标注需要审计日志的API端点，自动记录操作人、时间、参数
        /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SecurityAuditAttribute : Attribute
    {
        /// <summary>
        /// 操作名称
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// 是否记录参数
        /// </summary>
        public bool LogParameters { get; set; } = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="operationName">操作名称</param>
        public SecurityAuditAttribute(string operationName)
        {
            OperationName = operationName;
        }

        /// <summary>
        /// 记录审计日志
        /// </summary>
        /// <param name="username">操作人</param>
        /// <param name="methodName">方法名</param>
        /// <param name="parameters">参数</param>
        public static void LogAudit(string username, string methodName, object[] parameters = null)
        {
            try
            {
                StringBuilder logBuilder = new StringBuilder();
                logBuilder.AppendLine($"[审计日志]");
                logBuilder.AppendLine($"时间: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                logBuilder.AppendLine($"操作人: {username}");
                logBuilder.AppendLine($"操作: {methodName}");

                if (parameters != null && parameters.Length > 0)
                {
                    logBuilder.AppendLine("参数:");
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        string paramValue = parameters[i]?.ToString() ?? "null";
                        // 限制参数长度，避免日志过大
                        if (paramValue.Length > 1000)
                        {
                            paramValue = paramValue.Substring(0, 1000) + "...";
                        }
                        logBuilder.AppendLine($"  参数{i + 1}: {paramValue}");
                    }
                }

                // 写入审计日志文件
                string logDir = Path.Combine(AppContext.BaseDirectory, "Log", "Audit");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                string logFile = Path.Combine(logDir, $"Audit_{DateTime.Now.ToString("yyyy-MM-dd")}.log");
                File.AppendAllText(logFile, logBuilder.ToString() + "\n");

                // 同时记录到系统日志
                Logger.Info(logBuilder.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error("记录审计日志失败", ex);
            }
        }
    }
}
