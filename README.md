# Project Hub (quick-lunch)

一个跨平台的统一项目管理工具，用于管理多个IDE项目，支持快速搜索、标签分类、工作区管理，并提供MCP接口供AI工具调用。

## ✨ 核心特性

- 🎯 **统一入口** - 一个工具管理所有项目，不再依赖各IDE的工作区记录
- ⚡ **快速访问** - 全局快捷键 + 模糊搜索，秒级打开项目
- 🏷️ **灵活配置** - 自定义别名、标签、颜色，支持多种IDE
- 💼 **工作区管理** - 将多个项目组合成工作区，一键批量打开
- 🤖 **AI集成** - 通过MCP协议支持AI工具直接操作项目
- 🔍 **智能搜索** - 支持名称、别名、标签模糊搜索，以及中文拼音搜索

## 🛠 技术栈

| 技术 | 版本 | 说明 |
|------|------|------|
| Avalonia UI | 11.x | 跨平台UI框架 |
| .NET | 8 (LTS) | 运行时 |
| C# | 12 | 编程语言 |
| Entity Framework Core | 8.x | ORM框架 |
| SQLite | - | 本地数据库 |
| CommunityToolkit.Mvvm | 8.2.2 | MVVM框架 |

### 核心依赖

```xml
<!-- 核心框架 -->
<PackageReference Include="Avalonia" Version="11.1.0" />
<PackageReference Include="Avalonia.Desktop" Version="11.1.0" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.1.0" />

<!-- MVVM -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />

<!-- 数据库 -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />

<!-- JSON处理 -->
<PackageReference Include="System.Text.Json" Version="8.0.0" />

<!-- MCP支持 -->
<PackageReference Include="ModelContextProtocol" Version="*" />
```

## 📋 功能模块

### 项目管理
- ✅ 添加/编辑/删除项目
- ✅ 设置项目别名和描述
- ✅ 自定义标签和颜色标记
- ✅ 收藏常用项目
- ✅ 记录访问历史和打开次数

### 快速搜索
- ✅ 名称模糊匹配
- ✅ 别名搜索
- ✅ 标签筛选
- ✅ 最近访问排序
- ✅ 中文拼音搜索（P2）

### IDE管理
- ✅ 支持多IDE配置（VS Code、Trae、Trae-CN等）
- ✅ 自动检测已安装的IDE
- ✅ 自定义IDE添加
- ✅ IDE启动参数配置
- ✅ 右键菜单快速切换IDE

### 工作区管理
- ✅ 创建多项目工作区
- ✅ 工作区标签继承机制
- ✅ 批量打开工作区项目
- ✅ 自定义工作区标签

### 数据管理
- ✅ SQLite本地存储
- ✅ JSON格式导入导出
- ✅ 数据迁移支持

### MCP服务（AI集成）
- ✅ `list_projects` - 获取项目列表
- ✅ `open_project` - 打开指定项目
- ✅ `search_projects` - 搜索项目
- ✅ `get_recent_projects` - 查询最近项目
- ✅ `add_project` - 添加新项目

## 🎨 界面设计

### 双模式交互

#### 快速启动器模式
- 无边框半透明窗口
- 屏幕居中显示（600x500）
- 键盘驱动操作
- 失焦自动关闭
- ESC键快速退出

#### 管理界面模式
- 完整功能的管理界面
- 左侧导航面板
- 多视图切换（卡片/列表/紧凑）
- 鼠标友好的操作界面

### 视图模式

| 视图 | 说明 |
|------|------|
| 卡片视图 | 大卡片，显示详细信息 |
| 列表视图 | 紧凑列表，显示关键信息 |
| 紧凑视图 | 最小化显示，仅名称 |

## 📁 项目结构

```
ProjectHub/
│
├── ProjectHub.Core/                    # 核心类库
│   ├── Models/                         # 数据模型
│   │   ├── Project.cs
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
│   ├── Views/                          # 视图
│   │   ├── MainWindow.axaml            # 主窗口（管理界面）
│   │   ├── QuickLauncherWindow.axaml   # 快速启动器
│   │   ├── Controls/                   # 用户控件
│   │   ├── Dialogs/                    # 对话框
│   │   └── Pages/                      # 页面
│   │
│   ├── ViewModels/                     # 视图模型
│   ├── Services/                       # 服务实现
│   ├── Converters/                     # 值转换器
│   └── Assets/                         # 资源文件
│
├── ProjectHub.McpServer/               # MCP服务器
│   ├── Tools/                          # MCP工具实现
│   └── Transport/                      # 传输层
│
├── ProjectHub.Tests/                   # 单元测试
│
├── docs/                               # 项目文档
│
├── ProjectHub.sln                      # 解决方案文件
└── README.md
```

## 🚀 快速开始

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

### 运行测试

```bash
dotnet test
```

## 📖 使用说明

### 添加项目
1. 点击主界面的 "+" 按钮
2. 选择项目目录
3. 自动识别项目信息（可手动修改）
4. 设置别名、标签、描述等
5. 选择默认IDE
6. 保存

### 快速打开项目
1. 使用全局快捷键打开快速启动器
2. 输入项目名称、别名或标签进行搜索
3. 按 Enter 键使用默认IDE打开
4. 或使用方向键选择后按 Enter

### 管理工作区
1. 在左侧导航选择"工作区"
2. 点击创建新工作区
3. 添加多个项目到工作区
4. 设置自定义标签
5. 一键批量打开所有项目

### 配置IDE
1. 点击"IDE设置"按钮
2. 点击"自动检测"扫描已安装的IDE
3. 或手动添加自定义IDE
4. 配置IDE路径和启动参数

## 🎯 开发路线

### Phase 1: MVP核心功能 ✅
- [x] 项目初始化和数据模型
- [x] 主窗口框架和导航
- [x] 项目CRUD操作
- [x] IDE配置和管理
- [x] 基础搜索功能

### Phase 2: 快速启动器 🚧
- [ ] 快速启动器窗口UI
- [ ] 实时搜索交互
- [ ] 失焦自动关闭

### Phase 3: 高级功能 📋
- [ ] 标签系统和着色
- [ ] 工作区管理完善
- [ ] 收藏和最近访问
- [ ] 数据导入导出
- [ ] 拼音搜索

### Phase 4: MCP支持 📋
- [ ] MCP Server框架搭建
- [ ] 所有MCP工具实现
- [ ] MCP使用文档

### Phase 5: 优化与发布 📋
- [ ] 性能优化
- [ ] 主题切换（亮色/暗色）
- [ ] 系统托盘和开机自启
- [ ] 跨平台适配测试
- [ ] 安装包制作

## 🤝 贡献指南

欢迎贡献代码！请遵循以下步骤：

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 🙏 致谢

- [Avalonia UI](https://avaloniaui.net/) - 强大的跨平台UI框架
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) - 优秀的MVVM框架
- [.NET](https://dotnet.microsoft.com/) - 开发平台

---

**Project Hub** - 让项目管理更简单高效 ⚡
