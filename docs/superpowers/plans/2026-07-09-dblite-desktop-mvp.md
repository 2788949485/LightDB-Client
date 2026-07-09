# DB Lite Desktop MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a runnable `NET 8 WinForms` MVP that supports SQLite and MySQL connections, read-only SQL execution, local encrypted connection storage, and query history.

**Architecture:** Use one WinForms app project for UI, providers, models, and services, plus one small test project for service-level checks. Keep the runtime flow synchronous, open connections only when needed, and use `Microsoft.Data.Sqlite` for the local config store and `Windows DPAPI` for password encryption.

**Tech Stack:** `NET 8`, `WinForms`, `Microsoft.Data.Sqlite`, `MySqlConnector`, `xUnit`

---

## File Structure

### Create

- `DbLiteDesktop.sln`
- `DbLiteDesktop/DbLiteDesktop.csproj`
- `DbLiteDesktop/Program.cs`
- `DbLiteDesktop/MainForm.cs`
- `DbLiteDesktop/MainForm.Designer.cs`
- `DbLiteDesktop/Forms/ConnectionForm.cs`
- `DbLiteDesktop/Forms/ConnectionForm.Designer.cs`
- `DbLiteDesktop/Models/DbConnectionConfig.cs`
- `DbLiteDesktop/Models/TableColumnInfo.cs`
- `DbLiteDesktop/Models/QueryHistoryItem.cs`
- `DbLiteDesktop/Providers/IDatabaseProvider.cs`
- `DbLiteDesktop/Providers/DatabaseProviderFactory.cs`
- `DbLiteDesktop/Providers/MySqlProvider.cs`
- `DbLiteDesktop/Providers/SQLiteProvider.cs`
- `DbLiteDesktop/Services/ConfigService.cs`
- `DbLiteDesktop/Services/PasswordEncryptService.cs`
- `DbLiteDesktop/Services/QueryHistoryService.cs`
- `DbLiteDesktop/Services/SqlGuardService.cs`
- `DbLiteDesktop/Utils/IdentifierQuoteHelper.cs`
- `DbLiteDesktop.Tests/DbLiteDesktop.Tests.csproj`
- `DbLiteDesktop.Tests/Services/SqlGuardServiceTests.cs`
- `DbLiteDesktop.Tests/Services/PasswordEncryptServiceTests.cs`

### Modify

- `README.md`

## Task 1: Scaffold Solution

**Files:**
- Create: `DbLiteDesktop.sln`
- Create: `DbLiteDesktop/DbLiteDesktop.csproj`
- Create: `DbLiteDesktop/Program.cs`
- Create: `DbLiteDesktop.Tests/DbLiteDesktop.Tests.csproj`
- Modify: `README.md`

- [ ] **Step 1: Create the solution and projects**

Run:

```powershell
dotnet new sln -n DbLiteDesktop
dotnet new winforms -n DbLiteDesktop --framework net8.0-windows
dotnet new xunit -n DbLiteDesktop.Tests
dotnet sln DbLiteDesktop.sln add .\DbLiteDesktop\DbLiteDesktop.csproj
dotnet sln DbLiteDesktop.sln add .\DbLiteDesktop.Tests\DbLiteDesktop.Tests.csproj
dotnet add .\DbLiteDesktop.Tests\DbLiteDesktop.Tests.csproj reference .\DbLiteDesktop\DbLiteDesktop.csproj
```

- [ ] **Step 2: Add runtime and test packages**

Run:

```powershell
dotnet add .\DbLiteDesktop\DbLiteDesktop.csproj package Microsoft.Data.Sqlite
dotnet add .\DbLiteDesktop\DbLiteDesktop.csproj package MySqlConnector
```

Expected: both package restore commands finish with `Restore succeeded`.

- [ ] **Step 3: Replace the default app entry with a minimal bootstrap**

Write `DbLiteDesktop/Program.cs`:

```csharp
namespace DbLiteDesktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
```

- [ ] **Step 4: Update the README to reflect the actual MVP scope**

Write `README.md`:

```md
# LightDB-Client

Windows 下的轻量级只读数据库查询工具。

当前 MVP 范围：

- `NET 8 WinForms`
- `SQLite` / `MySQL`
- 连接配置保存
- 密码本地加密
- 表和字段浏览
- 只读 SQL 查询
- 查询历史
```

- [ ] **Step 5: Build the empty solution**

Run:

```powershell
dotnet build .\DbLiteDesktop.sln
```

Expected: `Build succeeded.`

- [ ] **Step 6: Commit the scaffold**

Run:

```powershell
git add DbLiteDesktop.sln DbLiteDesktop DbLiteDesktop.Tests README.md
git commit -m "chore: scaffold DB Lite Desktop solution"
```

## Task 2: Add Core Models and Service Tests

**Files:**
- Create: `DbLiteDesktop/Models/DbConnectionConfig.cs`
- Create: `DbLiteDesktop/Models/TableColumnInfo.cs`
- Create: `DbLiteDesktop/Models/QueryHistoryItem.cs`
- Create: `DbLiteDesktop/Services/SqlGuardService.cs`
- Create: `DbLiteDesktop/Services/PasswordEncryptService.cs`
- Create: `DbLiteDesktop.Tests/Services/SqlGuardServiceTests.cs`
- Create: `DbLiteDesktop.Tests/Services/PasswordEncryptServiceTests.cs`

- [ ] **Step 1: Write the failing SQL guard tests**

Write `DbLiteDesktop.Tests/Services/SqlGuardServiceTests.cs`:

