# BlockCut V1.7.1 → OmniFrame 迁移方案

## 一、项目概述

### 1.1 源项目：BlockCut V1.7.1
- **技术栈**：Qt 5.12 / C++11 / MSVC 2015 64-bit
- **功能**：精密玻璃/晶圆切割、摆放、点胶、UV 固化上位机
- **硬件**：APS PCI 运动控制卡 (APS168x64)，16 轴，Basler 相机，25 DO + 36 DI
- **视觉**：Halcon 边缘检测 + 线拟合算法库 (FitLineAlg)
- **通信**：MQTT (Qt5Mqtt) + HTTP MES + AES-128-ECB 加密
- **代码量**：37 .cpp / 26 .h / 20 .ui / ~15000 行核心逻辑

### 1.2 目标架构：OmniFrame
- **技术栈**：.NET Framework 4.8 / WinForms / C# 7.3+
- **DI 容器**：Microsoft.Extensions.DependencyInjection
- **核心模式**：SystemManager 门面 + 34 Manager + StationBase 工站
- **运动控制**：Motion 抽象基类 → GTS / DMC / Inovance / PCIeM60 实现
- **IO 控制**：IoCtrl 抽象基类
- **配置**：XML (ConfigManager) + AES 加密

---

## 二、模块映射表

### 2.1 运动控制子系统

| Qt C++ 源文件 | 迁移目标 C# 文件 | 说明 |
|---|---|---|
| `Motion/Axis/APS/ApsAxis.h/.cpp` | `MotionIO/Motion_APS.cs` | APS 卡 P/Invoke 封装，继承 Motion 基类 |
| `Motion/IOSignal/IOSignal.h/.cpp` | `MotionIO/IoCtrl_APS.cs` | APS 卡板载 IO，继承 IoCtrl 基类 |
| `Motion/Motion.h/.cpp` | 复用现有 `MotionManager.cs` | UI 层运动控制面板 |
| `Motion/Thread/ThreadParent.h/.cpp` | `BlockCutStationBase.cs` | 线程基类 → 工站抽象基类 |
| `Motion/Thread/ThreadCasselZ.cpp` | `Station_CasselZ.cs` | CasselZ 工站 |
| `Motion/Thread/ThreadLoad1.cpp` | `Station_Load.cs` | 上料工站 |
| `Motion/Thread/ThreadLoad2.cpp` | `Station_Load2.cs` | 二次上料工站 |
| `Motion/Thread/ThreadAdjust.cpp` | `Station_Adjust.cs` | 核心调整/生产工站 (~3041 行) |
| `Motion/Thread/ThreadBottomGetX.cpp` | `Station_BottomGet.cs` | 底板取放工站 |
| `Motion/Thread/ThreadSafe.cpp` | `Station_Safe.cs` | 安全监控工站 |
| `BaseData.h` | `BlockCutConstants.cs` + `BlockCutEnums.cs` | 轴/IO/线程枚举、超时常量 |

### 2.2 视觉子系统

| Qt C++ 源文件 | 迁移目标 C# 文件 | 说明 |
|---|---|---|
| `Alg/FitLine/FitLineAlg.h/.cpp` | `Vision/BlockCutVision.cs` | Halcon 线拟合，实现 IVisionSystem |
| `Vision/FitLine/FitLineWidget.h/.cpp` | `UI/FitLineControl.cs` | WinForms 视觉参数控件 |
| `Camera/CameraWidget.h/.cpp` | 复用 Basler .NET SDK | 相机采集（已有 Basler SDK 可用） |
| `Camera/CameraView.h/.cpp` | `UI/CameraViewControl.cs` | 相机显示控件 |

### 2.3 MES / 通信子系统

| Qt C++ 源文件 | 迁移目标 C# 文件 | 说明 |
|---|---|---|
| `Include/qmqttclient.h` | 扩展 `AdvancedFeatures/MqttManager.cs` | 添加 AES 加密发布/订阅 |
| `Mes/HttpToMes.h/.cpp` | `Communication/BlockCutMesClient.cs` | HTTP MES 验证 |
| `qaesencryption.h/.cpp` | 扩展 `Common/Security.cs` | 添加 `Aes128EcbEncrypt/Decrypt` |
| `Setting/Log/Log.h/.cpp` | 复用 `OperationLogService.cs` | Excel 日志写入 |
| `TcpSocket/` | `Communication/BarcodeScannerClient.cs` | 扫码枪 TCP 客户端 |

