# LightDB-Client

Windows 下轻量级、安全的**只读**多数据库桌面查询工具。基于 .NET 8 WinForms，支持 SQLite / MySQL / PostgreSQL，内置 SQL 只读守卫与密码本地加密，适合在不改动数据的前提下快速浏览、查询和分析数据库内容。

## 功能特性

- **多数据库支持** — SQLite（文件型）、MySQL（远程型）、PostgreSQL（远程型），通过 Provider 工厂统一抽象，易于扩展新类型
- **只读 SQL 守卫** — 双重过滤：白名单前缀（`SELECT` / `SHOW` / `DESC` / `EXPLAIN` / `PRAGMA` / `WITH`）+ 黑名单关键词（`INSERT` / `UPDATE` / `DELETE` / `DROP` 等写操作拦截），从源头杜绝误改数据
- **密码本地加密** — 使用 Windows DPAPI（`CurrentUser` 作用域）加密存储连接密码，无需第三方密钥管理
- **连接配置管理** — 多连接持久化保存，基于本地 SQLite（`app_config.db`）存储，支持自定义连接/命令超时
- **表结构与字段浏览** — 树形/列表展示表清单，点击查看列定义
- **数据预览** — 分页预览（每页 100 行），支持按字段精确/模糊筛选，单元格右键复制
- **查询历史** — 自动记录 SQL 执行历史，便于复用
- **数据导出** — 查询结果/预览数据一键导出 CSV / JSON，UTF-8 BOM 编码（Excel 兼容）
- **异步加载** — 所有数据库操作在后台线程执行，UI 不阻塞，加载状态实时反馈
- **快捷键** — `Ctrl+Enter` 执行 SQL，`F5` 刷新表列表，`Esc` 清空搜索条件
- **行数统计** — 点击表名后一键查看全表行数
- **结果排序** — 查询结果/预览数据支持点击列头排序，纯客户端操作
- **SQL 语法高亮** — 关键字、字符串、数字、注释着色显示

## 技术栈

| 层级 | 技术 |
|------|------|
| 运行时 | .NET 8 (`net8.0-windows`) |
| UI | Windows Forms |
| SQLite 驱动 | Microsoft.Data.Sqlite 8.0.8 |
| MySQL 驱动 | MySqlConnector 2.4.0 |
| PostgreSQL 驱动 | Npgsql 8.0.5 |
| 加密 | Windows DPAPI (`ProtectedData`) |

## 项目结构

```
LightDB-Client/
├── DbLiteDesktop.sln
├── DbLiteDesktop/                        # 主程序
│   ├── Program.cs                        # 入口
│   ├── MainForm.cs                       # 主窗体（表浏览/查询/预览）
│   ├── Forms/
│   │   └── ConnectionForm.cs             # 连接配置表单
│   ├── Models/
│   │   ├── DbConnectionConfig.cs         # 连接配置模型
│   │   ├── QueryHistoryItem.cs           # 查询历史模型
│   │   └── TableColumnInfo.cs            # 表列信息模型
│   ├── Controls/
│   │   └── SqlEditorTextBox.cs           # SQL 语法高亮编辑器
│   ├── Providers/                        # 数据库抽象层
│   │   ├── IDatabaseProvider.cs          # 统一接口
│   │   ├── DatabaseProviderFactory.cs    # 工厂（按类型分发）
│   │   ├── MySqlProvider.cs              # MySQL 实现
│   │   ├── SQLiteProvider.cs             # SQLite 实现
│   │   └── PostgresProvider.cs           # PostgreSQL 实现
│   ├── Services/
│   │   ├── ConfigService.cs              # 配置读写（本地 SQLite）
│   │   ├── PasswordEncryptService.cs     # DPAPI 加解密
│   │   ├── SqlGuardService.cs            # 只读 SQL 守卫
│   │   ├── QueryHistoryService.cs        # 查询历史
│   │   └── DataExportService.cs          # CSV/JSON 导出
│   └── Utils/
│       ├── IdentifierQuoteHelper.cs      # 标识符转义
│       └── PreviewSearchInputParser.cs   # 预览搜索解析
├── DbLiteDesktop.Tests/                  # 单元测试
│   └── ...
└── README.md
```

## 快速开始

### 环境要求

- Windows 10/11
- .NET 8 SDK（[下载](https://dotnet.microsoft.com/download/dotnet/8.0)）

### 构建

```bash
git clone https://github.com/2788949485/LightDB-Client.git
cd LightDB-Client
dotnet build -c Release
```

构建产物在 `DbLiteDesktop/bin/Release/net8.0-windows/DbLiteDesktop.exe`。

### 运行

```bash
dotnet run --project DbLiteDesktop
```

### 运行测试

```bash
dotnet test
```

## 架构说明

采用经典的分层 + Provider 模式：

1. **Provider 层** — `IDatabaseProvider` 统一抽象表查询、列查询、SQL 执行、行数统计、分页预览等操作；`DatabaseProviderFactory` 按 `DbType` 分发到具体实现，新增数据库类型只需实现接口并注册到工厂
2. **Service 层** — 配置管理、密码加密、SQL 守卫、查询历史、数据导出各司其职，互不耦合
3. **UI 层** — `MainForm` 编排所有交互，通过 Provider 和 Service 完成业务

数据安全是核心设计目标：
- 所有用户输入的 SQL 在执行前必须通过 `SqlGuardService.IsReadonlySql` 校验
- 连接密码仅在内存中明文存在，落盘一律经 DPAPI 加密

## 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+Enter` | 执行 SQL |
| `F5` | 刷新表列表 |
| `Esc` | 清空预览搜索条件 |
