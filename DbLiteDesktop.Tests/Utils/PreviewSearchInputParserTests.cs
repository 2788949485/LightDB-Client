using DbLiteDesktop.Utils;
using Xunit;

namespace DbLiteDesktop.Tests.Utils;

public class PreviewSearchInputParserTests
{
    [Fact]
    public void Parse_UsesExplicitFieldWhenInputContainsEquals()
    {
        var result = PreviewSearchInputParser.Parse(
            "member_id=1704244313",
            ["id", "member_id", "teacher_name"],
            "id",
            false
        );

        Assert.True(result.Success);
        Assert.Equal("member_id", result.FieldName);
        Assert.Equal("1704244313", result.Keyword);
        Assert.True(result.ExactMatch);
    }

    [Fact]
    public void Parse_UsesSelectedFieldWhenInputContainsOnlyKeyword()
    {
        var result = PreviewSearchInputParser.Parse(
            "1704244313",
            ["id", "member_id", "teacher_name"],
            "member_id",
            false
        );

        Assert.True(result.Success);
        Assert.Equal("member_id", result.FieldName);
        Assert.Equal("1704244313", result.Keyword);
        Assert.False(result.ExactMatch);
    }

    [Fact]
    public void Parse_ReturnsErrorWhenExplicitFieldDoesNotExist()
    {
        var result = PreviewSearchInputParser.Parse(
            "unknown=1704244313",
            ["id", "member_id", "teacher_name"],
            "member_id",
            true
        );

        Assert.False(result.Success);
        Assert.Equal("当前表不存在该字段：unknown", result.ErrorMessage);
    }

    [Fact]
    public void Parse_SupportsChineseEqualsCharacter()
    {
        var result = PreviewSearchInputParser.Parse(
            "member_id＝1704244313",
            ["id", "member_id"],
            "id",
            false
        );

        Assert.True(result.Success);
        Assert.Equal("member_id", result.FieldName);
        Assert.Equal("1704244313", result.Keyword);
        Assert.True(result.ExactMatch);
    }
}
