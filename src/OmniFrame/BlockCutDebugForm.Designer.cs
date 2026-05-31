using System.Drawing;
using System.Windows.Forms;

namespace OmniFrame
{
    partial class BlockCutDebugForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel panel;
        private Label lblTitle;
        private Button btnWorkMgr;
        private Button btnCameraDebug;
        private Button btnFitLine;
        private Button btnMotionSet;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panel = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnWorkMgr = new System.Windows.Forms.Button();
            this.btnCameraDebug = new System.Windows.Forms.Button();
            this.btnFitLine = new System.Windows.Forms.Button();
            this.btnMotionSet = new System.Windows.Forms.Button();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.panel.Controls.Add(this.lblTitle);
            this.panel.Controls.Add(this.btnWorkMgr);
            this.panel.Controls.Add(this.btnCameraDebug);
            this.panel.Controls.Add(this.btnFitLine);
            this.panel.Controls.Add(this.btnMotionSet);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Padding = new System.Windows.Forms.Padding(16);
            this.panel.Size = new System.Drawing.Size(672, 532);
            this.panel.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 16);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(174, 50);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "调试工具";
            // 
            // btnWorkMgr
            // 
            this.btnWorkMgr.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(140)))));
            this.btnWorkMgr.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWorkMgr.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnWorkMgr.ForeColor = System.Drawing.Color.White;
            this.btnWorkMgr.Location = new System.Drawing.Point(16, 60);
            this.btnWorkMgr.Name = "btnWorkMgr";
            this.btnWorkMgr.Size = new System.Drawing.Size(200, 36);
            this.btnWorkMgr.TabIndex = 1;
            this.btnWorkMgr.Text = "工单管理";
            this.btnWorkMgr.UseVisualStyleBackColor = false;
            // 
            // btnCameraDebug
            // 
            this.btnCameraDebug.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(100)))));
            this.btnCameraDebug.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCameraDebug.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnCameraDebug.ForeColor = System.Drawing.Color.White;
            this.btnCameraDebug.Location = new System.Drawing.Point(16, 100);
            this.btnCameraDebug.Name = "btnCameraDebug";
            this.btnCameraDebug.Size = new System.Drawing.Size(200, 36);
            this.btnCameraDebug.TabIndex = 2;
            this.btnCameraDebug.Text = "相机调试";
            this.btnCameraDebug.UseVisualStyleBackColor = false;
            // 
            // btnFitLine
            // 
            this.btnFitLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(60)))), ((int)(((byte)(140)))));
            this.btnFitLine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFitLine.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnFitLine.ForeColor = System.Drawing.Color.White;
            this.btnFitLine.Location = new System.Drawing.Point(16, 140);
            this.btnFitLine.Name = "btnFitLine";
            this.btnFitLine.Size = new System.Drawing.Size(200, 36);
            this.btnFitLine.TabIndex = 3;
            this.btnFitLine.Text = "视觉标定 (FitLine)";
            this.btnFitLine.UseVisualStyleBackColor = false;
            // 
            // btnMotionSet
            // 
            this.btnMotionSet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(80)))), ((int)(((byte)(0)))));
            this.btnMotionSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMotionSet.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnMotionSet.ForeColor = System.Drawing.Color.White;
            this.btnMotionSet.Location = new System.Drawing.Point(16, 180);
            this.btnMotionSet.Name = "btnMotionSet";
            this.btnMotionSet.Size = new System.Drawing.Size(200, 36);
            this.btnMotionSet.TabIndex = 4;
            this.btnMotionSet.Text = "运动参数设置";
            this.btnMotionSet.UseVisualStyleBackColor = false;
            // 
            // BlockCutDebugForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.ClientSize = new System.Drawing.Size(672, 532);
            this.Controls.Add(this.panel);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "BlockCutDebugForm";
            this.Text = "BlockCut 调试工具";
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
