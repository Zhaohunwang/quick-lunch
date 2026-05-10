# Project Hub 设计文档

## 文档结构

本目录包含Project Hub项目的详细设计文档，按模块拆分为多个文件，便于AI工具读取和理解。

### 文档列表

| 文件名 | 内容描述 |
|--------|----------|
| `01_project_overview.md` | 项目概述与需求分析 |
| `02_tech_stack.md` | 技术选型、依赖与NuGet调整说明（Semi.Avalonia替代方案） |
| `03_features.md` | 功能模块与优先级 |
| `04_ui_design.md` | 交互设计、界面布局、Light/Dark主题系统、主题切换设计 |
| `05_data_model.md` | 数据模型与实体定义 |
| `06_mcp_support.md` | MCP支持规划与工具定义 |
| `07_project_structure.md` | 项目结构与目录组织 |
| `08_development_plan.md` | 开发路线与时间规划 |
| `09_appendix.md` | 附录与参考信息 |
| `10_add_edit_interaction.md` | 新增/编辑交互设计 |
| `11_project_operations.md` | 项目操作功能设计 |
| `12_workspace_add_edit.md` | 工作区新增/编辑交互设计 |
| `13_ide_settings.md` | IDE设置功能设计 |
| `14_macos_packaging.md` | macOS 打包与分发方案 |

## 项目简介

Project Hub是一个跨平台的统一项目管理工具，用于管理多个IDE项目，支持快速搜索、标签分类、工作区管理，并提供MCP接口供AI工具调用。

### 核心功能

- 统一项目管理入口
- 快速搜索与访问
- 多IDE支持与配置
- 标签系统与工作区管理
- AI集成（MCP协议）
- Light/Dark 主题切换（Semi.Avalonia）

### 技术栈

- 前端：Avalonia UI 11.x + Semi.Avalonia
- 运行时：.NET 8 (LTS)
- 语言：C# 12
- 数据库：SQLite
- 架构：MVVM

## 如何使用这些文档

1. **开发参考**：按照开发路线文档的阶段进行开发
2. **功能实现**：参考功能模块文档实现具体功能
3. **界面开发**：参考交互设计文档实现UI
4. **数据设计**：参考数据模型文档设计数据库
5. **MCP集成**：参考MCP支持文档实现AI集成
6. **新增交互**：参考新增/编辑交互设计文档实现对话框
7. **项目操作**：参考项目操作功能设计文档实现项目相关操作
8. **工作区管理**：参考工作区新增/编辑交互设计文档实现工作区功能
9. **IDE配置**：参考IDE设置功能设计文档实现IDE管理功能
10. **主题系统**：参考交互设计文档中的主题系统设计章节实现 Light/Dark 主题切换
11. **macOS 打包**：参考 macOS 打包与分发方案实现 macOS 平台应用打包、签名与公证

## 版本信息

- 版本：1.1.0
- 更新日期：2026-05-02