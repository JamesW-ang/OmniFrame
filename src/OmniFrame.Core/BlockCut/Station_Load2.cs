using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MotionIO;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// 二次上料工站 — 替代 ThreadLoad2 (块料取放组)
    /// 控制 LoadX 轴，产品上料 Z 气缸 + 真空吸嘴，与 Load1、Adjust 协动。
    /// 主流程: 检测真空 → 取片 (GetOnce) → 扫码 → 放片 (SetOnce) → 行列检查
    /// </summary>
    public class Station_Load2 : BlockCutStationBase
    {
        #region 参数

        /// <summary>托盘取片 X 基准位置</summary>
        public double TrayXGetSlice { get; set; } = 100;

        /// <summary>托盘放片 X 基准位置</summary>
        public double TrayXSetSlice { get; set; } = 200;

        /// <summary>托盘列间距</summary>
        public double TrayColSpace { get; set; } = 20;

        /// <summary>底板列间距</summary>
        public double BottomColSpace { get; set; } = 25;

        /// <summary>当前托盘列</summary>
        public int TrayCol { get; set; } = 1;

        /// <summary>当前底板行</summary>
        public int BottomRow { get; set; } = 1;

        /// <summary>当前底板列</summary>
        public int BottomCol { get; set; } = 1;

        public bool IsCloseSliceCode { get; set; }

        #endregion

        #region 事件

        /// <summary>通知 Load1: X 轴已完成取片, Load1 可继续移动 Y</summary>
        public event Action OnNoticeLoad1CanMove;

        /// <summary>通知 Adjust: 块料已放好 (治具码, 块料码)</summary>
        public event Action<string, string> OnNoticeBottomCanMove;

        /// <summary>请求扫码</summary>
        public event Action OnNoticeSweepCode;

        /// <summary>报警状态上报</summary>
        public event Action<string, string, int> OnStatusSend;

        #endregion

        #region 内部状态

        private volatile bool _readyFromLoad1OnPos;
        private volatile bool _readyFromBottomOnPos;
        private volatile bool _readyFromBottomHasAdjust;
        private volatile bool _readyFromSweepCode;
        private volatile bool _isRepeat;
        private volatile string _jigCode = "";
        private volatile string _tmpJigCode = "";
        private volatile string _blockCode = "";
        private volatile string _lastBlockCode = "";

        #endregion

        public Station_Load2(IBlockCutHardware hardware, BlockCutConfig cfg)
            : base(BlockCutConstants.ThreadLoadXName, (int)ThreadId.LoadX, hardware)
        {
            TrayXGetSlice = cfg.TrayXGetSlice;
            TrayXSetSlice = cfg.TrayXSetSlice;
            TrayColSpace = cfg.TrayColSpace;
            BottomColSpace = cfg.BottomColSpace;
            IsEmptyTest = cfg.IsEmptyTest;
            IsCloseSliceCode = cfg.IsCloseSliceCode;
        }

        #region 回调

        public void ReadyFromLoad1OnPos(string jigCode)
        { _jigCode = jigCode; _readyFromLoad1OnPos = true; }

        public void ReadyFromBottomOnPos() => _readyFromBottomOnPos = true;

        public void ReadyFromBottomHasAdjust() => _readyFromBottomHasAdjust = true;

        public void NextMoveFromUser(bool isRepeat)
        { _isRepeat = isRepeat; }

        public void ReadyFromSweepCode(string code)
        { _blockCode = code; _readyFromSweepCode = true; }

        #endregion

        #region 主循环 — 状态机 (Qt QThread::exec() 对齐)

        private enum Load2State
        {
            Init,
            VacuumCheck,
            GetOnce,
            SweepCode,
            SetOnce,
            AdvancePosition,
        }

        private Load2State _load2State = Load2State.Init;
        private int _dryRunSeq;

        public override async Task RunAsync(CancellationToken token)
        {
            Logger.Info($"[{StationName}] ═══ 工站启动 ═══");
            Logger.Info($"[{StationName}]   空跑模式: {IsEmptyTest}");
            Logger.Info($"[{StationName}]   底板起始: 行={BottomRow} 列={BottomCol} 列距={BottomColSpace:F3}");
            _load2State = Load2State.Init;

            while (!token.IsCancellationRequested)
            {
                CheckPause(token);

                switch (_load2State)
                {
                    case Load2State.Init:
                        EmitMessage("上料组2初始化 — 检测真空...");
                        _load2State = Load2State.VacuumCheck;
                        break;

                    case Load2State.VacuumCheck:
                        if (CheckVacuum() && !IsEmptyTest)
                        {
                            string errMsg = $"{BlockCutConstants.ThreadBottomGetName}暂停，请取走吸嘴上的块料";
                            OnStatusSend?.Invoke("ERR-MIS-02", errMsg, 3);
                            EmitWarning(51000, $"{BlockCutConstants.ThreadLoadXName}暂停，请取走吸嘴上的块料", true);
                            // stay in this state until user clears vacuum
                        }
                        else
                        {
                            _load2State = Load2State.GetOnce;
                        }
                        break;

                    case Load2State.GetOnce:
                        await GetOnceAsync(TrayCol, token);
                        _load2State = Load2State.SweepCode;
                        break;

                    case Load2State.SweepCode:
                        EmitMessage("上料组2等待扫码...");
                        if (IsEmptyTest)
                        {
                            _blockCode = $"DRY-{++_dryRunSeq:D6}";
                            _lastBlockCode = _blockCode;
                            EmitMessage($"[空跑] 自动生成块料码: {_blockCode}");
                            _load2State = Load2State.SetOnce;
                        }
                        else
                        {
                            if (await SweepCodeAsync(token))
                                _load2State = Load2State.SetOnce;
                            // else: SweepCodeAsync internally pauses, stay here
                        }
                        break;

                    case Load2State.SetOnce:
                        await SetOnceAsync(BottomCol, token);
                        _load2State = Load2State.AdvancePosition;
                        break;

                    case Load2State.AdvancePosition:
                        CheckRowCol();
                        _load2State = Load2State.VacuumCheck;
                        break;
                }

                await Task.Delay(1, token);
            }
        }

        #endregion

        #region 取片 (替代 GetOnce)

        private async Task<bool> GetOnceAsync(int nCol, CancellationToken token)
        {
            EmitMessage("上料组2开始取块料...");
            double dPos = TrayXGetSlice + (nCol - 1) * TrayColSpace;
            if (!await XMoveAbsAsync(dPos, $"第{nCol}列取片源位置", token))
                return false;

            // 等待 Load1 的 Y 轴到位
            EmitMessage("等待上料组Y轴运动到位");
            while (!token.IsCancellationRequested)
            {
                if (_readyFromLoad1OnPos) { _readyFromLoad1OnPos = false; break; }
                await Task.Delay(1, token);
            }
            EmitMessage("等待结束");

            _tmpJigCode = _jigCode;

            // 真空打开
            SetDO(BlockCutConstants.DO_BlockGetVacuum, true);
            EmitMessage("真空信号打开");

            while (!token.IsCancellationRequested)
            {
                // Z 气缸下降
                await SetOneCylinder2Async(BlockCutConstants.DO_BlockGetZCylinder, true, BlockCutConstants.DI_BlockGetZCylinderUp, false, BlockCutConstants.DI_BlockGetZCylinderDown, true, $"{BlockCutConstants.ThreadLoadXName}Z气缸下降", token);

                await Task.Delay(500, token);

                // Z 气缸上升
                await SetOneCylinder2Async(BlockCutConstants.DO_BlockGetZCylinder, false, BlockCutConstants.DI_BlockGetZCylinderUp, true, BlockCutConstants.DI_BlockGetZCylinderDown, false, $"{BlockCutConstants.ThreadLoadXName}Z气缸上升", token);

                if (!CheckVacuum() && !IsEmptyTest)
                {
                    // 取料失败 — 弹框选择重试/跳过
                    EmitWarning(0, $"{BlockCutConstants.ThreadLoadXName}未取到块料", false);
                    if (!_isRepeat) break;
                }
                else
                {
                    break;
                }
                await Task.Delay(1, token);
            }

            OnNoticeLoad1CanMove?.Invoke();
            EmitMessage("上料组2取块料结束");

            return CheckVacuum();
        }

        #endregion

        #region 放片 (替代 SetOnce)

        private async Task<bool> SetOnceAsync(int nCol, CancellationToken token)
        {
            EmitMessage("上料组2开始放块料...");
            double dPos = TrayXSetSlice - (nCol - 1) * BottomColSpace;
            if (!await XMoveAbsAsync(dPos, $"第{nCol}列放片源位置", token))
                return false;

            // 等待底板组 Y 到位
            EmitMessage("等待底板组Y轴运动到位");
            while (!token.IsCancellationRequested)
            {
                if (_readyFromBottomOnPos) { _readyFromBottomOnPos = false; break; }
                await Task.Delay(1, token);
            }
            EmitMessage("等待结束");

            // Z 气缸下降
            await SetOneCylinder2Async(BlockCutConstants.DO_BlockGetZCylinder, true, BlockCutConstants.DI_BlockGetZCylinderUp, false, BlockCutConstants.DI_BlockGetZCylinderDown, true, $"{BlockCutConstants.ThreadLoadXName}Z气缸下降", token);

            // 真空关闭
            SetDO(BlockCutConstants.DO_BlockGetVacuum, false);
            EmitMessage("真空信号关闭");

            await Task.Delay(1000, token);

            // Z 气缸上升
            await SetOneCylinder2Async(BlockCutConstants.DO_BlockGetZCylinder, false, BlockCutConstants.DI_BlockGetZCylinderUp, true, BlockCutConstants.DI_BlockGetZCylinderDown, false, $"{BlockCutConstants.ThreadLoadXName}Z气缸上升", token);

            OnNoticeBottomCanMove?.Invoke(_tmpJigCode, _lastBlockCode);
            EmitMessage("上料组2放块料结束");
            return true;
        }

        #endregion

        #region 轴运动

        private async Task<bool> XMoveAbsAsync(double targetPos, string desc, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int result = await OneAxisMoveAbsAsync((int)AxisId.LoadX, targetPos,
                    $"{BlockCutConstants.ThreadLoadXName}X轴到{desc}", token: token);
                if (result == 1) return true;
                await Task.Delay(1, token);
            }
            return false;
        }

        #endregion

        #region 真空检测 (替代 CheckVaccum)

        private bool CheckVacuum()
        {
            return GetDI(BlockCutConstants.DI_BlockGetVacuum);
        }

        #endregion

        #region 扫码 (替代 SweepCode)

        private async Task<bool> SweepCodeAsync(CancellationToken token)
        {
            int nTime = 0;
            while (!token.IsCancellationRequested)
            {
                OnNoticeSweepCode?.Invoke();

                if (await WaitForSweepCodeAsync(token))
                {
                    if (CheckStr() && !_blockCode.Contains("NoRead"))
                    {
                        _lastBlockCode = _blockCode;
                        break;
                    }
                }

                if (IsCloseSliceCode) { _blockCode = ""; break; }

                if (nTime > 3)
                {
                    OnStatusSend?.Invoke("ERR-MIS-03",
                        $"{BlockCutConstants.ThreadBottomGetName}暂停，扫码失败超过三次,请手动取走块料", 3);
                    EmitWarning(51001,
                        $"{BlockCutConstants.ThreadLoadXName}暂停，扫码失败超过三次,请手动取走块料", true);
                    return false;
                }
                nTime++;
                await Task.Delay(1, token);
            }
            return true;
        }

        private async Task<bool> WaitForSweepCodeAsync(CancellationToken token)
        {
            int elapsed = 0;
            while (!token.IsCancellationRequested)
            {
                if (_readyFromSweepCode) { _readyFromSweepCode = false; break; }
                await Task.Delay(1, token);
                elapsed++;
                if (elapsed >= 2000) return false;
            }
            return true;
        }

        /// <summary>比较块码是否变化 (替代 CheckStr: 按 '-' 分割后比较前缀)</summary>
        private bool CheckStr()
        {
            if (string.IsNullOrEmpty(_lastBlockCode)) return true;
            var parts1 = _blockCode.Split('-');
            var parts2 = _lastBlockCode.Split('-');
            for (int i = 0; i < parts1.Length - 1 && i < parts2.Length; i++)
            {
                if (parts1[i] != parts2[i]) return true;
            }
            return false;
        }

        #endregion

        #region 行列检查 (替代 CheckRowCol)

        /// <returns>true=底板已满</returns>
        private bool CheckRowCol()
        {
            bool bIsX = BottomCol == BottomCols;
            bool bIsY = BottomRow == BottomRows;
            bool bIsFull = false;

            if (bIsX && bIsY)
            {
                bIsFull = true;
                BottomCol = 1;
                BottomRow = 1;
            }
            else if (bIsX && !bIsY)
            {
                BottomRow++;
                BottomCol = 1;
            }
            else
            {
                BottomCol++;
            }

            // 跳过位置 (3,2) — 来自原 Qt 硬编码
            if (BottomRow == 3 && BottomCol == 2)
                BottomCol = 3;

            return bIsFull;
        }

        private int BottomRows => 5;
        private int BottomCols => 4;

        #endregion

        #region 手动操作入口

        public void GroupVacuum()
        {
            bool current = GetDI(BlockCutConstants.DI_BlockGetVacuum);
            SetDO(BlockCutConstants.DO_BlockGetVacuum, !current);
            EmitMessage(current ? "真空信号打开" : "真空信号关闭");
        }

        public void GroupGetSlice(int row, int col)
        {
            if (CheckVacuum()) return;
            GetOnceAsync(col, CancellationToken.None).GetAwaiter().GetResult();
        }

        public void GroupSetSlice(int row, int col)
        {
            SetOnceAsync(col, CancellationToken.None).GetAwaiter().GetResult();
        }

        #endregion
    }
}
