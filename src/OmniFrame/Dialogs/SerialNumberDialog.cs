using System;
using System.Drawing;
using System.Windows.Forms;

namespace OmniFrame.Dialogs
{
    /// <summary>
    /// 手动输入条码 — 替代 Qt SNDialog.ui
    /// 扫码失败时弹出，操作员手动输入清洗篮/底板二维码
    /// </summary>
    public class SerialNumberDialog : Form
    {
        private readonly TextBox _txtBarcode;
        private readonly Label _lblInfo;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public string Barcode { get; private set; }

        /// <param name="prompt">提示文字 (如 "清洗篮扫码失败，请手动输入清洗篮二维码！")</param>
        /// <param name="defaultValue">预填值</param>
        public SerialNumberDialog(string prompt = "扫码失败，请手动输入二维码！", string defaultValue = "")
        {
            Text = "手动输入";
            Size = new Size(420, 250);
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
                Text = prompt,
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(40, 60),
                Size = new Size(340, 40),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var lblBarcode = new Label
            {
                Text = "二维码",
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(40, 115),
                Size = new Size(65, 30),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            _txtBarcode = new TextBox
            {
                Text = defaultValue,
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(110, 115),
                Size = new Size(260, 30),
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

            Controls.AddRange(new Control[] { _lblInfo, lblBarcode, _txtBarcode, titleBar, bottomBar });

            _btnOk.Click += (s, e) =>
            {
                Barcode = _txtBarcode.Text.Trim();
                if (string.IsNullOrEmpty(Barcode)) return;
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