### 2.4 UI 子系统

| Qt C++ 源文件 (Qt UI) | 迁移目标 C# 文件 (WinForms) | 说明 |
|---|---|---|
| `MainWindow.h/.cpp/.ui` (~2228 行) | `UI/BlockCutMainForm.cs` | 主窗体，全屏无边框 |
| `Work/Work.h/.cpp/.ui` | `UI/WorkSelectDialog.cs` | 工单选择对话框 |
| `Statistic/Statistic.h/.cpp/.ui` | `UI/StatisticsControl.cs` | 图表统计面板 |
| `Motion/Motion.ui` | 复用现有 `MotionControl.cs` | 运动控制面板 |
| `Setting/` 相关 | `UI/SettingsForm.cs` | 参数设置表单 |

### 2.5 配置 / 数据子系统

| Qt 源 | 迁移目标 | 说明 |
|---|---|---|
| `config.ini` / `D:/list.ini` | `Config/BlockCut.xml` | INI → XML 配置迁移 |
| QSettings 读写 | 复用 `ConfigManager` | XML 配置管理器 |
| Excel 日志写入 | 复用 `OperationLogService` | 生产日志 |
| 6 组参数映射 (list.ini) | XML recipe 节点 | 产品参数配方 |

---

## 三、核心类设计

### 3.1 Motion_APS : Motion

```csharp
// MotionIO/Motion_APS.cs
public class Motion_APS : Motion
{
    // P/Invoke APS168x64 SDK
    [DllImport("APS168x64.dll")]
    static extern int APS_init_board(int board_id);
    
    [DllImport("APS168x64.dll")]
    static extern int APS_set_axis_param(int board_id, int axis_id, 
        double acc, double dec, double vs, double vm, double vh);
    
    [DllImport("APS168x64.dll")]
    static extern int APS_absolute_move(int board_id, int axis_id, 
        double position, int mode);
    
    [DllImport("APS168x64.dll")]
    static extern int APS_relative_move(int board_id, int axis_id, 
        double distance, int mode);
    
    [DllImport("APS168x64.dll")]
    static extern int APS_stop_move(int board_id, int axis_id, int mode);
    
    [DllImport("APS168x64.dll")]
    static extern int APS_get_position(int board_id, int axis_id, 
        ref double position);
    
    [DllImport("APS168x64.dll")]
    static extern int APS_get_axis_state(int board_id, int axis_id, 
        ref int state);
    
    // 16 轴轴号映射 (eAxisCasselZ=0 .. eAxisLoadX=15)
    public override bool InitBoard(int boardId, Dictionary<int, AxisConfig> axes);
    public override bool MoveAbs(int axis, double pos, double speed);
    public override bool MoveRel(int axis, double dist, double speed);
    public override bool StopAxis(int axis);
    public override double GetPosition(int axis);
    
    // 轴状态等待 (替代 ThreadParent::CountRunTime)
    public bool WaitAxisDone(int axis, int timeoutMs = 10000);
}
```

### 3.2 IoCtrl_APS : IoCtrl

```csharp
// MotionIO/IoCtrl_APS.cs
public class IoCtrl_APS : IoCtrl
{
    [DllImport("APS168x64.dll")]
    static extern int APS_set_do_bit(int board_id, int do_index, int value);
    
    [DllImport("APS168x64.dll")]
    static extern int APS_get_di_bit(int board_id, int di_index, ref int value);
    
    // 25 DO + 36 DI（映射自 BaseData.h）
    // DO: OUTPUT_JigYCylinderOut(0) ... OUTPUT_BUZZER(24)
    // DI: INPUT_CheckCassel(0) ... INPUT_BottomYCylinderIn(35)
    public override bool SetDO(int index, bool value);
    public override bool GetDI(int index, out bool value);
    
    // 等待 DI 信号（替代 10s 超时逻辑）
    public bool WaitDI(int diIndex, bool expectedValue, int timeoutMs = 10000);
}
```

### 3.3 BlockCutStationBase : StationBase

