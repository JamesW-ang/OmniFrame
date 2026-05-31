using System;
using System.Collections.Generic;
using OmniFrame.Common;
using OmniFrame.DataAccess;

namespace OmniFrame.Core
{
    public class ProductInfo
    {
        public string SerialNumber { get; set; }
        public string ProductModel { get; set; }
        public string WorkOrder { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Result { get; set; }
        public string DefectCode { get; set; }
        public double CycleTime { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    public class WorkOrderInfo
    {
        public string WorkOrderNumber { get; set; }
        public string ProductModel { get; set; }
        public int TargetQuantity { get; set; }
        public int CompletedQuantity { get; set; }
        public int PassQuantity { get; set; }
        public int FailQuantity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsActive { get; set; }
    }

    public delegate void ProductCompletedHandler(ProductInfo product);

    public class ProductManager : IProductManager
    {
        private readonly IProductDb _productDb;
        private WorkOrderInfo _currentWorkOrder;
        private ProductInfo _currentProduct;
        private DateTime _cycleStartTime;
        private readonly IUserManager _userMgr;

        public WorkOrderInfo CurrentWorkOrder => _currentWorkOrder;
        public ProductInfo CurrentProduct => _currentProduct;

        public int TotalCount { get; private set; }
        public int PassCount { get; private set; }
        public int FailCount { get; private set; }
        public double AverageCT { get; private set; }

        public event ProductCompletedHandler ProductCompleted;

        public ProductManager(IUserManager userMgr, IProductDb productDb)
        {
            _userMgr = userMgr ?? throw new ArgumentNullException(nameof(userMgr));
            _productDb = productDb ?? throw new ArgumentNullException(nameof(productDb));
        }

        public bool Initialize(string dbPath)
        {
            try
            {
                return _productDb.Open(dbPath);
            }
            catch (Exception ex)
            {
                Logger.Error("初始化产品管理器失败", ex);
                return false;
            }
        }

        public bool StartWorkOrder(string workOrderNumber, string productModel, int targetQuantity)
        {
            if (_currentWorkOrder != null && _currentWorkOrder.IsActive)
            {
                Logger.Warning("当前有正在进行的工单，请先结束当前工单");
                return false;
            }

            _currentWorkOrder = new WorkOrderInfo
            {
                WorkOrderNumber = workOrderNumber,
                ProductModel = productModel,
                TargetQuantity = targetQuantity,
                CompletedQuantity = 0,
                PassQuantity = 0,
                FailQuantity = 0,
                StartTime = DateTime.Now,
                IsActive = true
            };

            TotalCount = 0;
            PassCount = 0;
            FailCount = 0;
            AverageCT = 0;

            Logger.Info($"工单开始: {workOrderNumber}, 产品型号: {productModel}, 目标数量: {targetQuantity}");
            return true;
        }

        public bool EndWorkOrder()
        {
            if (_currentWorkOrder == null)
                return false;

            _currentWorkOrder.EndTime = DateTime.Now;
            _currentWorkOrder.IsActive = false;

            Logger.Info($"工单结束: {_currentWorkOrder.WorkOrderNumber}, 完成数量: {_currentWorkOrder.CompletedQuantity}");
            return true;
        }

        public void StartProduct(string serialNumber)
        {
            _currentProduct = new ProductInfo
            {
                SerialNumber = serialNumber,
                ProductModel = _currentWorkOrder?.ProductModel,
                WorkOrder = _currentWorkOrder?.WorkOrderNumber,
                StartTime = DateTime.Now
            };

            _cycleStartTime = DateTime.Now;
            Logger.Info($"开始生产: {serialNumber}");
        }

        public void CompleteProduct(bool result, string defectCode = "")
        {
            if (_currentProduct == null)
                return;

            _currentProduct.EndTime = DateTime.Now;
            _currentProduct.Result = result;
            _currentProduct.DefectCode = defectCode;
            _currentProduct.CycleTime = (_currentProduct.EndTime - _currentProduct.StartTime).TotalSeconds;

            // 保存到数据库
            SaveProductToDb(_currentProduct);

            // 更新统计
            TotalCount++;
            if (result)
            {
                PassCount++;
                if (_currentWorkOrder != null)
                    _currentWorkOrder.PassQuantity++;
            }
            else
            {
                FailCount++;
                if (_currentWorkOrder != null)
                    _currentWorkOrder.FailQuantity++;
            }

            if (_currentWorkOrder != null)
                _currentWorkOrder.CompletedQuantity++;

            // 更新平均CT
            AverageCT = (AverageCT * (TotalCount - 1) + _currentProduct.CycleTime) / TotalCount;

            Logger.Info($"产品完成: {_currentProduct.SerialNumber}, 结果: {(result ? "OK" : "NG")}, CT: {_currentProduct.CycleTime:F2}s");

            ProductCompleted?.Invoke(_currentProduct);
            _currentProduct = null;
        }

        private void SaveProductToDb(ProductInfo product)
        {
            if (_productDb == null)
                return;

            try
            {
                var record = new ProductRecord
                {
                    ProductTime = product.EndTime,
                    SerialNumber = product.SerialNumber,
                    ProductModel = product.ProductModel,
                    WorkOrder = product.WorkOrder,
                    Result = product.Result,
                    DefectCode = product.DefectCode,
                    CycleTime = product.CycleTime,
                    Operator = _userMgr.CurrentUser?.UserName ?? "System"
                };

                _productDb.AddProduct(record);
            }
            catch (Exception ex)
            {
                Logger.Error("保存产品记录失败", ex);
            }
        }

        public double GetYieldRate()
        {
            if (TotalCount == 0)
                return 0;
            return (double)PassCount / TotalCount * 100;
        }

        public void SetProductParameter(string name, string value)
        {
            if (_currentProduct != null)
            {
                _currentProduct.Parameters[name] = value;
            }
        }

        public string GetProductParameter(string name)
        {
            if (_currentProduct != null && _currentProduct.Parameters.ContainsKey(name))
            {
                return _currentProduct.Parameters[name];
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取产量历史
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="productId">产品ID（可选）</param>
        /// <returns>产量历史列表</returns>
        public List<ProductInfo> GetProductionHistory(DateTime? startTime = null, DateTime? endTime = null, string productId = null)
        {
            List<ProductInfo> history = new List<ProductInfo>();

            try
            {
                if (_productDb != null)
                {
                    List<ProductRecord> records;

                    // 从数据库获取产品记录
                    if (startTime.HasValue && endTime.HasValue)
                    {
                        records = _productDb.GetProductsByTime(startTime.Value, endTime.Value);
                    }
                    else
                    {
                        records = _productDb.GetAllProducts();
                    }

                    foreach (var record in records)
                    {
                        // 如果指定了产品ID，进行过滤
                        if (!string.IsNullOrEmpty(productId) && record.SerialNumber != productId)
                        {
                            continue;
                        }

                        var productInfo = new ProductInfo
                        {
                            SerialNumber = record.SerialNumber,
                            ProductModel = record.ProductModel,
                            WorkOrder = record.WorkOrder,
                            StartTime = record.ProductTime.AddSeconds(-record.CycleTime),
                            EndTime = record.ProductTime,
                            Result = record.Result,
                            DefectCode = record.DefectCode,
                            CycleTime = record.CycleTime
                        };
                        history.Add(productInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("获取产量历史失败", ex);
            }

            return history;
        }
    }
}
