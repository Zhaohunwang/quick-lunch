# 数据模型

## 架构分层

项目采用**领域模型（Domain Model）与 EF 实体（EF Entity）分离**的架构：

```
┌──────────────────────────────────────────────────────────┐
│                      表现层 / ViewModel                    │
│              引用 Domain Model (Project, Tag, ...)        │
├──────────────────────────────────────────────────────────┤
│                                                          │
│              Domain Model（领域模型）                      │
│  ProjectHub.Core.Models.Project                          │
│  ProjectHub.Core.Models.Tag                              │
│  ProjectHub.Core.Models.Workspace                        │
│  ProjectHub.Core.Models.IdeTemplate                      │
│  ProjectHub.Core.Models.IdeConfiguration                 │
│  ProjectHub.Core.Models.AppSettings                      │
│                                                          │
│  特点：                                                   │
│  - 不依赖 EF Core                                        │
│  - POCO 类，无导航属性                                   │
│  - Tags 用 List<string> 表示                             │
│  - 供 UI 绑定和 Service 接口返回                          │
│                                                          │
├──────────────────────────────────────────────────────────┤
│                   EntityMapper                            │
│        ProjectHub.Core.Database.EntityMapper              │
│                                                          │
│        .ToDomain()  /  .ToEntity() 扩展方法               │
├──────────────────────────────────────────────────────────┤
│                                                          │
│              EF Entity（持久化实体）                       │
│  ProjectHub.Core.Models.Entities.ProjectEntity           │
│  ProjectHub.Core.Models.Entities.TagEntity               │
│  ProjectHub.Core.Models.Entities.WorkspaceEntity         │
│  ProjectHub.Core.Models.Entities.IdeTemplateEntity       │
│  ProjectHub.Core.Models.Entities.IdeConfigurationEntity  │
│  ProjectHub.Core.Models.Entities.ProjectTagEntity        │
│  ProjectHub.Core.Models.Entities.WorkspaceProjectEntity  │
│                                                          │
│  特点：                                                   │
│  - 包含 EF 导航属性和关系配置                             │
│  - 多对多用显式连接表                                     │
│  - JSON 列（CustomTagsJson, SupportedExtensionsJson）     │
│  - Id 使用 long 自增主键                                 │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

### long 自增主键策略

EF Entity 的 `long` 主键使用 SQLite 原生 **INTEGER PRIMARY KEY AUTOINCREMENT**：

1. **顺序生成**：SQLite 自增整数天然是顺序的，无需额外算法
2. **索引友好**：B-tree 索引末尾追加，无页分裂
3. **轻量高效**：无额外序列表，无 Hi/Lo 计算开销

EF Core 通过 `ValueGeneratedOnAdd()` 配置，SQLite 自动处理 `INTEGER PRIMARY KEY AUTOINCREMENT`。

## 实体关系图

### 数据库表结构（EF Entity）

```
┌──────────────────┐       ┌──────────────────┐       ┌──────────────────┐
│    Projects      │       │     Tags         │       │   Workspaces     │
├──────────────────┤       ├──────────────────┤       ├──────────────────┤
│ Id (PK, AutoInc) │       │ Id (PK, AutoInc) │       │ Id (PK, AutoInc) │
│ Name            │       │ Name (UNIQUE)    │       │ Name             │
│ Alias           │       │ Color            │       │ Description      │
│ Path            │       │ CreatedAt        │       │ CustomTagsJson   │
│ Description     │       └────────┬─────────┘       │ AutoInheritTags  │
│ Color           │                │                  │ CreatedAt        │
│ Icon            │                │                  │ UpdatedAt        │
│ DefaultIdeId    │                │                  └────────┬─────────┘
│ LastOpenedAt    │                │                           │
│ OpenCount       │                │                           │
│ IsFavorite      │                │                           │
│ CreatedAt       │                │                           │
│ UpdatedAt       │                │                           │
└────────┬────────┘                │                           │
         │                         │                           │
         │  ┌──────────────────────┘                           │
         │  │                                                  │
         │  ▼                                                  │
         │  ┌──────────────────┐                               │
         │  │   ProjectTags    │                               │
         │  ├──────────────────┤                               │
         │  │ ProjectId (PFK) │◄─── Projects                   │
         │  │ TagId (PFK)     │◄─── Tags                       │
         │  └──────────────────┘                               │
         │                                                     │
         │  ┌──────────────────────────┐                       │
         │  │    WorkspaceProjects     │                       │
         │  ├──────────────────────────┤                       │
         │  │ WorkspaceId (PFK)       │◄─── Workspaces         │
         │  │ ProjectId (PFK)         │◄─── Projects           │
         │  │ SortOrder               │                        │
         │  └──────────────────────────┘                       │
         │                                                     │
         ▼                                                     │
