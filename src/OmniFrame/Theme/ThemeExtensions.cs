using System.Windows.Forms;
using System.Drawing;

namespace OmniFrame.Theme
{
    /// <summary>
    /// 主题扩展方法类 - 提供控件主题应用的扩展方法
        /// </summary>
    public static class ThemeExtensions
    {
        /// <summary>
        /// 应用主题到窗体
        /// </summary>
        /// <param name="form">要应用主题的窗体</param>
        public static void ApplyTheme(this Form form)
        {
            var theme = UiTheme.CurrentTheme;

            // 设置窗体本身的背景色和前景色
            form.BackColor = theme.Background;
            form.ForeColor = theme.TextPrimary;

            // 递归设置所有子控件的主题
            ApplyThemeToControl(form, theme);

            // 刷新窗体，确保主题效果立即生效
            form.Refresh();
        }

        /// <summary>
        /// 递归应用主题到控件及其子控件
        /// </summary>
        /// <param name="control">要应用主题的控件</param>
        /// <param name="theme">主题颜色配置</param>
        private static void ApplyThemeToControl(Control control, UiTheme.ThemeColors theme)
        {
            // 根据控件类型设置不同的主题样式
            switch (control)
            {
                case Button button:
                    // 设置按钮的背景色、前景色和边框色
                    button.BackColor = theme.Primary;
                    button.ForeColor = Color.White;
                    if (button.FlatStyle == FlatStyle.Flat)
                    {
                        button.FlatAppearance.BorderColor = theme.PrimaryDark;
                    }
                    break;

                case TextBox textBox:
                    // 设置文本框的背景色、前景色和边框样式
                    textBox.BackColor = theme.Surface;
                    textBox.ForeColor = theme.TextPrimary;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    // 对于TextBox，BorderColor需要通过自定义绘制或第三方控件实现
                    break;

                case Label label:
                    // 设置标签的前景色
                    label.ForeColor = theme.TextPrimary;
                    break;

                case GroupBox groupBox:
                    // 设置分组框的前景色
                    groupBox.ForeColor = theme.TextPrimary;
                    break;

                case DataGridView dataGridView:
                    // 设置数据网格的背景色、网格色和交替行背景色
                    dataGridView.BackgroundColor = theme.Surface;
                    dataGridView.GridColor = theme.Border;
                    dataGridView.AlternatingRowsDefaultCellStyle.BackColor = theme.Background;
                    // 应用数据网格的特殊样式
                    dataGridView.ApplyDataGridStyle();
                    break;

                case TabControl tabControl:
                    // 设置选项卡控件的背景色
                    tabControl.BackColor = theme.Background;
                    break;

                case StatusStrip statusStrip:
                    // 设置状态栏的背景色和前景色
                    statusStrip.BackColor = theme.Primary;
                    statusStrip.ForeColor = Color.White;
                    break;

                case MenuStrip menuStrip:
                    // 设置菜单栏的背景色和前景色
                    menuStrip.BackColor = theme.Surface;
                    menuStrip.ForeColor = theme.TextPrimary;
                    break;
            }

            // 递归处理子控件，确保所有嵌套的控件都能应用主题
            foreach (Control childControl in control.Controls)
            {
                ApplyThemeToControl(childControl, theme);
            }
        }

        /// <summary>
        /// 应用按钮样式
        /// </summary>
        /// <param name="btn">要应用样式的按钮</param>
        /// <param name="size">按钮尺寸：small, medium, large</param>
        public static void ApplyButtonStyle(this Button btn, string size = "medium")
        {
            // 根据指定的尺寸设置按钮大小
            switch (size.ToLower())
            {
                case "small":
                    btn.Size = new Size(75, 30);
                    break;
                case "large":
                    btn.Size = new Size(140, 40);
                    break;
                case "medium":
                default:
                    btn.Size = new Size(100, 35);
                    break;
            }
        }

        /// <summary>
        /// 应用数据网格样式
        /// </summary>
        /// <param name="dgv">要应用样式的数据网格</param>
        public static void ApplyDataGridStyle(this DataGridView dgv)
        {
            var theme = UiTheme.CurrentTheme;

            // 设置行高
            dgv.RowTemplate.Height = 24;

            // 设置选中行的背景色和前景色
            dgv.DefaultCellStyle.SelectionBackColor = theme.PrimaryLight;
            dgv.DefaultCellStyle.SelectionForeColor = theme.TextPrimary;

            // 设置字体
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular);

            // 启用列宽自动调整
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            // 设置列头的背景色、前景色和选中背景色
            dgv.ColumnHeadersDefaultCellStyle.BackColor = theme.Surface;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = theme.TextPrimary;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = theme.PrimaryLight;
        }
    }
}
