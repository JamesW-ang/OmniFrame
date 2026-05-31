using System.Drawing;
using System.Windows.Forms;

namespace OmniFrame
{
    partial class BlockCutManualForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel _scrollPanel;
        private Panel _jogPanel;
        private Button _btnJogToggle;
        private ComboBox _cmbAxisSelect;
        private TextBox _txtJogTarget;
        private NumericUpDown _numJogSpeed;
        private Button _btnJogAbs, _btnJogRel, _btnJogStop, _btnJogReset;
        private Label _lblCurPos;
        private Panel _cylinderPanel;
        private Button _btnCylinderToggle;
        private Label _lblAlarmHeader;
        private Button _btnAlarmCasselZ, _btnAlarmLoad, _btnAlarmLoad2, _btnAlarmBottomGet, _btnAlarmAdjust;
        private Panel _maintPanel;
        private ComboBox _cmbMaintenanceType;
        private Button _btnMaintStart, _btnMaintEnd;
        private Label _lblMaintStatus;
        private Panel _gluePanel;
        private NumericUpDown _numGlueLimit;
        private Label _lblGlueCount;
        private Button _btnGlueReplace;
        private Panel _scannerPanel;
        private Button _btnScannerBottom, _btnScannerSlice;
        private Label _lblBottomScannerResult, _lblSliceScannerResult;
        private CheckBox _chkBypassBottom, _chkBypassSlice;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._scrollPanel = new Panel();
            this._jogPanel = new Panel();
            this._btnJogToggle = new Button();
            this._cmbAxisSelect = new ComboBox();
            this._txtJogTarget = new TextBox();
            this._numJogSpeed = new NumericUpDown();
            this._btnJogAbs = new Button();
            this._btnJogRel = new Button();
            this._btnJogStop = new Button();
            this._btnJogReset = new Button();
            this._lblCurPos = new Label();
            this._cylinderPanel = new Panel();
            this._btnCylinderToggle = new Button();
            this._lblAlarmHeader = new Label();
            this._btnAlarmCasselZ = new Button();
            this._btnAlarmLoad = new Button();
            this._btnAlarmLoad2 = new Button();
            this._btnAlarmBottomGet = new Button();
            this._btnAlarmAdjust = new Button();
            this._maintPanel = new Panel();
            this._cmbMaintenanceType = new ComboBox();
            this._btnMaintStart = new Button();
            this._btnMaintEnd = new Button();
            this._lblMaintStatus = new Label();
            this._gluePanel = new Panel();
            this._numGlueLimit = new NumericUpDown();
            this._lblGlueCount = new Label();
            this._btnGlueReplace = new Button();
            this._scannerPanel = new Panel();
            this._btnScannerBottom = new Button();
            this._btnScannerSlice = new Button();
            this._lblBottomScannerResult = new Label();
            this._lblSliceScannerResult = new Label();
            this._chkBypassBottom = new CheckBox();
            this._chkBypassSlice = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this._numJogSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numGlueLimit)).BeginInit();
            this._scrollPanel.SuspendLayout();
            this._jogPanel.SuspendLayout();
            this._cylinderPanel.SuspendLayout();
            this._maintPanel.SuspendLayout();
            this._gluePanel.SuspendLayout();
            this._scannerPanel.SuspendLayout();
            this.SuspendLayout();

            // _scrollPanel
            this._scrollPanel.Dock = DockStyle.Fill;
            this._scrollPanel.BackColor = Color.FromArgb(45, 45, 45);
            this._scrollPanel.AutoScroll = true;
            this._scrollPanel.Padding = new Padding(12);

            // _jogPanel
            this._jogPanel.Location = new Point(12, 16);
            this._jogPanel.Size = new Size(300, 24);
            this._jogPanel.BackColor = Color.FromArgb(38, 38, 38);
            this._jogPanel.BorderStyle = BorderStyle.FixedSingle;

            // _btnJogToggle
            this._btnJogToggle.Text = "手动轴控制";
            this._btnJogToggle.Location = new Point(0, 0);
            this._btnJogToggle.Size = new Size(298, 24);
            this._btnJogToggle.FlatStyle = FlatStyle.Flat;
            this._btnJogToggle.BackColor = Color.FromArgb(55, 55, 55);
            this._btnJogToggle.ForeColor = Color.White;
            this._btnJogToggle.Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold);

            // _cmbAxisSelect
            this._cmbAxisSelect.Location = new Point(6, 30);
            this._cmbAxisSelect.Size = new Size(140, 22);
            this._cmbAxisSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cmbAxisSelect.Font = new Font("Microsoft YaHei", 8F);

            // _txtJogTarget
            this._txtJogTarget.Location = new Point(150, 30);
            this._txtJogTarget.Size = new Size(80, 22);
            this._txtJogTarget.Text = "0";
            this._txtJogTarget.Font = new Font("Microsoft YaHei", 8F);

            // _numJogSpeed
            this._numJogSpeed.Location = new Point(234, 30);
            this._numJogSpeed.Size = new Size(56, 22);
            this._numJogSpeed.Minimum = 0.1M;
            this._numJogSpeed.Maximum = 1.0M;
            this._numJogSpeed.Value = 0.5M;
            this._numJogSpeed.DecimalPlaces = 1;
            this._numJogSpeed.Increment = 0.1M;
            this._numJogSpeed.Font = new Font("Microsoft YaHei", 8F);

            // _btnJogAbs
            this._btnJogAbs.Text = "绝对";
            this._btnJogAbs.Location = new Point(6, 58);
            this._btnJogAbs.Size = new Size(68, 22);
            this._btnJogAbs.FlatStyle = FlatStyle.Flat;
            this._btnJogAbs.BackColor = Color.FromArgb(0, 100, 0);
            this._btnJogAbs.ForeColor = Color.White;
            this._btnJogAbs.Font = new Font("Microsoft YaHei", 8F);

            // _btnJogRel
            this._btnJogRel.Text = "相对";
            this._btnJogRel.Location = new Point(78, 58);
            this._btnJogRel.Size = new Size(68, 22);
            this._btnJogRel.FlatStyle = FlatStyle.Flat;
            this._btnJogRel.BackColor = Color.FromArgb(100, 80, 0);
            this._btnJogRel.ForeColor = Color.White;
            this._btnJogRel.Font = new Font("Microsoft YaHei", 8F);

            // _btnJogStop
            this._btnJogStop.Text = "停止";
            this._btnJogStop.Location = new Point(150, 58);
            this._btnJogStop.Size = new Size(68, 22);
            this._btnJogStop.FlatStyle = FlatStyle.Flat;
            this._btnJogStop.BackColor = Color.FromArgb(140, 40, 0);
            this._btnJogStop.ForeColor = Color.White;
            this._btnJogStop.Font = new Font("Microsoft YaHei", 8F);

            // _btnJogReset
            this._btnJogReset.Text = "轴复位";
            this._btnJogReset.Location = new Point(222, 58);
            this._btnJogReset.Size = new Size(68, 22);
            this._btnJogReset.FlatStyle = FlatStyle.Flat;
            this._btnJogReset.BackColor = Color.FromArgb(60, 60, 140);
            this._btnJogReset.ForeColor = Color.White;
            this._btnJogReset.Font = new Font("Microsoft YaHei", 8F);

            // _lblCurPos
            this._lblCurPos.Text = "当前: --";
            this._lblCurPos.Location = new Point(6, 90);
            this._lblCurPos.ForeColor = Color.Cyan;
            this._lblCurPos.AutoSize = true;
            this._lblCurPos.Font = new Font("Microsoft YaHei", 8F);

            // _cylinderPanel
            this._cylinderPanel.Location = new Point(12, 46);
            this._cylinderPanel.Size = new Size(300, 24);
            this._cylinderPanel.BackColor = Color.FromArgb(38, 38, 38);
            this._cylinderPanel.BorderStyle = BorderStyle.FixedSingle;

            // _btnCylinderToggle
            this._btnCylinderToggle.Text = "气缸手动控制";
            this._btnCylinderToggle.Location = new Point(0, 0);
            this._btnCylinderToggle.Size = new Size(298, 24);
            this._btnCylinderToggle.FlatStyle = FlatStyle.Flat;
            this._btnCylinderToggle.BackColor = Color.FromArgb(55, 55, 55);
            this._btnCylinderToggle.ForeColor = Color.White;
            this._btnCylinderToggle.Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold);

            // _lblAlarmHeader
            this._lblAlarmHeader.Text = "━━━ 报警继续 ━━━";
            this._lblAlarmHeader.Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold);
            this._lblAlarmHeader.ForeColor = Color.FromArgb(120, 120, 120);
            this._lblAlarmHeader.Location = new Point(12, 78);
            this._lblAlarmHeader.AutoSize = true;

            // _btnAlarmCasselZ
            this._btnAlarmCasselZ.Text = "CasselZ 继续";
            this._btnAlarmCasselZ.Location = new Point(12, 102);
            this._btnAlarmCasselZ.Size = new Size(100, 24);
            this._btnAlarmCasselZ.FlatStyle = FlatStyle.Flat;
            this._btnAlarmCasselZ.BackColor = Color.FromArgb(200, 100, 0);
            this._btnAlarmCasselZ.ForeColor = Color.White;
            this._btnAlarmCasselZ.Font = new Font("Microsoft YaHei", 8F);

            // _btnAlarmLoad
            this._btnAlarmLoad.Text = "Load 继续";
            this._btnAlarmLoad.Location = new Point(122, 102);
            this._btnAlarmLoad.Size = new Size(100, 24);
            this._btnAlarmLoad.FlatStyle = FlatStyle.Flat;
            this._btnAlarmLoad.BackColor = Color.FromArgb(200, 100, 0);
            this._btnAlarmLoad.ForeColor = Color.White;
            this._btnAlarmLoad.Font = new Font("Microsoft YaHei", 8F);

            // _btnAlarmLoad2
            this._btnAlarmLoad2.Text = "Load2 继续";
            this._btnAlarmLoad2.Location = new Point(232, 102);
            this._btnAlarmLoad2.Size = new Size(100, 24);
            this._btnAlarmLoad2.FlatStyle = FlatStyle.Flat;
            this._btnAlarmLoad2.BackColor = Color.FromArgb(200, 100, 0);
            this._btnAlarmLoad2.ForeColor = Color.White;
            this._btnAlarmLoad2.Font = new Font("Microsoft YaHei", 8F);

            // _btnAlarmBottomGet
            this._btnAlarmBottomGet.Text = "BottomGet 继续";
            this._btnAlarmBottomGet.Location = new Point(12, 130);
            this._btnAlarmBottomGet.Size = new Size(100, 24);
            this._btnAlarmBottomGet.FlatStyle = FlatStyle.Flat;
            this._btnAlarmBottomGet.BackColor = Color.FromArgb(200, 100, 0);
            this._btnAlarmBottomGet.ForeColor = Color.White;
            this._btnAlarmBottomGet.Font = new Font("Microsoft YaHei", 8F);

            // _btnAlarmAdjust
            this._btnAlarmAdjust.Text = "Adjust 继续";
            this._btnAlarmAdjust.Location = new Point(122, 130);
            this._btnAlarmAdjust.Size = new Size(100, 24);
            this._btnAlarmAdjust.FlatStyle = FlatStyle.Flat;
            this._btnAlarmAdjust.BackColor = Color.FromArgb(200, 100, 0);
            this._btnAlarmAdjust.ForeColor = Color.White;
            this._btnAlarmAdjust.Font = new Font("Microsoft YaHei", 8F);

            // _maintPanel
            this._maintPanel.Location = new Point(12, 166);
            this._maintPanel.Size = new Size(300, 70);
            this._maintPanel.BackColor = Color.FromArgb(38, 38, 38);
            this._maintPanel.BorderStyle = BorderStyle.FixedSingle;

            // _cmbMaintenanceType
            this._cmbMaintenanceType.Location = new Point(6, 24);
            this._cmbMaintenanceType.Size = new Size(140, 22);
            this._cmbMaintenanceType.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cmbMaintenanceType.Font = new Font("Microsoft YaHei", 8F);
            this._cmbMaintenanceType.Items.AddRange(new object[] {
                "PM1-日常点检", "PM2-周检", "PM3-月检", "PM4-季检", "PM5-半年检", "PM6-年检" });
            this._cmbMaintenanceType.SelectedIndex = 0;

            // _btnMaintStart
            this._btnMaintStart.Text = "开始";
            this._btnMaintStart.Location = new Point(152, 22);
            this._btnMaintStart.Size = new Size(60, 24);
            this._btnMaintStart.FlatStyle = FlatStyle.Flat;
            this._btnMaintStart.BackColor = Color.FromArgb(0, 120, 0);
            this._btnMaintStart.ForeColor = Color.White;
            this._btnMaintStart.Font = new Font("Microsoft YaHei", 8F);

            // _btnMaintEnd
            this._btnMaintEnd.Text = "结束";
            this._btnMaintEnd.Location = new Point(216, 22);
            this._btnMaintEnd.Size = new Size(60, 24);
            this._btnMaintEnd.FlatStyle = FlatStyle.Flat;
            this._btnMaintEnd.BackColor = Color.FromArgb(180, 60, 0);
            this._btnMaintEnd.ForeColor = Color.White;
            this._btnMaintEnd.Font = new Font("Microsoft YaHei", 8F);

            // _lblMaintStatus
            this._lblMaintStatus.Text = "正常生产";
            this._lblMaintStatus.Location = new Point(6, 50);
            this._lblMaintStatus.ForeColor = Color.Lime;
            this._lblMaintStatus.AutoSize = true;
            this._lblMaintStatus.Font = new Font("Microsoft YaHei", 8F);

            // _gluePanel
            this._gluePanel.Location = new Point(12, 244);
            this._gluePanel.Size = new Size(300, 48);
            this._gluePanel.BackColor = Color.FromArgb(38, 38, 38);
            this._gluePanel.BorderStyle = BorderStyle.FixedSingle;

            // _numGlueLimit
            this._numGlueLimit.Location = new Point(100, 4);
            this._numGlueLimit.Size = new Size(70, 22);
            this._numGlueLimit.Minimum = 0M;
            this._numGlueLimit.Maximum = 999999M;
            this._numGlueLimit.Value = 50000M;
            this._numGlueLimit.Font = new Font("Microsoft YaHei", 8F);

            // _lblGlueCount
            this._lblGlueCount.Text = "已用: 0";
            this._lblGlueCount.Location = new Point(6, 28);
            this._lblGlueCount.ForeColor = Color.Cyan;
            this._lblGlueCount.AutoSize = true;
            this._lblGlueCount.Font = new Font("Microsoft YaHei", 8F);

            // _btnGlueReplace
            this._btnGlueReplace.Text = "更换胶水";
            this._btnGlueReplace.Location = new Point(180, 24);
            this._btnGlueReplace.Size = new Size(80, 22);
            this._btnGlueReplace.FlatStyle = FlatStyle.Flat;
            this._btnGlueReplace.BackColor = Color.FromArgb(0, 100, 140);
            this._btnGlueReplace.ForeColor = Color.White;
            this._btnGlueReplace.Font = new Font("Microsoft YaHei", 8F);

            // _scannerPanel
            this._scannerPanel.Location = new Point(12, 300);
            this._scannerPanel.Size = new Size(300, 105);
            this._scannerPanel.BackColor = Color.FromArgb(38, 38, 38);
            this._scannerPanel.BorderStyle = BorderStyle.FixedSingle;

            // _btnScannerBottom
            this._btnScannerBottom.Text = "测底板扫码枪";
            this._btnScannerBottom.Location = new Point(6, 24);
            this._btnScannerBottom.Size = new Size(120, 24);
            this._btnScannerBottom.FlatStyle = FlatStyle.Flat;
            this._btnScannerBottom.BackColor = Color.FromArgb(60, 60, 140);
            this._btnScannerBottom.ForeColor = Color.White;
            this._btnScannerBottom.Font = new Font("Microsoft YaHei", 8F);

            // _btnScannerSlice
            this._btnScannerSlice.Text = "测片源扫码枪";
            this._btnScannerSlice.Location = new Point(6, 52);
            this._btnScannerSlice.Size = new Size(120, 24);
            this._btnScannerSlice.FlatStyle = FlatStyle.Flat;
            this._btnScannerSlice.BackColor = Color.FromArgb(60, 60, 140);
            this._btnScannerSlice.ForeColor = Color.White;
            this._btnScannerSlice.Font = new Font("Microsoft YaHei", 8F);

            // _lblBottomScannerResult
            this._lblBottomScannerResult.Text = "结果: --";
            this._lblBottomScannerResult.Location = new Point(132, 28);
            this._lblBottomScannerResult.ForeColor = Color.Gray;
            this._lblBottomScannerResult.AutoSize = true;
            this._lblBottomScannerResult.Font = new Font("Microsoft YaHei", 8F);

            // _lblSliceScannerResult
            this._lblSliceScannerResult.Text = "结果: --";
            this._lblSliceScannerResult.Location = new Point(132, 56);
            this._lblSliceScannerResult.ForeColor = Color.Gray;
            this._lblSliceScannerResult.AutoSize = true;
            this._lblSliceScannerResult.Font = new Font("Microsoft YaHei", 8F);

            // _chkBypassBottom
            this._chkBypassBottom.Text = "跳过底板扫码";
            this._chkBypassBottom.Location = new Point(6, 80);
            this._chkBypassBottom.ForeColor = Color.White;
            this._chkBypassBottom.AutoSize = true;
            this._chkBypassBottom.Font = new Font("Microsoft YaHei", 8F);

            // _chkBypassSlice
            this._chkBypassSlice.Text = "跳过片源扫码";
            this._chkBypassSlice.Location = new Point(150, 80);
            this._chkBypassSlice.ForeColor = Color.White;
            this._chkBypassSlice.AutoSize = true;
            this._chkBypassSlice.Font = new Font("Microsoft YaHei", 8F);

            // Add cylinder content labels
            BuildCylinderContent();

            // Assemble control trees
            this._jogPanel.Controls.AddRange(new Control[] {
                this._btnJogToggle, this._cmbAxisSelect, this._txtJogTarget, this._numJogSpeed,
                this._btnJogAbs, this._btnJogRel, this._btnJogStop, this._btnJogReset, this._lblCurPos });

            this._cylinderPanel.Controls.Add(this._btnCylinderToggle);

            var maintHeader = new Label
            {
                Text = "维护模式", Location = new Point(6, 4),
                ForeColor = Color.White, AutoSize = true,
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold),
            };

            var glueHeader = new Label
            {
                Text = "点胶次数上限:", Location = new Point(6, 6),
                ForeColor = Color.White, AutoSize = true,
                Font = new Font("Microsoft YaHei", 8F),
            };

            var scannerHeader = new Label
            {
                Text = "扫描枪测试", Location = new Point(6, 4),
                ForeColor = Color.White, AutoSize = true,
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold),
            };

            this._maintPanel.Controls.AddRange(new Control[] {
                maintHeader, this._cmbMaintenanceType, this._btnMaintStart,
                this._btnMaintEnd, this._lblMaintStatus });

            this._gluePanel.Controls.AddRange(new Control[] {
                glueHeader, this._numGlueLimit, this._lblGlueCount, this._btnGlueReplace });

            this._scannerPanel.Controls.AddRange(new Control[] {
                scannerHeader, this._btnScannerBottom, this._lblBottomScannerResult,
                this._btnScannerSlice, this._lblSliceScannerResult,
                this._chkBypassBottom, this._chkBypassSlice });

            this._scrollPanel.Controls.AddRange(new Control[] {
                this._jogPanel, this._cylinderPanel,
                this._lblAlarmHeader,
                this._btnAlarmCasselZ, this._btnAlarmLoad, this._btnAlarmLoad2,
                this._btnAlarmBottomGet, this._btnAlarmAdjust,
                this._maintPanel, this._gluePanel, this._scannerPanel });

            // BlockCutManualForm
            this.Text = "BlockCut 手动控制";
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.ForeColor = Color.White;
            this.Font = new Font("Microsoft YaHei", 9F);
            this.Controls.Add(this._scrollPanel);
            ((System.ComponentModel.ISupportInitialize)(this._numJogSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numGlueLimit)).EndInit();
            this._scrollPanel.ResumeLayout(false);
            this._scrollPanel.PerformLayout();
            this._jogPanel.ResumeLayout(false);
            this._jogPanel.PerformLayout();
            this._cylinderPanel.ResumeLayout(false);
            this._maintPanel.ResumeLayout(false);
            this._maintPanel.PerformLayout();
            this._gluePanel.ResumeLayout(false);
            this._gluePanel.PerformLayout();
            this._scannerPanel.ResumeLayout(false);
            this._scannerPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        private void BuildCylinderContent()
        {
            string[] cylNames = {
                "治具移送Y气缸", "治具移送Z气缸", "产品上料Z气缸", "产品上料真空",
                "底板夹紧气缸", "片源气缸", "UV气缸", "夹爪夹紧", "推料气缸" };
            int[] doIndices = { 0, 2, 3, 4, 5, 6, 7, 16, 17 };

            for (int i = 0; i < cylNames.Length; i++)
            {
                int y = 28 + i * 24;
                var lbl = new Label
                {
                    Text = cylNames[i], Location = new Point(6, y + 2), AutoSize = true,
                    ForeColor = Color.White, Font = new Font("Microsoft YaHei", 8F),
                };

                var btnOn = new Button
                {
                    Text = "开", Location = new Point(170, y), Size = new Size(50, 22),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 100, 0),
                    ForeColor = Color.White, Font = new Font("Microsoft YaHei", 8F),
                    Tag = $"cylOn:{doIndices[i]}",
                };

                var btnOff = new Button
                {
                    Text = "关", Location = new Point(224, y), Size = new Size(50, 22),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(140, 40, 0),
                    ForeColor = Color.White, Font = new Font("Microsoft YaHei", 8F),
                    Tag = $"cylOff:{doIndices[i]}",
                };

                this._cylinderPanel.Controls.Add(lbl);
                this._cylinderPanel.Controls.Add(btnOn);
                this._cylinderPanel.Controls.Add(btnOff);
            }
        }
    }
}
