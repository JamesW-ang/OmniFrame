// 示例3：创建自定义插件
using System;
using System.Collections.Generic;
using AOIFrame.Hardware;

/// <summary>
/// 自定义插件示例：数据采集与分析
/// </summary>
public class DataCollectionPlugin : IPlugin
{
    private List<double> sensorData = new List<double>();
    private int sampleCount = 0;
    
    /// <summary>
    /// 插件初始化
    /// </summary>
    public void Initialize(IServiceProvider services)
    {
        Console.WriteLine("[插件] 数据采集插件初始化");
        sampleCount = 0;
    }
    
    /// <summary>
    /// 执行插件功能
    /// </summary>
    public void Execute(Dictionary<string, object> parameters)
    {
        if (parameters == null) return;
        
        // 获取参数
        if (parameters.TryGetValue("action", out var action))
        {
            switch (action.ToString())
            {
                case "start":
                    StartCollection();
                    break;
                    
                case "stop":
                    StopCollection();
                    break;
                    
                case "analyze":
                    AnalyzeData();
                    break;
                    
                case "reset":
                    ResetData();
                    break;
            }
        }
    }
    
    private void StartCollection()
    {
        Console.WriteLine("[插件] 开始采集数据");
        // 模拟采集数据
        for (int i = 0; i < 100; i++)
        {
            double value = Math.Sin(i * Math.PI / 50) * 10;
            sensorData.Add(value);
            sampleCount++;
        }
    }
    
    private void StopCollection()
    {
        Console.WriteLine($"[插件] 停止采集，共采集 {sampleCount} 个数据点");
    }
    
    private void AnalyzeData()
    {
        if (sensorData.Count == 0)
        {
            Console.WriteLine("[插件] 没有采集到数据");
            return;
        }
        
        // 计算统计值
        double min = double.MaxValue;
        double max = double.MinValue;
        double sum = 0;
        
        foreach (var value in sensorData)
        {
            min = Math.Min(min, value);
            max = Math.Max(max, value);
            sum += value;
        }
        
        double avg = sum / sensorData.Count;
        
        Console.WriteLine("[插件] 数据分析结果:");
        Console.WriteLine($"  最小值: {min:F2}");
        Console.WriteLine($"  最大值: {max:F2}");
        Console.WriteLine($"  平均值: {avg:F2}");
    }
    
    private void ResetData()
    {
        sensorData.Clear();
        sampleCount = 0;
        Console.WriteLine("[插件] 数据已重置");
    }
}

/// <summary>
/// 使用插件的示例
/// </summary>
class PluginUsageExample
{
    static void Main()
    {
        try
        {
            // 加载插件
            PluginManager.Instance.LoadPlugin("DataCollection.dll");
            
            // 执行插件功能
            var startParams = new Dictionary<string, object> { { "action", "start" } };
            PluginManager.Instance.ExecutePluginFunction(
                "DataCollectionPlugin", "Execute", startParams);
            
            // 分析数据
            var analyzeParams = new Dictionary<string, object> { { "action", "analyze" } };
            PluginManager.Instance.ExecutePluginFunction(
                "DataCollectionPlugin", "Execute", analyzeParams);
            
            // 停止采集
            var stopParams = new Dictionary<string, object> { { "action", "stop" } };
            PluginManager.Instance.ExecutePluginFunction(
                "DataCollectionPlugin", "Execute", stopParams);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
    }
}
