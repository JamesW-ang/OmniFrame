using System.Drawing;
using System.Windows.Forms;

namespace OmniFrame
{
    partial class BlockCutCameraForm
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel _cameraGrid;
        private Panel _camPanel0;
        private Panel _camPanel1;
        private Panel _camPanel2;
        private Panel _camPanel3;

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
            this._cameraGrid = new TableLayoutPanel();
            this._camPanel0 = new Panel();
            this._camPanel1 = new Panel();
            this._camPanel2 = new Panel();
            this._camPanel3 = new Panel();
            this._cameraGrid.SuspendLayout();
            this.SuspendLayout();

            // _cameraGrid
            this._cameraGrid.Dock = DockStyle.Fill;
            this._cameraGrid.ColumnCount = 2;
            this._cameraGrid.RowCount = 2;
            this._cameraGrid.BackColor = Color.FromArgb(25, 25, 25);
            this._cameraGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._cameraGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._cameraGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this._cameraGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // camera panels
            SetupCameraPanel(_camPanel0, 0, 0);
            SetupCameraPanel(_camPanel1, 1, 0);
            SetupCameraPanel(_camPanel2, 0, 1);
            SetupCameraPanel(_camPanel3, 1, 1);

            this._cameraGrid.Controls.Add(this._camPanel0, 0, 0);
            this._cameraGrid.Controls.Add(this._camPanel1, 1, 0);
            this._cameraGrid.Controls.Add(this._camPanel2, 0, 1);
            this._cameraGrid.Controls.Add(this._camPanel3, 1, 1);

            // BlockCutCameraForm
            this.Text = "BlockCut 相机预览";
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.ForeColor = Color.White;
            this.Font = new Font("Microsoft YaHei", 9F);
            this.Controls.Add(this._cameraGrid);
            this._cameraGrid.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private static void SetupCameraPanel(Panel p, int col, int row)
        {
            p.Dock = DockStyle.Fill;
            p.BackColor = Color.FromArgb(20, 20, 20);
            p.BorderStyle = BorderStyle.FixedSingle;
            p.Margin = new Padding(2);
        }
    }
}
