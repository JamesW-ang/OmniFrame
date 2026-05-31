using System;
using System.Collections.Generic;
using System.Threading;

namespace OmniFrame.Plugins.MockMotion
{
    [OmniFrame.Sdk.PluginSystem.PluginVersion(1, 0, 0)]
    public class MockMotionPlugin : OmniFrame.Sdk.PluginSystem.MotionPlugin
    {
        private readonly Dictionary<int, double> _axisPositions = new Dictionary<int, double>();
        private readonly Dictionary<int, double> _axisVelocities = new Dictionary<int, double>();
        private readonly Random _rng = new Random();
        private bool _connected;
        private string _ipAddress;

        public override string Name => "MockMotionPlugin";
        public override string Description => "模拟运动控制卡 — 支持8轴点位运动、回零、位置查询，用于离线调试";

        public override bool Initialize()
        {
            for (int i = 0; i < 8; i++)
            {
                _axisPositions[i] = 0.0;
                _axisVelocities[i] = 100.0;
            }
            return true;
        }

        public override void Unload()
        {
            Disconnect();
            _axisPositions.Clear();
        }

        public override bool Connect(string ipAddress)
        {
            _ipAddress = ipAddress;
            Thread.Sleep(_rng.Next(20, 80));
            _connected = true;
            return true;
        }

        public override void Disconnect()
        {
            _connected = false;
            _ipAddress = null;
        }

        public override bool Move(int axisId, double position, double speed)
        {
            if (!_connected) return false;
            if (axisId < 0 || axisId > 7) return false;

            double startPos = _axisPositions.ContainsKey(axisId) ? _axisPositions[axisId] : 0;
            double distance = Math.Abs(position - startPos);
            int simDelay = Math.Min((int)(distance / speed * 1000), 2000);
            if (simDelay < 5) simDelay = 5;

            Thread.Sleep(simDelay);
            _axisPositions[axisId] = position;
            _axisVelocities[axisId] = speed;
            return true;
        }

        public override bool Home(int axisId)
        {
            if (!_connected) return false;
            if (axisId < 0 || axisId > 7) return false;

            Thread.Sleep(_rng.Next(200, 800));
            _axisPositions[axisId] = 0.0;
            return true;
        }

        public override double GetCurrentPosition(int axisId)
        {
            if (!_connected) return double.NaN;
            return _axisPositions.ContainsKey(axisId) ? _axisPositions[axisId] : 0.0;
        }
    }
}
