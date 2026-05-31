using System;
using System.Drawing;
using System.Windows.Forms;
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    public partial class BlockCutLogForm : Form
    {
        private int _logCount;
        private bool _historyLoaded;

        public BlockCutLogForm()
        {
            InitializeComponent();
            SetupColumns();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
            _btnClear.Click += (s, e) => { _logListView.Items.Clear(); _logCount = 0; UpdateCount(); };

            // 先加载启动以来的所有历史日志
            LoadHistoryLogs();

            // 标记历史/实时分界，然后订阅新日志
            AddSeparator("━━━ 以下为实时日志 ━━━");
            Logger.OnLogWritten += OnLog;
        }

        private void SetupColumns()
        {
            _logListView.Columns.Add("Time", 120);
            _logListView.Columns.Add("Level", 60);
            _logListView.Columns.Add("Message", _logListView.Width - 186);
        }

        private void LoadHistoryLogs()
        {
            if (_logListView == null) return;

            var historyLogs = Logger.GetLogs();
            if (historyLogs.Count == 0) return;

            _logListView.BeginUpdate();
            foreach (var entry in historyLogs)
            {
                if (ShouldFilter(entry.Level)) continue;

                var item = CreateLogItem(entry);
                _logListView.Items.Add(item);
                _logCount++;
            }
            _logListView.EndUpdate();

            if (_logCount > 0 && _chkAutoScroll?.Checked == true && _logListView.Items.Count > 0)
            {
                _logListView.EnsureVisible(_logListView.Items.Count - 1);
                _logListView.TopItem = _logListView.Items[_logListView.Items.Count - 1];
            }

            UpdateCount();
            _historyLoaded = true;
        }

        private void AddSeparator(string text)
        {
            if (_logListView == null) return;

            var item = new ListViewItem("");
            item.SubItems.Add("");
            item.SubItems.Add(text);
            item.ForeColor = Color.FromArgb(100, 100, 100);
            item.Font = new Font("Consolas", 8F, FontStyle.Bold);
            _logListView.Items.Add(item);
        }

        private void OnLog(LogEntry entry)
        {
            if (_logListView == null) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<LogEntry>(OnLog), entry);
                return;
            }

            if (ShouldFilter(entry.Level)) return;

            var item = CreateLogItem(entry);
            _logListView.Items.Add(item);
            _logCount++;

            while (_logListView.Items.Count > 2000)
                _logListView.Items.RemoveAt(0);

            if (_chkAutoScroll?.Checked == true && _logListView.Items.Count > 0)
            {
                _logListView.EnsureVisible(_logListView.Items.Count - 1);
                _logListView.TopItem = _logListView.Items[_logListView.Items.Count - 1];
            }

            if (_logCount % 50 == 0) UpdateCount();
        }

        private bool ShouldFilter(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => !_chkDebug.Checked,
                LogLevel.Info => !_chkInfo.Checked,
                LogLevel.Warning => !_chkWarning.Checked,
                LogLevel.Error => !_chkError.Checked,
                LogLevel.Fatal => !_chkError.Checked,
                _ => false,
            };
        }

        private static ListViewItem CreateLogItem(LogEntry entry)
        {
            var item = new ListViewItem(entry.Time.ToString("HH:mm:ss.fff"));
            item.SubItems.Add(entry.Level.ToString());
            item.SubItems.Add(entry.Message);

            item.ForeColor = entry.Level switch
            {
                LogLevel.Error => Color.Red,
                LogLevel.Fatal => Color.DarkRed,
                LogLevel.Warning => Color.Yellow,
                LogLevel.Debug => Color.Gray,
                _ => Color.White,
            };

            return item;
        }

        private void UpdateCount()
        {
            _lblCount.Text = $"{_logCount} 条";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Logger.OnLogWritten -= OnLog;
            base.OnFormClosing(e);
        }
    }
}
