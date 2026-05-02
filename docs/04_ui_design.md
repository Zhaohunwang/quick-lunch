# 交互设计

## 设计理念

采用**双模式交互**设计：
- **快速启动器模式**：专注「快速打开」，键盘驱动
- **管理界面模式**：专注「配置管理」，鼠标友好

## 模式切换

## 快速启动器设计

### 界面布局

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
│     │    [🆚 用VS Code打开]  ⋮    │     │
│     ├────────────────────────────┤     │
│     │ ⚙️ 后端API服务               │     │
│     │    别名: api   标签: #java   │     │
│     │    [📘 用IDEA打开]    ⋮     │     │
│     ├────────────────────────────┤     │
│     │ 🎨 设计系统                  │     │
│     │    标签: #figma #ui          │     │
│     │    [🎨 用Figma打开]   ⋮     │     │
│     └────────────────────────────┘     │
│                                        │
│  ↵打开(默认IDE)  ⋮→选择其他IDE  Ctrl+E编辑              │
│                                        │
└────────────────────────────────────────┘
```

### 窗口特性

| 属性 | 值 |
|------|-----|
| 位置 | 屏幕居中 |
| 尺寸 | 600 x 500 |
| 边框 | 无边框 |
| 背景 | 半透明毛玻璃效果 |
| 任务栏 | 不显示 |
| 失焦行为 | 自动关闭 |
| ESC行为 | 关闭窗口 |

## 管理界面设计

### 界面布局

```
┌─────────────────────────────────────────────────────────┐
│  Project Hub                              [-] [□] [×]   │
├─────────────────────────────────────────────────────────┤
│  🔍 搜索项目...              [快速启动] [IDE设置] [+]    │
├──────────────┬──────────────────────────────────────────┤
│              │                                          │
│  📁 全部(24) │   □ 全部项目                    [□ ▤ ☰] │
│  ⭐ 收藏(5)  │   ┌─────────────────────────────────────┐│
│  🕐 最近(10) │   │ 🚀 电商平台重构     [🆚用VS Code][⋮]││
│              │   │    别名: 主站  标签: #vue #frontend ││
│  ─────────── │   ├─────────────────────────────────────┤│
│  🏷️ 标签     │   │ ⚙️ 后端API服务      [🤖用Trae][⋮]  ││
│    #web(8)   │   │    别名: api   标签: #java #spring  ││
│    #java(5)  │   ├─────────────────────────────────────┤│
│    #mobile(3)│   │ 🎨 设计系统          [🎨用Figma][⋮] ││
│              │   │    标签: #design #ui               ││
│  ─────────── │   └─────────────────────────────────────┘│
│  💼 工作区   │                                          │
│    工作区A   │                                          │
│    工作区B   │                                          │
│              │                                          │
│  ─────────── │                                          │
│  ⚙️ 设置     │                                          │
│    IDE设置    │                                          │
│  ┌──────────┐│                                          │
│  │☀️浅色│🌙深色││                                          │
│  └──────────┘│                                          │
└──────────────┴──────────────────────────────────────────┘
```

### 左侧导航结构

```
FolderOpen 全部          - 显示所有项目
Star 收藏               - 显示收藏的项目
ClockOutline 最近       - 按访问时间排序
───────────────
Tag 标签
   #web          - 按标签筛选
   #java
   #mobile
   ...
───────────────
BriefcaseOutline 工作区
   工作区A       - 自定义工作区
   工作区B
───────────────
Cog 设置
   IDE设置       - 配置和管理IDE
   主题切换      - Light / Dark 模式切换