```csharp
// OmniFrame.Core/BlockCutStationBase.cs
public abstract class BlockCutStationBase : StationBase
{
    protected Motion_APS Motion { get; private set; }
    protected IoCtrl_APS Io { get; private set; }
    protected CancellationTokenSource Cts { get; set; }
    
    // 替代 ThreadParent 方法
    protected bool SetOneCylinder(int doIndex, int diIndex, 
        bool outValue, int timeoutMs = 10000);
    protected bool CheckPause(CancellationToken token);
    protected void WaitForUser(ManualResetEventSlim signal);
    protected void PauseWarnMessage(string errCode, string message);
    protected bool OneAxisMoveAbs(int axis, double pos, double speed);
    protected bool TwoAxisMoveAbs(int a1, double p1, int a2, double p2, 
        double speed);
    protected bool UVWRotate(double angle, double cx, double cy); // 3 轴联动
    
    // 替代 QThread::msleep
    protected Task DelayMs(int ms, CancellationToken token);
    
    // 工站主循环（替代 ThreadParent::Run 的 while 循环）
    public abstract Task RunAsync(CancellationToken token);
}
```

### 3.4 Station_Adjust : BlockCutStationBase（核心工站）

```csharp
// OmniFrame.Core/Station_Adjust.cs
public class Station_Adjust : BlockCutStationBase
{
    // 对应 ThreadAdjust::Run() 主流程：
    public override async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            token.ThrowIfCancellationRequested();
            if (CheckPause(token)) continue;
            
            // 1. 检查底板就绪
            await CheckBottomPlateAsync(token);
            
            // 2. 扫码 → MES 验证
            var barcode = await ScanBarcodeAsync(token);
            var mesResult = await ValidateBarcodeAsync(barcode, token);
            if (!mesResult.IsValid) continue;
            
            // 3. 获取参数组 (替代 GetParamFromCode)
            var recipe = GetRecipeFromBarcode(barcode);
            
            // 4. UVW 初始化
            await UVWInitAsync(token);
            
            // 5. 行×列循环生产
            for (int row = 0; row < recipe.Rows; row++)
            {
                for (int col = 0; col < recipe.Cols; col++)
                {
                    token.ThrowIfCancellationRequested();
                    
                    await MeasureHeightAsync(row, col, token);      // 9 点测高
                    var angle = await DetectBottomAngleAsync(token);  // 角度检测
                    await UVWRotateAsync(angle, token);               // UVW 旋转
                    await PlaceSliceAsync(row, col, token);           // 摆放
                    await AlignSliceAsync(token);                     // 对齐 (Y1/Y2)
                    await DispenseGlueAsync(token);                   // 点胶
                    await UVCureAsync(token);                         // UV 固化
                }
            }
            
            // 6. 输出满板
            await OutputFullPlateAsync(token);
        }
    }
}
```

### 3.5 BlockCutVision : IVisionSystem

```csharp
// Vision/BlockCutVision.cs
public class BlockCutVision : IVisionSystem
{
    // Halcon .NET API 封装
    // 替代 CFitLineAlg + FitLineWidget
    public bool FitLine(HImage image, ROIRegion roi, FitLineParams p,
        out PointF p1, out PointF p2, out double angle);
    
    // 替代 BottomCameraGetAngle (两点相机取像 + 角度计算)
    public double DetectAngle(HImage img1, HImage img2, ROIRegion roi);
    
    // 替代 9 点高度测量
    public bool MeasureHeight(HImage[] images, out double[] heights);
    
    // 替代 GetMaxGray / 自动曝光调整
    public int GetMaxGray(HImage image);
    public double GetAvgGray(HImage image);
    public bool AutoExposure(HImage image, int targetGray);
    
    // ROI 区域管理
    public void DefineROI(ROIRegion region);
    public void ClearROIs();
}
```

### 3.6 BlockCutMesClient

```csharp
// Communication/BlockCutMesClient.cs
public class BlockCutMesClient
{
    private readonly MqttManager _mqtt;
    private readonly HttpClient _http;
    
    // 替代 HttpToMes::CheckCard
    public async Task<bool> ValidateCardAsync(string cardId, CancellationToken ct);
    
    // 替代 MainWindow::SlotStatusMessage (AES-128-ECB + MQTT)
    public async Task SendStatusAsync(MachineStatus status, CancellationToken ct);
    
    // 替代 MainWindow::SlotWorkMessage
    public async Task SendWorkReportAsync(WorkReport report, CancellationToken ct);
    
    // AES-128-ECB 加密（调用 Security.Aes128EcbEncrypt）
    private string EncryptPayload(object data);
    private T DecryptPayload<T>(string encrypted);
}
```

### 3.7 Security 扩展

