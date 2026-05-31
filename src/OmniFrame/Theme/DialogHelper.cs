using System.Windows.Forms;

namespace OmniFrame.Theme
{
    /// <summary>
    /// 对话框类型
        /// </summary>
    public enum AlertType
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success,
        /// <summary>
        /// 警告
        /// </summary>
        Warning,
        /// <summary>
        /// 错误
        /// </summary>
        Error,
        /// <summary>
        /// 信息
        /// </summary>
        Info
    }

    /// <summary>
    /// 对话框辅助类
        /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// 确认对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息</param>
        /// <returns>是否确认</returns>
        public static bool Confirm(string title, string message)
        {
            DialogResult result = MessageBox.Show(
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2
            );

            return result == DialogResult.Yes;
        }

        /// <summary>
        /// 提示对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息</param>
        /// <param name="type">对话框类型</param>
        public static void Alert(string title, string message, AlertType type = AlertType.Info)
        {
            MessageBoxIcon icon = MessageBoxIcon.Information;

            switch (type)
            {
                case AlertType.Success:
                    icon = MessageBoxIcon.Information;
                    break;
                case AlertType.Warning:
                    icon = MessageBoxIcon.Warning;
                    break;
                case AlertType.Error:
                    icon = MessageBoxIcon.Error;
                    break;
                case AlertType.Info:
                default:
                    icon = MessageBoxIcon.Information;
                    break;
            }

            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                icon
            );
        }

        /// <summary>
        /// 输入对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>输入的值</returns>
        public static string Input(string title, string message, string defaultValue = "")
        {
            return Microsoft.VisualBasic.Interaction.InputBox(
                message,
                title,
                defaultValue
            );
        }

        /// <summary>
        /// 显示错误消息
        /// </summary>
        /// <param name="message">错误消息</param>
        public static void ShowError(string message)
        {
            Alert("错误", message, AlertType.Error);
        }

        /// <summary>
        /// 显示成功消息
        /// </summary>
        /// <param name="message">成功消息</param>
        public static void ShowSuccess(string message)
        {
            Alert("成功", message, AlertType.Success);
        }

        /// <summary>
        /// 显示警告消息
        /// </summary>
        /// <param name="message">警告消息</param>
        public static void ShowWarning(string message)
        {
            Alert("警告", message, AlertType.Warning);
        }

        /// <summary>
        /// 显示信息消息
        /// </summary>
        /// <param name="message">信息消息</param>
        public static void ShowInfo(string message)
        {
            Alert("信息", message, AlertType.Info);
        }
    }
}
