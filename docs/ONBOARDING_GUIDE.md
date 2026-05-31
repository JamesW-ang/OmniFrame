# 🎓 OmniFrame 新人训练营（Day 0 → Week 3）

> **目标读者：** 刚毕业大学生。学过 C# 基础。
> **不需要：** 工业自动化经验、WPF 经验。
> **前置：** [WPF_BASICS.md](WPF_BASICS.md) 读完（1 小时）。

---

## 📅 路线图

```
Day 0  读 WPF_BASICS.md — 理解 XAML/绑定/MVVM/DataTemplate

Week 1  Day 1   跑起来 + 认识项目结构
        Day 2   理解 DI + MVVM（最重要的概念）
        Day 3   用断点跟踪：按钮 → ViewModel → Manager → 硬件
        Day 4   动手：给 BlockCutViewModel 加一个属性
        Day 5   写你的第一个 WPF 测试

Week 2  深入各层（通信/运动/数据）
Week 3  独立完成小需求，提交 PR
```

---

## Day 0：读懂 WPF_BASICS.md（1 小时）

> 🎯 搞懂 XAML 是什么、绑定怎么工作、为什么 MVVM 分开 View 和 ViewModel。

**读完后你应该能回答：**
- XAML 文件和 .cs 文件是什么关系？
- `{Binding Status}` 这行字做了什么？
- `DataTemplate` 为什么不写 switch-case 就能切换界面？

---

## Day 1：跑起来 + 认识项目结构

> 🎯 编译 → 登录 → 看到主界面

### 1.1 编译运行

```powershell
cd OmniFrame
dotnet restore src/OmniFrame.Wpf/OmniFrame.Wpf.csproj
dotnet build src/OmniFrame.Wpf/OmniFrame.Wpf.csproj
dotnet run --project src/OmniFrame.Wpf/OmniFrame.Wpf.csproj
```

登录：`admin / admin123`。

### 1.2 理解启动流程

```
App.xaml.cs → Application_Startup()
  ① new ServiceCollection()
  ② DiConfigurator.ConfigureServices()  ← 注册所有 Manager + ViewModel
  ③ BuildServiceProvider()               ← 构建 DI 容器
  ④ 启动基础设施（ConfigManager, ReconnectionService, Watchdog）
  ⑤ 注册 BlockCut 6 个工站
  ⑥ 显示 LoginWindow
  ⑦ 登录成功 → 显示 MainWindow
```

### 1.3 打开三个核心文件

按顺序双击打开：

| 顺序 | 文件 | 看什么 |
|:---|:---|:---|
| 1 | `src/OmniFrame.Wpf/DiConfigurator.cs` | DI 容器：所有服务在这里注册 |
| 2 | `src/OmniFrame.Wpf/ViewModels/MainViewModel.cs` | 主窗口的数据和命令 |
| 3 | `src/OmniFrame.Wpf/Views/MainWindow.xaml` | 主窗口的 XAML 界面 |

### ✅ Day 1 自检

- [ ] 编译运行成功，看到登录页面，登录成功
- [ ] 能在解决方案资源管理器里找到 `ViewModels/` 和 `Views/`
- [ ] 能说出 DiConfigurator.cs 是干什么的

---

## Day 2：理解 DI + MVVM

> 🎯 理解「为什么不直接 new」、「为什么界面和逻辑要分开」

### 2.1 看一个完整的例子

打开 `BlockCutViewModel.cs` 和 `BlockCutMainView.xaml`：

```csharp
// BlockCutViewModel.cs — 数据 + 命令
public class BlockCutViewModel : ViewModelBase
{
    private readonly StationCoordinator _coordinator;
    public ICommand StartCommand { get; }           // ← 命令
    public ObservableCollection<StationStatusItem> Stations { get; } // ← 数据

    public BlockCutViewModel(StationCoordinator coordinator, ...)
    {
        _coordinator = coordinator;                // ← DI 注入
        StartCommand = new RelayCommand(StartAll);  // ← 命令绑定方法
    }

    private void StartAll() { _coordinator.StartAll(); }
}
```

```xml
<!-- BlockCutMainView.xaml — 界面 -->
<Button Content="▶ 启动" Command="{Binding StartCommand}"/>
<ListBox ItemsSource="{Binding Stations}">
    <!-- 自动渲染每个工站状态 -->
</ListBox>
```

**数据流：** `StationCoordinator.StartAll()` → ViewModel 属性变化 → XAML 自动刷新。**XAML 没有一行 if/for。**

### 2.2 DI 的核心思想

```csharp
// ❌ 坏写法
public class BadViewModel
{
    public void Start()
    {
        var coordinator = new StationCoordinator(...);  // ← 自己 new
        coordinator.StartAll();
    }
}

// ✅ 好写法
public class GoodViewModel
{
    private readonly StationCoordinator _coordinator;

    public GoodViewModel(StationCoordinator coordinator)  // ← 别人传进来
    {
        _coordinator = coordinator;
    }
}
```

