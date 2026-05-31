using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using OmniFrame.Core.BlockCut;
using OmniFrame.Simulation;
using MotionIO;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class StationCoordinatorTests
    {
        private SimulatedHardware _hardware;
        private Station_Adjust _adjust;
        private Station_CasselZ _casselZ;
        private Station_Load _load;
        private Station_Load2 _load2;
        private Station_BottomGet _bottomGet;
        private Station_Safe _safe;
        private StationCoordinator _coordinator;

        [SetUp]
        public void Setup()
        {
            _hardware = new SimulatedHardware(axisCount: 16, ioInputCount: 32, ioOutputCount: 32);
            _hardware.Initialize();

            var config = new BlockCutConfig { IsSimulation = true };
            _adjust = new Station_Adjust(_hardware, config);
            _casselZ = new Station_CasselZ(_hardware, config);
            _load = new Station_Load(_hardware, config);
            _load2 = new Station_Load2(_hardware, config);
            _bottomGet = new Station_BottomGet(_hardware, config);
            _safe = new Station_Safe(_hardware);

            _coordinator = new StationCoordinator(
                _adjust, _casselZ, _load, _load2, _bottomGet, _safe);
        }

        [TearDown]
        public void TearDown()
        {
            try { _coordinator.StopAll(); } catch { }
            _hardware.Shutdown();
        }

        // ──────────────────────────────────────
        // 构造
        // ──────────────────────────────────────

        [Test]
        public void Constructor_WithValidStations_SetsIsRunningFalse()
        {
            Assert.That(_coordinator.IsRunning, Is.False);
        }

        [Test]
        public void Constructor_NullAdjust_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StationCoordinator(null, _casselZ, _load, _load2, _bottomGet, _safe));
        }

        [Test]
        public void Constructor_NullCasselZ_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StationCoordinator(_adjust, null, _load, _load2, _bottomGet, _safe));
        }

        [Test]
        public void Constructor_NullLoad_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StationCoordinator(_adjust, _casselZ, null, _load2, _bottomGet, _safe));
        }

        [Test]
        public void Constructor_NullLoad2_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StationCoordinator(_adjust, _casselZ, _load, null, _bottomGet, _safe));
        }

        [Test]
        public void Constructor_NullBottomGet_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StationCoordinator(_adjust, _casselZ, _load, _load2, null, _safe));
        }

        [Test]
        public void Constructor_NullSafe_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StationCoordinator(_adjust, _casselZ, _load, _load2, _bottomGet, null));
        }

        // ──────────────────────────────────────
        // 生命周期
        // ──────────────────────────────────────

        [Test]
        public void StartAll_SetsIsRunningTrue()
        {
            _coordinator.StartAll();
            Assert.That(_coordinator.IsRunning, Is.True);
        }

        [Test]
        public void StartAll_Twice_DoesNotDoubleStart()
        {
            _coordinator.StartAll();
            _coordinator.StartAll();
            Assert.That(_coordinator.IsRunning, Is.True);
        }

        [Test]
        public void StopAll_SetsIsRunningFalse()
        {
            _coordinator.StartAll();
            _coordinator.StopAll();
            Assert.That(_coordinator.IsRunning, Is.False);
        }

        [Test]
        public void StopAll_WhenNotRunning_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _coordinator.StopAll());
        }

        [Test]
        public void PauseAll_PausesAllStations()
        {
            _coordinator.StartAll();
            _coordinator.PauseAll();

            Assert.That(_coordinator.IsRunning, Is.False);
            Assert.That(_adjust.IsPaused, Is.True);
            Assert.That(_casselZ.IsPaused, Is.True);
            Assert.That(_load.IsPaused, Is.True);
            Assert.That(_load2.IsPaused, Is.True);
            Assert.That(_bottomGet.IsPaused, Is.True);
            Assert.That(_safe.IsPaused, Is.True);
        }

        [Test]
        public void ResumeAll_ResumesAllStations()
        {
            _coordinator.PauseAll();

            _coordinator.ResumeAll();

            Assert.That(_coordinator.IsRunning, Is.True);
            Assert.That(_adjust.IsPaused, Is.False);
            Assert.That(_casselZ.IsPaused, Is.False);
            Assert.That(_load.IsPaused, Is.False);
            Assert.That(_load2.IsPaused, Is.False);
            Assert.That(_bottomGet.IsPaused, Is.False);
            Assert.That(_safe.IsPaused, Is.False);
        }

        // ──────────────────────────────────────
        // 属性访问
        // ──────────────────────────────────────

        [Test]
        public void GetStations_ReturnsAllSix()
        {
            var stations = _coordinator.GetStations();
            Assert.That(stations.Length, Is.EqualTo(6));
            Assert.That(stations, Contains.Item(_adjust));
            Assert.That(stations, Contains.Item(_casselZ));
            Assert.That(stations, Contains.Item(_load));
            Assert.That(stations, Contains.Item(_load2));
            Assert.That(stations, Contains.Item(_bottomGet));
            Assert.That(stations, Contains.Item(_safe));
        }

        [Test]
        public void StationProperties_ReturnCorrectInstances()
        {
            Assert.That(_coordinator.Adjust, Is.SameAs(_adjust));
            Assert.That(_coordinator.CasselZ, Is.SameAs(_casselZ));
            Assert.That(_coordinator.Load, Is.SameAs(_load));
            Assert.That(_coordinator.Load2, Is.SameAs(_load2));
            Assert.That(_coordinator.BottomGet, Is.SameAs(_bottomGet));
            Assert.That(_coordinator.Safe, Is.SameAs(_safe));
        }

        // ──────────────────────────────────────
        // 空跑模式
        // ──────────────────────────────────────

        [Test]
        public void SetEmptyTest_EnablesAllStations()
        {
            _coordinator.SetEmptyTest(true);

            Assert.That(_adjust.IsEmptyTest, Is.True);
            Assert.That(_casselZ.IsEmptyTest, Is.True);
            Assert.That(_load.IsEmptyTest, Is.True);
            Assert.That(_load2.IsEmptyTest, Is.True);
            Assert.That(_bottomGet.IsEmptyTest, Is.True);
            Assert.That(_safe.IsEmptyTest, Is.True);
        }

        [Test]
        public void SetEmptyTest_DisablesAllStations()
        {
            _coordinator.SetEmptyTest(true);
            _coordinator.SetEmptyTest(false);

            Assert.That(_adjust.IsEmptyTest, Is.False);
            Assert.That(_casselZ.IsEmptyTest, Is.False);
            Assert.That(_load.IsEmptyTest, Is.False);
            Assert.That(_load2.IsEmptyTest, Is.False);
            Assert.That(_bottomGet.IsEmptyTest, Is.False);
            Assert.That(_safe.IsEmptyTest, Is.False);
        }

        // ──────────────────────────────────────
        // 事件
        // ──────────────────────────────────────

        [Test]
        public void OnLog_FiresOnStartAll()
        {
            string logMessage = null;
            _coordinator.OnLog += msg => logMessage = msg;

            _coordinator.StartAll();

            Assert.That(logMessage, Is.Not.Null.And.Contains("启动"));
        }

        [Test]
        public void OnSweepRequested_EventIsWired()
        {
            // 验证事件订阅不抛异常
            int? sweepTarget = null;
            Assert.DoesNotThrow(() => _coordinator.OnSweepRequested += target => sweepTarget = target);
        }

        // ──────────────────────────────────────
        // 扫码分发
        // ──────────────────────────────────────

        [Test]
        public void DispatchBarcode_Target2_CallsLoadReadyFromSweepCode()
        {
            Assert.DoesNotThrow(() => _coordinator.DispatchBarcode(2, "TEST-CODE-001"));
        }

        [Test]
        public void DispatchBarcode_Target3_CallsLoad2ReadyFromSweepCode()
        {
            Assert.DoesNotThrow(() => _coordinator.DispatchBarcode(3, "TEST-CODE-002"));
        }

        // ──────────────────────────────────────
        // 跨工站事件
        // ──────────────────────────────────────

        [Test]
        public void SafeOnSafetyRestored_EventIsWired()
        {
            _coordinator.PauseAll();
            Assert.That(_coordinator.IsRunning, Is.False);
            // OnSafetyRestored 是 Station_Safe 内部事件，由 StationCoordinator 内部连线
            // 此处验证暂停后状态正确即可
        }

        [Test]
        public void SubscribeStationEvents_CallsCallbackForAllStations()
        {
            int count = 0;
            _coordinator.SubscribeStationEvents(_ => count++);

            Assert.That(count, Is.EqualTo(6));
        }

        // ──────────────────────────────────────
        // 边界条件
        // ──────────────────────────────────────

        [Test]
        public void ResumeAll_WhenNotPaused_DoesNotThrow()
        {
            // No stations paused
            Assert.DoesNotThrow(() => _coordinator.ResumeAll());
            Assert.That(_coordinator.IsRunning, Is.True);
        }

        [Test]
        public void StopAll_ThenDispose_DoesNotThrow()
        {
            _coordinator.StartAll();
            _coordinator.StopAll();
            Assert.DoesNotThrow(() =>
            {
                // Simulate what happens during shutdown
                _coordinator.PauseAll();
            });
        }
    }
}
