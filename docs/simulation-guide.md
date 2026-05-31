# Simulation Usage Guide

The simulation layer lets you develop and test OmniFrame on a plain Windows laptop without any physical hardware (no motion controllers, no I/O boards, no PLCs).

---

## What the Simulation Layer Is

The `OmniFrame.Simulation` project provides in-process software replacements for every hardware interface. When simulation mode is active:

- **Motion commands** (e.g., "move axis X to position 500") complete instantly and report success.
- **I/O reads** return the last written value (loopback behavior).
- **PLC reads/writes** hit an in-memory register table instead of a physical device.

This means you can run the full application, step through station state machines in the debugger, and verify UI behavior — all without a factory floor.

---

## The Four Simulation Classes

| Class | What It Simulates |
|---|---|
| **SimulatedMotion** | Motion controller axes (servo drives, steppers). Accepts move/stop/home commands. Reports target position as current position after a configurable delay. |
| **SimulatedIoCtrl** | Digital and analog I/O boards. Digital inputs mirror the last digital output written (loopback). Analog values are stored in memory. |
| **SimulatedPlcDevice** | A PLC with Modbus-compatible soft elements. Maintains an in-memory table of X (input), Y (output), M (relay), and D (data register) values. Responds to Modbus TCP/RTU read/write requests. |
| **SimulationContext** | A container that wires the three device simulators together. Provides a single `Initialize()/Start()/Stop()` lifecycle call that the DI container uses. |

---

## How to Configure the App for Simulation

Configuration is typically driven by an `app.config` or `appsettings.json` key:

```xml
<!-- In app.config -->
<add key="UseSimulation" value="true" />
```

```json
// In appsettings.json
{
  "UseSimulation": true
}
```

In `Program.cs`, the DI registration checks this flag:

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

Set `UseSimulation` to `false` when running against real hardware.

---

## How to Add a New Simulated Device

Suppose you need to simulate a barcode scanner that writes strings over a serial port.

### Step 1: Create the class

Create a new file in `src/OmniFrame.Simulation/`:

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
        // Rotate through predefined barcodes
        string result = _predefinedScans[_index];
        _index = (_index + 1) % _predefinedScans.Count;
        return result;
    }

    public void Initialize() { }
    public void Start() { }
    public void Stop() { }
}
```

### Step 2: Register in the SimulationContext

Add the new simulator to `SimulationContext` so it is created and managed alongside the existing ones:

```csharp
public class SimulationContext
{
    public SimulatedBarcodeScanner BarcodeScanner { get; private set; }

    public void Initialize()
    {
        // ... existing initializers ...
        BarcodeScanner = new SimulatedBarcodeScanner();
        BarcodeScanner.Initialize();
    }
}
```

### Step 3: Register in DI

In `Program.cs`, add the registration under the simulation branch:

```csharp
if (configuration.UseSimulation)
{
    // ... existing registrations ...
    services.AddSingleton<IBarcodeScanner>(sp =>
        sp.GetRequiredService<SimulationContext>().BarcodeScanner);
}
```

### Step 4: Test it

Write a test in `OmniFrame.Tests` that instantiates `SimulatedBarcodeScanner` directly and verifies its behavior:

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

## Limitations

| Limitation | Detail |
|---|---|
| **Timing is approximate** | Real motion takes seconds; simulated motion reports completion in milliseconds. Do not rely on simulation timing for cycle-time calculations. |
| **No electrical signal fidelity** | Simulation cannot detect wiring faults, loose connectors, or EMI. These must be tested on real hardware. |
| **No vendor-specific edge cases** | Real PLCs have quirks (byte ordering, register alignment, connection drops). Simulation returns clean, ideal responses. |
| **Serial port simulation is in-memory** | The simulated IO controller does not open real COM ports. If you need to test serial communication to a real external device, use a physical serial port. |
| **Single-instance only** | The simulation layer is designed for one developer on one machine. It does not simulate a multi-controller factory network. |

---

## Quick Verification

After setting `UseSimulation = true` and launching the app:

1. Log in with `admin` / `admin123`.
2. Navigate to any station detail view.
3. The station should progress through its steps without real hardware.
4. Signal indicators in the UI should toggle as simulated I/O values change.
5. If a station is stuck at a `WaitSignal` call, check that the signal name matches what the simulator is supplying.
