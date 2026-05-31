# Code Patterns and Conventions

This document defines the coding patterns used throughout OmniFrame. Follow these when writing new code. They exist so that any developer can read any file and immediately understand the structure.

---

## Dependency Injection Registration

All registrations happen in `src/OmniFrame/DiConfigurator.cs` inside the `ConfigureServices` method (called by `Program.cs` at startup). Use **Microsoft.Extensions.DependencyInjection**.

| Lifetime | When to Use | Examples |
|---|---|---|
| **Singleton** | One instance for the entire application lifetime | `SystemManager`, `AlarmManager`, `StationBase` subclasses, `ConfigurationManager` |
| **Transient** | New instance every time it is requested | WinForms (`Form` subclasses), `UserControl` subclasses |
| **Scoped** | Rarely used in WinForms. Reserve for web-context plugins only. | (not typical) |

```csharp
// Correct — managers are singletons
services.AddSingleton<SystemManager>();
services.AddSingleton<IAlarmManager, AlarmManager>();

// Correct — forms are transient (WinForms creates them repeatedly)
services.AddTransient<MainForm>();
services.AddTransient<LoginForm>();

// Correct — register by interface when the interface exists
services.AddSingleton<IMotionController, RealMotionController>();

// Wrong — do NOT register a Form or UserControl as Singleton
services.AddSingleton<SomeForm>(); // Will cause state corruption on second open
```

---

## Manager Lifecycle

Every major manager follows this three-phase lifecycle, called by `SystemManager` at startup:

```csharp
public interface IManager
{
    void Initialize();  // Load config, open connections, validate prerequisites
    void Start();       // Begin active work (start timers, start listening, etc.)
    void Stop();        // Graceful shutdown (flush buffers, close connections)
}
```

| Phase | What Happens | Failures |
|---|---|---|
| `Initialize()` | Read configuration, open database/file/network connections, create internal state. | Throw a specific exception if a prerequisite is missing. The app will show an error dialog and refuse to start. |
| `Start()` | Start background threads, timers, begin polling. | Log the error and attempt to continue with degraded functionality. |
| `Stop()` | Cancel timers, flush buffers, close connections. Called on app exit. | Log and continue. Never throw from Stop — the app is shutting down. |

---

## Error Handling Pattern

Every `catch` block must follow this structure:

```csharp
try
{
    // operation
}
catch (TimeoutException ex)
{
    // Handle specific known exceptions with a targeted response.
    Logger.Error("Motion controller did not respond within timeout.", ex);
    SetAlarm("MotionTimeout", timeoutMs);
}
catch (IOException ex)
{
    // Another specific exception type.
    Logger.Error("I/O board communication failed.", ex);
    SetAlarm("IOCommFailure", 0);
}
catch (Exception ex)
{
    // Generic catch-all. Always log the full exception object, never ex.Message.
    Logger.Error("Unexpected error in station cycle.", ex);
    SetAlarm("UnexpectedError", 0);
}
```

**Rules:**

1. **Catch specific exceptions first**, generic `Exception` last.
2. **Always pass the exception object** to the logger — never `ex.Message` alone. Serilog captures the stack trace and inner exceptions from the object.
3. **Set an alarm** for any error the operator should see. Use a short, descriptive alarm code.
4. **Never swallow exceptions silently.** Even if you recover, log a `Warning`.

---

## Signal Naming Convention

All signals use the format **`DeviceName_Action`**:

| Signal | Meaning |
|---|---|
| `Robot1_AtPick` | Robot 1 has arrived at the pick position. |
| `Clamp1_Closed` | Clamp 1 cylinder is in the closed state. |
| `PlcConv1_Running` | PLC reports conveyor 1 motor is running. |
| `Feeder_HasPart` | The feeder sensor detects a part. |
| `Camera1_InspectionPass` | Vision camera reports inspection passed. |

**Rules:**

- `DeviceName` is the exact device name from the electrical schematic or PLC tag list.
- `Action` is a verb or state, in PascalCase.
- Use underscores to separate device from action.
- Do not abbreviate names — `Conveyor1`, not `Cnv1`.

Signal names are defined as constants in a `SignalNames` or `SignalConstants` class. Never hardcode a signal name as a string literal in station logic.

```csharp
// Correct
WaitSignal(SignalNames.FeederHasPart, true, TimeoutConstants.Medium);

// Wrong
WaitSignal("Feeder_HasPart", true, 5000);
```

---

## Timeout Constants

Never write a numeric timeout as a magic number. Use `TimeoutConstants`:

```csharp
public static class TimeoutConstants
{
    public const int Short   = 1000;   // 1 second — fast sensors, air cylinders
    public const int Medium  = 5000;   // 5 seconds — robot moves, PLC handshakes
    public const int Long    = 30000;  // 30 seconds — homing sequences, warm-up
    public const int Extreme = 120000; // 2 minutes — oven temperature ramp, etc.
}
```

If a station needs a very specific timeout (e.g., 3500ms for a particular pneumatic valve), add a named constant to the station class itself. Do not write `3500` inline.

---

## Logging

OmniFrame uses **Serilog** wrapped by a static `Logger` class in `Common`:

```csharp
// Informational — normal operation events
Logger.Info("Station {StationName} started cycle.", StationName);

// Warning — something unexpected but recoverable
Logger.Warning("Retry {RetryCount}/{MaxRetries} for signal {SignalName}.", retry, max, signal);

// Error — something failed. Always pass the Exception object.
Logger.Error(ex, "Failed to write to PLC register {Register} at {Address}.", register, address);
```

**Rules:**

1. **Always pass the full `Exception` object** to `Logger.Error`, not `ex.Message`.
2. **Use structured logging** — put variables in the message template with `{Name}` placeholders. Serilog captures them as properties.
3. **Do not log inside tight loops.** A `WaitSignal` retry loop should log once per second at most.
4. Log entry and exit of major operations (station start, cycle complete, alarm set/clear).

---

## Naming Conventions

| Element | Convention | Example |
|---|---|---|
| **Interface** | Prefixed with `I` | `IMotionController`, `IAlarmManager` |
| **Manager class** | Suffixed with `Manager` | `SystemManager`, `DatabaseManager` |
| **Station class** | Descriptive name ending with `Station` | `OHSLoadingStation`, `InspectionStation` |
| **Constants class** | Suffixed with `Constants` | `TimeoutConstants`, `SignalConstants` |
| **Form** | Suffixed with `Form` | `MainForm`, `LoginForm` |
| **UserControl** | Suffixed with `Control` | `StationControl`, `AlarmPanelControl` |
| **Enum members** | PascalCase | `WaitForPartAtPick` |
| **Private fields** | camelCase, no underscore prefix | `currentStep`, `logger` |
| **Method parameters** | camelCase | `signalName`, `timeoutMs` |
| **Local variables** | camelCase | `result`, `elapsed` |
