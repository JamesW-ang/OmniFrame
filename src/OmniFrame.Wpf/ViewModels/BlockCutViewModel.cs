using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MotionIO;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.Core.AdvancedFeatures;
using OmniFrame.Core.BlockCut;

namespace OmniFrame.Wpf.ViewModels
{
    public class BlockCutViewModel : ViewModelBase
    {
        private readonly StationCoordinator _coordinator;
        private readonly IAlarmManager _alarmMgr;
        private readonly ISystemManager _systemMgr;
        private readonly BlockCutConfig _cfg;
        private readonly IUphManager _uphMgr;
        private readonly Motion _motion;
        private readonly IoCtrl _io;

        public ObservableCollection<StationStatusItem> Stations { get; } = new();
        public ObservableCollection<AlarmLogItem> AlarmLogs { get; } = new();

        // 状态
        private string _statusText = "● 待机"; public string StatusText { get => _statusText; set => Set(ref _statusText, value); }
        private string _uphText = "UPH: 0";    public string UphText { get => _uphText; set => Set(ref _uphText, value); }
        private string _timeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); public string TimeText { get => _timeText; set => Set(ref _timeText, value); }
        private string _workInfo = "工单: -- | 机台: --"; public string WorkInfo { get => _workInfo; set => Set(ref _workInfo, value); }
        private string _alarmCode;             public string AlarmCode { get => _alarmCode; set => Set(ref _alarmCode, value); }
        private string _alarmMessage;          public string AlarmMessage { get => _alarmMessage; set => Set(ref _alarmMessage, value); }
        private bool _isRunning;               public bool IsRunning { get => _isRunning; set => Set(ref _isRunning, value); }
        private bool _isEmptyTest;             public bool IsEmptyTest { get => _isEmptyTest; set { if (Set(ref _isEmptyTest, value)) _coordinator.SetEmptyTest(value); } }
        private int _totalOutput;              public int TotalOutput { get => _totalOutput; set => Set(ref _totalOutput, value); }

        // 命令
        public ICommand StartCommand { get; } public ICommand PauseCommand { get; } public ICommand StopCommand { get; } public ICommand ResetCommand { get; }

        // ── JOG 手动轴控制 ──
        public ObservableCollection<string> AxisNames { get; } = new();
        private int _selectedAxis;             public int SelectedAxis { get => _selectedAxis; set => Set(ref _selectedAxis, value); }
        private string _jogTarget = "0";       public string JogTarget { get => _jogTarget; set => Set(ref _jogTarget, value); }
        private string _jogSpeed = "50";       public string JogSpeed { get => _jogSpeed; set => Set(ref _jogSpeed, value); }
        private string _currentPos = "--";     public string CurrentPos { get => _currentPos; set => Set(ref _currentPos, value); }
        private bool _isJogExpanded;           public bool IsJogExpanded { get => _isJogExpanded; set => Set(ref _isJogExpanded, value); }
        public ICommand JogAbsCommand { get; } public ICommand JogRelCommand { get; } public ICommand JogStopCommand { get; }

        // ── 气缸覆写 ──
        public ObservableCollection<CylinderItem> Cylinders { get; } = new();
        private bool _isCylinderExpanded;      public bool IsCylinderExpanded { get => _isCylinderExpanded; set => Set(ref _isCylinderExpanded, value); }

        // ── IO 信号 ──
        public ObservableCollection<IOSignalItem> DISignals { get; } = new();
        public ObservableCollection<IOSignalItem> DOSignals { get; } = new();
        private bool _isIoExpanded;            public bool IsIoExpanded { get => _isIoExpanded; set => Set(ref _isIoExpanded, value); }

        // ── 扫码 ──
        public ObservableCollection<string> ScanLogBottom { get; } = new();
        public ObservableCollection<string> ScanLogSlice { get; } = new();
        private bool _isScanExpanded;          public bool IsScanExpanded { get => _isScanExpanded; set => Set(ref _isScanExpanded, value); }

        public BlockCutViewModel(
            StationCoordinator coordinator, IAlarmManager alarmMgr, ISystemManager systemMgr,
            BlockCutConfig cfg, IUphManager uphMgr,
            Motion motion, IoCtrl io)
        {
            _coordinator = coordinator;
            _alarmMgr = alarmMgr;
            _systemMgr = systemMgr;
            _cfg = cfg;
            _uphMgr = uphMgr;
            _motion = motion;
            _io = io;

            StartCommand = new RelayCommand(StartAll);
            PauseCommand = new RelayCommand(PauseAll);
            StopCommand = new RelayCommand(StopAll);
            ResetCommand = new RelayCommand(() => _systemMgr.Reset());
            JogAbsCommand = new RelayCommand(JogAbsolute);
            JogRelCommand = new RelayCommand(JogRelative);
            JogStopCommand = new RelayCommand(JogStop);

            _isEmptyTest = cfg.IsSimulation;
            InitAxisList();
            InitCylinderList();
            InitIOSignals();
            WireEvents();
            RefreshStationList();
        }

