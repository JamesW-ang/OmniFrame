using System;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.Core.PluginSystem;

namespace OmniFrame.Core
{
    /// <summary>
    /// 系统管理器 - 系统核心管理类
        /// </summary>
    public class SystemManager : ISystemManager
    {
        private readonly object _lock = new object();
        private bool _isInitialized;
        private bool _isRunning;
        private Task _mainLoopTask;
        private CancellationTokenSource _cancellationTokenSource;

        // DI注入的子系统管理器（C# 表达式主体属性保证只读访问）
        private readonly IDeviceManager _deviceMgr;
        private readonly ITaskManager _taskMgr;
        private readonly IAlarmManager _alarmMgr;
        private readonly IDataManager _dataMgr;
        private readonly IUserManager _userMgr;
        private readonly IRecipeManager _recipeMgr;
        private readonly IPermissionManager _permissionMgr;
        private readonly IPluginManager _pluginMgr;
        private readonly IPlcManager _plcManager;
        private readonly IMotionManager _motionManager;
        private readonly IIoManager _ioManager;
        private readonly IProductManager _productManager;

        // 子系统管理器
        public IDeviceManager DeviceMgr => _deviceMgr;
        public ITaskManager TaskMgr => _taskMgr;
        public IAlarmManager AlarmMgr => _alarmMgr;
        public IDataManager DataMgr => _dataMgr;
        public IUserManager UserMgr => _userMgr;
        public IRecipeManager RecipeMgr => _recipeMgr;
        public IPlcManager PlcManager => _plcManager;
        public IMotionManager MotionManager => _motionManager;
        public IIoManager IoManager => _ioManager;
        public IProductManager ProductManager => _productManager;

        // 系统状态
        public SystemState State { get; private set; }
        public DateTime StartTime { get; private set; }
        public TimeSpan RunningTime => DateTime.Now - StartTime;
        public bool IsRunning => _isRunning;

        // 事件
        public event EventHandler<SystemStateChangedEventArgs> StateChanged;
        public event EventHandler<SystemErrorEventArgs> ErrorOccurred;
        public event EventHandler<EventArgs> SystemStarted;
        public event EventHandler<EventArgs> SystemStopped;

        /// <summary>
        /// 获取系统管理器实例（从DI容器解析）
        /// </summary>

        /// <summary>
        /// 构造函数（所有子管理器由DI容器注入）
        /// </summary>
        public SystemManager(
            IDeviceManager deviceMgr,
            ITaskManager taskMgr,
            IAlarmManager alarmMgr,
            IDataManager dataMgr,
            IUserManager userMgr,
            IRecipeManager recipeMgr,
            IPermissionManager permissionMgr,
            IPluginManager pluginMgr,
            IPlcManager plcManager,
            IMotionManager motionManager,
            IIoManager ioManager,
            IProductManager productManager)
        {
            _deviceMgr = deviceMgr ?? throw new ArgumentNullException(nameof(deviceMgr));
            _taskMgr = taskMgr ?? throw new ArgumentNullException(nameof(taskMgr));
            _alarmMgr = alarmMgr ?? throw new ArgumentNullException(nameof(alarmMgr));
            _dataMgr = dataMgr ?? throw new ArgumentNullException(nameof(dataMgr));
            _userMgr = userMgr ?? throw new ArgumentNullException(nameof(userMgr));
            _recipeMgr = recipeMgr ?? throw new ArgumentNullException(nameof(recipeMgr));
            _permissionMgr = permissionMgr ?? throw new ArgumentNullException(nameof(permissionMgr));
            _pluginMgr = pluginMgr ?? throw new ArgumentNullException(nameof(pluginMgr));
            _plcManager = plcManager ?? throw new ArgumentNullException(nameof(plcManager));
            _motionManager = motionManager ?? throw new ArgumentNullException(nameof(motionManager));
            _ioManager = ioManager ?? throw new ArgumentNullException(nameof(ioManager));
            _productManager = productManager ?? throw new ArgumentNullException(nameof(productManager));

            State = SystemState.Idle;
        }

