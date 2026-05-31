using System;
using Moq;
using NUnit.Framework;
using OmniFrame.Core;
using OmniFrame.DataAccess;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class AlarmManagerRobustnessTests
    {
        private Mock<IAlarmDb> _mockDb;
        private Mock<IAlarmNotification> _mockNotify;
        private AlarmManager _mgr;

        [SetUp]
        public void Setup()
        {
            _mockDb = new Mock<IAlarmDb>();
            _mockNotify = new Mock<IAlarmNotification>();
            _mgr = new AlarmManager(_mockNotify.Object, _mockDb.Object);
        }

        [Test]
        public void AddAlarm_NullCode_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _mgr.AddAlarm(null, "test", AlarmLevel.Error, "source"));
        }

        [Test]
        public void AddAlarm_EmptyMessage_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _mgr.AddAlarm("E001", "", AlarmLevel.Warning, "source"));
        }

        [Test]
        public void ClearAlarm_InvalidId_ReturnsFalse()
        {
            var result = _mgr.ClearAlarm(99999);
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetActiveAlarms_AfterClearAll_ReturnsEmpty()
        {
            _mgr.AddAlarm("E001", "test", AlarmLevel.Error, "s1");
            _mgr.AddAlarm("E002", "test", AlarmLevel.Warning, "s2");
            Assert.That(_mgr.ActiveAlarmCount, Is.EqualTo(2));

            _mgr.ClearAllAlarms("admin");
            Assert.That(_mgr.ActiveAlarmCount, Is.EqualTo(0));
        }

        [Test]
        public void GetAlarmsByLevel_FiltersCorrectly()
        {
            _mgr.AddAlarm("E001", "error", AlarmLevel.Error, "s");
            _mgr.AddAlarm("W001", "warn", AlarmLevel.Warning, "s");
            _mgr.AddAlarm("I001", "info", AlarmLevel.Info, "s");

            var errors = _mgr.GetAlarmsByLevel(AlarmLevel.Error);
            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0].Level, Is.EqualTo(AlarmLevel.Error));
        }

        [Test]
        public void HasActiveAlarm_NoAlarms_ReturnsFalse()
        {
            Assert.That(_mgr.HasActiveAlarm, Is.False);
        }

        [Test]
        public void AddAlarm_CriticalLevel_SetsHasCriticalAlarm()
        {
            _mgr.AddAlarm("C001", "critical", AlarmLevel.Critical, "s");
            Assert.That(_mgr.HasCriticalAlarm, Is.True);
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            _mgr.AddAlarm("E001", "test", AlarmLevel.Error, "s");
            Assert.DoesNotThrow(() => _mgr.Dispose());
        }
    }
}
