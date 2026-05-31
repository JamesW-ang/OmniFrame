using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MotionIO;
using OmniFrame.Common;
using OmniFrame.Core.BlockCut;
using OmniFrame.Theme;

namespace OmniFrame
{
    public partial class BlockCutIOSignalForm : Form
    {
        private readonly MotionIO.Motion _motion;
        private readonly IoCtrl _ioCtrl;
        private readonly Timer _updateTimer;
        private readonly Dictionary<int, bool> _previousDIStates;
        private readonly Dictionary<int, bool> _previousDOStates;

        public BlockCutIOSignalForm(MotionIO.Motion motion, IoCtrl ioCtrl)
        {
            _motion = motion;
            _ioCtrl = ioCtrl;
            _previousDIStates = new Dictionary<int, bool>();
            _previousDOStates = new Dictionary<int, bool>();
            
            InitializeComponent();
            SetupSignalTables();
            LoadCylinderStates();
            WireEvents();
            
            _updateTimer = new Timer { Interval = 100 };
            _updateTimer.Tick += OnTimerTick;
            _updateTimer.Start();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [Obsolete("For Designer only - use DI constructor")]
        public BlockCutIOSignalForm()
        {
            InitializeComponent();
        }

        private void WireEvents()
        {
            _txtDiFilter.TextChanged += OnDiFilterChanged;
            _txtDoFilter.TextChanged += OnDoFilterChanged;
            btnAllDoOn.Click += OnAllDoOn;
            btnAllDoOff.Click += OnAllDoOff;
            _doTable.DoubleClick += OnDoTableDoubleClick;
        }

        private void SetupSignalTables()
        {
            // DI Table
            _diTable.Columns.Add("Index", 50);
            _diTable.Columns.Add("Name", 100);
            _diTable.Columns.Add("State", 60);
            _diTable.Columns.Add("Time", 100);

            // DO Table
            _doTable.Columns.Add("Index", 50);
            _doTable.Columns.Add("Name", 100);
            _doTable.Columns.Add("State", 60);
            _doTable.Columns.Add("Control", 80);
            _doTable.Columns.Add("Time", 100);

            // Initialize with some sample signals
            for (int i = 0; i < 16; i++)
            {
                var diItem = new ListViewItem(i.ToString());
                diItem.SubItems.Add($"DI{i:00}");
                diItem.SubItems.Add("OFF");
                diItem.SubItems.Add(DateTime.Now.ToString("HH:mm:ss"));
                diItem.BackColor = Color.FromArgb(40, 40, 40);
                diItem.ForeColor = Color.Gray;
                _diTable.Items.Add(diItem);
                _previousDIStates[i] = false;
            }

            for (int i = 0; i < 16; i++)
            {
                var doItem = new ListViewItem(i.ToString());
                doItem.SubItems.Add($"DO{i:00}");
                doItem.SubItems.Add("OFF");
                doItem.SubItems.Add("Toggle");
                doItem.SubItems.Add(DateTime.Now.ToString("HH:mm:ss"));
                doItem.BackColor = Color.FromArgb(40, 40, 40);
                doItem.ForeColor = Color.Gray;
                _doTable.Items.Add(doItem);
                _previousDOStates[i] = false;
            }
        }

        private void LoadCylinderStates()
        {
            // Load cylinder states from config or IO
            _cylinderTable.Columns.Add("Name", 100);
            _cylinderTable.Columns.Add("State", 80);
            _cylinderTable.Columns.Add("Control", 100);

            string[] cylinderNames = { "Load1_Lift", "Load1_Clamp", "Load2_Lift", "Load2_Clamp", 
                                      "Cassel_Lift", "Adjust_Lift", "Bottom_Lift", "Unload_Lift" };

            foreach (var name in cylinderNames)
            {
                var item = new ListViewItem(name);
                item.SubItems.Add("Retracted");
                item.SubItems.Add("Extend");
                item.BackColor = Color.FromArgb(40, 40, 40);
                item.ForeColor = Color.White;
                _cylinderTable.Items.Add(item);
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_ioCtrl == null) return;
            
            UpdateDIStates();
            UpdateDOStates();
            UpdateCylinderStates();
        }

        private void UpdateDIStates()
        {
            for (int i = 0; i < 16; i++)
            {
                try
                {
                    bool currentState = _ioCtrl.GetDI(i);
                    bool previousState = _previousDIStates.ContainsKey(i) ? _previousDIStates[i] : false;

                    if (i < _diTable.Items.Count)
                    {
                        var item = _diTable.Items[i];
                        
                        if (currentState != previousState)
                        {
                            item.SubItems[2].Text = currentState ? "ON" : "OFF";
                            item.SubItems[3].Text = DateTime.Now.ToString("HH:mm:ss.fff");
                            item.BackColor = currentState ? Color.FromArgb(0, 80, 0) : Color.FromArgb(40, 40, 40);
                            item.ForeColor = currentState ? Color.Lime : Color.Gray;
                        }
                        
                        _previousDIStates[i] = currentState;
                    }
                }
                catch (Exception ex)
                {
                    // Ignore transient IO read errors during UI refresh
                    Logger.Debug($"更新DI状态失败: {ex.Message}", ex);
                }
            }
        }

        private void UpdateDOStates()
        {
            var random = new Random();
            for (int i = 0; i < 16; i++)
            {
                try
                {
                    bool currentState = random.Next(2) == 1; // 仿真模式: 随机状态
                    bool previousState = _previousDOStates.ContainsKey(i) ? _previousDOStates[i] : false;

                    if (i < _doTable.Items.Count)
                    {
                        var item = _doTable.Items[i];
                        
                        if (currentState != previousState)
                        {
                            item.SubItems[2].Text = currentState ? "ON" : "OFF";
                            item.SubItems[4].Text = DateTime.Now.ToString("HH:mm:ss.fff");
                            item.BackColor = currentState ? Color.FromArgb(100, 0, 0) : Color.FromArgb(40, 40, 40);
                            item.ForeColor = currentState ? Color.Red : Color.Gray;
                        }
                        
                        _previousDOStates[i] = currentState;
                    }
                }
                catch (Exception ex)
                {
                    // Ignore transient IO read errors during UI refresh
                    Logger.Debug($"更新DO状态失败: {ex.Message}", ex);
                }
            }
        }

        private void UpdateCylinderStates()
        {
            // Simulate cylinder state updates based on IO
            for (int i = 0; i < _cylinderTable.Items.Count; i++)
            {
                // This is just a placeholder - actual implementation would check specific IOs
            }
        }

        private void OnDiFilterChanged(object sender, EventArgs e)
        {
            // Apply filter to DI table
            string filter = _txtDiFilter.Text.ToLower();
            foreach (ListViewItem item in _diTable.Items)
            {
                item.Selected = false;
                item.ForeColor = string.IsNullOrEmpty(filter) || 
                                 item.SubItems[0].Text.ToLower().Contains(filter) || 
                                 item.SubItems[1].Text.ToLower().Contains(filter) 
                                 ? Color.White : Color.Gray;
            }
        }

        private void OnDoFilterChanged(object sender, EventArgs e)
        {
            // Apply filter to DO table
            string filter = _txtDoFilter.Text.ToLower();
            foreach (ListViewItem item in _doTable.Items)
            {
                item.Selected = false;
                item.ForeColor = string.IsNullOrEmpty(filter) || 
                                 item.SubItems[0].Text.ToLower().Contains(filter) || 
                                 item.SubItems[1].Text.ToLower().Contains(filter) 
                                 ? Color.White : Color.Gray;
            }
        }

        private void OnAllDoOn(object sender, EventArgs e)
        {
            // Turn all DO on
            if (MessageBox.Show("确定要打开所有DO输出吗?", "确认", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                for (int i = 0; i < 16; i++)
                {
                    _ioCtrl.SetDO(i, true);
                }
            }
        }

        private void OnAllDoOff(object sender, EventArgs e)
        {
            // Turn all DO off
            if (MessageBox.Show("确定要关闭所有DO输出吗?", "确认", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                for (int i = 0; i < 16; i++)
                {
                    _ioCtrl.SetDO(i, false);
                }
            }
        }

        private void OnDoTableDoubleClick(object sender, EventArgs e)
        {
            // Toggle DO when double-clicked (仿真模式)
            if (_doTable.SelectedItems.Count > 0)
            {
                int index = int.Parse(_doTable.SelectedItems[0].Text);
                // 仿真: 简单切换状态
                var item = _doTable.Items[index];
                bool isOn = item.SubItems[2].Text == "ON";
                item.SubItems[2].Text = isOn ? "OFF" : "ON";
                item.BackColor = isOn ? Color.FromArgb(40, 40, 40) : Color.FromArgb(100, 0, 0);
                item.ForeColor = isOn ? Color.Gray : Color.Red;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _updateTimer.Stop();
            _updateTimer.Dispose();
            base.OnFormClosing(e);
        }
    }
}
