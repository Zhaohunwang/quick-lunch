# 附录

## 常见IDE可执行文件路径

| IDE | Windows | macOS | Linux |
|-----|---------|-------|-------|
| VS Code | `%LOCALAPPDATA%\Programs\Microsoft VS Code\Code.exe` | `/Applications/Visual Studio Code.app` | `/usr/bin/code` |
| Trae | `%LOCALAPPDATA%\Programs\Trae\Trae.exe` | - | - |
| IntelliJ IDEA | `C:\Program Files\JetBrains\IntelliJ IDEA\bin\idea64.exe` | `/Applications/IntelliJ IDEA.app` | `/usr/bin/idea` |
| WebStorm | `C:\Program Files\JetBrains\WebStorm\bin\webstorm64.exe` | `/Applications/WebStorm.app` | `/usr/bin/webstorm` |
| Cursor | `%LOCALAPPDATA%\Programs\cursor\Cursor.exe` | `/Applications/Cursor.app` | - |

## 项目文件类型与IDE映射

| 文件类型 | 推荐IDE |
|----------|---------|
| `.csproj`, `.sln` | Visual Studio, Rider |
| `package.json` | VS Code, Trae, WebStorm |
| `pom.xml`, `build.gradle` | IntelliJ IDEA |
| `Cargo.toml` | VS Code, RustRover |
| `go.mod` | VS Code, GoLand |
| `*.py` | VS Code, PyCharm |

## 更新日志

### v1.0.0 (2026-03-20)
- 初始版本
- 完成需求分析和技术选型
- 完成交互设计
- 完成MCP支持规划