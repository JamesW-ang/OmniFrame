using NUnit.Framework;
using OmniFrame.Core;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class ErrorCodeTests
    {
        [Test]
        public void GetDescription_AllErrorCodes_ReturnsNonEmpty()
        {
            var codes = (ErrorCode[])System.Enum.GetValues(typeof(ErrorCode));
            foreach (var code in codes)
            {
                var desc = code.GetDescription();
                Assert.That(desc, Is.Not.Null.And.Not.Empty, $"ErrorCode {code} should have description");
            }
        }

        [Test]
        public void GetAlarmLevel_CriticalCodes_ReturnsCritical()
        {
            Assert.That(ErrorCode.SystemCrash.GetAlarmLevel(), Is.EqualTo(AlarmLevel.Critical));
            Assert.That(ErrorCode.PlcDisconnect.GetAlarmLevel(), Is.EqualTo(AlarmLevel.Critical));
            Assert.That(ErrorCode.AxisAlarm.GetAlarmLevel(), Is.EqualTo(AlarmLevel.Critical));
        }

        [Test]
        public void GetAlarmLevel_WarningCodes_ReturnsWarning()
        {
            Assert.That(ErrorCode.ConfigFileCorrupted.GetAlarmLevel(), Is.EqualTo(AlarmLevel.Warning));
            Assert.That(ErrorCode.MotionTimeout.GetAlarmLevel(), Is.EqualTo(AlarmLevel.Warning));
        }

        [Test]
        public void GetSource_SystemCodes_ReturnsSystem()
        {
            Assert.That(ErrorCode.SystemCrash.GetSource(), Is.EqualTo("System"));
            Assert.That(ErrorCode.ConfigFileCorrupted.GetSource(), Is.EqualTo("System"));
        }

        [Test]
        public void GetSource_CommunicationCodes_ReturnsCommunication()
        {
            Assert.That(ErrorCode.PlcDisconnect.GetSource(), Is.EqualTo("Communication"));
            Assert.That(ErrorCode.NetworkError.GetSource(), Is.EqualTo("Communication"));
        }

        [Test]
        public void ErrorGuide_GetGuide_ByEnum_ReturnsGuide()
        {
            var guide = ErrorGuide.GetGuide(ErrorCode.PlcDisconnect);
            Assert.That(guide, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void ErrorGuide_GetGuide_ByInt_ReturnsGuide()
        {
            var guide = ErrorGuide.GetGuide(2001);
            Assert.That(guide, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void ErrorGuide_GetGuide_UnknownCode_ReturnsGeneralAdvice()
        {
            var guide = ErrorGuide.GetGuide((ErrorCode)99999);
            Assert.That(guide, Is.Not.Null.And.Not.Empty);
        }
    }
}