```csharp
// Common/Security.cs 中添加
public static class Security
{
    // 现有方法...
    
    // 新增 AES-128-ECB 加密（替代 qaesencryption.cpp）
    public static string Aes128EcbEncrypt(string plainText, string key);
    public static string Aes128EcbDecrypt(string cipherText, string key);
}
```

---

## 四、实施计划

### Phase 1 — MVP (最小可行产品)，约 12 周 / 60 人天

| 序号 | 任务 | 产出物 | 估时 |
|---|---|---|---|
| 1.1 | APS 卡 P/Invoke 封装 | `Motion_APS.cs`, `IoCtrl_APS.cs` | 5d |
| 1.2 | BlockCut 常量/枚举定义 | `BlockCutConstants.cs`, `BlockCutEnums.cs` | 1d |
| 1.3 | BlockCutStationBase 基类 | `BlockCutStationBase.cs` | 3d |
| 1.4 | Station_BottomGet（底板取放） | `Station_BottomGet.cs` | 4d |
| 1.5 | Station_Load（上料） | `Station_Load.cs` | 3d |
| 1.6 | Station_Load2（二次上料） | `Station_Load2.cs` | 2d |
| 1.7 | Station_CasselZ（CasselZ） | `Station_CasselZ.cs` | 2d |
| 1.8 | Station_Safe（安全监控） | `Station_Safe.cs` | 3d |
| 1.9 | Station_Adjust（核心生产） | `Station_Adjust.cs` | 10d |
| 1.10 | Halcon 视觉 .NET 封装 | `BlockCutVision.cs` | 5d |
| 1.11 | Security AES-128-ECB 扩展 | 扩展 `Security.cs` | 1d |
| 1.12 | MqttManager MES 扩展 | 扩展 `MqttManager.cs` | 2d |
| 1.13 | BlockCutMesClient | `BlockCutMesClient.cs` | 3d |
| 1.14 | BlockCutMainForm (WinForms UI) | `BlockCutMainForm.cs` | 5d |
| 1.15 | INI → XML 配置工具 | `IniToXmlConverter.cs` + 配置 XML | 2d |
| 1.16 | 扩展 SystemManager / DI 注册 | 修改 `SystemManager.cs`, `Program.cs` | 2d |
| 1.17 | 集成测试（空跑 / 模拟） | 测试报告 | 7d |

### Phase 2 — 完整功能，约 10 周 / 53 人天

| 序号 | 任务 | 产出物 | 估时 |
|---|---|---|---|
| 2.1 | 扫码枪 TCP 客户端 | `BarcodeScannerClient.cs` | 2d |
| 2.2 | 统计图表面板 | `StatisticsControl.cs` | 3d |
| 2.3 | 工单选择对话框 | `WorkSelectDialog.cs` | 2d |
| 2.4 | FitLine 视觉参数控件 | `FitLineControl.cs` | 3d |
| 2.5 | 参数设置 UI | `SettingsForm.cs` | 3d |
| 2.6 | 角色权限 UI 切换 | 扩展 `PermissionManager` + UI | 2d |
| 2.7 | 报警代码映射 (ERR-DOR-01 等) | `BlockCutAlarmMap.cs` | 1d |
| 2.8 | 运动控制 UI 面板 | 复用 + 定制现有 MotionControl | 3d |
| 2.9 | Basler 相机 .NET SDK 接入 | 扩展 `BlockCutVision.cs` | 3d |
| 2.10 | 24h 产量统计 + UPH 计算 | `ProductivityTracker.cs` | 2d |
| 2.11 | 定时器（替代 QTimer 1s timeout） | `BlockCutTimerService.cs` | 1d |
| 2.12 | 日志/Excel 写入 | 扩展 `OperationLogService` | 2d |
| 2.13 | MQTT 订阅回复处理 | 扩展 `MqttManager.cs` | 2d |
| 2.14 | 真机调试（16 轴 + IO + 相机） | 调试报告 | 10d |
| 2.15 | 性能优化 + 稳定性 | — | 5d |
| 2.16 | 打包部署脚本 | 安装包 / 部署文档 | 2d |

### 总计：22 周 / ~113 人天

---

## 五、文件清单

### 新增文件（37+）

