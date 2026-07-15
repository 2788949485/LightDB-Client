namespace DbLiteDesktop.Models;

public class DbConnectionConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DbType { get; set; } = string.Empty;
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? DatabaseName { get; set; }
    public string? Username { get; set; }
    public string? PasswordEncrypted { get; set; }
    public string? FilePath { get; set; }
    public int? ConnectionTimeoutSec { get; set; }
    public int? CommandTimeoutSec { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;

    public override string ToString()
    {
        return Name;
    }
}