```

### 快速启动功能

**功能说明：**
- **位置**：搜索框右侧的"🚀 快速启动"按钮
- **作用**：快速打开最近访问的项目
- **逻辑**：
  1. 从当前筛选结果中查找最近访问的项目
  2. 按 `LastOpenedAt` 降序排序
  3. 取第一个项目（最近访问的）
  4. 使用默认IDE打开该项目

**查询数据：**
- `FilteredItems`：当前筛选后的项目和工作区列表
- `Project.LastOpenedAt`：项目最后打开时间
- 项目类型：仅处理 `Project` 类型，忽略 `Workspace`

**使用场景：**
- 用户频繁切换项目时，一键快速回到最近的项目
- 配合搜索框使用：先搜索找到目标项目，然后快速打开
- 提高工作效率，减少点击次数

### IDE设置界面

```
┌─────────────────────────────────────────────────────────────┐
│  ⚙️ IDE设置                                    [-] [□] [×]   │
├─────────────────────────────┬───────────────────────────────┤
│  已安装的IDE                │  添加自定义IDE                │
│  ┌───────────────────────┐  │                               │
│  │ 🔍 自动检测           │  │  IDE名称 *                   │
│  └───────────────────────┘  │  ┌─────────────────────────┐  │
│                             │  │ 例如：IntelliJ IDEA      │  │
│  🆚 VS Code                 │  └─────────────────────────┘  │
│  C:\Program Files\...       │                               │
│                             │  可执行文件路径 *             │
│  🤖 Trae                    │  ┌─────────────────────┐ ┌─┐ │
│  C:\Users\AppData\...       │  │ 选择IDE可执行文件   │ │浏│ │
│                             │  └─────────────────────┘ └─┘ │
│  🤖 Trae-CN                 │                               │
│  C:\Users\AppData\...       │  默认参数                    │
│                             │  ┌─────────────────────────┐  │
│                             │  │ 例如：--new-window      │  │
│                             │  └─────────────────────────┘  │
│                             │                               │
│                             │  [添加IDE]                    │
│                             │                               │
│                             │  [更新选中的IDE]              │
│                             │  [删除选中的IDE]              │
│                             │                               │
├─────────────────────────────┴───────────────────────────────┤
│                                  [关闭]                     │
└─────────────────────────────────────────────────────────────┘
```

**功能说明：**
- **左侧**：显示已配置的IDE列表，支持点击选择
- **自动检测**：一键扫描系统中已安装的IDE（VS Code、Trae、Trae-CN）
- **右侧表单**：添加或编辑IDE配置
  - IDE名称：显示名称
  - 可执行文件路径：IDE的.exe文件路径
  - 默认参数：启动时附加的参数（如 `--new-window`）
- **操作按钮**：更新或删除选中的IDE

### 右键菜单

```
┌─────────────────────┐
│ 💻 使用IDE打开    ─┬─→ ✓ 🆚 VS Code      ← 默认IDE
│                   ├─→   🤖 Trae
│                   ├─=>   📘 IntelliJ IDEA
│                   └─=>   ⚙️ Custom IDE
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

**说明：**
- 「💻 使用IDE打开」为子菜单项，悬停或点击后展开所有已配置的IDE列表
- 默认IDE（由项目的 DefaultIdeId 指定）前显示 ✓ 标记
- IDE列表根据系统中已配置的 IdeTemplate 动态生成，非硬编码

### 视图切换

| 视图 | Material Icon | 说明 |
|------|------|------|
| 卡片视图 | ViewGridOutline | 大卡片，显示详细信息 |
| 列表视图 | ViewListOutline | 紧凑列表，显示关键信息 |
| 紧凑视图 | ViewHeadline | 最小化显示，仅名称 |

## 视图详细设计

### 1. 卡片视图 (Card View)

**布局结构：**
- 大卡片布局，白色背景，浅灰色边框，圆角8px
- 内边距16px，外边距8px
- 网格布局，分为4行

**显示内容：**
1. **头部区域**
   - 项目/工作区类型图标（项目→LightningBolt，工作区→BriefcaseOutline，使用 Material Icon）
   - 项目名称（18px，半粗体，超长部分省略截断）
   - 别名（14px，灰色，格式：(别名)，超长部分省略截断）
   - 收藏按钮（Star/StarOutline Material Icon）
   - 更多按钮（⋮，点击显示菜单）

2. **描述区域**
   - 项目描述（13px，灰色，最多2行，超出部分省略）

3. **标签区域**
   - 彩色标签（最多10个）
   - 标签样式：浅蓝色背景，圆角4px，内边距6px

4. **底部区域**
   - 项目路径（11px，灰色，超出部分省略）
   - 最后打开时间（相对时间，如"2小时前"）
   - 默认IDE启动按钮（显示格式：`用 {IDE名称} 打开`，使用项目的DefaultIdeId确定显示哪个IDE）

**交互功能：**
- 点击收藏按钮切换收藏状态
- 点击默认IDE启动按钮使用项目的默认IDE打开项目
- 点击更多按钮（⋮）显示上下文菜单：
  - **💻 使用IDE打开**（子菜单）：展开所有已配置的IDE列表
    - 默认IDE前显示 ✓ 标记
    - 点击任意IDE使用对应IDE打开项目
    - 列表根据IdeTemplate动态渲染
  - ✏️ 编辑：打开编辑对话框
  - 🗑️ 删除：显示确认弹窗，确认后删除项目

