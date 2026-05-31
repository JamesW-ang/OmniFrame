namespace MotionIO
{
    /// <summary>
    /// 硬件抽象接口 — 解耦工站与具体硬件实现（APS / 仿真）。
    /// 所有工站通过此接口获取 Motion 和 Io，不再直接依赖具体硬件类。
    /// </summary>
    public interface IBlockCutHardware
    {
        Motion Motion { get; }
        IoCtrl Io { get; }
    }
}
