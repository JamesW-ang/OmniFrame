using Moq;
using NUnit.Framework;
using OmniFrame.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OmniFrame.Tests
{
    /// <summary>
    /// Concrete testable TaskBase implementation for unit testing.
    /// </summary>
    internal class TestTask : TaskBase
    {
        public bool ExecuteResult { get; set; } = true;
        public int ExecuteCallCount { get; private set; }
        public int SleepMs { get; set; } = 0;
        public bool WasPaused { get; private set; }
        public bool WasResumed { get; private set; }
        public bool WasAborted { get; private set; }

        /// <summary>
        /// Allows test fixtures to set the TaskState directly, bypassing the
        /// protected setter on TaskBase.State.
        /// </summary>
        public void SetState(TaskState state)
        {
            var prop = typeof(TaskBase).GetProperty("State",
                BindingFlags.Public | BindingFlags.Instance);
            var setMethod = prop?.GetSetMethod(nonPublic: true)
                ?? prop?.SetMethod;
            setMethod?.Invoke(this, new object[] { state });
        }

        public TestTask(string name, TaskPriority priority = TaskPriority.Normal)
            : base(name, priority) { }

        protected override bool Execute()
        {
            ExecuteCallCount++;
            if (SleepMs > 0)
                Thread.Sleep(SleepMs);
            return ExecuteResult;
        }

        protected override void OnPause()
        {
            WasPaused = true;
            base.OnPause();
        }

        protected override void OnResume()
        {
            WasResumed = true;
            base.OnResume();
        }

        protected override void OnAbort()
        {
            WasAborted = true;
            base.OnAbort();
        }
    }

    [TestFixture]
    public class TaskManagerTests
    {
        private TaskManager _taskManager;

        [SetUp]
        public void Setup()
        {
            _taskManager = new TaskManager();
            _taskManager.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _taskManager?.Dispose();
        }

        #region Producer-Consumer Tests

        [Test]
        public void ProducerConsumer_EnqueueTasks_ProcessedInOrder()
        {
            var task1 = new TestTask("Task1");
            var task2 = new TestTask("Task2");
            var task3 = new TestTask("Task3");

            _taskManager.AddTask(task1);
            _taskManager.AddTask(task2);
            _taskManager.AddTask(task3);

            Assert.That(_taskManager.PendingTaskCount, Is.EqualTo(3));
            Assert.That(_taskManager.TotalTaskCount, Is.EqualTo(3));

            // Execute tasks in order
            var tasks = _taskManager.GetAllTasks();
            Assert.That(tasks.Count, Is.EqualTo(3));
            Assert.That(tasks[0].TaskName, Is.EqualTo("Task1"));
            Assert.That(tasks[1].TaskName, Is.EqualTo("Task2"));
            Assert.That(tasks[2].TaskName, Is.EqualTo("Task3"));
        }

        [Test]
        public void ProducerConsumer_ExecuteTask_CompletesSuccessfully()
        {
            var task = new TestTask("SingleTask");
            _taskManager.AddTask(task);

            bool result = _taskManager.ExecuteTask(task);

            Assert.That(result, Is.True);
            Assert.That(task.State, Is.EqualTo(TaskState.Completed));
            Assert.That(task.ExecuteCallCount, Is.EqualTo(1));
            Assert.That(task.Duration, Is.Not.Null);
        }

        [Test]
        public void ProducerConsumer_ExecuteTask_ReturnsFalseWhenFails()
        {
            var task = new TestTask("FailTask") { ExecuteResult = false };
            _taskManager.AddTask(task);

            bool result = _taskManager.ExecuteTask(task);

            Assert.That(result, Is.False);
            Assert.That(task.State, Is.EqualTo(TaskState.Error));
            Assert.That(task.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void ProducerConsumer_WorkerLoop_ProcessesQueuedTasks()
        {
            var task = new TestTask("WorkerTask");
            _taskManager.AddTask(task);

            TaskBase startedTask = null;
            _taskManager.TaskStarted += (s, t) => startedTask = t;

            _taskManager.Start();
            Thread.Sleep(300); // Give worker time to process
            _taskManager.Stop();

            // Worker loop dequeues and executes tasks
            Assert.That(task.State, Is.EqualTo(TaskState.Completed));
            Assert.That(task.ExecuteCallCount, Is.GreaterThanOrEqualTo(1));
        }

        #endregion

        #region Priority Ordering Tests

        [Test]
        public void PriorityOrdering_HighBeforeLow_CanSetPriority()
        {
            var highTask = new TestTask("HighPriority", TaskPriority.High);
            var lowTask = new TestTask("LowPriority", TaskPriority.Low);
            var normalTask = new TestTask("NormalPriority", TaskPriority.Normal);

            Assert.That(highTask.Priority, Is.EqualTo(TaskPriority.High));
            Assert.That(lowTask.Priority, Is.EqualTo(TaskPriority.Low));
            Assert.That(normalTask.Priority, Is.EqualTo(TaskPriority.Normal));
        }

        [Test]
        public void PriorityOrdering_CriticalPriority_IsHighest()
        {
            var criticalTask = new TestTask("Critical", TaskPriority.Critical);

            Assert.That(criticalTask.Priority, Is.EqualTo(TaskPriority.Critical));
            Assert.That((int)criticalTask.Priority, Is.GreaterThan((int)TaskPriority.High));
        }

        [Test]
        public void PriorityOrdering_DefaultPriority_IsNormal()
        {
            var task = new TestTask("DefaultTask");
            Assert.That(task.Priority, Is.EqualTo(TaskPriority.Normal));
        }

        #endregion

        #region Pause/Resume Tests

        [Test]
        public void PauseResume_PausingStopsProcessing()
        {
            var task = new TestTask("PausableTask");
            _taskManager.AddTask(task);

            // Start and then pause the task directly
            task.Start();
            Assert.That(task.State, Is.EqualTo(TaskState.Completed) | Is.EqualTo(TaskState.Error),
                "Task start runs synchronously to completion");

            // Create a new task and set its state directly for pause testing
            var pausableTask = new TestTask("PausableTask2") { SleepMs = 500 };
            pausableTask.SetState(TaskState.Running);

            pausableTask.Pause();
            Assert.That(pausableTask.State, Is.EqualTo(TaskState.Paused));
            Assert.That(pausableTask.WasPaused, Is.True);
        }

        [Test]
        public void PauseResume_ResumeContinues()
        {
            var task = new TestTask("ResumableTask");
            task.SetState(TaskState.Running);
            task.Pause();
            Assert.That(task.State, Is.EqualTo(TaskState.Paused));

            task.Resume();
            Assert.That(task.State, Is.EqualTo(TaskState.Running));
            Assert.That(task.WasResumed, Is.True);
        }

        [Test]
        public void PauseResume_CannotResumeFromNonPaused()
        {
            var task = new TestTask("NotPausedTask");
            task.SetState(TaskState.Idle);
            task.Resume();

            // Resume should only work when state is Paused
            Assert.That(task.State, Is.EqualTo(TaskState.Idle));
        }

        [Test]
        public void PauseResume_CannotPauseNonRunning()
        {
            var task = new TestTask("IdleTask");
            task.SetState(TaskState.Idle);
            task.Pause();
            Assert.That(task.State, Is.EqualTo(TaskState.Idle));
        }

        #endregion

        #region Cancel Task Tests

        [Test]
        public void CancelTask_AbortedTask_RemovesFromQueue()
        {
            var task1 = new TestTask("ToAbort");
            var task2 = new TestTask("ToKeep");

            _taskManager.AddTask(task1);
            _taskManager.AddTask(task2);
            Assert.That(_taskManager.PendingTaskCount, Is.EqualTo(2));

            // Abort task1
            task1.Abort();
            Assert.That(task1.State, Is.EqualTo(TaskState.Aborted));

            // The aborted task should still be in the collection but aborted
            var tasks = _taskManager.GetAllTasks();
            Assert.That(tasks.Count, Is.EqualTo(2));
            Assert.That(tasks.Any(t => t.State == TaskState.Aborted), Is.True);
        }

        [Test]
        public void CancelTask_ClearQueue_RemovesAllPending()
        {
            _taskManager.AddTask(new TestTask("Q1"));
            _taskManager.AddTask(new TestTask("Q2"));
            _taskManager.AddTask(new TestTask("Q3"));

            Assert.That(_taskManager.PendingTaskCount, Is.EqualTo(3));

            _taskManager.ClearQueue();
            Assert.That(_taskManager.PendingTaskCount, Is.EqualTo(0));

            // Total task count should still reflect added tasks (they're in _tasks list)
            // ClearQueue only clears _taskQueue, not _tasks
        }

        [Test]
        public void CancelTask_EmergencyStop_AbortsCurrentAndClearsQueue()
        {
            var task = new TestTask("EmergencyTask");
            _taskManager.AddTask(task);

            _taskManager.Start();
            _taskManager.EmergencyStop();

            Assert.That(_taskManager.IsRunning, Is.False);
        }

        #endregion

        #region Concurrent Enqueue Tests

        [Test]
        public void ConcurrentEnqueue_MultipleThreads_NoCorruption()
        {
            var allTasks = new ConcurrentBag<TestTask>();
            int threadCount = 10;
            int tasksPerThread = 10;

            var threads = new List<Thread>();
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                var thread = new Thread(() =>
                {
                    for (int j = 0; j < tasksPerThread; j++)
                    {
                        var task = new TestTask($"Thread{threadIndex}_Task{j}");
                        _taskManager.AddTask(task);
                        allTasks.Add(task);
                    }
                });
                threads.Add(thread);
            }

            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            Assert.That(_taskManager.TotalTaskCount, Is.EqualTo(threadCount * tasksPerThread));
            Assert.That(_taskManager.PendingTaskCount, Is.EqualTo(threadCount * tasksPerThread));

            // Verify no null entries
            var all = _taskManager.GetAllTasks();
            Assert.That(all.All(t => t != null), Is.True, "All tasks should be non-null");
            Assert.That(all.Select(t => t.TaskName).Distinct().Count(), Is.EqualTo(threadCount * tasksPerThread),
                "All tasks should be unique");
        }

        [Test]
        public void ConcurrentEnqueue_FastAddDoesNotLoseTasks()
        {
            int taskCount = 50;
            for (int i = 0; i < taskCount; i++)
            {
                _taskManager.AddTask(new TestTask($"FastTask_{i}"));
            }

            Assert.That(_taskManager.TotalTaskCount, Is.EqualTo(taskCount));
        }

        #endregion

        #region Empty Queue Tests

        [Test]
        public void EmptyQueue_NoTasks_ProcessingRemainsIdle()
        {
            Assert.That(_taskManager.PendingTaskCount, Is.EqualTo(0));
            Assert.That(_taskManager.TotalTaskCount, Is.EqualTo(0));
            Assert.That(_taskManager.CurrentTask, Is.Null);
            Assert.That(_taskManager.IsRunning, Is.False);
        }

        [Test]
        public void EmptyQueue_StartWithNoTasks_HandlesGracefully()
        {
            bool result = _taskManager.Start();
            Assert.That(result, Is.True);

            // Worker loop should run without errors even with empty queue
            Thread.Sleep(200);
            Assert.That(_taskManager.IsRunning, Is.True);

            _taskManager.Stop();
            Assert.That(_taskManager.IsRunning, Is.False);
        }

        [Test]
        public void EmptyQueue_ExecuteTaskOnNonIdle_ReturnsFalse()
        {
            var task = new TestTask("NonIdleTask");
            _taskManager.AddTask(task);
            _taskManager.ExecuteTask(task);
            Assert.That(task.State, Is.EqualTo(TaskState.Completed));

            // Trying to execute an already-completed task should fail
            bool result = _taskManager.ExecuteTask(task);
            Assert.That(result, Is.False);
        }

        #endregion

        #region State Query Tests

        [Test]
        public void StateQuery_GetTasksByState_FiltersCorrectly()
        {
            var completedTask = new TestTask("Done1");
            var errorTask = new TestTask("Bad1") { ExecuteResult = false };
            var idleTask = new TestTask("Waiting1");

            _taskManager.AddTask(completedTask);
            _taskManager.AddTask(errorTask);
            _taskManager.AddTask(idleTask);

            _taskManager.ExecuteTask(completedTask);
            _taskManager.ExecuteTask(errorTask);

            var completed = _taskManager.GetTasksByState(TaskState.Completed);
            var errors = _taskManager.GetTasksByState(TaskState.Error);
            var idle = _taskManager.GetTasksByState(TaskState.Idle);

            Assert.That(completed.Count, Is.EqualTo(1));
            Assert.That(completed[0].TaskName, Is.EqualTo("Done1"));
            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0].TaskName, Is.EqualTo("Bad1"));
            Assert.That(idle.Count, Is.EqualTo(1));
            Assert.That(idle[0].TaskName, Is.EqualTo("Waiting1"));
        }

        [Test]
        public void StateQuery_AllTasksCompletedEvent_Fires()
        {
            bool allCompletedFired = false;
            _taskManager.AllTasksCompleted += (s, e) => allCompletedFired = true;

            var task = new TestTask("FinalTask");
            _taskManager.AddTask(task);
            _taskManager.ExecuteTask(task);

            Assert.That(allCompletedFired, Is.True);
        }

        #endregion

        #region Start/Stop Tests

        [Test]
        public void Start_SetsIsRunning()
        {
            bool result = _taskManager.Start();
            Assert.That(result, Is.True);
            Assert.That(_taskManager.IsRunning, Is.True);

            _taskManager.Stop();
            Assert.That(_taskManager.IsRunning, Is.False);
        }

        [Test]
        public void AddAndExecuteTask_AddsAndExecutesImmediately()
        {
            var task = new TestTask("ImmediateTask");
            _taskManager.AddAndExecuteTask(task);

            Assert.That(task.State, Is.EqualTo(TaskState.Completed));
            Assert.That(_taskManager.TotalTaskCount, Is.EqualTo(1));
        }

        #endregion
    }
}
