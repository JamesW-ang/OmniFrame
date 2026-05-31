# 模拟层使用指南

模拟层让你在普通的 Windows 笔记本上即可开发和测试 OmniFrame，无需任何实体硬件（无需运动控制器、IO 板卡或 PLC）。

---

## 什么是模拟层

`OmniFrame.Simulation` 项目为每一个硬件接口提供了进程内软件替代实现。当模拟模式激活时：

- **运动指令**（例如"将 X 轴移动到位置 500"）立即完成并返回成功。
- **I/O 读取** 返回上次写入的值（回环行为）。
- **PLC 读写** 访问内存中的寄存器表，而非实体设备。

这意味着你无需进入工厂车间，就可以运行完整的应用程序、在调试器中逐步跟踪工站状态机，并验证 UI 行为。

---

## 四大模拟类

| 类 | 模拟内容 |
|---|---|
| **SimulatedMotion** | 运动控制器轴（伺服驱动器、步进电机）。接收移动/停止/回零指令。在可配置的延迟后，将目标位置作为当前位置上报。 |
| **SimulatedIoCtrl** | 数字量和模拟量 IO 板卡。数字量输入镜像上次写入的数字量输出（回环行为）。模拟量值存储在内存中。 |
| **SimulatedPlcDevice** | 具有 Modbus 兼容软元件的 PLC。维护一份内存中的 X（输入）、Y（输出）、M（中间继电器）和 D（数据寄存器）值表。响应 Modbus TCP/RTU 读写请求。 |
| **SimulationContext** | 一个容器类，将上述三个设备模拟器串联在一起。提供统一的 `Initialize()/Start()/Stop()` 生命周期调用，供 DI 容器使用。 |

---

## 如何配置应用启用模拟模式

配置通常通过 `app.config` 或 `appsettings.json` 中的键值来驱动：

```xml
<!-- 在 app.config 中 -->
<add key="UseSimulation" value="true" />
```

```json
// 在 appsettings.json 中
{
  "UseSimulation": true
}
```

在 `Program.cs` 中，DI 注册会检查此标志：

```csharp
if (configuration.UseSimulation)
{
    services.AddSingleton<IMotionController, SimulatedMotion>();
    services.AddSingleton<IIoCtrl, SimulatedIoCtrl>();
    services.AddSingleton<IPlcDevice, SimulatedPlcDevice>();
}
else
{
    services.AddSingleton<IMotionController, RealMotionController>();
    services.AddSingleton<IIoCtrl, RealIoCtrl>();
    services.AddSingleton<IPlcDevice, RealPlcDevice>();
}
```

在连接真实硬件运行时，将 `UseSimulation` 设置为 `false`。

---

## 如何添加新的模拟设备

假设你需要模拟一个通过串口写入字符串的条码扫描器。

### 步骤 1：创建模拟类

在 `src/OmniFrame.Simulation/` 中创建新文件：

```csharp
public class SimulatedBarcodeScanner : IBarcodeScanner
{
    private readonly List<string> _predefinedScans = new()
    {
        "SN001-ABC-2024",
        "SN002-DEF-2024",
        "SN003-GHI-2024"
    };
    private int _index = 0;

    public string Read()
    {
        // 循环返回预定义的条码
        string result = _predefinedScans[_index];
        _index = (_index + 1) % _predefinedScans.Count;
        return result;
    }

    public void Initialize() { }
    public void Start() { }
    public void Stop() { }
}
```

### 步骤 2：在 SimulationContext 中注册

将新的模拟器添加到 `SimulationContext` 中，使其与现有模拟器一起被创建和管理：

```csharp
public class SimulationContext
{
    public SimulatedBarcodeScanner BarcodeScanner { get; private set; }

    public void Initialize()
    {
        // ... 已有初始化代码 ...
        BarcodeScanner = new SimulatedBarcodeScanner();
        BarcodeScanner.Initialize();
    }
}
```

### 步骤 3：在 DI 中注册

在 `Program.cs` 的模拟分支中添加注册：

```csharp
if (configuration.UseSimulation)
{
    // ... 已有注册代码 ...
    services.AddSingleton<IBarcodeScanner>(sp =>
        sp.GetRequiredService<SimulationContext>().BarcodeScanner);
}
```

### 步骤 4：编写测试

在 `OmniFrame.Tests` 中编写测试，直接实例化 `SimulatedBarcodeScanner` 并验证其行为：

```csharp
[Test]
public void BarcodeScanner_RotatesThroughPredefinedScans()
{
    var scanner = new SimulatedBarcodeScanner();
    string first = scanner.Read();
    string second = scanner.Read();
    Assert.That(first, Is.Not.EqualTo(second));
}
```

---

## 局限性

| 局限性 | 详细说明 |
|---|---|
| **时序仅近似模拟** | 真实运动需要数秒；模拟运动在毫秒内即报告完成。请勿依赖模拟时序进行节拍时间计算。 |
| **无电气信号保真度** | 模拟无法检测接线故障、连接器松动或电磁干扰。这些必须在真实硬件上进行测试。 |
| **无厂家特定边缘情况** | 真实 PLC 存在各种特性（字节序、寄存器对齐、连接断开）。模拟返回干净、理想的响应。 |
| **串口模拟为内存模拟** | 模拟 IO 控制器不会打开真实的 COM 口。如果需要测试与真实外部设备的串口通信，请使用物理串口。 |
| **仅支持单实例** | 模拟层面向单开发者在单台机器上使用。不支持模拟多控制器的工厂网络。 |

---

## 快速验证

将 `UseSimulation` 设置为 `true` 并启动应用后：

1. 使用 `admin` / `admin123` 登录。
2. 导航到任意工站详情视图。
3. 工站应能正常逐步执行，无需真实硬件。
4. UI 中的信号指示灯应随模拟 I/O 值的变化而切换。
5. 如果工站卡在某个 `WaitSignal` 调用处，请检查信号名称是否与模拟器提供的信号名称一致。
