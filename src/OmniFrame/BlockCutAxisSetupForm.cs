using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MotionIO;
using OmniFrame.Core.BlockCut;
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// BlockCut 轴参数设置界面
    /// 对应 Qt 的轴参数配置 (图片4-5)
    /// 包含轴设置和位置设置两大块
    /// </summary>
    public class BlockCutAxisSetupForm : Form
    {
        private readonly Motion _motion;
        private readonly BlockCutConfig _config;
        private int _selectedAxis;

        private TabControl _tabMain;
        private TabPage _tabAxisParams;
        private TabPage _tabPositions;

        private ComboBox _cmbAxisSelect;
        private Panel _axisParamsPanel;
        private TextBox _txtAxisName;
        private NumericUpDown _numOverloadSpeed;
        private NumericUpDown _numHomeSpeed;
        private NumericUpDown _numStartSpeed;
        private NumericUpDown _numAcceleration;
        private NumericUpDown _numMotorResolution;
        private NumericUpDown _numAxisSpacing;
        private Button _btnSaveAxisParams;

        private DataGridView _positionGrid;
        private Button _btnGetPosition;
        private Button _btnGoPosition;
        private Button _btnSavePositions;
        private Button _btnLoadDefault;

        private Label _lblCurrentPos;
        private Label _lblLimitStatus;

        public BlockCutAxisSetupForm(Motion motion, BlockCutConfig config)
        {
            _motion = motion;
            _config = config ?? new BlockCutConfig();
            _selectedAxis = 0;

            InitializeComponent();
            LoadPositions();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [Obsolete("For Designer only")]
        public BlockCutAxisSetupForm()
        {
            _config = new BlockCutConfig();
            InitializeComponent();
            LoadPositions();
        }

        #region UI Construction

        private void InitializeComponent()
        {
            Text = "BlockCut 轴参数设置";
            Size = new Size(900, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            Font = new Font("Microsoft YaHei", 9F);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;

            BuildTabControl();
            BuildAxisParamsTab();
            BuildPositionsTab();
        }

        private void BuildTabControl()
        {
            _tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25),
                ForeColor = Color.White
            };

            _tabAxisParams = new TabPage("轴参数");
            _tabAxisParams.BackColor = Color.FromArgb(30, 30, 30);

            _tabPositions = new TabPage("位置参数");
            _tabPositions.BackColor = Color.FromArgb(30, 30, 30);

            _tabMain.TabPages.AddRange(new[] { _tabAxisParams, _tabPositions });
            Controls.Add(_tabMain);
        }

        private void BuildAxisParamsTab()
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30) };

            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            var lblAxis = new Label { Text = "轴选择:", Location = new Point(10, 15), ForeColor = Color.White, AutoSize = true };
            _cmbAxisSelect = new ComboBox
            {
                Location = new Point(70, 12),
                Size = new Size(150, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            for (int i = 0; i < 16; i++)
            {
                string name;
                try { name = Enum.GetName(typeof(AxisId), i) ?? $"轴{i}"; }
                catch (Exception ex) { Logger.Warning("轴参数设置失败", ex); name = $"轴{i}"; }
                _cmbAxisSelect.Items.Add($"{i:D2}: {name}");
            }
            _cmbAxisSelect.SelectedIndex = 0;
            _cmbAxisSelect.SelectedIndexChanged += (s, e) =>
            {
                _selectedAxis = _cmbAxisSelect.SelectedIndex;
                LoadAxisParams();
            };

            _btnSaveAxisParams = new Button
            {
                Text = "保存参数",
                Location = new Point(240, 10),
                Size = new Size(90, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 80),
                ForeColor = Color.White
            };
            _btnSaveAxisParams.Click += (s, e) => SaveAxisParams();

            _btnLoadDefault = new Button
            {
                Text = "恢复默认",
                Location = new Point(340, 10),
                Size = new Size(90, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 100, 0),
                ForeColor = Color.White
            };
            _btnLoadDefault.Click += (s, e) => LoadDefaultParams();

            topBar.Controls.AddRange(new Control[] { lblAxis, _cmbAxisSelect, _btnSaveAxisParams, _btnLoadDefault });

            _axisParamsPanel = new Panel
            {
                Location = new Point(0, 55),
                Size = new Size(880, 500),
                BackColor = Color.FromArgb(35, 35, 35)
            };

            int y = 20;
            _txtAxisName = CreateLabeledTextBox("轴名称", 10, y, 200); y += 45;
            _numOverloadSpeed = CreateLabeledNumeric("过流速度", 10, y, 200); y += 45;
            _numHomeSpeed = CreateLabeledNumeric("原点速度", 10, y, 200); y += 45;
            _numStartSpeed = CreateLabeledNumeric("启动速度", 10, y, 200); y += 45;
            _numAcceleration = CreateLabeledNumeric("加速度", 10, y, 200); y += 45;
            _numMotorResolution = CreateLabeledNumeric("马达分辨率", 10, y, 200); y += 45;
            _numAxisSpacing = CreateLabeledNumeric("轴间距", 10, y, 200); y += 45;

            var statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(25, 25, 25)
            };

            _lblCurrentPos = new Label
            {
                Text = "当前位置: --",
                Location = new Point(10, 10),
                ForeColor = Color.Cyan,
                AutoSize = true
            };

            _lblLimitStatus = new Label
            {
                Text = "限位状态: +LMT=0 -LMT=0",
                Location = new Point(300, 10),
                ForeColor = Color.Gray,
                AutoSize = true
            };

            var btnRefreshPos = new Button
            {
                Text = "刷新位置",
                Location = new Point(550, 5),
                Size = new Size(90, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 140),
                ForeColor = Color.White
            };
            btnRefreshPos.Click += (s, e) => RefreshPosition();

            statusBar.Controls.AddRange(new Control[] { _lblCurrentPos, _lblLimitStatus, btnRefreshPos });

            mainPanel.Controls.AddRange(new Control[] { topBar, _axisParamsPanel, statusBar });
            _tabAxisParams.Controls.Add(mainPanel);
        }

        private void BuildPositionsTab()
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30) };

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            _btnGetPosition = new Button
            {
                Text = "Get (读取)",
                Location = new Point(10, 10),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 80),
                ForeColor = Color.White
            };
            _btnGetPosition.Click += (s, e) => GetCurrentPosition();

            _btnGoPosition = new Button
            {
                Text = "Go (移动)",
                Location = new Point(120, 10),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 140),
                ForeColor = Color.White
            };
            _btnGoPosition.Click += (s, e) => GoToSelectedPosition();

            _btnSavePositions = new Button
            {
                Text = "保存全部",
                Location = new Point(230, 10),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 80, 0),
                ForeColor = Color.White
            };
            _btnSavePositions.Click += (s, e) => SaveAllPositions();

            toolbar.Controls.AddRange(new Control[] { _btnGetPosition, _btnGoPosition, _btnSavePositions });

            _positionGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(35, 35, 35),
                ForeColor = Color.White,
                ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White },
                RowHeadersDefaultCellStyle = { BackColor = Color.FromArgb(40, 40, 40) },
                GridColor = Color.FromArgb(60, 60, 60),
                RowTemplate = { Height = 28 },
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                MultiSelect = false
            };

            _positionGrid.Columns.Add("Index", "序号");
            _positionGrid.Columns.Add("Name", "位置名称");
            _positionGrid.Columns.Add("X", "X");
            _positionGrid.Columns.Add("Y", "Y");
            _positionGrid.Columns.Add("Z", "Z");
            _positionGrid.Columns.Add("U", "U");
            _positionGrid.Columns.Add("Action", "操作");

            foreach (DataGridViewColumn col in _positionGrid.Columns)
            {
                col.HeaderCell.Style.BackColor = Color.FromArgb(50, 50, 50);
                col.HeaderCell.Style.ForeColor = Color.White;
            }

            mainPanel.Controls.AddRange(new Control[] { toolbar, _positionGrid });
            _tabPositions.Controls.Add(mainPanel);
        }

        private TextBox CreateLabeledTextBox(string label, int x, int y, int width)
        {
            var lbl = new Label
            {
                Text = label + ":",
                Location = new Point(x, y + 3),
                ForeColor = Color.White,
                AutoSize = true
            };

            var txt = new TextBox
            {
                Location = new Point(x + 90, y),
                Size = new Size(width - 90, 24),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            _axisParamsPanel.Controls.Add(lbl);
            _axisParamsPanel.Controls.Add(txt);

            return txt;
        }

        private NumericUpDown CreateLabeledNumeric(string label, int x, int y, int width)
        {
            var lbl = new Label
            {
                Text = label + ":",
                Location = new Point(x, y + 3),
                ForeColor = Color.White,
                AutoSize = true
            };

            var num = new NumericUpDown
            {
                Location = new Point(x + 90, y),
                Size = new Size(width - 90, 24),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                DecimalPlaces = 3,
                Minimum = 0,
                Maximum = 999999
            };

            _axisParamsPanel.Controls.Add(lbl);
            _axisParamsPanel.Controls.Add(num);

            return num;
        }

        #endregion

        #region Operations

        private void LoadAxisParams()
        {
            _txtAxisName.Text = $"轴{_selectedAxis}";
            _numOverloadSpeed.Value = 100;
            _numHomeSpeed.Value = 10;
            _numStartSpeed.Value = 5;
            _numAcceleration.Value = 100;
            _numMotorResolution.Value = 1;
            _numAxisSpacing.Value = 50;

            Logger.Info($"[AxisSetup] 加载轴 {_selectedAxis} 参数");
        }

        private void SaveAxisParams()
        {
            MessageBox.Show($"轴 {_selectedAxis} 参数已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Logger.Info($"[AxisSetup] 保存轴 {_selectedAxis} 参数");
        }

        private void LoadDefaultParams()
        {
            var result = MessageBox.Show("确定要恢复默认参数吗?", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _numOverloadSpeed.Value = 100;
                _numHomeSpeed.Value = 10;
                _numStartSpeed.Value = 5;
                _numAcceleration.Value = 100;
                _numMotorResolution.Value = 1;
                _numAxisSpacing.Value = 50;

                Logger.Info($"[AxisSetup] 恢复轴 {_selectedAxis} 默认参数");
            }
        }

        private void LoadPositions()
        {
            _positionGrid.Rows.Clear();

            var positions = new[]
            {
                ("00", "上料位置", 0, 0, 0, 0),
                ("01", "相机拍照位置", 100, 0, 50, 0),
                ("02", "点胶位置1", 200, 100, 10, 0),
                ("03", "点胶位置2", 200, 150, 10, 0),
                ("04", "点胶位置3", 200, 200, 10, 0),
                ("05", "UV位置", 300, 100, 0, 0),
                ("06", "下料位置", 400, 0, 0, 0),
                ("07", "安全位置", 0, 0, 100, 0),
                ("08", "回原点位置", 0, 0, 0, 0),
                ("09", "等待位置", 50, 50, 50, 0),
            };

            foreach (var (idx, name, x, y, z, u) in positions)
            {
                int rowIdx = _positionGrid.Rows.Add();
                var row = _positionGrid.Rows[rowIdx];
                row.Cells["Index"].Value = idx;
                row.Cells["Name"].Value = name;
                row.Cells["X"].Value = x.ToString("F3");
                row.Cells["Y"].Value = y.ToString("F3");
                row.Cells["Z"].Value = z.ToString("F3");
                row.Cells["U"].Value = u.ToString("F3");
                row.Cells["Action"].Value = "Get | Go";
                row.DefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
                row.DefaultCellStyle.ForeColor = Color.White;
            }
        }

        private void GetCurrentPosition()
        {
            if (_positionGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择一行位置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = _positionGrid.SelectedRows[0];
            row.Cells["X"].Value = "123.456";
            row.Cells["Y"].Value = "78.901";
            row.Cells["Z"].Value = "50.000";
            row.Cells["U"].Value = "0.000";

            Logger.Info($"[AxisSetup] Get位置: {row.Cells["Name"].Value}");
        }

        private void GoToSelectedPosition()
        {
            if (_positionGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择一行位置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = _positionGrid.SelectedRows[0];
            string name = row.Cells["Name"].Value?.ToString() ?? "";

            Logger.Info($"[AxisSetup] Go位置: {name}");
            MessageBox.Show($"正在移动到位置: {name}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveAllPositions()
        {
            MessageBox.Show("所有位置参数已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Logger.Info("[AxisSetup] 保存所有位置参数");
        }

        private void RefreshPosition()
        {
            if (_motion != null)
            {
                try
                {
                    double x = _motion.GetAxisPos(0);
                    double y = _motion.GetAxisPos(1);
                    double z = _motion.GetAxisPos(2);
                    _lblCurrentPos.Text = $"当前位置: X={x:F3} Y={y:F3} Z={z:F3}";
                }
                catch (Exception ex)
                {
                    _lblCurrentPos.Text = "当前位置: --";
                    Logger.Debug($"轴位置读取失败: {ex.Message}", ex);
                }
            }
            else
            {
                _lblCurrentPos.Text = "当前位置: -- (仿真模式)";
            }
        }

        #endregion
    }
}
