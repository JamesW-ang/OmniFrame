# 架构文档

## AOIFrame Lite 完整架构解析

### 一、四层架构总览

AOIFrame Lite 采用分层架构设计，从上到下分别为：**界面层 → 业务逻辑层 → 数据访问层 → 通信层**

```
┌─────────────────────────────────────────────────────────────────┐
│                    UI Layer (界面层)                               │
│  ├─ MainForm (主窗口)                                             │
│  ├─ ManualForm (手动控制)                                         │
│  ├─ DigitalTwinForm (数字孪生3D可视化)                            │
│  └─ Custom Controls (LED指示灯、数值显示器等)                     │
└────────────────────┬────────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────────┐
│            Business Logic Layer (业务逻辑层)                     │
│  ├─ MotionMgr (运动控制管理)                                      │
│  ├─ IoMgr (IO管理)                                               │
│  ├─ SystemManager (系统管理)                                      │
│  ├─ PluginManager (插件管理)                                      │
│  ├─ RecipeManager (工艺配方)                                      │
│  └─ AlarmHandler (报警处理)                                       │
└────────────────────┬────────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────────┐
│            Data Access Layer (数据访问层)                        │
│  ├─ ConfigManager (配置管理)                                      │
│  ├─ XmlHelper / IniHelper (配置文件)                             │
│  ├─ CsvOperation (数据导入导出)                                   │
│  ├─ AuditLogger (审计日志)                                        │
│  └─ Security (权限管理)                                           │
└────────────────────┬────────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────────┐
│           Communication Layer (通信层)                           │
│  ├─ TcpLink (TCP通信)                                             │
│  ├─ SerialLink / ComLink (串口通信)                              │
│  ├─ PlcLink (PLC协议)                                             │
│  │  ├─ ModbusTCP/RTU                                              │
│  │  ├─ 三菱MC协议                                                 │
│  │  ├─ 汇川InovanceProtocol                                       │
│  │  └─ 基恩士HostLink                                             │
│  ├─ OpcLink (OPC UA)                                              │
│  ├─ MqttLink (MQTT)                                               │
│  └─ DigitalTwinBridge (WebSocket 数字孪生桥)                     │
└─────────────────────────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────────────────────────┐
│         Hardware Abstraction Layer (硬件抽象层)                  │
│  通过反射机制动态加载设备驱动                                      │
│  ├─ Motion_DMC3000.dll (研华运动卡)                              │
│  ├─ Motion_GTS.dll (科运通运动卡)                                │
│  ├─ Motion_InoEcat.dll (汇川ECAT从站)                            │
│  └─ 自定义硬件适配器 (Plugin模式)                                │
└─────────────────────────────────────────────────────────────────┘
```

### 二、核心设计模式

#### 1. **单例模式（Singleton）**

全局管理器通过单例模式提供全局访问入口：

```csharp
// 获取运动控制管理器实例
var motion = MotionMgr.Instance;

// 获取IO管理器实例
var io = IoMgr.Instance;

// 获取系统管理器实例
var system = SystemManager.Instance;
```

**优势：**
- 保证全局唯一性
- 线程安全（使用lock或Lazy<T>）
- 简化依赖注入

#### 2. **反射桥接机制（Reflection Bridge）**

通过反射动态加载硬件驱动，实现零编译依赖：

```csharp
// 动态加载运动卡驱动
Assembly driverAsm = Assembly.LoadFrom("Motion_DMC3000.dll");
Type driverType = driverAsm.GetType("Motion.DMC3000.MotionController");
IMotionDevice device = (IMotionDevice)Activator.CreateInstance(driverType);
```

**优势：**
- 升级驱动无需重新编译主程序
- 支持多卡并行运行
- 降低框架与驱动的耦合度

#### 3. **WebSocket 数字孪生桥接**

实时推送设备数据到Web/3D前端：

```csharp
// 后端
var bridge = new DigitalTwinBridge("ws://localhost:3001");
await bridge.ConnectAsync();
bridge.PushAxisPosition(0, 123.45);
bridge.PushIOStatus("motor_enable", true);

// 前端 (Three.js / WebGL)
socket.on('axis_position', (data) => {
    updateAxisVisual(data.axis, data.position);
});
```

#### 4. **插件化扩展（Plugin Architecture）**

通过动态加载DLL扩展功能，无需修改核心代码：

```csharp
// 动态加载插件
PluginManager.Instance.LoadPlugin("MyCustomPlugin.dll");

// 插件实现IPlugin接口
public class MyCustomPlugin : IPlugin
{
    public void Initialize(IServiceProvider services) { }
    public void Execute(Dictionary<string, object> parameters) { }
}
```

#### 5. **管理器模式（Manager Pattern）**

通过专用管理器统一管理同类资源：

