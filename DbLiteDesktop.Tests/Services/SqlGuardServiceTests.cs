using DbLiteDesktop.Services;
using Xunit;

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
