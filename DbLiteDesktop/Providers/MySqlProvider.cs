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

        using var command = new MySqlCommand("SELECT 1;", connection);
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
            connection
        );
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
        TrimRows(table, maxRows);
        return table;
    }

    public string BuildPreviewSql(string tableName, int limit = 100)
    {
        return $"SELECT * FROM {IdentifierQuoteHelper.QuoteMySql(tableName)} LIMIT {limit};";
    }

    public string BuildPagedPreviewSql(string tableName, int page, int pageSize)
    {
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Max(pageSize, 1);
        var offset = (safePage - 1) * safePageSize;
        return $"SELECT * FROM {IdentifierQuoteHelper.QuoteMySql(tableName)} LIMIT {safePageSize} OFFSET {offset};";
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

    private static void TrimRows(DataTable table, int maxRows)
    {
        while (table.Rows.Count > maxRows)
        {
            table.Rows.RemoveAt(table.Rows.Count - 1);
        }
    }
}
