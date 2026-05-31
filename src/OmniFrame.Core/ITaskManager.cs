using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    /// <summary>
    /// 任务管理器接口
    /// </summary>
    public interface ITaskManager : IDisposable
    {
        bool IsRunning { get; }
        int PendingTaskCount { get; }
        int TotalTaskCount { get; }
        TaskBase CurrentTask { get; }

        event EventHandler<TaskBase> TaskStarted;
        event EventHandler<TaskBase> TaskCompleted;
        event EventHandler<TaskBase> TaskError;
        event EventHandler AllTasksCompleted;

        bool Initialize();
        bool Start();
        void Stop();
        void EmergencyStop();
        void AddTask(TaskBase task);
        void AddAndExecuteTask(TaskBase task);
        bool ExecuteTask(TaskBase task);
        List<TaskBase> GetAllTasks();
        List<TaskBase> GetTasksByState(TaskState state);
        void ClearQueue();
    }
}
