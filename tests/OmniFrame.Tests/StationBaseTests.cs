using Moq;
using NUnit.Framework;
using OmniFrame.Common;
using OmniFrame.Core;
using System;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class StationBaseTests
    {
        private class TestStation : StationBase
        {
            public bool DoExecuteResult { get; set; } = true;
            public int DoExecuteCallCount { get; private set; }
            public Exception DoExecuteException { get; set; }

            public TestStation(string name) : base(name) { }

            protected override bool DoExecute()
            {
                DoExecuteCallCount++;
                if (DoExecuteException != null)
                    throw DoExecuteException;
                return DoExecuteResult;
            }

            public void PublicSetSignal(string name) => SetSignal(name);
            public void PublicResetSignal(string name) => ResetSignal(name);
            public bool PublicCheckSignal(string name) => CheckSignal(name);
            public bool PublicWaitSignal(string name, int timeout) => WaitSignal(name, timeout);
        }

        private TestStation _station;

        [SetUp]
        public void Setup()
        {
            _station = new TestStation("Station1");
        }

        [TearDown]
        public void TearDown()
        {
            _station.Dispose();
        }

        [Test]
        public void Initialize_SetsInitialized()
        {
            bool result = _station.Initialize();

            Assert.That(result, Is.True);
            Assert.That(_station.IsInitialized, Is.True);
        }

        [Test]
        public void Initialize_Twice_DoesNotReinitialize()
        {
            _station.Initialize();
            bool result = _station.Initialize();

            Assert.That(result, Is.True);
        }

        [Test]
        public void Start_NotInitialized_ReturnsFalse()
        {
            bool result = _station.Start();
            Assert.That(result, Is.False);
        }

        [Test]
        public void Start_Initialized_ReturnsTrue()
        {
            _station.Initialize();

            bool result = _station.Start();

            Assert.That(result, Is.True);
            Assert.That(_station.IsRunning, Is.True);
        }

        [Test]
        public void Start_InError_ReturnsFalse()
        {
            _station.MaxRetryCount = 0;
            _station.Initialize();
            _station.DoExecuteResult = false;
            _station.Execute();

            bool result = _station.Start();

            Assert.That(result, Is.False);
        }

        [Test]
        public void Stop_SetsNotRunning()
        {
            _station.Initialize();
            _station.Start();

            _station.Stop();

            Assert.That(_station.IsRunning, Is.False);
        }

        [Test]
        public void Execute_DoExecuteSucceeds_ReturnsTrue()
        {
            _station.Initialize();

            bool result = _station.Execute();

            Assert.That(result, Is.True);
            Assert.That(_station.DoExecuteCallCount, Is.EqualTo(1));
            Assert.That(_station.ConsecutiveFailCount, Is.EqualTo(0));
        }

        [Test]
        public void Execute_DoExecuteFails_RetriesAndEntersError()
        {
            _station.MaxRetryCount = 2;
            _station.DoExecuteResult = false;
            _station.Initialize();

            _station.Execute();
            _station.Execute();
            bool result = _station.Execute();

            Assert.That(result, Is.False);
            Assert.That(_station.DoExecuteCallCount, Is.EqualTo(3));
            Assert.That(_station.IsInError, Is.True);
            Assert.That(_station.ConsecutiveFailCount, Is.EqualTo(3));
        }

        [Test]
        public void Execute_DoExecuteThrows_EntersError()
        {
            _station.MaxRetryCount = 0;
            _station.DoExecuteException = new InvalidOperationException("Critical failure");
            _station.Initialize();

            bool result = _station.Execute();

            Assert.That(result, Is.False);
            Assert.That(_station.IsInError, Is.True);
            Assert.That(_station.LastErrorMessage, Does.Contain("Critical failure"));
        }

        [Test]
        public void Execute_InError_ReturnsFalse()
        {
            _station.MaxRetryCount = 0;
            _station.DoExecuteResult = false;
            _station.Initialize();
            _station.Execute();

            bool result = _station.Execute();

            Assert.That(result, Is.False);
            Assert.That(_station.DoExecuteCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Reset_ClearsErrorAndReInitializes()
        {
            _station.MaxRetryCount = 0;
            _station.DoExecuteResult = false;
            _station.Initialize();
            _station.Execute();
            Assert.That(_station.IsInError, Is.True);

            _station.DoExecuteResult = true;
            _station.Reset();

            Assert.That(_station.IsInError, Is.False);
            Assert.That(_station.IsInitialized, Is.True);
            Assert.That(_station.ConsecutiveFailCount, Is.EqualTo(0));
        }

        [Test]
        public void StatusChangedEvent_FiredOnStateTransition()
        {
            string firedStatus = null;
            _station.StationStatusChanged += (s, e) => firedStatus = e.Status;
            _station.Initialize();

            Assert.That(firedStatus, Is.EqualTo("Initialized"));
        }

        [Test]
        public void ErrorEvent_FiredOnError()
        {
            string errorMsg = null;
            _station.StationErrorOccurred += (s, e) => errorMsg = e.ErrorMessage;
            _station.MaxRetryCount = 0;
            _station.DoExecuteResult = false;
            _station.Initialize();
            _station.Execute();

            Assert.That(errorMsg, Is.Not.Null);
        }

        [Test]
        public void Signal_SetAndWait_Works()
        {
            bool signalReceived = false;
            var thread = new System.Threading.Thread(() =>
            {
                signalReceived = _station.PublicWaitSignal("TestSignal", 2000);
            });
            thread.Start();
            System.Threading.Thread.Sleep(100);

            _station.PublicSetSignal("TestSignal");

            thread.Join(2000);
            Assert.That(signalReceived, Is.True);
        }

        [Test]
        public void Signal_Reset_ClearsState()
        {
            _station.PublicSetSignal("Sig");
            Assert.That(_station.PublicCheckSignal("Sig"), Is.True);

            _station.PublicResetSignal("Sig");

            Assert.That(_station.PublicCheckSignal("Sig"), Is.False);
        }

        [Test]
        public void Dispose_MultipleCalls_DoesNotThrow()
        {
            _station.Dispose();
            Assert.DoesNotThrow(() => _station.Dispose());
        }
    }
}
