using OmniFrame.Common;

namespace OmniFrame.Core.AdvancedFeatures
{
    public interface IVisionSystem
    {
        bool Connect(string ip, int port);
        void Disconnect();
        bool IsConnected { get; }
        VisionResult Detect(VisionParams parameters);
    }

    public class VisionResult
    {
        public bool IsPass { get; set; }
        public double Score { get; set; }
        public string Message { get; set; }
    }

    public class VisionParams
    {
        public string StationName { get; set; }
        public string ProductId { get; set; }
        public int ExposureTime { get; set; }
    }

    public class VisionSystem : IVisionSystem
    {

        public VisionSystem() { }

        public bool Connect(string ip, int port)
        {
            Logger.Info($"视觉系统连接: {ip}:{port}");
            IsConnected = true;
            return true;
        }

        public void Disconnect()
        {
            IsConnected = false;
            Logger.Info("视觉系统断开连接");
        }

        public bool IsConnected { get; private set; }

        public VisionResult Detect(VisionParams parameters)
        {
            Logger.Info($"视觉检测: 工位={parameters.StationName}, 产品={parameters.ProductId}");
            return new VisionResult { IsPass = true, Score = 0.95, Message = "检测通过" };
        }
    }
}