```csharp
using DbLiteDesktop.Services;

namespace DbLiteDesktop.Tests.Services;

public class SqlGuardServiceTests
{
    private readonly SqlGuardService _service = new();

    [Theory]
    [InlineData("select * from users")]
    [InlineData(" show tables ")]
    [InlineData("with cte as (select 1) select * from cte")]
    [InlineData("pragma table_info(\"users\")")]
    public void IsReadonlySql_Allows_ReadonlySql(string sql)
    {
        Assert.True(_service.IsReadonlySql(sql));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("delete from users")]
    [InlineData("select * from users; delete from users")]
    [InlineData("insert into users(id) values(1)")]
    [InlineData("update users set name = 'x'")]
    public void IsReadonlySql_Rejects_DangerousSql(string sql)
    {
        Assert.False(_service.IsReadonlySql(sql));
    }
}
```

- [ ] **Step 2: Write the failing password encryption tests**

Write `DbLiteDesktop.Tests/Services/PasswordEncryptServiceTests.cs`:

```csharp
using DbLiteDesktop.Services;

namespace DbLiteDesktop.Tests.Services;

public class PasswordEncryptServiceTests
{
    private readonly PasswordEncryptService _service = new();

    [Fact]
    public void Encrypt_ThenDecrypt_ReturnsOriginalText()
    {
        const string input = "secret-password";

        var encrypted = _service.Encrypt(input);
        var decrypted = _service.Decrypt(encrypted);

        Assert.NotEqual(input, encrypted);
        Assert.Equal(input, decrypted);
    }

    [Fact]
    public void Encrypt_EmptyString_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, _service.Encrypt(string.Empty));
    }
}
```

- [ ] **Step 3: Run tests to confirm they fail before implementation**

Run:

```powershell
dotnet test .\DbLiteDesktop.Tests\DbLiteDesktop.Tests.csproj
```

Expected: FAIL with compile errors because `SqlGuardService` and `PasswordEncryptService` do not exist yet.

- [ ] **Step 4: Add the core models**

Write `DbLiteDesktop/Models/DbConnectionConfig.cs`:

```csharp
namespace DbLiteDesktop.Models;

public class DbConnectionConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DbType { get; set; } = string.Empty;
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? DatabaseName { get; set; }
    public string? Username { get; set; }
    public string? PasswordEncrypted { get; set; }
    public string? FilePath { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;

    public override string ToString()
    {
        return Name;
    }
}
```

Write `DbLiteDesktop/Models/TableColumnInfo.cs`:

```csharp
namespace DbLiteDesktop.Models;

public class TableColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Nullable { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
    public string? Extra { get; set; }
    public string? Comment { get; set; }
}
```

Write `DbLiteDesktop/Models/QueryHistoryItem.cs`:

```csharp
namespace DbLiteDesktop.Models;

public class QueryHistoryItem
{
    public int Id { get; set; }
    public int? ConnectionId { get; set; }
    public string DbType { get; set; } = string.Empty;
    public string? DatabaseName { get; set; }
    public string SqlText { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public int RowCount { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
```

- [ ] **Step 5: Implement the minimal services**

Write `DbLiteDesktop/Services/SqlGuardService.cs`:

```csharp
using System.Text.RegularExpressions;

namespace DbLiteDesktop.Services;

public class SqlGuardService
{
    private static readonly string[] AllowedPrefixes =
    [
        "select",
        "show",
        "desc",
        "describe",
        "explain",
        "pragma",
        "with"
    ];

    private static readonly string[] BlockedKeywords =
    [
        "insert",
        "update",
        "delete",
        "drop",
        "alter",
        "truncate",
        "create",
        "replace",
        "grant",
        "revoke",
        "merge",
        "exec",
        "execute",
        "call"
    ];

    public bool IsReadonlySql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return false;
        }

        var cleanSql = sql.Trim().ToLowerInvariant();
        var statements = cleanSql
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (statements.Length != 1)
        {
            return false;
        }

        var firstWord = cleanSql.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries)[0];

        if (!AllowedPrefixes.Contains(firstWord))
        {
            return false;
        }

        foreach (var keyword in BlockedKeywords)
        {
            if (Regex.IsMatch(cleanSql, $@"\b{keyword}\b"))
            {
                return false;
            }
        }

        return true;
    }
}
```

Write `DbLiteDesktop/Services/PasswordEncryptService.cs`:

```csharp
using System.Security.Cryptography;
using System.Text;

namespace DbLiteDesktop.Services;

public class PasswordEncryptService
{
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        var data = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
        {
            return string.Empty;
        }

        var data = Convert.FromBase64String(encryptedText);
        var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decrypted);
    }
}
```

- [ ] **Step 6: Run tests to verify they pass**

Run:

```powershell
dotnet test .\DbLiteDesktop.Tests\DbLiteDesktop.Tests.csproj
```

Expected: PASS.

- [ ] **Step 7: Commit the models and core services**

Run:

```powershell
git add DbLiteDesktop/Models DbLiteDesktop/Services DbLiteDesktop.Tests/Services
git commit -m "feat: add core models and service guards"
```

## Task 3: Add Config and History Storage

**Files:**
- Create: `DbLiteDesktop/Services/ConfigService.cs`
- Create: `DbLiteDesktop/Services/QueryHistoryService.cs`

- [ ] **Step 1: Write the config and history service skeletons**

Write `DbLiteDesktop/Services/ConfigService.cs`:

