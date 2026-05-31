using System;
using System.Drawing;
using System.Windows.Forms;

namespace OmniFrame.Dialogs
{
    /// <summary>
    /// 卡塞起始序号确认 — 替代 Qt CountDialog.ui
    /// 操作员确认从第几个卡塞开始取料 (1-6)
    /// </summary>
    public class CountDialog : Form
    {
        private readonly Label _lblInfo;
        private readonly NumericUpDown _numIndex;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public int CasselIndex { get; private set; } = 1;

        public CountDialog(int currentIndex = 1)
        {
            Text = "卡塞序号";
            Size = new Size(400, 280);
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
                Text = "请确认卡塞料塔的起始序号",
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(40, 70),
                Size = new Size(320, 40),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var lblHint = new Label
            {
                Text = "继续运行点击确认，停止运行点击取消！",
                Font = new Font("Microsoft YaHei", 10F),
                ForeColor = Color.Gray,
                Location = new Point(40, 105),
                Size = new Size(320, 25),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var lblIndex = new Label
            {
                Text = "第几个卡塞开始",
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(40, 150),
                Size = new Size(130, 30),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            _numIndex = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 6,
                Value = currentIndex,
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(175, 150),
                Size = new Size(80, 30),
            };

            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.FromArgb(0x33, 0x4A, 0x74),
            };

            _btnOk = CreateButton("确认", 195);
            _btnCancel = CreateButton("取消", 295);

            bottomBar.Controls.Add(_btnOk);
            bottomBar.Controls.Add(_btnCancel);

            Controls.AddRange(new Control[] { _lblInfo, lblHint, lblIndex, _numIndex, titleBar, bottomBar });

            _btnOk.Click += (s, e) => { CasselIndex = (int)_numIndex.Value; DialogResult = DialogResult.OK; Close(); };
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
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
            // CountDialog
            // 
            this.ClientSize = new System.Drawing.Size(822, 596);
            this.Name = "CountDialog";
            this.ResumeLayout(false);

        }
    }
}
