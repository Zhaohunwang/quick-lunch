# IDE设置功能设计

## 功能概述

IDE设置功能允许用户配置和管理项目中使用的IDE（集成开发环境），支持自动检测系统中已安装的IDE，以及手动添加自定义IDE。

## 支持的IDE

| IDE | 标识 | 图标 | 基础 | 自动检测路径 |
|-----|------|------|------|-------------|
| Visual Studio Code | vscode | 🆚 | VS Code | `%PATH%\code.exe`<br>`%ProgramFiles%\Microsoft VS Code\code.exe`<br>`%LocalAppData%\Programs\Microsoft VS Code\code.exe` |
| Cursor | cursor | 🖱️ | VS Code | `%PATH%\cursor.exe`<br>`%LocalAppData%\Programs\cursor\cursor.exe`<br>`%LocalAppData%\cursor-user\cursor.exe` |
| Windsurf | windsurf | 🌊 | VS Code | `%PATH%\Windsurf.exe`<br>`%LocalAppData%\Programs\Windsurf\Windsurf.exe` |
| VSCodium | vscodium | 📦 | VS Code | `%PATH%\codium.exe`<br>`%ProgramFiles%\VSCodium\codium.exe`<br>`%LocalAppData%\Programs\VSCodium\codium.exe` |
| Trae | trae | 🤖 | 独立 | `%PATH%\Trae.exe`<br>`%LocalAppData%\Trae\Trae.exe` |
| Trae-CN | trae-cn | 🤖 | 独立 | `%PATH%\Trae.exe`<br>`%LocalAppData%\Trae CN\Trae.exe` |
| 自定义 | custom | ⚙️ | — | 用户手动选择 |

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
1. 扫描PATH环境变量中的可执行文件
2. 检查常见安装路径
3. 自动添加检测到的IDE到列表
4. 跳过已存在的IDE（根据名称判断）

### 2. 添加自定义IDE

用户可以手动添加IDE：
1. 输入IDE名称（必填）
2. 选择可执行文件路径（必填）
   - 点击"浏览..."按钮打开文件选择对话框
   - 支持选择.exe文件
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

用于 `ide_detection.json` 配置文件，驱动 IDE 自动检测。

```csharp
public class IdeDetectionRule
{
    public string Id { get; set; }           // 唯一标识：vscode, cursor, ...
    public string Name { get; set; }         // 显示名称："VS Code"
    public string ExeName { get; set; }      // 可执行文件名："code.exe"
    public List<string> SearchPaths { get; set; }  // 额外搜索目录（支持%Var%环境变量）
    public string? Icon { get; set; }        // 图标 emoji
    public int Priority { get; set; }        // 检测优先级（越小越优先）
    public string? DefaultArgs { get; set; } // 默认启动参数
    public List<string> SupportedExtensions { get; set; } // 支持的文件扩展名
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
加载优先级：内置(Embedded Resource) → 外部(%AppData%/ProjectHub/) 覆盖合并
```

```csharp
public class IdeDetectionService
{
    // 主入口：读取配置 → 逐条检测 → 按 Priority 排序返回
    public List<IdeTemplate> DetectInstalledIdes()
    
    // 配置加载（带缓存）
    private List<IdeDetectionRule> GetDetectionRules()
    private List<IdeDetectionRule> LoadEmbeddedConfig()   // 内置默认
    private List<IdeDetectionRule>? LoadExternalConfig()  // 用户覆盖/扩展
    private static List<IdeDetectionRule> ParseConfig(Stream)  // JSON解析
    
    // 检测引擎
    private string? WhereIs(string exeName)              // where.exe 系统级查找（首选）
    private string? FindExecutable(string exeName, string[] searchDirs)  // 三级 fallback 链路
    private string ExpandEnvironmentVars(string path)    // %ProgramFiles% → 实际路径
}
```

### ide_detection.json 配置文件格式

**位置**：
- 内置：编译嵌入为 Embedded Resource (`Assets/Config/ide_detection.json`)
- 外部：`%AppData%\ProjectHub\ide_detection.json`（可选，用于用户自定义扩展）

**合并策略**：外部配置中相同 `id` 的规则会覆盖内置规则；外部独有的 `id` 会追加到列表末尾。

