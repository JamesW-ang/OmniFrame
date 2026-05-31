using System.Drawing;
using System.Windows.Forms;
using OmniFrame.Core.BlockCut;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// BlockCut 相机预览 — 4 路相机 2x2 网格
    /// </summary>
    public partial class BlockCutCameraForm : Form
    {
        private readonly BlockCutVision _vision;
        private static readonly string[] CamNames = { "相机1 — 底板右侧", "相机2 — 底板左侧", "相机3 — 片源", "相机4 — 辅助" };

        public BlockCutCameraForm(BlockCutVision vision)
        {
            _vision = vision;
            InitializeComponent();
            WirePaintHandlers();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        [System.Obsolete("For Designer only - use DI constructor")]
        public BlockCutCameraForm()
        {
            InitializeComponent();
            WirePaintHandlers();
        }

        private void WirePaintHandlers()
        {
            Panel[] panels = { _camPanel0, _camPanel1, _camPanel2, _camPanel3 };
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                panels[i].Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    var r = (s as Panel).ClientRectangle;
                    g.DrawString(CamNames[index], Font, Brushes.DarkGray, r, new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                    });
                };
            }
        }
    }
}
