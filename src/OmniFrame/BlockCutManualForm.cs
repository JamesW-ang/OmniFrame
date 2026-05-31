using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using MotionIO;
using OmniFrame.Core.BlockCut;
using OmniFrame.Theme;

namespace OmniFrame
{
    public partial class BlockCutManualForm : Form
    {
        private readonly MotionIO.Motion _motion;
        private readonly MotionIO.IoCtrl _io;
        private readonly BarcodeScannerClient _barcodeClient;
        private readonly Station_Adjust _stationAdjust;
        private readonly Station_CasselZ _stationCasselZ;
        private readonly Station_Load _stationLoad;
        private readonly Station_Load2 _stationLoad2;
        private readonly Station_BottomGet _stationBottomGet;
        private readonly Station_Safe _stationSafe;

        private bool _jogPanelVisible;
        private bool _cylinderPanelVisible;
        private DateTime _maintStartTime;

        public BlockCutManualForm(
            MotionIO.Motion motion,
            MotionIO.IoCtrl io,
            BarcodeScannerClient barcodeClient,
            Station_Adjust stationAdjust,
            Station_CasselZ stationCasselZ,
            Station_Load stationLoad,
            Station_Load2 stationLoad2,
            Station_BottomGet stationBottomGet,
            Station_Safe stationSafe)
        {
            _motion = motion;
            _io = io;
            _barcodeClient = barcodeClient;
            _stationAdjust = stationAdjust;
            _stationCasselZ = stationCasselZ;
            _stationLoad = stationLoad;
            _stationLoad2 = stationLoad2;
            _stationBottomGet = stationBottomGet;
            _stationSafe = stationSafe;

            InitializeComponent();
            WireEvents();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [System.Obsolete("For Designer only - use DI constructor")]
        public BlockCutManualForm()
        {
            InitializeComponent();
        }

        private void WireEvents()
        {
            // Jog panel toggle
            _btnJogToggle.Click += (s, e) =>
            {
                _jogPanelVisible = !_jogPanelVisible;
                _jogPanel.Height = _jogPanelVisible ? 140 : 24;
            };

            // Jog actions
            _btnJogAbs.Click += (s, e) => JogAbsolute();
            _btnJogRel.Click += (s, e) => JogRelative();
            _btnJogStop.Click += (s, e) => JogStop();
            _btnJogReset.Click += (s, e) => AxisReset();

            // Cylinder panel toggle
            _btnCylinderToggle.Click += (s, e) =>
            {
                _cylinderPanelVisible = !_cylinderPanelVisible;
                _cylinderPanel.Height = _cylinderPanelVisible ? 250 : 24;
            };

            // Cylinder on/off buttons — wired via Tag
            foreach (Control c in _cylinderPanel.Controls)
            {
                if (c is Button btn && btn.Tag is string tag)
                {
                    if (tag.StartsWith("cylOn:"))
                    {
                        int doIdx = int.Parse(tag.Substring(6));
                        btn.Click += (s, e) => _io?.SetDO(doIdx, true);
                    }
                    else if (tag.StartsWith("cylOff:"))
                    {
                        int doIdx = int.Parse(tag.Substring(7));
                        btn.Click += (s, e) => _io?.SetDO(doIdx, false);
                    }
                }
            }

            // Alarm continue
            _btnAlarmCasselZ.Click += (s, e) => _stationCasselZ?.ContinueAfterAlarm();
            _btnAlarmLoad.Click += (s, e) => _stationLoad?.ContinueAfterAlarm();
            _btnAlarmLoad2.Click += (s, e) => _stationLoad2?.ContinueAfterAlarm();
            _btnAlarmBottomGet.Click += (s, e) => _stationBottomGet?.ContinueAfterAlarm();
            _btnAlarmAdjust.Click += (s, e) => _stationAdjust?.ContinueAfterAlarm();

            // Maintenance
            _btnMaintStart.Click += (s, e) => StartMaintenance();
            _btnMaintEnd.Click += (s, e) => EndMaintenance();

            // Glue
            _btnGlueReplace.Click += (s, e) =>
            {
                _lblGlueCount.Text = "已用: 0";
                _stationSafe?.ResetGlueUsage();
                LogInfo("[点胶] 胶水已更换，计数归零");
            };

            // Scanner test
            _btnScannerBottom.Click += async (s, e) => await TestScannerAsync("bottom", _lblBottomScannerResult);
            _btnScannerSlice.Click += async (s, e) => await TestScannerAsync("slice", _lblSliceScannerResult);
        }

        private void JogAbsolute()
        {
            if (!double.TryParse(_txtJogTarget.Text, out double target)) return;
            int axisIdx = _cmbAxisSelect.SelectedIndex;
            double ratio = (double)_numJogSpeed.Value;
            try { _motion?.AbsMove(axisIdx, target, ratio); }
            catch (Exception ex) { LogWarn($"轴运动失败: {ex.Message}"); }
        }

        private void JogRelative()
        {
            if (!double.TryParse(_txtJogTarget.Text, out double dist)) return;
            int axisIdx = _cmbAxisSelect.SelectedIndex;
            double ratio = (double)_numJogSpeed.Value;
            try { _motion?.RelativeMove(axisIdx, dist, ratio); }
            catch (Exception ex) { LogWarn($"轴运动失败: {ex.Message}"); }
        }

        private void JogStop()
        {
            int axisIdx = _cmbAxisSelect.SelectedIndex;
            try { _motion?.StopAxis(axisIdx); }
            catch (Exception ex) { LogWarn($"轴停止失败: {ex.Message}"); }
        }

        private void AxisReset()
        {
            try
            {
                _motion?.StopAllAxis();
                System.Threading.Thread.Sleep(200);
                for (int i = 0; i < 16; i++)
                {
                    _motion?.ClearAlarm(i);
                    _motion?.ServoOn(i);
                }
                LogInfo("[AxisReset] 复位完成");
            }
            catch (Exception ex)
            {
                LogError($"[AxisReset] 复位失败: {ex.Message}", ex);
            }
        }

        private void StartMaintenance()
        {
            _maintStartTime = DateTime.Now;
            string type = _cmbMaintenanceType.SelectedItem?.ToString() ?? "PM-未知";
            _lblMaintStatus.Text = $"维护中: {type}";
            _lblMaintStatus.ForeColor = Color.Yellow;
            LogInfo($"[维护] 开始 {type}");
        }

        private void EndMaintenance()
        {
            var elapsed = DateTime.Now - _maintStartTime;
            string type = _cmbMaintenanceType.SelectedItem?.ToString() ?? "PM-未知";
            _lblMaintStatus.Text = "正常生产";
            _lblMaintStatus.ForeColor = Color.Lime;
            LogInfo($"[维护] 结束 {type}, 耗时 {elapsed.TotalMinutes:F1} 分钟");
        }

        private async Task TestScannerAsync(string scanner, Label resultLabel)
        {
            resultLabel.Text = "结果: 测试中...";
            resultLabel.ForeColor = Color.Yellow;
            try
            {
                var result = await _barcodeClient.ScanAsync(scanner, 5000);
                resultLabel.Text = string.IsNullOrEmpty(result) ? "结果: 超时" : $"结果: {result}";
                resultLabel.ForeColor = string.IsNullOrEmpty(result) ? Color.Red : Color.Lime;
            }
            catch (Exception ex)
            {
                resultLabel.Text = "结果: 异常";
                resultLabel.ForeColor = Color.Red;
                LogError($"扫码枪测试异常: {ex.Message}", ex);
            }
        }

        private void LogInfo(string msg) => OmniFrame.Common.Logger.Info($"[BlockCut] {msg}");
        private void LogWarn(string msg) => OmniFrame.Common.Logger.Warning($"[BlockCut] {msg}");
        private void LogError(string msg, Exception ex) => OmniFrame.Common.Logger.Error($"[BlockCut] {msg}", ex);
    }
}
