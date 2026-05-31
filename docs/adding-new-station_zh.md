# 如何添加新工站

本指南完整讲解如何在 OmniFrame 中添加一个新的工站类型，从开始到结束。

---

## 概述：StationBase 与状态机模式

OmniFrame 中的每个工站都继承自 `StationBase`（定义在 `OmniFrame.Sdk` 中）。一个工站对应一个物理制造单元——机器人、传送带、检测相机等。

`StationBase` 使用**离散步骤状态机**。每个工站定义一个步骤枚举和一个 `switch` 语句，每次迭代执行一个步骤。当一个步骤完成时，工站推进到下一步骤。此模式刻意保持简单和显式——没有隐式状态跳转。

```
Idle → Step1 → Step2 → ... → StepN → Complete → Idle
```

运行时通过定时器（或后台线程的紧密循环）调用 `ExecuteCycle()`。每次调用时，工站检查其当前步骤，执行相应的逻辑，然后要么停留在该步骤（等待信号），要么向前推进。

---

## 步骤枚举模式

在工站类内部定义一个私有枚举：

```csharp
private enum Step
{
    Idle,
    WaitForPartPresent,
    ClampPart,
    SendToPlc,
    WaitForPlcAck,
    Unclamp,
    Complete
}
```

枚举设为 `private` 是因为只有工站自身需要了解其内部步骤。外部代码通过高层命令（如 `Start()`）与之交互。

---

## SetSignal / WaitSignal API

工站通过命名信号与外部世界（I/O、PLC、其他工站）进行通信。

```csharp
// 写入一个信号（例如，打开继电器、写入 PLC 线圈）
void SetSignal(string signalName, bool value);

// 阻塞等待，直到信号变为期望的值，或超时到期
SignalResult WaitSignal(string signalName, bool expectedValue, int timeoutMs);
```

`WaitSignal` 返回 `SignalResult`，该对象包含两个属性：

| 属性 | 类型 | 含义 |
|---|---|---|
| `Success` | `bool` | 如果信号在超时前匹配则为 `true`；如果超时到期则为 `false`。 |
| `ElapsedMs` | `int` | 等待所花费的毫秒数。 |

---

## 信号命名规范

信号遵循严格的命名规范：**`DeviceName_Action`**，例如：

- `Robot1_AtPick` — 机器人 1 处于取料位置。
- `Clamp_Closed` — 夹爪气缸处于闭合（伸出）位置。
- `PlcConveyor_Running` — PLC 上报传送带电机正在运行。
- `Camera_InspectionDone` — 视觉系统已完成检测。

在创建新的信号名称之前，请务必检查 `SignalNames` 常量类中已有的信号名称。

---

## 完整代码示例：一个简单的 3 步骤工站

```csharp
using OmniFrame.Sdk;
using Common;

namespace OmniFrame.Core.Stations
{
    public class PickAndPlaceStation : StationBase
    {
        private enum Step
        {
            Idle,
            WaitForPartAtPick,
            MoveToPlace,
            WaitForReleaseDone,
            Complete
        }

        public PickAndPlaceStation(ILogger logger) : base(logger)
        {
            StationName = "PickAndPlace";
        }

        protected override void ExecuteCycle()
        {
            switch (currentStep)
            {
                case Step.Idle:
                    // 不做任何事。工站等待外部 Start() 调用。
                    break;

                case Step.WaitForPartAtPick:
                    Logger.Info("等待取料位置来料...");
                    var pickResult = WaitSignal("Feeder_PartAtPick", true, TimeoutConstants.Medium);
                    if (pickResult.Success)
                    {
                        Logger.Info("检测到来料。正在夹紧...");
                        SetSignal("PickClamp_Close", true);
                        AdvanceTo(Step.MoveToPlace);
                    }
                    else
                    {
                        Logger.Warning("等待取料位置来料超时。正在重试...");
                    }
                    break;

                case Step.MoveToPlace:
                    SetSignal("Robot_MoveToPlace", true);
                    var moveResult = WaitSignal("Robot_AtPlace", true, TimeoutConstants.Long);
                    if (moveResult.Success)
                    {
                        SetSignal("PickClamp_Close", false); // 释放工件
                        AdvanceTo(Step.WaitForReleaseDone);
                    }
                    else
                    {
                        Logger.Error("机器人未能到达放料位置。");
                        SetAlarm("RobotMoveTimeout", moveResult.ElapsedMs);
                        AdvanceTo(Step.Idle); // 中止并回到 Idle
                    }
                    break;

                case Step.WaitForReleaseDone:
                    var releaseResult = WaitSignal("PlaceSensor_PartReceived", true, TimeoutConstants.Short);
                    if (releaseResult.Success)
                    {
                        Logger.Info("放料完成。循环结束。");
                        AdvanceTo(Step.Complete);
                    }
                    break;

                case Step.Complete:
                    SetSignal("PickClamp_Close", false);
                    Logger.Info("循环完成。返回 Idle。");
                    AdvanceTo(Step.Idle);
                    break;
            }
        }
    }
}
```

