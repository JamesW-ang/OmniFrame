using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.Core.PluginSystem;
using OmniFrame.Core.AdvancedFeatures;
using OmniFrame.Core.BlockCut;
using OmniFrame.DataAccess;
using OmniFrame.Simulation;
using MotionIO;
using RemoteMonitor;

namespace OmniFrame.Wpf
{
    /// <summary>
    /// DI 容器配置 — 组合根。
    /// 集中管理所有服务注册（Manager / 硬件 / 工站 / ViewModel）。
    /// </summary>
    public static class DiConfigurator
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // ── 核心 Manager ──
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

            // ── 硬件抽象 (仿真 / 真实 APS) ──
            services.AddSingleton<IBlockCutHardware>(sp =>
            {
                var cfg = sp.GetRequiredService<BlockCutConfig>();
                if (cfg.IsSimulation)
                {
                    var sim = SimulationContext.CreateSimulatedHardware(16);
                    sim.Initialize();
                    return sim;
                }
                return new ApsHardware(0);
            });
            services.AddSingleton(sp => ((IBlockCutHardware)sp.GetRequiredService<IBlockCutHardware>()).Motion);
            services.AddSingleton(sp => ((IBlockCutHardware)sp.GetRequiredService<IBlockCutHardware>()).Io);

            // ── 远程监控 ──
            services.AddSingleton<IRemoteMonitorManager, RemoteMonitorManager>();
            services.AddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddSingleton<IReconnectionService, ReconnectionService>();
            services.AddSingleton<IWatchdogService, WatchdogService>();

            // ── MES / 高级功能 ──
            services.AddSingleton<IOeeManager, OeeManager>();
            services.AddSingleton<IMqttManager, MqttManager>();
            services.AddSingleton<IUphManager, UphManager>();

            // ── BlockCut 配置 ──
            services.AddSingleton<BlockCutConfig>(sp =>
                sp.GetRequiredService<IConfigManager>().GetConfig<BlockCutConfig>("BlockCut.xml", new BlockCutConfig()));
            services.AddSingleton<BlockCutVision>();
            services.AddSingleton<BlockCutMesClient>(sp =>
            {
                var mqtt = sp.GetRequiredService<IMqttManager>();
                var config = sp.GetRequiredService<BlockCutConfig>();
                string aesKey = Environment.GetEnvironmentVariable("OMNIFRAME_MES_AES_KEY");
                if (string.IsNullOrEmpty(aesKey))
                {
                    if (!config.IsSimulation)
                        throw new InvalidOperationException("OMNIFRAME_MES_AES_KEY 未设置");
                    aesKey = "DefaultSimulationKey123";
                }
                var client = new BlockCutMesClient(mqtt, aesKey);
                client.SimulationMode = config.IsSimulation;
                return client;
            });
            services.AddSingleton<BarcodeScannerClient>(sp =>
            {
                var config = sp.GetRequiredService<BlockCutConfig>();
                string host = Environment.GetEnvironmentVariable("OMNIFRAME_BARCODE_HOST") ?? "127.0.0.1";
                string portStr = Environment.GetEnvironmentVariable("OMNIFRAME_BARCODE_PORT") ?? "5000";
                return new BarcodeScannerClient(host, int.Parse(portStr));
            });

            // ── BlockCut 工站 ──
            services.AddSingleton<Station_Adjust>(sp =>
                new Station_Adjust(sp.GetRequiredService<IBlockCutHardware>(), sp.GetRequiredService<BlockCutConfig>()));
            services.AddSingleton<Station_CasselZ>(sp =>
                new Station_CasselZ(sp.GetRequiredService<IBlockCutHardware>(), sp.GetRequiredService<BlockCutConfig>()));
            services.AddSingleton<Station_Load>(sp =>
                new Station_Load(sp.GetRequiredService<IBlockCutHardware>(), sp.GetRequiredService<BlockCutConfig>()));
            services.AddSingleton<Station_Load2>(sp =>
                new Station_Load2(sp.GetRequiredService<IBlockCutHardware>(), sp.GetRequiredService<BlockCutConfig>()));
            services.AddSingleton<Station_BottomGet>(sp =>
                new Station_BottomGet(sp.GetRequiredService<IBlockCutHardware>(), sp.GetRequiredService<BlockCutConfig>()));
            services.AddSingleton<Station_Safe>(sp =>
                new Station_Safe(sp.GetRequiredService<IBlockCutHardware>()));
            services.AddSingleton<StationCoordinator>();

            // ── WPF ViewModels (Transient: 每次导航新实例) ──
            services.AddTransient<ViewModels.LoginViewModel>();
            services.AddTransient<ViewModels.MainViewModel>();
            services.AddTransient<ViewModels.BlockCutViewModel>(sp => new ViewModels.BlockCutViewModel(sp.GetRequiredService<StationCoordinator>(), sp.GetRequiredService<IAlarmManager>(), sp.GetRequiredService<ISystemManager>(), sp.GetRequiredService<BlockCutConfig>(), sp.GetRequiredService<IUphManager>(), sp.GetRequiredService<Motion>(), sp.GetRequiredService<IoCtrl>()));
            services.AddTransient<ViewModels.ReportViewModel>();
            services.AddTransient<ViewModels.RecipeViewModel>();
            services.AddTransient<ViewModels.EquipmentViewModel>();
            services.AddTransient<ViewModels.StationViewModel>();
            services.AddTransient<ViewModels.OeeViewModel>();
            services.AddTransient<ViewModels.SettingsViewModel>();
            services.AddTransient<ViewModels.BlockCutProductionViewModel>();
            services.AddTransient<ViewModels.BlockCutStatsViewModel>();
            services.AddTransient<ViewModels.PluginViewModel>();
            services.AddTransient<ViewModels.OperationLogViewModel>();
            services.AddTransient<ViewModels.CameraDebugViewModel>();
            services.AddTransient<ViewModels.MotionSetViewModel>();
            services.AddTransient<ViewModels.FitLineToolViewModel>();
            services.AddTransient<ViewModels.WorkManagementViewModel>();
            services.AddTransient<ViewModels.AxisSetupViewModel>();
            services.AddTransient<ViewModels.MeasureTestViewModel>();
        }
    }
}
