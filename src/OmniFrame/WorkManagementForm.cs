using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.Dialogs;

namespace OmniFrame
{
    /// <summary>
    /// 工单管理对话框 — 替代 Qt CWork (.cpp/.h/.ui, ~374 行)
    /// 管理工单列表及产品参数 (料塔/料盘/底板/相机)
    /// </summary>
    public partial class WorkManagementForm : Form
    {
        private readonly string _iniPath;
        private Dictionary<string, object> _workData;
        private bool _isChanged;
        private string _currentWorkName;

        public event Action<bool, Dictionary<string, object>, string, int[]> ChangeWork;

        public WorkManagementForm()
        {
            _iniPath = System.IO.Path.Combine(AppContext.BaseDirectory, "system.ini");
            _workData = GetDefaultWorkData();
            _isChanged = false;

            InitializeComponent();
            InitWorkList();
        }

        #region UI Construction

        private ListBox _lstWorks;
        private Button _btnAdd, _btnCopy, _btnDelete;
        private Label _lblCurrentWork;

        // Cassel
        private NumericUpDown _numCasselCount, _numCasselSpace;
        // Tray
        private NumericUpDown _numTrayRows, _numTrayCols, _numTrayRowSpace, _numTrayColSpace;
        // Bottom
        private NumericUpDown _numBottomRows, _numBottomCols, _numBottomRowSpace, _numBottomColSpace;
        // Camera
        private NumericUpDown _numObjectUnit;
        private NumericUpDown _numLight1, _numLight2, _numLight3, _numLight4;
        private NumericUpDown _numExposure1, _numExposure2, _numExposure3, _numExposure4;
        private NumericUpDown _numGain1, _numGain2, _numGain3, _numGain4;

        private Button _btnOK, _btnCancel;