**JSON 结构示例**：
```json
{
  "$comment": "IDE自动检测规则配置。用户可在 %AppData%/ProjectHub/ide_detection.json 覆盖或扩展",
  "ides": [
    {
      "id": "vscode",
      "name": "VS Code",
      "exeName": "code.exe",
      "icon": "🆚",
      "priority": 1,
      "defaultArgs": "",
      "searchPaths": [
        "%ProgramFiles%\\Microsoft VS Code",
        "%LocalAppData%\\Programs\\Microsoft VS Code"
      ],
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
| `exeName` | ✅ | 可执行文件名（如 `code.exe`） |
| `searchPaths` | ❌ | 额外搜索目录列表，支持 `%Var%` 环境变量 |
| `icon` | ❌ | 图标 emoji，默认 `💻` |
| `priority` | ❌ | 检测优先级（数字越小越靠前），默认 `99` |
| `defaultArgs` | ❌ | 默认启动参数 |
| `supportedExtensions` | ❌ | 支持的文件扩展名列表 |

**添加自定义 IDE 示例**（在外部配置文件中追加）：
```json
{
  "ides": [
    {
      "id": "intellij",
      "name": "IntelliJ IDEA",
      "exeName": "idea64.exe",
      "icon": "📘",
      "priority": 10,
      "searchPaths": [
        "%ProgramFiles%\\JetBrains\\IntelliJ IDEA *.*"
      ],
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

### 通用：where 命令优先（Windows 内置）

所有 IDE 检测的第一步均使用 Windows 内置的 `where.exe` 命令，它具备以下优势：

- **自动处理 PATHEXT** — 无需手动拼接 `.exe` 后缀
- **识别 App Execution Alias** — 如 Microsoft Store 安装的 VS Code 通过别名解析
- **注册表关联感知** — 能找到通过注册表 `App Paths` 注册的程序
- **多结果返回** — 取第一个匹配项作为检测结果

```csharp
// 实现示例
private string? WhereIs(string exeName)
{
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "where.exe",
            Arguments = $"\"{exeName}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        }
    };
    process.Start();
    var output = process.StandardOutput.ReadToEnd().Trim();
    process.WaitForExit(timeout: 3000);
    
    if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
    {
        return output.Split('\n')[0].Trim(); // 取第一个匹配路径
    }
    return null;
}
```

**Fallback 链路**：
```
where.exe code.exe → 未找到 → 手动遍历 PATH 环境变量 → 扫描常见安装目录 → 返回 null
```

### VS Code检测

1. **PATH环境变量检测**
   ```csharp
   var pathEnv = Environment.GetEnvironmentVariable("PATH");
   var paths = pathEnv.Split(Path.PathSeparator);
   foreach (var path in paths)
   {
       var codePath = Path.Combine(path, "code.exe");
       if (File.Exists(codePath)) return codePath;
   }
   ```

2. **常见路径检测**
   - `%ProgramFiles%\Microsoft VS Code\code.exe`
   - `%LocalAppData%\Programs\Microsoft VS Code\code.exe`
   - `%UserProfile%\AppData\Local\Programs\Microsoft VS Code\code.exe`

### Cursor检测（VS Code 内核）

Cursor 是基于 VS Code 的 AI 优先代码编辑器，可执行文件为 `cursor.exe`。

1. **PATH环境变量检测** — 查找 `cursor.exe`

2. **常见安装路径检测**
   - `%LocalAppData%\Programs\cursor\cursor.exe`
   - `%LocalAppData%\cursor-user\cursor.exe`（用户级安装）
   - `%ProgramFiles%\cursor\cursor.exe`（系统级安装）

3. **注册表辅助检测**
   - `HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall` 中搜索 Cursor

### Windsurf检测（VS Code 内核）

Windsurf（原 Codeium）是基于 VS Code 的 AI 驱动编辑器，可执行文件为 `Windsurf.exe`。

1. **PATH环境变量检测** — 查找 `Windsurf.exe`

2. **常见安装路径检测**
   - `%LocalAppData%\Programs\Windsurf\Windsurf.exe`
   - `%ProgramFiles%\Windsurf\Windsurf.exe`

3. **快捷方式解析**
   - 桌面/开始菜单中查找 Windsurf 快捷方式并解析目标路径

### VSCodium检测（VS Code 内核）

VSCodium 是 VS Code 的开源无遥测版本，可执行文件为 `codium.exe`。

1. **PATH环境变量检测** — 查找 `codium.exe`

2. **常见安装路径检测**
   - `%ProgramFiles%\VSCodium\codium.exe`
   - `%LocalAppData%\Programs\VSCodium\codium.exe`
   - `%LocalAppData\VSCodium\codium.exe`

### Trae检测

1. **PATH环境变量检测**
   - 查找 `Trae.exe`

2. **常见路径检测**
   - `%LocalAppData%\Trae\Trae.exe`
   - `%ProgramFiles%\Trae\Trae.exe`

### Trae-CN检测

1. **PATH环境变量检测**
   - 查找 `Trae.exe`

2. **常见路径检测**
   - `%LocalAppData%\Trae CN\Trae.exe`
   - `%ProgramFiles%\Trae CN\Trae.exe`

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