```
MotionIO/
├── Motion_APS.cs              # APS 卡运动控制 P/Invoke
├── IoCtrl_APS.cs              # APS 卡 IO 控制 P/Invoke

OmniFrame.Core/
├── BlockCutStationBase.cs     # BlockCut 工站基类
├── Station_CasselZ.cs         # CasselZ 工站
├── Station_Load.cs            # 上料工站
├── Station_Load2.cs           # 二次上料工站
├── Station_Adjust.cs          # 核心调整工站 (~800 行)
├── Station_BottomGet.cs       # 底板取放工站
├── Station_Safe.cs            # 安全监控工站
├── BlockCutConstants.cs       # 轴/IO/线程常量
├── BlockCutEnums.cs           # 轴/IO/线程枚举
├── BlockCutAlarmMap.cs        # 报警码映射
├── BlockCutTimerService.cs    # 定时器服务
├── ProductivityTracker.cs     # UPH / 产量统计

Vision/
├── BlockCutVision.cs          # Halcon 视觉系统
├── IVisionSystem.cs           # 视觉接口（如不存在）

Communication/
├── BlockCutMesClient.cs       # MES 协议客户端
├── BarcodeScannerClient.cs    # 扫码枪 TCP 客户端

UI/
├── BlockCutMainForm.cs        # 主窗体
├── BlockCutMainForm.Designer.cs
├── WorkSelectDialog.cs        # 工单选择
├── FitLineControl.cs          # 视觉参数
├── CameraViewControl.cs       # 相机显示
├── StatisticsControl.cs       # 统计面板
├── SettingsForm.cs            # 参数设置

Config/
├── BlockCut.xml               # 系统配置 XML
├── Recipes.xml                # 产品配方 XML

Tests/
├── Station_Adjust_Tests.cs    # 核心工站单元测试
├── Motion_APS_Simulation.cs   # 运动模拟（测试用）
```

### 修改现有文件（10）

| 文件 | 变更内容 |
|---|---|
| `Common/Security.cs` | 添加 `Aes128EcbEncrypt/Decrypt` |
| `Core/AdvancedFeatures/MqttManager.cs` | 添加加密发布/订阅方法 |
| `Core/SystemManager.cs` | 添加 BlockCut 相关 Manager 属性 |
| `Core/MotionManager.cs` | 扩展支持 APS 卡类型 |
| `Core/ProductionManager.cs` | 添加 BlockCut 生产流程 |
| `Core/ConfigManager.cs` | 添加 BlockCut XML 配置节读取 |
| `Core/AlarmManager.cs` | 注册 BlockCut 报警码 |
| `Core/CylinderManager.cs` | 添加 BlockCut 气缸操作 |
| `OmniFrame/Program.cs` | DI 注册新服务 |
| `MotionIO/MotionIOManager.cs` | APS 卡添加到工厂 |

---

## 六、关键技术映射

### 6.1 线程模型迁移