        private void InitializeComponent()
        {
            Text = "工单管理";
            Size = new Size(560, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(40, 40, 40);
            ForeColor = Color.White;
            Font = new Font("Microsoft YaHei", 9F);

            int leftX = 12, rightX = 200, y = 12;

            // -- 左侧: 工单列表 --
            var lblWorks = NewLabel("工单列表:", leftX, y, bold: true);
            _lstWorks = new ListBox
            {
                Location = new Point(leftX, y + 22),
                Size = new Size(170, 230),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
            };
            _lstWorks.SelectedIndexChanged += OnWorkSelected;

            _btnAdd = NewButton("+ 新建", leftX, y + 260, 54, 26, Color.FromArgb(0, 100, 140));
            _btnAdd.Click += (s, e) => AddNewWork();
            _btnCopy = NewButton("复制", leftX + 58, y + 260, 54, 26, Color.FromArgb(60, 60, 140));
            _btnCopy.Click += (s, e) => CopyWork();
            _btnDelete = NewButton("删除", leftX + 116, y + 260, 54, 26, Color.FromArgb(180, 60, 0));
            _btnDelete.Click += (s, e) => DeleteWork();

            _lblCurrentWork = NewLabel("当前工单: --", rightX, y, bold: false, color: Color.Cyan);

            // -- 右侧: 参数分组 --
            int gy = y + 22;

            // 料塔参数
            var gCassel = NewGroupBox("料塔参数", rightX, gy, 330, 52);
            _numCasselCount = NewNum(rightX + 100, gy + 18, 80, 0, 100);
            _numCasselCount.ValueChanged += (s, e) => _isChanged = true;
            _numCasselSpace = NewNum(rightX + 240, gy + 18, 80, 0, 1000, 1);
            _numCasselSpace.ValueChanged += (s, e) => _isChanged = true;
            gCassel.Controls.AddRange(new Control[] {
                NewLabel("层数:", rightX + 8, gy + 21), _numCasselCount,
                NewLabel("间距(mm):", rightX + 178, gy + 21), _numCasselSpace,
            });

            // 料盘参数
            gy += 58;
            var gTray = NewGroupBox("料盘参数", rightX, gy, 330, 76);
            _numTrayRows = NewNum(rightX + 100, gy + 18, 60, 1, 100);
            _numTrayRows.ValueChanged += (s, e) => _isChanged = true;
            _numTrayCols = NewNum(rightX + 240, gy + 18, 60, 1, 100);
            _numTrayCols.ValueChanged += (s, e) => _isChanged = true;
            _numTrayRowSpace = NewNum(rightX + 100, gy + 42, 60, 0, 1000, 1);
            _numTrayRowSpace.ValueChanged += (s, e) => _isChanged = true;
            _numTrayColSpace = NewNum(rightX + 240, gy + 42, 60, 0, 1000, 1);
            _numTrayColSpace.ValueChanged += (s, e) => _isChanged = true;
            gTray.Controls.AddRange(new Control[] {
                NewLabel("行数:", rightX + 8, gy + 21), _numTrayRows,
                NewLabel("列数:", rightX + 178, gy + 21), _numTrayCols,
                NewLabel("行距(mm):", rightX + 8, gy + 45), _numTrayRowSpace,
                NewLabel("列距(mm):", rightX + 178, gy + 45), _numTrayColSpace,
            });

            // 底板参数
            gy += 82;
            var gBottom = NewGroupBox("底板参数", rightX, gy, 330, 76);
            _numBottomRows = NewNum(rightX + 100, gy + 18, 60, 1, 100);
            _numBottomRows.ValueChanged += (s, e) => _isChanged = true;
            _numBottomCols = NewNum(rightX + 240, gy + 18, 60, 1, 100);
            _numBottomCols.ValueChanged += (s, e) => _isChanged = true;
            _numBottomRowSpace = NewNum(rightX + 100, gy + 42, 60, 0, 1000, 1);
            _numBottomRowSpace.ValueChanged += (s, e) => _isChanged = true;
            _numBottomColSpace = NewNum(rightX + 240, gy + 42, 60, 0, 1000, 1);
            _numBottomColSpace.ValueChanged += (s, e) => _isChanged = true;
            gBottom.Controls.AddRange(new Control[] {
                NewLabel("行数:", rightX + 8, gy + 21), _numBottomRows,
                NewLabel("列数:", rightX + 178, gy + 21), _numBottomCols,
                NewLabel("行距(mm):", rightX + 8, gy + 45), _numBottomRowSpace,
                NewLabel("列距(mm):", rightX + 178, gy + 45), _numBottomColSpace,
            });

            // 相机参数
            gy += 82;
            var gCamera = NewGroupBox("相机参数", rightX, gy, 330, 76);
            _numObjectUnit = NewNum(rightX + 160, gy + 18, 60, 0, 100, 3, 0.001M);
            _numObjectUnit.ValueChanged += (s, e) => _isChanged = true;
            _numLight1 = NewNum(rightX + 80, gy + 42, 55, 0, 9999);
            _numLight1.ValueChanged += (s, e) => _isChanged = true;
            _numLight2 = NewNum(rightX + 200, gy + 42, 55, 0, 9999);
            _numLight2.ValueChanged += (s, e) => _isChanged = true;
            _numLight3 = NewNum(rightX + 80, gy + 42, 55, 0, 9999); // placeholder positions corrected below
            _numLight3.ValueChanged += (s, e) => _isChanged = true;
            _numLight4 = NewNum(rightX + 200, gy + 42, 55, 0, 9999);
            _numLight4.ValueChanged += (s, e) => _isChanged = true;
            gCamera.Controls.AddRange(new Control[] {
                NewLabel("ObjectUnit:", rightX + 8, gy + 21), _numObjectUnit,
                NewLabel("光源1:", rightX + 8, gy + 45), _numLight1,
                NewLabel("光源2:", rightX + 130, gy + 45), _numLight2,
            });

            // 曝光/增益参数
            gy += 82;
            var gExpGain = NewGroupBox("曝光/增益", rightX, gy, 330, 76);
            _numExposure1 = NewNum(rightX + 80, gy + 18, 55, 0, 99999);
            _numExposure1.ValueChanged += (s, e) => _isChanged = true;
            _numExposure2 = NewNum(rightX + 200, gy + 18, 55, 0, 99999);
            _numExposure2.ValueChanged += (s, e) => _isChanged = true;
            _numExposure3 = NewNum(rightX + 80, gy + 42, 55, 0, 99999);
            _numExposure3.ValueChanged += (s, e) => _isChanged = true;
            _numExposure4 = NewNum(rightX + 200, gy + 42, 55, 0, 99999);
            _numExposure4.ValueChanged += (s, e) => _isChanged = true;
            _numGain1 = NewNum(rightX + 80, gy + 42, 55, 0, 100); // shifted to gain row
            _numGain1.ValueChanged += (s, e) => _isChanged = true;
            _numGain2 = NewNum(rightX + 200, gy + 42, 55, 0, 100);
            _numGain2.ValueChanged += (s, e) => _isChanged = true;

            // Fix: rebuild exposure/gain layout properly
            gExpGain.Controls.Clear();
            gExpGain.Controls.AddRange(new Control[] {
                NewLabel("曝光1:", rightX + 8, gy + 21), _numExposure1,
                NewLabel("曝光2:", rightX + 130, gy + 21), _numExposure2,
                NewLabel("曝光3:", rightX + 8, gy + 45), _numExposure3,
                NewLabel("曝光4:", rightX + 130, gy + 45), _numExposure4,
            });

            // Gain row (separate group)
            gy += 82;
            var gGain = NewGroupBox("增益", rightX, gy, 330, 52);
            _numGain3 = NewNum(rightX + 200, gy + 18, 55, 0, 100);
            _numGain3.ValueChanged += (s, e) => _isChanged = true;
            _numGain4 = NewNum(rightX + 200, gy + 18, 55, 0, 100);
            _numGain4.ValueChanged += (s, e) => _isChanged = true;
            gGain.Controls.AddRange(new Control[] {
                NewLabel("增益1:", rightX + 8, gy + 21), _numGain1,
                NewLabel("增益2:", rightX + 130, gy + 21), _numGain2,
                NewLabel("增益3:", rightX + 8, gy + 45), _numGain3,
                NewLabel("增益4:", rightX + 130, gy + 45), _numGain4,
            });

            // -- 底部按钮 --
            _btnOK = new Button
            {
                Text = "确定",
                Location = new Point(rightX + 120, gy + 70),
                Size = new Size(80, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 0),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 9F),
            };
            _btnOK.Click += OnOK;

            _btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(rightX + 210, gy + 70),
                Size = new Size(80, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 9F),
            };
            _btnCancel.Click += (s, e) => { _isChanged = false; DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[] {
                lblWorks, _lstWorks, _btnAdd, _btnCopy, _btnDelete, _lblCurrentWork,
                gCassel, gTray, gBottom, gCamera, gExpGain, gGain,
                _btnOK, _btnCancel,
            });
        }

