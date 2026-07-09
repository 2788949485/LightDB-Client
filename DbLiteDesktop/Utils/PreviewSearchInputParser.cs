namespace DbLiteDesktop.Utils;

public static class PreviewSearchInputParser
{
    public static PreviewSearchParseResult Parse(
        string rawInput,
        IReadOnlyList<string> columns,
        string? selectedField,
        bool exactMatch
    )
    {
        var input = rawInput.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            return PreviewSearchParseResult.Empty(selectedField, exactMatch);
        }

        var normalizedInput = input.Replace('＝', '=');
        var separatorIndex = normalizedInput.IndexOf('=');

        if (separatorIndex > 0)
        {
            var fieldName = normalizedInput[..separatorIndex].Trim();
            var keyword = normalizedInput[(separatorIndex + 1)..].Trim();
            var matchedField = columns.FirstOrDefault(
                column => string.Equals(column, fieldName, StringComparison.OrdinalIgnoreCase)
            );

            if (matchedField is null)
            {
                return PreviewSearchParseResult.Fail($"当前表不存在该字段：{fieldName}");
            }

            return new PreviewSearchParseResult(true, matchedField, keyword, true, null);
        }

        return new PreviewSearchParseResult(true, selectedField, input, exactMatch, null);
    }
}

public sealed record PreviewSearchParseResult(
    bool Success,
    string? FieldName,
    string Keyword,
    bool ExactMatch,
    string? ErrorMessage
)
{
    public static PreviewSearchParseResult Empty(string? fieldName, bool exactMatch)
    {
        return new(true, fieldName, string.Empty, exactMatch, null);
    }

    public static PreviewSearchParseResult Fail(string message)
    {
        return new(false, null, string.Empty, false, message);
    }
}
