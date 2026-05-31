using System;
using System.Xml.Serialization;
using OmniFrame.Core;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    [Serializable]
    [XmlRoot("BlockCut")]
    public class BlockCutConfig
    {
        private static IConfigManager _configManager;

        public static void Initialize(IConfigManager configManager)
        {
            _configManager = configManager;
            configManager.ConfigChanged += OnConfigChanged;
        }

        private static void OnConfigChanged(object sender, ConfigChangedEventArgs e)
        {
            if (e.ConfigFileName.Equals("BlockCut.xml", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info("BlockCut配置已变更，重新加载");
                ConfigChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler ConfigChanged;

        public static BlockCutConfig GetConfig()
        {
            if (_configManager == null)
            {
                Logger.Warning("ConfigManager 未初始化，使用默认配置");
                return new BlockCutConfig();
            }
            return _configManager.GetConfig<BlockCutConfig>("BlockCut.xml", new BlockCutConfig());
        }

        public void SaveConfig()
        {
            if (_configManager != null)
            {
                _configManager.SaveConfig("BlockCut.xml", this);
                Logger.Info("BlockCut配置已保存");
            }
        }

        public bool Validate()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(AesKey) || AesKey.Length != 32)
            {
                Logger.Warning("AesKey 无效，应为32位字符");
                isValid = false;
            }

            if (CasselCount <= 0 || CasselCount > 20)
            {
                Logger.Warning("CasselCount 应在1-20之间");
                isValid = false;
            }

            if (TrayRows <= 0 || TrayRows > 10)
            {
                Logger.Warning("TrayRows 应在1-10之间");
                isValid = false;
            }

            if (TrayCols <= 0 || TrayCols > 10)
            {
                Logger.Warning("TrayCols 应在1-10之间");
                isValid = false;
            }

            return isValid;
        }

        // === Station_Load (上料工站 — LoadY) ===
        [XmlElement("CasselZFirstPos")]
        public double CasselZFirstPos { get; set; }

        [XmlElement("CasselZSpace")]
        public double CasselZSpace { get; set; } = 30;

        [XmlElement("TrayYGetTray")]
        public double TrayYGetTray { get; set; }

        [XmlElement("TrayYCode")]
        public double TrayYCode { get; set; } = 50;

        [XmlElement("TrayYGetSlice")]
        public double TrayYGetSlice { get; set; } = 100;

        [XmlElement("TrayRowSpace")]
        public double TrayRowSpace { get; set; } = 20;

        // === Station_Load2 (二次上料工站 — LoadX) ===
        [XmlElement("TrayXGetSlice")]
        public double TrayXGetSlice { get; set; } = 100;

        [XmlElement("TrayXSetSlice")]
        public double TrayXSetSlice { get; set; } = 200;

        [XmlElement("TrayColSpace")]
        public double TrayColSpace { get; set; } = 20;

        [XmlElement("BottomColSpace")]
        public double BottomColSpace { get; set; } = 25;

        // === Station_CasselZ (料塔工站 — CasselZ) ===
        [XmlElement("CasselCount")]
        public int CasselCount { get; set; } = 10;

        // === Station_Adjust (矫正工站 — UVW + 相机) ===
        [XmlElement("UVWAngle")]
        public double UVWAngle { get; set; }

        [XmlElement("BottomYSetSlice")]
        public double BottomYSetSlice { get; set; } = 100;

        [XmlElement("BottomYAdjustBottom")]
        public double BottomYAdjustBottom { get; set; } = 50;

        [XmlElement("BottomRowSpace")]
        public double BottomRowSpace { get; set; } = 25;

        [XmlElement("BottomColSpaceRecipe")]
        public double BottomColSpaceRecipe { get; set; } = 20;

        [XmlElement("ObjectUnit")]
        public double ObjectUnit { get; set; } = 0.005;

        [XmlElement("CameraBottomXRight")]
        public double CameraBottomXRight { get; set; } = 200;

        [XmlElement("CameraBottomXLeft")]
        public double CameraBottomXLeft { get; set; }

        [XmlElement("CameraXAdjustSlice1")]
        public double CameraXAdjustSlice1 { get; set; } = 180;

        [XmlElement("CameraXAdjustSlice2")]
        public double CameraXAdjustSlice2 { get; set; } = 20;

        [XmlElement("CameraXBottPos1")]
        public double CameraXBottPos1 { get; set; } = 100;

        [XmlElement("CameraZSafe")]
        public double CameraZSafe { get; set; } = 50;

        [XmlElement("CameraZAdjustBottom")]
        public double CameraZAdjustBottom { get; set; } = 30;

        [XmlElement("CameraZAdjustSlice")]
        public double CameraZAdjustSlice { get; set; } = 35;

        [XmlElement("SliceY1Adjust")]
        public double SliceY1Adjust { get; set; } = 80;

        [XmlElement("SliceY2Adjust")]
        public double SliceY2Adjust { get; set; } = 80;

        [XmlElement("SliceXAdjust")]
        public double SliceXAdjust { get; set; } = 150;

        [XmlElement("PosMax")]
        public double PosMax { get; set; } = 0.5;

        // === 行列参数 ===
        [XmlElement("TrayRows")]
        public int TrayRows { get; set; } = 5;

        [XmlElement("TrayCols")]
        public int TrayCols { get; set; } = 4;

        [XmlElement("BottomRows")]
        public int BottomRows { get; set; } = 5;

        [XmlElement("BottomCols")]
        public int BottomCols { get; set; } = 4;

        // === Station_BottomGet (底板取放工站 — BottomGetX) ===
        [XmlElement("BottomXGetEmptyBottom")]
        public double BottomXGetEmptyBottom { get; set; }

        [XmlElement("BottomXSetEmptyBottom")]
        public double BottomXSetEmptyBottom { get; set; } = 50;

        [XmlElement("BottomXSetEmptyBottom2")]
        public double BottomXSetEmptyBottom2 { get; set; } = 100;

        [XmlElement("BottomXGetFullBottom")]
        public double BottomXGetFullBottom { get; set; } = 150;

        [XmlElement("BottomXSetFullBottom")]
        public double BottomXSetFullBottom { get; set; } = 200;

        // === 多产品支持 (6 个产品槽位) ===
        [XmlElement("BottomYSetSlice1")] public double BottomYSetSlice1 { get; set; } = 100;
        [XmlElement("BottomYSetSlice2")] public double BottomYSetSlice2 { get; set; } = 100;
        [XmlElement("BottomYSetSlice3")] public double BottomYSetSlice3 { get; set; } = 100;
        [XmlElement("BottomYSetSlice4")] public double BottomYSetSlice4 { get; set; } = 100;
        [XmlElement("BottomYSetSlice5")] public double BottomYSetSlice5 { get; set; } = 100;
        [XmlElement("BottomYSetSlice6")] public double BottomYSetSlice6 { get; set; } = 100;

        [XmlElement("BottomYAdjustBottom1")] public double BottomYAdjustBottom1 { get; set; } = 50;
        [XmlElement("BottomYAdjustBottom2")] public double BottomYAdjustBottom2 { get; set; } = 50;
        [XmlElement("BottomYAdjustBottom3")] public double BottomYAdjustBottom3 { get; set; } = 50;
        [XmlElement("BottomYAdjustBottom4")] public double BottomYAdjustBottom4 { get; set; } = 50;
        [XmlElement("BottomYAdjustBottom5")] public double BottomYAdjustBottom5 { get; set; } = 50;
        [XmlElement("BottomYAdjustBottom6")] public double BottomYAdjustBottom6 { get; set; } = 50;

        [XmlElement("UVWAngle1")] public double UVWAngle1 { get; set; }
        [XmlElement("UVWAngle2")] public double UVWAngle2 { get; set; }
        [XmlElement("UVWAngle3")] public double UVWAngle3 { get; set; }
        [XmlElement("UVWAngle4")] public double UVWAngle4 { get; set; }
        [XmlElement("UVWAngle5")] public double UVWAngle5 { get; set; }
        [XmlElement("UVWAngle6")] public double UVWAngle6 { get; set; }

        public double GetBottomYSetSlice(int slot) => slot switch
        {
            1 => BottomYSetSlice1, 2 => BottomYSetSlice2, 3 => BottomYSetSlice3,
            4 => BottomYSetSlice4, 5 => BottomYSetSlice5, 6 => BottomYSetSlice6,
            _ => BottomYSetSlice
        };

        public double GetBottomYAdjustBottom(int slot) => slot switch
        {
            1 => BottomYAdjustBottom1, 2 => BottomYAdjustBottom2, 3 => BottomYAdjustBottom3,
            4 => BottomYAdjustBottom4, 5 => BottomYAdjustBottom5, 6 => BottomYAdjustBottom6,
            _ => BottomYAdjustBottom
        };

        public double GetUVWAngle(int slot) => slot switch
        {
            1 => UVWAngle1, 2 => UVWAngle2, 3 => UVWAngle3,
            4 => UVWAngle4, 5 => UVWAngle5, 6 => UVWAngle6,
            _ => UVWAngle
        };

        // === 9 点补偿 ===
        [XmlElement("LeftOffY_1")] public double LeftOffY1 { get; set; }
        [XmlElement("LeftOffY_2")] public double LeftOffY2 { get; set; }
        [XmlElement("LeftOffY_3")] public double LeftOffY3 { get; set; }
        [XmlElement("LeftOffY_4")] public double LeftOffY4 { get; set; }
        [XmlElement("LeftOffY_5")] public double LeftOffY5 { get; set; }
        [XmlElement("LeftOffY_6")] public double LeftOffY6 { get; set; }
        [XmlElement("LeftOffY_7")] public double LeftOffY7 { get; set; }
        [XmlElement("LeftOffY_8")] public double LeftOffY8 { get; set; }
        [XmlElement("LeftOffY_9")] public double LeftOffY9 { get; set; }

        [XmlElement("RightOffY_1")] public double RightOffY1 { get; set; }
        [XmlElement("RightOffY_2")] public double RightOffY2 { get; set; }
        [XmlElement("RightOffY_3")] public double RightOffY3 { get; set; }
        [XmlElement("RightOffY_4")] public double RightOffY4 { get; set; }
        [XmlElement("RightOffY_5")] public double RightOffY5 { get; set; }
        [XmlElement("RightOffY_6")] public double RightOffY6 { get; set; }
        [XmlElement("RightOffY_7")] public double RightOffY7 { get; set; }
        [XmlElement("RightOffY_8")] public double RightOffY8 { get; set; }
        [XmlElement("RightOffY_9")] public double RightOffY9 { get; set; }

        public double[] GetLeftOffsets()
        {
            return new[] { 0, LeftOffY1, LeftOffY2, LeftOffY3, LeftOffY4,
                LeftOffY5, LeftOffY6, LeftOffY7, LeftOffY8, LeftOffY9 };
        }

        public double[] GetRightOffsets()
        {
            return new[] { 0, RightOffY1, RightOffY2, RightOffY3, RightOffY4,
                RightOffY5, RightOffY6, RightOffY7, RightOffY8, RightOffY9 };
        }

        // === 视觉参数 ===
        [XmlElement("GrayLow1")] public int GrayLow1 { get; set; } = 50;
        [XmlElement("GrayLow2")] public int GrayLow2 { get; set; } = 50;
        [XmlElement("GrayHigh1")] public int GrayHigh1 { get; set; } = 200;
        [XmlElement("GrayHigh2")] public int GrayHigh2 { get; set; } = 200;
        [XmlElement("GraySpace")] public int GraySpace { get; set; } = 30;
        [XmlElement("ExposureSpace")] public int ExposureSpace { get; set; } = 100;

        // === 点胶/UV 参数 ===
        [XmlElement("DisTime")] public int DisTime { get; set; } = 500;
        [XmlElement("UVTime")] public int UVTime { get; set; } = 3000;
        [XmlElement("DisZDis")] public double DisZDis { get; set; } = 20;
        [XmlElement("CameraXWait")] public double CameraXWait { get; set; } = 20;
        [XmlElement("CameraXUV")] public double CameraXUV { get; set; } = 20;
        [XmlElement("CameraZUV")] public double CameraZUV { get; set; } = 35;

        // === 安全位 ===
        [XmlElement("SliceYWait")] public double SliceYWait { get; set; } = 80;
        [XmlElement("SliceXWait")] public double SliceXWait { get; set; } = 150;
        [XmlElement("DisZSafe")] public double DisZSafe { get; set; } = 50;
        [XmlElement("DisXSafe")] public double DisXSafe { get; set; } = 150;
        [XmlElement("DisYSafe")] public double DisYSafe { get; set; } = 100;

        // === 测高参数 ===
        [XmlElement("CameraXHeight1")] public double CameraXHeight1 { get; set; } = 100;
        [XmlElement("CameraXHeight2")] public double CameraXHeight2 { get; set; } = 150;
        [XmlElement("CameraXHeight3")] public double CameraXHeight3 { get; set; } = 200;
        [XmlElement("CameraZHeight")] public double CameraZHeight { get; set; } = 40;
        [XmlElement("HeightHigh")] public double HeightHigh { get; set; } = 0.3;
        [XmlElement("HeightLow")] public double HeightLow { get; set; } = -0.3;

        // === 扫描枪 ===
        [XmlElement("BottomScannerHost")] public string BottomScannerHost { get; set; } = "192.168.1.100";
        [XmlElement("BottomScannerPort")] public int BottomScannerPort { get; set; } = 6000;
        [XmlElement("SliceScannerHost")] public string SliceScannerHost { get; set; } = "192.168.1.101";
        [XmlElement("SliceScannerPort")] public int SliceScannerPort { get; set; } = 6001;

        // === 测试模式 ===
        [XmlElement("IsSimulation")]
        public bool IsSimulation { get; set; } = true;

        [XmlElement("IsEmptyTest")]
        public bool IsEmptyTest { get; set; }

        [XmlElement("IsCloseSliceCode")]
        public bool IsCloseSliceCode { get; set; }

        [XmlElement("IsCloseJigCode")]
        public bool IsCloseJigCode { get; set; }

        // === MES 配置 ===
        [XmlElement("AesKey")]
        public string AesKey { get; set; } = "your-32-char-key-here-change-in-production";
    }
}