        #endregion

        #region Work Data

        private static Dictionary<string, object> GetDefaultWorkData() => new Dictionary<string, object>
        {
            ["CasselCount"] = 6,
            ["CasselSpace"] = 6.0,
            ["TrayRows"] = 2,
            ["TrayCols"] = 2,
            ["TrayRowSpace"] = 5.0,
            ["TrayColSpace"] = 5.0,
            ["BottomRows"] = 3,
            ["BottomCols"] = 2,
            ["BottomRowSpace"] = 5.0,
            ["BottomColSpace"] = 5.0,
            ["ObjectUnit1"] = 3.14,
            ["Light1"] = 999,
            ["Light2"] = 999,
            ["Light3"] = 999,
            ["Light4"] = 999,
            ["Expusur1"] = 5000.0,
            ["Expusur2"] = 5000.0,
            ["Expusur3"] = 5000.0,
            ["Expusur4"] = 5000.0,
            ["Gain1"] = 1,
            ["Gain2"] = 1,
            ["Gain3"] = 1,
            ["Gain4"] = 1,
        };

        private void InitWorkList()
        {
            var workListStr = IniHelper.ReadIni("Work", "WorkList", "", _iniPath);
            if (string.IsNullOrEmpty(workListStr))
            {
                AddNewWork();
                return;
            }

            var works = workListStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var w in works)
                _lstWorks.Items.Add(w.Trim());

