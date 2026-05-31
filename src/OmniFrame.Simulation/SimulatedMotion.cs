using System;
using System.Threading;
using OmniFrame.Common;

namespace OmniFrame.Simulation
{
    public class SimulatedMotion : MotionIO.Motion
    {
        private readonly double[] _positions;
        private readonly MotionIO.AxisState[] _states;
        private readonly bool[] _homed;
        private readonly bool[] _servoOn;
        private readonly double[] _softLimitPositive;
        private readonly double[] _softLimitNegative;
        private readonly bool[] _softLimitEnabled;
        private readonly object _lock = new object();
        private readonly int _axisCount;

        public int SimulationDelayMs { get; set; } = 0;

        public SimulatedMotion(string name, int axisCount)
            : base(0, name, 0, axisCount - 1)
        {
            _axisCount = axisCount;
            _positions = new double[axisCount];
            _states = new MotionIO.AxisState[axisCount];
            _homed = new bool[axisCount];
            _servoOn = new bool[axisCount];
            _softLimitPositive = new double[axisCount];
            _softLimitNegative = new double[axisCount];
            _softLimitEnabled = new bool[axisCount];

            for (int i = 0; i < axisCount; i++)
            {
                _states[i] = MotionIO.AxisState.Disabled;
                _softLimitPositive[i] = double.MaxValue;
                _softLimitNegative[i] = double.MinValue;
            }
        }

        private bool ValidateAxis(int axisNo)
        {
            if (axisNo < 0 || axisNo >= _axisCount)
            {
                LogError($"轴号 {axisNo} 无效，有效范围: 0-{_axisCount - 1}");
                return false;
            }
            if (!_servoOn[axisNo])
            {
                LogError($"轴 {axisNo} 伺服未使能");
                return false;
            }
            return true;
        }

        public override bool Init()
        {
            lock (_lock)
            {
                Enable = true;
                LogInfo($"初始化完成，{_axisCount} 轴");
                return true;
            }
        }

        public override bool DeInit()
        {
            lock (_lock)
            {
                StopAllAxis();
                Enable = false;
                LogInfo("反初始化完成");
                return true;
            }
        }

        public override bool AbsMove(int axisNo, double pos, double speed)
        {
            lock (_lock)
            {
                if (!ValidateAxis(axisNo)) return false;

                if (_softLimitEnabled[axisNo])
                {
                    if (pos > _softLimitPositive[axisNo] || pos < _softLimitNegative[axisNo])
                    {
                        LogError($"轴 {axisNo} 目标位置 {pos} 超出软限位 [{_softLimitNegative[axisNo]}, {_softLimitPositive[axisNo]}]");
                        return false;
                    }
                }

                _states[axisNo] = MotionIO.AxisState.Moving;
            }

            if (SimulationDelayMs > 0)
                Thread.Sleep(SimulationDelayMs);

            lock (_lock)
            {
                _positions[axisNo] = pos;
                _states[axisNo] = MotionIO.AxisState.Idle;
            }
            LogInfo($"轴 {axisNo} 绝对运动到 {pos}，速度 {speed}");
            return true;
        }

        public override bool RelativeMove(int axisNo, double pos, double speed)
        {
            double target;
            lock (_lock)
            {
                if (!ValidateAxis(axisNo)) return false;
                target = _positions[axisNo] + pos;
            }
            return AbsMove(axisNo, target, speed);
        }

        public override bool Home(int axisNo, MotionIO.HomeMode mode)
        {
            lock (_lock)
            {
                if (!ValidateAxis(axisNo)) return false;
                _states[axisNo] = MotionIO.AxisState.Homing;
            }

            if (SimulationDelayMs > 0)
                Thread.Sleep(SimulationDelayMs * 2);

            lock (_lock)
            {
                _positions[axisNo] = 0;
                _homed[axisNo] = true;
                _states[axisNo] = MotionIO.AxisState.Idle;
            }
            LogInfo($"轴 {axisNo} 回原点完成，模式 {mode}");
            return true;
        }

