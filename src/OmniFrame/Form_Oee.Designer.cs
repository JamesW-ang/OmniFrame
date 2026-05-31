namespace OmniFrame
{
    partial class Form_Oee
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnCalculateOee = new System.Windows.Forms.Button();
            this.txtOeeValue = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnRecordBad = new System.Windows.Forms.Button();
            this.btnRecordGood = new System.Windows.Forms.Button();
            this.btnStopProduction = new System.Windows.Forms.Button();
            this.btnStartProduction = new System.Windows.Forms.Button();
            this.dtpEndTime = new System.Windows.Forms.DateTimePicker();
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.txtLineName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnRecordDowntime = new System.Windows.Forms.Button();
            this.txtDowntimeReason = new System.Windows.Forms.TextBox();
            this.txtDowntimeMinutes = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnCalculateOee);
            this.groupBox1.Controls.Add(this.txtOeeValue);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.btnRecordBad);
            this.groupBox1.Controls.Add(this.btnRecordGood);
            this.groupBox1.Controls.Add(this.btnStopProduction);
            this.groupBox1.Controls.Add(this.btnStartProduction);
            this.groupBox1.Controls.Add(this.dtpEndTime);
            this.groupBox1.Controls.Add(this.dtpStartTime);
            this.groupBox1.Controls.Add(this.txtLineName);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(460, 200);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "OEE 管理";
            // 
            // btnCalculateOee
            // 
            this.btnCalculateOee.Location = new System.Drawing.Point(300, 160);
            this.btnCalculateOee.Name = "btnCalculateOee";
            this.btnCalculateOee.Size = new System.Drawing.Size(120, 30);
            this.btnCalculateOee.TabIndex = 12;
            this.btnCalculateOee.Text = "计算 OEE";
            this.btnCalculateOee.UseVisualStyleBackColor = true;
            this.btnCalculateOee.Click += new System.EventHandler(this.btnCalculateOee_Click);
            // 
            // txtOeeValue
            // 
            this.txtOeeValue.Location = new System.Drawing.Point(100, 165);
            this.txtOeeValue.Name = "txtOeeValue";
            this.txtOeeValue.Size = new System.Drawing.Size(150, 21);
            this.txtOeeValue.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 170);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 10;
            this.label6.Text = "OEE 值：";
            // 
            // btnRecordBad
            // 
            this.btnRecordBad.Location = new System.Drawing.Point(300, 120);
            this.btnRecordBad.Name = "btnRecordBad";
            this.btnRecordBad.Size = new System.Drawing.Size(120, 30);
            this.btnRecordBad.TabIndex = 9;
            this.btnRecordBad.Text = "记录不良品";
            this.btnRecordBad.UseVisualStyleBackColor = true;
            this.btnRecordBad.Click += new System.EventHandler(this.btnRecordBad_Click);
            // 
            // btnRecordGood
            // 
            this.btnRecordGood.Location = new System.Drawing.Point(170, 120);
            this.btnRecordGood.Name = "btnRecordGood";
            this.btnRecordGood.Size = new System.Drawing.Size(120, 30);
            this.btnRecordGood.TabIndex = 8;
            this.btnRecordGood.Text = "记录良品";
            this.btnRecordGood.UseVisualStyleBackColor = true;
            this.btnRecordGood.Click += new System.EventHandler(this.btnRecordGood_Click);
            // 
            // btnStopProduction
            // 
            this.btnStopProduction.Location = new System.Drawing.Point(300, 80);
            this.btnStopProduction.Name = "btnStopProduction";
            this.btnStopProduction.Size = new System.Drawing.Size(120, 30);
            this.btnStopProduction.TabIndex = 7;
            this.btnStopProduction.Text = "停止生产";
            this.btnStopProduction.UseVisualStyleBackColor = true;
            this.btnStopProduction.Click += new System.EventHandler(this.btnStopProduction_Click);
            // 
            // btnStartProduction
            // 
            this.btnStartProduction.Location = new System.Drawing.Point(170, 80);
            this.btnStartProduction.Name = "btnStartProduction";
            this.btnStartProduction.Size = new System.Drawing.Size(120, 30);
            this.btnStartProduction.TabIndex = 6;
            this.btnStartProduction.Text = "开始生产";
            this.btnStartProduction.UseVisualStyleBackColor = true;
            this.btnStartProduction.Click += new System.EventHandler(this.btnStartProduction_Click);
            // 
            // dtpEndTime
            // 
            this.dtpEndTime.Location = new System.Drawing.Point(300, 40);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.Size = new System.Drawing.Size(120, 21);
            this.dtpEndTime.TabIndex = 5;
            // 
            // dtpStartTime
            // 
            this.dtpStartTime.Location = new System.Drawing.Point(144, 40);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.Size = new System.Drawing.Size(120, 21);
            this.dtpStartTime.TabIndex = 4;
            // 
            // txtLineName
            // 
            this.txtLineName.Location = new System.Drawing.Point(100, 10);
            this.txtLineName.Name = "txtLineName";
            this.txtLineName.Size = new System.Drawing.Size(320, 21);
            this.txtLineName.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(270, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "结束";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(108, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 1;
            this.label4.Text = "开始";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "生产线：";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnRecordDowntime);
            this.groupBox2.Controls.Add(this.txtDowntimeReason);
            this.groupBox2.Controls.Add(this.txtDowntimeMinutes);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 218);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(460, 100);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "停机时间记录";
            // 
            // btnRecordDowntime
            // 
            this.btnRecordDowntime.Location = new System.Drawing.Point(300, 60);
            this.btnRecordDowntime.Name = "btnRecordDowntime";
            this.btnRecordDowntime.Size = new System.Drawing.Size(120, 30);
            this.btnRecordDowntime.TabIndex = 4;
            this.btnRecordDowntime.Text = "记录停机";
            this.btnRecordDowntime.UseVisualStyleBackColor = true;
            this.btnRecordDowntime.Click += new System.EventHandler(this.btnRecordDowntime_Click);
            // 
            // txtDowntimeReason
            // 
            this.txtDowntimeReason.Location = new System.Drawing.Point(100, 65);
            this.txtDowntimeReason.Name = "txtDowntimeReason";
            this.txtDowntimeReason.Size = new System.Drawing.Size(180, 21);
            this.txtDowntimeReason.TabIndex = 3;
            // 
            // txtDowntimeMinutes
            // 
            this.txtDowntimeMinutes.Location = new System.Drawing.Point(100, 30);
            this.txtDowntimeMinutes.Name = "txtDowntimeMinutes";
            this.txtDowntimeMinutes.Size = new System.Drawing.Size(100, 21);
            this.txtDowntimeMinutes.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "停机原因：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "停机分钟：";
            // 
            // Form_Oee
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 389);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form_Oee";
            this.Text = "OEE 管理";
            this.Load += new System.EventHandler(this.Form_Oee_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnCalculateOee;
        private System.Windows.Forms.TextBox txtOeeValue;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnRecordBad;
        private System.Windows.Forms.Button btnRecordGood;
        private System.Windows.Forms.Button btnStopProduction;
        private System.Windows.Forms.Button btnStartProduction;
        private System.Windows.Forms.DateTimePicker dtpEndTime;
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private System.Windows.Forms.TextBox txtLineName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnRecordDowntime;
        private System.Windows.Forms.TextBox txtDowntimeReason;
        private System.Windows.Forms.TextBox txtDowntimeMinutes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}