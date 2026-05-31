using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using OmniFrame.Core;
using OmniFrame.Theme;

namespace OmniFrame
{
    public partial class EquipmentControlForm : Form
    {
        // 视觉检测相关
        private bool isVisionConnected = false;
        private System.Windows.Forms.Timer visionStatusTimer;
        private Image currentImage;

        // 上料机构相关
        private int productionCount = 0;
        private DateTime startTime = DateTime.Now;
        private bool isProductionRunning = false;
        private System.Windows.Forms.Timer upUpdateTimer;

        public EquipmentControlForm()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                InitializeTimers();
                listBoxDevices.SelectedIndex = 0;
                ShowDevicePanel(0);
            }

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        private void InitializeTimers()
        {
            visionStatusTimer = new System.Windows.Forms.Timer();
            visionStatusTimer.Interval = 2000;
            visionStatusTimer.Tick += VisionStatusTimer_Tick;

            upUpdateTimer = new System.Windows.Forms.Timer();
            upUpdateTimer.Interval = 1000;
            upUpdateTimer.Tick += UpUpdateTimer_Tick;
        }

        private void listBoxDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowDevicePanel(listBoxDevices.SelectedIndex);
        }

        private void ShowDevicePanel(int index)
        {
            panelControl.Controls.Clear();

            switch (index)
            {
                case 0:
                    panelControl.Controls.Add(panelVision);
                    panelVision.Dock = DockStyle.Fill;
                    break;
                case 1:
                    panelControl.Controls.Add(panelUp);
                    panelUp.Dock = DockStyle.Fill;
                    break;
            }
        }

        // ===== 视觉检测 =====

        private void VisionStatusTimer_Tick(object sender, EventArgs e)
        {
            if (isVisionConnected)
            {
                lblConnectionStatus_Vision.Text = "连接状态: 已连接";
                lblConnectionStatus_Vision.ForeColor = Color.Green;
            }
            else
            {
                lblConnectionStatus_Vision.Text = "连接状态: 未连接";
                lblConnectionStatus_Vision.ForeColor = Color.Red;
            }
        }

        private void btnConnect_Vision_Click(object sender, EventArgs e)
        {
            string visionIp = txtVisionIp.Text.Trim();
            if (string.IsNullOrEmpty(visionIp))
            {
                MessageBox.Show("请输入视觉系统IP", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            isVisionConnected = true;
            UpdateVisionUI(true);
            visionStatusTimer.Start();
            MessageBox.Show("视觉系统连接成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDisconnect_Vision_Click(object sender, EventArgs e)
        {
            isVisionConnected = false;
            UpdateVisionUI(false);
            visionStatusTimer.Stop();

            if (currentImage != null)
            {
                currentImage.Dispose();
                currentImage = null;
                pictureBox1.Image = null;
            }
        }

        private void UpdateVisionUI(bool connected)
        {
            lblConnectionStatus_Vision.Text = connected ? "连接状态: 已连接" : "连接状态: 未连接";
            lblConnectionStatus_Vision.ForeColor = connected ? Color.Green : Color.Red;
            btnConnect_Vision.Enabled = !connected;
            btnDisconnect_Vision.Enabled = connected;
            btnCapture.Enabled = connected;
            btnAnalyze.Enabled = connected;
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            if (!isVisionConnected)
            {
                MessageBox.Show("请先连接视觉系统", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                currentImage = GenerateTestImage();
                pictureBox1.Image = currentImage;
                lblImageStatus.Text = "图像状态: 已采集";
                btnAnalyze.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"图像采集失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            if (!isVisionConnected)
            {
                MessageBox.Show("请先连接视觉系统", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (currentImage == null)
            {
                MessageBox.Show("请先采集图像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            txtAnalysisResult.Text = "分析结果:\r\n" +
                "- 检测到目标: 是\r\n" +
                "- 目标位置: X: 120, Y: 85\r\n" +
                "- 目标尺寸: 25x30mm\r\n" +
                "- 质量评估: 良好\r\n" +
                "- 分析时间: 120ms";

            lblAnalysisStatus.Text = "分析状态: 已完成";
        }

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            if (currentImage == null)
            {
                MessageBox.Show("请先采集图像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPEG文件 (*.jpg)|*.jpg|PNG文件 (*.png)|*.png|BMP文件 (*.bmp)|*.bmp";
            saveFileDialog.Title = "保存图像";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filePath = saveFileDialog.FileName;
                    ImageFormat format;

                    if (filePath.EndsWith(".jpg"))
                        format = ImageFormat.Jpeg;
                    else if (filePath.EndsWith(".png"))
                        format = ImageFormat.Png;
                    else
                        format = ImageFormat.Bmp;

                    currentImage.Save(filePath, format);
                    MessageBox.Show("图像保存成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"图像保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private Image GenerateTestImage()
        {
            Bitmap bitmap = new Bitmap(320, 240);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.DrawRectangle(Pens.Black, 100, 70, 50, 60);
                g.DrawString("Test Image", new Font("Arial", 12), Brushes.Black, 10, 10);
            }
            return bitmap;
        }

        // ===== 上料机构 =====

        private void UpUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (isProductionRunning)
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                lblElapsedTime.Text = $"运行时间: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

                double uph = CalculateUPH();
                lblUPH.Text = $"UPH: {uph:F2}";
            }
        }

        private double CalculateUPH()
        {
            if (!isProductionRunning || productionCount == 0)
                return 0;

            TimeSpan elapsed = DateTime.Now - startTime;
            return elapsed.TotalHours > 0 ? productionCount / elapsed.TotalHours : 0;
        }

        private void btnStartProduction_Click(object sender, EventArgs e)
        {
            if (!isProductionRunning)
            {
                isProductionRunning = true;
                startTime = DateTime.Now;
                upUpdateTimer.Start();
                btnStartProduction.Enabled = false;
                btnStopProduction.Enabled = true;
                btnAddProduct.Enabled = true;
            }
        }

        private void btnStopProduction_Click(object sender, EventArgs e)
        {
            if (isProductionRunning)
            {
                isProductionRunning = false;
                upUpdateTimer.Stop();
                btnStartProduction.Enabled = true;
                btnStopProduction.Enabled = false;
                btnAddProduct.Enabled = false;
            }
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            productionCount++;
            lblProductionCount.Text = $"生产数量: {productionCount}";
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            productionCount = 0;
            startTime = DateTime.Now;
            isProductionRunning = false;
            upUpdateTimer.Stop();

            lblProductionCount.Text = "生产数量: 0";
            lblElapsedTime.Text = "运行时间: 00:00:00";
            lblUPH.Text = "UPH: 0.00";

            btnStartProduction.Enabled = true;
            btnStopProduction.Enabled = false;
            btnAddProduct.Enabled = false;
        }

        private void EquipmentControlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isVisionConnected)
                btnDisconnect_Vision_Click(sender, e);
            visionStatusTimer.Stop();
            visionStatusTimer.Dispose();

            upUpdateTimer.Stop();
            upUpdateTimer.Dispose();

            if (currentImage != null)
            {
                currentImage.Dispose();
                currentImage = null;
            }
        }
    }
}
