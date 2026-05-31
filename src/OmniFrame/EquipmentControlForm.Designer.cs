using OmniFrame.Theme;

namespace OmniFrame
{
    partial class EquipmentControlForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listBoxDevices = new System.Windows.Forms.ListBox();
            this.panelControl = new System.Windows.Forms.Panel();
            // 视觉检测面板
            this.panelVision = new System.Windows.Forms.Panel();
            this.btnSaveImage = new System.Windows.Forms.Button();
            this.txtAnalysisResult = new System.Windows.Forms.TextBox();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.btnCapture = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnDisconnect_Vision = new System.Windows.Forms.Button();
            this.btnConnect_Vision = new System.Windows.Forms.Button();
            this.txtVisionIp = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblAnalysisStatus = new System.Windows.Forms.Label();
            this.lblImageStatus = new System.Windows.Forms.Label();
            this.lblConnectionStatus_Vision = new System.Windows.Forms.Label();
            // 上料机构面板
            this.panelUp = new System.Windows.Forms.Panel();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnAddProduct = new System.Windows.Forms.Button();
            this.btnStopProduction = new System.Windows.Forms.Button();
            this.btnStartProduction = new System.Windows.Forms.Button();
            this.lblUPH = new System.Windows.Forms.Label();
            this.lblElapsedTime = new System.Windows.Forms.Label();
            this.lblProductionCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelVision.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelUp.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listBoxDevices);
            this.splitContainer1.Panel1MinSize = 180;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelControl);
            this.splitContainer1.Size = new System.Drawing.Size(800, 600);
            this.splitContainer1.SplitterDistance = 180;
            this.splitContainer1.TabIndex = 0;
            // 
            // listBoxDevices
            // 
            this.listBoxDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxDevices.FormattingEnabled = true;
            this.listBoxDevices.Items.AddRange(new object[] {
            "视觉检测",
            "上料机构"});
            this.listBoxDevices.Location = new System.Drawing.Point(0, 0);
            this.listBoxDevices.Name = "listBoxDevices";
            this.listBoxDevices.Size = new System.Drawing.Size(150, 600);
            this.listBoxDevices.TabIndex = 0;
            this.listBoxDevices.SelectedIndexChanged += new System.EventHandler(this.listBoxDevices_SelectedIndexChanged);
            // 
            // panelControl
            // 
            this.panelControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl.Location = new System.Drawing.Point(0, 0);
            this.panelControl.Name = "panelControl";
            this.panelControl.Size = new System.Drawing.Size(646, 600);
            this.panelControl.TabIndex = 0;
            // 
            // panelVision
            // 
            this.panelVision.Controls.Add(this.btnSaveImage);
            this.panelVision.Controls.Add(this.txtAnalysisResult);
            this.panelVision.Controls.Add(this.btnAnalyze);
            this.panelVision.Controls.Add(this.btnCapture);
            this.panelVision.Controls.Add(this.pictureBox1);
            this.panelVision.Controls.Add(this.btnDisconnect_Vision);
            this.panelVision.Controls.Add(this.btnConnect_Vision);
            this.panelVision.Controls.Add(this.txtVisionIp);
            this.panelVision.Controls.Add(this.label2);
            this.panelVision.Controls.Add(this.lblAnalysisStatus);
            this.panelVision.Controls.Add(this.lblImageStatus);
            this.panelVision.Controls.Add(this.lblConnectionStatus_Vision);
            this.panelVision.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelVision.Location = new System.Drawing.Point(0, 0);
            this.panelVision.Name = "panelVision";
            this.panelVision.Size = new System.Drawing.Size(646, 600);
            this.panelVision.TabIndex = 0;
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.Location = new System.Drawing.Point(350, 450);
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.Size = new System.Drawing.Size(80, 30);
            this.btnSaveImage.TabIndex = 11;
            this.btnSaveImage.Text = "保存图像";
            this.btnSaveImage.UseVisualStyleBackColor = true;

            this.btnSaveImage.Click += new System.EventHandler(this.btnSaveImage_Click);
            // 
            // txtAnalysisResult
            // 
            this.txtAnalysisResult.Location = new System.Drawing.Point(350, 200);
            this.txtAnalysisResult.Multiline = true;
            this.txtAnalysisResult.Name = "txtAnalysisResult";
            this.txtAnalysisResult.Size = new System.Drawing.Size(250, 200);
            this.txtAnalysisResult.TabIndex = 10;
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.Enabled = false;
            this.btnAnalyze.Location = new System.Drawing.Point(250, 450);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(80, 30);
            this.btnAnalyze.TabIndex = 9;
            this.btnAnalyze.Text = "分析图像";
            this.btnAnalyze.UseVisualStyleBackColor = true;

            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // btnCapture
            // 
            this.btnCapture.Enabled = false;
            this.btnCapture.Location = new System.Drawing.Point(150, 450);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(80, 30);
            this.btnCapture.TabIndex = 8;
            this.btnCapture.Text = "采集图像";
            this.btnCapture.UseVisualStyleBackColor = true;

            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(50, 100);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(280, 300);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // btnDisconnect_Vision
            // 
            this.btnDisconnect_Vision.Enabled = false;
            this.btnDisconnect_Vision.Location = new System.Drawing.Point(300, 50);
            this.btnDisconnect_Vision.Name = "btnDisconnect_Vision";
            this.btnDisconnect_Vision.Size = new System.Drawing.Size(80, 30);
            this.btnDisconnect_Vision.TabIndex = 6;
            this.btnDisconnect_Vision.Text = "断开连接";
            this.btnDisconnect_Vision.UseVisualStyleBackColor = true;

            this.btnDisconnect_Vision.Click += new System.EventHandler(this.btnDisconnect_Vision_Click);
            // 
            // btnConnect_Vision
            // 
            this.btnConnect_Vision.Location = new System.Drawing.Point(200, 50);
            this.btnConnect_Vision.Name = "btnConnect_Vision";
            this.btnConnect_Vision.Size = new System.Drawing.Size(80, 30);
            this.btnConnect_Vision.TabIndex = 5;
            this.btnConnect_Vision.Text = "连接";
            this.btnConnect_Vision.UseVisualStyleBackColor = true;

            this.btnConnect_Vision.Click += new System.EventHandler(this.btnConnect_Vision_Click);
            // 
            // txtVisionIp
            // 
            this.txtVisionIp.Location = new System.Drawing.Point(80, 55);
            this.txtVisionIp.Name = "txtVisionIp";
            this.txtVisionIp.Size = new System.Drawing.Size(100, 20);
            this.txtVisionIp.TabIndex = 4;
            this.txtVisionIp.Text = "192.168.1.100";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "视觉IP:";
            // 
            // lblAnalysisStatus
            // 
            this.lblAnalysisStatus.AutoSize = true;
            this.lblAnalysisStatus.Location = new System.Drawing.Point(350, 170);
            this.lblAnalysisStatus.Name = "lblAnalysisStatus";
            this.lblAnalysisStatus.Size = new System.Drawing.Size(71, 13);
            this.lblAnalysisStatus.TabIndex = 2;
            this.lblAnalysisStatus.Text = "分析状态: 未分析";
            // 
            // lblImageStatus
            // 
            this.lblImageStatus.AutoSize = true;
            this.lblImageStatus.Location = new System.Drawing.Point(50, 410);
            this.lblImageStatus.Name = "lblImageStatus";
            this.lblImageStatus.Size = new System.Drawing.Size(71, 13);
            this.lblImageStatus.TabIndex = 1;
            this.lblImageStatus.Text = "图像状态: 未采集";
            // 
            // lblConnectionStatus_Vision
            // 
            this.lblConnectionStatus_Vision.AutoSize = true;
            this.lblConnectionStatus_Vision.Location = new System.Drawing.Point(20, 20);
            this.lblConnectionStatus_Vision.Name = "lblConnectionStatus_Vision";
            this.lblConnectionStatus_Vision.Size = new System.Drawing.Size(77, 13);
            this.lblConnectionStatus_Vision.TabIndex = 0;
            this.lblConnectionStatus_Vision.Text = "连接状态: 未连接";
            // 
            // panelUp
            // 
            this.panelUp.Controls.Add(this.btnReset);
            this.panelUp.Controls.Add(this.btnAddProduct);
            this.panelUp.Controls.Add(this.btnStopProduction);
            this.panelUp.Controls.Add(this.btnStartProduction);
            this.panelUp.Controls.Add(this.lblUPH);
            this.panelUp.Controls.Add(this.lblElapsedTime);
            this.panelUp.Controls.Add(this.lblProductionCount);
            this.panelUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelUp.Location = new System.Drawing.Point(0, 0);
            this.panelUp.Name = "panelUp";
            this.panelUp.Size = new System.Drawing.Size(646, 600);
            this.panelUp.TabIndex = 1;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(350, 200);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(80, 30);
            this.btnReset.TabIndex = 6;
            this.btnReset.Text = "重置";
            this.btnReset.UseVisualStyleBackColor = true;

            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnAddProduct
            // 
            this.btnAddProduct.Enabled = false;
            this.btnAddProduct.Location = new System.Drawing.Point(250, 200);
            this.btnAddProduct.Name = "btnAddProduct";
            this.btnAddProduct.Size = new System.Drawing.Size(80, 30);
            this.btnAddProduct.TabIndex = 5;
            this.btnAddProduct.Text = "增加产品";
            this.btnAddProduct.UseVisualStyleBackColor = true;

            this.btnAddProduct.Click += new System.EventHandler(this.btnAddProduct_Click);
            // 
            // btnStopProduction
            // 
            this.btnStopProduction.Enabled = false;
            this.btnStopProduction.Location = new System.Drawing.Point(150, 200);
            this.btnStopProduction.Name = "btnStopProduction";
            this.btnStopProduction.Size = new System.Drawing.Size(80, 30);
            this.btnStopProduction.TabIndex = 4;
            this.btnStopProduction.Text = "停止生产";
            this.btnStopProduction.UseVisualStyleBackColor = true;

            this.btnStopProduction.Click += new System.EventHandler(this.btnStopProduction_Click);
            // 
            // btnStartProduction
            // 
            this.btnStartProduction.Location = new System.Drawing.Point(50, 200);
            this.btnStartProduction.Name = "btnStartProduction";
            this.btnStartProduction.Size = new System.Drawing.Size(80, 30);
            this.btnStartProduction.TabIndex = 3;
            this.btnStartProduction.Text = "开始生产";
            this.btnStartProduction.UseVisualStyleBackColor = true;

            this.btnStartProduction.Click += new System.EventHandler(this.btnStartProduction_Click);
            // 
            // lblUPH
            // 
            this.lblUPH.AutoSize = true;
            this.lblUPH.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUPH.Location = new System.Drawing.Point(50, 150);
            this.lblUPH.Name = "lblUPH";
            this.lblUPH.Size = new System.Drawing.Size(71, 20);
            this.lblUPH.TabIndex = 2;
            this.lblUPH.Text = "UPH: 0";
            // 
            // lblElapsedTime
            // 
            this.lblElapsedTime.AutoSize = true;
            this.lblElapsedTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblElapsedTime.Location = new System.Drawing.Point(50, 100);
            this.lblElapsedTime.Name = "lblElapsedTime";
            this.lblElapsedTime.Size = new System.Drawing.Size(136, 20);
            this.lblElapsedTime.TabIndex = 1;
            this.lblElapsedTime.Text = "运行时间: 00:00:00";
            // 
            // lblProductionCount
            // 
            this.lblProductionCount.AutoSize = true;
            this.lblProductionCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProductionCount.Location = new System.Drawing.Point(50, 50);
            this.lblProductionCount.Name = "lblProductionCount";
            this.lblProductionCount.Size = new System.Drawing.Size(97, 20);
            this.lblProductionCount.TabIndex = 0;
            this.lblProductionCount.Text = "生产数量: 0";
            //
            // EquipmentControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "EquipmentControlForm";
            this.Text = "设备控制面板";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EquipmentControlForm_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelVision.ResumeLayout(false);
            this.panelVision.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelUp.ResumeLayout(false);
            this.panelUp.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox listBoxDevices;
        private System.Windows.Forms.Panel panelControl;
        private System.Windows.Forms.Panel panelVision;
        private System.Windows.Forms.Button btnSaveImage;
        private System.Windows.Forms.TextBox txtAnalysisResult;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnDisconnect_Vision;
        private System.Windows.Forms.Button btnConnect_Vision;
        private System.Windows.Forms.TextBox txtVisionIp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblAnalysisStatus;
        private System.Windows.Forms.Label lblImageStatus;
        private System.Windows.Forms.Label lblConnectionStatus_Vision;
        private System.Windows.Forms.Panel panelUp;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnAddProduct;
        private System.Windows.Forms.Button btnStopProduction;
        private System.Windows.Forms.Button btnStartProduction;
        private System.Windows.Forms.Label lblUPH;
        private System.Windows.Forms.Label lblElapsedTime;
        private System.Windows.Forms.Label lblProductionCount;
    }
}
