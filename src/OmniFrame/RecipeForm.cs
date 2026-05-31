using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using OmniFrame.Core;
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    public partial class RecipeForm : Form
    {
        private IRecipeManager _recipeManager;
        private RecipeData _currentRecipe;
        private string _loadedRecipeId;
        private TabControl _tabParams;
        private DataGridView _gridProcess, _gridMotion, _gridPosition;
        private Label _lblLoadedBanner;

        public RecipeForm(IRecipeManager recipeManager)
        {
            InitializeComponent();
            _recipeManager = recipeManager;
            BuildEnhancedUI();
            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        private void BuildEnhancedUI()
        {
            // Add loaded-recipe status banner
            _lblLoadedBanner = new Label
            {
                Location = new Point(12, 9),
                Size = new Size(560, 28),
                Text = "○ 当前未加载任何生产配方",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(_lblLoadedBanner);

            // Configure recipe list for owner-draw to show loaded indicator
            listBoxRecipes.DrawMode = DrawMode.OwnerDrawFixed;
            listBoxRecipes.ItemHeight = 24;
            listBoxRecipes.DrawItem += listBoxRecipes_DrawItem;
            listBoxRecipes.BackColor = Color.FromArgb(40, 40, 45);
            listBoxRecipes.ForeColor = Color.White;
            listBoxRecipes.BorderStyle = BorderStyle.FixedSingle;

            // Replace the generic DataGridView with a TabControl showing all recipe data
            groupBoxParams.Controls.Remove(dataGridViewParams);

            _tabParams = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei", 10F),
                BackColor = Color.FromArgb(45, 45, 50),
                ForeColor = Color.White
            };

            // Tab 1: 工艺参数 (Process Parameters)
            var tabProcess = new TabPage("工艺参数");
            _gridProcess = CreateParamsGrid();
            _gridProcess.Columns.Add("colPName", "参数名称");
            _gridProcess.Columns.Add("colPValue", "设定值");
            _gridProcess.Columns.Add("colPUnit", "单位");
            _gridProcess.Columns.Add("colPMin", "最小值");
            _gridProcess.Columns.Add("colPMax", "最大值");
            _gridProcess.Columns.Add("colPDesc", "说明");
            _gridProcess.Columns[0].Width = 120;
            _gridProcess.Columns[1].Width = 80;
            _gridProcess.Columns[2].Width = 50;
            _gridProcess.Columns[3].Width = 70;
            _gridProcess.Columns[4].Width = 70;
            _gridProcess.Columns[5].Width = 180;
            tabProcess.Controls.Add(_gridProcess);

            // Tab 2: 运动参数 (Motion/Axis)
            var tabMotion = new TabPage("运动参数");
            _gridMotion = CreateParamsGrid();
            _gridMotion.Columns.Add("colMAxis", "轴名称");
            _gridMotion.Columns.Add("colMVel", "速度(mm/s)");
            _gridMotion.Columns.Add("colMAcc", "加速度");
            _gridMotion.Columns.Add("colMDec", "减速度");
            _gridMotion.Columns.Add("colMHome", "回零速度");
            _gridMotion.Columns.Add("colMLimitP", "正限位");
            _gridMotion.Columns.Add("colMLimitN", "负限位");
            foreach (DataGridViewColumn col in _gridMotion.Columns)
                col.Width = 85;
            tabMotion.Controls.Add(_gridMotion);

            // Tab 3: 位置点 (Position Points)
            var tabPosition = new TabPage("位置点");
            _gridPosition = CreateParamsGrid();
            _gridPosition.Columns.Add("colPtName", "点位名称");
            _gridPosition.Columns.Add("colPtAxes", "轴位置 (例: X=100,Y=50,Z=30)");
            _gridPosition.Columns.Add("colPtVel", "速度");
            _gridPosition.Columns.Add("colPtIsRel", "相对移动");
            _gridPosition.Columns.Add("colPtDesc", "说明");
            _gridPosition.Columns[0].Width = 120;
            _gridPosition.Columns[1].Width = 220;
            _gridPosition.Columns[2].Width = 70;
            _gridPosition.Columns[3].Width = 70;
            _gridPosition.Columns[4].Width = 100;
            tabPosition.Controls.Add(_gridPosition);

            _tabParams.TabPages.Add(tabProcess);
            _tabParams.TabPages.Add(tabMotion);
            _tabParams.TabPages.Add(tabPosition);
            groupBoxParams.Controls.Add(_tabParams);
        }

        private DataGridView CreateParamsGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            };
            grid.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 55);
            grid.DefaultCellStyle.ForeColor = Color.White;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 80, 120);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 65);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(60, 60, 65);
            return grid;
        }

        private void RecipeForm_Load(object sender, EventArgs e)
        {
            LoadRecipeList();
            if (listBoxRecipes.Items.Count > 0)
                listBoxRecipes.SelectedIndex = 0;
        }

        private void LoadRecipeList()
        {
            listBoxRecipes.Items.Clear();
            if (_recipeManager == null) return;

            var recipes = _recipeManager.GetRecipeList();
            foreach (var recipe in recipes)
            {
                string prefix = (recipe.Id == _loadedRecipeId) ? "● " : "  ";
                listBoxRecipes.Items.Add(recipe);
            }

            if (recipes.Count > 0 && listBoxRecipes.SelectedIndex < 0)
                listBoxRecipes.SelectedIndex = 0;

            UpdateLoadedBanner();
        }

        private void UpdateLoadedBanner()
        {
            if (!string.IsNullOrEmpty(_loadedRecipeId) && _recipeManager != null)
            {
                var loaded = _recipeManager.GetRecipeList().FirstOrDefault(r => r.Id == _loadedRecipeId);
                if (loaded != null)
                {
                    _lblLoadedBanner.Text = $"● 当前生产配方: {loaded.Name} (v{loaded.Version})";
                    _lblLoadedBanner.BackColor = Color.FromArgb(30, 80, 30);
                    _lblLoadedBanner.ForeColor = Color.Lime;
                    return;
                }
            }
            _lblLoadedBanner.Text = "○ 当前未加载任何生产配方";
            _lblLoadedBanner.BackColor = Color.FromArgb(55, 55, 55);
            _lblLoadedBanner.ForeColor = Color.White;
        }

        // Custom listbox drawing to show loaded indicator
        private void listBoxRecipes_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var recipe = listBoxRecipes.Items[e.Index] as RecipeData;
            if (recipe == null) return;

            bool isLoaded = recipe.Id == _loadedRecipeId;
            e.DrawBackground();
            using (var brush = new SolidBrush(isLoaded ? Color.Lime : e.ForeColor))
            {
                string text = isLoaded ? $"● {recipe.Name}" : $"  {recipe.Name}";
                e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
            }
            e.DrawFocusRectangle();
        }

        private void listBoxRecipes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxRecipes.SelectedItem is RecipeData recipe)
            {
                var fullRecipe = _recipeManager.AllRecipes.FirstOrDefault(r => r.Id == recipe.Id);
                if (fullRecipe != null)
                {
                    _currentRecipe = fullRecipe;
                    DisplayRecipe(fullRecipe);
                }
            }
        }

        private void DisplayRecipe(RecipeData recipe)
        {
            textBoxRecipeName.Text = recipe.Name;
            textBoxDescription.Text = recipe.Description ?? "";
            textBoxProductModel.Text = recipe.ProductModel ?? "";
            labelVersion.Text = $"版本: {recipe.Version}";
            labelCreateTime.Text = $"创建: {recipe.CreateTime:yyyy-MM-dd HH:mm}";
            labelAuthor.Text = $"作者: {recipe.Author}";
            groupBoxRecipeInfo.Text = $"配方详情 — {recipe.Name}";

            // Fill process parameters
            _gridProcess.Rows.Clear();
            foreach (var p in recipe.Parameters)
                _gridProcess.Rows.Add(p.Name, p.Value, p.Unit ?? "", p.MinValue ?? "", p.MaxValue ?? "", p.Description ?? "");

            // Fill motion parameters
            _gridMotion.Rows.Clear();
            foreach (var m in recipe.MotionParams)
                _gridMotion.Rows.Add(m.AxisName, m.Velocity, m.Acceleration, m.Deceleration, m.HomeVelocity,
                    m.EnableSoftLimit ? m.SoftLimitPositive.ToString() : "-",
                    m.EnableSoftLimit ? m.SoftLimitNegative.ToString() : "-");

            // Fill position points
            _gridPosition.Rows.Clear();
            foreach (var pt in recipe.PositionPoints)
            {
                var axesStr = pt.AxisPositions?.Count > 0
                    ? string.Join(", ", pt.AxisPositions.Select(kv => $"{kv.Key}={kv.Value:F1}"))
                    : "";
                _gridPosition.Rows.Add(pt.Name, axesStr, pt.Velocity, pt.IsRelative ? "是" : "否", pt.Description ?? "");
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            if (_recipeManager == null)
            {
                MessageBox.Show("配方管理器未初始化!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            using (var dialog = new InputDialog("新建配方", "请输入配方名称:"))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dialog.InputText))
                {
                    var recipe = _recipeManager.CreateRecipe(dialog.InputText, "BlockCut 生产配方");
                    if (recipe != null)
                    {
                        LoadRecipeList();
                        MessageBox.Show("配方创建成功! 请编辑参数后保存。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                        MessageBox.Show("配方创建失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (_currentRecipe == null) { MessageBox.Show("请先选择一个配方!", "提示"); return; }
            if (_recipeManager == null) { MessageBox.Show("配方管理器未初始化!", "错误"); return; }

            using (var dialog = new InputDialog("复制配方", "请输入新配方名称:"))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dialog.InputText))
                {
                    var newRecipe = _recipeManager.CopyRecipe(_currentRecipe.Id, dialog.InputText);
                    if (newRecipe != null)
                    {
                        LoadRecipeList();
                        MessageBox.Show("配方复制成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                        MessageBox.Show("配方复制失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_currentRecipe == null) { MessageBox.Show("请先选择一个配方!", "提示"); return; }
            if (_recipeManager == null) { MessageBox.Show("配方管理器未初始化!", "错误"); return; }

            if (MessageBox.Show($"确定要删除配方 '{_currentRecipe.Name}' 吗?\n此操作不可恢复。", "确认删除",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (_recipeManager.DeleteRecipe(_currentRecipe.Id))
                {
                    _currentRecipe = null;
                    LoadRecipeList();
                    MessageBox.Show("配方已删除。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show("配方删除失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (_currentRecipe == null) { MessageBox.Show("请先选择一个配方!", "提示"); return; }
            if (_recipeManager == null) { MessageBox.Show("配方管理器未初始化!", "错误"); return; }

            if (_recipeManager.LoadRecipe(_currentRecipe.Id))
            {
                _loadedRecipeId = _currentRecipe.Id;
                groupBoxRecipeInfo.Text = $"配方详情 — {_currentRecipe.Name} ● 已加载";
                LoadRecipeList();
                MessageBox.Show($"配方 '{_currentRecipe.Name}' 已加载到当前生产。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show("配方加载失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_currentRecipe == null) return;
            if (_recipeManager == null) { MessageBox.Show("配方管理器未初始化!", "错误"); return; }

            // Sync from UI to recipe object
            _currentRecipe.Name = textBoxRecipeName.Text;
            _currentRecipe.Description = textBoxDescription.Text;
            _currentRecipe.ProductModel = textBoxProductModel.Text;

            // Save process parameters
            _currentRecipe.Parameters.Clear();
            foreach (DataGridViewRow row in _gridProcess.Rows)
            {
                if (row.IsNewRow) continue;
                var name = row.Cells[0].Value?.ToString();
                if (string.IsNullOrEmpty(name)) continue;
                _currentRecipe.Parameters.Add(new RecipeParameter
                {
                    Name = name,
                    Value = row.Cells[1].Value?.ToString(),
                    Unit = row.Cells[2].Value?.ToString(),
                    MinValue = row.Cells[3].Value?.ToString(),
                    MaxValue = row.Cells[4].Value?.ToString(),
                    Description = row.Cells[5].Value?.ToString()
                });
            }

            // Save motion parameters
            _currentRecipe.MotionParams.Clear();
            foreach (DataGridViewRow row in _gridMotion.Rows)
            {
                if (row.IsNewRow) continue;
                var axis = row.Cells[0].Value?.ToString();
                if (string.IsNullOrEmpty(axis)) continue;
                _currentRecipe.MotionParams.Add(new MotionParameter
                {
                    AxisName = axis,
                    Velocity = ParseDouble(row.Cells[1].Value, 100),
                    Acceleration = ParseDouble(row.Cells[2].Value, 1000),
                    Deceleration = ParseDouble(row.Cells[3].Value, 1000),
                    HomeVelocity = ParseDouble(row.Cells[4].Value, 50),
                    SoftLimitPositive = ParseDouble(row.Cells[5].Value, 500),
                    SoftLimitNegative = ParseDouble(row.Cells[6].Value, 0),
                    EnableSoftLimit = true
                });
            }

            // Save position points
            _currentRecipe.PositionPoints.Clear();
            foreach (DataGridViewRow row in _gridPosition.Rows)
            {
                if (row.IsNewRow) continue;
                var name = row.Cells[0].Value?.ToString();
                if (string.IsNullOrEmpty(name)) continue;
                var axesStr = row.Cells[1].Value?.ToString() ?? "";
                var pt = new PositionPoint
                {
                    Name = name,
                    Velocity = ParseDouble(row.Cells[2].Value, 100),
                    IsRelative = row.Cells[3].Value?.ToString() == "是",
                    Description = row.Cells[4].Value?.ToString()
                };
                // Parse axis positions from string "X=100,Y=50,Z=30"
                foreach (var part in axesStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = part.Split('=');
                    if (kv.Length == 2 && double.TryParse(kv[1].Trim(), out var val))
                        pt.AxisPositions[kv[0].Trim()] = val;
                }
                _currentRecipe.PositionPoints.Add(pt);
            }

            _currentRecipe.ModifyTime = DateTime.Now;

            if (_recipeManager.SaveRecipe(_currentRecipe))
            {
                groupBoxRecipeInfo.Text = $"配方详情 — {_currentRecipe.Name} ● 已保存";
                LoadRecipeList();
                MessageBox.Show("配方保存成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show("配方保存失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private double ParseDouble(object value, double fallback)
        {
            if (value == null) return fallback;
            return double.TryParse(value.ToString(), out var r) ? r : fallback;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_currentRecipe == null) { MessageBox.Show("请先选择一个配方!", "提示"); return; }
            if (_recipeManager == null) { MessageBox.Show("配方管理器未初始化!", "错误"); return; }

            using (var dialog = new SaveFileDialog { Filter = "XML文件|*.xml", FileName = $"{_currentRecipe.Name}.xml" })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (_recipeManager.ExportRecipe(_currentRecipe.Id, dialog.FileName))
                        MessageBox.Show($"配方已导出到:\n{dialog.FileName}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("配方导出失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (_recipeManager == null) { MessageBox.Show("配方管理器未初始化!", "错误"); return; }

            using (var dialog = new OpenFileDialog { Filter = "XML文件|*.xml" })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var recipe = _recipeManager.ImportRecipe(dialog.FileName);
                    if (recipe != null)
                    {
                        LoadRecipeList();
                        MessageBox.Show($"配方 '{recipe.Name}' 导入成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                        MessageBox.Show("配方导入失败! 请检查文件格式。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    public class InputDialog : Form
    {
        private TextBox textBox;
        public string InputText => textBox.Text;

        public InputDialog(string title, string prompt)
        {
            Text = title;
            Size = new Size(420, 150);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var label = new Label { Text = prompt, Location = new Point(20, 20), Size = new Size(380, 20), Font = new Font("Microsoft YaHei", 10F) };
            textBox = new TextBox { Location = new Point(20, 50), Size = new Size(370, 25), Font = new Font("Microsoft YaHei", 10F) };
            var btnOK = new Button { Text = "确定", DialogResult = DialogResult.OK, Location = new Point(220, 85), Size = new Size(75, 30) };
            var btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Location = new Point(310, 85), Size = new Size(75, 30) };

            Controls.AddRange(new Control[] { label, textBox, btnOK, btnCancel });
            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }
    }
}
