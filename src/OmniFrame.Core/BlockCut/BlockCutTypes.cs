using System;
using Newtonsoft.Json;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// BlockCut 共享数据类型
    /// </summary>

    public struct Point2D
    {
        public double X, Y;
        public Point2D(double x, double y) { X = x; Y = y; }
        public override string ToString() => $"({X:F2}, {Y:F2})";
    }

    public class CameraResult
    {
        public bool Success;
        public Point2D Point1;
        public Point2D Point2;
        public double Angle;

        // ---- 仿真扩展属性 ----
        /// <summary>捕获时间戳</summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
        /// <summary>图像原始数据 (仅仿真模式)</summary>
        public byte[] ImageData { get; set; }
        /// <summary>图像宽度 (像素)</summary>
        public int Width { get; set; }
        /// <summary>图像高度 (像素)</summary>
        public int Height { get; set; }
        /// <summary>错误消息 (失败时)</summary>
        public string ErrorMessage { get; set; }
        /// <summary>相机名称</summary>
        public string CameraName { get; set; }
    }

    public class MesValidateResult
    {
        [JsonProperty("isValid")]
        public bool IsValid { get; set; }
        [JsonProperty("fileId")]
        public string FileId { get; set; }
        [JsonProperty("alertMsg")]
        public string AlertMsg { get; set; }
        /// <summary>片源状态: 0=OK, 1=次品, 2=异常, 3=黑名单</summary>
        [JsonProperty("pieceStatus")]
        public int PieceStatus { get; set; }
        /// <summary>业务错误消息</summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
