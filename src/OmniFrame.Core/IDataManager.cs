using System;
using System.Collections.Generic;
using OmniFrame.DataAccess;

namespace OmniFrame.Core
{
    /// <summary>
    /// 数据管理器接口
    /// </summary>
    public interface IDataManager : IDisposable
    {
        bool IsRunning { get; }
        int PendingDataCount { get; }

        event EventHandler<ProductData> DataSaved;
        event EventHandler<ProductionStatistics> StatisticsUpdated;

        bool Initialize();
        bool Start();
        void Stop();
        void AddProductData(ProductData data);
        ProductionStatistics GetTodayStatistics();
        List<ProductionStatistics> GetHistoryStatistics(DateTime startDate, DateTime endDate);
        List<ProductRecord> QueryProducts(string serialNumber = null, DateTime? startTime = null, DateTime? endTime = null, bool? result = null);
        bool ExportToCsv(string filePath, DateTime startTime, DateTime endTime);
        void PeriodicSave();
        void LogSystemStatus();
    }
}
