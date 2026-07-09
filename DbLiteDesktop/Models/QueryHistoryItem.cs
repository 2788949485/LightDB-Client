namespace DbLiteDesktop.Models;

public class QueryHistoryItem
{
    public int Id { get; set; }
    public int? ConnectionId { get; set; }
    public string DbType { get; set; } = string.Empty;
    public string? DatabaseName { get; set; }
    public string SqlText { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public int RowCount { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
