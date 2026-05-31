using System;
using System.Threading;
using System.Threading.Tasks;
using MotionIO;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// CasselZ 工站 — 替代 ThreadCassel (料塔组)
    /// 控制 CasselZ 轴升降取送治具托盘，与 Load1 协动。
    /// 主流程: 检测卡塞 → 循环取/送托盘 → 移至指定层 → 通知 Load1 → 等待 Load1 就位 → Z 轴微调
    /// </summary>
    public class Station_CasselZ : BlockCutStationBase
    {
        #region 参数

        public double CasselZFirstPos { get; set; }
        public double CasselZSpace { get; set; } = 30;
        public int CasselIndex { get; set; } = 1;
        public int CasselCount { get; set; } = 10;

        #endregion

        #region 事件

        public event Action OnNoticeLoad1CanMove;

        #endregion

        #region 内部状态

        private volatile bool _readyFromLoad1YOut;
        private volatile bool _readyFromLoad1YIn;
        private volatile bool _isSend;
        private volatile bool _isFromLoad1;

        #endregion

        public Station_CasselZ(IBlockCutHardware hardware, BlockCutConfig cfg)
            : base(BlockCutConstants.ThreadCasselZName, (int)ThreadId.CasselZ, hardware)
        {
            CasselZFirstPos = cfg.CasselZFirstPos;
            CasselZSpace = cfg.CasselZSpace;
            CasselCount = cfg.CasselCount;
            IsEmptyTest = cfg.IsEmptyTest;
        }

        #region 回调

        public void ReadyFromLoad1YOut() => _readyFromLoad1YOut = true;
        public void ReadyFromLoad1YIn() => _readyFromLoad1YIn = true;
        public void ReadyFromLoad1YGet() { _isFromLoad1 = true; _isSend = false; }
        public void ReadyFromLoad1YSend() { _isFromLoad1 = true; _isSend = true; }

        #endregion

        #region 主循环 — 状态机 (Qt QThread::exec() 对齐)

        private enum CasselZState
        {
            Init,
            WaitLoad1YOut,
            CheckCassel,
            WaitLoad1Request,
            MoveOnce,
            AllConsumed,
        }

        private CasselZState _czState = CasselZState.Init;

        public override async Task RunAsync(CancellationToken token)
        {
            Logger.Info($"[{StationName}] ═══ 工站启动 ═══");
            Logger.Info($"[{StationName}]   空跑模式: {IsEmptyTest}");
            Logger.Info($"[{StationName}]   卡塞总数={CasselCount} 起始序号={CasselIndex} 间距={CasselZSpace:F3}");
            _czState = CasselZState.Init;

            while (!token.IsCancellationRequested)
            {
                CheckPause(token);

                switch (_czState)
                {
                    case CasselZState.Init:
                        EmitMessage("料塔组初始化...");
                        _czState = CasselZState.WaitLoad1YOut;
                        break;

                    case CasselZState.WaitLoad1YOut:
                        await WaitForLoad1YOutAsync(token);
                        _czState = CasselZState.CheckCassel;
                        break;

                    case CasselZState.CheckCassel:
                        if (!CheckIsCassel() && !IsEmptyTest)
                        {
                            CasselIndex = 1;
                            EmitWarning(31000, $"{BlockCutConstants.ThreadCasselZName}暂停，未检测到卡塞,请放入卡塞", true);
                            // stay in this state until user resolves
                        }
                        else
                        {
                            _czState = CasselZState.WaitLoad1Request;
                        }
                        break;

                    case CasselZState.WaitLoad1Request:
                        if (CasselIndex > CasselCount)
                        {
                            CasselIndex = 1;
                            if (!IsEmptyTest)
                                EmitWarning(21001, $"{BlockCutConstants.ThreadCasselZName}暂停，所有料已经取完，请更换卡塞", true);
                            _czState = CasselZState.AllConsumed;
                        }
                        else
                        {
                            await WaitForLoad1Async(token);
                            _czState = CasselZState.MoveOnce;
                        }
                        break;

                    case CasselZState.MoveOnce:
                        await MoveOnceAsync(CasselIndex, token);
                        _czState = CasselZState.WaitLoad1Request;
                        break;

                    case CasselZState.AllConsumed:
                        // wait here until user replaces cassel, then re-check
                        _czState = CasselZState.CheckCassel;
                        break;
                }

                await Task.Delay(1, token);
            }
        }

        #endregion

        #region 单层取送

        private async Task MoveOnceAsync(int nIndex, CancellationToken token)
        {
            EmitMessage("料塔组开始取送治具...");
            bool bIsSendEmpty = _isSend;
            double dPos = CasselZFirstPos - (nIndex - 1) * CasselZSpace;

            if (bIsSendEmpty)
            {
                if (!await ZMoveAbsAsync(dPos - CasselZSpace, $"送料第{nIndex}个位置", token))
                    return;
            }
            else
            {
                if (!await ZMoveAbsAsync(dPos, $"取料第{nIndex}个位置", token))
                    return;
            }

            OnNoticeLoad1CanMove?.Invoke();
            await WaitForLoad1YInAsync(token);

            double currentPos = Motion.GetAxisPos((int)AxisId.CasselZ);
            double offset = bIsSendEmpty ? CasselZSpace : -CasselZSpace;
            string desc = bIsSendEmpty ? "向上运动小段距离" : "向下运动小段距离";

            if (!await ZMoveAbsAsync(currentPos + offset, desc, token))
                return;

            OnNoticeLoad1CanMove?.Invoke();
            await WaitForLoad1YOutAsync(token);

            EmitMessage("料塔组取送治具结束");
        }

        #endregion

        #region 卡塞检测

        private bool CheckIsCassel()
        {
            return GetDI(BlockCutConstants.DI_CheckCassel);
        }

        #endregion

        #region 等待信号

        private async Task WaitForLoad1YOutAsync(CancellationToken token)
        {
            if (IsEmptyTest) return;
            EmitMessage("等待上料组Y轴走出卡塞组到位");
            while (!token.IsCancellationRequested)
            {
                if (_readyFromLoad1YOut) { _readyFromLoad1YOut = false; break; }
                await Task.Delay(1, token);
            }
            EmitMessage("等待结束");
        }

        private async Task WaitForLoad1YInAsync(CancellationToken token)
        {
            if (IsEmptyTest) return;
            EmitMessage("等待上料组Y轴进入卡塞组到位");
            while (!token.IsCancellationRequested)
            {
                if (_readyFromLoad1YIn) { _readyFromLoad1YIn = false; break; }
                await Task.Delay(1, token);
            }
            EmitMessage("等待结束");
        }

        private async Task WaitForLoad1Async(CancellationToken token)
        {
            EmitMessage("等待上料组1通知是否送料");
            while (!token.IsCancellationRequested)
            {
                if (_isFromLoad1) { _isFromLoad1 = false; break; }
                await Task.Delay(1, token);
            }
            EmitMessage("等待结束");
        }

        #endregion

        #region 轴运动

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

        #region 手动操作入口

        public void GroupGetOrSendTray(int nIndex)
        {
            const int timeoutMs = 10_000;
            if (!SpinWait.SpinUntil(() => _readyFromLoad1YOut, timeoutMs))
            {
                Logger.Warning($"[{StationName}] 等待Load1YOut就位超时({timeoutMs}ms)");
                return;
            }
            _readyFromLoad1YOut = false;

            if (!SpinWait.SpinUntil(() => _isFromLoad1, timeoutMs))
            {
                Logger.Warning($"[{StationName}] 等待Load1通知超时({timeoutMs}ms)");
                return;
            }
            _isFromLoad1 = false;

            // 在 ThreadPool 上同步执行，避免 UI 死锁
            Task.Run(() => MoveOnceAsync(nIndex, CancellationToken.None)).GetAwaiter().GetResult();
        }

        #endregion
    }
}
