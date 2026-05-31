using Moq;
using NUnit.Framework;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.Core.PluginSystem;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class SystemManagerTests
    {
        private Mock<IDeviceManager> _deviceMgrMock;
        private Mock<ITaskManager> _taskMgrMock;
        private Mock<IAlarmManager> _alarmMgrMock;
        private Mock<IDataManager> _dataMgrMock;
        private Mock<IUserManager> _userMgrMock;
        private Mock<IRecipeManager> _recipeMgrMock;
        private Mock<IPermissionManager> _permissionMgrMock;
        private Mock<IPluginManager> _pluginMgrMock;
        private Mock<IPlcManager> _plcMgrMock;
        private Mock<IMotionManager> _motionMgrMock;
        private Mock<IIoManager> _ioMgrMock;
        private Mock<IProductManager> _productMgrMock;

        [SetUp]
        public void Setup()
        {
            _deviceMgrMock = new Mock<IDeviceManager>();
            _taskMgrMock = new Mock<ITaskManager>();
            _alarmMgrMock = new Mock<IAlarmManager>();
            _dataMgrMock = new Mock<IDataManager>();
            _userMgrMock = new Mock<IUserManager>();
            _recipeMgrMock = new Mock<IRecipeManager>();
            _permissionMgrMock = new Mock<IPermissionManager>();
            _pluginMgrMock = new Mock<IPluginManager>();
            _plcMgrMock = new Mock<IPlcManager>();
            _motionMgrMock = new Mock<IMotionManager>();
            _ioMgrMock = new Mock<IIoManager>();
            _productMgrMock = new Mock<IProductManager>();

            // 默认所有子管理器初始化成功
            _deviceMgrMock.Setup(m => m.Initialize()).Returns(true);
            _taskMgrMock.Setup(m => m.Initialize()).Returns(true);
            _alarmMgrMock.Setup(m => m.Initialize()).Returns(true);
            _dataMgrMock.Setup(m => m.Initialize()).Returns(true);
            _userMgrMock.Setup(m => m.Initialize()).Returns(true);
            _recipeMgrMock.Setup(m => m.Initialize()).Returns(true);
            _pluginMgrMock.Setup(m => m.Initialize(It.IsAny<string>())).Returns(true);

            // 默认子管理器启动成功
            _deviceMgrMock.Setup(m => m.Start()).Returns(true);
            _taskMgrMock.Setup(m => m.Start()).Returns(true);
            _dataMgrMock.Setup(m => m.Start()).Returns(true);
        }

        private SystemManager CreateSystemManager()
        {
            return new SystemManager(
                _deviceMgrMock.Object,
                _taskMgrMock.Object,
                _alarmMgrMock.Object,
                _dataMgrMock.Object,
                _userMgrMock.Object,
                _recipeMgrMock.Object,
                _permissionMgrMock.Object,
                _pluginMgrMock.Object,
                _plcMgrMock.Object,
                _motionMgrMock.Object,
                _ioMgrMock.Object,
                _productMgrMock.Object);
        }

        [Test]
        public void Initialize_AllSubManagersSucceed_ReturnsTrue()
        {
            var system = CreateSystemManager();
            var result = system.Initialize();

            Assert.That(result, Is.True);
            Assert.That(system.State, Is.EqualTo(SystemState.Idle));
        }

        [Test]
        public void Initialize_DeviceManagerFails_ReturnsFalse()
        {
            _deviceMgrMock.Setup(m => m.Initialize()).Returns(false);
            var system = CreateSystemManager();

            var result = system.Initialize();

            Assert.That(result, Is.False);
        }

        [Test]
        public void Initialize_TaskManagerFails_ReturnsFalse()
        {
            _taskMgrMock.Setup(m => m.Initialize()).Returns(false);
            var system = CreateSystemManager();

            var result = system.Initialize();

            Assert.That(result, Is.False);
        }

        [Test]
        public void Initialize_AlarmManagerFails_ReturnsFalse()
        {
            _alarmMgrMock.Setup(m => m.Initialize()).Returns(false);
            var system = CreateSystemManager();

            var result = system.Initialize();

            Assert.That(result, Is.False);
        }

        [Test]
        public void Start_AfterInitialize_ReturnsTrueAndChangesState()
        {
            var system = CreateSystemManager();
            system.Initialize();

            var result = system.Start();

            Assert.That(result, Is.True);
            Assert.That(system.IsRunning, Is.True);
        }

        [Test]
        public void Start_WithoutInitialize_ReturnsFalse()
        {
            var system = CreateSystemManager();
            var result = system.Start();

            Assert.That(result, Is.False);
            Assert.That(system.IsRunning, Is.False);
        }

        [Test]
        public void Start_DeviceManagerFails_ReturnsFalse()
        {
            _deviceMgrMock.Setup(m => m.Start()).Returns(false);
            var system = CreateSystemManager();
            system.Initialize();

            var result = system.Start();

            Assert.That(result, Is.False);
        }

        [Test]
        public void Stop_AfterStart_ChangesToIdle()
        {
            var system = CreateSystemManager();
            system.Initialize();
            system.Start();

            system.Stop();

            Assert.That(system.IsRunning, Is.False);
            Assert.That(system.State, Is.EqualTo(SystemState.Idle));
        }

        [Test]
        public void EmergencyStop_ChangesStateAndCallsDeviceStop()
        {
            var system = CreateSystemManager();
            system.Initialize();

            system.EmergencyStop();

            Assert.That(system.State, Is.EqualTo(SystemState.EmergencyStop));
            _deviceMgrMock.Verify(m => m.EmergencyStop(), Times.Once);
            _taskMgrMock.Verify(m => m.EmergencyStop(), Times.Once);
        }

        [Test]
        public void Reset_FromErrorState_ReturnsTrue()
        {
            _deviceMgrMock.Setup(m => m.Reset()).Returns(true);
            _alarmMgrMock.Setup(m => m.ClearAllAlarms(It.IsAny<string>())).Callback(() => { });
            var system = CreateSystemManager();
            system.Initialize();
            system.EmergencyStop();

            var result = system.Reset();

            Assert.That(result, Is.True);
        }

        [Test]
        public void Reset_FromIdle_ReturnsFalse()
        {
            var system = CreateSystemManager();
            system.Initialize();

            var result = system.Reset();

            Assert.That(result, Is.False);
        }

        [Test]
        public void GetSystemInfo_ReturnsCurrentState()
        {
            var system = CreateSystemManager();
            system.Initialize();

            var info = system.GetSystemInfo();

            Assert.That(info.IsInitialized, Is.True);
            Assert.That(info.IsRunning, Is.False);
        }

        [Test]
        public void StateChanged_OnStateTransition_FiresEvent()
        {
            var system = CreateSystemManager();
            system.Initialize();
            SystemState? receivedState = null;
            system.StateChanged += (s, e) => receivedState = e.NewState;

            system.EmergencyStop();

            Assert.That(receivedState, Is.EqualTo(SystemState.EmergencyStop));
        }

        [Test]
        public void Start_RunningTime_IsTracking()
        {
            var system = CreateSystemManager();
            system.Initialize();
            system.Start();

            Assert.That(system.RunningTime.TotalSeconds, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void Dispose_StopsSystem()
        {
            var system = CreateSystemManager();
            system.Initialize();
            system.Start();

            system.Dispose();

            Assert.That(system.IsRunning, Is.False);
        }
    }
}
