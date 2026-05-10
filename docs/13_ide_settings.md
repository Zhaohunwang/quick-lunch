# IDE设置功能设计

## 功能概述

IDE设置功能允许用户配置和管理项目中使用的IDE（集成开发环境），支持自动检测系统中已安装的IDE，以及手动添加自定义IDE。

## 支持的IDE

### Windows

| IDE | 标识 | 图标 | 可执行文件 | 搜索路径 |
|-----|------|------|-----------|---------|
| Visual Studio Code | vscode | 🆚 | `code.exe` | `%ProgramFiles%\Microsoft VS Code`、`%LocalAppData%\Programs\Microsoft VS Code` |
| Cursor | cursor | 🖱️ | `cursor.exe` | `%LocalAppData%\Programs\cursor`、`%LocalAppData%\cursor-user`、`%ProgramFiles%\cursor` |
| Windsurf | windsurf | 🌊 | `Windsurf.exe` | `%LocalAppData%\Programs\Windsurf`、`%ProgramFiles%\Windsurf` |
| VSCodium | vscodium | 📦 | `codium.exe` | `%ProgramFiles%\VSCodium`、`%LocalAppData%\Programs\VSCodium` |
| Trae | trae | 🤖 | `Trae.exe` | `%LocalAppData%\Trae`、`%ProgramFiles%\Trae` |
| Trae-CN | trae-cn | 🤖 | `Trae.exe` | `%LocalAppData%\Trae CN`、`%ProgramFiles%\Trae CN` |

### macOS

| IDE | 标识 | 图标 | 可执行文件 | 搜索路径 |
|-----|------|------|-----------|---------|
| Visual Studio Code | vscode | 🆚 | `code` | `/Applications/Visual Studio Code.app/Contents/Resources/app/bin` |
| Cursor | cursor | 🖱️ | `cursor` | `/Applications/Cursor.app/Contents/Resources/app/bin` |
| Windsurf | windsurf | 🌊 | `windsurf` | `/Applications/Windsurf.app/Contents/Resources/app/bin` |
| VSCodium | vscodium | 📦 | `codium` | `/Applications/VSCodium.app/Contents/Resources/app/bin`、`/usr/local/bin/codium` |
| Trae | trae | 🤖 | `trae` | `/Applications/Trae.app/Contents/Resources/app/bin` |
| Trae-CN | trae-cn | 🤖 | `trae-cn` | `/Applications/Trae CN.app/Contents/Resources/app/bin` |

### Linux

| IDE | 标识 | 图标 | 可执行文件 | 搜索路径 |
|-----|------|------|-----------|---------|
| Visual Studio Code | vscode | 🆚 | `code` | `/usr/share/code`、`/snap/code/current/usr/share/code`、`/usr/lib/code` |
| Cursor | cursor | 🖱️ | `cursor` | `/usr/share/cursor`、`/opt/Cursor/resources/app/bin` |
| Windsurf | windsurf | 🌊 | `windsurf` | `/usr/share/windsurf`、`/opt/Windsurf/resources/app/bin` |
| VSCodium | vscodium | 📦 | `codium` | `/usr/share/codium`、`/snap/codium/current/usr/share/codium` |
| Trae | trae | 🤖 | `trae` | `/usr/share/trae`、`/opt/Trae/resources/app/bin` |
| Trae-CN | trae-cn | 🤖 | `trae-cn` | `/usr/share/trae-cn`、`/opt/Trae CN/resources/app/bin` |

### 自定义

| 标识 | 说明 |
|------|------|
| custom | ⚙️ 用户手动选择可执行文件路径 |

> **VS Code 系列 IDE**：Cursor、Windsurf、VSCodium 均基于 VS Code 内核构建，共享相同的扩展生态和操作习惯。

## 界面设计

### IDE设置对话框