```csharp
using DbLiteDesktop.Models;
using Microsoft.Data.Sqlite;

namespace DbLiteDesktop.Services;

public class ConfigService
{
    public string DatabasePath { get; }

    public ConfigService()
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        Directory.CreateDirectory(dataDirectory);
        DatabasePath = Path.Combine(dataDirectory, "app_config.db");
    }

    public void Initialize()
    {
    }

    public List<DbConnectionConfig> GetConnections()
    {
        return [];
    }

    public DbConnectionConfig? GetConnectionById(int id)
    {
        return null;
    }

    public void SaveConnection(DbConnectionConfig config)
    {
    }

    public void DeleteConnection(int id)
    {
    }

    private SqliteConnection CreateConnection()
    {
        return new($"Data Source={DatabasePath}");
    }
}
```

Write `DbLiteDesktop/Services/QueryHistoryService.cs`:

```csharp
using DbLiteDesktop.Models;
using Microsoft.Data.Sqlite;

namespace DbLiteDesktop.Services;

public class QueryHistoryService
{
    private readonly string _databasePath;

    public QueryHistoryService(string databasePath)
    {
        _databasePath = databasePath;
    }

    public void Initialize()
    {
    }

    public void Add(QueryHistoryItem item)
    {
    }

    public List<QueryHistoryItem> GetRecent(int limit = 200)
    {
        return [];
    }

    private SqliteConnection CreateConnection()
    {
        return new($"Data Source={_databasePath}");
    }
}
```

- [ ] **Step 2: Implement table creation and connection CRUD**

Replace `DbLiteDesktop/Services/ConfigService.cs` with:

```csharp
using DbLiteDesktop.Models;
using Microsoft.Data.Sqlite;

namespace DbLiteDesktop.Services;

public class ConfigService
{
    public string DatabasePath { get; }

    public ConfigService()
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        Directory.CreateDirectory(dataDirectory);
        DatabasePath = Path.Combine(dataDirectory, "app_config.db");
    }

    public void Initialize()
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
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
            """;
        command.ExecuteNonQuery();
    }

    public List<DbConnectionConfig> GetConnections()
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, name, db_type, host, port, database_name, username, password_encrypted, file_path, created_at, updated_at
            FROM db_connections
            ORDER BY name;
            """;

        using var reader = command.ExecuteReader();
        var items = new List<DbConnectionConfig>();

        while (reader.Read())
        {
            items.Add(ReadConnection(reader));
        }

        return items;
    }

    public DbConnectionConfig? GetConnectionById(int id)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, name, db_type, host, port, database_name, username, password_encrypted, file_path, created_at, updated_at
            FROM db_connections
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadConnection(reader) : null;
    }

    public void SaveConnection(DbConnectionConfig config)
    {
        using var connection = CreateConnection();
        connection.Open();

        var now = DateTime.UtcNow.ToString("O");
        using var command = connection.CreateCommand();

        if (config.Id == 0)
        {
            command.CommandText =
                """
                INSERT INTO db_connections (
                    name, db_type, host, port, database_name, username, password_encrypted, file_path, created_at, updated_at
                )
                VALUES (
                    $name, $dbType, $host, $port, $databaseName, $username, $passwordEncrypted, $filePath, $createdAt, $updatedAt
                );
                """;
            config.CreatedAt = now;
        }
        else
        {
            command.CommandText =
                """
                UPDATE db_connections
                SET name = $name,
                    db_type = $dbType,
                    host = $host,
                    port = $port,
                    database_name = $databaseName,
                    username = $username,
                    password_encrypted = $passwordEncrypted,
                    file_path = $filePath,
                    updated_at = $updatedAt
                WHERE id = $id;
                """;
            command.Parameters.AddWithValue("$id", config.Id);
        }

        config.UpdatedAt = now;
        command.Parameters.AddWithValue("$name", config.Name);
        command.Parameters.AddWithValue("$dbType", config.DbType);
        command.Parameters.AddWithValue("$host", (object?)config.Host ?? DBNull.Value);
        command.Parameters.AddWithValue("$port", (object?)config.Port ?? DBNull.Value);
        command.Parameters.AddWithValue("$databaseName", (object?)config.DatabaseName ?? DBNull.Value);
        command.Parameters.AddWithValue("$username", (object?)config.Username ?? DBNull.Value);
        command.Parameters.AddWithValue("$passwordEncrypted", (object?)config.PasswordEncrypted ?? DBNull.Value);
        command.Parameters.AddWithValue("$filePath", (object?)config.FilePath ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", config.CreatedAt);
        command.Parameters.AddWithValue("$updatedAt", config.UpdatedAt);
        command.ExecuteNonQuery();
    }

    public void DeleteConnection(int id)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM db_connections WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private SqliteConnection CreateConnection()
    {
        return new($"Data Source={DatabasePath}");
    }

    private static DbConnectionConfig ReadConnection(SqliteDataReader reader)
    {
        return new()
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            DbType = reader.GetString(2),
            Host = reader.IsDBNull(3) ? null : reader.GetString(3),
            Port = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            DatabaseName = reader.IsDBNull(5) ? null : reader.GetString(5),
            Username = reader.IsDBNull(6) ? null : reader.GetString(6),
            PasswordEncrypted = reader.IsDBNull(7) ? null : reader.GetString(7),
            FilePath = reader.IsDBNull(8) ? null : reader.GetString(8),
            CreatedAt = reader.GetString(9),
            UpdatedAt = reader.GetString(10)
        };
    }
}
```

- [ ] **Step 3: Implement query history storage**

Replace `DbLiteDesktop/Services/QueryHistoryService.cs` with:

