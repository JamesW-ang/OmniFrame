using System;
using System.Threading;
using System.Threading.Tasks;
using MotionIO;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// 底板取放工站 — 替代 ThreadBottomGetX (底板 X 组)
    /// 控制 BottomGetX 轴，夹爪/推料/挡料/升降 Z 气缸，与 Adjust 协动。
    /// 主流程: 送空底板 → 等待生产 → 取回满板 → 输出满板 → 循环
    /// </summary>
    public class Station_BottomGet : BlockCutStationBase
    {
        #region 参数

        public double BottomXGetEmptyBottom { get; set; }
        public double BottomXSetEmptyBottom { get; set; }
        public double BottomXSetEmptyBottom2 { get; set; }
        public double BottomXGetFullBottom { get; set; }
        public double BottomXSetFullBottom { get; set; }

        #endregion

        #region 事件

        /// <summary>底板已就位 — 通知 Adjust: 取/送完成</summary>
        public event Action OnNoticeBottomOnPos;

        #endregion

        #region 内部状态

        private volatile bool _isReadyFromAdjust;
        private volatile bool _isGetFromAdjust;
        private volatile bool _isReadyFromAdjustPending;

        #endregion

        public Station_BottomGet(IBlockCutHardware hardware, BlockCutConfig cfg)
            : base(BlockCutConstants.ThreadBottomGetName, (int)ThreadId.BottomGet, hardware)
        {
            BottomXGetEmptyBottom = cfg.BottomXGetEmptyBottom;
            BottomXSetEmptyBottom = cfg.BottomXSetEmptyBottom;
            BottomXSetEmptyBottom2 = cfg.BottomXSetEmptyBottom2;
            BottomXGetFullBottom = cfg.BottomXGetFullBottom;
            BottomXSetFullBottom = cfg.BottomXSetFullBottom;
            IsEmptyTest = cfg.IsEmptyTest;
        }

        #region 回调

        /// <summary>Adjust 请求取送底板 (替代 ReadyFromBottom)</summary>
        public void ReadyFromAdjust(bool isGet)
        {
            _isGetFromAdjust = isGet;
            _isReadyFromAdjustPending = true;
            _isReadyFromAdjust = true;
        }

        #endregion

        #region 主循环 — 状态机 (Qt QThread::exec() 对齐)

        private enum BottomGetState
        {
            Init,
            InitCheckBottom,
            SendEmptyInitially,
            GetFull,
            SetFull,
            CheckEmptyAndBlock,
        }

        private BottomGetState _bgState = BottomGetState.Init;

        public override async Task RunAsync(CancellationToken token)
        {
            Logger.Info($"[{StationName}] ═══ 工站启动 ═══");
            Logger.Info($"[{StationName}]   空跑模式: {IsEmptyTest}");
            await InitParamsAsync(token);
            _bgState = BottomGetState.InitCheckBottom;

            while (!token.IsCancellationRequested)
            {
                CheckPause(token);

                switch (_bgState)
                {
                    case BottomGetState.InitCheckBottom:
                        EmitMessage("底板取送组检测初始底板状态...");
                        if (!CheckIsBlock() && CheckIsBottom() && !GetDI(BlockCutConstants.DI_CheckBottom))
                        {
                            _bgState = BottomGetState.SendEmptyInitially;
                        }
                        else if (CheckIsBlock())
                        {
                            PauseWarnMessage(21000,
                                $"{BlockCutConstants.ThreadBottomGetName}暂停，检测到带块料底板,请取走");
                            // stay in this state
                        }
                        else
                        {
                            _bgState = BottomGetState.GetFull;
                        }
                        break;

                    case BottomGetState.SendEmptyInitially:
                        await SendEmptyOnceAsync(true, token);
                        _bgState = BottomGetState.GetFull;
                        break;

                    case BottomGetState.GetFull:
                        EmitMessage("底板取送组 — 取满料底板...");
                        await GetFullOnceAsync(true, token);
                        _bgState = BottomGetState.SetFull;
                        break;

                    case BottomGetState.SetFull:
                        EmitMessage("底板取送组 — 输出满料底板...");
                        await SetFullOnceAsync(token);
                        _bgState = BottomGetState.CheckEmptyAndBlock;
                        break;

                    case BottomGetState.CheckEmptyAndBlock:
                        if (CheckIsBottom() && !CheckIsBlock())
                        {
                            await SetOneCylinder2Async(
                                BlockCutConstants.DO_BottomOutBlockCylinder, true,
                                BlockCutConstants.DI_BottomOutBlockCylinderUp, true,
                                BlockCutConstants.DI_BottomOutBlockCylinderDown, false,
                                $"{BlockCutConstants.ThreadBottomGetName}挡料气缸上升", token);
                        }
                        _bgState = BottomGetState.GetFull;
                        break;
                }

                await Task.Delay(1, token);
            }
        }

        #endregion

        #region 初始化

        private async Task InitParamsAsync(CancellationToken token)
        {
            if (!_isReadyFromAdjustPending)
                _isReadyFromAdjust = false;

            // 夹爪张开
            SetDO(BlockCutConstants.DO_BottomOutClawIn, false);
            SetDO(BlockCutConstants.DO_BottomOutClawOut, true);
            await Task.Delay(1000, token);

            // 推料气缸上升
            SetDO(BlockCutConstants.DO_BottomOutPushCylinder, false);

            // Z 气缸上升
            SetDO(BlockCutConstants.DO_BottomOutZCylinderDown, false);
            SetDO(BlockCutConstants.DO_BottomOutZCylinderUp, true);
            await Task.Delay(1000, token);

            // 缓存固定气缸缩回
            SetDO(BlockCutConstants.DO_BottomYCylinderIn, false);
            await Task.Delay(1000, token);
        }

        #endregion

        #region 送空底板 (替代 SendEmptyOnce)

        private async Task<bool> SendEmptyOnceAsync(bool bIsCheck, CancellationToken token)
        {
            if (bIsCheck)
            {
                while (!token.IsCancellationRequested)
                {
                    if (CheckIsBlock() && !IsEmptyTest)
                    {
                        PauseWarnMessage(21001,
                            $"{BlockCutConstants.ThreadBottomGetName}暂停，检测到带块料底板，请取走并更换为空底板");
                    }
                    else if (!CheckIsBottom() && !IsEmptyTest)
                    {
                        PauseWarnMessage(21002,
                            $"{BlockCutConstants.ThreadBottomGetName}暂停，未检测到空底板，请放入底板");
                    }
                    else
                    {
                        break;
                    }
                    await Task.Delay(1, token);
                }
            }

            // 夹爪夹紧
            await SetCylinderForTwoDoAsync(
                BlockCutConstants.DO_BottomOutClawIn, BlockCutConstants.DO_BottomOutClawOut,
                BlockCutConstants.DI_BottomOutClawIn, true,
                BlockCutConstants.DI_BottomOutClawOut, false,
                $"{BlockCutConstants.ThreadBottomGetName}夹爪夹紧", token);

            // X 轴 → 取空底板位置
            if (!await XMoveAsync(BottomXGetEmptyBottom, "取空底板位置", token))
                return false;

            // 推料气缸下降
            await SetOneCylinder2Async(
                BlockCutConstants.DO_BottomOutPushCylinder, true,
                BlockCutConstants.DI_BottomOutPushCylinderUp, false,
                BlockCutConstants.DI_BottomOutPushCylinderDown, true,
                $"{BlockCutConstants.ThreadBottomGetName}推料气缸下降", token);

            await WaitForAdjustAsync(token);

            // 挡料气缸下降
            await SetOneCylinder2Async(
                BlockCutConstants.DO_BottomOutBlockCylinder, false,
                BlockCutConstants.DI_BottomOutBlockCylinderUp, false,
                BlockCutConstants.DI_BottomOutBlockCylinderDown, true,
                $"{BlockCutConstants.ThreadBottomGetName}挡料气缸下降", token);

            // X 轴 → 送空底板位置
            if (!await XMoveAsync(BottomXSetEmptyBottom, "送空底板位置", token))
                return false;

            // 推料气缸上升
            await SetOneCylinder2Async(
                BlockCutConstants.DO_BottomOutPushCylinder, false,
                BlockCutConstants.DI_BottomOutPushCylinderUp, true,
                BlockCutConstants.DI_BottomOutPushCylinderDown, false,
                $"{BlockCutConstants.ThreadBottomGetName}推料气缸上升", token);

            OnNoticeBottomOnPos?.Invoke();
            EmitMessage("底板取送组送空底板成功");
            return true;
        }

        #endregion

        #region 取回满板 (替代 GetFullOnce)

        private async Task<bool> GetFullOnceAsync(bool bIsCheck, CancellationToken token)
        {
            EmitMessage("底板取送组开始取满料...");

            // X 轴 → 取满料底板位置
            if (!await XMoveAsync(BottomXGetFullBottom, "取满料底板位置", token))
                return false;

            // 夹爪张开
            await SetCylinderForTwoDoAsync(
                BlockCutConstants.DO_BottomOutClawOut, BlockCutConstants.DO_BottomOutClawIn,
                BlockCutConstants.DI_BottomOutClawIn, false,
                BlockCutConstants.DI_BottomOutClawOut, true,
                $"{BlockCutConstants.ThreadBottomGetName}夹爪张开", token);

            await WaitForAdjustAsync(token);

            // Z 气缸下降
            await SetCylinderForTwoDoAsync(
                BlockCutConstants.DO_BottomOutZCylinderDown, BlockCutConstants.DO_BottomOutZCylinderUp,
                BlockCutConstants.DI_BottomOutZCylinderUp, false,
                BlockCutConstants.DI_BottomOutZCylinderDown, true,
                $"{BlockCutConstants.ThreadBottomGetName}Z气缸下降", token);

            // 夹爪夹紧
            await SetCylinderForTwoDoAsync(
                BlockCutConstants.DO_BottomOutClawIn, BlockCutConstants.DO_BottomOutClawOut,
                BlockCutConstants.DI_BottomOutClawIn, true,
                BlockCutConstants.DI_BottomOutClawOut, false,
                $"{BlockCutConstants.ThreadBottomGetName}夹爪夹紧", token);

            // Z 气缸上升
            await SetCylinderForTwoDoAsync(
                BlockCutConstants.DO_BottomOutZCylinderUp, BlockCutConstants.DO_BottomOutZCylinderDown,
                BlockCutConstants.DI_BottomOutZCylinderUp, true,
                BlockCutConstants.DI_BottomOutZCylinderDown, false,
                $"{BlockCutConstants.ThreadBottomGetName}Z气缸上升", token);

            // 检查空底板
            if (bIsCheck)
            {
                while (!token.IsCancellationRequested)
                {
                    if ((CheckIsBottom() && !CheckIsBlock()) || IsEmptyTest)
                        break;
                    PauseWarnMessage(21003,
                        $"{BlockCutConstants.ThreadBottomGetName}暂停，未检测到空底板，请放入空底板");
                    await Task.Delay(1, token);
                }
            }

            // X 轴 → 取空底板位置
            if (!await XMoveAsync(BottomXGetEmptyBottom, "取空底板位置", token))
                return false;

            // 挡料气缸下降
            await SetOneCylinder2Async(
                BlockCutConstants.DO_BottomOutBlockCylinder, false,
                BlockCutConstants.DI_BottomOutBlockCylinderUp, false,
                BlockCutConstants.DI_BottomOutBlockCylinderDown, true,
                $"{BlockCutConstants.ThreadBottomGetName}挡料气缸下降", token);

            // 推料气缸下降
            await SetOneCylinder2Async(
                BlockCutConstants.DO_BottomOutPushCylinder, true,
                BlockCutConstants.DI_BottomOutPushCylinderUp, false,
                BlockCutConstants.DI_BottomOutPushCylinderDown, true,
                $"{BlockCutConstants.ThreadBottomGetName}推料气缸下降", token);

            // X 轴 → 送空底板位置
            if (!await XMoveAsync(BottomXSetEmptyBottom, "送空底板位置", token))
                return false;

            // 推料气缸上升
            await SetOneCylinder2Async(
                BlockCutConstants.DO_BottomOutPushCylinder, false,
                BlockCutConstants.DI_BottomOutPushCylinderUp, true,
                BlockCutConstants.DI_BottomOutPushCylinderDown, false,
                $"{BlockCutConstants.ThreadBottomGetName}推料气缸上升", token);

            OnNoticeBottomOnPos?.Invoke();
            EmitMessage("底板取送组取满料成功");
            return true;
        }

        #endregion

        #region 输出满板 (替代 SetFullOnce)

        private async Task<bool> SetFullOnceAsync(CancellationToken token)
        {
            EmitMessage("底板取送组开始放满料...");

            // X 轴 → 送满料底板位置
            if (!await XMoveAsync(BottomXSetFullBottom, "送满料底板位置", token))
                return false;

            // Z 气缸下降
            await SetCylinderForTwoDoAsync(
                BlockCutConstants.DO_BottomOutZCylinderDown, BlockCutConstants.DO_BottomOutZCylinderUp,
                BlockCutConstants.DI_BottomOutZCylinderUp, false,
                BlockCutConstants.DI_BottomOutZCylinderDown, true,
                $"{BlockCutConstants.ThreadBottomGetName}Z气缸下降", token);

            // 缓存固定气缸伸出
            await SetOneCylinderAsync(
                BlockCutConstants.DO_BottomYCylinderIn, true,
                BlockCutConstants.DI_BottomYCylinderIn, false,
                $"{BlockCutConstants.ThreadBottomGetName}料板缓存固定气缸伸出", token);

            // 夹爪张开
            await SetCylinderForTwoDoAsync(
                BlockCutConstants.DO_BottomOutClawOut, BlockCutConstants.DO_BottomOutClawIn,
                BlockCutConstants.DI_BottomOutClawIn, false,
                BlockCutConstants.DI_BottomOutClawOut, true,
                $"{BlockCutConstants.ThreadBottomGetName}夹爪张开", token);

            // Z 气缸上升
            await SetCylinderForTwoDoAsync(
                BlockCutConstants.DO_BottomOutZCylinderUp, BlockCutConstants.DO_BottomOutZCylinderDown,
                BlockCutConstants.DI_BottomOutZCylinderUp, true,
                BlockCutConstants.DI_BottomOutZCylinderDown, false,
                $"{BlockCutConstants.ThreadBottomGetName}Z气缸上升", token);

            if (IsEmptyTest)
                return true;

            // X 轴 → 取满料底板位置
            if (!await XMoveAsync(BottomXGetFullBottom, "取满料底板位置", token))
                return false;

            while (!token.IsCancellationRequested)
            {
                if (CheckIsBottom())
                    break;
                await Task.Delay(1, token);
            }

            EmitMessage("底板取送组放满料成功");

            if (CheckIsBlock())
            {
                PauseWarnMessage(21004,
                    $"{BlockCutConstants.ThreadBottomGetName}暂停,请取走满料底板，并更换空底板");
            }
            else
            {
                PauseWarnMessage(21005,
                    $"{BlockCutConstants.ThreadBottomGetName}暂停,请更换空底板");
            }

            return true;
        }

        #endregion

        #region 检测

        private bool CheckIsBottom() => GetDI(BlockCutConstants.DI_BottomOutCheckBottom);

        private bool CheckIsBlock() => GetDI(BlockCutConstants.DI_BottomOutCheckBlock);

        #endregion

        #region 等待信号

        private async Task WaitForAdjustAsync(CancellationToken token)
        {
            EmitMessage("等待底板组通知取送底板");
            while (!token.IsCancellationRequested)
            {
                if (_isReadyFromAdjust) 
                { 
                    _isReadyFromAdjust = false; 
                    _isReadyFromAdjustPending = false;
                    break; 
                }
                await Task.Delay(1, token);
            }
            EmitMessage("等待结束");
        }

        #endregion

        #region 轴运动

        private async Task<bool> XMoveAsync(double targetPos, string desc, CancellationToken token)
        {
            CheckPause(token);
            while (!token.IsCancellationRequested)
            {
                int result = await OneAxisMoveAbsAsync((int)AxisId.BottomGetX, targetPos,
                    $"{BlockCutConstants.ThreadBottomGetName}X轴到{desc}", token: token);
                if (result == 1) return true;
                await Task.Delay(1, token);
            }
            return false;
        }

        #endregion

        #region 手动操作入口

        public void GroupPushEmpty()
        {
            SendEmptyOnceAsync(false, CancellationToken.None).GetAwaiter().GetResult();
        }

        public void GroupGetFull()
        {
            GetFullOnceAsync(false, CancellationToken.None).GetAwaiter().GetResult();
        }

        public void GroupSetFull()
        {
            SetFullOnceAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        #endregion
    }
}
