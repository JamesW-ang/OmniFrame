using System;
using System.Collections.Generic;
using OmniFrame.Common;

namespace Plc
{
    public enum SoftElement
    {
        X,  // 输入继电器
        Y,  // 输出继电器
        M,  // 辅助继电器
        D   // 数据寄存器
    }

    /// <summary>
    /// PLC设备抽象基类
    /// 设计介绍：
    /// 2. 实现IDisposable接口，确保资源正确释放
    /// 4. 支持位、字、双字、浮点数的读写操作
    /// 6. 提供统一的日志记录机制
    /// 7. 支持设备的连接和断开操作
    /// 8. 实现了软元件（X、Y、M、D）的操作接口
        /// </summary>
    public abstract class PlcDevice : IDisposable, IDevice
    {
        public bool IsOpen { get; protected set; }
        public int Index { get; protected set; }
        public string Name { get; protected set; }
        public int CmdCountLimit { get; set; } = 32;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => IsOpen;

        /// <summary>
        /// 错误事件
        /// </summary>
        public event EventHandler<PlcErrorEventArgs> ErrorOccurred;

        protected PlcDevice(int index, string name)
        {
            Index = index;
            Name = name;
            IsOpen = false;
        }

        public abstract bool Open();
        public abstract void Close();

        /// <summary>
        /// 连接设备
        /// </summary>
        public bool Connect()
        {
            return Open();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            Close();
        }

