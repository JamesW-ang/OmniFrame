using System;
using OmniFrame.Common;
using Plc;

namespace OmniFrame.Core
{
    public class PlcManager : IPlcManager, IDisposable
    {
        private PlcDevice _plc;
        private string _ipAddress;
        private int _port;
        private bool _disposed;


        public PlcManager()
        {
        }

        public bool Initialize(string plcType, string ip, int port)
        {
            try
            {
                // 根据plcType创建对应的PlcDevice实例
                switch (plcType.ToUpper())
                {
                    case "MITSUBISHI":
                        var mitsubishiPlc = new Plc_Mitsubishi();
                        mitsubishiPlc.Initialize(ip, port);
                        _plc = mitsubishiPlc;
                        break;
                    default:
                        Logger.Error($"未知的PLC类型: {plcType}");
                        return false;
                }

                if (_plc == null)
                {
                    return false;
                }

                _ipAddress = ip;
                _port = port;
                return _plc.Connect();
            }
            catch (Exception ex)
            {
                Logger.Error("PlcManager初始化失败", ex);
                return false;
            }
        }

        public bool Write(Plc.SoftElement element, int address, object value)
        {
            try
            {
                if (_plc == null)
                    return false;

                switch (value)
                {
                    case bool boolValue:
                        return _plc.WriteBit(element, address, boolValue);
                    case ushort ushortValue:
                        return _plc.WriteWord(element, address, ushortValue);
                    case uint uintValue:
                        return _plc.WriteDWord(element, address, uintValue);
                    case float floatValue:
                        return _plc.WriteFloat(element, address, floatValue);
                    default:
                        Logger.Error($"不支持的写入值类型: {value.GetType()}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("PlcManager写入失败", ex);
                return false;
            }
        }

        public object Read(Plc.SoftElement element, int address, Type returnType)
        {
            try
            {
                if (_plc == null)
                    return null;

                if (returnType == typeof(bool))
                {
                    bool value = false;
                    if (_plc.ReadBit(element, address, ref value))
                    {
                        return value;
                    }
                }
                else if (returnType == typeof(ushort))
                {
                    ushort value = 0;
                    if (_plc.ReadWord(element, address, ref value))
                    {
                        return value;
                    }
                }
                else if (returnType == typeof(uint))
                {
                    uint value = 0;
                    if (_plc.ReadDWord(element, address, ref value))
                    {
                        return value;
                    }
                }
                else if (returnType == typeof(float))
                {
                    float value = 0;
                    if (_plc.ReadFloat(element, address, ref value))
                    {
                        return value;
                    }
                }
                else
                {
                    Logger.Error($"不支持的读取类型: {returnType}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("PlcManager读取失败", ex);
                return null;
            }
        }

        public bool IsConnected
        {
            get
            {
                try
                {
                    return _plc != null && _plc.IsConnected;
                }
                catch (Exception ex)
                {
                    Logger.Error("PlcManager获取连接状态失败", ex);
                    return false;
                }
            }
        }

        public void Disconnect()
        {
            try
            {
                _plc?.Disconnect();
            }
            catch (Exception ex)
            {
                Logger.Error("PlcManager断开连接失败", ex);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                _plc?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error("PlcManager释放失败", ex);
            }
            GC.SuppressFinalize(this);
        }
    }
}
