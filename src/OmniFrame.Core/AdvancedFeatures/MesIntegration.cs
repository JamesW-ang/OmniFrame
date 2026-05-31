using System;
using System.Net.Http;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Core.AdvancedFeatures
{
    public interface IMesIntegration
    {
        Task<bool> Initialize(string mesUrl);
        Task<bool> SendProductionData(string productId, string stationName, bool isPass, string errorCode);
        Task<bool> ValidateConnection();
        Task<string> GetWorkOrder(string workOrderId);
        Task<bool> ReportDowntime(string stationName, TimeSpan duration, string reason);
    }

    public class MesIntegration : IMesIntegration, IDisposable
    {
        private string _mesUrl;
        private HttpClient _httpClient;
        private bool _disposed;

        public MesIntegration()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        public async Task<bool> Initialize(string mesUrl)
        {
            _mesUrl = mesUrl;
            await Task.CompletedTask;
            Logger.Info($"MES初始化: {mesUrl}");
            return true;
        }

        public async Task<bool> SendProductionData(string productId, string stationName, bool isPass, string errorCode)
        {
            await Task.CompletedTask;
            Logger.Info($"MES上报生产数据: 产品={productId}, 工位={stationName}, 结果={(isPass ? "良品" : "不良品")}");
            return true;
        }

        public async Task<bool> ValidateConnection()
        {
            await Task.CompletedTask;
            return !string.IsNullOrEmpty(_mesUrl);
        }

        public async Task<string> GetWorkOrder(string workOrderId)
        {
            await Task.CompletedTask;
            Logger.Info($"MES获取工单: {workOrderId}");
            return $"{{\"WorkOrderId\": \"{workOrderId}\", \"ProductName\": \"Sample\"}}";
        }

        public async Task<bool> ReportDowntime(string stationName, TimeSpan duration, string reason)
        {
            await Task.CompletedTask;
            Logger.Info($"MES上报停机: 工位={stationName}, 时长={duration.TotalMinutes}分钟, 原因={reason}");
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _httpClient?.Dispose();
            }

            _disposed = true;
        }
    }
}
