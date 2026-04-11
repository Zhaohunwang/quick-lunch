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