**适用场景：**
- 需要查看项目详细信息的场景
- 首次浏览项目列表时

### 2. 列表视图 (List View)

**布局结构：**
- 紧凑列表布局，白色背景，底部边框
- 内边距12px，垂直间距10px
- 水平网格布局，分为3列

**显示内容：**
1. **图标列**
   - 项目/工作区类型图标（Material Icon：项目→LightningBolt，工作区→BriefcaseOutline）

2. **信息列**
   - 项目名称（14px，中等粗细）
   - 别名（12px，灰色，格式：(别名)）
   - 标签（最多3个，紧凑样式）

3. **操作列**
   - 快速启动按钮（使用默认IDE打开）
   - 收藏按钮（Star/StarOutline Material Icon）
   - 更多按钮（⋮，点击显示菜单）

**交互功能：**
- 点击快速启动按钮使用项目的默认IDE打开项目
- 点击收藏按钮切换收藏状态
- 点击更多按钮（⋮）显示上下文菜单：
  - **💻 使用IDE打开**（子菜单）：展开所有已配置的IDE列表
    - 默认IDE前显示 ✓ 标记
    - 点击任意IDE使用对应IDE打开项目
    - 列表根据IdeTemplate动态渲染
  - ✏️ 编辑：打开编辑对话框
  - 🗑️ 删除：显示确认弹窗，确认后删除项目

**适用场景：**
- 浏览多个项目，快速定位目标
- 查看项目基本信息

### 3. 紧凑视图 (Compact View)

**布局结构：**
- 最小化单行布局，白色背景，底部边框
- 内边距8px，垂直间距6px
- 水平网格布局，分为4列

**显示内容：**
1. **收藏列**
   - 收藏图标（Star/StarOutline Material Icon，12px）

2. **类型图标列**
   - 项目/工作区类型图标（14px Material Icon，项目→LightningBolt，工作区→BriefcaseOutline）

3. **名称列**
   - 项目名称（13px，单行，超出部分省略）

4. **操作列**
   - 快速启动按钮（使用默认IDE打开）

**交互功能：**
- 点击/双击行或快速启动按钮使用项目的默认IDE打开项目
- 右键单击显示上下文菜单：
  - **💻 使用IDE打开**（子菜单）：展开所有已配置的IDE列表
    - 默认IDE前显示 ✓ 标记
    - 点击任意IDE使用对应IDE打开项目
    - 列表根据IdeTemplate动态渲染
  - ✏️ 编辑：打开编辑对话框
  - ⭐ 收藏/取消收藏
  - 🗑️ 删除：显示确认弹窗，确认后删除项目

**适用场景：**
- 项目数量很多时，快速浏览和切换
- 已经熟悉项目结构，只需要快速找到项目

### 视图切换逻辑

**切换方式：**
- 点击顶部工具栏的视图切换按钮（□、▤、☰）
- 按钮状态会根据当前视图模式高亮显示

**数据同步：**
- 三种视图共享同一套数据和筛选条件
- 切换视图时，当前的筛选状态（如搜索关键词、标签筛选）会保持不变

**响应式设计：**
- 卡片视图：在宽屏幕上可显示2-3列，窄屏幕自动调整为1列
- 列表视图：自适应宽度，始终显示为单列
- 紧凑视图：自适应宽度，始终显示为单列

## 应用图标

应用图标采用 ICO 格式，文件位于 `Assets/app-icon.ico`。

**设计规范：**
- 多尺寸支持（16x16, 32x32, 48x48, 256x256）
- 风格：扁平化设计，蓝色渐变背景
- 元素：文件夹 + 闪电标志，寓意「快速启动项目」

**集成方式：**
- 在 `MainWindow.axaml` 中通过 `Icon` 属性直接引用：
  ```xml
  Icon="avares://ProjectHub.Desktop/Assets/app-icon.ico"
  ```
- 作为 `AvaloniaResource` 嵌入到程序集中

## 按钮样式系统

### IconButton（卡片内操作按钮）

用于卡片视图中的收藏按钮、更多按钮等。

