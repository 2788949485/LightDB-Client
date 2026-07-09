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
              AND name NOT LIKE 'sqlite_%'
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
                Nullable = reader["notnull"]?.ToString() == "1" ? "NO" : "YES",
                Key = reader["pk"]?.ToString() == "1" ? "PRI" : string.Empty,
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
        TrimRows(table, maxRows);
        return table;
    }

    public string BuildPreviewSql(string tableName, int limit = 100)
    {
        return $"SELECT * FROM {IdentifierQuoteHelper.QuoteSQLite(tableName)} LIMIT {limit};";
    }

    public string BuildPagedPreviewSql(string tableName, int page, int pageSize)
    {
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Max(pageSize, 1);
        var offset = (safePage - 1) * safePageSize;
        return $"SELECT * FROM {IdentifierQuoteHelper.QuoteSQLite(tableName)} LIMIT {safePageSize} OFFSET {offset};";
    }

    private static SqliteConnection CreateConnection(DbConnectionConfig config)
    {
        return new($"Data Source={config.FilePath}");
    }

    private static void TrimRows(DataTable table, int maxRows)
    {
        while (table.Rows.Count > maxRows)
        {
            table.Rows.RemoveAt(table.Rows.Count - 1);
        }
    }
}
