using System;

namespace OmniFrame.Core
{
    public interface IPlcManager
    {
        bool Initialize(string plcType, string ip, int port);
        bool Write(Plc.SoftElement element, int address, object value);
        object Read(Plc.SoftElement element, int address, Type returnType);
        bool IsConnected { get; }
        void Disconnect();
    }
}