```
┌─────────────────────────────────────────────────────────────┐
│  ⚙️ IDE设置                                    [-] [□] [×]   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────────────────┐  ┌─────────────────────────────┐│
│  │ 已安装的IDE             │  │ 添加自定义IDE               ││
│  │                         │  │                             ││
│  │ ┌─────────────────────┐ │  │ IDE名称 *                   ││
│  │ │ 🔍 自动检测         │ │  │ ┌─────────────────────────┐ ││
│  │ └─────────────────────┘ │  │ │ 例如：IntelliJ IDEA      │ ││
│  │                         │  │ └─────────────────────────┘ ││
│  │ 🆚 VS Code              │  │                             ││
│  │ C:\Program Files\...    │  │ 可执行文件路径 *            ││
│  │                         │  │ ┌─────────────────────┐ ┌─┐ ││
│  │ 🤖 Trae                 │  │ │ 选择IDE可执行文件   │ │浏│ ││
│  │ C:\Users\AppData\...    │  │ └─────────────────────┘ └─┘ ││
│  │                         │  │                             ││
│  │ 🤖 Trae-CN              │  │ 默认参数                    ││
│  │ C:\Users\AppData\...    │  │ ┌─────────────────────────┐ ││
│  │                         │  │ │ 例如：--new-window      │ ││
│  │                         │  │ └─────────────────────────┘ ││
│  │                         │  │                             ││
│  │                         │  │ [添加IDE]                   ││
│  │                         │  │                             ││
│  │                         │  │ [更新选中的IDE]             ││
│  │                         │  │ [删除选中的IDE]             ││
│  │                         │  │                             ││
│  └─────────────────────────┘  └─────────────────────────────┘│
│                                                              │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │                                                         │ │
│  │ 错误信息/成功提示                                       │ │
│  │                                                         │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                              │
│                                    [关闭]                    │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## 功能说明

### 1. 自动检测IDE

点击"🔍 自动检测"按钮，系统会：
1. 根据当前操作系统平台，从配置中选取对应的可执行文件名和搜索路径
2. 先通过系统命令查找：Windows 使用 `where.exe`，macOS/Linux 使用 `which`
3. 再按优先级依次搜索搜索路径目录（macOS 的 `.app` 包会通过 `Directory.Exists` 检测）
4. 自动添加检测到的IDE到列表
5. 跳过已存在的IDE（根据名称判断）

### 2. 添加自定义IDE

用户可以手动添加IDE：
1. 输入IDE名称（必填）
2. 选择可执行文件路径（必填）
   - 点击"浏览..."按钮打开文件选择对话框
   - Windows：筛选 `.exe`/`.bat`/`.cmd` 文件
   - macOS/Linux：显示所有文件（IDE 可能是 `.app` 包或 `/usr/local/bin` 下的脚本）
3. 输入默认参数（可选）
   - 例如：`--new-window` 用于在新窗口打开
4. 点击"添加IDE"按钮

**验证规则：**
- IDE名称不能为空
- IDE名称不能重复（不区分大小写）
- 可执行文件路径必须存在

### 3. 更新IDE配置

1. 从左侧列表选择一个IDE
2. 修改右侧表单中的信息
3. 点击"更新选中的IDE"按钮
4. 系统保存更改

### 4. 删除IDE

1. 从左侧列表选择一个IDE
2. 点击"删除选中的IDE"按钮
3. 系统从列表中移除该IDE

**注意：** 删除IDE不会影响已关联的项目，只是从可用IDE列表中移除。

## 数据结构

### IdeDetectionRule（IDE检测规则）

用于 `ide_detection.json` 配置文件，驱动 IDE 自动检测。采用平台分组结构，每个 IDE 规则按 `windows`/`macOS`/`linux` 分别指定可执行文件名和搜索路径。

```csharp
public class IdeDetectionRule
{
    public string Id { get; set; }                  // 唯一标识：vscode, cursor, ...
    public string Name { get; set; }                // 显示名称："VS Code"
    public PlatformValue ExeName { get; set; }      // 按平台的可执行文件名
    public PlatformPaths SearchPaths { get; set; }  // 按平台的搜索目录
    public string? Icon { get; set; }               // 图标 emoji
    public int Priority { get; set; }               // 检测优先级（越小越优先）
    public string? DefaultArgs { get; set; }        // 默认启动参数
    public List<string> SupportedExtensions { get; set; } // 支持的文件扩展名
}

public class PlatformValue
{
    public string? Windows { get; set; }   // Windows 可执行文件名（如 "code.exe"）
    public string? MacOS { get; set; }     // macOS 可执行文件名（如 "code"）
    public string? Linux { get; set; }     // Linux 可执行文件名（如 "code"）
}

