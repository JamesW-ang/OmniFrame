namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// BlockCut 全局常量 — 映射自 BaseData.h 宏定义
    /// </summary>
    public static class BlockCutConstants
    {
        /// <summary>轴总数</summary>
        public const int AxisNum = 16;

        /// <summary>超时报警时间 (ms)</summary>
        public const int TimeoutWarn = 10000;

        /// <summary>应用名称</summary>
        public const string ApplicationName = "WireCut";

        /// <summary>检测名称</summary>
        public const string CheckName = "CasselZ";

        /// <summary>到位公差 (单位: 脉冲 or mm，原 C++ 中硬编码为 2)</summary>
        public const double PositionTolerance = 2.0;

        /// <summary>轴停止检测 — 轴状态位偏移</summary>
        public const int AxisStateStopBit = 0;

        /// <summary>正限位检测位</summary>
        public const int AxisStatePelBit = 1;

        /// <summary>负限位检测位</summary>
        public const int AxisStateMelBit = 2;

        #region DO 输出定义 (25 路)

        /// <summary>治具移送取料气缸伸出</summary>
        public const int DO_JigYCylinderOut = 0;

        /// <summary>治具移送取料气缸缩回</summary>
        public const int DO_JigYCylinderIn = 1;

        /// <summary>治具移送Z气缸（默认下降）</summary>
        public const int DO_JigZCylinder = 2;

        /// <summary>产品上料Z气缸（默认上升）</summary>
        public const int DO_BlockGetZCylinder = 3;

        /// <summary>产品上料吸真空</summary>
        public const int DO_BlockGetVacuum = 4;

        /// <summary>底板夹紧气缸（默认缩回）</summary>
        public const int DO_BottomCompressCylinder = 5;

        /// <summary>把杆气缸（默认缩回）</summary>
        public const int DO_YCylinder = 6;

        /// <summary>UV气缸（默认上升）</summary>
        public const int DO_UVZCylinder = 7;

        /// <summary>UV固化灯输出</summary>
        public const int DO_UV = 8;

        /// <summary>测厚输出</summary>
        public const int DO_PosHeight = 9;

        /// <summary>点胶一次</summary>
        public const int DO_Dispensing = 10;

        /// <summary>片源调节Z气缸上升</summary>
        public const int DO_AdjustZUp = 11;

        /// <summary>片源调节Z气缸下降</summary>
        public const int DO_AdjustZDown = 12;

        /// <summary>料板下料Z气缸上升</summary>
        public const int DO_BottomOutZCylinderUp = 13;

        /// <summary>料板下料Z气缸下降</summary>
        public const int DO_BottomOutZCylinderDown = 14;

        /// <summary>料板下料夹爪张开</summary>
        public const int DO_BottomOutClawOut = 15;

        /// <summary>料板下料夹爪夹紧</summary>
        public const int DO_BottomOutClawIn = 16;

        /// <summary>料板下料推料气缸上升（默认上升）</summary>
        public const int DO_BottomOutPushCylinder = 17;

        /// <summary>料板推送挡料气缸（默认下降）</summary>
        public const int DO_BottomOutBlockCylinder = 18;

        /// <summary>料板载台顶升气缸（默认下降）</summary>
        public const int DO_BottomOutPlatformCylinder = 19;

        /// <summary>LED照明</summary>
        public const int DO_LED = 20;

        /// <summary>绿灯</summary>
        public const int DO_LightGreen = 21;

        /// <summary>黄灯</summary>
        public const int DO_LightYellow = 22;

        /// <summary>红灯</summary>
        public const int DO_LightRed = 23;

        /// <summary>蜂鸣器</summary>
        public const int DO_Buzzer = 24;

        /// <summary>电源启动 SSR</summary>
        public const int DO_PowerSupply24V = 28;

        /// <summary>治具移送取料气缸缩回 (备用)</summary>
        public const int DO_BottomYCylinderIn = 26;

        #endregion

        #region DI 输入定义 (36 路)

        /// <summary>卡塞检测</summary>
        public const int DI_CheckCassel = 0;

        /// <summary>托盘检测</summary>
        public const int DI_CheckTray = 1;

        /// <summary>治具移送Y气缸伸出</summary>
        public const int DI_JigYCylinderOut = 2;

        /// <summary>治具移送Y气缸缩回</summary>
        public const int DI_JigYCylinderIn = 3;

        /// <summary>治具移送Z气缸上升</summary>
        public const int DI_JigZCylinderUp = 4;

        /// <summary>治具移送Z气缸下降</summary>
        public const int DI_JigZCylinderDown = 5;

        /// <summary>产品上料真空检测</summary>
        public const int DI_BlockGetVacuum = 6;

        /// <summary>产品上料Z气缸上升</summary>
        public const int DI_BlockGetZCylinderUp = 7;

        /// <summary>产品上料Z气缸下降</summary>
        public const int DI_BlockGetZCylinderDown = 8;

        /// <summary>底板检测</summary>
        public const int DI_CheckBottom = 9;

        /// <summary>底板夹紧气缸伸出到位</summary>
        public const int DI_BottomCompressCylinderReach = 10;

        /// <summary>底板夹紧气缸缩回到位</summary>
        public const int DI_BottomCompressCylinderRetract = 11;

        /// <summary>料板载台顶升气缸上升</summary>
        public const int DI_BottomOutPlatformCylinderUp = 12;

        /// <summary>料板载台顶升气缸下降</summary>
        public const int DI_BottomOutPlatformCylinderDown = 13;

        /// <summary>片源气缸夹紧到位</summary>
        public const int DI_YCylinderReach = 14;

        /// <summary>片源气缸缩回到位</summary>
        public const int DI_YCylinderRetract = 15;

        /// <summary>片源调节Z气缸缩回</summary>
        public const int DI_AdjustZUp = 16;

        /// <summary>片源调节Z气缸伸出</summary>
        public const int DI_AdjustZDown = 17;

        /// <summary>UV升降气缸缩回到位</summary>
        public const int DI_UVCylinderUp = 18;

        /// <summary>UV升降气缸伸出到位</summary>
        public const int DI_UVCylinderDown = 19;

        /// <summary>料板下料Z气缸上升</summary>
        public const int DI_BottomOutZCylinderUp = 20;

        /// <summary>料板下料Z气缸下降</summary>
        public const int DI_BottomOutZCylinderDown = 21;

        /// <summary>料板下料推料气缸上升</summary>
        public const int DI_BottomOutPushCylinderUp = 22;

        /// <summary>料板下料推料气缸下降</summary>
        public const int DI_BottomOutPushCylinderDown = 23;

        /// <summary>料板下料夹爪松开到位</summary>
        public const int DI_BottomOutClawOut = 24;

        /// <summary>料板下料夹爪夹紧到位</summary>
        public const int DI_BottomOutClawIn = 25;

        /// <summary>料板推送挡料气缸上升</summary>
        public const int DI_BottomOutBlockCylinderUp = 26;

        /// <summary>料板推送挡料气缸下降</summary>
        public const int DI_BottomOutBlockCylinderDown = 27;

        /// <summary>料板推送底板检测</summary>
        public const int DI_BottomOutCheckBottom = 28;

        /// <summary>料板推送玻璃检测</summary>
        public const int DI_BottomOutCheckBlock = 29;

        /// <summary>急停</summary>
        public const int DI_EMGStop = 30;

        /// <summary>门吸 (磁吸)</summary>
        public const int DI_Door = 31;

        /// <summary>安全光幕</summary>
        public const int DI_Safe = 32;

        /// <summary>治具当前层有无检测</summary>
        public const int DI_CheckJig = 34;

        /// <summary>治具移送取料气缸缩回 (备用)</summary>
        public const int DI_BottomYCylinderIn = 35;

        #endregion

        #region 线程名称

        public const string ThreadCasselZName = "料塔组";
        public const string ThreadLoadYName = "治具取放组";
        public const string ThreadLoadXName = "块料取放组";
        public const string ThreadAdjustName = "调节组";
        public const string ThreadBottomGetName = "底板取放组";
        public const string ThreadSafeName = "安全组";

        #endregion

        #region 报警码前缀

        // === 安全门/光幕 ===
        /// <summary>安全门报警</summary>
        public const string ErrDoor01 = "ERR-DOR-01";
        /// <summary>门吸报警</summary>
        public const string ErrDoor02 = "ERR-DOR-02";
        /// <summary>安全光幕报警</summary>
        public const string ErrDoor03 = "ERR-DOR-03";

        // === 点胶 ===
        /// <summary>点胶寿命超限</summary>
        public const string ErrGlue01 = "ERR-GLUE-01";

        // === 生产管理 ===
        /// <summary>生产管理报警 1-7</summary>
        public const string ErrPm1 = "ERR-PM-1";
        public const string ErrPm2 = "ERR-PM-2";
        public const string ErrPm3 = "ERR-PM-3";
        public const string ErrPm4 = "ERR-PM-4";
        public const string ErrPm5 = "ERR-PM-5";
        public const string ErrPm6 = "ERR-PM-6";
        public const string ErrPm7 = "ERR-PM-7";

        // === 空闲/复位/离线 ===
        /// <summary>空闲超时报警</summary>
        public const string ErrIdle01 = "ERR-IDLE-01";
        /// <summary>复位超时报警</summary>
        public const string ErrRst01 = "ERR-RST-01";
        /// <summary>离线报警</summary>
        public const string ErrOff01 = "ERR-OFF-01";

        // === 视觉 ===
        /// <summary>相机拍照失败</summary>
        public const string ErrVis01 = "ERR-VIS-01";
        /// <summary>测高偏差过大</summary>
        public const string ErrVis02 = "ERR-VIS-02";
        /// <summary>测高多次失败</summary>
        public const string ErrVis03 = "ERR-VIS-03";

        // === MES ===
        /// <summary>MES 验证失败</summary>
        public const string ErrMis01 = "ERR-MIS-01";
        public const string ErrMis02 = "ERR-MIS-02";
        public const string ErrMis03 = "ERR-MIS-03";
        public const string ErrMis04 = "ERR-MIS-04";
        /// <summary>MES 验证失败 (CheckCard)</summary>
        public const string ErrMis05 = "ERR-MIS-05";
        public const string ErrMis06 = "ERR-MIS-06";
        /// <summary>MES 上报失败</summary>
        public const string ErrMis07 = "ERR-MIS-07";

        #endregion

        #region 超时/阈值

        /// <summary>空闲超时报警时间 (秒)</summary>
        public const int IdleTimeoutSec = 10;

        /// <summary>CT 超时 (可配置)</summary>
        public const int CtTimeoutSec = 60;

        /// <summary>点胶次数上限</summary>
        public const int GlueUsageLimit = 100000;

        #endregion
    }
}
