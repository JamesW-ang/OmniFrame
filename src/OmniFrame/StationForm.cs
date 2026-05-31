using System;
using System.Windows.Forms;
using OmniFrame.Core;
using OmniFrame.Theme;

namespace OmniFrame
{
    public partial class StationForm : Form
    {
        private IStationManager _stationManager;
        private AutoStation _selectedStation;

        public StationForm(IStationManager stationManager)
        {
            InitializeComponent();
            _stationManager = stationManager;

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();

            dataGridViewStations.SelectionChanged += OnStationSelected;
        }

        private void StationForm_Load(object sender, EventArgs e)
        {
            LoadStations();
        }

        private void LoadStations()
        {
            try
            {
                var stations = _stationManager.GetAllStations();
                dataGridViewStations.DataSource = null;
                dataGridViewStations.DataSource = stations;
                ClearRightPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载工作站失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnStationSelected(object sender, EventArgs e)
        {
            if (dataGridViewStations.SelectedRows.Count > 0)
            {
                var row = dataGridViewStations.SelectedRows[0];
                _selectedStation = row.DataBoundItem as AutoStation;
                PopulateRightPanel(_selectedStation);
            }
            else
            {
                _selectedStation = null;
                ClearRightPanel();
            }
        }

        private void PopulateRightPanel(AutoStation station)
        {
            if (station == null) return;
            txtStationName.Text = station.Name ?? "";
            txtCycleTime.Text = "0";
            checkBoxEnable.Checked = station.Enable;
            txtDescription.Text = $"索引: {station.Index} | 状态: {station.State}";
        }

        private void ClearRightPanel()
        {
            txtStationName.Text = "";
            txtCycleTime.Text = "0";
            checkBoxEnable.Checked = false;
            txtDescription.Text = "";
        }

        private void btnStartStation_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedStation != null)
                {
                    _selectedStation.Start();
                    LoadStations();
                }
                else if (_stationManager.GetAllStations().Count > 0)
                {
                    bool success = _stationManager.StartRun();
                    MessageBox.Show(success ? "所有工作站启动成功" : "工作站启动失败", "提示",
                        MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                    LoadStations();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStopStation_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedStation != null)
                {
                    _selectedStation.Stop();
                    LoadStations();
                }
                else
                {
                    _stationManager.StopRun();
                    MessageBox.Show("所有工作站已停止", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStations();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"停止失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnResetStation_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedStation != null)
                {
                    _selectedStation.Reset();
                    LoadStations();
                }
                else
                {
                    _stationManager.ResetAllStation();
                    MessageBox.Show("所有工作站已复位", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStations();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复位失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadStations();
        }
    }
}