```csharp
using DbLiteDesktop.Models;
using Microsoft.Data.Sqlite;

namespace DbLiteDesktop.Services;

public class QueryHistoryService
{
    private readonly string _databasePath;

    public QueryHistoryService(string databasePath)
    {
        _databasePath = databasePath;
    }

    public void Initialize()
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
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
            """;
        command.ExecuteNonQuery();
    }

    public void Add(QueryHistoryItem item)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO query_history (
                connection_id, db_type, database_name, sql_text, success, error_message, duration_ms, row_count, created_at
            )
            VALUES (
                $connectionId, $dbType, $databaseName, $sqlText, $success, $errorMessage, $durationMs, $rowCount, $createdAt
            );
            """;
        command.Parameters.AddWithValue("$connectionId", (object?)item.ConnectionId ?? DBNull.Value);
        command.Parameters.AddWithValue("$dbType", item.DbType);
        command.Parameters.AddWithValue("$databaseName", (object?)item.DatabaseName ?? DBNull.Value);
        command.Parameters.AddWithValue("$sqlText", item.SqlText);
        command.Parameters.AddWithValue("$success", item.Success ? 1 : 0);
        command.Parameters.AddWithValue("$errorMessage", (object?)item.ErrorMessage ?? DBNull.Value);
        command.Parameters.AddWithValue("$durationMs", item.DurationMs);
        command.Parameters.AddWithValue("$rowCount", item.RowCount);
        command.Parameters.AddWithValue("$createdAt", item.CreatedAt);
        command.ExecuteNonQuery();
    }

    public List<QueryHistoryItem> GetRecent(int limit = 200)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, connection_id, db_type, database_name, sql_text, success, error_message, duration_ms, row_count, created_at
            FROM query_history
            ORDER BY id DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", limit);

        using var reader = command.ExecuteReader();
        var items = new List<QueryHistoryItem>();

        while (reader.Read())
        {
            items.Add(new QueryHistoryItem
            {
                Id = reader.GetInt32(0),
                ConnectionId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                DbType = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                DatabaseName = reader.IsDBNull(3) ? null : reader.GetString(3),
                SqlText = reader.GetString(4),
                Success = reader.GetInt32(5) == 1,
                ErrorMessage = reader.IsDBNull(6) ? null : reader.GetString(6),
                DurationMs = reader.IsDBNull(7) ? 0 : reader.GetInt64(7),
                RowCount = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                CreatedAt = reader.GetString(9)
            });
        }

        return items;
    }

    private SqliteConnection CreateConnection()
    {
        return new($"Data Source={_databasePath}");
    }
}
```

- [ ] **Step 4: Build and test after storage services**

Run:

```powershell
dotnet test .\DbLiteDesktop.Tests\DbLiteDesktop.Tests.csproj
dotnet build .\DbLiteDesktop.sln
```

Expected: both commands PASS.

- [ ] **Step 5: Commit local storage**

Run:

```powershell
git add DbLiteDesktop/Services
git commit -m "feat: add local config and query history storage"
```

## Task 4: Add Database Providers

**Files:**
- Create: `DbLiteDesktop/Providers/IDatabaseProvider.cs`
- Create: `DbLiteDesktop/Providers/DatabaseProviderFactory.cs`
- Create: `DbLiteDesktop/Providers/MySqlProvider.cs`
- Create: `DbLiteDesktop/Providers/SQLiteProvider.cs`
- Create: `DbLiteDesktop/Utils/IdentifierQuoteHelper.cs`

- [ ] **Step 1: Add the provider interface and factory**

Write `DbLiteDesktop/Providers/IDatabaseProvider.cs`:

```csharp
using System.Data;
using DbLiteDesktop.Models;

namespace DbLiteDesktop.Providers;

public interface IDatabaseProvider
{
    bool TestConnection(DbConnectionConfig config, string password);
    List<string> GetTables(DbConnectionConfig config, string password);
    List<TableColumnInfo> GetColumns(DbConnectionConfig config, string password, string tableName);
    DataTable ExecuteQuery(DbConnectionConfig config, string password, string sql, int maxRows = 1000);
    string BuildPreviewSql(string tableName, int limit = 100);
}
```

Write `DbLiteDesktop/Providers/DatabaseProviderFactory.cs`:

```csharp
namespace DbLiteDesktop.Providers;

public static class DatabaseProviderFactory
{
    public static IDatabaseProvider Create(string dbType)
    {
        return dbType.ToLowerInvariant() switch
        {
            "mysql" => new MySqlProvider(),
            "sqlite" => new SQLiteProvider(),
            _ => throw new InvalidOperationException("暂不支持该数据库类型")
        };
    }
}
```

- [ ] **Step 2: Add the identifier quoting helper**

Write `DbLiteDesktop/Utils/IdentifierQuoteHelper.cs`:

```csharp
namespace DbLiteDesktop.Utils;

public static class IdentifierQuoteHelper
{
    public static string QuoteMySql(string name)
    {
        return $"`{name.Replace("`", "``")}`";
    }

    public static string QuoteSQLite(string name)
    {
        return $"\"{name.Replace("\"", "\"\"")}\"";
    }
}
```

- [ ] **Step 3: Implement the SQLite provider**

Write `DbLiteDesktop/Providers/SQLiteProvider.cs`:

