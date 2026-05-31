using System;
using System.Collections.Generic;
using MotionIO;
using Plc;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 设备管理器接口
    /// </summary>
    public interface IDeviceManager : IDisposable
    {
        bool IsReady { get; }
        bool HasError { get; }
        int DeviceCount { get; }

        event EventHandler<DeviceError> DeviceErrorOccurred;
        event EventHandler<string> DeviceConnected;
        event EventHandler<string> DeviceDisconnected;

        bool Initialize();
        bool AddDevice(string name, IDevice device);
        TDevice GetDevice<TDevice>(string name) where TDevice : class, IDevice;
        bool AddMotionDevice(string name, MotionDevice device);
        bool AddPlcDevice(string name, PlcDevice device);
        bool AddCustomDevice(string name, IDevice device);
        MotionDevice GetMotionDevice(string name);
        PlcDevice GetPlcDevice(string name);
        List<string> GetMotionDeviceNames();
        List<string> GetPlcDeviceNames();
        bool Start();
        void Stop();
        void EmergencyStop();
        bool Reset();
        List<DeviceError> GetErrors();
        void ClearErrors();
        DeviceStatusSummary GetStatusSummary();
    }
}
