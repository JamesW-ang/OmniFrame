using System;
using System.Xml;
using OmniFrame.Common;
using MotionIO;

namespace OmniFrame.Core
{
    /// <summary>
    /// 运动管理器
    /// 管理运动控制卡的操作，提供运动控制相关功能
        /// </summary>
    public class MotionManager : IMotionManager
    {
        private IMotionIOManager _motionMgr;


        /// <summary>
        /// 构造函数（DI注入）
        /// </summary>
        public MotionManager(IMotionIOManager motionIOMgr)
        {
            _motionMgr = motionIOMgr ?? throw new ArgumentNullException(nameof(motionIOMgr));
        }

        /// <summary>
        /// 初始化运动控制
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        /// <returns>初始化成功返回true，失败返回false</returns>
        public bool Initialize(string configPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);
                _motionMgr.ReadCfgFromXml(doc);
                return _motionMgr.InitAll();
            }
            catch (Exception ex)
            {
                Logger.Error("MotionManager初始化失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 移动到指定位置
        /// </summary>
        /// <param name="cardIndex">卡索引</param>
        /// <param name="axis">轴号</param>
        /// <param name="position">目标位置</param>
        /// <param name="speed">速度</param>
        /// <returns>移动成功返回true，失败返回false</returns>
        public bool MoveTo(int cardIndex, int axis, double position, double speed)
        {
            try
            {
                var motion = _motionMgr.GetMotion(cardIndex);
                if (motion == null)
                    return false;
                return motion.AbsMove(axis, position, speed);
            }
            catch (Exception ex)
            {
                Logger.Error("MotionManager移动失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 回零操作
        /// </summary>
        /// <param name="cardIndex">卡索引</param>
        /// <param name="axis">轴号</param>
        /// <param name="mode">回原点模式</param>
        /// <returns>回零成功返回true，失败返回false</returns>
        public bool Home(int cardIndex, int axis, MotionIO.HomeMode mode)
        {
            try
            {
                var motion = _motionMgr.GetMotion(cardIndex);
                if (motion == null)
                    return false;
                return motion.Home(axis, mode);
            }
            catch (Exception ex)
            {
                Logger.Error("MotionManager回零失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取当前位置
        /// </summary>
        /// <param name="cardIndex">卡索引</param>
        /// <param name="axis">轴号</param>
        /// <returns>返回当前位置</returns>
        public double GetPosition(int cardIndex, int axis)
        {
            try
            {
                var motion = _motionMgr.GetMotion(cardIndex);
                if (motion == null)
                    return 0;
                return motion.GetAxisPos(axis);
            }
            catch (Exception ex)
            {
                Logger.Error("MotionManager获取位置失败", ex);
                return 0;
            }
        }

        /// <summary>
        /// 停止运动
        /// </summary>
        /// <param name="cardIndex">卡索引</param>
        /// <param name="axis">轴号</param>
        /// <returns>停止成功返回true，失败返回false</returns>
        public bool Stop(int cardIndex, int axis)
        {
            try
            {
                var motion = _motionMgr.GetMotion(cardIndex);
                if (motion == null)
                    return false;
                return motion.StopAxis(axis);
            }
            catch (Exception ex)
            {
                Logger.Error("MotionManager停止失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取连接状态
        /// </summary>
        public bool IsConnected
        {
            get
            {
                try
                {
                    // 检查运动卡是否初始化成功
                    if (_motionMgr == null)
                        return false;

                    // 简单检查：如果有任何运动卡则认为已连接
                    // 实际应用中可以检查每个运动卡的状态
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error("MotionManager获取连接状态失败", ex);
                    return false;
                }
            }
        }

        private bool _disposed;

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                _motionMgr.DeInitAll();
            }
            catch (Exception ex)
            {
                Logger.Error("MotionManager释放失败", ex);
            }
            GC.SuppressFinalize(this);
        }
    }
}
