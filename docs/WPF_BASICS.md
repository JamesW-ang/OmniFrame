# 🎓 WPF 零基础教程 — OmniFrame 专版

> **目标读者：** 刚毕业大学生，学过 C# 基础，**从未用过 WPF**。
> **学习时间：** 2 小时读完 + 动手跟着写。
> **为什么写这个：** WPF 的名词太多（XAML、DataBinding、MVVM、DataTemplate...），本文只讲 OmniFrame 项目实际用到的部分，并且用你学过的 C# 概念类比。

---

## 目录

1. [WPF 是什么？和 WinForms 有什么区别？](#1-wpf-是什么和-winforms-有什么区别)
2. [XAML：用标签写界面](#2-xaml用标签写界面)
3. [DataBinding：自动同步数据和界面](#3-databinding自动同步数据和界面)
4. [MVVM：把界面和逻辑分开](#4-mvvm把界面和逻辑分开)
5. [INotifyPropertyChanged：告诉界面「我变了」](#5-inotifypropertychanged告诉界面我变了)
6. [ObservableCollection：列表自动刷新](#6-observablecollection列表自动刷新)
7. [ICommand / RelayCommand：按钮点击的优雅写法](#7-icommand--relaycommand按钮点击的优雅写法)
8. [DataTemplate：根据类型自动选界面](#8-datatemplate根据类型自动选界面)
9. [Style / DataTrigger：不用代码改样式](#9-style--datatrigger不用代码改样式)
10. [项目里如何添加引用（动态库 vs 静态库）](#10-项目里如何添加引用动态库-vs-静态库)
11. [如何运行 WPF 版本](#11-如何运行-wpf-版本)

---

## 1. WPF 是什么？和 WinForms 有什么区别？

### 一句话

**WinForms = 用 C# 代码画界面。WPF = 用 XAML（类似 HTML）画界面，用 C# 写逻辑。**

### 类比

| 你学过的 | WPF 里的对应物 |
|:---|:---|
| HTML（描述页面结构） | XAML（描述界面结构） |
| CSS（描述样式） | Style / DataTrigger（描述样式） |
| JavaScript（行为逻辑） | C# / ViewModel（行为逻辑） |
| `document.getElementById()` | `{Binding PropertyName}`（数据绑定） |

### 一个最简单的 WPF 窗口

```xml
<!-- MainWindow.xaml — 界面描述文件 -->
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        Height="300" Width="400">
    <Grid>
        <Button Content="点我" Width="100" Height="40"
                Click="Button_Click"/>
        <TextBlock x:Name="txtMessage" Text="Hello" Margin="0,60,0,0"/>
    </Grid>
</Window>
```

```csharp
// MainWindow.xaml.cs — 逻辑代码（和 XAML 配对）
private void Button_Click(object sender, RoutedEventArgs e)
{
    txtMessage.Text = "你点了我！";
}
```

**理解：** XAML 文件和 `.cs` 文件是一对。XAML 画界面，`.cs` 写点击后干什么。

### WPF 项目的文件结构

```
OmniFrame.Wpf/
├── App.xaml / App.xaml.cs       ← 程序入口（相当于 Program.cs）
├── Themes/DarkTheme.xaml         ← 全局样式（颜色、字体）
├── ViewModels/                   ← 逻辑层（数据和命令）
│   ├── ViewModelBase.cs          ← 基类
│   ├── BlockCutViewModel.cs      ← BlockCut 界面的数据
│   └── ...
└── Views/                        ← 界面层（XAML）
    ├── MainWindow.xaml           ← 主窗口
    ├── BlockCutMainView.xaml     ← BlockCut 生产界面
    └── ...
```

**规则：** 每个界面 = 一个 `View/XxxView.xaml` + 一个 `ViewModels/XxxViewModel.cs`。

---

## 2. XAML：用标签写界面

### 基础语法

```xml
<!-- 每个 XML 标签对应一个 C# 类 -->
<Button />              ← 对应 new Button()

<!-- 属性 = XML 属性 -->
<Button Content="启动" Width="100" Height="40" />

<!-- 嵌套 = 父子关系 -->
<Grid>
    <Button Content="启动" />    ← 这个按钮在 Grid 里面
</Grid>

<!-- 布局容器 -->
<Grid>                          ← 表格布局（行/列）
<StackPanel>                    ← 垂直或水平堆叠
<Border>                        ← 带边框的容器
<UniformGrid>                   ← 等分网格
```

### Grid 的行列定义

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="48"/>      <!-- 第 0 行: 固定 48 像素 -->
        <RowDefinition Height="*"/>       <!-- 第 1 行: 占满剩余空间 -->
        <RowDefinition Height="28"/>      <!-- 第 2 行: 固定 28 像素 -->
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="220"/>   <!-- 第 0 列: 固定 220 -->
        <ColumnDefinition Width="*"/>     <!-- 第 1 列: 占满 -->
    </Grid.ColumnDefinitions>

    <!-- 放在第 0 行，跨 2 列 -->
    <Border Grid.Row="0" Grid.ColumnSpan="2" Background="DarkBlue"/>

    <!-- 放在第 1 行，第 0 列 -->
    <ListBox Grid.Row="1" Grid.Column="0"/>

    <!-- 放在第 1 行，第 1 列 -->
    <ContentControl Grid.Row="1" Grid.Column="1"/>
</Grid>
```

**`*` 是什么意思？** 比例分配。`2*` 和 `*` 表示 2:1 分剩余空间。和 HTML 的 `fr` 单位一样。

---

## 3. DataBinding：自动同步数据和界面

**这是 WPF 最核心的概念。** 理解了就理解了 WPF 的 80%。

### 不用绑定的写法（你熟悉的）

```csharp
// WinForms 方式: 手动更新
txtName.Text = user.Name;
user.Name = "新名字";
txtName.Text = user.Name;  // ← 必须手动再写一次！
```

### 用绑定的写法

```xml
<!-- XAML: 声明"这个文本框永远显示 Name 属性" -->
<TextBox Text="{Binding Name}" />
```

```csharp
// C# ViewModel: 修改属性
public class UserViewModel : ViewModelBase
{
    private string _name = "张三";
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();  // ← 告诉界面「我变了，你刷新」
        }
    }
}

// 界面自动更新，不需要手动写 txtName.Text = ...
user.Name = "李四";  // 文本框自动显示"李四"
```

### 绑定方向

| 写法 | 含义 |
|:---|:---|
| `{Binding Name}` | 单向：ViewModel → View |
| `{Binding Name, Mode=TwoWay}` | 双向：ViewModel ↔ View（输入框用） |
| `{Binding Name, UpdateSourceTrigger=PropertyChanged}` | 每敲一个字就更新（默认失焦才更新） |

### 绑定到命令

```xml
<Button Content="启动" Command="{Binding StartCommand}" />
```

```csharp
public ICommand StartCommand { get; }  // ViewModel 提供命令对象
```

**理解：** 绑定 = 一根看不见的线，把 ViewModel 的属性/命令和界面连起来。你改 ViewModel，界面自动变。不需要手动操作控件。

---

## 4. MVVM：把界面和逻辑分开

MVVM = Model-View-ViewModel，是 WPF 的标准架构。

```
┌──────────┐     绑定      ┌──────────────┐     调用     ┌──────────┐
│  View    │ ←──────────→  │  ViewModel   │ ──────────→  │  Model   │
│ (XAML)   │   自动同步     │  (C# 类)     │              │ (Manager)│
└──────────┘               └──────────────┘              └──────────┘
   界面                       数据+命令                    业务逻辑
```

### 在 OmniFrame 里长什么样

```
View (XAML)                ViewModel (C#)               Model (Core)
────────────────────────────────────────────────────────────────────
BlockCutMainView.xaml  ←→  BlockCutViewModel       →  StationCoordinator
                              .Stations                IAlarmManager
                              .StartCommand            BlockCutConfig
                              .IsRunning
```

**规则：**
- **View** 只管「长什么样」（XAML 标签）。**绝不写 if/for/调用 Manager。**
- **ViewModel** 管「有什么数据、能做什么操作」（属性 + 命令）。通过 DI 拿到 Manager。
- **Model** 是原来的 Manager/工站，不改。

### 为什么要这样分？

假设明天你要把 WPF 换成网页版（Blazor/Avalonia）。你只需要重写 View（XAML → HTML），ViewModel 和 Model 一行不动。

---

## 5. INotifyPropertyChanged：告诉界面「我变了」

```csharp
// 这是 ViewModelBase.cs 的核心代码
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    // 通知界面：「名为 name 的属性变了，请刷新」
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // 简化写法：如果值真的变了，更新字段 + 通知
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;  // 值没变，不通知
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
```

### 使用

```csharp
public class MyViewModel : ViewModelBase
{
    private string _status = "空闲";
    public string Status
    {
        get => _status;
        set => Set(ref _status, value);  // ← 一行搞定：更新 + 通知
    }
}
```

**`[CallerMemberName]` 是什么？** 编译器自动填入调用者的名字。你写 `Set(ref _status, value)`，编译器自动补充 `name = "Status"`。

---

## 6. ObservableCollection：列表自动刷新

普通 `List<T>` 不会通知界面刷新。`ObservableCollection<T>` 会。

```csharp
// ❌ 不会刷新
public List<string> Items { get; } = new List<string>();

// ✅ 会自动刷新
public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();

// 添加/删除 → 界面自动更新
Items.Add("新项");    // 列表立刻显示新项
Items.RemoveAt(0);    // 列表立刻移除第一项
```

**原理：** `ObservableCollection` 实现了 `INotifyCollectionChanged` 接口。当你 Add/Remove 时，它触发事件，WPF 的 `ItemsControl` 监听到后自动重绘。

---

## 7. ICommand / RelayCommand：按钮点击的优雅写法

### 不用 Command（坏写法）

```csharp
// 在 View 的 .xaml.cs 里写
private void Button_Click(object sender, RoutedEventArgs e)
{
    // 业务逻辑写在这里 ← 坏！
    _coordinator.StartAll();
}
```

### 用 Command（好写法）

```csharp
// 在 ViewModel 里
public ICommand StartCommand { get; }

public MyViewModel()
{
    StartCommand = new RelayCommand(StartAll, () => !IsRunning);
    //                               ↑ 执行什么    ↑ 什么时候可点
}

private void StartAll() { _coordinator.StartAll(); }
```

```xml
<!-- XAML 绑定 -->
<Button Content="启动" Command="{Binding StartCommand}" />
```

**RelayCommand 的实现（你不需要改它）：**

```csharp
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object parameter) => _execute();
}
```

**好处：**
- 按钮能不能点由 `CanExecute` 决定（自动灰掉）
- 逻辑在 ViewModel 里，不污染 XAML
- 测试可以直接调 `StartCommand.Execute(null)`

---

## 8. DataTemplate：根据类型自动选界面

这是 OmniFrame 导航系统的核心。不需要写 `switch-case`：

```xml
<!-- MainWindow.xaml -->
<ContentControl Content="{Binding CurrentView}">
    <ContentControl.Resources>
        <!-- 当 CurrentView 是 BlockCutViewModel 时，自动显示 BlockCutMainView -->
        <DataTemplate DataType="{x:Type vm:BlockCutViewModel}">
            <local:BlockCutMainView/>
        </DataTemplate>

        <!-- 当 CurrentView 是 OeeViewModel 时，自动显示 OeeView -->
        <DataTemplate DataType="{x:Type vm:OeeViewModel}">
            <local:OeeView/>
        </DataTemplate>
    </ContentControl.Resources>
</ContentControl>
```

**原理：** WPF 看到 `CurrentView` 的类型，在 `Resources` 里找匹配的 `DataTemplate`，自动实例化对应的 View。

```csharp
// ViewModel 切换页面只需要改一个属性：
CurrentView = new BlockCutViewModel(...);  // → 自动显示 BlockCutMainView
CurrentView = new OeeViewModel(...);       // → 自动显示 OeeView
```

---

## 9. Style / DataTrigger：不用代码改样式

### Style：定义控件默认外观

```xml
<Style TargetType="Button">
    <Setter Property="Background" Value="#0078D4"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="FontSize" Value="13"/>
    <Setter Property="Cursor" Value="Hand"/>
</Style>
```

这个 Style 定义后，项目中**所有 Button** 自动应用这个外观。不需要在每个 Button 上重复写属性。

### DataTrigger：根据数据自动变样式

```xml
<!-- 轴状态指示灯 -->
<Ellipse Width="10" Height="10">
    <Ellipse.Style>
        <Style TargetType="Ellipse">
            <Setter Property="Fill" Value="Gray"/>   <!-- 默认灰色 -->
            <Style.Triggers>
                <!-- 如果 IsRunning = true → 绿色 -->
                <DataTrigger Binding="{Binding IsRunning}" Value="True">
                    <Setter Property="Fill" Value="Lime"/>
                </DataTrigger>
                <!-- 如果 IsPaused = true → 橙色 -->
                <DataTrigger Binding="{Binding IsPaused}" Value="True">
                    <Setter Property="Fill" Value="Orange"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Ellipse.Style>
</Ellipse>
```

**零行 C# 代码**实现指示灯变色。WinForms 需要 `_timer.Tick` + `if/else` + 手动改颜色。

### StaticResource：定义可复用常量

```xml
<!-- DarkTheme.xaml: 定义颜色 -->
<Color x:Key="RunColor">#4CAF50</Color>
<SolidColorBrush x:Key="RunBrush" Color="{StaticResource RunColor}"/>

<!-- 任何地方使用 -->
<Button Background="{StaticResource RunBrush}"/>
```

类似 C# 的 `const` 或 `static readonly`。改了定义，所有引用自动更新。

---

## 10. 项目里如何添加引用（动态库 vs 静态库）

### 概念

| 类型 | 后缀 | 什么时候用 | 例子 |
|:---|:---|:---|:---|
| **项目引用** | `.csproj` | 同一个 solution 里的项目 | `OmniFrame.Core.csproj` |
| **NuGet 包** | `.nupkg` | 第三方库 | `Microsoft.Extensions.DependencyInjection` |
| **动态库 (DLL)** | `.dll` | 厂商提供的硬件驱动 | `GTS800.dll`（固高运动控制卡） |
| **静态库** | `.lib` | C++ 编译产物 | 本项目不使用 |

> 📖 **完整的 DLL 接入教程（含雷赛/海康实战 + x86/x64 排坑 + 调试）：** [NATIVE_DLL_GUIDE.md](NATIVE_DLL_GUIDE.md)

### 在 Visual Studio 里怎么加

```
① 项目引用:
   右键「引用」→「添加引用」→ 勾选同一 solution 的项目

② NuGet 包:
   右键「引用」→「管理 NuGet 包」→ 搜索 → 安装

③ 动态库 (DLL):
   方式 A: 右键「引用」→「添加引用」→「浏览」→ 选择 .dll 文件
   方式 B: 把 .dll 放在项目根目录，在 .csproj 里加:
     <Reference Include="GTS800">
       <HintPath>.\GTS800.dll</HintPath>
     </Reference>
```

### OmniFrame.Wpf.csproj 里实际长什么样

```xml
<!-- 项目引用 -->
<ProjectReference Include="..\OmniFrame.Core\OmniFrame.Core.csproj" />
<ProjectReference Include="..\Common\Common.csproj" />
<ProjectReference Include="..\MotionIO\MotionIO.csproj" />

<!-- NuGet 包引用 -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="6.0.2" />
```

**理解：** `ProjectReference` = 引用自己写的项目。`PackageReference` = 引用别人写的包。

### 为什么动态库要区分 x86/x64

硬件 DLL（如固高 GTS800.dll）是 C/C++ 编译的本地代码，分 32 位和 64 位版本。你的 C# 程序如果是 64 位，必须引用 64 位的 DLL，否则运行时报 `BadImageFormatException`。

---

## 11. 如何运行 WPF 版本

### 环境要求

| 组件 | 要求 |
|:---|:---|
| Windows | 10/11 x64 |
| .NET Framework 4.8 | Developer Pack |
| Visual Studio 2022 | Community（免费） |
| 环境变量 | `OMNIFRAME_MES_AES_KEY` 等（仿真模式用默认值） |

### 步骤

```powershell
# 1. 设置环境变量（仅首次）
[Environment]::SetEnvironmentVariable("OMNIFRAME_MES_AES_KEY", "DefaultSimulationKey123", "User")
[Environment]::SetEnvironmentVariable("OMNIFRAME_BARCODE_HOST", "127.0.0.1", "User")
[Environment]::SetEnvironmentVariable("OMNIFRAME_BARCODE_PORT", "5000", "User")
[Environment]::SetEnvironmentVariable("OMNIFRAME_CONFIG_ENCRYPTION_KEY", "dev-key-placeholder", "User")

# 2. 还原 NuGet 包
cd OmniFrame
dotnet restore src/OmniFrame.Wpf/OmniFrame.Wpf.csproj

# 3. 编译
dotnet build src/OmniFrame.Wpf/OmniFrame.Wpf.csproj

# 4. 运行（VS 中按 F5，或命令行）
dotnet run --project src/OmniFrame.Wpf/OmniFrame.Wpf.csproj
```

### 启动流程

```
App.xaml.cs → Application_Startup()
  ├── 创建 DI 容器 → DiConfigurator.ConfigureServices()
  ├── 初始化基础设施（ConfigManager, ReconnectionService, Watchdog）
  ├── 注册 BlockCut 工站
  ├── 显示 LoginWindow（登录窗口）
  └── 显示 MainWindow（主窗口，左侧导航 + 右侧内容）
```

### 仿真模式 vs 真实硬件

默认启动即仿真模式（不需要连接任何硬件）。`BlockCutConfig.IsSimulation = true` → `DiConfigurator` 注入 `SimulatedHardware`。

要切换到真实硬件，修改 `Config/BlockCut.xml` 中 `IsSimulation` 为 `false`，并确保硬件 DLL 在输出目录中。

---

## 📚 进一步学习

| 你想学 | 去哪看 |
|:---|:---|
| WPF 官方文档 | [Microsoft Learn — WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/) |
| XAML 语法大全 | [XAML Overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/xaml/) |
| DataBinding 详解 | [Data Binding Overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/) |
| MVVM 深入 | 搜 "MVVM pattern WPF" |
| OmniFrame 项目架构 | [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) |
| 新人学习路径 | [ONBOARDING_GUIDE.md](ONBOARDING_GUIDE.md) |
| 5 分钟跑起来 | [quickstart_zh.md](quickstart_zh.md) |
