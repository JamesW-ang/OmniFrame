using System;

namespace OmniFrame.Core
{
    /// <summary>
    /// 运动管理器接口
    /// </summary>
    public interface IMotionManager
    {
        bool Initialize(string configPath);
        bool MoveTo(int cardIndex, int axis, double position, double speed);
        bool Home(int cardIndex, int axis, MotionIO.HomeMode mode);
        double GetPosition(int cardIndex, int axis);
        bool Stop(int cardIndex, int axis);
        bool IsConnected { get; }
        void Dispose();
    }
}
