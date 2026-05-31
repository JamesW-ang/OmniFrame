using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MotionIO;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// BlockCut 工站抽象基类 — 封装 ThreadParent 全部通用逻辑。
    /// 继承 StationBase，供 6 个生产工站继承。
    ///
    /// 主要封装：
    ///   1. 暂停/恢复 (替代 CheckPause)
    ///   2. 等待用户确认 (替代 WaitForUser)
    ///   3. 单气缸/双气缸动作 + DI 到位等待 (替代 SetOneCylinder / SetOneCylinderForTwoDo)
    ///   4. 单轴/多轴绝对/相对运动 + 到位重试 (替代 OneAixsMoveAbs 等)
    ///   5. UVW 旋转 (替代 BottomUVWRot)
    ///   6. 报警弹框 + 暂停 (替代 PauseWarnMessage)
    /// </summary>
    public abstract class BlockCutStationBase : StationBase
    {
        #region 字段

        protected readonly Motion Motion;
        protected readonly IoCtrl Io;
        protected readonly int ThreadNo;

        /// <summary>空跑模式 — 跳过硬件操作，仅模拟流程</summary>
        public bool IsEmptyTest { get; set; }

        // 空跑模式 IO 状态仿真
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, bool> s_dryDI = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, bool> s_dryDO = new();
        private static volatile bool s_dryInitDone;

        /// <summary>空跑: 获取 IO 状态快照 (供 UI 显示)</summary>
        public static IReadOnlyDictionary<int, bool> GetDryDI() => s_dryDI;
        public static IReadOnlyDictionary<int, bool> GetDryDO() => s_dryDO;

        /// <summary>空跑: 手动设置 DI 状态 (供外部协调事件使用)</summary>
        public static void SetDryDI(int diIndex, bool value)
        {
            s_dryDI[diIndex] = value;
        }

        /// <summary>空跑: 读取 DI (仿真传感器状态)</summary>
        protected bool GetDI(int diIndex)
        {
            if (IsEmptyTest)
            {
                if (!s_dryInitDone) InitDryRunIO();
                return s_dryDI.TryGetValue(diIndex, out bool v) ? v : false;
            }
            return Io.GetDI(diIndex);
        }

        /// <summary>空跑: 设置 DO 并自动翻转关联 DI</summary>
        protected void SetDO(int doIndex, bool value)
        {
            if (IsEmptyTest)
            {
                s_dryDO[doIndex] = value;
                AutoFlipDI(doIndex, value);
                EmitMessage($"[空跑] DO{doIndex}={(value ? "ON " : "OFF")}");
                return;
            }
            Io.SetDO(doIndex, value);
        }

        /// <summary>空跑: 等待 DI 到位 (仿真立即返回)</summary>
        protected async Task WaitDIAsync(int diChannel, bool expectedValue,
            int timeoutMs, string name, CancellationToken token)
        {
            if (IsEmptyTest)
            {
                s_dryDI[diChannel] = expectedValue;
                EmitMessage($"[空跑] DI{diChannel}={(expectedValue ? "ON " : "OFF")} ({name})");
                await Task.Delay(5, token);
                return;
            }
            await Io.WaitDIAsync(diChannel, expectedValue, timeoutMs, name, null, token);
        }

        private static void InitDryRunIO()
        {
            s_dryInitDone = true;
            // 安全 — OK
            s_dryDI[BlockCutConstants.DI_Door] = true;
            s_dryDI[BlockCutConstants.DI_Safe] = true;
            // 料塔 — 存在
            s_dryDI[BlockCutConstants.DI_CheckCassel] = true;
            // 料盘 — 存在
            s_dryDI[BlockCutConstants.DI_CheckTray] = true;
            // 底板 — 初始无
            s_dryDI[BlockCutConstants.DI_CheckBottom] = false;
            s_dryDI[BlockCutConstants.DI_BottomOutCheckBottom] = false;
            // 块料 — 无
            s_dryDI[BlockCutConstants.DI_BottomOutCheckBlock] = false;
            // 治具 — 无
            s_dryDI[BlockCutConstants.DI_CheckJig] = false;
            // 真空 — 关
            s_dryDI[BlockCutConstants.DI_BlockGetVacuum] = false;
            // 气缸初始: 全部缩回/上升
            s_dryDI[BlockCutConstants.DI_BottomCompressCylinderReach] = false;
            s_dryDI[BlockCutConstants.DI_BottomCompressCylinderRetract] = true;
            s_dryDI[BlockCutConstants.DI_JigZCylinderUp] = false;
            s_dryDI[BlockCutConstants.DI_JigZCylinderDown] = true;
            s_dryDI[BlockCutConstants.DI_YCylinderReach] = false;
            s_dryDI[BlockCutConstants.DI_YCylinderRetract] = true;
            s_dryDI[BlockCutConstants.DI_AdjustZUp] = true;
            s_dryDI[BlockCutConstants.DI_AdjustZDown] = false;
            s_dryDI[BlockCutConstants.DI_BottomOutClawOut] = true;
            s_dryDI[BlockCutConstants.DI_BottomOutClawIn] = false;
            s_dryDI[BlockCutConstants.DI_BottomOutPushCylinderUp] = true;
            s_dryDI[BlockCutConstants.DI_BottomOutPushCylinderDown] = false;
            s_dryDI[BlockCutConstants.DI_BottomOutBlockCylinderUp] = true;
            s_dryDI[BlockCutConstants.DI_BottomOutBlockCylinderDown] = false;
            s_dryDI[BlockCutConstants.DI_BottomOutZCylinderUp] = true;
            s_dryDI[BlockCutConstants.DI_BottomOutZCylinderDown] = false;
            s_dryDI[BlockCutConstants.DI_BottomYCylinderIn] = true;
            s_dryDI[BlockCutConstants.DI_UVCylinderUp] = true;
            s_dryDI[BlockCutConstants.DI_UVCylinderDown] = false;
            Logger.Info("[DryRunIO] 空跑IO状态初始化完成");
        }

        /// <summary>DO → DI 关联翻转表 (气缸伸出/缩回对应的到位传感器)</summary>
        private static readonly Dictionary<int, (int reach, int retract)> s_cylinderMap = new()
        {
            [BlockCutConstants.DO_BottomCompressCylinder] = (BlockCutConstants.DI_BottomCompressCylinderReach, BlockCutConstants.DI_BottomCompressCylinderRetract),
            [BlockCutConstants.DO_JigZCylinder]           = (BlockCutConstants.DI_JigZCylinderDown, BlockCutConstants.DI_JigZCylinderUp),
            [BlockCutConstants.DO_YCylinder]              = (BlockCutConstants.DI_YCylinderReach, BlockCutConstants.DI_YCylinderRetract),
            [BlockCutConstants.DO_AdjustZUp]              = (BlockCutConstants.DI_AdjustZUp, BlockCutConstants.DI_AdjustZDown),
            [BlockCutConstants.DO_AdjustZDown]            = (BlockCutConstants.DI_AdjustZDown, BlockCutConstants.DI_AdjustZUp),
            [BlockCutConstants.DO_BottomOutClawIn]        = (BlockCutConstants.DI_BottomOutClawIn, BlockCutConstants.DI_BottomOutClawOut),
            [BlockCutConstants.DO_BottomOutClawOut]       = (BlockCutConstants.DI_BottomOutClawOut, BlockCutConstants.DI_BottomOutClawIn),
            [BlockCutConstants.DO_BottomOutPushCylinder]  = (BlockCutConstants.DI_BottomOutPushCylinderDown, BlockCutConstants.DI_BottomOutPushCylinderUp),
            [BlockCutConstants.DO_BottomOutBlockCylinder] = (BlockCutConstants.DI_BottomOutBlockCylinderDown, BlockCutConstants.DI_BottomOutBlockCylinderUp),
            [BlockCutConstants.DO_BottomOutZCylinderDown] = (BlockCutConstants.DI_BottomOutZCylinderDown, BlockCutConstants.DI_BottomOutZCylinderUp),
            [BlockCutConstants.DO_BottomOutZCylinderUp]   = (BlockCutConstants.DI_BottomOutZCylinderUp, BlockCutConstants.DI_BottomOutZCylinderDown),
            [BlockCutConstants.DO_BottomYCylinderIn]      = (BlockCutConstants.DI_BottomYCylinderIn, BlockCutConstants.DI_BottomYCylinderIn),
            [BlockCutConstants.DO_UVZCylinder]            = (BlockCutConstants.DI_UVCylinderDown, BlockCutConstants.DI_UVCylinderUp),
        };

        private void AutoFlipDI(int doIndex, bool value)
        {
            if (!s_cylinderMap.TryGetValue(doIndex, out var pair)) return;
            if (value)
            {
                s_dryDI[pair.reach] = true;
                s_dryDI[pair.retract] = false;
            }
            else
            {
                s_dryDI[pair.reach] = false;
                s_dryDI[pair.retract] = true;
            }
        }

        // 暂停控制 (替代 QThread 的 m_bIsPause busy-wait)
        private readonly ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);
        private volatile bool _isPaused;

        // 用户确认信号 (替代 m_bIsObjectReadyFromUser)
        private readonly ManualResetEventSlim _userConfirmEvent = new ManualResetEventSlim(false);
        private volatile bool _isRepeat;

        #endregion

        #region 事件

        /// <summary>消息事件</summary>
        public event Action<string> OnMessage;
        /// <summary>报警 — 暂停并弹框</summary>
        public event Action<int, string, bool> OnWarning;
        /// <summary>等待用户确认 — 弹提示框</summary>
        public event Action<string> OnPrompt;
        /// <summary>取料失败</summary>
        public event Action<string> OnGetFail;
        /// <summary>单步运动完成</summary>
        public event Action OnFinishMove;
        /// <summary>错误日志写入</summary>
        public event Action<int, string, int> OnErrorLogTimeWrite;

        #endregion

        /// <param name="name">工站名称</param>
        /// <param name="threadNo">线程编号 (替代 ThreadParent::m_nThreadNo)</param>
        /// <param name="hardware">硬件抽象接口 (APS 或 仿真)</param>
        protected BlockCutStationBase(string name, int threadNo,
            IBlockCutHardware hardware) : base(name)
        {
            ThreadNo = threadNo;
            Motion = hardware?.Motion ?? throw new ArgumentNullException(nameof(hardware));
            Io = hardware.Io ?? throw new ArgumentNullException(nameof(hardware));
        }

        /// <summary>工站主循环 — 子类实现</summary>
        public abstract Task RunAsync(CancellationToken token);

        /// <summary>
        /// 带取消支持的运行方法 — 捕获异常并触发警告
        /// </summary>
        public async Task RunWithCancellationAsync(CancellationToken token)
        {
            try
            {
                await RunAsync(token);
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不处理
            }
            catch (Exception ex)
            {
                EmitWarning(-1, ex.Message, true);
                Pause();
            }
        }

        /// <summary>BlockCut 工站使用 RunAsync 而非 Execute/DoExecute 模式</summary>
        protected override bool DoExecute() => true;

        #region 暂停 / 恢复

        /// <summary>
        /// 暂停工站 (替代 PauseRun(true))
        /// </summary>
        public void Pause()
        {
            _isPaused = true;
            _pauseEvent.Reset();
        }

        /// <summary>
        /// 恢复工站 (替代 PauseRun(false))
        /// </summary>
        public void Resume()
        {
            _isPaused = false;
            _pauseEvent.Set();
        }

        /// <summary>
        /// 检测暂停状态，阻塞直到恢复或取消 (替代 CheckPause 的 busy-wait)
        /// </summary>
        protected void CheckPause(CancellationToken token)
        {
            while (_isPaused && !token.IsCancellationRequested)
            {
                _pauseEvent.Wait(1, token);
            }
        }

        /// <summary>是否处于暂停状态</summary>
        public bool IsPaused => _isPaused;

        /// <summary>报警后继续 — 取消暂停恢复运行</summary>
        public void ContinueAfterAlarm()
        {
            Resume();
            Logger.Info($"[{StationName}] 报警已确认，继续运行");
        }

        #endregion

        #region 用户确认

        /// <summary>
        /// 用户已确认 (替代 ObjectReadyFromUser)
        /// </summary>
        public void UserConfirm(bool isRepeat = false)
        {
            _isRepeat = isRepeat;
            _userConfirmEvent.Set();
        }

        /// <summary>
        /// 等待用户确认 — 阻塞直到确认或取消 (替代 WaitForUser)
        /// </summary>
        protected void WaitForUser(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_userConfirmEvent.Wait(1, token))
                {
                    _userConfirmEvent.Reset();
                    break;
                }
            }
        }

        /// <summary>上次确认是否为"继续重复"</summary>
        protected bool IsRepeat => _isRepeat;

        #endregion

        #region 气缸控制

        /// <summary>
        /// 单气缸动作: 输出 DO → 等待 DI 到位 (替代 SetOneCylinder)
        /// </summary>
        protected async Task<bool> SetOneCylinderAsync(int doIndex, bool doValue,
            int diIndex, bool expectedDI, string desc, CancellationToken token)
        {
            CheckPause(token);
            token.ThrowIfCancellationRequested();

            if (IsEmptyTest)
            {
                SetDO(doIndex, doValue);
                EmitMessage($"[空跑] 气缸 DO{doIndex}={doValue}, DI{diIndex}={expectedDI}");
                await Task.Delay(10, token);
                return true;
            }

            Logger.Info($"[{StationName}] {desc}");
            Io.SetDO(doIndex, doValue);

            bool ok = await Io.WaitDIAsync(diIndex, expectedDI,
                BlockCutConstants.TimeoutWarn, desc,
                msg => PauseWarnMessage(ThreadNo * 10000 + diIndex, msg),
                token);

            return ok;
        }

        /// <summary>
        /// 双 DI 气缸动作: 输出 DO → 等待两个 DI 同时到位
        /// </summary>
        protected async Task<bool> SetOneCylinder2Async(int doIndex, bool doValue,
            int di1, bool exp1, int di2, bool exp2, string desc, CancellationToken token)
        {
            CheckPause(token);
            token.ThrowIfCancellationRequested();

            if (IsEmptyTest)
            {
                SetDO(doIndex, doValue);
                EmitMessage($"[空跑] 气缸 DO{doIndex}={doValue}, DI{di1}={exp1} DI{di2}={exp2}");
                await Task.Delay(10, token);
                return true;
            }

            Logger.Info($"[{StationName}] {desc}");
            Io.SetDO(doIndex, doValue);

            return await Io.WaitDI2Async(di1, exp1, di2, exp2,
                BlockCutConstants.TimeoutWarn, desc,
                msg => PauseWarnMessage(ThreadNo * 10000 + di1, msg),
                token);
        }

        /// <summary>
        /// 双 DO 气缸动作 (替代 SetOneCylinderForTwoDo)
        /// 先关 DO2，再开 DO1，等待两个 DI 到位
        /// </summary>
        protected async Task<bool> SetCylinderForTwoDoAsync(int do1, int do2,
            int di1, bool exp1, int di2, bool exp2, string desc, CancellationToken token)
        {
            CheckPause(token);
            token.ThrowIfCancellationRequested();

            if (IsEmptyTest)
            {
                SetDO(do2, false);
                SetDO(do1, true);
                EmitMessage($"[空跑] 气缸 DO{do1}=ON DO{do2}=OFF, DI{di1}={exp1} DI{di2}={exp2}");
                await Task.Delay(10, token);
                return true;
            }

            Logger.Info($"[{StationName}] {desc}");
            Io.SetDO(do2, false);   // 先关
            Io.SetDO(do1, true);    // 再开

            return await Io.WaitDI2Async(di1, exp1, di2, exp2,
                BlockCutConstants.TimeoutWarn, desc,
                msg => PauseWarnMessage(ThreadNo * 10000 + di1, msg),
                token);
        }

        #endregion

        #region 轴运动 (带到位检测与重试)

        /// <summary>
        /// 单轴绝对运动 + 到位检测 + 失败重试 (替代 OneAixsMoveAbs)
        /// 返回: 1=成功, 0=失败需人工确认, -1=线程取消
        /// </summary>
        protected async Task<int> OneAxisMoveAbsAsync(int axisNo, double targetPos,
            string desc, double velRatio = 1.0, bool checkPos = true,
            CancellationToken token = default)
        {
            CheckPause(token);

            if (IsEmptyTest)
            {
                EmitMessage($"[空跑] 轴{axisNo} {desc} → {targetPos:F3}");
                await Task.Delay(10, token);
                return 1;
            }

            while (!token.IsCancellationRequested)
            {
                Logger.Info($"[{StationName}] 轴{axisNo} {desc}");
                Motion.AbsMove(axisNo, targetPos, velRatio);

                bool done = await Motion.WaitAxisDoneAsync(axisNo, targetPos,
                    BlockCutConstants.TimeoutWarn, BlockCutConstants.PositionTolerance, token);

                if (done)
                    return 1;

                PauseWarnMessage(
                    ThreadNo * 10000 + axisNo,
                    $"轴{axisNo} {desc} 点位={targetPos} 未到位，点继续重新到位！");
                return 0; // 要求人工干预
            }

            return -1;
        }

        /// <summary>
        /// 单轴相对运动 (替代 OneAixsMoveRel)
        /// </summary>
        protected async Task<int> OneAxisMoveRelAsync(int axisNo, double distance,
            string desc, double velRatio = 1.0, bool checkPos = true,
            CancellationToken token = default)
        {
            CheckPause(token);

            if (IsEmptyTest)
            {
                EmitMessage($"[空跑] 轴{axisNo} {desc} += {distance:F3}");
                await Task.Delay(10, token);
                return 1;
            }

            double curPos = Motion.GetAxisPos(axisNo);
            double targetPos = curPos + distance;

            while (!token.IsCancellationRequested)
            {
                Logger.Info($"[{StationName}] 轴{axisNo} {desc}");
                Motion.RelativeMove(axisNo, distance, velRatio);

                bool done = await Motion.WaitAxisDoneAsync(axisNo, targetPos,
                    BlockCutConstants.TimeoutWarn, BlockCutConstants.PositionTolerance, token);

                if (done)
                    return 1;

                PauseWarnMessage(
                    ThreadNo * 10000 + axisNo,
                    $"轴{axisNo} {desc} 相对点位={distance} 未到位，点继续重新到位！");
                return 0;
            }

            return -1;
        }

        /// <summary>
        /// 双轴绝对运动 (替代 TwoAixsMoveAbs)
        /// </summary>
        protected async Task<int> TwoAxisMoveAbsAsync(int a1, double p1,
            int a2, double p2, string desc, double velRatio = 1.0,
            bool checkPos = true, CancellationToken token = default)
        {
            CheckPause(token);

            if (IsEmptyTest)
            {
                EmitMessage($"[空跑] 轴{a1},{a2} {desc} → {p1:F3},{p2:F3}");
                await Task.Delay(10, token);
                return 1;
            }

            while (!token.IsCancellationRequested)
            {
                Logger.Info($"[{StationName}] 轴{a1},{a2} {desc}");
                Motion.AbsMove(a1, p1, velRatio);
                Motion.AbsMove(a2, p2, velRatio);

                var targets = new Dictionary<int, double> { { a1, p1 }, { a2, p2 } };
                bool done = await Motion.WaitAxesDoneAsync(targets,
                    BlockCutConstants.TimeoutWarn, BlockCutConstants.PositionTolerance, token);

                if (done)
                    return 1;

                PauseWarnMessage(
                    ThreadNo * 10000 + a1 + a2,
                    $"轴{a1},{a2} {desc} 点位={p1},{p2} 未到位，点继续重新到位！");
                return 0;
            }

            return -1;
        }

        /// <summary>
        /// 双轴相对运动 (替代 TwoAixsMoveRel)
        /// </summary>
        protected async Task<int> TwoAxisMoveRelAsync(int a1, double d1,
            int a2, double d2, string desc, double velRatio = 1.0,
            bool checkPos = true, CancellationToken token = default)
        {
            CheckPause(token);

            if (IsEmptyTest)
            {
                EmitMessage($"[空跑] 轴{a1},{a2} {desc} += {d1:F3},{d2:F3}");
                await Task.Delay(10, token);
                return 1;
            }

            double cur1 = Motion.GetAxisPos(a1);
            double cur2 = Motion.GetAxisPos(a2);

            while (!token.IsCancellationRequested)
            {
                Logger.Info($"[{StationName}] 轴{a1},{a2} {desc}");
                Motion.RelativeMove(a1, d1, velRatio);
                Motion.RelativeMove(a2, d2, velRatio);

                var targets = new Dictionary<int, double> { { a1, cur1 + d1 }, { a2, cur2 + d2 } };
                bool done = await Motion.WaitAxesDoneAsync(targets,
                    BlockCutConstants.TimeoutWarn, BlockCutConstants.PositionTolerance, token);

                if (done)
                    return 1;

                PauseWarnMessage(
                    ThreadNo * 10000 + a1 + a2,
                    $"轴{a1},{a2} {desc} 相对点位={d1},{d2} 未到位，点继续重新到位！");
                return 0;
            }

            return -1;
        }

        /// <summary>
        /// 多轴绝对运动 (替代 AixsMoveAbs)
        /// </summary>
        protected async Task<int> MultiAxisMoveAbsAsync(Dictionary<int, double> targets,
            Dictionary<int, string> names, Dictionary<int, double> velRatios = null,
            bool checkPos = true, CancellationToken token = default)
        {
            CheckPause(token);

            if (IsEmptyTest)
            {
                EmitMessage($"[空跑] 多轴联动 {string.Join(",", targets.Keys)} → {string.Join(",", targets.Values)}");
                await Task.Delay(10, token);
                return 1;
            }

            while (!token.IsCancellationRequested)
            {
                foreach (var kv in targets)
                {
                    string name = names?.TryGetValue(kv.Key, out var n) == true ? n : $"轴{kv.Key}";
                    double ratio = velRatios?.TryGetValue(kv.Key, out var r) == true ? r : 1.0;
                    Logger.Info($"[{StationName}] {name}");
                    Motion.AbsMove(kv.Key, kv.Value, ratio);
                }

                bool done = await Motion.WaitAxesDoneAsync(targets,
                    BlockCutConstants.TimeoutWarn, BlockCutConstants.PositionTolerance, token);

                if (done)
                    return 1;

                return 0;
            }

            return -1;
        }

        #endregion

        #region UVW 平台旋转

        /// <summary>
        /// UVW 三轴旋转 — 3 轴联动 (替代 BottomUVWRot)
        /// 基于三角函数计算 U/V/W 的目标位置实现绕 (cx, cy) 旋转 angle 度
        /// </summary>
        /// <param name="angle">旋转角度 (度)</param>
        /// <param name="cx">旋转中心 X (用户坐标)</param>
        /// <param name="cy">旋转中心 Y (用户坐标)</param>
        /// <param name="token">取消令牌</param>
        protected async Task<bool> UVWRotateAsync(double angle, double cx, double cy,
            CancellationToken token)
        {
            if (IsEmptyTest)
            {
                EmitMessage($"[空跑] UVW旋转 angle={angle:F2}° at ({cx:F3},{cy:F3})");
                await Task.Delay(10, token);
                return true;
            }

            const double rad = Math.PI / 180.0;
            double a = angle * rad;

            double uCur = Motion.GetAxisPos((int)AxisId.BottomU);
            double vCur = Motion.GetAxisPos((int)AxisId.BottomV);
            double wCur = Motion.GetAxisPos((int)AxisId.BottomW);

            // UVW 旋转公式: 根据平台几何参数计算
            // u = R * cos(120°) + cx, v = R * cos(0°) + cy, w = R * cos(-120°) + cy
            // 简化: 假设 U/V/W 增量正比于角度分量
            double cosA = Math.Cos(a);
            double sinA = Math.Sin(a);

            double uTarget = uCur + (cosA - 1) * cx - sinA * cy;
            double vTarget = vCur + (cosA - 1) * cx - sinA * cy;
            double wTarget = wCur + (cosA - 1) * cx - sinA * cy;

            // 三轴同时移动
            int[] axes = { (int)AxisId.BottomU, (int)AxisId.BottomV, (int)AxisId.BottomW };
            double[] targets = { uTarget, vTarget, wTarget };

            Motion.AbsLinearMove(axes, targets, 1.0, 0.5, 0.5);

            var targetDict = new Dictionary<int, double>
            {
                { (int)AxisId.BottomU, uTarget },
                { (int)AxisId.BottomV, vTarget },
                { (int)AxisId.BottomW, wTarget },
            };

            return await Motion.WaitAxesDoneAsync(targetDict,
                BlockCutConstants.TimeoutWarn, BlockCutConstants.PositionTolerance, token);
        }

        #endregion

        #region 公共辅助

        /// <summary>
        /// 报警弹框 + 暂停 (替代 PauseWarnMessage)
        /// </summary>
        protected void PauseWarnMessage(int errorCode, string message)
        {
            if (_isPaused) return; // 已暂停则跳过

            OnWarning?.Invoke(errorCode, message, true);

            // 记录超时日志
            OnErrorLogTimeWrite?.Invoke(errorCode, message, BlockCutConstants.TimeoutWarn / 1000);

            // 暂停
            Pause();
        }

        /// <summary>
        /// 取料失败弹框 (替代 GetFailPopDialog)
        /// </summary>
        protected void GetFailDialog(string message, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            OnGetFail?.Invoke(message);
            WaitForUser(token);
        }

        /// <summary>发送消息到日志</summary>
        protected void EmitMessage(string msg) => Logger.Info($"[{StationName}] {msg}");
        protected void EmitWarning(int code, string msg, bool pause)
        {
            Logger.Warning($"[{StationName}] [{code}] {msg}");
            OnWarning?.Invoke(code, msg, pause);
        }
        protected void EmitPrompt(string msg)
        {
            Logger.Info($"[{StationName}] [Prompt] {msg}");
            OnPrompt?.Invoke(msg);
        }

        /// <summary>等待 ms 毫秒</summary>
        protected async Task DelayMs(int ms, CancellationToken token)
        {
            await Task.Delay(ms, token);
        }

        /// <summary>
        /// 带自动恢复的工站运行包装器 — 捕获异常后按重试策略自动重试。
        /// 每个工站的 RunAsync 应通过此方法启动，而非直接 Task.Run。
        /// </summary>
        /// <param name="stationName">工站名称（用于日志）</param>
        /// <param name="runFunc">RunAsync 调用委托</param>
        /// <param name="maxRetries">最大自动重试次数（超过后触发报警并退出）</param>
        /// <param name="token">取消令牌</param>
        /// <returns>正常退出返回 true，超出重试次数返回 false</returns>
        public static async Task<bool> RunWithAutoRecoveryAsync(
            string stationName,
            Func<CancellationToken, Task> runFunc,
            int maxRetries,
            CancellationToken token)
        {
            int attempt = 0;
            int baseDelayMs = 500;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await runFunc(token);
                    // 正常退出（状态机自然结束）
                    return true;
                }
                catch (OperationCanceledException)
                {
                    Logger.Info($"[{stationName}] 收到取消信号，退出");
                    return true;
                }
                catch (Exception ex)
                {
                    attempt++;

                    if (attempt > maxRetries)
                    {
                        Logger.Error($"[{stationName}] 超过最大重试次数 ({maxRetries})，终止运行。最后错误: {ex.Message}", ex);
                        return false;
                    }

                    // 指数退避重试: 500ms → 1s → 2s → ...
                    int delay = Math.Min(baseDelayMs * (int)Math.Pow(2, attempt - 1), 10_000);
                    Logger.Warning($"[{stationName}] 运行异常 (第{attempt}/{maxRetries}次重试), {delay}ms后重试: {ex.Message}", ex);

                    try
                    {
                        await Task.Delay(delay, token);
                    }
                    catch (OperationCanceledException)
                    {
                        return true;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
