using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OmniFrame.Common;
using Newtonsoft.Json;

namespace OmniFrame.DataAccess
{
    /// <summary>
    /// MES客户端
    /// 设计介绍：
    /// 2. 使用HttpClient实现HTTP请求，支持RESTful API调用
    /// 3. 实现JWT令牌认证机制，确保通信安全
    /// 4. 支持生产数据、设备状态、报警数据的上传
    /// 5. 支持生产计划的下载
    /// 6. 提供连接测试功能，验证MES系统连接状态
    /// 7. 使用Newtonsoft.Json进行JSON序列化和反序列化
    /// 8. 集成日志系统，记录MES系统交互的执行情况
    /// 9. 提供完善的异常处理机制
        /// </summary>
    public class MesClient : IDisposable
    {
        private string _baseUrl;
        private string _token;
        private HttpClient _httpClient;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="baseUrl">MES系统基础URL</param>
        public MesClient(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>是否成功</returns>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var data = new { username, password };
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                    if (tokenData.ContainsKey("token"))
                    {
                        _token = tokenData["token"];
                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                        Logger.Info("MES系统登录成功");
                        return true;
                    }
                }
                Logger.Error("MES系统登录失败");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("MES系统登录失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 上传生产数据
        /// </summary>
        /// <param name="productionData">生产数据</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UploadProductionDataAsync(ProductionData productionData)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(productionData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/production/data", content);

                if (response.IsSuccessStatusCode)
                {
                    Logger.Info("生产数据上传成功");
                    return true;
                }
                Logger.Error("生产数据上传失败");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("生产数据上传失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 上传设备状态
        /// </summary>
        /// <param name="equipmentData">设备状态数据</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UploadEquipmentStatusAsync(EquipmentData equipmentData)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(equipmentData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/equipment/status", content);

                if (response.IsSuccessStatusCode)
                {
                    Logger.Info("设备状态上传成功");
                    return true;
                }
                Logger.Error("设备状态上传失败");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("设备状态上传失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 上传报警数据
        /// </summary>
        /// <param name="alarmData">报警数据</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UploadAlarmDataAsync(AlarmData alarmData)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(alarmData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/alarm/data", content);

                if (response.IsSuccessStatusCode)
                {
                    Logger.Info("报警数据上传成功");
                    return true;
                }
                Logger.Error("报警数据上传失败");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("报警数据上传失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 下载生产计划
        /// </summary>
        /// <param name="workshopId">车间ID</param>
        /// <returns>生产计划列表</returns>
        public async Task<List<ProductionPlan>> DownloadProductionPlanAsync(string workshopId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/production/plan?workshopId={Uri.EscapeDataString(workshopId)}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var plans = JsonConvert.DeserializeObject<List<ProductionPlan>>(result);
                    Logger.Info($"生产计划下载成功，计划数量: {plans.Count}");
                    return plans;
                }
                Logger.Error("生产计划下载失败");
                return new List<ProductionPlan>();
            }
            catch (Exception ex)
            {
                Logger.Error("生产计划下载失败", ex);
                return new List<ProductionPlan>();
            }
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/health");
                if (response.IsSuccessStatusCode)
                {
                    Logger.Info("MES系统连接测试成功");
                    return true;
                }
                Logger.Error("MES系统连接测试失败");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("MES系统连接测试失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// 生产数据
        /// </summary>
    public class ProductionData
    {
        /// <summary>
        /// 生产订单号
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// 产品编码
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 生产数量
        /// </summary>
        public int Quantity { get; set; }
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
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 操作员
        /// </summary>
        public string Operator { get; set; }
    }

    /// <summary>
    /// 设备状态数据
        /// </summary>
    public class EquipmentData
    {
        /// <summary>
        /// 设备编码
        /// </summary>
        public string EquipmentCode { get; set; }
        /// <summary>
        /// 设备状态（运行、停止、故障）
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 状态时间
        /// </summary>
        public DateTime StatusTime { get; set; }
        /// <summary>
        /// 运行时间
        /// </summary>
        public double RunTime { get; set; }
        /// <summary>
        /// 故障时间
        /// </summary>
        public double FaultTime { get; set; }
    }

    /// <summary>
    /// 报警数据
        /// </summary>
    public class AlarmData
    {
        /// <summary>
        /// 报警ID
        /// </summary>
        public string AlarmId { get; set; }
        /// <summary>
        /// 设备编码
        /// </summary>
        public string EquipmentCode { get; set; }
        /// <summary>
        /// 报警类型
        /// </summary>
        public string AlarmType { get; set; }
        /// <summary>
        /// 报警描述
        /// </summary>
        public string AlarmDescription { get; set; }
        /// <summary>
        /// 报警时间
        /// </summary>
        public DateTime AlarmTime { get; set; }
        /// <summary>
        /// 恢复时间
        /// </summary>
        public DateTime? RestoreTime { get; set; }
        /// <summary>
        /// 处理人员
        /// </summary>
        public string Handler { get; set; }
    }

    /// <summary>
    /// 生产计划
        /// </summary>
    public class ProductionPlan
    {
        /// <summary>
        /// 计划ID
        /// </summary>
        public string PlanId { get; set; }
        /// <summary>
        /// 生产订单号
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
        /// 开始日期
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
    }
}
