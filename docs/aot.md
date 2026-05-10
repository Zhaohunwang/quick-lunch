# Native AOT 发布方案

> 本文档说明 ProjectHub 项目各依赖库对 Native AOT 的兼容性分析，以及后续的可替换/调整方案。

---

## 1. 依赖库 AOT 兼容性总览

| 依赖库 | 当前版本 | AOT 兼容 | 备注 |
|---|---|---|---|
| Avalonia | 11.1.0 | ✅ 支持 | 需按官方文档配置 |
| Avalonia.Desktop | 11.1.0 | ✅ 支持 | 同上 |
| Avalonia.Diagnostics | 11.1.0 | ✅ 仅 Debug | 已在 Release 排除，无影响 |
| Semi.Avalonia | 11.1.0 | ✅ 支持 | 官方声明原生支持 NativeAOT，无需额外配置 |
| Material.Icons.Avalonia | 2.2.0 | ⚠️ 需测试 | 图标通过 SVG Path 编码为强类型枚举，理论上 AOT 友好，但未官方声明 |
| CommunityToolkit.Mvvm | 8.2.2 | ✅ 支持 | 使用源生成器，AOT 友好 |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.0 | ❌ 不支持 | EF Core AOT 支持仍处于实验性阶段 |
| System.Text.Json | 8.0.0 | ✅ 支持 | 需使用 `JsonSerializerContext` 源生成器 |
| Microsoft.Extensions.DependencyInjection | 10.0.7 | ⚠️ 部分支持 | 接口注册依赖反射解析 |

---

## 2. 各依赖详细分析

### 2.1 Avalonia 11.1.0 ✅

Avalonia 从 11.0 起官方支持 NativeAOT。配置要点：

```xml
<PropertyGroup>
    <PublishAot>true</PublishAot>
    <TrimMode>partial</TrimMode>
</PropertyGroup>

<ItemGroup>
    <TrimmerRootAssembly Include="Avalonia.Themes.Fluent" />
    <TrimmerRootAssembly Include="Avalonia.Themes.Default" />
</ItemGroup>
```

关键要求：
- XAML 中使用 `x:CompileBindings="True"` 开启编译时绑定
- 避免运行时动态加载 XAML
- 使用静态资源引用替代动态资源
- 所有资产使用 `AvaloniaResource` 构建操作
- ViewModels 在启动时注册，避免反射式服务定位

