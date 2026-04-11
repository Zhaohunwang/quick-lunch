# 项目结构

## 目录结构

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
│   ├── 01_project_overview.md
│   ├── 02_tech_stack.md
│   ├── 03_features.md
│   ├── 04_ui_design.md
│   ├── 05_data_model.md
│   ├── 06_mcp_support.md
│   ├── 07_project_structure.md
│   ├── 08_development_plan.md
│   ├── 09_appendix.md
│   └── 10_add_edit_interaction.md
│
├── ProjectHub.sln                      # 解决方案文件
├── README.md
└── .gitignore
```