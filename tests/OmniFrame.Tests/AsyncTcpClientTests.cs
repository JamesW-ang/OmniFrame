using NUnit.Framework;
using OmniFrame.Communication;
using System;
using System.Threading.Tasks;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class AsyncTcpClientTests
    {
        [Test]
        public void Constructor_DefaultValues()
        {
            using (var client = new AsyncTcpClient())
            {
                Assert.That(client.IsConnected, Is.False);
            }
        }

        [Test]
        public void Connect_ToNonExistentServer_Throws()
        {
            using (var client = new AsyncTcpClient())
            {
                Assert.ThrowsAsync<AggregateException>(async () =>
                    await client.ConnectAsync("192.0.2.1", 9));
            }
        }

        [Test]
        public void IsConnected_Initially_False()
        {
            using (var client = new AsyncTcpClient())
            {
                Assert.That(client.IsConnected, Is.False);
            }
        }

        [Test]
        public void Dispose_ClosesConnection()
        {
            var client = new AsyncTcpClient();
            Assert.DoesNotThrow(() => client.Dispose());
        }

        [Test]
        public void Dispose_MultipleCalls_DoesNotThrow()
        {
            var client = new AsyncTcpClient();
            client.Dispose();
            Assert.DoesNotThrow(() => client.Dispose());
        }

        [Test]
        public void Disconnect_WhenNotConnected_DoesNotThrow()
        {
            using (var client = new AsyncTcpClient())
            {
                Assert.DoesNotThrow(() => client.Disconnect());
            }
        }

        [Test]
        public void SendDataAsync_WhenDisconnected_Throws()
        {
            using (var client = new AsyncTcpClient())
            {
                byte[] data = new byte[] { 0x01, 0x02 };
                Assert.ThrowsAsync<InvalidOperationException>(async () => await client.SendDataAsync(data));
            }
        }

        [Test]
        public void SendDataAsync_NullData_Throws()
        {
            using (var client = new AsyncTcpClient())
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SendDataAsync(null));
            }
        }

        [Test]
        public async Task ReconnectAsync_WhenNotConnected_ReturnsFalse()
        {
            using (var client = new AsyncTcpClient())
            {
                bool result = await client.ReconnectAsync();
                Assert.That(result, Is.False);
            }
        }

        [Test]
        public void ConnectAsync_InvalidHost_Throws()
        {
            using (var client = new AsyncTcpClient())
            {
                Assert.ThrowsAsync<AggregateException>(async () =>
                    await client.ConnectAsync("invalid_host_name_xyz", 12345));
            }
        }
    }
}
