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

        if (string.IsNullOrWhiteSpace(item.CreatedAt))
        {
            item.CreatedAt = DateTime.UtcNow.ToString("O");
        }

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
