using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace MotionIO
{
    /// <summary>
    /// 回原点模式枚举
    /// 功能说明：定义各种回原点的方式，包括原点信号、限位信号、EZ信号等组合
        /// </summary>
    public enum HomeMode
    {
        ORG_P,          // 原点信号+正方向
        ORG_N,          // 原点信号+负方向
        PEL,            // 正限位为原点
        MEL,            // 负限位为原点
        EZ_PEL,         // EZ信号+正限位
        EZ_MEL,         // EZ信号+负限位
        ORG_P_EZ,       // 原点+正方向+EZ
        ORG_N_EZ,       // 原点+负方向+EZ
        GanTry_MEL,     // 龙门负限位回原点
        GanTry_PEL,     // 龙门正限位回原点
        BUS_BASE        // 总线型基础值
    }

    /// <summary>
    /// 轴状态枚举
    /// 功能说明：定义轴的各种状态，包括空闲、运动中、回原点中、报警中、未使能
        /// </summary>
    public enum AxisState
    {
        Idle,           // 空闲
        Moving,         // 运动中
        Homing,         // 回原点中
        Alarming,       // 报警中
        Disabled        // 未使能
    }

    /// <summary>
    /// 运动控制抽象基类
    /// 设计介绍：
    /// 4. 实现轴号映射，支持多轴控制
    /// 5. 提供虚方法，允许子类选择性实现高级功能
    /// 6. 提供统一的日志记录接口，便于问题排查
    /// 7. 实现轴号有效性检查，防止非法操作
        /// </summary>
    public abstract class Motion
    {
        /// <summary>
        /// 是否使能
        /// </summary>
        public bool Enable { get; protected set; }

        /// <summary>
        /// 卡索引
        /// </summary>
        public int CardIndex { get; protected set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 最小轴号
        /// </summary>
        public int MinAxisNo { get; protected set; }

        /// <summary>
        /// 最大轴号
        /// </summary>
        public int MaxAxisNo { get; protected set; }

        /// <summary>
        /// 轴号映射表
        /// </summary>
        public Dictionary<int, int> BoardMap { get; protected set; } = new Dictionary<int, int>();

        /// <summary>
        /// 构造函数
        /// 功能说明：初始化运动控制对象，设置卡索引、名称和轴号范围
        /// </summary>
        /// <param name="cardIndex">卡索引</param>
        /// <param name="name">名称</param>
        /// <param name="minAxisNo">最小轴号</param>
        /// <param name="maxAxisNo">最大轴号</param>
        protected Motion(int cardIndex, string name, int minAxisNo, int maxAxisNo)
        {
            CardIndex = cardIndex;
            Name = name;
            MinAxisNo = minAxisNo;
            MaxAxisNo = maxAxisNo;
            Enable = false;
        }

        /// <summary>
        /// 设置使能状态
        /// 功能说明：设置运动控制卡的使能状态
        /// </summary>
        /// <param name="enable">是否使能</param>
        public void SetEnable(bool enable)
        {
            Enable = enable;
        }

        /// <summary>
        /// 初始化运动控制卡
        /// 功能说明：初始化运动控制卡，准备进行运动控制
        /// 设计模式：抽象方法，由子类实现具体的初始化逻辑
        /// </summary>
        /// <returns>是否初始化成功</returns>
        public abstract bool Init();

        /// <summary>
        /// 反初始化运动控制卡
        /// 功能说明：释放运动控制卡资源
        /// 设计模式：抽象方法，由子类实现具体的反初始化逻辑
        /// </summary>
        /// <returns>是否反初始化成功</returns>
        public abstract bool DeInit();

        /// <summary>
        /// 绝对运动
        /// 功能说明：控制指定轴移动到绝对位置
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="pos">目标位置</param>
        /// <param name="speed">运动速度</param>
        /// <returns>是否执行成功</returns>
        public abstract bool AbsMove(int axisNo, double pos, double speed);

        /// <summary>
        /// 相对运动
        /// 功能说明：控制指定轴移动相对距离
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="pos">相对距离</param>
        /// <param name="speed">运动速度</param>
        /// <returns>是否执行成功</returns>
        public abstract bool RelativeMove(int axisNo, double pos, double speed);

        /// <summary>
        /// 回原点
        /// 功能说明：控制指定轴回原点
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="mode">回原点模式</param>
        /// <returns>是否执行成功</returns>
        public abstract bool Home(int axisNo, HomeMode mode);

        /// <summary>
        /// 停止指定轴
        /// 功能说明：停止指定轴的运动
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否执行成功</returns>
        public abstract bool StopAxis(int axisNo);

        /// <summary>
        /// 停止所有轴
        /// 功能说明：停止所有轴的运动
        /// </summary>
        /// <returns>是否执行成功</returns>
        public abstract bool StopAllAxis();

        /// <summary>
        /// 获取轴位置
        /// 功能说明：获取指定轴的当前位置
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>轴位置</returns>
        public abstract double GetAxisPos(int axisNo);

        /// <summary>
        /// 伺服使能
        /// 功能说明：使能指定轴的伺服
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否执行成功</returns>
        public abstract bool ServoOn(int axisNo);

        /// <summary>
        /// 伺服失能
        /// 功能说明：失能指定轴的伺服
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否执行成功</returns>
        public abstract bool ServoOff(int axisNo);

        /// <summary>
        /// 检查轴是否运动中
        /// 功能说明：检查指定轴是否正在运动
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否运动中</returns>
        public abstract bool IsAxisMoving(int axisNo);

        /// <summary>
        /// 检查轴是否已回原点
        /// 功能说明：检查指定轴是否已经回原点
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否已回原点</returns>
        public abstract bool IsAxisHomed(int axisNo);

        /// <summary>
        /// 获取轴状态
        /// 功能说明：获取指定轴的当前状态
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>轴状态</returns>
        public abstract AxisState GetAxisState(int axisNo);

        /// <summary>
        /// 清除报警
        /// 功能说明：清除指定轴的报警状态
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否执行成功</returns>
        public abstract bool ClearAlarm(int axisNo);

        /// <summary>
        /// 设置软限位
        /// 功能说明：设置指定轴的软限位
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="positive">正限位</param>
        /// <param name="negative">负限位</param>
        /// <returns>是否执行成功</returns>
        public abstract bool SetSoftLimit(int axisNo, double positive, double negative);

        /// <summary>
        /// 使能软限位
        /// 功能说明：使能或禁用指定轴的软限位
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="enable">是否使能</param>
        /// <returns>是否执行成功</returns>
        public abstract bool EnableSoftLimit(int axisNo, bool enable);

        /// <summary>
        /// 绝对直线插补运动
        /// 功能说明：多轴直线插补运动到绝对位置
        /// 设计模式：虚方法，子类可以选择性实现
        /// </summary>
        /// <param name="axisArray">轴号数组</param>
        /// <param name="posArray">位置数组</param>
        /// <param name="speed">速度</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        /// <returns>是否执行成功</returns>
        public virtual bool AbsLinearMove(int[] axisArray, double[] posArray, double speed, double acc, double dec)
        {
            Logger.Warning($"{Name} 不支持直线插补运动");
            return false;
        }

        /// <summary>
        /// 相对直线插补运动
        /// 功能说明：多轴直线插补运动相对距离
        /// 设计模式：虚方法，子类可以选择性实现
        /// </summary>
        /// <param name="axisArray">轴号数组</param>
        /// <param name="posArray">相对距离数组</param>
        /// <param name="speed">速度</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        /// <returns>是否执行成功</returns>
        public virtual bool RelativeLinearMove(int[] axisArray, double[] posArray, double speed, double acc, double dec)
        {
            Logger.Warning($"{Name} 不支持直线插补运动");
            return false;
        }

        /// <summary>
        /// 绝对圆弧插补运动
        /// 功能说明：多轴圆弧插补运动到绝对位置
        /// 设计模式：虚方法，子类可以选择性实现
        /// </summary>
        /// <param name="axisArray">轴号数组</param>
        /// <param name="centerArray">圆心坐标数组</param>
        /// <param name="angle">角度</param>
        /// <param name="speed">速度</param>
        /// <returns>是否执行成功</returns>
        public virtual bool AbsArcMove(int[] axisArray, double[] centerArray, double angle, double speed)
        {
            Logger.Warning($"{Name} 不支持圆弧插补运动");
            return false;
        }

        /// <summary>
        /// 相对圆弧插补运动
        /// 功能说明：多轴圆弧插补运动相对距离
        /// 设计模式：虚方法，子类可以选择性实现
        /// </summary>
        /// <param name="axisArray">轴号数组</param>
        /// <param name="centerOffsetArray">圆心相对偏移数组</param>
        /// <param name="angle">角度</param>
        /// <param name="speed">速度</param>
        /// <returns>是否执行成功</returns>
        public virtual bool RelativeArcMove(int[] axisArray, double[] centerOffsetArray, double angle, double speed)
        {
            Logger.Warning($"{Name} 不支持圆弧插补运动");
            return false;
        }

        /// <summary>
        /// JOG运动
        /// 功能说明：控制指定轴进行JOG运动
        /// 设计模式：虚方法，子类可以选择性实现
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="speed">速度</param>
        /// <param name="positiveDirection">正方向</param>
        /// <returns>是否执行成功</returns>
        public virtual bool JogMove(int axisNo, double speed, bool positiveDirection)
        {
            Logger.Warning($"{Name} 不支持JOG运动");
            return false;
        }

        /// <summary>
        /// 设置轴运动参数 (加速度、减速度、起始速度、运行速度、最高速度)
        /// </summary>
        public virtual bool SetAxisParam(int axisNo, double acc, double dec, double vs, double vm, double vh)
        {
            // 默认空实现 — 子类可覆写
            return true;
        }

        /// <summary>
        /// 等待轴运动到位
        /// </summary>
        public virtual async Task<bool> WaitAxisDoneAsync(int axisNo, double targetPos,
            int timeoutMs = 10000, double tolerance = 2.0,
            CancellationToken token = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                token.ThrowIfCancellationRequested();

                double curPos = GetAxisPos(axisNo);
                bool isMoving = IsAxisMoving(axisNo);
                double deviation = Math.Abs(curPos - targetPos);

                if (deviation <= tolerance && !isMoving)
                    return true;

                await Task.Delay(1, token);
            }

            LogWarning($"轴 {axisNo} 到位超时 (目标={targetPos}, 当前={GetAxisPos(axisNo)})");
            return false;
        }

        /// <summary>
        /// 多轴批量等待到位
        /// </summary>
        public virtual async Task<bool> WaitAxesDoneAsync(Dictionary<int, double> targetPositions,
            int timeoutMs = 10000, double tolerance = 2.0,
            CancellationToken token = default)
        {
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
        /// 设置轴速度
        /// 功能说明：动态设置指定轴的速度
        /// 设计模式：虚方法，子类可以选择性实现
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="speed">速度</param>
        /// <returns>是否执行成功</returns>
        public virtual bool SetAxisSpeed(int axisNo, double speed)
        {
            Logger.Warning($"{Name} 不支持动态设置速度");
            return false;
        }

        /// <summary>
        /// 设置轴加减速
        /// 功能说明：动态设置指定轴的加减速
        /// 设计模式：虚方法，子类可以选择性实现
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        /// <returns>是否执行成功</returns>
        public virtual bool SetAxisAccDec(int axisNo, double acc, double dec)
        {
            Logger.Warning($"{Name} 不支持动态设置加减速");
            return false;
        }

        /// <summary>
        /// 检查轴号是否有效
        /// 功能说明：检查指定的轴号是否在有效范围内
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否有效</returns>
        public bool IsAxisValid(int axisNo)
        {
            return axisNo >= MinAxisNo && axisNo <= MaxAxisNo;
        }

        /// <summary>
        /// JOG 速度模式连续运动
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="velocity">速度 (正=正方向, 负=负方向)</param>
        /// <returns>是否执行成功</returns>
        public virtual bool MoveVel(int axisNo, double velocity)
        {
            Logger.Warning($"{Name} 不支持速度模式运动");
            return false;
        }

        /// <summary>
        /// 设置轴当前位置
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="pos">设定位置</param>
        /// <returns>是否执行成功</returns>
        public virtual bool SetPos(int axisNo, double pos)
        {
            Logger.Warning($"{Name} 不支持设置轴位置");
            return false;
        }

        /// <summary>
        /// 检查轴停止标志
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否已停止</returns>
        public virtual bool CheckStopFlag(int axisNo)
        {
            return !IsAxisMoving(axisNo);
        }

        /// <summary>
        /// 检查异常停止
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <returns>是否异常停止</returns>
        public virtual bool CheckAbnormalStop(int axisNo)
        {
            return GetAxisState(axisNo) == AxisState.Alarming;
        }

        /// <summary>
        /// 切换到低速档
        /// </summary>
        public virtual bool SwitchVel(int axisNo, double lowSpeed)
        {
            return SetAxisSpeed(axisNo, lowSpeed);
        }

        /// <summary>
        /// 切换到加减速档
        /// </summary>
        public virtual bool SwitchAcc(int axisNo, double acc)
        {
            return SetAxisAccDec(axisNo, acc, acc);
        }

        /// <summary>
        /// 脉冲→工程单位转换因子 (PulsePerMM)
        /// </summary>
        public virtual double PulsePerMM { get; set; } = 1000.0;

        /// <summary>
        /// 脉冲位置 → mm 位置
        /// </summary>
        public virtual double PulseToMM(int axisNo, double pulse)
        {
            return pulse / PulsePerMM;
        }

        /// <summary>
        /// mm 位置 → 脉冲位置
        /// </summary>
        public virtual double MMToPulse(int axisNo, double mm)
        {
            return mm * PulsePerMM;
        }

        /// <summary>
        /// 记录错误日志
        /// 功能说明：记录错误日志，支持异常信息
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="ex">异常对象</param>
        protected void LogError(string message, Exception ex = null)
        {
            if (ex != null)
                Logger.Error($"[{Name}] {message}", ex);
            else
                Logger.Error($"[{Name}] {message}");
        }

        /// <summary>
        /// 记录信息日志
        /// 功能说明：记录信息日志
        /// </summary>
        /// <param name="message">信息消息</param>
        protected void LogInfo(string message)
        {
            Logger.Info($"[{Name}] {message}");
        }

        /// <summary>
        /// 记录警告日志
        /// 功能说明：记录警告日志
        /// </summary>
        /// <param name="message">警告消息</param>
        protected void LogWarning(string message)
        {
            Logger.Warning($"[{Name}] {message}");
        }
    }
}
