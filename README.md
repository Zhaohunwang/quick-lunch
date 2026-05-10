# Project Hub (quick-lunch)

一个跨平台的统一项目管理工具，用于管理多个IDE项目，支持快速搜索、标签分类、工作区管理，并提供MCP接口供AI工具调用。

## 核心特性

- **统一入口** - 一个工具管理所有项目，不再依赖各IDE的工作区记录
- **快速访问** - 全局快捷键 + 模糊搜索，秒级打开项目
- **灵活配置** - 自定义别名、标签、颜色，支持多种IDE
- **工作区管理** - 将多个项目组合成工作区，标签自动继承，一键批量打开
- **收藏功能** - 项目和工作区均支持收藏，快速定位常用项
- **AI集成** - 通过MCP协议支持AI工具直接操作项目
- **智能搜索** - 支持名称、别名、标签模糊搜索，以及中文拼音搜索
- **自动检测IDE** - 配置驱动的IDE检测，支持 VS Code、Cursor、Windsurf、VSCodium、Trae 等
- **主题切换** - 基于 Semi.Avalonia 支持亮色/暗色主题

## 技术栈

| 技术 | 版本 | 说明 |
|------|------|------|
| Avalonia UI | 11.1.0 | 跨平台UI框架 |
| Semi.Avalonia | 11.1.0 | UI主题（内置 Light/Dark） |
| .NET | 8 (LTS) | 运行时 |
| C# | 12 | 编程语言 |
| Entity Framework Core | 8.0 | ORM框架 |
| SQLite | - | 本地数据库 |
| CommunityToolkit.Mvvm | 8.2.2 | MVVM框架 |
| Material.Icons.Avalonia | 2.2.0 | 图标库 |

### 核心依赖

```xml
<!-- 核心框架 -->
<PackageReference Include="Avalonia" Version="11.1.0" />
<PackageReference Include="Avalonia.Desktop" Version="11.1.0" />

<!-- UI主题 -->
<PackageReference Include="Semi.Avalonia" Version="11.1.0" />

<!-- 图标 -->
<PackageReference Include="Material.Icons.Avalonia" Version="2.2.0" />

<!-- MVVM -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />

<!-- 数据库 -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />

<!-- JSON处理 -->
<PackageReference Include="System.Text.Json" Version="8.0.0" />
```

## 功能模块

### 项目管理
- 添加/编辑/删除项目
- 设置项目别名和描述
- 自定义标签和颜色标记
- 收藏常用项目
- 记录访问历史和打开次数
- 支持文件夹和文件两种路径类型

### 快速搜索
- 名称模糊匹配
- 别名搜索
- 标签筛选（项目和工作区）
- 最近访问排序
- 中文拼音搜索

### IDE管理
- 支持 VS Code 系列 IDE（VS Code、Cursor、Windsurf、VSCodium）及其他支持命令行打开的编辑器
- 配置驱动的自动检测（`ide_detection.json`）
- 自定义IDE添加（名称 + 可执行文件路径 + 启动参数）
- 右键菜单快速切换IDE
- IDE设置中可扩展检测规则

### 工作区管理
- 创建/编辑/删除多项目工作区
- 工作区标签继承机制（自动继承项目标签 + 自定义标签）
- 批量打开工作区项目
- 收藏工作区
- 工作区右键菜单（收藏/编辑/删除）

### 标签系统
- 自定义标签名称和颜色
- 标签筛选项目和工作区
- 工作区自动继承项目标签

### 数据管理
- SQLite本地存储
- JSON格式导入导出
- 自动数据库迁移

### MCP服务（AI集成）
- `list_projects` - 获取项目列表
- `open_project` - 打开指定项目
- `search_projects` - 搜索项目
- `get_recent_projects` - 查询最近项目
- `add_project` - 添加新项目

## 界面设计

### 双模式交互

#### 快速启动器模式
- 无边框半透明窗口
- 屏幕居中显示（600x500）
- 键盘驱动操作
- 失焦自动关闭
- ESC键快速退出

#### 管理界面模式
- 完整功能的管理界面
- 左侧导航面板（全部/收藏/最近/标签/工作区/设置）
- 工作区和项目混合列表（工作区在前）
- 鼠标友好的操作界面

### 数据模型架构

采用**领域模型（Domain Model）与 EF 实体（EF Entity）分离**的架构：

```
表现层 / ViewModel → Domain Model → EntityMapper → EF Entity → SQLite
```

- Domain Model：POCO 类，不依赖 EF Core，供 UI 绑定
- EF Entity：包含导航属性和关系配置，多对多用显式连接表
- EntityMapper：`.ToDomain()` / `.ToEntity()` 双向转换

## 项目结构

