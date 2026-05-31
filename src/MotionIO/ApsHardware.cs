namespace MotionIO
{
    /// <summary>
    /// 真实 APS 硬件实现 — 包装 Motion_APS + IoCtrl_APS。
    /// 实现 IBlockCutHardware 接口，供 DI 容器在非仿真模式下注入。
    /// </summary>
    public class ApsHardware : IBlockCutHardware
    {
        public Motion Motion { get; }
        public IoCtrl Io { get; }

        public ApsHardware(Motion motion, IoCtrl io)
        {
            Motion = motion ?? throw new System.ArgumentNullException(nameof(motion));
            Io = io ?? throw new System.ArgumentNullException(nameof(io));
        }

        public ApsHardware(int boardId = 0)
            : this(new Motion_APS(boardId, "APS168x64"), new IoCtrl_APS(boardId))
        {
        }
    }
}