        public virtual bool ReadBit(SoftElement element, int addr, ref bool bVal)
        {
            var startTime = DateTime.Now;
            var result = InternalReadBit(element, addr, ref bVal);
            var elapsed = DateTime.Now - startTime;
            LogInfo($"读取位 {element}{addr}: 值={bVal}, 结果={result}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return result;
        }

        protected abstract bool InternalReadBit(SoftElement element, int addr, ref bool bVal);

        public virtual bool ReadWord(SoftElement element, int addr, ref ushort nVal)
        {
            var startTime = DateTime.Now;
            var result = InternalReadWord(element, addr, ref nVal);
            var elapsed = DateTime.Now - startTime;
            LogInfo($"读取字 {element}{addr}: 值={nVal}, 结果={result}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return result;
        }

        protected abstract bool InternalReadWord(SoftElement element, int addr, ref ushort nVal);

        public virtual bool ReadDWord(SoftElement element, int addr, ref uint nVal)
        {
            var startTime = DateTime.Now;
            var result = InternalReadDWord(element, addr, ref nVal);
            var elapsed = DateTime.Now - startTime;
            LogInfo($"读取双字 {element}{addr}: 值={nVal}, 结果={result}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return result;
        }

        protected abstract bool InternalReadDWord(SoftElement element, int addr, ref uint nVal);

        public virtual bool ReadFloat(SoftElement element, int addr, ref float fVal)
        {
            var startTime = DateTime.Now;
            var result = InternalReadFloat(element, addr, ref fVal);
            var elapsed = DateTime.Now - startTime;
            LogInfo($"读取浮点数 {element}{addr}: 值={fVal}, 结果={result}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return result;
        }

        protected abstract bool InternalReadFloat(SoftElement element, int addr, ref float fVal);

        public virtual bool WriteBit(SoftElement element, int addr, bool bVal)
        {
            var startTime = DateTime.Now;
            var result = InternalWriteBit(element, addr, bVal);
            var elapsed = DateTime.Now - startTime;
            LogInfo($"写入位 {element}{addr}: 值={bVal}, 结果={result}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return result;
        }

        protected abstract bool InternalWriteBit(SoftElement element, int addr, bool bVal);

        public virtual bool WriteWord(SoftElement element, int addr, ushort nVal)
        {
            var startTime = DateTime.Now;
            var result = InternalWriteWord(element, addr, nVal);
            var elapsed = DateTime.Now - startTime;
            LogInfo($"写入字 {element}{addr}: 值={nVal}, 结果={result}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return result;
        }

        protected abstract bool InternalWriteWord(SoftElement element, int addr, ushort nVal);

        public virtual bool WriteDWord(SoftElement element, int addr, uint nVal)
        {
            var startTime = DateTime.Now;
            var result = InternalWriteDWord(element, addr, nVal);
            var elapsed = DateTime.Now - startTime;
            LogInfo($"写入双字 {element}{addr}: 值={nVal}, 结果={result}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return result;
        }

        protected abstract bool InternalWriteDWord(SoftElement element, int addr, uint nVal);

        public virtual bool WriteFloat(SoftElement element, int addr, float fVal)
        {
            var startTime = DateTime.Now;
            var result = InternalWriteFloat(element, addr, fVal);
            var elapsed = DateTime.Now - startTime;
            LogInfo($"写入浮点数 {element}{addr}: 值={fVal}, 结果={result}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return result;
        }

        protected abstract bool InternalWriteFloat(SoftElement element, int addr, float fVal);

        public virtual bool ReadBits(SoftElement element, int startAddr, int count, bool[] values)
        {
            if (values == null || values.Length < count)
                return false;

            var startTime = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                if (!ReadBit(element, startAddr + i, ref values[i]))
                    return false;
            }
            var elapsed = DateTime.Now - startTime;
            LogInfo($"批量读取位 {element}{startAddr} 共{count}个: 结果={true}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return true;
        }

        public virtual bool ReadWords(SoftElement element, int startAddr, int count, ushort[] values)
        {
            if (values == null || values.Length < count)
                return false;

            var startTime = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                if (!ReadWord(element, startAddr + i, ref values[i]))
                    return false;
            }
            var elapsed = DateTime.Now - startTime;
            LogInfo($"批量读取字 {element}{startAddr} 共{count}个: 结果={true}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return true;
        }

        public virtual bool WriteBits(SoftElement element, int startAddr, int count, bool[] values)
        {
            if (values == null || values.Length < count)
                return false;

            var startTime = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                if (!WriteBit(element, startAddr + i, values[i]))
                    return false;
            }
            var elapsed = DateTime.Now - startTime;
            LogInfo($"批量写入位 {element}{startAddr} 共{count}个: 结果={true}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return true;
        }

        public virtual bool WriteWords(SoftElement element, int startAddr, int count, ushort[] values)
        {
            if (values == null || values.Length < count)
                return false;

            var startTime = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                if (!WriteWord(element, startAddr + i, values[i]))
                    return false;
            }
            var elapsed = DateTime.Now - startTime;
            LogInfo($"批量写入字 {element}{startAddr} 共{count}个: 结果={true}, 耗时={elapsed.TotalMilliseconds:F2}ms");
            return true;
        }

        /// <summary>
        /// 初始化设备
        /// </summary>
        public virtual bool Initialize()
        {
            return Open();
        }

        /// <summary>
        /// 复位设备
        /// </summary>
        public virtual bool Reset()
        {
            LogInfo("PLC设备复位");
            return true;
        }

        /// <summary>
        /// 触发错误事件
        /// </summary>
        protected void OnError(string code, string message)
        {
            var args = new PlcErrorEventArgs
            {
                ErrorCode = code,
                ErrorMessage = message
            };

            LogError($"[{code}] {message}");
            ErrorOccurred?.Invoke(this, args);
        }

        protected void LogError(string message, Exception ex = null)
        {
            if (ex != null)
                Logger.Error($"[{Name}] {message}", ex);
            else
                Logger.Error($"[{Name}] {message}");
        }

        protected void LogInfo(string message)
        {
            Logger.Info($"[{Name}] {message}");
        }

        protected void LogWarning(string message)
        {
            Logger.Warning($"[{Name}] {message}");
        }

        public void Dispose()
        {
            if (IsOpen)
            {
                Close();
            }
        }
    }
}
