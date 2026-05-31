using System;
using System.Drawing;
using System.Windows.Forms;

namespace OmniFrame.Dialogs
{
    /// <summary>
    /// 新建工单 — 替代 Qt CWorkPrivate.ui
    /// 操作员输入工单名称以启动新批次
    /// </summary>
    public class WorkPrivateDialog : Form
    {
        private readonly TextBox _txtWorkName;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public string WorkName { get; private set; }

        public WorkPrivateDialog(string defaultName = "")
        {
            Text = "新建工单";
            Size = new Size(420, 240);
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
                Text = "新建工单",
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 14F, FontStyle.Bold),
                Location = new Point(12, 8),
                AutoSize = true,
            };
            titleBar.Controls.Add(lblTitle);

            var lblName = new Label
            {
                Text = "工单名称",
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(45, 75),
                Size = new Size(85, 30),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            _txtWorkName = new TextBox
            {
                Text = defaultName,
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(135, 75),
                Size = new Size(235, 30),
                BorderStyle = BorderStyle.FixedSingle,
            };

            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.FromArgb(0x33, 0x4A, 0x74),
            };

            _btnOk = CreateButton("确认", 210);
            _btnCancel = CreateButton("取消", 310);

            bottomBar.Controls.Add(_btnOk);
            bottomBar.Controls.Add(_btnCancel);

            Controls.AddRange(new Control[] { lblName, _txtWorkName, titleBar, bottomBar });

            _btnOk.Click += (s, e) =>
            {
                WorkName = _txtWorkName.Text.Trim();
                if (string.IsNullOrEmpty(WorkName)) return;
                DialogResult = DialogResult.OK;
                Close();
            };
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            AcceptButton = _btnOk;
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
    }
}