> 参考：[Avalonia Native AOT Deployment](https://docs.avaloniaui.net/docs/deployment/native-aot)

### 2.2 Semi.Avalonia 11.1.0 ✅

Semi.Avalonia 官方声明原生支持 NativeAOT，无需针对 Semi 进行任何额外配置即可开启 AOT 编译。

> 参考：[Semi.Avalonia Native AOT 文档](https://docs.irihi.tech/semi/docs/advanced/native-aot)

### 2.3 Material.Icons.Avalonia 2.2.0 ⚠️

该库将 Material Design 图标编码为强类型枚举 + SVG Path，不依赖字体文件加载。图标以静态类方式暴露（如 `SearchIcon.Instance`），理论上对 AOT/Trimming 友好。

但该库 **未官方声明** AOT 兼容性，需要实际测试验证。若出现问题，可替换为 `NStyles.MeterialIcons`，后者明确声明为 AOT 裁剪友好。

### 2.4 CommunityToolkit.Mvvm 8.2.2 ✅

从 8.2.0 起使用源生成器替代运行时反射，完全兼容 AOT。需要确认：
- 使用 `[ObservableProperty]`、`[RelayCommand]` 等源生成器属性
- 避免手动继承 `ObservableObject`（如必须继承，需确保通过源生成器辅助）

### 2.5 Microsoft.EntityFrameworkCore.Sqlite 8.0.0 ❌

**这是 AOT 发布的最大障碍。** EF Core 当前状态：

- EF Core 9.0 开始提供实验性 NativeAOT 支持，但官方明确声明 **不建议在生产环境使用**
- EF Core 10.0 RC 仍有已知 AOT 兼容性 Bug（如 `ValueComparer.CreateDefault` 在 AOT 下失败）
- EF Core 的模型发现、变更追踪、LINQ 查询翻译大量依赖运行时反射和动态代码生成
- `Microsoft.Data.Sqlite` 底层依赖 SQLitePCLRaw，AOT 下原生库加载存在问题

> 参考：[EF Core NativeAOT Support (Experimental)](https://learn.microsoft.com/ef/core/performance/nativeaot-and-precompiled-queries)
> 参考：[Microsoft.Data.Sqlite and NativeAOT compatibility #36068](https://github.com/dotnet/efcore/issues/36068)

### 2.6 System.Text.Json 8.0.0 ✅

通过 `JsonSerializerContext` 源生成器完全支持 AOT。当前项目中直接使用了 `JsonSerializer.Serialize/Deserialize`，需要改造为源生成器方式。

### 2.7 Microsoft.Extensions.DependencyInjection 10.0.7 ⚠️

DI 容器本身在 .NET 8+ 中对 AOT 有部分支持，但通过接口注册（如 `services.AddSingleton<IProjectService, ProjectService>()`）时依赖反射解析。AOT 下需要通过 `[DynamicDependency]` 或 `TrimmerRootAssembly` 保留相关类型。

---

## 3. 当前项目的 AOT 障碍

### 3.1 最大障碍：EF Core

EF Core 是唯一 **完全不支持 AOT** 的核心依赖。即使使用 EF Core 9+ 的实验性 AOT 支持，仍然存在：
- 编译模型生成的代码不完全兼容 AOT
- 运行时 `ValueComparer`、`MethodInfo.MakeGenericMethod()` 等调用在 AOT 下失败
- 官方明确标记为"实验性，不建议生产使用"

### 3.2 代码层面的障碍

| 问题 | 位置 | 说明 |
|---|---|---|
| 无源生成器的 JSON 序列化 | `EntityMapper.cs`、`SettingsService.cs` | AOT 下运行时反射序列化会失败 |
| XAML 未开启编译绑定 | `AvaloniaUseCompiledBindingsByDefault=false` | 不利于 AOT 和 Trimming |
| 反射式 DI 注册 | `App.axaml.cs` | 接口到实现的映射依赖反射 |

---

## 4. AOT 可替换/调整方案

### 方案 A：替换 EF Core 为 Dapper（推荐用于 AOT）

**适用场景**：需要完整 NativeAOT 支持

**改动范围**：
- 移除 `Microsoft.EntityFrameworkCore.Sqlite`
- 引入 `Dapper`（AOT 友好的轻量级 ORM）
- 底层保留 `Microsoft.Data.Sqlite` 作为数据库驱动
- 重写 `AppDbContext` 为 Dapper 仓储层

**Dapper AOT 兼容性**：Dapper 从 2.1+ 开始提供 `DapperAOT` 源生成器支持，可生成 AOT 兼容的数据库访问代码。

**优点**：
- 完全移除 EF Core 的反射依赖
- 数据库访问性能更好
- 包体积显著减小

**缺点**：
- 丧失 EF Core 的 LINQ 查询能力
- 需要手动编写更多 SQL
- 需要重写所有数据库访问层

### 方案 B：替换 EF Core 为 SQLite ADO.NET 直接访问

**适用场景**：追求最小依赖和最大 AOT 兼容

**改动范围**：
- 移除 `Microsoft.EntityFrameworkCore.Sqlite`
- 直接使用 `Microsoft.Data.Sqlite`（注意：底层 SQLitePCLRaw 的 AOT 兼容性也需验证）
- 手动编写所有 SQL 和实体映射

**优点**：
- 最小依赖链
- 完全控制 SQL

**缺点**：
- 开发效率最低
- 需要维护大量样板代码

### 方案 C：EF Core 保持 + 不支持 AOT（保守方案）

**适用场景**：优先保持功能完整性，放弃 AOT

- 保持现有 EF Core 不变
- 仅做 Trimming 优化（参见 trimming.md）
- 等待 EF Core 官方正式支持 AOT（预计 EF Core 12.0+）

### 方案 D：MCP Server 独立 AOT

**适用场景**：独立服务组件可先行 AOT 化

`ProjectHub.McpServer` 是纯控制台项目，无 EF Core 依赖，可立即进行 AOT 发布：

```xml
<PropertyGroup>
    <PublishAot>true</PublishAot>
</PropertyGroup>
```

---

## 5. 其他依赖的调整建议

### 5.1 System.Text.Json → 源生成器改造

无论选择哪种方案，`System.Text.Json` 都应改造为使用源生成器：

```csharp
[JsonSerializable(typeof(Project))]
[JsonSerializable(typeof(Tag))]
[JsonSerializable(typeof(Workspace))]
[JsonSerializable(typeof(AppSettings))]
internal partial class AppJsonContext : JsonSerializerContext
{
}
```

改造位置：
- `EntityMapper.cs` 中的 `JsonSerializer.Deserialize<List<string>>`
- `SettingsService.cs` 中的 `JsonSerializer.Serialize/Deserialize`

### 5.2 Material.Icons.Avalonia → NStyles.MeterialIcons

如果 `Material.Icons.Avalonia` 在 AOT 下出现问题，可替换为 `NStyles.MeterialIcons`：
- 官方声明为 AOT 友好（图标为独立静态类，方便裁剪）
- 使用方式类似：`<PathIcon Data="{x:Static icons:SearchIcon.Instance}" />`

### 5.3 DI 容器 → 编译时注册

在 AOT 场景下，可考虑：
- 使用 `Microsoft.Extensions.DependencyInjection` 的 `ActivatorUtilities` 替代接口解析
- 或引入 `[DynamicDependency]` 属性保留必要类型
- 或替换为更 AOT 友好的 DI 容器（如 `MessagePipe`、`Volo.Abp.DependencyInjection`）

---

## 6. 推荐路径

### 阶段 1：MCP Server AOT 化（零风险）
1. 为 `ProjectHub.McpServer.csproj` 添加 `<PublishAot>true</PublishAot>`
2. 测试发布并验证功能

### 阶段 2：系统性改造（为未来 AOT 做准备）
1. 将 `System.Text.Json` 改造为源生成器
2. 将 `AvaloniaUseCompiledBindingsByDefault` 改为 `true`，并修复所有 XAML 绑定警告
3. 验证 `Material.Icons.Avalonia` 在 AOT 下的表现

### 阶段 3：EF Core 替换（若需要 AOT）
1. 评估 Dapper + DapperAOT 方案
2. 设计新的数据访问层
3. 逐步迁移数据库操作

### 阶段 4：完整 AOT 发布
1. 应用所有 Avalonia AOT 配置
2. 测试各平台发布
3. 性能对比测试

---

## 7. 平台支持

| 平台 | AOT 支持 | 备注 |
|---|---|---|
| Windows x64 | ✅ | 主要目标平台 |
| Windows Arm64 | ✅ | |
| Linux x64 | ✅ | |
| Linux Arm64 | ✅ | |
| macOS x64 | ✅ | |
| macOS Arm64 | ✅ | |

---

## 8. 参考链接

- [Avalonia Native AOT Deployment](https://docs.avaloniaui.net/docs/deployment/native-aot)
- [Semi.Avalonia Native AOT](https://docs.irihi.tech/semi/docs/advanced/native-aot)
- [EF Core NativeAOT Support](https://learn.microsoft.com/ef/core/performance/nativeaot-and-precompiled-queries)
- [.NET Native AOT 官方文档](https://learn.microsoft.com/dotnet/core/deploying/native-aot)
- [CommunityToolkit.Mvvm AOT 支持](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [Material.Icons.Avalonia](https://github.com/SKProCH/Material.Icons)
- [NStyles.MeterialIcons (AOT 友好替代)](https://www.nuget.org/packages/NStyles.MeterialIcons/)
