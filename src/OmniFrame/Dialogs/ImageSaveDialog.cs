using System;
using System.Drawing;
using System.Windows.Forms;

namespace OmniFrame.Dialogs
{
    /// <summary>
    /// 图片记录设置 — 替代 Qt CImageSave.ui
    /// 配置视觉检测图片保存策略: 类型(NG/OK/所有)、数量、有效期(天)
    /// </summary>
    public class ImageSaveDialog : Form
    {
        private readonly ComboBox _cmbType;
        private readonly NumericUpDown _numCount;
        private readonly NumericUpDown _numValidity;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        /// <summary>保存类型: 0=仅NG, 1=仅OK, 2=所有</summary>
        public int SaveType { get; private set; }
        /// <summary>保存数量上限</summary>
        public int MaxCount { get; private set; } = 100;
        /// <summary>有效期 (天)</summary>
        public int ValidityDays { get; private set; } = 7;

        public ImageSaveDialog(int saveType = 2, int maxCount = 100, int validityDays = 7)
        {
            Text = "图片记录";
            Size = new Size(420, 280);
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
                Text = "图片记录",
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 14F, FontStyle.Bold),
                Location = new Point(12, 8),
                AutoSize = true,
            };
            titleBar.Controls.Add(lblTitle);

            // 类型
            var lblType = new Label
            {
                Text = "类型",
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(60, 65),
                Size = new Size(60, 30),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            _cmbType = new ComboBox
            {
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(130, 65),
                Size = new Size(120, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _cmbType.Items.AddRange(new object[] { "仅NG", "仅OK", "所有" });
            _cmbType.SelectedIndex = saveType;

            // 数量
            var lblCount = new Label
            {
                Text = "数量",
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(60, 110),
                Size = new Size(60, 30),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            _numCount = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 99999,
                Value = maxCount,
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(130, 110),
                Size = new Size(120, 30),
            };

            // 有效期
            var lblValidity = new Label
            {
                Text = "有效期",
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(60, 155),
                Size = new Size(60, 30),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            _numValidity = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 30,
                Value = validityDays,
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(130, 155),
                Size = new Size(120, 30),
            };

            var lblUnit1 = new Label
            {
                Text = "张",
                Font = new Font("Microsoft YaHei", 10F),
                ForeColor = Color.Gray,
                Location = new Point(255, 112),
                Size = new Size(40, 25),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var lblUnit2 = new Label
            {
                Text = "天",
                Font = new Font("Microsoft YaHei", 10F),
                ForeColor = Color.Gray,
                Location = new Point(255, 157),
                Size = new Size(40, 25),
                TextAlign = ContentAlignment.MiddleLeft,
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

            Controls.AddRange(new Control[] {
                lblType, _cmbType, lblCount, _numCount, lblValidity, _numValidity,
                lblUnit1, lblUnit2, titleBar, bottomBar
            });

            _btnOk.Click += (s, e) =>
            {
                SaveType = _cmbType.SelectedIndex;
                MaxCount = (int)_numCount.Value;
                ValidityDays = (int)_numValidity.Value;
                DialogResult = DialogResult.OK;
                Close();
            };
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
