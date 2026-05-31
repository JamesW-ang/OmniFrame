using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OmniFrame.Common;

namespace OmniFrame.Common
{
    /// <summary>
    /// Contains the time of the last input.
        /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LASTINPUTINFO
    {
        /// <summary>
        /// The size of the structure, in bytes. This member must be set to sizeof(LASTINPUTINFO).
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public int cbSize;
        /// <summary>
        /// The tick count when the last input event was received.
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint dwTime;
    }

    /// <summary>
    /// 帮助类
    /// 设计介绍：
    /// 2. 封装了Windows API调用，实现系统级操作
    /// 3. 提供文件操作、进程管理、系统状态查询等功能
    /// 4. 使用DllImport特性实现与原生Windows API的交互
        /// </summary>
    public static class HelpTool
    {
        /// <summary>
        /// 调用windows API获取鼠标键盘空闲时间
        /// </summary>
        /// <param name="plii"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        /// <summary>
        /// Hides the window and activates another window
        /// </summary>
        public const int SW_HIDE = 0;

        /// <summary>
        /// Activates and displays a window. 
        /// If the window is minimized or maximized, 
        /// the system restores it to its original size and position.
        ///  An application should specify this flag when displaying the window for the first time.
        /// </summary>
        public const int SW_SHOWNORMAL = 1;

        /// <summary>
        /// Activates the window and displays it in its current size and position
        /// </summary>
        public const int SW_SHOW = 5;

        /// <summary>
        /// Activates and displays the window. 
        /// If the window is minimized or maximized, 
        /// the system restores it to its original size and position. 
        /// An application should specify this flag when restoring a minimized window
        /// </summary>
        public const int SW_RESTORE = 9;

        /// <summary>
        /// 该函数设置由不同线程产生的窗口的显示状态
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="cmdShow">指定窗口如何显示。查看允许值列表，请查阅ShowWindow函数的说明部分</param>
        /// <returns>如果函数原来可见，返回值为非零；如果函数原来被隐藏，返回值为零</returns>
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        /// <summary>
        ///  该函数将创建指定窗口的线程设置到前台，并且激活该窗口。键盘输入转向该窗口，并为用户改各种可视的记号。
        ///  系统给创建前台窗口的线程分配的权限稍高于其他线程。
        /// </summary>
        /// <param name="hWnd">将被激活并被调入前台的窗口句柄</param>
        /// <returns>如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零</returns>
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// .指定的窗口是否最小化
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        /// <summary>
        /// 复制文件夹
        /// </summary>
        /// <param name="sourceFolder">源文件夹</param>
        /// <param name="destFolder">目标文件夹</param>
        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destFolder, fileName);
                File.Copy(file, destFile, true);
            }

            string[] subFolders = Directory.GetDirectories(sourceFolder);
            foreach (string subFolder in subFolders)
            {
                string folderName = Path.GetFileName(subFolder);
                string destSubFolder = Path.Combine(destFolder, folderName);
                CopyFolder(subFolder, destSubFolder);
            }
        }

        /// <summary>
        /// 连接共享文件夹
        /// </summary>
        /// <param name="sharePath">共享路径</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public static bool ConnectShare(string sharePath, string userName, string password)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "net";
                process.StartInfo.Arguments = string.Format("use {0} /user:{1} {2}", sharePath, userName, password);
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Logger.Warning($"网络共享连接失败, Path={sharePath}: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 启动应用程序
        /// </summary>
        /// <param name="appPath">应用程序路径</param>
        /// <param name="arguments">命令行参数</param>
        /// <returns></returns>
        public static Process StartApplication(string appPath, string arguments = "")
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = appPath;
                process.StartInfo.Arguments = arguments;
                process.Start();
                return process;
            }
            catch (Exception ex)
            {
                Logger.Error($"启动应用程序失败, Path={appPath}: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// 判断应用程序是否运行
        /// </summary>
        /// <param name="appName">应用程序名称</param>
        /// <returns></returns>
        public static bool IsAppRunning(string appName)
        {
            Process[] processes = Process.GetProcessesByName(appName);
            return processes.Length > 0;
        }

        /// <summary>
        /// 关闭应用程序
        /// </summary>
        /// <param name="appName">应用程序名称</param>
        /// <returns></returns>
        public static bool StopApplication(string appName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(appName);
                foreach (Process process in processes)
                {
                    process.Kill();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"停止应用程序失败, Name={appName}: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取鼠标键盘空闲时间
        /// </summary>
        /// <returns>空闲时间（秒）</returns>
        public static int GetIdleTime()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
            GetLastInputInfo(ref lastInputInfo);
            return (int)(Environment.TickCount - lastInputInfo.dwTime) / 1000;
        }
    }
}