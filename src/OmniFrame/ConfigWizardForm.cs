using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
    /// 配置向导窗体类 - 多步骤配置引导流程
        /// </summary>
    public partial class ConfigWizardForm : Form
    {
        private int _currentStep = 1;
        private const int TotalSteps = 4;
        private SystemConfig _systemConfig;

        // 步骤面板
        private Panel pnlStep1;
        private Panel pnlStep2;
        private Panel pnlStep3;
        private Panel pnlStep4;

        // 步骤1控件
        private TextBox txtPlcIp;
        private NumericUpDown numPlcPort;

        // 步骤2控件
        private NumericUpDown numWebApiPort;
        private NumericUpDown numWebSocketPort;
        private TextBox txtCorsWhitelist;

        // 步骤3控件
        private TextBox txtDataSavePath;
        private TextBox txtLogPath;
        private Button btnBrowseDataPath;
        private Button btnBrowseLogPath;

        // 步骤4控件
        private NumericUpDown numWatchdogInterval;
        private ComboBox cbxLogLevel;

        private readonly IConfigManager _configManager;

        // 导航按钮
        private Button btnPrevious;
        private Button btnNext;
        private Button btnCancel;

        // 步骤指示器
        private Label lblStep1;
        private Label lblStep2;
        private Label lblStep3;
        private Panel panel1;
        private Label lblTitle;
        private Panel panel3;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Panel panel4;
        private Label label19;
        private Label label20;
        private Label label15;
        private Label label16;
        private Label label10;
        private Label label11;
        private Label label12;
        private Label label6;
        private Label label7;
        private Panel panel2;
        private Label lblStep4;

        public ConfigWizardForm(IConfigManager configManager)
        {
            _configManager = configManager;
            InitializeComponent();
            _systemConfig = new SystemConfig();
            InitializeDefaultValues();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }



        private void InitializeDefaultValues()
        {
            // 设备连接参数默认值
            txtPlcIp.Text = "192.168.1.100";
            numPlcPort.Value = 502;

            // 网络地址默认值
            numWebApiPort.Value = 8080;
            numWebSocketPort.Value = 8081;
            txtCorsWhitelist.Text = "http://localhost:3000,http://127.0.0.1:3000";

            // 数据库和文件路径默认值
            txtDataSavePath.Text = @"D:\Data";
            txtLogPath.Text = @"Logs";

            // 系统参数默认值
            numWatchdogInterval.Value = 10000;
            cbxLogLevel.SelectedIndex = 2; // Debug
        }

        private void ShowStep(int step)
        {
            // 隐藏所有步骤面板
            pnlStep1.Visible = false;
            pnlStep2.Visible = false;
            pnlStep3.Visible = false;
            pnlStep4.Visible = false;

            // 显示当前步骤面板
            switch (step)
            {
                case 1:
                    pnlStep1.Visible = true;
                    break;
                case 2:
                    pnlStep2.Visible = true;
                    break;
                case 3:
                    pnlStep3.Visible = true;
                    break;
                case 4:
                    pnlStep4.Visible = true;
                    break;
            }

            // 更新步骤指示器
            UpdateStepIndicator();

            // 更新按钮状态
            btnPrevious.Enabled = step > 1;
            btnNext.Text = step == TotalSteps ? "完成" : "下一步";
        }

        private void UpdateStepIndicator()
        {
            // 更新步骤指示器
            lblStep1.Text = _currentStep == 1 ? "●" : "○";
            lblStep2.Text = _currentStep == 2 ? "●" : "○";
            lblStep3.Text = _currentStep == 3 ? "●" : "○";
            lblStep4.Text = _currentStep == 4 ? "●" : "○";

            lblStep1.ForeColor = _currentStep == 1 ? Color.Blue : Color.Gray;
            lblStep2.ForeColor = _currentStep == 2 ? Color.Blue : Color.Gray;
            lblStep3.ForeColor = _currentStep == 3 ? Color.Blue : Color.Gray;
            lblStep4.ForeColor = _currentStep == 4 ? Color.Blue : Color.Gray;
        }

        private bool ValidateCurrentStep()
        {
            switch (_currentStep)
            {
                case 1:
                    // 验证设备连接参数
                    if (string.IsNullOrEmpty(txtPlcIp.Text))
                    {
                        MessageBox.Show("请输入PLC IP地址", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    if (!System.Net.IPAddress.TryParse(txtPlcIp.Text, out _))
                    {
                        MessageBox.Show("PLC IP地址格式不正确", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    break;
                case 2:
                    // 验证网络地址
                    if (numWebApiPort.Value < 1 || numWebApiPort.Value > 65535)
                    {
                        MessageBox.Show("Web API端口必须在1-65535之间", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    if (numWebSocketPort.Value < 1 || numWebSocketPort.Value > 65535)
                    {
                        MessageBox.Show("WebSocket端口必须在1-65535之间", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    break;
                case 3:
                    // 验证文件路径
                    if (string.IsNullOrEmpty(txtDataSavePath.Text))
                    {
                        MessageBox.Show("请输入数据保存路径", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    if (string.IsNullOrEmpty(txtLogPath.Text))
                    {
                        MessageBox.Show("请输入日志路径", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    break;
                case 4:
                    // 验证系统参数
                    if (numWatchdogInterval.Value < 1000 || numWatchdogInterval.Value > 60000)
                    {
                        MessageBox.Show("看门狗间隔必须在1000-60000ms之间", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    break;
            }
            return true;
        }

        private void SaveCurrentStep()
        {
            switch (_currentStep)
            {
                case 1:
                    // 保存设备连接参数
                    _systemConfig.NetworkConfig.PlcIp = txtPlcIp.Text;
                    _systemConfig.NetworkConfig.PlcPort = (int)numPlcPort.Value;
                    break;
                case 2:
                    // 保存网络地址
                    _systemConfig.NetworkConfig.WebApiPort = (int)numWebApiPort.Value;
                    _systemConfig.NetworkConfig.WebSocketPort = (int)numWebSocketPort.Value;
                    // 解析CORS白名单
                    _systemConfig.NetworkConfig.CorsWhitelist = txtCorsWhitelist.Text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim()).ToList();
                    break;
                case 3:
                    // 保存文件路径
                    _systemConfig.DataSavePath = txtDataSavePath.Text;
                    _systemConfig.LogPath = txtLogPath.Text;
                    break;
                case 4:
                    // 保存系统参数
                    _systemConfig.WatchdogInterval = (int)numWatchdogInterval.Value;
                    // 这里可以添加日志级别的保存逻辑
                    break;
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (_currentStep > 1)
            {
                _currentStep--;
                ShowStep(_currentStep);
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (!ValidateCurrentStep())
                return;

            SaveCurrentStep();

            if (_currentStep < TotalSteps)
            {
                _currentStep++;
                ShowStep(_currentStep);
            }
            else
            {
                // 完成配置向导
                if (SaveConfig())
                {
                    MessageBox.Show("配置保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("配置保存失败，请重试", "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool SaveConfig()
        {
            try
            {
                // 保存系统配置
                bool success = _configManager.SaveConfig("SystemCfg.xml", _systemConfig);
                
                return success;
            }
            catch (Exception ex)
            {
                Logger.Error("保存配置失败", ex);
                return false;
            }
        }

        private void btnBrowseDataPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择数据保存路径";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtDataSavePath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnBrowseLogPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择日志保存路径";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtLogPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
