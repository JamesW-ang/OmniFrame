using System;

namespace OmniFrame.Core
{
    public interface IIoManager
    {
        bool Initialize(string ioType, string configPath);
        bool SetOutput(int port, int pin, bool value);
        bool GetInput(int port, int pin);
        bool IsConnected { get; }
        void Dispose();
    }
}
