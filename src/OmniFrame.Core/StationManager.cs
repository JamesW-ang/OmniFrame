using System;
using System.Collections.Generic;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    public enum StationState
    {
        Idle,
        Running,
        Paused,
        Error,
        Stopping
    }

    public delegate void StateChangedHandler(StationState oldState, StationState newState);

    public class StationManager : IStationManager
    {
        private readonly object _lock = new object();
        private StationState _state = StationState.Idle;
        public StationState State
        {
            get { lock (_lock) return _state; }
            private set { lock (_lock) _state = value; }
        }
        public bool IsAutoRunning => State == StationState.Running;
        public bool IsPause => State == StationState.Paused;
        public bool IsError => State == StationState.Error;

        public event StateChangedHandler StateChanged;

        private List<AutoStation> _stations = new List<AutoStation>();
        private bool _stopRequested = false;

        public StationManager()
        {
        }

        public void AddStation(AutoStation station)
        {
            lock (_lock)
            {
                if (station != null && !_stations.Contains(station))
                {
                    _stations.Add(station);
                }
            }
        }

        public bool StartRun()
        {
            lock (_lock)
            {
                if (_state == StationState.Running)
                    return true;

                if (_state == StationState.Error)
                {
                    Logger.Warning("设备处于错误状态，无法启动");
                    return false;
                }

                _stopRequested = false;
                SetState(StationState.Running);

                foreach (var station in _stations)
                {
                    if (station.Enable)
                    {
                        station.Start();
                    }
                }

                Logger.Info("自动运行启动");
                return true;
            }
        }

        public void StopRun()
        {
            lock (_lock)
            {
                if (_state == StationState.Idle || _state == StationState.Stopping)
                    return;

                _stopRequested = true;
                SetState(StationState.Stopping);

                foreach (var station in _stations)
                {
                    station.Stop();
                }

                SetState(StationState.Idle);
                Logger.Info("自动运行停止");
            }
        }

        public void PauseAllStation()
        {
            lock (_lock)
            {
                if (_state != StationState.Running)
                    return;

                SetState(StationState.Paused);

                foreach (var station in _stations)
                {
                    station.Pause();
                }

                Logger.Info("所有站位暂停");
            }
        }

        public bool ResumeAllStation()
        {
            lock (_lock)
            {
                if (_state != StationState.Paused)
                    return false;

                SetState(StationState.Running);

                foreach (var station in _stations)
                {
                    station.Resume();
                }

                Logger.Info("所有站位恢复运行");
                return true;
            }
        }

        public void ResetAllStation()
        {
            lock (_lock)
            {
                if (_state == StationState.Running)
                    return;

                foreach (var station in _stations)
                {
                    station.Reset();
                }

                if (_state == StationState.Error)
                {
                    SetState(StationState.Idle);
                }

                Logger.Info("所有站位复位");
            }
        }

        public void EmergencyStop()
        {
            lock (_lock)
            {
                SetState(StationState.Error);

                foreach (var station in _stations)
                {
                    station.EmergencyStop();
                }

                Logger.Error("急停触发！");
            }
        }

        private void SetState(StationState newState)
        {
            if (State == newState)
                return;

            StationState oldState = State;
            State = newState;

            Logger.Info($"站位状态变更: {oldState} -> {newState}");
            StateChanged?.Invoke(oldState, newState);
        }

        public void Update()
        {
            lock (_lock)
            {
                if (_state != StationState.Running)
                    return;

                bool allCompleted = true;
                bool hasError = false;

                foreach (var station in _stations)
                {
                    if (station.Enable)
                    {
                        station.Update();

                        if (station.State == StationState.Error)
                        {
                            hasError = true;
                        }

                        if (station.State != StationState.Idle)
                        {
                            allCompleted = false;
                        }
                    }
                }

                if (hasError)
                {
                    SetState(StationState.Error);
                }
                else if (allCompleted && _stopRequested)
                {
                    SetState(StationState.Idle);
                }
            }
        }

        public List<AutoStation> GetAllStations()
        {
            lock (_lock)
            {
                return new List<AutoStation>(_stations);
            }
        }
    }

    /// <summary>
    /// 自动运行工步 - 表示自动化流程中的一个执行步骤
    /// 与 StationBase（物理工站）不同，AutoStation 是状态机中的流程步骤
        /// </summary>
    public abstract class AutoStation
    {
        public int Index { get; protected set; }
        public string Name { get; protected set; }
        public bool Enable { get; set; } = true;
        public StationState State { get; protected set; } = StationState.Idle;

        protected AutoStation(int index, string name)
        {
            Index = index;
            Name = name;
        }

        public virtual void Start()
        {
            State = StationState.Running;
            Logger.Info($"站位 {Name} 启动");
        }

        public virtual void Stop()
        {
            State = StationState.Idle;
            Logger.Info($"站位 {Name} 停止");
        }

        public virtual void Pause()
        {
            if (State == StationState.Running)
            {
                State = StationState.Paused;
                Logger.Info($"站位 {Name} 暂停");
            }
        }

        public virtual void Resume()
        {
            if (State == StationState.Paused)
            {
                State = StationState.Running;
                Logger.Info($"站位 {Name} 恢复");
            }
        }

        public virtual void Reset()
        {
            State = StationState.Idle;
            Logger.Info($"站位 {Name} 复位");
        }

        public virtual void EmergencyStop()
        {
            State = StationState.Error;
            Logger.Error($"站位 {Name} 急停");
        }

        public abstract void Update();
    }
}
