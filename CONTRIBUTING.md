# 贡献指南

感谢你对 AOIFrame Lite 的兴趣！我们欢迎任何形式的贡献，包括代码、文档、测试和问题反馈。

## 贡献方式

### 📋 报告问题

如发现Bug或有功能建议，请在 [Issues](https://github.com/yourusername/AOIFrame-Lite/issues) 中提交：

1. **检查是否已存在类似Issue**
2. **提供详细信息：**
   - 问题描述
   - 复现步骤
   - 预期行为 vs 实际行为
   - 系统环境（OS、.NET版本等）
   - 错误日志或截图

```markdown
## Bug报告模板

**描述问题**
简短描述问题内容

**复现步骤**
1. 步骤1
2. 步骤2
3. 步骤3

**预期行为**
应该发生什么

**实际行为**
实际发生了什么

**系统环境**
- OS: Windows 10
- .NET: 4.8
- AOIFrame: 1.0.0

**日志或截图**
附加日志或截图帮助排查
```

### 🎯 功能建议

1. 在 [Discussions](https://github.com/yourusername/AOIFrame-Lite/discussions) 中发起讨论
2. 详细描述功能需求
3. 等待社区反馈和维护者评审

### 💻 代码贡献

#### Fork 和 Clone

```bash
# 1. Fork项目到你的账户
# 访问 https://github.com/yourusername/AOIFrame-Lite/fork

# 2. Clone到本地
git clone https://github.com/your-username/AOIFrame-Lite.git
cd AOIFrame-Lite

# 3. 添加上游远程仓库
git remote add upstream https://github.com/yourusername/AOIFrame-Lite.git
```

#### 创建分支

```bash
# 基于最新的main分支创建分支
git fetch upstream
git checkout -b feature/your-feature-name upstream/main

# 或创建bugfix分支
git checkout -b bugfix/issue-description upstream/main
```

#### 代码风格

遵循以下代码规范：

```csharp
// 1. 命名约定
public class MyMotionController { }           // PascalCase
public int _internalField;                    // _camelCase for private
public int publicProperty { get; set; }       // PascalCase for properties

// 2. 缩进和间距
void MyMethod()
{
    if (condition)
    {
        // 使用4个空格缩进
        int value = 10;
    }
}

// 3. XML注释
/// <summary>
/// 获取轴的当前位置
/// </summary>
/// <param name="axis">轴号</param>
/// <returns>位置值</returns>
public double GetAxisPosition(int axis)
{
    // 实现代码
}

// 4. 异常处理
try
{
    // 操作代码
}
catch (ArgumentException ex)
{
    logger.Error($"参数错误: {ex.Message}");
    throw;
}
```

#### 提交代码

```bash
# 1. 提交更改
git add .
git commit -m "feat: 添加新功能描述

- 详细的修改内容
- 第二个要点"

# 提交消息格式
# feat: 新功能
# fix: 缺陷修复
# docs: 文档更新
# style: 代码格式调整
# refactor: 代码重构
# test: 单元测试
# chore: 构建或配置修改

# 2. 推送到你的fork
git push origin feature/your-feature-name
```

#### 创建 Pull Request

1. 访问你的fork仓库页面
2. 点击 "Compare & pull request"
3. 填写PR描述：

```markdown
## 说明

简短描述此PR解决的问题或添加的功能

## 相关Issue

修复 #123

## 修改清单

- [x] 修改项1
- [x] 修改项2
- [x] 添加单元测试
- [x] 更新文档

## 测试方法

描述如何验证这些修改

## 截图/日志

（如需要）
```

### ✅ 代码审查

- 维护者会审查你的PR
- 可能要求做一些调整
- 一旦批准，PR将被合并

---

## 测试

### 运行单元测试

```bash
# 运行所有测试
dotnet test AOIFrame-Lite.sln

# 运行特定测试项目
dotnet test tests/AOIFrame.Tests/

# 运行特定测试类
dotnet test --filter "ClassName=MotionMgrTests"
```

### 编写单元测试

```csharp
using Xunit;
using AOIFrame.Core;

public class MotionMgrTests
{
    [Fact]
    public void GetAxisPosition_ShouldReturnValidPosition()
    {
        // Arrange
        var mgr = MotionMgr.Instance;
        
        // Act
        double position = mgr.GetAxisPosition(0);
        
        // Assert
        Assert.NotNull(position);
        Assert.True(position >= 0);
    }
    
    [Theory]
    [InlineData(0, 100.0)]
    [InlineData(1, 50.0)]
    public void MoveAbsolute_ShouldMoveToPosisiton(int axis, double target)
    {
        // 测试实现
    }
}
```

### 代码覆盖率

```bash
# 使用OpenCover生成代码覆盖率报告
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# 生成HTML报告
dotnet reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport
```

---

## 文档贡献

### 改进现有文档

1. 克隆仓库
2. 编辑 `docs/` 下的markdown文件
3. 提交Pull Request

### 添加新文档

```bash
# 在docs目录创建新文件
touch docs/my-topic.md

# 在README.md中添加链接
# [我的主题](docs/my-topic.md)
```

### 文档格式要求

- 使用Markdown格式
- 代码块标注语言：\`\`\`csharp
- 使用清晰的标题层级
- 包含代码示例和最佳实践

---

## 提交规范

### Commit Message 格式

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Type:**
- `feat` - 新功能
- `fix` - 缺陷修复
- `docs` - 文档
- `style` - 格式调整
- `refactor` - 代码重构
- `test` - 测试
- `chore` - 构建或依赖更新

**Scope:** 影响的模块（可选）
- `core` - 核心模块
- `communication` - 通信层
- `hardware` - 硬件抽象
- `ui` - 用户界面

**示例：**

```
feat(motion): 添加多轴同步运动接口

新增 SyncMove() 方法支持多轴同时运动到相同位置。
实现指数补偿算法确保各轴同时到达。

Closes #123
```

---

## 开发工作流

### 1. 开发环境设置

```bash
# 克隆项目
git clone https://github.com/yourusername/AOIFrame-Lite.git

# 打开Visual Studio
start AOIFrame-Lite.sln

# 或使用VS Code
code .
```

### 2. 修改代码

```bash
# 创建特性分支
git checkout -b feature/new-feature

# 编写代码...
# 运行测试
dotnet test

# 提交更改
git commit -am "feat: 新功能描述"
```

### 3. 推送和发起PR

```bash
# 推送分支
git push origin feature/new-feature

# 访问GitHub创建Pull Request
```

### 4. 回应反馈

根据审查意见进行修改，继续提交更新

---

## 核心贡献者

我们感谢所有的贡献者！查看 [CONTRIBUTORS.md](CONTRIBUTORS.md) 了解完整列表。

---

## 许可证

通过提交代码，你同意你的贡献将在 [MIT License](LICENSE) 下发布。

---

## 有疑问？

- 📖 查看 [文档](README.md)
- 💬 在 [Discussions](https://github.com/yourusername/AOIFrame-Lite/discussions) 提问
- 📧 联系维护者：maintainers@example.com

再次感谢你的贡献！🎉

