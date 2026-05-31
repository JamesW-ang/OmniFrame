using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    public interface IStationManager
    {
        StationState State { get; }
        bool IsAutoRunning { get; }
        bool IsPause { get; }
        bool IsError { get; }

        event StateChangedHandler StateChanged;

        void AddStation(AutoStation station);
        bool StartRun();
        void StopRun();
        void PauseAllStation();
        bool ResumeAllStation();
        void ResetAllStation();
        void EmergencyStop();
        void Update();
        List<AutoStation> GetAllStations();
    }
}
