# Project Hub - 统一项目管理工具

> 版本: 1.0.0  
> 更新日期: 2026-03-20  
> 技术栈: C# + Avalonia UI + .NET 8

---

## 目录

- [项目概述](#项目概述)
- [需求分析](#需求分析)
- [技术选型](#技术选型)
- [功能模块](#功能模块)
- [交互设计](#交互设计)
- [数据模型](#数据模型)
- [MCP支持规划](#mcp支持规划)
- [项目结构](#项目结构)
- [开发路线](#开发路线)

---

## 项目概述

### 项目名称
Project Hub（项目管理中心）

### 项目定位
一个跨平台的统一项目管理工具，用于管理多个IDE项目，支持快速搜索、标签分类、工作区管理，并提供MCP接口供AI工具调用。

### 核心价值
1. **统一入口** - 一个工具管理所有项目，不再依赖各IDE的工作区记录
2. **快速访问** - 全局快捷键 + 模糊搜索，秒级打开项目
3. **灵活配置** - 自定义别名、标签、颜色，支持多种IDE
4. **AI集成** - 通过MCP协议支持AI工具直接操作项目

---

## 需求分析

### 用户痛点

| 痛点 | 描述 | 解决方案 |
|------|------|----------|
| 项目分散 | 多个IDE打开不同项目，难以统一管理 | 统一项目管理入口 |
| 检索困难 | 项目多了之后，找项目不方便 | 全局搜索 + 标签 + 别名 |
| 工作区隔离 | 不同IDE之间工作区记录无法共享 | 跨IDE工作区管理 |
| IDE选择 | 需要根据项目类型选择不同的IDE打开 | 多IDE配置 + 快速切换 |

### 目标用户
- 同时维护多个项目的开发者
- 需要快速切换不同项目的用户
- 使用多种IDE进行开发的团队
- 希望通过AI工具管理项目的用户

---

## 技术选型

### 技术栈确认

```
前端框架: Avalonia UI 11.x
运行时:   .NET 8 (LTS)
语言:     C# 12
ORM:      Entity Framework Core 8.x
数据库:   SQLite
架构模式: MVVM (CommunityToolkit.Mvvm)
```

### 选型理由

| 维度 | Avalonia UI 优势 |
|------|------------------|
| 跨平台 | 原生支持 Windows / macOS / Linux |
| 开发效率 | XAML + C#，WPF开发者零成本上手 |
| 性能 | 原生编译，内存占用低（约30-50MB） |
| 生态 | 控件丰富，社区活跃，文档完善 |
| 打包 | 支持单文件发布，体积适中 |

### NuGet 依赖

```xml
<!-- 核心框架 -->
<PackageReference Include="Avalonia" Version="11.1.0" />
<PackageReference Include="Avalonia.Desktop" Version="11.1.0" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.1.0" />

<!-- MVVM -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />

<!-- 数据库 -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />

<!-- 全局快捷键 -->
<PackageReference Include="GlobalHotKey" Version="1.0.0" />

<!-- JSON处理 -->
<PackageReference Include="System.Text.Json" Version="8.0.0" />

<!-- MCP支持 -->
<PackageReference Include="ModelContextProtocol" Version="*" />
```

---

## 功能模块

### 功能架构图

```
┌─────────────────────────────────────────────────────────┐
│                    Project Hub                           │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │   项目管理   │  │  快速搜索   │  │   工作区管理     │  │
│  │  - 添加项目  │  │  - 模糊搜索  │  │  - 多项目组合    │  │
│  │  - 编辑项目  │  │  - 标签筛选  │  │  - 一键打开     │  │
│  │  - 删除项目  │  │  - 拼音搜索  │  │  - 状态保存     │  │
│  │  - 别名设置  │  │  - 最近访问  │  │                 │  │
│  └─────────────┘  └─────────────┘  └─────────────────┘  │
│                                                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │   IDE管理   │  │   标签系统   │  │    数据管理     │  │
│  │  - IDE配置  │  │  - 自定义标签  │  │  - 本地存储     │  │
│  │  - 自动检测  │  │  - 颜色标记   │  │  - 导入导出     │  │
│  │  - 快速启动  │  │  - 智能分类   │  │  - 数据迁移     │  │
│  └─────────────┘  └─────────────┘  └─────────────────┘  │
│                                                          │
│  ┌───────────────────────────────────────────────────┐  │
│  │                    MCP 服务层                      │  │
│  │  - 项目列表查询    - 打开指定项目    - 项目搜索     │  │
│  └───────────────────────────────────────────────────┘  │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 功能优先级

| 优先级 | 功能模块 | 功能点 | 说明 |
|--------|----------|--------|------|
| P0 | 项目管理 | 添加项目 | 选择目录，自动识别项目信息 |
| P0 | 项目管理 | 删除项目 | 从列表移除，不删除实际文件 |
| P0 | 项目管理 | 编辑项目 | 修改名称、别名、描述、标签 |
| P0 | 快速搜索 | 名称搜索 | 支持模糊匹配 |
| P0 | IDE管理 | 配置IDE | 设置IDE名称和可执行文件路径 |
| P0 | IDE管理 | 启动项目 | 使用指定IDE打开项目 |
| P1 | 快速搜索 | 标签筛选 | 按标签过滤项目 |
| P1 | 快速搜索 | 别名搜索 | 通过别名快速定位 |
| P1 | 快速搜索 | 最近访问 | 显示最近打开的项目 |
| P1 | 标签系统 | 创建标签 | 自定义标签名称和颜色 |
| P1 | 标签系统 | 标签管理 | 编辑、删除标签 |
| P1 | 工作区 | 创建工作区 | 将多个项目组合 |
| P1 | 工作区 | 打开工作区 | 批量打开工作区内所有项目 |
| P2 | 数据管理 | 导出数据 | 导出为JSON格式 |
| P2 | 数据管理 | 导入数据 | 从JSON导入项目配置 |
| P2 | 快速搜索 | 拼音搜索 | 支持中文拼音搜索 |
| P2 | 全局快捷键 | 呼出搜索 | Ctrl+Shift+P 全局呼出 |
| P2 | 全局快捷键 | 呼出管理 | Ctrl+Shift+O 打开管理窗口 |
| P3 | MCP支持 | 项目列表 | MCP工具：获取项目列表 |
| P3 | MCP支持 | 打开项目 | MCP工具：打开指定项目 |
| P3 | MCP支持 | 搜索项目 | MCP工具：搜索项目 |

---

## 交互设计

### 设计理念

采用**双模式交互**设计：
- **快速启动器模式**：专注「快速打开」，键盘驱动
- **管理界面模式**：专注「配置管理」，鼠标友好

### 模式切换

```
┌─────────────────────────────────────────────────────────┐
│  全局快捷键: Ctrl+Shift+P  →  快速启动器（浮层窗口）     │
│  全局快捷键: Ctrl+Shift+O  →  管理界面（主窗口）         │
│  快速启动器内按 Ctrl+,     →  切换到管理界面             │
└─────────────────────────────────────────────────────────┘
```

### 快速启动器设计

#### 界面布局

```
┌────────────────────────────────────────┐
│                                        │
│     ┌────────────────────────────┐     │
│     │  🔍 搜索项目、别名、标签...   │     │
│     └────────────────────────────┘     │
│                                        │
│     ┌────────────────────────────┐     │
│     │ 🚀 电商平台重构              │     │
│     │    别名: 主站  标签: #vue    │     │
│     │    [VS Code] [Trae] [WebStorm]│    │
│     ├────────────────────────────┤     │
│     │ ⚙️ 后端API服务               │     │
│     │    别名: api   标签: #java   │     │
│     │    [IDEA] [Trae]            │     │
│     ├────────────────────────────┤     │
│     │ 🎨 设计系统                  │     │
│     │    标签: #figma #ui          │     │
│     │    [Figma] [VS Code]        │     │
│     └────────────────────────────┘     │
│                                        │
│  ↵打开  Ctrl+1/2/3选IDE  Ctrl+E编辑   │
│                                        │
└────────────────────────────────────────┘
```

#### 窗口特性

| 属性 | 值 |
|------|-----|
| 位置 | 屏幕居中 |
| 尺寸 | 600 x 500 |
| 边框 | 无边框 |
| 背景 | 半透明毛玻璃效果 |
| 任务栏 | 不显示 |
| 失焦行为 | 自动关闭 |
| ESC行为 | 关闭窗口 |

#### 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Enter` | 使用默认IDE打开选中项目 |
| `Ctrl+1/2/3` | 使用第1/2/3个IDE打开 |
| `Ctrl+E` | 编辑选中项目 |
| `Ctrl+,` | 打开管理界面 |
| `Esc` | 关闭快速启动器 |
| `↑/↓` | 上下选择项目 |

### 管理界面设计

#### 界面布局

```
┌─────────────────────────────────────────────────────────┐
│  Project Hub                              [-] [□] [×]   │
├─────────────────────────────────────────────────────────┤
│  🔍 搜索项目...              [快速启动 ▄] [设置] [+]    │
├──────────────┬──────────────────────────────────────────┤
│              │                                          │
│  📁 全部(24) │   📌 快速访问                             │
│  ⭐ 收藏(5)  │   ┌────────┐ ┌────────┐ ┌────────┐      │
│  🕐 最近(10) │   │ 🚀     │ │ ⚙️     │ │ 🎨     │      │
│              │   │电商平台 │ │后端API │ │设计系统 │      │
│  ─────────── │   │[VSCode]│ │ [Trae] │ │[Figma] │      │
│  🏷️ 标签     │   └────────┘ └────────┘ └────────┘      │
│    #web(8)   │                                          │
│    #java(5)  │   📁 全部项目                    [□ ▤ ☰] │
│    #mobile(3)│   ┌─────────────────────────────────────┐│
│              │   │ 🚀 电商平台重构          [VSCode][▶]││
│  ─────────── │   │    别名: 主站  标签: #vue #frontend ││
│  💼 工作区   │   ├─────────────────────────────────────┤│
│    工作区A   │   │ ⚙️ 后端API服务           [Trae][▶]  ││
│    工作区B   │   │    别名: api   标签: #java #spring  ││
│              │   ├─────────────────────────────────────┤│
│  ─────────── │   │ 🎨 设计系统              [Figma][▶] ││
│  ⚙️ 设置     │   │    标签: #design #ui               ││
│              │   └─────────────────────────────────────┘│
│              │                                          │
└──────────────┴──────────────────────────────────────────┘
```

#### 左侧导航结构

```
📁 全部          - 显示所有项目
⭐ 收藏          - 显示收藏的项目
🕐 最近          - 按访问时间排序
───────────────
🏷️ 标签
   #web          - 按标签筛选
   #java
   #mobile
   ...
───────────────
💼 工作区
   工作区A       - 自定义工作区
   工作区B
───────────────
⚙️ 设置         - IDE配置、快捷键、主题
```

#### 右键菜单

```
┌─────────────────────┐
│ ▶ 使用 VS Code 打开 │
│ ▶ 使用 Trae 打开    │
│ ▶ 使用...          │
├─────────────────────┤
│ ✏️ 编辑项目        │
│ ⭐ 收藏/取消收藏   │
├─────────────────────┤
│ 📋 复制路径        │
│ 📂 在资源管理器中显示│
├─────────────────────┤
│ 🗑️ 删除项目        │
└─────────────────────┘
```

#### 视图切换

| 视图 | 图标 | 说明 |
|------|------|------|
| 卡片视图 | □ | 大卡片，显示详细信息 |
| 列表视图 | ▤ | 紧凑列表，显示关键信息 |
| 紧凑视图 | ☰ | 最小化显示，仅名称 |

### 全局快捷键体系

| 快捷键 | 作用域 | 功能 |
|--------|--------|------|
| `Ctrl+Shift+P` | 全局 | 呼出快速启动器 |
| `Ctrl+Shift+O` | 全局 | 打开/聚焦管理界面 |
| `Enter` | 快速启动器 | 用默认IDE打开 |
| `Ctrl+1/2/3` | 快速启动器 | 用指定IDE打开 |
| `Ctrl+E` | 快速启动器 | 编辑项目 |
| `Ctrl+,` | 快速启动器 | 打开管理界面 |
| `Ctrl+N` | 管理界面 | 添加新项目 |
| `Ctrl+F` | 管理界面 | 聚焦搜索框 |
| `Delete` | 管理界面 | 删除选中项目 |
| `F2` | 管理界面 | 重命名项目 |

---

## 数据模型

### 实体关系图

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│   Project   │       │    Tag      │       │  Workspace  │
├─────────────┤       ├─────────────┤       ├─────────────┤
│ Id          │       │ Id          │       │ Id          │
│ Name        │       │ Name        │       │ Name        │
│ Alias       │       │ Color       │       │ Description │
│ Path        │       │ CreatedAt   │       │ ProjectIds  │
│ Description │       └─────────────┘       │ CreatedAt   │
│ Tags[]      │◄──────────────┘             │ UpdatedAt   │
│ Color       │                             └─────────────┘
│ DefaultIdeId│                                   │
│ LastOpenedAt│                                   │
│ OpenCount   │◄──────────────────────────────────┘
│ CreatedAt   │
│ UpdatedAt   │
└─────────────┘
       │
       │ 1:N
       ▼
┌─────────────┐
│IdeConfig    │
├─────────────┤
│ Id          │
│ Name        │
│ ExecutablePath│
│ CommandArgs │
│ IsDefault   │
│ ProjectId   │
└─────────────┘
```

### 实体定义

#### Project（项目）

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
    
    public string? Icon { get; set; }
    
    public Guid? DefaultIdeId { get; set; }
    
    public List<IdeConfiguration> IdeConfigurations { get; set; } = new();
    
    public DateTime LastOpenedAt { get; set; }
    
    public int OpenCount { get; set; }
    
    public bool IsFavorite { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public string SearchText => $"{Name} {Alias} {string.Join(" ", Tags)}".ToLower();
}
```

#### IdeConfiguration（IDE配置）

```csharp
public class IdeConfiguration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public string ExecutablePath { get; set; } = string.Empty;
    
    public string? CommandArgs { get; set; }
    
    public bool IsDefault { get; set; }
    
    public string? Icon { get; set; }
    
    public Guid ProjectId { get; set; }
    
    public Project Project { get; set; } = null!;
}
```

#### IdeTemplate（IDE模板 - 全局配置）

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

#### Workspace（工作区）

```csharp
public class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public List<Guid> ProjectIds { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

#### Tag（标签）

```csharp
public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public string Color { get; set; } = "#3498db";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### AppSettings（应用设置）

```csharp
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

---

## MCP支持规划

### MCP协议概述

MCP (Model Context Protocol) 是一种让AI模型与外部工具交互的协议。通过实现MCP Server，可以让AI助手（如Claude、Trae等）直接操作Project Hub。

### MCP工具定义

#### 1. list_projects

列出所有项目或按条件筛选

```json
{
  "name": "list_projects",
  "description": "获取项目列表，支持按标签、名称筛选",
  "inputSchema": {
    "type": "object",
    "properties": {
      "tag": {
        "type": "string",
        "description": "按标签筛选"
      },
      "name": {
        "type": "string",
        "description": "按名称模糊搜索"
      },
      "limit": {
        "type": "integer",
        "description": "返回数量限制，默认20",
        "default": 20
      }
    }
  }
}
```

**返回示例**：
```json
{
  "projects": [
    {
      "id": "uuid-1",
      "name": "电商平台重构",
      "alias": "主站",
      "path": "D:\\projects\\ecommerce",
      "tags": ["vue", "frontend"],
      "defaultIde": "VS Code"
    }
  ],
  "total": 24
}
```

#### 2. open_project

打开指定项目

```json
{
  "name": "open_project",
  "description": "使用指定IDE打开项目",
  "inputSchema": {
    "type": "object",
    "properties": {
      "projectId": {
        "type": "string",
        "description": "项目ID"
      },
      "projectName": {
        "type": "string",
        "description": "项目名称或别名（与projectId二选一）"
      },
      "ideName": {
        "type": "string",
        "description": "IDE名称，不指定则使用默认IDE"
      }
    },
    "required": []
  }
}
```

**返回示例**：
```json
{
  "success": true,
  "message": "已使用 VS Code 打开项目「电商平台重构」",
  "project": {
    "id": "uuid-1",
    "name": "电商平台重构",
    "path": "D:\\projects\\ecommerce"
  },
  "ide": "VS Code"
}
```

#### 3. search_projects

搜索项目

```json
{
  "name": "search_projects",
  "description": "搜索项目，支持名称、别名、标签、路径搜索",
  "inputSchema": {
    "type": "object",
    "properties": {
      "query": {
        "type": "string",
        "description": "搜索关键词"
      },
      "searchIn": {
        "type": "array",
        "items": {
          "type": "string",
          "enum": ["name", "alias", "tags", "path"]
        },
        "description": "搜索范围，默认全部",
        "default": ["name", "alias", "tags"]
      }
    },
    "required": ["query"]
  }
}
```

#### 4. get_recent_projects

获取最近访问的项目

```json
{
  "name": "get_recent_projects",
  "description": "获取最近访问的项目列表",
  "inputSchema": {
    "type": "object",
    "properties": {
      "limit": {
        "type": "integer",
        "description": "返回数量，默认10",
        "default": 10
      }
    }
  }
}
```

#### 5. add_project

添加新项目

```json
{
  "name": "add_project",
  "description": "添加新项目到列表",
  "inputSchema": {
    "type": "object",
    "properties": {
      "path": {
        "type": "string",
        "description": "项目路径"
      },
      "name": {
        "type": "string",
        "description": "项目名称，不指定则自动检测"
      },
      "alias": {
        "type": "string",
        "description": "别名"
      },
      "tags": {
        "type": "array",
        "items": { "type": "string" },
        "description": "标签列表"
      }
    },
    "required": ["path"]
  }
}
```

### MCP Server实现架构

```
┌─────────────────────────────────────────────────────────┐
│                    MCP Server                            │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │              Transport Layer                     │    │
│  │  - Stdio Transport (标准输入输出)                 │    │
│  │  - HTTP Transport (可选，用于远程调用)            │    │
│  └─────────────────────────────────────────────────┘    │
│                         │                               │
│                         ▼                               │
│  ┌─────────────────────────────────────────────────┐    │
│  │              Protocol Handler                    │    │
│  │  - 请求解析                                      │    │
│  │  - 响应封装                                      │    │
│  │  - 错误处理                                      │    │
│  └─────────────────────────────────────────────────┘    │
│                         │                               │
│                         ▼                               │
│  ┌─────────────────────────────────────────────────┐    │
│  │              Tool Registry                       │    │
│  │  - list_projects                                 │    │
│  │  - open_project                                  │    │
│  │  - search_projects                               │    │
│  │  - get_recent_projects                           │    │
│  │  - add_project                                   │    │
│  └─────────────────────────────────────────────────┘    │
│                         │                               │
│                         ▼                               │
│  ┌─────────────────────────────────────────────────┐    │
│  │              Service Layer                       │    │
│  │  - ProjectService                                │    │
│  │  - IdeLauncherService                            │    │
│  │  - SearchService                                 │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### MCP配置示例

在AI客户端（如Claude Desktop、Trae）中配置：

```json
{
  "mcpServers": {
    "project-hub": {
      "command": "ProjectHub.McpServer.exe",
      "args": ["--stdio"],
      "env": {}
    }
  }
}
```

---

## 项目结构

### 目录结构

```
ProjectHub/
│
├── ProjectHub.Core/                    # 核心类库
│   ├── Models/                         # 数据模型
│   │   ├── Project.cs
│   │   ├── IdeConfiguration.cs
│   │   ├── IdeTemplate.cs
│   │   ├── Workspace.cs
│   │   ├── Tag.cs
│   │   └── AppSettings.cs
│   │
│   ├── Services/                       # 服务接口
│   │   ├── IProjectService.cs
│   │   ├── IIdeLauncherService.cs
│   │   ├── ISearchService.cs
│   │   ├── ISettingsService.cs
│   │   └── IWorkspaceService.cs
│   │
│   ├── Database/                       # 数据库
│   │   ├── AppDbContext.cs
│   │   └── Migrations/
│   │
│   └── Utils/                          # 工具类
│       ├── PathHelper.cs
│       └── PinyinConverter.cs
│
├── ProjectHub.Desktop/                 # Avalonia桌面应用
│   ├── App.axaml                       # 应用入口
│   ├── App.axaml.cs
│   │
│   ├── Views/                          # 视图
│   │   ├── MainWindow.axaml            # 主窗口（管理界面）
│   │   ├── MainWindow.axaml.cs
│   │   ├── QuickLauncherWindow.axaml   # 快速启动器
│   │   ├── QuickLauncherWindow.axaml.cs
│   │   │
│   │   ├── Controls/                   # 用户控件
│   │   │   ├── ProjectCard.axaml
│   │   │   ├── ProjectListItem.axaml
│   │   │   ├── TagBadge.axaml
│   │   │   ├── IdeButton.axaml
│   │   │   └── NavigationPanel.axaml
│   │   │
│   │   ├── Dialogs/                    # 对话框
│   │   │   ├── ProjectEditDialog.axaml
│   │   │   ├── AddProjectDialog.axaml
│   │   │   ├── IdeConfigDialog.axaml
│   │   │   └── WorkspaceEditDialog.axaml
│   │   │
│   │   └── Pages/                      # 页面
│   │       ├── ProjectsPage.axaml
│   │       ├── WorkspacesPage.axaml
│   │       ├── TagsPage.axaml
│   │       └── SettingsPage.axaml
│   │
│   ├── ViewModels/                     # 视图模型
│   │   ├── MainWindowViewModel.cs
│   │   ├── QuickLauncherViewModel.cs
│   │   ├── ProjectCardViewModel.cs
│   │   ├── ProjectsPageViewModel.cs
│   │   ├── WorkspacesPageViewModel.cs
│   │   ├── TagsPageViewModel.cs
│   │   └── SettingsPageViewModel.cs
│   │
│   ├── Services/                       # 服务实现
│   │   ├── ProjectService.cs
│   │   ├── IdeLauncherService.cs
│   │   ├── SearchService.cs
│   │   ├── SettingsService.cs
│   │   ├── WorkspaceService.cs
│   │   ├── HotKeyService.cs
│   │   └── WindowService.cs
│   │
│   ├── Converters/                     # 值转换器
│   │   ├── BoolToVisibilityConverter.cs
│   │   ├── TagListToStringConverter.cs
│   │   └── DateTimeToRelativeConverter.cs
│   │
│   ├── Assets/                         # 资源文件
│   │   ├── Icons/
│   │   ├── Fonts/
│   │   └── Styles/
│   │       └── AppTheme.axaml
│   │
│   └── Program.cs                      # 入口点
│
├── ProjectHub.McpServer/               # MCP服务器
│   ├── Program.cs
│   ├── McpServer.cs
│   ├── Tools/
│   │   ├── ListProjectsTool.cs
│   │   ├── OpenProjectTool.cs
│   │   ├── SearchProjectsTool.cs
│   │   ├── GetRecentProjectsTool.cs
│   │   └── AddProjectTool.cs
│   └── Transport/
│       └── StdioTransport.cs
│
├── ProjectHub.Tests/                   # 单元测试
│   ├── Services/
│   │   ├── ProjectServiceTests.cs
│   │   ├── SearchServiceTests.cs
│   │   └── IdeLauncherServiceTests.cs
│   └── ViewModels/
│       └── QuickLauncherViewModelTests.cs
│
├── docs/                               # 文档
│   ├── ARCHITECTURE.md
│   ├── API.md
│   └── MCP.md
│
├── ProjectHub.sln                      # 解决方案文件
├── README.md
└── .gitignore
```

---

## 开发路线

### Phase 1: MVP核心功能（预计2-3周）

**目标**：实现基本的项目管理和启动功能

| 任务 | 说明 | 优先级 |
|------|------|--------|
| 项目初始化 | 创建解决方案、配置NuGet依赖 | P0 |
| 数据模型 | 实现所有实体类和DbContext | P0 |
| 主窗口框架 | 实现主窗口布局和导航 | P0 |
| 项目列表 | 实现项目列表展示（列表视图） | P0 |
| 添加项目 | 实现添加项目对话框 | P0 |
| 编辑项目 | 实现编辑项目对话框 | P0 |
| 删除项目 | 实现删除项目功能 | P0 |
| IDE配置 | 实现IDE模板配置页面 | P0 |
| 项目启动 | 实现使用指定IDE打开项目 | P0 |
| 基础搜索 | 实现名称模糊搜索 | P0 |

### Phase 2: 快速启动器（预计1-2周）

**目标**：实现快速启动器窗口和全局快捷键

| 任务 | 说明 | 优先级 |
|------|------|--------|
| 快速启动器窗口 | 实现浮层窗口UI | P0 |
| 搜索交互 | 实现实时搜索和选择 | P0 |
| 全局快捷键 | 实现Ctrl+Shift+P呼出 | P1 |
| IDE快速选择 | 实现Ctrl+1/2/3选择IDE | P1 |
| 失焦关闭 | 实现窗口失焦自动关闭 | P1 |

### Phase 3: 高级功能（预计2周）

**目标**：完善标签、工作区等功能

| 任务 | 说明 | 优先级 |
|------|------|--------|
| 标签系统 | 实现标签CRUD和筛选 | P1 |
| 标签着色 | 实现标签颜色标记 | P1 |
| 工作区管理 | 实现工作区CRUD | P1 |
| 批量打开 | 实现工作区批量打开项目 | P1 |
| 最近访问 | 实现访问记录和排序 | P1 |
| 收藏功能 | 实现项目收藏 | P1 |
| 卡片视图 | 实现卡片展示模式 | P2 |
| 数据导入导出 | 实现JSON导入导出 | P2 |
| 拼音搜索 | 实现中文拼音搜索 | P2 |

### Phase 4: MCP支持（预计1-2周）

**目标**：实现MCP Server，支持AI工具调用

| 任务 | 说明 | 优先级 |
|------|------|--------|
| MCP Server框架 | 搭建MCP Server项目 | P3 |
| list_projects工具 | 实现项目列表查询 | P3 |
| open_project工具 | 实现打开项目 | P3 |
| search_projects工具 | 实现项目搜索 | P3 |
| get_recent_projects工具 | 实现最近项目查询 | P3 |
| add_project工具 | 实现添加项目 | P3 |
| MCP文档 | 编写MCP使用文档 | P3 |

### Phase 5: 优化与发布（持续）

**目标**：性能优化、体验打磨、跨平台测试

| 任务 | 说明 | 优先级 |
|------|------|--------|
| 性能优化 | 优化启动速度、搜索性能 | P2 |
| 主题切换 | 支持亮色/暗色主题 | P2 |
| 开机启动 | 支持开机自启动 | P2 |
| 托盘图标 | 实现系统托盘 | P2 |
| macOS适配 | 测试和适配macOS | P3 |
| Linux适配 | 测试和适配Linux | P3 |
| 安装包制作 | 制作各平台安装包 | P3 |

---

## 附录

### 常见IDE可执行文件路径

| IDE | Windows | macOS | Linux |
|-----|---------|-------|-------|
| VS Code | `%LOCALAPPDATA%\Programs\Microsoft VS Code\Code.exe` | `/Applications/Visual Studio Code.app` | `/usr/bin/code` |
| Trae | `%LOCALAPPDATA%\Programs\Trae\Trae.exe` | - | - |
| IntelliJ IDEA | `C:\Program Files\JetBrains\IntelliJ IDEA\bin\idea64.exe` | `/Applications/IntelliJ IDEA.app` | `/usr/bin/idea` |
| WebStorm | `C:\Program Files\JetBrains\WebStorm\bin\webstorm64.exe` | `/Applications/WebStorm.app` | `/usr/bin/webstorm` |
| Cursor | `%LOCALAPPDATA%\Programs\cursor\Cursor.exe` | `/Applications/Cursor.app` | - |

### 项目文件类型与IDE映射

| 文件类型 | 推荐IDE |
|----------|---------|
| `.csproj`, `.sln` | Visual Studio, Rider |
| `package.json` | VS Code, Trae, WebStorm |
| `pom.xml`, `build.gradle` | IntelliJ IDEA |
| `Cargo.toml` | VS Code, RustRover |
| `go.mod` | VS Code, GoLand |
| `*.py` | VS Code, PyCharm |

---

## 更新日志

### v1.0.0 (2026-03-20)
- 初始版本
- 完成需求分析和技术选型
- 完成交互设计
- 完成MCP支持规划
