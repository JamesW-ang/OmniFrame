using System;
using System.Drawing;
using System.Windows.Forms;
using OmniFrame.Core;
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    public partial class MonitorForm : Form
    {
        private ISystemManager _systemManager;
        private IDeviceManager _deviceManager;
        private IDataManager _dataManager;
        private IAlarmManager _alarmManager;
        private Timer _updateTimer;
        private bool _isOperatorMode = true;

        public MonitorForm(ISystemManager systemManager, IDeviceManager deviceManager, IDataManager dataManager, IAlarmManager alarmManager)
        {
            InitializeComponent();
            _systemManager = systemManager;
            _deviceManager = deviceManager;
            _dataManager = dataManager;
            _alarmManager = alarmManager;
            ApplyViewMode();

            // Apply theme
            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        private void MonitorForm_Load(object sender, EventArgs e)
        {
            _updateTimer = new Timer { Interval = 500 };
            _updateTimer.Tick += OnUpdateTimerTick;
            _updateTimer.Start();

            if (_systemManager != null)
            {
                _systemManager.StateChanged += OnSystemStateChanged;
                if (_alarmManager != null)
                {
                    _alarmManager.AlarmOccurred += OnAlarmOccurred;
                }
                if (_dataManager != null)
                {
                    _dataManager.StatisticsUpdated += OnStatisticsUpdated;
                }
                UpdateDisplay();
            }

            // 模拟AOI初始数据
            SimulateAOIData();
        }

        private void MonitorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
        }

        /// <summary>
        /// 切换操作员/工程师视图模式
        /// </summary>
        public void SetOperatorMode(bool isOperator)
        {
            _isOperatorMode = isOperator;
            ApplyViewMode();
        }

        private void ApplyViewMode()
        {
            lblModeTag.Text = _isOperatorMode ? "[操作员模式]" : "[工程师模式]";
            lblModeTag.ForeColor = _isOperatorMode
                ? Color.FromArgb(100, 200, 255)
                : Color.FromArgb(255, 200, 100);

            // 工程师模式可见的元素
            bool showEngineer = !_isOperatorMode;
            ApplyTagVisibility(this, showEngineer);
        }

        private static void ApplyTagVisibility(Control parent, bool visible)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.Tag?.ToString() == "EngineerOnly")
                {
                    c.Visible = visible;
                }
                if (c.HasChildren)
                {
                    ApplyTagVisibility(c, visible);
                }
            }
        }

        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void OnSystemStateChanged(object sender, SystemStateChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnSystemStateChanged(sender, e)));
                return;
            }
            UpdateSystemStateDisplay();
        }

        private void OnAlarmOccurred(object sender, OmniFrame.Core.AlarmInfo e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnAlarmOccurred(sender, e)));
                return;
            }
            AddAlarmToList(e);
        }

        private void OnStatisticsUpdated(object sender, ProductionStatistics e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnStatisticsUpdated(sender, e)));
                return;
            }
            UpdateStatsDisplay();
        }

        private void UpdateDisplay()
        {
            UpdateSystemStateDisplay();
            UpdateStatsDisplay();
        }

        private void UpdateSystemStateDisplay()
        {
            if (_systemManager == null) return;

            var state = _systemManager.State;
            lblSystemState.Text = GetStateText(state);
            lblSystemState.ForeColor = GetStateColor(state);

            if (_deviceManager != null)
            {
                var summary = _deviceManager.GetStatusSummary();
                lblDeviceStatus.Text = summary.IsReady ? "设备: 正常" : "设备: 异常";
                lblDeviceStatus.ForeColor = summary.IsReady ? Color.LimeGreen : Color.Red;
            }

            var info = _systemManager.GetSystemInfo();
            lblRunTime.Text = $"运行时间: {info.RunningTime:hh\\:mm\\:ss}";
            lblRecipeName.Text = $"当前配方: {info.CurrentRecipe}";
        }

        private void UpdateStatsDisplay()
        {
            if (_dataManager == null)
            {
                lblTotalInspected.Text = "总检测: 0";
                lblPassCount.Text = "OK: 0";
                lblFailCount.Text = "NG: 0";
                lblPassRate.Text = "良率: 0.00%";
                lblAvgCycleTime.Text = "平均节拍: 0.00s";
                return;
            }

            var stats = _dataManager.GetTodayStatistics();
            lblTotalInspected.Text = $"总检测: {stats.TotalCount}";
            lblPassCount.Text = $"OK: {stats.PassCount}";
            lblFailCount.Text = $"NG: {stats.FailCount}";
            lblPassRate.Text = $"良率: {stats.PassRate:F2}%";
            lblAvgCycleTime.Text = $"平均节拍: {stats.AverageCycleTime:F2}s";

            // 更新大OK/NG指示器
            if (stats.TotalCount > 0)
            {
                double rate = stats.PassRate;
                lblYieldBig.Text = $"良率 {rate:F1}%";
                lblYieldBig.ForeColor = rate >= 95
                    ? Color.FromArgb(0, 140, 60)
                    : rate >= 80
                        ? Color.Orange
                        : Color.Red;
            }
        }

        /// <summary>
        /// 模拟AOI数据 — 后续接入真实检测结果
        /// </summary>
        private void SimulateAOIData()
        {
            var rand = new Random();
            bool isPass = rand.Next(10) > 1;
            lblAOIStatus.Text = isPass ? "OK" : "NG";
            lblAOIStatus.BackColor = isPass ? Color.LimeGreen : Color.Red;
            lblAOIBoardId.Text = $"当前板号: PCB-{DateTime.Now:HHmmss}-{rand.Next(100):D2}";
            lblAOICycleTime.Text = $"检测节拍: {0.8 + rand.NextDouble() * 0.6:F2}s";

            if (!isPass)
            {
                int defectCount = rand.Next(1, 5);
                lblAOIDefectCount.Text = $"缺陷数: {defectCount}";
                listViewDefects.Items.Clear();
                string[] types = { "划痕", "缺件", "偏移", "桥接", "少锡" };
                for (int i = 0; i < defectCount; i++)
                {
                    var item = new ListViewItem(new[]
                    {
                        $"NG-{DateTime.Now:HHmmss}-{i + 1:D2}",
                        types[rand.Next(types.Length)],
                        $"({rand.Next(0, 500)}, {rand.Next(0, 300)})",
                        $"{rand.NextDouble() * 0.5:F2}mm"
                    });
                    item.ForeColor = Color.Red;
                    listViewDefects.Items.Add(item);
                }
            }
            else
            {
                lblAOIDefectCount.Text = "缺陷数: 0";
                listViewDefects.Items.Clear();
            }
        }

        private void AddAlarmToList(OmniFrame.Core.AlarmInfo alarm)
        {
            var item = new ListViewItem(new[]
            {
                alarm.OccurTime.ToString("HH:mm:ss"),
                alarm.AlarmCode,
                alarm.AlarmMessage
            });

            switch (alarm.Level)
            {
                case OmniFrame.Core.AlarmLevel.Warning:
                    item.ForeColor = Color.Orange;
                    break;
                case OmniFrame.Core.AlarmLevel.Error:
                case OmniFrame.Core.AlarmLevel.Critical:
                    item.ForeColor = Color.Red;
                    break;
            }

            listViewAlarms.Items.Insert(0, item);
            while (listViewAlarms.Items.Count > 100)
            {
                listViewAlarms.Items.RemoveAt(listViewAlarms.Items.Count - 1);
            }
        }

        private string GetStateText(SystemState state) => state switch
        {
            SystemState.Idle => "空闲",
            SystemState.Initializing => "初始化中",
            SystemState.Ready => "就绪",
            SystemState.Running => "运行中",
            SystemState.Paused => "暂停",
            SystemState.Error => "错误",
            SystemState.EmergencyStop => "急停",
            _ => "未知"
        };

        private Color GetStateColor(SystemState state) => state switch
        {
            SystemState.Running => Color.LimeGreen,
            SystemState.Ready => Color.LimeGreen,
            SystemState.Paused => Color.Orange,
            SystemState.Error => Color.Red,
            SystemState.EmergencyStop => Color.Red,
            _ => Color.Gray
        };
    }
}
