using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OmniFrame.Common;
using OmniFrame.Core.PluginSystem;
using OmniFrame.Theme;

namespace OmniFrame
{
    public partial class PluginManagerForm : Form
    {
        private IPluginManager _pluginManager;
        private PluginInfo _selectedPlugin;

        // Right panel controls
        private Panel _rightPanel;
        private SplitContainer _splitContainer;
        private Label _lblDetailTitle, _lblDetailName, _lblDetailType, _lblDetailVersion, _lblDetailStatus;
        private Label _lblDetailDesc;
        private Button _btnLoad, _btnUnload, _btnRefresh;
        private GroupBox _grpOperation;
        private TextBox _txtOpOutput;

        // Motion plugin controls
        private Panel _pnlMotion;
        private TextBox _txtMotionIp, _txtMotionAxis, _txtMotionPos, _txtMotionSpeed;
        private Button _btnMotionConnect, _btnMotionDisconnect, _btnMotionMove, _btnMotionHome, _btnMotionPos;

        // PLC plugin controls
        private Panel _pnlPlc;
        private TextBox _txtPlcIp, _txtPlcPort, _txtPlcAddr, _txtPlcValue;
        private Button _btnPlcConnect, _btnPlcDisconnect, _btnPlcRead, _btnPlcWrite;

        // Business plugin controls
        private Panel _pnlBusiness;
        private TextBox _txtBizParams;
        private Button _btnBizExecute;

        public PluginManagerForm(IPluginManager pluginManager)
        {
            _pluginManager = pluginManager;
            InitializeComponent();
            BuildEnhancedUI();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        private void BuildEnhancedUI()
        {
            Text = "插件管理";
            MinimumSize = new Size(960, 520);

            _splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            _splitContainer.HandleCreated += (s, e) =>
            {
                try
                {
                    int minPanel1 = Math.Max(200, _splitContainer.Width / 3);
                    int minPanel2 = Math.Max(200, _splitContainer.Width / 3);
                    
                    _splitContainer.Panel1MinSize = minPanel1;
                    _splitContainer.Panel2MinSize = minPanel2;
                    
                    int minDistance = minPanel1;
                    int maxDistance = _splitContainer.Width - minPanel2 - 20;
                    
                    if (maxDistance <= minDistance)
                    {
                        maxDistance = minDistance + 100;
                    }
                    
                    _splitContainer.SplitterDistance = Math.Max(minDistance, Math.Min(500, maxDistance));
                }
                catch (Exception ex)
                {
                    Logger.Error($"设置 SplitterDistance 失败: {ex.Message}");
                    try { _splitContainer.SplitterDistance = 400; } catch { }
                }
            };

            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Height = 56;
            
            dataGridView_Plugins.Dock = DockStyle.Fill;
            dataGridView_Plugins.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView_Plugins.MultiSelect = false;
            dataGridView_Plugins.SelectionChanged += OnPluginSelected;
            
            tableLayoutPanel1.Controls.Remove(dataGridView_Plugins);
            
            _splitContainer.Panel1.Controls.Add(dataGridView_Plugins);
            _splitContainer.Panel1.Controls.Add(tableLayoutPanel1);

            BuildRightPanel();
            _splitContainer.Panel2.Controls.Add(_rightPanel);

            Controls.Clear();
            Controls.Add(_splitContainer);
        }

        private void BuildRightPanel()
        {
            _rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), AutoScroll = true };

            var titleFont = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            var sectionFont = new Font("Microsoft YaHei", 10F, FontStyle.Bold);
            var bodyFont = new Font("Microsoft YaHei", 9.5F);
            int y = 6;

            // ── Title ──
            _lblDetailTitle = new Label { Text = "插件详情", Font = titleFont, Location = new Point(0, y), Size = new Size(340, 26), ForeColor = Color.SteelBlue };
            _rightPanel.Controls.Add(_lblDetailTitle);
            y += 34;

            // ── Info group ──
            var grpInfo = new GroupBox { Text = "基本信息", Font = sectionFont, Location = new Point(0, y), Size = new Size(340, 150) };
            _lblDetailName = new Label { Text = "名称: -", Font = bodyFont, Location = new Point(10, 22), Size = new Size(320, 20) };
            _lblDetailType = new Label { Text = "类型: -", Font = bodyFont, Location = new Point(10, 44), Size = new Size(320, 20) };
            _lblDetailVersion = new Label { Text = "版本: -", Font = bodyFont, Location = new Point(10, 66), Size = new Size(320, 20) };
            _lblDetailStatus = new Label { Text = "状态: -", Font = bodyFont, Location = new Point(10, 88), Size = new Size(320, 20) };
            _lblDetailDesc = new Label { Text = "描述: -", Font = bodyFont, Location = new Point(10, 110), Size = new Size(320, 20) };
            grpInfo.Controls.AddRange(new Control[] { _lblDetailName, _lblDetailType, _lblDetailVersion, _lblDetailStatus, _lblDetailDesc });
            _rightPanel.Controls.Add(grpInfo);
            y += 158;

