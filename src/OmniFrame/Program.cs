using System;
using System.IO;
using System.Windows.Forms;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.Core.PluginSystem;
using OmniFrame.Core.AdvancedFeatures;
using OmniFrame.Core.BlockCut;
using OmniFrame.DataAccess;
using OmniFrame.Simulation;
using MotionIO;
using RemoteMonitor;
using Microsoft.Extensions.DependencyInjection;

namespace OmniFrame
{
    static class Program
    {
        private static ServiceProvider _provider;
        private static HealthEndpoint _healthEndpoint;

        [STAThread]
        static void Main()
        {
            try
            {
                Logger.SetLogPath("Logs");
                Logger.Info("==============================================");
                Logger.Info("应用程序启动");
                Logger.Info($"版本: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
                Logger.Info($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Logger.Info("==============================================");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
                Console.CancelKeyPress += OnCancelKeyPress;

                var services = new ServiceCollection();
                ConfigureServices(services);
                _provider = services.BuildServiceProvider();

                StartupInfrastructure(_provider);

                _provider.GetRequiredService<IUserManager>().Initialize();
                Logger.Info("用户管理器初始化完成");

                using (var loginForm = _provider.GetRequiredService<LoginForm>())
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        Logger.Info("用户取消登录，应用程序退出");
                        return;
                    }
                }

                try
                {
                    Logger.Info("初始化高级功能模块...");

                    _provider.GetRequiredService<IOeeManager>().StartProduction("DefaultLine");
                    Logger.Info("OEE管理器初始化完成");

                    Logger.Info("高级功能模块初始化完成");
                }
                catch (Exception ex)
                {
                    Logger.Error("高级功能模块初始化失败", ex);
                    bool isSimulation = false;
                    try { isSimulation = _provider.GetRequiredService<BlockCutConfig>().IsSimulation; }
                    catch (Exception cfgEx) { Logger.Warning("无法读取BlockCutConfig: " + cfgEx.Message); }
                    if (!isSimulation) throw;
                }

                using (var mainForm = _provider.GetRequiredService<MainForm>())
                {
                    Application.Run(mainForm);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用程序启动失败:\n{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Error("应用程序启动失败", ex);
            }
            finally
            {
                ShutdownInfrastructure();
                _provider?.Dispose();
                Logger.Info("应用程序退出");
            }
        }

        private static void StartupInfrastructure(ServiceProvider provider)
        {
            var configManager = provider.GetRequiredService<IConfigManager>();
            configManager.StartWatching();
            configManager.ConfigChanged += (sender, args) =>
            {
                Logger.Info($"配置热加载: {args.ConfigFileName} 已变更");
            };

            configManager.StartAutoBackup(6);

            var sysConfig = provider.GetRequiredService<IConfigManager>()
                .GetConfig<SystemConfig>("SystemCfg.xml", new SystemConfig());

            _healthEndpoint = new HealthEndpoint(
                provider.GetRequiredService<IHealthCheckService>(),
                port: sysConfig.HealthPort);
            _healthEndpoint.Start();

            // 启动产线级后台服务
            try
            {

                if (sysConfig.EnableAutoReconnect)
                {
                    var reconn = provider.GetRequiredService<IReconnectionService>();
                    reconn.Start();
                    Logger.Info("ReconnectionService 已启动 (自动重连)");
                }

                if (sysConfig.EnableWatchdog)
                {
                    var watchdog = provider.GetRequiredService<IWatchdogService>();
                    int intervalMs = sysConfig.WatchdogInterval > 0 ? sysConfig.WatchdogInterval : 1000;
                    watchdog.Start(intervalMs);
                    Logger.Info($"WatchdogService 已启动 (间隔={intervalMs}ms)");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"产线服务启动失败（不阻塞启动）: {ex.Message}", ex);
            }

            RegisterBlockCutStations(provider);
        }

        private static void RegisterBlockCutStations(ServiceProvider provider)
        {
            try
            {
                Logger.Info("注册 BlockCut 工站到 StationManager...");
                
                var stationManager = provider.GetRequiredService<IStationManager>();
                
                var adjustStation = provider.GetService<Station_Adjust>();
                if (adjustStation != null)
                {
                    stationManager.AddStation(new BlockCutStationAdapter(1, "矫正工站", adjustStation));
                    Logger.Info("矫正工站已注册");
                }
                
                var casselZStation = provider.GetService<Station_CasselZ>();
                if (casselZStation != null)
                {
                    stationManager.AddStation(new BlockCutStationAdapter(2, "料塔工站", casselZStation));
                    Logger.Info("料塔工站已注册");
                }
                
                var loadStation = provider.GetService<Station_Load>();
                if (loadStation != null)
                {
                    stationManager.AddStation(new BlockCutStationAdapter(3, "上料工站", loadStation));
                    Logger.Info("上料工站已注册");
                }
                
                var load2Station = provider.GetService<Station_Load2>();
                if (load2Station != null)
                {
                    stationManager.AddStation(new BlockCutStationAdapter(4, "二次上料工站", load2Station));
                    Logger.Info("二次上料工站已注册");
                }
                
                var bottomGetStation = provider.GetService<Station_BottomGet>();
                if (bottomGetStation != null)
                {
                    stationManager.AddStation(new BlockCutStationAdapter(5, "底板取放工站", bottomGetStation));
                    Logger.Info("底板取放工站已注册");
                }
                
                var safeStation = provider.GetService<Station_Safe>();
                if (safeStation != null)
                {
                    stationManager.AddStation(new BlockCutStationAdapter(6, "安全工站", safeStation));
                    Logger.Info("安全工站已注册");
                }
                
                Logger.Info("BlockCut 工站注册完成");
            }
            catch (Exception ex)
            {
                Logger.Error("注册 BlockCut 工站失败", ex);
            }
        }

        private static void ShutdownInfrastructure()
        {
            // 停止产线级后台服务
            try
            {
                if (_provider != null)
                {
                    _provider.GetService<IWatchdogService>()?.Stop();
                    _provider.GetService<IReconnectionService>()?.Stop();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("停止产线服务失败", ex);
            }

            _healthEndpoint?.Dispose();

            if (_provider != null)
            {
                try
                {
                    var deviceMgr = _provider.GetService<IDeviceManager>();
                    deviceMgr?.EmergencyStop();
                    deviceMgr?.Stop();

                    var configMgr = _provider.GetService<IConfigManager>();
                    configMgr?.StopAutoBackup();
                    configMgr?.StopWatching();

                    var alarmMgr = _provider.GetService<IAlarmManager>();
                    alarmMgr?.ClearAllAlarms("System");
                }
                catch (Exception ex)
                {
                    Logger.Error("关闭基础设施时发生异常", ex);
                }
            }

            Logger.Info("应用程序基础设施已释放");
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Logger.Info("收到 ProcessExit 信号，开始优雅关闭");
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Logger.Info("收到 Ctrl+C 信号，开始优雅关闭");
            ShutdownInfrastructure();
            Application.Exit();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfigManager, ConfigManager>();
            services.AddSingleton<IUserManager, UserManager>();
            services.AddSingleton<ISystemManager, SystemManager>();

            services.AddSingleton<IDeviceManager, DeviceManager>();
            services.AddSingleton<ITaskManager, TaskManager>();
            services.AddSingleton<IDataManager, DataManager>();
            services.AddSingleton<IRecipeManager, RecipeManager>();
            services.AddSingleton<IAuditLogger, OmniFrame.Core.AuditLogger>();
            services.AddSingleton<IPermissionManager, PermissionManager>();
            services.AddSingleton<IPluginManager, PluginManager>();
            services.AddSingleton<IAlarmManager, AlarmManager>();
            services.AddSingleton<IAlarmDb, AlarmDb>();
            services.AddSingleton<IProductDb, ProductDb>();
            services.AddSingleton<IStationManager, StationManager>();

            services.AddSingleton<IProductManager, ProductManager>();
            services.AddSingleton<IPlcManager, PlcManager>();
            services.AddSingleton<IIoManager, IoManager>();
            services.AddSingleton<IMotionManager, OmniFrame.Core.MotionManager>();
            services.AddSingleton<IProductionManager, ProductionManager>();
            services.AddSingleton<IReportManager, ReportManager>();
            services.AddSingleton<IAlarmNotification, AlarmNotification>();

            services.AddSingleton<IMotionIOManager, MotionIO.MotionIOManager>();

            services.AddSingleton<IBlockCutHardware>(sp =>
            {
                var cfg = sp.GetRequiredService<BlockCutConfig>();
                if (cfg.IsSimulation)
                {
                    var sim = SimulationContext.CreateSimulatedHardware(16);
                    sim.Initialize();
                    return sim;
                }
                var aps = new ApsHardware(0);
                return aps;
            });
            services.AddSingleton(sp => ((IBlockCutHardware)sp.GetRequiredService<IBlockCutHardware>()).Motion);
            services.AddSingleton(sp => ((IBlockCutHardware)sp.GetRequiredService<IBlockCutHardware>()).Io);

            services.AddSingleton<IRemoteMonitorManager, RemoteMonitorManager>();

            services.AddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddSingleton<IReconnectionService, ReconnectionService>();
            services.AddSingleton<IWatchdogService, WatchdogService>();

            services.AddSingleton<MesClient>(sp =>
            {
                var config = sp.GetRequiredService<IConfigManager>();
                string encryptionKey = Environment.GetEnvironmentVariable("OMNIFRAME_CONFIG_ENCRYPTION_KEY");
                if (string.IsNullOrEmpty(encryptionKey))
                {
                    throw new InvalidOperationException("环境变量 OMNIFRAME_CONFIG_ENCRYPTION_KEY 未设置，拒绝启动。请设置此环境变量后重新运行。");
                }
                string url = config.GetEncryptedConfig("MesConfig.xml", encryptionKey,
                    defaultValue: "http://localhost:5000");
                return new MesClient(url);
            });

            services.AddSingleton<IOeeManager, OeeManager>();
            services.AddSingleton<IMqttManager, MqttManager>();
            services.AddSingleton<IUphManager, UphManager>();

            services.AddSingleton<BlockCutConfig>(sp =>
            {
                var configMgr = sp.GetRequiredService<IConfigManager>();
                return configMgr.GetConfig<BlockCutConfig>("BlockCut.xml", new BlockCutConfig());
            });

            services.AddSingleton<BlockCutVision>();

            services.AddSingleton<BlockCutMesClient>(sp =>
            {
                var mqtt = sp.GetRequiredService<IMqttManager>();
                var config = sp.GetRequiredService<BlockCutConfig>();
                string aesKey = Environment.GetEnvironmentVariable("OMNIFRAME_MES_AES_KEY");
                if (string.IsNullOrEmpty(aesKey))
                {
                    if (config.IsSimulation)
                    {
                        Logger.Warning("OMNIFRAME_MES_AES_KEY 未设置，使用仿真模式密钥");
                        aesKey = "DefaultSimulationKey123";
                    }
                    else
                    {
                        throw new InvalidOperationException("环境变量 OMNIFRAME_MES_AES_KEY 未设置，拒绝启动。请设置此环境变量后重新运行。");
                    }
                }
                var client = new BlockCutMesClient(mqtt, aesKey);
                client.SimulationMode = config.IsSimulation;
                return client;
            });
            services.AddSingleton<BarcodeScannerClient>(sp =>
            {
                var config = sp.GetRequiredService<BlockCutConfig>();
                string host = Environment.GetEnvironmentVariable("OMNIFRAME_BARCODE_HOST");
                string portStr = Environment.GetEnvironmentVariable("OMNIFRAME_BARCODE_PORT");
                
                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(portStr))
                {
                    if (config.IsSimulation)
                    {
                        Logger.Warning("OMNIFRAME_BARCODE_HOST 或 OMNIFRAME_BARCODE_PORT 未设置，使用仿真模式默认值");
                        host = "127.0.0.1";
                        portStr = "5000";
                    }
                    else
                    {
                        throw new InvalidOperationException("环境变量 OMNIFRAME_BARCODE_HOST 或 OMNIFRAME_BARCODE_PORT 未设置，拒绝启动。请设置此环境变量后重新运行。");
                    }
                }
                int port = int.Parse(portStr);
                return new BarcodeScannerClient(host, port);
            });

            services.AddSingleton<Station_Adjust>(sp =>
            {
                var hw = sp.GetRequiredService<IBlockCutHardware>();
                var cfg = sp.GetRequiredService<BlockCutConfig>();
                return new Station_Adjust(hw, cfg);
            });
            services.AddSingleton<Station_CasselZ>(sp =>
            {
                var hw = sp.GetRequiredService<IBlockCutHardware>();
                var cfg = sp.GetRequiredService<BlockCutConfig>();
                return new Station_CasselZ(hw, cfg);
            });
            services.AddSingleton<Station_Load>(sp =>
            {
                var hw = sp.GetRequiredService<IBlockCutHardware>();
                var cfg = sp.GetRequiredService<BlockCutConfig>();
                return new Station_Load(hw, cfg);
            });
            services.AddSingleton<Station_Load2>(sp =>
            {
                var hw = sp.GetRequiredService<IBlockCutHardware>();
                var cfg = sp.GetRequiredService<BlockCutConfig>();
                return new Station_Load2(hw, cfg);
            });
            services.AddSingleton<Station_BottomGet>(sp =>
            {
                var hw = sp.GetRequiredService<IBlockCutHardware>();
                var cfg = sp.GetRequiredService<BlockCutConfig>();
                return new Station_BottomGet(hw, cfg);
            });
            services.AddSingleton<Station_Safe>(sp =>
            {
                var hw = sp.GetRequiredService<IBlockCutHardware>();
                return new Station_Safe(hw);
            });

            services.AddSingleton<StationCoordinator>();

            services.AddTransient<BlockCutMainForm>();
            services.AddTransient<BlockCutCameraForm>();
            services.AddTransient<BlockCutStatsForm>();
            services.AddTransient<BlockCutManualForm>();
            services.AddTransient<BlockCutDebugForm>();
            services.AddTransient<BlockCutLogForm>();
            services.AddTransient<BlockCutIOSignalForm>();
            services.AddTransient<BlockCutMesConfigForm>();
            services.AddTransient<BlockCutAxisSetupForm>();
            services.AddTransient<BlockCutVisionPositionForm>();
            services.AddTransient<BlockCutMeasureTestForm>();
            services.AddTransient<BlockCutProductionForm>();

            services.AddSingleton<FormFactory>();
            services.AddTransient<MainForm>();
            services.AddTransient<LoginForm>();
            services.AddTransient<MonitorForm>();
            services.AddTransient<ReportCenterForm>();
            services.AddTransient<RecipeForm>();
            services.AddTransient<SettingForm>();
            services.AddTransient<StationForm>();
            services.AddTransient<EquipmentControlForm>();
            services.AddTransient<ConfigForm>();
            services.AddTransient<ConfigWizardForm>();
            services.AddTransient<RoleManagerForm>();
            services.AddTransient<Form_Oee>();
            services.AddTransient<PluginManagerForm>();
            services.AddTransient<OperationLogForm>();
            services.AddTransient<FitLineToolForm>();
            services.AddTransient<MotionSetForm>();
            services.AddTransient<CameraDebugForm>();
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Logger.Error("UI线程异常", e.Exception);
            MessageBox.Show($"UI线程异常:\n{e.Exception.Message}\n\n{e.Exception.StackTrace}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Logger.Error("未处理的异常", ex);

            // 写入崩溃转储（配合 WER / 事后分析）
            try
            {
                string dumpDir = Path.Combine(AppContext.BaseDirectory, "Logs", "CrashDumps");
                Directory.CreateDirectory(dumpDir);
                string dumpFile = Path.Combine(dumpDir, $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                using var writer = new StreamWriter(dumpFile, append: false);
                writer.WriteLine($"=== 未处理异常转储 ===");
                writer.WriteLine($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                writer.WriteLine($"终止进程: {e.IsTerminating}");
                writer.WriteLine($"异常: {ex?.GetType().FullName}");
                writer.WriteLine($"消息: {ex?.Message}");
                writer.WriteLine($"堆栈:\n{ex?.StackTrace}");
                if (ex?.InnerException != null)
                {
                    writer.WriteLine($"内部异常: {ex.InnerException.GetType().FullName}");
                    writer.WriteLine($"内部消息: {ex.InnerException.Message}");
                    writer.WriteLine($"内部堆栈:\n{ex.InnerException.StackTrace}");
                }
                writer.WriteLine($"进程: {System.Diagnostics.Process.GetCurrentProcess().Id}");
                Logger.Info($"崩溃转储已写入: {dumpFile}");
            }
            catch { }

            if (!e.IsTerminating)
            {
                MessageBox.Show($"未处理的异常:\n{ex?.Message}", "致命错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
    }
}
