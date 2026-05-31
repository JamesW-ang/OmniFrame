using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ToolEx
{
    public class RoundButton : Button
    {
        private int _cornerRadius = 10;
        private Color _borderColor = Color.Gray;
        private int _borderWidth = 1;
        private bool _alwaysShowBorder = false;
        private Color _baseColor = Color.Green;
        private Color _baseColorEnd = Color.Green;
        private int _imageHeight = 80;
        private int _imageWidth = 80;
        private int _imageTextSpace = 0;
        private int _spliteButtonWidth = 18;
        private ImageList _imageList;

        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        public int BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                _borderWidth = value;
                Invalidate();
            }
        }

        public bool AlwaysShowBorder
        {
            get { return _alwaysShowBorder; }
            set { _alwaysShowBorder = value; }
        }

        public Color BaseColor
        {
            get { return _baseColor; }
            set { _baseColor = value; }
        }

        public Color BaseColorEnd
        {
            get { return _baseColorEnd; }
            set { _baseColorEnd = value; }
        }

        public int ImageHeight
        {
            get { return _imageHeight; }
            set { _imageHeight = value; }
        }

        public int ImageWidth
        {
            get { return _imageWidth; }
            set { _imageWidth = value; }
        }

        public int ImageTextSpace
        {
            get { return _imageTextSpace; }
            set { _imageTextSpace = value; }
        }

        public int SpliteButtonWidth
        {
            get { return _spliteButtonWidth; }
            set { _spliteButtonWidth = value; }
        }

        public new ImageList ImageList
        {
            get { return _imageList; }
            set { _imageList = value; }
        }

        public int Radius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = value;
                Invalidate();
            }
        }

        public RoundButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(0, 122, 204);
            ForeColor = Color.White;
            Font = new Font("Microsoft YaHei", 10F, FontStyle.Regular);
            Size = new Size(100, 40);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = GetRoundedRect(rect, _cornerRadius))
            {
                // 绘制背景
                using (SolidBrush brush = new SolidBrush(Enabled ? BackColor : Color.Gray))
                {
                    g.FillPath(brush, path);
                }

                // 绘制边框
                if (_borderWidth > 0)
                {
                    using (Pen pen = new Pen(_borderColor, _borderWidth))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // 绘制文字
                TextRenderer.DrawText(g, Text, Font, rect, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

            // 左上角
            path.AddArc(arcRect, 180, 90);

            // 右上角
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // 右下角
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // 左下角
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);

            path.CloseFigure();
            return path;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
