# MCP支持规划

## MCP协议概述

MCP (Model Context Protocol) 是一种让AI模型与外部工具交互的协议。通过实现MCP Server，可以让AI助手（如Claude、Trae等）直接操作Project Hub。

## MCP工具定义

### 1. list_projects

列出所有项目或按条件筛选

```json
{
  "name": "list_projects",
  "description": "获取项目列表，支持按标签、名称筛选",
  "inputSchema": {
    "type": "object",
    "properties": {
      "tag": {
        "type": "string",
        "description": "按标签筛选"
      },
      "name": {
        "type": "string",
        "description": "按名称模糊搜索"
      },
      "limit": {
        "type": "integer",
        "description": "返回数量限制，默认20",
        "default": 20
      }
    }
  }
}
```

**返回示例**：
```json
{
  "projects": [
    {
      "id": "uuid-1",
      "name": "电商平台重构",
      "alias": "主站",
      "path": "D:\\projects\\ecommerce",
      "tags": ["vue", "frontend"],
      "defaultIde": "VS Code"
    }
  ],
  "total": 24
}
```

### 2. open_project

打开指定项目

```json
{
  "name": "open_project",
  "description": "使用指定IDE打开项目",
  "inputSchema": {
    "type": "object",
    "properties": {
      "projectId": {
        "type": "string",
        "description": "项目ID"
      },
      "projectName": {
        "type": "string",
        "description": "项目名称或别名（与projectId二选一）"
      },
      "ideName": {
        "type": "string",
        "description": "IDE名称，不指定则使用默认IDE"
      }
    },
    "required": []
  }
}
```

**返回示例**：
```json
{
  "success": true,
  "message": "已使用 VS Code 打开项目「电商平台重构」",
  "project": {
    "id": "uuid-1",
    "name": "电商平台重构",
    "path": "D:\\projects\\ecommerce"
  },
  "ide": "VS Code"
}
```

### 3. search_projects

搜索项目

```json
{
  "name": "search_projects",
  "description": "搜索项目，支持名称、别名、标签、路径搜索",
  "inputSchema": {
    "type": "object",
    "properties": {
      "query": {
        "type": "string",
        "description": "搜索关键词"
      },
      "searchIn": {
        "type": "array",
        "items": {
          "type": "string",
          "enum": ["name", "alias", "tags", "path"]
        },
        "description": "搜索范围，默认全部",
        "default": ["name", "alias", "tags"]
      }
    },
    "required": ["query"]
  }
}
```

### 4. get_recent_projects

获取最近访问的项目

```json
{
  "name": "get_recent_projects",
  "description": "获取最近访问的项目列表",
  "inputSchema": {
    "type": "object",
    "properties": {
      "limit": {
        "type": "integer",
        "description": "返回数量，默认10",
        "default": 10
      }
    }
  }
}
```

### 5. add_project

添加新项目

```json
{
  "name": "add_project",
  "description": "添加新项目到列表",
  "inputSchema": {
    "type": "object",
    "properties": {
      "path": {
        "type": "string",
        "description": "项目路径"
      },
      "name": {
        "type": "string",
        "description": "项目名称，不指定则自动检测"
      },
      "alias": {
        "type": "string",
        "description": "别名"
      },
      "tags": {
        "type": "array",
        "items": { "type": "string" },
        "description": "标签列表"
      }
    },
    "required": ["path"]
  }
}
```

## MCP Server实现架构

```
┌─────────────────────────────────────────────────────────┐
│                    MCP Server                            │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │              Transport Layer                     │    │
│  │  - Stdio Transport (标准输入输出)                 │    │
│  │  - HTTP Transport (可选，用于远程调用)            │    │
│  └─────────────────────────────────────────────────┘    │
│                         │                               │
│                         ▼                               │
│  ┌─────────────────────────────────────────────────┐    │
│  │              Protocol Handler                    │    │
│  │  - 请求解析                                      │    │
│  │  - 响应封装                                      │    │
│  │  - 错误处理                                      │    │
│  └─────────────────────────────────────────────────┘    │
│                         │                               │
│                         ▼                               │
│  ┌─────────────────────────────────────────────────┐    │
│  │              Tool Registry                       │    │
│  │  - list_projects                                 │    │
│  │  - open_project                                  │    │
│  │  - search_projects                               │    │
│  │  - get_recent_projects                           │    │
│  │  - add_project                                   │    │
│  └─────────────────────────────────────────────────┘    │
│                         │                               │
│                         ▼                               │
│  ┌─────────────────────────────────────────────────┐    │
│  │              Service Layer                       │    │
│  │  - ProjectService                                │    │
│  │  - IdeLauncherService                            │    │
│  │  - SearchService                                 │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

## MCP配置示例

在AI客户端（如Claude Desktop、Trae）中配置：

```json
{
  "mcpServers": {
    "project-hub": {
      "command": "ProjectHub.McpServer.exe",
      "args": ["--stdio"],
      "env": {}
    }
  }
}
```