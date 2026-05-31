using System;
using System.Collections.Generic;
using System.Data;

namespace OmniFrame.DataAccess
{
    public interface IProductDb
    {
        bool Open(string dbPath = null);
        void Close();
        bool AddProduct(ProductRecord record);
        DataTable GetProductHistory(DateTime startTime, DateTime endTime, string workOrder = null, bool? result = null);
        int GetProductCount(DateTime startTime, DateTime endTime, bool? result = null);
        double GetAverageCycleTime(DateTime startTime, DateTime endTime);
        double GetYieldRate(DateTime startTime, DateTime endTime);
        DataTable GetYieldByHour(DateTime date);
        List<ProductRecord> GetProductsByTime(DateTime startTime, DateTime endTime);
        ProductRecord GetProductBySerialNumber(string serialNumber);
        List<ProductRecord> GetAllProducts();
    }
}
