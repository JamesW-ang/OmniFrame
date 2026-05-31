namespace OmniFrame
{
    partial class BlockCutIOSignalForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.panelDi = new System.Windows.Forms.Panel();
            this._txtDiFilter = new System.Windows.Forms.TextBox();
            this._diTable = new System.Windows.Forms.ListView();
            this.panelDo = new System.Windows.Forms.Panel();
            this.btnAllDoOff = new System.Windows.Forms.Button();
            this.btnAllDoOn = new System.Windows.Forms.Button();
            this._txtDoFilter = new System.Windows.Forms.TextBox();
            this._doTable = new System.Windows.Forms.ListView();
            this.panelCylinder = new System.Windows.Forms.Panel();
            this._cylinderTable = new System.Windows.Forms.ListView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panelDi.SuspendLayout();
            this.panelDo.SuspendLayout();
            this.panelCylinder.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelCylinder);
            this.splitContainer1.Size = new System.Drawing.Size(900, 600);
            this.splitContainer1.SplitterDistance = 600;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.panelDi);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panelDo);
            this.splitContainer2.Size = new System.Drawing.Size(600, 600);
            this.splitContainer2.SplitterDistance = 300;
            this.splitContainer2.TabIndex = 0;
            // 
            // panelDi
            // 
            this.panelDi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panelDi.Controls.Add(this._txtDiFilter);
            this.panelDi.Controls.Add(this._diTable);
            this.panelDi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDi.Location = new System.Drawing.Point(0, 0);
            this.panelDi.Name = "panelDi";
            this.panelDi.Size = new System.Drawing.Size(600, 300);
            this.panelDi.TabIndex = 0;
            // 
            // _txtDiFilter
            // 
            this._txtDiFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this._txtDiFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtDiFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this._txtDiFilter.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this._txtDiFilter.ForeColor = System.Drawing.Color.White;
            this._txtDiFilter.Location = new System.Drawing.Point(0, 0);
            this._txtDiFilter.Name = "_txtDiFilter";
            this._txtDiFilter.Size = new System.Drawing.Size(600, 23);
            this._txtDiFilter.TabIndex = 1;
            // 
            // _diTable
            // 
            this._diTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this._diTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this._diTable.Font = new System.Drawing.Font("Consolas", 9F);
            this._diTable.ForeColor = System.Drawing.Color.White;
            this._diTable.FullRowSelect = true;
            this._diTable.GridLines = true;
            this._diTable.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._diTable.HideSelection = false;
            this._diTable.Location = new System.Drawing.Point(0, 23);
            this._diTable.Name = "_diTable";
            this._diTable.Scrollable = true;
            this._diTable.ShowItemToolTips = true;
            this._diTable.Size = new System.Drawing.Size(600, 277);
            this._diTable.TabIndex = 0;
            this._diTable.UseCompatibleStateImageBehavior = false;
            this._diTable.View = System.Windows.Forms.View.Details;
            // 
            // panelDo
            // 
            this.panelDo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panelDo.Controls.Add(this.btnAllDoOff);
            this.panelDo.Controls.Add(this.btnAllDoOn);
            this.panelDo.Controls.Add(this._txtDoFilter);
            this.panelDo.Controls.Add(this._doTable);
            this.panelDo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDo.Location = new System.Drawing.Point(0, 0);
            this.panelDo.Name = "panelDo";
            this.panelDo.Size = new System.Drawing.Size(600, 296);
            this.panelDo.TabIndex = 0;
            // 
            // btnAllDoOff
            // 
            this.btnAllDoOff.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnAllDoOff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAllDoOff.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.btnAllDoOff.ForeColor = System.Drawing.Color.White;
            this.btnAllDoOff.Location = new System.Drawing.Point(140, 26);
            this.btnAllDoOff.Name = "btnAllDoOff";
            this.btnAllDoOff.Size = new System.Drawing.Size(85, 25);
            this.btnAllDoOff.TabIndex = 3;
            this.btnAllDoOff.Text = "全部关闭";
            this.btnAllDoOff.UseVisualStyleBackColor = false;
            // 
            // btnAllDoOn
            // 
            this.btnAllDoOn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(0)))));
            this.btnAllDoOn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAllDoOn.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.btnAllDoOn.ForeColor = System.Drawing.Color.White;
            this.btnAllDoOn.Location = new System.Drawing.Point(25, 26);
            this.btnAllDoOn.Name = "btnAllDoOn";
            this.btnAllDoOn.Size = new System.Drawing.Size(85, 25);
            this.btnAllDoOn.TabIndex = 2;
            this.btnAllDoOn.Text = "全部打开";
            this.btnAllDoOn.UseVisualStyleBackColor = false;
            // 
            // _txtDoFilter
            // 
            this._txtDoFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this._txtDoFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtDoFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this._txtDoFilter.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this._txtDoFilter.ForeColor = System.Drawing.Color.White;
            this._txtDoFilter.Location = new System.Drawing.Point(0, 0);
            this._txtDoFilter.Name = "_txtDoFilter";
            this._txtDoFilter.Size = new System.Drawing.Size(600, 23);
            this._txtDoFilter.TabIndex = 1;
            // 
            // _doTable
            // 
            this._doTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this._doTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this._doTable.Font = new System.Drawing.Font("Consolas", 9F);
            this._doTable.ForeColor = System.Drawing.Color.White;
            this._doTable.FullRowSelect = true;
            this._doTable.GridLines = true;
            this._doTable.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._doTable.HideSelection = false;
            this._doTable.Location = new System.Drawing.Point(0, 54);
            this._doTable.Name = "_doTable";
            this._doTable.Scrollable = true;
            this._doTable.ShowItemToolTips = true;
            this._doTable.Size = new System.Drawing.Size(600, 242);
            this._doTable.TabIndex = 0;
            this._doTable.UseCompatibleStateImageBehavior = false;
            this._doTable.View = System.Windows.Forms.View.Details;
            // 
            // panelCylinder
            // 
            this.panelCylinder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panelCylinder.Controls.Add(this._cylinderTable);
            this.panelCylinder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCylinder.Location = new System.Drawing.Point(0, 0);
            this.panelCylinder.Name = "panelCylinder";
            this.panelCylinder.Size = new System.Drawing.Size(296, 600);
            this.panelCylinder.TabIndex = 0;
            // 
            // _cylinderTable
            // 
            this._cylinderTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this._cylinderTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cylinderTable.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this._cylinderTable.ForeColor = System.Drawing.Color.White;
            this._cylinderTable.FullRowSelect = true;
            this._cylinderTable.GridLines = true;
            this._cylinderTable.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._cylinderTable.HideSelection = false;
            this._cylinderTable.Location = new System.Drawing.Point(0, 0);
            this._cylinderTable.Name = "_cylinderTable";
            this._cylinderTable.Scrollable = true;
            this._cylinderTable.ShowItemToolTips = true;
            this._cylinderTable.Size = new System.Drawing.Size(296, 600);
            this._cylinderTable.TabIndex = 0;
            this._cylinderTable.UseCompatibleStateImageBehavior = false;
            this._cylinderTable.View = System.Windows.Forms.View.Details;
            // 
            // BlockCutIOSignalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.splitContainer1);
            this.Name = "BlockCutIOSignalForm";
            this.Text = "IO信号监控";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panelDi.ResumeLayout(false);
            this.panelDi.PerformLayout();
            this.panelDo.ResumeLayout(false);
            this.panelDo.PerformLayout();
            this.panelCylinder.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Panel panelDi;
        private System.Windows.Forms.ListView _diTable;
        private System.Windows.Forms.Panel panelDo;
        private System.Windows.Forms.ListView _doTable;
        private System.Windows.Forms.Panel panelCylinder;
        private System.Windows.Forms.ListView _cylinderTable;
        private System.Windows.Forms.TextBox _txtDiFilter;
        private System.Windows.Forms.TextBox _txtDoFilter;
        private System.Windows.Forms.Button btnAllDoOn;
        private System.Windows.Forms.Button btnAllDoOff;
    }
}
