using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OmniFrame.Core;
using OmniFrame.Core.BlockCut;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// 相机调试窗口 — 替代 Qt CameraWidget (.cpp/.h/.ui, ~73 行头)
    /// 提供实时相机预览、单次/连续取像、十字线、缩放、光源控制
    /// </summary>
    public partial class CameraDebugForm : Form
    {
        private readonly int _cameraNo;
        private readonly BlockCutVision _vision;
        private readonly Action<int, bool> _setLight;

        private Image _currentImage;
        private bool _isCrosshair;
        private bool _isContinuous;
        private CancellationTokenSource _continuousCts;
        private double _zoomRatio = 1.0;
        private PointF _panOffset;

        #region Controls

        private Panel _imagePanel;
        private Button _btnOnce, _btnContinuous, _btnCrosshair;
        private Button _btnZoomIn, _btnZoomOut, _btnReset, _btnAutoFit;
        private Label _lblZoom, _lblStatus, _lblResolution;
        private NumericUpDown _numLight1, _numLight2, _numLight3, _numLight4;
        private NumericUpDown _numExposure, _numGain;

        #endregion

        public CameraDebugForm(int cameraNo, BlockCutVision vision, Action<int, bool> setLight = null)
        {
            _cameraNo = cameraNo;
            _vision = vision;
            _setLight = setLight;
            _isCrosshair = false;
            _isContinuous = false;

            InitializeComponent();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [Obsolete("For Designer only - use DI constructor")]
        public CameraDebugForm()
        {
            _cameraNo = 1;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = $"相机{cameraLabel} — 调试";
            Size = new Size(680, 540);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Microsoft YaHei", 9F);
            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();

            // -- 工具栏 --
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.FromArgb(50, 50, 50),
                Padding = new Padding(6, 4, 6, 4),
            };

            _btnOnce = ToolBtn("单次取像", Color.FromArgb(0, 110, 140), 0);
            _btnOnce.Click += async (s, e) => await GrabOnceAsync();

            _btnContinuous = ToolBtn("连续取像", Color.FromArgb(0, 140, 80), 1);
            _btnContinuous.Click += (s, e) => ToggleContinuous();

            _btnCrosshair = ToolBtn("十字线", Color.FromArgb(100, 80, 0), 2);
            _btnCrosshair.Click += (s, e) =>
            {
                _isCrosshair = !_isCrosshair;
                _btnCrosshair.BackColor = _isCrosshair ? Color.FromArgb(200, 160, 0) : Color.FromArgb(100, 80, 0);
                _imagePanel.Invalidate();
            };

            _btnZoomOut = ToolBtn("−", Color.FromArgb(80, 80, 80), 3, 32);
            _btnZoomOut.Click += (s, e) => { _zoomRatio = Math.Max(0.1, _zoomRatio - 0.25); UpdateZoom(); };

            _btnZoomIn = ToolBtn("+", Color.FromArgb(80, 80, 80), 3, 32, 38);
            _btnZoomIn.Click += (s, e) => { _zoomRatio = Math.Min(10.0, _zoomRatio + 0.25); UpdateZoom(); };

            _btnAutoFit = ToolBtn("适应", Color.FromArgb(80, 80, 80), 3, 32, 76);
            _btnAutoFit.Click += (s, e) => { _zoomRatio = 1.0; _panOffset = PointF.Empty; _imagePanel.Invalidate(); };

            _btnReset = ToolBtn("原始", Color.FromArgb(80, 80, 80), 3, 32, 114);
            _btnReset.Click += (s, e) => { _zoomRatio = 1.0; _panOffset = PointF.Empty; _imagePanel.Invalidate(); };

            _lblZoom = new Label
            {
                Text = "100%",
                Location = new Point(152, 8),
                AutoSize = true,
                ForeColor = Color.Cyan,
            };

            toolbar.Controls.AddRange(new Control[] {
                _btnOnce, _btnContinuous, _btnCrosshair,
                _btnZoomOut, _btnZoomIn, _btnAutoFit, _btnReset,
                _lblZoom,
            });

            // -- 图像区域 --
            _imagePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle,
            };
            _imagePanel.Paint += OnImagePanelPaint;
            _imagePanel.MouseDown += OnImageMouseDown;
            _imagePanel.MouseMove += OnImageMouseMove;

            // -- 底部状态栏 --
            var statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = Color.FromArgb(50, 50, 50),
                Padding = new Padding(6, 2, 6, 2),
            };
            _lblStatus = new Label { Text = "就绪", AutoSize = true, ForeColor = Color.Lime, Location = new Point(6, 4) };
            _lblResolution = new Label { Text = "", AutoSize = true, ForeColor = Color.Gray, Location = new Point(160, 4) };
            statusBar.Controls.AddRange(new Control[] { _lblStatus, _lblResolution });

            // -- 右侧参数面板 --
            var paramPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 140,
                BackColor = Color.FromArgb(42, 42, 42),
                Padding = new Padding(6, 6, 6, 6),
            };

            int py = 6;
            var header = NewLabel("相机参数", 6, py, bold: true, color: Color.Cyan); py += 22;

            var lblLight1 = NewLabel("光源1:", 6, py); py += 16;
            _numLight1 = NewNum(6, py, 120, 0, 9999); py += 24;
            _numLight1.ValueChanged += (s, e) => _setLight?.Invoke(1, _numLight1.Value > 0);

            var lblLight2 = NewLabel("光源2:", 6, py); py += 16;
            _numLight2 = NewNum(6, py, 120, 0, 9999); py += 24;
            _numLight2.ValueChanged += (s, e) => _setLight?.Invoke(2, _numLight2.Value > 0);

            var lblLight3 = NewLabel("光源3:", 6, py); py += 16;
            _numLight3 = NewNum(6, py, 120, 0, 9999); py += 24;
            _numLight3.ValueChanged += (s, e) => _setLight?.Invoke(3, _numLight3.Value > 0);

            var lblLight4 = NewLabel("光源4:", 6, py); py += 16;
            _numLight4 = NewNum(6, py, 120, 0, 9999); py += 24;
            _numLight4.ValueChanged += (s, e) => _setLight?.Invoke(4, _numLight4.Value > 0);

            py += 6;
            var lblExp = NewLabel("曝光(us):", 6, py); py += 16;
            _numExposure = NewNum(6, py, 120, 0, 999999); py += 24;

            var lblGain = NewLabel("增益:", 6, py); py += 16;
            _numGain = NewNum(6, py, 120, 0, 1957); py += 24;

            paramPanel.Controls.AddRange(new Control[] {
                header,
                lblLight1, _numLight1, lblLight2, _numLight2,
                lblLight3, _numLight3, lblLight4, _numLight4,
                lblExp, _numExposure, lblGain, _numGain,
            });

            Controls.Add(_imagePanel);
            Controls.Add(paramPanel);
            Controls.Add(toolbar);
            Controls.Add(statusBar);
        }

        #region Camera Operations

        private string cameraLabel => _cameraNo switch
        {
            1 => "底板右侧",
            2 => "底板左侧",
            3 => "片源",
            4 => "辅助",
            _ => $"{_cameraNo}"
        };

        private async Task GrabOnceAsync()
        {
            try
            {
                _lblStatus.Text = "取像中...";
                _lblStatus.ForeColor = Color.Yellow;

                // In production: await _camera[_cameraNo].GrabOneAsync()
                // For now: generate placeholder image
                await Task.Delay(50);
                _currentImage = GeneratePlaceholderImage();
                _lblStatus.Text = "取像完成";
                _lblStatus.ForeColor = Color.Lime;
                _lblResolution.Text = _currentImage != null
                    ? $"{_currentImage.Width} × {_currentImage.Height}"
                    : "";
                _imagePanel.Invalidate();
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"取像失败: {ex.Message}";
                _lblStatus.ForeColor = Color.Red;
            }
        }

        private void ToggleContinuous()
        {
            if (_isContinuous)
            {
                _isContinuous = false;
                _continuousCts?.Cancel();
                _btnContinuous.Text = "连续取像";
                _btnContinuous.BackColor = Color.FromArgb(0, 140, 80);
                _lblStatus.Text = "连续取像已停止";
            }
            else
            {
                _isContinuous = true;
                _btnContinuous.Text = "停止";
                _btnContinuous.BackColor = Color.FromArgb(180, 60, 0);
                _continuousCts = new CancellationTokenSource();
                var token = _continuousCts.Token;
                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        await GrabOnceAsync();
                        await Task.Delay(100, token);
                    }
                }, token);
            }
        }

        private Bitmap GeneratePlaceholderImage()
        {
            var bmp = new Bitmap(640, 480);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(30, 30, 30));
                // Simulated cross pattern for alignment
                using (var pen = new Pen(Color.FromArgb(60, 60, 60), 1))
                {
                    g.DrawLine(pen, bmp.Width / 2, 0, bmp.Width / 2, bmp.Height);
                    g.DrawLine(pen, 0, bmp.Height / 2, bmp.Width, bmp.Height / 2);
                }
                // Pseudo workpiece rectangle
                var r = new Rectangle(bmp.Width / 4, bmp.Height / 4, bmp.Width / 2, bmp.Height / 2);
                g.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 60)), r);
                g.DrawRectangle(new Pen(Color.FromArgb(100, 100, 120), 1), r);

                g.DrawString($"Camera {_cameraNo} — {cameraLabel}\n(Halcon DLL pending)",
                    new Font("Microsoft YaHei", 11F), Brushes.DarkGray,
                    new RectangleF(0, 0, bmp.Width, bmp.Height),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            return bmp;
        }

        #endregion

        #region Image Display

        private void OnImagePanelPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (_currentImage != null)
            {
                g.TranslateTransform(_imagePanel.Width / 2f, _imagePanel.Height / 2f);
                g.ScaleTransform((float)_zoomRatio, (float)_zoomRatio);
                g.TranslateTransform(-_imagePanel.Width / 2f + _panOffset.X, -_imagePanel.Height / 2f + _panOffset.Y);

                var destRect = new Rectangle(
                    (_imagePanel.Width - (int)(_currentImage.Width * _zoomRatio)) / 2,
                    (_imagePanel.Height - (int)(_currentImage.Height * _zoomRatio)) / 2,
                    (int)(_currentImage.Width * _zoomRatio),
                    (int)(_currentImage.Height * _zoomRatio));
                if (destRect.Width > 0 && destRect.Height > 0)
                    g.DrawImage(_currentImage, destRect);
            }
            else
            {
                g.DrawString("无图像", Font, Brushes.DarkGray,
                    _imagePanel.ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            if (_isCrosshair)
            {
                g.ResetTransform();
                using (var pen = new Pen(Color.FromArgb(120, Color.Lime), 1))
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawLine(pen, _imagePanel.Width / 2, 0, _imagePanel.Width / 2, _imagePanel.Height);
                    g.DrawLine(pen, 0, _imagePanel.Height / 2, _imagePanel.Width, _imagePanel.Height / 2);
                }
            }
        }

        private Point _lastMousePos;
        private void OnImageMouseDown(object sender, MouseEventArgs e)
        {
            _lastMousePos = e.Location;
        }

        private void OnImageMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _panOffset = new PointF(
                    _panOffset.X + (e.X - _lastMousePos.X),
                    _panOffset.Y + (e.Y - _lastMousePos.Y));
                _lastMousePos = e.Location;
                _imagePanel.Invalidate();
            }
        }

        private void UpdateZoom()
        {
            _lblZoom.Text = $"{_zoomRatio * 100:F0}%";
            _imagePanel.Invalidate();
        }

        #endregion

        #region Helpers

        private static Button ToolBtn(string text, Color back, int order, int width = 78, int? xOverride = null)
        {
            int x = xOverride ?? (order * 84 + 6);
            return new Button
            {
                Text = text,
                Location = new Point(x, 4),
                Size = new Size(width, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F),
            };
        }

        private static Label NewLabel(string text, int x, int y, bool bold = false, Color? color = null)
            => new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = color ?? Color.White,
                Font = new Font("Microsoft YaHei", 8F, bold ? FontStyle.Bold : FontStyle.Regular),
            };

        private static NumericUpDown NewNum(int x, int y, int w, decimal min, decimal max)
            => new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(w, 22),
                Minimum = min,
                Maximum = max,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F),
            };

        #endregion

        #region Cleanup

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isContinuous)
            {
                _isContinuous = false;
                _continuousCts?.Cancel();
            }
            _currentImage?.Dispose();
            base.OnFormClosing(e);
        }

        // Exposed for FitLineToolForm integration
        public Image CurrentImage => _currentImage;
        public event Action<Image> OnImageGrabbed;

        #endregion
    }
}
