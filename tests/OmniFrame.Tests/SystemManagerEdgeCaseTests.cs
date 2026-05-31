using System;
using Moq;
using NUnit.Framework;
using OmniFrame.Core;
using OmniFrame.Core.PluginSystem;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class SystemManagerEdgeCaseTests
    {
        private Mock<IDeviceManager> _deviceMgr;
        private Mock<ITaskManager> _taskMgr;
        private Mock<IAlarmManager> _alarmMgr;
        private Mock<IDataManager> _dataMgr;
        private Mock<IUserManager> _userMgr;
        private Mock<IRecipeManager> _recipeMgr;
        private Mock<IPermissionManager> _permMgr;
        private Mock<IPluginManager> _pluginMgr;
        private Mock<IPlcManager> _plcMgr;
        private Mock<IMotionManager> _motionMgr;
        private Mock<IIoManager> _ioMgr;
        private Mock<IProductManager> _productMgr;
        private SystemManager _sysMgr;

        [SetUp]
        public void Setup()
        {
            _deviceMgr = new Mock<IDeviceManager>();
            _taskMgr = new Mock<ITaskManager>();
            _alarmMgr = new Mock<IAlarmManager>();
            _dataMgr = new Mock<IDataManager>();
            _userMgr = new Mock<IUserManager>();
            _recipeMgr = new Mock<IRecipeManager>();
            _permMgr = new Mock<IPermissionManager>();
            _pluginMgr = new Mock<IPluginManager>();
            _plcMgr = new Mock<IPlcManager>();
            _motionMgr = new Mock<IMotionManager>();
            _ioMgr = new Mock<IIoManager>();
            _productMgr = new Mock<IProductManager>();

            _sysMgr = new SystemManager(
                _deviceMgr.Object, _taskMgr.Object, _alarmMgr.Object,
                _dataMgr.Object, _userMgr.Object, _recipeMgr.Object,
                _permMgr.Object, _pluginMgr.Object, _plcMgr.Object,
                _motionMgr.Object, _ioMgr.Object, _productMgr.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sysMgr.Dispose();
        }

        [Test]
        public void Constructor_NullDeviceManager_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SystemManager(null, _taskMgr.Object, _alarmMgr.Object,
                    _dataMgr.Object, _userMgr.Object, _recipeMgr.Object,
                    _permMgr.Object, _pluginMgr.Object, _plcMgr.Object,
                    _motionMgr.Object, _ioMgr.Object, _productMgr.Object));
        }

        [Test]
        public void Start_BeforeInitialize_ReturnsFalse()
        {
            _deviceMgr.Setup(d => d.IsReady).Returns(false);
            var result = _sysMgr.Start();
            Assert.That(result, Is.False);
        }

        [Test]
        public void Initialize_DeviceManagerFails_ReturnsFalse()
        {
            _deviceMgr.Setup(d => d.Initialize()).Returns(false);
            var result = _sysMgr.Initialize();
            Assert.That(result, Is.False);
        }

        [Test]
        public void Initialize_AllManagersSucceed_ReturnsTrue()
        {
            _deviceMgr.Setup(d => d.Initialize()).Returns(true);
            _taskMgr.Setup(t => t.Initialize()).Returns(true);
            _alarmMgr.Setup(a => a.Initialize()).Returns(true);
            _pluginMgr.Setup(p => p.Initialize(null)).Returns(true);
            _dataMgr.Setup(d => d.Initialize()).Returns(true);
            _userMgr.Setup(u => u.Initialize()).Returns(true);
            _recipeMgr.Setup(r => r.Initialize()).Returns(true);

            var result = _sysMgr.Initialize();
            Assert.That(result, Is.True);
        }

        [Test]
        public void Stop_WhenNotRunning_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _sysMgr.Stop());
        }

        [Test]
        public void EmergencyStop_WhenNotRunning_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _sysMgr.EmergencyStop());
        }

        [Test]
        public void GetSystemInfo_ReturnsNonNull()
        {
            var info = _sysMgr.GetSystemInfo();
            Assert.That(info, Is.Not.Null);
            Assert.That(info.State, Is.EqualTo(SystemState.Idle));
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _sysMgr.Dispose());
        }
    }
}
