using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    public static class OperationLogService
    {
        private const string logFilePath = "operation_logs.json";
        private static readonly object _lock = new object();
        private static List<OperationLog> logs = new List<OperationLog>();

        static OperationLogService()
        {
            LoadLogs();
        }

        public static void WriteLog(string user, string actionType, string description, bool success)
        {
            lock (_lock)
            {
                var log = new OperationLog
                {
                    Time = DateTime.Now,
                    User = user,
                    ActionType = actionType,
                    Description = description,
                    Success = success
                };

                logs.Add(log);
                SaveLogs();
            }
        }

        public static List<OperationLog> QueryLogs(DateTime start, DateTime end, string type, string user)
        {
            var result = new List<OperationLog>();

            lock (_lock)
            {
                foreach (var log in logs)
                {
                    if (log.Time < start || log.Time > end)
                        continue;
                    if (type != "全部" && log.ActionType != type)
                        continue;
                    if (user != "全部" && log.User != user)
                        continue;

                    result.Add(log);
                }
            }

            result.Sort((x, y) => y.Time.CompareTo(x.Time));
            return result;
        }

        public static void ClearLogs()
        {
            lock (_lock)
            {
                logs.Clear();
                SaveLogs();
            }
        }

        private static void LoadLogs()
        {
            if (File.Exists(logFilePath))
            {
                try
                {
                    string json = File.ReadAllText(logFilePath);
                    logs = JsonConvert.DeserializeObject<List<OperationLog>>(json);
                }
                catch (Exception ex)
                {
                    Logger.Error("操作日志读取失败", ex);
                    logs = new List<OperationLog>();
                }
            }
            else
            {
                logs = new List<OperationLog>();
                GenerateSampleLogs();
                SaveLogs();
            }
        }

        private static void SaveLogs()
        {
            try
            {
                string json = JsonConvert.SerializeObject(logs, Formatting.Indented);
                File.WriteAllText(logFilePath, json);
            }
            catch (Exception ex)
            {
                Logger.Error("操作日志保存失败", ex);
            }
        }

        private static void GenerateSampleLogs()
        {
            var users = new string[] { "admin", "operator", "engineer", "guest" };
            var actionTypes = new string[] { "登录", "配方修改", "参数变更", "设备操作" };
            var descriptions = new string[]
            {
                "用户登录系统", "修改配方参数", "调整设备参数",
                "启动设备", "停止设备", "导入配方", "导出报表"
            };

            var random = new Random();
            for (int i = 0; i < 100; i++)
            {
                logs.Add(new OperationLog
                {
                    Time = DateTime.Now.AddMinutes(-i * 10),
                    User = users[random.Next(users.Length)],
                    ActionType = actionTypes[random.Next(actionTypes.Length)],
                    Description = descriptions[random.Next(descriptions.Length)],
                    Success = random.Next(10) > 0
                });
            }
        }
    }
}
