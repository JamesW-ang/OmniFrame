# OmniFrame 运动控制开发指南

本文档面向 OmniFrame 项目的开发人员，详细说明运动控制系统的架构设计、硬件支持、API 使用、工站集成、故障排查及安全规范。

---

## 目录

1. [架构总览](#1-架构总览)
2. [支持的硬件](#2-支持的硬件)
3. [快速入门：控制一个轴](#3-快速入门控制一个轴)
4. [轴配置](#4-轴配置)
5. [回原点](#5-回原点)
6. [IO 控制](#6-io-控制)
7. [如何添加新的运动控制卡驱动](#7-如何添加新的运动控制卡驱动)
8. [模拟模式](#8-模拟模式)
9. [工站集成](#9-工站集成)
10. [故障排查](#10-故障排查)
11. [安全规范](#11-安全规范)

---

## 1. 架构总览

### 1.1 分层结构

运动控制系统采用经典的分层架构，从上到下共四层：

```
┌──────────────────────────────────────────────────┐
│                   UI 层 (WinForms)                 │
│   MainForm / StationForm / EquipmentControlForm   │
│         通过依赖注入获取 IMotionManager             │
└──────────────────────┬───────────────────────────┘
                       │ 调用接口
┌──────────────────────▼───────────────────────────┐
│            OmniFrame.Core (业务逻辑层)              │
│   MotionManager : IMotionManager                  │
│   IoManager : IIoManager                         │
│   StationBase (SetSignal / WaitSignal)            │
│         读取 XML 配置，委托给 MotionIO 层            │
└──────────────────────┬───────────────────────────┘
                       │ 调用接口
┌──────────────────────▼───────────────────────────┐
│            MotionIO (硬件抽象层)                    │
│   MotionIOManager : IMotionIOManager              │
│       ┌─ Motion (抽象基类)                         │
│       ├─ Motion_GTS        (固高)                  │
│       ├─ Motion_Dmc3000    (雷赛 DMC3000)          │
│       ├─ Motion_DMC3400    (雷赛 DMC3400)          │
│       ├─ Motion_InoEcat    (汇川 EtherCAT)         │
│       └─ Motion_PCIeM60    (PCIeM60)              │
│   IoCtrl (抽象基类) / IoCtrl_PCIeM60               │
└──────────────────────┬───────────────────────────┘
                       │ P/Invoke 调用
┌──────────────────────▼───────────────────────────┐
│               原生 DLL 层                          │
│   gts.dll / LTDMC.dll / inoecat.dll / ...        │
│   (厂商提供的 C/C++ 驱动程序)                       │
└──────────────────────────────────────────────────┘
```

### 1.2 核心设计模式：工厂方法

`MotionIOManager` 使用工厂方法模式创建运动控制卡实例。XML 配置文件中声明的品牌字符串被转换为具体的 `Motion` 子类对象：

```
XML 配置文件
    │
    ▼
ReadCfgFromXml(XmlDocument doc)
    │
    ├─ 读取 <MotionCard Type="GTS" .../>
    ├─ 读取 <MotionCard Type="DMC3000" .../>
    ├─ 读取 <MotionCard Type="DMC3400" .../>
    └─ 读取 <MotionCard Type="INOVANCE" .../>
    │
    ▼
CreateMotion(string type, int index, string name, int minAxis, int maxAxis)
    │
    ├─ case "GTS":        → new Motion_GTS(...)
    ├─ case "DMC3000":    → new Motion_Dmc3000(...)
    ├─ case "DMC3400":    → new Motion_DMC3400(...)
    └─ case "INOVANCE":   → new Motion_InoEcat(...)
    │
    ▼
InitAll() → 遍历所有 Motion，调用 motion.Init()
```

这种设计的优势：
- 上层代码不感知具体硬件品牌
- 新增硬件只需添加一个子类和一行 case 分支
- XML 配置即可切换硬件，无需重新编译业务层

### 1.3 关键接口与类

| 类型 | 所在项目 | 职责 |
|------|----------|------|
| `Motion` (abstract) | MotionIO | 运动控制抽象基类，定义所有抽象和虚方法 |
| `MotionDevice` | MotionIO | 封装 `Motion`，实现 `IDevice`，统一异常处理 |
| `IMotionIOManager` / `MotionIOManager` | MotionIO | 管理多个运动卡的生命周期，工厂方法创建 |
| `IMotionManager` / `MotionManager` | OmniFrame.Core | 对上层暴露简化的运动控制 API，依赖注入 |
| `IIoManager` / `IoManager` | OmniFrame.Core | IO 点管理，工厂方法创建 `IoCtrl` |
| `IoCtrl` (abstract) | MotionIO | IO 控制抽象基类 |
| `IDevice` | Common | 统一设备接口（`Initialize`, `Connect`, `Disconnect`, `Reset`） |

### 1.4 依赖注入注册

在 `Program.cs` 的 `ConfigureServices` 方法中：

```csharp
// MotionIO 层
services.AddSingleton<IMotionIOManager, MotionIO.MotionIOManager>();

// OmniFrame.Core 层
services.AddSingleton<IMotionManager, OmniFrame.Core.MotionManager>();
services.AddSingleton<IIoManager, IoManager>();
```

上层通过构造函数注入获取：

```csharp
public class SomeForm : Form
{
    private readonly IMotionManager _motion;

    public SomeForm(IMotionManager motionManager)
    {
        _motion = motionManager;
    }
}
```

---

## 2. 支持的硬件

### 2.1 硬件品牌对比

| 品牌 | XML Type 字符串 | 实现类 | 原生 DLL | 通信方式 | 关键特性 |
|------|----------------|--------|----------|----------|----------|
| **固高 (GoogolTech)** | `GTS` | `Motion_GTS` | `gts.dll` | PCI/PCIe 总线 | 8 轴/卡，支持直线/圆弧插补，JOG |
| **雷赛 (Leadshine) DMC3000** | `DMC3000` | `Motion_Dmc3000` | `LTDMC.dll` | PCI/PCIe 总线 | 多轴控制，直线插补 |
| **雷赛 (Leadshine) DMC3400** | `DMC3400` | `Motion_DMC3400` | `LTDMC.dll` | PCI/PCIe 总线 | DMC3000 升级版，更高性能 |
| **汇川 (Inovance) EtherCAT** | `INOVANCE` | `Motion_InoEcat` | `inoecat.dll` | EtherCAT 总线 | 总线型，支持远程 IO，多轴同步 |
| **PCIeM60** | `PCIEM60` | `Motion_PCIeM60` | 厂商提供 | PCIe 总线 | 运动 + IO 一体卡 |

### 2.2 原生 DLL 部署要求

所有厂商提供的原生 DLL 必须放置在应用程序的运行目录下（即 `bin/Debug/` 或 `bin/Release/`），与可执行文件同级。如果使用发布部署，确保 DLL 文件被复制到输出目录：

```xml
<!-- 在 .csproj 中添加 -->
<ItemGroup>
  <Content Include="..\..\lib\*.dll">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

DLL 缺失时的典型异常：`System.DllNotFoundException: Unable to load DLL 'xxx'`。

---

## 3. 快速入门：控制一个轴

### 3.1 完整工作流程（同步方式，直接使用 MotionIO 层）

以下示例展示控制一个轴从初始化到关闭的完整生命周期。此方式绕过 `MotionManager`，直接操作 `Motion` 对象，适用于底层调试或单元测试。

```csharp
using System;
using MotionIO;
using OmniFrame.Common;

public class DirectAxisControlExample
{
    public static void Run()
    {
        // 1. 创建运动控制卡实例 (固高 GTS，卡索引 0，轴范围 0-7)
        Motion motion = new Motion_GTS(
            cardIndex: 0,
            name: "MainMotion",
            minAxisNo: 0,
            maxAxisNo: 7
        );

        try
        {
            // 2. 初始化运动控制卡
            if (!motion.Init())
            {
                Logger.Error("运动控制卡初始化失败");
                return;
            }
            Logger.Info("运动控制卡初始化成功");

            // 3. 伺服使能（轴 0）
            if (!motion.ServoOn(axisNo: 0))
            {
                Logger.Error("轴 0 伺服使能失败");
                return;
            }
            Logger.Info("轴 0 伺服已使能");

            // 4. 设置软限位（轴 0：正限位 200.0，负限位 -200.0）
            motion.SetSoftLimit(axisNo: 0, positive: 200.0, negative: -200.0);
            motion.EnableSoftLimit(axisNo: 0, enable: true);

            // 5. 回原点
            if (!motion.Home(axisNo: 0, HomeMode.ORG_P))
            {
                Logger.Error("轴 0 回原点失败");
                return;
            }
            Logger.Info("轴 0 回原点完成");

            // 6. 绝对运动：移动到位置 100.0 mm，速度 50.0 mm/s
            if (!motion.AbsMove(axisNo: 0, pos: 100.0, speed: 50.0))
            {
                Logger.Error("轴 0 绝对运动失败");
                return;
            }

            // 7. 等待运动完成
            while (motion.IsAxisMoving(axisNo: 0))
            {
                System.Threading.Thread.Sleep(10);
            }

            // 8. 读取当前位置
            double currentPos = motion.GetAxisPos(axisNo: 0);
            Logger.Info($"轴 0 当前位置: {currentPos} mm");

            // 9. 相对运动：正向移动 25.0 mm
            motion.RelativeMove(axisNo: 0, pos: 25.0, speed: 30.0);
            while (motion.IsAxisMoving(axisNo: 0))
            {
                System.Threading.Thread.Sleep(10);
            }
            currentPos = motion.GetAxisPos(axisNo: 0);
            Logger.Info($"轴 0 相对运动后位置: {currentPos} mm");

            // 10. 停止轴（紧急情况）
            motion.StopAxis(axisNo: 0);

            // 11. 伺服失能
            motion.ServoOff(axisNo: 0);
            Logger.Info("轴 0 伺服已失能");
        }
        catch (DllNotFoundException ex)
        {
            Logger.Error("运动控制卡 DLL 未找到，请检查驱动是否正确安装", ex);
        }
        catch (Exception ex)
        {
            Logger.Error("运动控制异常", ex);
        }
        finally
        {
            // 12. 关闭运动控制卡
            motion.DeInit();
            Logger.Info("运动控制卡已关闭");
        }
    }
}
```

### 3.2 使用 MotionManager（推荐方式，通过依赖注入）

以下示例展示通过 DI 获取 `IMotionManager` 并使用其简化 API：

```csharp
using System;
using MotionIO;
using OmniFrame.Core;
using OmniFrame.Common;

public class AxisControlViaManager
{
    private readonly IMotionManager _motionManager;

    // 通过构造函数注入
    public AxisControlViaManager(IMotionManager motionManager)
    {
        _motionManager = motionManager;
    }

    public void Run(string configPath)
    {
        // 1. 从 XML 配置文件初始化所有运动控制卡
        if (!_motionManager.Initialize(configPath))
        {
            Logger.Error("MotionManager 初始化失败，请检查配置文件");
            return;
        }
        Logger.Info("MotionManager 初始化成功");

        try
        {
            // 2. 回原点（卡 0，轴 1，模式 ORG_P）
            if (!_motionManager.Home(cardIndex: 0, axis: 1, HomeMode.ORG_P))
            {
                Logger.Error("轴 1 回原点失败");
                return;
            }

            // 3. 移动到位置 150.0，速度 80.0
            if (!_motionManager.MoveTo(cardIndex: 0, axis: 1, position: 150.0, speed: 80.0))
            {
                Logger.Error("轴 1 运动失败");
                return;
            }

            // 4. 读取当前位置
            double pos = _motionManager.GetPosition(cardIndex: 0, axis: 1);
            Logger.Info($"轴 1 当前位置: {pos} mm");

            // 5. 停止
            _motionManager.Stop(cardIndex: 0, axis: 1);
        }
        catch (Exception ex)
        {
            Logger.Error("运动控制操作异常", ex);
        }
        finally
        {
            _motionManager.Dispose();
        }
    }
}
```

### 3.3 使用 MotionDevice 包装类（推荐用于设备管理器）

`MotionDevice` 封装了 `Motion`，提供统一的异常处理和事件通知：

```csharp
using MotionIO;
using OmniFrame.Common;

public class MotionDeviceExample
{
    public static void Run()
    {
        var motion = new Motion_GTS(0, "GTS_Card0", 0, 7);
        var device = new MotionDevice("主运动卡", motion);

        // 订阅错误事件
        device.ErrorOccurred += (sender, args) =>
        {
            Logger.Error($"[{args.Time:HH:mm:ss}] 运动卡错误: [{args.ErrorCode}] {args.ErrorMessage}, 轴={args.AxisNo}");
        };

        // 初始化
        if (!device.Initialize())
        {
            Logger.Error("设备初始化失败");
            return;
        }

        // 绝对运动
        device.MoveAbsolute(axisNo: 0, position: 50.0, speed: 30.0);

        // 检查运动状态
        bool moving = device.IsAxisMoving(0);
        Logger.Info($"轴 0 运动状态: {(moving ? "运动中" : "空闲")}");

        // 使能/失能
        device.SetAxisEnabled(0, enabled: false);

        // 释放
        device.Dispose();
    }
}
```

---

## 4. 轴配置

### 4.1 XML 配置文件格式

运动控制卡通过 XML 文件配置。配置文件结构如下：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Configuration>
  <MotionCards>
    <!-- 卡 1：固高 GTS，8 轴 -->
    <MotionCard
      Index="0"
      Name="MainMotion"
      Type="GTS"
      MinAxis="0"
      MaxAxis="7"
      Enable="true" />

    <!-- 卡 2：雷赛 DMC3000，4 轴 -->
    <MotionCard
      Index="1"
      Name="AuxMotion"
      Type="DMC3000"
      MinAxis="0"
      MaxAxis="3"
      Enable="true" />

    <!-- 卡 3：汇川 EtherCAT，6 轴 -->
    <MotionCard
      Index="2"
      Name="EtherCATMotion"
      Type="INOVANCE"
      MinAxis="0"
      MaxAxis="5"
      Enable="false" />
  </MotionCards>
</Configuration>
```

**属性说明：**

| 属性 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `Index` | int | 是 | 卡索引，全局唯一，用于 MotionManager 中的 cardIndex 参数 |
| `Name` | string | 是 | 逻辑名称，用于日志和显示 |
| `Type` | string | 是 | 品牌类型。有效值：`GTS`、`DMC3000`、`DMC3400`、`INOVANCE` |
| `MinAxis` | int | 是 | 该卡的最小轴号 |
| `MaxAxis` | int | 是 | 该卡的最大轴号。轴数 = MaxAxis - MinAxis + 1 |
| `Enable` | bool | 否 | 是否启用该卡。为 `false` 时 `InitAll()` 会跳过此卡 |

### 4.2 AxisParam 参数模型

```csharp
[Serializable]
public class AxisParam
{
    public int AxisNo { get; set; }          // 轴号
    public double Speed { get; set; }         // 运动速度 (mm/s)
    public double Acceleration { get; set; }  // 加速度 (mm/s²)
    public double PositiveLimit { get; set; } // 正软限位 (mm)
    public double NegativeLimit { get; set; } // 负软限位 (mm)
}
```

```csharp
[Serializable]
public class MotionConfig
{
    public string Brand { get; set; }               // 品牌：GTS/DMC3000/DMC3400/INOVANCE
    public string IP { get; set; }                  // IP 地址（总线型使用）
    public int AxisCount { get; set; }              // 轴数量
    public List<AxisParam> AxisParams { get; set; } // 各轴参数列表
}
```

### 4.3 速度和加速度调优建议

| 参数 | 建议范围 | 说明 |
|------|----------|------|
| Speed | 5 - 500 mm/s | 低速用于精密定位 (5-30 mm/s)；高速用于长行程空跑 (100-500 mm/s) |
| Acceleration | 50 - 2000 mm/s² | 低加速度可减少机械震动，延长丝杆寿命；高加速度可缩短节拍 |
| PositiveLimit | 根据机械行程 | 应设为硬限位开关内侧 5-10 mm，防止过冲撞到硬限位 |
| NegativeLimit | 根据机械行程 | 同上 |

**调优原则：**
1. 先低速 (10-20 mm/s) 验证行程范围，确认无碰撞风险
2. 从低加速度 (100 mm/s²) 开始，逐步提高
3. 观察电机是否有异响或震动，如有则降低加速度
4. 丝杆传动通常使用梯形速度曲线 (T-curve)，平稳性优先于速度

---

## 5. 回原点

### 5.1 HomeMode 枚举详解

```csharp
public enum HomeMode
{
    ORG_P,       // 原点信号 + 正方向
    ORG_N,       // 原点信号 + 负方向
    PEL,         // 正限位为原点
    MEL,         // 负限位为原点
    EZ_PEL,      // EZ 信号 + 正限位
    EZ_MEL,      // EZ 信号 + 负限位
    ORG_P_EZ,    // 原点 + 正方向 + EZ
    ORG_N_EZ,    // 原点 + 负方向 + EZ
    GanTry_MEL,  // 龙门负限位回原点
    GanTry_PEL,  // 龙门正限位回原点
    BUS_BASE     // 总线型基础值
}
```

### 5.2 各模式详解

#### ORG_P — 正方向寻原点（最常用）

```
轴当前位置
    │
    ▼  向正方向移动
    ├──────────────────────────● ORG 传感器
                               │
                               ▼ 检测到 ORG 信号上升沿 → 停止 → 位置清零
```

**使用场景：** 轴位于原点传感器负侧时，向正方向移动寻找原点。适用于绝大多数单轴系统。

#### ORG_N — 负方向寻原点

```
                               ● ORG 传感器
                               │
   向负方向移动                  ▼ 检测到 ORG 信号上升沿 → 停止 → 位置清零
    ◄──────────────────────────┤
                               │
                         轴当前位置
```

**使用场景：** 轴位于原点传感器正侧时，向负方向移动寻找原点。

#### PEL / MEL — 限位为原点

以正限位（PEL）或负限位（MEL）作为原点位置。适用于没有独立原点传感器、仅有限位开关的简化系统。

```
PEL 模式：
轴 → 向正方向移动 → 碰到正限位开关 → 停止 → 位置清零

MEL 模式：
轴 ← 向负方向移动 → 碰到负限位开关 → 停止 → 位置清零
```

#### EZ_PEL / EZ_MEL — EZ 信号 + 限位

先移动到限位，再反向寻找最近的 EZ（编码器 Z 相）信号。EZ 信号每转一圈产生一次，精度远高于限位开关。

```
EZ_PEL 模式：
轴 → 向正方向移动 → 碰到正限位 → 反向移动 ← 寻找最近 EZ 信号 → 位置清零
```

**使用场景：** 需要高重复精度回原点的场合（如半导体设备、精密装配）。

#### ORG_P_EZ / ORG_N_EZ — 原点 + EZ 双重确认

先找到原点传感器，再寻找最近的 EZ 信号：

```
ORG_P_EZ 模式：
轴 → 向正方向移动 → 检测到 ORG 信号 → 继续移动 → 检测到第一个 EZ 信号 → 位置清零
```

**使用场景：** 最高精度的原点回归方式。原点传感器提供粗定位，EZ 信号提供精定位。

#### GanTry_MEL / GanTry_PEL — 龙门回原点

用于龙门双驱结构的回原点。两个平行轴的同步回原点，先同时移动到限位，再反向寻找 EZ 信号。确保龙门结构两侧轴回到对齐位置。

#### BUS_BASE — 总线型基础值

用于 EtherCAT 等总线型伺服。总线伺服驱动器自带绝对值编码器或电池备份的多圈编码器，原点位置由驱动器内部保存，通过总线读取当前位置即可。

### 5.3 回原点代码示例

```csharp
// 标准回原点（最常用）
motion.Home(axisNo: 0, HomeMode.ORG_P);

// 高精度回原点（ORG + EZ）
motion.Home(axisNo: 1, HomeMode.ORG_P_EZ);

// 总线型伺服：先读取当前位置，无需回原点
// 总线伺服的位置由驱动器内部保持
if (motion is Motion_InoEcat)
{
    double pos = motion.GetAxisPos(axisNo: 0);
    Logger.Info($"总线伺服轴 0 当前位置: {pos}");
}

// 龙门双驱回原点
motion.Home(axisNo: 2, HomeMode.GanTry_MEL);

// 检查回原点状态
if (motion.IsAxisHomed(axisNo: 0))
{
    Logger.Info("轴 0 已回原点，可以开始运动");
}
else
{
    Logger.Warning("轴 0 尚未回原点，请先执行 Homing");
}
```

### 5.4 回原点注意事项

1. **回原点前必须检查伺服使能状态**：伺服未使能时调用 `Home()` 会失败。
2. **确保行程范围内无障碍物**：回原点过程中轴会移动，确保机械结构可自由运动。
3. **限位开关功能正常**：回原点依赖限位或原点传感器信号。传感器故障可能导致撞机。
4. **不要在运动中回原点**：先调用 `StopAxis()` 停止当前运动。
5. **回原点后位置自动清零**：`Home()` 成功后，`GetAxisPos()` 返回 0。

---

## 6. IO 控制

### 6.1 IoCtrl 抽象基类

```csharp
public abstract class IoCtrl
{
    public abstract bool Init(object param);
    public abstract bool Close();

    // 位操作
    public abstract bool ReadInput(int port, out bool value);
    public abstract bool WriteOutput(int port, bool value);

    // 端口操作（32 位为一组）
    public abstract bool ReadInputPort(int port, out int value);
    public abstract bool WriteOutputPort(int port, int value);

    // 批量操作
    public abstract Dictionary<int, bool> ReadAllInputs();
    public abstract Dictionary<int, bool> ReadAllOutputs();

    public abstract bool Reset();
    public abstract string GetError();
}
```

### 6.2 IoManager 使用

`IoManager` 封装了 `IoCtrl`，通过 IIoManager 接口提供给上层。当前支持的硬件类型：`PCIEM60`。

#### 初始化

```csharp
var ioManager = new IoManager();

// 初始化指定类型的 IO 控制器
string ioType = "PCIEM60";         // 当前支持的类型
string configPath = @"C:\Config\IoConfig.xml";

if (!ioManager.Initialize(ioType, configPath))
{
    Logger.Error("IO 管理器初始化失败");
    return;
}
```

#### 读写 IO 点

```csharp
// 设置输出点（端口 0 的第 3 位）
bool success = ioManager.SetOutput(port: 0, pin: 3, value: true);
if (success)
{
    Logger.Info("输出点 0.3 已置为高电平");
}

// 读取输入点（端口 1 的第 5 位）
bool inputValue = ioManager.GetInput(port: 1, pin: 5);
Logger.Info($"输入点 1.5 当前状态: {(inputValue ? "高电平" : "低电平")}");
```

### 6.3 信号映射

硬件 IO 点与工站信号的映射关系通过信号名称常量来表达。例如：

| 信号名称 | IO 端口 | 说明 |
|----------|---------|------|
| `Robot1_Pick` | 输出 0.0 | 机器人 1 抓取指令 |
| `Robot1_PickDone` | 输入 1.0 | 机器人 1 抓取完成反馈 |
| `Cylinder1_Extend` | 输出 0.1 | 气缸 1 伸出 |
| `Cylinder1_Extended` | 输入 1.1 | 气缸 1 伸出到位 |
| `Camera1_Trigger` | 输出 0.2 | 相机 1 触发拍照 |
| `Camera1_Done` | 输入 1.2 | 相机 1 拍照完成 |

这些映射关系在实际部署时需要根据电气图纸和接线定义，在配置文件中完成绑定。

### 6.4 模拟 IO 测试

在无硬件环境下，使用 `SimulatedIoCtrl` 进行测试：

```csharp
var simIo = new SimulatedIoCtrl("TestIO", inputCount: 64, outputCount: 64);
simIo.Init(null);

// 模拟输入信号（模拟传感器触发）
simIo.SetInput(index: 5, value: true);
simIo.SetInput(port: 1, pin: 5, value: true);  // 等价于 index = 1*32+5 = 37

// 写入输出
simIo.WriteOutput(port: 0, value: true);

// 验证输出
bool outputState = simIo.GetOutput(index: 0);
Logger.Info($"输出 0 状态: {outputState}");

simIo.Close();
```

---

## 7. 如何添加新的运动控制卡驱动

本章以添加一个假设的「新品牌 XYZ-8000」运动控制卡为例，逐步说明完整流程。

### 7.1 步骤 1：创建继承 Motion 的新类

在 `src/MotionIO/` 下新建文件 `Motion_XYZ8000.cs`：

```csharp
using System.Runtime.InteropServices;
using System;
using OmniFrame.Common;

namespace MotionIO
{
    public class Motion_XYZ8000 : Motion
    {
        // 内部状态变量
        private bool _initialized = false;
        private double[] _currentPos;
        private bool[] _isMoving;
        private bool[] _isHomed;
        private bool[] _servoOn;
        private int _axisCount;

        // === P/Invoke 声明：原生 DLL 函数 ===
        // 注意：以下函数签名仅为示例，实际需参考厂商 SDK 文档
        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_OpenCard(int cardIndex);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_CloseCard(int cardIndex);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_ServoOn(int cardIndex, int axis);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_ServoOff(int cardIndex, int axis);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_AbsMove(int cardIndex, int axis, double pos, double speed);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_RelMove(int cardIndex, int axis, double dist, double speed);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_StopAxis(int cardIndex, int axis);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_StopAll(int cardIndex);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_Home(int cardIndex, int axis, int mode);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern double XYZ_GetPosition(int cardIndex, int axis);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_IsMoving(int cardIndex, int axis);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_ClearAlarm(int cardIndex, int axis);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_SetSoftLimit(int cardIndex, int axis, double positive, double negative);

        [DllImport("xyz8000_api.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int XYZ_EnableSoftLimit(int cardIndex, int axis, int enable);

        // === 构造函数 ===
        public Motion_XYZ8000(int cardIndex, string name, int minAxisNo, int maxAxisNo)
            : base(cardIndex, name, minAxisNo, maxAxisNo)
        {
            _axisCount = maxAxisNo - minAxisNo + 1;
            _currentPos = new double[_axisCount];
            _isMoving = new bool[_axisCount];
            _isHomed = new bool[_axisCount];
            _servoOn = new bool[_axisCount];
        }

        // === 实现所有抽象方法 ===

        public override bool Init()
        {
            try
            {
                if (_initialized)
                {
                    LogWarning("运动卡已初始化，跳过重复初始化");
                    return true;
                }

                LogInfo("正在初始化 XYZ-8000 运动控制卡...");

                int result = XYZ_OpenCard(CardIndex);
                if (result != 0)
                {
                    LogError($"XYZ_OpenCard 返回错误码: {result}");
                    return false;
                }

                _initialized = true;
                LogInfo("XYZ-8000 运动控制卡初始化成功");
                return true;
            }
            catch (DllNotFoundException ex)
            {
                LogError("XYZ-8000 驱动 DLL 未找到 (xyz8000_api.dll)，请检查 DLL 是否在运行目录下", ex);
                return false;
            }
            catch (EntryPointNotFoundException ex)
            {
                LogError("XYZ-8000 DLL 版本不匹配，找不到必需的函数入口", ex);
                return false;
            }
            catch (SEHException ex)
            {
                LogError("XYZ-8000 驱动产生结构化异常 (SEH)，可能是卡硬件故障或驱动崩溃", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError("XYZ-8000 初始化异常", ex);
                return false;
            }
        }

        public override bool DeInit()
        {
            try
            {
                if (!_initialized)
                    return true;

                LogInfo("正在关闭 XYZ-8000 运动控制卡...");
                XYZ_CloseCard(CardIndex);
                _initialized = false;
                LogInfo("XYZ-8000 运动控制卡已关闭");
                return true;
            }
            catch (DllNotFoundException ex)
            {
                LogError("关闭时 DLL 未找到", ex);
                return false;
            }
            catch (SEHException ex)
            {
                LogError("关闭时驱动异常", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError("关闭 XYZ-8000 失败", ex);
                return false;
            }
        }

        public override bool AbsMove(int axisNo, double pos, double speed)
        {
            if (!CheckAxisReady(axisNo))
                return false;

            try
            {
                int result = XYZ_AbsMove(CardIndex, axisNo, pos, speed);
                if (result != 0)
                {
                    LogError($"XYZ_AbsMove 返回错误码: {result}, 轴={axisNo}, 位置={pos}, 速度={speed}");
                    return false;
                }

                int localAxis = axisNo - MinAxisNo;
                _isMoving[localAxis] = true;
                _currentPos[localAxis] = pos;

                LogInfo($"轴 {axisNo} 绝对运动: 目标={pos}, 速度={speed}");
                return true;
            }
            catch (DllNotFoundException ex)
            {
                LogError("DLL 未找到", ex);
                return false;
            }
            catch (SEHException ex)
            {
                LogError("驱动异常", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 绝对运动失败", ex);
                return false;
            }
        }

        public override bool RelativeMove(int axisNo, double pos, double speed)
        {
            if (!CheckAxisReady(axisNo))
                return false;

            try
            {
                int result = XYZ_RelMove(CardIndex, axisNo, pos, speed);
                if (result != 0)
                {
                    LogError($"XYZ_RelMove 返回错误码: {result}, 轴={axisNo}");
                    return false;
                }

                int localAxis = axisNo - MinAxisNo;
                _isMoving[localAxis] = true;
                _currentPos[localAxis] += pos;

                LogInfo($"轴 {axisNo} 相对运动: 距离={pos}, 速度={speed}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 相对运动失败", ex);
                return false;
            }
        }

        public override bool Home(int axisNo, HomeMode mode)
        {
            if (!CheckAxisReady(axisNo))
                return false;

            try
            {
                int modeCode = ConvertHomeMode(mode);
                int result = XYZ_Home(CardIndex, axisNo, modeCode);
                if (result != 0)
                {
                    LogError($"XYZ_Home 返回错误码: {result}, 轴={axisNo}, 模式={mode}");
                    return false;
                }

                int localAxis = axisNo - MinAxisNo;
                _isHomed[localAxis] = true;
                _currentPos[localAxis] = 0;

                LogInfo($"轴 {axisNo} 回原点完成, 模式={mode}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 回原点失败", ex);
                return false;
            }
        }

        public override bool StopAxis(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            try
            {
                XYZ_StopAxis(CardIndex, axisNo);
                int localAxis = axisNo - MinAxisNo;
                _isMoving[localAxis] = false;
                LogInfo($"轴 {axisNo} 已停止");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 停止失败", ex);
                return false;
            }
        }

        public override bool StopAllAxis()
        {
            try
            {
                XYZ_StopAll(CardIndex);
                for (int i = 0; i < _axisCount; i++)
                    _isMoving[i] = false;
                LogInfo("所有轴已停止");
                return true;
            }
            catch (Exception ex)
            {
                LogError("停止所有轴失败", ex);
                return false;
            }
        }

        public override double GetAxisPos(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return 0;

            try
            {
                return XYZ_GetPosition(CardIndex, axisNo);
            }
            catch (Exception ex)
            {
                LogError($"读取轴 {axisNo} 位置失败", ex);
                return 0;
            }
        }

        public override bool ServoOn(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            try
            {
                int result = XYZ_ServoOn(CardIndex, axisNo);
                if (result != 0)
                {
                    LogError($"XYZ_ServoOn 返回错误码: {result}, 轴={axisNo}");
                    return false;
                }

                int localAxis = axisNo - MinAxisNo;
                _servoOn[localAxis] = true;
                LogInfo($"轴 {axisNo} 伺服使能成功");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 伺服使能失败", ex);
                return false;
            }
        }

        public override bool ServoOff(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            try
            {
                XYZ_ServoOff(CardIndex, axisNo);
                int localAxis = axisNo - MinAxisNo;
                _servoOn[localAxis] = false;
                LogInfo($"轴 {axisNo} 伺服失能");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 伺服失能失败", ex);
                return false;
            }
        }

        public override bool IsAxisMoving(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            try
            {
                return XYZ_IsMoving(CardIndex, axisNo) != 0;
            }
            catch
            {
                return false;
            }
        }

        public override bool IsAxisHomed(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            int localAxis = axisNo - MinAxisNo;
            return _isHomed[localAxis];
        }

        public override AxisState GetAxisState(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return AxisState.Disabled;

            int localAxis = axisNo - MinAxisNo;

            if (!_servoOn[localAxis])
                return AxisState.Disabled;

            if (_isMoving[localAxis])
                return AxisState.Moving;

            return AxisState.Idle;
        }

        public override bool ClearAlarm(int axisNo)
        {
            if (!IsAxisValid(axisNo))
                return false;

            try
            {
                int result = XYZ_ClearAlarm(CardIndex, axisNo);
                LogInfo($"轴 {axisNo} 清除报警: {(result == 0 ? "成功" : "失败")}");
                return result == 0;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 清除报警失败", ex);
                return false;
            }
        }

        public override bool SetSoftLimit(int axisNo, double positive, double negative)
        {
            if (!IsAxisValid(axisNo))
                return false;

            try
            {
                int result = XYZ_SetSoftLimit(CardIndex, axisNo, positive, negative);
                LogInfo($"轴 {axisNo} 软限位: [{negative}, {positive}]");
                return result == 0;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 设置软限位失败", ex);
                return false;
            }
        }

        public override bool EnableSoftLimit(int axisNo, bool enable)
        {
            if (!IsAxisValid(axisNo))
                return false;

            try
            {
                int result = XYZ_EnableSoftLimit(CardIndex, axisNo, enable ? 1 : 0);
                LogInfo($"轴 {axisNo} 软限位{(enable ? "启用" : "禁用")}");
                return result == 0;
            }
            catch (Exception ex)
            {
                LogError($"轴 {axisNo} 切换软限位失败", ex);
                return false;
            }
        }

        // === 辅助方法 ===

        private bool CheckAxisReady(int axisNo)
        {
            if (!_initialized)
            {
                LogError("运动卡未初始化");
                return false;
            }

            if (!IsAxisValid(axisNo))
            {
                LogError($"轴号 {axisNo} 无效，有效范围: [{MinAxisNo}, {MaxAxisNo}]");
                return false;
            }

            int localAxis = axisNo - MinAxisNo;
            if (!_servoOn[localAxis])
            {
                LogError($"轴 {axisNo} 伺服未使能，请先调用 ServoOn");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 将 OmniFrame HomeMode 转换为 XYZ 驱动定义的 HomeMode 值
        /// 注意：具体映射关系参考 XYZ SDK 手册
        /// </summary>
        private int ConvertHomeMode(HomeMode mode)
        {
            switch (mode)
            {
                case HomeMode.ORG_P: return 1;
                case HomeMode.ORG_N: return 2;
                case HomeMode.PEL: return 3;
                case HomeMode.MEL: return 4;
                case HomeMode.EZ_PEL: return 5;
                case HomeMode.EZ_MEL: return 6;
                case HomeMode.ORG_P_EZ: return 7;
                case HomeMode.ORG_N_EZ: return 8;
                default:
                    LogWarning($"不支持的回原点模式: {mode}，使用默认 ORG_P");
                    return 1;
            }
        }
    }
}
```

### 7.2 步骤 2：在工厂方法中添加品牌分支

在 `MotionIOManager.cs` 的 `CreateMotion` 方法中添加新的 case：

```csharp
private Motion CreateMotion(string type, int index, string name, int minAxis, int maxAxis)
{
    switch (type.ToUpper())
    {
        case "GTS":
            return new Motion_GTS(index, name, minAxis, maxAxis);
        case "DMC3000":
            return new Motion_Dmc3000(index, name, minAxis, maxAxis);
        case "DMC3400":
            return new Motion_DMC3400(index, name, minAxis, maxAxis);
        case "INOVANCE":
            return new Motion_InoEcat(index, name, minAxis, maxAxis);
        // === 新增品牌 ===
        case "XYZ8000":
            return new Motion_XYZ8000(index, name, minAxis, maxAxis);
        default:
            Logger.Error($"未知的运动卡类型: {type}");
            return null;
    }
}
```

### 7.3 步骤 3：添加特定异常处理

原生 DLL 调用可能抛出多种异常，每个方法都应处理以下异常类型：

| 异常类型 | 触发场景 | 处理方式 |
|----------|----------|----------|
| `DllNotFoundException` | DLL 文件不在运行目录或系统 PATH 中 | 提示用户安装驱动，返回 false |
| `EntryPointNotFoundException` | DLL 版本不匹配，缺少某个导出函数 | 提示 DLL 版本问题，返回 false |
| `SEHException` | 原生 C/C++ 代码崩溃（访问违规、除零等） | 记录错误，重置状态，返回 false |
| `InvalidOperationException` | 设备未初始化就执行操作 | 记录错误，返回 false |
| `Exception` | 其他未知异常 | 兜底捕获，记录并返回 false |

参考 `MotionDevice.cs` 中的异常处理模式：

```csharp
catch (InvalidOperationException ex)
{
    OnError("MOVE_ABSOLUTE_FAILED", "设备操作无效状态", axisNo, ex);
    return false;
}
catch (DllNotFoundException ex)
{
    OnError("MOVE_ABSOLUTE_FAILED", "运动控制卡DLL未找到", axisNo, ex);
    return false;
}
catch (SEHException ex)
{
    OnError("MOVE_ABSOLUTE_FAILED", "运动控制卡原生异常", axisNo, ex);
    return false;
}
catch (Exception ex)
{
    OnError("MOVE_ABSOLUTE_FAILED", $"轴 {axisNo} 绝对移动失败", axisNo, ex);
    return false;
}
```

### 7.4 步骤 4：DI 注册（无需修改）

由于 `MotionIOManager` 已通过工厂方法创建所有 `Motion` 子类，且 `IMotionManager` 通过 DI 注入 `IMotionIOManager`，因此**不需要修改 Program.cs 的 DI 注册**。新增的驱动通过 XML 配置即可自动加载。

### 7.5 步骤 5：先使用模拟模式测试

在联机真硬件之前，模拟测试所有 API：

```csharp
// 单元测试：验证基本流程
[Test]
public void XYZ8000_Lifecycle_ShouldSucceed()
{
    var motion = new Motion_XYZ8000(0, "TestXYZ", 0, 3);

    Assert.That(motion.Init(), Is.True);
    Assert.That(motion.ServoOn(0), Is.True);
    Assert.That(motion.Home(0, HomeMode.ORG_P), Is.True);
    Assert.That(motion.AbsMove(0, 100.0, 50.0), Is.True);
    Assert.That(motion.GetAxisPos(0), Is.EqualTo(100.0));
    Assert.That(motion.ServoOff(0), Is.True);
    Assert.That(motion.DeInit(), Is.True);
}
```

测试通过后，在 XML 配置中设置 `Enable="true"`，连接真硬件进行联调。

---

## 8. 模拟模式

### 8.1 模拟模式概述

`OmniFrame.Simulation` 项目提供完全软件的硬件替代实现。启用模拟模式后：

- **运动指令**（如移动轴到位置 500）瞬间完成并返回成功
- **IO 读取**返回最后写入的值（环回行为）
- **PLC 读写**操作内存中的寄存器表而非物理设备

这使得开发人员可以在普通笔记本电脑上运行完整应用程序，无需连接任何物理硬件。

### 8.2 模拟类一览

| 类 | 模拟对象 | 关键特性 |
|----|----------|----------|
| `SimulatedMotion` | 运动控制器 | 继承 `Motion`，状态机完整：Idle/Moving/Homing/Alarming/Disabled |
| `SimulatedIoCtrl` | IO 板卡 | 64 输入/64 输出可配置，提供 `SetInput`/`GetOutput` 测试注入方法 |
| `SimulatedPlcDevice` | PLC | 内存寄存器表，Modbus TCP 兼容 |
| `SimulationContext` | 统一容器 | 管理三个模拟器的生命周期，一键初始化/关闭 |

### 8.3 SimulatedMotion 详细介绍

`SimulatedMotion` 继承自 `Motion`，完整实现了所有抽象方法和虚方法：

```csharp
public class SimulatedMotion : MotionIO.Motion
{
    public int SimulationDelayMs { get; set; } = 50; // 可配置的模拟延迟

    public SimulatedMotion(string name, int axisCount)
        : base(0, name, 0, axisCount - 1) { ... }
}
```

**支持的功能：**
- 绝对/相对运动（含软限位检查）
- 回原点（多种模式）
- JOG 运动
- 多轴直线插补（`AbsLinearMove`, `RelativeLinearMove`）
- 圆弧插补（`AbsArcMove`, `RelativeArcMove`）
- 软限位设置和启用/禁用
- 速度/加减速动态设置
- 报警清除

**软限位保护：** 模拟器在执行 `AbsMove` 时会检查目标位置是否超出软限位范围，超限则返回 false 并记录错误日志。

### 8.4 使用 SimulationContext 创建完整模拟环境

```csharp
using OmniFrame.Simulation;

// 方式一：使用便捷工厂方法
var simMotion = SimulationContext.CreateSimulatedMotion("TestAxis", axisCount: 4);
var simIo = SimulationContext.CreateSimulatedIo("TestIO", inputCount: 64, outputCount: 64);
var simPlc = SimulationContext.CreateSimulatedPlc("TestPLC", "127.0.0.1", 502);

// 单独初始化
simMotion.Init();
simIo.Init(null);
simPlc.Open();

// 执行操作
simMotion.ServoOn(0);
simMotion.Home(0, MotionIO.HomeMode.ORG_P);
simMotion.AbsMove(0, 100.0, 50.0);

// 方式二：一键创建完整环境
var fullSim = SimulationContext.CreateFullSimulation();
fullSim.InitializeAll();

// ... 执行测试 ...

fullSim.ShutdownAll();
```

### 8.5 如何扩展 SimulatedMotion

当硬件支持新特性（如特殊的回原点模式、自定义 IO 映射）时，可以在 `SimulatedMotion` 中覆盖对应方法：

```csharp
public class CustomSimulatedMotion : SimulatedMotion
{
    public CustomSimulatedMotion(string name, int axisCount)
        : base(name, axisCount) { }

    // 例如：覆盖 Home 方法添加特殊行为
    public override bool Home(int axisNo, MotionIO.HomeMode mode)
    {
        // 自定义前处理
        Logger.Info($"自定义回原点前处理: 轴 {axisNo}");

        // 调用基类标准行为
        bool result = base.Home(axisNo, mode);

        // 自定义后处理
        if (result)
        {
            Logger.Info($"自定义回原点后处理完成: 轴 {axisNo}");
        }

        return result;
    }

    // 添加测试辅助方法
    public void SimulateAlarm(int axisNo)
    {
        // 通过 ClearAlarm 前的状态设置模拟报警
        Logger.Warning($"模拟轴 {axisNo} 报警");
    }
}
```

### 8.6 真实硬件 vs 模拟模式切换

通过配置文件控制模式切换：

```xml
<!-- 真实硬件模式 -->
<appSettings>
  <add key="UseSimulation" value="false" />
</appSettings>

<!-- 模拟模式 -->
<appSettings>
  <add key="UseSimulation" value="true" />
</appSettings>
```

在代码中根据配置决定注册模拟类还是真实类。模拟模式下的 `SimulationDelayMs` 参数可调整延迟来模拟真实硬件的响应时间，便于测试超时逻辑。

---

## 9. 工站集成

### 9.1 StationBase 信号机制

所有工站继承自 `StationBase`，通过信号（Signal）机制实现工站之间的协调：

```csharp
public abstract class StationBase : IDisposable
{
    // 设置信号（发布事件到其他工站）
    protected void SetSignal(string signalName);

    // 等待信号（阻塞直到收到信号或超时）
    protected bool WaitSignal(string signalName, int timeoutMs);

    // 检查信号当前值（非阻塞）
    protected bool CheckSignal(string signalName);

    // 重置信号
    protected void ResetSignal(string signalName);

    // 子类必须实现的核心执行逻辑
    protected abstract bool DoExecute();

    // 模板方法：包含异常处理和自动重试
    public bool Execute();
}
```

### 9.2 信号名称常量

`SignalNameConstants` 为所有信号提供了编译时检查的常量定义，避免字符串拼写错误：

```csharp
public static class SignalNames
{
    public static class Robot1
    {
        public const string MoveToPick = "Robot1_MoveToPick";
        public const string AtPick    = "Robot1_AtPick";
        public const string Pick      = "Robot1_Pick";
        public const string PickDone  = "Robot1_PickDone";
        public const string MoveToPlace = "Robot1_MoveToPlace";
        public const string AtPlace   = "Robot1_AtPlace";
        public const string Place     = "Robot1_Place";
    }

    public static class Robot2 { /* 同 Robot1 */ }
    public static class Robot3 { /* 同 Robot1 */ }

    public static class Camera1
    {
        public const string Trigger = "Camera1_Trigger";
        public const string Done    = "Camera1_Done";
    }

    public static class Buffer
    {
        public const string ClampOff        = "BufferClamp_Off";
        public const string ClampOn         = "BufferClamp_On";
        public const string ConveyorForward = "BufferConveyor_Forward";
        public const string ConveyorStop    = "BufferConveyor_Stop";
        public const string PosArrived      = "BufferPos_Arrived";
        public const string MaterialPresent = "MaterialPresent";
    }

    public static class Transfer
    {
        public const string MoveToPick  = "Transfer_MoveToPick";
        public const string AtPick      = "Transfer_AtPick";
        public const string MoveToPlace = "Transfer_MoveToPlace";
        public const string AtPlace     = "Transfer_AtPlace";
    }

    public static class Cylinder1
    {
        public const string Extend    = "Cylinder1_Extend";
        public const string Extended  = "Cylinder1_Extended";
        public const string Retract   = "Cylinder1_Retract";
        public const string Retracted = "Cylinder1_Retracted";
    }

    public static class Cylinder2 { /* 同 Cylinder1 */ }
    public static class Gripper     { /* Close, Open */ }
    public static class UnloadGripper { /* Close, Open */ }
    public static class UnloadTransfer { /* MoveToPick, AtPick, MoveToPlace, AtPlace */ }
    public static class Lighting1   { /* On, Ready */ }
    public static class Lighting2   { /* On, Ready */ }
}
```

### 9.3 超时常量

`TimeoutConstants` 定义了所有标准超时值（单位：毫秒）：

```csharp
public static class TimeoutConstants
{
    public const int SignalWaitDefault = 3000;  // 默认信号等待 (3 秒)
    public const int SignalWaitShort   = 1000;  // 短等待 (1 秒)
    public const int SignalWaitLong    = 5000;  // 长等待 (5 秒)
    public const int SignalWaitInstant = 500;   // 几乎立即 (0.5 秒)

    public const int ThreadJoinTimeout  = 5000;  // 线程 Join 超时
    public const int ReconnectInterval  = 5000;  // 重连间隔
    public const int WatchdogDefault    = 5000;  // 看门狗默认值

    public const int PlaceDelay    = 500;   // 放置延时
    public const int CylinderWait  = 2000;  // 气缸动作等待
    public const int TransferWait  = 2000;  // 传送等待
}
```

### 9.4 完整工站示例：OHSloading1

以下是项目中的实际工站实现 `OHSloading1`，展示了 Step 枚举 + SetSignal/WaitSignal 的标准模式：

```csharp
using System;
using static OmniFrame.Core.SignalNames;
using static OmniFrame.Core.TimeoutConstants;
using OmniFrame.Common;

namespace OmniFrame.Core.Stations
{
    public class OHSloading1 : StationBase
    {
        // 定义工站的执行步骤
        private enum Step
        {
            Idle,
            MoveToPickPos,   // 移动至取料位
            Pick,            // 抓取
            MoveToPlacePos,  // 移动至放料位
            Place,           // 放置
            Complete         // 完成
        }

        private Step _currentStep = Step.Idle;

        public OHSloading1() : base("OHSloading1") { }

        protected override bool DoExecute()
        {
            _currentStep = Step.MoveToPickPos;

            while (_currentStep < Step.Complete)
            {
                switch (_currentStep)
                {
                    case Step.MoveToPickPos:
                        Logger.Info("OHS1: 机器人移动至取料位置");
                        SetSignal(SignalNames.Robot1.MoveToPick);
                        if (!WaitSignal(SignalNames.Robot1.AtPick, TimeoutConstants.SignalWaitDefault))
                        {
                            Logger.Error("OHS1: 取料位置到达超时");
                            return false;
                        }
                        _currentStep = Step.Pick;
                        break;

                    case Step.Pick:
                        Logger.Info("OHS1: 机器人抓取物料");
                        SetSignal(SignalNames.Robot1.Pick);
                        if (!WaitSignal(SignalNames.Robot1.PickDone, TimeoutConstants.SignalWaitShort))
                        {
                            Logger.Error("OHS1: 抓取超时");
                            return false;
                        }
                        _currentStep = Step.MoveToPlacePos;
                        break;

                    case Step.MoveToPlacePos:
                        Logger.Info("OHS1: 机器人移动至放置位置");
                        SetSignal(SignalNames.Robot1.MoveToPlace);
                        if (!WaitSignal(SignalNames.Robot1.AtPlace, TimeoutConstants.SignalWaitDefault))
                        {
                            Logger.Error("OHS1: 放置位置到达超时");
                            return false;
                        }
                        _currentStep = Step.Place;
                        break;

                    case Step.Place:
                        Logger.Info("OHS1: 机器人放置物料");
                        SetSignal(SignalNames.Robot1.Place);
                        System.Threading.Thread.Sleep(TimeoutConstants.PlaceDelay);
                        _currentStep = Step.Complete;
                        break;
                }
            }

            return true;
        }
    }
}
```

### 9.5 工站执行模式解析

OHSloading1 的执行流程：

```
DoExecute() 被调用
    │
    ▼
MoveToPickPos: 发信号 Robot1.MoveToPick → 等待 Robot1.AtPick (最多 3 秒)
    │ (超时 → 返回 false, StationBase 累计失败计数)
    ▼
Pick: 发信号 Robot1.Pick → 等待 Robot1.PickDone (最多 1 秒)
    │ (超时 → 返回 false)
    ▼
MoveToPlacePos: 发信号 Robot1.MoveToPlace → 等待 Robot1.AtPlace (最多 3 秒)
    │ (超时 → 返回 false)
    ▼
Place: 发信号 Robot1.Place → 延迟 500ms
    │
    ▼
Complete: 返回 true (ConsecutiveFailCount 清零)
```

### 9.6 自动重试机制

`StationBase.Execute()` 模板方法实现了自动重试：

- 每次 `DoExecute()` 返回 `false` → `ConsecutiveFailCount++`
- 连续失败 < `MaxRetryCount`（默认 3）→ 记录警告，调用方可选择重试
- 连续失败 >= `MaxRetryCount` → 工站进入错误状态 (`IsInError = true`)，触发 `StationErrorOccurred` 事件
- 调用 `Reset()` 可清除错误状态并重新初始化

### 9.7 与运动控制集成

工站通过 `IMotionManager` 和 `IIoManager` 与运动控制层交互。典型模式为在工站构造函数中注入所需服务：

```csharp
public class PickAndPlaceStation : StationBase
{
    private readonly IMotionManager _motion;
    private readonly IIoManager _io;

    public PickAndPlaceStation(IMotionManager motion, IIoManager io)
        : base("PickAndPlace")
    {
        _motion = motion;
        _io = io;
    }

    protected override bool DoExecute()
    {
        // 移动 X 轴到取料位置
        if (!_motion.MoveTo(cardIndex: 0, axis: 0, position: 10.0, speed: 50.0))
            return false;

        // 等待运动完成（通过 IO 信号确认）
        SetSignal(SignalNames.Cylinder1.Extend);
        if (!WaitSignal(SignalNames.Cylinder1.Extended, TimeoutConstants.CylinderWait))
            return false;

        // 抓取动作
        SetSignal(SignalNames.Gripper.Close);

        // 移动 X 轴到放料位置
        if (!_motion.MoveTo(cardIndex: 0, axis: 0, position: 200.0, speed: 80.0))
            return false;

        return true;
    }
}
```

---

## 10. 故障排查

### 10.1 DLL 未找到

**现象：** `System.DllNotFoundException: Unable to load DLL 'xxx.dll'`

**原因：**
- 原生 DLL 不在应用程序运行目录下
- DLL 依赖的 VC++ 运行时未安装
- 64 位 DLL 被 32 位进程加载（或反之）

**解决步骤：**
1. 检查 `bin/Debug/` 目录下是否存在对应的 `.dll` 文件
2. 安装厂商要求的 VC++ Redistributable（通常为 VS2015-2022 版本）
3. 确认 DLL 位数与应用程序目标平台一致（OmniFrame 目标为 x64）
4. 使用 Dependency Walker 或 Dependencies 工具检查 DLL 的依赖链是否完整

```powershell
# 在运行目录检查 DLL 是否存在
ls bin\Debug\*.dll | Select-String "gts|LTDMC|inoecat"
```

### 10.2 运动控制卡初始化失败

**现象：** `Init()` 返回 `false`，日志显示"运动控制卡初始化失败"

**排查步骤：**
1. **确认卡是否正确插入 PCIe 插槽**：检查设备管理器中是否出现该设备（无黄色感叹号）
2. **确认驱动是否安装**：运行厂商提供的驱动安装程序或测试工具，验证卡是否可被识别
3. **卡索引是否正确**：XML 配置中的 `Index` 与物理卡的编号是否匹配（通常第一张卡索引为 0）
4. **是否被其他程序占用**：关闭其他可能使用该卡的程序
5. **查看详细日志**：日志中 `[卡名称]` 前缀的错误信息会指明具体原因

```csharp
// 增加详细日志的初始化验证
var motion = new Motion_GTS(0, "TestCard", 0, 7);
bool result = motion.Init();
if (!result)
{
    Logger.Error("初始化失败，请检查: 1)卡是否插入 2)驱动是否安装 3)索引是否正确");
}
```

### 10.3 伺服无法使能

**现象：** 调用 `ServoOn()` 返回 `false` 或轴状态保持 `Disabled`

**排查步骤：**
1. **伺服驱动器是否上电**：检查驱动器面板指示灯，确认主电源和控制电源均已接通
2. **急停是否释放**：急停按钮按下时，伺服无法使能
3. **报警是否清除**：先调用 `ClearAlarm(axisNo)` 清除伺服驱动器报警
4. **限位开关状态**：轴是否已压到硬限位（导致驱动器保护性禁止使能），手动将轴移离限位
5. **使能信号接线**：检查运动控制卡到伺服驱动器的使能信号线（SON）是否正常连接

```csharp
// 安全使能流程
var card = motionManager.GetMotion(cardIndex: 0);
if (card.GetAxisState(axisNo: 0) == AxisState.Alarming)
{
    Logger.Warning("轴 0 处于报警状态，尝试清除...");
    card.ClearAlarm(axisNo: 0);
    System.Threading.Thread.Sleep(500); // 等待驱动器恢复
}

if (card.ServoOn(axisNo: 0))
{
    Logger.Info("轴 0 伺服使能成功");
}
else
{
    Logger.Error("轴 0 伺服使能失败，请检查驱动器状态");
}
```

### 10.4 轴报警

**现象：** `GetAxisState()` 返回 `Alarming`，轴无法运动

**常见报警原因：**

| 报警类型 | 可能原因 | 解决方式 |
|----------|----------|----------|
| 过流 (Over Current) | 机械卡死、负载过大、参数不当 | 检查机械自由度，降低加速度 |
| 过速 (Over Speed) | 速度设定过高、编码器故障 | 降低速度参数，检查编码器线缆 |
| 跟随误差 (Following Error) | 实际位置与指令位置偏差过大 | 增大跟随误差阈值或降低加速度 |
| 编码器故障 | 编码器线缆松动或断裂 | 检查编码器接线 |
| 驱动器过热 | 散热不良、环境温度过高 | 改善散热条件 |
| 欠压/过压 | 电源异常 | 检查电源电压 |

```csharp
// 报警处理流程
if (motion.GetAxisState(0) == AxisState.Alarming)
{
    Logger.Error($"轴 0 报警！");
    motion.StopAxis(0);                     // 确保停止
    System.Threading.Thread.Sleep(1000);     // 等待驱动器稳定
    motion.ClearAlarm(0);                   // 清除报警
    System.Threading.Thread.Sleep(500);
    if (motion.GetAxisState(0) != AxisState.Alarming)
    {
        Logger.Info("轴 0 报警已清除");
    }
    else
    {
        Logger.Error("轴 0 报警无法清除，需要断电复位");
    }
}
```

### 10.5 限位开关触发

**现象：** 轴只能向一个方向运动，反方向运动命令无响应或立即停止

**原因：** 轴运动过程中触发了硬限位开关或软限位

**软限位 vs 硬限位：**

| 限位类型 | 实现方式 | 触发后行为 |
|----------|----------|------------|
| **软限位 (Soft Limit)** | 运动控制卡软件设定位置范围 | 轴在到达软限位前自动减速停止，可反向运动 |
| **硬限位 (Hard Limit)** | 物理限位开关（光电或机械） | 轴立即停止，需要通过手动移动或特定回原点离开限位位置 |

**解决步骤：**
1. 读取当前位置确认是否超出软限位范围
2. 如果触发硬限位，手动将轴移离限位开关（通常需要手动旋转丝杆或使用手动模式）
3. 调整软限位范围，确保在硬限位开关内侧
4. 检查限位开关接线和信号是否正常

```csharp
// 限位触发后手动退出
double currentPos = motion.GetAxisPos(0);
if (currentPos >= positiveSoftLimit)
{
    Logger.Warning($"轴 0 超出正限位 (当前位置={currentPos})，尝试反向运动...");
    // 临时禁用软限位
    motion.EnableSoftLimit(0, enable: false);
    // 反向运动 5mm
    motion.RelativeMove(0, -5.0, speed: 10.0);
    while (motion.IsAxisMoving(0)) { System.Threading.Thread.Sleep(10); }
    // 恢复软限位
    motion.EnableSoftLimit(0, enable: true);
}
```

### 10.6 通信超时（总线型）

**现象：** EtherCAT 或其他总线型运动控制卡初始化失败或运动中通信断开

**排查步骤：**
1. **物理连接检查**：网线是否插好，指示灯是否正常闪烁
2. **IP 地址配置**：PC 与伺服驱动器/总线耦合器在同一个网段
3. **防火墙设置**：Windows 防火墙可能阻止 EtherCAT 通信。将 EtherCAT 端口或应用加入白名单
4. **网络负载**：EtherCAT 需要独占网卡或在专用网络上运行。检查是否有其他程序占用网络
5. **从站状态**：使用厂商工具检查 EtherCAT 从站是否在线

```csharp
// 总线型初始化带重试
for (int retry = 0; retry < 3; retry++)
{
    if (motion.Init())
    {
        Logger.Info("EtherCAT 初始化成功");
        break;
    }
    Logger.Warning($"EtherCAT 初始化失败 (第 {retry + 1}/3 次尝试)，等待 {TimeoutConstants.ReconnectInterval}ms 重试...");
    System.Threading.Thread.Sleep(TimeoutConstants.ReconnectInterval);
}
```

---

## 11. 安全规范

### 11.1 紧急停止模式

`DeviceManager.EmergencyStop()` 是全局紧急停止方法，在应用退出（`ShutdownInfrastructure`）时自动调用：

```csharp
public void EmergencyStop()
{
    Logger.Error("执行设备紧急停止!");

    foreach (var kvp in _devices)
    {
        if (kvp.Value is MotionDevice motion)
        {
            // 停止该设备的所有 8 个轴
            for (int i = 0; i < 8; i++)
            {
                try
                {
                    motion.StopAxis(i);
                }
                catch (Exception ex)
                {
                    Logger.Error($"紧急停止轴 {i} 失败: {motion.Name}", ex);
                }
            }
        }
    }
}
```

**调用时机：**
- 用户点击 UI 上的「急停」按钮
- 程序正常退出时（`ShutdownInfrastructure`）
- 安全光幕或门锁触发
- 看门狗超时

### 11.2 限位开关处理

**硬限位 (Hard Limit)：**
- 连接到伺服驱动器的限位输入（通常为 P-OT 和 N-OT 信号）
- 触发后驱动器硬件级别封锁脉冲输出
- 轴**无法**通过软件命令运动（除非反向离开限位）
- 必须确保软限位在硬限位内侧，避免频繁触发硬限位

**软限位 (Soft Limit)：**
- 在运动控制卡固件层面实现，由 `SetSoftLimit` 和 `EnableSoftLimit` 控制
- 触发后轴可反向运动
- 应在初始化后立即配置

```csharp
// 推荐的限位安全配置顺序
// 1. 初始化卡
motion.Init();

// 2. 伺服使能
motion.ServoOn(axisNo: 0);

// 3. 先设置软限位（在硬限位内侧留余量）
double hardLimitPositive = 210.0;  // 硬限位开关位置（由机械设计决定）
double hardLimitNegative = -10.0;
double safetyMargin = 5.0;          // 安全余量

motion.SetSoftLimit(
    axisNo: 0,
    positive: hardLimitPositive - safetyMargin,  // 205.0
    negative: hardLimitNegative + safetyMargin    // -5.0
);

// 4. 启用软限位
motion.EnableSoftLimit(axisNo: 0, enable: true);

// 5. 回原点
motion.Home(axisNo: 0, HomeMode.ORG_P);

Logger.Info($"轴 0 软限位已启用: [-5.0, 205.0]");
```

### 11.3 速度安全策略

```csharp
public static class SpeedSafety
{
    // 不同模式的速度限制
    public const double JogSpeedMax     = 30.0;   // JOG 手动模式最大速度
    public const double AutoSpeedMax    = 200.0;  // 自动模式最大速度
    public const double HomeSpeedMax    = 20.0;   // 回原点最大速度
    public const double DebugSpeedMax   = 10.0;   // 调试模式最大速度

    /// <summary>
    /// 根据当前模式限制速度
    /// </summary>
    public static double ClampSpeed(string mode, double requestedSpeed)
    {
        double maxSpeed = mode switch
        {
            "Debug"  => DebugSpeedMax,
            "Jog"    => JogSpeedMax,
            "Home"   => HomeSpeedMax,
            "Auto"   => AutoSpeedMax,
            _        => DebugSpeedMax   // 未知模式：使用最低速度
        };

        if (requestedSpeed > maxSpeed)
        {
            Logger.Warning($"速度 {requestedSpeed} 超出 {mode} 模式上限 {maxSpeed}，已限制");
            return maxSpeed;
        }

        return requestedSpeed;
    }
}

// 使用示例
double safeSpeed = SpeedSafety.ClampSpeed("Auto", 250.0);  // 返回 200.0
motion.AbsMove(0, 100.0, safeSpeed);
```

### 11.4 运动前安全检查清单

在执行任何运动命令前，应检查以下条件：

```csharp
public class MotionSafetyGuard
{
    private readonly IMotionManager _motion;

    public MotionSafetyGuard(IMotionManager motion)
    {
        _motion = motion;
    }

    /// <summary>
    /// 运动前安全检查，全部通过才允许运动
    /// </summary>
    public bool PreMoveCheck(int cardIndex, int axis)
    {
        // 1. 检查通信状态
        if (!_motion.IsConnected)
        {
            Logger.Error("运动控制卡未连接，无法执行运动");
            return false;
        }

        // 2. 检查是否有未确认的报警
        var motionIO = ServiceProviderCache.GetService<IMotionIOManager>();
        var card = motionIO.GetMotion(cardIndex);
        if (card == null)
        {
            Logger.Error($"卡索引 {cardIndex} 无效");
            return false;
        }

        var axisState = card.GetAxisState(axis);
        if (axisState == AxisState.Alarming)
        {
            Logger.Error($"轴 {axis} 处于报警状态，请先清除报警");
            return false;
        }

        // 3. 检查伺服使能状态
        if (axisState == AxisState.Disabled)
        {
            Logger.Error($"轴 {axis} 伺服未使能，请先调用 ServoOn");
            return false;
        }

        // 4. 检查是否已在运动中（避免重叠指令）
        if (card.IsAxisMoving(axis))
        {
            Logger.Warning($"轴 {axis} 正在运动中，请等待当前运动完成或先停止");
            return false;
        }

        Logger.Info($"轴 {axis} 安全检查通过");
        return true;
    }
}
```

### 11.5 设备管理器生命周期

```
程序启动
    │
    ▼
Initialize()      -- 读取配置，创建设备
    │
    ▼
Start()           -- 连接所有设备 (Connect)
    │
    ▼
├─ 正常运行 ──────────── Reset() (可选)
│                               │
│                               ▼
│                          ClearErrors() + Reset
│                               │
│                               ▼
│                          回到正常运行
│
├─ 异常发生
│       │
│       ▼
│   EmergencyStop()  -- 停止所有轴
│
├─ 正常退出
│       │
│       ▼
│   Stop()      -- 断开所有设备 (Disconnect)
│       │
│       ▼
│   Dispose()   -- 释放所有资源
```

### 11.6 安全编程注意事项总结

1. **任何运动命令前确保伺服已使能**：`ServoOff` 状态下发送运动命令将失败
2. **长行程运动前先低速验证**：确保行程范围内无障碍物
3. **软限位务必设置并启用**：作为硬限位的最后一道软件防线
4. **回原点后确认位置为 0**：防止累积误差导致撞机
5. **所有异常必须捕获并记录**：原生 DLL 崩溃（SEHException）不会抛到 CLR 上层，必须在每个 P/Invoke 调用处捕获
6. **程序退出时释放所有设备**：`finally` 块中调用 `DeInit()` 或依赖 `Dispose()` 模式
7. **紧急停止不可恢复**：急停后需要人工确认并 `Reset()` 才能恢复
8. **龙门结构特殊处理**：龙门双驱必须使用 `GanTry_MEL` 或 `GanTry_PEL` 回原点，确保两侧同步

---

## 附录

### A. Motion 抽象类完整 API 速查表

| 方法 | 签名 | 类型 | 说明 |
|------|------|------|------|
| `Init` | `bool Init()` | abstract | 初始化运动控制卡 |
| `DeInit` | `bool DeInit()` | abstract | 反初始化，释放资源 |
| `ServoOn` | `bool ServoOn(int axisNo)` | abstract | 伺服使能 |
| `ServoOff` | `bool ServoOff(int axisNo)` | abstract | 伺服失能 |
| `AbsMove` | `bool AbsMove(int axisNo, double pos, double speed)` | abstract | 绝对运动 |
| `RelativeMove` | `bool RelativeMove(int axisNo, double pos, double speed)` | abstract | 相对运动 |
| `Home` | `bool Home(int axisNo, HomeMode mode)` | abstract | 回原点 |
| `StopAxis` | `bool StopAxis(int axisNo)` | abstract | 停止指定轴 |
| `StopAllAxis` | `bool StopAllAxis()` | abstract | 停止所有轴 |
| `GetAxisPos` | `double GetAxisPos(int axisNo)` | abstract | 获取轴位置 |
| `IsAxisMoving` | `bool IsAxisMoving(int axisNo)` | abstract | 是否运动中 |
| `IsAxisHomed` | `bool IsAxisHomed(int axisNo)` | abstract | 是否已回原点 |
| `GetAxisState` | `AxisState GetAxisState(int axisNo)` | abstract | 获取轴状态 |
| `ClearAlarm` | `bool ClearAlarm(int axisNo)` | abstract | 清除报警 |
| `SetSoftLimit` | `bool SetSoftLimit(int axisNo, double pos, double neg)` | abstract | 设置软限位 |
| `EnableSoftLimit` | `bool EnableSoftLimit(int axisNo, bool enable)` | abstract | 启用/禁用软限位 |
| `AbsLinearMove` | `bool AbsLinearMove(int[], double[], double speed, double acc, double dec)` | virtual | 直线插补(绝对) |
| `RelativeLinearMove` | `bool RelativeLinearMove(int[], double[], double speed, double acc, double dec)` | virtual | 直线插补(相对) |
| `AbsArcMove` | `bool AbsArcMove(int[], double[], double angle, double speed)` | virtual | 圆弧插补(绝对) |
| `RelativeArcMove` | `bool RelativeArcMove(int[], double[], double angle, double speed)` | virtual | 圆弧插补(相对) |
| `JogMove` | `bool JogMove(int axisNo, double speed, bool positiveDir)` | virtual | JOG 运动 |
| `SetAxisSpeed` | `bool SetAxisSpeed(int axisNo, double speed)` | virtual | 动态设置速度 |
| `SetAxisAccDec` | `bool SetAxisAccDec(int axisNo, double acc, double dec)` | virtual | 动态设置加减速 |

### B. IServiceProvider 访问

由于工站不是通过 DI 创建的实例（工站由 `StationManager` 管理），访问 DI 注册的服务需要通过 `ServiceProviderCache`：

```csharp
// 在工站内部获取 MotionManager
var motionMgr = ServiceProviderCache.GetService<IMotionManager>();
var ioMgr = ServiceProviderCache.GetService<IIoManager>();
```

### C. 关键文件路径汇总

| 文件 | 路径 |
|------|------|
| Motion 抽象基类 | `src/MotionIO/Motion.cs` |
| MotionDevice 包装类 | `src/MotionIO/MotionDevice.cs` |
| MotionIOManager | `src/MotionIO/MotionIOManager.cs` |
| Motion_GTS (固高) | `src/MotionIO/Motion_GTS.cs` |
| Motion_Dmc3000 (雷赛) | `src/MotionIO/Motion_Dmc3000.cs` |
| Motion_DMC3400 (雷赛) | `src/MotionIO/Motion_DMC3400.cs` |
| Motion_InoEcat (汇川) | `src/MotionIO/Motion_InoEcat.cs` |
| Motion_PCIeM60 | `src/MotionIO/Motion_PCIeM60.cs` |
| IoCtrl 抽象基类 | `src/MotionIO/IoCtrl.cs` |
| IoCtrl_PCIeM60 | `src/MotionIO/IoCtrl_PCIeM60.cs` |
| MotionManager (Core) | `src/OmniFrame.Core/MotionManager.cs` |
| IoManager (Core) | `src/OmniFrame.Core/IoManager.cs` |
| DeviceManager | `src/OmniFrame.Core/DeviceManager.cs` |
| StationBase | `src/OmniFrame.Core/StationBase.cs` |
| OHSloading1 (示例工站) | `src/OmniFrame.Core/Stations/OHSloading1.cs` |
| TimeoutConstants | `src/OmniFrame.Core/TimeoutConstants.cs` |
| SignalNameConstants | `src/OmniFrame.Core/SignalNameConstants.cs` |
| MotionConfig | `src/OmniFrame.Core/ConfigModels/MotionConfig.cs` |
| AxisParam | `src/OmniFrame.Core/ConfigModels/AxisParam.cs` |
| SimulatedMotion | `src/OmniFrame.Simulation/SimulatedMotion.cs` |
| SimulatedIoCtrl | `src/OmniFrame.Simulation/SimulatedIoCtrl.cs` |
| SimulationContext | `src/OmniFrame.Simulation/SimulationContext.cs` |
| DI 注册 (Program.cs) | `src/OmniFrame/Program.cs` |
