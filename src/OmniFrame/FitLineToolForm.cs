using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.Core.BlockCut;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// 直线拟合视觉标定工具 — 替代 Qt FitLineWidget (.cpp/.h/.ui, ~134 行头)
    /// 提供 ROI 编辑、FitLine 参数调整、运行、结果显示
    /// </summary>
    public partial class FitLineToolForm : Form
    {
        private readonly string _title;
        private readonly CameraDebugForm _cameraForm;
        private readonly BlockCutVision _vision;

        private ROIRegion _roi;
        private FitLineParams _fitParams;
        private Point2D _startPt, _endPt;
        private double _angle;
        private List<Point2D> _fitPoints;
        private bool _resultReady;

        // ROI drag state
        private bool _draggingRoi;
        private Point _dragStart;
        private int _dragHandle = -1; // -1=move, 0-7=corner handles

        #region Controls

        private Panel _imagePanel;
        private NumericUpDown _numThreshold, _numSigma;
        private ComboBox _cmbTransition, _cmbSelect, _cmbInterpolation;
        private NumericUpDown _numSegWidth, _numSegStep, _numSegNum;
        private NumericUpDown _numMaxPoints, _numIterations, _numDistFactor;
        private Button _btnRun, _btnCreateParams, _btnOpenFile;
        private Label _lblResultStart, _lblResultEnd, _lblResultAngle;
        private TextBox _txtImagePath;

        #endregion

        public event Action<CameraDebugForm> CameraWidgetChange;

        public FitLineToolForm(string title, CameraDebugForm cameraForm, BlockCutVision vision)
        {
            _title = title;
            _cameraForm = cameraForm;
            _vision = vision;
            _roi = new ROIRegion();
            _fitParams = new FitLineParams();
            _fitPoints = new List<Point2D>();

            InitializeComponent();
            CopyFitParamsToUI();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [Obsolete("For Designer only - use DI constructor")]
        public FitLineToolForm()
        {
            _title = "直线拟合工具";
            _roi = new ROIRegion();
            _fitParams = new FitLineParams();
            _fitPoints = new List<Point2D>();

            InitializeComponent();
            CopyFitParamsToUI();
        }

        private void InitializeComponent()
        {
            Text = $"直线拟合 — {_title}";
            Size = new Size(820, 560);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Microsoft YaHei", 9F);
            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();

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
            _imagePanel.MouseUp += OnImageMouseUp;

            // -- 右侧参数面板 --
            var rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 280,
                BackColor = Color.FromArgb(42, 42, 42),
                AutoScroll = true,
                Padding = new Padding(6, 6, 6, 6),
            };

            int py = 6;
            // 基本参数
            var gBasic = NewGroup("基本参数", ref py);
            _cmbTransition = NewCombo(gBasic, "边缘方向:", ref py,
                "从黑到白", "从白到黑", "双方向", "两直线");
            _cmbTransition.SelectedIndexChanged += (s, e) => { _fitParams.Transition = _cmbTransition.SelectedIndex; OnParamChanged(); };

            _cmbSelect = NewCombo(gBasic, "检测方向:", ref py,
                "最近开始", "最近末尾");
            _cmbSelect.SelectedIndexChanged += (s, e) => { _fitParams.Select = _cmbSelect.SelectedIndex; OnParamChanged(); };

            _numThreshold = NewNum(gBasic, "梯度下限:", ref py, 0, 255, _fitParams.Threshold);
            _numThreshold.ValueChanged += (s, e) => { _fitParams.Threshold = (int)_numThreshold.Value; OnParamChanged(); };

            _numSigma = NewNum(gBasic, "滤波系数:", ref py, 0.1M, 32M, (decimal)_fitParams.Sigma, 2, 0.1M);
            _numSigma.ValueChanged += (s, e) => { _fitParams.Sigma = (double)_numSigma.Value; OnParamChanged(); };
            gBasic.Height = py - gBasic.Top + 4;

            // 高级参数
            py += 6;
            var gAdvanced = NewGroup("高级参数", ref py);
            _numSegWidth = NewNum(gAdvanced, "段宽:", ref py, 1, 10000, _fitParams.SegWidth);
            _numSegWidth.ValueChanged += (s, e) => { _fitParams.SegWidth = (int)_numSegWidth.Value; OnParamChanged(); };

            _numSegStep = NewNum(gAdvanced, "段间距:", ref py, 1, 10000, _fitParams.SegStep);
            _numSegStep.ValueChanged += (s, e) => { _fitParams.SegStep = (int)_numSegStep.Value; OnParamChanged(); };

            _numSegNum = NewNum(gAdvanced, "段个数:", ref py, 1, 999, _fitParams.SegNum);
            _numSegNum.ValueChanged += (s, e) => { _fitParams.SegNum = (int)_numSegNum.Value; OnParamChanged(); };

            _cmbInterpolation = NewCombo(gAdvanced, "插值方法:", ref py,
                "最小二乘法", "二次矩", "三次矩");
            _cmbInterpolation.SelectedIndexChanged += (s, e) => { _fitParams.Interpolation = _cmbInterpolation.SelectedIndex + 1; OnParamChanged(); };

            _numDistFactor = NewNum(gAdvanced, "距离阈值:", ref py, 0.1M, 100M, (decimal)_fitParams.DistanceFactor, 2, 0.1M);
            _numDistFactor.ValueChanged += (s, e) => { _fitParams.DistanceFactor = (double)_numDistFactor.Value; OnParamChanged(); };

            _numMaxPoints = NewNum(gAdvanced, "最大拟合点数:", ref py, 1, 99999, _fitParams.MaxNumPoints);
            _numMaxPoints.ValueChanged += (s, e) => { _fitParams.MaxNumPoints = (int)_numMaxPoints.Value; OnParamChanged(); };

            _numIterations = NewNum(gAdvanced, "迭代次数:", ref py, 1, 999999, _fitParams.Iterations);
            _numIterations.ValueChanged += (s, e) => { _fitParams.Iterations = (int)_numIterations.Value; OnParamChanged(); };
            gAdvanced.Height = py - gAdvanced.Top + 4;

            // ROI 参数
            py += 6;
            var gRoi = NewGroup("ROI 区域", ref py);
            var numRoiCX = NewNumInline(gRoi, "中心X:", ref py, 0, 2000, (decimal)_roi.CenterX);
            numRoiCX.ValueChanged += (s, e) => { _roi.CenterX = (double)numRoiCX.Value; _imagePanel.Invalidate(); };
            var numRoiCY = NewNumInline(gRoi, "中心Y:", ref py, 0, 2000, (decimal)_roi.CenterY);
            numRoiCY.ValueChanged += (s, e) => { _roi.CenterY = (double)numRoiCY.Value; _imagePanel.Invalidate(); };
            var numRoiW = NewNumInline(gRoi, "宽度:", ref py, 1, 4000, (decimal)_roi.Width);
            numRoiW.ValueChanged += (s, e) => { _roi.Width = (double)numRoiW.Value; _imagePanel.Invalidate(); };
            var numRoiH = NewNumInline(gRoi, "高度:", ref py, 1, 4000, (decimal)_roi.Height);
            numRoiH.ValueChanged += (s, e) => { _roi.Height = (double)numRoiH.Value; _imagePanel.Invalidate(); };
            var numRoiAngle = NewNumInline(gRoi, "角度:", ref py, -180, 180, (decimal)_roi.Angle);
            numRoiAngle.ValueChanged += (s, e) => { _roi.Angle = (double)numRoiAngle.Value; _imagePanel.Invalidate(); };
            gRoi.Height = py - gRoi.Top + 4;

            // 按钮
            py += 8;
            _btnOpenFile = NewButton("导入图片", py, Color.FromArgb(60, 60, 140));
            _btnOpenFile.Click += (s, e) =>
            {
                using var dlg = new OpenFileDialog { Filter = "图片|*.bmp;*.jpg;*.png;*.tif" };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _txtImagePath.Text = dlg.FileName;
                    try
                    {
                        _cameraForm?.GetType().GetProperty("CurrentImage")?.SetValue(_cameraForm, Image.FromFile(dlg.FileName));
                        _imagePanel.Invalidate();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("加载图片失败", ex);
                        MessageBox.Show($"加载图片失败:\n{ex.Message}\n\n堆栈跟踪:\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            _txtImagePath = new TextBox
            {
                Location = new Point(78, py + 2),
                Size = new Size(160, 22),
                ReadOnly = true,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.Gray,
                Text = "(使用相机取像或导入)",
            };

            py += 28;
            _btnCreateParams = NewButton("创建参数", py, Color.FromArgb(0, 100, 140));
            _btnCreateParams.Click += (s, e) =>
            {
                CopyFitParamsToUI();
                UpdateSearchSegments();
                _imagePanel.Invalidate();
            };

            _btnRun = NewButton("▶ 运行", py, Color.FromArgb(0, 140, 0), 80);
            _btnRun.Location = new Point(106, py);
            _btnRun.Click += async (s, e) => await RunFitLineAsync();

            // 结果
            py += 34;
            var gResult = NewGroup("拟合结果", ref py);
            _lblResultStart = NewResultLabel(gResult, "起点:", ref py);
            _lblResultEnd = NewResultLabel(gResult, "终点:", ref py);
            _lblResultAngle = NewResultLabel(gResult, "角度:", ref py);
            gResult.Height = py - gResult.Top + 4;

            // OK/Cancel
            py += 12;
            var btnOK = new Button
            {
                Text = "确定",
                Location = new Point(28, py),
                Size = new Size(90, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 0),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 10F),
            };
            btnOK.Click += (s, e) => { SaveSettings(); DialogResult = DialogResult.OK; Close(); };

            var btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(130, py),
                Size = new Size(90, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 10F),
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            rightPanel.Controls.AddRange(new Control[] {
                gBasic, gAdvanced, gRoi,
                _btnOpenFile, _txtImagePath,
                _btnCreateParams, _btnRun,
                gResult, btnOK, btnCancel,
            });

            Controls.Add(_imagePanel);
            Controls.Add(rightPanel);
        }

        #region Vision Operations

        private async System.Threading.Tasks.Task RunFitLineAsync()
        {
            try
            {
                var image = _cameraForm?.CurrentImage;
                if (image == null)
                {
                    MessageBox.Show("请先取像或导入图片", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _btnRun.Text = "运行中...";
                _btnRun.Enabled = false;

                Point2D p1 = default, p2 = default;
                double angle = 0;
                bool success = await System.Threading.Tasks.Task.Run(() =>
                    _vision.FitLine(image, _roi, _fitParams, out p1, out p2, out angle));

                if (success)
                {
                    _startPt = p1;
                    _endPt = p2;
                    _angle = angle;
                    _resultReady = true;

                    _lblResultStart.Text = $"起点: ({p1.X:F2}, {p1.Y:F2})";
                    _lblResultEnd.Text = $"终点: ({p2.X:F2}, {p2.Y:F2})";
                    _lblResultAngle.Text = $"角度: {angle:F4}°";
                }
                else
                {
                    _lblResultStart.Text = "拟合失败";
                    _lblResultEnd.Text = "";
                    _lblResultAngle.Text = "";
                }

                _imagePanel.Invalidate();
            }
            catch (Exception ex)
            {
                Logger.Error("拟合线工具运行失败", ex);
                MessageBox.Show($"运行失败:\n{ex.Message}\n\n堆栈跟踪:\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnRun.Text = "▶ 运行";
                _btnRun.Enabled = true;
            }
        }

        private void UpdateSearchSegments()
        {
            // Convert ROI region to visual segments for drawing
            if (_roi == null) return;
            _fitPoints.Clear();
            // Generate visual segment lines based on SegWidth/SegStep/SegNum
            double roiLeft = _roi.CenterX - _roi.Width / 2;
            double roiTop = _roi.CenterY - _roi.Height / 2;
            double segPitch = (_roi.Height - _fitParams.SegWidth * _fitParams.SegNum - _fitParams.SegStep * (_fitParams.SegNum - 1));
        }

        private void SaveSettings()
        {
            try
            {
                var iniPath = System.IO.Path.Combine(AppContext.BaseDirectory, "system.ini");
                var prefix = $"FitLine/{_title}";
                IniHelper.WriteIni(prefix, "Threshold", _fitParams.Threshold.ToString(), iniPath);
                IniHelper.WriteIni(prefix, "Sigma", _fitParams.Sigma.ToString("F2"), iniPath);
                IniHelper.WriteIni(prefix, "Transition", _fitParams.Transition.ToString(), iniPath);
                IniHelper.WriteIni(prefix, "Select", _fitParams.Select.ToString(), iniPath);
                IniHelper.WriteIni(prefix, "SegWidth", _fitParams.SegWidth.ToString(), iniPath);
                IniHelper.WriteIni(prefix, "SegStep", _fitParams.SegStep.ToString(), iniPath);
                IniHelper.WriteIni(prefix, "SegNum", _fitParams.SegNum.ToString(), iniPath);
                IniHelper.WriteIni(prefix, "Interpolation", _fitParams.Interpolation.ToString(), iniPath);
                IniHelper.WriteIni(prefix, "MaxNumPoints", _fitParams.MaxNumPoints.ToString(), iniPath);
                IniHelper.WriteIni(prefix, "Iterations", _fitParams.Iterations.ToString(), iniPath);
                IniHelper.WriteIni(prefix, "DistanceFactor", _fitParams.DistanceFactor.ToString("F2"), iniPath);
                IniHelper.WriteIni(prefix, "RoiCX", _roi.CenterX.ToString("F2"), iniPath);
                IniHelper.WriteIni(prefix, "RoiCY", _roi.CenterY.ToString("F2"), iniPath);
                IniHelper.WriteIni(prefix, "RoiWidth", _roi.Width.ToString("F2"), iniPath);
                IniHelper.WriteIni(prefix, "RoiHeight", _roi.Height.ToString("F2"), iniPath);
                IniHelper.WriteIni(prefix, "RoiAngle", _roi.Angle.ToString("F2"), iniPath);
            }
            catch (Exception ex) { Logger.Error("FitLineTool操作失败", ex); }
        }

        #endregion

        #region Image Display & ROI Editing

        private void OnImagePanelPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            var image = _cameraForm?.CurrentImage;
            if (image != null)
            {
                g.DrawImage(image, _imagePanel.ClientRectangle);
            }
            else
            {
                g.DrawString("无图像\n请从相机取像或导入图片", Font, Brushes.DarkGray,
                    _imagePanel.ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            // Draw ROI rectangle
            using (var roiPen = new Pen(Color.FromArgb(200, Color.Lime), 2))
            {
                roiPen.DashStyle = DashStyle.Solid;
                var rect = MapRoiToScreen(_roi);
                g.DrawRectangle(roiPen, rect.X, rect.Y, rect.Width, rect.Height);

                // Corner handles
                int handleSize = 7;
                using (var handleBrush = new SolidBrush(Color.Lime))
                {
                    g.FillRectangle(handleBrush, rect.Left - handleSize / 2, rect.Top - handleSize / 2, handleSize, handleSize);
                    g.FillRectangle(handleBrush, rect.Right - handleSize / 2, rect.Top - handleSize / 2, handleSize, handleSize);
                    g.FillRectangle(handleBrush, rect.Left - handleSize / 2, rect.Bottom - handleSize / 2, handleSize, handleSize);
                    g.FillRectangle(handleBrush, rect.Right - handleSize / 2, rect.Bottom - handleSize / 2, handleSize, handleSize);
                }

                // ROI center label
                g.DrawString($"ROI ({_roi.CenterX:F0},{_roi.CenterY:F0})",
                    new Font("Microsoft YaHei", 7F), Brushes.Lime,
                    rect.Left + 2, rect.Top - 14);
            }

            // Draw search segments
            if (_fitPoints.Count > 0)
            {
                using (var segPen = new Pen(Color.FromArgb(100, Color.Cyan), 1))
                {
                    // Draw segment visualization
                }
            }

            // Draw fit result line
            if (_resultReady)
            {
                var p1 = MapPointToScreen(_startPt);
                var p2 = MapPointToScreen(_endPt);
                using (var linePen = new Pen(Color.Red, 2))
                using (var circBrush = new SolidBrush(Color.Red))
                {
                    g.DrawLine(linePen, p1, p2);
                    g.FillEllipse(circBrush, p1.X - 4, p1.Y - 4, 8, 8);
                    g.FillEllipse(circBrush, p2.X - 4, p2.Y - 4, 8, 8);
                }
            }
        }

        private RectangleF MapRoiToScreen(ROIRegion roi)
        {
            // Simple linear mapping — scale to fit panel
            float scaleX = (float)_imagePanel.Width / 640f;
            float scaleY = (float)_imagePanel.Height / 480f;
            return new RectangleF(
                (float)(roi.CenterX - roi.Width / 2) * scaleX,
                (float)(roi.CenterY - roi.Height / 2) * scaleY,
                (float)roi.Width * scaleX,
                (float)roi.Height * scaleY);
        }

        private PointF MapPointToScreen(Point2D pt)
        {
            float scaleX = (float)_imagePanel.Width / 640f;
            float scaleY = (float)_imagePanel.Height / 480f;
            return new PointF((float)pt.X * scaleX, (float)pt.Y * scaleY);
        }

        private void OnImageMouseDown(object sender, MouseEventArgs e)
        {
            var roiRect = MapRoiToScreen(_roi);
            _dragHandle = HitTestHandle(roiRect, e.Location);
            _draggingRoi = true;
            _dragStart = e.Location;
        }

        private void OnImageMouseMove(object sender, MouseEventArgs e)
        {
            if (!_draggingRoi) return;

            float scaleX = 640f / (float)_imagePanel.Width;
            float scaleY = 480f / (float)_imagePanel.Height;
            float dx = (e.X - _dragStart.X) * scaleX;
            float dy = (e.Y - _dragStart.Y) * scaleY;

            if (_dragHandle == -1) // Move
            {
                _roi.CenterX += dx;
                _roi.CenterY += dy;
            }
            else if (_dragHandle == 0) // Top-left
            {
                _roi.CenterX += dx / 2;
                _roi.CenterY += dy / 2;
                _roi.Width -= dx;
                _roi.Height -= dy;
            }
            else if (_dragHandle == 2) // Bottom-right
            {
                _roi.CenterX += dx / 2;
                _roi.CenterY += dy / 2;
                _roi.Width += dx;
                _roi.Height += dy;
            }

            _roi.Width = Math.Max(10, _roi.Width);
            _roi.Height = Math.Max(10, _roi.Height);
            _dragStart = e.Location;
            _imagePanel.Invalidate();
        }

        private void OnImageMouseUp(object sender, MouseEventArgs e)
        {
            _draggingRoi = false;
            _dragHandle = -1;
            UpdateSearchSegments();
        }

        private int HitTestHandle(RectangleF roi, Point mouse, int threshold = 10)
        {
            float x = mouse.X, y = mouse.Y;
            if (Math.Abs(x - roi.Left) < threshold && Math.Abs(y - roi.Top) < threshold) return 0;     // TL
            if (Math.Abs(x - roi.Right) < threshold && Math.Abs(y - roi.Top) < threshold) return 1;    // TR
            if (Math.Abs(x - roi.Right) < threshold && Math.Abs(y - roi.Bottom) < threshold) return 2; // BR
            if (Math.Abs(x - roi.Left) < threshold && Math.Abs(y - roi.Bottom) < threshold) return 3;  // BL
            if (roi.Contains(x, y)) return -1; // Move
            return -2; // None
        }

        #endregion

        #region Helpers

        private void CopyFitParamsToUI()
        {
            _cmbTransition.SelectedIndex = _fitParams.Transition;
            _cmbSelect.SelectedIndex = _fitParams.Select;
            _numThreshold.Value = _fitParams.Threshold;
            _numSigma.Value = (decimal)_fitParams.Sigma;
            _numSegWidth.Value = _fitParams.SegWidth;
            _numSegStep.Value = _fitParams.SegStep;
            _numSegNum.Value = _fitParams.SegNum;
            _cmbInterpolation.SelectedIndex = _fitParams.Interpolation - 1 >= 0 ? _fitParams.Interpolation - 1 : 0;
            _numMaxPoints.Value = _fitParams.MaxNumPoints;
            _numIterations.Value = _fitParams.Iterations;
            _numDistFactor.Value = (decimal)_fitParams.DistanceFactor;
        }

        private void OnParamChanged()
        {
            UpdateSearchSegments();
            if (_btnRun.Enabled) _imagePanel.Invalidate();
        }

        private GroupBox NewGroup(string text, ref int y)
        {
            var g = new GroupBox
            {
                Text = text,
                Location = new Point(4, y),
                Size = new Size(254, 100),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F, FontStyle.Bold),
            };
            y += 18;
            return g;
        }

        private ComboBox NewCombo(GroupBox parent, string label, ref int y, params string[] items)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(8, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Microsoft YaHei", 8F),
            });
            var cmb = new ComboBox
            {
                Location = new Point(80, y - 2),
                Size = new Size(165, 22),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F),
            };
            cmb.Items.AddRange(items);
            cmb.SelectedIndex = 0;
            parent.Controls.Add(cmb);
            y += 24;
            return cmb;
        }

        private NumericUpDown NewNum(GroupBox parent, string label, ref int y,
            decimal min, decimal max, decimal value, int decimals = 0, decimal inc = 1)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(8, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Microsoft YaHei", 8F),
            });
            var num = new NumericUpDown
            {
                Location = new Point(120, y - 2),
                Size = new Size(125, 22),
                Minimum = min,
                Maximum = max,
                Value = value,
                DecimalPlaces = decimals,
                Increment = inc,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F),
            };
            parent.Controls.Add(num);
            y += 24;
            return num;
        }

        private NumericUpDown NewNumInline(GroupBox parent, string label, ref int y,
            decimal min, decimal max, decimal value, int decimals = 0, decimal inc = 1)
        {
            return NewNum(parent, label, ref y, min, max, value, decimals, inc);
        }

        private Label NewResultLabel(GroupBox parent, string label, ref int y)
        {
            var lbl = new Label
            {
                Text = $"{label} --",
                Location = new Point(8, y),
                AutoSize = true,
                ForeColor = Color.Cyan,
                Font = new Font("Consolas", 9F),
            };
            parent.Controls.Add(lbl);
            y += 20;
            return lbl;
        }

        private Button NewButton(string text, int y, Color back, int width = 90)
            => new Button
            {
                Text = text,
                Location = new Point(28, y),
                Size = new Size(width, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F),
            };

        #endregion

        #region Public API

        public void GetResult(out Point2D p1, out Point2D p2, out double angle)
        {
            p1 = _startPt;
            p2 = _endPt;
            angle = _angle;
        }

        public void UpdateRegion(Point2D p1, Point2D p2)
        {
            _roi.CenterX = (p1.X + p2.X) / 2;
            _roi.CenterY = (p1.Y + p2.Y) / 2;
            _roi.Width = Math.Abs(p2.X - p1.X) + 20;
            _roi.Height = Math.Abs(p2.Y - p1.Y) + 20;
            _imagePanel.Invalidate();
        }

        public void ClearOverlays()
        {
            _resultReady = false;
            _fitPoints.Clear();
            _imagePanel.Invalidate();
        }

        public FitLineParams GetFitLineParams() => _fitParams;
        public ROIRegion GetROIRegion() => _roi;

        #endregion
    }
}