```
ProjectHub/
│
├── ProjectHub.Core/                    # 核心类库
│   ├── Models/                         # 领域模型
│   │   ├── Project.cs
│   │   ├── IdeTemplate.cs
│   │   ├── IdeConfiguration.cs
│   │   ├── IdeDetectionRule.cs
│   │   ├── Workspace.cs
│   │   ├── Tag.cs
│   │   ├── AppSettings.cs
│   │   └── Entities/                   # EF 实体
│   │       ├── ProjectEntity.cs
│   │       ├── TagEntity.cs
│   │       ├── WorkspaceEntity.cs
│   │       ├── IdeTemplateEntity.cs
│   │       ├── IdeConfigurationEntity.cs
│   │       ├── ProjectTagEntity.cs
│   │       └── WorkspaceProjectEntity.cs
│   │
│   ├── Services/                       # 服务接口
│   │   ├── IProjectService.cs
│   │   ├── ITagService.cs
│   │   ├── IIdeLauncherService.cs
│   │   ├── ISearchService.cs
│   │   ├── ISettingsService.cs
│   │   ├── IWorkspaceService.cs
│   │   └── IWorkspaceLauncherService.cs
│   │
│   ├── Database/                       # 数据库
│   │   ├── AppDbContext.cs
│   │   └── EntityMapper.cs
│   │
│   └── Utils/                          # 工具类
│       ├── PathHelper.cs
│       └── PinyinConverter.cs
│
├── ProjectHub.Desktop/                 # Avalonia桌面应用
│   ├── Views/                          # 视图
│   │   ├── MainWindow.axaml            # 主窗口（管理界面）
│   │   ├── QuickLauncherWindow.axaml   # 快速启动器
│   │   ├── AddProjectDialog.axaml      # 添加/编辑项目对话框
│   │   ├── AddWorkspaceDialog.axaml    # 添加/编辑工作区对话框
│   │   ├── AddTagDialog.axaml          # 添加/编辑标签对话框
│   │   ├── IdeSettingsDialog.axaml     # IDE设置对话框
│   │   ├── TagListDialog.axaml         # 标签列表对话框
│   │   └── ConfirmDialog.axaml         # 确认对话框
│   │
│   ├── ViewModels/                     # 视图模型
│   │   ├── MainWindowViewModel.cs      # 主窗口逻辑
│   │   ├── QuickLauncherViewModel.cs   # 快速启动器逻辑
│   │   ├── AddProjectDialogViewModel.cs
│   │   ├── AddWorkspaceDialogViewModel.cs
│   │   ├── AddTagDialogViewModel.cs
│   │   ├── IdeSettingsDialogViewModel.cs
│   │   └── TagListDialogViewModel.cs
│   │
│   ├── Services/                       # 服务实现
│   │   ├── ProjectService.cs
│   │   ├── TagService.cs
│   │   ├── IdeLauncherService.cs
│   │   ├── IdeDetectionService.cs      # IDE自动检测（配置驱动）
│   │   ├── SearchService.cs
│   │   ├── SettingsService.cs
│   │   ├── WorkspaceService.cs
│   │   ├── WorkspaceLauncherService.cs
│   │   ├── ThemeService.cs
│   │   ├── FileLogger.cs
│   │   ├── GlobalExceptionHandler.cs
│   │   └── ErrorMessageBox.cs
│   │
│   ├── Templates/                      # 数据模板选择器
│   │   ├── ItemTemplateSelector.cs     # 项目/工作区卡片模板
│   │   └── TagTemplateSelector.cs      # 标签模板
│   │
│   ├── Converters/                     # 值转换器
│   │   ├── BoolToFavoriteIconConverter.cs
│   │   ├── DefaultIdeConverter.cs
│   │   ├── IsWorkspaceConverter.cs
│   │   ├── ItemTypeToIconConverter.cs
│   │   ├── StringFormatConverter.cs
│   │   └── ... (共12个转换器)
│   │
│   └── Assets/                         # 资源文件
│       ├── Styles/AppTheme.axaml       # 应用主题
│       ├── Config/ide_detection.json   # IDE检测规则配置
│       └── app-icon.ico
│
├── ProjectHub.McpServer/               # MCP服务器
│   ├── McpServer.cs
│   ├── Program.cs
│   └── Transport/StdioTransport.cs
│
├── docs/                               # 项目文档
│
├── ProjectHub.sln                      # 解决方案文件
└── README.md
```

## 快速开始

### 环境要求

- .NET 8 SDK 或更高版本
- 支持 Windows / macOS / Linux

### 安装与运行

1. **克隆仓库**
```bash
git clone <repository-url>
cd quick-lunch
```

2. **还原依赖**
```bash
dotnet restore
```

3. **构建项目**
```bash
dotnet build
```

4. **运行应用**
```bash
dotnet run --project ProjectHub.Desktop
```

