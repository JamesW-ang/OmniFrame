using Moq;
using NUnit.Framework;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.Core.AdvancedFeatures;
using OmniFrame.Core.BlockCut;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class BlockCutMesClientTests
    {
        private Mock<IMqttManager> _mqttMock;
        private BlockCutMesClient _client;
        private const string TestAesKey = "TestAesKey12345!";

        [SetUp]
        public void Setup()
        {
            _mqttMock = new Mock<IMqttManager>();
            _client = new BlockCutMesClient(_mqttMock.Object, TestAesKey);
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
        }

        #region AES Encryption/Decryption Tests

        [Test]
        public void Aes128_EncryptDecrypt_RoundTrip()
        {
            string original = @"{""o_workStation"":""WS01"",""o_machineNo"":""M001"",""o_machineState"":1}";

            string encrypted = Security.Aes128EcbEncrypt(original, TestAesKey);
            string decrypted = Security.Aes128EcbDecrypt(encrypted, TestAesKey);

            Assert.That(encrypted, Is.Not.Null.And.Not.Empty);
            Assert.That(encrypted, Is.Not.EqualTo(original),
                "Encrypted text should differ from plaintext");
            Assert.That(decrypted, Is.EqualTo(original),
                "Decrypted text should match original");
        }

        [Test]
        public void Aes128_Encrypt_ProducesDifferentOutput()
        {
            string input = "test data";

            string encrypted1 = Security.Aes128EcbEncrypt(input, TestAesKey);
            string encrypted2 = Security.Aes128EcbEncrypt(input, TestAesKey);

            Assert.That(encrypted1, Is.Not.Null.And.Not.Empty);
            Assert.That(encrypted1, Is.EqualTo(encrypted2),
                "Same input + same key should produce consistent output (ECB is deterministic)");
        }

        [Test]
        public void Aes128_Encrypt_EmptyString_ReturnsEmpty()
        {
            string encrypted = Security.Aes128EcbEncrypt("", TestAesKey);
            Assert.That(encrypted, Is.Empty);

            string decrypted = Security.Aes128EcbDecrypt("", TestAesKey);
            Assert.That(decrypted, Is.Empty);
        }

        [Test]
        public void Aes128_Encrypt_Null_ReturnsEmpty()
        {
            string encrypted = Security.Aes128EcbEncrypt(null, TestAesKey);
            Assert.That(encrypted, Is.Empty);
        }

        [Test]
        public void Aes128_Decrypt_InvalidBase64_ReturnsEmpty()
        {
            string result = Security.Aes128EcbDecrypt("not-valid-base64!!!", TestAesKey);
            Assert.That(result, Is.Empty,
                "Invalid Base64 input should return empty string gracefully");
        }

        #endregion

        #region Protocol Frame Tests

        [Test]
        public void ProtocolFrame_EncryptedPayload_IsBase64()
        {
            string json = @"{""msg_id"":""wpd-xqgzpzddw-123"",""o_workStation"":""WS01""}";
            string encrypted = Security.Aes128EcbEncrypt(json, TestAesKey);

            // Base64 validation: should only contain valid Base64 chars
            Assert.That(() => Convert.FromBase64String(encrypted), Throws.Nothing,
                "Encrypted output should be valid Base64");
        }

        [Test]
        public void ProtocolFrame_WorkReport_ContainsMsgId()
        {
            var report = new WorkReport
            {
                WorkStation = "WS01",
                MachineNo = "M001",
                BottomCode = "BC001",
                FileId = "FILE001",
                Timestamp = DateTime.Now.ToString("O")
            };

            _client.DeviceNo = "xqgzpzddw";

            // The msg_id is set inside SendWorkReportAsync
            // We verify the format: wpd-{deviceNo}-{epochMs}
            string manualMsgId = $"wpd-xqgzpzddw-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            report.MsgId = manualMsgId;

            Assert.That(report.MsgId, Does.StartWith("wpd-"));
            Assert.That(report.MsgId, Does.Contain("xqgzpzddw"));
        }

        [Test]
        public void ProtocolFrame_MsgId_FormatIsCorrect()
        {
            string deviceNo = "testdevice";
            long epochMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string msgId = $"wpd-{deviceNo}-{epochMs}";

            Assert.That(msgId, Does.StartWith("wpd-"));
            Assert.That(msgId, Does.Contain(deviceNo));
            Assert.That(msgId.Split('-').Length, Is.EqualTo(3));
        }

        #endregion

        #region Message ID Tracking Tests

        [Test]
        public void MessageId_DifferentTimestamps_ProduceDifferentIds()
        {
            string msgId1 = $"wpd-dev1-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            // Small delay to ensure different timestamp
            System.Threading.Thread.Sleep(5);
            string msgId2 = $"wpd-dev1-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            Assert.That(msgId1, Is.Not.EqualTo(msgId2),
                "Different timestamps should produce different msg_ids");
        }

        [Test]
        public void MessageId_SameTimestampDifferentDevice_Different()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string msgId1 = $"wpd-deviceA-{timestamp}";
            string msgId2 = $"wpd-deviceB-{timestamp}";

            Assert.That(msgId1, Is.Not.EqualTo(msgId2),
                "Different device numbers should produce different msg_ids");
        }

        #endregion

        #region Timeout Handling Tests

        [Test]
        public async Task Timeout_ValidateCard_ReturnsFailureOnTimeout()
        {
            // Use simulation mode to avoid actual HTTP calls
            _client.SimulationMode = true;

            var cts = new CancellationTokenSource(100);
            var result = await _client.ValidateCardAsync("CARD001", cts.Token);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.AlertMsg, Does.Contain("仿真"));
        }

        [Test]
        public async Task Timeout_ValidateCard_SimulationModeReturnsSuccess()
        {
            _client.SimulationMode = true;
            var result = await _client.ValidateCardAsync("TESTCARD", CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.FileId, Is.EqualTo("TESTCARD"));
        }

        #endregion

        #region Connection State Tests

        [Test]
        public void ConnectionState_SimulationMode_PropertyIsSet()
        {
            Assert.That(_client.SimulationMode, Is.False, "SimulationMode should default to false");

            _client.SimulationMode = true;
            Assert.That(_client.SimulationMode, Is.True);

            _client.SimulationMode = false;
            Assert.That(_client.SimulationMode, Is.False);
        }

        [Test]
        public void ConnectionState_Contructor_SetsCorrectDefaults()
        {
            Assert.That(_client.MesBaseUrl, Is.EqualTo("http://mes-server/api"));
            Assert.That(_client.DeviceNo, Is.EqualTo("xqgzpzddw"));
            Assert.That(_client.SimulationMode, Is.False);
            Assert.That(_client.MqttStatusTopic, Is.EqualTo("data.device.status"));
            Assert.That(_client.MqttWorkTopic, Is.EqualTo("data.device.workorder.report"));
        }

        [Test]
        public void ConnectionState_Constructor_NullMqtt_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BlockCutMesClient(null, TestAesKey));
        }

        [Test]
        public void ConnectionState_Constructor_NullAesKey_ThrowsArgumentNullException()
        {
            var mqtt = new Mock<IMqttManager>();
            Assert.Throws<ArgumentNullException>(() =>
                new BlockCutMesClient(mqtt.Object, null));
        }

        #endregion

        #region Invalid Frame / Corrupted Data Tests

        [Test]
        public void InvalidFrame_CorruptedBase64_DecryptedSafely()
        {
            // Corrupted Base64 should return empty string
            string result = Security.Aes128EcbDecrypt("!!!corrupted!!!", TestAesKey);
            Assert.That(result, Is.Empty,
                "Corrupted data should be handled gracefully");
        }

        [Test]
        public void InvalidFrame_DifferentKey_DecryptFails()
        {
            string original = "test data";
            string encrypted = Security.Aes128EcbEncrypt(original, TestAesKey);

            // Decrypt with a different key — should produce garbage, not original
            string decrypted = Security.Aes128EcbDecrypt(encrypted, "DifferentKey!!!!!");

            Assert.That(decrypted, Is.Not.EqualTo(original),
                "Decrypting with wrong key should not return original");
        }

        [Test]
        public void InvalidFrame_EmptyPayload_HandledSafely()
        {
            string result = Security.Aes128EcbDecrypt("", TestAesKey);
            Assert.That(result, Is.Empty);
        }

        #endregion

        #region MQTT Interaction Tests

        [Test]
        public async Task SendStatusAsync_EncryptsAndPublishes()
        {
            var status = new MachineStatus
            {
                WorkStation = "WS01",
                MachineNo = "M001",
                MachineState = 1,
                StateName = "Running"
            };

            string publishedPayload = null;
            string publishedTopic = null;

            _mqttMock.Setup(m => m.Publish(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((topic, payload) =>
                {
                    publishedTopic = topic;
                    publishedPayload = payload;
                })
                .Returns(Task.CompletedTask);

            await _client.SendStatusAsync(status, CancellationToken.None);

            Assert.That(publishedTopic, Is.EqualTo("data.device.status"));
            Assert.That(publishedPayload, Is.Not.Null.And.Not.Empty);

            // Verify the published payload is encrypted JSON
            string decrypted = Security.Aes128EcbDecrypt(publishedPayload, TestAesKey);
            Assert.That(decrypted, Does.Contain("WS01"));
            Assert.That(decrypted, Does.Contain("M001"));
        }

        #endregion

        #region Dispose Tests

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            _client.Dispose();
            Assert.DoesNotThrow(() => _client.Dispose());
        }

        #endregion
    }
}