| 属性 | Light 值 | Dark 值 |
|------|---------|---------|
| Background | Transparent | Transparent |
| Border | 无边框 | 无边框 |
| Padding | 4 | 4 |
| MinSize | 28x28 | 28x28 |
| Hover 背景 | #F0F0F0 | #363650 |
| Hover 前景 | #495057 | #B0B0BC |
| Pressed 背景 | #E0E0E0 | #40405A |

### SmallIconButton（列表/紧凑模式操作按钮）

用于列表视图和紧凑视图中的操作按钮。

| 属性 | Light 值 | Dark 值 |
|------|---------|---------|
| Background | Transparent | Transparent |
| Border | 无边框 | 无边框 |
| Padding | 6,4 | 6,4 |
| Hover 背景 | #F0F0F0 | #363650 |
| Hover 前景 | #495057 | #B0B0BC |
| Pressed 背景 | #E0E0E0 | #40405A |

## 依赖组件

### Material Icons

项目使用 `Material.Icons.Avalonia` 包提供图标支持。

**NuGet 包：**
- `Material.Icons.Avalonia` v2.2.0
- `Material.Icons` v2.2.0

**XAML 命名空间：**
```xml
xmlns:icons="clr-namespace:Material.Icons.Avalonia;assembly=Material.Icons.Avalonia"
xmlns:micons="clr-namespace:Material.Icons;assembly=Material.Icons"
```

**App.axaml 注册：**
```xml
<materialIcons:MaterialIconStyles />
```

**常用图标映射：**
| 用途 | 图标名称 |
|------|---------|
| 项目类型 | LightningBolt |
| 工作区类型 | BriefcaseOutline |
| 收藏 | Star / StarOutline |
| 全部 | FolderOpen |
| 收藏筛选 | Star |
| 最近 | ClockOutline |
| 标签 | Tag |
| IDE设置 | Cog |
| 主题切换(浅色) | WeatherSunny |
| 主题切换(深色) | WeatherNight |

## 主题系统设计

### 基础主题框架

项目采用 **Semi.Avalonia** 作为基础主题，替代原有的 FluentTheme + Material.Avalonia 方案。

**App.axaml 配置：**
```xml
<Application xmlns:semi="https://irihi.tech/semi"
             RequestedThemeVariant="Light">
    <Application.Styles>
        <semi:SemiTheme Locale="zh-CN" />
        <materialIcons:MaterialIconStyles />
        <StyleInclude Source="Assets/Styles/AppTheme.axaml" />
    </Application.Styles>
</Application>
```

### 色彩 Token 体系

通过 Avalonia 原生 `ThemeDictionaries` 机制定义 Light/Dark 双主题色彩 Token：

| Token 名称 | 用途 | Light 值 | Dark 值 |
|---|---|---|---|
| Primary | 主色调（强调色） | #0078D4 | #60CDFF |
| PrimaryHover | 主色调悬停 | #106EBE | #4DB8E8 |
| PrimaryPressed | 主色调按下 | #005A9E | #3AA5D6 |
| PrimaryForeground | 主色上文字 | #FFFFFF | #003A5E |
| Background | 主背景 | #FFFFFF | #1E1E2E |
| Surface | 次级背景（侧栏/卡片） | #F8F9FA | #2A2A3C |
| SurfaceVariant | 悬浮面板/弹出层 | #FFFFFF | #333348 |
| Border | 边框/分隔线 | #E9ECEF | #3E3E55 |
| BorderStrong | 强调边框 | #DEE2E6 | #4A4A62 |
| TextPrimary | 主要文字 | #212529 | #E4E4E8 |
| TextSecondary | 次要文字 | #495057 | #B0B0BC |
| TextMuted | 辅助/提示文字 | #6C757D | #808090 |
| HoverBackground | 通用悬停背景 | #F0F0F0 | #363650 |
| HoverSurface | 卡片/列表项悬停 | #F8F9FA | #32324A |
| PressedBackground | 通用按下背景 | #E0E0E0 | #40405A |
| SelectedBackground | 选中态背景 | #E3F2FD | #1A3A5C |
| SelectedForeground | 选中态文字 | #1976D2 | #60CDFF |
| Danger | 危险/删除 | #DC3545 | #F47067 |
| DangerHover | 危险悬停 | #C82333 | #E05550 |
| DangerPressed | 危险按下 | #BD2130 | #CC4440 |
| Success | 成功/启动 | #28A745 | #57E389 |
| SuccessHover | 成功悬停 | #218838 | #4BD47A |
| SuccessPressed | 成功按下 | #1E7E34 | #3FBE68 |
| TagBackground | 标签背景 | #E3F2FD | #1A3A5C |
| TagForeground | 标签文字 | #1976D2 | #60CDFF |

