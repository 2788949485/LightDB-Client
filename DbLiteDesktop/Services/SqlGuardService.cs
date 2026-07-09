using System.Text.RegularExpressions;

namespace DbLiteDesktop.Services;

public class SqlGuardService
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

    private static readonly string[] BlockedKeywords =
    [
        "insert",
        "update",
        "delete",
        "drop",
        "alter",
        "truncate",
        "create",
        "replace",
        "grant",
        "revoke",
        "merge",
        "exec",
        "execute",
        "call"
    ];

    public bool IsReadonlySql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return false;
        }

        var cleanSql = sql.Trim().ToLowerInvariant();
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

        if (!AllowedPrefixes.Contains(firstWord))
        {
            return false;
        }

        foreach (var keyword in BlockedKeywords)
        {
            if (Regex.IsMatch(cleanSql, $@"\b{keyword}\b"))
            {
                return false;
            }
        }

        return true;
    }
}
