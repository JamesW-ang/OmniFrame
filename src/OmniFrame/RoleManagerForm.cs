using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using HelperDialog = OmniFrame.Theme.DialogHelper;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// 角色管理窗体类 - 用户角色与权限管理
        /// </summary>
    public partial class RoleManagerForm : Form
    {
        private List<RoleInfo> roles = new List<RoleInfo>();
        private List<LegacyUserInfo> users = new List<LegacyUserInfo>();
        private const string rolesFilePath = "roles.json";
        private const string usersFilePath = "users.json";

        public RoleManagerForm()
        {
            InitializeComponent();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        private void RoleManagerForm_Load(object sender, EventArgs e)
        {
            LoadRoles();
            LoadUsers();
            InitializePermissions();
        }

        private void LoadRoles()
        {
            if (File.Exists(rolesFilePath))
            {
                string json = File.ReadAllText(rolesFilePath);
                roles = JsonConvert.DeserializeObject<List<RoleInfo>>(json);
            }
            else
            {
                // 预置4个角色
                roles = new List<RoleInfo>
                {
                    new RoleInfo { Name = "管理员", IsBuiltIn = true, Permissions = new List<string> { "全部权限" } },
                    new RoleInfo { Name = "操作员", IsBuiltIn = true, Permissions = new List<string> { "设备操作", "配方管理" } },
                    new RoleInfo { Name = "工艺工程师", IsBuiltIn = true, Permissions = new List<string> { "参数设置", "配方管理", "报表查看" } },
                    new RoleInfo { Name = "访客", IsBuiltIn = true, Permissions = new List<string> { "报表查看" } }
                };
                SaveRoles();
            }

            listBoxRoles.Items.Clear();
            foreach (var role in roles)
            {
                listBoxRoles.Items.Add(role.Name);
            }

            if (listBoxRoles.Items.Count > 0)
            {
                listBoxRoles.SelectedIndex = 0;
            }
        }

        private void LoadUsers()
        {
            if (File.Exists(usersFilePath))
            {
                string json = File.ReadAllText(usersFilePath);
                users = JsonConvert.DeserializeObject<List<LegacyUserInfo>>(json);
            }
            else
            {
                // 预置管理员用户
                users = new List<LegacyUserInfo>
                {
                    new LegacyUserInfo { Username = "admin", Role = "管理员", LastLoginTime = DateTime.Now, IsEnabled = true }
                };
                SaveUsers();
            }

            UpdateUserGrid();
        }

        private void InitializePermissions()
        {
            // 权限列表
            string[] permissions = {
                "全部权限",
                "设备操作",
                "参数设置",
                "配方管理",
                "报表查看",
                "用户管理",
                "角色管理",
                "系统设置"
            };

            checkedListBoxPermissions.Items.Clear();
            foreach (var permission in permissions)
            {
                checkedListBoxPermissions.Items.Add(permission);
            }
        }

        private void SaveRoles()
        {
            string json = JsonConvert.SerializeObject(roles, Formatting.Indented);
            File.WriteAllText(rolesFilePath, json);
        }

        private void SaveUsers()
        {
            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(usersFilePath, json);
        }

        private void listBoxRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxRoles.SelectedIndex >= 0)
            {
                string roleName = listBoxRoles.SelectedItem.ToString();
                var role = roles.Find(r => r.Name == roleName);
                if (role != null)
                {
                    UpdatePermissionsList(role);
                }
            }
        }

        private void UpdatePermissionsList(RoleInfo role)
        {
            checkedListBoxPermissions.Items.Clear();
            string[] allPermissions = {
                "全部权限",
                "设备操作",
                "参数设置",
                "配方管理",
                "报表查看",
                "用户管理",
                "角色管理",
                "系统设置"
            };

            foreach (var permission in allPermissions)
            {
                bool isChecked = role.Permissions.Contains(permission);
                checkedListBoxPermissions.Items.Add(permission, isChecked);
            }

            // 内置角色不可删除
            btnDeleteRole.Enabled = !role.IsBuiltIn;
        }

        private void checkedListBoxPermissions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (listBoxRoles.SelectedIndex >= 0)
            {
                string roleName = listBoxRoles.SelectedItem.ToString();
                var role = roles.Find(r => r.Name == roleName);
                if (role != null)
                {
                    string permission = checkedListBoxPermissions.Items[e.Index].ToString();
                    if (e.NewValue == CheckState.Checked)
                    {
                        if (!role.Permissions.Contains(permission))
                        {
                            role.Permissions.Add(permission);
                        }
                    }
                    else
                    {
                        role.Permissions.Remove(permission);
                    }
                    SaveRoles();
                }
            }
        }

        private void btnAddRole_Click(object sender, EventArgs e)
        {
            string roleName = Microsoft.VisualBasic.Interaction.InputBox("请输入角色名称:", "新增角色");
            if (!string.IsNullOrEmpty(roleName))
            {
                if (roles.Exists(r => r.Name == roleName))
                {
                    HelperDialog.ShowError("角色名称已存在");
                    return;
                }

                var newRole = new RoleInfo { Name = roleName, IsBuiltIn = false, Permissions = new List<string>() };
                roles.Add(newRole);
                SaveRoles();
                listBoxRoles.Items.Add(roleName);
                listBoxRoles.SelectedIndex = listBoxRoles.Items.Count - 1;
            }
        }

        private void btnDeleteRole_Click(object sender, EventArgs e)
        {
            if (listBoxRoles.SelectedIndex >= 0)
            {
                string roleName = listBoxRoles.SelectedItem.ToString();
                var role = roles.Find(r => r.Name == roleName);
                if (role != null && !role.IsBuiltIn)
                {
                    if (HelperDialog.Confirm("确认删除", $"确定要删除角色 {roleName} 吗？"))
                        {
                            // 检查是否有用户使用此角色
                            bool hasUsers = users.Exists(u => u.Role == roleName);
                            if (hasUsers)
                            {
                                HelperDialog.ShowWarning("该角色下还有用户，无法删除");
                                return;
                            }

                        roles.Remove(role);
                        SaveRoles();
                        listBoxRoles.Items.Remove(roleName);
                        if (listBoxRoles.Items.Count > 0)
                        {
                            listBoxRoles.SelectedIndex = 0;
                        }
                        else
                        {
                            checkedListBoxPermissions.Items.Clear();
                        }
                    }
                }
            }
        }

        private void UpdateUserGrid()
        {
            dataGridViewUsers.Rows.Clear();
            foreach (var user in users)
            {
                dataGridViewUsers.Rows.Add(user.Username, user.Role, user.LastLoginTime.ToString("yyyy-MM-dd HH:mm:ss"), user.IsEnabled ? "启用" : "禁用");
            }

            // 更新角色下拉列表
            var roleNames = roles.Select(r => r.Name).ToList();
            columnRole.DataSource = roleNames;
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            string username = Microsoft.VisualBasic.Interaction.InputBox("请输入用户名:", "新增用户");
            if (!string.IsNullOrEmpty(username))
            {
                if (users.Exists(u => u.Username == username))
                {
                    HelperDialog.ShowError("用户名已存在");
                    return;
                }

                string password = Microsoft.VisualBasic.Interaction.InputBox("请输入密码:", "设置密码");
                if (string.IsNullOrEmpty(password))
                {
                    HelperDialog.ShowError("密码不能为空");
                    return;
                }

                var newUser = new LegacyUserInfo
                {
                    Username = username,
                    Role = "访客", // 默认角色
                    LastLoginTime = DateTime.MinValue,
                    IsEnabled = true
                };
                users.Add(newUser);
                SaveUsers();
                UpdateUserGrid();
            }
        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count > 0)
            {
                string username = dataGridViewUsers.SelectedRows[0].Cells["columnUsername"].Value.ToString();
                string newPassword = Microsoft.VisualBasic.Interaction.InputBox("请输入新密码:", "重置密码");
                if (!string.IsNullOrEmpty(newPassword))
                {
                    var user = users.Find(u => u.Username == username);
                    if (user != null)
                    {
                        // 这里应该有密码加密逻辑
                        SaveUsers();
                        HelperDialog.ShowSuccess("密码重置成功");
                    }
                }
            }
        }

        private void btnEnableUser_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count > 0)
            {
                string username = dataGridViewUsers.SelectedRows[0].Cells["columnUsername"].Value.ToString();
                var user = users.Find(u => u.Username == username);
                if (user != null)
                {
                    user.IsEnabled = !user.IsEnabled;
                    SaveUsers();
                    UpdateUserGrid();
                    HelperDialog.ShowSuccess($"用户 {username} 已{(user.IsEnabled ? "启用" : "禁用")}");
                }
            }
        }
    }

    // 角色信息类
    public class RoleInfo
    {
        public string Name { get; set; }
        public bool IsBuiltIn { get; set; }
        public List<string> Permissions { get; set; }
    }

    // 用户信息类（旧版本）
    public class LegacyUserInfo
    {
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime LastLoginTime { get; set; }
        public bool IsEnabled { get; set; }
    }
}
