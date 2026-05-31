using System;
using System.IO;
using System.Runtime.InteropServices;
using OmniFrame.Common;

namespace MotionIO
{
    /// <summary>
    /// P/Invoke 声明模板 — 接真实硬件 DLL 的标准模式。
    /// 
    /// 使用步骤:
    ///   1. 将厂商 DLL 放到 src/MotionIO/Libs/ 下
    ///   2. 在 .csproj 中添加: &lt;Content Include="Libs\*.dll"&gt;&lt;CopyToOutputDirectory&gt;PreserveNewest&lt;/CopyToOutputDirectory&gt;&lt;/Content&gt;
    ///   3. 参考此模板写 DllImport 声明
    ///   4. 在 Motion_Xxx.cs 的 Init() 中加载 DLL，AbsMove() 等调用
    /// 
    /// 错误处理铁律:
    ///   - 始终捕获 DllNotFoundException (DLL 未找到)
    ///   - 始终捕获 BadImageFormatException (32/64 位不匹配)
    ///   - 始终捕获 SEHException (DLL 内部崩溃)
    ///   - 始终捕获 EntryPointNotFoundException (函数名错误)
    /// </summary>
    internal static class NativeMethods_Template
    {
        private const string DllName = "YOUR_DRIVER.dll";

        /// <summary>检查 DLL 文件是否存在（Init 前调用）</summary>
        public static bool IsDriverAvailable()
        {
            string path = Path.Combine(AppContext.BaseDirectory, DllName);
            if (!File.Exists(path))
            {
                Logger.Error($"驱动 DLL 未找到: {path}");
                return false;
            }
            return true;
        }

        // ── 模板: 无参数无返回 ──
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int board_init();

        // ── 模板: 值类型参数 ──
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int board_close();

        // ── 模板: 指针/输出参数 ──
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int get_position(int axis, out double pos);

        // ── 模板: 数组参数 ──
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int set_positions(int[] axes, double[] positions, int count);

        /// <summary>安全调用 DLL 函数（统一错误处理）</summary>
        public static bool SafeCall(Func<int> dllCall, string operationName)
        {
            try
            {
                int result = dllCall();
                if (result != 0)
                {
                    Logger.Error($"{operationName} 失败, 错误码: {result}");
                    return false;
                }
                return true;
            }
            catch (DllNotFoundException ex)
            {
                Logger.Error($"{operationName}: DLL 文件未找到。请确认 {DllName} 在程序目录下。", ex);
                return false;
            }
            catch (BadImageFormatException ex)
            {
                Logger.Error($"{operationName}: DLL 位数不匹配。请检查项目平台目标 (当前: {(Environment.Is64BitProcess ? "x64" : "x86")})。", ex);
                return false;
            }
            catch (EntryPointNotFoundException ex)
            {
                Logger.Error($"{operationName}: DLL 函数入口未找到。请用 dumpbin /exports 检查实际函数名。", ex);
                return false;
            }
            catch (SEHException ex)
            {
                Logger.Error($"{operationName}: DLL 内部崩溃 (SEH)。可能是传参错误或驱动未安装。", ex);
                return false;
            }
        }
    }
}
