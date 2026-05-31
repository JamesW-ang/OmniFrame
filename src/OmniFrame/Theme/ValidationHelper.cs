using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace OmniFrame.Theme
{
    /// <summary>
    /// 输入验证工具类
        /// </summary>
    public static class ValidationHelper
    {
        private static ErrorProvider errorProvider = new ErrorProvider();

        /// <summary>
        /// 验证必填项
        /// </summary>
        /// <param name="ctrl">控件</param>
        /// <param name="fieldName">字段名称</param>
        /// <returns>是否验证通过</returns>
        public static bool ValidateRequired(Control ctrl, string fieldName)
        {
            if (ctrl is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
            {
                errorProvider.SetError(ctrl, $"{fieldName}不能为空");
                return false;
            }
            else if (ctrl is ComboBox comboBox && (comboBox.SelectedIndex == -1 || comboBox.Text == string.Empty))
            {
                errorProvider.SetError(ctrl, $"请选择{fieldName}");
                return false;
            }
            else if (ctrl is NumericUpDown numericUpDown && numericUpDown.Value == 0)
            {
                errorProvider.SetError(ctrl, $"{fieldName}不能为空");
                return false;
            }

            errorProvider.SetError(ctrl, string.Empty);
            return true;
        }

        /// <summary>
        /// 验证数值范围
        /// </summary>
        /// <param name="ctrl">控件</param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>是否验证通过</returns>
        public static bool ValidateRange(Control ctrl, string fieldName, decimal min, decimal max)
        {
            if (ctrl is NumericUpDown numericUpDown)
            {
                if (numericUpDown.Value < min || numericUpDown.Value > max)
                {
                    errorProvider.SetError(ctrl, $"{fieldName}必须在{min}到{max}之间");
                    return false;
                }
            }
            else if (ctrl is TextBox textBox && decimal.TryParse(textBox.Text, out decimal value))
            {
                if (value < min || value > max)
                {
                    errorProvider.SetError(ctrl, $"{fieldName}必须在{min}到{max}之间");
                    return false;
                }
            }

            errorProvider.SetError(ctrl, string.Empty);
            return true;
        }

        /// <summary>
        /// 验证正则表达式
        /// </summary>
        /// <param name="ctrl">控件</param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="pattern">正则表达式</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns>是否验证通过</returns>
        public static bool ValidateRegex(Control ctrl, string fieldName, string pattern, string errorMsg)
        {
            if (ctrl is TextBox textBox)
            {
                if (!Regex.IsMatch(textBox.Text, pattern))
                {
                    errorProvider.SetError(ctrl, errorMsg);
                    return false;
                }
            }

            errorProvider.SetError(ctrl, string.Empty);
            return true;
        }

        /// <summary>
        /// 通过 ErrorProvider 统一显示所有验证错误
        /// </summary>
        /// <param name="form">窗体</param>
        /// <returns>是否所有验证都通过</returns>
        public static bool ShowErrors(Form form)
        {
            bool isValid = true;

            // 检查所有控件的错误状态
            foreach (Control ctrl in form.Controls)
            {
                if (!string.IsNullOrEmpty(errorProvider.GetError(ctrl)))
                {
                    isValid = false;
                    ctrl.Focus();
                    break;
                }

                // 递归检查子控件
                if (HasErrors(ctrl))
                {
                    isValid = false;
                    break;
                }
            }

            if (!isValid)
            {
                errorProvider.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
            }

            return isValid;
        }

        /// <summary>
        /// 检查控件及其子控件是否有错误
        /// </summary>
        /// <param name="control">控件</param>
        /// <returns>是否有错误</returns>
        private static bool HasErrors(Control control)
        {
            foreach (Control childControl in control.Controls)
            {
                if (!string.IsNullOrEmpty(errorProvider.GetError(childControl)))
                {
                    childControl.Focus();
                    return true;
                }

                if (HasErrors(childControl))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 清除所有错误
        /// </summary>
        /// <param name="form">窗体</param>
        public static void ClearErrors(Form form)
        {
            errorProvider.Clear();

            // 递归清除所有子控件的错误
            ClearErrorsRecursive(form);
        }

        /// <summary>
        /// 递归清除错误
        /// </summary>
        /// <param name="control">控件</param>
        private static void ClearErrorsRecursive(Control control)
        {
            foreach (Control childControl in control.Controls)
            {
                errorProvider.SetError(childControl, string.Empty);
                ClearErrorsRecursive(childControl);
            }
        }
    }
}
