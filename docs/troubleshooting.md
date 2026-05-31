# Troubleshooting FAQ

Common problems encountered when building, running, or debugging OmniFrame, and how to fix them.

---

## Build Issues

### Build fails with NuGet package restore errors

**Symptoms:** `NU1101: Unable to find package ...`, or `Package ... is not found on source ...`.

**Causes:**
- Corporate network blocks NuGet.org.
- NuGet cache is corrupted.
- NuGet.config has an incorrect source URL.

**Fix:**

```powershell
# 1. Check what sources are configured
dotnet nuget list source

# 2. Add nuget.org if missing
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org

# 3. Clear the local cache and force re-restore
dotnet nuget locals all --clear
dotnet restore src/OmniFrame.sln --force
```

If behind a corporate proxy, add the proxy to `nuget.config`:

```xml
<configuration>
  <config>
    <add key="http_proxy" value="http://proxy.company.com:8080" />
    <add key="https_proxy" value="http://proxy.company.com:8080" />
  </config>
</configuration>
```

---

### "IsExternalInit" compile error (CS0518)

**Symptoms:** `Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported.`

**Cause:** This C# 9.0 feature requires a polyfill on .NET Framework 4.8. The compiler expects `IsExternalInit` to exist in the runtime, but .NET Framework 4.8 does not provide it.

**Fix:** The OmniFrame solution includes a polyfill in `Common/IsExternalInit.cs`. Verify this file exists:

```csharp
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
```

If the file is missing, re-create it in the `Common` project. This is a standard [official Microsoft polyfill](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/init).

---

### "DLL not found" for motion control libraries

**Symptoms:** `System.DllNotFoundException: Unable to load DLL 'xxx_motion_api.dll'.`

**Cause:** Native motion controller driver DLLs are not in the application's working directory or PATH.

**Fix:**
1. Locate the native DLLs in the `lib/` directory at the repository root.
2. Copy them to the output directory `src/OmniFrame/bin/Debug/` (or `Release/`).
3. Alternatively, add a post-build event to the `.csproj`:

```xml
<ItemGroup>
  <Content Include="..\..\lib\*.dll">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

4. Clean and rebuild.

---

## Runtime Issues

### PLC connection timeout

**Symptoms:** `SocketException: A connection attempt failed` or station hangs at a `WaitSignal` for a PLC signal.

**Checklist:**

1. **Ping the PLC IP address** from the Windows machine:
   ```powershell
   ping 192.168.1.100
   ```
   If ping fails, the physical network is not connected.

2. **Verify the IP and port** in the configuration file match the PLC's settings. Modbus TCP default port is 502.

3. **Check the Windows firewall.** It may be blocking outbound TCP to the PLC's port. Temporarily disable the firewall to test:
   ```powershell
   # Test only — re-enable after
   netsh advfirewall set allprofiles state off
   ```

4. **Check the physical cable.** Ensure the Ethernet cable is plugged in and link lights are blinking on both ends.

5. **Try telnet to the PLC port:**
   ```powershell
   telnet 192.168.1.100 502
   ```
   If the connection is refused, the PLC is not listening on that port or a firewall is blocking it.

---

### OPC DA initialization fails

**Symptoms:** Exception mentioning `OPCServer`, `IOPCServer`, or `Class not registered (HRESULT: 0x80040154)`.

**Cause:** OPC DA uses COM/DCOM. The OPC server's COM component must be registered on the Windows machine, and DCOM permissions must grant access.

**Fix:**

1. **Run Visual Studio as Administrator.** OPC DA requires admin privileges for COM registration.
2. **Verify the OPC server is installed** on the machine. Check Programs and Features for the vendor's OPC server.
3. **Register the COM component** (check the vendor's documentation for the DLL name):
   ```powershell
   regsvr32 "C:\Path\To\Vendor\OPCServer.dll"
   ```
4. **Configure DCOM permissions** via `dcomcnfg.exe` — the user account running the app needs Launch and Access permissions on the OPC server component.

If you do not need real OPC DA, set `UseSimulation = true` to bypass OPC entirely.

---

### SQLite database locked

**Symptoms:** `SQLiteException: database is locked` when writing.

**Cause:** Another process has the SQLite file open with a write lock. This commonly happens when:
- Another instance of OmniFrame is running.
- A database browser tool (DB Browser for SQLite, DBeaver) has the file open.
- The previous run did not cleanly close the connection.

**Fix:**
1. Close all other instances of OmniFrame.
2. Close any database tools that have the `.db` file open.
3. If the lock persists after all instances are closed, a stale WAL (Write-Ahead Log) or journal file may exist. Delete `database.db-wal` and `database.db-shm` files from the data directory, then restart.
4. If deploying on a network share, move the SQLite file to a local drive. SQLite does not reliably handle concurrent access over SMB.

---

### WebSocket port already in use

**Symptoms:** `System.Net.Sockets.SocketException: Only one usage of each socket address is normally permitted.` when starting the WebSocket server.

**Cause:** Another process is already bound to port 8080 (WebSocket) or 8081 (REST API).

**Fix:**

```powershell
# Find the process using port 8080
netstat -ano | findstr :8080

# Example output: TCP 0.0.0.0:8080 0.0.0.0:0 LISTENING 12345
# Kill the process by PID (replace 12345 with the actual PID)
taskkill /PID 12345 /F
```

To change the ports permanently, edit the configuration file:

```json
{
  "RemoteMonitor": {
    "WebSocketPort": 8080,
    "RestApiPort": 8081
  }
}
```

Change to unused ports (e.g., 9090 for WebSocket, 9091 for REST).

---

### Form not showing after login

**Symptoms:** Login succeeds but the main form never appears, or the application exits silently.

**Causes:**
- DI registration is missing for `MainForm` or a dependency it needs.
- An unhandled exception in `MainForm`'s constructor or `Load` event crashes the application.

**Fix:**

1. Open `Program.cs` and verify `MainForm` is registered:
   ```csharp
   services.AddTransient<MainForm>();
   ```

2. Check that all of `MainForm`'s constructor parameters are also registered. If `MainForm` takes an `ISystemManager`, then `SystemManager` must be registered as `ISystemManager`.

3. Run with debugging and enable **break on all CLR exceptions**:
   - In Visual Studio: Debug → Windows → Exception Settings → Check "Common Language Runtime Exceptions".
   - This will break on the exact line that throws, including inside constructors.

4. Check the Serilog log file (typically `logs/app-YYYYMMDD.log`) for an exception logged just before the crash.

---

## General Debugging Tips

| Scenario | Approach |
|---|---|
| Station stuck on a step | Attach the debugger, pause, and inspect `currentStep` and which `WaitSignal` is blocking. Check if the expected signal is being set by the I/O or PLC. |
| UI freezes | A station's `ExecuteCycle` is running synchronously on the UI thread (instead of a background thread). Check that the station loop runs on a `Task` or `BackgroundWorker`. |
| Memory grows over time | Look for event handler leaks — forms that subscribe to manager events but never unsubscribe on close. Use a memory profiler (Visual Studio Diagnostic Tools). |
| Intermittent COM port failure | USB-to-serial adapters can disappear from the device tree. Check Device Manager for yellow exclamation marks. Try a different USB port or adapter. |
| Config changes not taking effect | The app reads config at startup only. Restart after any config change. Check that the correct config file is being read (look in `bin/Debug/` for the deployed copy). |
