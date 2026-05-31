using Moq;
using NUnit.Framework;
using OmniFrame.Core;
using OmniFrame.DataAccess;
using System.Data;
using System.Threading.Tasks;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class AlarmManagerTests
    {
        private Mock<IAlarmNotification> _notificationMock;
        private Mock<IAlarmDb> _alarmDbMock;

        [SetUp]
        public void Setup()
        {
            _notificationMock = new Mock<IAlarmNotification>();
            _notificationMock
                .Setup(n => n.SendNotification(It.IsAny<AlarmInfo>(), null, null, null))
                .Returns(Task.CompletedTask);

            _alarmDbMock = new Mock<IAlarmDb>();
            _alarmDbMock.Setup(d => d.Open(It.IsAny<string>())).Returns(true);
        }

        private AlarmManager CreateAlarmManager()
        {
            return new AlarmManager(_notificationMock.Object, _alarmDbMock.Object);
        }

        [Test]
        public void Initialize_DatabaseOpens_ReturnsTrue()
        {
            var mgr = CreateAlarmManager();

            var result = mgr.Initialize();

            Assert.That(result, Is.True);
            _alarmDbMock.Verify(d => d.Open(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void Initialize_DatabaseFailsToOpen_ReturnsFalse()
        {
            _alarmDbMock.Setup(d => d.Open(It.IsAny<string>())).Returns(false);
            var mgr = CreateAlarmManager();

            var result = mgr.Initialize();

            Assert.That(result, Is.False);
        }

        [Test]
        public void AddAlarm_NewAlarm_ReturnsAlarmInfo()
        {
            var mgr = CreateAlarmManager();

            var alarm = mgr.AddAlarm("ERR001", "Test error", AlarmLevel.Error, "TestSource");

            Assert.That(alarm, Is.Not.Null);
            Assert.That(alarm.AlarmCode, Is.EqualTo("ERR001"));
            Assert.That(alarm.AlarmMessage, Is.EqualTo("Test error"));
            Assert.That(alarm.Level, Is.EqualTo(AlarmLevel.Error));
            Assert.That(alarm.Source, Is.EqualTo("TestSource"));
        }

        [Test]
        public void AddAlarm_NewAlarm_AddedToActiveList()
        {
            var mgr = CreateAlarmManager();
            mgr.AddAlarm("ERR001", "Test", AlarmLevel.Error, "Src");

            var active = mgr.GetActiveAlarms();

            Assert.That(active.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddAlarm_DuplicateCode_ReturnsExistingAlarm()
        {
            var mgr = CreateAlarmManager();
            var first = mgr.AddAlarm("ERR001", "First", AlarmLevel.Error, "Src");
            var second = mgr.AddAlarm("ERR001", "Second", AlarmLevel.Error, "Src");

            Assert.That(second, Is.SameAs(first));
            Assert.That(mgr.ActiveAlarmCount, Is.EqualTo(1));
        }

        [Test]
        public void AddAlarm_CriticalAlarm_HasCriticalFlag()
        {
            var mgr = CreateAlarmManager();

            mgr.AddAlarm("CRIT001", "Critical", AlarmLevel.Critical, "Src");

            Assert.That(mgr.HasCriticalAlarm, Is.True);
        }

        [Test]
        public void AddAlarm_InfoLevel_DoesNotSetCriticalFlag()
        {
            var mgr = CreateAlarmManager();

            mgr.AddAlarm("INF001", "Info", AlarmLevel.Info, "Src");

            Assert.That(mgr.HasCriticalAlarm, Is.False);
        }

        [Test]
        public void AddAlarm_CriticalAlarm_SavesToDatabase()
        {
            var mgr = CreateAlarmManager();

            mgr.AddAlarm("CRIT001", "Critical", AlarmLevel.Critical, "Src");

            _alarmDbMock.Verify(d => d.AddAlarm(It.Is<AlarmRecord>(r =>
                r.AlarmCode == "CRIT001" &&
                r.AlarmLevel == "Critical")), Times.Once);
        }

        [Test]
        public void AddAlarm_CriticalAlarm_FiresNotification()
        {
            var mgr = CreateAlarmManager();

            mgr.AddAlarm("CRIT001", "Critical", AlarmLevel.Critical, "Src");

            _notificationMock.Verify(n => n.SendNotification(
                It.Is<AlarmInfo>(a => a.AlarmCode == "CRIT001"), null, null, null), Times.Once);
        }

        [Test]
        public void ClearAlarm_ExistingAlarm_RemovesFromActive()
        {
            var mgr = CreateAlarmManager();
            var alarm = mgr.AddAlarm("ERR001", "Test", AlarmLevel.Error, "Src");

            var result = mgr.ClearAlarm(alarm.Id, "TestUser");

            Assert.That(result, Is.True);
            Assert.That(mgr.ActiveAlarmCount, Is.EqualTo(0));
        }

        [Test]
        public void ClearAlarm_NonExistent_ReturnsFalse()
        {
            var mgr = CreateAlarmManager();

            var result = mgr.ClearAlarm(99999);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ClearAlarm_UpdatesDatabase()
        {
            var mgr = CreateAlarmManager();
            var alarm = mgr.AddAlarm("ERR001", "Test", AlarmLevel.Error, "Src");

            mgr.ClearAlarm(alarm.Id, "Operator");

            _alarmDbMock.Verify(d => d.ClearAlarm(alarm.Id, "Operator"), Times.Once);
        }

        [Test]
        public void ClearAllAlarms_ClearsAllActive()
        {
            var mgr = CreateAlarmManager();
            mgr.AddAlarm("ERR001", "Error1", AlarmLevel.Error, "Src");
            mgr.AddAlarm("ERR002", "Error2", AlarmLevel.Warning, "Src");

            mgr.ClearAllAlarms("Admin");

            Assert.That(mgr.ActiveAlarmCount, Is.EqualTo(0));
        }

        [Test]
        public void GetAlarmsByLevel_FiltersCorrectly()
        {
            var mgr = CreateAlarmManager();
            mgr.AddAlarm("ERR001", "Error", AlarmLevel.Error, "Src");
            mgr.AddAlarm("WRN001", "Warning", AlarmLevel.Warning, "Src");

            var warnings = mgr.GetAlarmsByLevel(AlarmLevel.Warning);

            Assert.That(warnings.Count, Is.EqualTo(1));
            Assert.That(warnings[0].AlarmCode, Is.EqualTo("WRN001"));
        }

        [Test]
        public void GetStatistics_ReturnsCorrectCounts()
        {
            var mgr = CreateAlarmManager();
            mgr.AddAlarm("INF001", "Info", AlarmLevel.Info, "Src");
            mgr.AddAlarm("WRN001", "Warning", AlarmLevel.Warning, "Src");
            mgr.AddAlarm("ERR001", "Error", AlarmLevel.Error, "Src");
            mgr.AddAlarm("CRIT001", "Critical", AlarmLevel.Critical, "Src");

            var stats = mgr.GetStatistics();

            Assert.That(stats.TotalCount, Is.EqualTo(4));
            Assert.That(stats.InfoCount, Is.EqualTo(1));
            Assert.That(stats.WarningCount, Is.EqualTo(1));
            Assert.That(stats.ErrorCount, Is.EqualTo(1));
            Assert.That(stats.CriticalCount, Is.EqualTo(1));
            Assert.That(stats.ActiveCount, Is.EqualTo(4));
        }

        [Test]
        public void HasActiveAlarm_WithActiveAlarms_ReturnsTrue()
        {
            var mgr = CreateAlarmManager();
            mgr.AddAlarm("ERR001", "Test", AlarmLevel.Error, "Src");

            Assert.That(mgr.HasActiveAlarm, Is.True);
        }

        [Test]
        public void HasActiveAlarm_AfterClearAll_ReturnsFalse()
        {
            var mgr = CreateAlarmManager();
            mgr.AddAlarm("ERR001", "Test", AlarmLevel.Error, "Src");
            mgr.ClearAllAlarms();

            Assert.That(mgr.HasActiveAlarm, Is.False);
        }
    }
}
