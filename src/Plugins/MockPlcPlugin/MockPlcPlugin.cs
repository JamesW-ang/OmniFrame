using System;
using System.Collections.Generic;
using System.Threading;

namespace OmniFrame.Plugins.MockPlc
{
    [OmniFrame.Sdk.PluginSystem.PluginVersion(1, 0, 0)]
    public class MockPlcPlugin : OmniFrame.Sdk.PluginSystem.PlcPlugin
    {
        private readonly Dictionary<string, int> _registers = new Dictionary<string, int>();
        private readonly Random _rng = new Random();
        private bool _connected;
        private string _ipAddress;
        private int _port;

        public override string Name => "MockPlcPlugin";
        public override string Description => "模拟PLC控制器 — 支持寄存器读写，预置常用IO点位，用于离线调试";

        public override bool Initialize()
        {
            _registers["D0"] = 0;
            _registers["D100"] = 0;
            _registers["D200"] = 0;
            _registers["M0"] = 0;
            _registers["M100"] = 0;
            _registers["X0"] = 0;
            _registers["Y0"] = 0;
            return true;
        }

        public override void Unload()
        {
            Disconnect();
            _registers.Clear();
        }

        public override bool Connect(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            Thread.Sleep(_rng.Next(30, 100));
            _connected = true;
            return true;
        }

        public override void Disconnect()
        {
            _connected = false;
            _ipAddress = null;
        }

        public override int ReadRegister(string address)
        {
            if (!_connected) return -1;
            if (_registers.ContainsKey(address))
            {
                if (address.StartsWith("X") || address.StartsWith("D"))
                    _registers[address] = _rng.Next(0, 65535);
                return _registers[address];
            }
            _registers[address] = 0;
            return 0;
        }

        public override bool WriteRegister(string address, int value)
        {
            if (!_connected) return false;
            _registers[address] = value;
            Thread.Sleep(2);
            return true;
        }
    }
}
