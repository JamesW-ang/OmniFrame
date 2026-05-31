using Moq;
using NUnit.Framework;
using OmniFrame.Common;
using OmniFrame.Core;
using System;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class DeviceManagerTests
    {
        private DeviceManager _deviceManager;

        [SetUp]
        public void Setup()
        {
            _deviceManager = new DeviceManager();
        }

        [TearDown]
        public void TearDown()
        {
            _deviceManager.Dispose();
        }

        [Test]
        public void Initialize_CompletesSuccessfully()
        {
            var result = _deviceManager.Initialize();
            Assert.That(result, Is.True);
        }

        [Test]
        public void AddDevice_ValidDevice_AddsToCollection()
        {
            var mock = new Mock<IDevice>();
            mock.Setup(d => d.Name).Returns("TestDevice");

            bool added = _deviceManager.AddDevice("TestDevice", mock.Object);

            Assert.That(added, Is.True);
            Assert.That(_deviceManager.DeviceCount, Is.EqualTo(1));
        }

        [Test]
        public void AddDevice_DuplicateName_ReturnsFalse()
        {
            var mock = new Mock<IDevice>();
            mock.Setup(d => d.Name).Returns("Dev");
            _deviceManager.AddDevice("Dev", mock.Object);

            bool added = _deviceManager.AddDevice("Dev", new Mock<IDevice>().Object);

            Assert.That(added, Is.False);
        }

        [Test]
        public void AddDevice_NullDevice_ReturnsFalse()
        {
            bool added = _deviceManager.AddDevice("Null", null);
            Assert.That(added, Is.False);
        }

        [Test]
        public void GetDevice_ExistingName_ReturnsDevice()
        {
            var mock = new Mock<IDevice>();
            mock.Setup(d => d.Name).Returns("Dev");
            _deviceManager.AddDevice("Dev", mock.Object);

            var result = _deviceManager.GetDevice<IDevice>("Dev");

            Assert.That(result, Is.SameAs(mock.Object));
        }

        [Test]
        public void GetDevice_NonExisting_ReturnsNull()
        {
            var result = _deviceManager.GetDevice<IDevice>("NoExist");
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Start_AllDevicesConnect_RaisesConnectedEvent()
        {
            var mock = new Mock<IDevice>();
            mock.Setup(d => d.Connect()).Returns(true);
            mock.Setup(d => d.Name).Returns("Dev");
            _deviceManager.AddDevice("Dev", mock.Object);

            bool eventFired = false;
            _deviceManager.DeviceConnected += (s, name) => { if (name == "Dev") eventFired = true; };

            bool result = _deviceManager.Start();

            Assert.That(result, Is.True);
            Assert.That(_deviceManager.IsReady, Is.True);
            Assert.That(eventFired, Is.True);
            mock.Verify(d => d.Connect(), Times.Once);
        }

        [Test]
        public void Start_DeviceFailsToConnect_RaisesError()
        {
            var mock = new Mock<IDevice>();
            mock.Setup(d => d.Connect()).Returns(false);
            mock.Setup(d => d.Name).Returns("FailingDev");
            _deviceManager.AddDevice("FailingDev", mock.Object);

            bool errorFired = false;
            _deviceManager.DeviceErrorOccurred += (s, err) => errorFired = true;

            bool result = _deviceManager.Start();

            Assert.That(result, Is.False);
            Assert.That(_deviceManager.IsReady, Is.False);
            Assert.That(errorFired, Is.True);
        }

        [Test]
        public void Stop_CallsDisconnectOnAllDevices()
        {
            var mock = new Mock<IDevice>();
            mock.Setup(d => d.Connect()).Returns(true);
            mock.Setup(d => d.Name).Returns("Dev");
            _deviceManager.AddDevice("Dev", mock.Object);
            _deviceManager.Start();

            _deviceManager.Stop();

            mock.Verify(d => d.Disconnect(), Times.Once);
            Assert.That(_deviceManager.IsReady, Is.False);
        }

        [Test]
        public void AddCustomDevice_AddsDevice()
        {
            var mock = new Mock<IDevice>();
            mock.Setup(d => d.Name).Returns("Custom");
            bool added = _deviceManager.AddCustomDevice("Custom", mock.Object);
            Assert.That(added, Is.True);
        }

        [Test]
        public void ClearErrors_AfterError_ListIsEmpty()
        {
            var mock = new Mock<IDevice>();
            mock.Setup(d => d.Connect()).Returns(false);
            mock.Setup(d => d.Name).Returns("BadDev");
            _deviceManager.AddDevice("BadDev", mock.Object);
            _deviceManager.Start();

            _deviceManager.ClearErrors();

            Assert.That(_deviceManager.HasError, Is.False);
        }

        [Test]
        public void Reset_CallsResetOnAllDevices()
        {
            var mock = new Mock<IDevice>();
            mock.Setup(d => d.Reset()).Returns(true);
            mock.Setup(d => d.Name).Returns("Dev");
            _deviceManager.AddDevice("Dev", mock.Object);

            bool result = _deviceManager.Reset();

            mock.Verify(d => d.Reset(), Times.Once);
            Assert.That(result, Is.True);
        }

        [Test]
        public void EmergencyStop_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _deviceManager.EmergencyStop());
        }
    }
}
