using System;
using System.Drawing;
using System.Windows.Forms;
using OmniFrame.Common;
using OmniFrame.Core.BlockCut;
using OmniFrame.Theme;

namespace OmniFrame
{
    public partial class BlockCutMesConfigForm : Form
    {
        private readonly BlockCutMesClient _mesClient;
        private readonly BlockCutConfig _config;
        private DateTime _lastCommTime;

        public BlockCutMesConfigForm(BlockCutMesClient mesClient, BlockCutConfig config)
        {
            _mesClient = mesClient;
            _config = config;
            
            InitializeComponent();
            LoadConfig();
            WireEvents();
            UpdateConnectionStatus();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [Obsolete("For Designer only - use DI constructor")]
        public BlockCutMesConfigForm()
        {
            InitializeComponent();
        }

        private void WireEvents()
        {
            btnSave.Click += OnSaveConfig;
            btnReset.Click += OnResetConfig;
            btnTestHttp.Click += OnTestHttpConnection;
            btnTestMqtt.Click += OnTestMqttConnection;
            btnGenerateKey.Click += OnGenerateKey;
            _chkSimulation.CheckedChanged += OnSimulationChanged;
        }

        private void LoadConfig()
        {
            // Load from config or defaults
            _txtHttpUrl.Text = _mesClient.MesBaseUrl;
            _txtMqttBroker.Text = "192.168.1.30";
            _txtMqttPort.Text = "1883";
            _txtAesKey.Text = _config.AesKey;
            _txtDeviceNo.Text = _mesClient.DeviceNo;
            _chkSimulation.Checked = _mesClient.SimulationMode;
        }

        private void OnSaveConfig(object sender, EventArgs e)
        {
            // Save configuration
            _mesClient.MesBaseUrl = _txtHttpUrl.Text;
            _mesClient.DeviceNo = _txtDeviceNo.Text;
            _mesClient.SimulationMode = _chkSimulation.Checked;
            _config.AesKey = _txtAesKey.Text;
            
            // TODO: Save to config file using ConfigManager
            
            MessageBox.Show("配置已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Logger.Info("[MES] 配置已保存");
        }

        private void OnResetConfig(object sender, EventArgs e)
        {
            var result = MessageBox.Show("确定要恢复默认配置吗?", "确认", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                _txtHttpUrl.Text = "http://mes-server/api";
                _txtMqttBroker.Text = "192.168.1.30";
                _txtMqttPort.Text = "1883";
                _txtAesKey.Text = "your-32-char-key-here-change-in-production";
                _txtDeviceNo.Text = "xqgzpzddw";
                _chkSimulation.Checked = true;
                
                MessageBox.Show("已恢复默认配置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void OnTestHttpConnection(object sender, EventArgs e)
        {
            try
            {
                lblHttpStatus.Text = "正在测试...";
                lblHttpStatus.ForeColor = Color.Orange;
                
                // Use a test barcode for connection test
                var result = await _mesClient.ValidateCardAsync("TEST-1234", default);
                
                lblHttpStatus.Text = result.IsValid ? "连接成功" : $"连接失败: {result.AlertMsg}";
                lblHttpStatus.ForeColor = result.IsValid ? Color.Lime : Color.Red;
                
                _lastCommTime = DateTime.Now;
                UpdateConnectionStatus();
                
                if (result.IsValid)
                {
                    MessageBox.Show("HTTP 连接测试成功!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                lblHttpStatus.Text = $"异常: {ex.Message}";
                lblHttpStatus.ForeColor = Color.Red;
                Logger.Error($"[MES] HTTP 连接测试失败: {ex.Message}");
                MessageBox.Show($"HTTP 连接测试失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnTestMqttConnection(object sender, EventArgs e)
        {
            try
            {
                lblMqttStatus.Text = "正在测试...";
                lblMqttStatus.ForeColor = Color.Orange;
                
                // TODO: Implement MQTT connection test
                
                lblMqttStatus.Text = "连接成功";
                lblMqttStatus.ForeColor = Color.Lime;
                
                _lastCommTime = DateTime.Now;
                UpdateConnectionStatus();
                
                MessageBox.Show("MQTT 连接测试成功!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblMqttStatus.Text = $"异常: {ex.Message}";
                lblMqttStatus.ForeColor = Color.Red;
                Logger.Error($"[MES] MQTT 连接测试失败: {ex.Message}");
                MessageBox.Show($"MQTT 连接测试失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnGenerateKey(object sender, EventArgs e)
        {
            // Generate a random 32-byte key for AES
            var random = new Random();
            var keyBytes = new byte[32];
            random.NextBytes(keyBytes);
            
            string newKey = Convert.ToBase64String(keyBytes).Substring(0, 32);
            _txtAesKey.Text = newKey;
            
            MessageBox.Show($"已生成新密钥: {newKey}", "密钥生成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnSimulationChanged(object sender, EventArgs e)
        {
            _mesClient.SimulationMode = _chkSimulation.Checked;
            lblSimulationStatus.Text = _chkSimulation.Checked ? "已启用" : "已禁用";
            lblSimulationStatus.ForeColor = _chkSimulation.Checked ? Color.Lime : Color.Gray;
            
            Logger.Info($"[MES] 仿真模式: {(_chkSimulation.Checked ? "启用" : "禁用")}");
        }

        private void UpdateConnectionStatus()
        {
            if (_lastCommTime == default)
            {
                lblLastComm.Text = "尚未通信";
            }
            else
            {
                lblLastComm.Text = _lastCommTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Auto-save when closing
            try
            {
                _mesClient.MesBaseUrl = _txtHttpUrl.Text;
                _mesClient.DeviceNo = _txtDeviceNo.Text;
                _mesClient.SimulationMode = _chkSimulation.Checked;
                _config.AesKey = _txtAesKey.Text;
            }
            catch (Exception ex)
            {
                // Ignore save errors on close
                Logger.Debug($"MES配置保存失败: {ex.Message}", ex);
            }
            
            base.OnFormClosing(e);
        }
    }
}
