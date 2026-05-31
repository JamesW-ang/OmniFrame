using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 任务状态
        /// </summary>
    public enum TaskState
    {
        Idle,
        Running,
        Paused,
        Completed,
        Error,
        Aborted
    }

    /// <summary>
    /// 任务优先级
        /// </summary>
    public enum TaskPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    /// <summary>
    /// 任务基类 - 任务的抽象基类
        /// </summary>
    public abstract class TaskBase
    {
        public string TaskId { get; protected set; }
        public string TaskName { get; protected set; }
        public TaskState State { get; protected set; }
        public TaskPriority Priority { get; set; }
        public DateTime CreateTime { get; protected set; }
        public DateTime? StartTime { get; protected set; }
        public DateTime? CompleteTime { get; protected set; }
        public TimeSpan? Duration => CompleteTime.HasValue && StartTime.HasValue
            ? CompleteTime.Value - StartTime.Value
            : null;
        public string ErrorMessage { get; protected set; }
        public bool IsRunning => State == TaskState.Running;
        public bool IsCompleted => State == TaskState.Completed;
        public bool HasError => State == TaskState.Error;

        public event EventHandler<TaskStateChangedEventArgs> StateChanged;
        public event EventHandler<TaskCompletedEventArgs> Completed;
        public event EventHandler<TaskErrorEventArgs> ErrorOccurred;

        protected TaskBase(string taskName, TaskPriority priority = TaskPriority.Normal)
        {
            TaskId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            TaskName = taskName;
            Priority = priority;
            State = TaskState.Idle;
            CreateTime = DateTime.Now;
        }

        public bool Start()
        {
            if (State != TaskState.Idle && State != TaskState.Paused)
                return false;

            try
            {
                StartTime = DateTime.Now;
                ChangeState(TaskState.Running);

                if (Execute())
                {
                    Complete();
                    return true;
                }
                else
                {
                    Fail("任务执行失败");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Fail($"任务执行异常: {ex.Message}");
                return false;
            }
        }

        public void Pause()
        {
            if (State == TaskState.Running)
            {
                ChangeState(TaskState.Paused);
                OnPause();
            }
        }

        public void Resume()
        {
            if (State == TaskState.Paused)
            {
                ChangeState(TaskState.Running);
                OnResume();
            }
        }

        public void Abort()
        {
            if (State == TaskState.Running || State == TaskState.Paused)
            {
                OnAbort();
                ChangeState(TaskState.Aborted);
            }
        }

        protected abstract bool Execute();
        protected virtual void OnPause() { }
        protected virtual void OnResume() { }
        protected virtual void OnAbort() { }

        protected void Complete()
        {
            CompleteTime = DateTime.Now;
            ChangeState(TaskState.Completed);
            Completed?.Invoke(this, new TaskCompletedEventArgs
            {
                TaskId = TaskId,
                TaskName = TaskName,
                Duration = Duration
            });
        }

        protected void Fail(string errorMessage)
        {
            ErrorMessage = errorMessage;
            CompleteTime = DateTime.Now;
            ChangeState(TaskState.Error);
            ErrorOccurred?.Invoke(this, new TaskErrorEventArgs
            {
                TaskId = TaskId,
                TaskName = TaskName,
                ErrorMessage = errorMessage
            });
        }

        protected void ChangeState(TaskState newState)
        {
            var oldState = State;
            State = newState;
            StateChanged?.Invoke(this, new TaskStateChangedEventArgs
            {
                TaskId = TaskId,
                TaskName = TaskName,
                OldState = oldState,
                NewState = newState
            });
        }

        public virtual void Reset()
        {
            State = TaskState.Idle;
            StartTime = null;
            CompleteTime = null;
            ErrorMessage = null;
        }
    }

    /// <summary>
    /// 任务状态变更事件参数
        /// </summary>
    public class TaskStateChangedEventArgs : EventArgs
    {
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public TaskState OldState { get; set; }
        public TaskState NewState { get; set; }
    }

    /// <summary>
    /// 任务完成事件参数
        /// </summary>
    public class TaskCompletedEventArgs : EventArgs
    {
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public TimeSpan? Duration { get; set; }
    }

    /// <summary>
    /// 任务错误事件参数
        /// </summary>
    public class TaskErrorEventArgs : EventArgs
    {
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 任务管理器 - 管理任务的添加、执行、队列
        /// </summary>
    public class TaskManager : IDisposable, ITaskManager
    {
        private readonly object _lock = new object();
        private List<TaskBase> _tasks;
        private Queue<TaskBase> _taskQueue;
        private Task _workerTask;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private volatile TaskBase _currentTask;


        public bool IsRunning => _isRunning;
        public int PendingTaskCount { get { lock (_lock) return _taskQueue.Count; } }
        public int TotalTaskCount { get { lock (_lock) return _tasks.Count; } }
        public TaskBase CurrentTask => _currentTask;

        public event EventHandler<TaskBase> TaskStarted;
        public event EventHandler<TaskBase> TaskCompleted;
        public event EventHandler<TaskBase> TaskError;
        public event EventHandler AllTasksCompleted;

        public TaskManager()
        {
            _tasks = new List<TaskBase>();
            _taskQueue = new Queue<TaskBase>();
        }

        public bool Initialize()
        {
            try
            {
                Logger.Info("初始化任务管理器...");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("任务管理器初始化失败", ex);
                return false;
            }
        }

        public bool Start()
        {
            try
            {
                Logger.Info("启动任务管理器...");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerTask = Task.Factory.StartNew(WorkerLoop, TaskCreationOptions.LongRunning);
                _isRunning = true;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("任务管理器启动失败", ex);
                return false;
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            _workerTask?.Wait(5000);
        }

        public void EmergencyStop()
        {
            Logger.Error("任务管理器紧急停止!");
            _currentTask?.Abort();
            Stop();
            ClearQueue();
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        public void AddTask(TaskBase task)
        {
            lock (_lock)
            {
                _tasks.Add(task);
                _taskQueue.Enqueue(task);

                task.StateChanged += OnTaskStateChanged;
                task.Completed += OnTaskCompleted;
                task.ErrorOccurred += OnTaskError;
            }
        }

        /// <summary>
        /// 添加并立即执行任务
        /// </summary>
        public void AddAndExecuteTask(TaskBase task)
        {
            AddTask(task);
            ExecuteTask(task);
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public bool ExecuteTask(TaskBase task)
        {
            if (task.State != TaskState.Idle)
                return false;

            return task.Start();
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        public List<TaskBase> GetAllTasks()
        {
            lock (_lock)
            {
                return _tasks.ToList();
            }
        }

        /// <summary>
        /// 获取指定状态的任务
        /// </summary>
        public List<TaskBase> GetTasksByState(TaskState state)
        {
            lock (_lock)
            {
                return _tasks.Where(t => t.State == state).ToList();
            }
        }

        /// <summary>
        /// 清除任务队列
        /// </summary>
        public void ClearQueue()
        {
            lock (_lock)
            {
                _taskQueue.Clear();
            }
        }

        /// <summary>
        /// 工作线程循环
        /// </summary>
        private void WorkerLoop()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    TaskBase task = null;

                    lock (_lock)
                    {
                        if (_taskQueue.Count > 0)
                        {
                            task = _taskQueue.Dequeue();
                        }
                    }

                    if (task != null && task.State == TaskState.Idle)
                    {
                        _currentTask = task;
                        TaskStarted?.Invoke(this, task);
                        task.Start();
                        _currentTask = null;
                    }

                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Logger.Error("任务管理器工作线程异常", ex);
                }
            }
        }

        private void OnTaskStateChanged(object sender, TaskStateChangedEventArgs e)
        {
            Logger.Debug($"任务 {e.TaskName} 状态变更: {e.OldState} -> {e.NewState}");
        }

        private void OnTaskCompleted(object sender, TaskCompletedEventArgs e)
        {
            if (sender is TaskBase task)
            {
                TaskCompleted?.Invoke(this, task);
                CheckAllTasksCompleted();
            }
        }

        private void OnTaskError(object sender, TaskErrorEventArgs e)
        {
            if (sender is TaskBase task)
            {
                Logger.Error($"任务 {e.TaskName} 执行错误: {e.ErrorMessage}");
                TaskError?.Invoke(this, task);
            }
        }

        private void CheckAllTasksCompleted()
        {
            lock (_lock)
            {
                if (_tasks.All(t => t.IsCompleted || t.HasError))
                {
                    AllTasksCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }
}
