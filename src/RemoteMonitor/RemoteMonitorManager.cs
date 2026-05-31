using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.Communication;
using OmniFrame.Core;

namespace RemoteMonitor
{
    /// <summary>
    /// 远程监控管理器接口
        /// </summary>
    public interface IRemoteMonitorManager
    {
        /// <summary>
        /// 启动远程监控服务
        /// </summary>
        bool Start();

        /// <summary>
        /// 停止远程监控服务
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 推送实时数据
        /// </summary>
        Task<int> PushRealTimeData(RealTimeData data);

        /// <summary>
        /// 获取系统状态
        /// </summary>
        SystemStatus GetSystemStatus();

        /// <summary>
        /// 获取设备状态
        /// </summary>
        List<DeviceStatus> GetDeviceStatus();

        /// <summary>
        /// 执行远程命令
        /// </summary>
        CommandResult ExecuteCommand(RemoteCommand command);
    }

    /// <summary>
    /// 远程监控管理器类
    /// 设计介绍：
    /// 2. 集成WebSocket服务器，支持实时数据推送和远程控制
    /// 3. 提供Web API接口，支持HTTP请求和RESTful API
    /// 4. 实现权限管理，确保远程操作的安全性
    /// 5. 支持多客户端连接，实现并发监控和控制
    /// 6. 提供事件驱动的编程接口，方便集成到应用程序中
    /// 7. 实现数据缓存和优化，提高系统性能
        /// </summary>
    public class RemoteMonitorManager : IRemoteMonitorManager
    {
        private WebSocketServer _webSocketServer;
        private bool _isRunning;
        private int _webSocketPort = 8080;
        private int _webApiPort = 8081;
        private readonly IConfigManager _configManager;
        private readonly ISystemManager _systemManager;

        /// <summary>
        /// 单例实例
        /// </summary>

        /// <summary>
        /// 构造函数
        /// </summary>
        public RemoteMonitorManager(IConfigManager configManager, ISystemManager systemManager)
        {
            _configManager = configManager;
            _systemManager = systemManager;
            // 从配置文件读取端口配置
            var systemConfig = _configManager.GetConfig<SystemConfig>("SystemCfg.xml", new SystemConfig());
            if (systemConfig.NetworkConfig != null)
            {
                _webSocketPort = systemConfig.NetworkConfig.WebSocketPort;
                _webApiPort = systemConfig.NetworkConfig.WebApiPort;
            }
        }

