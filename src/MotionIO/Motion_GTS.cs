using System.Runtime.InteropServices;
using System;
using OmniFrame.Common;

namespace MotionIO
{
    public class Motion_GTS : Motion
    {
        private bool _initialized = false;
        private double[] _currentPos;
        private bool[] _isMoving;
        private bool[] _isHomed;

        public Motion_GTS(int cardIndex, string name, int minAxisNo, int maxAxisNo)
            : base(cardIndex, name, minAxisNo, maxAxisNo)
        {
            int axisCount = maxAxisNo - minAxisNo + 1;
            _currentPos = new double[axisCount];
            _isMoving = new bool[axisCount];
            _isHomed = new bool[axisCount];
        }

        public override bool Init()
        {
            try
            {
                if (_initialized)
                    return true;

                LogInfo("初始化固高运动卡...");
                
                // 模拟初始化过程
                // 实际实现需要调用固高API: mc.GT_OpenCard, mc.GT_Reset 等
                
                _initialized = true;
                LogInfo("固高运动卡初始化成功");
                return true;
            }
            catch (DllNotFoundException ex)
            {
                LogError("运动控制卡DLL未找到", ex);
                return false;
            }
            catch (EntryPointNotFoundException ex)
            {
                LogError("运动控制卡函数入口未找到", ex);
                return false;
            }
            catch (SEHException ex)
            {
                LogError("运动控制卡原生异常", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError("初始化失败", ex);
                return false;
            }
        }

        public override bool DeInit()
        {
            try
            {
                if (!_initialized)
                    return true;

                LogInfo("关闭固高运动卡...");
                
                // 模拟关闭过程
                // 实际实现需要调用固高API: mc.GT_CloseCard
                
                _initialized = false;
                LogInfo("固高运动卡已关闭");
                return true;
            }
            catch (DllNotFoundException ex)
            {
                LogError("运动控制卡DLL未找到", ex);
                return false;
            }
            catch (EntryPointNotFoundException ex)
            {
                LogError("运动控制卡函数入口未找到", ex);
                return false;
            }
            catch (SEHException ex)
            {
                LogError("运动控制卡原生异常", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError("关闭失败", ex);
                return false;
            }
        }

        public override bool AbsMove(int axisNo, double pos, double speed)
        {
            if (!CheckAxisReady(axisNo))
                return false;

            try
            {
                int localAxis = axisNo - MinAxisNo;
                LogInfo($"轴 {axisNo} 绝对定位到 {pos} mm, 速度 {speed} mm/s");
                
                // 模拟运动
                // 实际实现需要调用固高API: mc.GT_PrfTrap, mc.GT_SetVel, mc.GT_Update 等
                
                _isMoving[localAxis] = true;
                _currentPos[localAxis] = pos;
                
                return true;
            }
            catch (DllNotFoundException ex)
            {
                LogError("运动控制卡DLL未找到", ex);
                return false;
            }
            catch (EntryPointNotFoundException ex)
            {
                LogError("运动控制卡函数入口未找到", ex);
                return false;
            }
            catch (SEHException ex)
            {
                LogError("运动控制卡原生异常", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 绝对定位失败", ex);
                return false;
            }
        }

        public override bool RelativeMove(int axisNo, double pos, double speed)
        {
            if (!CheckAxisReady(axisNo))
                return false;

            try
            {
                int localAxis = axisNo - MinAxisNo;
                double targetPos = _currentPos[localAxis] + pos;
                LogInfo($"轴 {axisNo} 相对移动 {pos} mm, 目标位置 {targetPos} mm");
                
                _isMoving[localAxis] = true;
                _currentPos[localAxis] = targetPos;
                
                return true;
            }
            catch (DllNotFoundException ex)
            {
                LogError("运动控制卡DLL未找到", ex);
                return false;
            }
            catch (EntryPointNotFoundException ex)
            {
                LogError("运动控制卡函数入口未找到", ex);
                return false;
            }
            catch (SEHException ex)
            {
                LogError("运动控制卡原生异常", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 相对移动失败", ex);
                return false;
            }
        }

        public override bool Home(int axisNo, HomeMode mode)
        {
            if (!CheckAxisReady(axisNo))
                return false;

            try
            {
                int localAxis = axisNo - MinAxisNo;
                LogInfo($"轴 {axisNo} 开始回原点, 模式: {mode}");
                
                // 模拟回原点
                // 实际实现需要调用固高API: mc.GT_GoHome 等
                
                _isMoving[localAxis] = true;
                _currentPos[localAxis] = 0;
                _isHomed[localAxis] = true;
                
                return true;
            }
            catch (DllNotFoundException ex)
            {
                LogError("运动控制卡DLL未找到", ex);
                return false;
            }
            catch (EntryPointNotFoundException ex)
            {
                LogError("运动控制卡函数入口未找到", ex);
                return false;
            }
            catch (SEHException ex)
            {
                LogError("运动控制卡原生异常", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 回原点失败", ex);
                return false;
            }
        }

        public override bool StopAxis(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            try
            {
                int localAxis = axisNo - MinAxisNo;
                LogInfo($"轴 {axisNo} 停止");
                
                _isMoving[localAxis] = false;
                
                return true;
            }
            catch (DllNotFoundException ex)
            {
                LogError("运动控制卡DLL未找到", ex);
                return false;
            }
            catch (EntryPointNotFoundException ex)
            {
                LogError("运动控制卡函数入口未找到", ex);
                return false;
            }
            catch (SEHException ex)
            {
                LogError("运动控制卡原生异常", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 停止失败", ex);
                return false;
            }
        }

        public override bool StopAllAxis()
        {
            try
            {
                LogInfo("停止所有轴");
                
                for (int i = 0; i < _isMoving.Length; i++)
                {
                    _isMoving[i] = false;
                }
                
                return true;
            }
            catch (DllNotFoundException ex)
            {
                LogError("运动控制卡DLL未找到", ex);
                return false;
            }
            catch (EntryPointNotFoundException ex)
            {
                LogError("运动控制卡函数入口未找到", ex);
                return false;
            }
            catch (SEHException ex)
            {
                LogError("运动控制卡原生异常", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError("停止所有轴失败", ex);
                return false;
            }
        }

        public override double GetAxisPos(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return 0;

            int localAxis = axisNo - MinAxisNo;
            
            // 模拟读取位置
            // 实际实现需要调用固高API: mc.GT_GetPrfPos, mc.GT_GetEncPos 等
            
            return _currentPos[localAxis];
        }

        public override bool ServoOn(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            LogInfo($"轴 {axisNo} 伺服使能开启");
            return true;
        }

        public override bool ServoOff(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            LogInfo($"轴 {axisNo} 伺服使能关闭");
            return true;
        }

        public override bool IsAxisMoving(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            int localAxis = axisNo - MinAxisNo;
            return _isMoving[localAxis];
        }

        public override bool IsAxisHomed(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            int localAxis = axisNo - MinAxisNo;
            return _isHomed[localAxis];
        }

        public override AxisState GetAxisState(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return AxisState.Disabled;

            int localAxis = axisNo - MinAxisNo;
            
            if (_isMoving[localAxis])
                return AxisState.Moving;
            
            return AxisState.Idle;
        }

        public override bool ClearAlarm(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            LogInfo($"轴 {axisNo} 清除报警");
            return true;
        }

        public override bool SetSoftLimit(int axisNo, double positive, double negative)
        {
            if (!IsAxisValid(axisNo))
                return false;

            LogInfo($"轴 {axisNo} 设置软限位: 正限位 {positive}, 负限位 {negative}");
            return true;
        }

        public override bool EnableSoftLimit(int axisNo, bool enable)
        {
            if (!IsAxisValid(axisNo))
                return false;

            LogInfo($"轴 {axisNo} {(enable ? "启用" : "禁用")}软限位");
            return true;
        }

        private bool CheckAxisReady(int axisNo)
        {
            if (!_initialized)
            {
                LogError("运动卡未初始化");
                return false;
            }

            if (!IsAxisValid(axisNo))
            {
                LogError($"轴号 {axisNo} 无效");
                return false;
            }

            return true;
        }
    }
}
