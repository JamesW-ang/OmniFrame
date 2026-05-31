using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 权限控制扩展类
        /// </summary>
    public static class PermissionExtensions
    {
        public static void SetControlPermission(this System.Windows.Forms.Control control, IPermissionManager permissionManager, UserLevel requiredLevel)
        {
            bool hasPermission = permissionManager.CheckPermission(requiredLevel);
            control.Enabled = hasPermission;
            control.Visible = hasPermission;
        }

        public static void SetControlsPermission(this System.Windows.Forms.Control control, IPermissionManager permissionManager, UserLevel requiredLevel)
        {
            foreach (System.Windows.Forms.Control childControl in control.Controls)
            {
                childControl.SetControlPermission(permissionManager, requiredLevel);
                if (childControl.HasChildren)
                {
                    childControl.SetControlsPermission(permissionManager, requiredLevel);
                }
            }
        }
    }
}
