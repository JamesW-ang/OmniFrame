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
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// 设置窗体类 - 系统参数配置与管理
        /// </summary>
    public partial class SettingForm : Form
    {
        // 状态色
        private Color Running = Color.FromArgb(76, 175, 80);    // 运行-绿色
        private Color Idle = Color.FromArgb(33, 150, 243);   // 待机-蓝色
        private Color Alarm = Color.FromArgb(244, 67, 54);     // 报警-红色
        private Color Emergency = Color.FromArgb(183, 28, 28);     // 急停-深红
        private Color Normal = Color.FromArgb(46, 125, 50);     // 正常-深绿
        private Color Warning = Color.FromArgb(255, 152, 0);    // 警告-橙色
        private Color Disabled = Color.FromArgb(158, 158, 158);   // 禁用-灰色

        // 界面色
        private Color BgMain = Color.FromArgb(245, 245, 245);  // 主背景-浅灰
        private Color BgPanel = Color.White;                     // 面板背景-白色
        private Color BgHeader = Color.FromArgb(33, 33, 33);     // 表头背景-深灰
        private Color Border = Color.FromArgb(224, 224, 224);  // 边框-灰
        private Color TextTitle = Color.FromArgb(33, 33, 33);     // 标题文字-深灰
        private Color TextBody = Color.FromArgb(66, 66, 66);     // 正文文字-中灰
        private Color TextLight = Color.White;                     // 浅色文字-白色

        private IConfigManager _configManager;

        public SettingForm(IConfigManager configManager)
        {
            _configManager = configManager;
            InitializeComponent();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
            try
            {
                // 初始化界面
                InitUI();

                // 加载配置
                LoadSettings();

                // 开始监视配置文件变化
                if (_configManager != null)
                {
                    _configManager.StartWatching();
                    _configManager.ConfigChanged += ConfigManager_ConfigChanged;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SettingForm加载失败", ex);
                MessageBox.Show("设置窗体加载失败，请检查系统配置!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 配置变更事件处理
        /// </summary>
        private void ConfigManager_ConfigChanged(object sender, ConfigChangedEventArgs e)
        {
            // 在UI线程中更新界面
            this.Invoke((MethodInvoker)delegate
            {
                LoadSettings();
                MessageBox.Show($"配置文件 {e.ConfigFileName} 已更新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void InitUI()
        {
            // 设置DataGridView样式
            SetDataGridViewStyle(dgvMotionCards);
            SetDataGridViewStyle(dgvAxisParams);
            SetDataGridViewStyle(dgvPlcList);
            SetDataGridViewStyle(dgvRegisterMap);
            SetDataGridViewStyle(dgvInputPoints);
            SetDataGridViewStyle(dgvOutputPoints);
            SetDataGridViewStyle(dgvUserList);

            // 初始化导航按钮状态
            UpdateNavButtonState();

            // 初始化DataGridView列
            InitDataGridViewColumns();
        }

        /// <summary>
        /// 设置DataGridView样式
        /// </summary>
        /// <param name="dgv">DataGridView控件</param>
        private void SetDataGridViewStyle(DataGridView dgv)
        {
            // 交替行颜色
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextBody;

            // 默认行颜色
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = TextBody;

            // 表头样式
            dgv.ColumnHeadersDefaultCellStyle.BackColor = BgHeader;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextLight;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 禁用视觉样式
            dgv.EnableHeadersVisualStyles = false;

            // 自动调整列宽
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        /// <summary>
        /// 初始化DataGridView列
        /// </summary>
        private void InitDataGridViewColumns()
        {
            // 运动卡列表
            InitMotionCardColumns();

            // 轴参数列表
            InitAxisParamColumns();

            // PLC列表
            InitPlcListColumns();

            // 寄存器映射
            InitRegisterMapColumns();

            // 输入点位列表
            InitInputPointsColumns();

            // 输出点位列表
            InitOutputPointsColumns();

            // 用户列表
            InitUserListColumns();
        }

        /// <summary>
        /// 初始化运动卡列表列
        /// </summary>
        private void InitMotionCardColumns()
        {
            dgvMotionCards.Columns.Add("序号", "序号");
            dgvMotionCards.Columns.Add("品牌", "品牌");
            dgvMotionCards.Columns.Add("型号", "型号");
            dgvMotionCards.Columns.Add("IP", "IP地址");
            dgvMotionCards.Columns.Add("启用状态", "启用状态");
            dgvMotionCards.Columns.Add("操作", "操作");
        }

        /// <summary>
        /// 初始化轴参数列表列
        /// </summary>
        private void InitAxisParamColumns()
        {
            dgvAxisParams.Columns.Add("轴号", "轴号");
            dgvAxisParams.Columns.Add("速度", "速度");
            dgvAxisParams.Columns.Add("加速度", "加速度");
            dgvAxisParams.Columns.Add("限位+", "限位+");
            dgvAxisParams.Columns.Add("限位-", "限位-");
            dgvAxisParams.Columns.Add("使能", "使能");
        }

        /// <summary>
        /// 初始化PLC列表列
        /// </summary>
        private void InitPlcListColumns()
        {
            dgvPlcList.Columns.Add("序号", "序号");
            dgvPlcList.Columns.Add("品牌", "品牌");
            dgvPlcList.Columns.Add("型号", "型号");
            dgvPlcList.Columns.Add("IP", "IP地址");
            dgvPlcList.Columns.Add("端口", "端口");
            dgvPlcList.Columns.Add("启用状态", "启用状态");
        }

        /// <summary>
        /// 初始化寄存器映射列
        /// </summary>
        private void InitRegisterMapColumns()
        {
            dgvRegisterMap.Columns.Add("名称", "名称");
            dgvRegisterMap.Columns.Add("地址", "地址");
            dgvRegisterMap.Columns.Add("类型", "类型");
            dgvRegisterMap.Columns.Add("描述", "描述");
        }

        /// <summary>
        /// 初始化输入点位列表列
        /// </summary>
        private void InitInputPointsColumns()
        {
            dgvInputPoints.Columns.Add("点位名", "点位名");
            dgvInputPoints.Columns.Add("地址", "地址");
            dgvInputPoints.Columns.Add("描述", "描述");
            dgvInputPoints.Columns.Add("使能", "使能");
        }

        /// <summary>
        /// 初始化输出点位列表列
        /// </summary>
        private void InitOutputPointsColumns()
        {
            dgvOutputPoints.Columns.Add("点位名", "点位名");
            dgvOutputPoints.Columns.Add("地址", "地址");
            dgvOutputPoints.Columns.Add("描述", "描述");
            dgvOutputPoints.Columns.Add("默认值", "默认值");
            dgvOutputPoints.Columns.Add("使能", "使能");
        }

        /// <summary>
        /// 初始化用户列表列
        /// </summary>
        private void InitUserListColumns()
        {
            dgvUserList.Columns.Add("用户名", "用户名");
            dgvUserList.Columns.Add("角色", "角色");
            dgvUserList.Columns.Add("最后登录", "最后登录");
            dgvUserList.Columns.Add("操作", "操作");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // 加载系统配置
                LoadSystemSettings();

                // 加载运动卡配置
                LoadMotionSettings();

                // 加载PLC配置
                LoadPlcSettings();

                // 加载IO配置
                LoadIoSettings();

                // 加载网络配置
                LoadNetworkSettings();

                // 加载用户配置
                LoadUserSettings();
            }
            catch (Exception ex)
            {
                Logger.Error("加载配置失败", ex);
            }
        }

        /// <summary>
        /// 加载系统配置
        /// </summary>
        private void LoadSystemSettings()
        {
            try
            {
                // 从ConfigManager加载配置
                SystemConfig systemConfig = new SystemConfig();
                if (_configManager != null)
                {
                    systemConfig = _configManager.GetConfig<SystemConfig>("SystemCfg.xml", new SystemConfig());
                }
                
                // 填充界面
                txtDeviceName.Text = systemConfig.LogPath ?? "自动测试设备";
                txtDeviceModel.Text = "AutoTest-2000";
                txtDeviceSN.Text = "SN20260411001";

                numDefaultSpeed.Value = 200;
                numDefaultAccel.Value = 1000;
                numHomeSpeed.Value = 100;

                cbSoftwareLimit.Checked = true;
                numAlarmDelay.Value = 500;
            }
            catch (Exception ex)
            {
                Logger.Error("加载系统配置失败", ex);
            }
        }

        /// <summary>
        /// 加载运动卡配置
        /// </summary>
        private void LoadMotionSettings()
        {
            try
            {
                // 从ConfigManager加载配置
                MotionConfig motionConfig = new MotionConfig();
                if (_configManager != null)
                {
                    motionConfig = _configManager.GetConfig<MotionConfig>("MotionConfig.xml", new MotionConfig());
                }
                
                // 填充运动卡列表
                DataTable dtMotion = new DataTable();
                dtMotion.Columns.Add("序号", typeof(int));
                dtMotion.Columns.Add("品牌", typeof(string));
                dtMotion.Columns.Add("型号", typeof(string));
                dtMotion.Columns.Add("IP", typeof(string));
                dtMotion.Columns.Add("启用状态", typeof(bool));
                dtMotion.Columns.Add("操作", typeof(string));

                if (!string.IsNullOrEmpty(motionConfig.Brand))
                {
                    dtMotion.Rows.Add(1, motionConfig.Brand, "PCIeM60-4A", motionConfig.IP, true, "编辑");
                }
                else
                {
                    // 模拟数据
                    dtMotion.Rows.Add(1, "PCIeM60", "PCIeM60-4A", "192.168.1.100", true, "编辑");
                    dtMotion.Rows.Add(2, "GTS", "GTS-800", "192.168.1.101", false, "编辑");
                }

                if (dgvMotionCards != null)
                {
                    dgvMotionCards.DataSource = dtMotion;
                }

                // 加载轴参数
                DataTable dtAxis = new DataTable();
                dtAxis.Columns.Add("轴号", typeof(int));
                dtAxis.Columns.Add("速度", typeof(int));
                dtAxis.Columns.Add("加速度", typeof(int));
                dtAxis.Columns.Add("限位+", typeof(int));
                dtAxis.Columns.Add("限位-", typeof(int));
                dtAxis.Columns.Add("使能", typeof(bool));

                if (motionConfig.AxisParams.Count > 0)
                {
                    foreach (var axis in motionConfig.AxisParams)
                    {
                        dtAxis.Rows.Add(axis.AxisNo, axis.Speed, axis.Acceleration, axis.PositiveLimit, axis.NegativeLimit, true);
                    }
                }
                else
                {
                    // 模拟数据
                    for (int i = 0; i < 4; i++)
                    {
                        dtAxis.Rows.Add(i, 100, 1000, 1000, 0, true);
                    }
                }

                if (dgvAxisParams != null)
                {
                    dgvAxisParams.DataSource = dtAxis;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("加载运动卡配置失败", ex);
            }
        }

        /// <summary>
        /// 加载PLC配置
        /// </summary>
        private void LoadPlcSettings()
        {
            try
            {
                // 从ConfigManager加载配置
                PlcConfig plcConfig = new PlcConfig();
                if (_configManager != null)
                {
                    plcConfig = _configManager.GetConfig<PlcConfig>("PlcConfig.xml", new PlcConfig());
                }
                
                // 填充PLC列表
                DataTable dtPlc = new DataTable();
                dtPlc.Columns.Add("序号", typeof(int));
                dtPlc.Columns.Add("品牌", typeof(string));
                dtPlc.Columns.Add("型号", typeof(string));
                dtPlc.Columns.Add("IP", typeof(string));
                dtPlc.Columns.Add("端口", typeof(int));
                dtPlc.Columns.Add("启用状态", typeof(bool));

                if (!string.IsNullOrEmpty(plcConfig.Brand))
                {
                    dtPlc.Rows.Add(1, plcConfig.Brand, "ModbusRTU", plcConfig.IP, plcConfig.Port, true);
                }
                else
                {
                    // 模拟数据
                    dtPlc.Rows.Add(1, "ModbusTCP", "ModbusRTU", "192.168.1.200", 502, true);
                    dtPlc.Rows.Add(2, "S7-1200", "CPU 1214C", "192.168.1.201", 102, false);
                }

                if (dgvPlcList != null)
                {
                    dgvPlcList.DataSource = dtPlc;
                }

                // 加载寄存器映射
                DataTable dtRegister = new DataTable();
                dtRegister.Columns.Add("名称", typeof(string));
                dtRegister.Columns.Add("地址", typeof(string));
                dtRegister.Columns.Add("类型", typeof(string));
                dtRegister.Columns.Add("描述", typeof(string));

                if (plcConfig.RegisterMaps.Count > 0)
                {
                    foreach (var map in plcConfig.RegisterMaps)
                    {
                        dtRegister.Rows.Add(map.Name, map.Address, map.Type, "");
                    }
                }
                else
                {
                    // 模拟数据
                    dtRegister.Rows.Add("StartButton", "M0.0", "BOOL", "启动按钮");
                    dtRegister.Rows.Add("StopButton", "M0.1", "BOOL", "停止按钮");
                    dtRegister.Rows.Add("ResetButton", "M0.2", "BOOL", "复位按钮");
                    dtRegister.Rows.Add("RunningIndicator", "Q0.0", "BOOL", "运行指示灯");
                    dtRegister.Rows.Add("ErrorIndicator", "Q0.1", "BOOL", "错误指示灯");
                }

                if (dgvRegisterMap != null)
                {
                    dgvRegisterMap.DataSource = dtRegister;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("加载PLC配置失败", ex);
            }
        }

        /// <summary>
        /// 加载IO配置
        /// </summary>
        private void LoadIoSettings()
        {
            try
            {
                // 从ConfigManager加载配置
                IoConfig ioConfig = new IoConfig();
                if (_configManager != null)
                {
                    ioConfig = _configManager.GetConfig<IoConfig>("IoConfig.xml", new IoConfig());
                }
                
                // 加载输入点位
                DataTable dtInput = new DataTable();
                dtInput.Columns.Add("点位名", typeof(string));
                dtInput.Columns.Add("地址", typeof(string));
                dtInput.Columns.Add("描述", typeof(string));
                dtInput.Columns.Add("使能", typeof(bool));

                if (ioConfig.Inputs.Count > 0)
                {
                    foreach (var input in ioConfig.Inputs)
                    {
                        dtInput.Rows.Add(input.Name, $"DI{input.Port}.{input.Pin}", "", true);
                    }
                }
                else
                {
                    // 模拟数据
                    dtInput.Rows.Add("Sensor1", "DI0.0", "物料传感器", true);
                    dtInput.Rows.Add("Sensor2", "DI0.1", "位置传感器", true);
                    dtInput.Rows.Add("LimitSwitch1", "DI0.2", "限位开关1", true);
                    dtInput.Rows.Add("LimitSwitch2", "DI0.3", "限位开关2", true);
                }

                if (dgvInputPoints != null)
                {
                    dgvInputPoints.DataSource = dtInput;
                }

                // 加载输出点位
                DataTable dtOutput = new DataTable();
                dtOutput.Columns.Add("点位名", typeof(string));
                dtOutput.Columns.Add("地址", typeof(string));
                dtOutput.Columns.Add("描述", typeof(string));
                dtOutput.Columns.Add("默认值", typeof(bool));
                dtOutput.Columns.Add("使能", typeof(bool));

                if (ioConfig.Outputs.Count > 0)
                {
                    foreach (var output in ioConfig.Outputs)
                    {
                        dtOutput.Rows.Add(output.Name, $"DO{output.Port}.{output.Pin}", "", false, true);
                    }
                }
                else
                {
                    // 模拟数据
                    dtOutput.Rows.Add("Light1", "DO1.0", "指示灯1", false, true);
                    dtOutput.Rows.Add("Light2", "DO1.1", "指示灯2", false, true);
                    dtOutput.Rows.Add("Cylinder1", "DO1.2", "气缸1", false, true);
                    dtOutput.Rows.Add("Cylinder2", "DO1.3", "气缸2", false, true);
                }

                if (dgvOutputPoints != null)
                {
                    dgvOutputPoints.DataSource = dtOutput;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("加载IO配置失败", ex);
            }
        }

        /// <summary>
        /// 加载网络配置
        /// </summary>
        private void LoadNetworkSettings()
        {
            // 模拟加载配置
            txtMesIP.Text = "192.168.1.30";
            numMesPort.Value = 5000;
            cbMesProtocol.Text = "TCP";

            txtMqttBroker.Text = "192.168.1.40";
            numMqttPort.Value = 1883;
            txtMqttTopic.Text = "auto/device";

            numRemotePort.Value = 8080;
            cbRemoteEnable.Checked = true;
        }

        /// <summary>
        /// 加载用户配置
        /// </summary>
        private void LoadUserSettings()
        {
            // 模拟加载配置
            DataTable dtUser = new DataTable();
            dtUser.Columns.Add("用户名", typeof(string));
            dtUser.Columns.Add("角色", typeof(string));
            dtUser.Columns.Add("最后登录", typeof(string));
            dtUser.Columns.Add("操作", typeof(string));

            dtUser.Rows.Add("admin", "管理员", "2026-04-11 10:00:00", "编辑");
            dtUser.Rows.Add("engineer", "工程师", "2026-04-11 09:30:00", "编辑");
            dtUser.Rows.Add("operator", "操作员", "2026-04-11 09:00:00", "编辑");

            dgvUserList.DataSource = dtUser;
        }

        /// <summary>
        /// 更新导航按钮状态
        /// </summary>
        private void UpdateNavButtonState()
        {
            // 重置所有按钮状态
            ResetNavButtons();

            // 根据当前选中的Tab页更新按钮状态
            switch (tabControl1.SelectedIndex)
            {
                case 0: // 系统配置
                    btnSystem.BackColor = Idle;
                    btnSystem.ForeColor = TextLight;
                    break;
                case 1: // 运动卡配置
                    btnDevice.BackColor = Idle;
                    btnDevice.ForeColor = TextLight;
                    btnMotionCard.BackColor = Running;
                    btnMotionCard.ForeColor = TextLight;
                    break;
                case 2: // PLC配置
                    btnDevice.BackColor = Idle;
                    btnDevice.ForeColor = TextLight;
                    btnPlcConfig.BackColor = Running;
                    btnPlcConfig.ForeColor = TextLight;
                    break;
                case 3: // IO配置
                    btnDevice.BackColor = Idle;
                    btnDevice.ForeColor = TextLight;
                    btnIoConfig.BackColor = Running;
                    btnIoConfig.ForeColor = TextLight;
                    break;
                case 4: // 网络配置
                    btnNetwork.BackColor = Idle;
                    btnNetwork.ForeColor = TextLight;
                    break;
                case 5: // 用户管理
                    btnUserManage.BackColor = Idle;
                    btnUserManage.ForeColor = TextLight;
                    break;
            }
        }

        /// <summary>
        /// 重置导航按钮状态
        /// </summary>
        private void ResetNavButtons()
        {
            btnSystem.BackColor = BgMain;
            btnSystem.ForeColor = TextBody;
            btnDevice.BackColor = BgMain;
            btnDevice.ForeColor = TextBody;
            btnMotionCard.BackColor = BgMain;
            btnMotionCard.ForeColor = TextBody;
            btnPlcConfig.BackColor = BgMain;
            btnPlcConfig.ForeColor = TextBody;
            btnIoConfig.BackColor = BgMain;
            btnIoConfig.ForeColor = TextBody;
            btnNetwork.BackColor = BgMain;
            btnNetwork.ForeColor = TextBody;
            btnUserManage.BackColor = BgMain;
            btnUserManage.ForeColor = TextBody;
        }

        /// <summary>
        /// 系统配置按钮点击事件
        /// </summary>
        private void btnSystem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
            UpdateNavButtonState();
        }

        /// <summary>
        /// 设备配置按钮点击事件
        /// </summary>
        private void btnDevice_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            UpdateNavButtonState();
        }

        /// <summary>
        /// 运动卡配置按钮点击事件
        /// </summary>
        private void btnMotionCard_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            UpdateNavButtonState();
        }

        /// <summary>
        /// PLC配置按钮点击事件
        /// </summary>
        private void btnPlcConfig_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
            UpdateNavButtonState();
        }

        /// <summary>
        /// IO配置按钮点击事件
        /// </summary>
        private void btnIoConfig_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 3;
            UpdateNavButtonState();
        }

        /// <summary>
        /// 网络配置按钮点击事件
        /// </summary>
        private void btnNetwork_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 4;
            UpdateNavButtonState();
        }

        /// <summary>
        /// 用户管理按钮点击事件
        /// </summary>
        private void btnUserManage_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 5;
            UpdateNavButtonState();
        }

        /// <summary>
        /// 保存配置按钮点击事件
        /// </summary>
        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            // 验证配置
            if (ValidateSettings())
            {
                // 保存配置
                SaveSettings();
                
                // 显示保存成功提示
                MessageBox.Show("配置保存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 导入配置按钮点击事件
        /// </summary>
        private void btnImportConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "配置文件 (*.xml;*.json)|*.xml;*.json|XML文件 (*.xml)|*.xml|JSON文件 (*.json)|*.json",
                Title = "导入配置"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    // 读取配置文件
                    string configContent = File.ReadAllText(filePath);
                    string fileName = Path.GetFileName(filePath);
                    
                    // 根据文件名确定配置类型
                    if (fileName.Equals("SystemCfg.xml", StringComparison.OrdinalIgnoreCase))
                    {
                        var systemConfig = XmlHelper.Deserialize<SystemConfig>(configContent);
                        _configManager.SaveConfig("SystemCfg.xml", systemConfig);
                    }
                    else if (fileName.Equals("MotionConfig.xml", StringComparison.OrdinalIgnoreCase))
                    {
                        var motionConfig = XmlHelper.Deserialize<MotionConfig>(configContent);
                        _configManager.SaveConfig("MotionConfig.xml", motionConfig);
                    }
                    else if (fileName.Equals("PlcConfig.xml", StringComparison.OrdinalIgnoreCase))
                    {
                        var plcConfig = XmlHelper.Deserialize<PlcConfig>(configContent);
                        _configManager.SaveConfig("PlcConfig.xml", plcConfig);
                    }
                    else if (fileName.Equals("IoConfig.xml", StringComparison.OrdinalIgnoreCase))
                    {
                        var ioConfig = XmlHelper.Deserialize<IoConfig>(configContent);
                        _configManager.SaveConfig("IoConfig.xml", ioConfig);
                    }
                    
                    MessageBox.Show("配置导入成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadSettings();
                }
                catch (Exception ex)
                {
                    Logger.Error("导入配置失败", ex);
                    MessageBox.Show("导入配置失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 导出配置按钮点击事件
        /// </summary>
        private void btnExportConfig_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "XML文件 (*.xml)|*.xml|JSON文件 (*.json)|*.json",
                Title = "导出配置"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                try
                {
                    string fileExtension = Path.GetExtension(filePath);
                    
                    // 根据扩展名选择导出格式
                    if (fileExtension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        // 导出为XML
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        
                        if (fileName.Equals("SystemCfg", StringComparison.OrdinalIgnoreCase))
                        {
                            var systemConfig = _configManager.GetConfig<SystemConfig>("SystemCfg.xml", new SystemConfig());
                            string configContent = XmlHelper.Serialize(systemConfig);
                            File.WriteAllText(filePath, configContent);
                        }
                        else if (fileName.Equals("MotionConfig", StringComparison.OrdinalIgnoreCase))
                        {
                            var motionConfig = _configManager.GetConfig<MotionConfig>("MotionConfig.xml", new MotionConfig());
                            string configContent = XmlHelper.Serialize(motionConfig);
                            File.WriteAllText(filePath, configContent);
                        }
                        else if (fileName.Equals("PlcConfig", StringComparison.OrdinalIgnoreCase))
                        {
                            var plcConfig = _configManager.GetConfig<PlcConfig>("PlcConfig.xml", new PlcConfig());
                            string configContent = XmlHelper.Serialize(plcConfig);
                            File.WriteAllText(filePath, configContent);
                        }
                        else if (fileName.Equals("IoConfig", StringComparison.OrdinalIgnoreCase))
                        {
                            var ioConfig = _configManager.GetConfig<IoConfig>("IoConfig.xml", new IoConfig());
                            string configContent = XmlHelper.Serialize(ioConfig);
                            File.WriteAllText(filePath, configContent);
                        }
                    }
                    else if (fileExtension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        // 导出为JSON
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        
                        if (fileName.Equals("SystemCfg", StringComparison.OrdinalIgnoreCase))
                        {
                            var systemConfig = _configManager.GetConfig<SystemConfig>("SystemCfg.xml", new SystemConfig());
                            string configContent = Newtonsoft.Json.JsonConvert.SerializeObject(systemConfig, Newtonsoft.Json.Formatting.Indented);
                            File.WriteAllText(filePath, configContent);
                        }
                        else if (fileName.Equals("MotionConfig", StringComparison.OrdinalIgnoreCase))
                        {
                            var motionConfig = _configManager.GetConfig<MotionConfig>("MotionConfig.xml", new MotionConfig());
                            string configContent = Newtonsoft.Json.JsonConvert.SerializeObject(motionConfig, Newtonsoft.Json.Formatting.Indented);
                            File.WriteAllText(filePath, configContent);
                        }
                        else if (fileName.Equals("PlcConfig", StringComparison.OrdinalIgnoreCase))
                        {
                            var plcConfig = _configManager.GetConfig<PlcConfig>("PlcConfig.xml", new PlcConfig());
                            string configContent = Newtonsoft.Json.JsonConvert.SerializeObject(plcConfig, Newtonsoft.Json.Formatting.Indented);
                            File.WriteAllText(filePath, configContent);
                        }
                        else if (fileName.Equals("IoConfig", StringComparison.OrdinalIgnoreCase))
                        {
                            var ioConfig = _configManager.GetConfig<IoConfig>("IoConfig.xml", new IoConfig());
                            string configContent = Newtonsoft.Json.JsonConvert.SerializeObject(ioConfig, Newtonsoft.Json.Formatting.Indented);
                            File.WriteAllText(filePath, configContent);
                        }
                    }
                    
                    MessageBox.Show("配置导出成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Logger.Error("导出配置失败", ex);
                    MessageBox.Show("导出配置失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        /// <returns>是否验证通过</returns>
        private bool ValidateSettings()
        {
            bool isValid = true;

            // 验证系统配置
            if (string.IsNullOrEmpty(txtDeviceName.Text))
            {
                errorProvider1.SetError(txtDeviceName, "设备名称不能为空");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(txtDeviceName, "");
            }

            if (string.IsNullOrEmpty(txtDeviceModel.Text))
            {
                errorProvider1.SetError(txtDeviceModel, "设备型号不能为空");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(txtDeviceModel, "");
            }

            if (string.IsNullOrEmpty(txtDeviceSN.Text))
            {
                errorProvider1.SetError(txtDeviceSN, "序列号不能为空");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(txtDeviceSN, "");
            }

            // 验证运动卡配置
            if (string.IsNullOrEmpty(txtMotionIP.Text))
            {
                errorProvider1.SetError(txtMotionIP, "IP地址不能为空");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(txtMotionIP, "");
            }

            // 验证PLC配置
            if (string.IsNullOrEmpty(txtPlcIP.Text))
            {
                errorProvider1.SetError(txtPlcIP, "IP地址不能为空");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(txtPlcIP, "");
            }

            // 验证网络配置
            if (string.IsNullOrEmpty(txtMesIP.Text))
            {
                errorProvider1.SetError(txtMesIP, "MES服务器IP不能为空");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(txtMesIP, "");
            }

            if (string.IsNullOrEmpty(txtMqttBroker.Text))
            {
                errorProvider1.SetError(txtMqttBroker, "Broker地址不能为空");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(txtMqttBroker, "");
            }

            if (string.IsNullOrEmpty(txtMqttTopic.Text))
            {
                errorProvider1.SetError(txtMqttTopic, "默认Topic不能为空");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(txtMqttTopic, "");
            }

            // 使用ConfigManager进行更全面的校验
            try
            {
                // 验证运动卡配置
                var motionConfig = new MotionConfig
                {
                    Brand = "PCIeM60",
                    IP = txtMotionIP.Text,
                    AxisCount = (int)numAxisCount.Value,
                    AxisParams = new List<AxisParam>()
                };
                
                foreach (DataGridViewRow row in dgvAxisParams.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        var axisParam = new AxisParam
                        {
                            AxisNo = Convert.ToInt32(row.Cells["轴号"].Value),
                            Speed = Convert.ToDouble(row.Cells["速度"].Value),
                            Acceleration = Convert.ToDouble(row.Cells["加速度"].Value),
                            PositiveLimit = Convert.ToDouble(row.Cells["限位+"].Value),
                            NegativeLimit = Convert.ToDouble(row.Cells["限位-"].Value)
                        };
                        motionConfig.AxisParams.Add(axisParam);
                    }
                }
                
                List<string> errors;
                if (!_configManager.ValidateConfig(motionConfig, out errors))
                {
                    isValid = false;
                    foreach (var error in errors)
                    {
                        MessageBox.Show(error, "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                // 验证PLC配置
                var plcConfig = new PlcConfig
                {
                    Brand = cbPlcBrand.Text,
                    IP = txtPlcIP.Text,
                    Port = (int)numPlcPort.Value
                };
                
                if (!_configManager.ValidateConfig(plcConfig, out errors))
                {
                    isValid = false;
                    foreach (var error in errors)
                    {
                        MessageBox.Show(error, "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("验证配置失败", ex);
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                // 保存系统配置
                var systemConfig = new SystemConfig
                {
                    LogPath = txtDeviceName.Text,
                    WatchdogInterval = 5000
                };
                _configManager.SaveConfig("SystemCfg.xml", systemConfig);

                // 保存运动卡配置
                var motionConfig = new MotionConfig
                {
                    Brand = "PCIeM60",
                    IP = txtMotionIP.Text,
                    AxisCount = (int)numAxisCount.Value,
                    AxisParams = new List<AxisParam>()
                };
                
                // 从DataGridView获取轴参数
                foreach (DataGridViewRow row in dgvAxisParams.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        var axisParam = new AxisParam
                        {
                            AxisNo = Convert.ToInt32(row.Cells["轴号"].Value),
                            Speed = Convert.ToDouble(row.Cells["速度"].Value),
                            Acceleration = Convert.ToDouble(row.Cells["加速度"].Value),
                            PositiveLimit = Convert.ToDouble(row.Cells["限位+"].Value),
                            NegativeLimit = Convert.ToDouble(row.Cells["限位-"].Value)
                        };
                        motionConfig.AxisParams.Add(axisParam);
                    }
                }
                _configManager.SaveConfig("MotionConfig.xml", motionConfig);

                // 保存PLC配置
                var plcConfig = new PlcConfig
                {
                    Brand = cbPlcBrand.Text,
                    IP = txtPlcIP.Text,
                    Port = (int)numPlcPort.Value,
                    RegisterMaps = new List<RegisterMap>()
                };
                
                // 从DataGridView获取寄存器映射
                foreach (DataGridViewRow row in dgvRegisterMap.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        var registerMap = new RegisterMap
                        {
                            Name = row.Cells["名称"].Value.ToString(),
                            Address = row.Cells["地址"].Value.ToString(),
                            Type = row.Cells["类型"].Value.ToString()
                        };
                        plcConfig.RegisterMaps.Add(registerMap);
                    }
                }
                _configManager.SaveConfig("PlcConfig.xml", plcConfig);

                // 保存IO配置
                var ioConfig = new IoConfig
                {
                    Inputs = new List<IoPoint>(),
                    Outputs = new List<IoPoint>(),
                    TriggerLogics = new List<TriggerLogic>()
                };
                
                // 从DataGridView获取输入点位
                foreach (DataGridViewRow row in dgvInputPoints.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        string address = row.Cells["地址"].Value.ToString();
                        string[] parts = address.Substring(2).Split('.');
                        var ioPoint = new IoPoint
                        {
                            Name = row.Cells["点位名"].Value.ToString(),
                            Port = Convert.ToInt32(parts[0]),
                            Pin = Convert.ToInt32(parts[1])
                        };
                        ioConfig.Inputs.Add(ioPoint);
                    }
                }
                
                // 从DataGridView获取输出点位
                foreach (DataGridViewRow row in dgvOutputPoints.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        string address = row.Cells["地址"].Value.ToString();
                        string[] parts = address.Substring(2).Split('.');
                        var ioPoint = new IoPoint
                        {
                            Name = row.Cells["点位名"].Value.ToString(),
                            Port = Convert.ToInt32(parts[0]),
                            Pin = Convert.ToInt32(parts[1])
                        };
                        ioConfig.Outputs.Add(ioPoint);
                    }
                }
                _configManager.SaveConfig("IoConfig.xml", ioConfig);

                Logger.Info("配置保存成功");
            }
            catch (Exception ex)
            {
                Logger.Error("保存配置失败", ex);
                MessageBox.Show("保存配置失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 按键按下事件
        /// </summary>
        private void SettingForm_KeyDown(object sender, KeyEventArgs e)
        {
            // 支持Ctrl+S快捷键保存
            if (e.Control && e.KeyCode == Keys.S)
            {
                btnSaveConfig_Click(sender, e);
            }
        }
    }
}