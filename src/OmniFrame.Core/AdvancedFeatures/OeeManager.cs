using System;
using System.Collections.Generic;
using System.Linq;
using OmniFrame.Common;

namespace OmniFrame.Core.AdvancedFeatures
{
    public interface IOeeManager
    {
        void StartProduction(string lineName);
        void StopProduction(string lineName);
        void RecordGoodProduct(string lineName);
        void RecordBadProduct(string lineName);
        double CalculateOee(string lineName);
        void RecordDowntime(string lineName, TimeSpan duration, string reason);
    }

    internal class LineOeeData
    {
        public int GoodCount { get; set; }
        public int BadCount { get; set; }
        public int TotalCount => GoodCount + BadCount;
        public double PassRate => TotalCount > 0 ? (double)GoodCount / TotalCount : 1.0;
        public DateTime? ProductionStart { get; set; }
        public DateTime? ProductionEnd { get; set; }
        public TimeSpan Downtime { get; set; }
        public readonly List<DowntimeRecord> DowntimeRecords = new List<DowntimeRecord>();

        public TimeSpan AvailableTime
        {
            get
            {
                if (ProductionStart == null) return TimeSpan.Zero;
                var end = ProductionEnd ?? DateTime.Now;
                return end - ProductionStart.Value - Downtime;
            }
        }
    }

    internal class DowntimeRecord
    {
        public TimeSpan Duration { get; set; }
        public string Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class OeeManager : IOeeManager
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, LineOeeData> _lines = new Dictionary<string, LineOeeData>();
        private const double IdealCycleTimeSeconds = 1.0;

        public OeeManager() { }

        public void StartProduction(string lineName)
        {
            lock (_lock)
            {
                if (!_lines.TryGetValue(lineName, out var data))
                {
                    data = new LineOeeData();
                    _lines[lineName] = data;
                }
                data.ProductionStart = data.ProductionStart ?? DateTime.Now;
                data.ProductionEnd = null;
                Logger.Info($"OEE: 生产线 {lineName} 开始生产");
            }
        }

        public void StopProduction(string lineName)
        {
            lock (_lock)
            {
                if (_lines.TryGetValue(lineName, out var data))
                {
                    data.ProductionEnd = DateTime.Now;
                }
                Logger.Info($"OEE: 生产线 {lineName} 停止生产");
            }
        }

        public void RecordGoodProduct(string lineName)
        {
            lock (_lock)
            {
                if (!_lines.TryGetValue(lineName, out var data))
                {
                    data = new LineOeeData();
                    _lines[lineName] = data;
                }
                data.GoodCount++;
            }
            Logger.Info($"OEE: 生产线 {lineName} 记录良品");
        }

        public void RecordBadProduct(string lineName)
        {
            lock (_lock)
            {
                if (!_lines.TryGetValue(lineName, out var data))
                {
                    data = new LineOeeData();
                    _lines[lineName] = data;
                }
                data.BadCount++;
            }
            Logger.Info($"OEE: 生产线 {lineName} 记录不良品");
        }

        public double CalculateOee(string lineName)
        {
            lock (_lock)
            {
                if (!_lines.TryGetValue(lineName, out var data) || data.TotalCount == 0)
                {
                    Logger.Info($"OEE: 生产线 {lineName} 无数据，返回默认值");
                    return 0.0;
                }

                var availableTime = data.AvailableTime;
                if (availableTime.TotalSeconds <= 0) return 0.0;

                double availability = availableTime.TotalSeconds /
                    Math.Max((DateTime.Now - (data.ProductionStart ?? DateTime.Now)).TotalSeconds, 1);
                double performance = (data.TotalCount * IdealCycleTimeSeconds) / availableTime.TotalSeconds;
                double quality = data.PassRate;

                double oee = availability * performance * quality * 100;
                oee = Math.Round(Math.Min(oee, 100.0), 1);

                Logger.Info($"OEE: 计算生产线 {lineName} 综合效率 = {oee}% (A={availability:P2}, P={performance:P2}, Q={quality:P2})");
                return oee;
            }
        }

        public void RecordDowntime(string lineName, TimeSpan duration, string reason)
        {
            lock (_lock)
            {
                if (!_lines.TryGetValue(lineName, out var data))
                {
                    data = new LineOeeData();
                    _lines[lineName] = data;
                }
                data.Downtime += duration;
                data.DowntimeRecords.Add(new DowntimeRecord
                {
                    Duration = duration,
                    Reason = reason,
                    Timestamp = DateTime.Now
                });
            }
            Logger.Info($"OEE: 生产线 {lineName} 停机 {duration.TotalMinutes} 分钟，原因: {reason}");
        }
    }
}
