using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.Sdk.PluginSystem;

namespace OmniFrame.Core.PluginSystem.Testing
{
    /// <summary>
    /// 测试管理器
        /// </summary>
    public class PluginTestManager
    {
        private IPluginManager _pluginManager;

        public PluginTestManager(IPluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        /// <summary>
        /// 运行插件测试
        /// </summary>
        public async Task<TestSummary> RunPluginTest(string pluginName, Version version = null)
        {
            try
            {
                // 加载插件
                bool loaded = _pluginManager.LoadPlugin(pluginName, version);
                if (!loaded)
                {
                    throw new Exception("插件加载失败");
                }

                // 获取插件信息
                var plugins = _pluginManager.GetPlugins();
                var pluginInfo = version == null
                    ? plugins.Where(p => p.Name == pluginName).OrderByDescending(p => p.Version).FirstOrDefault()
                    : plugins.FirstOrDefault(p => p.Name == pluginName && p.Version == version);

                if (pluginInfo == null)
                {
                    throw new Exception("插件信息获取失败");
                }

                // 获取已加载的插件
                var plugin = _pluginManager.GetLoadedPlugin<OmniFrame.Sdk.PluginSystem.MotionPlugin>(pluginName);
                if (plugin == null)
                {
                    throw new Exception("已加载的插件获取失败");
                }

                // 创建测试套件
                var testSuite = new PluginTestSuite(plugin, pluginInfo);

                // 运行所有测试
                var results = await testSuite.RunAllTests();

                // 生成测试报告
                string reportPath = GenerateTestReport(pluginInfo, results);

                // 卸载插件
                _pluginManager.UnloadPlugin(pluginName);

                // 生成测试摘要
                var summary = new TestSummary
                {
                    PluginName = pluginName,
                    PluginVersion = version?.ToString() ?? "最新版本",
                    TestResults = results,
                    ReportPath = reportPath,
                    TestTime = DateTime.Now
                };

                return summary;
            }
            catch (Exception ex)
            {
                Logger.Error($"运行插件测试失败: {pluginName}", ex);
                throw;
            }
        }

        /// <summary>
        /// 运行所有插件测试
        /// </summary>
        public async Task<List<TestSummary>> RunAllPluginsTest()
        {
            List<TestSummary> summaries = new List<TestSummary>();

            try
            {
                // 获取所有插件
                var plugins = _pluginManager.GetPlugins();
                var pluginNames = plugins.Select(p => p.Name).Distinct().ToList();

                foreach (var pluginName in pluginNames)
                {
                    try
                    {
                        var summary = await RunPluginTest(pluginName);
                        summaries.Add(summary);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"运行插件 {pluginName} 测试失败", ex);
                    }
                }

                return summaries;
            }
            catch (Exception ex)
            {
                Logger.Error("运行所有插件测试失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 生成测试报告
        /// </summary>
        private string GenerateTestReport(PluginInfo pluginInfo, List<TestResult> results)
        {
            try
            {
                // 创建报告目录
                string reportDir = Path.Combine(AppContext.BaseDirectory, "TestReports");
                if (!Directory.Exists(reportDir))
                {
                    Directory.CreateDirectory(reportDir);
                }

                // 生成报告文件名
                string reportFileName = $"{pluginInfo.Name}_{pluginInfo.Version}_{DateTime.Now:yyyyMMddHHmmss}.html";
                string reportPath = Path.Combine(reportDir, reportFileName);

                // 生成HTML报告
                StringBuilder html = new StringBuilder();
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html>");
                html.AppendLine("<head>");
                html.AppendLine("<title>插件测试报告</title>");
                html.AppendLine("<style>");
                html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
                html.AppendLine("h1 { color: #333; }");
                html.AppendLine("h2 { color: #666; }");
                html.AppendLine(".summary { background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin-bottom: 20px; }");
                html.AppendLine(".test-result { margin-bottom: 20px; border: 1px solid #ddd; border-radius: 5px; overflow: hidden; }");
                html.AppendLine(".test-header { padding: 10px; font-weight: bold; }");
                html.AppendLine(".test-header.passed { background-color: #d4edda; color: #155724; }");
                html.AppendLine(".test-header.failed { background-color: #f8d7da; color: #721c24; }");
                html.AppendLine(".test-header.error { background-color: #fff3cd; color: #856404; }");
                html.AppendLine(".test-details { padding: 10px; background-color: #f9f9f9; }");
                html.AppendLine(".details-list { list-style-type: none; padding-left: 0; }");
                html.AppendLine(".details-list li { margin-bottom: 5px; }");
                html.AppendLine(".duration { font-size: 0.9em; color: #666; }");
                html.AppendLine("</style>");
                html.AppendLine("</head>");
                html.AppendLine("<body>");
                html.AppendLine($"<h1>插件测试报告</h1>");
                html.AppendLine("<div class='summary'>");
                html.AppendLine($"<h2>插件信息</h2>");
                html.AppendLine($"<p><strong>插件名称:</strong> {pluginInfo.Name}</p>");
                html.AppendLine($"<p><strong>版本:</strong> {pluginInfo.Version}</p>");
                html.AppendLine($"<p><strong>描述:</strong> {pluginInfo.Description}</p>");
                html.AppendLine($"<p><strong>测试时间:</strong> {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}</p>");
                html.AppendLine("</div>");
                html.AppendLine("<h2>测试结果</h2>");

                foreach (var result in results)
                {
                    string statusClass = "passed";
                    if (result.Status == TestResultStatus.Failed)
                    {
                        statusClass = "failed";
                    }
                    else if (result.Status == TestResultStatus.Error)
                    {
                        statusClass = "error";
                    }

                    html.AppendLine($"<div class='test-result'>");
                    html.AppendLine($"<div class='test-header {statusClass}'>");
                    html.AppendLine($"{result.TestName} - {GetStatusText(result.Status)}");
                    html.AppendLine($"<span class='duration'>耗时: {result.Duration.TotalSeconds:F2} 秒</span>");
                    html.AppendLine("</div>");
                    html.AppendLine("<div class='test-details'>");
                    html.AppendLine($"<p><strong>消息:</strong> {result.Message}</p>");
                    if (result.Details.Count > 0)
                    {
                        html.AppendLine("<h4>详细信息:</h4>");
                        html.AppendLine("<ul class='details-list'>");
                        foreach (var detail in result.Details)
                        {
                            html.AppendLine($"<li>{detail}</li>");
                        }
                        html.AppendLine("</ul>");
                    }
                    html.AppendLine("</div>");
                    html.AppendLine("</div>");
                }

                html.AppendLine("</body>");
                html.AppendLine("</html>");

                // 写入报告文件
                File.WriteAllText(reportPath, html.ToString(), Encoding.UTF8);
                Logger.Info($"测试报告生成成功: {reportPath}");

                return reportPath;
            }
            catch (Exception ex)
            {
                Logger.Error("生成测试报告失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 获取状态文本
        /// </summary>
        private string GetStatusText(TestResultStatus status)
        {
            switch (status)
            {
                case TestResultStatus.Passed:
                    return "通过";
                case TestResultStatus.Failed:
                    return "失败";
                case TestResultStatus.Error:
                    return "错误";
                case TestResultStatus.Running:
                    return "运行中";
                default:
                    return "未开始";
            }
        }

        /// <summary>
        /// 生成硬件支持清单
        /// </summary>
        public string GenerateHardwareSupportList()
        {
            try
            {
                // 获取所有插件
                var plugins = _pluginManager.GetPlugins();

                // 按插件名称分组
                var pluginGroups = plugins.GroupBy(p => p.Name).ToList();

                // 创建清单目录
                string listDir = Path.Combine(AppContext.BaseDirectory, "HardwareLists");
                if (!Directory.Exists(listDir))
                {
                    Directory.CreateDirectory(listDir);
                }

                // 生成清单文件名
                string listFileName = $"HardwareSupportList_{DateTime.Now:yyyyMMdd}.html";
                string listPath = Path.Combine(listDir, listFileName);

                // 生成HTML清单
                StringBuilder html = new StringBuilder();
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html>");
                html.AppendLine("<head>");
                html.AppendLine("<title>硬件支持清单</title>");
                html.AppendLine("<style>");
                html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
                html.AppendLine("h1 { color: #333; }");
                html.AppendLine("h2 { color: #666; margin-top: 30px; }");
                html.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 10px; }");
                html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
                html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
                html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
                html.AppendLine("</style>");
                html.AppendLine("</head>");
                html.AppendLine("<body>");
                html.AppendLine("<h1>硬件支持清单</h1>");
                html.AppendLine($"<p>生成时间: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}</p>");
                html.AppendLine($"<p>总插件数: {pluginGroups.Count}</p>");

                foreach (var group in pluginGroups)
                {
                    html.AppendLine($"<h2>{group.Key}</h2>");
                    html.AppendLine("<table>");
                    html.AppendLine("<tr>");
                    html.AppendLine("<th>版本</th>");
                    html.AppendLine("<th>描述</th>");
                    html.AppendLine("<th>是否官方</th>");
                    html.AppendLine("<th>路径</th>");
                    html.AppendLine("</tr>");

                    foreach (var plugin in group.OrderByDescending(p => p.Version))
                    {
                        html.AppendLine("<tr>");
                        html.AppendLine($"<td>{plugin.Version}</td>");
                        html.AppendLine($"<td>{plugin.Description}</td>");
                        html.AppendLine($"<td>{(plugin.IsOfficial ? "是" : "否")}</td>");
                        html.AppendLine($"<td>{plugin.Path}</td>");
                        html.AppendLine("</tr>");
                    }

                    html.AppendLine("</table>");
                }

                html.AppendLine("</body>");
                html.AppendLine("</html>");

                // 写入清单文件
                File.WriteAllText(listPath, html.ToString(), Encoding.UTF8);
                Logger.Info($"硬件支持清单生成成功: {listPath}");

                return listPath;
            }
            catch (Exception ex)
            {
                Logger.Error("生成硬件支持清单失败", ex);
                throw;
            }
        }
    }

    /// <summary>
    /// 测试摘要
        /// </summary>
    public class TestSummary
    {
        public string PluginName { get; set; }
        public string PluginVersion { get; set; }
        public List<TestResult> TestResults { get; set; }
        public string ReportPath { get; set; }
        public DateTime TestTime { get; set; }

        public TestSummary()
        {
            TestResults = new List<TestResult>();
        }

        public bool AllPassed
        {
            get { return TestResults.All(r => r.Status == TestResultStatus.Passed); }
        }

        public int PassedCount
        {
            get { return TestResults.Count(r => r.Status == TestResultStatus.Passed); }
        }

        public int FailedCount
        {
            get { return TestResults.Count(r => r.Status == TestResultStatus.Failed); }
        }

        public int ErrorCount
        {
            get { return TestResults.Count(r => r.Status == TestResultStatus.Error); }
        }
    }
}