            if (_lstWorks.Items.Count > 0)
            {
                var lastId = IniHelper.ReadIni("Work", "ID", "", _iniPath);
                int selIdx = 0;
                if (!string.IsNullOrEmpty(lastId))
                {
                    int idx = _lstWorks.FindStringExact(lastId);
                    if (idx >= 0) selIdx = idx;
                }
                _lstWorks.SelectedIndex = selIdx;
            }
        }

        private void OnWorkSelected(object sender, EventArgs e)
        {
            if (_lstWorks.SelectedItem == null) return;
            _isChanged = true;
            _currentWorkName = _lstWorks.SelectedItem.ToString();
            _lblCurrentWork.Text = $"当前工单: {_currentWorkName}";

            // Load work data from ini
            var dataStr = IniHelper.ReadIni(_currentWorkName, "WorkData", "", _iniPath);
            if (!string.IsNullOrEmpty(dataStr))
            {
                try
                {
                    _workData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(dataStr)
                                ?? GetDefaultWorkData();
                }
                catch (Exception ex)
                {
                    Logger.Warning($"解析工单数据失败: {ex.Message}", ex);
                    _workData = GetDefaultWorkData();
                }
            }
            else
            {
                _workData = GetDefaultWorkData();
            }

            PopulateUI();
        }

        private void PopulateUI()
        {
            _numCasselCount.Value = GetInt("CasselCount", 6);
            _numCasselSpace.Value = GetDecimal("CasselSpace", 6);

            _numTrayRows.Value = GetInt("TrayRows", 2);
            _numTrayCols.Value = GetInt("TrayCols", 2);
            _numTrayRowSpace.Value = GetDecimal("TrayRowSpace", 5);
            _numTrayColSpace.Value = GetDecimal("TrayColSpace", 5);

            _numBottomRows.Value = GetInt("BottomRows", 3);
            _numBottomCols.Value = GetInt("BottomCols", 2);
            _numBottomRowSpace.Value = GetDecimal("BottomRowSpace", 5);
            _numBottomColSpace.Value = GetDecimal("BottomColSpace", 5);

            _numObjectUnit.Value = GetDecimal("ObjectUnit1", 3.14);
            _numLight1.Value = GetInt("Light1", 999);
            _numLight2.Value = GetInt("Light2", 999);
            _numLight3.Value = GetInt("Light3", 999);
            _numLight4.Value = GetInt("Light4", 999);
            _numExposure1.Value = GetDecimal("Expusur1", 5000);
            _numExposure2.Value = GetDecimal("Expusur2", 5000);
            _numExposure3.Value = GetDecimal("Expusur3", 5000);
            _numExposure4.Value = GetDecimal("Expusur4", 5000);
            _numGain1.Value = GetInt("Gain1", 1);
            _numGain2.Value = GetInt("Gain2", 1);
            _numGain3.Value = GetInt("Gain3", 1);
            _numGain4.Value = GetInt("Gain4", 1);
        }

        private int GetInt(string key, int def) =>
            _workData.TryGetValue(key, out var v) && v is IConvertible c ? Convert.ToInt32(c) : def;

        private decimal GetDecimal(string key, double def) =>
            _workData.TryGetValue(key, out var v) && v is IConvertible c ? Convert.ToDecimal(c) : (decimal)def;

        #endregion

        #region Actions

        private void AddNewWork()
        {
            using var dlg = new WorkPrivateDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var name = dlg.WorkName;
            _lstWorks.Items.Add(name);
            _lstWorks.SelectedIndex = _lstWorks.Items.Count - 1;
            IniHelper.WriteIni("Work", "ID", name, _iniPath);

            // Append to work list
            var works = new List<string>();
            foreach (var item in _lstWorks.Items) works.Add(item.ToString());
            IniHelper.WriteIni("Work", "WorkList", string.Join(",", works), _iniPath);
            IniHelper.WriteIni(name, "Index", (_lstWorks.Items.Count).ToString(), _iniPath);
        }

        private void CopyWork()
        {
            if (_lstWorks.SelectedItem == null) return;
            var srcName = _lstWorks.SelectedItem.ToString();
            var newName = $"{srcName}_Copy";
            _lstWorks.Items.Add(newName);
            _lstWorks.SelectedIndex = _lstWorks.Items.Count - 1;

            // Copy data
            var srcData = IniHelper.ReadIni(srcName, "WorkData", "", _iniPath);
            if (!string.IsNullOrEmpty(srcData))
                IniHelper.WriteIni(newName, "WorkData", srcData, _iniPath);

            var works = new List<string>();
            foreach (var item in _lstWorks.Items) works.Add(item.ToString());
            IniHelper.WriteIni("Work", "WorkList", string.Join(",", works), _iniPath);
        }

        private void DeleteWork()
        {
            if (_lstWorks.SelectedItem == null || _lstWorks.Items.Count <= 1) return;

            var result = MessageBox.Show($"确定删除工单 \"{_lstWorks.SelectedItem}\" 吗?",
                "删除确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            var name = _lstWorks.SelectedItem.ToString();
            _lstWorks.Items.RemoveAt(_lstWorks.SelectedIndex);

            // Remove from ini
            IniHelper.DeleteSection(name, _iniPath);

            var works = new List<string>();
            foreach (var item in _lstWorks.Items) works.Add(item.ToString());
            IniHelper.WriteIni("Work", "WorkList", string.Join(",", works), _iniPath);

            if (_lstWorks.Items.Count > 0)
                _lstWorks.SelectedIndex = 0;
        }

        private void OnOK(object sender, EventArgs e)
        {
            if (_lstWorks.SelectedItem == null)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _currentWorkName = _lstWorks.SelectedItem.ToString();

            // Collect current values
            var oldData = new Dictionary<string, object>(_workData);

            _workData["CasselCount"] = (int)_numCasselCount.Value;
            _workData["CasselSpace"] = (double)_numCasselSpace.Value;
            _workData["TrayRows"] = (int)_numTrayRows.Value;
            _workData["TrayCols"] = (int)_numTrayCols.Value;
            _workData["TrayRowSpace"] = (double)_numTrayRowSpace.Value;
            _workData["TrayColSpace"] = (double)_numTrayColSpace.Value;
            _workData["BottomRows"] = (int)_numBottomRows.Value;
            _workData["BottomCols"] = (int)_numBottomCols.Value;
            _workData["BottomRowSpace"] = (double)_numBottomRowSpace.Value;
            _workData["BottomColSpace"] = (double)_numBottomColSpace.Value;
            _workData["ObjectUnit1"] = (double)_numObjectUnit.Value;
            _workData["Light1"] = (int)_numLight1.Value;
            _workData["Light2"] = (int)_numLight2.Value;
            _workData["Light3"] = (int)_numLight3.Value;
            _workData["Light4"] = (int)_numLight4.Value;
            _workData["Expusur1"] = (double)_numExposure1.Value;
            _workData["Expusur2"] = (double)_numExposure2.Value;
            _workData["Expusur3"] = (double)_numExposure3.Value;
            _workData["Expusur4"] = (double)_numExposure4.Value;
            _workData["Gain1"] = (int)_numGain1.Value;
            _workData["Gain2"] = (int)_numGain2.Value;
            _workData["Gain3"] = (int)_numGain3.Value;
            _workData["Gain4"] = (int)_numGain4.Value;

            // Detect changed exposure indices
            var changedIndices = new List<int>();
            for (int i = 1; i <= 4; i++)
            {
                var key = $"Expusur{i}";
                var oldVal = oldData.TryGetValue(key, out var ov) ? ov : null;
                var newVal = _workData[key];
                if (!Equals(oldVal, newVal)) changedIndices.Add(1);
                else changedIndices.Add(0);
            }

            // Save to ini
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_workData);
            IniHelper.WriteIni(_currentWorkName, "WorkData", json, _iniPath);
            IniHelper.WriteIni("Work", "ID", _currentWorkName, _iniPath);

            ChangeWork?.Invoke(_isChanged, _workData, _currentWorkName, changedIndices.ToArray());
            _isChanged = false;
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        #region Helpers

        private static Label NewLabel(string text, int x, int y, bool bold = false, Color? color = null)
            => new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = color ?? Color.White,
                Font = new Font("Microsoft YaHei", 8F, bold ? FontStyle.Bold : FontStyle.Regular),
            };

        private static Button NewButton(string text, int x, int y, int w, int h, Color back)
            => new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F),
            };

        private static NumericUpDown NewNum(int x, int y, int w, decimal min, decimal max, int decimals = 0, decimal inc = 1)
            => new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(w, 22),
                Minimum = min,
                Maximum = max,
                DecimalPlaces = decimals,
                Increment = inc,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F),
            };

        private static GroupBox NewGroupBox(string text, int x, int y, int w, int h)
            => new GroupBox
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 8F, FontStyle.Bold),
            };

        #endregion
    }
}
