using System;
using System.Threading;
using System.Threading.Tasks;
using MotionIO;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// 安全监控工站 — 替代 ThreadSafe
    /// 轮询门吸 (DI 31) 和安全光幕 (DI 32)
    /// 触发时: 关闭 UV → 报警 → 暂停生产；恢复时: 通知继续
    /// </summary>
    public class Station_Safe : BlockCutStationBase
    {
        private bool _isSafe = true;
        private DateTime _warnStartTime;
        private string _currentErrCode;

        // IDLE 超时跟踪
        private DateTime _lastProductionTime = DateTime.Now;
        private bool _wasIdleWarning;

        // CT 超时跟踪
        private DateTime _cycleStartTime;
        private bool _isInCycle;

        // 点胶次数跟踪
        private int _glueUsageCount;
        private bool _wasGlueWarning;

        /// <summary>安全恢复 — 通知其余工站继续</summary>
        public event Action OnSafetyRestored;

        public Station_Safe(IBlockCutHardware hardware)
            : base(BlockCutConstants.ThreadSafeName, (int)ThreadId.Safe, hardware)
        {
        }

        /// <summary>记录生产活动 (其他工站调用，重置 IDLE 计时)</summary>
        public void NotifyProductionActivity()
        {
            _lastProductionTime = DateTime.Now;
            _wasIdleWarning = false;
        }

        /// <summary>开始一个生产周期</summary>
        public void BeginCycle()
        {
            _cycleStartTime = DateTime.Now;
            _isInCycle = true;
        }

        /// <summary>结束一个生产周期</summary>
        public void EndCycle()
        {
            _isInCycle = false;
        }

        /// <summary>记录点胶次数 + 检查超限</summary>
        public void IncrementGlueUsage()
        {
            _glueUsageCount++;
            if (_glueUsageCount >= BlockCutConstants.GlueUsageLimit && !_wasGlueWarning)
            {
                _wasGlueWarning = true;
                PauseWarnMessage(0, $"{BlockCutConstants.ErrGlue01}: 点胶次数已达 {_glueUsageCount}，请更换胶筒");
            }
        }

        /// <summary>重置点胶计数 (胶筒更换后)</summary>
        public void ResetGlueUsage()
        {
            _glueUsageCount = 0;
            _wasGlueWarning = false;
        }

        /// <summary>当前点胶次数</summary>
        public int GlueUsageCount => _glueUsageCount;

        /// <summary>
        /// 主循环 — 轮询 DI + IDLE/CT 超时检测
        /// </summary>
        public override async Task RunAsync(CancellationToken token)
        {
            Logger.Info($"[{StationName}] ═══ 工站启动 ═══");
            Logger.Info($"[{StationName}]   空跑模式: {IsEmptyTest}");

            while (!token.IsCancellationRequested)
            {
                // === 安全门/光幕检测 ===
                bool door = IsEmptyTest || GetDI(BlockCutConstants.DI_Door);
                bool safe = IsEmptyTest || GetDI(BlockCutConstants.DI_Safe);

                if (!door || !safe)
                {
                    if (_isSafe)
                    {
                        _isSafe = false;
                        SetDO(BlockCutConstants.DO_UV, false);
                        _warnStartTime = DateTime.Now;

                        if (!door && !safe)
                        {
                            _currentErrCode = BlockCutConstants.ErrDoor01;
                            EmitWarning(0, "[安全提醒]门吸和安全光幕已经打开", true);
                        }
                        else if (!door)
                        {
                            _currentErrCode = BlockCutConstants.ErrDoor02;
                            EmitWarning(0, "[安全提醒]门吸已经打开", true);
                        }
                        else
                        {
                            _currentErrCode = BlockCutConstants.ErrDoor03;
                            EmitWarning(0, "[安全提醒]安全光幕已经打开", true);
                        }

                        Pause();
                    }
                    await Task.Delay(100, token);
                }
                else
                {
                    if (!_isSafe)
                    {
                        _isSafe = true;
                        _currentErrCode = null;

                        Logger.Info($"[Station_Safe] 安全恢复 (停止 {DateTime.Now - _warnStartTime:g})");

                        OnSafetyRestored?.Invoke();
                        EmitMessage("安全已恢复，继续生产");
                    }
                }

                // === IDLE 超时检测 (10 秒无生产活动 → 报警) ===
                if (!IsEmptyTest && !IsPaused && !_wasIdleWarning)
                {
                    double idleSec = (DateTime.Now - _lastProductionTime).TotalSeconds;
                    if (idleSec > BlockCutConstants.IdleTimeoutSec)
                    {
                        _wasIdleWarning = true;
                        EmitWarning(0,
                            $"{BlockCutConstants.ErrIdle01}: 机器空闲超过 {BlockCutConstants.IdleTimeoutSec} 秒", false);
                    }
                }

                // === CT 超时检测 ===
                if (!IsEmptyTest && _isInCycle && !IsPaused)
                {
                    double ctSec = (DateTime.Now - _cycleStartTime).TotalSeconds;
                    if (ctSec > BlockCutConstants.CtTimeoutSec)
                    {
                        EmitWarning(0,
                            $"ERR-RST-01: 生产周期超过 {BlockCutConstants.CtTimeoutSec} 秒", false);
                    }
                }

                await Task.Delay(1, token);
            }
        }
    }
}
