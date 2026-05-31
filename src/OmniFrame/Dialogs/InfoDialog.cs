using System;
using System.Drawing;
using System.Windows.Forms;

namespace OmniFrame.Dialogs
{
    /// <summary>
    /// 通用信息确认弹窗 — 替代 Qt CInfoDialog
    /// 用于取料失败、物料确认等场景，支持 重复/确认/取消 三按钮
    /// </summary>
    public class InfoDialog : Form
    {
        private readonly Label _lblInfo;
        private readonly Button _btnRepeat;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        /// <summary>用户选择: true=确认, false=取消</summary>
        public bool Confirmed { get; private set; }
        /// <summary>用户选择重复动作</summary>
        public bool IsRepeat { get; private set; }

        /// <param name="message">显示消息</param>
        /// <param name="showRepeat">是否显示"重复动作"按钮</param>
        public InfoDialog(string message, bool showRepeat = false)
        {
            Text = "提示";
            Size = new Size(400, 260);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Microsoft YaHei", 12F);

            var titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(0x33, 0x4A, 0x74),
            };
            var lblTitle = new Label
            {
                Text = "提示",
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 14F, FontStyle.Bold),
                Location = new Point(12, 8),
                AutoSize = true,
            };
            titleBar.Controls.Add(lblTitle);

            _lblInfo = new Label
            {
                Text = message,
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(50, 65),
                Size = new Size(320, 80),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.FromArgb(0x33, 0x4A, 0x74),
            };

            _btnRepeat = CreateButton("重复动作", 105);
            _btnOk = CreateButton("确认", 205);
            _btnCancel = CreateButton("取消", 300);

            _btnRepeat.Visible = showRepeat;
            if (showRepeat)
                _btnOk.Location = new Point(200, 6);

            bottomBar.Controls.Add(_btnRepeat);
            bottomBar.Controls.Add(_btnOk);
            bottomBar.Controls.Add(_btnCancel);

            Controls.AddRange(new Control[] { _lblInfo, titleBar, bottomBar });

            _btnRepeat.Click += (s, e) => { IsRepeat = true; Confirmed = true; DialogResult = DialogResult.Yes; Close(); };
            _btnOk.Click += (s, e) => { IsRepeat = false; Confirmed = true; DialogResult = DialogResult.OK; Close(); };
            _btnCancel.Click += (s, e) => { Confirmed = false; DialogResult = DialogResult.Cancel; Close(); };
        }

        private static Button CreateButton(string text, int x)
        {
            return new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0x33, 0x4A, 0x74),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 11F),
                Size = new Size(90, 32),
                Location = new Point(x, 6),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand,
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(0x33, 0x4A, 0x74), 2))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // InfoDialog
            // 
            this.ClientSize = new System.Drawing.Size(711, 586);
            this.Name = "InfoDialog";
            this.ResumeLayout(false);

        }
    }
}