---

## 如何在 DI 中注册工站

打开 `src/OmniFrame/DiConfigurator.cs`，找到 `ConfigureServices` 方法，添加你的工站：

```csharp
// 在 DiConfigurator.cs → ConfigureServices 中
services.AddSingleton<PickAndPlaceStation>();
```

如果你的工站实现了接口，则按接口注册：

```csharp
services.AddSingleton<IStation, PickAndPlaceStation>();
```

工站注册为 **Singleton**，因为工厂中每个物理工站有且仅有一个实例。

---

## 如何将工站添加到 UI

1. **添加显示控件**（如需要）：在 `OmniFrame` 项目中创建一个 `UserControl`，用于渲染工站状态（步骤名称、信号状态、报警信息）。

2. **在主窗体中注册**：在 `MainForm.cs` 中，创建实例并将其添加到布局中：

   ```csharp
   var pickPlaceStation = serviceProvider.GetRequiredService<PickAndPlaceStation>();
   var pickPlaceControl = new StationControl(pickPlaceStation);
   flowLayoutPanel.Controls.Add(pickPlaceControl);
   ```

3. **启动工站**：`StationBase` 暴露了 `Start()` 和 `Stop()` 方法。当操作员按下对应的 UI 按钮时调用 `Start()`。

---

## 如何为工站编写测试

工站测试使用 **NUnit + Moq**。由于工站依赖信号（信号来自 I/O/PLC），你需要模拟（mock）信号基础设施。

```csharp
[TestFixture]
public class PickAndPlaceStationTests
{
    private Mock<ILogger> _mockLogger;
    private PickAndPlaceStation _station;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger>();
        _station = new PickAndPlaceStation(_mockLogger.Object);
    }

    [Test]
    public void Start_TransitionsFromIdleToWaitForPartAtPick()
    {
        _station.Start();
        // 当 WaitSignal 成功时，ExecuteCycle 会推进状态。
        // 在实际测试中，你会注入一个 mock 信号提供器，
        // 使其对 "Feeder_PartAtPick" 返回成功。
        Assert.That(_station.CurrentStep, Is.EqualTo("WaitForPartAtPick"));
    }

    [Test]
    public void WaitSignalTimeout_SetsAlarmAndGoesToIdle()
    {
        // Arrange：配置 mock 信号提供器始终超时。
        // Act：启动工站并驱动 ExecuteCycle。
        // Assert：报警被设置，步骤重置为 Idle。
    }
}
```

关键测试原则：

- **模拟信号提供器** — 工站在测试中不应依赖真实 I/O。注入一个 `Mock<ISignalProvider>`，以便在每个测试用例中按需配置。
- **测试超时行为** — 务必测试 `WaitSignal` 超时时会发生什么。超时是生产环境中最常见的故障模式。
- **测试报警路径** — 验证在出现问题时，`SetAlarm` 是否使用预期的报警码被调用。
- **先测正常流程** — 一个运行完整 Idle → Complete 循环的测试，即可确认工站逻辑正确串联。
