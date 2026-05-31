using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmniFrame.Common;
using OmniFrame.DataAccess;

namespace OmniFrame.Core
{
    /// <summary>
    /// 生产管理器
    /// 设计介绍：
    /// 1. 管理生产订单的整个生命周期，包括加载、开始、结束和数据记录
    /// 2. 提供生产统计功能，计算产量、良率等关键指标
    /// 3. 仿真模式支持内存存储，同时提供 SaveToDatabase/LoadFromDatabase 持久化
    /// 4. 集成日志系统，记录生产过程中的重要事件
    /// 5. 提供完善的异常处理机制
    /// 6. 支持与MES系统的集成，可从MES系统加载生产订单
    /// </summary>
    public class ProductionManager : IProductionManager
    {
        private readonly object _orderLock = new object();
        private List<ProductionOrder> _productionOrders;
        private ProductionOrder _currentOrder;
        private readonly IDataManager _dataManager;
        private bool _hasLoadedFromDatabase;
        private const string OrderDataFile = "Data/ProductionOrders.json";

        /// <summary>
        /// 构造函数 (仿真/独立模式)
        /// </summary>
        public ProductionManager() : this(null) { }

        /// <summary>
        /// 构造函数 (带 IDataManager 注入)
        /// </summary>
        public ProductionManager(IDataManager dataManager)
        {
            _dataManager = dataManager;
            _productionOrders = new List<ProductionOrder>();
        }

        /// <summary>
        /// 加载生产订单
        /// 优先从数据库加载，若无数据则加载示例订单
        /// </summary>
        /// <returns>是否成功</returns>
        public bool LoadProductionOrders()
        {
            try
            {
                Logger.Info("开始加载生产订单...");

                // 优先尝试从数据库/文件加载
                if (!_hasLoadedFromDatabase)
                {
                    if (LoadFromDatabase())
                    {
                        _hasLoadedFromDatabase = true;
                        Logger.Info($"从数据库加载了 {_productionOrders.Count} 个生产订单");
                        return true;
                    }

                    if (LoadFromFile())
                    {
                        _hasLoadedFromDatabase = true;
                        Logger.Info($"从文件加载了 {_productionOrders.Count} 个生产订单");
                        return true;
                    }
                }

                // 首次运行或无数据: 加载示例数据
                if (_productionOrders.Count == 0)
                {
                    LoadSampleOrders();
                    _hasLoadedFromDatabase = true;
                }

                Logger.Info($"生产订单加载完成，共 {_productionOrders.Count} 个订单");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("加载生产订单失败", ex);
                LoadSampleOrders();
                return false;
            }
        }

        /// <summary>
        /// 加载示例订单 (首次运行无数据时)
        /// </summary>
        private void LoadSampleOrders()
        {
            lock (_orderLock)
            {
                _productionOrders = new List<ProductionOrder>
                {
                    new ProductionOrder
                    {
                        OrderId = "ORD001",
                        ProductCode = "PROD001",
                        ProductName = "产品A",
                        PlanQuantity = 1000,
                        ActualQuantity = 0,
                        PassQuantity = 0,
                        FailQuantity = 0,
                        StartTime = DateTime.Now,
                        EndTime = null,
                        Status = "待生产"
                    },
                    new ProductionOrder
                    {
                        OrderId = "ORD002",
                        ProductCode = "PROD002",
                        ProductName = "产品B",
                        PlanQuantity = 500,
                        ActualQuantity = 0,
                        PassQuantity = 0,
                        FailQuantity = 0,
                        StartTime = DateTime.Now,
                        EndTime = null,
                        Status = "待生产"
                    }
                };
                Logger.Info("已加载示例生产订单");
            }
        }

        /// <summary>
        /// 创建新订单
        /// </summary>
        public bool CreateOrder(ProductionOrder order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (string.IsNullOrWhiteSpace(order.OrderId))
            {
                Logger.Error("创建订单失败: OrderId 不能为空");
                return false;
            }

            lock (_orderLock)
            {
                // 检查重复
                if (_productionOrders.Any(o => o.OrderId == order.OrderId))
                {
                    Logger.Warning($"订单 {order.OrderId} 已存在，创建失败");
                    return false;
                }

                order.Status = string.IsNullOrEmpty(order.Status) ? "待生产" : order.Status;
                _productionOrders.Add(order);
            }

            Logger.Info($"订单创建成功: {order.OrderId} ({order.ProductName}, 计划 {order.PlanQuantity})");
            return true;
        }

        /// <summary>
        /// 根据 ID 获取订单
        /// </summary>
        public ProductionOrder GetOrderById(string orderId)
        {
            lock (_orderLock)
            {
                return _productionOrders.FirstOrDefault(o => o.OrderId == orderId);
            }
        }