        private void InitAxisList() { for (int i = 1; i <= 8; i++) AxisNames.Add($"轴 {i}"); }
        private void InitCylinderList()
        {
            var list = new[] { ("治具移送Y", 0), ("治具移送Z", 2), ("产品上料Z", 3), ("产品上料真空", 4), ("底板夹紧", 5), ("片源气缸", 6), ("UV气缸", 7), ("夹爪夹紧", 16), ("推料气缸", 17) };
            foreach (var (name, idx) in list) Cylinders.Add(new CylinderItem(name, idx, _io));
        }
        private void InitIOSignals()
        {
            for (int i = 0; i < 16; i++) { DISignals.Add(new IOSignalItem(i)); DOSignals.Add(new IOSignalItem(i)); }
        }

        private void WireEvents()
        {
            _alarmMgr.AlarmOccurred += (s, e) => System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                AlarmCode = e.AlarmCode; AlarmMessage = e.AlarmMessage;
                AlarmLogs.Insert(0, new AlarmLogItem { Time = DateTime.Now.ToString("HH:mm:ss"), Code = e.AlarmCode ?? "", Message = e.AlarmMessage ?? "" });
                if (AlarmLogs.Count > 50) AlarmLogs.RemoveAt(AlarmLogs.Count - 1);
            });
            _alarmMgr.AlarmCleared += (s, e) => System.Windows.Application.Current.Dispatcher.Invoke(() => { AlarmCode = null; AlarmMessage = null; });
            _coordinator.OnOutputIncremented += () => System.Windows.Application.Current.Dispatcher.Invoke(() => { TotalOutput++; _uphMgr.RecordProduction("DefaultLine", 1); });
            _coordinator.OnLog += msg => System.Windows.Application.Current.Dispatcher.Invoke(() => StatusText = $"● {msg}");
        }

        private void StartAll() { _coordinator.StartAll(); IsRunning = true; StatusText = "● 运行中"; }
        private void PauseAll() { _coordinator.PauseAll(); IsRunning = false; StatusText = "● 已暂停"; }
        private void StopAll() { _coordinator.StopAll(); IsRunning = false; StatusText = "● 待机"; }
        private void JogAbsolute() { if (double.TryParse(JogTarget, out var t) && double.TryParse(JogSpeed, out var s)) try { _motion?.AbsMove(SelectedAxis + 1, t, s); CurrentPos = $"{t:F3}"; } catch (Exception ex) { Logger.Warning($"JOG失败: {ex.Message}"); } }
        private void JogRelative() { if (double.TryParse(JogTarget, out var t) && double.TryParse(JogSpeed, out var s)) try { _motion?.RelativeMove(SelectedAxis + 1, t, s); } catch (Exception ex) { Logger.Warning($"JOG失败: {ex.Message}"); } }
        private void JogStop() { try { _motion?.StopAxis(SelectedAxis + 1); } catch { } }

        private void RefreshStationList()
        {
            Stations.Clear();
            foreach (var s in _coordinator.GetStations())
                Stations.Add(new StationStatusItem { Name = s.StationName, IsPaused = s.IsPaused, IsRunning = s.IsRunning, IsEmptyTest = s.IsEmptyTest });
        }
    }

    public class StationStatusItem { public string Name { get; set; } public bool IsPaused { get; set; } public bool IsRunning { get; set; } public bool IsEmptyTest { get; set; } public string StatusText => IsPaused ? "⏸ 暂停" : IsRunning ? "▶ 运行" : "■ 待机"; }
    public class AlarmLogItem { public string Time { get; set; } public string Code { get; set; } public string Message { get; set; } }
    public class CylinderItem { public string Name { get; } public int DoIndex { get; } private readonly IoCtrl _io; public CylinderItem(string name, int idx, IoCtrl io) { Name = name; DoIndex = idx; _io = io; OnCommand = new RelayCommand(() => _io?.SetDO(DoIndex, true)); OffCommand = new RelayCommand(() => _io?.SetDO(DoIndex, false)); } public ICommand OnCommand { get; } public ICommand OffCommand { get; } }
    public class IOSignalItem { public int Index { get; } private bool _value; public bool Value { get => _value; set => _value = value; } public IOSignalItem(int idx) { Index = idx; } }
}
