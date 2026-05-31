using OmniFrame.Theme;

namespace OmniFrame
{
    partial class PluginManagerForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.button_Refresh = new System.Windows.Forms.Button();
            this.button_Load = new System.Windows.Forms.Button();
            this.button_Unload = new System.Windows.Forms.Button();
            this.button_Update = new System.Windows.Forms.Button();
            this.button_Rollback = new System.Windows.Forms.Button();
            this.label_Status = new System.Windows.Forms.Label();
            this.dataGridView_Plugins = new System.Windows.Forms.DataGridView();
            this.pluginName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pluginTypeCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Version = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsLoaded = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsOfficial = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Path = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Plugins)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridView_Plugins, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 500);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(794, 54);
            this.panel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.button_Refresh, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_Load, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_Unload, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_Update, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_Rollback, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.label_Status, 5, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(794, 54);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // button_Refresh
            // 
            this.button_Refresh.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.button_Refresh.Location = new System.Drawing.Point(3, 9);
            this.button_Refresh.Name = "button_Refresh";
            this.button_Refresh.Size = new System.Drawing.Size(121, 36);
            this.button_Refresh.TabIndex = 0;
            this.button_Refresh.Text = "刷新插件列表";
            this.button_Refresh.UseVisualStyleBackColor = true;
            this.button_Refresh.Click += new System.EventHandler(this.button_Refresh_Click);
            // 
            // button_Load
            // 
            this.button_Load.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.button_Load.Location = new System.Drawing.Point(133, 9);
            this.button_Load.Name = "button_Load";
            this.button_Load.Size = new System.Drawing.Size(75, 36);
            this.button_Load.TabIndex = 1;
            this.button_Load.Text = "加载";
            this.button_Load.UseVisualStyleBackColor = true;
            this.button_Load.Click += new System.EventHandler(this.button_Load_Click);
            // 
            // button_Unload
            // 
            this.button_Unload.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.button_Unload.Location = new System.Drawing.Point(223, 9);
            this.button_Unload.Name = "button_Unload";
            this.button_Unload.Size = new System.Drawing.Size(75, 36);
            this.button_Unload.TabIndex = 2;
            this.button_Unload.Text = "卸载";
            this.button_Unload.UseVisualStyleBackColor = true;
            this.button_Unload.Click += new System.EventHandler(this.button_Unload_Click);
            // 
            // button_Update
            // 
            this.button_Update.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.button_Update.Location = new System.Drawing.Point(313, 9);
            this.button_Update.Name = "button_Update";
            this.button_Update.Size = new System.Drawing.Size(75, 36);
            this.button_Update.TabIndex = 3;
            this.button_Update.Text = "更新";
            this.button_Update.UseVisualStyleBackColor = true;
            this.button_Update.Click += new System.EventHandler(this.button_Update_Click);
            // 
            // button_Rollback
            // 
            this.button_Rollback.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.button_Rollback.Location = new System.Drawing.Point(403, 9);
            this.button_Rollback.Name = "button_Rollback";
            this.button_Rollback.Size = new System.Drawing.Size(75, 36);
            this.button_Rollback.TabIndex = 4;
            this.button_Rollback.Text = "回滚";
            this.button_Rollback.UseVisualStyleBackColor = true;
            this.button_Rollback.Click += new System.EventHandler(this.button_Rollback_Click);
            // 
            // label_Status
            // 
            this.label_Status.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label_Status.AutoSize = true;
            this.label_Status.Location = new System.Drawing.Point(493, 17);
            this.label_Status.Name = "label_Status";
            this.label_Status.Size = new System.Drawing.Size(0, 20);
            this.label_Status.TabIndex = 5;
            // 
            // dataGridView_Plugins
            // 
            this.dataGridView_Plugins.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Plugins.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pluginName,
            this.pluginTypeCol,
            this.Description,
            this.Version,
            this.IsLoaded,
            this.IsOfficial,
            this.Path});
            this.dataGridView_Plugins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Plugins.Location = new System.Drawing.Point(3, 63);
            this.dataGridView_Plugins.Name = "dataGridView_Plugins";
            this.dataGridView_Plugins.ReadOnly = true;
            this.dataGridView_Plugins.RowTemplate.Height = 23;
            this.dataGridView_Plugins.Size = new System.Drawing.Size(794, 434);
            this.dataGridView_Plugins.TabIndex = 1;
            this.dataGridView_Plugins.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_Plugins_CellDoubleClick);
            //
            // pluginName
            //
            this.pluginName.HeaderText = "插件名称";
            this.pluginName.Name = "Name";
            this.pluginName.ReadOnly = true;
            this.pluginName.Width = 120;
            //
            // pluginTypeCol
            //
            this.pluginTypeCol.HeaderText = "类型";
            this.pluginTypeCol.Name = "PluginType";
            this.pluginTypeCol.ReadOnly = true;
            this.pluginTypeCol.Width = 70;
            //
            // Description
            // 
            this.Description.HeaderText = "描述";
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.Width = 200;
            // 
            // Version
            // 
            this.Version.HeaderText = "版本";
            this.Version.Name = "Version";
            this.Version.ReadOnly = true;
            this.Version.Width = 80;
            // 
            // IsLoaded
            // 
            this.IsLoaded.HeaderText = "是否加载";
            this.IsLoaded.Name = "IsLoaded";
            this.IsLoaded.ReadOnly = true;
            this.IsLoaded.Width = 80;
            // 
            // IsOfficial
            // 
            this.IsOfficial.HeaderText = "是否官方";
            this.IsOfficial.Name = "IsOfficial";
            this.IsOfficial.ReadOnly = true;
            this.IsOfficial.Width = 80;
            // 
            // Path
            // 
            this.Path.HeaderText = "路径";
            this.Path.Name = "Path";
            this.Path.ReadOnly = true;
            this.Path.Width = 200;
            // 
            // PluginManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MinimumSize = new System.Drawing.Size(640, 400);
            this.Name = "PluginManagerForm";
            this.Text = "插件管理";
            this.Load += new System.EventHandler(this.PluginManagerForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Plugins)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button_Rollback;
        private System.Windows.Forms.Button button_Update;
        private System.Windows.Forms.Button button_Unload;
        private System.Windows.Forms.Button button_Load;
        private System.Windows.Forms.Button button_Refresh;
        private System.Windows.Forms.Label label_Status;
        private System.Windows.Forms.DataGridView dataGridView_Plugins;
        private System.Windows.Forms.DataGridViewTextBoxColumn pluginName;
        private System.Windows.Forms.DataGridViewTextBoxColumn pluginTypeCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
        private System.Windows.Forms.DataGridViewTextBoxColumn Version;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsLoaded;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsOfficial;
        private System.Windows.Forms.DataGridViewTextBoxColumn Path;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}