using System;
using System.Collections.Generic;
using System.Linq;
using OmniFrame.Common;

namespace OmniFrame.Core.AdvancedFeatures
{
    public record DataStatistics(int Count, double Min, double Max, double Mean, double Median, double StdDev, double PassRate);

    public interface IDataAnalysisManager
    {
        void RecordData(string category, string key, double value);
        double CalculatePassRate(string stationName);
        double CalculateYield(string productId);
        DataStatistics GetStatistics(string category);
        List<(DateTime Time, double Value)> GetTrend(string category, string key);
        IReadOnlyList<string> GetAllCategories();
        void ResetCategory(string category);
        void ResetAll();
    }

    internal class AnalysisDataStore
    {
        public Dictionary<string, List<double>> Values { get; } = new Dictionary<string, List<double>>();
        public int TotalCount { get; set; }
        public int PassCount { get; set; }
    }

    public class DataAnalysisManager : IDataAnalysisManager
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, AnalysisDataStore> _categories = new Dictionary<string, AnalysisDataStore>();
        private readonly Dictionary<string, AnalysisDataStore> _products = new Dictionary<string, AnalysisDataStore>();
        private readonly Dictionary<string, List<(DateTime Time, double Value)>> _trends
            = new Dictionary<string, List<(DateTime Time, double Value)>>();

        public DataAnalysisManager() { }

        public void RecordData(string category, string key, double value)
        {
            lock (_lock)
            {
                if (!_categories.TryGetValue(category, out var store))
                {
                    store = new AnalysisDataStore();
                    _categories[category] = store;
                }

                if (!store.Values.TryGetValue(key, out var list))
                {
                    list = new List<double>();
                    store.Values[key] = list;
                }
                list.Add(value);

                store.TotalCount++;
                if (value >= 0) store.PassCount++;

                // Record trend data
                var trendKey = $"{category}/{key}";
                if (!_trends.TryGetValue(trendKey, out var trendList))
                {
                    trendList = new List<(DateTime Time, double Value)>();
                    _trends[trendKey] = trendList;
                }
                trendList.Add((DateTime.Now, value));
            }
            Logger.Info($"数据分析: 记录 {category}/{key} = {value}");
        }

        public double CalculatePassRate(string stationName)
        {
            lock (_lock)
            {
                if (!_categories.TryGetValue(stationName, out var store) || store.TotalCount == 0)
                {
                    Logger.Info($"数据分析: 计算 {stationName} 良率 = N/A (无数据)");
                    return 0.0;
                }

                double rate = (double)store.PassCount / store.TotalCount;
                rate = Math.Round(rate, 4);
                Logger.Info($"数据分析: 计算 {stationName} 良率 = {rate:P2} ({store.PassCount}/{store.TotalCount})");
                return rate;
            }
        }

        public double CalculateYield(string productId)
        {
            lock (_lock)
            {
                if (!_products.TryGetValue(productId, out var store) || store.TotalCount == 0)
                {
                    Logger.Info($"数据分析: 计算产品 {productId} 收益率 = N/A (无数据)");
                    return 0.0;
                }

                double yield = (double)store.PassCount / store.TotalCount;
                yield = Math.Round(yield, 4);
                Logger.Info($"数据分析: 计算产品 {productId} 收益率 = {yield:P2} ({store.PassCount}/{store.TotalCount})");
                return yield;
            }
        }

        public DataStatistics GetStatistics(string category)
        {
            lock (_lock)
            {
                if (!_categories.TryGetValue(category, out var store) || store.TotalCount == 0)
                {
                    Logger.Info($"数据分析: {category} 统计 = N/A (无数据)");
                    return new DataStatistics(0, 0, 0, 0, 0, 0, 0);
                }

                var allValues = store.Values.SelectMany(kvp => kvp.Value).ToList();
                int count = allValues.Count;
                double min = allValues.Min();
                double max = allValues.Max();
                double mean = allValues.Average();
                double median = CalculateMedian(allValues);
                double stdDev = CalculateStdDev(allValues, mean);
                double passRate = (double)store.PassCount / store.TotalCount;

                var stats = new DataStatistics(count, Math.Round(min, 4), Math.Round(max, 4),
                    Math.Round(mean, 4), Math.Round(median, 4), Math.Round(stdDev, 4), Math.Round(passRate, 4));
                Logger.Info($"数据分析: {category} 统计 = Mean:{stats.Mean}, StdDev:{stats.StdDev}, PassRate:{stats.PassRate:P2}");
                return stats;
            }
        }

        public List<(DateTime Time, double Value)> GetTrend(string category, string key)
        {
            lock (_lock)
            {
                var trendKey = $"{category}/{key}";
                if (!_trends.TryGetValue(trendKey, out var trendList))
                {
                    return new List<(DateTime Time, double Value)>();
                }
                return new List<(DateTime Time, double Value)>(trendList);
            }
        }

        public IReadOnlyList<string> GetAllCategories()
        {
            lock (_lock)
            {
                return _categories.Keys.ToList();
            }
        }

        public void ResetCategory(string category)
        {
            lock (_lock)
            {
                _categories.Remove(category);
                var prefix = $"{category}/";
                var trendKeys = _trends.Keys.Where(k => k.StartsWith(prefix)).ToList();
                foreach (var key in trendKeys)
                {
                    _trends.Remove(key);
                }
            }
            Logger.Info($"数据分析: 重置分类 {category}");
        }

        public void ResetAll()
        {
            lock (_lock)
            {
                _categories.Clear();
                _products.Clear();
                _trends.Clear();
            }
            Logger.Info("数据分析: 重置所有数据");
        }

        private static double CalculateMedian(List<double> values)
        {
            if (values.Count == 0) return 0;
            var sorted = values.OrderBy(v => v).ToList();
            int mid = sorted.Count / 2;
            return sorted.Count % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2.0
                : sorted[mid];
        }

        private static double CalculateStdDev(List<double> values, double mean)
        {
            if (values.Count < 2) return 0;
            double sumSquaredDiffs = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumSquaredDiffs / (values.Count - 1));
        }
    }
}
