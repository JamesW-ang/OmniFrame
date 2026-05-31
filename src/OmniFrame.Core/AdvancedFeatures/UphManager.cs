using System;
using System.Collections.Generic;
using System.Linq;
using OmniFrame.Common;

namespace OmniFrame.Core.AdvancedFeatures
{
    public interface IUphManager
    {
        void RecordProduction(string lineName, int count);
        double CalculateUph(string lineName);
        double CalculateTargetUph(string lineName);
    }

    internal class LineUphData
    {
        public int TotalCount { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? LastRecordTime { get; set; }
        public double TargetUph { get; set; } = 150.0;

        public double GetElapsedHours()
        {
            return Math.Max((DateTime.Now - StartTime).TotalHours, 0.01);
        }
    }

    public class UphManager : IUphManager
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, LineUphData> _lines = new Dictionary<string, LineUphData>();

        public UphManager() { }

        public void RecordProduction(string lineName, int count)
        {
            lock (_lock)
            {
                if (!_lines.TryGetValue(lineName, out var data))
                {
                    data = new LineUphData();
                    _lines[lineName] = data;
                }
                data.TotalCount += count;
                data.LastRecordTime = DateTime.Now;
            }
            Logger.Info($"UPH: 记录产量 {lineName} = {count}");
        }

        public double CalculateUph(string lineName)
        {
            lock (_lock)
            {
                if (!_lines.TryGetValue(lineName, out var data) || data.TotalCount == 0)
                {
                    Logger.Info($"UPH: 计算 {lineName} 每小时产量 = 0 (无数据)");
                    return 0.0;
                }

                double uph = data.TotalCount / data.GetElapsedHours();
                uph = Math.Round(uph, 1);
                Logger.Info($"UPH: 计算 {lineName} 每小时产量 = {uph}");
                return uph;
            }
        }

        public double CalculateTargetUph(string lineName)
        {
            lock (_lock)
            {
                double target = _lines.TryGetValue(lineName, out var data)
                    ? data.TargetUph
                    : 150.0;

                Logger.Info($"UPH: 计算 {lineName} 目标每小时产量 = {target}");
                return target;
            }
        }
    }
}
