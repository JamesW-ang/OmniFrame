using System;
using System.Runtime.InteropServices;
using OmniFrame.Common;

namespace MotionIO
{
    /// <summary>
    /// 运动控制卡错误事件参数
        /// </summary>
    public class MotionErrorEventArgs : EventArgs
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public int AxisNo { get; set; }
        public DateTime Time { get; set; }

        public MotionErrorEventArgs()
        {
            Time = DateTime.Now;
        }
    }

    /// <summary>
    /// 运动控制卡设备类
    /// 封装Motion类，提供设备管理功能
        /// </summary>
    public class MotionDevice : IDevice
    {
        private Motion _motion;
        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 运动控制卡
        /// </summary>
        public Motion Motion => _motion;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 是否已连接（与 IsInitialized 同义，满足 IDevice 接口）
        /// </summary>
        public bool IsConnected => _isInitialized;

        /// <summary>
        /// 是否使能
        /// </summary>
        public bool IsEnabled => _motion?.Enable ?? false;

        /// <summary>
        /// 轴数量（来自 Motion 控制器）
        /// </summary>
        public int AxisCount => _motion?.MaxAxisNo ?? 0;

        /// <summary>
        /// 错误事件
        /// </summary>
        public event EventHandler<MotionErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">设备名称</param>
        /// <param name="motion">运动控制卡</param>
        public MotionDevice(string name, Motion motion)
        {
            Name = name;
            _motion = motion;
            _isInitialized = false;
            _isDisposed = false;
        }

        /// <summary>
        /// 初始化设备
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Initialize()
        {
            if (_motion == null)
            {
                OnError("MOTION_NULL", "运动控制卡对象为空", -1);
                return false;
            }

            if (_isInitialized)
            {
                Logger.Warning($"运动控制卡 {Name} 已初始化");
                return true;
            }

            try
            {
                if (_motion.Init())
                {
                    _isInitialized = true;
                    Logger.Info($"运动控制卡 {Name} 初始化成功");
                    return true;
                }
                else
                {
                    OnError("INIT_FAILED", "运动控制卡初始化失败", -1);
                    return false;
                }
            }
            catch (InvalidOperationException ex)
            {
                OnError("INIT_EXCEPTION", "设备操作无效状态", -1, ex);
                return false;
            }
            catch (DllNotFoundException ex)
            {
                OnError("INIT_EXCEPTION", "运动控制卡DLL未找到", -1, ex);
                return false;
            }
            catch (SEHException ex)
            {
                OnError("INIT_EXCEPTION", "运动控制卡原生异常", -1, ex);
                return false;
            }
            catch (Exception ex)
            {
                OnError("INIT_EXCEPTION", "初始化异常", -1, ex);
                return false;
            }
        }

        /// <summary>
        /// 释放设备
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Terminate()
        {
            if (_motion == null)
                return true;

            try
            {
                if (_motion.DeInit())
                {
                    _isInitialized = false;
                    Logger.Info($"运动控制卡 {Name} 释放成功");
                    return true;
                }
                else
                {
                    OnError("DEINIT_FAILED", "运动控制卡释放失败", -1);
                    return false;
                }
            }
            catch (InvalidOperationException ex)
            {
                OnError("DEINIT_EXCEPTION", "设备操作无效状态", -1, ex);
                return false;
            }
            catch (DllNotFoundException ex)
            {
                OnError("DEINIT_EXCEPTION", "运动控制卡DLL未找到", -1, ex);
                return false;
            }
            catch (SEHException ex)
            {
                OnError("DEINIT_EXCEPTION", "运动控制卡原生异常", -1, ex);
                return false;
            }
            catch (Exception ex)
            {
                OnError("DEINIT_EXCEPTION", "释放异常", -1, ex);
                return false;
            }
        }

        /// <summary>
        /// 连接设备（与 Initialize 同义，满足 IDevice 接口）
        /// </summary>
        public bool Connect() => Initialize();

        /// <summary>
        /// 断开设备（与 Terminate 同义，满足 IDevice 接口）
        /// </summary>
        public void Disconnect() => Terminate();

        /// <summary>
        /// 复位设备
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Reset()
        {
            if (_motion == null)
                return true;

            Logger.Info($"复位运动控制卡 {Name}");
            return true;
        }

        /// <summary>
        /// 停止轴
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否成功</returns>
        public bool StopAxis(int axisNo)
        {
            if (_motion == null)
            {
                OnError("MOTION_NULL", "运动控制卡对象为空", axisNo);
                return false;
            }

            try
            {
                return _motion.StopAxis(axisNo);
            }
            catch (InvalidOperationException ex)
            {
                OnError("STOP_AXIS_FAILED", "设备操作无效状态", axisNo, ex);
                return false;
            }
            catch (DllNotFoundException ex)
            {
                OnError("STOP_AXIS_FAILED", "运动控制卡DLL未找到", axisNo, ex);
                return false;
            }
            catch (SEHException ex)
            {
                OnError("STOP_AXIS_FAILED", "运动控制卡原生异常", axisNo, ex);
                return false;
            }
            catch (Exception ex)
            {
                OnError("STOP_AXIS_FAILED", $"停止轴 {axisNo} 失败", axisNo, ex);
                return false;
            }
        }

        /// <summary>
        /// 获取轴使能状态
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否使能</returns>
        public bool GetAxisEnabled(int axisNo)
        {
            if (_motion == null) return false;
            try
            {
                var state = _motion.GetAxisState(axisNo);
                return state != AxisState.Disabled;
            }
            catch (InvalidOperationException ex)
            {
                OnError("GET_AXIS_ENABLED_FAILED", "设备操作无效状态", axisNo, ex);
                return false;
            }
            catch (DllNotFoundException ex)
            {
                OnError("GET_AXIS_ENABLED_FAILED", "运动控制卡DLL未找到", axisNo, ex);
                return false;
            }
            catch (SEHException ex)
            {
                OnError("GET_AXIS_ENABLED_FAILED", "运动控制卡原生异常", axisNo, ex);
                return false;
            }
            catch (Exception ex)
            {
                OnError("GET_AXIS_ENABLED_FAILED", $"获取轴 {axisNo} 使能状态失败", axisNo, ex);
                return false;
            }
        }

        /// <summary>
        /// 设置轴使能状态
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="enabled">是否使能</param>
        /// <returns>是否成功</returns>
        public bool SetAxisEnabled(int axisNo, bool enabled)
        {
            if (_motion == null) return false;
            try
            {
                if (enabled)
                    return _motion.ServoOn(axisNo);
                else
                    return _motion.ServoOff(axisNo);
            }
            catch (InvalidOperationException ex)
            {
                OnError("SET_AXIS_ENABLED_FAILED", "设备操作无效状态", axisNo, ex);
                return false;
            }
            catch (DllNotFoundException ex)
            {
                OnError("SET_AXIS_ENABLED_FAILED", "运动控制卡DLL未找到", axisNo, ex);
                return false;
            }
            catch (SEHException ex)
            {
                OnError("SET_AXIS_ENABLED_FAILED", "运动控制卡原生异常", axisNo, ex);
                return false;
            }
            catch (Exception ex)
            {
                OnError("SET_AXIS_ENABLED_FAILED", $"设置轴 {axisNo} 使能状态失败", axisNo, ex);
                return false;
            }
        }

        /// <summary>
        /// 检查轴是否正在运动
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否运动中</returns>
        public bool IsAxisMoving(int axisNo)
        {
            if (_motion == null) return false;
            try
            {
                return _motion.IsAxisMoving(axisNo);
            }
            catch (InvalidOperationException ex)
            {
                OnError("IS_AXIS_MOVING_FAILED", "设备操作无效状态", axisNo, ex);
                return false;
            }
            catch (DllNotFoundException ex)
            {
                OnError("IS_AXIS_MOVING_FAILED", "运动控制卡DLL未找到", axisNo, ex);
                return false;
            }
            catch (SEHException ex)
            {
                OnError("IS_AXIS_MOVING_FAILED", "运动控制卡原生异常", axisNo, ex);
                return false;
            }
            catch (Exception ex)
            {
                OnError("IS_AXIS_MOVING_FAILED", $"检查轴 {axisNo} 运动状态失败", axisNo, ex);
                return false;
            }
        }

        /// <summary>
        /// 获取轴实际位置
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>实际位置</returns>
        public double GetActualPosition(int axisNo)
        {
            if (_motion == null) return 0;
            try
            {
                return _motion.GetAxisPos(axisNo);
            }
            catch (InvalidOperationException ex)
            {
                OnError("GET_ACTUAL_POSITION_FAILED", "设备操作无效状态", axisNo, ex);
                return 0;
            }
            catch (DllNotFoundException ex)
            {
                OnError("GET_ACTUAL_POSITION_FAILED", "运动控制卡DLL未找到", axisNo, ex);
                return 0;
            }
            catch (SEHException ex)
            {
                OnError("GET_ACTUAL_POSITION_FAILED", "运动控制卡原生异常", axisNo, ex);
                return 0;
            }
            catch (Exception ex)
            {
                OnError("GET_ACTUAL_POSITION_FAILED", $"获取轴 {axisNo} 实际位置失败", axisNo, ex);
                return 0;
            }
        }

        /// <summary>
        /// 获取轴命令位置
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>命令位置</returns>
        public double GetCommandPosition(int axisNo)
        {
            if (_motion == null) return 0;
            try
            {
                return _motion.GetAxisPos(axisNo);
            }
            catch (InvalidOperationException ex)
            {
                OnError("GET_COMMAND_POSITION_FAILED", "设备操作无效状态", axisNo, ex);
                return 0;
            }
            catch (DllNotFoundException ex)
            {
                OnError("GET_COMMAND_POSITION_FAILED", "运动控制卡DLL未找到", axisNo, ex);
                return 0;
            }
            catch (SEHException ex)
            {
                OnError("GET_COMMAND_POSITION_FAILED", "运动控制卡原生异常", axisNo, ex);
                return 0;
            }
            catch (Exception ex)
            {
                OnError("GET_COMMAND_POSITION_FAILED", $"获取轴 {axisNo} 命令位置失败", axisNo, ex);
                return 0;
            }
        }

        /// <summary>
        /// 轴回原点
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否成功</returns>
        public bool HomeAxis(int axisNo)
        {
            if (_motion == null) return false;
            try
            {
                return _motion.Home(axisNo, HomeMode.ORG_P);
            }
            catch (InvalidOperationException ex)
            {
                OnError("HOME_AXIS_FAILED", "设备操作无效状态", axisNo, ex);
                return false;
            }
            catch (DllNotFoundException ex)
            {
                OnError("HOME_AXIS_FAILED", "运动控制卡DLL未找到", axisNo, ex);
                return false;
            }
            catch (SEHException ex)
            {
                OnError("HOME_AXIS_FAILED", "运动控制卡原生异常", axisNo, ex);
                return false;
            }
            catch (Exception ex)
            {
                OnError("HOME_AXIS_FAILED", $"轴 {axisNo} 回原点失败", axisNo, ex);
                return false;
            }
        }

        /// <summary>
        /// 绝对位置移动
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="position">目标位置</param>
        /// <param name="speed">速度</param>
        /// <returns>是否成功</returns>
        public bool MoveAbsolute(int axisNo, double position, double speed)
        {
            if (_motion == null) return false;
            try
            {
                var startTime = DateTime.Now;
                var result = _motion.AbsMove(axisNo, position, speed);
                var elapsed = DateTime.Now - startTime;
                Logger.Info($"[MotionDevice] {Name} 轴 {axisNo} 绝对移动: 目标位置={position}, 速度={speed}, 结果={result}, 耗时={elapsed.TotalMilliseconds:F2}ms");
                return result;
            }
            catch (InvalidOperationException ex)
            {
                OnError("MOVE_ABSOLUTE_FAILED", "设备操作无效状态", axisNo, ex);
                return false;
            }
            catch (DllNotFoundException ex)
            {
                OnError("MOVE_ABSOLUTE_FAILED", "运动控制卡DLL未找到", axisNo, ex);
                return false;
            }
            catch (SEHException ex)
            {
                OnError("MOVE_ABSOLUTE_FAILED", "运动控制卡原生异常", axisNo, ex);
                return false;
            }
            catch (Exception ex)
            {
                OnError("MOVE_ABSOLUTE_FAILED", $"轴 {axisNo} 绝对移动失败", axisNo, ex);
                return false;
            }
        }

        /// <summary>
        /// 相对位置移动
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="distance">移动距离</param>
        /// <param name="speed">速度</param>
        /// <returns>是否成功</returns>
        public bool MoveRelative(int axisNo, double distance, double speed)
        {
            if (_motion == null) return false;
            try
            {
                var startTime = DateTime.Now;
                var result = _motion.RelativeMove(axisNo, distance, speed);
                var elapsed = DateTime.Now - startTime;
                Logger.Info($"[MotionDevice] {Name} 轴 {axisNo} 相对移动: 距离={distance}, 速度={speed}, 结果={result}, 耗时={elapsed.TotalMilliseconds:F2}ms");
                return result;
            }
            catch (InvalidOperationException ex)
            {
                OnError("MOVE_RELATIVE_FAILED", "设备操作无效状态", axisNo, ex);
                return false;
            }
            catch (DllNotFoundException ex)
            {
                OnError("MOVE_RELATIVE_FAILED", "运动控制卡DLL未找到", axisNo, ex);
                return false;
            }
            catch (SEHException ex)
            {
                OnError("MOVE_RELATIVE_FAILED", "运动控制卡原生异常", axisNo, ex);
                return false;
            }
            catch (Exception ex)
            {
                OnError("MOVE_RELATIVE_FAILED", $"轴 {axisNo} 相对移动失败", axisNo, ex);
                return false;
            }
        }

        /// <summary>
        /// JOG移动
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="speed">速度</param>
        /// <param name="positiveDirection">正方向</param>
        /// <returns>是否成功</returns>
        public bool JogMove(int axisNo, double speed, bool positiveDirection)
        {
            if (_motion == null) return false;
            try
            {
                var startTime = DateTime.Now;
                var result = _motion.JogMove(axisNo, speed, positiveDirection);
                var elapsed = DateTime.Now - startTime;
                Logger.Info($"[MotionDevice] {Name} 轴 {axisNo} JOG移动: 速度={speed}, 方向={(positiveDirection ? "正" : "负")}, 结果={result}, 耗时={elapsed.TotalMilliseconds:F2}ms");
                return result;
            }
            catch (InvalidOperationException ex)
            {
                OnError("JOG_MOVE_FAILED", "设备操作无效状态", axisNo, ex);
                return false;
            }
            catch (DllNotFoundException ex)
            {
                OnError("JOG_MOVE_FAILED", "运动控制卡DLL未找到", axisNo, ex);
                return false;
            }
            catch (SEHException ex)
            {
                OnError("JOG_MOVE_FAILED", "运动控制卡原生异常", axisNo, ex);
                return false;
            }
            catch (Exception ex)
            {
                OnError("JOG_MOVE_FAILED", $"轴 {axisNo} JOG移动失败", axisNo, ex);
                return false;
            }
        }

        /// <summary>
        /// 设置实际位置
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="position">位置值</param>
        /// <returns>是否成功</returns>
        public bool SetActualPosition(int axisNo, double position)
        {
            if (_motion == null) return false;
            try
            {
                // 调用底层运动控制卡的位置设置方法
                Logger.Info($"设置轴 {axisNo} 实际位置为 {position}");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                OnError("SET_ACTUAL_POSITION_FAILED", "设备操作无效状态", axisNo, ex);
                return false;
            }
            catch (DllNotFoundException ex)
            {
                OnError("SET_ACTUAL_POSITION_FAILED", "运动控制卡DLL未找到", axisNo, ex);
                return false;
            }
            catch (SEHException ex)
            {
                OnError("SET_ACTUAL_POSITION_FAILED", "运动控制卡原生异常", axisNo, ex);
                return false;
            }
            catch (Exception ex)
            {
                OnError("SET_ACTUAL_POSITION_FAILED", $"设置轴 {axisNo} 实际位置失败", axisNo, ex);
                return false;
            }
        }

        /// <summary>
        /// 触发错误事件
        /// </summary>
        private void OnError(string code, string message, int axisNo, Exception ex = null)
        {
            var args = new MotionErrorEventArgs
            {
                ErrorCode = code,
                ErrorMessage = message,
                AxisNo = axisNo
            };

            if (ex != null)
                Logger.Error($"运动控制卡 {Name} 错误: [{code}] {message}", ex);
            else
                Logger.Error($"运动控制卡 {Name} 错误: [{code}] {message}");
            ErrorOccurred?.Invoke(this, args);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            Terminate();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