```csharp
using System.Data;
using DbLiteDesktop.Models;
using DbLiteDesktop.Services;
using DbLiteDesktop.Utils;
using Microsoft.Data.Sqlite;

namespace DbLiteDesktop.Providers;

public class SQLiteProvider : IDatabaseProvider
{
    public bool TestConnection(DbConnectionConfig config, string password)
    {
        using var connection = CreateConnection(config);
        connection.Open();
        return true;
    }

    public List<string> GetTables(DbConnectionConfig config, string password)
    {
        using var connection = CreateConnection(config);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT name
            FROM sqlite_master
            WHERE type = 'table'
            ORDER BY name;
            """;

        using var reader = command.ExecuteReader();
        var items = new List<string>();

        while (reader.Read())
        {
            items.Add(reader.GetString(0));
        }

        return items;
    }

    public List<TableColumnInfo> GetColumns(DbConnectionConfig config, string password, string tableName)
    {
        using var connection = CreateConnection(config);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({IdentifierQuoteHelper.QuoteSQLite(tableName)});";

        using var reader = command.ExecuteReader();
        var items = new List<TableColumnInfo>();

        while (reader.Read())
        {
            items.Add(new TableColumnInfo
            {
                Name = reader["name"]?.ToString() ?? string.Empty,
                Type = reader["type"]?.ToString() ?? string.Empty,
                Nullable = (reader["notnull"]?.ToString() == "1") ? "NO" : "YES",
                Key = (reader["pk"]?.ToString() == "1") ? "PRI" : string.Empty,
                DefaultValue = reader["dflt_value"]?.ToString(),
                Extra = string.Empty,
                Comment = string.Empty
            });
        }

        return items;
    }

    public DataTable ExecuteQuery(DbConnectionConfig config, string password, string sql, int maxRows = 1000)
    {
        var guard = new SqlGuardService();
        if (!guard.IsReadonlySql(sql))
        {
            throw new InvalidOperationException("当前工具只允许执行只读 SQL");
        }

        using var connection = CreateConnection(config);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 30;

        using var reader = command.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);

        if (table.Rows.Count > maxRows)
        {
            while (table.Rows.Count > maxRows)
            {
                table.Rows.RemoveAt(table.Rows.Count - 1);
            }
        }

        return table;
    }

    public string BuildPreviewSql(string tableName, int limit = 100)
    {
        return $"SELECT * FROM {IdentifierQuoteHelper.QuoteSQLite(tableName)} LIMIT {limit};";
    }

    private static SqliteConnection CreateConnection(DbConnectionConfig config)
    {
        return new($"Data Source={config.FilePath}");
    }
}
```

- [ ] **Step 4: Implement the MySQL provider**

Write `DbLiteDesktop/Providers/MySqlProvider.cs`:

```csharp
using System.Data;
using DbLiteDesktop.Models;
using DbLiteDesktop.Services;
using DbLiteDesktop.Utils;
using MySqlConnector;

namespace DbLiteDesktop.Providers;

public class MySqlProvider : IDatabaseProvider
{
    public bool TestConnection(DbConnectionConfig config, string password)
    {
        using var connection = CreateConnection(config, password);
        connection.Open();

        using var command = new MySqlCommand("SELECT 1", connection);
        return command.ExecuteScalar() is not null;
    }

    public List<string> GetTables(DbConnectionConfig config, string password)
    {
        using var connection = CreateConnection(config, password);
        connection.Open();

        using var command = new MySqlCommand("SHOW TABLES;", connection);
        using var reader = command.ExecuteReader();
        var items = new List<string>();

        while (reader.Read())
        {
            items.Add(reader.GetString(0));
        }

        return items;
    }

    public List<TableColumnInfo> GetColumns(DbConnectionConfig config, string password, string tableName)
    {
        using var connection = CreateConnection(config, password);
        connection.Open();

        using var command = new MySqlCommand(
            $"SHOW FULL COLUMNS FROM {IdentifierQuoteHelper.QuoteMySql(tableName)};",
            connection);
        using var reader = command.ExecuteReader();
        var items = new List<TableColumnInfo>();

        while (reader.Read())
        {
            items.Add(new TableColumnInfo
            {
                Name = reader["Field"]?.ToString() ?? string.Empty,
                Type = reader["Type"]?.ToString() ?? string.Empty,
                Nullable = reader["Null"]?.ToString() ?? string.Empty,
                Key = reader["Key"]?.ToString() ?? string.Empty,
                DefaultValue = reader["Default"]?.ToString(),
                Extra = reader["Extra"]?.ToString(),
                Comment = reader["Comment"]?.ToString()
            });
        }

        return items;
    }

    public DataTable ExecuteQuery(DbConnectionConfig config, string password, string sql, int maxRows = 1000)
    {
        var guard = new SqlGuardService();
        if (!guard.IsReadonlySql(sql))
        {
            throw new InvalidOperationException("当前工具只允许执行只读 SQL");
        }

        using var connection = CreateConnection(config, password);
        connection.Open();

        using var command = new MySqlCommand(sql, connection)
        {
            CommandTimeout = 30
        };
        using var reader = command.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);

        if (table.Rows.Count > maxRows)
        {
            while (table.Rows.Count > maxRows)
            {
                table.Rows.RemoveAt(table.Rows.Count - 1);
            }
        }

        return table;
    }

    public string BuildPreviewSql(string tableName, int limit = 100)
    {
        return $"SELECT * FROM {IdentifierQuoteHelper.QuoteMySql(tableName)} LIMIT {limit};";
    }

    private static MySqlConnection CreateConnection(DbConnectionConfig config, string password)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = config.Host,
            Port = (uint)(config.Port ?? 3306),
            Database = config.DatabaseName,
            UserID = config.Username,
            Password = password,
            CharacterSet = "utf8mb4",
            ConnectionTimeout = 10,
            DefaultCommandTimeout = 30,
            Pooling = false
        };

        return new(builder.ConnectionString);
    }
}
```

- [ ] **Step 5: Build and run tests after adding providers**

Run:

