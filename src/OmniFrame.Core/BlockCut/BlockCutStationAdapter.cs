using System;
using OmniFrame.Core;

namespace OmniFrame.Core.BlockCut
{
    public class BlockCutStationAdapter : AutoStation
    {
        private readonly BlockCutStationBase _station;

        public BlockCutStationBase InnerStation => _station;

        public StationState CurrentState
        {
            get
            {
                if (_station.IsInError) return StationState.Error;
                if (_station.IsPaused) return StationState.Paused;
                if (_station.IsRunning) return StationState.Running;
                return StationState.Idle;
            }
        }

        public BlockCutStationAdapter(int index, string name, BlockCutStationBase station) 
            : base(index, name)
        {
            _station = station ?? throw new ArgumentNullException(nameof(station));
            Enable = true;
        }

        public override void Start()
        {
            base.Start();
            _station.Start();
        }

        public override void Stop()
        {
            base.Stop();
            _station.Stop();
        }

        public override void Pause()
        {
            base.Pause();
            _station.Pause();
        }

        public override void Resume()
        {
            base.Resume();
            _station.Resume();
        }

        public override void Reset()
        {
            base.Reset();
            _station.Reset();
        }

        public override void EmergencyStop()
        {
            base.EmergencyStop();
            _station.Stop();
            _station.Reset();
        }

        public override void Update()
        {
            base.State = CurrentState;
        }
    }
}
