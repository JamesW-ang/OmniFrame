using System;
using System.Drawing;
using System.Windows.Forms;
using MotionIO;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.Core.BlockCut;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// 轴参数设置对话框 — 替代 Qt MotionSet (.cpp/.h/.ui, ~223k .ui 行)
    /// 提供各轴位置参数查看/修改、Get/Go 操作、参数持久化
    /// </summary>
    public partial class MotionSetForm : Form
    {
        private readonly Motion _motion;
        private readonly BlockCutConfig _cfg;
        private readonly BlockCutVision _vision;
        private readonly string _iniPath;
        private int _selectedAxis;

        // Events for camera/jig test operations
        public event Action<int> CameraBaketFix;
        public event Action<int, double> CameraBaketXUnFix, CameraBaketYUnFix, CameraBaketZUnFix;
        public event Action<int, int, double> AxisTest;
        public event Action AxisTestStop;
        public event Action GetMeasureHeight;
        public event Action CameraBaketStop;

        public MotionSetForm(Motion motion, BlockCutConfig cfg = null, BlockCutVision vision = null)
        {
            _motion = motion;
            _cfg = cfg ?? new BlockCutConfig();
            _vision = vision;
            _iniPath = System.IO.Path.Combine(AppContext.BaseDirectory, "system.ini");
            _selectedAxis = 0;

            InitializeComponent();
            LoadFromConfig();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [Obsolete("For Designer only - use DI constructor")]
        public MotionSetForm()
        {
            _cfg = new BlockCutConfig();
            _iniPath = System.IO.Path.Combine(AppContext.BaseDirectory, "system.ini");
            _selectedAxis = 0;

            InitializeComponent();
            LoadFromConfig();
        }

        #region UI Construction

        private TabControl _tabMain;
        private ComboBox _cmbAxis;
        private Button _btnSave, _btnSaveAs, _btnRefreshAll;

        private void InitializeComponent()
        {
            Text = "轴参数设置 (MotionSet)";
            Size = new Size(720, 600);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(40, 40, 40);
            ForeColor = Color.White;
            Font = new Font("Microsoft YaHei", 9F);

            // -- Top toolbar --
            var topBar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.FromArgb(50, 50, 50), Padding = new Padding(8, 4, 8, 4) };

            topBar.Controls.Add(NewLabel("轴选择:", 8, 8));
            _cmbAxis = new ComboBox
            {
                Location = new Point(60, 6), Size = new Size(100, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 9F),
            };
            for (int i = 0; i < 16; i++)
            {
                try { var name = Enum.GetName(typeof(AxisId), i); _cmbAxis.Items.Add(name ?? $"轴{i}"); }
                catch (Exception ex) { Logger.Warning("MotionSet初始化失败", ex); _cmbAxis.Items.Add($"轴{i}"); }
            }
            _cmbAxis.SelectedIndex = 0;
            _cmbAxis.SelectedIndexChanged += (s, e) => _selectedAxis = _cmbAxis.SelectedIndex;

            _btnRefreshAll = NewButton("刷新全部位置", 170, 6, 110, 30, Color.FromArgb(0, 100, 140));
            _btnRefreshAll.Click += (s, e) => RefreshAllPositions();

            _btnSave = NewButton("保存参数", 290, 6, 90, 30, Color.FromArgb(0, 130, 0));
            _btnSave.Click += (s, e) => SaveToConfig();

            _btnSaveAs = NewButton("导出Excel", 390, 6, 90, 30, Color.FromArgb(100, 80, 0));
            _btnSaveAs.Click += (s, e) => Logger.Info("Excel导出功能未配置");

            topBar.Controls.AddRange(new Control[] { _cmbAxis, _btnRefreshAll, _btnSave, _btnSaveAs });

            // -- TabControl --
            _tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(35, 35, 35),
            };

            BuildTabCasselZ();
            BuildTabTray();
            BuildTabBottom();
            BuildTabCamera();
            BuildTabUVW();
            BuildTabMultiProduct();
            BuildTabVision();
            BuildTabCalibration();

            Controls.Add(_tabMain);
            Controls.Add(topBar);
        }

        #endregion

        #region Tab Builders

        // Position field trackers — keyed by config property name
        private System.Collections.Generic.Dictionary<string, TextBox> _posFields
            = new System.Collections.Generic.Dictionary<string, TextBox>();
        private System.Collections.Generic.Dictionary<string, Label> _posCurLabels
            = new System.Collections.Generic.Dictionary<string, Label>();

        private void AddPosRow(Panel panel, string label, string key, ref int y,
            double defaultValue = 0, bool showGetGo = true)
        {
            var lbl = new Label { Text = label, Location = new Point(8, y), AutoSize = true,
                ForeColor = Color.FromArgb(200, 200, 200), Font = new Font("Microsoft YaHei", 8F) };

            var txt = new TextBox { Location = new Point(180, y - 2), Size = new Size(90, 22),
                Text = defaultValue.ToString("F3"), BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White, Font = new Font("Consolas", 8F) };
            _posFields[key] = txt;

            panel.Controls.Add(lbl);
            panel.Controls.Add(txt);

            if (showGetGo)
            {
                var curLbl = new Label { Text = "当前: --", Location = new Point(280, y), AutoSize = true,
                    ForeColor = Color.Cyan, Font = new Font("Microsoft YaHei", 7F) };
                _posCurLabels[key] = curLbl;
                panel.Controls.Add(curLbl);

                var btnGet = new Button { Text = "Get", Location = new Point(380, y - 3), Size = new Size(40, 22),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 100, 140),
                    ForeColor = Color.White, Font = new Font("Microsoft YaHei", 7F) };
                btnGet.Click += (s, e) => GetCurrentPos(key, txt);
                panel.Controls.Add(btnGet);

                var btnGo = new Button { Text = "Go", Location = new Point(424, y - 3), Size = new Size(40, 22),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(140, 100, 0),
                    ForeColor = Color.White, Font = new Font("Microsoft YaHei", 7F) };
                btnGo.Click += (s, e) => GoToPos(key, txt);
                panel.Controls.Add(btnGo);
            }

            y += 26;
        }

        private void AddNumRow(Panel panel, string label, string key, ref int y, decimal min, decimal max, decimal value, int decimals = 0)
        {
            var lbl = new Label { Text = label, Location = new Point(8, y), AutoSize = true,
                ForeColor = Color.FromArgb(200, 200, 200), Font = new Font("Microsoft YaHei", 8F) };
            var num = new NumericUpDown { Location = new Point(180, y - 2), Size = new Size(90, 22),
                Minimum = min, Maximum = max, Value = value, DecimalPlaces = decimals,
                BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, Font = new Font("Consolas", 8F) };
            num.Tag = key;
            panel.Controls.Add(lbl);
            panel.Controls.Add(num);
            y += 26;
        }

        private Panel NewTabPanel() => new Panel { Dock = DockStyle.Fill, AutoScroll = true,
            BackColor = Color.FromArgb(38, 38, 38), Padding = new Padding(8) };

        private Label NewSectionLabel(Panel panel, string text, ref int y)
        {
            var lbl = new Label { Text = text, Location = new Point(4, y), AutoSize = true,
                ForeColor = Color.Cyan, Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold) };
            panel.Controls.Add(lbl);
            y += 24;
            return lbl;
        }

        private void BuildTabCasselZ()
        {
            var panel = NewTabPanel();
            int y = 6;

            NewSectionLabel(panel, "料塔 (CasselZ) 参数", ref y);
            AddPosRow(panel, "CasselZ 首位置:", "CasselZFirstPos", ref y, _cfg.CasselZFirstPos);
            AddPosRow(panel, "CasselZ 间距:", "CasselZSpace", ref y, _cfg.CasselZSpace);
            AddNumRow(panel, "CasselZ 层数:", "CasselCount", ref y, 1, 100, _cfg.CasselCount);

            var btnGoCasselSpace = NewButton("移动 CasselZ 间距", 8, y, 150, 28, Color.FromArgb(140, 100, 0));
            btnGoCasselSpace.Click += (s, e) => EmitCameraBaket(0);
            panel.Controls.Add(btnGoCasselSpace);
            y += 34;

            var btnGoCasselIndex = NewButton("移动 CasselZ 编号", 8, y, 150, 28, Color.FromArgb(140, 100, 0));
            btnGoCasselIndex.Click += (s, e) => EmitCameraBaket(1);
            panel.Controls.Add(btnGoCasselIndex);

            var tab = new TabPage("料塔") { BackColor = Color.FromArgb(38, 38, 38) };
            tab.Controls.Add(panel);
            _tabMain.TabPages.Add(tab);
        }

        private void BuildTabTray()
        {
            var panel = NewTabPanel();
            int y = 6;

            NewSectionLabel(panel, "料盘 (TrayY / TrayX) 参数", ref y);
            AddPosRow(panel, "TrayY 扫码位:", "TrayYCode", ref y, _cfg.TrayYCode);
            AddPosRow(panel, "TrayY 取盘:", "TrayYGetTray", ref y, _cfg.TrayYGetTray);
            AddPosRow(panel, "TrayY 取片:", "TrayYGetSlice", ref y, _cfg.TrayYGetSlice);
            AddPosRow(panel, "TrayX 取片:", "TrayXGetSlice", ref y, _cfg.TrayXGetSlice);
            AddPosRow(panel, "TrayX 放片:", "TrayXSetSlice", ref y, _cfg.TrayXSetSlice);

            y += 4;
            NewSectionLabel(panel, "行列参数", ref y);
            AddNumRow(panel, "料盘行数:", "TrayRows", ref y, 1, 100, _cfg.TrayRows);
            AddNumRow(panel, "料盘列数:", "TrayCols", ref y, 1, 100, _cfg.TrayCols);
            AddNumRow(panel, "行距:", "TrayRowSpace", ref y, 0.1M, 1000, (decimal)_cfg.TrayRowSpace, 1);
            AddNumRow(panel, "列距:", "TrayColSpace", ref y, 0.1M, 1000, (decimal)_cfg.TrayColSpace, 1);

            var btnGoTrayRC = NewButton("移动 Tray 行列", 8, y, 150, 28, Color.FromArgb(140, 100, 0));
            btnGoTrayRC.Click += (s, e) => EmitCameraBaket(2);
            panel.Controls.Add(btnGoTrayRC);

            var tab = new TabPage("料盘") { BackColor = Color.FromArgb(38, 38, 38) };
            tab.Controls.Add(panel);
            _tabMain.TabPages.Add(tab);
        }

        private void BuildTabBottom()
        {
            var panel = NewTabPanel();
            int y = 6;

            NewSectionLabel(panel, "底板 (BottomX / BottomY) 参数", ref y);
            AddPosRow(panel, "BottomY 放片位:", "BottomYSetSlice", ref y, _cfg.BottomYSetSlice);
            AddPosRow(panel, "BottomY 对位:", "BottomYAdjustBottom", ref y, _cfg.BottomYAdjustBottom);
            AddPosRow(panel, "BottomY 取底板:", "BottomYGetBottom", ref y, 0);
            AddPosRow(panel, "BottomX 取满底板:", "BottomXGetFullBottom", ref y, _cfg.BottomXGetFullBottom);
            AddPosRow(panel, "BottomX 放满底板:", "BottomXSetFullBottom", ref y, _cfg.BottomXSetFullBottom);
            AddPosRow(panel, "BottomX 取空底板:", "BottomXGetEmptyBottom", ref y, _cfg.BottomXGetEmptyBottom);
            AddPosRow(panel, "BottomX 放空底板1:", "BottomXSetEmptyBottom", ref y, _cfg.BottomXSetEmptyBottom);
            AddPosRow(panel, "BottomX 放空底板2:", "BottomXSetEmptyBottom2", ref y, _cfg.BottomXSetEmptyBottom2);

            y += 4;
            NewSectionLabel(panel, "底板行列参数", ref y);
            AddNumRow(panel, "底板行数:", "BottomRows", ref y, 1, 100, _cfg.BottomRows);
            AddNumRow(panel, "底板列数:", "BottomCols", ref y, 1, 100, _cfg.BottomCols);
            AddNumRow(panel, "行距:", "BottomRowSpace", ref y, 0.1M, 1000, (decimal)_cfg.BottomRowSpace, 1);
            AddNumRow(panel, "列距:", "BottomColSpace", ref y, 0.1M, 1000, (decimal)_cfg.BottomColSpace, 1);

            var btnGoBottomRC = NewButton("移动 Bottom 行列", 8, y, 150, 28, Color.FromArgb(140, 100, 0));
            btnGoBottomRC.Click += (s, e) => EmitCameraBaket(3);
            panel.Controls.Add(btnGoBottomRC);

            var tab = new TabPage("底板") { BackColor = Color.FromArgb(38, 38, 38) };
            tab.Controls.Add(panel);
            _tabMain.TabPages.Add(tab);
        }

        private void BuildTabCamera()
        {
            var panel = NewTabPanel();
            int y = 6;

            NewSectionLabel(panel, "相机位置", ref y);
            AddPosRow(panel, "相机 X 底板右:", "CameraBottomXRight", ref y, _cfg.CameraBottomXRight);
            AddPosRow(panel, "相机 X 底板左:", "CameraBottomXLeft", ref y, _cfg.CameraBottomXLeft);
            AddPosRow(panel, "相机 X 片源1:", "CameraXAdjustSlice1", ref y, _cfg.CameraXAdjustSlice1);
            AddPosRow(panel, "相机 X 片源2:", "CameraXAdjustSlice2", ref y, _cfg.CameraXAdjustSlice2);
            AddPosRow(panel, "相机 X 底板位1:", "CameraXBottPos1", ref y, _cfg.CameraXBottPos1);
            AddPosRow(panel, "相机 Z 安全位:", "CameraZSafe", ref y, _cfg.CameraZSafe);
            AddPosRow(panel, "相机 Z 底板对位:", "CameraZAdjustBottom", ref y, _cfg.CameraZAdjustBottom);
            AddPosRow(panel, "相机 Z 片源对位:", "CameraZAdjustSlice", ref y, _cfg.CameraZAdjustSlice);

            y += 4;
            NewSectionLabel(panel, "片源对位位置", ref y);
            AddPosRow(panel, "片源 Y1 对位:", "SliceY1Adjust", ref y, _cfg.SliceY1Adjust);
            AddPosRow(panel, "片源 Y2 对位:", "SliceY2Adjust", ref y, _cfg.SliceY2Adjust);
            AddPosRow(panel, "片源 X 对位:", "SliceXAdjust", ref y, _cfg.SliceXAdjust);
            AddPosRow(panel, "片源 Y 等待:", "SliceYWait", ref y, _cfg.SliceYWait);
            AddPosRow(panel, "片源 X 等待:", "SliceXWait", ref y, _cfg.SliceXWait);

            y += 4;
            NewSectionLabel(panel, "安全/点胶位置", ref y);
            AddPosRow(panel, "Z 安全:", "DisZSafe", ref y, _cfg.DisZSafe);
            AddPosRow(panel, "X 安全:", "DisXSafe", ref y, _cfg.DisXSafe);
            AddPosRow(panel, "Y 安全:", "DisYSafe", ref y, _cfg.DisYSafe);
            AddPosRow(panel, "Z 点胶:", "DisZDis", ref y, _cfg.DisZDis);

            // Camera basket buttons
            y += 6;
            var gBasket = new GroupBox { Text = "相机工装测试", Location = new Point(4, y), Size = new Size(460, 60),
                ForeColor = Color.White, Font = new Font("Microsoft YaHei", 8F, FontStyle.Bold) };
            int bx = 8, by = 20;

            var btnBasketFix = NewButton("工装固定", bx, by, 80, 24, Color.FromArgb(0, 120, 0));
            btnBasketFix.Click += (s, e) => CameraBaketFix?.Invoke(1);
            gBasket.Controls.Add(btnBasketFix);
            bx += 86;
            var btnBasketUnFixX = NewButton("X 释放", bx, by, 80, 24, Color.FromArgb(180, 60, 0));
            btnBasketUnFixX.Click += (s, e) => CameraBaketXUnFix?.Invoke(1, 0.5);
            gBasket.Controls.Add(btnBasketUnFixX);
            bx += 86;
            var btnBasketUnFixY = NewButton("Y 释放", bx, by, 80, 24, Color.FromArgb(180, 60, 0));
            btnBasketUnFixY.Click += (s, e) => CameraBaketYUnFix?.Invoke(1, 0.5);
            gBasket.Controls.Add(btnBasketUnFixY);
            bx += 86;
            var btnBasketUnFixZ = NewButton("Z 释放", bx, by, 80, 24, Color.FromArgb(180, 60, 0));
            btnBasketUnFixZ.Click += (s, e) => CameraBaketZUnFix?.Invoke(1, 0.5);
            gBasket.Controls.Add(btnBasketUnFixZ);
            bx += 86;
            var btnBasketStop = NewButton("停止", bx, by, 50, 24, Color.FromArgb(100, 100, 100));
            btnBasketStop.Click += (s, e) => CameraBaketStop?.Invoke();
            gBasket.Controls.Add(btnBasketStop);

            panel.Controls.Add(gBasket);
            y += 66;

            var tab = new TabPage("相机") { BackColor = Color.FromArgb(38, 38, 38) };
            tab.Controls.Add(panel);
            _tabMain.TabPages.Add(tab);
        }

        private void BuildTabUVW()
        {
            var panel = NewTabPanel();
            int y = 6;

            NewSectionLabel(panel, "UVW 旋转参数", ref y);
            AddPosRow(panel, "U 初始位:", "UInit", ref y, 0);
            AddPosRow(panel, "V 初始位:", "VInit", ref y, 0);
            AddPosRow(panel, "W 初始位:", "WInit", ref y, 0);
            AddPosRow(panel, "UVW 角度(通用):", "UVWAngle", ref y, _cfg.UVWAngle);

            y += 4;
            NewSectionLabel(panel, "高度测量", ref y);
            AddPosRow(panel, "相机 X 高度1:", "CameraXHeight1", ref y, _cfg.CameraXHeight1);
            AddPosRow(panel, "相机 X 高度2:", "CameraXHeight2", ref y, _cfg.CameraXHeight2);
            AddPosRow(panel, "相机 X 高度3:", "CameraXHeight3", ref y, _cfg.CameraXHeight3);
            AddPosRow(panel, "相机 Z 高度:", "CameraZHeight", ref y, _cfg.CameraZHeight);
            AddNumRow(panel, "高度上限:", "HeightHigh", ref y, -10, 10, (decimal)_cfg.HeightHigh, 3);
            AddNumRow(panel, "高度下限:", "HeightLow", ref y, -10, 10, (decimal)_cfg.HeightLow, 3);

            y += 6;
            var btnMeasureHeight = NewButton("获取测量高度", 8, y, 120, 28, Color.FromArgb(0, 100, 140));
            btnMeasureHeight.Click += (s, e) => GetMeasureHeight?.Invoke();
            panel.Controls.Add(btnMeasureHeight);

            // Axis test
            y += 34;
            var gAxisTest = new GroupBox { Text = "轴测试", Location = new Point(4, y), Size = new Size(460, 60),
                ForeColor = Color.White, Font = new Font("Microsoft YaHei", 8F, FontStyle.Bold) };
            var numAxisTestCount = new NumericUpDown { Location = new Point(8, 22), Size = new Size(60, 22),
                Minimum = 1, Maximum = 1000, Value = 10, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
            var numAxisTestSpace = new NumericUpDown { Location = new Point(74, 22), Size = new Size(60, 22),
                Minimum = 0.1M, Maximum = 100, Value = 1, DecimalPlaces = 1, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
            var btnAxisTestGo = NewButton("轴测试", 140, 20, 70, 24, Color.FromArgb(0, 120, 0));
            btnAxisTestGo.Click += (s, e) => AxisTest?.Invoke(_selectedAxis, (int)numAxisTestCount.Value, (double)numAxisTestSpace.Value);
            var btnAxisTestStop = NewButton("停止", 216, 20, 50, 24, Color.FromArgb(180, 60, 0));
            btnAxisTestStop.Click += (s, e) => AxisTestStop?.Invoke();
            gAxisTest.Controls.AddRange(new Control[] {
                NewLabel("次数:", 10, 24), numAxisTestCount,
                NewLabel("间距:", 78, 24), numAxisTestSpace,
                btnAxisTestGo, btnAxisTestStop,
            });
            panel.Controls.Add(gAxisTest);
            y += 66;

            // UV/点胶参数
            NewSectionLabel(panel, "UV/点胶参数", ref y);
            AddPosRow(panel, "相机 X UV:", "CameraXUV", ref y, _cfg.CameraXUV);
            AddPosRow(panel, "相机 Z UV:", "CameraZUV", ref y, _cfg.CameraZUV);
            AddPosRow(panel, "相机 X 等待:", "CameraXWait", ref y, _cfg.CameraXWait);
            AddNumRow(panel, "UV 时间(ms):", "UVTime", ref y, 0, 60000, _cfg.UVTime);
            AddNumRow(panel, "点胶时间(ms):", "DisTime", ref y, 0, 60000, _cfg.DisTime);

            var tab = new TabPage("UVW") { BackColor = Color.FromArgb(38, 38, 38) };
            tab.Controls.Add(panel);
            _tabMain.TabPages.Add(tab);
        }

        private void BuildTabMultiProduct()
        {
            var panel = NewTabPanel();
            int y = 6;

            NewSectionLabel(panel, "产品参数 (6 槽位)", ref y);
            for (int i = 1; i <= 6; i++)
            {
                int slotIndex = i;
                var g = new GroupBox { Text = $"产品槽位 {slotIndex}", Location = new Point(4, y), Size = new Size(460, 104),
                    ForeColor = Color.Cyan, Font = new Font("Microsoft YaHei", 8F, FontStyle.Bold) };
                int gy = 18;

                var lblBottomY = NewLabel("BottomY 放片:", 8, gy);
                var txtBottomY = new TextBox { Location = new Point(120, gy - 2), Size = new Size(80, 22),
                    Text = _cfg.GetBottomYSetSlice(slotIndex).ToString("F3"),
                    BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, Font = new Font("Consolas", 8F) };
                txtBottomY.Tag = $"BottomYSetSlice{slotIndex}";
                _posFields[$"BottomYSetSlice{slotIndex}"] = txtBottomY;
                var btnGetBottomY = new Button { Text = "Get", Location = new Point(206, gy - 3), Size = new Size(40, 22),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 100, 140), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 7F) };
                btnGetBottomY.Click += (s, e) => GetCurrentPos($"BottomYSetSlice{slotIndex}", txtBottomY);
                var btnGoBottomY = new Button { Text = "Go", Location = new Point(250, gy - 3), Size = new Size(40, 22),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(140, 100, 0), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 7F) };
                btnGoBottomY.Click += (s, e) => GoToPos($"BottomYSetSlice{slotIndex}", txtBottomY);

                gy += 24;
                var lblAdjustY = NewLabel("BottomY 对位:", 8, gy);
                var txtAdjustY = new TextBox { Location = new Point(120, gy - 2), Size = new Size(80, 22),
                    Text = _cfg.GetBottomYAdjustBottom(slotIndex).ToString("F3"),
                    BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, Font = new Font("Consolas", 8F) };
                txtAdjustY.Tag = $"BottomYAdjustBottom{slotIndex}";
                _posFields[$"BottomYAdjustBottom{slotIndex}"] = txtAdjustY;
                var btnGetAdjustY = new Button { Text = "Get", Location = new Point(206, gy - 3), Size = new Size(40, 22),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 100, 140), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 7F) };
                btnGetAdjustY.Click += (s, e) => GetCurrentPos($"BottomYAdjustBottom{slotIndex}", txtAdjustY);
                var btnGoAdjustY = new Button { Text = "Go", Location = new Point(250, gy - 3), Size = new Size(40, 22),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(140, 100, 0), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 7F) };
                btnGoAdjustY.Click += (s, e) => GoToPos($"BottomYAdjustBottom{slotIndex}", txtAdjustY);

                gy += 24;
                var lblUVW = NewLabel("UVW 角度:", 8, gy);
                var txtUVW = new TextBox { Location = new Point(120, gy - 2), Size = new Size(80, 22),
                    Text = _cfg.GetUVWAngle(slotIndex).ToString("F3"),
                    BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, Font = new Font("Consolas", 8F) };
                txtUVW.Tag = $"UVWAngle{slotIndex}";
                _posFields[$"UVWAngle{slotIndex}"] = txtUVW;

                g.Controls.AddRange(new Control[] {
                    lblBottomY, txtBottomY, btnGetBottomY, btnGoBottomY,
                    lblAdjustY, txtAdjustY, btnGetAdjustY, btnGoAdjustY,
                    lblUVW, txtUVW,
                });
                panel.Controls.Add(g);
                y += 110;
            }

            var tab = new TabPage("产品") { BackColor = Color.FromArgb(38, 38, 38) };
            tab.Controls.Add(panel);
            _tabMain.TabPages.Add(tab);
        }

        private void BuildTabVision()
        {
            var panel = NewTabPanel();
            int y = 6;

            NewSectionLabel(panel, "视觉参数", ref y);
            AddNumRow(panel, "灰度下限1:", "GrayLow1", ref y, 0, 255, _cfg.GrayLow1);
            AddNumRow(panel, "灰度上限1:", "GrayHigh1", ref y, 0, 255, _cfg.GrayHigh1);
            AddNumRow(panel, "灰度下限2:", "GrayLow2", ref y, 0, 255, _cfg.GrayLow2);
            AddNumRow(panel, "灰度上限2:", "GrayHigh2", ref y, 0, 255, _cfg.GrayHigh2);
            AddNumRow(panel, "灰度间距:", "GraySpace", ref y, 0, 500, _cfg.GraySpace);
            AddNumRow(panel, "曝光间距:", "ExposureSpace", ref y, 0, 10000, _cfg.ExposureSpace);
            AddPosRow(panel, "PosMax:", "PosMax", ref y, _cfg.PosMax);
            AddNumRow(panel, "ObjectUnit:", "ObjectUnit", ref y, 0.001M, 1M, (decimal)_cfg.ObjectUnit, 3);

            var tab = new TabPage("视觉") { BackColor = Color.FromArgb(38, 38, 38) };
            tab.Controls.Add(panel);
            _tabMain.TabPages.Add(tab);
        }

        private void BuildTabCalibration()
        {
            var panel = NewTabPanel();
            int y = 6;

            NewSectionLabel(panel, "9 点标定 — 左侧偏移 (LeftOffY)", ref y);
            for (int i = 0; i < 9; i++)
            {
                var offsets = _cfg.GetLeftOffsets();
                AddPosRow(panel, $"点 {i + 1}:", $"LeftOffY_{i + 1}", ref y, i < offsets.Length ? offsets[i] : 0);
            }

            y += 4;
            NewSectionLabel(panel, "9 点标定 — 右侧偏移 (RightOffY)", ref y);
            for (int i = 0; i < 9; i++)
            {
                var offsets = _cfg.GetRightOffsets();
                AddPosRow(panel, $"点 {i + 1}:", $"RightOffY_{i + 1}", ref y, i < offsets.Length ? offsets[i] : 0);
            }

            var tab = new TabPage("标定") { BackColor = Color.FromArgb(38, 38, 38) };
            tab.Controls.Add(panel);
            _tabMain.TabPages.Add(tab);
        }

        #endregion

        #region Get/Go Operations

        private void GetCurrentPos(string key, TextBox txt)
        {
            try
            {
                double pos = _motion?.GetAxisPos(_selectedAxis) ?? 0;
                txt.Text = pos.ToString("F3");
                if (_posCurLabels.TryGetValue(key, out var lbl))
                    lbl.Text = $"当前: {pos:F3}";
            }
            catch (Exception ex)
            {
                if (_posCurLabels.TryGetValue(key, out var lbl))
                    lbl.Text = "当前: --";
                Logger.Debug($"运动轴位置刷新失败, Key={key}: {ex.Message}", ex);
            }
        }

        private void GoToPos(string key, TextBox txt)
        {
            if (!double.TryParse(txt.Text, out double target)) return;
            try
            {
                _motion?.AbsMove(_selectedAxis, target, 0.5);
            }
            catch (Exception ex)
            {
                Logger.Error("轴移动失败", ex);
                MessageBox.Show($"移动失败:\n{ex.Message}\n\n堆栈跟踪:\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshAllPositions()
        {
            foreach (var kvp in _posFields)
            {
                try
                {
                    double pos = _motion?.GetAxisPos(_selectedAxis) ?? 0;
                    kvp.Value.Text = pos.ToString("F3");
                    if (_posCurLabels.TryGetValue(kvp.Key, out var lbl))
                        lbl.Text = $"当前: {pos:F3}";
                }
                catch (Exception ex)
                {
                    Logger.Error("刷新轴位置失败", ex);
                }
            }
        }

        private void EmitCameraBaket(int type)
        {
            switch (type)
            {
                case 0: CameraBaketFix?.Invoke(1); break;
                case 1: CameraBaketFix?.Invoke(2); break;
                case 2: CameraBaketFix?.Invoke(3); break;
                case 3: CameraBaketFix?.Invoke(4); break;
            }
        }

        #endregion

        #region Load/Save

        private void LoadFromConfig()
        {
            // Populate from BlockCutConfig
            foreach (var kvp in _posFields)
            {
                var prop = typeof(BlockCutConfig).GetProperty(kvp.Key);
                if (prop != null && prop.PropertyType == typeof(double))
                {
                    kvp.Value.Text = ((double)prop.GetValue(_cfg)).ToString("F3");
                }
            }

            // Try loading from INI (overrides XML defaults)
            try
            {
                var iniPrefix = "MotionSet";
                foreach (var kvp in _posFields)
                {
                    var val = IniHelper.ReadIni(iniPrefix, kvp.Key, "", _iniPath);
                    if (!string.IsNullOrEmpty(val))
                        kvp.Value.Text = val;
                }
            }
            catch (Exception ex) { Logger.Error("MotionSet操作失败", ex); }
        }

        private void SaveToConfig()
        {
            // Write to INI
            try
            {
                var iniPrefix = "MotionSet";
                foreach (var kvp in _posFields)
                {
                    IniHelper.WriteIni(iniPrefix, kvp.Key, kvp.Value.Text, _iniPath);
                }

                // Reflect back to BlockCutConfig
                foreach (var kvp in _posFields)
                {
                    var prop = typeof(BlockCutConfig).GetProperty(kvp.Key);
                    if (prop != null && prop.CanWrite && double.TryParse(kvp.Value.Text, out double val))
                    {
                        prop.SetValue(_cfg, val);
                    }
                }

                Logger.Info("[MotionSet] 参数已保存");
                MessageBox.Show("参数已保存到 system.ini", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("[MotionSet] 保存失败", ex);
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Helpers

        private static Label NewLabel(string text, int x, int y)
            => new Label { Text = text, Location = new Point(x, y), AutoSize = true,
                ForeColor = Color.White, Font = new Font("Microsoft YaHei", 8F) };

        private static Button NewButton(string text, int x, int y, int w, int h, Color back)
            => new Button { Text = text, Location = new Point(x, y), Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F) };

        #endregion
    }
}
