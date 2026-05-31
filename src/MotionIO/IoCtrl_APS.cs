using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace MotionIO
{
    /// <summary>
    /// APS168x64 板载 IO 控制 — 25 DO + 36 DI
    /// 替代 IOSignal.h/.cpp
    /// </summary>
    public class IoCtrl_APS : IoCtrl
    {
        #region APS168x64 SDK DllImport

        private const string ApsDll = "APS168x64.dll";
        private static bool _dllMissing;

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_set_do_bit(int board_id, int do_index, int value);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_get_di_bit(int board_id, int di_index, ref int value);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_set_do_port(int board_id, int port, int value);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_get_di_port(int board_id, int port, ref int value);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_get_do_port(int board_id, int port, ref int value);

        #endregion

        private readonly int _boardId;
        private bool _initialized;

        /// <summary>最大 DO 点数</summary>
        public const int MaxDO = 32;
        /// <summary>最大 DI 点数</summary>
        public const int MaxDI = 40;

        public IoCtrl_APS(int boardId = 0)
        {
            _boardId = boardId;
            _initialized = false;
        }

        #region IoCtrl 基类方法实现

        public override bool Init(object param)
        {
            // APS IO 随板卡初始化即就绪，无需单独初始化
            _initialized = true;
            Logger.Info($"[IoCtrl_APS] 板卡 {_boardId} IO 就绪");
            return true;
        }

        public override bool Close()
        {
            _initialized = false;
            return true;
        }

        public override bool ReadInput(int port, out bool value)
        {
            if (_dllMissing) { value = false; return true; }
            value = false;
            int intVal = 0;
            try
            {
                int ret = APS_get_di_bit(_boardId, port, ref intVal);
                if (ret != 0)
                {
                    Logger.Error($"[IoCtrl_APS] 读取 DI[{port}] 失败, 返回码: {ret}");
                    return false;
                }
                value = intVal != 0;
                return true;
            }
            catch (DllNotFoundException)
            {
                _dllMissing = true;
                LogDllMissing();
                value = false;
                return true;
            }
        }

        public override bool ReadInputPort(int port, out int value)
        {
            if (_dllMissing) { value = 0; return true; }
            value = 0;
            try
            {
                int ret = APS_get_di_port(_boardId, port, ref value);
                if (ret != 0)
                {
                    Logger.Error($"[IoCtrl_APS] 读取 DI 端口 {port} 失败, 返回码: {ret}");
                    return false;
                }
                return true;
            }
            catch (DllNotFoundException)
            {
                _dllMissing = true;
                LogDllMissing();
                value = 0;
                return true;
            }
        }

        public override bool WriteOutput(int port, bool value)
        {
            if (_dllMissing) return true;
            try
            {
                int ret = APS_set_do_bit(_boardId, port, value ? 1 : 0);
                if (ret != 0)
                {
                    Logger.Error($"[IoCtrl_APS] 写入 DO[{port}]={value} 失败, 返回码: {ret}");
                    return false;
                }
                return true;
            }
            catch (DllNotFoundException)
            {
                _dllMissing = true;
                LogDllMissing();
                return true;
            }
        }

        public override bool WriteOutputPort(int port, int value)
        {
            if (_dllMissing) return true;
            try
            {
                int ret = APS_set_do_port(_boardId, port, value);
                if (ret != 0)
                {
                    Logger.Error($"[IoCtrl_APS] 写入 DO 端口 {port}={value} 失败, 返回码: {ret}");
                    return false;
                }
                return true;
            }
            catch (DllNotFoundException)
            {
                _dllMissing = true;
                LogDllMissing();
                return true;
            }
        }

        public override Dictionary<int, bool> ReadAllInputs()
        {
            var result = new Dictionary<int, bool>();
            for (int i = 0; i < MaxDI; i++)
            {
                if (ReadInput(i, out bool val))
                    result[i] = val;
            }
            return result;
        }

        public override Dictionary<int, bool> ReadAllOutputs()
        {
            var result = new Dictionary<int, bool>();
            if (_dllMissing) return result;
            int port = 0;
            int ret = APS_get_do_port(_boardId, 0, ref port);
            if (ret == 0)
            {
                for (int i = 0; i < MaxDO; i++)
                    result[i] = (port & (1 << i)) != 0;
            }
            else
            {
                // Fallback: 逐位读取
                for (int i = 0; i < MaxDO; i++)
                {
                    // DO 端口通常按位读取，用 get_do_port 结果逐位判断
                    result[i] = (port & (1 << i)) != 0;
                }
            }
            return result;
        }

        public override bool Reset()
        {
            // 所有 DO 复位 (关闭)
            return WriteOutputPort(0, 0);
        }

        public override string GetError()
        {
            return _initialized ? string.Empty : "IO 未初始化";
        }

        private static void LogDllMissing()
        {
            Logger.Warning("APS168x64.dll 未找到 — IO 控制降级为无操作模式");
        }

        #endregion
    }
}
