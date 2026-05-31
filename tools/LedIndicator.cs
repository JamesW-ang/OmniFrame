using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ToolEx
{
    public class LedIndicator : Control
    {
        private bool _isOn = false;
        private Color _onColor = Color.Lime;
        private Color _offColor = Color.Gray;
        private string _labelText = "";

        // GDI+对象缓存
        private SolidBrush _onBrush;
        private SolidBrush _offBrush;
        private Pen _borderPen;

        public bool IsOn
        {
            get { return _isOn; }
            set
            {
                _isOn = value;
                Invalidate();
            }
        }

        public Color OnColor
        {
            get { return _onColor; }
            set
            {
                _onColor = value;
                _onBrush?.Dispose();
                _onBrush = new SolidBrush(_onColor);
                Invalidate();
            }
        }

        public Color OffColor
        {
            get { return _offColor; }
            set
            {
                _offColor = value;
                _offBrush?.Dispose();
                _offBrush = new SolidBrush(_offColor);
                Invalidate();
            }
        }

        public string LabelText
        {
            get { return _labelText; }
            set
            {
                _labelText = value;
                Invalidate();
            }
        }

        public LedIndicator()
        {
            Size = new Size(60, 20);
            BackColor = Color.White;
            
            // 初始化GDI+对象
            _onBrush = new SolidBrush(_onColor);
            _offBrush = new SolidBrush(_offColor);
            _borderPen = new Pen(Color.DarkGray, 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 释放GDI+对象
                _onBrush?.Dispose();
                _offBrush?.Dispose();
                _borderPen?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 绘制LED圆形
            int ledSize = Math.Min(Width / 3, Height - 4);
            Rectangle ledRect = new Rectangle(2, (Height - ledSize) / 2, ledSize, ledSize);

            // 使用缓存的brush
            SolidBrush currentBrush = _isOn ? _onBrush : _offBrush;
            g.FillEllipse(currentBrush, ledRect);

            // 使用缓存的pen绘制边框
            g.DrawEllipse(_borderPen, ledRect);

            // 绘制标签
            if (!string.IsNullOrEmpty(_labelText))
            {
                Rectangle textRect = new Rectangle(ledSize + 6, 0, Width - ledSize - 8, Height);
                TextRenderer.DrawText(g, _labelText, Font, textRect, ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
