namespace OmniFrame
{
    partial class RoleManagerForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageRoles = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelRoleList = new System.Windows.Forms.Panel();
            this.listBoxRoles = new System.Windows.Forms.ListBox();
            this.panelRoleButtons = new System.Windows.Forms.Panel();
            this.btnDeleteRole = new System.Windows.Forms.Button();
            this.btnAddRole = new System.Windows.Forms.Button();
            this.panelPermissions = new System.Windows.Forms.Panel();
            this.checkedListBoxPermissions = new System.Windows.Forms.CheckedListBox();
            this.labelPermissions = new System.Windows.Forms.Label();
            this.tabPageUsers = new System.Windows.Forms.TabPage();
            this.panelUserButtons = new System.Windows.Forms.Panel();
            this.btnEnableUser = new System.Windows.Forms.Button();
            this.btnResetPassword = new System.Windows.Forms.Button();
            this.btnAddUser = new System.Windows.Forms.Button();
            this.dataGridViewUsers = new System.Windows.Forms.DataGridView();
            this.columnUsername = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnRole = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.columnLastLogin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControl1.SuspendLayout();
            this.tabPageRoles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelRoleList.SuspendLayout();
            this.panelRoleButtons.SuspendLayout();
            this.panelPermissions.SuspendLayout();
            this.tabPageUsers.SuspendLayout();
            this.panelUserButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUsers)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageRoles);
            this.tabControl1.Controls.Add(this.tabPageUsers);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 500);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageRoles
            // 
            this.tabPageRoles.Controls.Add(this.splitContainer1);
            this.tabPageRoles.Location = new System.Drawing.Point(4, 22);
            this.tabPageRoles.Name = "tabPageRoles";
            this.tabPageRoles.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRoles.Size = new System.Drawing.Size(792, 474);
            this.tabPageRoles.TabIndex = 0;
            this.tabPageRoles.Text = "角色管理";
            this.tabPageRoles.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelRoleList);
            this.splitContainer1.Panel1.Controls.Add(this.panelRoleButtons);
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelPermissions);
            this.splitContainer1.Panel2MinSize = 400;
            this.splitContainer1.Size = new System.Drawing.Size(786, 468);
            this.splitContainer1.SplitterDistance = 250;
            this.splitContainer1.TabIndex = 0;
            // 
            // panelRoleList
            // 
            this.panelRoleList.Controls.Add(this.listBoxRoles);
            this.panelRoleList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRoleList.Location = new System.Drawing.Point(0, 0);
            this.panelRoleList.Name = "panelRoleList";
            this.panelRoleList.Size = new System.Drawing.Size(250, 428);
            this.panelRoleList.TabIndex = 0;
            // 
            // listBoxRoles
            // 
            this.listBoxRoles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxRoles.FormattingEnabled = true;
            this.listBoxRoles.Location = new System.Drawing.Point(0, 0);
            this.listBoxRoles.Name = "listBoxRoles";
            this.listBoxRoles.Size = new System.Drawing.Size(250, 428);
            this.listBoxRoles.TabIndex = 0;
            this.listBoxRoles.SelectedIndexChanged += new System.EventHandler(this.listBoxRoles_SelectedIndexChanged);
            // 
            // panelRoleButtons
            // 
            this.panelRoleButtons.Controls.Add(this.btnDeleteRole);
            this.panelRoleButtons.Controls.Add(this.btnAddRole);
            this.panelRoleButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelRoleButtons.Location = new System.Drawing.Point(0, 428);
            this.panelRoleButtons.Name = "panelRoleButtons";
            this.panelRoleButtons.Size = new System.Drawing.Size(250, 40);
            this.panelRoleButtons.TabIndex = 1;
            // 
            // btnDeleteRole
            // 
            this.btnDeleteRole.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnDeleteRole.Location = new System.Drawing.Point(130, 5);
            this.btnDeleteRole.Name = "btnDeleteRole";
            this.btnDeleteRole.Size = new System.Drawing.Size(110, 30);
            this.btnDeleteRole.TabIndex = 1;
            this.btnDeleteRole.Text = "删除角色";
            this.btnDeleteRole.UseVisualStyleBackColor = true;
            this.btnDeleteRole.Click += new System.EventHandler(this.btnDeleteRole_Click);
            // 
            // btnAddRole
            // 
            this.btnAddRole.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAddRole.Location = new System.Drawing.Point(10, 5);
            this.btnAddRole.Name = "btnAddRole";
            this.btnAddRole.Size = new System.Drawing.Size(110, 30);
            this.btnAddRole.TabIndex = 0;
            this.btnAddRole.Text = "新增角色";
            this.btnAddRole.UseVisualStyleBackColor = true;
            this.btnAddRole.Click += new System.EventHandler(this.btnAddRole_Click);
            // 
            // panelPermissions
            // 
            this.panelPermissions.Controls.Add(this.checkedListBoxPermissions);
            this.panelPermissions.Controls.Add(this.labelPermissions);
            this.panelPermissions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPermissions.Location = new System.Drawing.Point(0, 0);
            this.panelPermissions.Name = "panelPermissions";
            this.panelPermissions.Size = new System.Drawing.Size(532, 468);
            this.panelPermissions.TabIndex = 0;
            // 
            // checkedListBoxPermissions
            // 
            this.checkedListBoxPermissions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBoxPermissions.FormattingEnabled = true;
            this.checkedListBoxPermissions.Location = new System.Drawing.Point(0, 30);
            this.checkedListBoxPermissions.Name = "checkedListBoxPermissions";
            this.checkedListBoxPermissions.Size = new System.Drawing.Size(532, 438);
            this.checkedListBoxPermissions.TabIndex = 1;
            this.checkedListBoxPermissions.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxPermissions_ItemCheck);
            // 
            // labelPermissions
            // 
            this.labelPermissions.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelPermissions.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.labelPermissions.Location = new System.Drawing.Point(0, 0);
            this.labelPermissions.Name = "labelPermissions";
            this.labelPermissions.Size = new System.Drawing.Size(532, 30);
            this.labelPermissions.TabIndex = 0;
            this.labelPermissions.Text = "权限列表";
            this.labelPermissions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabPageUsers
            // 
            this.tabPageUsers.Controls.Add(this.panelUserButtons);
            this.tabPageUsers.Controls.Add(this.dataGridViewUsers);
            this.tabPageUsers.Location = new System.Drawing.Point(4, 22);
            this.tabPageUsers.Name = "tabPageUsers";
            this.tabPageUsers.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUsers.Size = new System.Drawing.Size(792, 474);
            this.tabPageUsers.TabIndex = 1;
            this.tabPageUsers.Text = "用户管理";
            this.tabPageUsers.UseVisualStyleBackColor = true;
            // 
            // panelUserButtons
            // 
            this.panelUserButtons.Controls.Add(this.btnEnableUser);
            this.panelUserButtons.Controls.Add(this.btnResetPassword);
            this.panelUserButtons.Controls.Add(this.btnAddUser);
            this.panelUserButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelUserButtons.Location = new System.Drawing.Point(3, 434);
            this.panelUserButtons.Name = "panelUserButtons";
            this.panelUserButtons.Size = new System.Drawing.Size(786, 37);
            this.panelUserButtons.TabIndex = 1;
            // 
            // btnEnableUser
            // 
            this.btnEnableUser.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnEnableUser.Location = new System.Drawing.Point(666, 4);
            this.btnEnableUser.Name = "btnEnableUser";
            this.btnEnableUser.Size = new System.Drawing.Size(110, 30);
            this.btnEnableUser.TabIndex = 2;
            this.btnEnableUser.Text = "启用/禁用";
            this.btnEnableUser.UseVisualStyleBackColor = true;
            this.btnEnableUser.Click += new System.EventHandler(this.btnEnableUser_Click);
            // 
            // btnResetPassword
            // 
            this.btnResetPassword.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnResetPassword.Location = new System.Drawing.Point(550, 4);
            this.btnResetPassword.Name = "btnResetPassword";
            this.btnResetPassword.Size = new System.Drawing.Size(110, 30);
            this.btnResetPassword.TabIndex = 1;
            this.btnResetPassword.Text = "重置密码";
            this.btnResetPassword.UseVisualStyleBackColor = true;
            this.btnResetPassword.Click += new System.EventHandler(this.btnResetPassword_Click);
            // 
            // btnAddUser
            // 
            this.btnAddUser.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAddUser.Location = new System.Drawing.Point(10, 4);
            this.btnAddUser.Name = "btnAddUser";
            this.btnAddUser.Size = new System.Drawing.Size(110, 30);
            this.btnAddUser.TabIndex = 0;
            this.btnAddUser.Text = "新增用户";
            this.btnAddUser.UseVisualStyleBackColor = true;
            this.btnAddUser.Click += new System.EventHandler(this.btnAddUser_Click);
            // 
            // dataGridViewUsers
            // 
            this.dataGridViewUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewUsers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnUsername,
            this.columnRole,
            this.columnLastLogin,
            this.columnStatus});
            this.dataGridViewUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewUsers.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewUsers.Name = "dataGridViewUsers";
            this.dataGridViewUsers.RowTemplate.Height = 23;
            this.dataGridViewUsers.Size = new System.Drawing.Size(786, 431);
            this.dataGridViewUsers.TabIndex = 0;
            // 
            // columnUsername
            // 
            this.columnUsername.HeaderText = "用户名";
            this.columnUsername.Name = "columnUsername";
            this.columnUsername.ReadOnly = true;
            this.columnUsername.Width = 150;
            // 
            // columnRole
            // 
            this.columnRole.HeaderText = "角色";
            this.columnRole.Name = "columnRole";
            this.columnRole.Width = 150;
            // 
            // columnLastLogin
            // 
            this.columnLastLogin.HeaderText = "最后登录时间";
            this.columnLastLogin.Name = "columnLastLogin";
            this.columnLastLogin.ReadOnly = true;
            this.columnLastLogin.Width = 200;
            // 
            // columnStatus
            // 
            this.columnStatus.HeaderText = "状态";
            this.columnStatus.Name = "columnStatus";
            this.columnStatus.ReadOnly = true;
            this.columnStatus.Width = 100;
            // 
            // RoleManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "RoleManagerForm";
            this.Text = "角色权限管理";
            this.Load += new System.EventHandler(this.RoleManagerForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageRoles.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelRoleList.ResumeLayout(false);
            this.panelRoleButtons.ResumeLayout(false);
            this.panelPermissions.ResumeLayout(false);
            this.tabPageUsers.ResumeLayout(false);
            this.panelUserButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUsers)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageRoles;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelRoleList;
        private System.Windows.Forms.ListBox listBoxRoles;
        private System.Windows.Forms.Panel panelRoleButtons;
        private System.Windows.Forms.Button btnDeleteRole;
        private System.Windows.Forms.Button btnAddRole;
        private System.Windows.Forms.Panel panelPermissions;
        private System.Windows.Forms.CheckedListBox checkedListBoxPermissions;
        private System.Windows.Forms.Label labelPermissions;
        private System.Windows.Forms.TabPage tabPageUsers;
        private System.Windows.Forms.Panel panelUserButtons;
        private System.Windows.Forms.Button btnEnableUser;
        private System.Windows.Forms.Button btnResetPassword;
        private System.Windows.Forms.Button btnAddUser;
        private System.Windows.Forms.DataGridView dataGridViewUsers;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnUsername;
        private System.Windows.Forms.DataGridViewComboBoxColumn columnRole;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLastLogin;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnStatus;
    }
}
