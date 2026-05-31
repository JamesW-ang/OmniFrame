using System;
using OmniFrame.Common;
using MotionIO;

namespace OmniFrame.Core
{
    public class IoManager : IIoManager
    {
        private IoCtrl _ioCtrl;


        public IoManager()
        {
        }

        public bool Initialize(string ioType, string configPath)
        {
            try
            {
                // 根据ioType创建对应的IoCtrl实例
                switch (ioType.ToUpper())
                {
                    case "PCIEM60":
                        _ioCtrl = new IoCtrl_PCIeM60();
                        break;
                    default:
                        Logger.Error($"未知的IO类型: {ioType}");
                        return false;
                }

                if (_ioCtrl == null)
                {
                    return false;
                }

                // 使用正确的Init方法
                return _ioCtrl.Init(configPath);
            }
            catch (Exception ex)
            {
                Logger.Error("IoManager初始化失败", ex);
                return false;
            }
        }

        public bool SetOutput(int port, int pin, bool value)
        {
            try
            {
                if (_ioCtrl == null)
                    return false;
                // 使用正确的WriteOutput方法
                return _ioCtrl.WriteOutput(port, value);
            }
            catch (Exception ex)
            {
                Logger.Error("IoManager设置输出失败", ex);
                return false;
            }
        }

        public bool GetInput(int port, int pin)
        {
            try
            {
                if (_ioCtrl == null)
                    return false;

                bool value;
                // 使用正确的ReadInput方法
                if (_ioCtrl.ReadInput(port, out value))
                {
                    return value;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("IoManager获取输入失败", ex);
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
                    return _ioCtrl != null;
                }
                catch (Exception ex)
                {
                    Logger.Error("IoManager获取连接状态失败", ex);
                    return false;
                }
            }
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                _ioCtrl?.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("IoManager释放失败", ex);
            }
            GC.SuppressFinalize(this);
        }
    }
}
