using System;
using System.Collections.Generic;
using System.Threading;

namespace OmniFrame.Plugins.MockBusiness
{
    [OmniFrame.Sdk.PluginSystem.PluginVersion(1, 0, 0)]
    public class MockBusinessPlugin : OmniFrame.Sdk.PluginSystem.BusinessPlugin
    {
        private readonly Random _rng = new Random();

        public override string Name => "MockBusinessPlugin";
        public override string Description => "模拟业务插件 — 称重上传、报表生成、配方校验等示例业务逻辑";

        public override bool Initialize()
        {
            return true;
        }

        public override void Unload()
        {
        }

        public override object Execute(object parameters)
        {
            var paramStr = parameters?.ToString() ?? "";
            Thread.Sleep(_rng.Next(50, 300));

            if (paramStr.Contains("WeighAndUpload") || paramStr.Contains("称重"))
            {
                double weight = 100.0 + _rng.NextDouble() * 50.0;
                return new Dictionary<string, object>
                {
                    ["Operation"] = "WeighAndUpload",
                    ["Weight"] = weight,
                    ["Unit"] = "g",
                    ["Uploaded"] = true,
                    ["Timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                };
            }

            if (paramStr.Contains("GenerateReport") || paramStr.Contains("报表"))
            {
                return new Dictionary<string, object>
                {
                    ["Operation"] = "GenerateReport",
                    ["ReportId"] = Guid.NewGuid().ToString("N").Substring(0, 8),
                    ["TotalCount"] = _rng.Next(500, 2000),
                    ["PassRate"] = 95.0 + _rng.NextDouble() * 4.5,
                    ["GeneratedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
            }

            if (paramStr.Contains("ValidateRecipe") || paramStr.Contains("校验"))
            {
                return new Dictionary<string, object>
                {
                    ["Operation"] = "ValidateRecipe",
                    ["IsValid"] = true,
                    ["Warnings"] = new[] { "DispenseTimeMs 超出推荐范围", "UvCureTimeMs 接近上限" },
                    ["CheckedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
            }

            return new Dictionary<string, object>
            {
                ["Operation"] = paramStr,
                ["Result"] = "OK",
                ["ExecutedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            };
        }
    }
}
