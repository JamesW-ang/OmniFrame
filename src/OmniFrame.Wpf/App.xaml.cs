using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.Core.BlockCut;
using OmniFrame.Wpf.ViewModels;
using OmniFrame.Wpf.Views;

namespace OmniFrame.Wpf
{
    public partial class App : Application
    {
        private ServiceProvider _provider;
        private static readonly string CrashDumpDir = Path.Combine(AppContext.BaseDirectory, "Logs", "CrashDumps");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                Logger.SetLogPath("Logs");
                Logger.Info("==============================================");
                Logger.Info("OmniFrame WPF v4.0 启动");
                Logger.Info($"进程: {(Environment.Is64BitProcess ? "x64" : "x86")}");
                Logger.Info($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Logger.Info("==============================================");

                var services = new ServiceCollection();
                ConfigureServices(services);
                _provider = services.BuildServiceProvider();

                StartupInfrastructure(_provider);

                _provider.GetRequiredService<IUserManager>().Initialize();
                Logger.Info("用户管理器初始化完成");

                var loginVm = _provider.GetRequiredService<LoginViewModel>();
                var loginWindow = new LoginWindow(loginVm);
                if (loginWindow.ShowDialog() != true)
                {
                    Logger.Info("用户取消登录");
                    Shutdown();
                    return;
                }

                var mainVm = _provider.GetRequiredService<MainViewModel>();
                var mainWindow = new MainWindow(mainVm);
                mainWindow.Closed += (s, args) => ShutdownInfrastructure();
                mainWindow.Show();

                Logger.Info("WPF 主窗口已启动");
            }
            catch (Exception ex)
            {
                WriteCrashDump(ex);
                Logger.Error("应用程序启动失败", ex);
                MessageBox.Show($"启动失败:\n{ex.Message}", "致命错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void StartupInfrastructure(ServiceProvider provider)
        {
            var configManager = provider.GetRequiredService<IConfigManager>();
            configManager.StartWatching();
            configManager.StartAutoBackup(6);

            var sysConfig = provider.GetRequiredService<IConfigManager>()
                .GetConfig<SystemConfig>("SystemCfg.xml", new SystemConfig());

            if (sysConfig.EnableAutoReconnect)
            {
                provider.GetRequiredService<IReconnectionService>().Start();
                Logger.Info("ReconnectionService 已启动");
            }

            if (sysConfig.EnableWatchdog)
            {
                int intervalMs = sysConfig.WatchdogInterval > 0 ? sysConfig.WatchdogInterval : 1000;
                provider.GetRequiredService<IWatchdogService>().Start(intervalMs);
                Logger.Info($"WatchdogService 已启动 ({intervalMs}ms)");
            }

            RegisterBlockCutStations(provider);
        }

        private static void RegisterBlockCutStations(ServiceProvider provider)
        {
            try
            {
                var stationManager = provider.GetRequiredService<IStationManager>();
                var stations = new (int id, string name, Func<BlockCutStationBase> factory)[]
                {
                    (1, "矫正工站",   () => provider.GetRequiredService<Station_Adjust>()),
                    (2, "料塔工站",   () => provider.GetRequiredService<Station_CasselZ>()),
                    (3, "上料工站",   () => provider.GetRequiredService<Station_Load>()),
                    (4, "二次上料工站", () => provider.GetRequiredService<Station_Load2>()),
                    (5, "底板取放工站", () => provider.GetRequiredService<Station_BottomGet>()),
                    (6, "安全工站",   () => provider.GetRequiredService<Station_Safe>()),
                };
                foreach (var (id, name, factory) in stations)
                {
                    var station = factory();
                    if (station != null)
                    {
                        stationManager.AddStation(new BlockCutStationAdapter(id, name, station));
                        Logger.Info($"{name} 已注册");
                    }
                }
            }
            catch (Exception ex) { Logger.Error("注册 BlockCut 工站失败", ex); }
        }

        private void ShutdownInfrastructure()
        {
            try
            {
                _provider?.GetService<IWatchdogService>()?.Stop();
                _provider?.GetService<IReconnectionService>()?.Stop();
                var deviceMgr = _provider?.GetService<IDeviceManager>();
                deviceMgr?.EmergencyStop();
                deviceMgr?.Stop();
            }
            catch (Exception ex) { Logger.Error("关闭基础设施异常", ex); }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            DiConfigurator.ConfigureServices(services);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ShutdownInfrastructure();
            _provider?.Dispose();
            Logger.Info("WPF 应用程序退出");
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            WriteCrashDump(e.Exception);
            Logger.Error("WPF UI 线程未处理异常", e.Exception);
            MessageBox.Show($"UI 异常:\n{e.Exception.Message}\n\n详细信息已写入崩溃转储。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private static void WriteCrashDump(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(CrashDumpDir);
                string file = Path.Combine(CrashDumpDir, $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                using var w = new StreamWriter(file);
                w.WriteLine($"=== 崩溃转储 ===");
                w.WriteLine($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                w.WriteLine($"进程: {(Environment.Is64BitProcess ? "x64" : "x86")}");
                w.WriteLine($"异常: {ex?.GetType().FullName}");
                w.WriteLine($"消息: {ex?.Message}");
                w.WriteLine($"堆栈:\n{ex?.StackTrace}");
                if (ex?.InnerException != null)
                {
                    w.WriteLine($"--- 内部异常 ---");
                    w.WriteLine($"{ex.InnerException.GetType().FullName}: {ex.InnerException.Message}");
                    w.WriteLine($"{ex.InnerException.StackTrace}");
                }
                Logger.Info($"崩溃转储已写入: {file}");
            }
            catch { /* 如果连写文件都失败，不要递归崩溃 */ }
        }
    }
}
