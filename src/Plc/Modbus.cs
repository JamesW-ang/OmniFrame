using System;

namespace Plc
{
    public enum ModbusFunction : byte
    {
        ReadCoils = 0x01,
        ReadDiscreteInputs = 0x02,
        ReadHoldingRegisters = 0x03,
        ReadInputRegisters = 0x04,
        WriteSingleCoil = 0x05,
        WriteSingleRegister = 0x06,
        WriteMultipleCoils = 0x0F,
        WriteMultipleRegisters = 0x10
    }

    public class Modbus
    {
        private ushort _transactionId = 0;

        public byte[] BuildReadRequest(byte slaveId, ModbusFunction function, ushort startAddr, ushort quantity)
        {
            byte[] request = new byte[12];

            _transactionId++;
            request[0] = (byte)(_transactionId >> 8);
            request[1] = (byte)(_transactionId & 0xFF);
            request[2] = 0;
            request[3] = 0;
            request[4] = 0;
            request[5] = 6;
            request[6] = slaveId;
            request[7] = (byte)function;
            request[8] = (byte)(startAddr >> 8);
            request[9] = (byte)(startAddr & 0xFF);
            request[10] = (byte)(quantity >> 8);
            request[11] = (byte)(quantity & 0xFF);

            return request;
        }

        public byte[] BuildWriteCoilRequest(byte slaveId, ushort coilAddr, bool value)
        {
            byte[] request = new byte[12];

            _transactionId++;
            request[0] = (byte)(_transactionId >> 8);
            request[1] = (byte)(_transactionId & 0xFF);
            request[2] = 0;
            request[3] = 0;
            request[4] = 0;
            request[5] = 6;
            request[6] = slaveId;
            request[7] = (byte)ModbusFunction.WriteSingleCoil;
            request[8] = (byte)(coilAddr >> 8);
            request[9] = (byte)(coilAddr & 0xFF);
            request[10] = value ? (byte)0xFF : (byte)0x00;
            request[11] = 0;

            return request;
        }

        public byte[] BuildWriteRegisterRequest(byte slaveId, ushort registerAddr, ushort value)
        {
            byte[] request = new byte[12];

            _transactionId++;
            request[0] = (byte)(_transactionId >> 8);
            request[1] = (byte)(_transactionId & 0xFF);
            request[2] = 0;
            request[3] = 0;
            request[4] = 0;
            request[5] = 6;
            request[6] = slaveId;
            request[7] = (byte)ModbusFunction.WriteSingleRegister;
            request[8] = (byte)(registerAddr >> 8);
            request[9] = (byte)(registerAddr & 0xFF);
            request[10] = (byte)(value >> 8);
            request[11] = (byte)(value & 0xFF);

            return request;
        }

        public byte[] BuildWriteMultipleRegistersRequest(byte slaveId, ushort startAddr, ushort[] values)
        {
            int byteCount = values.Length * 2;
            byte[] request = new byte[13 + byteCount];

            _transactionId++;
            request[0] = (byte)(_transactionId >> 8);
            request[1] = (byte)(_transactionId & 0xFF);
            request[2] = 0;
            request[3] = 0;
            request[4] = (byte)((7 + byteCount) >> 8);
            request[5] = (byte)((7 + byteCount) & 0xFF);
            request[6] = slaveId;
            request[7] = (byte)ModbusFunction.WriteMultipleRegisters;
            request[8] = (byte)(startAddr >> 8);
            request[9] = (byte)(startAddr & 0xFF);
            request[10] = (byte)(values.Length >> 8);
            request[11] = (byte)(values.Length & 0xFF);
            request[12] = (byte)byteCount;

            for (int i = 0; i < values.Length; i++)
            {
                request[13 + i * 2] = (byte)(values[i] >> 8);
                request[14 + i * 2] = (byte)(values[i] & 0xFF);
            }

            return request;
        }

        public bool ValidateResponse(byte[] request, byte[] response)
        {
            if (response == null || response.Length < 5)
                return false;

            if (response[0] != request[0] || response[1] != request[1])
                return false;

            if ((response[7] & 0x80) != 0)
                return false;

            return true;
        }

        public string GetErrorMessage(byte exceptionCode)
        {
            switch (exceptionCode)
            {
                case 0x01: return "非法功能";
                case 0x02: return "非法数据地址";
                case 0x03: return "非法数据值";
                case 0x04: return "从站设备故障";
                case 0x05: return "确认";
                case 0x06: return "从站设备忙";
                case 0x08: return "存储奇偶性差错";
                case 0x0A: return "不可用网关路径";
                case 0x0B: return "网关目标设备响应失败";
                default: return $"未知错误码: 0x{exceptionCode:X2}";
            }
        }
    }
}
