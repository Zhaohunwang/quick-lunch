# MCP 配置指南

## 什么是 MCP

MCP（Model Context Protocol）是一种开放协议，用于标准化应用如何向 LLM 提供上下文。可以将 MCP 理解为 AI 应用的 USB-C 接口——它提供了一种标准化的方式，将 AI 模型连接到不同的数据源和工具。

## 架构

本项目的 MCP Server 采用 **Standard I/O (stdio)** 传输方式，宿主进程（如 Claude Desktop）通过 stdin/stdout 与 MCP Server 通信。

```
┌─────────────┐    stdio    ┌──────────────────┐    SQLite    ┌──────────────┐
│ Claude Desktop│ ◄────────► │ ProjectHub MCP   │ ◄──────────► │ ProjectHub.db│
│  (Host)      │            │ Server           │              │              │
└─────────────┘            └──────────────────┘              └──────────────┘
```

### 核心组件

- **McpServer.cs**: MCP 协议实现，处理 JSON-RPC 消息路由
- **StdioTransport.cs**: 标准输入/输出传输层
- **Tools.cs**: 定义所有可用的 MCP 工具（Tool）

## 安装

```bash
cd ProjectHub.McpServer
dotnet publish -c Release
```

发布产物位于 `bin/Release/net9.0/publish/` 目录。

## 配置

### Claude Desktop

编辑 Claude Desktop 配置文件：

| 操作系统 | 配置文件路径 |
|---------|------------|
| Windows | `%APPDATA%\Claude\claude_desktop_config.json` |
| macOS | `~/Library/Application Support/Claude/claude_desktop_config.json` |

添加以下配置：

```json
{
  "mcpServers": {
    "ProjectHub": {
      "command": "dotnet",
      "args": [
        "exec",
        "D:\\path\\to\\ProjectHub.McpServer.dll"
      ]
    }
  }
}
```

### Cursor / Trae / 其他 MCP 客户端

在 MCP 客户端的 MCP Server 配置中添加：

```json
{
  "name": "ProjectHub",
  "type": "stdio",
  "command": "dotnet",
  "args": ["exec", "D:\\path\\to\\ProjectHub.McpServer.dll"]
}
```

### 前置条件

- 安装 [.NET 9.0 Runtime](https://dotnet.microsoft.com/download)（或更高版本）
- 项目数据库文件位于：`%APPDATA%\ProjectHub\ProjectHub.db`（Windows）

## 提供的工具

MCP Server 提供以下工具，AI 助手可通过这些工具查询和管理项目数据：

### 项目管理

| 工具名称 | 说明 |
|---------|------|
| `list_projects` | 获取所有项目列表，支持按标签、状态过滤 |
| `get_project` | 根据 ID 或名称获取单个项目的完整信息 |
| `search_projects` | 按关键词搜索项目 |
| `create_project` | 创建新项目 |
| `update_project` | 更新项目信息 |
| `delete_project` | 删除项目 |

### 标签管理

| 工具名称 | 说明 |
|---------|------|
| `list_tags` | 获取所有标签及其关联的项目数量 |
| `create_tag` | 创建新标签 |
| `delete_tag` | 删除标签 |

### 快捷操作

| 工具名称 | 说明 |
|---------|------|
| `get_recent_projects` | 获取最近访问的项目（默认 5 个） |
| `get_project_stats` | 获取项目统计摘要（总数、按状态分组、按标签分组） |
| `create_quick_project` | 快速创建项目（仅需名称，自动设置默认值） |

### IDE 操作

| 工具名称 | 说明 |
|---------|------|
| `list_ides` | 获取可用的 IDE 列表 |
| `open_project_in_ide` | 使用指定 IDE 打开项目目录 |

### AI 集成

| 工具名称 | 说明 |
|---------|------|
| `generate_ai_context` | 为指定项目生成 AI 上下文（包含项目元数据、目录结构、README 等） |

## 使用示例

配置完成后，可以在 Claude Desktop 中这样使用：

```
用户：帮我创建一个叫 "MyWebApp" 的 Web 项目
AI：[调用 create_quick_project 工具]
    已创建项目 "MyWebApp"！

用户：列出所有 Web 开发相关的项目
AI：[调用 list_projects 工具，过滤标签 "Web 开发"]
    找到以下 Web 开发项目：...

用户：用 VS Code 打开 MyWebApp 项目
AI：[调用 open_project_in_ide 工具]
    已在 VS Code 中打开 MyWebApp 项目。
```

## 故障排查

### Server 无法启动

1. 确认 .NET Runtime 已安装：`dotnet --list-runtimes`
2. 手动测试 Server 是否能启动：`dotnet exec ProjectHub.McpServer.dll`（应等待 stdin 输入）
3. 检查数据库文件路径是否正确

### 工具调用失败

1. 确认数据库文件存在且可读写
2. 检查 Claude Desktop 的 MCP 日志输出
3. 验证 JSON-RPC 消息格式是否正确

### 数据库位置

Windows: `%APPDATA%\ProjectHub\ProjectHub.db`

如果数据库文件不存在，MCP Server 会自动创建。
