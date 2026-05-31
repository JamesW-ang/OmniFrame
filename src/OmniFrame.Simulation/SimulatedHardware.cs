using MotionIO;
using OmniFrame.Core.BlockCut;

namespace OmniFrame.Simulation
{
    /// <summary>
    /// 仿真硬件实现 — 包装 SimulatedMotion + SimulatedIoCtrl
    /// </summary>
    public class SimulatedHardware : IBlockCutHardware
    {
        public Motion Motion { get; }
        public IoCtrl Io { get; }

        public SimulatedHardware(int axisCount = 16, int ioInputCount = 40, int ioOutputCount = 32)
        {
            Motion = new SimulatedMotion("SimAPS", axisCount);
            Io = new SimulatedIoCtrl("SimIO", ioInputCount, ioOutputCount);
        }

        public SimulatedHardware(Motion motion, IoCtrl io)
        {
            Motion = motion ?? throw new System.ArgumentNullException(nameof(motion));
            Io = io ?? throw new System.ArgumentNullException(nameof(io));
        }

        public void Initialize()
        {
            Motion.Init();
            Io.Init(null);
        }

        public void Shutdown()
        {
            Motion.DeInit();
            Io.Close();
        }
    }
}
