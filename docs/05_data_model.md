# 数据模型

## 实体关系图

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│   Project   │       │    Tag      │       │  Workspace  │
├─────────────┤       ├─────────────┤       ├─────────────┤
│ Id          │       │ Id          │       │ Id          │
│ Name        │       │ Name        │       │ Name        │
│ Alias       │       │ Color       │       │ Description │
│ Path        │       │ CreatedAt   │       │ ProjectIds  │
│ Description │       └─────────────┘       │ CustomTags[]│◄──┐
│ Tags[]      │◄──────────────┘             │ Inherited[]│   │
│ Color       │                             │ AutoInherit│   │
│ DefaultIdeId│                             │ CreatedAt  │   │
│ LastOpenedAt│                             │ UpdatedAt  │   │
│ OpenCount   │◄──────────────────────────────────┘        │
│ IsFavorite  │                                            │
│ CreatedAt   │                                            │
│ UpdatedAt   │◄───────────────────────────────────────────┘
└─────────────┘              标签继承关系
       │
       │ 引用
       ▼
┌─────────────┐
│ IdeTemplate │
├─────────────┤
│ Id          │
│ Name        │
│ ExecutablePath│
│ DefaultArgs │
│ Icon        │
│ Priority    │
└─────────────┘
```

## 实体定义

### Project（项目）

```csharp
public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public string? Alias { get; set; }
    
    public string Path { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public List<string> Tags { get; set; } = new();
    
    public string? Color { get; set; }
    
    public Guid? DefaultIdeId { get; set; }
    
    public DateTime LastOpenedAt { get; set; }
    
    public int OpenCount { get; set; }
    
    public bool IsFavorite { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public string SearchText => $"{Name} {Alias} {string.Join(" ", Tags)}".ToLower();
}
```

### IdeTemplate（IDE模板 - 全局配置）

```csharp
public class IdeTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public string ExecutablePath { get; set; } = string.Empty;
    
    public string? DefaultArgs { get; set; }
    
    public string? Icon { get; set; }
    
    public List<string> SupportedExtensions { get; set; } = new();
    
    public int Priority { get; set; }
}
```

#### IDE检测服务

```csharp
public class IdeDetectionService
{
    /// <summary>
    /// 检测系统中已安装的IDE
    /// </summary>
    public List<IdeTemplate> DetectInstalledIdes()
    {
        var detectedIdes = new List<IdeTemplate>();
        
        // 检测 VS Code
        var vscodePath = DetectVsCode();
        if (vscodePath != null)
        {
            detectedIdes.Add(new IdeTemplate
            {
                Name = "VS Code",
                ExecutablePath = vscodePath,
                DefaultArgs = "",
                Icon = "🆚",
                Priority = 1
            });
        }
        
        // 检测 Trae
        var traePath = DetectTrae();
        if (traePath != null)
        {
            detectedIdes.Add(new IdeTemplate
            {
                Name = "Trae",
                ExecutablePath = traePath,
                DefaultArgs = "",
                Icon = "🤖",
                Priority = 2
            });
        }
        
        // 检测 Trae-CN
        var traeCnPath = DetectTraeCn();
        if (traeCnPath != null)
        {
            detectedIdes.Add(new IdeTemplate
            {
                Name = "Trae-CN",
                ExecutablePath = traeCnPath,
                DefaultArgs = "",
                Icon = "🤖",
                Priority = 3
            });
        }
        
        return detectedIdes;
    }
    
    private string? DetectVsCode()
    {
        // 1. 检查 PATH 环境变量
        // 2. 检查常见安装路径
        // Windows: %ProgramFiles%\Microsoft VS Code\code.exe
        // Windows: %LocalAppData%\Programs\Microsoft VS Code\code.exe
    }
    
    private string? DetectTrae()
    {
        // 1. 检查 PATH 环境变量
        // 2. 检查常见安装路径
        // Windows: %LocalAppData%\Trae\Trae.exe
    }
    
    private string? DetectTraeCn()
    {
        // 1. 检查 PATH 环境变量
        // 2. 检查常见安装路径
        // Windows: %LocalAppData%\Trae CN\Trae.exe
    }
}
```

### Workspace（工作区）

```csharp
public class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public List<Guid> ProjectIds { get; set; } = new();
    
    /// <summary>
    /// 自定义标签（用户手动添加的）
    /// </summary>
    public List<string> CustomTags { get; set; } = new();
    
    /// <summary>
    /// 继承自项目的标签（自动计算，不持久化）
    /// </summary>
    public List<string> InheritedTags { get; set; } = new();
    
    /// <summary>
    /// 是否自动继承项目标签
    /// </summary>
    public bool AutoInheritTags { get; set; } = true;
    
    /// <summary>
    /// 所有标签（自定义+继承，去重）
    /// </summary>
    public List<string> AllTags => AutoInheritTags
        ? CustomTags.Union(InheritedTags).ToList()
        : CustomTags;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 获取搜索文本（用于搜索功能）
    /// </summary>
    public string SearchText => $"{Name} {Description} {string.Join(" ", AllTags)}".ToLower();
}
```

### Tag（标签）

```csharp
public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public string Color { get; set; } = "#3498db";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### AppSettings（应用设置）

```csharp
public class AppSettings
{
    public string Theme { get; set; } = "System";
    
    public string Language { get; set; } = "zh-CN";
    
    public bool StartWithSystem { get; set; } = false;
    
    public bool MinimizeToTray { get; set; } = true;
    
    public int MaxRecentProjects { get; set; } = 10;
}
```

## 实体关系说明

### Project 与 Tag
- 多对多关系
- 一个项目可以有多个标签
- 一个标签可以关联多个项目

### Workspace 与 Project
- 一对多关系
- 一个工作区包含多个项目
- 一个项目可以属于多个工作区

### Workspace 标签继承机制

```
┌─────────────────────────────────────────────────────────┐
│                    工作区标签系统                         │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  项目A [标签: web, frontend, react]                      │
│  项目B [标签: backend, api, dotnet]                      │
│  项目C [标签: web, frontend, vue]                        │
│       │         │         │                             │
│       └─────────┴─────────┘                             │
│                 │                                       │
│                 ▼ 自动继承（去重）                        │
│  ┌─────────────────────────────────┐                   │
│  │ 工作区继承标签: [web, frontend,  │                   │
│  │               backend, api,     │                   │
│  │               react, vue]       │                   │
│  └─────────────────────────────────┘                   │
│                 │                                       │
│  用户自定义标签: [fullstack, important]                  │
│                 │                                       │
│                 ▼ 合并                                   │
│  ┌─────────────────────────────────┐                   │
│  │ 工作区所有标签:                 │                   │
│  │ [web, frontend, backend, api,   │                   │
│  │  react, vue, fullstack,         │                   │
│  │  important]                     │                   │
│  └─────────────────────────────────┘                   │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

#### 继承规则
1. **自动继承**：工作区自动收集所有包含项目的标签
2. **去重处理**：相同标签只保留一个
3. **优先级**：自定义标签优先于继承标签
4. **动态更新**：当项目标签变化时，工作区继承标签自动更新

#### 标签类型区分
- **自定义标签（CustomTags）**：用户手动添加，持久化存储
- **继承标签（InheritedTags）**：自动计算，不持久化
- **所有标签（AllTags）**：两者合并后的完整列表

#### 使用场景
- **筛选**：点击标签可以筛选包含该标签的工作区和项目
- **搜索**：支持按标签搜索工作区
- **显示**：工作区卡片上显示标签（最多3个，多的用...+n）
