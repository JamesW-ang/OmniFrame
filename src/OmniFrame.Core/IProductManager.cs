using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    public interface IProductManager
    {
        WorkOrderInfo CurrentWorkOrder { get; }
        ProductInfo CurrentProduct { get; }
        int TotalCount { get; }
        int PassCount { get; }
        int FailCount { get; }
        double AverageCT { get; }

        event ProductCompletedHandler ProductCompleted;

        bool Initialize(string dbPath);
        bool StartWorkOrder(string workOrderNumber, string productModel, int targetQuantity);
        bool EndWorkOrder();
        void StartProduct(string serialNumber);
        void CompleteProduct(bool result, string defectCode = "");
        double GetYieldRate();
        void SetProductParameter(string name, string value);
        string GetProductParameter(string name);
        List<ProductInfo> GetProductionHistory(DateTime? startTime = null, DateTime? endTime = null, string productId = null);
    }
}