        public override bool StopAxis(int axisNo)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return false;
                _states[axisNo] = MotionIO.AxisState.Idle;
            }
            LogInfo($"轴 {axisNo} 已停止");
            return true;
        }

        public override bool StopAllAxis()
        {
            lock (_lock)
            {
                for (int i = 0; i < _axisCount; i++)
                    _states[i] = MotionIO.AxisState.Idle;
            }
            LogInfo("所有轴已停止");
            return true;
        }

        public override double GetAxisPos(int axisNo)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return 0;
                return _positions[axisNo];
            }
        }

        public override bool ServoOn(int axisNo)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return false;
                _servoOn[axisNo] = true;
                _states[axisNo] = MotionIO.AxisState.Idle;
            }
            LogInfo($"轴 {axisNo} 伺服使能");
            return true;
        }

        public override bool ServoOff(int axisNo)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return false;
                _servoOn[axisNo] = false;
                _states[axisNo] = MotionIO.AxisState.Disabled;
            }
            LogInfo($"轴 {axisNo} 伺服失能");
            return true;
        }

        public override bool IsAxisMoving(int axisNo)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return false;
                return _states[axisNo] == MotionIO.AxisState.Moving || _states[axisNo] == MotionIO.AxisState.Homing;
            }
        }

        public override bool IsAxisHomed(int axisNo)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return false;
                return _homed[axisNo];
            }
        }

        public override MotionIO.AxisState GetAxisState(int axisNo)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return MotionIO.AxisState.Disabled;
                return _states[axisNo];
            }
        }

        public override bool ClearAlarm(int axisNo)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return false;
                if (_states[axisNo] == MotionIO.AxisState.Alarming)
                    _states[axisNo] = MotionIO.AxisState.Idle;
            }
            LogInfo($"轴 {axisNo} 报警已清除");
            return true;
        }

        public override bool SetSoftLimit(int axisNo, double positive, double negative)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return false;
                _softLimitPositive[axisNo] = positive;
                _softLimitNegative[axisNo] = negative;
            }
            LogInfo($"轴 {axisNo} 软限位设置: [{negative}, {positive}]");
            return true;
        }

        public override bool EnableSoftLimit(int axisNo, bool enable)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return false;
                _softLimitEnabled[axisNo] = enable;
            }
            LogInfo($"轴 {axisNo} 软限位 {(enable ? "使能" : "禁用")}");
            return true;
        }

        public override bool JogMove(int axisNo, double speed, bool positiveDirection)
        {
            if (!ValidateAxis(axisNo)) return false;
            double delta = (positiveDirection ? 1 : -1) * speed * 0.01;
            lock (_lock)
            {
                _positions[axisNo] += delta;
                _states[axisNo] = MotionIO.AxisState.Moving;
            }
            Thread.Sleep(Math.Max(1, SimulationDelayMs / 5));
            lock (_lock) { _states[axisNo] = MotionIO.AxisState.Idle; }
            LogInfo($"轴 {axisNo} JOG {(positiveDirection ? "正" : "负")}方向，速度 {speed}");
            return true;
        }

        public override bool SetAxisParam(int axisNo, double acc, double dec, double vs, double vm, double vh)
        {
            LogInfo($"轴 {axisNo} 参数设置: acc={acc}, dec={dec}, vs={vs}, vm={vm}, vh={vh}");
            return true;
        }

        public override bool SetAxisSpeed(int axisNo, double speed) => true;

        public override bool SetAxisAccDec(int axisNo, double acc, double dec) => true;

        public override bool AbsLinearMove(int[] axisArray, double[] posArray, double speed, double acc, double dec)
        {
            if (axisArray == null || posArray == null || axisArray.Length != posArray.Length) return false;
            for (int i = 0; i < axisArray.Length; i++)
                if (!ValidateAxis(axisArray[i])) return false;
            Thread.Sleep(SimulationDelayMs);
            lock (_lock)
            {
                for (int i = 0; i < axisArray.Length; i++)
                    _positions[axisArray[i]] = posArray[i];
            }
            LogInfo($"多轴直线插补运动完成");
            return true;
        }

        public override bool RelativeLinearMove(int[] axisArray, double[] posArray, double speed, double acc, double dec)
        {
            if (axisArray == null || posArray == null || axisArray.Length != posArray.Length) return false;
            double[] targets = new double[axisArray.Length];
            lock (_lock)
            {
                for (int i = 0; i < axisArray.Length; i++)
                    targets[i] = _positions[axisArray[i]] + posArray[i];
            }
            return AbsLinearMove(axisArray, targets, speed, acc, dec);
        }

        public override bool AbsArcMove(int[] axisArray, double[] centerArray, double angle, double speed)
        {
            Thread.Sleep(SimulationDelayMs);
            LogInfo($"圆弧插补运动完成");
            return true;
        }

        public override bool RelativeArcMove(int[] axisArray, double[] centerOffsetArray, double angle, double speed)
        {
            Thread.Sleep(SimulationDelayMs);
            LogInfo($"相对圆弧插补运动完成");
            return true;
        }

        public override bool MoveVel(int axisNo, double velocity)
        {
            lock (_lock)
            {
                if (!ValidateAxis(axisNo)) return false;
                _states[axisNo] = MotionIO.AxisState.Moving;
            }
            Thread.Sleep(SimulationDelayMs);
            lock (_lock)
            {
                _positions[axisNo] += velocity * 0.01;
                _states[axisNo] = MotionIO.AxisState.Idle;
            }
            LogInfo($"轴 {axisNo} 速度模式运动，速度 {velocity}");
            return true;
        }

        public override bool SetPos(int axisNo, double pos)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return false;
                _positions[axisNo] = pos;
            }
            LogInfo($"轴 {axisNo} 位置设定为 {pos}");
            return true;
        }

        public override bool CheckStopFlag(int axisNo)
        {
            lock (_lock)
            {
                if (axisNo < 0 || axisNo >= _axisCount) return true;
                return _states[axisNo] != MotionIO.AxisState.Moving;
            }
        }

        public override bool CheckAbnormalStop(int axisNo)
        {
            return GetAxisState(axisNo) == MotionIO.AxisState.Alarming;
        }

        public override double PulsePerMM { get; set; } = 1000.0;
    }
}
