using System;

namespace OmniFrame.Common
{
    /// <summary>
    /// 统一设备接口 — 所有硬件设备（PLC、运动控制卡、自定义设备）的通用抽象
        /// </summary>
    public interface IDevice : IDisposable
    {
        string Name { get; }
        bool IsConnected { get; }
        bool Initialize();
        bool Connect();
        void Disconnect();
        bool Reset();
    }
}
