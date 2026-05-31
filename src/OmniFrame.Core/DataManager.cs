using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.DataAccess;

namespace OmniFrame.Core
{
    /// <summary>
    /// 产品数据
        /// </summary>
    public class ProductData
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string ProductModel { get; set; }
        public string WorkOrder { get; set; }
        public string Station { get; set; }
        public bool Result { get; set; }
        public string DefectCode { get; set; }
        public double CycleTime { get; set; }
        public string Operator { get; set; }
        public DateTime ProductTime { get; set; }
        public Dictionary<string, object> ExtraData { get; set; }

        public ProductData()
        {
            ProductTime = DateTime.Now;
            ExtraData = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// 生产统计
        /// </summary>
    public class ProductionStatistics
    {
        public DateTime Date { get; set; }
        public int TotalCount { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public double PassRate => TotalCount > 0 ? (double)PassCount / TotalCount * 100 : 0;
        public double AverageCycleTime { get; set; }
        public double OEE { get; set; }
    }

    /// <summary>
    /// 数据管理器
        /// </summary>
    public class DataManager : IDisposable, IDataManager
    {
        private const string DefaultDataPath = "Data";
        private readonly object _lock = new object();
        private readonly IProductDb _productDb;
        private Queue<ProductData> _dataQueue;
        private Task _saveTask;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private string _dataPath;


        // 生产统计
        private int _todayTotalCount;
        private int _todayPassCount;
        private int _todayFailCount;
        private List<double> _cycleTimes;

        public bool IsRunning => _isRunning;
        public int PendingDataCount { get { lock (_lock) { return _dataQueue.Count; } } }

        public event EventHandler<ProductData> DataSaved;
        public event EventHandler<ProductionStatistics> StatisticsUpdated;

        private void FireStatisticsUpdated()
        {
            var stats = GetTodayStatistics();
            StatisticsUpdated?.Invoke(this, stats);
        }

        public DataManager(IProductDb productDb)
        {
            _productDb = productDb ?? throw new ArgumentNullException(nameof(productDb));
            _dataQueue = new Queue<ProductData>();
            _cycleTimes = new List<double>();
            _dataPath = DefaultDataPath;
        }

        public bool Initialize()
        {
            try
            {
                Logger.Info("初始化数据管理器...");

                // 确保数据目录存在
                if (!Directory.Exists(_dataPath))
                {
                    Directory.CreateDirectory(_dataPath);
                }

                // 初始化数据库
                string dbPath = Path.Combine(_dataPath, "Production.db");
                _productDb.Open(dbPath);

                // 加载今日统计
                LoadTodayStatistics();

                Logger.Info("数据管理器初始化完成");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("数据管理器初始化失败", ex);
                return false;
            }
        }

        public bool Start()
        {
            try
            {
                Logger.Info("启动数据管理器...");
                _cancellationTokenSource = new CancellationTokenSource();
                _saveTask = Task.Factory.StartNew(SaveLoop, TaskCreationOptions.LongRunning);
                _isRunning = true;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("数据管理器启动失败", ex);
                return false;
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            _saveTask?.Wait(5000);

            // 保存剩余数据
            FlushQueue();
        }

        /// <summary>
        /// 添加产品数据
        /// </summary>
        public void AddProductData(ProductData data)
        {
            lock (_lock)
            {
                _dataQueue.Enqueue(data);

                // 更新统计
                _todayTotalCount++;
                if (data.Result)
                {
                    _todayPassCount++;
                }
                else
                {
                    _todayFailCount++;
                }

                if (data.CycleTime > 0)
                {
                    _cycleTimes.Add(data.CycleTime);
                }

                FireStatisticsUpdated();
            }
        }

        /// <summary>
        /// 保存数据线程
        /// </summary>
        private void SaveLoop()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    FlushQueue();
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Logger.Error("数据保存线程异常，等待重启", ex);
                    Thread.Sleep(5000); // 等待5秒后继续
                }
            }
        }

        /// <summary>
        /// 刷新队列
        /// </summary>
        private void FlushQueue()
        {
            List<ProductData> dataToSave = new List<ProductData>();

            lock (_lock)
            {
                while (_dataQueue.Count > 0)
                {
                    dataToSave.Add(_dataQueue.Dequeue());
                }
            }

            foreach (var data in dataToSave)
            {
                try
                {
                    SaveToDatabase(data);
                    DataSaved?.Invoke(this, data);
                }
                catch (Exception ex)
                {
                    Logger.Error($"保存产品数据失败: {data.SerialNumber}", ex);
                }
            }
        }

        /// <summary>
        /// 保存到数据库
        /// </summary>
        private void SaveToDatabase(ProductData data)
        {
            var record = new ProductRecord
            {
                ProductTime = data.ProductTime,
                SerialNumber = data.SerialNumber,
                ProductModel = data.ProductModel,
                WorkOrder = data.WorkOrder,
                Station = data.Station,
                Result = data.Result,
                DefectCode = data.DefectCode,
                CycleTime = data.CycleTime,
                Operator = data.Operator
            };

            _productDb.AddProduct(record);
        }

        /// <summary>
        /// 加载今日统计
        /// </summary>
        private void LoadTodayStatistics()
        {
            try
            {
                var today = DateTime.Now.Date;
                var tomorrow = today.AddDays(1);

                var records = _productDb.GetProductsByTime(today, tomorrow);
                _todayTotalCount = records.Count;
                _todayPassCount = records.Count(r => r.Result);
                _todayFailCount = _todayTotalCount - _todayPassCount;
                FireStatisticsUpdated();
            }
            catch (Exception ex)
            {
                Logger.Error("加载今日统计失败", ex);
            }
        }

        /// <summary>
        /// 获取生产统计
        /// </summary>
        public ProductionStatistics GetTodayStatistics()
        {
            lock (_lock)
            {
                return new ProductionStatistics
                {
                    Date = DateTime.Now.Date,
                    TotalCount = _todayTotalCount,
                    PassCount = _todayPassCount,
                    FailCount = _todayFailCount,
                    AverageCycleTime = _cycleTimes.Count > 0 ? _cycleTimes.Average() : 0
                };
            }
        }

        /// <summary>
        /// 获取历史统计
        /// </summary>
        public List<ProductionStatistics> GetHistoryStatistics(DateTime startDate, DateTime endDate)
        {
            var statistics = new List<ProductionStatistics>();

            try
            {
                var records = _productDb.GetProductsByTime(startDate, endDate);

                var dailyGroups = records.GroupBy(r => r.ProductTime.Date);

                foreach (var group in dailyGroups)
                {
                    var dayRecords = group.ToList();
                    var cycleTimes = dayRecords.Where(r => r.CycleTime > 0).Select(r => r.CycleTime).ToList();
                    statistics.Add(new ProductionStatistics
                    {
                        Date = group.Key,
                        TotalCount = dayRecords.Count,
                        PassCount = dayRecords.Count(r => r.Result),
                        FailCount = dayRecords.Count(r => !r.Result),
                        AverageCycleTime = cycleTimes.Count > 0 ? cycleTimes.Average() : 0
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("获取历史统计失败", ex);
            }

            return statistics;
        }

        /// <summary>
        /// 查询产品记录
        /// </summary>
        public List<ProductRecord> QueryProducts(string serialNumber = null, DateTime? startTime = null, DateTime? endTime = null, bool? result = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(serialNumber))
                {
                    var record = _productDb.GetProductBySerialNumber(serialNumber);
                    return record != null ? new List<ProductRecord> { record } : new List<ProductRecord>();
                }

                if (startTime.HasValue && endTime.HasValue)
                {
                    return _productDb.GetProductsByTime(startTime.Value, endTime.Value);
                }

                return _productDb.GetAllProducts();
            }
            catch (Exception ex)
            {
                Logger.Error("查询产品记录失败", ex);
                return new List<ProductRecord>();
            }
        }

        /// <summary>
        /// 导出数据到CSV
        /// </summary>
        public bool ExportToCsv(string filePath, DateTime startTime, DateTime endTime)
        {
            try
            {
                var records = _productDb.GetProductsByTime(startTime, endTime);

                using (var writer = new StreamWriter(filePath))
                {
                    // 写入表头
                    writer.WriteLine("序号,时间,产品序列号,产品型号,工单号,工位,检测结果,缺陷代码,周期时间(秒),操作员");

                    // 写入数据
                    int index = 1;
                    foreach (var record in records)
                    {
                        writer.WriteLine($"{index},{record.ProductTime:yyyy-MM-dd HH:mm:ss},{record.SerialNumber},{record.ProductModel},{record.WorkOrder},{record.Station},{(record.Result ? "OK" : "NG")},{record.DefectCode},{record.CycleTime:F2},{record.Operator}");
                        index++;
                    }
                }

                Logger.Info($"数据导出完成: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("导出数据失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 周期性保存
        /// </summary>
        public void PeriodicSave()
        {
            FlushQueue();
        }

        /// <summary>
        /// 记录系统状态
        /// </summary>
        public void LogSystemStatus()
        {
            var stats = GetTodayStatistics();
            Logger.Info($"生产统计 - 总数: {stats.TotalCount}, OK: {stats.PassCount}, NG: {stats.FailCount}, 良率: {stats.PassRate:F2}%");
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }
}
