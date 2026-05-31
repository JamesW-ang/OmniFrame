using System.ComponentModel;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// 轴 ID — 16 轴定义，映射自 BaseData.h eAxisID
    /// </summary>
    public enum AxisId
    {
        [Description("CasselZ")]
        CasselZ = 0,
        [Description("AdjustX")]
        AdjustX = 1,
        [Description("DisX")]
        DisX = 2,
        [Description("DisZ")]
        DisZ = 3,
        [Description("CameraZ")]
        CameraZ = 4,
        [Description("CameraX")]
        CameraX = 5,
        [Description("BottomY")]
        BottomY = 6,
        [Description("BottomGetX")]
        BottomGetX = 7,
        [Description("AdjustY1")]
        AdjustY1 = 8,
        [Description("AdjustY2")]
        AdjustY2 = 9,
        [Description("BottomU")]
        BottomU = 10,
        [Description("BottomV")]
        BottomV = 11,
        [Description("BottomW")]
        BottomW = 12,
        [Description("LoadY")]
        LoadY = 13,
        [Description("DisY")]
        DisY = 14,
        [Description("LoadX")]
        LoadX = 15,
    }

    /// <summary>
    /// 运动线程 ID — 6 线程定义，映射自 BaseData.h eThreadID
    /// </summary>
    public enum ThreadId
    {
        CasselZ = 0,
        LoadY = 1,
        LoadX = 2,
        Adjust = 3,
        BottomGet = 4,
        Safe = 5,
    }

    /// <summary>
    /// 回原点状态
    /// </summary>
    public enum HomeStatus
    {
        HomeOk = 0,
        HomingOn = 1,
        HomeFail = 2,
    }

    /// <summary>
    /// 用户角色
    /// </summary>
    public enum UserType
    {
        Operator = 0,
        Engineer = 1,
        Administrator = 2,
    }

    /// <summary>
    /// ROI 图形类型，映射自 BaseData.h eRegiongType
    /// </summary>
    public enum RegionType
    {
        Rect = 0,
        RotRect = 1,
        Circle = 2,
        Ring = 3,
        Polygon = 4,
        MeasureRing = 5,
        MeasureRect = 6,
    }

    /// <summary>
    /// 过渡类型 (Halcon edge transition)
    /// </summary>
    public enum EdgeTransition
    {
        All = 0,
        Positive = 1,
        Negative = 2,
    }

    /// <summary>
    /// 边缘选择 (first/last/all)
    /// </summary>
    public enum EdgeSelect
    {
        First = 0,
        Last = 1,
        All = 2,
    }

    /// <summary>
    /// 拟合算法类型
    /// </summary>
    public enum FitAlgorithm
    {
        Tukey = 0,
        Huber = 1,
        LeastSquares = 2,
    }

    /// <summary>
    /// 插值类型
    /// </summary>
    public enum InterpolationType
    {
        NearestNeighbor = 0,
        Bilinear = 1,
        Bicubic = 2,
    }
}