```powershell
dotnet test .\DbLiteDesktop.Tests\DbLiteDesktop.Tests.csproj
dotnet build .\DbLiteDesktop.sln
```

Expected: both commands PASS.

- [ ] **Step 6: Commit providers**

Run:

```powershell
git add DbLiteDesktop/Providers DbLiteDesktop/Utils
git commit -m "feat: add SQLite and MySQL providers"
```

## Task 5: Build the Connection Form

**Files:**
- Create: `DbLiteDesktop/Forms/ConnectionForm.cs`
- Create: `DbLiteDesktop/Forms/ConnectionForm.Designer.cs`

- [ ] **Step 1: Create the form shell**

Write `DbLiteDesktop/Forms/ConnectionForm.cs`:

```csharp
using DbLiteDesktop.Models;
using DbLiteDesktop.Providers;
using DbLiteDesktop.Services;

namespace DbLiteDesktop.Forms;

public partial class ConnectionForm : Form
{
    private readonly PasswordEncryptService _passwordEncryptService = new();
    public DbConnectionConfig ConnectionConfig { get; private set; }
    public bool ConnectAfterSave { get; private set; }

    public ConnectionForm(DbConnectionConfig? existing = null)
    {
        InitializeComponent();
        ConnectionConfig = existing is null ? new DbConnectionConfig { DbType = "sqlite" } : Clone(existing);
        LoadFromModel();
    }
}
```

- [ ] **Step 2: Build the connection form layout**

Write `DbLiteDesktop/Forms/ConnectionForm.Designer.cs` with controls:

```csharp
namespace DbLiteDesktop.Forms;

partial class ConnectionForm
{
    private System.ComponentModel.IContainer components = null!;
    private Label lblName = null!;
    private TextBox txtName = null!;
    private Label lblDbType = null!;
    private ComboBox cboDbType = null!;
    private Label lblHost = null!;
    private TextBox txtHost = null!;
    private Label lblPort = null!;
    private NumericUpDown numPort = null!;
    private Label lblDatabaseName = null!;
    private TextBox txtDatabaseName = null!;
    private Label lblUsername = null!;
    private TextBox txtUsername = null!;
    private Label lblPassword = null!;
    private TextBox txtPassword = null!;
    private Label lblFilePath = null!;
    private TextBox txtFilePath = null!;
    private Button btnBrowse = null!;
    private Button btnTest = null!;
    private Button btnSave = null!;
    private Button btnConnect = null!;
    private Button btnCancel = null!;

    private void InitializeComponent()
    {
        lblName = new Label();
        txtName = new TextBox();
        lblDbType = new Label();
        cboDbType = new ComboBox();
        lblHost = new Label();
        txtHost = new TextBox();
        lblPort = new Label();
        numPort = new NumericUpDown();
        lblDatabaseName = new Label();
        txtDatabaseName = new TextBox();
        lblUsername = new Label();
        txtUsername = new TextBox();
        lblPassword = new Label();
        txtPassword = new TextBox();
        lblFilePath = new Label();
        txtFilePath = new TextBox();
        btnBrowse = new Button();
        btnTest = new Button();
        btnSave = new Button();
        btnConnect = new Button();
        btnCancel = new Button();
        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        SuspendLayout();
    }
}
```

Expected: a normal designer file with explicit sizes, positions, handlers, and `txtPassword.UseSystemPasswordChar = true`.

- [ ] **Step 3: Implement the form behavior**

Complete `DbLiteDesktop/Forms/ConnectionForm.cs` with methods:

```csharp
private void LoadFromModel()
{
    cboDbType.Items.AddRange(["sqlite", "mysql"]);
    cboDbType.SelectedItem = ConnectionConfig.DbType;
    txtName.Text = ConnectionConfig.Name;
    txtHost.Text = ConnectionConfig.Host;
    numPort.Value = ConnectionConfig.Port ?? 3306;
    txtDatabaseName.Text = ConnectionConfig.DatabaseName;
    txtUsername.Text = ConnectionConfig.Username;
    txtFilePath.Text = ConnectionConfig.FilePath;

    if (!string.IsNullOrEmpty(ConnectionConfig.PasswordEncrypted) && ConnectionConfig.DbType != "sqlite")
    {
        txtPassword.Text = _passwordEncryptService.Decrypt(ConnectionConfig.PasswordEncrypted);
    }

    UpdateFieldVisibility();
}

private void UpdateFieldVisibility()
{
    var isSqlite = string.Equals(cboDbType.SelectedItem?.ToString(), "sqlite", StringComparison.OrdinalIgnoreCase);

    lblHost.Visible = txtHost.Visible = !isSqlite;
    lblPort.Visible = numPort.Visible = !isSqlite;
    lblDatabaseName.Visible = txtDatabaseName.Visible = !isSqlite;
    lblUsername.Visible = txtUsername.Visible = !isSqlite;
    lblPassword.Visible = txtPassword.Visible = !isSqlite;

    lblFilePath.Visible = txtFilePath.Visible = btnBrowse.Visible = isSqlite;
}

private DbConnectionConfig BuildConfigFromInputs()
{
    var dbType = cboDbType.SelectedItem?.ToString() ?? "sqlite";

    return new DbConnectionConfig
    {
        Id = ConnectionConfig.Id,
        Name = txtName.Text.Trim(),
        DbType = dbType,
        Host = dbType == "mysql" ? txtHost.Text.Trim() : null,
        Port = dbType == "mysql" ? (int)numPort.Value : null,
        DatabaseName = dbType == "mysql" ? txtDatabaseName.Text.Trim() : null,
        Username = dbType == "mysql" ? txtUsername.Text.Trim() : null,
        PasswordEncrypted = dbType == "mysql" ? _passwordEncryptService.Encrypt(txtPassword.Text) : null,
        FilePath = dbType == "sqlite" ? txtFilePath.Text.Trim() : null,
        CreatedAt = ConnectionConfig.CreatedAt,
        UpdatedAt = ConnectionConfig.UpdatedAt
    };
}
```

