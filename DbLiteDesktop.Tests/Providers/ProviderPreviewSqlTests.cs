using DbLiteDesktop.Providers;
using Xunit;

namespace DbLiteDesktop.Tests.Providers;

public class ProviderPreviewSqlTests
{
    [Fact]
    public void SQLiteProvider_BuildPagedPreviewSql_UsesLimitOffset()
    {
        var provider = new SQLiteProvider();

        var sql = provider.BuildPagedPreviewSql("users", 2, 100);

        Assert.Equal("SELECT * FROM \"users\" LIMIT 100 OFFSET 100;", sql);
    }

    [Fact]
    public void MySqlProvider_BuildPagedPreviewSql_UsesLimitOffset()
    {
        var provider = new MySqlProvider();

        var sql = provider.BuildPagedPreviewSql("orders", 3, 50);

        Assert.Equal("SELECT * FROM `orders` LIMIT 50 OFFSET 100;", sql);
    }
}
