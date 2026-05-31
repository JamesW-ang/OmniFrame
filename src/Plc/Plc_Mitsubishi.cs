using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.Communication;

namespace Plc
{
    /// <summary>
    /// 三菱PLC驱动
    /// 设计介绍：
    /// 1. 继承自PlcDevice抽象基类，实现三菱PLC的通信功能
    /// 3. 实现三菱PLC专用的通信协议和数据格式
    /// 4. 支持位、字、双字、浮点数的读写操作
    /// 5. 提供完善的错误处理和日志记录
    /// </summary>
    public class Plc_Mitsubishi : PlcDevice
    {
        private TcpLink _tcpLink;
        private string _ipAddress;
        private int _port;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Plc_Mitsubishi() : base(0, "Mitsubishi")
        {
        }

        /// <summary>
        /// 初始化连接参数（在 Open 前调用）
        /// </summary>
        public void Initialize(string ipAddress, int port)
        {
            _ipAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            _port = port;
        }

        /// <summary>
        /// 打开连接
        /// 功能说明：建立与三菱PLC的TCP连接
        /// </summary>
        /// <returns>是否打开成功</returns>
        public override bool Open()
        {
            try
            {
                if (_tcpLink == null)
                {
                    _tcpLink = new TcpLink(0, "Mitsubishi", _ipAddress, _port);
                }
                bool success = _tcpLink.Open();
                if (success)
                {
                    IsOpen = true;
                    Logger.Info($"三菱PLC连接成功，IP: {_ipAddress}, 端口: {_port}");
                }
                else
                {
                    Logger.Error("三菱PLC连接失败");
                }
                return success;
            }
            catch (SocketException ex)
            {
                Logger.Error("PLC TCP操作失败(网络错误)", ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC TCP操作失败(IO错误)", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("PLC TCP操作失败(连接已断开)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("三菱PLC连接失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 关闭连接
        /// 功能说明：断开与三菱PLC的连接
        /// </summary>
        public override void Close()
        {
            try
            {
                if (_tcpLink != null)
                {
                    _tcpLink.Close();
                }
                IsOpen = false;
                Logger.Info("三菱PLC断开连接");
            }
            catch (SocketException ex)
            {
                Logger.Error("PLC TCP操作失败(网络错误)", ex);
            }
            catch (IOException ex)
            {
                Logger.Error("PLC TCP操作失败(IO错误)", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("PLC TCP操作失败(连接已断开)", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("三菱PLC断开连接失败", ex);
            }
        }

        /// <summary>
        /// 读取位
        /// </summary>
        protected override bool InternalReadBit(SoftElement element, int addr, ref bool bVal)
        {
            try
            {
                byte[] command = BuildReadCommand((ushort)addr, 1, 0x30);
                _tcpLink.Write(command);

                byte[] buffer = new byte[13];
                int bytesRead = _tcpLink.Read(buffer, 0, 13);

                if (bytesRead >= 13)
                {
                    bVal = buffer[12] == 0x01;
                    return true;
                }
                return false;
            }
            catch (SocketException ex)
            {
                Logger.Error("PLC TCP操作失败(网络错误)", ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC TCP操作失败(IO错误)", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("PLC TCP操作失败(连接已断开)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("三菱PLC读取位失败", ex);
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
                byte[] command = BuildReadCommand((ushort)addr, 1, 0x32);
                _tcpLink.Write(command);

                byte[] buffer = new byte[14];
                int bytesRead = _tcpLink.Read(buffer, 0, 14);

                if (bytesRead >= 14)
                {
                    nVal = (ushort)((buffer[12] << 8) | buffer[13]);
                    return true;
                }
                return false;
            }
            catch (SocketException ex)
            {
                Logger.Error("PLC TCP操作失败(网络错误)", ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC TCP操作失败(IO错误)", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("PLC TCP操作失败(连接已断开)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("三菱PLC读取字失败", ex);
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
                byte[] command = BuildReadCommand((ushort)addr, 2, 0x32);
                _tcpLink.Write(command);

                byte[] buffer = new byte[16];
                int bytesRead = _tcpLink.Read(buffer, 0, 16);

                if (bytesRead >= 16)
                {
                    nVal = (uint)((buffer[12] << 24) | (buffer[13] << 16) | (buffer[14] << 8) | buffer[15]);
                    return true;
                }
                return false;
            }
            catch (SocketException ex)
            {
                Logger.Error("PLC TCP操作失败(网络错误)", ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC TCP操作失败(IO错误)", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("PLC TCP操作失败(连接已断开)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("三菱PLC读取双字失败", ex);
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
                byte[] command = BuildReadCommand((ushort)addr, 2, 0x32);
                _tcpLink.Write(command);

                byte[] buffer = new byte[16];
                int bytesRead = _tcpLink.Read(buffer, 0, 16);

                if (bytesRead >= 16)
                {
                    byte[] floatBytes = new byte[4] { buffer[12], buffer[13], buffer[14], buffer[15] };
                    fVal = BitConverter.ToSingle(floatBytes, 0);
                    return true;
                }
                return false;
            }
            catch (SocketException ex)
            {
                Logger.Error("PLC TCP操作失败(网络错误)", ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC TCP操作失败(IO错误)", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("PLC TCP操作失败(连接已断开)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("三菱PLC读取浮点数失败", ex);
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
                byte[] command = BuildWriteCommand((ushort)addr, bVal ? (byte)0x01 : (byte)0x00, 0x31);
                _tcpLink.Write(command);

                byte[] buffer = new byte[12];
                int bytesRead = _tcpLink.Read(buffer, 0, 12);

                if (bytesRead == 12)
                {
                    return true;
                }
                return false;
            }
            catch (SocketException ex)
            {
                Logger.Error("PLC TCP操作失败(网络错误)", ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC TCP操作失败(IO错误)", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("PLC TCP操作失败(连接已断开)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("三菱PLC写入位失败", ex);
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
                byte[] command = BuildWriteWordCommand((ushort)addr, nVal, 0x33);
                _tcpLink.Write(command);

                byte[] buffer = new byte[12];
                int bytesRead = _tcpLink.Read(buffer, 0, 12);

                if (bytesRead == 12)
                {
                    return true;
                }
                return false;
            }
            catch (SocketException ex)
            {
                Logger.Error("PLC TCP操作失败(网络错误)", ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC TCP操作失败(IO错误)", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("PLC TCP操作失败(连接已断开)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("三菱PLC写入字失败", ex);
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
                byte[] command = new byte[15];
                command[0] = 0x50;
                command[1] = 0x00;
                command[2] = 0x00;
                command[3] = 0xFF;
                command[4] = 0x06;
                command[5] = 0x00;
                command[6] = 0x33;
                command[7] = (byte)(addr >> 8);
                command[8] = (byte)addr;
                command[9] = (byte)(nVal >> 24);
                command[10] = (byte)(nVal >> 16);
                command[11] = (byte)(nVal >> 8);
                command[12] = (byte)nVal;
                command[13] = 0x00;
                command[14] = 0x00;

                _tcpLink.Write(command);

                byte[] buffer = new byte[12];
                int bytesRead = _tcpLink.Read(buffer, 0, 12);

                if (bytesRead == 12)
                {
                    return true;
                }
                return false;
            }
            catch (SocketException ex)
            {
                Logger.Error("PLC TCP操作失败(网络错误)", ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC TCP操作失败(IO错误)", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("PLC TCP操作失败(连接已断开)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("三菱PLC写入双字失败", ex);
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
                byte[] command = new byte[15];
                command[0] = 0x50;
                command[1] = 0x00;
                command[2] = 0x00;
                command[3] = 0xFF;
                command[4] = 0x06;
                command[5] = 0x00;
                command[6] = 0x33;
                command[7] = (byte)(addr >> 8);
                command[8] = (byte)addr;
                command[9] = floatBytes[0];
                command[10] = floatBytes[1];
                command[11] = floatBytes[2];
                command[12] = floatBytes[3];
                command[13] = 0x00;
                command[14] = 0x00;

                _tcpLink.Write(command);

                byte[] buffer = new byte[12];
                int bytesRead = _tcpLink.Read(buffer, 0, 12);

                if (bytesRead == 12)
                {
                    return true;
                }
                return false;
            }
            catch (SocketException ex)
            {
                Logger.Error("PLC TCP操作失败(网络错误)", ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("PLC TCP操作失败(IO错误)", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("PLC TCP操作失败(连接已断开)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("三菱PLC写入浮点数失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 构建读取命令
        /// </summary>
        private byte[] BuildReadCommand(ushort address, ushort count, byte functionCode)
        {
            byte[] command = new byte[12];
            command[0] = 0x50; // 帧头
            command[1] = 0x00; // 帧头
            command[2] = 0x00; // 网络号
            command[3] = 0xFF; // 站号
            command[4] = 0x03; // 长度
            command[5] = 0x00; // 长度
            command[6] = functionCode; // 功能码
            command[7] = (byte)(address >> 8); // 地址高字节
            command[8] = (byte)address; // 地址低字节
            command[9] = (byte)(count >> 8); // 数量高字节
            command[10] = (byte)count; // 数量低字节
            command[11] = 0x00; // 结束符
            return command;
        }

        /// <summary>
        /// 构建写入命令
        /// </summary>
        private byte[] BuildWriteCommand(ushort address, byte value, byte functionCode)
        {
            byte[] command = new byte[13];
            command[0] = 0x50; // 帧头
            command[1] = 0x00; // 帧头
            command[2] = 0x00; // 网络号
            command[3] = 0xFF; // 站号
            command[4] = 0x04; // 长度
            command[5] = 0x00; // 长度
            command[6] = functionCode; // 功能码
            command[7] = (byte)(address >> 8); // 地址高字节
            command[8] = (byte)address; // 地址低字节
            command[9] = value; // 值
            command[10] = 0x00; // 结束符
            command[11] = 0x00; // 结束符
            command[12] = 0x00; // 结束符
            return command;
        }

        /// <summary>
        /// 构建写入字命令
        /// </summary>
        private byte[] BuildWriteWordCommand(ushort address, ushort value, byte functionCode)
        {
            byte[] command = new byte[14];
            command[0] = 0x50; // 帧头
            command[1] = 0x00; // 帧头
            command[2] = 0x00; // 网络号
            command[3] = 0xFF; // 站号
            command[4] = 0x05; // 长度
            command[5] = 0x00; // 长度
            command[6] = functionCode; // 功能码
            command[7] = (byte)(address >> 8); // 地址高字节
            command[8] = (byte)address; // 地址低字节
            command[9] = (byte)(value >> 8); // 值高字节
            command[10] = (byte)value; // 值低字节
            command[11] = 0x00; // 结束符
            command[12] = 0x00; // 结束符
            command[13] = 0x00; // 结束符
            return command;
        }
    }
}