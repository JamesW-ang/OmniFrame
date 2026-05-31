using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using OmniFrame.Communication;
using OmniFrame.Common;

namespace Plc
{
    /// <summary>
    /// Modbus TCP PLC 驱动。
    /// 基于 PlcDevice 模板方法模式，实现 Modbus TCP 协议的位/字/双字/浮点数读写。
    /// </summary>
    public class Plc_ModbusTcp : PlcDevice
    {
        private TcpLink _tcpLink;
        private Modbus _modbus;

        /// <summary>Modbus 从站地址（默认 1）</summary>
        public byte SlaveId { get; set; } = 1;

        /// <summary>读取超时（毫秒），默认 200ms</summary>
        public int ReadTimeoutMs { get; set; } = 200;

        /// <summary>单次请求失败后的重试次数（默认 0 = 不重试）</summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>重试间隔（毫秒）</summary>
        public int RetryDelayMs { get; set; } = 20;

        public Plc_ModbusTcp(int index, string name, TcpLink tcpLink)
            : base(index, name)
        {
            _tcpLink = tcpLink;
            _modbus = new Modbus();
        }

        public override bool Open()
        {
            try
            {
                if (IsOpen)
                    return true;

                if (_tcpLink == null)
                {
                    LogError("TCP连接未配置");
                    return false;
                }

                if (!_tcpLink.IsOpen())
                {
                    if (!_tcpLink.Open())
                    {
                        LogError("TCP连接失败");
                        return false;
                    }
                }

                IsOpen = true;
                LogInfo("ModbusTCP连接成功");
                return true;
            }
            catch (SocketException ex)
            {
                LogError("PLC TCP操作失败(网络错误)", ex);
                return false;
            }
            catch (IOException ex)
            {
                LogError("PLC TCP操作失败(IO错误)", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                LogError("PLC TCP操作失败(连接已断开)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                LogError("PLC TCP操作失败(超时)", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError("打开连接失败", ex);
                return false;
            }
        }

        public override void Close()
        {
            try
            {
                IsOpen = false;
                _tcpLink?.Close();
                LogInfo("ModbusTCP连接已关闭");
            }
            catch (SocketException ex)
            {
                LogError("PLC TCP操作失败(网络错误)", ex);
            }
            catch (IOException ex)
            {
                LogError("PLC TCP操作失败(IO错误)", ex);
            }
            catch (ObjectDisposedException ex)
            {
                LogError("PLC TCP操作失败(连接已断开)", ex);
            }
            catch (TimeoutException ex)
            {
                LogError("PLC TCP操作失败(超时)", ex);
            }
            catch (Exception ex)
            {
                LogError("关闭连接失败", ex);
            }
        }

        protected override bool InternalReadBit(SoftElement element, int addr, ref bool bVal)
        {
            if (!IsOpen) return false;

            try
            {
                ushort modbusAddr = GetModbusAddress(element, addr);
                byte[] request = _modbus.BuildReadRequest(SlaveId, ModbusFunction.ReadCoils, modbusAddr, 1);

                if (!SendAndReceive(request, out byte[] response, expectedPduLen: 4))
                    return false;

                // response: [unitId][func][byteCount(0x01)][coilData]
                bVal = (response[3] & 0x01) != 0;
                return true;
            }
            catch (SocketException ex) { LogError("PLC TCP操作失败(网络错误)", ex); return false; }
            catch (IOException ex)      { LogError("PLC TCP操作失败(IO错误)", ex); return false; }
            catch (ObjectDisposedException ex) { LogError("PLC TCP操作失败(连接已断开)", ex); return false; }
            catch (TimeoutException ex) { LogError("PLC TCP操作失败(超时)", ex); return false; }
            catch (Exception ex)        { LogError($"读取位失败: {element}{addr}", ex); return false; }
        }

        protected override bool InternalReadWord(SoftElement element, int addr, ref ushort nVal)
        {
            if (!IsOpen) return false;

            try
            {
                ushort modbusAddr = GetModbusAddress(element, addr);
                byte[] request = _modbus.BuildReadRequest(SlaveId, ModbusFunction.ReadHoldingRegisters, modbusAddr, 1);

                if (!SendAndReceive(request, out byte[] response, expectedPduLen: 5))
                    return false;

                // response: [unitId][func][byteCount(0x02)][regHi][regLo]
                nVal = (ushort)((response[3] << 8) | response[4]);
                return true;
            }
            catch (SocketException ex) { LogError("PLC TCP操作失败(网络错误)", ex); return false; }
            catch (IOException ex)      { LogError("PLC TCP操作失败(IO错误)", ex); return false; }
            catch (ObjectDisposedException ex) { LogError("PLC TCP操作失败(连接已断开)", ex); return false; }
            catch (TimeoutException ex) { LogError("PLC TCP操作失败(超时)", ex); return false; }
            catch (Exception ex)        { LogError($"读取字失败: {element}{addr}", ex); return false; }
        }

        protected override bool InternalReadDWord(SoftElement element, int addr, ref uint nVal)
        {
            if (!IsOpen) return false;

            try
            {
                ushort modbusAddr = GetModbusAddress(element, addr);
                byte[] request = _modbus.BuildReadRequest(SlaveId, ModbusFunction.ReadHoldingRegisters, modbusAddr, 2);

                if (!SendAndReceive(request, out byte[] response, expectedPduLen: 7))
                    return false;

                // response: [unitId][func][byteCount(0x04)][reg1Hi][reg1Lo][reg2Hi][reg2Lo]
                nVal = (uint)((response[3] << 24) | (response[4] << 16) | (response[5] << 8) | response[6]);
                return true;
            }
            catch (SocketException ex) { LogError("PLC TCP操作失败(网络错误)", ex); return false; }
            catch (IOException ex)      { LogError("PLC TCP操作失败(IO错误)", ex); return false; }
            catch (ObjectDisposedException ex) { LogError("PLC TCP操作失败(连接已断开)", ex); return false; }
            catch (TimeoutException ex) { LogError("PLC TCP操作失败(超时)", ex); return false; }
            catch (Exception ex)        { LogError($"读取双字失败: {element}{addr}", ex); return false; }
        }

        protected override bool InternalReadFloat(SoftElement element, int addr, ref float fVal)
        {
            if (!IsOpen) return false;

            try
            {
                ushort modbusAddr = GetModbusAddress(element, addr);
                byte[] request = _modbus.BuildReadRequest(SlaveId, ModbusFunction.ReadHoldingRegisters, modbusAddr, 2);

                if (!SendAndReceive(request, out byte[] response, expectedPduLen: 7))
                    return false;

                // response: [unitId][func][byteCount(0x04)][bytes...]
                // Modbus 浮点数为大端序，BitConverter 需要对应布局
                byte[] floatBytes = new byte[4];
                floatBytes[0] = response[5];  // 低字高位
                floatBytes[1] = response[6];  // 低字低位
                floatBytes[2] = response[3];  // 高字高位
                floatBytes[3] = response[4];  // 高字低位
                fVal = BitConverter.ToSingle(floatBytes, 0);
                return true;
            }
            catch (SocketException ex) { LogError("PLC TCP操作失败(网络错误)", ex); return false; }
            catch (IOException ex)      { LogError("PLC TCP操作失败(IO错误)", ex); return false; }
            catch (ObjectDisposedException ex) { LogError("PLC TCP操作失败(连接已断开)", ex); return false; }
            catch (TimeoutException ex) { LogError("PLC TCP操作失败(超时)", ex); return false; }
            catch (Exception ex)        { LogError($"读取浮点数失败: {element}{addr}", ex); return false; }
        }

        protected override bool InternalWriteBit(SoftElement element, int addr, bool bVal)
        {
            if (!IsOpen) return false;

            try
            {
                ushort modbusAddr = GetModbusAddress(element, addr);
                byte[] request = _modbus.BuildWriteCoilRequest(SlaveId, modbusAddr, bVal);

                if (!SendAndReceive(request, out byte[] response, expectedPduLen: 6))
                    return false;

                return true;
            }
            catch (SocketException ex) { LogError("PLC TCP操作失败(网络错误)", ex); return false; }
            catch (IOException ex)      { LogError("PLC TCP操作失败(IO错误)", ex); return false; }
            catch (ObjectDisposedException ex) { LogError("PLC TCP操作失败(连接已断开)", ex); return false; }
            catch (TimeoutException ex) { LogError("PLC TCP操作失败(超时)", ex); return false; }
            catch (Exception ex)        { LogError($"写入位失败: {element}{addr}", ex); return false; }
        }

        protected override bool InternalWriteWord(SoftElement element, int addr, ushort nVal)
        {
            if (!IsOpen) return false;

            try
            {
                ushort modbusAddr = GetModbusAddress(element, addr);
                byte[] request = _modbus.BuildWriteRegisterRequest(SlaveId, modbusAddr, nVal);

                if (!SendAndReceive(request, out byte[] response, expectedPduLen: 6))
                    return false;

                return true;
            }
            catch (SocketException ex) { LogError("PLC TCP操作失败(网络错误)", ex); return false; }
            catch (IOException ex)      { LogError("PLC TCP操作失败(IO错误)", ex); return false; }
            catch (ObjectDisposedException ex) { LogError("PLC TCP操作失败(连接已断开)", ex); return false; }
            catch (TimeoutException ex) { LogError("PLC TCP操作失败(超时)", ex); return false; }
            catch (Exception ex)        { LogError($"写入字失败: {element}{addr}", ex); return false; }
        }

        protected override bool InternalWriteDWord(SoftElement element, int addr, uint nVal)
        {
            if (!IsOpen) return false;

            try
            {
                ushort[] values = new ushort[2];
                values[0] = (ushort)(nVal >> 16);
                values[1] = (ushort)(nVal & 0xFFFF);
                return WriteWords(element, addr, 2, values);
            }
            catch (SocketException ex) { LogError("PLC TCP操作失败(网络错误)", ex); return false; }
            catch (IOException ex)      { LogError("PLC TCP操作失败(IO错误)", ex); return false; }
            catch (ObjectDisposedException ex) { LogError("PLC TCP操作失败(连接已断开)", ex); return false; }
            catch (TimeoutException ex) { LogError("PLC TCP操作失败(超时)", ex); return false; }
            catch (Exception ex)        { LogError($"写入双字失败: {element}{addr}", ex); return false; }
        }

        protected override bool InternalWriteFloat(SoftElement element, int addr, float fVal)
        {
            if (!IsOpen) return false;

            try
            {
                byte[] bytes = BitConverter.GetBytes(fVal);
                ushort[] values = new ushort[2];
                values[0] = (ushort)((bytes[2] << 8) | bytes[3]);
                values[1] = (ushort)((bytes[0] << 8) | bytes[1]);
                return WriteWords(element, addr, 2, values);
            }
            catch (SocketException ex) { LogError("PLC TCP操作失败(网络错误)", ex); return false; }
            catch (IOException ex)      { LogError("PLC TCP操作失败(IO错误)", ex); return false; }
            catch (ObjectDisposedException ex) { LogError("PLC TCP操作失败(连接已断开)", ex); return false; }
            catch (TimeoutException ex) { LogError("PLC TCP操作失败(超时)", ex); return false; }
            catch (Exception ex)        { LogError($"写入浮点数失败: {element}{addr}", ex); return false; }
        }

        private ushort GetModbusAddress(SoftElement element, int addr)
        {
            switch (element)
            {
                case SoftElement.X:
                    return (ushort)(addr + 0);
                case SoftElement.Y:
                    return (ushort)(addr + 8192);
                case SoftElement.M:
                    return (ushort)(addr + 2048);
                case SoftElement.D:
                    return (ushort)addr;
                default:
                    return (ushort)addr;
            }
        }

        /// <summary>
        /// 发送 Modbus TCP 请求并接收响应。
        /// 剥离 6 字节 MBAP 头（事务ID + 协议ID + 长度），返回 PDU 部分。
        /// 包含响应验证、超时轮询和可选重试。
        /// </summary>
        /// <param name="request">完整的 Modbus TCP 请求帧（12+ 字节）</param>
        /// <param name="response">剥离 MBAP 头后的 PDU 响应</param>
        /// <param name="expectedPduLen">期望的 PDU 最小长度（unitId + func + data）</param>
        private bool SendAndReceive(byte[] request, out byte[] response, int expectedPduLen)
        {
            response = null;

            if (_tcpLink == null || !_tcpLink.IsOpen())
                return false;

            int attempts = RetryCount + 1;
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                if (attempt > 0)
                {
                    LogWarning($"Modbus TCP 重试 ({attempt}/{RetryCount})");
                    Thread.Sleep(RetryDelayMs);
                }

                // ① 发送请求
                if (!_tcpLink.Write(request))
                    continue;

                // ② 轮询等待响应（替代原 Thread.Sleep(10)）
                byte[] buffer = new byte[256];
                int readLen = 0;
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < ReadTimeoutMs)
                {
                    readLen = _tcpLink.Read(buffer, 0, buffer.Length);
                    if (readLen > 0)
                        break;
                    Thread.Sleep(1);
                }

                if (readLen == 0)
                {
                    LogWarning($"Modbus TCP 读取超时 ({ReadTimeoutMs}ms)");
                    continue;
                }

                // ③ 剥离 6 字节 MBAP 头（事务ID[2] + 协议ID[2] + 长度[2]）
                // 之后为 PDU：[unitId][func][data...]
                const int mbapHeaderLen = 6;
                int pduLen = readLen - mbapHeaderLen;
                if (pduLen < expectedPduLen)
                {
                    LogWarning($"Modbus TCP PDU 长度不足: 期望>={expectedPduLen}, 实际={pduLen}");
                    continue;
                }

                response = new byte[pduLen];
                Array.Copy(buffer, mbapHeaderLen, response, 0, pduLen);

                // ④ 验证响应
                if (!ValidateStrippedResponse(request, response))
                {
                    LogWarning("Modbus TCP 响应验证失败");
                    response = null;
                    continue;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 验证剥离 MBAP 头后的 Modbus TCP PDU 响应。
        /// response 格式：[unitId][func][data...]
        /// </summary>
        private bool ValidateStrippedResponse(byte[] request, byte[] response)
        {
            if (response == null || response.Length < 4)
                return false;

            // 检查从站地址是否匹配
            byte expectedSlaveId = request[6];  // 请求中的 slaveId
            if (response[0] != expectedSlaveId)
            {
                LogWarning($"Modbus TCP 从站地址不匹配: 期望={expectedSlaveId}, 实际={response[0]}");
                return false;
            }

            // 检查是否为异常响应（功能码 bit7=1）
            byte funcCode = response[1];
            if ((funcCode & 0x80) != 0)
            {
                byte exceptionCode = response.Length > 2 ? response[2] : (byte)0;
                LogError($"Modbus TCP 异常响应: 功能码=0x{funcCode:X2}, 异常码=0x{exceptionCode:X2} ({_modbus.GetErrorMessage(exceptionCode)})");
                return false;
            }

            return true;
        }
    }
}