5. **运行MCP服务器**（可选）
```bash
dotnet run --project ProjectHub.McpServer
```

## 使用说明

### 添加项目
1. 点击主界面的 "+" 按钮
2. 选择项目目录（文件夹）或文件
3. 自动识别项目信息（可手动修改名称、别名、描述）
4. 设置标签
5. 选择默认IDE（从已配置的IDE列表中选择，IDE列表从数据库动态加载）
6. 设置收藏状态
7. 保存

### 快速打开项目
1. 使用全局快捷键打开快速启动器
2. 输入项目名称、别名或标签进行搜索
3. 按 Enter 键使用默认IDE打开
4. 或使用方向键选择后按 Enter

### 管理工作区
1. 在左侧导航点击 "+" 创建新工作区
2. 设置工作区名称和描述
3. 从项目列表中选择多个项目
4. 工作区自动继承项目的标签，也可添加自定义标签
5. 一键批量打开所有项目

### 配置IDE
1. 点击"IDE设置"按钮
2. 点击"自动检测"扫描系统中已安装的IDE
3. 或手动添加自定义IDE（输入名称、选择可执行文件路径、设置启动参数）
4. IDE设置中可更新或删除已配置的IDE

> **注意**：当前自动检测主要支持 VS Code 系列 IDE（VS Code、Cursor、Windsurf、VSCodium）以及 Trae，其他支持命令行打开文件夹的编辑器可手动添加。

### 收藏功能
- 项目和工作区均支持收藏
- 在添加/编辑对话框中勾选"收藏"
- 在列表中通过右键菜单 → "收藏"/"取消收藏"切换
- 点击左侧导航"收藏"查看所有已收藏的项目和工作区

### 标签筛选
- 点击左侧导航中的标签可筛选包含该标签的项目和工作区
- 工作区的标签自动从包含的项目继承（可关闭）
- 支持自定义工作区标签

## 开发路线

### Phase 1: MVP核心功能
- [x] 项目初始化和数据模型（领域模型与EF实体分离）
- [x] 主窗口框架和导航
- [x] 项目CRUD操作（添加/编辑/删除）
- [x] IDE配置和管理
- [x] 基础搜索功能

### Phase 2: 快速启动器
- [x] 快速启动器窗口UI
- [x] 实时搜索交互
- [x] 失焦自动关闭

### Phase 3: 高级功能
- [x] 标签系统和着色
- [x] 工作区管理（CRUD + 标签继承 + 批量打开）
- [x] 收藏功能（项目 + 工作区）
- [x] 最近访问排序
- [x] 卡片视图
- [x] IDE自动检测（配置驱动）
- [x] 主题切换（亮色/暗色，基于 Semi.Avalonia）
- [ ] 数据导入导出
- [ ] 拼音搜索

### Phase 4: MCP支持
- [x] MCP Server框架搭建
- [x] 所有MCP工具实现
- [ ] MCP使用文档

### Phase 5: 优化与发布
- [ ] 性能优化
- [ ] 系统托盘和开机自启
- [ ] 跨平台适配测试（macOS / Linux）
- [ ] 安装包制作

## 文档

| 文档 | 说明 |
|------|------|
| [项目概述](docs/01_project_overview.md) | 项目定位和需求分析 |
| [技术选型](docs/02_tech_stack.md) | 技术栈选择理由 |
| [功能模块](docs/03_features.md) | 功能架构和优先级 |
| [交互设计](docs/04_ui_design.md) | UI交互规范 |
| [数据模型](docs/05_data_model.md) | 领域模型和EF实体定义 |
| [MCP支持](docs/06_mcp_support.md) | MCP工具定义 |
| [项目结构](docs/07_project_structure.md) | 目录结构说明 |
| [开发路线](docs/08_development_plan.md) | 开发计划和进度 |
| [项目添加/编辑](docs/10_project_add_edit.md) | 项目对话框交互设计 |
| [项目操作](docs/11_project_operations.md) | 项目编辑和删除流程 |
| [标签添加/编辑](docs/11_tag_add_edit.md) | 标签对话框交互设计 |
| [工作区添加/编辑](docs/12_workspace_add_edit.md) | 工作区对话框交互设计 |
| [IDE设置](docs/13_ide_settings.md) | IDE设置功能设计 |

## 贡献指南

欢迎贡献代码！请遵循以下步骤：

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 致谢

- [Avalonia UI](https://avaloniaui.net/) - 强大的跨平台UI框架
- [Semi.Avalonia](https://github.com/irihitech/Semi.Avalonia) - 字节跳动 Semi Design 风格 Avalonia 主题
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) - 优秀的MVVM框架
- [Material Icons](https://pictogrammers.com/library/mdi/) - 图标库
- [.NET](https://dotnet.microsoft.com/) - 开发平台

---

**Project Hub** - 让项目管理更简单高效
