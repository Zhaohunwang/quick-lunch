# 技术选型

## 技术栈确认

```
前端框架: Avalonia UI 11.x
运行时:   .NET 8 (LTS)
语言:     C# 12
ORM:      Entity Framework Core 8.x
数据库:   SQLite
架构模式: MVVM (CommunityToolkit.Mvvm)
```

## 选型理由

| 维度 | Avalonia UI 优势 |
|------|------------------|
| 跨平台 | 原生支持 Windows / macOS / Linux |
| 开发效率 | XAML + C#，WPF开发者零成本上手 |
| 性能 | 原生编译，内存占用低（约30-50MB） |
| 生态 | 控件丰富，社区活跃，文档完善 |
| 打包 | 支持单文件发布，体积适中 |

## NuGet 依赖

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

<!-- 调试 (仅Debug模式) -->
<PackageReference Include="Avalonia.Diagnostics" Version="11.1.0" />
```

### 依赖调整说明

| 包名 | 操作 | 原因 |
|------|------|------|
| `Avalonia.Themes.Fluent` | 🗑️ 移除 | 被 Semi.Avalonia 替代作为基础主题 |
| `Avalonia.Fonts.Inter` | 🗑️ 移除 | FluentTheme 配套字体，Semi.Avalonia 自带字体 |
| `Material.Avalonia` | 🗑️ 移除 | 项目中未实际使用，且与 FluentTheme/SemiTheme 存在控件样式冲突 |
| `Semi.Avalonia` | ➕ 新增 | 新基础主题，内置 Light/Dark 主题，支持 ThemeDictionaries |
| `Material.Icons.Avalonia` | ✅ 保留 | 仅提供图标资源，与主题系统无关，不产生冲突 |

### Semi.Avalonia 选型理由

| 维度 | Semi.Avalonia 优势 |
|------|-------------------|
| 主题系统 | 内置 Light/Dark 主题，通过 `RequestedThemeVariant` 一键切换 |
| 设计风格 | 字节跳动 Semi Design，现代简洁，适合工具类应用 |
| 主题变量 | 完善的 Token 体系，支持 `ThemeDictionaries` 自定义扩展 |
| 中文支持 | 原生中文 Locale 支持（`Locale="zh-CN"`） |
| 社区活跃 | GitHub 1.9k+ Star，持续维护，Avalonia 11.x 兼容 |
| 版本兼容 | 11.1.x 版本与当前 Avalonia 11.1.0 完全匹配 |