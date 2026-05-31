using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MotionIO;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.Core.AdvancedFeatures;
using OmniFrame.Core.BlockCut;
using OmniFrame.Dialogs;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// BlockCut 生产主界面 — 嵌入 MainForm.panel_Main 的子窗体。
    /// 替代 Qt MainWindow(.h/.cpp/.ui, ~2228 行)。
    ///
    /// 嵌入方式: 由 MainForm.ShowCachedForm&lt;BlockCutMainForm&gt;() 加载，
    /// 设置 TopLevel=false, FormBorderStyle=None, Dock=Fill 嵌入 panel_Main。
    /// </summary>
    public partial class BlockCutMainForm : Form
    {
        #region DI 注入

        private readonly ISystemManager _systemMgr;
        private readonly IAlarmManager _alarmMgr;
        private readonly IMotionManager _motionMgr;
        private readonly IIoManager _ioMgr;
        private readonly IStationManager _stationManager;
        private readonly MotionIO.Motion _motion;
        private readonly MotionIO.IoCtrl _io;
        private readonly BlockCutVision _vision;
        private readonly BlockCutMesClient _mesClient;
        private readonly BarcodeScannerClient _barcodeClient;
        private readonly BlockCutConfig _cfg;
        private readonly IUphManager _uphManager;
        private readonly StationCoordinator _coordinator;

        #endregion

        #region 控件

        private TableLayoutPanel _mainLayout;

        // 顶栏
        private Panel _topBar;
        private Label _lblTime, _lblUPH, _lblStatus, _lblRole;

        // 相机区 (4 路)
        private TableLayoutPanel _cameraGrid;
        private Panel[] _cameraPanels = new Panel[4];

        // 控制区
        private Panel _controlPanel;
        private Button _btnStart, _btnPause, _btnStop, _btnReset;
        private Label _lblWorkTitle;

        // 底栏
        private Panel _bottomBar;
        private Label _lblWorkInfo, _lblAlarm;

        // 定时器
        private System.Windows.Forms.Timer _timer;

        // 空跑 / 安全
        private CheckBox _chkEmptyTest;
        private CheckBox _chkCloseSafe;
        private DateTime _closeSafeTimeStart;

        // 扫码记录
        private ListBox _scanListBottom;
        private ListBox _scanListSlice;

        // 工站状态仪表板
        private TableLayoutPanel _stationStatusGrid;
        private Label[] _stationStatusLights;
        private Label[] _stationStatusLabels;

        // 报警日志
        private ListView _alarmListView;

        // 手动轴控制
        private Panel _jogPanel;
        private ComboBox _cmbAxisSelect;
        private TextBox _txtJogTarget;
        private NumericUpDown _numJogSpeed;
        private Label _lblCurPos;
        private Button _btnJogAbs, _btnJogRel, _btnJogStop;
        private bool _jogPanelVisible;

        // 气缸覆写
        private Panel _cylinderPanel;
        private bool _cylinderPanelVisible;

        // IO 信号监视 (空跑)
        private Panel _ioPanel;
        private bool _ioPanelVisible;
        private TableLayoutPanel _ioGrid;
        private Label[,] _ioLabels; // [type, index] type: 0=DI, 1=DO

        #endregion

        #region 状态

        private string _workId, _machineId;
        private UserType _currentRole = UserType.Operator;
        private int _totalOutput;
        private double _uph;
        private DateTime _startTime;
        private string _currentAlarmCode, _currentAlarmMsg;
        private DateTime _lastUphUpdate = DateTime.Now;
        private volatile int _sweepTarget; // 0=none, 1=Adjust, 2=Load, 3=Load2
        private readonly Random _random = new Random();
        private bool _isFormReady;

        #endregion

        public BlockCutMainForm(
            ISystemManager systemMgr,
            IAlarmManager alarmMgr,
            IMotionManager motionMgr,
            IIoManager ioMgr,
            IStationManager stationManager,
            MotionIO.Motion motion,
            MotionIO.IoCtrl io,
            BlockCutVision vision,
            BlockCutMesClient mesClient,
            BarcodeScannerClient barcodeClient,
            BlockCutConfig cfg,
            IUphManager uphManager,
            StationCoordinator coordinator)
        {
            _systemMgr = systemMgr;
            _alarmMgr = alarmMgr;
            _motionMgr = motionMgr;
            _ioMgr = ioMgr;
            _stationManager = stationManager;
            _motion = motion;
            _io = io;
            _vision = vision;
            _mesClient = mesClient;
            _barcodeClient = barcodeClient;
            _cfg = cfg;
            _uphManager = uphManager;
            _coordinator = coordinator;

            LogInfo("BlockCutMainForm 初始化开始...");
            LogInfo($"  机台: {_machineId ?? "未设置"}");
            LogInfo($"  工单: {_workId ?? "未设置"}");
            LogInfo($"  空跑模式: {_cfg.IsEmptyTest}");

            InitializeComponent();
            LogInfo("  UI构建完成");

            WireEvents();
            LogInfo("  事件绑定完成");

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();

            this.Load += (s, e) =>
            {
                _isFormReady = true;
                _timer.Start();
                LogInfo("BlockCutMainForm 就绪，定时器已启动");
            };

            LogInfo("BlockCutMainForm 初始化完成");
        }

        [System.Obsolete("For Designer only - use DI constructor")]
        public BlockCutMainForm()
        {
            InitializeComponent();
        }

        #region 界面构建

        private void InitializeComponent()
        {
            Text = "BlockCut 生产控制";
            BackColor = Color.FromArgb(40, 40, 40);
            ForeColor = Color.White;
            Font = new Font("Microsoft YaHei", 9F);
            KeyPreview = true;
            KeyDown += OnKeyDown;

            BuildLayout();
        }

        private void BuildLayout()
        {
            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(4),
            };
            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            // === 顶栏 ===
            _topBar = new Panel { BackColor = Color.FromArgb(55, 55, 55), Dock = DockStyle.Fill };
            _lblTime = MakeLabel("yyyy-MM-dd HH:mm:ss", 12, 10);
            _lblUPH = MakeLabel("UPH: 0", 300, 10, Color.Lime);
            _lblStatus = MakeLabel("● 待机", 430, 10, Color.Gray);
            _lblRole = MakeLabel("操作员", 560, 10, Color.Cyan);
            _topBar.Controls.AddRange(new Control[] { _lblTime, _lblUPH, _lblStatus, _lblRole });

            // === 4 路相机 ===
            _cameraGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.FromArgb(25, 25, 25),
            };
            _cameraGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            _cameraGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            _cameraGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            _cameraGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            string[] camNames = { "相机1 — 底板右侧", "相机2 — 底板左侧", "相机3 — 片源", "相机4 — 辅助" };
            for (int i = 0; i < 4; i++)
            {
                int idx = i; // 创建局部变量保存索引值
                _cameraPanels[idx] = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(20, 20, 20),
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(2),
                };
                _cameraPanels[idx].Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    var r = (s as Panel).ClientRectangle;
                    g.DrawString(camNames[idx], Font, Brushes.DarkGray, r, new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                    });
                };
                _cameraGrid.Controls.Add(_cameraPanels[idx], idx % 2, idx / 2);
            }

            // === 控制区 ===
            _controlPanel = new Panel { BackColor = Color.FromArgb(45, 45, 45), Dock = DockStyle.Fill, AutoScroll = true };

            _lblWorkTitle = new Label
            {
                Text = "生产控制",
                Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 16),
                AutoSize = true,
            };

            _btnStart = MakeButton("▶ 启动 (F5)", 16, 60, Color.FromArgb(0, 140, 0));
            _btnPause = MakeButton("⏸ 暂停 (F1)", 16, 110, Color.FromArgb(180, 140, 0));
            _btnStop = MakeButton("⏹ 停止 (F6)", 16, 160, Color.FromArgb(180, 60, 0));
            _btnReset = MakeButton("↺ 复位 (F3)", 16, 210, Color.FromArgb(60, 60, 140));

            _btnStart.Click += (s, e) => StartAllStations();
            _btnPause.Click += (s, e) => PauseAll();
            _btnStop.Click += (s, e) => StopAllStations();
            _btnReset.Click += (s, e) => _stationManager?.ResetAllStation();

            // 空跑 / 关闭安全监测 (对应 Qt checkBox_2 / checkBox)
            _chkEmptyTest = new CheckBox
            {
                Text = "空跑",
                Location = new Point(16, 260),
                Size = new Size(100, 24),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 45, 45),
                Font = new Font("Microsoft YaHei", 10F),
            };
            _chkEmptyTest.CheckedChanged += (s, e) =>
            {
                bool isEmpty = _chkEmptyTest.Checked;
                _vision.IsEmptyTest = isEmpty;
                _mesClient.SimulationMode = isEmpty;
                _coordinator.SetEmptyTest(isEmpty);
                LogInfo(isEmpty ? "空跑模式: ON (所有工站硬件跳过, MES仿真)" : "空跑模式: OFF");
            };

            _chkCloseSafe = new CheckBox
            {
                Text = "关闭安全监测(5min自动恢复)",
                Location = new Point(16, 286),
                Size = new Size(200, 24),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 45, 45),
                Font = new Font("Microsoft YaHei", 8F),
            };
            _chkCloseSafe.CheckedChanged += (s, e) =>
            {
                if (_chkCloseSafe.Checked) _closeSafeTimeStart = DateTime.Now;
                LogInfo(_chkCloseSafe.Checked ? "安全监测已关闭 (5min后自动恢复)" : "安全监测已恢复");
            };

            // 图片记录设置
            var btnImageSave = new Button
            {
                Text = "图片记录",
                Location = new Point(16, 312),
                Size = new Size(160, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 100),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 9F),
            };
            btnImageSave.Click += (s, e) =>
            {
                using var dlg = new ImageSaveDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    LogInfo($"图片记录设置: 类型={dlg.SaveType}, 数量={dlg.MaxCount}, 有效期={dlg.ValidityDays}天");
            };

            // 扫码记录
            var lblScanTitle = new Label
            {
                Text = "━━━ 扫码记录 ━━━",
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 160, 160),
                Location = new Point(16, 355),
                AutoSize = true,
            };
            _scanListBottom = new ListBox
            {
                Location = new Point(16, 378),
                Size = new Size(170, 50),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Cyan,
                Font = new Font("Microsoft YaHei", 7F),
                BorderStyle = BorderStyle.FixedSingle,
            };
            var lblScanBottom = new Label
            {
                Text = "底板扫码:",
                Location = new Point(16, 430),
                ForeColor = Color.Gray,
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 7F),
            };
            _scanListSlice = new ListBox
            {
                Location = new Point(16, 445),
                Size = new Size(170, 50),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Lime,
                Font = new Font("Microsoft YaHei", 7F),
                BorderStyle = BorderStyle.FixedSingle,
            };
            var lblScanSlice = new Label
            {
                Text = "片源扫码:",
                Location = new Point(16, 498),
                ForeColor = Color.Gray,
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 7F),
            };

            _controlPanel.Controls.AddRange(new Control[] {
                _lblWorkTitle, _btnStart, _btnPause, _btnStop, _btnReset,
                _chkEmptyTest, _chkCloseSafe, btnImageSave,
                lblScanTitle, _scanListBottom, lblScanBottom, _scanListSlice, lblScanSlice,
            });

            // === 工站状态仪表板 (Task 6) ===
            BuildStationStatusPanel();

            // === 报警日志 (Task 7) ===
            BuildAlarmLogPanel();

            // === 手动轴控制 (Task 8) ===
            BuildJogPanel();

            // === 气缸覆写 (Task 9) ===
            BuildCylinderPanel();

            // === IO 信号监视 (空跑) ===
            BuildIOPanel();

            // === 底栏 ===
            _bottomBar = new Panel { BackColor = Color.FromArgb(55, 55, 55), Dock = DockStyle.Fill };
            _lblWorkInfo = MakeLabel("工单: -- | 机台: --", 8, 8);
            _lblAlarm = MakeLabel("", 500, 8, Color.Red);
            _bottomBar.Controls.AddRange(new Control[] { _lblWorkInfo, _lblAlarm });

            // === 组装 ===
            _mainLayout.Controls.Add(_topBar, 0, 0);
            _mainLayout.SetColumnSpan(_topBar, 2);
            _mainLayout.Controls.Add(_cameraGrid, 0, 1);
            _mainLayout.Controls.Add(_controlPanel, 1, 1);
            _mainLayout.Controls.Add(_bottomBar, 0, 2);
            _mainLayout.SetColumnSpan(_bottomBar, 2);

            Controls.Add(_mainLayout);

            // === 定时器: 1s (替代 QTimer::timeoutSlot) ===
            _timer = new System.Windows.Forms.Timer { Interval = 1000 };
            _timer.Tick += OnTimerTick;
        }

        private static Label MakeLabel(string text, int x, int y, Color? c = null)
            => new Label { Text = text, AutoSize = true, ForeColor = c ?? Color.White, Location = new Point(x, y) };

        private static Button MakeButton(string text, int x, int y, Color back)
            => new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(160, 38),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei", 10F),
            };

        private int _nextControlY = 540;

        private void BuildStationStatusPanel()
        {
            string[] names = { "Adjust", "CasselZ", "Load", "Load2", "BottomGet", "Safe" };
            _stationStatusGrid = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 7,
                Location = new Point(12, _nextControlY),
                Size = new Size(170, 152),
                BackColor = Color.FromArgb(38, 38, 38),
                BorderStyle = BorderStyle.FixedSingle,
            };
            _stationStatusGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            _stationStatusGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _stationStatusGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // 标题行
            var header = new Label { Text = "工站状态", Font = new Font("Microsoft YaHei", 8F, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true };
            _stationStatusGrid.Controls.Add(header, 0, 0);
            _stationStatusGrid.SetColumnSpan(header, 3);

            _stationStatusLights = new Label[6];
            _stationStatusLabels = new Label[6];

            for (int i = 0; i < 6; i++)
            {
                _stationStatusLights[i] = new Label
                {
                    Text = "●",
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei", 8F),
                    Margin = new Padding(2, 1, 2, 1),
                };
                _stationStatusLabels[i] = new Label
                {
                    Text = names[i],
                    ForeColor = Color.White,
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei", 7F),
                };
                _stationStatusGrid.Controls.Add(_stationStatusLights[i], 0, i + 1);
                _stationStatusGrid.Controls.Add(_stationStatusLabels[i], 1, i + 1);
            }

            _controlPanel.Controls.Add(_stationStatusGrid);
            _nextControlY += 160;
        }

        private void BuildAlarmLogPanel()
        {
            var headerLabel = new Label
            {
                Text = "报警日志",
                Font = new Font("Microsoft YaHei", 8F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, _nextControlY),
                AutoSize = true,
            };

            _alarmListView = new ListView
            {
                Location = new Point(12, _nextControlY + 18),
                Size = new Size(170, 120),
                View = View.Details,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            _alarmListView.Columns.Add("时间", 46);
            _alarmListView.Columns.Add("代码", 50);
            _alarmListView.Columns.Add("消息", 68);

            _controlPanel.Controls.Add(headerLabel);
            _controlPanel.Controls.Add(_alarmListView);
            _nextControlY += 145;
        }

        private void BuildJogPanel()
        {
            _jogPanel = new Panel
            {
                Location = new Point(12, _nextControlY),
                Size = new Size(170, 140),
                BackColor = Color.FromArgb(38, 38, 38),
                BorderStyle = BorderStyle.FixedSingle,
            };

            var toggleBtn = new Button
            {
                Text = "手动轴控制",
                Location = new Point(0, 0),
                Size = new Size(168, 22),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F, FontStyle.Bold),
            };
            toggleBtn.Click += (s, e) =>
            {
                _jogPanelVisible = !_jogPanelVisible;
                _jogPanel.Height = _jogPanelVisible ? 140 : 22;
            };
            _jogPanel.Controls.Add(toggleBtn);

            _cmbAxisSelect = new ComboBox
            {
                Location = new Point(6, 26), Size = new Size(158, 20),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Microsoft YaHei", 7F),
            };
            foreach (var name in Enum.GetNames(typeof(OmniFrame.Core.BlockCut.AxisId)))
                _cmbAxisSelect.Items.Add(name);
            _cmbAxisSelect.SelectedIndex = 0;

            _txtJogTarget = new TextBox
            {
                Location = new Point(6, 50), Size = new Size(100, 20),
                Text = "0", Font = new Font("Microsoft YaHei", 7F),
            };
            _numJogSpeed = new NumericUpDown
            {
                Location = new Point(110, 50), Size = new Size(54, 20),
                Minimum = 0.1M, Maximum = 1.0M, Value = 0.5M, DecimalPlaces = 1,
                Increment = 0.1M, Font = new Font("Microsoft YaHei", 7F),
            };

            _btnJogAbs = new Button { Text = "绝对", Location = new Point(6, 74), Size = new Size(50, 22),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 100, 0), ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 7F) };
            _btnJogAbs.Click += (s, e) => JogAbsolute();

            _btnJogRel = new Button { Text = "相对", Location = new Point(60, 74), Size = new Size(50, 22),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(100, 80, 0), ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 7F) };
            _btnJogRel.Click += (s, e) => JogRelative();

            _btnJogStop = new Button { Text = "停止", Location = new Point(114, 74), Size = new Size(50, 22),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(140, 40, 0), ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 7F) };
            _btnJogStop.Click += (s, e) => JogStop();

            _lblCurPos = new Label
            {
                Text = "当前: --", Location = new Point(6, 100),
                ForeColor = Color.Cyan, AutoSize = true, Font = new Font("Microsoft YaHei", 7F),
            };

            _jogPanel.Controls.AddRange(new Control[] {
                _cmbAxisSelect, _txtJogTarget, _numJogSpeed,
                _btnJogAbs, _btnJogRel, _btnJogStop, _lblCurPos,
            });
            _jogPanel.Height = 22; // 默认折叠

            _controlPanel.Controls.Add(_jogPanel);
            _nextControlY += 28;
        }

        private void BuildCylinderPanel()
        {
            _cylinderPanel = new Panel
            {
                Location = new Point(12, _nextControlY),
                Size = new Size(170, 230),
                BackColor = Color.FromArgb(38, 38, 38),
                BorderStyle = BorderStyle.FixedSingle,
            };

            var toggleBtn = new Button
            {
                Text = "气缸手动控制",
                Location = new Point(0, 0),
                Size = new Size(168, 22),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F, FontStyle.Bold),
            };
            toggleBtn.Click += (s, e) =>
            {
                _cylinderPanelVisible = !_cylinderPanelVisible;
                _cylinderPanel.Height = _cylinderPanelVisible ? 230 : 22;
            };
            _cylinderPanel.Controls.Add(toggleBtn);

            // 关键气缸列表 (名称, DO索引)
            var cylinders = new (string name, int doIndex, int diUp, int diDown)[]
            {
                ("治具移送Y气缸", 0, -1, -1),     // DO_JigYCylinderIn
                ("治具移送Z气缸", 2, -1, -1),     // DO_JigZCylinder
                ("产品上料Z气缸", 3, -1, -1),     // DO_BlockGetZCylinder
                ("产品上料真空", 4, -1, -1),      // DO_BlockGetVacuum
                ("底板夹紧气缸", 5, 10, 11),      // DO_BottomCompressCylinder
                ("片源气缸", 6, 16, 17),          // DO_YCylinder
                ("UV气缸", 7, 19, -1),            // DO_UVZCylinder
                ("夹爪夹紧", 16, 25, 24),         // DO_BottomOutClawIn
                ("推料气缸", 17, -1, 23),         // DO_BottomOutPushCylinder
            };

            int y = 26;
            foreach (var cyl in cylinders)
            {
                var lbl = new Label
                {
                    Text = cyl.name, Location = new Point(4, y), AutoSize = true,
                    ForeColor = Color.White, Font = new Font("Microsoft YaHei", 7F),
                };
                var btnOn = new Button
                {
                    Text = "开", Location = new Point(100, y - 2), Size = new Size(30, 18),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 100, 0),
                    ForeColor = Color.White, Font = new Font("Microsoft YaHei", 7F),
                };
                var btnOff = new Button
                {
                    Text = "关", Location = new Point(134, y - 2), Size = new Size(30, 18),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(140, 40, 0),
                    ForeColor = Color.White, Font = new Font("Microsoft YaHei", 7F),
                };

                int doIdx = cyl.doIndex;
                btnOn.Click += (s, e) => _io?.SetDO(doIdx, true);
                btnOff.Click += (s, e) => _io?.SetDO(doIdx, false);

                _cylinderPanel.Controls.Add(lbl);
                _cylinderPanel.Controls.Add(btnOn);
                _cylinderPanel.Controls.Add(btnOff);
                y += 22;
            }

            _cylinderPanel.Height = 22; // 默认折叠
            _controlPanel.Controls.Add(_cylinderPanel);
            _nextControlY += 28;
        }

        private void BuildIOPanel()
        {
            _ioPanel = new Panel
            {
                Location = new Point(12, _nextControlY),
                Size = new Size(170, 400),
                BackColor = Color.FromArgb(38, 38, 38),
                BorderStyle = BorderStyle.FixedSingle,
            };

            var toggleBtn = new Button
            {
                Text = "IO信号监视 (空跑)",
                Location = new Point(0, 0),
                Size = new Size(168, 22),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F, FontStyle.Bold),
            };
            toggleBtn.Click += (s, e) =>
            {
                _ioPanelVisible = !_ioPanelVisible;
                _ioPanel.Height = _ioPanelVisible ? 400 : 22;
            };
            _ioPanel.Controls.Add(toggleBtn);

            _ioGrid = new TableLayoutPanel
            {
                Location = new Point(2, 24),
                Size = new Size(164, 370),
                ColumnCount = 3,
                RowCount = 37, // header + 36 DI
                AutoScroll = true,
                BackColor = Color.FromArgb(30, 30, 30),
            };
            _ioGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24));
            _ioGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 68));
            _ioGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 68));
            _ioGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));

            // 表头: DI / DO
            var diHeader = new Label { Text = "DI", ForeColor = Color.Cyan, Font = new Font("Microsoft YaHei", 7F, FontStyle.Bold), AutoSize = true };
            var doHeader = new Label { Text = "DO", ForeColor = Color.Yellow, Font = new Font("Microsoft YaHei", 7F, FontStyle.Bold), AutoSize = true };
            _ioGrid.Controls.Add(new Label(), 0, 0);
            _ioGrid.Controls.Add(diHeader, 1, 0);
            _ioGrid.Controls.Add(doHeader, 2, 0);

            // 信号名映射
            var diNames = new Dictionary<int, string>
            {
                [0]="料塔检测", [1]="托盘检测", [2]="治具Y出", [3]="治具Y回",
                [4]="治具Z升", [5]="治具Z降", [6]="真空检测", [7]="上料Z升",
                [8]="上料Z降", [9]="底板检测", [10]="夹紧出", [11]="夹紧回",
                [12]="载台升", [13]="载台降", [14]="片源夹紧", [15]="片源缩回",
                [16]="AdjustZ升", [17]="AdjustZ降", [18]="UV升", [19]="UV降",
                [20]="Z升", [21]="Z降", [22]="推料升", [23]="推料降",
                [24]="夹爪张", [25]="夹爪紧", [26]="挡料升", [27]="挡料降",
                [28]="底板有无", [29]="玻璃有无", [30]="急停", [31]="门吸",
                [32]="光幕", [34]="治具有无", [35]="缓存固",
            };
            var doNames = new Dictionary<int, string>
            {
                [0]="治具Y出", [1]="治具Y回", [2]="治具Z", [3]="上料Z",
                [4]="真空", [5]="夹紧", [6]="片源Y", [7]="UV缸",
                [8]="UV灯", [9]="测厚", [10]="点胶", [11]="AdjustZ升",
                [12]="AdjustZ降", [13]="下料Z升", [14]="下料Z降", [15]="夹爪张",
                [16]="夹爪紧", [17]="推料", [18]="挡料", [19]="载台顶升",
                [20]="LED", [21]="绿灯", [22]="黄灯", [23]="红灯",
                [24]="蜂鸣", [26]="缓存固", [28]="24V",
            };

            int maxDi = 36;
            _ioLabels = new Label[2, maxDi];

            for (int i = 0; i < maxDi; i++)
            {
                var idxLabel = new Label
                {
                    Text = i.ToString(), ForeColor = Color.Gray,
                    Font = new Font("Consolas", 6F), AutoSize = true,
                    TextAlign = ContentAlignment.MiddleRight,
                };
                _ioGrid.Controls.Add(idxLabel, 0, i + 1);

                // DI 列
                var diLbl = new Label
                {
                    Text = diNames.TryGetValue(i, out var dn) ? dn : $"--",
                    ForeColor = Color.Gray,
                    Font = new Font("Microsoft YaHei", 6F),
                    AutoSize = true,
                    BackColor = Color.FromArgb(35, 35, 35),
                    Margin = new Padding(1),
                };
                _ioLabels[0, i] = diLbl;
                _ioGrid.Controls.Add(diLbl, 1, i + 1);

                // DO 列
                var doLbl = new Label
                {
                    Text = doNames.TryGetValue(i, out var don) ? don : (i < 29 ? $"--" : ""),
                    ForeColor = Color.Gray,
                    Font = new Font("Microsoft YaHei", 6F),
                    AutoSize = true,
                    BackColor = Color.FromArgb(35, 35, 35),
                    Margin = new Padding(1),
                };
                _ioLabels[1, i] = doLbl;
                _ioGrid.Controls.Add(doLbl, 2, i + 1);
            }

            _ioPanel.Controls.Add(_ioGrid);
            _ioPanel.Height = 22; // 默认折叠
            _controlPanel.Controls.Add(_ioPanel);
            _nextControlY += 28;
        }

        private void UpdateIOPanel()
        {
            if (_ioLabels == null) return;
            var dryDI = BlockCutStationBase.GetDryDI();
            var dryDO = BlockCutStationBase.GetDryDO();

            for (int i = 0; i < 36; i++)
            {
                if (_ioLabels[0, i] != null && dryDI.TryGetValue(i, out bool diVal))
                {
                    _ioLabels[0, i].ForeColor = diVal ? Color.Lime : Color.Gray;
                    _ioLabels[0, i].BackColor = diVal ? Color.FromArgb(0, 60, 0) : Color.FromArgb(35, 35, 35);
                }
                if (_ioLabels[1, i] != null && dryDO.TryGetValue(i, out bool doVal))
                {
                    _ioLabels[1, i].ForeColor = doVal ? Color.Yellow : Color.Gray;
                    _ioLabels[1, i].BackColor = doVal ? Color.FromArgb(60, 50, 0) : Color.FromArgb(35, 35, 35);
                }
            }
        }

        private void JogAbsolute()
        {
            if (!double.TryParse(_txtJogTarget.Text, out double target)) return;
            int axisIdx = _cmbAxisSelect.SelectedIndex;
            double ratio = (double)_numJogSpeed.Value;
            try { _motion?.AbsMove(axisIdx, target, ratio); }
            catch (Exception ex) { LogWarn($"轴运动失败: {ex.Message}"); }
        }

        private void JogRelative()
        {
            if (!double.TryParse(_txtJogTarget.Text, out double dist)) return;
            int axisIdx = _cmbAxisSelect.SelectedIndex;
            double ratio = (double)_numJogSpeed.Value;
            try { _motion?.RelativeMove(axisIdx, dist, ratio); }
            catch (Exception ex) { LogWarn($"轴运动失败: {ex.Message}"); }
        }

        private void JogStop()
        {
            int axisIdx = _cmbAxisSelect.SelectedIndex;
            try { _motion?.StopAxis(axisIdx); }
            catch (Exception ex) { LogWarn($"轴停止失败: {ex.Message}"); }
        }

        #endregion

        #region 事件绑定

        private void WireEvents()
        {
            _alarmMgr.AlarmOccurred += OnAlarmOccurred;
            _alarmMgr.AlarmCleared += OnAlarmCleared;

            // 工站 UI 事件（告警、状态变更）
            _coordinator.SubscribeStationEvents(WireStationEvents);

            // 产量追踪
            _coordinator.OnOutputIncremented += () =>
            {
                _totalOutput++;
                _uphManager.RecordProduction("DefaultLine", 1);
            };

            // 扫码请求 → UI 处理（弹框输入）
            _coordinator.OnSweepRequested += target => { _sweepTarget = target; };

            // 扫码枪回调 — Load1 / Load2 按 _sweepTarget 分发
            _barcodeClient.OnCodeScanned += code =>
            {
                if (_sweepTarget != 2 && _sweepTarget != 3) return;

                if (string.IsNullOrEmpty(code))
                {
                    var target = _sweepTarget;
                    BeginInvoke((Action)(() =>
                    {
                        if (!_isFormReady) return;
                        string prompt = target == 2
                            ? "清洗篮扫码失败，请手动输入清洗篮二维码！"
                            : "底板扫码失败，请手动输入底板二维码！";
                        using var dlg = new SerialNumberDialog(prompt);
                        if (dlg.ShowDialog(this) == DialogResult.OK)
                            _coordinator.DispatchBarcode(target, dlg.Barcode);
                    }));
                }
                else
                {
                    _coordinator.DispatchBarcode(_sweepTarget, code);
                    AppendScanList(_sweepTarget == 2 ? _scanListSlice : _scanListBottom, code);
                    _sweepTarget = 0;
                }
                LogInfo($"扫码: {code}");
            };

            // === 工站特定委托 ===

            // 相机拍照 → 委托 BlockCutVision (仿真 / 硬件)
            _coordinator.Adjust.OnCameraCapture += async (n, col) =>
            {
                // 仿真模式: 模拟 Basler 相机取像 + Halcon 拟合
                var sw = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    // 模拟曝光/传输延迟 (50-200ms)
                    int exposureMs = _random.Next(50, 200);
                    await Task.Delay(exposureMs);

                    // 随机故障注入: 1/1000 概率模拟相机超时
                    if (_random.Next(0, 1000) == 0)
                    {
                        sw.Stop();
                        LogWarn($"[Camera] 模拟相机超时 (camera={n}, col={col})");
                        return new CameraResult
                        {
                            Success = false,
                            CameraName = $"Cam{n}",
                            ErrorMessage = "Simulated capture timeout",
                            Timestamp = DateTime.Now
                        };
                    }

                    // 生成模拟图像数据 (简单的灰度渐变测试图案)
                    int w = 800, h = 600;
                    var imageData = new byte[w * h];
                    for (int i = 0; i < imageData.Length; i++)
                        imageData[i] = (byte)((i % w) * 255 / w); // 水平渐变

                    sw.Stop();
                    LogInfo($"[Camera] 模拟拍照完成: camera={n}, col={col}, w={w}, h={h}, 耗时={sw.ElapsedMilliseconds}ms");

                    return new CameraResult
                    {
                        Success = true,
                        Timestamp = DateTime.Now,
                        ImageData = imageData,
                        Width = w,
                        Height = h,
                        CameraName = $"Cam{n}",
                        Point1 = new Point2D(100, 200),
                        Point2 = new Point2D(300, 400),
                        Angle = 0.5
                    };
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    LogError($"[Camera] 捕获异常: {ex.Message}");
                    return new CameraResult
                    {
                        Success = false,
                        CameraName = $"Cam{n}",
                        ErrorMessage = ex.Message,
                        Timestamp = DateTime.Now
                    };
                }
            };

            // MES 验证
            _coordinator.Adjust.OnMesValidate += async code =>
            {
                return await _mesClient.ValidateCardAsync(code, CancellationToken.None);
            };

            // 扫码请求 — Adjust 工站: 带超时自动回退到手动输入
            // Adjust 扫码回调 — 使用 TaskCompletionSource 独立处理
            _coordinator.Adjust.OnSweepCode += () =>
            {
                var tcs = new TaskCompletionSource<string>();
                _barcodeClient.OnCodeScanned += OnSweepCallback;
                return tcs.Task;

                void OnSweepCallback(string code)
                {
                    _barcodeClient.OnCodeScanned -= OnSweepCallback;
                    if (string.IsNullOrEmpty(code))
                    {
                        // 扫码失败 → 弹出手动输入框
                        BeginInvoke((Action)(() =>
                        {
                            if (!_isFormReady) { tcs.TrySetResult(null); return; }
                            using var dlg = new SerialNumberDialog("扫码失败，请手动输入清洗篮二维码！");
                            if (dlg.ShowDialog(this) == DialogResult.OK)
                                tcs.TrySetResult(dlg.Barcode);
                            else
                                tcs.TrySetResult(null);
                        }));
                    }
                    else
                    {
                        tcs.TrySetResult(code);
                    }
                }
            };
        }

        private void WireStationEvents(BlockCutStationBase st)
        {
            st.OnWarning += (code, msg, pause) =>
            {
                LogWarn($"[{st.StationName}] [{code}] {msg}");

                BeginInvoke((Action)(() =>
                {
                    var item = new ListViewItem(DateTime.Now.ToString("HH:mm:ss"));
                    item.SubItems.Add(code.ToString());
                    item.SubItems.Add(msg);
                    item.ForeColor = Color.Red;
                    _alarmListView?.Items.Insert(0, item);
                    while (_alarmListView?.Items.Count > 50)
                        _alarmListView.Items.RemoveAt(_alarmListView.Items.Count - 1);

                    _currentAlarmCode = code.ToString();
                    _currentAlarmMsg = msg;

                    // 弹窗确认 — 操作员可选择确认/重复/取消
                    if (pause && _isFormReady)
                    {
                        using var dlg = new InfoDialog($"[{st.StationName}] {msg}", showRepeat: true);
                        var result = dlg.ShowDialog(this);
                        if (dlg.IsRepeat)
                        {
                            LogInfo($"[{st.StationName}] 操作员选择重复动作");
                            st.ContinueAfterAlarm();
                        }
                        else if (dlg.Confirmed)
                        {
                            LogInfo($"[{st.StationName}] 操作员确认告警，继续运行");
                            st.ContinueAfterAlarm();
                        }
                        // 取消则保持暂停
                    }
                }));
            };
            st.OnGetFail += msg =>
            {
                BeginInvoke((Action)(() =>
                {
                    if (!_isFormReady) return;
                    using var dlg = new InfoDialog($"[{st.StationName}] {msg}\n是否重复取料动作？", showRepeat: true);
                    var result = dlg.ShowDialog(this);
                    if (dlg.IsRepeat)
                    {
                        LogInfo($"[{st.StationName}] 操作员选择重试取料");
                        st.ContinueAfterAlarm();
                    }
                    else if (dlg.Confirmed)
                    {
                        LogInfo($"[{st.StationName}] 操作员确认跳过取料");
                        st.ContinueAfterAlarm();
                    }
                }));
            };
            st.OnPrompt += msg =>
            {
                BeginInvoke((Action)(() =>
                {
                    if (!_isFormReady) return;
                    using var dlg = new InfoDialog(msg, showRepeat: false);
                    dlg.ShowDialog(this);
                }));
            };
        }

        #endregion

        #region 定时器 (替代 timeoutSlot)

        private void OnTimerTick(object sender, EventArgs e)
        {
            _lblTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if ((DateTime.Now - _lastUphUpdate).TotalMinutes >= 1)
            {
                _lastUphUpdate = DateTime.Now;
                _uph = _uphManager.CalculateUph("DefaultLine");
                _lblUPH.Text = $"UPH: {_uph:F1}";
            }

            _lblStatus.Text = _coordinator.IsRunning ? "● 运行中" : "● 待机";
            _lblStatus.ForeColor = _coordinator.IsRunning ? Color.Lime : Color.Gray;

            _lblAlarm.Text = string.IsNullOrEmpty(_currentAlarmCode)
                ? "" : $"⚠ {_currentAlarmCode}: {_currentAlarmMsg}";

            // 更新轴当前位置
            if (_jogPanelVisible && _cmbAxisSelect.SelectedIndex >= 0)
            {
                try
                {
                    double pos = _motion.GetAxisPos(_cmbAxisSelect.SelectedIndex);
                    _lblCurPos.Text = $"当前: {pos:F3}";
                }
                catch (Exception ex) { Logger.Error("BlockCutMainForm操作失败", ex); _lblCurPos.Text = "当前: --"; }
            }

            // 5分钟自动恢复安全监测
            if (_chkCloseSafe.Checked && (DateTime.Now - _closeSafeTimeStart).TotalMinutes >= 5)
            {
                _chkCloseSafe.Checked = false;
                LogInfo("安全监测已自动恢复 (5min超时)");
            }

            // 更新工站状态指示灯
            UpdateStationLights();

            // 更新 IO 信号监视 (空跑)
            if (_ioPanelVisible) UpdateIOPanel();
        }

        private void AppendScanList(ListBox list, string code)
        {
            if (list == null) return;
            BeginInvoke((Action)(() =>
            {
                if (list.Items.Count > 10) list.Items.Clear();
                list.Items.Insert(0, $"{DateTime.Now:HH:mm:ss}  {code}");
            }));
        }

        private void UpdateStationLights()
        {
            var stations = _coordinator.GetStations();
            for (int i = 0; i < stations.Length && i < _stationStatusLights.Length; i++)
            {
                if (_stationStatusLights[i] == null) continue;
                var st = stations[i];
                _stationStatusLights[i].ForeColor = st.IsInError ? Color.Red
                    : st.IsPaused ? Color.Yellow
                    : _coordinator.IsRunning ? Color.Lime : Color.Gray;
            }
        }

        #endregion

        #region 启停控制

        public void StartAllStations()
        {
            if (_coordinator.IsRunning) return;

            // 1. 新建工单确认
            using (var dlg = new WorkPrivateDialog(_workId))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                _workId = dlg.WorkName;
                UpdateWorkInfo(_workId, _machineId ?? "Default");
                LogInfo($"工单确认: {_workId}");
            }

            // 2. 卡塞起始序号确认
            using (var dlg = new CountDialog(_coordinator.CasselZ.CasselIndex))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                _coordinator.CasselZ.CasselIndex = dlg.CasselIndex;
                LogInfo($"卡塞起始序号: {dlg.CasselIndex}");
            }

            // 3. 行列起始序号确认 (料盘行/列 + 底板行/列)
            using (var dlg = new RowColDialog(
                _coordinator.Load.TrayRow, _coordinator.Load.TrayCol,
                _cfg.TrayRows, _cfg.TrayCols))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                _coordinator.Load.TrayRow = dlg.StartRow;
                _coordinator.Load.TrayCol = dlg.StartCol;
                _coordinator.Load2.BottomRow = dlg.StartRow;
                _coordinator.Load2.BottomCol = dlg.StartCol;
                LogInfo($"行列起始: 行={dlg.StartRow}, 列={dlg.StartCol}");
            }

            _coordinator.StartAll();
            _chkEmptyTest.Enabled = false;
            _startTime = DateTime.Now;
            LogInfo("所有工站已启动");
        }

        public void StopAllStations()
        {
            if (!_coordinator.IsRunning) return;
            _coordinator.StopAll();
        }

        public void PauseAll()
        {
            _coordinator.PauseAll();
        }

        public void ResumeAll()
        {
            _coordinator.ResumeAll();
        }

        #endregion

        #region 事件处理

        private void OnAlarmOccurred(object sender, OmniFrame.Core.AlarmInfo e)
        {
            BeginInvoke((Action)(() =>
            {
                _currentAlarmCode = e.AlarmCode;
                _currentAlarmMsg = e.AlarmMessage;

                // 添加到报警列表
                var item = new ListViewItem(DateTime.Now.ToString("HH:mm:ss"));
                item.SubItems.Add(e.AlarmCode ?? "");
                item.SubItems.Add(e.AlarmMessage ?? "");
                item.ForeColor = Color.Red;
                _alarmListView?.Items.Insert(0, item);

                // 限制最多 50 条
                while (_alarmListView?.Items.Count > 50)
                    _alarmListView.Items.RemoveAt(_alarmListView.Items.Count - 1);
            }));
        }

        private void OnAlarmCleared(object sender, OmniFrame.Core.AlarmInfo e)
        {
            BeginInvoke((Action)(() =>
            {
                _currentAlarmCode = null;
                _currentAlarmMsg = null;

                // 从列表中标记为已清除
                if (_alarmListView == null) return;
                foreach (ListViewItem item in _alarmListView.Items)
                {
                    if (item.SubItems.Count > 1 && item.SubItems[1].Text == (e.AlarmCode ?? ""))
                        item.ForeColor = Color.Gray;
                }
            }));
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1: PauseAll(); break;
                case Keys.F2: ResumeAll(); break;
                case Keys.F5: StartAllStations(); break;
                case Keys.F6: StopAllStations(); break;
            }
        }

        #endregion

        #region UI 更新

        private void UpdateAlarm(string code, string msg)
        {
            BeginInvoke((Action)(() =>
            {
                _currentAlarmCode = code;
                _currentAlarmMsg = msg;
            }));
        }

        public void UpdateWorkInfo(string workId, string machineId)
        {
            _workId = workId;
            _machineId = machineId;
            BeginInvoke((Action)(() =>
                _lblWorkInfo.Text = $"工单: {workId} | 机台: {machineId}"));
        }

        public void ChangeRole(UserType role)
        {
            _currentRole = role;
            _lblRole.Text = role switch
            {
                UserType.Operator => "操作员",
                UserType.Engineer => "工程师",
                UserType.Administrator => "管理员",
                _ => "未知",
            };
        }

        #endregion

        #region Log

        private void LogInfo(string msg) => Logger.Info($"[BlockCut] {msg}");
        private void LogWarn(string msg) => Logger.Warning($"[BlockCut] {msg}");
        private void LogError(string msg) => Logger.Error($"[BlockCut] {msg}");

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAllStations();
            _timer?.Stop();
            base.OnFormClosing(e);
        }
    }
}
