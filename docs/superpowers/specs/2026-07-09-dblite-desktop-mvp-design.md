# DB Lite Desktop MVP 设计文档

## 1. 目标

本次只实现一个可运行的 Windows 桌面版 MVP，用于安全地查看和查询数据库。

本轮范围固定为：

- `NET 8 WinForms`
- 仅支持 `SQLite` 和 `MySQL`
- 支持连接的新增、编辑、删除、保存、测试、连接
- 支持查看表、查看字段、执行只读 SQL、展示结果、保存查询历史
- 使用 `SQLite` 保存本地配置
- 使用 `Windows DPAPI` 加密保存密码

本轮明确不做：

- PostgreSQL、SQL Server
- 数据编辑、建表改表、导入导出
- 本地 Web 服务、HTTP 接口
- 多标签查询、SQL 智能补全、主题切换

## 2. 方案选择

本次采用单项目 MVP 方案。

原因：

- 当前仓库是空白起步，先做最小可运行版本价值最高
- WinForms 本身更适合直接在一个项目内交付首版
- 通过保留 `IDatabaseProvider` 和 `DatabaseProviderFactory`，后续仍可扩展数据库类型
- 避免为了未来功能提前拆分多个工程

## 3. 项目结构

项目采用一个 WinForms 项目，目录控制在最小必要范围内。

```text
DbLiteDesktop
├── DbLiteDesktop.sln
├── DbLiteDesktop
│   ├── Program.cs
│   ├── MainForm.cs
│   ├── MainForm.Designer.cs
│   ├── Forms
│   │   ├── ConnectionForm.cs
│   │   └── ConnectionForm.Designer.cs
│   ├── Models
│   │   ├── DbConnectionConfig.cs
│   │   ├── TableColumnInfo.cs
│   │   └── QueryHistoryItem.cs
│   ├── Providers
│   │   ├── IDatabaseProvider.cs
│   │   ├── DatabaseProviderFactory.cs
│   │   ├── MySqlProvider.cs
│   │   └── SQLiteProvider.cs
│   ├── Services
│   │   ├── ConfigService.cs
│   │   ├── PasswordEncryptService.cs
│   │   ├── QueryHistoryService.cs
│   │   └── SqlGuardService.cs
│   └── data
│       └── app_config.db
```

说明：

- 不额外拆分类库项目
- 不引入 Repository、Application Service 等中间层
- 查询结果直接使用 `DataTable`
- 本地配置库也使用 `Microsoft.Data.Sqlite`

## 4. UI 设计

### 4.1 主界面

主界面采用左右布局和顶部工具栏。

- 顶部工具栏：`连接下拉框`、`新建`、`编辑`、`删除`、`测试`、`连接`、`刷新`、`断开`
- 左侧：`TreeView` 展示当前连接下的表名
- 右侧：`TabControl`，包含 `字段信息`、`SQL 查询`、`查询历史`

### 4.2 字段信息页

使用 `DataGridView` 展示字段列表。

列固定为：

- 字段名
- 类型
- 是否为空
- 主键
- 默认值
- 额外信息
- 备注

### 4.3 SQL 查询页

界面保持最小可用：

- 上方一个多行 `TextBox` 作为 SQL 输入框
- 一排按钮：`执行`、`清空`、`复制 SQL`
- 中部 `DataGridView` 展示查询结果
- 底部状态文本显示执行结果、耗时、返回行数

### 4.4 查询历史页

使用 `DataGridView` 展示历史记录。

列包括：

- 时间
- 数据库
- SQL
- 是否成功
- 耗时
- 错误信息

点击某条历史记录后，将 SQL 回填到查询输入框。

### 4.5 连接窗口

新建和编辑共用 `ConnectionForm`。

`MySQL` 模式显示：

- 连接名称
- 数据库类型
- 主机地址
- 端口
- 数据库名称
- 用户名
- 密码

`SQLite` 模式显示：

- 连接名称
- 数据库类型
- 数据库文件路径

按钮统一为：

- `测试连接`
- `保存连接`
- `连接`
- `取消`

## 5. 数据流

### 5.1 程序启动

流程：

1. 启动 WinForms 程序
2. 初始化 `data/app_config.db`
3. 自动建表
4. 读取已保存连接
5. 填充顶部连接下拉框

### 5.2 连接数据库

流程：

1. 用户从下拉框选择连接或新建连接
2. 从本地配置库读取连接信息
3. 如果是 `MySQL`，解密密码
4. 通过 `DatabaseProviderFactory` 创建 Provider
5. 测试连接成功后加载表名

### 5.3 浏览表和字段

流程：

1. 连接成功后只加载表名
2. 点击表名时再读取字段信息
3. 将字段列表绑定到字段页表格
4. 同时生成一条表预览 SQL 放入 SQL 输入框

### 5.4 执行 SQL

流程：

1. 读取 SQL 输入框内容
2. 通过 `SqlGuardService` 做只读校验
3. 校验通过后调用对应 Provider 执行查询
4. 将结果绑定到结果表格
5. 显示耗时和返回行数
6. 写入查询历史

### 5.5 查询历史复用

流程：

1. 进入历史页时读取历史记录
2. 点击某条历史记录
3. 将 SQL 回填到查询框
4. 用户可直接再次执行

## 6. 模型设计

### 6.1 DbConnectionConfig

负责保存连接配置。

字段：

- `Id`
- `Name`
- `DbType`
- `Host`
- `Port`
- `DatabaseName`
- `Username`
- `PasswordEncrypted`
- `FilePath`
- `CreatedAt`
- `UpdatedAt`

### 6.2 TableColumnInfo

负责展示表字段信息。

字段：

