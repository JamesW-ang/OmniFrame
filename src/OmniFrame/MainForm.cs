using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OmniFrame.Core;
using OmniFrame.Core.AdvancedFeatures;
using OmniFrame.Common;
using OmniFrame.Theme;
using ToolEx;

namespace OmniFrame
{
    public partial class MainForm : Form
    {
        private readonly ISystemManager _systemManager;
        private readonly IUserManager _userManager;
        private readonly IAlarmManager _alarmManager;
        private readonly IPermissionManager _permissionManager;
        private readonly FormFactory _formFactory;
        private readonly IUphManager _uphManager;
        private LoginForm _loginForm;

        private Dictionary<string, Form> _formCache;
        private readonly object _formCacheLock = new object();

        private LedIndicator _ledRun;
        private LedIndicator _ledAlarm;

        private bool _isOperatorMode = true;

        public MainForm(ISystemManager systemManager, IUserManager userManager, IAlarmManager alarmManager, IPermissionManager permissionManager, FormFactory formFactory, IUphManager uphManager)
        {
            _systemManager = systemManager;
            _userManager = userManager;
            _alarmManager = alarmManager;
            _permissionManager = permissionManager;
            _formFactory = formFactory;
            _uphManager = uphManager;
            InitializeComponent();
            InitializeSystem();
        }

