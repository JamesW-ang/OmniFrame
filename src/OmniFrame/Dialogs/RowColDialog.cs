using System;
using System.Drawing;
using System.Windows.Forms;

namespace OmniFrame.Dialogs
{
    /// <summary>
    /// 行列起始序号确认 — 替代 Qt RowColDialog.ui
    /// 操作员确认载台组上料的起始行列位置
    /// </summary>
    public class RowColDialog : Form
    {
        private readonly NumericUpDown _numRow;
        private readonly NumericUpDown _numCol;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public int StartRow { get; private set; } = 1;
        public int StartCol { get; private set; } = 1;

        public RowColDialog(int currentRow = 1, int currentCol = 1, int maxRow = 20, int maxCol = 20)
        {
            Text = "行列选择";
            Size = new Size(400, 300);
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

            var lblInfo = new Label
            {
                Text = "请确认载台组上料的起始序号！",
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(40, 65),
                Size = new Size(320, 35),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var lblHint = new Label
            {
                Text = "继续运行点击确认，停止运行点击取消",
                Font = new Font("Microsoft YaHei", 10F),
                ForeColor = Color.Gray,
                Location = new Point(40, 95),
                Size = new Size(320, 25),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var lblRow = new Label
            {
                Text = "行-竖",
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(60, 140),
                Size = new Size(70, 30),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            _numRow = new NumericUpDown
            {
                Minimum = 1,
                Maximum = maxRow,
                Value = currentRow,
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(135, 140),
                Size = new Size(80, 30),
            };

            var lblCol = new Label
            {
                Text = "列-横",
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(60, 180),
                Size = new Size(70, 30),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            _numCol = new NumericUpDown
            {
                Minimum = 1,
                Maximum = maxCol,
                Value = currentCol,
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(135, 180),
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

            Controls.AddRange(new Control[] { lblInfo, lblHint, lblRow, _numRow, lblCol, _numCol, titleBar, bottomBar });

            _btnOk.Click += (s, e) => { StartRow = (int)_numRow.Value; StartCol = (int)_numCol.Value; DialogResult = DialogResult.OK; Close(); };
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
    }
}
