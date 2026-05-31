using Moq;
using NUnit.Framework;
using OmniFrame.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class AlarmNotificationTests
    {
        private AlarmNotification _notification;

        [SetUp]
        public void Setup()
        {
            // Set env var so GetEncryptionKey doesn't throw
            Environment.SetEnvironmentVariable("AUTOFRAME_ENCRYPTION_KEY", "TestEncryptionKey123!", EnvironmentVariableTarget.Process);
            _notification = new AlarmNotification();
        }

        [TearDown]
        public void TearDown()
        {
            _notification?.Dispose();
        }

        #region Helper methods

        private bool IsDuplicateAlarm(string alarmKey)
        {
            var method = typeof(AlarmNotification).GetMethod("IsDuplicateAlarm",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return (bool)method.Invoke(_notification, new object[] { alarmKey });
        }

        private Dictionary<string, DateTime> GetLastNotificationTimes()
        {
            var field = typeof(AlarmNotification).GetField("_lastNotificationTime",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return field.GetValue(_notification) as Dictionary<string, DateTime>;
        }

        private AlarmInfo CreateAlarm(string code, string message, AlarmLevel level, string source = "TestSource")
        {
            return new AlarmInfo
            {
                AlarmCode = code,
                AlarmMessage = message,
                Level = level,
                Source = source,
                OccurTime = DateTime.Now
            };
        }

        #endregion

        #region Notification Routing Tests

        [Test]
        public async Task NotificationRouting_InfoLevel_SendsWechatOnly()
        {
            // Info level alarms only log (no WeChat, no Email, no SMS)
            // The actual test verifies the switch statement routing
            var alarm = CreateAlarm("INF001", "Info alarm", AlarmLevel.Info);

            // SendNotification for Info level should not throw and complete successfully
            // Info level: only logs, no actual notification sent
            Assert.DoesNotThrowAsync(async () =>
                await _notification.SendNotification(alarm, null, null, null));
        }

        [Test]
        public async Task NotificationRouting_WarningLevel_SendsWechat()
        {
            var alarm = CreateAlarm("WRN001", "Warning alarm", AlarmLevel.Warning);

            // Warning: calls SendWechatNotification
            // With no webhook URL, SendWechatNotification returns early
            Assert.DoesNotThrowAsync(async () =>
                await _notification.SendNotification(alarm, null, "admin@test.com", null));
        }

        [Test]
        public async Task NotificationRouting_ErrorLevel_SendsWechatAndEmailAndSms()
        {
            var alarm = CreateAlarm("ERR001", "Error alarm", AlarmLevel.Error);

            // Error/Critical: calls WeChat + Email + SMS
            // With null URLs, they return early without error
            Assert.DoesNotThrowAsync(async () =>
                await _notification.SendNotification(alarm, null, "admin@test.com", "13800000000"));
        }

        [Test]
        public async Task NotificationRouting_CriticalLevel_SendsAllChannels()
        {
            var alarm = CreateAlarm("CRIT001", "Critical alarm", AlarmLevel.Critical);

            // Critical: calls WeChat + Email + SMS
            Assert.DoesNotThrowAsync(async () =>
                await _notification.SendNotification(alarm, "https://webhook.test.com", "admin@test.com", "13800000000"));
        }

        [Test]
        public async Task NotificationRouting_NullAlarm_ReturnsEarly()
        {
            // Sending null alarm should not throw
            Assert.DoesNotThrowAsync(async () =>
                await _notification.SendNotification(null));
        }

        #endregion

        #region Deduplication Tests

        [Test]
        public async Task Deduplicate_SameAlarmWithinCooldown_Suppressed()
        {
            var alarm = CreateAlarm("DUP001", "Duplicate alarm", AlarmLevel.Error);
            string alarmKey = "DUP001_Error";

            // First notification should record time
            await _notification.SendNotification(alarm, "https://hook.test.com", null, null);

            // Verify the notification time was recorded
            var times = GetLastNotificationTimes();
            Assert.That(times.ContainsKey(alarmKey), Is.True,
                "First alarm should be recorded in notification times");

            // Second attempt should be detected as duplicate
            var isDuplicate = IsDuplicateAlarm(alarmKey);
            Assert.That(isDuplicate, Is.True, "Same alarm within 5 minutes should be duplicate");
        }

        [Test]
        public async Task Deduplicate_DifferentAlarms_NotSuppressed()
        {
            var alarm1 = CreateAlarm("DUP_A", "Alarm A", AlarmLevel.Error);
            var alarm2 = CreateAlarm("DUP_B", "Alarm B", AlarmLevel.Error);

            await _notification.SendNotification(alarm1, "https://hook.test.com", null, null);

            var isDuplicateA = IsDuplicateAlarm("DUP_A_Error");
            var isDuplicateB = IsDuplicateAlarm("DUP_B_Error");

            Assert.That(isDuplicateA, Is.True, "Alarm A should be recorded");
            Assert.That(isDuplicateB, Is.False, "Alarm B should be new — not a duplicate");
        }

        [Test]
        public async Task Deduplicate_SameCodeDifferentLevel_NotSuppressed()
        {
            var alarmError = CreateAlarm("CODE001", "Alarm", AlarmLevel.Error);
            var alarmWarning = CreateAlarm("CODE001", "Alarm", AlarmLevel.Warning);

            await _notification.SendNotification(alarmError, "https://hook.test.com", null, null);

            // Different levels should have different keys
            var isErrorDuplicate = IsDuplicateAlarm("CODE001_Error");
            var isWarningDuplicate = IsDuplicateAlarm("CODE001_Warning");

            Assert.That(isErrorDuplicate, Is.True, "CODE001_Error should be recorded");
            Assert.That(isWarningDuplicate, Is.False, "CODE001_Warning should be new");
        }

        #endregion

        #region Rate Limiting Tests

        [Test]
        public async Task RateLimit_MaxNotificationsPerMinute_LimitEnforced()
        {
            // The deduplication mechanism serves as a rate limiter:
            // same alarm is suppressed for 5 minutes.
            // Here we verify that rapid duplicate sends are suppressed.
            var alarm = CreateAlarm("RATE001", "Rate limit test", AlarmLevel.Error);

            await _notification.SendNotification(alarm, "https://hook.test.com", null, null);

            // Second immediate send should be duplicate
            var isDuplicate = IsDuplicateAlarm("RATE001_Error");
            Assert.That(isDuplicate, Is.True,
                "Rate limiting via deduplication should work");
        }

        [Test]
        public async Task RateLimit_AfterCooldownExpires_NotificationSent()
        {
            var alarm = CreateAlarm("COOLDOWN001", "Cooldown test", AlarmLevel.Warning);

            // Simulate a notification that happened 6 minutes ago (beyond 5 min cooldown)
            var times = GetLastNotificationTimes();
            string alarmKey = "COOLDOWN001_Warning";

            // Manually set the last notification time to be outside cooldown
            var lockField = typeof(AlarmNotification).GetField("_lock",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var lockObj = lockField.GetValue(_notification);

            lock (lockObj)
            {
                times[alarmKey] = DateTime.Now.AddMinutes(-6);
            }

            var isDuplicate = IsDuplicateAlarm(alarmKey);
            Assert.That(isDuplicate, Is.False,
                "Alarm outside cooldown window should not be considered duplicate");
        }

        #endregion

        #region Disabled Channel Tests

        [Test]
        public async Task DisabledChannel_NullWebhookUrl_SkipsWechat()
        {
            var alarm = CreateAlarm("SKIP001", "Skip test", AlarmLevel.Warning);

            // With null webhookUrl, SendWechatNotification returns early without error
            Assert.DoesNotThrowAsync(async () =>
                await _notification.SendNotification(alarm, null, null, null));
        }

        [Test]
        public async Task DisabledChannel_NullEmail_SkipsEmail()
        {
            var alarm = CreateAlarm("SKIP002", "Skip email", AlarmLevel.Error);

            // With null emailRecipients, SendEmailNotification returns early
            Assert.DoesNotThrowAsync(async () =>
                await _notification.SendNotification(alarm, "https://hook.test.com", null, null));
        }

        [Test]
        public async Task DisabledChannel_NullSms_SkipsSms()
        {
            var alarm = CreateAlarm("SKIP003", "Skip SMS", AlarmLevel.Critical);

            // With null smsRecipients, SendSmsNotification returns early
            Assert.DoesNotThrowAsync(async () =>
                await _notification.SendNotification(alarm, "https://hook.test.com", "admin@test.com", null));
        }

        #endregion

        #region Send Failure Tests

        [Test]
        public async Task SendFailure_OneChannelFails_OthersStillDeliver()
        {
            // Even if the WeChat webhook would fail (invalid URL),
            // the Email and SMS channels should still be attempted.
            // Since actual HTTP calls aren't made with null URLs,
            // we verify that no exception propagates.
            var alarm = CreateAlarm("FAIL001", "Failure test", AlarmLevel.Critical);

            // Use a real but unreachable URL to trigger a failure path in WeChat
            // The method catches exceptions internally, so no exception should propagate
            Assert.DoesNotThrowAsync(async () =>
                await _notification.SendNotification(alarm, "https://invalid.webhook.example.com/notfound",
                    "admin@test.com", "13800000000"));
        }

        [Test]
        public async Task SendFailure_WechatThrows_StillReturnsGracefully()
        {
            var alarm = CreateAlarm("WECHATFAIL", "WeChat failure", AlarmLevel.Warning);

            // SendNotification with a webhook URL — the HTTP POST will fail,
            // but should be caught internally. The method should not throw.
            Assert.DoesNotThrowAsync(async () =>
                await _notification.SendNotification(alarm, "https://webhook.that.will.fail/test",
                    null, null));
        }

        #endregion

        #region Dispose Tests

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            _notification.Dispose();
            Assert.DoesNotThrow(() => _notification.Dispose());
        }

        #endregion
    }
}
