using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using OmniFrame.Common;
using OmniFrame.Communication;

namespace Plc
{
    /// <summary>
    /// Modbus RTU PLC驱动
    /// 设计介绍：
    /// 1. 继承自PlcDevice抽象基类，实现Modbus RTU协议的PLC通信功能
    /// 3. 实现Modbus RTU协议的基本功能，包括线圈和保持寄存器的读写
    /// 4. 支持CRC校验，确保通信数据的可靠性
    /// 5. 提供完善的错误处理和日志记录
    /// 6. 支持设备的初始化、连接和断开操作
    /// </summary>
    public class Plc_ModbusRtu : PlcDevice
    {
        private ComLink _comLink;
        private int _slaveId;

        private string _port;
        private int _baudRate;
        private int _dataBits;
        private string _parity;
        private int _stopBits;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Plc_ModbusRtu() : base(0, "ModbusRTU")
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="param">初始化参数</param>
        /// <returns>是否成功</returns>
        public bool Init(object param)
        {
            try
            {
                // 解析参数
                Dictionary<string, object> parameters = param as Dictionary<string, object>;
                if (parameters == null)
                {
                    Logger.Error("Modbus RTU初始化参数错误");
                    return false;
                }

                _port = parameters.ContainsKey("Port") ? parameters["Port"].ToString() : "COM1";
                _baudRate = parameters.ContainsKey("BaudRate") ? Convert.ToInt32(parameters["BaudRate"]) : 9600;
                _dataBits = parameters.ContainsKey("DataBits") ? Convert.ToInt32(parameters["DataBits"]) : 8;
                _parity = parameters.ContainsKey("Parity") ? parameters["Parity"].ToString() : "None";
                _stopBits = parameters.ContainsKey("StopBits") ? Convert.ToInt32(parameters["StopBits"]) : 1;
                _slaveId = parameters.ContainsKey("SlaveId") ? Convert.ToInt32(parameters["SlaveId"]) : 1;

                Logger.Info($"Modbus RTU初始化参数设置成功，端口: {_port}, 波特率: {_baudRate}");
                return true;
            }
            catch (FormatException ex)
            {
                Logger.Error("Modbus RTU初始化失败(参数格式错误)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU初始化失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 打开连接
        /// </summary>
        /// <returns>是否成功</returns>
        public override bool Open()
        {
            try
            {
                if (_comLink == null)
                {
                    // 从端口字符串中提取串口号，如"COM1" -> 1
                    int comNo = int.Parse(_port.Replace("COM", ""));
                    _comLink = new ComLink(comNo, "ModbusRTU", _baudRate);
                    _comLink.DataBits = _dataBits;
                    _comLink.StopBits = (StopBits)_stopBits;
                    _comLink.Parity = (Parity)Enum.Parse(typeof(Parity), _parity);
                }
                bool success = _comLink.Open();
                if (success)
                {
                    IsOpen = true;
                    Logger.Info($"Modbus RTU连接成功，端口: {_port}");
                }
                else
                {
                    Logger.Error("Modbus RTU连接失败");
                }
                return success;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU连接失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public override void Close()
        {
            try
            {
                if (_comLink != null)
                {
                    _comLink.Close();
                }
                IsOpen = false;
                Logger.Info("Modbus RTU断开连接");
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU断开连接失败", ex);
            }
        }

        /// <summary>
        /// 读取位
        /// </summary>
        protected override bool InternalReadBit(SoftElement element, int addr, ref bool bVal)
        {
            try
            {
                bool[] values;
                bool success = ReadCoils((ushort)addr, 1, out values);
                if (success && values.Length > 0)
                {
                    bVal = values[0];
                    return true;
                }
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU读取位失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 读取字
        /// </summary>
        protected override bool InternalReadWord(SoftElement element, int addr, ref ushort nVal)
        {
            try
            {
                ushort[] values;
                bool success = ReadHoldingRegisters((ushort)addr, 1, out values);
                if (success && values.Length > 0)
                {
                    nVal = values[0];
                    return true;
                }
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU读取字失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 读取双字
        /// </summary>
        protected override bool InternalReadDWord(SoftElement element, int addr, ref uint nVal)
        {
            try
            {
                ushort[] values;
                bool success = ReadHoldingRegisters((ushort)addr, 2, out values);
                if (success && values.Length >= 2)
                {
                    nVal = (uint)((values[0] << 16) | values[1]);
                    return true;
                }
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU读取双字失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 读取浮点数
        /// </summary>
        protected override bool InternalReadFloat(SoftElement element, int addr, ref float fVal)
        {
            try
            {
                ushort[] values;
                bool success = ReadHoldingRegisters((ushort)addr, 2, out values);
                if (success && values.Length >= 2)
                {
                    byte[] floatBytes = new byte[4];
                    floatBytes[0] = (byte)(values[0] >> 8);
                    floatBytes[1] = (byte)values[0];
                    floatBytes[2] = (byte)(values[1] >> 8);
                    floatBytes[3] = (byte)values[1];
                    fVal = BitConverter.ToSingle(floatBytes, 0);
                    return true;
                }
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU读取浮点数失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 写入位
        /// </summary>
        protected override bool InternalWriteBit(SoftElement element, int addr, bool bVal)
        {
            try
            {
                return WriteCoil((ushort)addr, bVal);
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU写入位失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 写入字
        /// </summary>
        protected override bool InternalWriteWord(SoftElement element, int addr, ushort nVal)
        {
            try
            {
                return WriteHoldingRegister((ushort)addr, nVal);
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU写入字失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 写入双字
        /// </summary>
        protected override bool InternalWriteDWord(SoftElement element, int addr, uint nVal)
        {
            try
            {
                ushort highWord = (ushort)(nVal >> 16);
                ushort lowWord = (ushort)nVal;

                bool success1 = WriteHoldingRegister((ushort)addr, highWord);
                bool success2 = WriteHoldingRegister((ushort)(addr + 1), lowWord);

                return success1 && success2;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU写入双字失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 写入浮点数
        /// </summary>
        protected override bool InternalWriteFloat(SoftElement element, int addr, float fVal)
        {
            try
            {
                byte[] floatBytes = BitConverter.GetBytes(fVal);
                ushort highWord = (ushort)((floatBytes[0] << 8) | floatBytes[1]);
                ushort lowWord = (ushort)((floatBytes[2] << 8) | floatBytes[3]);

                bool success1 = WriteHoldingRegister((ushort)addr, highWord);
                bool success2 = WriteHoldingRegister((ushort)(addr + 1), lowWord);

                return success1 && success2;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU写入浮点数失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 读取线圈
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="count">数量</param>
        /// <param name="values">值</param>
        /// <returns>是否成功</returns>
        public bool ReadCoils(ushort address, ushort count, out bool[] values)
        {
            values = null;
            try
            {
                // 构建Modbus RTU读取线圈命令
                byte[] command = BuildReadCoilsCommand((byte)_slaveId, address, count);

                // 发送命令
                if (!_comLink.Write(command))
                {
                    return false;
                }

                // 读取响应 — Modbus RTU 响应通常在 20-80ms 内，等待最多 150ms
                var sw = System.Diagnostics.Stopwatch.StartNew();
                int expectedLength = 5 + count / 8 + (count % 8 > 0 ? 1 : 0);
                while (sw.ElapsedMilliseconds < 150 && _comLink.BytesToRead < expectedLength)
                    Thread.Sleep(5);
                byte[] response = new byte[expectedLength];
                int bytesRead = _comLink.Read(response, 0, expectedLength);

                // 解析响应
                if (bytesRead >= expectedLength)
                {
                    values = new bool[count];
                    int byteCount = response[2];
                    for (int i = 0; i < count; i++)
                    {
                        int byteIndex = 3 + i / 8;
                        int bitIndex = i % 8;
                        values[i] = (response[byteIndex] & (1 << bitIndex)) != 0;
                    }
                    Logger.Info($"Modbus RTU读取线圈成功，地址: {address}, 数量: {count}");
                    return true;
                }
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU读取线圈失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 写入线圈
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public bool WriteCoil(ushort address, bool value)
        {
            try
            {
                // 构建Modbus RTU写入线圈命令
                byte[] command = BuildWriteCoilCommand((byte)_slaveId, address, value);

                // 发送命令
                if (!_comLink.Write(command))
                {
                    return false;
                }

                // 读取响应 — Modbus RTU 响应通常在 20-80ms 内，等待最多 150ms
                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 150 && _comLink.BytesToRead < 8)
                    Thread.Sleep(5);
                byte[] response = new byte[8];
                int bytesRead = _comLink.Read(response, 0, 8);

                // 解析响应
                if (bytesRead == 8)
                {
                    Logger.Info($"Modbus RTU写入线圈成功，地址: {address}, 值: {value}");
                    return true;
                }
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU写入线圈失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 读取保持寄存器
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="count">数量</param>
        /// <param name="values">值</param>
        /// <returns>是否成功</returns>
        public bool ReadHoldingRegisters(ushort address, ushort count, out ushort[] values)
        {
            values = null;
            try
            {
                // 构建Modbus RTU读取保持寄存器命令
                byte[] command = BuildReadHoldingRegistersCommand((byte)_slaveId, address, count);

                // 发送命令
                if (!_comLink.Write(command))
                {
                    return false;
                }

                // 读取响应 — Modbus RTU 响应通常在 20-80ms 内，等待最多 150ms
                var sw = System.Diagnostics.Stopwatch.StartNew();
                int expectedLength = 5 + count * 2;
                while (sw.ElapsedMilliseconds < 150 && _comLink.BytesToRead < expectedLength)
                    Thread.Sleep(5);
                byte[] response = new byte[expectedLength];
                int bytesRead = _comLink.Read(response, 0, expectedLength);

                // 解析响应
                if (bytesRead >= expectedLength)
                {
                    values = new ushort[count];
                    for (int i = 0; i < count; i++)
                    {
                        values[i] = (ushort)((response[3 + i * 2] << 8) | response[4 + i * 2]);
                    }
                    Logger.Info($"Modbus RTU读取保持寄存器成功，地址: {address}, 数量: {count}");
                    return true;
                }
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU读取保持寄存器失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 写入保持寄存器
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public bool WriteHoldingRegister(ushort address, ushort value)
        {
            try
            {
                // 构建Modbus RTU写入保持寄存器命令
                byte[] command = BuildWriteHoldingRegisterCommand((byte)_slaveId, address, value);

                // 发送命令
                if (!_comLink.Write(command))
                {
                    return false;
                }

                // 读取响应 — Modbus RTU 响应通常在 20-80ms 内，等待最多 150ms
                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 150 && _comLink.BytesToRead < 8)
                    Thread.Sleep(5);
                byte[] response = new byte[8];
                int bytesRead = _comLink.Read(response, 0, 8);

                // 解析响应
                if (bytesRead == 8)
                {
                    Logger.Info($"Modbus RTU写入保持寄存器成功，地址: {address}, 值: {value}");
                    return true;
                }
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC串口操作失败(IO错误)", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                Logger.Error("PLC串口操作失败(超时)", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("PLC串口操作失败(端口无效)", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("PLC串口操作失败(端口访问被拒绝)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Modbus RTU写入保持寄存器失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 构建读取线圈命令
        /// </summary>
        private byte[] BuildReadCoilsCommand(byte slaveId, ushort address, ushort count)
        {
            byte[] command = new byte[8];
            command[0] = slaveId;
            command[1] = 0x01; // 读取线圈
            command[2] = (byte)(address >> 8);
            command[3] = (byte)address;
            command[4] = (byte)(count >> 8);
            command[5] = (byte)count;

            // 计算CRC校验
            ushort crc = CalculateCRC(command, 6);
            command[6] = (byte)crc;
            command[7] = (byte)(crc >> 8);

            return command;
        }

        /// <summary>
        /// 构建写入线圈命令
        /// </summary>
        private byte[] BuildWriteCoilCommand(byte slaveId, ushort address, bool value)
        {
            byte[] command = new byte[8];
            command[0] = slaveId;
            command[1] = 0x05; // 写入单个线圈
            command[2] = (byte)(address >> 8);
            command[3] = (byte)address;
            command[4] = value ? (byte)0xFF : (byte)0x00;
            command[5] = 0x00;

            // 计算CRC校验
            ushort crc = CalculateCRC(command, 6);
            command[6] = (byte)crc;
            command[7] = (byte)(crc >> 8);

            return command;
        }

        /// <summary>
        /// 构建读取保持寄存器命令
        /// </summary>
        private byte[] BuildReadHoldingRegistersCommand(byte slaveId, ushort address, ushort count)
        {
            byte[] command = new byte[8];
            command[0] = slaveId;
            command[1] = 0x03; // 读取保持寄存器
            command[2] = (byte)(address >> 8);
            command[3] = (byte)address;
            command[4] = (byte)(count >> 8);
            command[5] = (byte)count;

            // 计算CRC校验
            ushort crc = CalculateCRC(command, 6);
            command[6] = (byte)crc;
            command[7] = (byte)(crc >> 8);

            return command;
        }

        /// <summary>
        /// 构建写入保持寄存器命令
        /// </summary>
        private byte[] BuildWriteHoldingRegisterCommand(byte slaveId, ushort address, ushort value)
        {
            byte[] command = new byte[8];
            command[0] = slaveId;
            command[1] = 0x06; // 写入单个保持寄存器
            command[2] = (byte)(address >> 8);
            command[3] = (byte)address;
            command[4] = (byte)(value >> 8);
            command[5] = (byte)value;

            // 计算CRC校验
            ushort crc = CalculateCRC(command, 6);
            command[6] = (byte)crc;
            command[7] = (byte)(crc >> 8);

            return command;
        }

        /// <summary>
        /// 计算CRC校验
        /// </summary>
        private ushort CalculateCRC(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }
    }
}