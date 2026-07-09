namespace DbLiteDesktop.Utils;

public static class IdentifierQuoteHelper
{
    public static string QuoteMySql(string name)
    {
        return $"`{name.Replace("`", "``")}`";
    }

    public static string QuoteSQLite(string name)
    {
        return $"\"{name.Replace("\"", "\"\"")}\"";
    }
}
