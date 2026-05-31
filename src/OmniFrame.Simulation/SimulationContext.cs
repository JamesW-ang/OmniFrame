using System;

namespace OmniFrame.Simulation
{
    public struct FullSimulation
    {
        public SimulatedMotion Motion;
        public SimulatedPlcDevice Plc;
        public SimulatedIoCtrl Io;

        public void InitializeAll()
        {
            Motion?.Init();
            Plc?.Open();
            Io?.Init(null);
        }

        public void ShutdownAll()
        {
            Motion?.DeInit();
            Plc?.Close();
            Io?.Close();
        }
    }

    public static class SimulationContext
    {
        public static SimulatedMotion CreateSimulatedMotion(string name, int axisCount)
            => new SimulatedMotion(name, axisCount);

        public static SimulatedPlcDevice CreateSimulatedPlc(string name, string ip, int port)
            => new SimulatedPlcDevice(name, ip, port);

        public static SimulatedIoCtrl CreateSimulatedIo(string name, int inputCount, int outputCount)
            => new SimulatedIoCtrl(name, inputCount, outputCount);

        public static FullSimulation CreateFullSimulation()
            => new FullSimulation
            {
                Motion = CreateSimulatedMotion("SimAxis", 16),
                Plc = CreateSimulatedPlc("SimPLC", "127.0.0.1", 502),
                Io = CreateSimulatedIo("SimIO", 40, 32)
            };

        /// <summary>创建仿真硬件抽象 (16 轴，匹配 APS168x64)</summary>
        public static SimulatedHardware CreateSimulatedHardware(int axisCount = 16)
            => new SimulatedHardware(axisCount, 40, 32);
    }
}
