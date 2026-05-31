using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// 工站协调器 — 管理 6 个 BlockCut 工站的生命周期和跨工站信号连接。
    /// 替代 BlockCutMainForm 直接管理工站，使业务逻辑可测试、UI 职责清晰。
    /// </summary>
    public class StationCoordinator
    {
        private readonly Station_Adjust _adjust;
        private readonly Station_CasselZ _casselZ;
        private readonly Station_Load _load;
        private readonly Station_Load2 _load2;
        private readonly Station_BottomGet _bottomGet;
        private readonly Station_Safe _safe;

        private List<Task> _stationTasks = new();
        private CancellationTokenSource _cts;
        private bool _isRunning;

        public bool IsRunning => _isRunning;

        /// <summary>日志消息（供 UI 显示）</summary>
        public event Action<string> OnLog;
        /// <summary>产量增加</summary>
        public event Action OnOutputIncremented;

        public StationCoordinator(
            Station_Adjust adjust,
            Station_CasselZ casselZ,
            Station_Load load,
            Station_Load2 load2,
            Station_BottomGet bottomGet,
            Station_Safe safe)
        {
            _adjust = adjust ?? throw new ArgumentNullException(nameof(adjust));
            _casselZ = casselZ ?? throw new ArgumentNullException(nameof(casselZ));
            _load = load ?? throw new ArgumentNullException(nameof(load));
            _load2 = load2 ?? throw new ArgumentNullException(nameof(load2));
            _bottomGet = bottomGet ?? throw new ArgumentNullException(nameof(bottomGet));
            _safe = safe ?? throw new ArgumentNullException(nameof(safe));

            WireCrossStationEvents();
        }

        /// <summary>启动所有工站</summary>
        public void StartAll()
        {
            if (_isRunning) return;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            const int maxRetries = 3;

            _stationTasks.Clear();
            _stationTasks.Add(Task.Run(() =>
                BlockCutStationBase.RunWithAutoRecoveryAsync("Adjust", ct => _adjust.RunAsync(ct), maxRetries, token), token));
            _stationTasks.Add(Task.Run(() =>
                BlockCutStationBase.RunWithAutoRecoveryAsync("CasselZ", ct => _casselZ.RunAsync(ct), maxRetries, token), token));
            _stationTasks.Add(Task.Run(() =>
                BlockCutStationBase.RunWithAutoRecoveryAsync("Load", ct => _load.RunAsync(ct), maxRetries, token), token));
            _stationTasks.Add(Task.Run(() =>
                BlockCutStationBase.RunWithAutoRecoveryAsync("Load2", ct => _load2.RunAsync(ct), maxRetries, token), token));
            _stationTasks.Add(Task.Run(() =>
                BlockCutStationBase.RunWithAutoRecoveryAsync("BottomGet", ct => _bottomGet.RunAsync(ct), maxRetries, token), token));
            _stationTasks.Add(Task.Run(() =>
                BlockCutStationBase.RunWithAutoRecoveryAsync("Safe", ct => _safe.RunAsync(ct), maxRetries, token), token));

            _isRunning = true;
            Log("所有工站已启动");
        }

        /// <summary>停止所有工站</summary>
        public void StopAll()
        {
            if (!_isRunning) return;
            _cts?.Cancel();
            _isRunning = false;
            PauseAll();
            Log("所有工站已停止");
        }

        /// <summary>暂停所有工站</summary>
        public void PauseAll()
        {
            _adjust.Pause();
            _casselZ.Pause();
            _load.Pause();
            _load2.Pause();
            _bottomGet.Pause();
            _safe.Pause();
            _isRunning = false;
            Log("所有工站已暂停");
        }

        /// <summary>恢复所有工站</summary>
        public void ResumeAll()
        {
            _adjust.Resume();
            _casselZ.Resume();
            _load.Resume();
            _load2.Resume();
            _bottomGet.Resume();
            _safe.Resume();
            _isRunning = true;
            Log("所有工站已恢复");
        }

        /// <summary>获取所有工站（供 UI 状态显示）</summary>
        public BlockCutStationBase[] GetStations() => new BlockCutStationBase[] {
            _adjust, _casselZ, _load, _load2, _bottomGet, _safe
        };

        /// <summary>设置空跑模式</summary>
        public void SetEmptyTest(bool enabled)
        {
            _adjust.IsEmptyTest = enabled;
            _casselZ.IsEmptyTest = enabled;
            _load.IsEmptyTest = enabled;
            _load2.IsEmptyTest = enabled;
            _bottomGet.IsEmptyTest = enabled;
            _safe.IsEmptyTest = enabled;
        }

        /// <summary>获取工站（供事件订阅等需要直接访问的场景）</summary>
        public Station_Adjust Adjust => _adjust;
        public Station_CasselZ CasselZ => _casselZ;
        public Station_Load Load => _load;
        public Station_Load2 Load2 => _load2;
        public Station_BottomGet BottomGet => _bottomGet;
        public Station_Safe Safe => _safe;

        /// <summary>订阅所有工站的事件（供外部绑定 UI 回调）</summary>
        public void SubscribeStationEvents(Action<BlockCutStationBase> onEach)
        {
            onEach(_adjust);
            onEach(_casselZ);
            onEach(_load);
            onEach(_load2);
            onEach(_bottomGet);
            onEach(_safe);
        }

        // ──────────────────────────────────────────────
        // 跨工站信号连接 (替代 Qt 信号/槽)
        // ──────────────────────────────────────────────
        private void WireCrossStationEvents()
        {
            // UPH 追踪
            _adjust.OnUpdateUPH += (jigCode, bottomCode, blockCode, pos, mesFileId) =>
                OnOutputIncremented?.Invoke();

            // 安全恢复 → 自动重启
            _safe.OnSafetyRestored += () =>
            {
                Log("安全已恢复");
                ResumeAll();
            };

            // Station_Load → Station_CasselZ
            _load.OnNoticeCasselLoad1Out += () => _casselZ.ReadyFromLoad1YOut();
            _load.OnNoticeCasselLoad1In += () => _casselZ.ReadyFromLoad1YIn();
            _load.OnNoticeCasselLoad1Get += () => _casselZ.ReadyFromLoad1YGet();
            _load.OnNoticeCasselLoad1Send += () => _casselZ.ReadyFromLoad1YSend();

            // Station_Load → Station_Load2 (传送治具码)
            _load.OnNoticeLoad2CanGetSlice += (jigCode) => _load2.ReadyFromLoad1OnPos(jigCode);
            // Station_Load → 扫码请求 (外部通过 BarcodeScanned 触发)
            _load.OnNoticeSweepCode += () => OnSweepRequested?.Invoke(2);

            // Station_Load2 → Station_Load
            _load2.OnNoticeLoad1CanMove += () => _load.ReadyFromLoad2OnPos();

            // Station_Adjust → Station_Load2
            _adjust.OnNoticeLoad2CanMove += () => _load2.ReadyFromBottomOnPos();

            // Station_Load2 → Station_Adjust (传送治具码 + 块料码)
            _load2.OnNoticeBottomCanMove += (jigCode, blockCode) =>
                _adjust.OnLoad2Ready(jigCode, blockCode);

            // Station_Load2 → 扫码请求
            _load2.OnNoticeSweepCode += () => OnSweepRequested?.Invoke(3);

            // Station_CasselZ → Station_Load
            _casselZ.OnNoticeLoad1CanMove += () => _load.ReadyFromCasselOnPos();

            // Station_Adjust ↔ Station_BottomGet (底板取放)
            _adjust.OnNoticeBottomXCanMove += (isGet) =>
            {
                if (isGet) BlockCutStationBase.SetDryDI(BlockCutConstants.DI_CheckBottom, false);
                _bottomGet.ReadyFromAdjust(isGet);
            };
            _bottomGet.OnNoticeBottomOnPos += () =>
            {
                BlockCutStationBase.SetDryDI(BlockCutConstants.DI_CheckBottom, true);
                _adjust.ReadyFromBottomXOnPos();
            };
        }

        /// <summary>扫码请求（target: 2=Load料盘, 3=Load2底板），UI 负责显示对话框并调用 DispatchBarcode</summary>
        public event Action<int> OnSweepRequested;

        /// <summary>分发扫码结果到对应工站</summary>
        public void DispatchBarcode(int target, string code)
        {
            if (target == 2)
                _load.ReadyFromSweepCode(code);
            else if (target == 3)
                _load2.ReadyFromSweepCode(code);
            Log($"扫码: {code}");
        }

        private void Log(string msg)
        {
            Logger.Info($"[StationCoordinator] {msg}");
            OnLog?.Invoke(msg);
        }
    }
}