Also add:

- input validation for empty name
- input validation for empty SQLite path or MySQL host/database/user
- file picker for SQLite
- `btnTest_Click` that creates a provider and tests the connection
- `btnSave_Click` that stores `ConnectionConfig`, sets `DialogResult = DialogResult.OK`, and leaves `ConnectAfterSave = false`
- `btnConnect_Click` that stores `ConnectionConfig`, sets `ConnectAfterSave = true`, then closes with `DialogResult.OK`

- [ ] **Step 4: Build after connection form**

Run:

```powershell
dotnet build .\DbLiteDesktop.sln
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit the connection form**

Run:

```powershell
git add DbLiteDesktop/Forms
git commit -m "feat: add database connection form"
```

## Task 6: Build the Main Form

**Files:**
- Create: `DbLiteDesktop/MainForm.cs`
- Create: `DbLiteDesktop/MainForm.Designer.cs`

- [ ] **Step 1: Create the main form layout**

Write `DbLiteDesktop/MainForm.Designer.cs` with these controls:

```csharp
namespace DbLiteDesktop;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private ToolStrip toolStrip = null!;
    private ToolStripComboBox cboConnections = null!;
    private ToolStripButton btnNewConnection = null!;
    private ToolStripButton btnEditConnection = null!;
    private ToolStripButton btnDeleteConnection = null!;
    private ToolStripButton btnTestConnection = null!;
    private ToolStripButton btnConnect = null!;
    private ToolStripButton btnRefresh = null!;
    private ToolStripButton btnDisconnect = null!;
    private SplitContainer splitContainer = null!;
    private TreeView treeTables = null!;
    private TabControl tabMain = null!;
    private TabPage tabColumns = null!;
    private TabPage tabSql = null!;
    private TabPage tabHistory = null!;
    private DataGridView gridColumns = null!;
    private TextBox txtSql = null!;
    private FlowLayoutPanel sqlButtonPanel = null!;
    private Button btnRunSql = null!;
    private Button btnClearSql = null!;
    private Button btnCopySql = null!;
    private DataGridView gridResults = null!;
    private Label lblStatus = null!;
    private DataGridView gridHistory = null!;

    private void InitializeComponent()
    {
    }
}
```

Expected: a working layout with the left tree docked, right tabs docked, and grids set `ReadOnly = true`.

- [ ] **Step 2: Add the main form constructor and startup initialization**

Write the start of `DbLiteDesktop/MainForm.cs`:

```csharp
using System.Data;
using DbLiteDesktop.Forms;
using DbLiteDesktop.Models;
using DbLiteDesktop.Providers;
using DbLiteDesktop.Services;

namespace DbLiteDesktop;

public partial class MainForm : Form
{
    private readonly ConfigService _configService = new();
    private readonly PasswordEncryptService _passwordEncryptService = new();
    private QueryHistoryService _queryHistoryService = null!;
    private DbConnectionConfig? _currentConfig;

    public MainForm()
    {
        InitializeComponent();
        _configService.Initialize();
        _queryHistoryService = new QueryHistoryService(_configService.DatabasePath);
        _queryHistoryService.Initialize();
        LoadConnections();
        LoadHistory();
    }
}
```

- [ ] **Step 3: Implement connection list and form actions**

Add to `DbLiteDesktop/MainForm.cs`:

```csharp
private void LoadConnections()
{
    var items = _configService.GetConnections();
    cboConnections.Items.Clear();

    foreach (var item in items)
    {
        cboConnections.Items.Add(item);
    }

    if (cboConnections.Items.Count > 0)
    {
        cboConnections.SelectedIndex = 0;
    }
}

private DbConnectionConfig? GetSelectedConnection()
{
    return cboConnections.SelectedItem as DbConnectionConfig;
}

private void OpenConnectionForm(DbConnectionConfig? config = null)
{
    using var form = new ConnectionForm(config);
    if (form.ShowDialog(this) != DialogResult.OK)
    {
        return;
    }

    _configService.SaveConnection(form.ConnectionConfig);
    LoadConnections();

    if (form.ConnectAfterSave)
    {
        SelectConnection(form.ConnectionConfig.Name);
        ConnectSelected();
    }
}
```

Also add handlers for:

- `btnNewConnection_Click`
- `btnEditConnection_Click`
- `btnDeleteConnection_Click`
- `btnTestConnection_Click`
- `btnConnect_Click`
- `btnRefresh_Click`
- `btnDisconnect_Click`

`SelectConnection` should re-select the saved item by name after reload.

- [ ] **Step 4: Implement database loading and SQL execution**

Add to `DbLiteDesktop/MainForm.cs`:

```csharp
private void ConnectSelected()
{
    var config = GetSelectedConnection();
    if (config is null)
    {
        MessageBox.Show("请先选择连接。", "提示");
        return;
    }

    var provider = DatabaseProviderFactory.Create(config.DbType);
    var password = config.DbType == "sqlite"
        ? string.Empty
        : _passwordEncryptService.Decrypt(config.PasswordEncrypted ?? string.Empty);

    provider.TestConnection(config, password);
    _currentConfig = config;
    LoadTables();
    lblStatus.Text = $"已连接：{config.Name}";
}

