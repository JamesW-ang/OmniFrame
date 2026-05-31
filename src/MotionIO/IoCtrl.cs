using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace MotionIO
{
    /// <summary>
    /// IO控制基类
    /// </summary>
    public abstract class IoCtrl
    {
        /// <summary>
        /// 初始化IO控制
        /// </summary>
        /// <param name="param">初始化参数</param>
        /// <returns>是否成功</returns>
        public abstract bool Init(object param);

        /// <summary>
        /// 关闭IO控制
        /// </summary>
        /// <returns>是否成功</returns>
        public abstract bool Close();

        /// <summary>
        /// 读取输入
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public abstract bool ReadInput(int port, out bool value);

        /// <summary>
        /// 读取输入端口
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public abstract bool ReadInputPort(int port, out int value);

        /// <summary>
        /// 写入输出
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public abstract bool WriteOutput(int port, bool value);

        /// <summary>
        /// 写入输出端口
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public abstract bool WriteOutputPort(int port, int value);

        /// <summary>
        /// 读取所有输入
        /// </summary>
        /// <returns>输入状态</returns>
        public abstract Dictionary<int, bool> ReadAllInputs();

        /// <summary>
        /// 读取所有输出
        /// </summary>
        /// <returns>输出状态</returns>
        public abstract Dictionary<int, bool> ReadAllOutputs();

        /// <summary>
        /// 复位IO
        /// </summary>
        /// <returns>是否成功</returns>
        public abstract bool Reset();

        /// <summary>
        /// 获取错误信息
        /// </summary>
        /// <returns>错误信息</returns>
        public abstract string GetError();

        #region BlockCut 扩展方法 (virtual — 子类可覆写)

        /// <summary>设置 DO 点</summary>
        public virtual void SetDO(int index, bool value)
        {
            WriteOutput(index, value);
            Logger.Debug($"[IO] DO[{index}] = {(value ? "ON" : "OFF")}");
        }

        /// <summary>读取 DI 点</summary>
        public virtual bool GetDI(int index)
        {
            ReadInput(index, out bool val);
            return val;
        }

        /// <summary>等待单个 DI 点到位</summary>
        public virtual async Task<bool> WaitDIAsync(int diChannel, bool expectedValue,
            int timeoutMs = 10000, string name = "",
            Action<string> onTimeoutWarn = null,
            CancellationToken token = default)
        {
            string label = string.IsNullOrEmpty(name) ? $"DI[{diChannel}]" : name;
            Logger.Debug($"[IO] 等待 {label} = {expectedValue}");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long lastWarnMs = 0;

            while (!token.IsCancellationRequested)
            {
                if (ReadInput(diChannel, out bool val) && val == expectedValue)
                {
                    Logger.Debug($"[IO] {label} 到位 ({sw.ElapsedMilliseconds}ms)");
                    return true;
                }

                long elapsed = sw.ElapsedMilliseconds;
                if (elapsed - lastWarnMs >= timeoutMs)
                {
                    lastWarnMs = elapsed;
                    string msg = string.IsNullOrEmpty(name)
                        ? $"DI[{diChannel}] 未到位，已超过 {timeoutMs / 1000} 秒，请确认！"
                        : $"{name} 未到位，已超过 {timeoutMs / 1000} 秒，请确认！";
                    onTimeoutWarn?.Invoke(msg);
                    Logger.Warning($"[IO] {msg}");
                }

                await Task.Delay(1, token);
            }

            return false;
        }

        /// <summary>等待两个 DI 点同时到位</summary>
        public virtual async Task<bool> WaitDI2Async(int diCh1, bool exp1, int diCh2, bool exp2,
            int timeoutMs = 10000, string name = "",
            Action<string> onTimeoutWarn = null,
            CancellationToken token = default)
        {
            string label = string.IsNullOrEmpty(name) ? $"DI[{diCh1},{diCh2}]" : name;
            Logger.Debug($"[IO] 等待 {label}");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long lastWarnMs = 0;

            while (!token.IsCancellationRequested)
            {
                bool ok1 = ReadInput(diCh1, out bool v1) && v1 == exp1;
                bool ok2 = ReadInput(diCh2, out bool v2) && v2 == exp2;

                if (ok1 && ok2)
                {
                    Logger.Debug($"[IO] {label} 到位 ({sw.ElapsedMilliseconds}ms)");
                    return true;
                }

                long elapsed = sw.ElapsedMilliseconds;
                if (elapsed - lastWarnMs >= timeoutMs)
                {
                    lastWarnMs = elapsed;
                    string msg = string.IsNullOrEmpty(name)
                        ? $"DI[{diCh1},{diCh2}] 未到位，已超过 {timeoutMs / 1000} 秒，请确认！"
                        : $"{name} 未到位，已超过 {timeoutMs / 1000} 秒，请确认！";
                    onTimeoutWarn?.Invoke(msg);
                    Logger.Warning($"[IO] {msg}");
                }

                await Task.Delay(1, token);
            }

            return false;
        }

        /// <summary>亮绿灯</summary>
        public virtual void GreenLight(bool on) => WriteOutput(21, on);
        /// <summary>亮黄灯</summary>
        public virtual void YellowLight(bool on) => WriteOutput(22, on);
        /// <summary>亮红灯</summary>
        public virtual void RedLight(bool on) => WriteOutput(23, on);
        /// <summary>蜂鸣器</summary>
        public virtual void Buzzer(bool on) => WriteOutput(24, on);
        /// <summary>LED 照明</summary>
        public virtual void LED(bool on) => WriteOutput(20, on);

        #endregion
    }
}
