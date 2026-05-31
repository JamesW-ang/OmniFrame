using System;
using System.Drawing;

namespace OmniFrame.Theme
{
    /// <summary>
    /// UI主题系统类 - 提供统一的主题管理功能
        /// </summary>
    public static class UiTheme
    {
        /// <summary>
        /// 主题颜色结构体 - 定义主题的所有颜色属性
        /// </summary>
        public struct ThemeColors
        {
            /// <summary>
            /// 主色调 - 用于主要按钮、高亮元素等
        /// </summary>
            public Color Primary { get; set; }

            /// <summary>
            /// 主色深色 - 用于悬停状态、边框等
        /// </summary>
            public Color PrimaryDark { get; set; }

            /// <summary>
            /// 主色浅色 - 用于背景、选中状态等
        /// </summary>
            public Color PrimaryLight { get; set; }

            /// <summary>
            /// 辅助色 - 用于次要元素、文本等
        /// </summary>
            public Color Secondary { get; set; }

            /// <summary>
            /// 背景色 - 用于窗体背景
        /// </summary>
            public Color Background { get; set; }

            /// <summary>
            /// 卡片/面板色 - 用于面板、卡片等表面元素
        /// </summary>
            public Color Surface { get; set; }

            /// <summary>
            /// 主文本色 - 用于主要文本
        /// </summary>
            public Color TextPrimary { get; set; }

            /// <summary>
            /// 辅助文本色 - 用于次要文本、提示信息等
        /// </summary>
            public Color TextSecondary { get; set; }

            /// <summary>
            /// 边框色 - 用于控件边框、分隔线等
        /// </summary>
            public Color Border { get; set; }

            /// <summary>
            /// 成功色 - 用于成功状态、成功消息等
        /// </summary>
            public Color Success { get; set; }

            /// <summary>
            /// 警告色 - 用于警告状态、警告消息等
        /// </summary>
            public Color Warning { get; set; }

            /// <summary>
            /// 错误色 - 用于错误状态、错误消息等
        /// </summary>
            public Color Error { get; set; }

            /// <summary>
            /// 信息色 - 用于信息状态、信息消息等
        /// </summary>
            public Color Info { get; set; }
        }

        /// <summary>
        /// 浅色主题 - 适合明亮环境使用
        /// </summary>
        public static ThemeColors LightTheme => new ThemeColors
        {
            Primary = Color.FromArgb(37, 99, 235),       // #2563EB
            PrimaryDark = Color.FromArgb(29, 78, 216),    // #1D4ED8
            PrimaryLight = Color.FromArgb(147, 197, 253),  // #93C5FD
            Secondary = Color.FromArgb(100, 116, 139),    // #64748B
            Background = Color.FromArgb(248, 250, 252),   // #F8FAFC
            Surface = Color.White,                        // #FFFFFF
            TextPrimary = Color.FromArgb(30, 41, 59),     // #1E293B
            TextSecondary = Color.FromArgb(100, 116, 139), // #64748B
            Border = Color.FromArgb(226, 232, 240),       // #E2E8F0
            Success = Color.FromArgb(34, 197, 94),        // #22C55E
            Warning = Color.FromArgb(245, 158, 11),       // #F59E0B
            Error = Color.FromArgb(239, 68, 68),          // #EF4444
            Info = Color.FromArgb(59, 130, 246)           // #3B82F6
        };

        /// <summary>
        /// 深色主题 - 适合暗环境使用，减少眼睛疲劳
        /// </summary>
        public static ThemeColors DarkTheme => new ThemeColors
        {
            Primary = Color.FromArgb(59, 130, 246),       // #3B82F6
            PrimaryDark = Color.FromArgb(37, 99, 235),    // #2563EB
            PrimaryLight = Color.FromArgb(147, 197, 253),  // #93C5FD
            Secondary = Color.FromArgb(148, 163, 184),    // #94A3B8
            Background = Color.FromArgb(15, 23, 42),       // #0F172A
            Surface = Color.FromArgb(30, 41, 59),         // #1E293B
            TextPrimary = Color.FromArgb(241, 245, 249),  // #F1F5F9
            TextSecondary = Color.FromArgb(148, 163, 184), // #94A3B8
            Border = Color.FromArgb(51, 65, 85),          // #334155
            Success = Color.FromArgb(34, 197, 94),        // #22C55E
            Warning = Color.FromArgb(245, 158, 11),       // #F59E0B
            Error = Color.FromArgb(239, 68, 68),          // #EF4444
            Info = Color.FromArgb(59, 130, 246)           // #3B82F6
        };

        /// <summary>
        /// 当前主题存储
        /// </summary>
        private static ThemeColors _currentTheme = LightTheme;

        /// <summary>
        /// 当前主题 - 获取或设置当前使用的主题
        /// </summary>
        public static ThemeColors CurrentTheme
        {
            get { return _currentTheme; }
            set
            {
                if (!_currentTheme.Equals(value))
                {
                    _currentTheme = value;
                    // 触发主题变更事件
                    ThemeChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 主题变更事件 - 当主题变更时触发
        /// </summary>
        public static event EventHandler ThemeChanged;

        /// <summary>
        /// 切换主题 - 在浅色主题和深色主题之间切换
        /// </summary>
        public static void SwitchTheme()
        {
            CurrentTheme = CurrentTheme.Equals(LightTheme) ? DarkTheme : LightTheme;
        }

        #region 间距常量

        /// <summary>
        /// 控件组之间的间距 - 用于分隔不同功能的控件组
        /// </summary>
        public const int ControlGroupSpacing = 12;

        /// <summary>
        /// 相关控件之间的间距 - 用于同一功能组内的控件间距
        /// </summary>
        public const int ControlItemSpacing = 6;

        /// <summary>
        /// GroupBox 内边距 - 用于 GroupBox 内部控件与边框的距离
        /// </summary>
        public const int GroupBoxPadding = 10;

        /// <summary>
        /// GroupBox 外间距 - 用于 GroupBox 与其他控件的距离
        /// </summary>
        public const int GroupBoxMargin = 8;

        /// <summary>
        /// 区块之间的间距 - 用于分隔不同的功能区块
        /// </summary>
        public const int SectionSpacing = 18;

        #endregion
    }
}
