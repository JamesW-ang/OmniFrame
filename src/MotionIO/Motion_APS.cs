using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace MotionIO
{
    /// <summary>
    /// APS168x64 运动控制卡 P/Invoke 封装
    /// 替代 ApsAxis.h/.cpp + MotionAxis.h/.cpp
    /// </summary>
    public class Motion_APS : Motion
    {
        #region APS168x64 SDK DllImport

        private const string ApsDll = "APS168x64.dll";

        /// <summary>DLL 不可用时自动降级为无操作模式（不抛异常）</summary>
        private static bool _dllMissing;

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_init_board(int board_id);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_close_board(int board_id);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_set_axis_param(int board_id, int axis_id,
            double acc, double dec, double vs, double vm, double vh);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_absolute_move(int board_id, int axis_id,
            double position, int mode);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_relative_move(int board_id, int axis_id,
            double distance, int mode);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_stop_move(int board_id, int axis_id, int mode);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_emg_stop(int board_id);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_get_position(int board_id, int axis_id, ref double position);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_get_axis_state(int board_id, int axis_id, ref int state);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_servo_on(int board_id, int axis_id);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_servo_off(int board_id, int axis_id);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_set_position(int board_id, int axis_id, double position);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_reset_axis_alarm(int board_id, int axis_id);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_get_io_status(int board_id, int axis_id, ref long ioStatus);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_set_soft_limit(int board_id, int axis_id,
            double pel, double mel);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_enable_soft_limit(int board_id, int axis_id, int enable);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_get_motion_profile_time(int board_id, int axis_id,
            double distance, ref double time);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_move_linear_abs(int board_id, int axisCount,
            int[] axisArray, double[] posArray);

        [DllImport(ApsDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int APS_move_linear_rel(int board_id, int axisCount,
            int[] axisArray, double[] posArray);

        #endregion

        private readonly Dictionary<int, AxisParams> _axisParams = new Dictionary<int, AxisParams>();
        private const int DefaultMode = 0; // 0 = 等待完成

        /// <summary>
        /// 轴参数配置
        /// </summary>
        public class AxisParams
        {
            public double Acc;    // 加速度
            public double Dec;    // 减速度
            public double Vs;     // 起始速度
            public double Vm;     // 运行速度
            public double Vh;     // 最高速度

            public AxisParams(double acc, double dec, double vs, double vm, double vh)
            {
                Acc = acc; Dec = dec; Vs = vs; Vm = vm; Vh = vh;
            }
        }

        public Motion_APS(int cardIndex, string name = "APS")
            : base(cardIndex, name, 0, 15) // 16 轴 (0-15)
        {
        }

        #region Motion 基类方法实现

        public override bool Init()
        {
            try
            {
                int ret = APS_init_board(CardIndex);
                if (ret != 0)
                {
                    LogError($"初始化板卡失败, 返回码: {ret}");
                    return false;
                }
                LogInfo($"板卡 {CardIndex} 初始化成功");
                Enable = true;
                return true;
            }
            catch (DllNotFoundException)
            {
                _dllMissing = true;
                LogInfo($"APS168x64.dll 未找到 — 运动控制降级为无操作模式 (板卡 {CardIndex})");
                Enable = true;
                return true;
            }
            catch (Exception ex)
            {
                LogError("初始化板卡异常", ex);
                return false;
            }
        }

        public override bool DeInit()
        {
            if (_dllMissing) { Enable = false; return true; }
            try
            {
                APS_close_board(CardIndex);
                Enable = false;
                LogInfo($"板卡 {CardIndex} 已关闭");
                return true;
            }
            catch (Exception ex)
            {
                LogError("关闭板卡异常", ex);
                return false;
            }
        }

        public override bool AbsMove(int axisNo, double pos, double speed)
        {
            if (_dllMissing) return true;
            if (!Enable) return false;
            try
            {
                // 运动前应用速度
                SetAxisSpeedInternal(axisNo, speed);
                int ret = APS_absolute_move(CardIndex, axisNo, pos, DefaultMode);
                if (ret != 0)
                {
                    LogError($"轴 {axisNo} 绝对运动到 {pos} 失败, 返回码: {ret}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 绝对运动异常", ex);
                return false;
            }
        }

        public override bool RelativeMove(int axisNo, double pos, double speed)
        {
            if (_dllMissing) return true;
            if (!Enable) return false;
            try
            {
                SetAxisSpeedInternal(axisNo, speed);
                int ret = APS_relative_move(CardIndex, axisNo, pos, DefaultMode);
                if (ret != 0)
                {
                    LogError($"轴 {axisNo} 相对运动 {pos} 失败, 返回码: {ret}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 相对运动异常", ex);
                return false;
            }
        }

        public override bool Home(int axisNo, HomeMode mode)
        {
            if (_dllMissing) return true;
            if (!Enable) return false;
            try
            {
                // APS 卡回原点: 向负方向运动直到碰到原点信号
                // mode 参数可用于选择回原点策略
                int ret = APS_absolute_move(CardIndex, axisNo, -999999, DefaultMode);
                return ret == 0;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 回原点异常", ex);
                return false;
            }
        }

        public override bool StopAxis(int axisNo)
        {
            if (_dllMissing) return true;
            if (!Enable) return false;
            int ret = APS_stop_move(CardIndex, axisNo, 0);
            return ret == 0;
        }

        public override bool StopAllAxis()
        {
            if (_dllMissing) return true;
            if (!Enable) return false;
            // 急停所有轴
            int ret = APS_emg_stop(CardIndex);
            return ret == 0;
        }

        public override double GetAxisPos(int axisNo)
        {
            if (_dllMissing) return axisNo * 100.0;
            double pos = 0;
            int ret = APS_get_position(CardIndex, axisNo, ref pos);
            if (ret != 0)
            {
                LogError($"获取轴 {axisNo} 位置失败, 返回码: {ret}");
            }
            return pos;
        }

        public override bool ServoOn(int axisNo)
        {
            if (_dllMissing) return true;
            int ret = APS_servo_on(CardIndex, axisNo);
            return ret == 0;
        }

        public override bool ServoOff(int axisNo)
        {
            if (_dllMissing) return true;
            int ret = APS_servo_off(CardIndex, axisNo);
            return ret == 0;
        }

        public override bool IsAxisMoving(int axisNo)
        {
            if (_dllMissing) return false;
            int state = 0;
            APS_get_axis_state(CardIndex, axisNo, ref state);
            // bit 0: 运动停止标志 (0=运行中, 1=已停止)
            return (state & 1) == 0;
        }

        public override bool IsAxisHomed(int axisNo)
        {
            if (_dllMissing) return true;
            long ioStatus = 0;
            APS_get_io_status(CardIndex, axisNo, ref ioStatus);
            // bit 0: 原点信号 (通常)
            return (ioStatus & 1) != 0;
        }

        public override AxisState GetAxisState(int axisNo)
        {
            if (_dllMissing) return AxisState.Idle;
            int state = 0;
            APS_get_axis_state(CardIndex, axisNo, ref state);
            long ioStatus = 0;
            APS_get_io_status(CardIndex, axisNo, ref ioStatus);

            // 判断报警: bit 4/5 为限位或驱动报警
            if (((ioStatus >> 4) & 1) != 0 || ((ioStatus >> 5) & 1) != 0)
                return AxisState.Alarming;

            // bit 0: 停止标志 → 0=运动中
            if ((state & 1) == 0)
                return AxisState.Moving;

            return AxisState.Idle;
        }

        public override bool ClearAlarm(int axisNo)
        {
            if (_dllMissing) return true;
            int ret = APS_reset_axis_alarm(CardIndex, axisNo);
            return ret == 0;
        }

        public override bool SetSoftLimit(int axisNo, double positive, double negative)
        {
            if (_dllMissing) return true;
            int ret = APS_set_soft_limit(CardIndex, axisNo, positive, negative);
            return ret == 0;
        }

        public override bool EnableSoftLimit(int axisNo, bool enable)
        {
            if (_dllMissing) return true;
            int ret = APS_enable_soft_limit(CardIndex, axisNo, enable ? 1 : 0);
            return ret == 0;
        }

        #endregion

        #region 覆写基类扩展方法

        /// <summary>
        /// 设置轴运动参数 (加速度、减速度、起始速度、运行速度、最高速度)
        /// </summary>
        public override bool SetAxisParam(int axisNo, double acc, double dec, double vs, double vm, double vh)
        {
            if (_dllMissing) { _axisParams[axisNo] = new AxisParams(acc, dec, vs, vm, vh); return true; }
            int ret = APS_set_axis_param(CardIndex, axisNo, acc, dec, vs, vm, vh);
            if (ret == 0)
            {
                _axisParams[axisNo] = new AxisParams(acc, dec, vs, vm, vh);
                return true;
            }
            LogError($"设置轴 {axisNo} 参数失败, 返回码: {ret}");
            return false;
        }

        /// <summary>
        /// 批量设置所有轴参数
        /// </summary>
        public void SetupAllAxes(Dictionary<int, AxisParams> config)
        {
            foreach (var kv in config)
            {
                var p = kv.Value;
                SetAxisParam(kv.Key, p.Acc, p.Dec, p.Vs, p.Vm, p.Vh);
            }
        }

        /// <summary>
        /// 等待轴运动到位 (替代 CountRunTime)
        /// 轮询检查轴到位 (偏差 &lt; PositionTolerance 且停止标志为真)
        /// 同时检测限位传感器
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="timeoutMs">超时 (ms)，默认 10000</param>
        /// <param name="tolerance">到位公差，默认 2.0</param>
        /// <param name="token">取消令牌</param>
        /// <returns>到位成功 true，超时/取消 false</returns>
        public override async Task<bool> WaitAxisDoneAsync(int axisNo, double targetPos,
            int timeoutMs = 10000, double tolerance = 2.0,
            CancellationToken token = default)
        {
            if (_dllMissing) { await Task.Delay(50, token); return true; }
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                token.ThrowIfCancellationRequested();

                double curPos = GetAxisPos(axisNo);
                bool isMoving = IsAxisMoving(axisNo);
                double deviation = Math.Abs(curPos - targetPos);

                if (deviation <= tolerance && !isMoving)
                    return true;

                // 检查限位
                long ioStatus = 0;
                APS_get_io_status(CardIndex, axisNo, ref ioStatus);
                if (((ioStatus >> 1) & 1) != 0 || ((ioStatus >> 2) & 1) != 0)
                {
                    LogError($"轴 {axisNo} 碰到限位传感器");
                    return false;
                }

                await Task.Delay(1, token);
            }

            LogError($"轴 {axisNo} 到位超时 (目标={targetPos}, 当前={GetAxisPos(axisNo)})");
            return false;
        }

        /// <summary>
        /// 多轴批量等待到位
        /// </summary>
        public override async Task<bool> WaitAxesDoneAsync(Dictionary<int, double> targetPositions,
            int timeoutMs = 10000, double tolerance = 2.0,
            CancellationToken token = default)
        {
            if (_dllMissing) { await Task.Delay(50, token); return true; }
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                token.ThrowIfCancellationRequested();

                bool allDone = true;
                foreach (var kv in targetPositions)
                {
                    double curPos = GetAxisPos(kv.Key);
                    bool isMoving = IsAxisMoving(kv.Key);
                    double deviation = Math.Abs(curPos - kv.Value);

                    if (deviation > tolerance || isMoving)
                    {
                        allDone = false;
                        break;
                    }
                }

                if (allDone) return true;

                await Task.Delay(1, token);
            }

            return false;
        }

        /// <summary>
        /// 获取运动完成预估时间 (ms)
        /// 替代 GetMotionProfileTime
        /// </summary>
        public double GetMotionProfileTime(int axisNo, double distance)
        {
            if (_dllMissing) return 100;
            double time = 0;
            APS_get_motion_profile_time(CardIndex, axisNo, distance, ref time);
            return time;
        }

        /// <summary>
        /// 多轴绝对直线插补 (替代 MoveLinearAbsPulse)
        /// </summary>
        public bool AbsLinearMove(int[] axisArray, double[] posArray)
        {
            if (_dllMissing) return true;
            if (!Enable) return false;
            int ret = APS_move_linear_abs(CardIndex, axisArray.Length, axisArray, posArray);
            if (ret != 0)
            {
                LogError($"直线插补绝对运动失败, 返回码: {ret}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 多轴相对直线插补
        /// </summary>
        public bool RelLinearMove(int[] axisArray, double[] posArray)
        {
            if (_dllMissing) return true;
            if (!Enable) return false;
            int ret = APS_move_linear_rel(CardIndex, axisArray.Length, axisArray, posArray);
            if (ret != 0)
            {
                LogError($"直线插补相对运动失败, 返回码: {ret}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取 IO 状态 (限位、原点、报警等)
        /// 替代 GetIOStatus
        /// </summary>
        public long GetIOStatus(int axisNo)
        {
            if (_dllMissing) return 0;
            long ioStatus = 0;
            APS_get_io_status(CardIndex, axisNo, ref ioStatus);
            return ioStatus;
        }

        /// <summary>
        /// 检测停止标志位 (替代 CheckStopFlag)
        /// </summary>
        public bool CheckStopFlag(int axisNo, out bool isStopped)
        {
            if (_dllMissing) { isStopped = true; return true; }
            int state = 0;
            int ret = APS_get_axis_state(CardIndex, axisNo, ref state);
            isStopped = (state & 1) != 0; // bit 0: 停止
            return ret == 0;
        }

        #endregion

        #region Task 8 增强方法

        public override bool MoveVel(int axisNo, double velocity)
        {
            return JogMove(axisNo, Math.Abs(velocity), velocity > 0);
        }

        public override bool SetPos(int axisNo, double pos)
        {
            if (_dllMissing) return true;
            // APS 通过 set_position 设定当前位置
            int ret = APS_set_position(CardIndex, axisNo, pos);
            return ret == 0;
        }

        public override bool SwitchVel(int axisNo, double lowSpeed)
        {
            SetAxisSpeedInternal(axisNo, lowSpeed);
            return true;
        }

        public override bool SwitchAcc(int axisNo, double acc)
        {
            if (_dllMissing) return true;
            if (_axisParams.TryGetValue(axisNo, out var p))
            {
                APS_set_axis_param(CardIndex, axisNo, acc, acc, p.Vs, p.Vm, p.Vh);
            }
            return true;
        }

        #endregion

        #region 内部辅助

        private void SetAxisSpeedInternal(int axisNo, double speed)
        {
            if (_dllMissing) return;
            if (_axisParams.TryGetValue(axisNo, out var p))
            {
                APS_set_axis_param(CardIndex, axisNo, p.Acc, p.Dec, p.Vs, speed, speed);
            }
        }

        #endregion
    }
}
