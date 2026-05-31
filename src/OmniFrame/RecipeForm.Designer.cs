using OmniFrame.Theme;

namespace OmniFrame
{
    partial class RecipeForm
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

        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel_Left = new System.Windows.Forms.Panel();
            this.groupBoxRecipeList = new System.Windows.Forms.GroupBox();
            this.listBoxRecipes = new System.Windows.Forms.ListBox();
            this.panel_Bottom = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.groupBoxParams = new System.Windows.Forms.GroupBox();
            this.dataGridViewParams = new System.Windows.Forms.DataGridView();
            this.columnParamName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnParamValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnParamDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBoxRecipeInfo = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelRecipeName = new System.Windows.Forms.Label();
            this.textBoxRecipeName = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.labelProductModel = new System.Windows.Forms.Label();
            this.textBoxProductModel = new System.Windows.Forms.TextBox();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelCreateTime = new System.Windows.Forms.Label();
            this.labelAuthor = new System.Windows.Forms.Label();
            this.panel_Right = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel_Left.SuspendLayout();
            this.groupBoxRecipeList.SuspendLayout();
            this.panel_Bottom.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBoxParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParams)).BeginInit();
            this.groupBoxRecipeInfo.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.panel_Left);
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel_Bottom);
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxParams);
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxRecipeInfo);
            this.splitContainer1.Size = new System.Drawing.Size(784, 520);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.TabIndex = 0;
            // 
            // panel_Left
            // 
            this.panel_Left.Controls.Add(this.groupBoxRecipeList);
            this.panel_Left.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Left.Location = new System.Drawing.Point(0, 0);
            this.panel_Left.Name = "panel_Left";
            this.panel_Left.Size = new System.Drawing.Size(200, 520);
            this.panel_Left.TabIndex = 0;
            // 
            // groupBoxRecipeList
            // 
            this.groupBoxRecipeList.Controls.Add(this.listBoxRecipes);
            this.groupBoxRecipeList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxRecipeList.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxRecipeList.Location = new System.Drawing.Point(0, 0);
            this.groupBoxRecipeList.Margin = new System.Windows.Forms.Padding(8);
            this.groupBoxRecipeList.Name = "groupBoxRecipeList";
            this.groupBoxRecipeList.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxRecipeList.Size = new System.Drawing.Size(200, 520);
            this.groupBoxRecipeList.TabIndex = 0;
            this.groupBoxRecipeList.TabStop = false;
            this.groupBoxRecipeList.Text = "配方列表";
            // 
            // listBoxRecipes
            // 
            this.listBoxRecipes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxRecipes.FormattingEnabled = true;
            this.listBoxRecipes.ItemHeight = 36;
            this.listBoxRecipes.Location = new System.Drawing.Point(10, 46);
            this.listBoxRecipes.Name = "listBoxRecipes";
            this.listBoxRecipes.Size = new System.Drawing.Size(180, 464);
            this.listBoxRecipes.TabIndex = 0;
            this.listBoxRecipes.SelectedIndexChanged += new System.EventHandler(this.listBoxRecipes_SelectedIndexChanged);
            // 
            // panel_Bottom
            // 
            this.panel_Bottom.Controls.Add(this.tableLayoutPanel3);
            this.panel_Bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_Bottom.Location = new System.Drawing.Point(0, 475);
            this.panel_Bottom.Name = "panel_Bottom";
            this.panel_Bottom.Size = new System.Drawing.Size(580, 45);
            this.panel_Bottom.TabIndex = 2;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 7;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.Controls.Add(this.btnNew, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnCopy, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnDelete, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnLoad, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnSave, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnExport, 5, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnImport, 6, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(580, 45);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // btnNew
            // 
            this.btnNew.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNew.Location = new System.Drawing.Point(3, 3);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(76, 39);
            this.btnNew.TabIndex = 0;
            this.btnNew.Text = "新建";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCopy.Location = new System.Drawing.Point(85, 3);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(76, 39);
            this.btnCopy.TabIndex = 1;
            this.btnCopy.Text = "复制";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.Red;
            this.btnDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(167, 3);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(76, 39);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoad.Location = new System.Drawing.Point(249, 3);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(76, 39);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "加载";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(331, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(76, 39);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnExport
            // 
            this.btnExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExport.Location = new System.Drawing.Point(413, 3);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(76, 39);
            this.btnExport.TabIndex = 5;
            this.btnExport.Text = "导出";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnImport
            // 
            this.btnImport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnImport.Location = new System.Drawing.Point(495, 3);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(82, 39);
            this.btnImport.TabIndex = 6;
            this.btnImport.Text = "导入";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // groupBoxParams
            // 
            this.groupBoxParams.Controls.Add(this.dataGridViewParams);
            this.groupBoxParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxParams.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxParams.Location = new System.Drawing.Point(0, 180);
            this.groupBoxParams.Margin = new System.Windows.Forms.Padding(8);
            this.groupBoxParams.Name = "groupBoxParams";
            this.groupBoxParams.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxParams.Size = new System.Drawing.Size(580, 340);
            this.groupBoxParams.TabIndex = 1;
            this.groupBoxParams.TabStop = false;
            this.groupBoxParams.Text = "参数设置";
            // 
            // dataGridViewParams
            // 
            this.dataGridViewParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewParams.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnParamName,
            this.columnParamValue,
            this.columnParamDesc});
            this.dataGridViewParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewParams.Location = new System.Drawing.Point(10, 46);
            this.dataGridViewParams.Name = "dataGridViewParams";
            this.dataGridViewParams.RowHeadersWidth = 82;
            this.dataGridViewParams.RowTemplate.Height = 23;
            this.dataGridViewParams.Size = new System.Drawing.Size(560, 284);
            this.dataGridViewParams.TabIndex = 0;
            // 
            // columnParamName
            // 
            this.columnParamName.HeaderText = "参数名";
            this.columnParamName.MinimumWidth = 10;
            this.columnParamName.Name = "columnParamName";
            this.columnParamName.Width = 150;
            // 
            // columnParamValue
            // 
            this.columnParamValue.HeaderText = "参数值";
            this.columnParamValue.MinimumWidth = 10;
            this.columnParamValue.Name = "columnParamValue";
            this.columnParamValue.Width = 200;
            // 
            // columnParamDesc
            // 
            this.columnParamDesc.HeaderText = "描述";
            this.columnParamDesc.MinimumWidth = 10;
            this.columnParamDesc.Name = "columnParamDesc";
            this.columnParamDesc.Width = 200;
            // 
            // groupBoxRecipeInfo
            // 
            this.groupBoxRecipeInfo.Controls.Add(this.tableLayoutPanel1);
            this.groupBoxRecipeInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxRecipeInfo.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxRecipeInfo.Location = new System.Drawing.Point(0, 0);
            this.groupBoxRecipeInfo.Margin = new System.Windows.Forms.Padding(8);
            this.groupBoxRecipeInfo.Name = "groupBoxRecipeInfo";
            this.groupBoxRecipeInfo.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxRecipeInfo.Size = new System.Drawing.Size(580, 180);
            this.groupBoxRecipeInfo.TabIndex = 0;
            this.groupBoxRecipeInfo.TabStop = false;
            this.groupBoxRecipeInfo.Text = "配方信息";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.Controls.Add(this.labelRecipeName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxRecipeName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelDescription, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxDescription, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelProductModel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxProductModel, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelVersion, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelCreateTime, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.labelAuthor, 2, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 46);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(560, 124);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // labelRecipeName
            // 
            this.labelRecipeName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelRecipeName.AutoSize = true;
            this.labelRecipeName.Location = new System.Drawing.Point(3, 0);
            this.labelRecipeName.Name = "labelRecipeName";
            this.labelRecipeName.Size = new System.Drawing.Size(74, 30);
            this.labelRecipeName.TabIndex = 0;
            this.labelRecipeName.Text = "配方名称：";
            // 
            // textBoxRecipeName
            // 
            this.textBoxRecipeName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRecipeName.Location = new System.Drawing.Point(83, 3);
            this.textBoxRecipeName.Name = "textBoxRecipeName";
            this.textBoxRecipeName.ReadOnly = true;
            this.textBoxRecipeName.Size = new System.Drawing.Size(282, 43);
            this.textBoxRecipeName.TabIndex = 1;
            // 
            // labelDescription
            // 
            this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(3, 30);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(74, 60);
            this.labelDescription.TabIndex = 2;
            this.labelDescription.Text = "描述：";
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDescription.Location = new System.Drawing.Point(83, 37);
            this.textBoxDescription.Multiline = true;
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(282, 45);
            this.textBoxDescription.TabIndex = 3;
            // 
            // labelProductModel
            // 
            this.labelProductModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelProductModel.AutoSize = true;
            this.labelProductModel.Location = new System.Drawing.Point(3, 90);
            this.labelProductModel.Name = "labelProductModel";
            this.labelProductModel.Size = new System.Drawing.Size(74, 30);
            this.labelProductModel.TabIndex = 4;
            this.labelProductModel.Text = "产品型号：";
            // 
            // textBoxProductModel
            // 
            this.textBoxProductModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxProductModel.Location = new System.Drawing.Point(83, 93);
            this.textBoxProductModel.Name = "textBoxProductModel";
            this.textBoxProductModel.Size = new System.Drawing.Size(282, 43);
            this.textBoxProductModel.TabIndex = 5;
            // 
            // labelVersion
            // 
            this.labelVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(371, 90);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(186, 30);
            this.labelVersion.TabIndex = 6;
            this.labelVersion.Text = "版本: --";
            // 
            // labelCreateTime
            // 
            this.labelCreateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCreateTime.AutoSize = true;
            this.labelCreateTime.Location = new System.Drawing.Point(83, 120);
            this.labelCreateTime.Name = "labelCreateTime";
            this.labelCreateTime.Size = new System.Drawing.Size(282, 30);
            this.labelCreateTime.TabIndex = 7;
            this.labelCreateTime.Text = "创建时间: --";
            // 
            // labelAuthor
            // 
            this.labelAuthor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAuthor.AutoSize = true;
            this.labelAuthor.Location = new System.Drawing.Point(371, 120);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new System.Drawing.Size(186, 30);
            this.labelAuthor.TabIndex = 8;
            this.labelAuthor.Text = "作者: --";
            // 
            // panel_Right
            // 
            this.panel_Right.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Right.Location = new System.Drawing.Point(0, 0);
            this.panel_Right.Name = "panel_Right";
            this.panel_Right.Size = new System.Drawing.Size(580, 520);
            this.panel_Right.TabIndex = 0;
            // 
            // RecipeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(17F, 36F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 520);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MinimumSize = new System.Drawing.Size(627, 416);
            this.Name = "RecipeForm";
            this.Text = "配方管理";
            this.Load += new System.EventHandler(this.RecipeForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel_Left.ResumeLayout(false);
            this.groupBoxRecipeList.ResumeLayout(false);
            this.panel_Bottom.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.groupBoxParams.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParams)).EndInit();
            this.groupBoxRecipeInfo.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel_Left;
        private System.Windows.Forms.GroupBox groupBoxRecipeList;
        private System.Windows.Forms.ListBox listBoxRecipes;
        private System.Windows.Forms.Panel panel_Right;
        private System.Windows.Forms.GroupBox groupBoxRecipeInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label labelRecipeName;
        private System.Windows.Forms.TextBox textBoxRecipeName;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label labelProductModel;
        private System.Windows.Forms.TextBox textBoxProductModel;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelCreateTime;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.GroupBox groupBoxParams;
        private System.Windows.Forms.DataGridView dataGridViewParams;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnParamName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnParamValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnParamDesc;
        private System.Windows.Forms.Panel panel_Bottom;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnImport;
    }
}
