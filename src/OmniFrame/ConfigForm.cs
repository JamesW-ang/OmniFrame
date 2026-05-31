using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OmniFrame.Core;
using OmniFrame.Core.BlockCut;
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// 配置窗体类 - 系统配置管理
        /// </summary>
    public partial class ConfigForm : Form
    {
        private readonly IConfigManager _configManager;
        private MotionConfig _motionConfig;
        private PlcConfig _plcConfig;
        private IoConfig _ioConfig;
        private SystemConfig _systemConfig;
        private BlockCutConfig _blockCutConfig;

        // BlockCut tab controls
        private TabPage _tabBlockCut;
        private Dictionary<string, Control> _bcControls = new Dictionary<string, Control>();

        /// <summary>
        /// 构造函数（DI容器使用）
        /// </summary>
        public ConfigForm(IConfigManager configManager)
        {
            _configManager = configManager;
            InitializeComponent();
            InitUI();
            LoadConfig();
            _configManager.ConfigChanged += ConfigManager_ConfigChanged;
            _configManager.StartWatching();
            this.FormClosing += ConfigForm_FormClosing;

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        /// <summary>
        /// 配置变更事件处理
        /// </summary>
        private void ConfigManager_ConfigChanged(object sender, ConfigChangedEventArgs e)
        {
            LoadConfig();
        }

        /// <summary>
        /// 窗体关闭事件 - 取消订阅，停止监视
        /// </summary>
        private void ConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _configManager.ConfigChanged -= ConfigManager_ConfigChanged;
            _configManager.StopWatching();
        }

        /// <summary>
        /// 初始化UI - 设置列、默认值等
        /// </summary>
        private void InitUI()
        {
            cmbMotionBrand.Items.AddRange(new string[] { "PCIeM60", "DMC3400", "GTS", "InoEcat" });
            cmbPlcBrand.Items.AddRange(new string[] { "ModbusTCP", "S7-1200", "S7-1500", "三菱" });

            dgvAxisParams.AutoGenerateColumns = false;
            dgvAxisParams.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AxisNo", HeaderText = "轴号" });
            dgvAxisParams.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Speed", HeaderText = "速度(mm/s)" });
            dgvAxisParams.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Acceleration", HeaderText = "加速度(mm/s²)" });
            dgvAxisParams.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PositiveLimit", HeaderText = "正限位" });
            dgvAxisParams.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NegativeLimit", HeaderText = "负限位" });

            dgvRegisterMaps.AutoGenerateColumns = false;
            dgvRegisterMaps.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "名称" });
            dgvRegisterMaps.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Address", HeaderText = "地址" });
            dgvRegisterMaps.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Type", HeaderText = "类型" });

            dgvInputs.AutoGenerateColumns = false;
            dgvInputs.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "名称" });
            dgvInputs.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Port", HeaderText = "端口" });
            dgvInputs.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Pin", HeaderText = "引脚" });

            dgvOutputs.AutoGenerateColumns = false;
            dgvOutputs.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "名称" });
            dgvOutputs.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Port", HeaderText = "端口" });
            dgvOutputs.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Pin", HeaderText = "引脚" });

            dgvTriggerLogics.AutoGenerateColumns = false;
            dgvTriggerLogics.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "名称" });
            dgvTriggerLogics.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InputCondition", HeaderText = "输入条件" });
            dgvTriggerLogics.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OutputAction", HeaderText = "输出动作" });

            dgvAlarmRules.AutoGenerateColumns = false;
            dgvAlarmRules.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "名称" });
            dgvAlarmRules.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Condition", HeaderText = "条件" });
            dgvAlarmRules.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Level", HeaderText = "级别" });

            BuildBlockCutTab();

            RefreshBackupFiles();
        }

        private void LoadConfig()
        {
            // 加载运动卡配置
            _motionConfig = _configManager.GetConfig<MotionConfig>("MotionConfig.xml", new MotionConfig
            {
                Brand = "PCIeM60",
                IP = "192.168.1.100",
                AxisCount = 4,
                AxisParams = new List<AxisParam>
                {
                    new AxisParam { AxisNo = 0, Speed = 100, Acceleration = 1000, PositiveLimit = 1000, NegativeLimit = 0 },
                    new AxisParam { AxisNo = 1, Speed = 100, Acceleration = 1000, PositiveLimit = 1000, NegativeLimit = 0 },
                    new AxisParam { AxisNo = 2, Speed = 100, Acceleration = 1000, PositiveLimit = 500, NegativeLimit = 0 },
                    new AxisParam { AxisNo = 3, Speed = 100, Acceleration = 1000, PositiveLimit = 360, NegativeLimit = 0 }
                }
            });

            // 加载PLC配置
            _plcConfig = _configManager.GetConfig<PlcConfig>("PlcConfig.xml", new PlcConfig
            {
                Brand = "ModbusTCP",
                IP = "192.168.1.101",
                Port = 502,
                RegisterMaps = new List<RegisterMap>
                {
                    new RegisterMap { Name = "StartButton", Address = "M0.0", Type = "BOOL" },
                    new RegisterMap { Name = "StopButton", Address = "M0.1", Type = "BOOL" },
                    new RegisterMap { Name = "ResetButton", Address = "M0.2", Type = "BOOL" },
                    new RegisterMap { Name = "RunningIndicator", Address = "Q0.0", Type = "BOOL" },
                    new RegisterMap { Name = "ErrorIndicator", Address = "Q0.1", Type = "BOOL" },
                    new RegisterMap { Name = "ProductCount", Address = "DB1.DBD0", Type = "DWORD" }
                }
            });

            // 加载IO配置
            _ioConfig = _configManager.GetConfig<IoConfig>("IoConfig.xml", new IoConfig
            {
                Inputs = new List<IoPoint>
                {
                    new IoPoint { Name = "Sensor1", Port = 0, Pin = 0 },
                    new IoPoint { Name = "Sensor2", Port = 0, Pin = 1 },
                    new IoPoint { Name = "LimitSwitch1", Port = 0, Pin = 2 },
                    new IoPoint { Name = "LimitSwitch2", Port = 0, Pin = 3 }
                },
                Outputs = new List<IoPoint>
                {
                    new IoPoint { Name = "Light1", Port = 1, Pin = 0 },
                    new IoPoint { Name = "Light2", Port = 1, Pin = 1 },
                    new IoPoint { Name = "Cylinder1", Port = 1, Pin = 2 },
                    new IoPoint { Name = "Cylinder2", Port = 1, Pin = 3 }
                },
                TriggerLogics = new List<TriggerLogic>
                {
                    new TriggerLogic { Name = "自动启动", InputCondition = "Sensor1=1", OutputAction = "Cylinder1=1" },
                    new TriggerLogic { Name = "自动停止", InputCondition = "Sensor2=1", OutputAction = "Cylinder1=0" }
                }
            });

            // 加载系统配置
            _systemConfig = _configManager.GetConfig<SystemConfig>("SystemConfig.xml", new SystemConfig
            {
                LogPath = "Logs/",
                WatchdogInterval = 5000,
                AlarmRules = new List<AlarmRule>
                {
                    new AlarmRule { Name = "温度过高", Condition = "Temperature>80", Level = "Error" },
                    new AlarmRule { Name = "压力异常", Condition = "Pressure<10", Level = "Warning" }
                }
            });

            // 加载BlockCut配置
            _blockCutConfig = _configManager.GetConfig<BlockCutConfig>("BlockCut.xml", new BlockCutConfig());

            // 显示配置
            DisplayConfig();
        }

        private void DisplayConfig()
        {
            // 显示运动卡配置
            cmbMotionBrand.Text = _motionConfig.Brand;
            txtMotionIP.Text = _motionConfig.IP;
            nudAxisCount.Value = _motionConfig.AxisCount;
            dgvAxisParams.DataSource = _motionConfig.AxisParams;

            // 显示PLC配置
            cmbPlcBrand.Text = _plcConfig.Brand;
            txtPlcIP.Text = _plcConfig.IP;
            nudPlcPort.Value = _plcConfig.Port;
            dgvRegisterMaps.DataSource = _plcConfig.RegisterMaps;

            // 显示IO配置
            dgvInputs.DataSource = _ioConfig.Inputs;
            dgvOutputs.DataSource = _ioConfig.Outputs;
            dgvTriggerLogics.DataSource = _ioConfig.TriggerLogics;

            // 显示系统配置
            txtLogPath.Text = _systemConfig.LogPath;
            nudWatchdogInterval.Value = _systemConfig.WatchdogInterval;
            dgvAlarmRules.DataSource = _systemConfig.AlarmRules;

            // 显示BlockCut配置
            DisplayBlockCutConfig();
        }

        private void RefreshBackupFiles()
        {
            lstBackupFiles.Items.Clear();
            List<string> backupFiles = _configManager.GetBackupFiles();
            foreach (string file in backupFiles)
            {
                lstBackupFiles.Items.Add(file);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 保存运动卡配置
            _motionConfig.Brand = cmbMotionBrand.Text;
            _motionConfig.IP = txtMotionIP.Text;
            _motionConfig.AxisCount = (int)nudAxisCount.Value;

            // 保存PLC配置
            _plcConfig.Brand = cmbPlcBrand.Text;
            _plcConfig.IP = txtPlcIP.Text;
            _plcConfig.Port = (int)nudPlcPort.Value;

            // 保存IO配置
            // 数据源已绑定，自动更新

            // 保存系统配置
            _systemConfig.LogPath = txtLogPath.Text;
            _systemConfig.WatchdogInterval = (int)nudWatchdogInterval.Value;

            // 验证配置
            List<string> errors;
            bool isValid = true;

            // 验证运动卡配置
            if (!_configManager.ValidateConfig(_motionConfig, out errors))
            {
                isValid = false;
                MessageBox.Show(string.Join("\n", errors), "配置验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 验证PLC配置
            if (isValid && !_configManager.ValidateConfig(_plcConfig, out errors))
            {
                isValid = false;
                MessageBox.Show(string.Join("\n", errors), "配置验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 验证IO配置
            if (isValid && !_configManager.ValidateConfig(_ioConfig, out errors))
            {
                isValid = false;
                MessageBox.Show(string.Join("\n", errors), "配置验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 验证系统配置
            if (isValid && !_configManager.ValidateConfig(_systemConfig, out errors))
            {
                isValid = false;
                MessageBox.Show(string.Join("\n", errors), "配置验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (isValid)
            {
                // 保存配置
                _configManager.SaveConfig("MotionConfig.xml", _motionConfig);
                _configManager.SaveConfig("PlcConfig.xml", _plcConfig);
                _configManager.SaveConfig("IoConfig.xml", _ioConfig);
                _configManager.SaveConfig("SystemConfig.xml", _systemConfig);
                SaveBlockCutConfig();
                _configManager.SaveConfig("BlockCut.xml", _blockCutConfig);

                // 刷新备份文件列表
                RefreshBackupFiles();

                MessageBox.Show("配置保存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "配置文件 (*.xml;*.json)|*.xml;*.json|XML文件 (*.xml)|*.xml|JSON文件 (*.json)|*.json",
                Title = "导入配置"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(filePath);

                if (fileName.Contains("Motion"))
                {
                    MotionConfig config = ConfigImportExport.Import<MotionConfig>(filePath);
                    if (config != null)
                    {
                        _motionConfig = config;
                        DisplayConfig();
                        MessageBox.Show("运动卡配置导入成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fileName.Contains("Plc"))
                {
                    PlcConfig config = ConfigImportExport.Import<PlcConfig>(filePath);
                    if (config != null)
                    {
                        _plcConfig = config;
                        DisplayConfig();
                        MessageBox.Show("PLC配置导入成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fileName.Contains("Io"))
                {
                    IoConfig config = ConfigImportExport.Import<IoConfig>(filePath);
                    if (config != null)
                    {
                        _ioConfig = config;
                        DisplayConfig();
                        MessageBox.Show("IO配置导入成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fileName.Contains("System"))
                {
                    SystemConfig config = ConfigImportExport.Import<SystemConfig>(filePath);
                    if (config != null)
                    {
                        _systemConfig = config;
                        DisplayConfig();
                        MessageBox.Show("系统配置导入成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (fileName.Contains("BlockCut"))
                {
                    BlockCutConfig config = ConfigImportExport.Import<BlockCutConfig>(filePath);
                    if (config != null)
                    {
                        _blockCutConfig = config;
                        DisplayBlockCutConfig();
                        MessageBox.Show("BlockCut配置导入成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "XML文件 (*.xml)|*.xml|JSON文件 (*.json)|*.json",
                Title = "导出配置"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                string extension = Path.GetExtension(filePath).ToLower();

                switch (tabControl1.SelectedTab.Name)
                {
                    case "tabMotion":
                        if (ConfigImportExport.Export(_motionConfig, filePath))
                        {
                            MessageBox.Show("运动卡配置导出成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;
                    case "tabPlc":
                        if (ConfigImportExport.Export(_plcConfig, filePath))
                        {
                            MessageBox.Show("PLC配置导出成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;
                    case "tabIo":
                        if (ConfigImportExport.Export(_ioConfig, filePath))
                        {
                            MessageBox.Show("IO配置导出成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;
                    case "tabSystem":
                        if (ConfigImportExport.Export(_systemConfig, filePath))
                        {
                            MessageBox.Show("系统配置导出成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;
                    case "tabBlockCut":
                        SaveBlockCutConfig();
                        if (ConfigImportExport.Export(_blockCutConfig, filePath))
                        {
                            MessageBox.Show("BlockCut配置导出成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;
                }
            }
        }

        private void btnRollback_Click(object sender, EventArgs e)
        {
            if (lstBackupFiles.SelectedItem != null)
            {
                string backupFileName = lstBackupFiles.SelectedItem.ToString();
                if (_configManager.RollbackConfig(backupFileName))
                {
                    LoadConfig();
                    RefreshBackupFiles();
                    MessageBox.Show("配置回滚成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("配置回滚失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("请选择要回滚的备份文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnAddAxisParam_Click(object sender, EventArgs e)
        {
            _motionConfig.AxisParams.Add(new AxisParam { AxisNo = _motionConfig.AxisParams.Count, Speed = 100, Acceleration = 1000, PositiveLimit = 1000, NegativeLimit = 0 });
            dgvAxisParams.DataSource = null;
            dgvAxisParams.DataSource = _motionConfig.AxisParams;
        }

        private void btnRemoveAxisParam_Click(object sender, EventArgs e)
        {
            if (dgvAxisParams.SelectedRows.Count > 0)
            {
                int index = dgvAxisParams.SelectedRows[0].Index;
                _motionConfig.AxisParams.RemoveAt(index);
                dgvAxisParams.DataSource = null;
                dgvAxisParams.DataSource = _motionConfig.AxisParams;
            }
        }

        private void btnAddRegisterMap_Click(object sender, EventArgs e)
        {
            _plcConfig.RegisterMaps.Add(new RegisterMap { Name = "NewRegister", Address = "", Type = "BOOL" });
            dgvRegisterMaps.DataSource = null;
            dgvRegisterMaps.DataSource = _plcConfig.RegisterMaps;
        }

        private void btnRemoveRegisterMap_Click(object sender, EventArgs e)
        {
            if (dgvRegisterMaps.SelectedRows.Count > 0)
            {
                int index = dgvRegisterMaps.SelectedRows[0].Index;
                _plcConfig.RegisterMaps.RemoveAt(index);
                dgvRegisterMaps.DataSource = null;
                dgvRegisterMaps.DataSource = _plcConfig.RegisterMaps;
            }
        }

        private void btnAddInput_Click(object sender, EventArgs e)
        {
            _ioConfig.Inputs.Add(new IoPoint { Name = "NewInput", Port = 0, Pin = 0 });
            dgvInputs.DataSource = null;
            dgvInputs.DataSource = _ioConfig.Inputs;
        }

        private void btnRemoveInput_Click(object sender, EventArgs e)
        {
            if (dgvInputs.SelectedRows.Count > 0)
            {
                int index = dgvInputs.SelectedRows[0].Index;
                _ioConfig.Inputs.RemoveAt(index);
                dgvInputs.DataSource = null;
                dgvInputs.DataSource = _ioConfig.Inputs;
            }
        }

        private void btnAddOutput_Click(object sender, EventArgs e)
        {
            _ioConfig.Outputs.Add(new IoPoint { Name = "NewOutput", Port = 1, Pin = 0 });
            dgvOutputs.DataSource = null;
            dgvOutputs.DataSource = _ioConfig.Outputs;
        }

        private void btnRemoveOutput_Click(object sender, EventArgs e)
        {
            if (dgvOutputs.SelectedRows.Count > 0)
            {
                int index = dgvOutputs.SelectedRows[0].Index;
                _ioConfig.Outputs.RemoveAt(index);
                dgvOutputs.DataSource = null;
                dgvOutputs.DataSource = _ioConfig.Outputs;
            }
        }

        private void btnAddTriggerLogic_Click(object sender, EventArgs e)
        {
            _ioConfig.TriggerLogics.Add(new TriggerLogic { Name = "NewLogic", InputCondition = "", OutputAction = "" });
            dgvTriggerLogics.DataSource = null;
            dgvTriggerLogics.DataSource = _ioConfig.TriggerLogics;
        }

        private void btnRemoveTriggerLogic_Click(object sender, EventArgs e)
        {
            if (dgvTriggerLogics.SelectedRows.Count > 0)
            {
                int index = dgvTriggerLogics.SelectedRows[0].Index;
                _ioConfig.TriggerLogics.RemoveAt(index);
                dgvTriggerLogics.DataSource = null;
                dgvTriggerLogics.DataSource = _ioConfig.TriggerLogics;
            }
        }

        private void btnAddAlarmRule_Click(object sender, EventArgs e)
        {
            _systemConfig.AlarmRules.Add(new AlarmRule { Name = "NewRule", Condition = "", Level = "Warning" });
            dgvAlarmRules.DataSource = null;
            dgvAlarmRules.DataSource = _systemConfig.AlarmRules;
        }

        private void btnRemoveAlarmRule_Click(object sender, EventArgs e)
        {
            if (dgvAlarmRules.SelectedRows.Count > 0)
            {
                int index = dgvAlarmRules.SelectedRows[0].Index;
                _systemConfig.AlarmRules.RemoveAt(index);
                dgvAlarmRules.DataSource = null;
                dgvAlarmRules.DataSource = _systemConfig.AlarmRules;
            }
        }

        // ────────────────────────────────────────
        //  BlockCut 配置标签页
        // ────────────────────────────────────────

        private void BuildBlockCutTab()
        {
            _tabBlockCut = new TabPage("BlockCut");
            _tabBlockCut.Name = "tabBlockCut";

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };
            int y = 4;
            var titleFont = new Font("Microsoft YaHei", 8.5F);

            // ── 上料工站 (Load) ──
            var grpLoad = CreateGroup("上料工站 (Load) — CasselZ / TrayY 位置参数", 0, ref y);
            AddDoubleInput(grpLoad, "CasselZ 起始位置 (CasselZFirstPos)", "CasselZFirstPos", 0, ref y);
            AddDoubleInput(grpLoad, "CasselZ 间距 (CasselZSpace)", "CasselZSpace", 1, ref y);
            AddDoubleInput(grpLoad, "TrayY 取料盘 (TrayYGetTray)", "TrayYGetTray", 2, ref y);
            AddDoubleInput(grpLoad, "TrayY 扫码 (TrayYCode)", "TrayYCode", 3, ref y);
            AddDoubleInput(grpLoad, "TrayY 取片 (TrayYGetSlice)", "TrayYGetSlice", 4, ref y);
            AddDoubleInput(grpLoad, "Tray 行间距 (TrayRowSpace)", "TrayRowSpace", 5, ref y);
            scroll.Controls.Add(grpLoad);
            y += grpLoad.Height + 8;

            // ── 二次上料 (Load2) ──
            var grpLoad2 = CreateGroup("二次上料工站 (Load2) — TrayX 位置参数", 0, ref y);
            AddDoubleInput(grpLoad2, "TrayX 取片 (TrayXGetSlice)", "TrayXGetSlice", 0, ref y);
            AddDoubleInput(grpLoad2, "TrayX 放片 (TrayXSetSlice)", "TrayXSetSlice", 1, ref y);
            AddDoubleInput(grpLoad2, "Tray 列间距 (TrayColSpace)", "TrayColSpace", 2, ref y);
            AddDoubleInput(grpLoad2, "底板列间距 (BottomColSpace)", "BottomColSpace", 3, ref y);
            scroll.Controls.Add(grpLoad2);
            y += grpLoad2.Height + 8;

            // ── 料塔 + 行列参数 ──
            var grpCassel = CreateGroup("料塔工站 (CasselZ) & 行列参数", 0, ref y);
            AddIntInput(grpCassel, "料塔数量 (CasselCount)", "CasselCount", 0, ref y);
            AddIntInput(grpCassel, "Tray 行数 (TrayRows)", "TrayRows", 1, ref y);
            AddIntInput(grpCassel, "Tray 列数 (TrayCols)", "TrayCols", 2, ref y);
            AddIntInput(grpCassel, "底板行数 (BottomRows)", "BottomRows", 3, ref y);
            AddIntInput(grpCassel, "底板列数 (BottomCols)", "BottomCols", 4, ref y);
            scroll.Controls.Add(grpCassel);
            y += grpCassel.Height + 8;

            // ── 矫正工站 (Adjust) ──
            var grpAdjust = CreateGroup("矫正工站 (Adjust) — UVW / 相机 / Slice 位置", 0, ref y);
            AddDoubleInput(grpAdjust, "UVW 角度 (UVWAngle)", "UVWAngle", 0, ref y);
            AddDoubleInput(grpAdjust, "底板Y 放片 (BottomYSetSlice)", "BottomYSetSlice", 1, ref y);
            AddDoubleInput(grpAdjust, "底板Y 矫正 (BottomYAdjustBottom)", "BottomYAdjustBottom", 2, ref y);
            AddDoubleInput(grpAdjust, "底板行间距 (BottomRowSpace)", "BottomRowSpace", 3, ref y);
            AddDoubleInput(grpAdjust, "底板列间距 (BottomColSpaceRecipe)", "BottomColSpaceRecipe", 4, ref y);
            AddDoubleInput(grpAdjust, "物距 (ObjectUnit)", "ObjectUnit", 5, ref y);
            AddDoubleInput(grpAdjust, "相机底右X (CameraBottomXRight)", "CameraBottomXRight", 0, ref y);
            AddDoubleInput(grpAdjust, "相机底左X (CameraBottomXLeft)", "CameraBottomXLeft", 1, ref y);
            AddDoubleInput(grpAdjust, "相机矫正片1X (CameraXAdjustSlice1)", "CameraXAdjustSlice1", 2, ref y);
            AddDoubleInput(grpAdjust, "相机矫正片2X (CameraXAdjustSlice2)", "CameraXAdjustSlice2", 3, ref y);
            AddDoubleInput(grpAdjust, "相机底板位1X (CameraXBottPos1)", "CameraXBottPos1", 4, ref y);
            AddDoubleInput(grpAdjust, "相机Z安全 (CameraZSafe)", "CameraZSafe", 5, ref y);
            AddDoubleInput(grpAdjust, "相机Z矫正底 (CameraZAdjustBottom)", "CameraZAdjustBottom", 0, ref y);
            AddDoubleInput(grpAdjust, "相机Z矫正片 (CameraZAdjustSlice)", "CameraZAdjustSlice", 1, ref y);
            AddDoubleInput(grpAdjust, "片Y1 (SliceY1Adjust)", "SliceY1Adjust", 2, ref y);
            AddDoubleInput(grpAdjust, "片Y2 (SliceY2Adjust)", "SliceY2Adjust", 3, ref y);
            AddDoubleInput(grpAdjust, "片X (SliceXAdjust)", "SliceXAdjust", 4, ref y);
            AddDoubleInput(grpAdjust, "PosMax", "PosMax", 5, ref y);
            scroll.Controls.Add(grpAdjust);
            y += grpAdjust.Height + 8;

            // ── 底板取放 (BottomGet) ──
            var grpBottom = CreateGroup("底板取放工站 (BottomGet) — X 位置", 0, ref y);
            AddDoubleInput(grpBottom, "取空底板X (BottomXGetEmptyBottom)", "BottomXGetEmptyBottom", 0, ref y);
            AddDoubleInput(grpBottom, "放空底板X (BottomXSetEmptyBottom)", "BottomXSetEmptyBottom", 1, ref y);
            AddDoubleInput(grpBottom, "放空底板X2 (BottomXSetEmptyBottom2)", "BottomXSetEmptyBottom2", 2, ref y);
            AddDoubleInput(grpBottom, "取满底板X (BottomXGetFullBottom)", "BottomXGetFullBottom", 3, ref y);
            AddDoubleInput(grpBottom, "放满底板X (BottomXSetFullBottom)", "BottomXSetFullBottom", 4, ref y);
            scroll.Controls.Add(grpBottom);
            y += grpBottom.Height + 8;

            // ── 多产品槽位 ──
            var grpSlots = CreateGroup("多产品槽位 (6 Slots) — 每槽位的 BottomY / UVW角度", 0, ref y);
            for (int s = 1; s <= 6; s++)
            {
                AddDoubleInput(grpSlots, $"槽位{s} BottomY放片 (BottomYSetSlice{s})", $"BottomYSetSlice{s}", s - 1, ref y);
            }
            for (int s = 1; s <= 6; s++)
            {
                AddDoubleInput(grpSlots, $"槽位{s} BottomY矫正底 (BottomYAdjustBottom{s})", $"BottomYAdjustBottom{s}", s - 1, ref y);
            }
            for (int s = 1; s <= 6; s++)
            {
                AddDoubleInput(grpSlots, $"槽位{s} UVW角度 (UVWAngle{s})", $"UVWAngle{s}", s - 1, ref y);
            }
            scroll.Controls.Add(grpSlots);
            y += grpSlots.Height + 8;

            // ── 9点标定偏移 ──
            var grpCalib = CreateGroup("9点标定偏移 — 左/右相机 Y 补偿", 0, ref y);
            for (int i = 1; i <= 9; i++)
            {
                AddDoubleInput(grpCalib, $"左侧偏移Y_{i} (LeftOffY_{i})", $"LeftOffY_{i}", i - 1, ref y);
            }
            for (int i = 1; i <= 9; i++)
            {
                AddDoubleInput(grpCalib, $"右侧偏移Y_{i} (RightOffY_{i})", $"RightOffY_{i}", i - 1, ref y);
            }
            scroll.Controls.Add(grpCalib);
            y += grpCalib.Height + 8;

            // ── 视觉参数 ──
            var grpVision = CreateGroup("视觉参数 — 灰度阈值 & 曝光", 0, ref y);
            AddIntInput(grpVision, "灰度低1 (GrayLow1)", "GrayLow1", 0, ref y);
            AddIntInput(grpVision, "灰度低2 (GrayLow2)", "GrayLow2", 1, ref y);
            AddIntInput(grpVision, "灰度高1 (GrayHigh1)", "GrayHigh1", 2, ref y);
            AddIntInput(grpVision, "灰度高2 (GrayHigh2)", "GrayHigh2", 3, ref y);
            AddIntInput(grpVision, "灰度间距 (GraySpace)", "GraySpace", 4, ref y);
            AddIntInput(grpVision, "曝光间距 (ExposureSpace)", "ExposureSpace", 5, ref y);
            scroll.Controls.Add(grpVision);
            y += grpVision.Height + 8;

            // ── 点胶/UV ──
            var grpDisp = CreateGroup("点胶 & UV 参数", 0, ref y);
            AddIntInput(grpDisp, "点胶时间 ms (DisTime)", "DisTime", 0, ref y);
            AddIntInput(grpDisp, "UV时间 ms (UVTime)", "UVTime", 1, ref y);
            AddDoubleInput(grpDisp, "点胶Z距离 (DisZDis)", "DisZDis", 2, ref y);
            AddDoubleInput(grpDisp, "相机X等待 (CameraXWait)", "CameraXWait", 3, ref y);
            AddDoubleInput(grpDisp, "相机X UV (CameraXUV)", "CameraXUV", 4, ref y);
            AddDoubleInput(grpDisp, "相机Z UV (CameraZUV)", "CameraZUV", 5, ref y);
            scroll.Controls.Add(grpDisp);
            y += grpDisp.Height + 8;

            // ── 安全位置 ──
            var grpSafe = CreateGroup("安全位置", 0, ref y);
            AddDoubleInput(grpSafe, "片Y等待 (SliceYWait)", "SliceYWait", 0, ref y);
            AddDoubleInput(grpSafe, "片X等待 (SliceXWait)", "SliceXWait", 1, ref y);
            AddDoubleInput(grpSafe, "点胶Z安全 (DisZSafe)", "DisZSafe", 2, ref y);
            AddDoubleInput(grpSafe, "点胶X安全 (DisXSafe)", "DisXSafe", 3, ref y);
            AddDoubleInput(grpSafe, "点胶Y安全 (DisYSafe)", "DisYSafe", 4, ref y);
            scroll.Controls.Add(grpSafe);
            y += grpSafe.Height + 8;

            // ── 测高参数 ──
            var grpHeight = CreateGroup("测高参数", 0, ref y);
            AddDoubleInput(grpHeight, "相机X高度1 (CameraXHeight1)", "CameraXHeight1", 0, ref y);
            AddDoubleInput(grpHeight, "相机X高度2 (CameraXHeight2)", "CameraXHeight2", 1, ref y);
            AddDoubleInput(grpHeight, "相机X高度3 (CameraXHeight3)", "CameraXHeight3", 2, ref y);
            AddDoubleInput(grpHeight, "相机Z高度 (CameraZHeight)", "CameraZHeight", 3, ref y);
            AddDoubleInput(grpHeight, "高度上限 (HeightHigh)", "HeightHigh", 4, ref y);
            AddDoubleInput(grpHeight, "高度下限 (HeightLow)", "HeightLow", 5, ref y);
            scroll.Controls.Add(grpHeight);
            y += grpHeight.Height + 8;

            // ── 扫描枪 ──
            var grpScanner = CreateGroup("扫描枪设置", 0, ref y);
            AddStringInput(grpScanner, "底板扫码枪 Host", "BottomScannerHost", 0, ref y);
            AddIntInput(grpScanner, "底板扫码枪 Port", "BottomScannerPort", 1, ref y);
            AddStringInput(grpScanner, "切片扫码枪 Host", "SliceScannerHost", 2, ref y);
            AddIntInput(grpScanner, "切片扫码枪 Port", "SliceScannerPort", 3, ref y);
            scroll.Controls.Add(grpScanner);
            y += grpScanner.Height + 8;

            // ── 测试模式 ──
            var grpTest = CreateGroup("测试模式开关", 0, ref y);
            AddBoolInput(grpTest, "模拟模式 (IsSimulation)", "IsSimulation", 0, ref y);
            AddBoolInput(grpTest, "空跑测试 (IsEmptyTest)", "IsEmptyTest", 1, ref y);
            AddBoolInput(grpTest, "关闭切片扫码 (IsCloseSliceCode)", "IsCloseSliceCode", 2, ref y);
            AddBoolInput(grpTest, "关闭夹具扫码 (IsCloseJigCode)", "IsCloseJigCode", 3, ref y);
            scroll.Controls.Add(grpTest);
            y += grpTest.Height + 8;

            // ── MES ──
            var grpMes = CreateGroup("MES 通讯密钥", 0, ref y);
            AddStringInput(grpMes, "AES 密钥 (32字符)", "AesKey", 0, ref y);
            scroll.Controls.Add(grpMes);

            _tabBlockCut.Controls.Add(scroll);
            tabControl1.TabPages.Add(_tabBlockCut);
        }

        private GroupBox CreateGroup(string title, int x, ref int y)
        {
            var gb = new GroupBox
            {
                Text = title,
                Location = new Point(4, y),
                Size = new Size(790, 60),
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold)
            };
            var panel = new FlowLayoutPanel
            {
                Location = new Point(6, 20),
                Size = new Size(776, 36),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            gb.Controls.Add(panel);
            return gb;
        }

        private TextBox AddDoubleInput(GroupBox group, string label, string key, int col, ref int rowY)
        {
            var panel = group.Controls[0];
            var lbl = new Label { Text = label, Size = new Size(210, 22), TextAlign = ContentAlignment.MiddleRight, Font = new Font("Microsoft YaHei", 8F) };
            var tb = new TextBox { Size = new Size(94, 22), Tag = "double", Font = new Font("Consolas", 8.5F) };
            panel.Controls.Add(lbl);
            panel.Controls.Add(tb);
            _bcControls[key] = tb;

            // Auto-size group
            int rows = (panel.Controls.Count / 2 + 2) / 3;
            group.Height = 22 + rows * 26 + 8;

            return tb;
        }

        private TextBox AddIntInput(GroupBox group, string label, string key, int col, ref int rowY)
        {
            var tb = AddDoubleInput(group, label, key, col, ref rowY);
            tb.Tag = "int";
            return tb;
        }

        private TextBox AddStringInput(GroupBox group, string label, string key, int col, ref int rowY)
        {
            var panel = group.Controls[0];
            var lbl = new Label { Text = label, Size = new Size(210, 22), TextAlign = ContentAlignment.MiddleRight, Font = new Font("Microsoft YaHei", 8F) };
            var tb = new TextBox { Size = new Size(160, 22), Tag = "string", Font = new Font("Consolas", 8.5F) };
            panel.Controls.Add(lbl);
            panel.Controls.Add(tb);
            _bcControls[key] = tb;

            int rows = (panel.Controls.Count / 2 + 2) / 3;
            group.Height = 22 + rows * 26 + 8;

            return tb;
        }

        private CheckBox AddBoolInput(GroupBox group, string label, string key, int col, ref int rowY)
        {
            var panel = group.Controls[0];
            var cb = new CheckBox { Text = label, Size = new Size(230, 22), Font = new Font("Microsoft YaHei", 8F) };
            panel.Controls.Add(cb);
            _bcControls[key] = cb;

            int rows = (panel.Controls.Count + 2) / 3;
            group.Height = 22 + rows * 26 + 8;

            return cb;
        }

        private void DisplayBlockCutConfig()
        {
            if (_blockCutConfig == null || _bcControls.Count == 0) return;

            foreach (var kv in _bcControls)
            {
                var prop = typeof(BlockCutConfig).GetProperty(kv.Key);
                if (prop == null) continue;
                var val = prop.GetValue(_blockCutConfig);

                if (kv.Value is TextBox tb)
                {
                    tb.Text = val?.ToString() ?? "";
                }
                else if (kv.Value is CheckBox cb)
                {
                    cb.Checked = val is bool b && b;
                }
            }
        }

        private void SaveBlockCutConfig()
        {
            if (_blockCutConfig == null || _bcControls.Count == 0) return;

            foreach (var kv in _bcControls)
            {
                var prop = typeof(BlockCutConfig).GetProperty(kv.Key);
                if (prop == null) continue;

                try
                {
                    if (kv.Value is TextBox tb)
                    {
                        if (prop.PropertyType == typeof(double))
                            prop.SetValue(_blockCutConfig, double.TryParse(tb.Text, out var dv) ? dv : 0.0);
                        else if (prop.PropertyType == typeof(int))
                            prop.SetValue(_blockCutConfig, int.TryParse(tb.Text, out var iv) ? iv : 0);
                        else if (prop.PropertyType == typeof(string))
                            prop.SetValue(_blockCutConfig, tb.Text);
                    }
                    else if (kv.Value is CheckBox cb)
                    {
                        if (prop.PropertyType == typeof(bool))
                            prop.SetValue(_blockCutConfig, cb.Checked);
                    }
                }
                catch { /* skip parse errors */ }
            }
        }
    }
}