private void LoadTables()
{
    if (_currentConfig is null)
    {
        return;
    }

    var provider = DatabaseProviderFactory.Create(_currentConfig.DbType);
    var password = _currentConfig.DbType == "sqlite"
        ? string.Empty
        : _passwordEncryptService.Decrypt(_currentConfig.PasswordEncrypted ?? string.Empty);
    var tables = provider.GetTables(_currentConfig, password);

    treeTables.Nodes.Clear();
    foreach (var table in tables)
    {
        treeTables.Nodes.Add(table);
    }
}

private void LoadColumnsForTable(string tableName)
{
    if (_currentConfig is null)
    {
        return;
    }

    var provider = DatabaseProviderFactory.Create(_currentConfig.DbType);
    var password = _currentConfig.DbType == "sqlite"
        ? string.Empty
        : _passwordEncryptService.Decrypt(_currentConfig.PasswordEncrypted ?? string.Empty);
    var columns = provider.GetColumns(_currentConfig, password, tableName);

    gridColumns.DataSource = columns;
    txtSql.Text = provider.BuildPreviewSql(tableName, 100);
    tabMain.SelectedTab = tabColumns;
}
```

Also implement `RunSql()`:

```csharp
private void RunSql()
{
    if (_currentConfig is null)
    {
        MessageBox.Show("请先连接数据库。", "提示");
        return;
    }

    var sql = txtSql.Text.Trim();
    var start = DateTime.UtcNow;

    try
    {
        var provider = DatabaseProviderFactory.Create(_currentConfig.DbType);
        var password = _currentConfig.DbType == "sqlite"
            ? string.Empty
            : _passwordEncryptService.Decrypt(_currentConfig.PasswordEncrypted ?? string.Empty);
        var result = provider.ExecuteQuery(_currentConfig, password, sql, 1000);
        var duration = (long)(DateTime.UtcNow - start).TotalMilliseconds;

        gridResults.DataSource = result;
        lblStatus.Text = $"查询成功，返回 {result.Rows.Count} 行，耗时 {duration} ms";

        _queryHistoryService.Add(new QueryHistoryItem
        {
            ConnectionId = _currentConfig.Id,
            DbType = _currentConfig.DbType,
            DatabaseName = _currentConfig.DatabaseName ?? _currentConfig.FilePath,
            SqlText = sql,
            Success = true,
            DurationMs = duration,
            RowCount = result.Rows.Count,
            CreatedAt = DateTime.UtcNow.ToString("O")
        });

        LoadHistory();
        tabMain.SelectedTab = tabSql;
    }
    catch (Exception ex)
    {
        var duration = (long)(DateTime.UtcNow - start).TotalMilliseconds;
        lblStatus.Text = $"查询失败：{ex.Message}";

        _queryHistoryService.Add(new QueryHistoryItem
        {
            ConnectionId = _currentConfig.Id,
            DbType = _currentConfig.DbType,
            DatabaseName = _currentConfig.DatabaseName ?? _currentConfig.FilePath,
            SqlText = sql,
            Success = false,
            ErrorMessage = ex.Message,
            DurationMs = duration,
            RowCount = 0,
            CreatedAt = DateTime.UtcNow.ToString("O")
        });

        LoadHistory();
        MessageBox.Show(ex.Message, "查询失败");
    }
}
```

- [ ] **Step 5: Implement history reload and interaction**

Add to `DbLiteDesktop/MainForm.cs`:

```csharp
private void LoadHistory()
{
    gridHistory.DataSource = _queryHistoryService.GetRecent();
}

private void Disconnect()
{
    _currentConfig = null;
    treeTables.Nodes.Clear();
    gridColumns.DataSource = null;
    gridResults.DataSource = null;
    lblStatus.Text = "未连接";
}
```

Add handlers:

- `treeTables.AfterSelect` -> `LoadColumnsForTable(e.Node.Text)`
- `btnRunSql.Click` -> `RunSql()`
- `btnClearSql.Click` -> clear `txtSql`
- `btnCopySql.Click` -> `Clipboard.SetText(txtSql.Text)`
- `gridHistory.CellDoubleClick` -> copy selected row `SqlText` back into `txtSql` and switch to `tabSql`

- [ ] **Step 6: Build after the main form**

Run:

```powershell
dotnet build .\DbLiteDesktop.sln
```

Expected: `Build succeeded.`

- [ ] **Step 7: Commit the main workflow**

Run:

```powershell
git add DbLiteDesktop/MainForm.cs DbLiteDesktop/MainForm.Designer.cs
git commit -m "feat: add main database browsing workflow"
```

## Task 7: Final Verification

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Run the automated checks**

Run:

```powershell
dotnet test .\DbLiteDesktop.Tests\DbLiteDesktop.Tests.csproj
dotnet build .\DbLiteDesktop.sln -c Release
```

Expected: both commands PASS.

- [ ] **Step 2: Launch the desktop app for manual verification**

Run:

```powershell
dotnet run --project .\DbLiteDesktop\DbLiteDesktop.csproj
```

Manual check list:

- app starts without crashing
- `data/app_config.db` is created
- can add a SQLite connection and save it
- can reopen and edit that connection
- can delete a saved connection
- dangerous SQL like `delete from users` is blocked
- history grid updates after each query

- [ ] **Step 3: Record any observed manual-test-only limitation in README if needed**

If a limitation is discovered, update `README.md` with one short note. Otherwise make no code change.

- [ ] **Step 4: Commit the finished MVP**

Run:

```powershell
git add README.md
git commit -m "feat: deliver DB Lite Desktop MVP"
```
