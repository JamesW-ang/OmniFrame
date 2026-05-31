# How to Add a New Station

This guide walks through adding a new station type to OmniFrame, end-to-end.

---

## Overview: StationBase and the State Machine Pattern

Every station in OmniFrame inherits from `StationBase` (defined in `OmniFrame.Sdk`). A station models a physical manufacturing cell — a robot, a conveyor, an inspection camera, etc.

`StationBase` uses a **discrete-step state machine**. Each station defines an enum of step names and a `switch` statement that executes one step per iteration. When a step completes, the station advances to the next step. This pattern is intentionally simple and explicit — no hidden transitions.

```
Idle → Step1 → Step2 → ... → StepN → Complete → Idle
```

The runtime invokes `ExecuteCycle()` on a timer (or in a tight loop from a background thread). On each call, the station checks its current step, executes the associated logic, and either remains on that step (waiting for a signal) or advances.

---

## The Step Enum Pattern

Define a private enum inside your station class:

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

The enum is `private` because only the station itself needs to know its internal steps. External code interacts through high-level commands (e.g., `Start()`).

---

## SetSignal / WaitSignal API

Stations communicate with the outside world (I/O, PLCs, other stations) via named signals.

```csharp
// Write a signal (e.g., turn on a relay, write a PLC coil)
void SetSignal(string signalName, bool value);

// Block until a signal becomes the expected value, or timeout expires
SignalResult WaitSignal(string signalName, bool expectedValue, int timeoutMs);
```

`WaitSignal` returns `SignalResult`, which has two properties:

| Property | Type | Meaning |
|---|---|---|
| `Success` | `bool` | `true` if the signal matched before timeout; `false` if timeout elapsed. |
| `ElapsedMs` | `int` | Milliseconds spent waiting. |

---

## Signal Naming Convention

Signals follow a strict naming convention: **`DeviceName_Action`**, e.g.:

- `Robot1_AtPick` — Robot 1 is at the pick position.
- `Clamp_Closed` — The clamp cylinder is in the closed (extended) position.
- `PlcConveyor_Running` — The PLC reports the conveyor motor is on.
- `Camera_InspectionDone` — The vision system has finished inspection.

Always check the existing signal names in the `SignalNames` constants class before inventing new ones.

---

## Full Code Example: A Simple 3-Step Station

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
                    // Do nothing. Station waits for an external Start() call.
                    break;

                case Step.WaitForPartAtPick:
                    Logger.Info("Waiting for part at pick position...");
                    var pickResult = WaitSignal("Feeder_PartAtPick", true, TimeoutConstants.Medium);
                    if (pickResult.Success)
                    {
                        Logger.Info("Part detected. Clamping...");
                        SetSignal("PickClamp_Close", true);
                        AdvanceTo(Step.MoveToPlace);
                    }
                    else
                    {
                        Logger.Warning("Timeout waiting for part at pick. Retrying...");
                    }
                    break;

                case Step.MoveToPlace:
                    SetSignal("Robot_MoveToPlace", true);
                    var moveResult = WaitSignal("Robot_AtPlace", true, TimeoutConstants.Long);
                    if (moveResult.Success)
                    {
                        SetSignal("PickClamp_Close", false); // Release part
                        AdvanceTo(Step.WaitForReleaseDone);
                    }
                    else
                    {
                        Logger.Error("Robot failed to reach place position.");
                        SetAlarm("RobotMoveTimeout", moveResult.ElapsedMs);
                        AdvanceTo(Step.Idle); // Abort and go to Idle
                    }
                    break;

                case Step.WaitForReleaseDone:
                    var releaseResult = WaitSignal("PlaceSensor_PartReceived", true, TimeoutConstants.Short);
                    if (releaseResult.Success)
                    {
                        Logger.Info("Place complete. Cycle done.");
                        AdvanceTo(Step.Complete);
                    }
                    break;

                case Step.Complete:
                    SetSignal("PickClamp_Close", false);
                    Logger.Info("Cycle complete. Returning to Idle.");
                    AdvanceTo(Step.Idle);
                    break;
            }
        }
    }
}
```

---

## How to Register the Station in DI

Open `src/OmniFrame/DiConfigurator.cs`, locate the `ConfigureServices` method, and add your station:

```csharp
// In DiConfigurator.cs → ConfigureServices
services.AddSingleton<PickAndPlaceStation>();
```

If your station implements an interface, register by interface:

```csharp
services.AddSingleton<IStation, PickAndPlaceStation>();
```

Stations are registered as **Singleton** because there is exactly one instance of each physical station in the factory.

---

## How to Add the Station to the UI

1. **Add a display control** (if needed): Create a `UserControl` in the `OmniFrame` project that renders the station's status (step name, signal states, alarms).

2. **Register in the main form**: In `MainForm.cs`, create an instance and add it to the layout:

   ```csharp
   var pickPlaceStation = serviceProvider.GetRequiredService<PickAndPlaceStation>();
   var pickPlaceControl = new StationControl(pickPlaceStation);
   flowLayoutPanel.Controls.Add(pickPlaceControl);
   ```

3. **Start the station**: `StationBase` exposes `Start()` and `Stop()`. Call `Start()` when the operator presses the corresponding UI button.

---

## How to Write Tests for a Station

Tests for stations use **NUnit + Moq**. Because stations depend on signals (which come from I/O/PLC), you mock the signal infrastructure.

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
        // ExecuteCycle advances when WaitSignal succeeds.
        // In a real test you would inject a mock signal provider
        // that returns success for "Feeder_PartAtPick".
        Assert.That(_station.CurrentStep, Is.EqualTo("WaitForPartAtPick"));
    }

    [Test]
    public void WaitSignalTimeout_SetsAlarmAndGoesToIdle()
    {
        // Arrange: configure mock signal provider to always timeout.
        // Act: start station and pump ExecuteCycle.
        // Assert: alarm is set, step resets to Idle.
    }
}
```

Key testing principles:

- **Mock the signal provider** — your station should not depend on real I/O during tests. Inject a `Mock<ISignalProvider>` that you can configure per test case.
- **Test timeout behavior** — always test what happens when a `WaitSignal` times out. Timeouts are the most common failure mode in production.
- **Test the alarm path** — verify that `SetAlarm` is called with the expected alarm code when things go wrong.
- **Test the happy path first** — a single test that runs the full Idle → Complete cycle confirms the station logic is wired up correctly.
