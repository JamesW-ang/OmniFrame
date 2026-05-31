using NUnit.Framework;
using Plc;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class ModbusTests
    {
        private Modbus _modbus;

        [SetUp]
        public void Setup()
        {
            _modbus = new Modbus();
        }

        [Test]
        public void BuildReadRequest_ReadCoils_ReturnsValidRequest()
        {
            byte[] request = _modbus.BuildReadRequest(1, ModbusFunction.ReadCoils, 0, 10);

            Assert.That(request, Is.Not.Null);
            Assert.That(request.Length, Is.EqualTo(12));
            Assert.That(request[6], Is.EqualTo(1));  // slaveId
            Assert.That(request[7], Is.EqualTo(0x01)); // function code
            Assert.That(request[8], Is.EqualTo(0));   // startAddr high
            Assert.That(request[9], Is.EqualTo(0));   // startAddr low
            Assert.That(request[10], Is.EqualTo(0));  // quantity high
            Assert.That(request[11], Is.EqualTo(10)); // quantity low
        }

        [Test]
        public void BuildReadRequest_IncrementsTransactionId()
        {
            byte[] req1 = _modbus.BuildReadRequest(1, ModbusFunction.ReadHoldingRegisters, 0, 5);
            byte[] req2 = _modbus.BuildReadRequest(1, ModbusFunction.ReadHoldingRegisters, 0, 5);

            ushort id1 = (ushort)((req1[0] << 8) | req1[1]);
            ushort id2 = (ushort)((req2[0] << 8) | req2[1]);

            Assert.That(id2, Is.EqualTo(id1 + 1));
        }

        [Test]
        public void BuildWriteCoilRequest_WriteTrue_SetsValue()
        {
            byte[] request = _modbus.BuildWriteCoilRequest(1, 0, true);

            Assert.That(request[7], Is.EqualTo(0x05)); // function code
            Assert.That(request[10], Is.EqualTo(0xFF)); // value high (ON)
            Assert.That(request[11], Is.EqualTo(0x00)); // value low
        }

        [Test]
        public void BuildWriteCoilRequest_WriteFalse_SetsValue()
        {
            byte[] request = _modbus.BuildWriteCoilRequest(1, 0, false);

            Assert.That(request[10], Is.EqualTo(0x00));
            Assert.That(request[11], Is.EqualTo(0x00));
        }

        [Test]
        public void BuildWriteRegisterRequest_WritesValue()
        {
            byte[] request = _modbus.BuildWriteRegisterRequest(1, 0, 0x1234);

            Assert.That(request[7], Is.EqualTo(0x06)); // function code
            Assert.That(request[10], Is.EqualTo(0x12)); // value high
            Assert.That(request[11], Is.EqualTo(0x34)); // value low
        }

        [Test]
        public void BuildWriteMultipleRegistersRequest_WritesValues()
        {
            ushort[] values = { 0x0001, 0x0002 };
            byte[] request = _modbus.BuildWriteMultipleRegistersRequest(1, 0, values);

            Assert.That(request[7], Is.EqualTo(0x10)); // function code
            Assert.That(request[10], Is.EqualTo(0));   // quantity high
            Assert.That(request[11], Is.EqualTo(2));   // quantity low
            Assert.That(request[12], Is.EqualTo(4));   // byte count
            Assert.That(request[13], Is.EqualTo(0));   // value[0] high
            Assert.That(request[14], Is.EqualTo(1));   // value[0] low
        }

        [Test]
        public void ValidateResponse_ValidResponse_ReturnsTrue()
        {
            byte[] request = _modbus.BuildReadRequest(1, ModbusFunction.ReadHoldingRegisters, 0, 3);
            byte[] response = { 0, 0, 0, 0, 0, 6, 1, 3, 6, 0x00, 0x01, 0x00, 0x02, 0x00, 0x03 };

            bool valid = _modbus.ValidateResponse(request, response);
            Assert.That(valid, Is.True);
        }

        [Test]
        public void ValidateResponse_WrongSlaveId_ReturnsFalse()
        {
            byte[] request = _modbus.BuildReadRequest(1, ModbusFunction.ReadHoldingRegisters, 0, 3);
            byte[] response = { 0, 0, 0, 0, 0, 6, 2, 3, 6, 0x00, 0x01, 0x00, 0x02, 0x00, 0x03 };

            bool valid = _modbus.ValidateResponse(request, response);
            Assert.That(valid, Is.False);
        }

        [Test]
        public void ValidateResponse_WrongFunctionCode_ReturnsFalse()
        {
            byte[] request = _modbus.BuildReadRequest(1, ModbusFunction.ReadHoldingRegisters, 0, 3);
            byte[] response = { 0, 0, 0, 0, 0, 6, 1, 4, 6, 0x00, 0x01, 0x00, 0x02, 0x00, 0x03 };

            bool valid = _modbus.ValidateResponse(request, response);
            Assert.That(valid, Is.False);
        }

        [Test]
        public void ValidateResponse_ExceptionResponse_ReturnsFalse()
        {
            byte[] request = _modbus.BuildReadRequest(1, ModbusFunction.ReadHoldingRegisters, 0, 3);
            byte[] response = { 0, 0, 0, 0, 0, 3, 1, 0x83, 0x02 };

            bool valid = _modbus.ValidateResponse(request, response);
            Assert.That(valid, Is.False);
        }

        [Test]
        public void ValidateResponse_NullResponse_ReturnsFalse()
        {
            bool valid = _modbus.ValidateResponse(new byte[12], null);
            Assert.That(valid, Is.False);
        }

        [Test]
        public void GetErrorMessage_KnownCode_ReturnsMessage()
        {
            string msg = _modbus.GetErrorMessage(0x02);
            Assert.That(msg, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GetErrorMessage_UnknownCode_ReturnsDefault()
        {
            string msg = _modbus.GetErrorMessage(0xFF);
            Assert.That(msg, Is.Not.Null.And.Not.Empty);
        }
    }
}
