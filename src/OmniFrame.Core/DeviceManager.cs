using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using OmniFrame.Common;
using MotionIO;
using Plc;

namespace OmniFrame.Core
{
    /// <summary>
    /// 设备错误信息
        /// </summary>
    public class DeviceError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public DateTime Time { get; set; }
    }

    /// <summary>
    /// 设备管理器 - 管理所有硬件设备
        /// </summary>
    public class DeviceManager : IDeviceManager
    {
        private readonly object _lock = new object();
        private Dictionary<string, IDevice> _devices;
        private List<DeviceError> _errors;


        public bool IsReady { get; private set; }
        public bool HasError { get { lock (_lock) return _errors.Count > 0; } }
        public int DeviceCount { get { lock (_lock) return _devices.Count; } }

        public event EventHandler<DeviceError> DeviceErrorOccurred;
        public event EventHandler<string> DeviceConnected;
        public event EventHandler<string> DeviceDisconnected;

        public DeviceManager()
        {
            _devices = new Dictionary<string, IDevice>();
            _errors = new List<DeviceError>();
        }

        public bool Initialize()
        {
            try
            {
                Logger.Info("初始化设备管理器...");
                LoadDeviceConfiguration();
                Logger.Info("设备管理器初始化完成");
                return true;
            }
            catch (IOException ex)
            {
                Logger.Error("设备管理器初始化失败(IO错误)", ex);
                return false;
            }
            catch (XmlException ex)
            {
                Logger.Error("设备管理器初始化失败(XML解析错误)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("设备管理器初始化失败", ex);
                return false;
            }
        }

        private void LoadDeviceConfiguration()
        {
            string configFile = Path.Combine(AppContext.BaseDirectory, "Config", "DeviceConfig.xml");
            if (!File.Exists(configFile))
            {
                Logger.Warning("设备配置文件不存在，跳过加载");
                return;
            }

            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.Load(configFile);

                var deviceNodes = doc.SelectNodes("//Device");
                if (deviceNodes == null || deviceNodes.Count == 0)
                {
                    Logger.Warning("设备配置文件中未定义设备");
                    return;
                }

                foreach (System.Xml.XmlNode node in deviceNodes)
                {
                    string name = node.Attributes?["Name"]?.Value;
                    string type = node.Attributes?["Type"]?.Value;
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type))
                        continue;

                    Logger.Info($"配置文件定义设备: {name} ({type})");
                }

                Logger.Info($"设备配置加载完成，共 {deviceNodes.Count} 个设备定义");
            }
            catch (Exception ex)
            {
                Logger.Error("加载设备配置文件失败", ex);
            }
        }

        /// <summary>
        /// 添加任意设备（统一入口）
        /// </summary>
        public bool AddDevice(string name, IDevice device)
        {
            if (device == null)
                return false;

            lock (_lock)
            {
                if (_devices.ContainsKey(name))
                {
                    Logger.Warning($"设备 {name} 已存在");
                    return false;
                }

                _devices[name] = device;
                SubscribeDeviceEvents(device);
                Logger.Info($"添加设备: {name} ({device.GetType().Name})");
                return true;
            }
        }

        /// <summary>
        /// 获取指定类型的设备
        /// </summary>
        public TDevice GetDevice<TDevice>(string name) where TDevice : class, IDevice
        {
            lock (_lock)
            {
                if (_devices.TryGetValue(name, out var device) && device is TDevice typed)
                    return typed;
                return null;
            }
        }

        public bool AddMotionDevice(string name, MotionDevice device)
        {
            return AddDevice(name, device);
        }

        public bool AddPlcDevice(string name, PlcDevice device)
        {
            return AddDevice(name, device);
        }

        public bool AddCustomDevice(string name, IDevice device)
        {
            return AddDevice(name, device);
        }

        public MotionDevice GetMotionDevice(string name)
        {
            return GetDevice<MotionDevice>(name);
        }

        public PlcDevice GetPlcDevice(string name)
        {
            return GetDevice<PlcDevice>(name);
        }

        public List<string> GetMotionDeviceNames()
        {
            lock (_lock)
            {
                return _devices
                    .Where(kvp => kvp.Value is MotionDevice)
                    .Select(kvp => kvp.Key)
                    .ToList();
            }
        }

        public List<string> GetPlcDeviceNames()
        {
            lock (_lock)
            {
                return _devices
                    .Where(kvp => kvp.Value is PlcDevice)
                    .Select(kvp => kvp.Key)
                    .ToList();
            }
        }

        public bool Start()
        {
            try
            {
                Logger.Info("启动所有设备...");
                bool allSuccess = true;

                lock (_lock)
                {
                    foreach (var kvp in _devices)
                    {
                        bool success = kvp.Value.Connect();
                        if (!success)
                        {
                            Logger.Error($"设备 {kvp.Key} 连接失败");
                            AddError($"DEVICE_{kvp.Key}", $"设备 {kvp.Key} 连接失败", kvp.Key);
                            allSuccess = false;
                        }
                        else
                        {
                            DeviceConnected?.Invoke(this, kvp.Key);
                        }
                    }

                    IsReady = allSuccess;
                }
                return allSuccess;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("启动设备失败(操作无效)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("启动设备失败", ex);
                return false;
            }
        }

        public void Stop()
        {
            Logger.Info("停止所有设备...");

            lock (_lock)
            {
                foreach (var kvp in _devices)
                {
                    kvp.Value.Disconnect();
                    DeviceDisconnected?.Invoke(this, kvp.Key);
                }

                IsReady = false;
            }
        }

        public void EmergencyStop()
        {
            Logger.Error("执行设备紧急停止!");

            lock (_lock)
            {
                foreach (var kvp in _devices)
                {
                    if (kvp.Value is MotionDevice motion)
                    {
                        int axisCount = motion.AxisCount;
                        for (int i = 0; i < axisCount; i++)
                        {
                            try { motion.StopAxis(i); }
                            catch (Exception ex) { Logger.Error($"紧急停止轴 {i} 失败: {motion.Name}", ex); }
                        }
                    }
                }
            }
        }

        public bool Reset()
        {
            try
            {
                Logger.Info("复位设备...");
                ClearErrors();

                bool allSuccess = true;

                lock (_lock)
                {
                    foreach (var kvp in _devices)
                    {
                        if (!kvp.Value.Reset())
                        {
                            AddError($"RESET_{kvp.Key}", $"设备 {kvp.Key} 复位失败", kvp.Key);
                            allSuccess = false;
                        }
                    }

                    IsReady = allSuccess;
                }
                return allSuccess;
            }
            catch (Exception ex)
            {
                Logger.Error("复位设备失败", ex);
                return false;
            }
        }

        private void SubscribeDeviceEvents(IDevice device)
        {
            if (device is MotionDevice motion)
                motion.ErrorOccurred += OnMotionDeviceError;
            else if (device is PlcDevice plc)
                plc.ErrorOccurred += OnPlcDeviceError;
        }

        private void OnMotionDeviceError(object sender, MotionErrorEventArgs e)
        {
            AddError(e.ErrorCode, e.ErrorMessage, "Motion");
        }

        private void OnPlcDeviceError(object sender, PlcErrorEventArgs e)
        {
            AddError(e.ErrorCode, e.ErrorMessage, "PLC");
        }

        private void AddError(string code, string message, string source)
        {
            var error = new DeviceError
            {
                Code = code,
                Message = message,
                Source = source,
                Time = DateTime.Now
            };

            lock (_lock)
            {
                _errors.Add(error);
            }

            DeviceErrorOccurred?.Invoke(this, error);
        }

        public List<DeviceError> GetErrors()
        {
            lock (_lock)
            {
                return _errors.ToList();
            }
        }

        public void ClearErrors()
        {
            lock (_lock)
            {
                _errors.Clear();
            }
        }

        public DeviceStatusSummary GetStatusSummary()
        {
            var summary = new DeviceStatusSummary();

            lock (_lock)
            {
                summary.TotalDeviceCount = _devices.Count;
                summary.MotionDeviceCount = _devices.Count(kvp => kvp.Value is MotionDevice);
                summary.PlcDeviceCount = _devices.Count(kvp => kvp.Value is PlcDevice);
                summary.CustomDeviceCount = _devices.Count(kvp => kvp.Value is IDevice
                    && !(kvp.Value is MotionDevice) && !(kvp.Value is PlcDevice));
                summary.ErrorCount = _errors.Count;
                summary.IsReady = IsReady;

                summary.MotionDeviceStatus = _devices
                    .Where(kvp => kvp.Value is MotionDevice)
                    .ToDictionary(kvp => kvp.Key, kvp => ((MotionDevice)kvp.Value).IsInitialized);

                summary.PlcDeviceStatus = _devices
                    .Where(kvp => kvp.Value is PlcDevice)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.IsConnected);
            }

            return summary;
        }

        public void Dispose()
        {
            Stop();

            lock (_lock)
            {
                foreach (var device in _devices.Values)
                {
                    device.Dispose();
                }
            }
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// 设备状态摘要
        /// </summary>
    public class DeviceStatusSummary
    {
        public int TotalDeviceCount { get; set; }
        public int MotionDeviceCount { get; set; }
        public int PlcDeviceCount { get; set; }
        public int CustomDeviceCount { get; set; }
        public int ErrorCount { get; set; }
        public bool IsReady { get; set; }
        public Dictionary<string, bool> MotionDeviceStatus { get; set; }
        public Dictionary<string, bool> PlcDeviceStatus { get; set; }
    }
}
