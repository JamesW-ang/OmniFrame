using NUnit.Framework;
using Moq;
using Moq.Protected;
using Plc;
using System.Reflection;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class PlcDeviceTests
    {
        [Test]
        public void Connect_CallsOpen_ReturnsTrue()
        {
            var mock = new Mock<PlcDevice>(MockBehavior.Loose, 1, "TestDevice") { CallBase = true };
            mock.Protected().Setup<bool>("Open").Returns(true);

            bool result = mock.Object.Connect();

            Assert.That(result, Is.True);
            mock.Protected().Verify("Open", Times.Once());
        }

        [Test]
        public void Connect_CallsOpen_ReturnsFalse()
        {
            var mock = new Mock<PlcDevice>(MockBehavior.Loose, 1, "TestDevice") { CallBase = true };
            mock.Protected().Setup<bool>("Open").Returns(false);

            bool result = mock.Object.Connect();

            Assert.That(result, Is.False);
        }

        [Test]
        public void Disconnect_CallsClose()
        {
            var mock = new Mock<PlcDevice>(MockBehavior.Loose, 1, "TestDevice") { CallBase = true };

            mock.Object.Disconnect();

            mock.Protected().Verify("Close", Times.Once());
        }

        [Test]
        public void IsConnected_WhenIsOpenTrue_ReturnsTrue()
        {
            var mock = new Mock<PlcDevice>(MockBehavior.Loose, 1, "TestDevice") { CallBase = true };

            var isOpenProp = typeof(PlcDevice).GetProperty("IsOpen",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            isOpenProp.SetValue(mock.Object, true);

            Assert.That(mock.Object.IsConnected, Is.True);
        }

        [Test]
        public void IsConnected_ByDefault_ReturnsFalse()
        {
            var mock = new Mock<PlcDevice>(MockBehavior.Loose, 1, "TestDevice") { CallBase = true };
            Assert.That(mock.Object.IsConnected, Is.False);
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            var mock = new Mock<PlcDevice>(MockBehavior.Loose, 2, "PLC-01") { CallBase = true };

            Assert.That(mock.Object.Index, Is.EqualTo(2));
            Assert.That(mock.Object.Name, Is.EqualTo("PLC-01"));
            Assert.That(mock.Object.IsOpen, Is.False);
        }

        [Test]
        public void WriteBit_DelegatesToInternal()
        {
            var mock = new Mock<PlcDevice>(MockBehavior.Loose, 1, "Test") { CallBase = true };
            mock.Protected().Setup<bool>("InternalWriteBit", ItExpr.IsAny<SoftElement>(), ItExpr.IsAny<int>(), ItExpr.IsAny<bool>()).Returns(true);

            bool result = mock.Object.WriteBit(SoftElement.Y, 0, true);

            Assert.That(result, Is.True);
        }

        [Test]
        public void WriteWord_DelegatesToInternal()
        {
            var mock = new Mock<PlcDevice>(MockBehavior.Loose, 1, "Test") { CallBase = true };
            mock.Protected().Setup<bool>("InternalWriteWord", ItExpr.IsAny<SoftElement>(), ItExpr.IsAny<int>(), ItExpr.IsAny<ushort>()).Returns(true);

            bool result = mock.Object.WriteWord(SoftElement.D, 0, (ushort)1234);

            Assert.That(result, Is.True);
        }

        [Test]
        public void WriteDWord_DelegatesToInternal()
        {
            var mock = new Mock<PlcDevice>(MockBehavior.Loose, 1, "Test") { CallBase = true };
            mock.Protected().Setup<bool>("InternalWriteDWord", ItExpr.IsAny<SoftElement>(), ItExpr.IsAny<int>(), ItExpr.IsAny<uint>()).Returns(true);

            bool result = mock.Object.WriteDWord(SoftElement.D, 0, 12345u);

            Assert.That(result, Is.True);
        }

        [Test]
        public void WriteFloat_DelegatesToInternal()
        {
            var mock = new Mock<PlcDevice>(MockBehavior.Loose, 1, "Test") { CallBase = true };
            mock.Protected().Setup<bool>("InternalWriteFloat", ItExpr.IsAny<SoftElement>(), ItExpr.IsAny<int>(), ItExpr.IsAny<float>()).Returns(true);

            bool result = mock.Object.WriteFloat(SoftElement.D, 0, 3.14f);

            Assert.That(result, Is.True);
        }
    }
}
