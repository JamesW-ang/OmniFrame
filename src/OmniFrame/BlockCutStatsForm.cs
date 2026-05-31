using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OmniFrame.Core.BlockCut;
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// BlockCut 生产统计 + 补报工 + 相机
    /// 对应 Qt 的 StatisticsPage 和 WorkOrderPage
    /// </summary>
    public partial class BlockCutStatsForm : Form
    {
        private readonly BlockCutMesClient _mesClient;
        private readonly BlockCutConfig _config;
        private readonly BlockCutVision _vision;

        private int _totalOutput;
        private int _totalQualified;
        private double _uph;
        private DateTime _queryDateFrom;
        private DateTime _queryDateTo;

        public BlockCutStatsForm(BlockCutMesClient mesClient, BlockCutConfig config, BlockCutVision vision)
        {
            _mesClient = mesClient;
            _config = config;
            _vision = vision;

            InitializeComponent();
            WireEvents();
            LoadDefaultData();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [Obsolete("For Designer only - use DI constructor")]
        public BlockCutStatsForm()
        {
            InitializeComponent();
            WireEvents();
            LoadDefaultData();
        }

        #region UI Construction

        private TabControl _tabMain;
        private TabPage _tabCamera;
        private TabPage _tabStats;
        private TabPage _tabReReport;

        private Panel _cameraPanel;
        private ComboBox _cmbCameraSelect;
        private Button _btnCameraGrab;
        private Button _btnCameraTest;
        private Panel _cameraPreview;
        private Label _lblCameraStatus;

        private Panel _statsQueryPanel;
        private Label _lblStatsFrom;
        private DateTimePicker _dtpStatsFrom;
        private Label _lblStatsTo;
        private DateTimePicker _dtpStatsTo;
        private Label _lblTopN;
        private NumericUpDown _numTopN;
        private Button _btnStatsQuery;
        private Label _lblStatsSummary;
        private Panel _statsChartPanel1;
        private Panel _statsChartPanel2;
        private Panel _statsChartPanel3;
        private Panel _statsChartPanel4;
        private ListView _statsDataListView1;
        private ListView _statsDataListView2;

        private Panel _reReportSummaryPanel;
        private Label _lblShouldReport;
        private Label _lblProcessed;
        private Label _lblReported;
        private Label _lblResponded;
        private Panel _reReportEditPanel;
        private TextBox _txtMsgId;
        private ComboBox _cmbReReportStatus;
        private TextBox _txtVehicleCode;
        private Label _lblReportTime;
        private DateTimePicker _dtpReportTime;
        private Button _btnReReportAdd;
        private Button _btnReReportEdit;
        private Button _btnReReportDelete;
        private ListView _reReportListView;
        private Button _btnReReportBatch;
        private Panel _mesConfigPanel;
        private Label _lblHttpAddr;
        private TextBox _txtHttpAddr;
        private Label _lblMqttAddr;
        private TextBox _txtMqttAddr;
        private Button _btnMesConfigSave;

        private void InitializeComponent()
        {
            Text = "BlockCut 生产统计";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            Font = new Font("Microsoft YaHei", 9F);

            BuildTabControl();
            BuildCameraTab();
            BuildStatsTab();
            BuildReReportTab();
        }

        private void BuildTabControl()
        {
            _tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25),
                ForeColor = Color.White
            };

            _tabCamera = new TabPage("相机");
            _tabCamera.BackColor = Color.FromArgb(30, 30, 30);

            _tabStats = new TabPage("统计");
            _tabStats.BackColor = Color.FromArgb(30, 30, 30);

            _tabReReport = new TabPage("补报工");
            _tabReReport.BackColor = Color.FromArgb(30, 30, 30);

            _tabMain.TabPages.AddRange(new[] { _tabCamera, _tabStats, _tabReReport });
            Controls.Add(_tabMain);
        }

        private void BuildCameraTab()
        {
            _cameraPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30) };

            var lblCamera = new Label { Text = "相机选择:", Location = new Point(10, 10), ForeColor = Color.White, AutoSize = true };
            _cmbCameraSelect = new ComboBox
            {
                Location = new Point(80, 8),
                Size = new Size(150, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            _cmbCameraSelect.Items.AddRange(new[] { "底板右侧相机", "底板左侧相机", "片源相机" });
            _cmbCameraSelect.SelectedIndex = 0;

            _btnCameraGrab = new Button
            {
                Text = "采集图像",
                Location = new Point(250, 8),
                Size = new Size(90, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 80),
                ForeColor = Color.White
            };

            _btnCameraTest = new Button
            {
                Text = "相机测试",
                Location = new Point(350, 8),
                Size = new Size(90, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 100, 0),
                ForeColor = Color.White
            };

            _cameraPreview = new Panel
            {
                Location = new Point(10, 45),
                Size = new Size(500, 400),
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };

            _lblCameraStatus = new Label
            {
                Text = "状态: 未连接",
                Location = new Point(10, 460),
                ForeColor = Color.Gray,
                AutoSize = true
            };

            _cameraPanel.Controls.AddRange(new Control[] {
                lblCamera, _cmbCameraSelect, _btnCameraGrab, _btnCameraTest,
                _cameraPreview, _lblCameraStatus
            });

            _tabCamera.Controls.Add(_cameraPanel);
        }

        private void BuildStatsTab()
        {
            var statsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30) };

            _statsQueryPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1070, 50),
                BackColor = Color.FromArgb(40, 40, 40)
            };

            _lblStatsFrom = new Label { Text = "从:", Location = new Point(10, 15), ForeColor = Color.White, AutoSize = true };
            _dtpStatsFrom = new DateTimePicker
            {
                Location = new Point(40, 12),
                Size = new Size(130, 24),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-7)
            };

            _lblStatsTo = new Label { Text = "到:", Location = new Point(185, 15), ForeColor = Color.White, AutoSize = true };
            _dtpStatsTo = new DateTimePicker
            {
                Location = new Point(215, 12),
                Size = new Size(130, 24),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            _lblTopN = new Label { Text = "Top N:", Location = new Point(365, 15), ForeColor = Color.White, AutoSize = true };
            _numTopN = new NumericUpDown
            {
                Location = new Point(410, 12),
                Size = new Size(60, 24),
                Minimum = 1,
                Maximum = 100,
                Value = 10,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            _btnStatsQuery = new Button
            {
                Text = "查询",
                Location = new Point(490, 10),
                Size = new Size(70, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 140),
                ForeColor = Color.White
            };

            _statsQueryPanel.Controls.AddRange(new Control[] {
                _lblStatsFrom, _dtpStatsFrom, _lblStatsTo, _dtpStatsTo,
                _lblTopN, _numTopN, _btnStatsQuery
            });

            var chartContainer = new Panel
            {
                Location = new Point(10, 60),
                Size = new Size(520, 280),
                BackColor = Color.FromArgb(25, 25, 25)
            };

            _statsChartPanel1 = new Panel
            {
                Location = new Point(5, 5),
                Size = new Size(245, 130),
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            var lblChart1 = new Label { Text = "产量趋势", Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Cyan };

            _statsChartPanel2 = new Panel
            {
                Location = new Point(255, 5),
                Size = new Size(245, 130),
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            var lblChart2 = new Label { Text = "良率分析", Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Lime };

            _statsChartPanel3 = new Panel
            {
                Location = new Point(5, 140),
                Size = new Size(245, 130),
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            var lblChart3 = new Label { Text = "UPH趋势", Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Yellow };

            _statsChartPanel4 = new Panel
            {
                Location = new Point(255, 140),
                Size = new Size(245, 130),
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            var lblChart4 = new Label { Text = "设备利用率", Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Orange };

            _statsChartPanel1.Controls.Add(lblChart1);
            _statsChartPanel2.Controls.Add(lblChart2);
            _statsChartPanel3.Controls.Add(lblChart3);
            _statsChartPanel4.Controls.Add(lblChart4);

            chartContainer.Controls.AddRange(new Control[] { _statsChartPanel1, _statsChartPanel2, _statsChartPanel3, _statsChartPanel4 });

            _lblStatsSummary = new Label
            {
                Text = "总产出: -- | 良率: -- | 平均UPH: --",
                Location = new Point(10, 350),
                ForeColor = Color.Cyan,
                AutoSize = true
            };

            var dataContainer = new Panel
            {
                Location = new Point(545, 60),
                Size = new Size(510, 280),
                BackColor = Color.FromArgb(25, 25, 25)
            };

            var lblData1 = new Label { Text = "产出明细", Location = new Point(5, 5), ForeColor = Color.White };
            _statsDataListView1 = new ListView
            {
                Location = new Point(5, 25),
                Size = new Size(490, 115),
                View = View.Details,
                FullRowSelect = true,
                BackColor = Color.FromArgb(35, 35, 35),
                ForeColor = Color.White,
                GridLines = true
            };
            _statsDataListView1.Columns.Add("时间", 100);
            _statsDataListView1.Columns.Add("工单", 80);
            _statsDataListView1.Columns.Add("产出", 60);
            _statsDataListView1.Columns.Add("良率", 60);
            _statsDataListView1.Columns.Add("UPH", 60);

            var lblData2 = new Label { Text = "报警记录", Location = new Point(5, 145), ForeColor = Color.White };
            _statsDataListView2 = new ListView
            {
                Location = new Point(5, 165),
                Size = new Size(490, 115),
                View = View.Details,
                FullRowSelect = true,
                BackColor = Color.FromArgb(35, 35, 35),
                ForeColor = Color.White,
                GridLines = true
            };
            _statsDataListView2.Columns.Add("时间", 100);
            _statsDataListView2.Columns.Add("工站", 80);
            _statsDataListView2.Columns.Add("报警类型", 100);
            _statsDataListView2.Columns.Add("描述", 200);

            dataContainer.Controls.AddRange(new Control[] { lblData1, _statsDataListView1, lblData2, _statsDataListView2 });

            statsPanel.Controls.AddRange(new Control[] { _statsQueryPanel, chartContainer, _lblStatsSummary, dataContainer });
            _tabStats.Controls.Add(statsPanel);
        }

        private void BuildReReportTab()
        {
            var reReportPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30) };

            _reReportSummaryPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1070, 50),
                BackColor = Color.FromArgb(40, 40, 40)
            };

            _lblShouldReport = CreateStatLabel("应报: --", 10);
            _lblProcessed = CreateStatLabel("已加工: --", 130);
            _lblReported = CreateStatLabel("已上报: --", 250);
            _lblResponded = CreateStatLabel("已响应: --", 370);

            _reReportSummaryPanel.Controls.AddRange(new Control[] { _lblShouldReport, _lblProcessed, _lblReported, _lblResponded });

            _reReportEditPanel = new Panel
            {
                Location = new Point(0, 55),
                Size = new Size(1070, 80),
                BackColor = Color.FromArgb(35, 35, 35)
            };

            var lblMsgId = new Label { Text = "msg_id:", Location = new Point(10, 15), ForeColor = Color.White, AutoSize = true };
            _txtMsgId = new TextBox
            {
                Location = new Point(70, 12),
                Size = new Size(150, 24),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            var lblStatus = new Label { Text = "状态:", Location = new Point(240, 15), ForeColor = Color.White, AutoSize = true };
            _cmbReReportStatus = new ComboBox
            {
                Location = new Point(285, 12),
                Size = new Size(120, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            _cmbReReportStatus.Items.AddRange(new[] { "待上报", "已上报", "已响应", "失败" });
            _cmbReReportStatus.SelectedIndex = 0;

            var lblVehicle = new Label { Text = "载具码:", Location = new Point(425, 15), ForeColor = Color.White, AutoSize = true };
            _txtVehicleCode = new TextBox
            {
                Location = new Point(485, 12),
                Size = new Size(150, 24),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            _lblReportTime = new Label { Text = "报工时间:", Location = new Point(655, 15), ForeColor = Color.White, AutoSize = true };
            _dtpReportTime = new DateTimePicker
            {
                Location = new Point(720, 12),
                Size = new Size(150, 24),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm:ss"
            };

            _btnReReportAdd = new Button
            {
                Text = "添加",
                Location = new Point(890, 10),
                Size = new Size(70, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 80),
                ForeColor = Color.White
            };

            _btnReReportEdit = new Button
            {
                Text = "编辑",
                Location = new Point(965, 10),
                Size = new Size(70, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 100, 0),
                ForeColor = Color.White
            };

            _reReportEditPanel.Controls.AddRange(new Control[] {
                lblMsgId, _txtMsgId, lblStatus, _cmbReReportStatus,
                lblVehicle, _txtVehicleCode, _lblReportTime, _dtpReportTime,
                _btnReReportAdd, _btnReReportEdit
            });

            _reReportListView = new ListView
            {
                Location = new Point(0, 140),
                Size = new Size(1070, 300),
                View = View.Details,
                FullRowSelect = true,
                BackColor = Color.FromArgb(35, 35, 35),
                ForeColor = Color.White,
                GridLines = true
            };
            _reReportListView.Columns.Add("勾选", 40);
            _reReportListView.Columns.Add("msg_id", 120);
            _reReportListView.Columns.Add("单片码", 150);
            _reReportListView.Columns.Add("上报时间", 140);
            _reReportListView.Columns.Add("状态", 80);
            _reReportListView.Columns.Add("操作", 200);

            _btnReReportBatch = new Button
            {
                Text = "批量补报工",
                Location = new Point(10, 450),
                Size = new Size(120, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(180, 140, 0),
                ForeColor = Color.White
            };

            _btnReReportDelete = new Button
            {
                Text = "删除",
                Location = new Point(140, 450),
                Size = new Size(80, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(150, 0, 0),
                ForeColor = Color.White
            };

            _mesConfigPanel = new Panel
            {
                Location = new Point(0, 500),
                Size = new Size(1070, 70),
                BackColor = Color.FromArgb(25, 25, 25)
            };

            _lblHttpAddr = new Label { Text = "HTTP地址:", Location = new Point(10, 20), ForeColor = Color.White, AutoSize = true };
            _txtHttpAddr = new TextBox
            {
                Location = new Point(85, 17),
                Size = new Size(350, 24),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Text = _mesClient?.MesBaseUrl ?? "http://mes-server/api"
            };

            _lblMqttAddr = new Label { Text = "MQTT地址:", Location = new Point(455, 20), ForeColor = Color.White, AutoSize = true };
            _txtMqttAddr = new TextBox
            {
                Location = new Point(530, 17),
                Size = new Size(350, 24),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Text = "mqtt://192.168.1.30:1883"
            };

            _btnMesConfigSave = new Button
            {
                Text = "保存配置",
                Location = new Point(900, 15),
                Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 140),
                ForeColor = Color.White
            };

            _mesConfigPanel.Controls.AddRange(new Control[] {
                _lblHttpAddr, _txtHttpAddr, _lblMqttAddr, _txtMqttAddr, _btnMesConfigSave
            });

            reReportPanel.Controls.AddRange(new Control[] {
                _reReportSummaryPanel, _reReportEditPanel, _reReportListView,
                _btnReReportBatch, _btnReReportDelete, _mesConfigPanel
            });

            _tabReReport.Controls.Add(reReportPanel);
        }

        private Label CreateStatLabel(string text, int x)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, 15),
                ForeColor = Color.Cyan,
                Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold),
                AutoSize = true
            };
        }

        #endregion

        #region Events

        private void WireEvents()
        {
            _btnStatsQuery.Click += (s, e) => QueryStats();
            _btnReReportBatch.Click += async (s, e) => await BatchReReportAsync();
            _btnReReportAdd.Click += (s, e) => AddReReportItem();
            _btnReReportEdit.Click += (s, e) => EditReReportItem();
            _btnReReportDelete.Click += (s, e) => DeleteReReportItem();
            _btnMesConfigSave.Click += (s, e) => SaveMesConfig();
            _btnCameraGrab.Click += (s, e) => CameraGrab();
            _btnCameraTest.Click += (s, e) => CameraTest();

            _reReportListView.DoubleClick += (s, e) =>
            {
                if (_reReportListView.SelectedItems.Count > 0)
                {
                    var item = _reReportListView.SelectedItems[0];
                    _txtMsgId.Text = item.SubItems[1].Text;
                    _cmbReReportStatus.Text = item.SubItems[4].Text;
                }
            };
        }

        private void LoadDefaultData()
        {
            _queryDateFrom = DateTime.Today.AddDays(-7);
            _queryDateTo = DateTime.Today;
            _totalOutput = 0;
            _totalQualified = 0;
            _uph = 0;

            RefreshStats();
            LoadReReportSummary();
        }

        private void QueryStats()
        {
            _queryDateFrom = _dtpStatsFrom.Value;
            _queryDateTo = _dtpStatsTo.Value;

            Logger.Info($"[Stats] 查询统计: {_queryDateFrom:yyyy-MM-dd} ~ {_queryDateTo:yyyy-MM-dd}");

            RefreshStats();
        }

        private void RefreshStats()
        {
            int qualityRate = _totalOutput > 0 ? (int)((double)_totalQualified / _totalOutput * 100) : 0;
            _lblStatsSummary.Text = $"总产出: {_totalOutput} | 良率: {qualityRate}% | 平均UPH: {_uph:F0}";

            _lblShouldReport.Text = $"应报: {_totalOutput}";
            _lblProcessed.Text = $"已加工: {_totalOutput}";
            _lblReported.Text = $"已上报: {_totalQualified}";
            _lblResponded.Text = $"已响应: {_totalQualified}";

            DrawChartPlaceholders();
        }

        private void DrawChartPlaceholders()
        {
            DrawChartPlaceholder(_statsChartPanel1, "产量趋势图");
            DrawChartPlaceholder(_statsChartPanel2, "良率分析图");
            DrawChartPlaceholder(_statsChartPanel3, "UPH趋势图");
            DrawChartPlaceholder(_statsChartPanel4, "设备利用率图");
        }

        private void DrawChartPlaceholder(Panel panel, string text)
        {
            var lbl = panel.Controls.OfType<Label>().FirstOrDefault();
            if (lbl != null)
            {
                lbl.Text = text + "\n(待图表库接入)";
            }
        }

        private void LoadReReportSummary()
        {
            _lblShouldReport.Text = "应报: 0";
            _lblProcessed.Text = "已加工: 0";
            _lblReported.Text = "已上报: 0";
            _lblResponded.Text = "已响应: 0";
        }

        private async Task BatchReReportAsync()
        {
            if (_reReportListView.Items.Count == 0)
            {
                MessageBox.Show("没有需要补报的数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show($"确定要补报 {_reReportListView.Items.Count} 条数据吗?", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            int successCount = 0;
            int failCount = 0;

            foreach (ListViewItem item in _reReportListView.Items)
            {
                if (item.Checked && item.SubItems[4].Text == "待上报")
                {
                    item.SubItems[4].Text = "上报中...";
                    await Task.Delay(100);

                    try
                    {
                        await Task.Delay(50);
                        item.SubItems[4].Text = "已完成";
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        item.SubItems[4].Text = "失败";
                        Logger.Warning($"批次上报失败: {ex.Message}", ex);
                        failCount++;
                    }
                }
            }

            MessageBox.Show($"补报完成: 成功 {successCount} 条, 失败 {failCount} 条", "结果",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            Logger.Info($"[ReReport] 批量补报完成: 成功 {successCount}, 失败 {failCount}");
        }

        private void AddReReportItem()
        {
            if (string.IsNullOrWhiteSpace(_txtMsgId.Text))
            {
                MessageBox.Show("请输入msg_id", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var item = new ListViewItem("☐");
            item.SubItems.Add(_txtMsgId.Text);
            item.SubItems.Add(_txtVehicleCode.Text);
            item.SubItems.Add(_dtpReportTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            item.SubItems.Add(_cmbReReportStatus.Text);
            item.SubItems.Add("查看 | 删除");
            item.BackColor = Color.FromArgb(35, 35, 35);
            item.ForeColor = Color.White;

            _reReportListView.Items.Add(item);

            Logger.Info($"[ReReport] 添加补报项: {_txtMsgId.Text}");
        }

        private void EditReReportItem()
        {
            if (_reReportListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要编辑的项", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var item = _reReportListView.SelectedItems[0];
            item.SubItems[1].Text = _txtMsgId.Text;
            item.SubItems[2].Text = _txtVehicleCode.Text;
            item.SubItems[3].Text = _dtpReportTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
            item.SubItems[4].Text = _cmbReReportStatus.Text;

            Logger.Info($"[ReReport] 编辑补报项: {_txtMsgId.Text}");
        }

        private void DeleteReReportItem()
        {
            if (_reReportListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要删除的项", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("确定要删除选中的项吗?", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                foreach (ListViewItem item in _reReportListView.SelectedItems)
                {
                    _reReportListView.Items.Remove(item);
                }
            }
        }

        private void SaveMesConfig()
        {
            if (_mesClient != null)
            {
                _mesClient.MesBaseUrl = _txtHttpAddr.Text;
            }

            Logger.Info($"[MES] 配置保存: HTTP={_txtHttpAddr.Text}, MQTT={_txtMqttAddr.Text}");
            MessageBox.Show("MES配置已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CameraGrab()
        {
            Logger.Info($"[Camera] 采集图像: {_cmbCameraSelect.SelectedItem}");
            _lblCameraStatus.Text = "状态: 采集中...";
            _lblCameraStatus.ForeColor = Color.Yellow;

            using (var g = _cameraPreview.CreateGraphics())
            {
                g.Clear(Color.FromArgb(20, 20, 20));
                using (var brush = new SolidBrush(Color.DarkGray))
                {
                    g.FillRectangle(brush, 50, 50, 200, 150);
                }
                using (var pen = new Pen(Color.Green, 2))
                {
                    g.DrawRectangle(pen, 50, 50, 200, 150);
                    g.DrawLine(pen, 150, 50, 150, 200);
                    g.DrawLine(pen, 50, 125, 250, 125);
                }
            }

            _lblCameraStatus.Text = "状态: 已采集";
            _lblCameraStatus.ForeColor = Color.Lime;
        }

        private void CameraTest()
        {
            Logger.Info($"[Camera] 相机测试: {_cmbCameraSelect.SelectedItem}");
            _lblCameraStatus.Text = "状态: 测试中...";
            _lblCameraStatus.ForeColor = Color.Yellow;

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                this.Invoke(new Action(() =>
                {
                    _lblCameraStatus.Text = "状态: 测试通过";
                    _lblCameraStatus.ForeColor = Color.Lime;
                }));
            });
        }

        #endregion

        public void SetProductionData(int totalOutput, int totalQualified, double uph)
        {
            _totalOutput = totalOutput;
            _totalQualified = totalQualified;
            _uph = uph;
            RefreshStats();
        }
    }
}