        [Obsolete("For Designer only - use DI constructor")]
        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeSystem()
        {
            try
            {
                _formCache = new Dictionary<string, Form>();

                if (!_systemManager.Initialize())
                {
                    Logger.Error("系统管理器初始化失败");
                    MessageBox.Show("系统初始化失败，请检查系统配置!");
                    return;
                }

                _systemManager.StateChanged += OnSystemStateChanged;
                _systemManager.ErrorOccurred += OnSystemError;

                InitializeStatusBar();
                InitializeTreeView();
                UpdateViewMode();

                UiTheme.CurrentTheme = UiTheme.DarkTheme;
                this.ApplyTheme();

                Logger.Info("主窗体初始化完成");
            }
            catch (Exception ex)
            {
                Logger.Error("主窗体初始化失败", ex);
                MessageBox.Show($"系统初始化失败:\n{ex.Message}\n\n堆栈跟踪:\n{ex.StackTrace}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeStatusBar()
        {
            _ledRun = new LedIndicator
            {
                LabelText = "运行",
                OnColor = Color.Lime,
                OffColor = Color.Gray,
                ForeColor = Color.White,
                Size = new Size(80, 20)
            };

            _ledAlarm = new LedIndicator
            {
                LabelText = "报警",
                OnColor = Color.Red,
                OffColor = Color.Gray,
                ForeColor = Color.White,
                Size = new Size(80, 20)
            };

            var ledTcp = new LedIndicator { LabelText = "TCP", OnColor = Color.Green, OffColor = Color.Red, ForeColor = Color.White, Size = new Size(80, 20), IsOn = true };
            var ledOpc = new LedIndicator { LabelText = "OPC", OnColor = Color.Green, OffColor = Color.Red, ForeColor = Color.White, Size = new Size(80, 20), IsOn = true };
            var ledMqtt = new LedIndicator { LabelText = "MQTT", OnColor = Color.Green, OffColor = Color.Red, ForeColor = Color.White, Size = new Size(80, 20), IsOn = true };

            toolStripStatusLabel_userRole.Text = "用户: admin | 角色: 管理员";
            toolStripStatusLabel_userRole.Font = new Font("Microsoft YaHei", 9F);
            toolStripStatusLabel_stationStatus.Text = "工位: 工位1 | 状态: 运行";
            toolStripStatusLabel_stationStatus.Font = new Font("Microsoft YaHei", 9F);
            toolStripStatusLabel_systemInfo.Text = $"{DateTime.Now.ToString("HH:mm:ss")} | 今日产量: 0 | 良率: 0%";
            toolStripStatusLabel_systemInfo.Font = new Font("Microsoft YaHei", 9F);

            statusStrip1.Items.Add(new ToolStripControlHost(ledTcp));
            statusStrip1.Items.Add(new ToolStripControlHost(ledOpc));
            statusStrip1.Items.Add(new ToolStripControlHost(ledMqtt));
        }

        private void InitializeTreeView()
        {
            treeView1.Nodes.Clear();

            TreeNode mainNode = new TreeNode("主界面");
            mainNode.Nodes.Add("数据报表");
            mainNode.Nodes.Add("配方管理");
            mainNode.Nodes.Add("设备控制");
            mainNode.Nodes.Add("工位管理");

            var blockCutNode = new TreeNode("BlockCut 生产");
            blockCutNode.Nodes.Add("生产总览");
            blockCutNode.Nodes.Add("相机预览");
            blockCutNode.Nodes.Add("生产统计");
            blockCutNode.Nodes.Add("手动控制");
            blockCutNode.Nodes.Add("调试工具");
            blockCutNode.Nodes.Add("实时日志");
            mainNode.Nodes.Add(blockCutNode);

            var blockCutConfigNode = new TreeNode("BlockCut 配置");
            blockCutConfigNode.Nodes.Add("轴参数配置");
            blockCutConfigNode.Nodes.Add("IO信号配置");
            blockCutConfigNode.Nodes.Add("视觉参数配置");
            blockCutConfigNode.Nodes.Add("测量测试");
            blockCutConfigNode.Nodes.Add("MES配置");
            mainNode.Nodes.Add(blockCutConfigNode);

            mainNode.Nodes.Add("系统设置");

            TreeNode auxNode = new TreeNode("辅助功能");
            auxNode.Nodes.Add("配置管理");
            auxNode.Nodes.Add("配置向导");

            TreeNode advancedNode = new TreeNode("高级功能");
            advancedNode.Nodes.Add("OEE 管理");
            advancedNode.Nodes.Add("UPH 统计");
            advancedNode.Nodes.Add("插件管理");
            advancedNode.Nodes.Add("操作日志");

            TreeNode systemNode = new TreeNode("系统");
            systemNode.Nodes.Add("角色管理");
            systemNode.Nodes.Add("登录");
            systemNode.Nodes.Add("退出");

            treeView1.Nodes.Add(mainNode);
            treeView1.Nodes.Add(auxNode);
            treeView1.Nodes.Add(advancedNode);
            treeView1.Nodes.Add(systemNode);

            treeView1.ExpandAll();
            treeView1.AfterSelect += TreeView1_AfterSelect;
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string selectedNode = e.Node.Text;
            switch (selectedNode)
            {
                case "数据报表": ShowDataForm(); break;
                case "配方管理": ShowRecipeForm(); break;
                case "设备控制": ShowEquipmentForm(); break;
                case "工位管理": ShowStationForm(); break;
                case "BlockCut 生产":
                    e.Node.Expand();
                    if (e.Node.FirstNode != null)
                    {
                        treeView1.AfterSelect -= TreeView1_AfterSelect;
                        treeView1.SelectedNode = e.Node.FirstNode;
                        treeView1.AfterSelect += TreeView1_AfterSelect;
                    }
                    return;
                case "生产总览": ShowBlockCutForm(); break;
                case "相机预览": ShowBlockCutCameraForm(); break;
                case "生产统计": ShowBlockCutStatsForm(); break;
                case "手动控制": ShowBlockCutManualForm(); break;
                case "调试工具": ShowBlockCutDebugForm(); break;
                case "实时日志": ShowBlockCutLogForm(); break;
                case "BlockCut 配置":
                    e.Node.Expand();
                    if (e.Node.FirstNode != null)
                    {
                        treeView1.AfterSelect -= TreeView1_AfterSelect;
                        treeView1.SelectedNode = e.Node.FirstNode;
                        treeView1.AfterSelect += TreeView1_AfterSelect;
                    }
                    return;
                case "轴参数配置": ShowBlockCutAxisSetupForm(); break;
                case "IO信号配置": ShowBlockCutIOSignalForm(); break;
                case "视觉参数配置": ShowBlockCutVisionPositionForm(); break;
                case "测量测试": ShowBlockCutMeasureTestForm(); break;
                case "MES配置": ShowBlockCutMesConfigForm(); break;
                case "系统设置": ShowSettingForm(); break;
                case "配置管理": ShowConfigForm(); break;
                case "配置向导": ShowConfigWizardForm(); break;
                case "OEE 管理": ShowOeeForm(); break;
                case "UPH 统计": ShowUphForm(); break;
                case "插件管理": ShowPluginManagerForm(); break;
                case "操作日志": ShowOperationLogForm(); break;
                case "角色管理": ShowRoleManagerForm(); break;
                case "登录": ShowLoginForm(); break;
                case "退出": Application.Exit(); break;
            }
        }

        private void ShowLoginForm()
        {
            using (_loginForm = _formFactory.GetForm<LoginForm>())
            {
                if (_loginForm.ShowDialog(this) == DialogResult.OK)
                    UpdateUserInfo();
            }
        }

        private void UpdateUserInfo()
        {
            var user = _userManager?.CurrentUser;
            toolStripStatusLabel_userRole.Text = user != null
                ? $"用户: {user.UserName} | 角色: {user.Level}"
                : "用户: 未登录 | 角色: 访客";
            UpdateViewMode();
        }

        private void UpdateStatusBar()
        {
            if (_systemManager == null)
            {
                toolStripStatusLabel_systemInfo.Text = $"{DateTime.Now.ToString("HH:mm:ss")} | 今日产量: 0 | 良率: 0%";
                return;
            }

            var user = _userManager?.CurrentUser;
            toolStripStatusLabel_userRole.Text = user != null
                ? $"用户: {user.UserName} | 角色: {user.Level}"
                : "用户: 未登录 | 角色: 访客";

            string stateText = GetStateText(_systemManager.State);
            toolStripStatusLabel_stationStatus.Text = $"工位: 工位1 | 状态: {stateText}";
            toolStripStatusLabel_stationStatus.ForeColor = stateText switch
            {
                "运行" => Color.Green,
                "暂停" => Color.Yellow,
                "报警" or "错误" or "急停" => Color.Red,
                _ => Color.Black
            };

            toolStripStatusLabel_systemInfo.Text = $"{DateTime.Now.ToString("HH:mm:ss")} | 今日产量: 100 | 良率: 95%";

            _ledRun.IsOn = _systemManager.IsRunning;
            _ledAlarm.IsOn = _alarmManager?.ActiveAlarmCount > 0;
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

        private void OnSystemStateChanged(object sender, SystemStateChangedEventArgs e)
        {
            if (InvokeRequired) { Invoke(new Action(() => OnSystemStateChanged(sender, e))); return; }
            Logger.Info($"系统状态变更: {e.OldState} -> {e.NewState}");
        }

        private void OnSystemError(object sender, SystemErrorEventArgs e)
        {
            if (InvokeRequired) { Invoke(new Action(() => OnSystemError(sender, e))); return; }
            Logger.Error($"系统错误: {e.ErrorMessage}");
            MessageBox.Show($"系统错误:\n{e.ErrorMessage}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ToggleViewMode() { _isOperatorMode = !_isOperatorMode; UpdateViewMode(); }

        private void UpdateViewMode()
        {
            var user = _userManager?.CurrentUser;
            if (user != null && user.Level >= UserLevel.Administrator)
                _isOperatorMode = false;

            lblViewMode.Text = _isOperatorMode ? "模式: 操作员" : "模式: 工程师";
            lblViewMode.ForeColor = _isOperatorMode ? Color.FromArgb(100, 200, 255) : Color.FromArgb(255, 200, 100);
            UpdateTreeViewByRole();
        }

        private void UpdateTreeViewByRole()
        {
            foreach (TreeNode rootNode in treeView1.Nodes)
            {
                if (rootNode.Text == "高级功能")
                {
                    if (_isOperatorMode) { rootNode.ForeColor = Color.Gray; }
                    else { rootNode.ForeColor = treeView1.ForeColor; }
                    rootNode.Expand();
                }
                if (rootNode.Text == "系统")
                {
                    foreach (TreeNode node in rootNode.Nodes)
                    {
                        if (!_isOperatorMode)
                        {
                            node.Text = node.Text.Replace(" (受限)", "");
                            node.ForeColor = treeView1.ForeColor;
                        }
                        else if (node.Text != "登录")
                        {
                            if (!node.Text.EndsWith(" (受限)")) node.Text += " (受限)";
                            node.ForeColor = Color.Gray;
                        }
                    }
                }
            }
        }

        private void lblViewMode_Click(object sender, EventArgs e) => ToggleViewMode();
        private void timer1_Tick(object sender, EventArgs e) => UpdateStatusBar();

        private void RoundButton_Start_Click(object sender, EventArgs e)
        {
            if (_systemManager == null) { MessageBox.Show("系统未初始化!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (!_systemManager.Start()) MessageBox.Show("系统启动失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void RoundButton_Stop_Click(object sender, EventArgs e)
        {
            if (_systemManager == null) { MessageBox.Show("系统未初始化!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            _systemManager.Stop();
        }

        private void RoundButton_Reset_Click(object sender, EventArgs e)
        {
            if (_systemManager == null) { MessageBox.Show("系统未初始化!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (!_systemManager.Reset()) MessageBox.Show("系统复位失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void RoundButton_Pause_Click(object sender, EventArgs e) => MessageBox.Show("暂停功能暂不支持，请使用停止按钮", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void RoundButton_Login_Click(object sender, EventArgs e) => ShowLoginForm();
        private void RoundButton_Help_Click(object sender, EventArgs e) { /* 帮助按钮已移除 */ }

        #region 显示子窗体

        private void ShowCachedForm<T>(string formKey, Func<T> creator) where T : Form
        {
            Form form;
            lock (_formCacheLock)
            {
                if (_formCache.TryGetValue(formKey, out form) && form.IsDisposed)
                    form = null;
                if (form == null)
                {
                    form = creator();
                    _formCache[formKey] = form;
                }
            }

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.FormClosing += (s, e) =>
            {
                lock (_formCacheLock) { _formCache.Remove(formKey); }
                form.Dispose();
            };

            panel_Main.Controls.Clear();
            panel_Main.Controls.Add(form);
            form.Show();
        }

        private void ShowForm<T>() where T : Form
        {
            ShowCachedForm(typeof(T).Name, () => _formFactory.GetForm<T>());
        }

        private void ShowDataForm() => ShowForm<ReportCenterForm>();
        private void ShowRecipeForm() => ShowForm<RecipeForm>();
        private void ShowSettingForm() => ShowForm<SettingForm>();
        private void ShowStationForm() => ShowForm<StationForm>();
        private void ShowEquipmentForm() => ShowForm<EquipmentControlForm>();
        private void ShowBlockCutForm() => ShowForm<BlockCutMainForm>();
        private void ShowBlockCutCameraForm() => ShowForm<BlockCutCameraForm>();
        private void ShowBlockCutStatsForm() => ShowForm<BlockCutStatsForm>();
        private void ShowBlockCutManualForm() => ShowForm<BlockCutManualForm>();
        private void ShowBlockCutDebugForm() => ShowForm<BlockCutDebugForm>();
        private void ShowBlockCutLogForm() => ShowForm<BlockCutLogForm>();
        private void ShowBlockCutIOSignalForm() => ShowForm<BlockCutIOSignalForm>();
        private void ShowBlockCutMesConfigForm() => ShowForm<BlockCutMesConfigForm>();
        private void ShowBlockCutAxisSetupForm() => ShowForm<BlockCutAxisSetupForm>();
        private void ShowBlockCutVisionPositionForm() => ShowForm<BlockCutVisionPositionForm>();
        private void ShowBlockCutMeasureTestForm() => ShowForm<BlockCutMeasureTestForm>();
        private void ShowBlockCutProductionForm() => ShowForm<BlockCutProductionForm>();
        private void ShowFitLineToolForm() => ShowForm<FitLineToolForm>();
        private void ShowMotionSetForm() => ShowForm<MotionSetForm>();
        private void ShowOeeForm() => ShowForm<Form_Oee>();
        private void ShowConfigForm() => ShowForm<ConfigForm>();
        private void ShowConfigWizardForm() => ShowForm<ConfigWizardForm>();
        private void ShowRoleManagerForm() => ShowForm<RoleManagerForm>();
        private void ShowPluginManagerForm() => ShowForm<PluginManagerForm>();
        private void ShowOperationLogForm() => ShowForm<OperationLogForm>();

        private void ShowUphForm()
        {
            try
            {
                var uph = _uphManager.CalculateUph("DefaultLine");
                var target = _uphManager.CalculateTargetUph("DefaultLine");
                var rate = target > 0 ? uph / target : 0;
                MessageBox.Show($"当前UPH: {uph:F1}\n目标UPH: {target:F1}\n达成率: {rate:P1}", "UPH 统计", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("UPH统计获取失败", ex);
                MessageBox.Show($"UPH统计功能不可用:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_systemManager.IsRunning)
            {
                var result = MessageBox.Show("系统正在运行中，确定要退出吗?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) { e.Cancel = true; return; }
            }

            lock (_formCacheLock)
            {
                foreach (var form in _formCache.Values)
                    if (!form.IsDisposed) form.Dispose();
                _formCache.Clear();
            }
            _systemManager.Dispose();
        }

        #region 菜单事件处理

        private void 监控界面ToolStripMenuItem_Click(object sender, EventArgs e) => MessageBox.Show("监控界面已移除，请使用仪表盘或其他功能", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void 数据报表ToolStripMenuItem_Click(object sender, EventArgs e) => ShowDataForm();
        private void 配方管理ToolStripMenuItem_Click(object sender, EventArgs e) => ShowRecipeForm();
        private void 设备控制ToolStripMenuItem_Click(object sender, EventArgs e) => ShowEquipmentForm();
        private void 工位管理ToolStripMenuItem_Click(object sender, EventArgs e) => ShowStationForm();
        private void 系统设置ToolStripMenuItem_Click(object sender, EventArgs e) => ShowSettingForm();
        private void 用户管理ToolStripMenuItem_Click(object sender, EventArgs e) => ShowRoleManagerForm();
        private void 系统日志ToolStripMenuItem_Click(object sender, EventArgs e) => ShowOperationLogForm();

        private void 关于系统ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            MessageBox.Show($"OmniFrame 自动化控制系统\n版本: {version}\n运行时间: {_systemManager?.RunningTime}\n状态: {GetStateText(_systemManager?.State ?? SystemState.Idle)}\n\n© 2024-2026 OmniFrame Team", "关于系统", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}
