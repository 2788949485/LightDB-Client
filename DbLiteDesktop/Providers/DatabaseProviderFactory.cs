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
