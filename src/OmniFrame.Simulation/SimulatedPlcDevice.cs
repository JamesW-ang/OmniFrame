using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OmniFrame.Common;

namespace OmniFrame.Simulation
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct FloatUIntConverter
    {
        [FieldOffset(0)] public float FloatValue;
        [FieldOffset(0)] public uint UIntValue;
    }

    public class SimulatedPlcDevice : Plc.PlcDevice
    {
        private readonly Dictionary<int, bool> _xBits = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool> _yBits = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool> _mBits = new Dictionary<int, bool>();
        private readonly Dictionary<int, ushort> _dWords = new Dictionary<int, ushort>();
        private readonly object _lock = new object();

        private const int MaxX = 255;
        private const int MaxY = 255;
        private const int MaxM = 8191;
        private const int MaxD = 8191;

        public string IP { get; }
        public int Port { get; }

        public SimulatedPlcDevice(string name, string ip, int port)
            : base(0, name)
        {
            IP = ip;
            Port = port;
        }

        public override bool Open()
        {
            lock (_lock)
            {
                IsOpen = true;
                LogInfo($"模拟PLC连接成功: {IP}:{Port}");
                return true;
            }
        }

        public override void Close()
        {
            lock (_lock)
            {
                IsOpen = false;
                LogInfo("模拟PLC已断开");
            }
        }

        private Dictionary<int, bool> GetBitDict(Plc.SoftElement element)
        {
            return element switch
            {
                Plc.SoftElement.X => _xBits,
                Plc.SoftElement.Y => _yBits,
                Plc.SoftElement.M => _mBits,
                _ => null
            };
        }

        private bool IsValidBitAddr(Plc.SoftElement element, int addr)
        {
            return element switch
            {
                Plc.SoftElement.X => addr >= 0 && addr <= MaxX,
                Plc.SoftElement.Y => addr >= 0 && addr <= MaxY,
                Plc.SoftElement.M => addr >= 0 && addr <= MaxM,
                _ => false
            };
        }

        private bool IsValidDAddr(int addr) => addr >= 0 && addr <= MaxD;

        protected override bool InternalReadBit(Plc.SoftElement element, int addr, ref bool bVal)
        {
            lock (_lock)
            {
                if (element == Plc.SoftElement.D) return false;
                if (!IsValidBitAddr(element, addr)) return false;

                var dict = GetBitDict(element);
                bVal = dict.TryGetValue(addr, out bool val) ? val : false;
                return true;
            }
        }

        protected override bool InternalReadWord(Plc.SoftElement element, int addr, ref ushort nVal)
        {
            lock (_lock)
            {
                if (element != Plc.SoftElement.D) return false;
                if (!IsValidDAddr(addr)) return false;

                nVal = _dWords.TryGetValue(addr, out ushort val) ? val : (ushort)0;
                return true;
            }
        }

        protected override bool InternalReadDWord(Plc.SoftElement element, int addr, ref uint nVal)
        {
            lock (_lock)
            {
                if (element != Plc.SoftElement.D) return false;
                if (!IsValidDAddr(addr) || !IsValidDAddr(addr + 1)) return false;

                ushort low = _dWords.TryGetValue(addr, out ushort l) ? l : (ushort)0;
                ushort high = _dWords.TryGetValue(addr + 1, out ushort h) ? h : (ushort)0;
                nVal = (uint)((high << 16) | low);
                return true;
            }
        }

        protected override bool InternalReadFloat(Plc.SoftElement element, int addr, ref float fVal)
        {
            uint raw = 0;
            if (!InternalReadDWord(element, addr, ref raw)) return false;
            fVal = new FloatUIntConverter { UIntValue = raw }.FloatValue;
            return true;
        }

        protected override bool InternalWriteBit(Plc.SoftElement element, int addr, bool bVal)
        {
            lock (_lock)
            {
                if (element == Plc.SoftElement.D) return false;
                if (!IsValidBitAddr(element, addr)) return false;

                var dict = GetBitDict(element);
                dict[addr] = bVal;
                return true;
            }
        }

        protected override bool InternalWriteWord(Plc.SoftElement element, int addr, ushort nVal)
        {
            lock (_lock)
            {
                if (element != Plc.SoftElement.D) return false;
                if (!IsValidDAddr(addr)) return false;

                _dWords[addr] = nVal;
                return true;
            }
        }

        protected override bool InternalWriteDWord(Plc.SoftElement element, int addr, uint nVal)
        {
            lock (_lock)
            {
                if (element != Plc.SoftElement.D) return false;
                if (!IsValidDAddr(addr) || !IsValidDAddr(addr + 1)) return false;

                _dWords[addr] = (ushort)(nVal & 0xFFFF);
                _dWords[addr + 1] = (ushort)((nVal >> 16) & 0xFFFF);
                return true;
            }
        }

        protected override bool InternalWriteFloat(Plc.SoftElement element, int addr, float fVal)
        {
            var converter = new FloatUIntConverter { FloatValue = fVal };
            return InternalWriteDWord(element, addr, converter.UIntValue);
        }

        public void SetBitValue(Plc.SoftElement element, int addr, bool value)
        {
            InternalWriteBit(element, addr, value);
        }

        public void SetWordValue(Plc.SoftElement element, int addr, ushort value)
        {
            InternalWriteWord(element, addr, value);
        }

        public void ClearAllRegisters()
        {
            lock (_lock)
            {
                _xBits.Clear();
                _yBits.Clear();
                _mBits.Clear();
                _dWords.Clear();
            }
            LogInfo("所有寄存器已清除");
        }
    }
}
