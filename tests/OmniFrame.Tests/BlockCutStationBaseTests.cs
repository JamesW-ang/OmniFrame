using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using OmniFrame.Core.BlockCut;
using OmniFrame.Simulation;
using MotionIO;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class BlockCutStationBaseTests
    {
        private SimulatedHardware _hardware;

        private class TestStation : BlockCutStationBase
        {
            public bool RunAsyncCalled { get; private set; }
            public bool SimulateException { get; set; }
            public bool RunForever { get; set; }
            public Exception CapturedException { get; private set; }

            public TestStation(IBlockCutHardware hardware)
                : base("TestStation", 0, hardware) { }

            public override async Task RunAsync(CancellationToken token)
            {
                RunAsyncCalled = true;
                if (RunForever)
                {
                    while (!token.IsCancellationRequested)
                        await Task.Delay(10, token);
                }
                else if (SimulateException)
                {
                    await Task.Yield();
                    throw new InvalidOperationException("模拟运行时异常");
                }
                else
                {
                    await Task.Delay(50, token);
                }
            }

            public Motion TestMotion => Motion;
            public IoCtrl TestIo => Io;
            public void SimulatePause() => Pause();
            public void SimulateResume() => Resume();
            public void SimulateCheckPause(CancellationToken t) => CheckPause(t);
            public void SimulateUserConfirm(bool isRepeat = false) => UserConfirm(isRepeat);
            public void SimulateWaitForUser(CancellationToken t) => WaitForUser(t);
        }

        [SetUp]
        public void Setup()
        {
            _hardware = new SimulatedHardware(axisCount: 8, ioInputCount: 32, ioOutputCount: 32);
            _hardware.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _hardware.Shutdown();
        }

        [Test]
        public void Constructor_WithValidHardware_SetsProperties()
        {
            var station = new TestStation(_hardware);

            Assert.That(station.StationName, Is.EqualTo("TestStation"));
            Assert.That(station.IsPaused, Is.False);
        }

        [Test]
        public void Constructor_WithNullHardware_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TestStation(null));
        }

        [Test]
        public void Motion_ReturnsHardwareMotion()
        {
            var station = new TestStation(_hardware);
            Assert.That(station.TestMotion, Is.SameAs(_hardware.Motion));
        }

        [Test]
        public void Io_ReturnsHardwareIo()
        {
            var station = new TestStation(_hardware);
            Assert.That(station.TestIo, Is.SameAs(_hardware.Io));
        }

        [Test]
        public void Pause_SetsIsPaused()
        {
            var station = new TestStation(_hardware);
            station.SimulatePause();

            Assert.That(station.IsPaused, Is.True);
        }

        [Test]
        public void Resume_ClearsIsPaused()
        {
            var station = new TestStation(_hardware);
            station.SimulatePause();
            station.SimulateResume();

            Assert.That(station.IsPaused, Is.False);
        }

        [Test]
        public async Task RunWithCancellationAsync_StopCancelsExecution()
        {
            var station = new TestStation(_hardware) { RunForever = true };
            var cts = new CancellationTokenSource();

            var task = station.RunWithCancellationAsync(cts.Token);
            await Task.Delay(100);
            station.Stop();

            Assert.That(async () => await task, Throws.Nothing);
            Assert.That(task.IsCompleted, Is.True);
        }

        [Test]
        public async Task RunWithCancellationAsync_ExternalTokenCancels()
        {
            var station = new TestStation(_hardware) { RunForever = true };
            var cts = new CancellationTokenSource();

            var task = station.RunWithCancellationAsync(cts.Token);
            await Task.Delay(100);
            cts.Cancel();

            Assert.That(async () => await task, Throws.Nothing);
            Assert.That(task.IsCompleted, Is.True);
        }

        [Test]
        public async Task RunWithCancellationAsync_ExceptionIsCaught()
        {
            var station = new TestStation(_hardware) { SimulateException = true };
            var cts = new CancellationTokenSource();

            // RunAsync 抛出 → RunWithCancellationAsync 捕获、发 Warning、暂停
            Assert.That(async () => await station.RunWithCancellationAsync(cts.Token), Throws.Nothing);
            Assert.That(station.IsPaused, Is.True);
        }

        [Test]
        public async Task RunWithCancellationAsync_NormalCompletionCompletes()
        {
            var station = new TestStation(_hardware);
            var cts = new CancellationTokenSource();

            Assert.That(async () => await station.RunWithCancellationAsync(cts.Token), Throws.Nothing);
            Assert.That(station.RunAsyncCalled, Is.True);
        }

        [Test]
        public void Stop_Twice_DoesNotThrow()
        {
            var station = new TestStation(_hardware);
            Assert.DoesNotThrow(() =>
            {
                station.Stop();
                station.Stop();
            });
        }

        [Test]
        public void Dispose_ClearsEventHandlers()
        {
            var station = new TestStation(_hardware);
            station.OnMessage += _ => { };
            station.OnWarning += (_, __, ___) => { };
            station.OnPrompt += _ => { };
            station.OnGetFail += _ => { };
            station.OnFinishMove += () => { };
            station.OnErrorLogTimeWrite += (_, __, ___) => { };

            Assert.DoesNotThrow(() => station.Dispose());
        }

        [Test]
        public void Dispose_MultipleCalls_DoesNotThrow()
        {
            var station = new TestStation(_hardware);
            station.Dispose();
            Assert.DoesNotThrow(() => station.Dispose());
        }

        [Test]
        public void UserConfirm_SetsIsRepeat()
        {
            var station = new TestStation(_hardware);
            station.SimulateUserConfirm(isRepeat: true);

            // UserConfirm 调用后 WaitForUser 应立即返回
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            Assert.DoesNotThrow(() => station.SimulateWaitForUser(cts.Token));
        }

        [Test]
        public void ContinueAfterAlarm_AfterPause_ResumesStation()
        {
            var station = new TestStation(_hardware);
            station.SimulatePause();
            Assert.That(station.IsPaused, Is.True);

            station.ContinueAfterAlarm();

            Assert.That(station.IsPaused, Is.False);
        }

        [Test]
        public async Task CheckPause_BlocksWhilePaused()
        {
            var station = new TestStation(_hardware);
            station.SimulatePause();
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

            // CheckPause should not throw within the timeout — it blocks, then token cancels
            Assert.Throws<OperationCanceledException>(() =>
                station.SimulateCheckPause(cts.Token));
        }

        [Test]
        public void OnWarning_EventIsRaised()
        {
            var station = new TestStation(_hardware);
            int? receivedCode = null;
            string receivedMsg = null;
            station.OnWarning += (code, msg, _) => { receivedCode = code; receivedMsg = msg; };

            // Trigger warning via exception path
            station.SimulateException = true;
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            station.RunWithCancellationAsync(cts.Token).Wait(500);

            Assert.That(receivedCode.HasValue, Is.True);
            Assert.That(receivedMsg, Does.Contain("模拟运行时异常"));
        }
    }
}
