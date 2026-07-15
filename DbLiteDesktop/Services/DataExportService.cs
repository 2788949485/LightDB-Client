using System.Data;
using System.Text;
using System.Text.Json;

namespace DbLiteDesktop.Services;

public class DataExportService
{
    public enum ExportFormat
    {
        Csv,
        Json
    }

    public void Export(DataTable table, string filePath, ExportFormat format)
    {
        switch (format)
        {
            case ExportFormat.Csv:
                ExportCsv(table, filePath);
                break;
            case ExportFormat.Json:
                ExportJson(table, filePath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    public string GetFileExtension(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Csv => "csv",
            ExportFormat.Json => "json",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    public string GetFilter(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Csv => "CSV 文件|*.csv",
            ExportFormat.Json => "JSON 文件|*.json",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private static void ExportCsv(DataTable table, string filePath)
    {
        var sb = new StringBuilder();

        // UTF-8 BOM 以便 Excel 正确识别
        sb.Append('\uFEFF');

        var headers = table.Columns.Cast<DataColumn>()
            .Select(c => EscapeCsvValue(c.ColumnName))
            .ToArray();
        sb.AppendLine(string.Join(",", headers));

        foreach (DataRow row in table.Rows)
        {
            var values = table.Columns.Cast<DataColumn>()
                .Select(col => EscapeCsvValue(row[col]?.ToString() ?? string.Empty))
                .ToArray();
            sb.AppendLine(string.Join(",", values));
        }

        File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(false));
    }

    private static string EscapeCsvValue(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static void ExportJson(DataTable table, string filePath)
    {
        var rows = new List<Dictionary<string, object?>>();

        foreach (DataRow row in table.Rows)
        {
            var dict = new Dictionary<string, object?>();
            foreach (DataColumn col in table.Columns)
            {
                dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
            }
            rows.Add(dict);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var json = JsonSerializer.Serialize(rows, options);
        File.WriteAllText(filePath, json, new UTF8Encoding(false));
    }
}
