using System.Text.RegularExpressions;

namespace DbLiteDesktop.Services;

public static class SqlGuardService
{
    private static readonly string[] AllowedPrefixes =
    [
        "select",
        "show",
        "desc",
        "describe",
        "explain",
        "pragma",
        "with"
    ];

    private static readonly Regex[] BlockedKeywordRegexes = new[]
    {
        "insert", "update", "delete", "drop", "alter", "truncate",
        "create", "replace", "grant", "revoke", "merge", "exec",
        "execute", "call"
    }
    .Select(keyword => new Regex($@"\b{keyword}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase))
    .ToArray();

    public static bool IsReadonlySql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return false;
        }

        var cleanSql = sql.Trim();
        var statements = cleanSql.Split(
            ';',
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
        );

        if (statements.Length != 1)
        {
            return false;
        }

        var firstWord = cleanSql
            .Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries)[0];

        if (!AllowedPrefixes.Contains(firstWord, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        foreach (var regex in BlockedKeywordRegexes)
        {
            if (regex.IsMatch(cleanSql))
            {
                return false;
            }
        }

        return true;
    }
}
