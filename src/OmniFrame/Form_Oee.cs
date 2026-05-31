using System;
using System.Windows.Forms;
using OmniFrame.Core.AdvancedFeatures;
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// OEE 管理表单
    /// 用于计算和显示设备综合效率
        /// </summary>
    public partial class Form_Oee : Form
    {
        private IOeeManager _oeeManager;

        public Form_Oee(IOeeManager oeeManager)
        {
            InitializeComponent();
            _oeeManager = oeeManager;

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        private void Form_Oee_Load(object sender, EventArgs e)
        {
            // 初始化表单
            InitializeForm();
        }

        private void InitializeForm()
        {
            // 设置默认值
            txtLineName.Text = "Line1";
            dtpStartTime.Value = DateTime.Now.AddHours(-1);
            dtpEndTime.Value = DateTime.Now;

            // 初始化按钮状态
            btnStartProduction.Enabled = true;
            btnStopProduction.Enabled = false;
            btnRecordGood.Enabled = false;
            btnRecordBad.Enabled = false;
            btnCalculateOee.Enabled = false;
        }

        private void btnStartProduction_Click(object sender, EventArgs e)
        {
            try
            {
                string lineName = txtLineName.Text;
                _oeeManager.StartProduction(lineName);
                MessageBox.Show("生产开始", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 更新按钮状态
                btnStartProduction.Enabled = false;
                btnStopProduction.Enabled = true;
                btnRecordGood.Enabled = true;
                btnRecordBad.Enabled = true;
            }
            catch (Exception ex)
            {
                Logger.Error("生产开始失败", ex);
                MessageBox.Show($"生产开始失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStopProduction_Click(object sender, EventArgs e)
        {
            try
            {
                string lineName = txtLineName.Text;
                _oeeManager.StopProduction(lineName);
                MessageBox.Show("生产停止", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 更新按钮状态
                btnStartProduction.Enabled = true;
                btnStopProduction.Enabled = false;
                btnRecordGood.Enabled = false;
                btnRecordBad.Enabled = false;
                btnCalculateOee.Enabled = true;
            }
            catch (Exception ex)
            {
                Logger.Error("生产停止失败", ex);
                MessageBox.Show($"生产停止失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRecordGood_Click(object sender, EventArgs e)
        {
            try
            {
                string lineName = txtLineName.Text;
                _oeeManager.RecordGoodProduct(lineName);
                MessageBox.Show("记录良品成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("记录良品失败", ex);
                MessageBox.Show($"记录良品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRecordBad_Click(object sender, EventArgs e)
        {
            try
            {
                string lineName = txtLineName.Text;
                _oeeManager.RecordBadProduct(lineName);
                MessageBox.Show("记录不良品成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("记录不良品失败", ex);
                MessageBox.Show($"记录不良品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCalculateOee_Click(object sender, EventArgs e)
        {
            try
            {
                string lineName = txtLineName.Text;
                double oee = _oeeManager.CalculateOee(lineName);
                txtOeeValue.Text = $"{oee:P2}";
                MessageBox.Show($"OEE 计算完成: {oee:P2}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("OEE 计算失败", ex);
                MessageBox.Show($"OEE 计算失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRecordDowntime_Click(object sender, EventArgs e)
        {
            try
            {
                string lineName = txtLineName.Text;
                TimeSpan duration = TimeSpan.FromMinutes(double.Parse(txtDowntimeMinutes.Text));
                string reason = txtDowntimeReason.Text;
                _oeeManager.RecordDowntime(lineName, duration, reason);
                MessageBox.Show("记录停机时间成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("记录停机时间失败", ex);
                MessageBox.Show($"记录停机时间失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}