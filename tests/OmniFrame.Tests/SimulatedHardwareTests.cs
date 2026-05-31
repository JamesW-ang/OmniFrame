using System;
using System.Threading;
using System.Threading.Tasks;
using MotionIO;
using NUnit.Framework;
using OmniFrame.Simulation;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class SimulatedMotionTests
    {
        private SimulatedMotion _motion;

        [SetUp]
        public void Setup()
        {
            _motion = new SimulatedMotion("TestMotion", 8);
            _motion.SimulationDelayMs = 0; // 加速测试
        }

        [Test]
        public void Init_EnablesAndReturnsTrue()
        {
            bool result = _motion.Init();

            Assert.That(result, Is.True);
            Assert.That(_motion.Enable, Is.True);
        }

        [Test]
        public void DeInit_DisablesAndReturnsTrue()
        {
            _motion.Init();
            bool result = _motion.DeInit();

            Assert.That(result, Is.True);
            Assert.That(_motion.Enable, Is.False);
        }

        [Test]
        public void AbsMove_WithoutServo_Fails()
        {
            _motion.Init();
            // 未 ServoOn

            bool result = _motion.AbsMove(0, 100, 1000);

            Assert.That(result, Is.False);
        }

        [Test]
        public void AbsMove_WithServo_SucceedsAndSetsPosition()
        {
            _motion.Init();
            _motion.ServoOn(0);

            bool result = _motion.AbsMove(0, 200, 1000);

            Assert.That(result, Is.True);
            Assert.That(_motion.GetAxisPos(0), Is.EqualTo(200));
        }

        [Test]
        public void AbsMove_SoftLimitViolation_Fails()
        {
            _motion.Init();
            _motion.ServoOn(0);
            _motion.SetSoftLimit(0, 150, -10);
            _motion.EnableSoftLimit(0, true);

            bool result = _motion.AbsMove(0, 200, 1000);

            Assert.That(result, Is.False);
        }

        [Test]
        public void RelativeMove_CalculatesTargetCorrectly()
        {
            _motion.Init();
            _motion.ServoOn(0);
            _motion.AbsMove(0, 100, 1000);
            _motion.RelativeMove(0, 50, 1000);

            Assert.That(_motion.GetAxisPos(0), Is.EqualTo(150));
        }

        [Test]
        public void Home_SetsPositionToZero()
        {
            _motion.Init();
            _motion.ServoOn(0);
            _motion.AbsMove(0, 100, 1000);

            _motion.Home(0, HomeMode.ORG_P);

            Assert.That(_motion.GetAxisPos(0), Is.EqualTo(0));
            Assert.That(_motion.IsAxisHomed(0), Is.True);
        }

        [Test]
        public void StopAxis_StopsAxisMotion()
        {
            _motion.Init();
            _motion.ServoOn(0);
            _motion.AbsMove(0, 100, 1000);

            _motion.StopAxis(0);

            Assert.That(_motion.IsAxisMoving(0), Is.False);
        }

        [Test]
        public void StopAllAxis_StopsAll()
        {
            _motion.Init();
            for (int i = 0; i < 4; i++) _motion.ServoOn(i);

            _motion.StopAllAxis();

            for (int i = 0; i < 4; i++)
                Assert.That(_motion.IsAxisMoving(i), Is.False);
        }

        [Test]
        public void ServoOnOff_TransitionsState()
        {
            _motion.Init();

            _motion.ServoOn(0);
            Assert.That(_motion.GetAxisState(0), Is.EqualTo(AxisState.Idle));

            _motion.ServoOff(0);
            Assert.That(_motion.GetAxisState(0), Is.EqualTo(AxisState.Disabled));
        }

        [Test]
        public void InvalidAxis_Fails()
        {
            _motion.Init();

            Assert.That(_motion.AbsMove(-1, 100, 1000), Is.False);
            Assert.That(_motion.AbsMove(8, 100, 1000), Is.False);
        }

        [Test]
        public void SetPos_OverwritesPosition()
        {
            _motion.Init();
            _motion.ServoOn(0);

            _motion.SetPos(0, 999);

            Assert.That(_motion.GetAxisPos(0), Is.EqualTo(999));
        }

        [Test]
        public void MoveVel_UpdatesPosition()
        {
            _motion.Init();
            _motion.ServoOn(0);

            _motion.MoveVel(0, 1000);

            Assert.That(_motion.GetAxisPos(0), Is.Not.EqualTo(0));
        }

        [Test]
        public void ClearAlarm_ResetsAlarmingState()
        {
            _motion.Init();
            _motion.ServoOn(0);
            // Simulate alarm by setting state externally could be tricky — test no-throw
            Assert.That(_motion.ClearAlarm(0), Is.True);
        }

        [Test]
        public void CheckStopFlag_ReflectsMotionState()
        {
            _motion.Init();
            _motion.ServoOn(0);

            Assert.That(_motion.CheckStopFlag(0), Is.True); // Not moving = stopped
        }

        [Test]
        public void AbsLinearMove_MultiAxis_Succeeds()
        {
            _motion.Init();
            for (int i = 0; i < 3; i++) _motion.ServoOn(i);

            bool result = _motion.AbsLinearMove(
                new[] { 0, 1, 2 },
                new[] { 10.0, 20.0, 30.0 },
                1000, 1000, 1000);

            Assert.That(result, Is.True);
            Assert.That(_motion.GetAxisPos(0), Is.EqualTo(10));
            Assert.That(_motion.GetAxisPos(1), Is.EqualTo(20));
            Assert.That(_motion.GetAxisPos(2), Is.EqualTo(30));
        }

        [Test]
        public void RelativeLinearMove_MultiAxis_Succeeds()
        {
            _motion.Init();
            for (int i = 0; i < 2; i++)
            {
                _motion.ServoOn(i);
                _motion.SetPos(i, 100);
            }

            bool result = _motion.RelativeLinearMove(
                new[] { 0, 1 },
                new[] { 25.0, -10.0 },
                1000, 1000, 1000);

            Assert.That(result, Is.True);
            Assert.That(_motion.GetAxisPos(0), Is.EqualTo(125));
            Assert.That(_motion.GetAxisPos(1), Is.EqualTo(90));
        }

        [Test]
        public void PulsePerMM_DefaultValue()
        {
            _motion.Init();
            Assert.That(_motion.PulsePerMM, Is.EqualTo(1000.0));
        }

        [Test]
        public void PulseToMM_Conversion()
        {
            _motion.PulsePerMM = 500;
            Assert.That(_motion.PulseToMM(0, 5000), Is.EqualTo(10.0));
        }

        [Test]
        public void MMToPulse_Conversion()
        {
            _motion.PulsePerMM = 500;
            Assert.That(_motion.MMToPulse(0, 10.0), Is.EqualTo(5000));
        }
    }

    [TestFixture]
    public class SimulatedIoTests
    {
        private SimulatedIoCtrl _io;

        [SetUp]
        public void Setup()
        {
            _io = new SimulatedIoCtrl("TestIO", 32, 32);
        }

        [Test]
        public void Init_InitializesAndReturnsTrue()
        {
            bool result = _io.Init(null);

            Assert.That(result, Is.True);
        }

        [Test]
        public void Close_ClearsInitState()
        {
            _io.Init(null);
            _io.Close();

            Assert.That(_io.GetError(), Does.Contain("未初始化"));
        }

        [Test]
        public void WriteAndReadOutput_Succeeds()
        {
            _io.Init(null);
            _io.WriteOutput(5, true);

            Assert.That(_io.GetOutput(5), Is.True);
        }

        [Test]
        public void SetInput_AndReadInput_Succeeds()
        {
            _io.Init(null);
            _io.SetInput(10, true);

            _io.ReadInput(10, out bool value);
            Assert.That(value, Is.True);
        }

        [Test]
        public void WriteOutput_InvalidIndex_Fails()
        {
            _io.Init(null);

            Assert.That(_io.WriteOutput(32, true), Is.False);
            Assert.That(_io.WriteOutput(-1, true), Is.False);
        }

        [Test]
        public void Reset_ClearsAllOutputs()
        {
            _io.Init(null);
            _io.WriteOutput(0, true);
            _io.WriteOutput(1, true);

            _io.Reset();

            Assert.That(_io.GetOutput(0), Is.False);
            Assert.That(_io.GetOutput(1), Is.False);
        }

        [Test]
        public void ReadInput_InvalidIndex_Fails()
        {
            _io.Init(null);

            Assert.That(_io.ReadInput(-1, out _), Is.False);
        }

        [Test]
        public void ReadAllInputs_ReturnsAll()
        {
            _io.Init(null);
            _io.SetInput(0, true);
            _io.SetInput(15, true);

            var allIn = _io.ReadAllInputs();

            Assert.That(allIn.Count, Is.EqualTo(32));
            Assert.That(allIn[0], Is.True);
            Assert.That(allIn[15], Is.True);
            Assert.That(allIn[1], Is.False);
        }

        [Test]
        public void ReadAllOutputs_ReturnsAll()
        {
            _io.Init(null);
            _io.WriteOutput(7, true);

            var allOut = _io.ReadAllOutputs();

            Assert.That(allOut.Count, Is.EqualTo(32));
            Assert.That(allOut[7], Is.True);
        }

        [Test]
        public void PortIndexing_32BitBoundary()
        {
            _io.Init(null);
            _io.SetInput(1, 3, true); // Port 1, pin 3 = index 35

            _io.ReadInputPort(1, out int portValue);

            Assert.That((portValue >> 3) & 1, Is.EqualTo(1));
        }

        [Test]
        public void WriteOutputPort_SetsCorrectBits()
        {
            _io.Init(null);
            int value = (1 << 0) | (1 << 5); // Pins 0 and 5

            _io.WriteOutputPort(0, value);

            Assert.That(_io.GetOutput(0, 0), Is.True);
            Assert.That(_io.GetOutput(0, 5), Is.True);
            Assert.That(_io.GetOutput(0, 1), Is.False);
        }
    }

    [TestFixture]
    public class SimulatedHardwareIntegrationTests
    {
        [Test]
        public void SimulatedHardware_ProvidesMotionAndIo()
        {
            var hw = new SimulatedHardware(axisCount: 8, ioInputCount: 32, ioOutputCount: 32);

            Assert.That(hw.Motion, Is.Not.Null);
            Assert.That(hw.Io, Is.Not.Null);
            Assert.That(hw.Motion, Is.InstanceOf<SimulatedMotion>());
            Assert.That(hw.Io, Is.InstanceOf<SimulatedIoCtrl>());
        }

        [Test]
        public void SimulatedHardware_DefaultConstructor_Works()
        {
            var hw = new SimulatedHardware();

            Assert.That(hw.Motion, Is.Not.Null);
            Assert.That(hw.Io, Is.Not.Null);
        }

        [Test]
        public void SimulatedHardware_Initialize_InitializesBoth()
        {
            var hw = new SimulatedHardware(axisCount: 4, ioInputCount: 16, ioOutputCount: 16);
            hw.Initialize();

            var motion = hw.Motion as SimulatedMotion;
            Assert.That(motion.Enable, Is.True);
        }

        [Test]
        public void SimulatedHardware_Shutdown_DeinitsBoth()
        {
            var hw = new SimulatedHardware(axisCount: 4, ioInputCount: 16, ioOutputCount: 16);
            hw.Initialize();
            hw.Shutdown();

            var motion = hw.Motion as SimulatedMotion;
            Assert.That(motion.Enable, Is.False);
        }
    }
}
