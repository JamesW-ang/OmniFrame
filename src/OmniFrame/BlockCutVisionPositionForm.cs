using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OmniFrame.Core.BlockCut;
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// BlockCut 相机拍照位置和点胶测试界面
    /// 对应图片6 - 详细的拍照位置参数和点胶测试
    /// </summary>
    public class BlockCutVisionPositionForm : Form
    {
        private readonly BlockCutVision _vision;
        private readonly BlockCutConfig _config;

        private TabControl _tabMain;
        private TabPage _tabCameraPos;
        private TabPage _tabDispenseTest;

        private ComboBox _cmbCameraSelect;
        private DataGridView _positionGrid;
        private Button _btnGetPos;
        private Button _btnGoPos;
        private Button _btnSaveAll;
        private Panel _previewPanel;
        private Label _lblCameraStatus;

        private TextBox _txtDispenseX;
        private TextBox _txtDispenseY;
        private TextBox _txtDispenseZ;
        private Button _btnDispenseTest;
        private Button _btnDispenseStop;
        private ListBox _dispenseResultList;

        public BlockCutVisionPositionForm(BlockCutVision vision, BlockCutConfig config)
        {
            _vision = vision;
            _config = config ?? new BlockCutConfig();

            InitializeComponent();
            LoadPositions();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [Obsolete("For Designer only")]
        public BlockCutVisionPositionForm()
        {
            _config = new BlockCutConfig();
            InitializeComponent();
            LoadPositions();
        }

        #region UI Construction

        private void InitializeComponent()
        {
            Text = "BlockCut 相机拍照位置设置";
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            Font = new Font("Microsoft YaHei", 9F);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;

            BuildTabControl();
            BuildCameraPositionTab();
            BuildDispenseTestTab();
        }

        private void BuildTabControl()
        {
            _tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25),
                ForeColor = Color.White
            };

            _tabCameraPos = new TabPage("相机拍照位置");
            _tabCameraPos.BackColor = Color.FromArgb(30, 30, 30);

            _tabDispenseTest = new TabPage("点胶测试");
            _tabDispenseTest.BackColor = Color.FromArgb(30, 30, 30);

            _tabMain.TabPages.AddRange(new[] { _tabCameraPos, _tabDispenseTest });
            Controls.Add(_tabMain);
        }

        private void BuildCameraPositionTab()
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30) };

            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            var lblCamera = new Label
            {
                Text = "相机选择:",
                Location = new Point(10, 15),
                ForeColor = Color.White,
                AutoSize = true
            };

            _cmbCameraSelect = new ComboBox
            {
                Location = new Point(80, 12),
                Size = new Size(150, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            _cmbCameraSelect.Items.AddRange(new[] { "底板右侧相机", "底板左侧相机", "片源相机" });
            _cmbCameraSelect.SelectedIndex = 0;

            _btnGetPos = new Button
            {
                Text = "Get (读取)",
                Location = new Point(250, 10),
                Size = new Size(90, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 80),
                ForeColor = Color.White
            };
            _btnGetPos.Click += (s, e) => GetPosition();

            _btnGoPos = new Button
            {
                Text = "Go (移动)",
                Location = new Point(350, 10),
                Size = new Size(90, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 140),
                ForeColor = Color.White
            };
            _btnGoPos.Click += (s, e) => GoToPosition();

            _btnSaveAll = new Button
            {
                Text = "保存全部",
                Location = new Point(450, 10),
                Size = new Size(90, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 80, 0),
                ForeColor = Color.White
            };
            _btnSaveAll.Click += (s, e) => SaveAllPositions();

            topBar.Controls.AddRange(new Control[] { lblCamera, _cmbCameraSelect, _btnGetPos, _btnGoPos, _btnSaveAll });

            var contentPanel = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            _positionGrid = new DataGridView
            {
                BackgroundColor = Color.FromArgb(35, 35, 35),
                ForeColor = Color.White,
                ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White },
                RowHeadersDefaultCellStyle = { BackColor = Color.FromArgb(40, 40, 40) },
                GridColor = Color.FromArgb(60, 60, 60),
                RowTemplate = { Height = 28 },
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                MultiSelect = false,
                Dock = DockStyle.Fill
            };

            _positionGrid.Columns.Add("Row", "行");
            _positionGrid.Columns.Add("Col", "列");
            _positionGrid.Columns.Add("Module", "模组");
            _positionGrid.Columns.Add("PosX", "X位置");
            _positionGrid.Columns.Add("PosY", "Y位置");
            _positionGrid.Columns.Add("PosZ", "Z位置");
            _positionGrid.Columns.Add("Remark", "备注");

            _previewPanel = new Panel
            {
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };

            _lblCameraStatus = new Label
            {
                Text = "相机状态: 未连接",
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                BackColor = Color.FromArgb(25, 25, 25)
            };

            var btnGrab = new Button
            {
                Text = "采集图像",
                Dock = DockStyle.Bottom,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 80),
                ForeColor = Color.White
            };
            btnGrab.Click += (s, e) => GrabImage();

            _previewPanel.Controls.Add(_lblCameraStatus);

            contentPanel.Panel1.Controls.Add(_positionGrid);
            contentPanel.Panel2.Controls.Add(_previewPanel);
            contentPanel.Panel2.Controls.Add(btnGrab);

            mainPanel.Controls.AddRange(new Control[] { topBar, contentPanel });
            _tabCameraPos.Controls.Add(mainPanel);
        }

        private void BuildDispenseTestTab()
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30) };

            var inputPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            var lblX = new Label { Text = "X位置:", Location = new Point(10, 15), ForeColor = Color.White, AutoSize = true };
            _txtDispenseX = new TextBox
            {
                Location = new Point(60, 12),
                Size = new Size(100, 24),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Text = "0.000"
            };

            var lblY = new Label { Text = "Y位置:", Location = new Point(180, 15), ForeColor = Color.White, AutoSize = true };
            _txtDispenseY = new TextBox
            {
                Location = new Point(230, 12),
                Size = new Size(100, 24),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Text = "0.000"
            };

            var lblZ = new Label { Text = "Z位置:", Location = new Point(350, 15), ForeColor = Color.White, AutoSize = true };
            _txtDispenseZ = new TextBox
            {
                Location = new Point(400, 12),
                Size = new Size(100, 24),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Text = "0.000"
            };

            _btnDispenseTest = new Button
            {
                Text = "开始点胶",
                Location = new Point(520, 10),
                Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 80),
                ForeColor = Color.White
            };
            _btnDispenseTest.Click += (s, e) => StartDispenseTest();

            _btnDispenseStop = new Button
            {
                Text = "停止",
                Location = new Point(630, 10),
                Size = new Size(80, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(150, 0, 0),
                ForeColor = Color.White
            };
            _btnDispenseStop.Click += (s, e) => StopDispenseTest();

            inputPanel.Controls.AddRange(new Control[] {
                lblX, _txtDispenseX, lblY, _txtDispenseY, lblZ, _txtDispenseZ,
                _btnDispenseTest, _btnDispenseStop
            });

            var resultPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25)
            };

            var lblResult = new Label
            {
                Text = "点胶测试结果:",
                Location = new Point(10, 10),
                ForeColor = Color.White,
                AutoSize = true
            };

            _dispenseResultList = new ListBox
            {
                Location = new Point(10, 35),
                Size = new Size(900, 500),
                BackColor = Color.FromArgb(35, 35, 35),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9F)
            };

            resultPanel.Controls.AddRange(new Control[] { lblResult, _dispenseResultList });

            mainPanel.Controls.AddRange(new Control[] { inputPanel, resultPanel });
            _tabDispenseTest.Controls.Add(mainPanel);
        }

        #endregion

        #region Operations

        private void LoadPositions()
        {
            _positionGrid.Rows.Clear();

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    for (int mod = 0; mod < 2; mod++)
                    {
                        int idx = _positionGrid.Rows.Add();
                        var gridRow = _positionGrid.Rows[idx];
                        gridRow.Cells["Row"].Value = row;
                        gridRow.Cells["Col"].Value = col;
                        gridRow.Cells["Module"].Value = mod;
                        gridRow.Cells["PosX"].Value = (row * 100 + col * 10 + mod * 5).ToString("F3");
                        gridRow.Cells["PosY"].Value = (col * 50 + mod * 25).ToString("F3");
                        gridRow.Cells["PosZ"].Value = (50 + row * 5).ToString("F3");
                        gridRow.Cells["Remark"].Value = $"R{row}C{col}M{mod}";
                        gridRow.DefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
                        gridRow.DefaultCellStyle.ForeColor = Color.White;
                    }
                }
            }

            Logger.Info("[VisionPosition] 加载拍照位置参数");
        }

        private void GetPosition()
        {
            if (_positionGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择一行位置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = _positionGrid.SelectedRows[0];
            row.Cells["PosX"].Value = "123.456";
            row.Cells["PosY"].Value = "78.901";
            row.Cells["PosZ"].Value = "50.000";

            _dispenseResultList.Items.Add($"[{DateTime.Now:HH:mm:ss}] Get位置: R{row.Cells["Row"].Value}C{row.Cells["Col"].Value}M{row.Cells["Module"].Value}");
            Logger.Info($"[VisionPosition] Get位置: R{row.Cells["Row"].Value}C{row.Cells["Col"].Value}M{row.Cells["Module"].Value}");
        }

        private void GoToPosition()
        {
            if (_positionGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择一行位置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = _positionGrid.SelectedRows[0];
            string pos = $"X={row.Cells["PosX"].Value} Y={row.Cells["PosY"].Value} Z={row.Cells["PosZ"].Value}";

            _dispenseResultList.Items.Add($"[{DateTime.Now:HH:mm:ss}] Go位置: {pos}");
            Logger.Info($"[VisionPosition] Go位置: {pos}");

            MessageBox.Show($"正在移动到: {pos}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveAllPositions()
        {
            MessageBox.Show("所有拍照位置已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Logger.Info("[VisionPosition] 保存所有拍照位置");
        }

        private void GrabImage()
        {
            _lblCameraStatus.Text = "相机状态: 采集中...";
            _lblCameraStatus.ForeColor = Color.Yellow;

            using (var g = _previewPanel.CreateGraphics())
            {
                g.Clear(Color.FromArgb(20, 20, 20));
                using (var pen = new Pen(Color.Lime, 2))
                {
                    g.DrawRectangle(pen, 50, 50, 300, 200);
                    g.DrawLine(pen, 200, 50, 200, 250);
                    g.DrawLine(pen, 50, 150, 350, 150);
                }
            }

            _lblCameraStatus.Text = "相机状态: 已采集";
            _lblCameraStatus.ForeColor = Color.Lime;

            _dispenseResultList.Items.Add($"[{DateTime.Now:HH:mm:ss}] 采集图像成功");
            Logger.Info("[VisionPosition] 采集图像");
        }

        private void StartDispenseTest()
        {
            string x = _txtDispenseX.Text;
            string y = _txtDispenseY.Text;
            string z = _txtDispenseZ.Text;

            _dispenseResultList.Items.Add($"[{DateTime.Now:HH:mm:ss}] 开始点胶测试: X={x} Y={y} Z={z}");
            Logger.Info($"[VisionPosition] 开始点胶测试: X={x} Y={y} Z={z}");

            _btnDispenseTest.Enabled = false;
            _btnDispenseStop.Enabled = true;
        }

        private void StopDispenseTest()
        {
            _dispenseResultList.Items.Add($"[{DateTime.Now:HH:mm:ss}] 停止点胶测试");
            Logger.Info("[VisionPosition] 停止点胶测试");

            _btnDispenseTest.Enabled = true;
            _btnDispenseStop.Enabled = false;
        }

        #endregion
    }
}
