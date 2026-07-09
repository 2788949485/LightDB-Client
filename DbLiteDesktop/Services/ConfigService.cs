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
            config.CreatedAt = string.IsNullOrWhiteSpace(config.CreatedAt) ? now : config.CreatedAt;
            command.CommandText =
                """
                INSERT INTO db_connections (
                    name, db_type, host, port, database_name, username, password_encrypted, file_path, created_at, updated_at
                )
                VALUES (
                    $name, $dbType, $host, $port, $databaseName, $username, $passwordEncrypted, $filePath, $createdAt, $updatedAt
                );
                SELECT last_insert_rowid();
                """;
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

        if (config.Id == 0)
        {
            config.Id = Convert.ToInt32(command.ExecuteScalar());
        }
        else
        {
            command.ExecuteNonQuery();
        }
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