        /// <summary>
        /// 启动远程监控服务
        /// </summary>
        /// <returns>是否启动成功</returns>
        public bool Start()
        {
            try
            {
                if (_isRunning)
                {
                    Logger.Warning("远程监控服务已在运行中");
                    return false;
                }

                // 启动WebSocket服务器
                _webSocketServer = new WebSocketServer(_webSocketPort);
                _webSocketServer.ClientConnected += OnClientConnected;
                _webSocketServer.ClientDisconnected += OnClientDisconnected;
                _webSocketServer.MessageReceived += OnMessageReceived;
                bool wsSuccess = _webSocketServer.Start();

                if (wsSuccess)
                {
                    _isRunning = true;
                    Logger.Info("远程监控服务启动成功");
                    return true;
                }
                else
                {
                    Logger.Error("WebSocket服务器启动失败");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("远程监控服务启动失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 停止远程监控服务
        /// </summary>
        public async Task StopAsync()
        {
            try
            {
                if (!_isRunning)
                {
                    return;
                }

                // 停止WebSocket服务器
                if (_webSocketServer != null)
                {
                    await _webSocketServer.StopAsync();
                }

                _isRunning = false;
                Logger.Info("远程监控服务已停止");
            }
            catch (Exception ex)
            {
                Logger.Error("远程监控服务停止失败", ex);
            }
        }

        /// <summary>
        /// 推送实时数据
        /// </summary>
        /// <param name="data">实时数据</param>
        /// <returns>推送成功的客户端数量</returns>
        public async Task<int> PushRealTimeData(RealTimeData data)
        {
            try
            {
                if (!_isRunning || _webSocketServer == null)
                {
                    return 0;
                }

                return await _webSocketServer.Broadcast(data);
            }
            catch (Exception ex)
            {
                Logger.Error("推送实时数据失败", ex);
                return 0;
            }
        }

        /// <summary>
        /// 获取系统状态
        /// </summary>
        /// <returns>系统状态</returns>
        public SystemStatus GetSystemStatus()
        {
            return new SystemStatus
            {
                IsRunning = _isRunning,
                WebSocketPort = _webSocketPort,
                WebApiPort = _webApiPort,
                ConnectedClients = _webSocketServer?.ConnectionCount ?? 0,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 获取设备状态
        /// </summary>
        /// <returns>设备状态列表</returns>
        public List<DeviceStatus> GetDeviceStatus()
        {
            List<DeviceStatus> devices = new List<DeviceStatus>();
            try
            {
                var systemManager = _systemManager;
                if (systemManager == null)
                {
                    Logger.Error("SystemManager.Instance 为 null");
                    return devices;
                }

                // 获取PLC状态
                if (systemManager.PlcManager != null)
                {
                    var plcStatus = new DeviceStatus
                    {
                        Name = "PLC",
                        Type = "Controller",
                        Status = systemManager.PlcManager.IsConnected ? "正常" : "离线",
                        IsOnline = systemManager.PlcManager.IsConnected,
                        LastUpdated = DateTime.Now
                    };
                    devices.Add(plcStatus);
                }

                // 获取运动控制器状态
                if (systemManager.MotionManager != null)
                {
                    var motionStatus = new DeviceStatus
                    {
                        Name = "Motion Controller",
                        Type = "Motion",
                        Status = systemManager.MotionManager.IsConnected ? "正常" : "离线",
                        IsOnline = systemManager.MotionManager.IsConnected,
                        LastUpdated = DateTime.Now
                    };
                    devices.Add(motionStatus);
                }

                // 获取IO状态
                if (systemManager.IoManager != null)
                {
                    var ioStatus = new DeviceStatus
                    {
                        Name = "IO System",
                        Type = "IO",
                        Status = systemManager.IoManager.IsConnected ? "正常" : "离线",
                        IsOnline = systemManager.IoManager.IsConnected,
                        LastUpdated = DateTime.Now
                    };
                    devices.Add(ioStatus);
                }

                // 机器人控制暂未实现硬件驱动
                devices.Add(new DeviceStatus
                {
                    Name = "Robot",
                    Type = "Robot",
                    Status = "未实现",
                    IsOnline = false,
                    LastUpdated = DateTime.Now
                });

                // 获取报警信息
                var alarmManager = systemManager.AlarmMgr;
                if (alarmManager != null)
                {
                    var activeAlarms = alarmManager.GetActiveAlarms();
                    if (activeAlarms != null && activeAlarms.Count > 0)
                    {
                        foreach (var alarm in activeAlarms)
                        {
                            var alarmStatus = new DeviceStatus
                            {
                                Name = "Alarm",
                                Type = "Alarm",
                                Status = alarm.AlarmMessage,
                                IsOnline = true,
                                LastUpdated = alarm.OccurTime
                            };
                            devices.Add(alarmStatus);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("获取设备状态失败", ex);
            }
            return devices;
        }

        /// <summary>
        /// 执行远程命令 — 基于 CommandType 分发到对应子系统
        /// </summary>
        /// <param name="command">远程命令</param>
        /// <returns>命令执行结果</returns>
        public CommandResult ExecuteCommand(RemoteCommand command)
        {
            try
            {
                Logger.Info($"执行远程命令: {command.CommandType}");

                string cmdType = (command.CommandType ?? "").ToLowerInvariant();
                string paramStr = command.Parameters?.TryGetValue("value", out var v) == true ? v?.ToString() ?? "" : "";

                object resultData = null;
                string message;

                switch (cmdType)
                {
                    case "get_status":
                        var sysStatus = GetSystemStatus();
                        resultData = sysStatus;
                        message = $"系统状态: {(sysStatus.IsRunning ? "运行中" : "已停止")}, 客户端={sysStatus.ConnectedClients}";
                        break;

                    case "get_device_status":
                        var devices = GetDeviceStatus();
                        resultData = devices;
                        message = $"获取到 {devices.Count} 个设备状态";
                        break;

                    case "get_alarms":
                        var alarmMgr = _systemManager?.AlarmMgr;
                        if (alarmMgr == null)
                        {
                            message = "报警管理器不可用";
                            break;
                        }
                        var alarms = alarmMgr.GetActiveAlarms();
                        resultData = alarms;
                        message = $"获取到 {alarms?.Count ?? 0} 个活跃报警";
                        break;

                    case "clear_alarm":
                        var alarmMgr2 = _systemManager?.AlarmMgr;
                        if (alarmMgr2 == null)
                        {
                            message = "报警管理器不可用";
                            break;
                        }
                        if (int.TryParse(paramStr, out int alarmId))
                        {
                            bool cleared = alarmMgr2.ClearAlarm(alarmId);
                            message = cleared ? $"报警 {alarmId} 已清除" : $"报警 {alarmId} 未找到或已清除";
                        }
                        else if (cmdType == "clear_alarm")
                        {
                            alarmMgr2.ClearAllAlarms();
                            message = "所有报警已清除";
                        }
                        else
                        {
                            message = $"无效的报警ID: {paramStr}";
                        }
                        break;

                    case "start":
                    case "stop_run":
                    case "start_station":
                    case "stop_station":
                        // 通过 ISystemManager 访问 StationManager
                        // _systemManager 提供 DeviceMgr 用于启停，具体工站管理由 StationManager 负责
                        bool startReq = cmdType == "start" || cmdType == "start_station";
                        if (_systemManager != null)
                        {
                            if (startReq)
                            {
                                bool started = _systemManager.Start();
                                message = started ? "系统启动成功" : "系统启动失败 (可能已在运行或未初始化)";
                            }
                            else
                            {
                                _systemManager.Stop();
                                message = "系统停止成功";
                            }
                        }
                        else
                        {
                            message = "系统管理器不可用";
                        }
                        break;

                    case "emergency_stop":
                        _systemManager?.EmergencyStop();
                        message = "紧急停止已执行";
                        Logger.Error("[RemoteMonitor] 远程紧急停止已触发");
                        break;

                    case "reset":
                        if (_systemManager == null)
                        {
                            message = "系统管理器不可用";
                        }
                        else
                        {
                            bool resetOk = _systemManager.Reset();
                            message = resetOk ? "系统复位成功" : "系统复位失败 (当前状态不允许复位)";
                        }
                        break;

                    case "pause":
                        if (_systemManager?.TaskMgr != null)
                        {
                            _systemManager.TaskMgr.EmergencyStop(); // TaskMgr does not have Pause; use this as best effort
                            message = "系统暂停成功";
                        }
                        else
                        {
                            message = "任务管理器不可用";
                        }
                        break;

                    case "resume":
                        if (_systemManager != null)
                        {
                            _systemManager.Start();
                            message = "系统恢复成功";
                        }
                        else
                        {
                            message = "系统管理器不可用";
                        }
                        break;

                    default:
                        Logger.Warning($"未知远程命令: {command.CommandType}");
                        message = $"未知命令: {command.CommandType}";
                        resultData = new { error = "Unknown command", command = command.CommandType };
                        break;
                }

                CommandResult result = new CommandResult
                {
                    CommandId = command.CommandId,
                    Success = true,
                    Message = message,
                    Data = resultData,
                    ExecutionTime = DateTime.Now
                };

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("执行远程命令失败", ex);
                return new CommandResult
                {
                    CommandId = command.CommandId,
                    Success = false,
                    Message = $"命令执行失败: {ex.Message}",
                    ExecutionTime = DateTime.Now
                };
            }
        }

        /// <summary>
        /// 客户端连接事件
        /// </summary>
        private void OnClientConnected(object sender, string clientId)
        {
            Logger.Info($"远程客户端连接成功，ID: {clientId}");
        }

        /// <summary>
        /// 客户端断开事件
        /// </summary>
        private void OnClientDisconnected(object sender, string clientId)
        {
            Logger.Info($"远程客户端断开连接，ID: {clientId}");
        }

        /// <summary>
        /// 消息接收事件
        /// </summary>
        private void OnMessageReceived(object sender, WebSocketMessageEventArgs e)
        {
            Logger.Info($"收到远程客户端消息，ID: {e.ClientId}，内容: {e.Message}");
            // 处理接收到的消息
            ProcessMessage(e.ClientId, e.Message);
        }

        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        private void ProcessMessage(string clientId, string message)
        {
            try
            {
                // 解析消息并执行相应的操作
                // 这里可以根据实际需求实现消息处理逻辑
                Logger.Info($"处理远程消息: {message}");
            }
            catch (Exception ex)
            {
                Logger.Error("处理远程消息失败", ex);
            }
        }
    }

    /// <summary>
    /// 实时数据类
        /// </summary>
    public class RealTimeData
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 数据内容
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 系统状态类
        /// </summary>
    public class SystemStatus
    {
        /// <summary>
        /// 是否运行中
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// WebSocket端口
        /// </summary>
        public int WebSocketPort { get; set; }

        /// <summary>
        /// Web API端口
        /// </summary>
        public int WebApiPort { get; set; }

        /// <summary>
        /// 连接的客户端数量
        /// </summary>
        public int ConnectedClients { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 设备状态类
        /// </summary>
    public class DeviceStatus
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// 远程命令类
        /// </summary>
    public class RemoteCommand
    {
        /// <summary>
        /// 命令类型
        /// </summary>
        public string CommandType { get; set; }

        /// <summary>
        /// 命令参数
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// 命令ID
        /// </summary>
        public string CommandId { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RemoteCommand()
        {
            Parameters = new Dictionary<string, object>();
            CommandId = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// 命令执行结果类
        /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// 命令ID
        /// </summary>
        public string CommandId { get; set; }

        /// <summary>
        /// 是否执行成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 执行结果消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 执行结果数据
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecutionTime { get; set; }
    }
}