- `Name`
- `Type`
- `Nullable`
- `Key`
- `DefaultValue`
- `Extra`
- `Comment`

### 6.3 QueryHistoryItem

负责展示查询历史。

字段：

- `Id`
- `ConnectionId`
- `DbType`
- `DatabaseName`
- `SqlText`
- `Success`
- `ErrorMessage`
- `DurationMs`
- `RowCount`
- `CreatedAt`

## 7. Provider 设计

### 7.1 接口

所有数据库实现统一接口：

```csharp
public interface IDatabaseProvider
{
    bool TestConnection(DbConnectionConfig config, string password);

    List<string> GetTables(DbConnectionConfig config, string password);

    List<TableColumnInfo> GetColumns(
        DbConnectionConfig config,
        string password,
        string tableName
    );

    DataTable ExecuteQuery(
        DbConnectionConfig config,
        string password,
        string sql,
        int maxRows = 1000
    );

    string BuildPreviewSql(string tableName, int limit = 100);
}
```

### 7.2 工厂

`DatabaseProviderFactory` 只做数据库类型分发。

MVP 只支持：

- `mysql`
- `sqlite`

不在工厂中加入缓存、日志、重试等附加逻辑。

### 7.3 MySqlProvider

职责：

- 测试连接
- 获取表
- 获取字段
- 执行只读查询
- 生成预览 SQL

连接设置：

- 连接超时 `10` 秒
- 命令超时 `30` 秒
- `Pooling=false`

### 7.4 SQLiteProvider

职责与 `MySqlProvider` 相同，但基于文件路径连接。

说明：

- 不需要主机、端口、用户名、密码
- 预览 SQL 使用 `LIMIT`

## 8. 服务设计

### 8.1 ConfigService

职责：

- 初始化本地配置库
- 创建 `db_connections`
- 提供连接配置的新增、编辑、删除、查询

数据库文件路径固定为：

```text
程序目录/data/app_config.db
```

### 8.2 PasswordEncryptService

职责：

- 使用 `Windows DPAPI` 加密密码
- 使用 `Windows DPAPI` 解密密码

规则：

- 仅对需要密码的连接类型使用
- `SQLite` 不保存密码

### 8.3 QueryHistoryService

职责：

- 创建 `query_history`
- 写入每次 SQL 执行记录
- 查询最近历史记录

### 8.4 SqlGuardService

职责：

- 拦截空 SQL
- 校验首关键字是否属于只读集合
- 拦截危险关键字
- 拦截多语句

允许前缀：

- `select`
- `show`
- `desc`
- `describe`
- `explain`
- `pragma`
- `with`

禁止关键字：

- `insert`
- `update`
- `delete`
- `drop`
- `alter`
- `truncate`
- `create`
- `replace`
- `grant`
- `revoke`
- `merge`
- `exec`
- `execute`
- `call`

## 9. 本地配置库设计

### 9.1 db_connections

```sql
CREATE TABLE IF NOT EXISTS db_connections (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    db_type TEXT NOT NULL,
    host TEXT,
    port INTEGER,
    database_name TEXT,
    username TEXT,
    password_encrypted TEXT,
    file_path TEXT,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);
```

### 9.2 query_history

```sql
CREATE TABLE IF NOT EXISTS query_history (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    connection_id INTEGER,
    db_type TEXT,
    database_name TEXT,
    sql_text TEXT NOT NULL,
    success INTEGER NOT NULL,
    error_message TEXT,
    duration_ms INTEGER,
    row_count INTEGER,
    created_at TEXT NOT NULL
);
```

## 10. 安全边界

本工具的安全目标是防误操作，不是替代数据库权限系统。

MVP 强制执行以下边界：

- 只允许只读 SQL
- 禁止多语句执行
- 禁止危险关键字
- 普通查询最多返回 `1000` 行
- 表预览默认 `100` 行
- 查询超时 `30` 秒
- 连接超时 `10` 秒
- 不启动本地 Web 服务
- 不暴露本地端口
- 密码不明文保存

额外说明：

- 实际生产使用时，数据库账号仍应为只读账号
- 程序层拦截只是第二道保险

## 11. 非功能取舍

为了保持轻量，本轮明确做以下取舍：

- 不维护长连接，所有操作按需打开和关闭连接
- 不做异步任务编排，先用同步调用跑通主流程
- 不引入 Monaco Editor 等额外编辑器依赖
- 不引入 UI 自动化测试框架
- 不预加载字段和数据，只按需加载

如果后续确认界面卡顿，再补最小异步方案。

## 12. 验收标准

MVP 完成后至少满足：

1. 程序可以启动，并自动创建本地配置库
2. 可以新增、编辑、删除、保存 `SQLite` 和 `MySQL` 连接
3. 可以测试连接
4. 可以连接成功后加载表列表
5. 点击表后可以查看字段信息
6. 点击表后会自动生成预览 SQL
7. 可以执行只读 SQL，并展示结果、耗时、行数
8. 危险 SQL 和多语句会被拦截
9. 查询历史会被记录并可回填

## 13. 验证策略

首轮采用最小验证方案：

- `SqlGuardService` 编写单元测试
- `PasswordEncryptService` 编写单元测试
- WinForms 主流程通过手工联调验证

手工验证覆盖：

- SQLite 连接完整流程
- MySQL 连接完整流程
- 只读 SQL 成功执行
- 危险 SQL 拦截
- 查询历史回填

## 14. 后续扩展

本设计为后续留出的扩展点只有两处：

- 在 `DatabaseProviderFactory` 中补充更多 Provider
- 在连接窗口中增加对应数据库类型字段

本轮不为 PostgreSQL、SQL Server 预埋额外 UI 和服务逻辑。
