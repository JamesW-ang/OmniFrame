using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    /// <summary>
    /// 生产管理器接口
    /// </summary>
    public interface IProductionManager
    {
        bool LoadProductionOrders();
        bool StartProductionOrder(string orderId);
        bool EndProductionOrder(string orderId);
        bool RecordProductionData(bool isPass);
        ProductionOrder GetCurrentOrder();
        ProductionOrder GetOrderById(string orderId);
        List<ProductionOrder> GetProductionOrders();
        List<ProductionOrder> GetOrders(string statusFilter = null);
        ProductionStats GetProductionStats();
        bool CreateOrder(ProductionOrder order);
        bool UpdateOrderStatus(string orderId, string newStatus);
        bool SaveToDatabase();
        bool LoadFromDatabase();
    }
}
