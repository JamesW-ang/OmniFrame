using System;
using OmniFrame.Common;

namespace Plc
{
    public abstract class DataBlock
    {
        public int Start { get; protected set; }
        public int End { get; protected set; }
        public int Count => End - Start + 1;
        public abstract int IntervalMax { get; }

        protected ushort[] _dataBuffer;

        public DataBlock(int start, int count)
        {
            Start = start;
            End = start + count - 1;
            _dataBuffer = new ushort[count];
        }

        public bool AllowMerge(int addr)
        {
            if (addr >= Start && addr <= End)
                return true;

            if (addr < Start)
            {
                return (Start - addr) <= IntervalMax;
            }
            else
            {
                return (addr - End) <= IntervalMax;
            }
        }

        public void Merge(int addr)
        {
            if (addr >= Start && addr <= End)
                return;

            int newStart = Math.Min(Start, addr);
            int newEnd = Math.Max(End, addr);
            int newCount = newEnd - newStart + 1;

            ushort[] newBuffer = new ushort[newCount];
            Array.Copy(_dataBuffer, 0, newBuffer, Start - newStart, _dataBuffer.Length);

            Start = newStart;
            End = newEnd;
            _dataBuffer = newBuffer;
        }

        public bool Contain(int addr)
        {
            return addr >= Start && addr <= End;
        }

        public abstract bool ReadFromPlc(PlcDevice plc, SoftElement element);
        public abstract bool ReadBit(int addr);
        public abstract bool ReadWord(int addr, ref ushort val);
    }

    public class BitBlock : DataBlock
    {
        public override int IntervalMax => 128;

        public BitBlock(int start, int count) : base(start, count)
        {
        }

        public override bool ReadFromPlc(PlcDevice plc, SoftElement element)
        {
            try
            {
                int byteCount = (Count + 7) / 8;
                byte[] buffer = new byte[byteCount];

                for (int i = 0; i < Count; i++)
                {
                    bool val = false;
                    if (!plc.ReadBit(element, Start + i, ref val))
                        return false;

                    int byteIndex = i / 8;
                    int bitIndex = i % 8;
                    if (val)
                        buffer[byteIndex] |= (byte)(1 << bitIndex);
                }

                for (int i = 0; i < Count; i++)
                {
                    int byteIndex = i / 8;
                    int bitIndex = i % 8;
                    _dataBuffer[i] = (ushort)((buffer[byteIndex] >> bitIndex) & 0x01);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Warning($"BitBlock读取PLC失败, Start={Start}, Count={Count}: {ex.Message}", ex);
                return false;
            }
        }

        public override bool ReadBit(int addr)
        {
            if (!Contain(addr))
                return false;

            return _dataBuffer[addr - Start] != 0;
        }

        public override bool ReadWord(int addr, ref ushort val)
        {
            return false;
        }
    }

    public class WordBlock : DataBlock
    {
        public override int IntervalMax => 16;

        public WordBlock(int start, int count) : base(start, count)
        {
        }

        public override bool ReadFromPlc(PlcDevice plc, SoftElement element)
        {
            try
            {
                for (int i = 0; i < Count; i++)
                {
                    ushort val = 0;
                    if (!plc.ReadWord(element, Start + i, ref val))
                        return false;
                    _dataBuffer[i] = val;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warning($"WordBlock读取PLC失败, Start={Start}, Count={Count}: {ex.Message}", ex);
                return false;
            }
        }

        public override bool ReadBit(int addr)
        {
            return false;
        }

        public override bool ReadWord(int addr, ref ushort val)
        {
            if (!Contain(addr))
                return false;

            val = _dataBuffer[addr - Start];
            return true;
        }
    }
}
