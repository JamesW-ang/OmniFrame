using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Simulation
{
    public class SimulatedIoCtrl : MotionIO.IoCtrl
    {
        private readonly bool[] _inputs;
        private readonly bool[] _outputs;
        private readonly object _lock = new object();
        private readonly string _name;
        private bool _initialized;

        public int InputCount { get; }
        public int OutputCount { get; }
        public int InputPortCount { get; }
        public int OutputPortCount { get; }

        public SimulatedIoCtrl(string name, int inputCount, int outputCount)
        {
            _name = name;
            InputCount = inputCount;
            OutputCount = outputCount;
            _inputs = new bool[inputCount];
            _outputs = new bool[outputCount];
            InputPortCount = (inputCount + 31) / 32;
            OutputPortCount = (outputCount + 31) / 32;
        }

        public override bool Init(object param)
        {
            lock (_lock)
            {
                _initialized = true;
                for (int i = 0; i < _inputs.Length; i++) _inputs[i] = false;
                for (int i = 0; i < _outputs.Length; i++) _outputs[i] = false;
            }
            Logger.Info($"[{_name}] 模拟IO初始化: {InputCount}输入, {OutputCount}输出");
            return true;
        }

        public override bool Close()
        {
            lock (_lock)
            {
                _initialized = false;
            }
            Logger.Info($"[{_name}] 模拟IO已关闭");
            return true;
        }

        private bool IsValidInputIndex(int index) => index >= 0 && index < InputCount;
        private bool IsValidOutputIndex(int index) => index >= 0 && index < OutputCount;

        public override bool ReadInput(int port, out bool value)
        {
            int index = port;
            lock (_lock)
            {
                if (IsValidInputIndex(index))
                {
                    value = _inputs[index];
                    return true;
                }
                value = false;
                return false;
            }
        }

        public override bool ReadInputPort(int port, out int value)
        {
            value = 0;
            int baseIndex = port * 32;
            lock (_lock)
            {
                for (int i = 0; i < 32; i++)
                {
                    int idx = baseIndex + i;
                    if (idx >= InputCount) break;
                    if (_inputs[idx]) value |= (1 << i);
                }
                return true;
            }
        }

        public override bool WriteOutput(int port, bool value)
        {
            int index = port;
            lock (_lock)
            {
                if (!IsValidOutputIndex(index)) return false;
                _outputs[index] = value;
                return true;
            }
        }

        public override bool WriteOutputPort(int port, int value)
        {
            int baseIndex = port * 32;
            lock (_lock)
            {
                for (int i = 0; i < 32; i++)
                {
                    int idx = baseIndex + i;
                    if (idx >= OutputCount) break;
                    _outputs[idx] = (value & (1 << i)) != 0;
                }
                return true;
            }
        }

        public override Dictionary<int, bool> ReadAllInputs()
        {
            lock (_lock)
            {
                return _inputs.Select((v, i) => new { i, v }).ToDictionary(x => x.i, x => x.v);
            }
        }

        public override Dictionary<int, bool> ReadAllOutputs()
        {
            lock (_lock)
            {
                return _outputs.Select((v, i) => new { i, v }).ToDictionary(x => x.i, x => x.v);
            }
        }

        public override bool Reset()
        {
            lock (_lock)
            {
                for (int i = 0; i < _outputs.Length; i++) _outputs[i] = false;
            }
            Logger.Info($"[{_name}] 模拟IO已复位");
            return true;
        }

        /// <summary>仿真模式 — DI 立即到位无需等待</summary>
        public override async System.Threading.Tasks.Task<bool> WaitDIAsync(int diChannel, bool expectedValue,
            int timeoutMs = 10000, string name = "",
            Action<string> onTimeoutWarn = null,
            System.Threading.CancellationToken token = default)
        {
            // 自动将输入置为期望值并立即返回
            lock (_lock)
            {
                if (IsValidInputIndex(diChannel))
                    _inputs[diChannel] = expectedValue;
            }
            await System.Threading.Tasks.Task.Delay(10, token);
            return true;
        }

        /// <summary>仿真模式 — 双 DI 立即到位</summary>
        public override async System.Threading.Tasks.Task<bool> WaitDI2Async(int diCh1, bool exp1, int diCh2, bool exp2,
            int timeoutMs = 10000, string name = "",
            Action<string> onTimeoutWarn = null,
            System.Threading.CancellationToken token = default)
        {
            lock (_lock)
            {
                if (IsValidInputIndex(diCh1))
                    _inputs[diCh1] = exp1;
                if (IsValidInputIndex(diCh2))
                    _inputs[diCh2] = exp2;
            }
            await System.Threading.Tasks.Task.Delay(10, token);
            return true;
        }

        public override string GetError()
        {
            return _initialized ? string.Empty : "IO未初始化";
        }

        // Test injection helpers

        public void SetInput(int index, bool value)
        {
            lock (_lock)
            {
                if (IsValidInputIndex(index))
                    _inputs[index] = value;
            }
        }

        public void SetInput(int port, int pin, bool value)
        {
            SetInput(port * 32 + pin, value);
        }

        public void SetInputPort(int port, int value)
        {
            int baseIndex = port * 32;
            lock (_lock)
            {
                for (int i = 0; i < 32; i++)
                {
                    int idx = baseIndex + i;
                    if (idx >= InputCount) break;
                    _inputs[idx] = (value & (1 << i)) != 0;
                }
            }
        }

        public bool GetOutput(int index)
        {
            lock (_lock)
            {
                return IsValidOutputIndex(index) ? _outputs[index] : false;
            }
        }

        public bool GetOutput(int port, int pin)
        {
            return GetOutput(port * 32 + pin);
        }

        /// <summary>空跑模式预设关键 DI 传感器状态，使 GetDI 直接读取返回合理值</summary>
        public void SetupDryRunDIs()
        {
            lock (_lock)
            {
                // 安全信号 — 正常 (DI 30-32)
                _inputs[31] = true;  // DI_Door
                _inputs[32] = true;  // DI_Safe
                _inputs[30] = false; // DI_EMGStop

                // 料塔/治具 — 存在 (DI 0-1, 34)
                _inputs[0] = true;   // DI_CheckCassel
                _inputs[1] = true;   // DI_CheckTray
                _inputs[34] = false; // DI_CheckJig

                // 底板/玻璃 — 底板存在，无块料 (DI 9, 28-29)
                _inputs[9] = true;   // DI_CheckBottom
                _inputs[28] = true;  // DI_BottomOutCheckBottom
                _inputs[29] = false; // DI_BottomOutCheckBlock

                // 真空 — 默认无 (DI 6)
                _inputs[6] = false;  // DI_BlockGetVacuum
            }
            Logger.Info($"[{_name}] 空跑 DI 预设完成");
        }
    }
}
