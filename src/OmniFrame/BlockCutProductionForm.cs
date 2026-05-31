using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OmniFrame.Core.BlockCut;
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// BlockCut 主生产界面
    /// 对应图片1 - 完整的主界面布局，包含模组可视化、气缸状态、行列控制等
    /// </summary>
    public class BlockCutProductionForm : Form
    {
        private readonly BlockCutConfig _config;
        private readonly BlockCutMesClient _mesClient;
        private readonly BlockCutVision _vision;

        private Timer _updateTimer;

        #region UI Components

        private Panel _topInfoPanel;
        private TextBox _txtStationNo;
        private TextBox _txtMachineNo;
        private TextBox _txtBlueSliceCode;
        private TextBox _txtOperator;
        private TextBox _txtMaterialNo;
        private Label _lblStartTime;
        private Label _lblRunTime;

        private TabControl _tabMain;
        private TabPage _tabCamera;
        private TabPage _tabStats;
        private TabPage _tabDebug;

        private Panel _cameraPreviewPanel;
        private Label _lblCameraStatus;

        private Panel _testPanel;
        private ComboBox _cmbChannel;
        private Button _btnChannelTest;
        private Button _btnVisionScan;
        private Label _lblWarnTime;
        private NumericUpDown _numWarnTime;
        private Label _lblElapsedTime;

        private Panel _rowColPanel;
        private Label _lblRow;
        private NumericUpDown _numRow;
        private Label _lblCol;
        private NumericUpDown _numCol;
        private Button _btnTakeSlice;
        private Button _btnSendBoard;
        private Button _btnSetOrigin;
        private Button _btnStaticGRR;
        private Button _btnContinueBoard;
        private Label _lblSerialNo;
        private TextBox _txtSerialNo;
        private Button _btnTrayTest;

        private Panel _moduleVisualPanel;
        private Dictionary<string, Label> _moduleLabels;

        private Panel _cylinderPanel;
        private Dictionary<string, Label> _cylinderLabels;

        private Panel _bottomPanel;
        private Label _lblWaterLife;
        private ProgressBar _waterLifeBar;
        private Label _lblDispenseCount;
        private Button _btnAddWater;
        private Button _btnAddLine;
        private Button _btnUnload;
        private Button _btnLoad;

        private Panel _controlPanel;
        private Button _btnMotion;
        private Button _btnStart;
        private Button _btnStop;
        private Button _btnPause;
        private Button _btnInit;
        private Button _btnUser;
        private Button _btnExit;

        private ListView _logListView;

        #endregion

        public BlockCutProductionForm(BlockCutConfig config, BlockCutMesClient mesClient, BlockCutVision vision)
        {
            _config = config;
            _mesClient = mesClient;
            _vision = vision;
            _moduleLabels = new Dictionary<string, Label>();
            _cylinderLabels = new Dictionary<string, Label>();

            InitializeComponent();
            StartMonitoring();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [Obsolete("For Designer only")]
        public BlockCutProductionForm()
        {
            _config = new BlockCutConfig();
            _moduleLabels = new Dictionary<string, Label>();
            _cylinderLabels = new Dictionary<string, Label>();

            InitializeComponent();
            StartMonitoring();
        }

        #region UI Construction

        private void InitializeComponent()
        {
            Text = "BlockCut 生产监控";
            Size = new Size(1400, 900);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(25, 25, 25);
            ForeColor = Color.White;
            Font = new Font("Microsoft YaHei", 9F);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            WindowState = FormWindowState.Maximized;

            BuildTopInfoPanel();
            BuildMainContent();
            BuildBottomPanel();
            BuildControlPanel();
            BuildLogPanel();
        }

        private void BuildTopInfoPanel()
        {
            _topInfoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(35, 35, 35)
            };

            int x = 10;
            AddLabeledTextBox(_topInfoPanel, "工站号:", ref x, 10, 100, out _txtStationNo);
            AddLabeledTextBox(_topInfoPanel, "机台号:", ref x, 10, 100, out _txtMachineNo);
            AddLabeledTextBox(_topInfoPanel, "蓝片编号:", ref x, 10, 120, out _txtBlueSliceCode);
            AddLabeledTextBox(_topInfoPanel, "作业人:", ref x, 10, 80, out _txtOperator);
            AddLabeledTextBox(_topInfoPanel, "料号:", ref x, 10, 120, out _txtMaterialNo);

            _lblStartTime = new Label
            {
                Text = "开始: --:--:--",
                Location = new Point(x + 20, 15),
                ForeColor = Color.Cyan,
                AutoSize = true
            };

            _lblRunTime = new Label
            {
                Text = "运行时长: 00:00:00",
                Location = new Point(x + 150, 15),
                ForeColor = Color.Lime,
                AutoSize = true
            };

            _topInfoPanel.Controls.AddRange(new Control[] { _lblStartTime, _lblRunTime });
            Controls.Add(_topInfoPanel);
        }

        private void AddLabeledTextBox(Panel parent, string label, ref int x, int y, int width, out TextBox textBox)
        {
            var lbl = new Label
            {
                Text = label,
                Location = new Point(x, y + 3),
                ForeColor = Color.White,
                AutoSize = true
            };

            textBox = new TextBox
            {
                Location = new Point(x + 55, y),
                Size = new Size(width - 55, 24),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            parent.Controls.Add(lbl);
            parent.Controls.Add(textBox);

            x += width + 15;
        }

        private void BuildMainContent()
        {
            var splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            BuildLeftPanel(splitMain.Panel1);
            BuildRightPanel(splitMain.Panel2);

            splitMain.Panel1.Width = 900;
            Controls.Add(splitMain);
        }

        private void BuildLeftPanel(Panel parent)
        {
            parent.Controls.Clear();

            var splitLeft = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };

            BuildTabAndTest(splitLeft.Panel1);
            BuildModuleVisual(splitLeft.Panel2);

            splitLeft.Panel1.Width = 450;
            parent.Controls.Add(splitLeft);
        }

        private void BuildTabAndTest(Panel parent)
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            BuildCameraTab(split.Panel1);
            BuildTestAndRowCol(split.Panel2);

            split.Panel1.Width = 450;
            parent.Controls.Add(split);
        }

        private void BuildCameraTab(Panel parent)
        {
            _tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25),
                ForeColor = Color.White
            };

            _tabCamera = new TabPage("相机");
            _tabCamera.BackColor = Color.FromArgb(30, 30, 30);

            _cameraPreviewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 20)
            };

            _lblCameraStatus = new Label
            {
                Text = "相机状态: 未连接",
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                BackColor = Color.FromArgb(25, 25, 25)
            };

            _cameraPreviewPanel.Controls.Add(_lblCameraStatus);
            _tabCamera.Controls.Add(_cameraPreviewPanel);

            _tabStats = new TabPage("统计");
            _tabStats.BackColor = Color.FromArgb(30, 30, 30);
            var lblStats = new Label
            {
                Text = "统计信息\n(集成BlockCutStatsForm)",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            _tabStats.Controls.Add(lblStats);

            _tabDebug = new TabPage("调试");
            _tabDebug.BackColor = Color.FromArgb(30, 30, 30);
            var lblDebug = new Label
            {
                Text = "调试信息\n(集成BlockCutDebugForm)",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            _tabDebug.Controls.Add(lblDebug);

            _tabMain.TabPages.AddRange(new[] { _tabCamera, _tabStats, _tabDebug });
            parent.Controls.Add(_tabMain);
        }

        private void BuildTestAndRowCol(Panel parent)
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };

            BuildTestPanel(split.Panel1);
            BuildRowColPanel(split.Panel2);

            parent.Controls.Add(split);
        }

        private void BuildTestPanel(Panel parent)
        {
            _testPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(35, 35, 35)
            };

            int x = 10;

            var lblChannel = new Label { Text = "通道:", Location = new Point(x, 20), ForeColor = Color.White, AutoSize = true };
            x += 40;

            _cmbChannel = new ComboBox
            {
                Location = new Point(x, 17),
                Size = new Size(80, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            _cmbChannel.Items.AddRange(new[] { "通道1", "通道2", "通道3", "通道4" });
            _cmbChannel.SelectedIndex = 0;
            x += 90;

            _btnChannelTest = new Button
            {
                Text = "通道测试",
                Location = new Point(x, 15),
                Size = new Size(80, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 140),
                ForeColor = Color.White
            };
            x += 90;

            _btnVisionScan = new Button
            {
                Text = "视觉扫描",
                Location = new Point(x, 15),
                Size = new Size(80, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 80),
                ForeColor = Color.White
            };
            x += 90;

            var lblWarn = new Label { Text = "预警:", Location = new Point(x, 20), ForeColor = Color.White, AutoSize = true };
            x += 35;

            _numWarnTime = new NumericUpDown
            {
                Location = new Point(x, 17),
                Size = new Size(50, 24),
                Minimum = 1,
                Maximum = 60,
                Value = 30,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            x += 60;

            var lblMin = new Label { Text = "分钟", Location = new Point(x, 20), ForeColor = Color.Gray, AutoSize = true };
            x += 40;

            _lblElapsedTime = new Label
            {
                Text = "耗时: 00:00",
                Location = new Point(x, 20),
                ForeColor = Color.Yellow,
                AutoSize = true
            };

            _testPanel.Controls.AddRange(new Control[] {
                lblChannel, _cmbChannel, _btnChannelTest, _btnVisionScan,
                lblWarn, _numWarnTime, lblMin, _lblElapsedTime
            });

            parent.Controls.Add(_testPanel);
        }

        private void BuildRowColPanel(Panel parent)
        {
            _rowColPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            int y = 10;

            var lblRowCol = new Label { Text = "行列控制", Location = new Point(10, y), ForeColor = Color.Cyan, Font = new Font("Microsoft YaHei", 10F) };
            y += 30;

            _lblRow = new Label { Text = "行:", Location = new Point(10, y), ForeColor = Color.White, AutoSize = true };
            _numRow = new NumericUpDown
            {
                Location = new Point(35, y - 3),
                Size = new Size(60, 24),
                Minimum = 1,
                Maximum = 10,
                Value = 4,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            _lblCol = new Label { Text = "列:", Location = new Point(110, y), ForeColor = Color.White, AutoSize = true };
            _numCol = new NumericUpDown
            {
                Location = new Point(135, y - 3),
                Size = new Size(60, 24),
                Minimum = 1,
                Maximum = 10,
                Value = 6,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            y += 35;

            _btnTakeSlice = new Button
            {
                Text = "取片",
                Location = new Point(10, y),
                Size = new Size(70, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 80),
                ForeColor = Color.White
            };

            _btnSendBoard = new Button
            {
                Text = "送板",
                Location = new Point(90, y),
                Size = new Size(70, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 140),
                ForeColor = Color.White
            };

            _btnSetOrigin = new Button
            {
                Text = "原点设定",
                Location = new Point(170, y),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 80, 0),
                ForeColor = Color.White
            };

            y += 40;

            _btnStaticGRR = new Button
            {
                Text = "静态GRR",
                Location = new Point(10, y),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 0, 80),
                ForeColor = Color.White
            };

            _btnContinueBoard = new Button
            {
                Text = "连板送出",
                Location = new Point(100, y),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 80, 120),
                ForeColor = Color.White
            };

            y += 40;

            _lblSerialNo = new Label { Text = "序号:", Location = new Point(10, y), ForeColor = Color.White, AutoSize = true };
            _txtSerialNo = new TextBox
            {
                Location = new Point(50, y - 3),
                Size = new Size(100, 24),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            _btnTrayTest = new Button
            {
                Text = "托盘取放测试",
                Location = new Point(160, y - 3),
                Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 60, 0),
                ForeColor = Color.White
            };

            _rowColPanel.Controls.AddRange(new Control[] {
                lblRowCol, _lblRow, _numRow, _lblCol, _numCol,
                _btnTakeSlice, _btnSendBoard, _btnSetOrigin,
                _btnStaticGRR, _btnContinueBoard,
                _lblSerialNo, _txtSerialNo, _btnTrayTest
            });
        }

        private void BuildModuleVisual(Panel parent)
        {
            _moduleVisualPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            var lblTitle = new Label
            {
                Text = "模组状态",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Cyan,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            var moduleContainer = new Panel { Dock = DockStyle.Fill };

            string[] modules = { "卡塞组", "上料组Y", "底板取送组", "上料组X", "底板组", "片源调节组", "相机组" };
            int x = 10;
            int y = 10;

            foreach (var module in modules)
            {
                var lbl = new Label
                {
                    Text = $"{module}: 待机",
                    Location = new Point(x, y),
                    Size = new Size(110, 25),
                    ForeColor = Color.Gray,
                    BackColor = Color.FromArgb(40, 40, 40),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                _moduleLabels[module] = lbl;
                moduleContainer.Controls.Add(lbl);

                x += 120;
                if (x > 400)
                {
                    x = 10;
                    y += 35;
                }
            }

            moduleContainer.Controls.Add(lblTitle);
            parent.Controls.Add(moduleContainer);
        }

        private void BuildRightPanel(Panel parent)
        {
            parent.Controls.Clear();

            var splitRight = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };

            BuildCylinderPanel(splitRight.Panel1);
            BuildLogPanel();

            parent.Controls.Add(splitRight);
        }

        private void BuildCylinderPanel(Panel parent)
        {
            _cylinderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(35, 35, 35)
            };

            var lblTitle = new Label
            {
                Text = "气缸状态",
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Cyan,
                BackColor = Color.FromArgb(45, 45, 45)
            };

            var cylContainer = new Panel { Dock = DockStyle.Fill };

            string[] cylinders = { "真空", "Z气缸", "位置安全", "片源拍照", "链存气缸", "快备气缸" };
            int x = 10;
            int y = 10;

            foreach (var cyl in cylinders)
            {
                var lbl = new Label
                {
                    Text = $"{cyl}: ●",
                    Location = new Point(x, y),
                    Size = new Size(90, 20),
                    ForeColor = Color.Gray,
                    BackColor = Color.FromArgb(40, 40, 40)
                };

                _cylinderLabels[cyl] = lbl;
                cylContainer.Controls.Add(lbl);

                x += 100;
                if (x > 350)
                {
                    x = 10;
                    y += 25;
                }
            }

            _cylinderPanel.Controls.Add(lblTitle);
            _cylinderPanel.Controls.Add(cylContainer);

            parent.Controls.Add(_cylinderPanel);
        }

        private void BuildBottomPanel()
        {
            _bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(35, 35, 35)
            };

            int x = 10;

            _lblWaterLife = new Label { Text = "散水寿命:", Location = new Point(x, 15), ForeColor = Color.White, AutoSize = true };
            x += 70;

            _waterLifeBar = new ProgressBar
            {
                Location = new Point(x, 12),
                Size = new Size(100, 25),
                Maximum = 100,
                Value = 75,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.Lime
            };
            x += 110;

            _lblDispenseCount = new Label { Text = "点胶次数: 0", Location = new Point(x, 15), ForeColor = Color.Yellow, AutoSize = true };
            x += 120;

            _btnAddWater = new Button
            {
                Text = "加水",
                Location = new Point(x, 10),
                Size = new Size(60, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 140),
                ForeColor = Color.White
            };
            x += 70;

            _btnAddLine = new Button
            {
                Text = "加线",
                Location = new Point(x, 10),
                Size = new Size(60, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 80),
                ForeColor = Color.White
            };
            x += 70;

            _btnUnload = new Button
            {
                Text = "下料",
                Location = new Point(x, 10),
                Size = new Size(60, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(150, 0, 0),
                ForeColor = Color.White
            };
            x += 70;

            _btnLoad = new Button
            {
                Text = "上料",
                Location = new Point(x, 10),
                Size = new Size(60, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 80),
                ForeColor = Color.White
            };

            _bottomPanel.Controls.AddRange(new Control[] {
                _lblWaterLife, _waterLifeBar, _lblDispenseCount,
                _btnAddWater, _btnAddLine, _btnUnload, _btnLoad
            });

            Controls.Add(_bottomPanel);
        }

        private void BuildControlPanel()
        {
            _controlPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 100,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            int y = 10;
            int btnWidth = 85;
            int btnHeight = 40;

            _btnMotion = new Button
            {
                Text = "运动",
                Location = new Point(8, y),
                Size = new Size(btnWidth, btnHeight),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 100, 0),
                ForeColor = Color.White
            };
            y += btnHeight + 10;

            _btnStart = new Button
            {
                Text = "开始",
                Location = new Point(8, y),
                Size = new Size(btnWidth, btnHeight),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 140, 0),
                ForeColor = Color.White
            };
            y += btnHeight + 10;

            _btnStop = new Button
            {
                Text = "停止",
                Location = new Point(8, y),
                Size = new Size(btnWidth, btnHeight),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(180, 0, 0),
                ForeColor = Color.White
            };
            y += btnHeight + 10;

            _btnPause = new Button
            {
                Text = "暂停",
                Location = new Point(8, y),
                Size = new Size(btnWidth, btnHeight),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 140),
                ForeColor = Color.White
            };
            y += btnHeight + 10;

            _btnInit = new Button
            {
                Text = "初始化",
                Location = new Point(8, y),
                Size = new Size(btnWidth, btnHeight),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 60, 0),
                ForeColor = Color.White
            };
            y += btnHeight + 20;

            _btnUser = new Button
            {
                Text = "用户",
                Location = new Point(8, y),
                Size = new Size(btnWidth, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            y += 35;

            _btnExit = new Button
            {
                Text = "退出",
                Location = new Point(8, y),
                Size = new Size(btnWidth, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 0, 0),
                ForeColor = Color.White
            };

            _controlPanel.Controls.AddRange(new Control[] {
                _btnMotion, _btnStart, _btnStop, _btnPause, _btnInit,
                _btnUser, _btnExit
            });

            Controls.Add(_controlPanel);
        }

        private void BuildLogPanel()
        {
            var logPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25)
            };

            var lblLog = new Label
            {
                Text = "运行日志",
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Cyan,
                BackColor = Color.FromArgb(35, 35, 35)
            };

            _logListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                GridLines = true
            };
            _logListView.Columns.Add("时间", 80);
            _logListView.Columns.Add("类型", 60);
            _logListView.Columns.Add("消息", 300);

            logPanel.Controls.AddRange(new Control[] { lblLog, _logListView });
        }

        #endregion

        #region Monitoring

        private void StartMonitoring()
        {
            _updateTimer = new Timer { Interval = 500 };
            _updateTimer.Tick += OnTimerTick;
            _updateTimer.Start();

            WireEvents();
            AddLog("Info", "生产监控已启动");
        }

        private void WireEvents()
        {
            _btnChannelTest.Click += (s, e) =>
            {
                AddLog("Test", $"通道测试: {_cmbChannel.SelectedItem}");
                MessageBox.Show("通道测试完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            _btnVisionScan.Click += (s, e) =>
            {
                AddLog("Vision", "开始视觉扫描");
                MessageBox.Show("视觉扫描完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            _btnTakeSlice.Click += (s, e) =>
            {
                AddLog("Action", $"取片: 行={_numRow.Value} 列={_numCol.Value}");
            };

            _btnSendBoard.Click += (s, e) =>
            {
                AddLog("Action", "送板");
            };

            _btnSetOrigin.Click += (s, e) =>
            {
                AddLog("Action", "原点设定");
            };

            _btnStaticGRR.Click += (s, e) =>
            {
                AddLog("Test", "静态GRR测试");
            };

            _btnContinueBoard.Click += (s, e) =>
            {
                AddLog("Action", "连板送出");
            };

            _btnTrayTest.Click += (s, e) =>
            {
                AddLog("Test", "托盘取放测试");
            };

            _btnAddWater.Click += (s, e) =>
            {
                _waterLifeBar.Value = 100;
                AddLog("Action", "加水完成");
            };

            _btnAddLine.Click += (s, e) =>
            {
                AddLog("Action", "加线完成");
            };

            _btnStart.Click += (s, e) =>
            {
                AddLog("Control", "开始生产");
                UpdateModuleStatus("卡塞组", "运行");
                UpdateModuleStatus("上料组Y", "运行");
            };

            _btnStop.Click += (s, e) =>
            {
                AddLog("Control", "停止生产");
                ResetModuleStatus();
            };

            _btnPause.Click += (s, e) =>
            {
                AddLog("Control", "暂停生产");
            };

            _btnInit.Click += (s, e) =>
            {
                AddLog("Control", "系统初始化");
                MessageBox.Show("初始化完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            UpdateSimulatedData();
        }

        private void UpdateSimulatedData()
        {
            var random = new Random();
            int elapsed = (int)(DateTime.Now.TimeOfDay.TotalSeconds % 3600);
            _lblElapsedTime.Text = $"耗时: {elapsed / 60:D2}:{elapsed % 60:D2}";

            foreach (var module in _moduleLabels)
            {
                if (module.Value.Text.Contains("运行"))
                {
                    module.Value.ForeColor = Color.Lime;
                }
            }

            foreach (var cyl in _cylinderLabels)
            {
                bool isActive = random.Next(2) == 1;
                cyl.Value.ForeColor = isActive ? Color.Lime : Color.Gray;
                cyl.Value.Text = $"{cyl.Key}: {(isActive ? "●" : "○")}";
            }
        }

        private void UpdateModuleStatus(string module, string status)
        {
            if (_moduleLabels.ContainsKey(module))
            {
                _moduleLabels[module].Text = $"{module}: {status}";
                _moduleLabels[module].ForeColor = status == "运行" ? Color.Lime : Color.Gray;
            }
        }

        private void ResetModuleStatus()
        {
            foreach (var module in _moduleLabels)
            {
                module.Value.Text = $"{module.Key}: 待机";
                module.Value.ForeColor = Color.Gray;
            }
        }

        private void AddLog(string type, string message)
        {
            if (_logListView.InvokeRequired)
            {
                _logListView.Invoke(new Action(() => AddLog(type, message)));
                return;
            }

            var item = new ListViewItem(DateTime.Now.ToString("HH:mm:ss"));
            item.SubItems.Add(type);
            item.SubItems.Add(message);

            switch (type)
            {
                case "Error":
                    item.ForeColor = Color.Red;
                    break;
                case "Warn":
                    item.ForeColor = Color.Yellow;
                    break;
                case "Control":
                    item.ForeColor = Color.Cyan;
                    break;
                default:
                    item.ForeColor = Color.White;
                    break;
            }

            _logListView.Items.Insert(0, item);

            if (_logListView.Items.Count > 100)
            {
                _logListView.Items.RemoveAt(_logListView.Items.Count - 1);
            }

            Logger.Info($"[Production] [{type}] {message}");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            base.OnFormClosing(e);
        }

        #endregion

        public void SetStationInfo(string stationNo, string machineNo, string operatorName)
        {
            _txtStationNo.Text = stationNo;
            _txtMachineNo.Text = machineNo;
            _txtOperator.Text = operatorName;
        }

        public void SetProductionData(string blueSliceCode, string materialNo)
        {
            _txtBlueSliceCode.Text = blueSliceCode;
            _txtMaterialNo.Text = materialNo;
        }
    }
}