### 组件样式 Light/Dark 对照

#### Window 主窗口

| 属性 | Light | Dark |
|---|---|---|
| Background | #FFFFFF | #1E1E2E |
| FontFamily | Segoe UI | Segoe UI |

#### 左侧导航栏

| 属性 | Light | Dark |
|---|---|---|
| Background | #F8F9FA | #2A2A3C |
| BorderBrush | #E9ECEF | #3E3E55 |
| SectionTitle Foreground | #6C757D | #808090 |
| Separator Background | #E9ECEF | #3E3E55 |

#### Button 基础

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground |
|---|---|---|---|---|
| Normal | #F0F0F0 | #363650 | #333333 | #E4E4E8 |
| Hover | #E0E0E0 | #40405A | — | — |
| Pressed | #D0D0D0 | #4A4A62 | — | — |

#### NavigationButton 导航按钮

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground |
|---|---|---|---|---|
| Normal | Transparent | Transparent | #495057 | #B0B0BC |
| Hover | #E9ECEF | #363650 | #212529 | #E4E4E8 |
| Pressed | #DEE2E6 | #40405A | — | — |
| Active/Selected | #0078D4 | #60CDFF | #FFFFFF | #003A5E |

#### ToolButton 工具栏按钮

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground | Light Border | Dark Border |
|---|---|---|---|---|---|---|
| Normal | Transparent | Transparent | #495057 | #B0B0BC | #E9ECEF | #3E3E55 |
| Hover | #F8F9FA | #363650 | — | #E4E4E8 | #DEE2E6 | #4A4A62 |

#### PrimaryButton 主要按钮

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground |
|---|---|---|---|---|
| Normal | #0078D4 | #60CDFF | #FFFFFF | #003A5E |
| Hover | #106EBE | #4DB8E8 | — | — |
| Pressed | #005A9E | #3AA5D6 | — | — |

#### DangerButton 危险按钮

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground |
|---|---|---|---|---|
| Normal | #DC3545 | #F47067 | #FFFFFF | #1E1E2E |
| Hover | #C82333 | #E05550 | — | — |
| Pressed | #BD2130 | #CC4440 | — | — |

#### PlayButton 启动按钮

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground |
|---|---|---|---|---|
| Normal | #28A745 | #57E389 | #FFFFFF | #1E1E2E |
| Hover | #218838 | #4BD47A | — | — |
| Pressed | #1E7E34 | #3FBE68 | — | — |

#### IconButton / SmallIconButton

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground |
|---|---|---|---|---|
| Normal | Transparent | Transparent | #6C757D | #808090 |
| Hover | #F0F0F0 | #363650 | #495057 | #B0B0BC |
| Pressed | #E0E0E0 | #40405A | — | — |

#### IdeButton IDE选择按钮

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground |
|---|---|---|---|---|
| Normal | #F8F9FA | #2A2A3C | #495057 | #B0B0BC |
| Hover | #E9ECEF | #363650 | — | #E4E4E8 |

#### IdeListItem IDE列表项

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground | Light Border | Dark Border |
|---|---|---|---|---|---|---|
| Normal | Transparent | Transparent | #495057 | #B0B0BC | #E9ECEF | #3E3E55 |
| Hover | #F8F9FA | #32324A | — | #E4E4E8 | #DEE2E6 | #4A4A62 |
| Pressed | #E9ECEF | #363650 | — | — | — | — |

#### ViewButton 视图切换

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground | Light Border | Dark Border |
|---|---|---|---|---|---|---|
| Normal | Transparent | Transparent | #6C757D | #808090 | #E9ECEF | #3E3E55 |
| Hover | #F8F9FA | #363650 | #495057 | #B0B0BC | #DEE2E6 | #4A4A62 |
| Pressed | #E9ECEF | #32324A | — | — | — | — |
| Active | #E3F2FD | #1A3A5C | #1976D2 | #60CDFF | #1976D2 | #60CDFF |

