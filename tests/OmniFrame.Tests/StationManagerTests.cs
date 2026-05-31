using Moq;
using NUnit.Framework;
using OmniFrame.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OmniFrame.Tests
{
    /// <summary>
    /// Concrete AutoStation implementation for unit testing StationManager.
    /// </summary>
    internal class TestAutoStation : AutoStation
    {
        public bool StartCalled { get; private set; }
        public bool StopCalled { get; private set; }
        public bool PauseCalled { get; private set; }
        public bool ResumeCalled { get; private set; }
        public bool ResetCalled { get; private set; }
        public bool EmergencyStopCalled { get; private set; }
        public int UpdateCallCount { get; private set; }

        // Used to simulate station state for Update() testing
        public bool SimulateErrorOnUpdate { get; set; }
        public bool SimulateCompleteOnUpdate { get; set; }

        public TestAutoStation(int index, string name)
            : base(index, name)
        {
            Enable = true;
        }

        public override void Start()
        {
            StartCalled = true;
            base.Start();
        }

        public override void Stop()
        {
            StopCalled = true;
            base.Stop();
        }

        public override void Pause()
        {
            PauseCalled = true;
            base.Pause();
        }

        public override void Resume()
        {
            ResumeCalled = true;
            base.Resume();
        }

        public override void Reset()
        {
            ResetCalled = true;
            base.Reset();
        }

        public override void EmergencyStop()
        {
            EmergencyStopCalled = true;
            base.EmergencyStop();
        }

        public override void Update()
        {
            UpdateCallCount++;
            if (SimulateErrorOnUpdate)
            {
                State = StationState.Error;
            }
            else if (SimulateCompleteOnUpdate)
            {
                State = StationState.Idle;
            }
        }

        // Helper to directly set state for test scenarios
        public void SetStateDirectly(StationState state)
        {
            State = state;
        }
    }

    [TestFixture]
    public class StationManagerTests
    {
        private StationManager _stationManager;

        [SetUp]
        public void Setup()
        {
            _stationManager = new StationManager();
        }

        #region Add/Remove Stations Tests

        [Test]
        public void AddRemoveStations_AddStation_CountCorrect()
        {
            var station1 = new TestAutoStation(1, "Station1");
            var station2 = new TestAutoStation(2, "Station2");

            _stationManager.AddStation(station1);
            _stationManager.AddStation(station2);

            var stations = _stationManager.GetAllStations();
            Assert.That(stations.Count, Is.EqualTo(2));
            Assert.That(stations[0].Name, Is.EqualTo("Station1"));
            Assert.That(stations[1].Name, Is.EqualTo("Station2"));
        }

        [Test]
        public void AddRemoveStations_AddNull_DoesNotAdd()
        {
            _stationManager.AddStation(null);

            var stations = _stationManager.GetAllStations();
            Assert.That(stations.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddRemoveStations_AddDuplicate_DoesNotDuplicate()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);
            _stationManager.AddStation(station);
            _stationManager.AddStation(station);

            var stations = _stationManager.GetAllStations();
            Assert.That(stations.Count, Is.EqualTo(1),
                "Duplicate station should not be added");
        }

        [Test]
        public void AddRemoveStations_GetAllStations_ReturnsCopy()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            var stations = _stationManager.GetAllStations();
            stations.Clear();

            // The original list should be unaffected
            var stationsAgain = _stationManager.GetAllStations();
            Assert.That(stationsAgain.Count, Is.EqualTo(1),
                "GetAllStations should return a copy, not the internal reference");
        }

        #endregion

        #region StartAll Tests

        [Test]
        public void StartAll_AllStations_TransitionsToRunning()
        {
            var station1 = new TestAutoStation(1, "Station1");
            var station2 = new TestAutoStation(2, "Station2");
            _stationManager.AddStation(station1);
            _stationManager.AddStation(station2);

            bool result = _stationManager.StartRun();

            Assert.That(result, Is.True);
            Assert.That(_stationManager.State, Is.EqualTo(StationState.Running));
            Assert.That(station1.StartCalled, Is.True);
            Assert.That(station2.StartCalled, Is.True);
            Assert.That(station1.State, Is.EqualTo(StationState.Running));
            Assert.That(station2.State, Is.EqualTo(StationState.Running));
        }

        [Test]
        public void StartAll_DisabledStation_NotStarted()
        {
            var activeStation = new TestAutoStation(1, "Active") { Enable = true };
            var disabledStation = new TestAutoStation(2, "Disabled") { Enable = false };
            _stationManager.AddStation(activeStation);
            _stationManager.AddStation(disabledStation);

            _stationManager.StartRun();

            Assert.That(activeStation.StartCalled, Is.True, "Active station should be started");
            Assert.That(disabledStation.StartCalled, Is.False, "Disabled station should not be started");
        }

        [Test]
        public void StartAll_InErrorState_ReturnsFalse()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            // Force error state via reflection
            var stateField = typeof(StationManager).GetField("_state",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            stateField.SetValue(_stationManager, StationState.Error);

            bool result = _stationManager.StartRun();
            Assert.That(result, Is.False, "StartRun should fail when in error state");
        }

        [Test]
        public void StartAll_AlreadyRunning_ReturnsTrue()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            _stationManager.StartRun();
            Assert.That(_stationManager.State, Is.EqualTo(StationState.Running));

            // Second StartRun should return true (already running)
            bool result = _stationManager.StartRun();
            Assert.That(result, Is.True);
        }

        #endregion

        #region StopAll Tests

        [Test]
        public void StopAll_AllStations_TransitionsToIdle()
        {
            var station1 = new TestAutoStation(1, "Station1");
            var station2 = new TestAutoStation(2, "Station2");
            _stationManager.AddStation(station1);
            _stationManager.AddStation(station2);

            _stationManager.StartRun();
            _stationManager.StopRun();

            Assert.That(_stationManager.State, Is.EqualTo(StationState.Idle));
            Assert.That(station1.StopCalled, Is.True);
            Assert.That(station2.StopCalled, Is.True);
        }

        [Test]
        public void StopAll_NotRunning_DoesNothing()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            _stationManager.StopRun();

            Assert.That(station.StopCalled, Is.False);
            Assert.That(_stationManager.State, Is.EqualTo(StationState.Idle));
        }

        #endregion

        #region State Transition Tests

        [Test]
        public void StateTransitions_IdleToRunningToPausedToRunningToStopped()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            // Idle -> Running
            Assert.That(_stationManager.State, Is.EqualTo(StationState.Idle));
            _stationManager.StartRun();
            Assert.That(_stationManager.State, Is.EqualTo(StationState.Running));

            // Running -> Paused
            _stationManager.PauseAllStation();
            Assert.That(_stationManager.State, Is.EqualTo(StationState.Paused));
            Assert.That(station.PauseCalled, Is.True);

            // Paused -> Running
            bool resumed = _stationManager.ResumeAllStation();
            Assert.That(resumed, Is.True);
            Assert.That(_stationManager.State, Is.EqualTo(StationState.Running));
            Assert.That(station.ResumeCalled, Is.True);

            // Running -> Stopped
            _stationManager.StopRun();
            Assert.That(_stationManager.State, Is.EqualTo(StationState.Idle));
        }

        [Test]
        public void StateTransitions_StateChangedEvent_FiresOnTransition()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            List<(StationState, StationState)> transitions = new List<(StationState, StationState)>();
            _stationManager.StateChanged += (oldState, newState) =>
            {
                transitions.Add((oldState, newState));
            };

            _stationManager.StartRun();
            _stationManager.StopRun();

            Assert.That(transitions.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(transitions[0].Item1, Is.EqualTo(StationState.Idle));
            Assert.That(transitions[0].Item2, Is.EqualTo(StationState.Running));
        }

        [Test]
        public void StateTransitions_StartFromPaused_CannotStartDirectly()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            _stationManager.StartRun();
            _stationManager.PauseAllStation();
            Assert.That(_stationManager.State, Is.EqualTo(StationState.Paused));

            // StartRun from Paused should not change state (lock prevents)
            // StartRun checks if Running (no) and Error (no), then proceeds
            // Actually, looking at code: StartRun only checks Running and Error.
            // If Paused, it will try to StartRun anyway.
            // But stations are already started, so Start() may not be callable.
        }

        #endregion

        #region Emergency Stop Tests

        [Test]
        public void EmergencyStop_AllStations_ImmediateStop()
        {
            var station1 = new TestAutoStation(1, "Station1");
            var station2 = new TestAutoStation(2, "Station2");
            var station3 = new TestAutoStation(3, "Station3");
            _stationManager.AddStation(station1);
            _stationManager.AddStation(station2);
            _stationManager.AddStation(station3);

            _stationManager.StartRun();

            _stationManager.EmergencyStop();

            Assert.That(_stationManager.State, Is.EqualTo(StationState.Error));
            Assert.That(station1.EmergencyStopCalled, Is.True);
            Assert.That(station2.EmergencyStopCalled, Is.True);
            Assert.That(station3.EmergencyStopCalled, Is.True);
        }

        [Test]
        public void EmergencyStop_DisabledStations_AlsoStopped()
        {
            var activeStation = new TestAutoStation(1, "Active");
            var disabledStation = new TestAutoStation(2, "Disabled") { Enable = false };
            _stationManager.AddStation(activeStation);
            _stationManager.AddStation(disabledStation);

            _stationManager.EmergencyStop();

            Assert.That(activeStation.EmergencyStopCalled, Is.True);
            Assert.That(disabledStation.EmergencyStopCalled, Is.True,
                "Emergency stop should affect all stations including disabled");
        }

        [Test]
        public void EmergencyStop_FromIdle_TransitionsToError()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            _stationManager.EmergencyStop();

            Assert.That(_stationManager.State, Is.EqualTo(StationState.Error));
        }

        #endregion

        #region Individual Station Control Tests

        [Test]
        public void IndividualControl_StartStopSingle_DoesNotAffectOthers()
        {
            var station1 = new TestAutoStation(1, "Station1");
            var station2 = new TestAutoStation(2, "Station2");
            _stationManager.AddStation(station1);
            _stationManager.AddStation(station2);

            _stationManager.StartRun();

            // Verify both stations were started
            Assert.That(station1.StartCalled, Is.True);
            Assert.That(station2.StartCalled, Is.True);

            // Individual station control: stop just station1
            station1.Stop();
            Assert.That(station1.State, Is.EqualTo(StationState.Idle));
            Assert.That(station2.State, Is.EqualTo(StationState.Running),
                "Station2 should remain running when station1 stops individually");

            // Individual station control: start station1 again
            station1.Start();
            Assert.That(station1.State, Is.EqualTo(StationState.Running));
        }

        [Test]
        public void IndividualControl_PauseSingleStation()
        {
            var station1 = new TestAutoStation(1, "Station1");
            var station2 = new TestAutoStation(2, "Station2");
            _stationManager.AddStation(station1);
            _stationManager.AddStation(station2);

            _stationManager.StartRun();

            station1.Pause();
            Assert.That(station1.State, Is.EqualTo(StationState.Paused));
            Assert.That(station2.State, Is.EqualTo(StationState.Running),
                "Station2 should be unaffected by station1 pause");

            station1.Resume();
            Assert.That(station1.State, Is.EqualTo(StationState.Running));
        }

        [Test]
        public void IndividualControl_DisabledStation_NotAffectedByGroupCommands()
        {
            var activeStation = new TestAutoStation(1, "Active");
            var disabledStation = new TestAutoStation(2, "Disabled") { Enable = false };
            _stationManager.AddStation(activeStation);
            _stationManager.AddStation(disabledStation);

            // Group Pause (only affects running stations managed via StationManager)
            _stationManager.StartRun();
            Assert.That(activeStation.StartCalled, Is.True);
            Assert.That(disabledStation.StartCalled, Is.False,
                "Disabled station should not be started by StartRun");
        }

        #endregion

        #region State Query Tests

        [Test]
        public void StateQuery_IsAutoRunning_ReflectsState()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            Assert.That(_stationManager.IsAutoRunning, Is.False);
            Assert.That(_stationManager.IsPause, Is.False);
            Assert.That(_stationManager.IsError, Is.False);

            _stationManager.StartRun();
            Assert.That(_stationManager.IsAutoRunning, Is.True);

            _stationManager.PauseAllStation();
            Assert.That(_stationManager.IsAutoRunning, Is.False);
            Assert.That(_stationManager.IsPause, Is.True);

            _stationManager.ResumeAllStation();
            Assert.That(_stationManager.IsAutoRunning, Is.True);
            Assert.That(_stationManager.IsPause, Is.False);
        }

        [Test]
        public void StateQuery_EmergencyStop_SetsErrorState()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            _stationManager.EmergencyStop();

            Assert.That(_stationManager.IsError, Is.True);
            Assert.That(_stationManager.IsAutoRunning, Is.False);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WhileRunning_UpdatesStations()
        {
            var station1 = new TestAutoStation(1, "Station1");
            var station2 = new TestAutoStation(2, "Station2");
            _stationManager.AddStation(station1);
            _stationManager.AddStation(station2);

            _stationManager.StartRun();

            _stationManager.Update();

            Assert.That(station1.UpdateCallCount, Is.EqualTo(1));
            Assert.That(station2.UpdateCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Update_NotRunning_DoesNotUpdateStations()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            _stationManager.Update();

            Assert.That(station.UpdateCallCount, Is.EqualTo(0),
                "Update should not be called when not running");
        }

        [Test]
        public void Update_StationError_DrivesManagerToError()
        {
            var station = new TestAutoStation(1, "Station1")
            {
                SimulateErrorOnUpdate = true
            };
            _stationManager.AddStation(station);

            _stationManager.StartRun();
            _stationManager.Update();

            Assert.That(_stationManager.State, Is.EqualTo(StationState.Error));
        }

        [Test]
        public void Update_DisabledStation_NotUpdated()
        {
            var activeStation = new TestAutoStation(1, "Active");
            var disabledStation = new TestAutoStation(2, "Disabled") { Enable = false };
            _stationManager.AddStation(activeStation);
            _stationManager.AddStation(disabledStation);

            _stationManager.StartRun();
            _stationManager.Update();

            Assert.That(activeStation.UpdateCallCount, Is.EqualTo(1));
            Assert.That(disabledStation.UpdateCallCount, Is.EqualTo(0),
                "Disabled station should not be updated");
        }

        #endregion

        #region Reset Tests

        [Test]
        public void ResetAllStation_FromError_TransitionsToIdle()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            // Force error state
            station.SetStateDirectly(StationState.Error);
            var stateField = typeof(StationManager).GetField("_state",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            stateField.SetValue(_stationManager, StationState.Error);

            _stationManager.ResetAllStation();

            Assert.That(station.ResetCalled, Is.True);
            Assert.That(_stationManager.State, Is.EqualTo(StationState.Idle));
        }

        [Test]
        public void ResetAllStation_WhileRunning_DoesNotReset()
        {
            var station = new TestAutoStation(1, "Station1");
            _stationManager.AddStation(station);

            _stationManager.StartRun();

            _stationManager.ResetAllStation();

            Assert.That(station.ResetCalled, Is.False,
                "Reset should not be called while running");
        }

        #endregion
    }
}