**好处：** 如果换成仿真硬件，只要 DI 容器改注册一行。ViewModel 不用改。

### ✅ Day 2 自检

- [ ] 能在 BlockCutViewModel 里找到 StartCommand 的定义
- [ ] 能解释 `{Binding StartCommand}` 是怎么把按钮和代码连起来的
- [ ] 能说出 DI 为什么比 `new` 好

---

## Day 3：用断点跟踪完整链路

> 🎯 从按钮点击到硬件调用，走完一整条路

### 3.1 打 4 个断点

```
① BlockCutViewModel.cs → StartAll() 方法
② StationCoordinator.cs → StartAll() 方法
③ Station_Load.cs → RunAsync() 方法
④ SimulatedHardware.cs → MoveAbs() 或类似方法
```

F5 启动 → 点击 BlockCut 界面的「启动」按钮 → 观察每个断点命中。

### 3.2 画出调用链

```
Button.Click
  → BlockCutViewModel.StartAll()
    → StationCoordinator.StartAll()
      → Task.Run(() → Station_Load.RunAsync(token))
        → MoveAbsAsync()        ← 告诉轴移动
        → WaitDIAsync()         ← 等待传感器
        → SetDOAsync()          ← 控制气缸
```

**能画出这张图 = 理解了项目的 60%。**

---

## Day 4：加一个属性

> 🎯 在 BlockCutViewModel 加一个属性，在 XAML 显示出来

```csharp
// BlockCutViewModel.cs — 加这个属性
private int _cycleCount;
public int CycleCount { get => _cycleCount; set => Set(ref _cycleCount, value); }
```

```xml
<!-- BlockCutMainView.xaml — 加这行显示 -->
<TextBlock Text="{Binding CycleCount, StringFormat='循环次数: {0}'}"
           Foreground="{StaticResource AccentBrush}" FontSize="14"/>
```

F5 → 看到新属性显示。

### ✅ Day 4 自检

- [ ] 编译通过
- [ ] F5 后在界面上看到你加的属性

---

## Day 5：写一个测试

```csharp
[Test]
public void StartCommand_Execute_ShouldCallCoordinatorStartAll()
{
    // Arrange
    var vm = new BlockCutViewModel(coordinator, alarmMgr, systemMgr, cfg, uphMgr);

    // Act
    vm.StartCommand.Execute(null);

    // Assert
    Assert.That(vm.IsRunning, Is.True);
}
```

右键 → 运行测试 → 绿色 = 通过。

---

## Week 2：深入各层

### PLC 通信
打开 `src/Plc/PlcDevice.cs` → 理解模板方法模式。
动手：在仿真下读一个 Modbus 寄存器（用 `IPlcManager`）。

### DLL 接入实战
**新人必读** [NATIVE_DLL_GUIDE.md](NATIVE_DLL_GUIDE.md) — 雷赛/海康 DLL 接入 + x86/x64 排坑 + DllImport 教程。

### 运动控制
打开 `src/MotionIO/Motion.cs` → 理解抽象基类。
动手：让仿真轴做一个点位运动（用 `IMotionManager`）。

### 数据层
打开 `src/DataAccess/SqliteHelper.cs` → Dapper ORM。
动手：查询一条产品记录（用 `IDataManager.QueryProducts()`）。

---

## ❓ 新人 FAQ

### 1. XAML 文件里 `{Binding}` 报错「找不到属性」？
检查 ViewModel 的 `DataContext` 是否正确设置。在 Window 构造器里 `DataContext = viewModel;`。

### 2. 修改 ViewModel 属性但界面不刷新？
检查是否调用了 `Set()` 或 `OnPropertyChanged()`。直接赋值字段不会触发刷新。

### 3. 为什么我的按钮点了没反应？
检查 Command 绑定是否正确，`CanExecute` 是否返回 true。

### 4. 代码太多从哪开始？
只打开这 4 个文件：`DiConfigurator.cs`、`MainViewModel.cs`、`MainWindow.xaml`、`BlockCutViewModel.cs`。其他先不看。

### 5. 仿真和真实硬件区别？
逻辑完全一样。只是仿真硬件返回假数据，真硬件返回传感器值。

---

## 📚 文档索引

| 你需要 | 读这个 |
|:---|:---|
| WPF 基础（必读） | [WPF_BASICS.md](WPF_BASICS.md) |
| 5 分钟跑起来 | [quickstart_zh.md](quickstart_zh.md) |
| 架构决策 | [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) |
| 编码规范 | [code-patterns_zh.md](code-patterns_zh.md) |
| 创建工站 | [adding-new-station_zh.md](adding-new-station_zh.md) |
| 术语表 | [glossary_zh.md](glossary_zh.md) |
| 排错 | [troubleshooting_zh.md](troubleshooting_zh.md) |
