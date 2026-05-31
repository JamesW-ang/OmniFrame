using System;
using System.Threading;
using System.Threading.Tasks;
using MotionIO;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// 上料工站 — 替代 ThreadLoad1 (治具取放组)
    /// 控制 LoadY 轴，治具移送气缸，与 CasselZ、Load2 协动。
    /// 主流程: Z 气缸降 → Y 安全位 → 通知 Cassel → 检测治具 → 取/送治具 → 扫码 → 送片
    /// </summary>
    public class Station_Load : BlockCutStationBase
    {
        #region 参数

        public double CasselZFirstPos { get; set; }
        public double CasselZSpace { get; set; } = 30;
        public int CasselIndex { get; set; } = 1;
        public double TrayYGetTray { get; set; }
        public double TrayYCode { get; set; } = 50;
        public double TrayYGetSlice { get; set; } = 100;
        public double TrayRowSpace { get; set; } = 20;
        public int TrayRow { get; set; } = 1;
        public int TrayCol { get; set; } = 1;
        public bool IsCloseJigCode { get; set; }

        #endregion

        #region 事件

        public event Action OnNoticeCasselLoad1Out;
        public event Action OnNoticeCasselLoad1In;
        public event Action OnNoticeCasselLoad1Get;
        public event Action OnNoticeCasselLoad1Send;
        public event Action<string> OnNoticeLoad2CanGetSlice;
        public event Action OnNoticeSweepCode;
        public event Action<string, string, int> OnStatusSend;

        #endregion

        #region 内部状态

        private volatile bool _readyFromCasselOnPos;
        private volatile bool _readyFromLoad2OnPos;
        private volatile bool _readyFromSweepCode;
        private volatile string _code = "";
        private volatile string _lastCode = "";
        private volatile bool _isPausedLocal;
        private volatile bool _isRepeat;

        #endregion

        public Station_Load(IBlockCutHardware hardware, BlockCutConfig cfg)
            : base(BlockCutConstants.ThreadLoadYName, (int)ThreadId.LoadY, hardware)
        {
            CasselZFirstPos = cfg.CasselZFirstPos;
            CasselZSpace = cfg.CasselZSpace;
            TrayYGetTray = cfg.TrayYGetTray;
            TrayYCode = cfg.TrayYCode;
            TrayYGetSlice = cfg.TrayYGetSlice;
            TrayRowSpace = cfg.TrayRowSpace;
            IsEmptyTest = cfg.IsEmptyTest;
            IsCloseJigCode = cfg.IsCloseJigCode;
        }

        #region 回调

        public void ReadyFromCasselOnPos() => _readyFromCasselOnPos = true;
        public void ReadyFromLoad2OnPos() => _readyFromLoad2OnPos = true;
        public void ReadyFromSweepCode(string code) { _code = code; _readyFromSweepCode = true; }
        public void NextMoveFromUser(bool isRepeat) { _isRepeat = isRepeat; }

        #endregion

        #region 主循环 — 状态机 (Qt QThread::exec() 对齐)

        private enum LoadState
        {
            Init,
            InitMoveToLayer,
            CheckJigAndTray,
            GetInitialTray,
            PosJig,
            SweepCode,
            SendTrayToCassel,
            GetSlice,
            AdvancePosition,
            GetNextTray,
        }

        private LoadState _loadState = LoadState.Init;
        private int _dryRunSeq;

        public override async Task RunAsync(CancellationToken token)
        {
            Logger.Info($"[{StationName}] ═══ 工站启动 ═══");
            Logger.Info($"[{StationName}]   空跑模式: {IsEmptyTest}");
            Logger.Info($"[{StationName}]   料盘起始: 行={TrayRow} 列={TrayCol} 行距={TrayRowSpace:F3}");
            _loadState = LoadState.Init;

            while (!token.IsCancellationRequested)
            {
                CheckPause(token);

                switch (_loadState)
                {
                    case LoadState.Init:
                        EmitMessage("上料组1初始化...");
                        await SetOneCylinder2Async(BlockCutConstants.DO_JigZCylinder, false,
                            BlockCutConstants.DI_JigZCylinderUp, false,
                            BlockCutConstants.DI_JigZCylinderDown, true,
                            $"{BlockCutConstants.ThreadLoadYName}Z气缸下降", token);
                        await SetCylinderForTwoDoAsync(BlockCutConstants.DO_JigYCylinderIn,
                            BlockCutConstants.DO_JigYCylinderOut,
                            BlockCutConstants.DI_JigYCylinderIn, true,
                            BlockCutConstants.DI_JigYCylinderOut, false,
                            $"{BlockCutConstants.ThreadLoadYName}Y气缸缩回", token);
                        _loadState = LoadState.InitMoveToLayer;
                        break;

                    case LoadState.InitMoveToLayer:
                    {
                        double dPos = CasselZFirstPos - (CasselIndex - 1) * CasselZSpace;
                        if (!await ZMoveAbsAsync(dPos - CasselZSpace, $"送料第{CasselIndex}个位置", token))
                        {
                            _loadState = LoadState.Init; // retry
                            break;
                        }
                        OnNoticeCasselLoad1Out?.Invoke();
                        _loadState = IsEmptyTest ? LoadState.PosJig : LoadState.CheckJigAndTray;
                        break;
                    }

                    case LoadState.CheckJigAndTray:
                        if (GetDI(BlockCutConstants.DI_CheckJig) && await CheckIsTrayAsync(token))
                        {
                            string errMsg = $"{BlockCutConstants.ThreadBottomGetName}暂停，当前层有治具,请拿走后继续";
                            OnStatusSend?.Invoke("ERR-MIS-04", errMsg, 3);
                            EmitWarning(41002, $"{BlockCutConstants.ThreadLoadYName}暂停，当前层有治具,请拿走后继续!", true);
                            // stay in this state until user resolves
                        }
                        else
                        {
                            _loadState = LoadState.GetInitialTray;
                        }
                        break;

                    case LoadState.GetInitialTray:
                        if (!await CheckIsTrayAsync(token))
                        {
                            OnNoticeCasselLoad1Get?.Invoke();
                            if (await GetOrSendTrayAsync(true, token))
                                _loadState = LoadState.PosJig;
                            // else: GetOrSendTrayAsync internally pauses, stays in this state
                        }
                        else
                        {
                            _loadState = LoadState.PosJig;
                        }
                        break;

                    case LoadState.PosJig:
                        await PosJigAsync(token);
                        _loadState = LoadState.SweepCode;
                        break;

                    case LoadState.SweepCode:
                        EmitMessage("上料组1等待扫码...");
                        if (IsEmptyTest)
                        {
                            _code = $"DRY-{++_dryRunSeq:D6}";
                            _lastCode = _code;
                            EmitMessage($"[空跑] 自动生成条码: {_code}");
                            _loadState = LoadState.SendTrayToCassel;
                        }
                        else
                        {
                            if (await SweepCodeAsync(token))
                                _loadState = LoadState.SendTrayToCassel;
                            // else: SweepCodeAsync internally pauses, stays here
                        }
                        break;

                    case LoadState.SendTrayToCassel:
                        OnNoticeCasselLoad1Send?.Invoke();
                        _loadState = LoadState.GetSlice;
                        break;

                    case LoadState.GetSlice:
                        await GetSliceAsync(TrayRow, token);
                        _loadState = LoadState.AdvancePosition;
                        break;

                    case LoadState.AdvancePosition:
                        if (!CheckRowCol())
                        {
                            // tray fully consumed — send empty tray back
                            await GetOrSendTrayAsync(false, token);
                            _loadState = LoadState.GetNextTray;
                        }
                        else
                        {
                            _loadState = LoadState.GetSlice;
                        }
                        break;

                    case LoadState.GetNextTray:
                        OnNoticeCasselLoad1Get?.Invoke();
                        if (await GetOrSendTrayAsync(true, token))
                            _loadState = LoadState.PosJig;
                        // else: GetOrSendTrayAsync internally pauses, stay here
                        break;
                }

                await Task.Delay(1, token);
            }
        }

        #endregion

        #region 治具定位

        private async Task PosJigAsync(CancellationToken token)
        {
            EmitMessage("上料组1开始定位治具...");
            await YMoveAbsAsync(TrayYGetTray, "取托盘位置", token);

            SetDO(BlockCutConstants.DO_JigYCylinderIn, false);
            SetDO(BlockCutConstants.DO_JigYCylinderOut, false);

            await SetOneCylinder2Async(BlockCutConstants.DO_JigZCylinder, true, BlockCutConstants.DI_JigZCylinderUp, true, BlockCutConstants.DI_JigZCylinderDown, false, $"{BlockCutConstants.ThreadLoadYName}Z气缸上升", token);

            await Task.Delay(2000, token);
            EmitMessage("上料组1定位治具结束");
        }

        private async Task<bool> CheckIsTrayAsync(CancellationToken token)
        {
            await YMoveAbsAsync(TrayYGetTray, "取托盘位置", token);

            SetDO(BlockCutConstants.DO_JigYCylinderIn, false);
            SetDO(BlockCutConstants.DO_JigYCylinderOut, false);

            await SetOneCylinder2Async(BlockCutConstants.DO_JigZCylinder, true, BlockCutConstants.DI_JigZCylinderUp, true, BlockCutConstants.DI_JigZCylinderDown, false, $"{BlockCutConstants.ThreadLoadYName}Z气缸上升", token);

            await Task.Delay(2000, token);
            return GetDI(BlockCutConstants.DI_CheckTray);
        }

        #endregion

        #region 等待信号

        private async Task WaitForCasselOnPosAsync(CancellationToken token)
        {
            if (IsEmptyTest) return;
            EmitMessage("等待卡塞组Z轴运动到位");
            while (!token.IsCancellationRequested)
            {
                if (_readyFromCasselOnPos) { _readyFromCasselOnPos = false; break; }
                await Task.Delay(1, token);
            }
            EmitMessage("等待结束");
        }

        private async Task WaitForLoad2OnPosAsync(CancellationToken token)
        {
            if (IsEmptyTest) return;
            EmitMessage("等待上料组X轴气缸上升");
            while (!token.IsCancellationRequested)
            {
                if (_readyFromLoad2OnPos) { _readyFromLoad2OnPos = false; break; }
                await Task.Delay(1, token);
            }
            EmitMessage("等待结束");
        }

        private async Task<bool> WaitForSweepCodeAsync(CancellationToken token)
        {
            EmitMessage("等待扫码");
            int elapsed = 0;
            while (!token.IsCancellationRequested)
            {
                if (_readyFromSweepCode) { _readyFromSweepCode = false; break; }
                await Task.Delay(1, token);
                elapsed++;
                if (elapsed >= 2000) { EmitMessage("等待超时"); return false; }
            }
            EmitMessage("等待结束");
            return true;
        }

        #endregion

        #region 轴运动

        private async Task<bool> YMoveAbsAsync(double targetPos, string desc, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int result = await OneAxisMoveAbsAsync((int)AxisId.LoadY, targetPos,
                    $"{BlockCutConstants.ThreadLoadYName}Y轴到{desc}", token: token);
                if (result == 1) return true;
                await Task.Delay(1, token);
            }
            return false;
        }

        private async Task<bool> ZMoveAbsAsync(double targetPos, string desc, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int result = await OneAxisMoveAbsAsync((int)AxisId.CasselZ, targetPos,
                    $"{BlockCutConstants.ThreadCasselZName}Z轴到{desc}", token: token);
                if (result == 1) return true;
                await Task.Delay(1, token);
            }
            return false;
        }

        #endregion

        #region 取送治具 (替代 GetOrSendTray)

        private async Task<bool> GetOrSendTrayAsync(bool bIsGet, CancellationToken token)
        {
            EmitMessage("上料组1开始取送治具...");

            await SetOneCylinder2Async(BlockCutConstants.DO_JigZCylinder, false, BlockCutConstants.DI_JigZCylinderUp, false, BlockCutConstants.DI_JigZCylinderDown, true, $"{BlockCutConstants.ThreadLoadYName}Z气缸下降", token);

            await WaitForCasselOnPosAsync(token);

            await SetCylinderForTwoDoAsync(BlockCutConstants.DO_JigYCylinderOut, BlockCutConstants.DO_JigYCylinderIn, BlockCutConstants.DI_JigYCylinderIn, false, BlockCutConstants.DI_JigYCylinderOut, true, $"{BlockCutConstants.ThreadLoadYName}治具移送Y气缸伸出", token);

            OnNoticeCasselLoad1In?.Invoke();
            await WaitForCasselOnPosAsync(token);

            await SetCylinderForTwoDoAsync(BlockCutConstants.DO_JigYCylinderIn, BlockCutConstants.DO_JigYCylinderOut, BlockCutConstants.DI_JigYCylinderIn, true, BlockCutConstants.DI_JigYCylinderOut, false, $"{BlockCutConstants.ThreadLoadYName}治具移送Y气缸缩回", token);

            OnNoticeCasselLoad1Out?.Invoke();
            EmitMessage("上料组1取送治具结束");

            if (bIsGet)
            {
                if (!await CheckIsTrayAsync(token) && !IsEmptyTest)
                {
                    CasselIndex++;
                    EmitWarning(41000, $"{BlockCutConstants.ThreadLoadYName}暂停，取治具失败，点击继续，继续下一层取治具", true);
                    return false;
                }
            }
            else
            {
                CasselIndex++;
            }
            return true;
        }

        #endregion

        #region 送片 (替代 GetSlice)

        private async Task GetSliceAsync(int nRow, CancellationToken token)
        {
            EmitMessage("上料组1开始送块料...");
            double dPos = TrayYGetSlice + (nRow - 1) * TrayRowSpace;
            await YMoveAbsAsync(dPos, $"第{nRow}行位置", token);

            OnNoticeLoad2CanGetSlice?.Invoke(_lastCode);
            await WaitForLoad2OnPosAsync(token);

            EmitMessage("上料组1送块料结束");
        }

        #endregion

        #region 行列检查

        private bool CheckRowCol()
        {
            bool bIsX = TrayCol == TrayCols;
            bool bIsY = TrayRow == TrayRows;
            if (bIsX && bIsY) return false;
            if (bIsX && !bIsY) { TrayRow++; TrayCol = 1; }
            else { TrayCol++; }
            return true;
        }

        private int TrayRows => 5;
        private int TrayCols => 4;

        #endregion

        #region 扫码

        private async Task<bool> SweepCodeAsync(CancellationToken token)
        {
            EmitMessage("上料组1开始扫码...");
            await YMoveAbsAsync(TrayYCode, "扫码位置", token);

            int nTime = 0;
            while (!token.IsCancellationRequested)
            {
                OnNoticeSweepCode?.Invoke();
                if (await WaitForSweepCodeAsync(token))
                {
                    if (_lastCode != _code && !_code.Contains("NoRead"))
                        break;
                }
                if (IsCloseJigCode) { _code = ""; break; }
                if (nTime > 3)
                {
                    EmitWarning(41001, $"{BlockCutConstants.ThreadLoadYName}暂停，扫码失败超过三次,请手动取走治具", true);
                    return false;
                }
                nTime++;
                await Task.Delay(1, token);
            }

            _lastCode = _code;
            EmitMessage("上料组1扫码结束");
            return true;
        }

        #endregion

        #region 手动操作入口

        public void GroupGetOrSendTray()
        {
            SetOneCylinder2Async(BlockCutConstants.DO_BlockGetZCylinder, false, BlockCutConstants.DI_BlockGetZCylinderUp, false, BlockCutConstants.DI_BlockGetZCylinderDown, false, $"{BlockCutConstants.ThreadLoadYName}产品上料Z气缸", CancellationToken.None).GetAwaiter().GetResult();

            YMoveAbsAsync(0, "原点位置", CancellationToken.None).GetAwaiter().GetResult();
            OnNoticeCasselLoad1Out?.Invoke();

            if (CheckIsTrayAsync(CancellationToken.None).GetAwaiter().GetResult()) OnNoticeCasselLoad1Send?.Invoke();
            else OnNoticeCasselLoad1Get?.Invoke();

            GetOrSendTrayAsync(true, CancellationToken.None).GetAwaiter().GetResult();
        }

        public void GroupGetSlice(int row, int col)
        {
            GetSliceAsync(row, CancellationToken.None).GetAwaiter().GetResult();
        }

        #endregion
    }
}
