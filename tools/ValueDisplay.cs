using System;
using System.Drawing;
using System.Windows.Forms;

namespace ToolEx
{
    public class ValueDisplay : Control
    {
        private string _title = "Value";
        private string _value = "0.00";
        private string _unit = "";
        private Color _valueColor = Color.Blue;
        private Color _titleColor = Color.Gray;

        // GDI+对象缓存
        private Pen _borderPen;
        private SolidBrush _titleBrush;
        private SolidBrush _valueBrush;
        private Font _valueFont;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                Invalidate();
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                Invalidate();
            }
        }

        public string Unit
        {
            get { return _unit; }
            set
            {
                _unit = value;
                Invalidate();
            }
        }

        public Color ValueColor
        {
            get { return _valueColor; }
            set
            {
                _valueColor = value;
                _valueBrush?.Dispose();
                _valueBrush = new SolidBrush(_valueColor);
                Invalidate();
            }
        }

        public Color TitleColor
        {
            get { return _titleColor; }
            set
            {
                _titleColor = value;
                _titleBrush?.Dispose();
                _titleBrush = new SolidBrush(_titleColor);
                Invalidate();
            }
        }

        public ValueDisplay()
        {
            Size = new Size(120, 50);
            BackColor = Color.White;
            Font = new Font("Microsoft YaHei", 9F);
            
            // 初始化GDI+对象
            _borderPen = new Pen(Color.LightGray, 1);
            _titleBrush = new SolidBrush(_titleColor);
            _valueBrush = new SolidBrush(_valueColor);
            _valueFont = new Font(Font.FontFamily, 14F, FontStyle.Bold);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 释放GDI+对象
                _borderPen?.Dispose();
                _titleBrush?.Dispose();
                _valueBrush?.Dispose();
                _valueFont?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // 绘制边框
            g.DrawRectangle(_borderPen, 0, 0, Width - 1, Height - 1);

            // 绘制标题
            g.DrawString(_title, Font, _titleBrush, 5, 5);

            // 绘制数值
            string displayText = _value + " " + _unit;
            g.DrawString(displayText, _valueFont, _valueBrush, 5, 22);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
