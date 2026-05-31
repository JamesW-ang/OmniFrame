using System;
using System.Drawing;
using System.Windows.Forms;
using MotionIO;
using OmniFrame.Core.BlockCut;
using OmniFrame.Theme;

namespace OmniFrame
{
    public partial class BlockCutDebugForm : Form
    {
        private readonly MotionIO.Motion _motion;
        private readonly BlockCutVision _vision;
        private readonly BlockCutConfig _cfg;

        private WorkManagementForm _workMgrForm;
        private CameraDebugForm _cameraDebugForm1, _cameraDebugForm2;
        private FitLineToolForm _fitLineToolForm1, _fitLineToolForm2;
        private MotionSetForm _motionSetForm;

        public BlockCutDebugForm(MotionIO.Motion motion, BlockCutVision vision, BlockCutConfig cfg)
        {
            _motion = motion;
            _vision = vision;
            _cfg = cfg;
            InitializeComponent();
            WireEvents();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [System.Obsolete("For Designer only - use DI constructor")]
        public BlockCutDebugForm()
        {
            InitializeComponent();
        }

        private void WireEvents()
        {
            btnWorkMgr.Click += (s, e) => OpenWorkManagement();
            btnCameraDebug.Click += (s, e) => OpenCameraDebug();
            btnFitLine.Click += (s, e) => OpenFitLineTool();
            btnMotionSet.Click += (s, e) => OpenMotionSet();
        }

        private void OpenWorkManagement()
        {
            if (_workMgrForm == null || _workMgrForm.IsDisposed)
            {
                _workMgrForm = new WorkManagementForm();
                _workMgrForm.ChangeWork += (changed, data, name, indices) =>
                {
                    LogInfo($"[工单] 切换至: {name}, 有变更: {changed}");
                    if (data.TryGetValue("TrayRows", out var tr)) _cfg.TrayRows = Convert.ToInt32(tr);
                    if (data.TryGetValue("TrayCols", out var tc)) _cfg.TrayCols = Convert.ToInt32(tc);
                    if (data.TryGetValue("BottomRows", out var br)) _cfg.BottomRows = Convert.ToInt32(br);
                    if (data.TryGetValue("BottomCols", out var bc)) _cfg.BottomCols = Convert.ToInt32(bc);
                    if (data.TryGetValue("TrayRowSpace", out var trs)) _cfg.TrayRowSpace = Convert.ToDouble(trs);
                    if (data.TryGetValue("TrayColSpace", out var tcs)) _cfg.TrayColSpace = Convert.ToDouble(tcs);
                    if (data.TryGetValue("BottomRowSpace", out var brs)) _cfg.BottomRowSpace = Convert.ToDouble(brs);
                    if (data.TryGetValue("BottomColSpace", out var bcs)) _cfg.BottomColSpace = Convert.ToDouble(bcs);
                    if (data.TryGetValue("CasselCount", out var cc)) _cfg.CasselCount = Convert.ToInt32(cc);
                    if (data.TryGetValue("CasselSpace", out var cs)) _cfg.CasselZSpace = Convert.ToDouble(cs);
                };
            }
            _workMgrForm.ShowDialog(this);
        }

        private void OpenCameraDebug()
        {
            using var selDlg = new Form
            {
                Text = "选择相机",
                Size = new Size(260, 200),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(40, 40, 40),
                MaximizeBox = false, MinimizeBox = false,
            };
            int camIndex = 0;
            string[] camNames = { "相机1 — 底板右侧", "相机2 — 底板左侧", "相机3 — 片源", "相机4 — 辅助" };
            for (int i = 0; i < 4; i++)
            {
                var idx = i + 1;
                var btn = new Button
                {
                    Text = camNames[i],
                    Location = new Point(20, 20 + i * 36),
                    Size = new Size(200, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0, 100, 140),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft YaHei", 9F),
                };
                btn.Click += (s, e) => { camIndex = idx; selDlg.DialogResult = DialogResult.OK; selDlg.Close(); };
                selDlg.Controls.Add(btn);
            }
            if (selDlg.ShowDialog(this) != DialogResult.OK || camIndex == 0) return;

            var camForm = camIndex <= 2 ? _cameraDebugForm1 : _cameraDebugForm2;
            if (camForm == null || camForm.IsDisposed)
            {
                camForm = new CameraDebugForm(camIndex, _vision,
                    setLight: (lightNo, on) =>
                    {
                        LogInfo($"[相机{camIndex}] 光源{lightNo}: {(on ? "开" : "关")}");
                    });
                camForm.OnImageGrabbed += img => { };
                if (camIndex <= 2) _cameraDebugForm1 = camForm;
                else _cameraDebugForm2 = camForm;
            }
            camForm.Show(this);
        }

        private void OpenFitLineTool()
        {
            if (_cameraDebugForm1 == null || _cameraDebugForm1.IsDisposed)
                _cameraDebugForm1 = new CameraDebugForm(1, _vision);

            using var selDlg = new Form
            {
                Text = "选择直线拟合工具",
                Size = new Size(260, 150),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(40, 40, 40),
                MaximizeBox = false, MinimizeBox = false,
            };
            int choice = 0;
            for (int i = 1; i <= 2; i++)
            {
                var idx = i;
                var btn = new Button
                {
                    Text = $"FitLine {idx} — {(idx == 1 ? "底板右侧" : "底板左侧")}",
                    Location = new Point(20, 20 + (idx - 1) * 36),
                    Size = new Size(200, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(80, 60, 140),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft YaHei", 9F),
                };
                btn.Click += (s, e) => { choice = idx; selDlg.DialogResult = DialogResult.OK; selDlg.Close(); };
                selDlg.Controls.Add(btn);
            }
            if (selDlg.ShowDialog(this) != DialogResult.OK || choice == 0) return;

            var fitForm = choice == 1 ? _fitLineToolForm1 : _fitLineToolForm2;
            if (fitForm == null || fitForm.IsDisposed)
            {
                fitForm = new FitLineToolForm(
                    choice == 1 ? "底板右侧" : "底板左侧",
                    _cameraDebugForm1, _vision);
                if (choice == 1) _fitLineToolForm1 = fitForm;
                else _fitLineToolForm2 = fitForm;
            }
            fitForm.ShowDialog(this);
        }

        private void OpenMotionSet()
        {
            if (_motionSetForm == null || _motionSetForm.IsDisposed)
            {
                _motionSetForm = new MotionSetForm(_motion, _cfg, _vision);
                _motionSetForm.CameraBaketFix += count =>
                    LogInfo($"[MotionSet] 相机工装固定 x{count}");
                _motionSetForm.CameraBaketXUnFix += (count, space) =>
                    LogInfo($"[MotionSet] 相机工装 X 释放 x{count}, 间距={space}");
                _motionSetForm.CameraBaketYUnFix += (count, space) =>
                    LogInfo($"[MotionSet] 相机工装 Y 释放 x{count}, 间距={space}");
                _motionSetForm.CameraBaketZUnFix += (count, space) =>
                    LogInfo($"[MotionSet] 相机工装 Z 释放 x{count}, 间距={space}");
                _motionSetForm.AxisTest += (axisId, count, space) =>
                    LogInfo($"[MotionSet] 轴测试: 轴{axisId}, {count}次, 间距={space}");
                _motionSetForm.AxisTestStop += () =>
                    LogInfo("[MotionSet] 轴测试已停止");
                _motionSetForm.GetMeasureHeight += () =>
                    LogInfo("[MotionSet] 获取测量高度");
                _motionSetForm.CameraBaketStop += () =>
                    LogInfo("[MotionSet] 相机工装已停止");
            }
            _motionSetForm.ShowDialog(this);
        }

        private void LogInfo(string msg) => OmniFrame.Common.Logger.Info($"[BlockCut] {msg}");
    }
}