public class PlatformPaths
{
    public List<string> Windows { get; set; } = new();  // Windows 搜索路径（支持 %Var%）
    public List<string> MacOS { get; set; } = new();    // macOS 搜索路径（支持 ~ 展开）
    public List<string> Linux { get; set; } = new();    // Linux 搜索路径
}
```

### IdeTemplate（IDE模板）

```csharp
public class IdeTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// IDE名称（如：VS Code、Trae）
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 可执行文件完整路径
    /// </summary>
    public string ExecutablePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 默认启动参数
    /// </summary>
    public string? DefaultArgs { get; set; }
    
    /// <summary>
    /// 图标（emoji或图标路径）
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// 支持的文件扩展名
    /// </summary>
    public List<string> SupportedExtensions { get; set; } = new();
    
    /// <summary>
    /// 优先级（用于排序）
    /// </summary>
    public int Priority { get; set; }
}
```

### IdeDetectionService（IDE检测服务）

**配置驱动架构**：从 `ide_detection.json` 读取检测规则，通用循环检测，无需硬编码。

```
加载优先级：内置(Embedded Resource) → 外部(跨平台配置目录) 覆盖合并
外部配置路径：Windows: %AppData%/ProjectHub/    macOS/Linux: ~/.config/ProjectHub/
```

```csharp
public class IdeDetectionService
{
    // 主入口：读取配置 → 按平台选取 exeName/searchPaths → 检测 → 按 Priority 排序返回
    public List<IdeTemplate> DetectInstalledIdes()

    // 平台适配
    private static string? GetPlatformValue(PlatformValue value)   // 按平台取可执行文件名
    private static List<string> GetPlatformPaths(PlatformPaths paths) // 按平台取搜索路径

    // 配置加载（带缓存）
    private List<IdeDetectionRule> GetDetectionRules()
    private List<IdeDetectionRule> LoadEmbeddedConfig()   // 内置默认
    private List<IdeDetectionRule>? LoadExternalConfig()  // 用户覆盖/扩展
    private static List<IdeDetectionRule> ParseConfig(Stream)  // JSON解析（兼容对象和字符串格式）

    // 检测引擎
    private static string? CommandExists(string command)  // Windows: where.exe / macOS&Linux: which
    private static string? FindExecutable(string exeName, string[] searchDirs)
    private static string ExpandPath(string path)         // Windows: %Var% / macOS&Linux: ~/$HOME
}
```

### ide_detection.json 配置文件格式

**位置**：
- 内置：编译嵌入为 Embedded Resource (`Assets/Config/ide_detection.json`)
- 外部（用户自定义扩展/覆盖）：
  - Windows：`%AppData%\ProjectHub\ide_detection.json`
  - macOS/Linux：`~/.config/ProjectHub/ide_detection.json`

**合并策略**：外部配置中相同 `id` 的规则会覆盖内置规则；外部独有的 `id` 会追加到列表末尾。

**JSON 结构示例**：
```json
{
  "$comment": "IDE自动检测规则配置。用户可在外部配置文件中覆盖或扩展",
  "ides": [
    {
      "id": "vscode",
      "name": "VS Code",
      "exeName": {
        "windows": "code.exe",
        "macOS": "code",
        "linux": "code"
      },
      "searchPaths": {
        "windows": [
          "%ProgramFiles%\\Microsoft VS Code",
          "%LocalAppData%\\Programs\\Microsoft VS Code"
        ],
        "macOS": [
          "/Applications/Visual Studio Code.app/Contents/Resources/app/bin"
        ],
        "linux": [
          "/usr/share/code",
          "/snap/code/current/usr/share/code"
        ]
      },
      "icon": "🆚",
      "priority": 1,
      "defaultArgs": "",
      "supportedExtensions": [".cs", ".ts", ".js", ".py"]
    }
  ]
}
```

**字段说明**：

| 字段 | 必填 | 说明 |
|------|------|------|
| `id` | ✅ | 唯一标识，用于去重和外部覆盖匹配 |
| `name` | ✅ | IDE 显示名称 |
| `exeName` | ✅ | 按平台的可执行文件名对象（含 `windows`/`macOS`/`linux` 字段） |
| `searchPaths` | ❌ | 按平台的搜索目录列表对象 |
| `icon` | ❌ | 图标 emoji，默认 `💻` |
| `priority` | ❌ | 检测优先级（数字越小越靠前），默认 `99` |
| `defaultArgs` | ❌ | 默认启动参数 |
| `supportedExtensions` | ❌ | 支持的文件扩展名列表 |

> **路径展开**：Windows 路径中的 `%ProgramFiles%` 等环境变量会被自动展开；macOS/Linux 路径中的 `~` 会被展开为用户主目录。

**添加自定义 IDE 示例**（在外部配置文件中追加）：
```json
{
  "ides": [
    {
      "id": "intellij",
      "name": "IntelliJ IDEA",
      "exeName": {
        "windows": "idea64.exe",
        "macOS": "idea",
        "linux": "idea"
      },
      "searchPaths": {
        "windows": ["%ProgramFiles%\\JetBrains\\IntelliJ IDEA *.*"],
        "macOS": ["/Applications/IntelliJ IDEA.app/Contents/Resources/app/bin"],
        "linux": ["/opt/idea/bin"]
      },
      "icon": "📘",
      "priority": 10,
      "supportedExtensions": [".java", ".kt", ".scala"]
    }
  ]
}
```

### IdeSettingsDialogViewModel（IDE设置ViewModel）

```csharp
public partial class IdeSettingsDialogViewModel : ObservableObject
{
    // 属性
    public ObservableCollection<IdeTemplate> AvailableIdes { get; }
    public IdeTemplate? SelectedIde { get; set; }
    public string NewIdeName { get; set; }
    public string NewIdePath { get; set; }
    public string NewIdeArgs { get; set; }
    public string ErrorMessage { get; set; }
    