#### SmallButton 小按钮 (+号等)

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground | Light Border | Dark Border |
|---|---|---|---|---|---|---|
| Normal | #E9ECEF | #3E3E55 | #495057 | #B0B0BC | #DEE2E6 | #4A4A62 |
| Hover | #DEE2E6 | #4A4A62 | #212529 | #E4E4E8 | — | — |
| Pressed | #CED4DA | #55556A | — | — | — | — |

#### TextBox 文本框

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground | Light Border | Dark Border |
|---|---|---|---|---|---|---|
| Normal | #FFFFFF | #2A2A3C | #333333 | #E4E4E8 | #E9ECEF | #3E3E55 |
| Focus | — | — | — | — | #0078D4 | #60CDFF |

#### Menu / MenuItem

| 状态 | Light Background | Dark Background | Light Foreground | Dark Foreground |
|---|---|---|---|---|
| Menu Background | #FFFFFF | #333348 | — | — |
| Menu Border | #E9ECEF | #3E3E55 | — | — |
| MenuItem Normal | Transparent | Transparent | — | #E4E4E8 |
| MenuItem Hover | #F8F9FA | #363650 | — | — |

#### Tag 标签

| 属性 | Light | Dark |
|---|---|---|
| Normal Background | #E3F2FD | #1A3A5C |
| Normal Foreground | #1976D2 | #60CDFF |
| Normal BorderBrush | #1976D2 | #60CDFF |
| Selected Background | #BBDEFB | #264F78 |
| Selected Foreground | #0D47A1 | #60CDFF |

#### Card 卡片

| 属性 | Light | Dark |
|---|---|---|
| Background | #FFFFFF | #2A2A3C |
| BorderBrush | #E9ECEF | #3E3E55 |
| Hover Background | #F8F9FA | #32324A |
| Hover BorderBrush | #DEE2E6 | #4A4A62 |

### 整体布局效果

**Light 模式：**
```
┌─────────────────────────────────────────────────────────┐
│  Project Hub                              [-] [□] [×]   │
├──────────────┬──────────────────────────────────────────┤
│              │                                          │
│  📁 全部(24) │   🔍 搜索项目...  [快速启动][IDE设置][+] │
│  ⭐ 收藏(5)  │  ──────────────────────────────────────  │
│  🕐 最近(10) │                                          │
│              │   ┌─────────────────────────────────┐    │
│  🏷️ 标签     │   │ 🚀 电商平台重构    [用VS Code][⋮]│    │
│    #web      │   │    别名: 主站  标签: #vue        │    │
│    #java     │   └─────────────────────────────────┘    │
│              │                                          │
│  💼 工作区   │                                          │
│    工作区A   │                                          │
│              │                                          │
│  ─────────── │                                          │
│  ⚙️ 设置     │                                          │
│    IDE设置   │                                          │
│  ┌──────────┐│                                         │
│  │☀️浅色│🌙深色││                                         │
│  └──────────┘│                                          │
└──────────────┴──────────────────────────────────────────┘
背景: #FFFFFF / 侧栏: #F8F9FA / 文字: #212529 / 主题色: #0078D4
```

**Dark 模式：**
```
┌─────────────────────────────────────────────────────────┐
│  Project Hub                              [-] [□] [×]   │
├──────────────┬──────────────────────────────────────────┤
│              │                                          │
│  📁 全部(24) │   🔍 搜索项目...  [快速启动][IDE设置][+] │
│  ⭐ 收藏(5)  │  ──────────────────────────────────────  │
│  🕐 最近(10) │                                          │
│              │   ┌─────────────────────────────────┐    │
│  🏷️ 标签     │   │ 🚀 电商平台重构    [用VS Code][⋮]│    │
│    #web      │   │    别名: 主站  标签: #vue        │    │
│    #java     │   └─────────────────────────────────┘    │
│              │                                          │
│  💼 工作区   │                                          │
│    工作区A   │                                          │
│              │                                          │
│  ─────────── │                                          │
│  ⚙️ 设置     │                                          │
│    IDE设置   │                                          │
│  ┌──────────┐│                                         │
│  │☀️浅色│🌙深色││                                         │
│  └──────────┘│                                          │
└──────────────┴──────────────────────────────────────────┘
背景: #1E1E2E / 侧栏: #2A2A3C / 文字: #E4E4E8 / 主题色: #60CDFF
```

## 主题切换设计

### 切换位置

主题切换控件放置在 **左侧导航栏底部「设置」分组内**，位于「IDE设置」下方。

**选位理由：**