- **MotionMgr** - 多轴运动控制
- **IoMgr** - 数字/模拟IO
- **SystemManager** - 系统级别配置与状态
- **RecipeManager** - 生产工艺配方

### 三、数据流向

#### 正常工作流程

```
用户交互 (UI Layer)
    ↓
业务逻辑处理 (Business Logic Layer)
    ↓
数据访问 (Data Access Layer) ← 配置文件、数据库
    ↓
设备通信 (Communication Layer) ← 实时设备数据
    ↓
硬件执行 (Hardware)
```

#### 数字孪生数据流

```
Physical Device (运动卡/PLC/传感器)
    ↓
Communication Layer (数据采集)
    ↓
DigitalTwinBridge (WebSocket推送)
    ↓
Web Frontend (Three.js 3D可视化)
```

### 四、关键组件详解

#### MotionMgr（运动控制管理器）

**职责：**
- 管理多块运动控制卡
- 轴位置读写、运动规划
- 多轴同步控制
- 故障诊断与恢复

**API示例：**

```csharp
// 初始化
MotionMgr.Instance.Initialize();

// 读取轴位置
double position = MotionMgr.Instance.GetAxisPosition(0);

// 设置运动参数
MotionMgr.Instance.SetAxisVelocity(0, 100.0);  // mm/s
MotionMgr.Instance.SetAxisAccel(0, 500.0);     // mm/s²

// 绝对运动
MotionMgr.Instance.MoveAbsolute(0, 50.0);

// 相对运动
MotionMgr.Instance.MoveRelative(0, 10.0);

// 等待运动完成
MotionMgr.Instance.WaitMotionDone(0, timeout: 5000);
```

#### IoMgr（IO管理器）

**职责：**
- 数字输入/输出（DI/DO）
- 模拟输入/输出（AI/AO）
- IO映射配置
- 实时IO状态监听

**API示例：**

```csharp
// 读取DI
bool diStatus = IoMgr.Instance.ReadDI(0);

// 写入DO
IoMgr.Instance.WriteDO(0, true);

// 读取AI（模拟输入）
double aiValue = IoMgr.Instance.ReadAI(0);

// 写入AO（模拟输出）
IoMgr.Instance.WriteAO(0, 5.0);  // 0-10V范围

// 注册IO变化事件
IoMgr.Instance.OnDIChanged += (index, newState) => {
    Console.WriteLine($"DI[{index}] changed to {newState}");
};
```

#### ConfigManager（配置管理器）

**职责：**
- 加载系统配置文件
- 参数读写与校验
- 配置文件热更新（支持XML、INI格式）

**API示例：**

```csharp
// 初始化
ConfigManager.Instance.Initialize("Config/SystemCfg.xml");

// 读取参数
string dbConnection = ConfigManager.Instance.GetConfig("Database", "ConnectionString");
int maxConnections = ConfigManager.Instance.GetConfig<int>("Database", "MaxConnections");

// 设置参数
ConfigManager.Instance.SetConfig("Machine", "Speed", 100);

// 保存配置
ConfigManager.Instance.SaveConfig();
```

#### PluginManager（插件管理器）

**职责：**
- 动态加载/卸载插件
- 插件生命周期管理
- 插件间通信

**API示例：**

```csharp
// 加载插件
PluginManager.Instance.LoadPlugin("plugins/MyPlugin.dll");

// 获取已加载插件列表
var plugins = PluginManager.Instance.GetLoadedPlugins();

// 卸载插件
PluginManager.Instance.UnloadPlugin("MyPlugin");

// 执行插件函数
PluginManager.Instance.ExecutePluginFunction("MyPlugin", "DoSomething", parameters);
```

### 五、通信协议支持

| 协议 | 应用场景 | 支持 |
|------|--------|------|
| ModbusTCP | 标准工业协议 | ✅ 完全支持 |
| ModbusRTU | 串口通信 | ✅ 完全支持 |
| 三菱MC协议 | 三菱PLC | ✅ 完全支持 |
| 汇川Inovance | 汇川PLC | ✅ 完全支持 |
| 基恩士HostLink | 基恩士PLC | ✅ 完全支持 |
| OPC UA | 企业集成 | ✅ 完全支持 |
| MQTT | 边缘计算 | ✅ 完全支持 |
| WebSocket | 数字孪生 | ✅ 完全支持 |

### 六、设计原则总结

1. **分层隔离** - 各层职责单一，模块间松耦合
2. **单一职责** - 每个Manager类只负责一类资源
3. **开闭原则** - 通过插件扩展功能，无需修改核心
4. **依赖注入** - 减少硬编码依赖，提高可测试性
5. **约定优于配置** - 降低配置复杂度
6. **容错恢复** - 自动重连、异常捕获、日志记录