┌──────────────────┐          ┌──────────────────┐             │
│IdeConfigurations │          │  IdeTemplates    │             │
├──────────────────┤          ├──────────────────┤             │
│ Id (PK, AutoInc) │          │ Id (PK, AutoInc) │             │
│ Name            │          │ Name             │             │
│ ExecutablePath  │          │ ExecutablePath   │             │
│ CommandArgs     │          │ DefaultArgs      │             │
│ IsDefault       │          │ Icon             │             │
│ Icon            │          │ SupportedExtJson │             │
│ ProjectId (FK)  │──►Projects│ Priority         │             │
└──────────────────┘          └──────────────────┘             │
```

## 领域模型定义

### Project（项目）

```csharp
namespace ProjectHub.Core.Models;

public class Project
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Alias { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public long? DefaultIdeId { get; set; }
    public List<IdeConfiguration> IdeConfigurations { get; set; } = new();
    public DateTime LastOpenedAt { get; set; }
    public int OpenCount { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string SearchText => $"{Name} {Alias} {string.Join(" ", Tags)}".ToLower();
}
```

### Tag（标签）

```csharp
namespace ProjectHub.Core.Models;

public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#3498db";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Workspace（工作区）

```csharp
namespace ProjectHub.Core.Models;

public class Workspace
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<long> ProjectIds { get; set; } = new();
    public List<string> CustomTags { get; set; } = new();
    public List<string> InheritedTags { get; set; } = new();
    public bool AutoInheritTags { get; set; } = true;
    public List<string> AllTags => AutoInheritTags
        ? CustomTags.Union(InheritedTags).ToList()
        : CustomTags;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string SearchText => $"{Name} {Description} {string.Join(" ", AllTags)}".ToLower();
}
```

### IdeTemplate（IDE模板 - 全局配置）

```csharp
namespace ProjectHub.Core.Models;

public class IdeTemplate
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string? DefaultArgs { get; set; }
    public string? Icon { get; set; }
    public List<string> SupportedExtensions { get; set; } = new();
    public int Priority { get; set; }
}
```

### IdeConfiguration（项目IDE配置）

```csharp
namespace ProjectHub.Core.Models;

public class IdeConfiguration
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string? CommandArgs { get; set; }
    public bool IsDefault { get; set; }
    public string? Icon { get; set; }
    public long ProjectId { get; set; }
}
```

### AppSettings（应用设置）

```csharp
namespace ProjectHub.Core.Models;

public class AppSettings
{
    public string HotKeyQuickLaunch { get; set; } = "Ctrl+Shift+P";
    public string HotKeyOpenManager { get; set; } = "Ctrl+Shift+O";
    public string Theme { get; set; } = "System";
    public string Language { get; set; } = "zh-CN";
    public bool StartWithSystem { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public int MaxRecentProjects { get; set; } = 10;
}
```

## EF 实体定义

### ProjectEntity

```csharp
namespace ProjectHub.Core.Models.Entities;

public class ProjectEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Alias { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public long? DefaultIdeId { get; set; }
    public DateTime LastOpenedAt { get; set; }
    public int OpenCount { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // EF 导航属性
    public List<IdeConfigurationEntity> IdeConfigurations { get; set; } = new();
    public List<ProjectTagEntity> ProjectTags { get; set; } = new();
}
```

### TagEntity

```csharp
namespace ProjectHub.Core.Models.Entities;

public class TagEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#3498db";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ProjectTagEntity> ProjectTags { get; set; } = new();
}
```

### WorkspaceEntity

```csharp
namespace ProjectHub.Core.Models.Entities;

public class WorkspaceEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool AutoInheritTags { get; set; } = true;
    public string CustomTagsJson { get; set; } = "[]";   // JSON 序列化存储
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<WorkspaceProjectEntity> WorkspaceProjects { get; set; } = new();
}
```

### ProjectTagEntity（多对多连接表）

```csharp
namespace ProjectHub.Core.Models.Entities;

public class ProjectTagEntity
{
    public Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; } = null!;
    public Guid TagId { get; set; }
    public TagEntity Tag { get; set; } = null!;
}
```

### WorkspaceProjectEntity（多对多连接表）

```csharp
namespace ProjectHub.Core.Models.Entities;

public class WorkspaceProjectEntity
{
    public Guid WorkspaceId { get; set; }
    public WorkspaceEntity Workspace { get; set; } = null!;
    public Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; } = null!;
    public int SortOrder { get; set; }
}
```

### IdeConfigurationEntity

```csharp
namespace ProjectHub.Core.Models.Entities;

public class IdeConfigurationEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string? CommandArgs { get; set; }
    public bool IsDefault { get; set; }
    public string? Icon { get; set; }
    public long ProjectId { get; set; }
    public ProjectEntity Project { get; set; } = null!;
}
```

### IdeTemplateEntity

```csharp
namespace ProjectHub.Core.Models.Entities;

public class IdeTemplateEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string? DefaultArgs { get; set; }
    public string? Icon { get; set; }
    public string SupportedExtensionsJson { get; set; } = "[]";  // JSON 序列化存储
    public int Priority { get; set; }
}
```

### HiLoSequenceEntry

```csharp
namespace ProjectHub.Core.Database;

public class HiLoSequenceEntry
{
    public string Name { get; set; } = string.Empty;
    public long CurrentValue { get; set; }
}
```

## 实体映射

`EntityMapper` 提供 Domain Model ↔ EF Entity 的双向映射扩展方法：

```csharp
namespace ProjectHub.Core.Database;

public static class EntityMapper
{
    // Domain → Entity
    public static ProjectEntity ToEntity(this Project domain) => ...;
    public static TagEntity ToEntity(this Tag domain) => ...;
    public static IdeConfigurationEntity ToEntity(this IdeConfiguration domain) => ...;
    public static IdeTemplateEntity ToEntity(this IdeTemplate domain) => ...;
    public static WorkspaceEntity ToEntity(this Workspace domain) => ...;

    // Entity → Domain
    public static Project ToDomain(this ProjectEntity entity) => ...;
    public static Tag ToDomain(this TagEntity entity) => ...;
    public static IdeConfiguration ToDomain(this IdeConfigurationEntity entity) => ...;
    public static IdeTemplate ToDomain(this IdeTemplateEntity entity) => ...;
    public static Workspace ToDomain(this WorkspaceEntity entity, List<string> inheritedTags) => ...;
}
```

## 实体关系说明

### Project 与 Tag
- **多对多**关系，通过 `ProjectTags` 显式连接表
- 领域模型中 `Project.Tags` 是 `List<string>`，存储标签名称
- EF 实体中通过 `ProjectTagEntity` 建立关联

### Workspace 与 Project
- **多对多**关系，通过 `WorkspaceProjects` 显式连接表
- 领域模型中 `Workspace.ProjectIds` 是 `List<Guid>`
- EF 实体中通过 `WorkspaceProjectEntity` 建立关联，包含 `SortOrder` 排序

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
- **自定义标签（CustomTags）**：用户手动添加，持久化存储（JSON 列）
- **继承标签（InheritedTags）**：自动计算，不持久化
- **所有标签（AllTags）**：两者合并后的完整列表

#### 使用场景
- **筛选**：点击标签可以筛选包含该标签的工作区和项目
- **搜索**：支持按标签搜索工作区
- **显示**：工作区卡片上显示标签（最多3个，多的用...+n）