| 候选位置 | 优点 | 缺点 | 推荐度 |
|---|---|---|---|
| 左侧导航栏底部「设置」区域 | 与IDE设置功能分区一致；固定位置不随内容滚动；符合VS Code等桌面应用惯例 | 需要滚动到底部 | ⭐⭐⭐⭐⭐ |
| 顶部工具栏右侧 | 一目了然，操作方便 | 工具栏已比较拥挤 | ⭐⭐⭐ |
| 标题栏区域 | 不占用内容空间 | Avalonia自定义标题栏实现较复杂 | ⭐⭐ |

### 交互控件

使用 **Segmented Toggle（分段切换器）** 作为主题切换控件：

```
┌─────────────────────┐
│  ☀️ 浅色  │  🌙 深色  │    ← Segmented Toggle
└─────────────────────┘
```

**Material Icon 映射：**
- Light 模式 → `WeatherSunny`
- Dark 模式 → `WeatherNight`

### 主题切换按钮样式

| 属性 | Light(选中) | Light(未选中) | Dark(选中) | Dark(未选中) |
|---|---|---|---|---|
| Background | #0078D4 | #E9ECEF | #60CDFF | #3E3E55 |
| Foreground | #FFFFFF | #495057 | #003A5E | #808090 |
| BorderBrush | #0078D4 | #DEE2E6 | #60CDFF | #4A4A62 |
| CornerRadius | 左6右0 / 左0右6 | 左6右0 / 左0右6 | 左6右0 / 左0右6 | 左6右0 / 左0右6 |
| Padding | 12,6 | 12,6 | 12,6 | 12,6 |

### 交互细节

| 属性 | 说明 |
|---|---|
| 控件类型 | Segmented Toggle（两段式切换器） |
| 位置 | 左侧导航栏底部「设置」分组内，「IDE设置」下方 |
| 默认值 | 跟随系统主题（RequestedThemeVariant="Default"） |
| 持久化 | 用户选择保存到本地配置文件，下次启动时恢复 |
| 切换效果 | 即时生效，无需重启应用 |
| 动画 | 200ms 颜色过渡动画 |

### 切换实现

```csharp
// 主题切换核心逻辑
Application.Current.RequestedThemeVariant = ThemeVariant.Light;  // 浅色
Application.Current.RequestedThemeVariant = ThemeVariant.Dark;   // 深色
Application.Current.RequestedThemeVariant = ThemeVariant.Default; // 跟随系统
```

```xml
<!-- 导航栏中的主题切换区域 -->
<StackPanel Orientation="Horizontal" Spacing="0" Margin="8,4">
    <Button Classes="ThemeSegmentButton"
            IsEnabled="{Binding !IsDarkTheme}"
            Command="{Binding SwitchToLightThemeCommand}">
        <StackPanel Orientation="Horizontal" Spacing="4">
            <icons:MaterialIcon Kind="WeatherSunny" Width="14" Height="14" />
            <TextBlock Text="浅色" FontSize="12" />
        </StackPanel>
    </Button>
    <Button Classes="ThemeSegmentButton"
            IsEnabled="{Binding IsDarkTheme}"
            Command="{Binding SwitchToDarkThemeCommand}">
        <StackPanel Orientation="Horizontal" Spacing="4">
            <icons:MaterialIcon Kind="WeatherNight" Width="14" Height="14" />
            <TextBlock Text="深色" FontSize="12" />
        </StackPanel>
    </Button>
</StackPanel>
```

### ThemeService 设计

```csharp
public class ThemeService
{
    private const string SettingsKey = "AppTheme";

    public ThemeMode CurrentTheme { get; private set; } = ThemeMode.Default;

    public void ApplyTheme(Application app, ThemeMode mode)
    {
        CurrentTheme = mode;
        app.RequestedThemeVariant = mode switch
        {
            ThemeMode.Light   => ThemeVariant.Light,
            ThemeMode.Dark    => ThemeVariant.Dark,
            _                 => ThemeVariant.Default
        };
        SavePreference(mode);
    }

    public void LoadSavedTheme(Application app)
    {
        var saved = LoadPreference();
        ApplyTheme(app, saved);
    }

    private void SavePreference(ThemeMode mode) { /* 持久化到本地配置 */ }
    private ThemeMode LoadPreference() { /* 从本地配置读取 */ }
}

public enum ThemeMode { Light, Dark, Default }
```