    // 命令
    [RelayCommand]
    private async Task DetectIdes()           // 自动检测IDE
    
    [RelayCommand]
    private async Task AddCustomIde()         // 添加自定义IDE
    
    [RelayCommand]
    private void SelectIde(IdeTemplate? ide)  // 选择IDE
    
    [RelayCommand]
    private async Task UpdateSelectedIde()    // 更新选中的IDE
    
    [RelayCommand]
    private async Task DeleteSelectedIde()    // 删除选中的IDE
    
    [RelayCommand]
    private async Task BrowseIdePath()        // 浏览选择IDE路径
}
```

## 检测策略详解

### 跨平台命令查找（CommandExists）

检测的第一步通过系统命令在 PATH 中查找可执行文件：

- **Windows**：使用 `where.exe`（内置命令，自动处理 PATHEXT、App Execution Alias）
- **macOS/Linux**：使用 `which`（POSIX 标准命令）

```csharp
private static string? CommandExists(string command)
{
    var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    var psi = new ProcessStartInfo
    {
        FileName = isWindows ? "where.exe" : "which",
        Arguments = $"\"{command}\"",
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    // 优先级：最高，因为系统 PATH 已包含所有标准安装位置
}
```

### 路径展开（ExpandPath）

不同平台使用不同的路径变量语法：

| 平台 | 变量语法 | 展开方式 |
|------|---------|---------|
| Windows | `%ProgramFiles%`、`%LocalAppData%` | `Environment.GetEnvironmentVariable()` |
| macOS/Linux | `~`、`$HOME` | `Environment.GetFolderPath()` + 字符串替换 |

### 搜索路径查找（FindExecutable）

对配置中 `searchPaths` 的每个目录按优先级搜索：
1. **Windows**：`File.Exists()` 检查可执行文件
2. **macOS/Linux**：`Directory.Exists()` 检查 `.app` 包目录 + `File.Exists()` 检查 CLI 工具

### macOS 特殊处理

macOS 的 `.app` 实际上是一个**目录**而非文件：
```
/Applications/Visual Studio Code.app/          ← 这是一个目录！
  └── Contents/
      └── Resources/
          └── app/
              └── bin/
                  └── code                      ← 实际的可执行文件
```

检测逻辑通过 `Directory.Exists()` 检查 `.app` 路径是否存在于搜索路径中，而不是用 `File.Exists()`。

### 跨平台 IDE 启动

**核心原则：** 项目路径/工作区文件路径必须作为**命令行参数**传入，而非仅设为 `WorkingDirectory`。

**项目打开（IdeLauncherService）：**

| 平台 | 场景 | 启动方式 |
|------|------|---------|
| macOS | `.app` 包目录 | `open -a "/Applications/IDE.app" "/path/to/project"` |
| macOS | CLI 工具（如 `code`） | `code "/path/to/project"` |
| Linux | CLI 工具存在 | `code "/path/to/project"` |
| Linux | 兜底 | `gio open "/path/to/project"` |
| Windows | 所有 | `Process.Start` + `UseShellExecute=true` + `WorkingDirectory` |

**工作区打开（WorkspaceLauncherService）：**

| 平台 | 场景 | 启动方式 |
|------|------|---------|
| macOS | `.app` 包目录 | `open -a "/Applications/IDE.app" "/tmp/ProjectHub/Workspaces/xxx.code-workspace"` |
| macOS | CLI 工具 | `code "/tmp/ProjectHub/Workspaces/xxx.code-workspace"` |
| Linux | CLI 工具 | `code "/tmp/ProjectHub/Workspaces/xxx.code-workspace"` |
| Windows | 所有 | `Process.Start` + `UseShellExecute=true` + `.code-workspace` 文件作为参数 |

### Windows 内置的 where 命令优势

- **自动处理 PATHEXT** — 无需手动拼接 `.exe` 后缀
- **识别 App Execution Alias** — 如 Microsoft Store 安装的 VS Code 通过别名解析
- **注册表关联感知** — 能找到通过注册表 `App Paths` 注册的程序
- **多结果返回** — 取第一个匹配项作为检测结果

### Fallback 链路

```
系统命令(where/which) → 未找到 → 手动搜索配置中的搜索路径 → 扫描 PATH 环境变量 → 返回 null
```

## 使用流程

### 首次使用

1. 打开主界面
2. 点击左侧"⚙️ IDE设置"
3. 点击"🔍 自动检测"按钮
4. 系统自动检测并添加已安装的IDE
5. 如需添加其他IDE，使用右侧表单手动添加
6. 点击"关闭"保存设置

### 添加自定义IDE

1. 打开IDE设置对话框
2. 在右侧表单输入：
   - IDE名称（如：IntelliJ IDEA）
   - 点击"浏览..."选择可执行文件
   - 可选：输入默认参数
3. 点击"添加IDE"按钮
4. 新IDE出现在左侧列表

### 修改IDE配置

1. 打开IDE设置对话框
2. 从左侧列表点击要修改的IDE
3. 在右侧修改信息（目前通过删除后重新添加实现）
4. 点击"更新选中的IDE"按钮

### 删除IDE

1. 打开IDE设置对话框
2. 从左侧列表点击要删除的IDE
3. 点击"删除选中的IDE"按钮
4. 确认删除

## 样式定义

### IDE列表项样式

```xml
<Style Selector="Button.IdeListItem">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="BorderBrush" Value="#E9ECEF" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="CornerRadius" Value="4" />
    <Setter Property="Foreground" Value="#495057" />
    <Setter Property="Padding" Value="12,8" />
    <Setter Property="HorizontalContentAlignment" Value="Stretch" />
    <Setter Property="HorizontalAlignment" Value="Stretch" />
</Style>
<Style Selector="Button.IdeListItem:pointerover">
    <Setter Property="Background" Value="#F8F9FA" />
    <Setter Property="BorderBrush" Value="#DEE2E6" />
</Style>
<Style Selector="Button.IdeListItem:pressed">
    <Setter Property="Background" Value="#E9ECEF" />
</Style>
```

### 危险按钮样式（删除按钮）

```xml
<Style Selector="Button.DangerButton">
    <Setter Property="Background" Value="#DC3545" />
    <Setter Property="BorderBrush" Value="#DC3545" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="CornerRadius" Value="4" />
    <Setter Property="Foreground" Value="#FFFFFF" />
    <Setter Property="Padding" Value="12,8" />
    <Setter Property="FontSize" Value="13" />
</Style>
<Style Selector="Button.DangerButton:pointerover">
    <Setter Property="Background" Value="#C82333" />
</Style>
```

## 注意事项

1. **路径验证**：添加IDE时会验证可执行文件是否存在
2. **名称唯一性**：IDE名称不能重复，不区分大小写
3. **自动检测覆盖**：自动检测不会覆盖已存在的IDE配置
4. **删除确认**：删除IDE前最好有确认提示（待实现）
5. **参数格式**：默认参数不需要包含项目路径，系统会自动追加

## 后续优化

1. 添加IDE图标支持（目前使用emoji）
2. 支持拖拽排序IDE列表
3. 添加IDE使用统计
4. 支持导入/导出IDE配置
5. 添加更多IDE的自动检测（如：Visual Studio、Rider、WebStorm、Sublime Text 等）
