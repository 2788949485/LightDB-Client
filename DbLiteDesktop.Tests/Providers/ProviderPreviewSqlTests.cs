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

    [Fact]
    public void SQLiteProvider_BuildFilteredPreviewSql_UsesLikeForAllColumns()
    {
        var provider = new SQLiteProvider();

        var sql = provider.BuildFilteredPreviewSql(
            "users",
            ["name", "phone"],
            null,
            "138",
            false,
            1,
            100
        );

        Assert.Equal(
            "SELECT * FROM \"users\" WHERE CAST(\"name\" AS TEXT) LIKE '%138%' OR CAST(\"phone\" AS TEXT) LIKE '%138%' LIMIT 100 OFFSET 0;",
            sql
        );
    }

    [Fact]
    public void MySqlProvider_BuildFilteredPreviewSql_UsesExactMatchForSingleColumn()
    {
        var provider = new MySqlProvider();

        var sql = provider.BuildFilteredPreviewSql(
            "orders",
            ["order_no", "member_id"],
            "order_no",
            "A001",
            true,
            2,
            50
        );

        Assert.Equal(
            "SELECT * FROM `orders` WHERE CAST(`order_no` AS CHAR) = 'A001' LIMIT 50 OFFSET 50;",
            sql
        );
    }
}
