using System.Drawing;
using System.Windows.Forms;

namespace OmniFrame
{
    partial class BlockCutLogForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel toolbar;
        private CheckBox _chkAutoScroll;
        private CheckBox _chkDebug;
        private CheckBox _chkInfo;
        private CheckBox _chkWarning;
        private CheckBox _chkError;
        private Button _btnClear;
        private Label _lblCount;
        private ListView _logListView;

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
            this.toolbar = new System.Windows.Forms.Panel();
            this._chkAutoScroll = new System.Windows.Forms.CheckBox();
            this._chkDebug = new System.Windows.Forms.CheckBox();
            this._chkInfo = new System.Windows.Forms.CheckBox();
            this._chkWarning = new System.Windows.Forms.CheckBox();
            this._chkError = new System.Windows.Forms.CheckBox();
            this._btnClear = new System.Windows.Forms.Button();
            this._lblCount = new System.Windows.Forms.Label();
            this._logListView = new System.Windows.Forms.ListView();
            this.toolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolbar
            // 
            this.toolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.toolbar.Controls.Add(this._chkAutoScroll);
            this.toolbar.Controls.Add(this._chkDebug);
            this.toolbar.Controls.Add(this._chkInfo);
            this.toolbar.Controls.Add(this._chkWarning);
            this.toolbar.Controls.Add(this._chkError);
            this.toolbar.Controls.Add(this._btnClear);
            this.toolbar.Controls.Add(this._lblCount);
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbar.Location = new System.Drawing.Point(0, 0);
            this.toolbar.Name = "toolbar";
            this.toolbar.Size = new System.Drawing.Size(931, 32);
            this.toolbar.TabIndex = 1;
            // 
            // _chkAutoScroll
            // 
            this._chkAutoScroll.AutoSize = true;
            this._chkAutoScroll.Checked = true;
            this._chkAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkAutoScroll.ForeColor = System.Drawing.Color.White;
            this._chkAutoScroll.Location = new System.Drawing.Point(8, 6);
            this._chkAutoScroll.Name = "_chkAutoScroll";
            this._chkAutoScroll.Size = new System.Drawing.Size(148, 32);
            this._chkAutoScroll.TabIndex = 0;
            this._chkAutoScroll.Text = "自动滚动";
            // 
            // _chkDebug
            // 
            this._chkDebug.AutoSize = true;
            this._chkDebug.Checked = true;
            this._chkDebug.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkDebug.ForeColor = System.Drawing.Color.Gray;
            this._chkDebug.Location = new System.Drawing.Point(100, 6);
            this._chkDebug.Name = "_chkDebug";
            this._chkDebug.Size = new System.Drawing.Size(109, 32);
            this._chkDebug.TabIndex = 1;
            this._chkDebug.Text = "Debug";
            // 
            // _chkInfo
            // 
            this._chkInfo.AutoSize = true;
            this._chkInfo.Checked = true;
            this._chkInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkInfo.ForeColor = System.Drawing.Color.Lime;
            this._chkInfo.Location = new System.Drawing.Point(160, 6);
            this._chkInfo.Name = "_chkInfo";
            this._chkInfo.Size = new System.Drawing.Size(96, 32);
            this._chkInfo.TabIndex = 2;
            this._chkInfo.Text = "Info";
            // 
            // _chkWarning
            // 
            this._chkWarning.AutoSize = true;
            this._chkWarning.Checked = true;
            this._chkWarning.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkWarning.ForeColor = System.Drawing.Color.Yellow;
            this._chkWarning.Location = new System.Drawing.Point(210, 6);
            this._chkWarning.Name = "_chkWarning";
            this._chkWarning.Size = new System.Drawing.Size(96, 32);
            this._chkWarning.TabIndex = 3;
            this._chkWarning.Text = "Warn";
            // 
            // _chkError
            // 
            this._chkError.AutoSize = true;
            this._chkError.Checked = true;
            this._chkError.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkError.ForeColor = System.Drawing.Color.Red;
            this._chkError.Location = new System.Drawing.Point(266, 6);
            this._chkError.Name = "_chkError";
            this._chkError.Size = new System.Drawing.Size(109, 32);
            this._chkError.TabIndex = 4;
            this._chkError.Text = "Error";
            // 
            // _btnClear
            // 
            this._btnClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this._btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnClear.Font = new System.Drawing.Font("微软雅黑", 8F);
            this._btnClear.ForeColor = System.Drawing.Color.White;
            this._btnClear.Location = new System.Drawing.Point(320, 4);
            this._btnClear.Name = "_btnClear";
            this._btnClear.Size = new System.Drawing.Size(50, 24);
            this._btnClear.TabIndex = 5;
            this._btnClear.Text = "清空";
            this._btnClear.UseVisualStyleBackColor = false;
            // 
            // _lblCount
            // 
            this._lblCount.AutoSize = true;
            this._lblCount.ForeColor = System.Drawing.Color.Gray;
            this._lblCount.Location = new System.Drawing.Point(380, 8);
            this._lblCount.Name = "_lblCount";
            this._lblCount.Size = new System.Drawing.Size(64, 28);
            this._lblCount.TabIndex = 6;
            this._lblCount.Text = "0 条";
            // 
            // _logListView
            // 
            this._logListView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this._logListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._logListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._logListView.Font = new System.Drawing.Font("Consolas", 9F);
            this._logListView.ForeColor = System.Drawing.Color.White;
            this._logListView.FullRowSelect = true;
            this._logListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._logListView.HideSelection = false;
            this._logListView.Location = new System.Drawing.Point(0, 32);
            this._logListView.Name = "_logListView";
            this._logListView.Size = new System.Drawing.Size(931, 678);
            this._logListView.TabIndex = 0;
            this._logListView.UseCompatibleStateImageBehavior = false;
            this._logListView.View = System.Windows.Forms.View.Details;
            // 
            // BlockCutLogForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(931, 710);
            this.Controls.Add(this._logListView);
            this.Controls.Add(this.toolbar);
            this.Font = new System.Drawing.Font("Consolas", 9F);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "BlockCutLogForm";
            this.Text = "实时日志 — 全系统";
            this.toolbar.ResumeLayout(false);
            this.toolbar.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