        /// <summary>
        /// 获取订单列表 (可按状态筛选)
        /// </summary>
        public List<ProductionOrder> GetOrders(string statusFilter = null)
        {
            lock (_orderLock)
            {
                if (string.IsNullOrEmpty(statusFilter))
                    return _productionOrders.ToList(); // 返回副本
                return _productionOrders.Where(o =>
                    string.Equals(o.Status, statusFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        public bool UpdateOrderStatus(string orderId, string newStatus)
        {
            lock (_orderLock)
            {
                var order = _productionOrders.FirstOrDefault(o => o.OrderId == orderId);
                if (order == null)
                {
                    Logger.Warning($"更新订单状态失败: 订单 {orderId} 不存在");
                    return false;
                }

                string oldStatus = order.Status;
                order.Status = newStatus;

                if (newStatus == "生产中" && oldStatus != "生产中")
                {
                    order.StartTime = DateTime.Now;
                    _currentOrder = order;
                }
                else if (newStatus == "已完成" || newStatus == "已取消")
                {
                    order.EndTime = DateTime.Now;
                    if (_currentOrder == order)
                        _currentOrder = null;
                }

                Logger.Info($"订单状态更新: {orderId} [{oldStatus}] → [{newStatus}]");
                return true;
            }
        }

        /// <summary>
        /// 保存到数据库 (通过 IDataManager)
        /// </summary>
        public bool SaveToDatabase()
        {
            try
            {
                if (_dataManager == null)
                {
                    Logger.Info("IDataManager 未注入，保存到文件");
                    return SaveToFile();
                }

                // 将订单数据作为产品记录保存
                foreach (var order in _productionOrders)
                {
                    var data = new ProductData
                    {
                        SerialNumber = order.OrderId,
                        ProductModel = order.ProductCode,
                        WorkOrder = order.OrderId,
                        Station = "ProductionManager",
                        Result = order.Status == "已完成",
                        CycleTime = order.EndTime.HasValue && order.StartTime != DateTime.MinValue
                            ? (order.EndTime.Value - order.StartTime).TotalSeconds
                            : 0,
                        Operator = "System",
                        ExtraData = new Dictionary<string, object>
                        {
                            ["ProductName"] = order.ProductName,
                            ["PlanQuantity"] = order.PlanQuantity,
                            ["ActualQuantity"] = order.ActualQuantity,
                            ["PassQuantity"] = order.PassQuantity,
                            ["FailQuantity"] = order.FailQuantity,
                            ["Status"] = order.Status,
                            ["StartTime"] = order.StartTime.ToString("O"),
                            ["EndTime"] = order.EndTime?.ToString("O") ?? ""
                        }
                    };
                    _dataManager.AddProductData(data);
                }

                Logger.Info($"[ProductionManager] 已保存 {_productionOrders.Count} 个订单到数据库");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("[ProductionManager] 保存订单到数据库失败", ex);
                return SaveToFile(); // 回退到文件
            }
        }

        /// <summary>
        /// 从数据库加载 (通过 IDataManager)
        /// </summary>
        public bool LoadFromDatabase()
        {
            try
            {
                if (_dataManager == null)
                    return false;

                // 通过 IDataManager 查询历史产品记录
                var records = _dataManager.QueryProducts(null, null, null, null);
                if (records == null || records.Count == 0)
                    return false;

                // 从产品记录重建订单 (合并相同工单)
                var orders = new Dictionary<string, ProductionOrder>(StringComparer.OrdinalIgnoreCase);
                foreach (var record in records)
                {
                    if (string.IsNullOrEmpty(record.WorkOrder))
                        continue;

                    if (!orders.TryGetValue(record.WorkOrder, out var order))
                    {
                        order = new ProductionOrder
                        {
                            OrderId = record.WorkOrder,
                            ProductCode = record.ProductModel ?? "",
                            ProductName = record.ProductModel ?? "",
                            PlanQuantity = 0,
                            ActualQuantity = 0,
                            PassQuantity = 0,
                            FailQuantity = 0,
                            Status = "待生产",
                        };
                        orders[record.WorkOrder] = order;
                    }

                    order.ActualQuantity++;
                    if (record.Result)
                        order.PassQuantity++;
                    else
                        order.FailQuantity++;
                }

                lock (_orderLock)
                {
                    if (orders.Count > 0)
                    {
                        _productionOrders = orders.Values.ToList();
                        Logger.Info($"[ProductionManager] 从数据库重建了 {orders.Count} 个订单");
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("[ProductionManager] 从数据库加载订单失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 保存到本地文件 (JSON)
        /// </summary>
        private bool SaveToFile()
        {
            try
            {
                string dir = Path.GetDirectoryName(OrderDataFile);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonConvert.SerializeObject(_productionOrders, Formatting.Indented);
                File.WriteAllText(OrderDataFile, json, Encoding.UTF8);
                Logger.Info($"[ProductionManager] 已保存 {_productionOrders.Count} 个订单到 {OrderDataFile}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("[ProductionManager] 保存订单到文件失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 从本地文件加载 (JSON)
        /// </summary>
        private bool LoadFromFile()
        {
            try
            {
                if (!File.Exists(OrderDataFile))
                    return false;

                string json = File.ReadAllText(OrderDataFile, Encoding.UTF8);
                var orders = JsonConvert.DeserializeObject<List<ProductionOrder>>(json);
                if (orders == null || orders.Count == 0)
                    return false;

                lock (_orderLock)
                {
                    _productionOrders = orders;
                }
                Logger.Info($"[ProductionManager] 从文件加载了 {orders.Count} 个订单");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warning($"[ProductionManager] 从文件加载订单失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 开始生产订单
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>是否成功</returns>
        public bool StartProductionOrder(string orderId)
        {
            var order = _productionOrders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.Status = "生产中";
                order.StartTime = DateTime.Now;
                _currentOrder = order;
                Logger.Info($"开始生产订单: {orderId} ({order.ProductName})");
                return true;
            }
            Logger.Warning($"开始订单失败: 订单 {orderId} 不存在");
            return false;
        }

        /// <summary>
        /// 结束生产订单
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>是否成功</returns>
        public bool EndProductionOrder(string orderId)
        {
            var order = _productionOrders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.Status = "已完成";
                order.EndTime = DateTime.Now;
                if (_currentOrder == order)
                {
                    _currentOrder = null;
                }
                Logger.Info($"结束生产订单: {orderId} (合格:{order.PassQuantity}, 不合格:{order.FailQuantity}, 良率:{order.PassQuantity * 100.0 / Math.Max(order.ActualQuantity, 1):F1}%)");
                return true;
            }
            Logger.Warning($"结束订单失败: 订单 {orderId} 不存在");
            return false;
        }

        /// <summary>
        /// 记录生产数据
        /// </summary>
        /// <param name="isPass">是否合格</param>
        /// <returns>是否成功</returns>
        public bool RecordProductionData(bool isPass)
        {
            if (_currentOrder != null)
            {
                lock (_orderLock)
                {
                    _currentOrder.ActualQuantity++;
                    if (isPass)
                    {
                        _currentOrder.PassQuantity++;
                    }
                    else
                    {
                        _currentOrder.FailQuantity++;
                    }
                }
                Logger.Info($"记录生产数据: {_currentOrder.OrderId}, 合格={isPass}, 累计={_currentOrder.ActualQuantity}/{_currentOrder.PlanQuantity}");
                return true;
            }
            Logger.Warning("记录生产数据失败: 无当前活动订单");
            return false;
        }

        /// <summary>
        /// 获取当前生产订单
        /// </summary>
        /// <returns>当前生产订单</returns>
        public ProductionOrder GetCurrentOrder()
        {
            return _currentOrder;
        }

        /// <summary>
        /// 获取生产订单列表
        /// </summary>
        /// <returns>生产订单列表</returns>
        public List<ProductionOrder> GetProductionOrders()
        {
            lock (_orderLock)
            {
                return _productionOrders.ToList();
            }
        }

        /// <summary>
        /// 获取生产统计数据
        /// </summary>
        /// <returns>生产统计数据</returns>
        public ProductionStats GetProductionStats()
        {
            ProductionStats stats;
            lock (_orderLock)
            {
                stats = new ProductionStats
                {
                    TotalOrders = _productionOrders.Count,
                    CompletedOrders = _productionOrders.Count(o => o.Status == "已完成"),
                    InProgressOrders = _productionOrders.Count(o => o.Status == "生产中"),
                    TotalQuantity = _productionOrders.Sum(o => o.ActualQuantity),
                    PassQuantity = _productionOrders.Sum(o => o.PassQuantity),
                    FailQuantity = _productionOrders.Sum(o => o.FailQuantity)
                };
            }
            stats.YieldRate = stats.TotalQuantity > 0 ? (double)stats.PassQuantity / stats.TotalQuantity * 100 : 0;
            return stats;
        }
    }

    /// <summary>
    /// 生产订单
    /// </summary>
    public class ProductionOrder
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// 产品编码
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 计划数量
        /// </summary>
        public int PlanQuantity { get; set; }
        /// <summary>
        /// 实际数量
        /// </summary>
        public int ActualQuantity { get; set; }
        /// <summary>
        /// 合格数量
        /// </summary>
        public int PassQuantity { get; set; }
        /// <summary>
        /// 不合格数量
        /// </summary>
        public int FailQuantity { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 状态 (待生产/生产中/已完成/已取消)
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// 生产统计数据
    /// </summary>
    public class ProductionStats
    {
        /// <summary>
        /// 总订单数
        /// </summary>
        public int TotalOrders { get; set; }
        /// <summary>
        /// 已完成订单数
        /// </summary>
        public int CompletedOrders { get; set; }
        /// <summary>
        /// 生产中订单数
        /// </summary>
        public int InProgressOrders { get; set; }
        /// <summary>
        /// 总产量
        /// </summary>
        public int TotalQuantity { get; set; }
        /// <summary>
        /// 合格产量
        /// </summary>
        public int PassQuantity { get; set; }
        /// <summary>
        /// 不合格产量
        /// </summary>
        public int FailQuantity { get; set; }
        /// <summary>
        /// 良率
        /// </summary>
        public double YieldRate { get; set; }
    }
}