| Qt C++ 模式 | .NET / C# 对应 |
|---|---|
| `QThread` + `while(!isInterruptionRequested())` | `Task.Run(() => Station.RunAsync(CancellationToken))` |
| `QThread::msleep(n)` | `await Task.Delay(n, token)` |
| `QThread::requestInterruption()` | `CancellationTokenSource.Cancel()` |
| `CheckPause()` (busy spin) | `ManualResetEventSlim.Wait(token)` |
| `WaitForUser()` (busy spin) | `ManualResetEventSlim.Wait(token)` + UI 事件 |
| `connect(sender, SIGNAL, receiver, SLOT)` | `sender.Event += receiver.Handler` (C# event) |
| `QMap<QString, QVariant>` | `Dictionary<string, object>` |
| `QMutex` + `QMutexLocker` | `lock(_mutex)` / `SemaphoreSlim` |

### 6.2 配置格式迁移

```
// INI 原格式
[Work]
workID=W001
machID=M001

[AxisParam]
CasselZ_speed=100.0
LoadX_speed=200.0

↓ 迁移为 ↓

<!-- XML 目标格式 -->
<BlockCut>
  <Work workID="W001" machID="M001" />
  <Axes>
    <Axis name="CasselZ" index="0" speed="100.0" acc="500" dec="500" />
    <Axis name="LoadX" index="1" speed="200.0" acc="500" dec="500" />
    ...
  </Axes>
  <IO>
    <DO index="0" name="JigYCylinderOut" />
    ...
  </IO>
  <Recipes>
    <Recipe code="A001" rows="5" cols="4" plateWidth="300" plateHeight="200" />
    ...
  </Recipes>
</BlockCut>
```

### 6.3 信号/槽 → C# Event 映射

| Qt 信号 (MainWindow) | C# Event (BlockCutMainForm) |
|---|---|
| `GoOnRunOneThread` | `StationManager.RequestResume` |
| `EmitAlarmMessage(int, QString)` | `AlarmManager.OnAlarm(AlarmEventArgs)` |
| `NoticeBottomSweepCode(QString)` | `BarcodeScannerClient.OnBarcodeScanned(string)` |
| `EmitCameraMessage(QString)` | `CameraManager.OnMessage(string)` |
| `UpdateDisOnceTime(QString)` | `UIManager.OnStatusUpdate(string)` |
| `ChangeWork(bool, QMap, QString, QVector)` | `WorkManager.OnWorkChanged(WorkEventArgs)` |

---

## 七、16 轴轴号映射表

| eAxisID 枚举 | 轴号 | 名称 | 功能 |
|---|---|---|---|
| eAxisCasselZ | 0 | CasselZ | Cassel Z 升降 |
| eAxisLoadX | 1 | LoadX | 上料 X 轴 |
| eAxisLoadY | 2 | LoadY | 上料 Y 轴 |
| eAxisAdjustX | 3 | AdjustX | 调整 X 轴 |
| eAxisAdjustY1 | 4 | AdjustY1 | 调整 Y1 轴 |
| eAxisAdjustY2 | 5 | AdjustY2 | 调整 Y2 轴 |
| eAxisBottomU | 6 | BottomU | UVW 平台 U 轴 |
| eAxisBottomV | 7 | BottomV | UVW 平台 V 轴 |
| eAxisBottomW | 8 | BottomW | UVW 平台 W 轴 |
| eAxisDisX | 9 | DisX | 点胶 X 轴 |
| eAxisDisY | 10 | DisY | 点胶 Y 轴 |
| eAxisDisZ | 11 | DisZ | 点胶 Z 轴 |
| eAxisCameraX | 12 | CameraX | 相机 X 轴 |
| eAxisCameraZ | 13 | CameraZ | 相机 Z 轴 |
| eAxisBottomY | 14 | BottomY | 底板 Y 轴 |
| eAxisBottomGetX | 15 | BottomGetX | 底板取料 X 轴 |

---

## 八、风险点与对策

| 风险 | 等级 | 对策 |
|---|---|---|
| APS SDK P/Invoke 复杂，参数不正确 | 高 | 先写 C++/CLI 桥接层测试，再迁移到 P/Invoke |
| Halcon .NET 版本差异（原 Halcon 10） | 高 | 先验证 Halcon .NET 版本兼容性，必要时升级 License |
| ThreadAdjust::Run() 3041 行业务逻辑复杂 | 高 | 拆分为 15+ 私有方法，逐方法迁移 + 单元测试 |
| INI 配置项散落在多文件中 | 中 | 先做 INI 全量收集整理，再统一转为 XML |
| 气缸 10s 超时逻辑在业务代码中耦合 | 中 | 在 BlockCutStationBase 中统一封装超时处理 |
| 扫码枪 TCP 多端口监听 | 中 | 使用独立的 BarcodeScannerClient + Task |
| Basler Pylon SDK .NET 版本适配 | 低 | Pylon 5.1 有官方 .NET wrapper，直接引用 |
| Excel 日志格式兼容 | 低 | 保持与 CLog::Instance() 相同列结构 |

---

## 九、前置验证清单

在正式开始 Phase 1 编码前，建议先完成以下验证：

1. [ ] Halcon .NET SDK 安装 + License 验证（原项目使用 Halcon 10，确认 .NET 版可用）
2. [ ] APS SDK (APS168x64.dll) 存在性 + C# 加载测试
3. [ ] Basler Pylon .NET SDK 安装 + 相机取图测试
4. [ ] 现有设备 API 手册准备就绪（轴参数、IO 地址映射）
5. [ ] MES 接口文档 / URL / Key 确认
6. [ ] 生产环境 D:/list.ini 内容备份

---

## 十、下一步

1. **用户确认方案** — 确认迁移范围、优先级、时间线
2. **前置验证** — 完成上述 6 项验证
3. **Phase 1 启动** — 从 `Motion_APS.cs` + `BlockCutConstants.cs` 开始编码
4. **增量交付** — 每完成一个工站，提供测试版本