            // ── Load/Unload buttons ──
            _btnLoad = new Button { Text = "加载插件", Location = new Point(0, y), Size = new Size(100, 34), BackColor = Color.FromArgb(50, 140, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnUnload = new Button { Text = "卸载插件", Location = new Point(112, y), Size = new Size(100, 34), BackColor = Color.FromArgb(180, 60, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
            _btnRefresh = new Button { Text = "刷新列表", Location = new Point(224, y), Size = new Size(100, 34) };
            _btnLoad.Click += BtnLoad_Click;
            _btnUnload.Click += BtnUnload_Click;
            _btnRefresh.Click += (s, e) => { _pluginManager.ScanPlugins(); LoadPluginList(); };
            _rightPanel.Controls.AddRange(new Control[] { _btnLoad, _btnUnload, _btnRefresh });
            y += 42;

            // ── Operation group (context-sensitive) ──
            _grpOperation = new GroupBox { Text = "插件操作", Font = sectionFont, Location = new Point(0, y), Size = new Size(340, 170), Visible = false };
            BuildMotionPanel();
            BuildPlcPanel();
            BuildBusinessPanel();
            _grpOperation.Controls.AddRange(new Control[] { _pnlMotion, _pnlPlc, _pnlBusiness });
            _rightPanel.Controls.Add(_grpOperation);
            y += 178;

            // ── Output ──
            var lblOut = new Label { Text = "操作结果", Font = sectionFont, Location = new Point(0, y), Size = new Size(340, 20) };
            _rightPanel.Controls.Add(lblOut);
            y += 22;
            _txtOpOutput = new TextBox
            {
                Location = new Point(0, y),
                Size = new Size(340, 100),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGreen
            };
            _rightPanel.Controls.Add(_txtOpOutput);
        }

        private void BuildMotionPanel()
        {
            _pnlMotion = new Panel { Dock = DockStyle.Fill, Visible = false };
            int y = 18;
            var f = new Font("Microsoft YaHei", 9F);

            _pnlMotion.Controls.Add(new Label { Text = "IP 地址:", Location = new Point(10, y + 3), Size = new Size(60, 22), Font = f });
            _txtMotionIp = new TextBox { Text = "192.168.1.100", Location = new Point(72, y), Size = new Size(120, 24), Font = f };
            _btnMotionConnect = new Button { Text = "连接", Location = new Point(200, y), Size = new Size(60, 26), BackColor = Color.FromArgb(50, 120, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnMotionDisconnect = new Button { Text = "断开", Location = new Point(266, y), Size = new Size(60, 26), BackColor = Color.FromArgb(160, 50, 30), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnMotionConnect.Click += BtnMotionConnect_Click;
            _btnMotionDisconnect.Click += BtnMotionDisconnect_Click;
            _pnlMotion.Controls.AddRange(new Control[] { _txtMotionIp, _btnMotionConnect, _btnMotionDisconnect });
            y += 32;

            _pnlMotion.Controls.Add(new Label { Text = "轴号:", Location = new Point(10, y + 3), Size = new Size(42, 22), Font = f });
            _txtMotionAxis = new TextBox { Text = "0", Location = new Point(52, y), Size = new Size(48, 24), Font = f };
            _pnlMotion.Controls.Add(new Label { Text = "位置:", Location = new Point(108, y + 3), Size = new Size(42, 22), Font = f });
            _txtMotionPos = new TextBox { Text = "100", Location = new Point(144, y), Size = new Size(60, 24), Font = f };
            _pnlMotion.Controls.Add(new Label { Text = "速度:", Location = new Point(212, y + 3), Size = new Size(42, 22), Font = f });
            _txtMotionSpeed = new TextBox { Text = "200", Location = new Point(248, y), Size = new Size(60, 24), Font = f };
            _pnlMotion.Controls.AddRange(new Control[] { _txtMotionAxis, _txtMotionPos, _txtMotionSpeed });
            y += 32;

            _btnMotionMove = new Button { Text = "移动到目标位置", Location = new Point(10, y), Size = new Size(140, 30), BackColor = Color.FromArgb(50, 100, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnMotionHome = new Button { Text = "回零", Location = new Point(160, y), Size = new Size(80, 30) };
            _btnMotionPos = new Button { Text = "读取位置", Location = new Point(250, y), Size = new Size(80, 30) };
            _btnMotionMove.Click += BtnMotionMove_Click;
            _btnMotionHome.Click += BtnMotionHome_Click;
            _btnMotionPos.Click += BtnMotionPos_Click;
            _pnlMotion.Controls.AddRange(new Control[] { _btnMotionMove, _btnMotionHome, _btnMotionPos });
        }

        private void BuildPlcPanel()
        {
            _pnlPlc = new Panel { Dock = DockStyle.Fill, Visible = false };
            int y = 18;
            var f = new Font("Microsoft YaHei", 9F);

            _pnlPlc.Controls.Add(new Label { Text = "IP:", Location = new Point(10, y + 3), Size = new Size(30, 22), Font = f });
            _txtPlcIp = new TextBox { Text = "192.168.1.10", Location = new Point(40, y), Size = new Size(110, 24), Font = f };
            _pnlPlc.Controls.Add(new Label { Text = "端口:", Location = new Point(156, y + 3), Size = new Size(40, 22), Font = f });
            _txtPlcPort = new TextBox { Text = "502", Location = new Point(196, y), Size = new Size(50, 24), Font = f };
            _btnPlcConnect = new Button { Text = "连接", Location = new Point(254, y), Size = new Size(60, 26), BackColor = Color.FromArgb(50, 120, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnPlcDisconnect = new Button { Text = "断开", Location = new Point(254, y + 30), Size = new Size(60, 26), BackColor = Color.FromArgb(160, 50, 30), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnPlcConnect.Click += BtnPlcConnect_Click;
            _btnPlcDisconnect.Click += BtnPlcDisconnect_Click;
            _pnlPlc.Controls.AddRange(new Control[] { _txtPlcIp, _txtPlcPort, _btnPlcConnect, _btnPlcDisconnect });
            y += 32;

            _pnlPlc.Controls.Add(new Label { Text = "寄存器地址:", Location = new Point(10, y + 3), Size = new Size(76, 22), Font = f });
            _txtPlcAddr = new TextBox { Text = "D100", Location = new Point(88, y), Size = new Size(70, 24), Font = f };
            _pnlPlc.Controls.Add(new Label { Text = "写入值:", Location = new Point(166, y + 3), Size = new Size(54, 22), Font = f });
            _txtPlcValue = new TextBox { Text = "1234", Location = new Point(220, y), Size = new Size(60, 24), Font = f };
            _pnlPlc.Controls.AddRange(new Control[] { _txtPlcAddr, _txtPlcValue });
            y += 32;

            _btnPlcRead = new Button { Text = "读取寄存器", Location = new Point(10, y), Size = new Size(120, 30), BackColor = Color.FromArgb(50, 100, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnPlcWrite = new Button { Text = "写入寄存器", Location = new Point(140, y), Size = new Size(120, 30) };
            _btnPlcRead.Click += BtnPlcRead_Click;
            _btnPlcWrite.Click += BtnPlcWrite_Click;
            _pnlPlc.Controls.AddRange(new Control[] { _btnPlcRead, _btnPlcWrite });
        }

        private void BuildBusinessPanel()
        {
            _pnlBusiness = new Panel { Dock = DockStyle.Fill, Visible = false };
            int y = 18;
            var f = new Font("Microsoft YaHei", 9F);

            _pnlBusiness.Controls.Add(new Label { Text = "操作参数:", Location = new Point(10, y + 3), Size = new Size(70, 22), Font = f });
            _txtBizParams = new TextBox { Text = "WeighAndUpload", Location = new Point(82, y), Size = new Size(180, 24), Font = f };
            _pnlBusiness.Controls.Add(_txtBizParams);

            _btnBizExecute = new Button { Text = "执行业务", Location = new Point(10, y + 36), Size = new Size(130, 34), BackColor = Color.FromArgb(50, 100, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnBizExecute.Click += BtnBizExecute_Click;
            _pnlBusiness.Controls.Add(_btnBizExecute);

            var lblHint = new Label
            {
                Text = "示例: WeighAndUpload | GenerateReport | ValidateRecipe",
                Location = new Point(10, y + 76),
                Size = new Size(320, 20),
                Font = new Font("Microsoft YaHei", 8F),
                ForeColor = Color.Gray
            };
            _pnlBusiness.Controls.Add(lblHint);
        }

        private void PluginManagerForm_Load(object sender, EventArgs e)
        {
            LoadPluginList();
        }

        private void LoadPluginList()
        {
            try
            {
                var plugins = _pluginManager.GetPlugins();
                dataGridView_Plugins.Rows.Clear();

                foreach (var p in plugins)
                {
                    dataGridView_Plugins.Rows.Add(
                        p.Name,
                        PluginTypeToString(p.PluginType),
                        p.Description,
                        p.Version.ToString(),
                        p.IsLoaded ? "已加载" : "未加载",
                        p.IsOfficial ? "官方" : "第三方",
                        p.Path
                    );
                }

                if (plugins.Count > 0 && dataGridView_Plugins.SelectedRows.Count == 0)
                    dataGridView_Plugins.Rows[0].Selected = true;
            }
            catch (Exception ex)
            {
                LogOutput($"加载插件列表失败: {ex.Message}");
            }
        }

        private void OnPluginSelected(object sender, EventArgs e)
        {
            if (dataGridView_Plugins.SelectedRows.Count > 0)
            {
                var row = dataGridView_Plugins.SelectedRows[0];
                string name = row.Cells[0].Value?.ToString();
                string typeStr = row.Cells[1].Value?.ToString();
                string desc = row.Cells[2].Value?.ToString();
                string ver = row.Cells[3].Value?.ToString();
                string loaded = row.Cells[4].Value?.ToString();

                _selectedPlugin = _pluginManager.GetPlugins().FirstOrDefault(p => p.Name == name && p.Version.ToString() == ver);

                _lblDetailTitle.Text = name ?? "插件详情";
                _lblDetailName.Text = $"名称: {name}";
                _lblDetailType.Text = $"类型: {typeStr}";
                _lblDetailVersion.Text = $"版本: v{ver}";
                _lblDetailStatus.Text = $"状态: {loaded}";
                _lblDetailDesc.Text = $"描述: {desc}";

                _btnUnload.Enabled = loaded == "已加载";
                ShowOperationPanel(typeStr);
            }
            else
            {
                _selectedPlugin = null;
                ClearRightPanel();
            }
        }

        private void ShowOperationPanel(string typeStr)
        {
            _pnlMotion.Visible = false;
            _pnlPlc.Visible = false;
            _pnlBusiness.Visible = false;
            _grpOperation.Visible = true;

            switch (typeStr)
            {
                case "运动控制":
                    _pnlMotion.Visible = true;
                    _grpOperation.Text = "运动控制操作";
                    break;
                case "PLC":
                    _pnlPlc.Visible = true;
                    _grpOperation.Text = "PLC 操作";
                    break;
                case "业务逻辑":
                    _pnlBusiness.Visible = true;
                    _grpOperation.Text = "业务操作";
                    break;
                default:
                    _grpOperation.Visible = false;
                    break;
            }
        }

        private void ClearRightPanel()
        {
            _lblDetailTitle.Text = "插件详情";
            _lblDetailName.Text = "名称: -";
            _lblDetailType.Text = "类型: -";
            _lblDetailVersion.Text = "版本: -";
            _lblDetailStatus.Text = "状态: -";
            _lblDetailDesc.Text = "描述: -";
            _btnUnload.Enabled = false;
            _grpOperation.Visible = false;
        }

        // ─── Button handlers ───

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            if (_selectedPlugin == null) { LogOutput("请先在左侧选择一个插件"); return; }
            bool ok = _pluginManager.LoadPlugin(_selectedPlugin.Name, _selectedPlugin.Version);
            LogOutput(ok ? $"插件 {_selectedPlugin.Name} 加载成功" : $"插件 {_selectedPlugin.Name} 加载失败");
            LoadPluginList();
            if (ok) _btnUnload.Enabled = true;
        }

        private void BtnUnload_Click(object sender, EventArgs e)
        {
            if (_selectedPlugin == null) { LogOutput("请先在左侧选择一个插件"); return; }
            _pluginManager.UnloadPlugin(_selectedPlugin.Name);
            LogOutput($"插件 {_selectedPlugin.Name} 已卸载");
            LoadPluginList();
            _btnUnload.Enabled = false;
        }

        // ─── Motion operations ───

        private void BtnMotionConnect_Click(object sender, EventArgs e)
        {
            var plugin = _pluginManager.GetLoadedPlugin<OmniFrame.Sdk.PluginSystem.MotionPlugin>(_selectedPlugin?.Name ?? "");
            if (plugin == null) { LogOutput("请先加载此插件"); return; }
            bool ok = plugin.Connect(_txtMotionIp.Text);
            LogOutput(ok ? $"已连接到 {_txtMotionIp.Text}" : "连接失败");
        }

        private void BtnMotionDisconnect_Click(object sender, EventArgs e)
        {
            var plugin = _pluginManager.GetLoadedPlugin<OmniFrame.Sdk.PluginSystem.MotionPlugin>(_selectedPlugin?.Name ?? "");
            if (plugin == null) { LogOutput("请先加载此插件"); return; }
            plugin.Disconnect();
            LogOutput("已断开连接");
        }

        private void BtnMotionMove_Click(object sender, EventArgs e)
        {
            var plugin = _pluginManager.GetLoadedPlugin<OmniFrame.Sdk.PluginSystem.MotionPlugin>(_selectedPlugin?.Name ?? "");
            if (plugin == null) { LogOutput("请先加载此插件"); return; }
            if (!int.TryParse(_txtMotionAxis.Text, out int axis)) { LogOutput("请输入有效轴号 (0-7)"); return; }
            if (!double.TryParse(_txtMotionPos.Text, out double pos)) { LogOutput("请输入有效位置值"); return; }
            if (!double.TryParse(_txtMotionSpeed.Text, out double speed)) { LogOutput("请输入有效速度值"); return; }
            bool ok = plugin.Move(axis, pos, speed);
            LogOutput(ok ? $"轴{axis} 移动到 {pos} (速度 {speed})" : $"轴{axis} 运动失败");
        }

        private void BtnMotionHome_Click(object sender, EventArgs e)
        {
            var plugin = _pluginManager.GetLoadedPlugin<OmniFrame.Sdk.PluginSystem.MotionPlugin>(_selectedPlugin?.Name ?? "");
            if (plugin == null) { LogOutput("请先加载此插件"); return; }
            if (!int.TryParse(_txtMotionAxis.Text, out int axis)) { LogOutput("请输入有效轴号 (0-7)"); return; }
            bool ok = plugin.Home(axis);
            LogOutput(ok ? $"轴{axis} 回零完成" : $"轴{axis} 回零失败");
        }

        private void BtnMotionPos_Click(object sender, EventArgs e)
        {
            var plugin = _pluginManager.GetLoadedPlugin<OmniFrame.Sdk.PluginSystem.MotionPlugin>(_selectedPlugin?.Name ?? "");
            if (plugin == null) { LogOutput("请先加载此插件"); return; }
            if (!int.TryParse(_txtMotionAxis.Text, out int axis)) { LogOutput("请输入有效轴号 (0-7)"); return; }
            double pos = plugin.GetCurrentPosition(axis);
            LogOutput($"轴{axis} 当前位置: {pos:F2}");
            _txtMotionPos.Text = pos.ToString("F1");
        }

        // ─── PLC operations ───

        private void BtnPlcConnect_Click(object sender, EventArgs e)
        {
            var plugin = _pluginManager.GetLoadedPlugin<OmniFrame.Sdk.PluginSystem.PlcPlugin>(_selectedPlugin?.Name ?? "");
            if (plugin == null) { LogOutput("请先加载此插件"); return; }
            if (!int.TryParse(_txtPlcPort.Text, out int port)) { LogOutput("请输入有效端口号"); return; }
            bool ok = plugin.Connect(_txtPlcIp.Text, port);
            LogOutput(ok ? $"已连接到 {_txtPlcIp.Text}:{port}" : "连接失败");
        }

        private void BtnPlcDisconnect_Click(object sender, EventArgs e)
        {
            var plugin = _pluginManager.GetLoadedPlugin<OmniFrame.Sdk.PluginSystem.PlcPlugin>(_selectedPlugin?.Name ?? "");
            if (plugin == null) { LogOutput("请先加载此插件"); return; }
            plugin.Disconnect();
            LogOutput("已断开PLC连接");
        }

        private void BtnPlcRead_Click(object sender, EventArgs e)
        {
            var plugin = _pluginManager.GetLoadedPlugin<OmniFrame.Sdk.PluginSystem.PlcPlugin>(_selectedPlugin?.Name ?? "");
            if (plugin == null) { LogOutput("请先加载此插件"); return; }
            int val = plugin.ReadRegister(_txtPlcAddr.Text);
            LogOutput($"读取 {_txtPlcAddr.Text} = {val}");
            _txtPlcValue.Text = val.ToString();
        }

        private void BtnPlcWrite_Click(object sender, EventArgs e)
        {
            var plugin = _pluginManager.GetLoadedPlugin<OmniFrame.Sdk.PluginSystem.PlcPlugin>(_selectedPlugin?.Name ?? "");
            if (plugin == null) { LogOutput("请先加载此插件"); return; }
            if (!int.TryParse(_txtPlcValue.Text, out int val)) { LogOutput("请输入有效数值"); return; }
            bool ok = plugin.WriteRegister(_txtPlcAddr.Text, val);
            LogOutput(ok ? $"写入 {_txtPlcAddr.Text} = {val}" : "写入失败");
        }

        // ─── Business operations ───

        private void BtnBizExecute_Click(object sender, EventArgs e)
        {
            var plugin = _pluginManager.GetLoadedPlugin<OmniFrame.Sdk.PluginSystem.BusinessPlugin>(_selectedPlugin?.Name ?? "");
            if (plugin == null) { LogOutput("请先加载此插件"); return; }
            var result = plugin.Execute(_txtBizParams.Text);
            if (result is Dictionary<string, object> dict)
            {
                var lines = string.Join("\r\n", dict.Select(kv => $"{kv.Key}: {kv.Value}"));
                LogOutput($"业务执行完成:\r\n{lines}");
            }
            else
            {
                LogOutput($"执行结果: {result}");
            }
        }

        private void LogOutput(string msg)
        {
            string ts = DateTime.Now.ToString("HH:mm:ss");
            _txtOpOutput.AppendText($"[{ts}] {msg}\r\n");
        }

        private static string PluginTypeToString(PluginType t) => t switch
        {
            PluginType.Motion => "运动控制",
            PluginType.Plc => "PLC",
            PluginType.Business => "业务逻辑",
            _ => t.ToString()
        };

        // Forward the designer button clicks to our handlers
        private void button_Refresh_Click(object sender, EventArgs e) { _pluginManager.ScanPlugins(); LoadPluginList(); LogOutput("插件列表已刷新"); }
        private void button_Load_Click(object sender, EventArgs e) => BtnLoad_Click(sender, e);
        private void button_Unload_Click(object sender, EventArgs e) => BtnUnload_Click(sender, e);

        private async void button_Update_Click(object sender, EventArgs e)
        {
            if (_selectedPlugin == null) { LogOutput("请先选择插件"); return; }
            using (var dialog = new InputDialog("更新插件", "请输入插件更新URL:"))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dialog.InputText))
                {
                    button_Update.Enabled = false;
                    label_Status.Text = "正在更新...";
                    try
                    {
                        bool ok = await _pluginManager.UpdatePlugin(_selectedPlugin.Name, dialog.InputText);
                        LogOutput(ok ? $"插件 {_selectedPlugin.Name} 更新成功" : $"插件 {_selectedPlugin.Name} 更新失败");
                        if (ok) LoadPluginList();
                    }
                    catch (Exception ex) { LogOutput($"更新失败: {ex.Message}"); }
                    finally { button_Update.Enabled = true; label_Status.Text = ""; }
                }
            }
        }

        private void button_Rollback_Click(object sender, EventArgs e)
        {
            if (_selectedPlugin == null) { LogOutput("请先选择插件"); return; }
            if (MessageBox.Show($"确定要回滚插件 {_selectedPlugin.Name} 到上一版本吗？", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _pluginManager.UnloadPlugin(_selectedPlugin.Name);
                _pluginManager.LoadPlugin(_selectedPlugin.Name);
                LoadPluginList();
                LogOutput($"插件 {_selectedPlugin.Name} 已回滚");
            }
        }

        private void dataGridView_Plugins_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dataGridView_Plugins.Rows[e.RowIndex];
                string name = row.Cells[0].Value?.ToString();
                string type = row.Cells[1].Value?.ToString();
                string ver = row.Cells[3].Value?.ToString();
                string desc = row.Cells[2].Value?.ToString();
                string loaded = row.Cells[4].Value?.ToString();
                string official = row.Cells[5].Value?.ToString();

                string msg = $"插件: {name}\r\n类型: {type}\r\n版本: v{ver}\r\n描述: {desc}\r\n状态: {loaded}\r\n签名: {official}";
                MessageBox.Show(msg, "插件详情", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
