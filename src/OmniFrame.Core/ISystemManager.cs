using System;

namespace OmniFrame.Core
{
    /// <summary>
    /// 系统管理器接口
    /// </summary>
    public interface ISystemManager : IDisposable
    {
        // 子系统管理器（由DI容器注入）
        IDeviceManager DeviceMgr { get; }
        ITaskManager TaskMgr { get; }
        IAlarmManager AlarmMgr { get; }
        IDataManager DataMgr { get; }
        IUserManager UserMgr { get; }
        IRecipeManager RecipeMgr { get; }
        IPlcManager PlcManager { get; }
        IMotionManager MotionManager { get; }
        IIoManager IoManager { get; }
        IProductManager ProductManager { get; }

        // 系统状态
        SystemState State { get; }
        DateTime StartTime { get; }
        TimeSpan RunningTime { get; }
        bool IsRunning { get; }

        // 事件
        event EventHandler<SystemStateChangedEventArgs> StateChanged;
        event EventHandler<SystemErrorEventArgs> ErrorOccurred;
        event EventHandler<EventArgs> SystemStarted;
        event EventHandler<EventArgs> SystemStopped;

        // 方法
        bool Initialize();
        bool Start();
        void Stop();
        void EmergencyStop();
        bool Reset();
        SystemInfo GetSystemInfo();
    }
}
