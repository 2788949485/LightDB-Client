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