        /// <summary>
        /// 初始化系统（使用DI注入的实例，不创建新对象）
        /// </summary>
        public bool Initialize()
        {
            lock (_lock)
            {
                if (_isInitialized)
                    return true;

                try
                {
                    Logger.Info("开始初始化系统...");

                    // 使用DI注入的设备管理器实例
                    if (!_deviceMgr.Initialize())
                    {
                        Logger.Error("设备管理器初始化失败");
                        return false;
                    }

                    // 使用DI注入的任务管理器实例
                    if (!_taskMgr.Initialize())
                    {
                        Logger.Error("任务管理器初始化失败");
                        return false;
                    }

                    // 使用DI注入的报警管理器实例
                    if (!_alarmMgr.Initialize())
                    {
                        Logger.Error("报警管理器初始化失败");
                        return false;
                    }

                    // 告警通知管理器由DI管理生命周期
                    Logger.Info("告警通知管理器初始化完成");

                    // 使用DI注入的插件管理器实例
                    if (!_pluginMgr.Initialize())
                    {
                        Logger.Error("插件管理器初始化失败");
                        return false;
                    }
                    Logger.Info("插件管理器初始化完成");

                    // 使用DI注入的数据管理器实例
                    if (!_dataMgr.Initialize())
                    {
                        Logger.Error("数据管理器初始化失败");
                        return false;
                    }

                    // 使用DI注入的用户管理器实例
                    if (!_userMgr.Initialize())
                    {
                        Logger.Error("用户管理器初始化失败");
                        return false;
                    }

                    // 使用DI注入的权限管理器实例
                    _permissionMgr.Initialize(_userMgr);
                    Logger.Info("权限管理器初始化完成");

                    // 使用DI注入的配方管理器实例
                    if (!_recipeMgr.Initialize())
                    {
                        Logger.Error("配方管理器初始化失败");
                        return false;
                    }

                    _isInitialized = true;
                    Logger.Info("系统初始化完成");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error("系统初始化失败", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 启动系统
        /// </summary>
        public bool Start()
        {
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    Logger.Error("系统未初始化，无法启动");
                    return false;
                }

                if (_isRunning)
                    return true;

                try
                {
                    var systemInfo = GetSystemInfo();
                    Logger.Info($"启动系统... [当前状态: {systemInfo.State}, 设备数: {systemInfo.DeviceCount}, 当前用户: {systemInfo.CurrentUser}]");

                    // 启动设备管理器
                    if (!DeviceMgr.Start())
                    {
                        Logger.Error("设备管理器启动失败");
                        return false;
                    }

                    // 启动任务管理器
                    if (!TaskMgr.Start())
                    {
                        Logger.Error("任务管理器启动失败");
                        DeviceMgr.Stop();
                        return false;
                    }

                    // 启动数据管理器
                    if (!DataMgr.Start())
                    {
                        Logger.Error("数据管理器启动失败");
                        TaskMgr.Stop();
                        DeviceMgr.Stop();
                        return false;
                    }

                    // 启动主循环
                    _cancellationTokenSource = new CancellationTokenSource();
                    _mainLoopTask = Task.Factory.StartNew(MainLoop, TaskCreationOptions.LongRunning);

                    _isRunning = true;
                    StartTime = DateTime.Now;
                    ChangeState(SystemState.Ready);

                    SystemStarted?.Invoke(this, EventArgs.Empty);
                    systemInfo = GetSystemInfo();
                    Logger.Info($"系统启动完成 [新状态: {systemInfo.State}, 启动时间: {StartTime}, 当前用户: {systemInfo.CurrentUser}]");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error("系统启动失败", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 停止系统
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                if (!_isRunning)
                    return;

                var systemInfo = GetSystemInfo();
                Logger.Info($"停止系统... [当前状态: {systemInfo.State}, 运行时间: {systemInfo.RunningTime}, 当前用户: {systemInfo.CurrentUser}]");

                // 停止主循环
                _cancellationTokenSource?.Cancel();
                _mainLoopTask?.Wait(5000);

                // 停止各子系统
                TaskMgr?.Stop();
                DataMgr?.Stop();
                DeviceMgr?.Stop();

                _isRunning = false;
                ChangeState(SystemState.Idle);

                SystemStopped?.Invoke(this, EventArgs.Empty);
                systemInfo = GetSystemInfo();
                Logger.Info($"系统已停止 [新状态: {systemInfo.State}, 当前用户: {systemInfo.CurrentUser}]");
            }
        }

        /// <summary>
        /// 主循环
        /// </summary>
        private void MainLoop()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // 更新系统状态
                    UpdateSystemState();

                    // 检查报警
                    CheckAlarms();

                    // 执行周期性任务
                    PeriodicTasks();

                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Logger.Error("主循环异常", ex);
                    ErrorOccurred?.Invoke(this, new SystemErrorEventArgs
                    {
                        ErrorMessage = "主循环异常",
                        Exception = ex
                    });
                }
            }
        }

        /// <summary>
        /// 更新系统状态
        /// </summary>
        private void UpdateSystemState()
        {
            SystemState newState;
            if (DeviceMgr.HasError || AlarmMgr?.HasCriticalAlarm == true)
                newState = SystemState.Error;
            else if (TaskMgr.IsRunning)
                newState = SystemState.Running;
            else if (DeviceMgr.IsReady)
                newState = SystemState.Ready;
            else
                return;

            if (State != newState)
            {
                lock (_lock)
                {
                    if (State != newState)
                        ChangeState(newState);
                }
            }
        }

        /// <summary>
        /// 检查报警
        /// </summary>
        private void CheckAlarms()
        {
            // 设备报警检查
            if (DeviceMgr.HasError)
            {
                var errors = DeviceMgr.GetErrors();
                foreach (var error in errors)
                {
                    AlarmMgr.AddAlarm(error.Code, error.Message, AlarmLevel.Error, error.Source);
                }
            }
        }

        /// <summary>
        /// 执行周期性任务
        /// </summary>
        private void PeriodicTasks()
        {
            // 数据保存
            DataMgr?.PeriodicSave();

            // 状态记录
            if (DateTime.Now.Second == 0)
            {
                DataMgr?.LogSystemStatus();
            }
        }

        /// <summary>
        /// 改变系统状态
        /// </summary>
        private void ChangeState(SystemState newState)
        {
            var oldState = State;
            State = newState;
            StateChanged?.Invoke(this, new SystemStateChangedEventArgs
            {
                OldState = oldState,
                NewState = newState
            });
            Logger.Info($"系统状态变更: {oldState} -> {newState}");
        }

        /// <summary>
        /// 紧急停止
        /// </summary>
        public void EmergencyStop()
        {
            lock (_lock)
            {
                var systemInfo = GetSystemInfo();
                Logger.Error($"执行紧急停止! [当前状态: {systemInfo.State}, 运行时间: {systemInfo.RunningTime}, 当前用户: {systemInfo.CurrentUser}]");
                ChangeState(SystemState.EmergencyStop);

                DeviceMgr?.EmergencyStop();
                TaskMgr?.EmergencyStop();
                AlarmMgr?.AddAlarm("ESTOP001", "紧急停止被触发", AlarmLevel.Critical, "System");

                systemInfo = GetSystemInfo();
                Logger.Error($"紧急停止执行完成 [新状态: {systemInfo.State}]");
            }
        }

        /// <summary>
        /// 复位系统
        /// </summary>
        public bool Reset()
        {
            lock (_lock)
            {
                if (State != SystemState.Error && State != SystemState.EmergencyStop)
                    return false;

                var systemInfo = GetSystemInfo();
                Logger.Info($"复位系统... [当前状态: {systemInfo.State}, 活跃报警数: {systemInfo.ActiveAlarmCount}, 当前用户: {systemInfo.CurrentUser}]");

                // 清除报警
                AlarmMgr?.ClearAllAlarms();

                // 复位设备
                if (DeviceMgr?.Reset() == true)
                {
                    ChangeState(SystemState.Ready);
                    systemInfo = GetSystemInfo();
                    Logger.Info($"系统复位完成 [新状态: {systemInfo.State}, 当前用户: {systemInfo.CurrentUser}]");
                    return true;
                }

                Logger.Warning("系统复位失败");
                return false;
            }
        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        public SystemInfo GetSystemInfo()
        {
            return new SystemInfo
            {
                State = State,
                IsRunning = _isRunning,
                IsInitialized = _isInitialized,
                StartTime = StartTime,
                RunningTime = RunningTime,
                DeviceCount = DeviceMgr?.DeviceCount ?? 0,
                ActiveAlarmCount = AlarmMgr?.ActiveAlarmCount ?? 0,
                CurrentUser = UserMgr?.CurrentUser?.UserName ?? "未登录",
                CurrentRecipe = RecipeMgr?.CurrentRecipe?.Name ?? "未选择"
            };
        }

        /// <summary>
        /// 释放资源（只停止循环，DI容器管理子管理器生命周期）
        /// </summary>
        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }

    /// <summary>
    /// 系统状态
        /// </summary>
    public enum SystemState
    {
        Idle,           // 空闲
        Initializing,   // 初始化中
        Ready,          // 就绪
        Running,        // 运行中
        Paused,         // 暂停
        Error,          // 错误
        EmergencyStop   // 紧急停止
    }

    /// <summary>
    /// 系统状态变更事件参数
        /// </summary>
    public class SystemStateChangedEventArgs : EventArgs
    {
        public SystemState OldState { get; set; }
        public SystemState NewState { get; set; }
    }

    /// <summary>
    /// 系统错误事件参数
        /// </summary>
    public class SystemErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// 系统信息
        /// </summary>
    public class SystemInfo
    {
        public SystemState State { get; set; }
        public bool IsRunning { get; set; }
        public bool IsInitialized { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan RunningTime { get; set; }
        public int DeviceCount { get; set; }
        public int ActiveAlarmCount { get; set; }
        public string CurrentUser { get; set; }
        public string CurrentRecipe { get; set; }
    }
}
