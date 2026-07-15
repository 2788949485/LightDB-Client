using System.Data;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace DbLiteDesktop.Controls;

public class SqlEditorTextBox : RichTextBox
{
    private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT", "FROM", "WHERE", "AND", "OR", "NOT", "NULL", "IS", "IN", "LIKE",
        "BETWEEN", "ORDER", "BY", "GROUP", "HAVING", "LIMIT", "OFFSET", "AS",
        "JOIN", "LEFT", "RIGHT", "INNER", "OUTER", "FULL", "CROSS", "ON", "USING",
        "UNION", "ALL", "DISTINCT", "CASE", "WHEN", "THEN", "ELSE", "END",
        "COUNT", "SUM", "AVG", "MIN", "MAX", "CAST", "CONVERT",
        "SHOW", "TABLES", "DESC", "DESCRIBE", "EXPLAIN", "PRAGMA",
        "WITH", "INSERT", "UPDATE", "DELETE", "CREATE", "ALTER", "DROP",
        "TABLE", "INDEX", "VIEW", "INTO", "VALUES", "SET",
        "ASC", "DESC"
    };

    private static readonly Color KeywordColor = Color.FromArgb(59, 130, 246);
    private static readonly Color StringColor = Color.FromArgb(22, 163, 74);
    private static readonly Color CommentColor = Color.FromArgb(100, 116, 139);
    private static readonly Color NumberColor = Color.FromArgb(217, 119, 6);

    private bool _suppressHighlight;
    private static readonly Regex TokenRegex = new(
        @"('[^']*')|(--[^\r\n]*)|(/\*[\s\S]*?\*/)|(\b\d+\.?\d*\b)|(\w+)",
        RegexOptions.Compiled
    );

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

    private const int EM_SETCUEBANNER = 0x1501;
    private const int EM_GETCUEBANNER = 0x1502;

    public string PlaceholderText
    {
        get => _placeholderText;
        set
        {
            _placeholderText = value;
            if (IsHandleCreated)
            {
                SendMessage(Handle, EM_SETCUEBANNER, 1, value ?? string.Empty);
            }
        }
    }

    private string _placeholderText = string.Empty;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (!string.IsNullOrEmpty(_placeholderText))
        {
            SendMessage(Handle, EM_SETCUEBANNER, 1, _placeholderText);
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);

        if (_suppressHighlight)
        {
            return;
        }

        HighlightSyntax();
    }

    private void HighlightSyntax()
    {
        if (string.IsNullOrEmpty(Text))
        {
            return;
        }

        _suppressHighlight = true;
        try
        {
            var selStart = SelectionStart;
            var selLength = SelectionLength;

            // 重置为默认颜色
            SelectAll();
            SelectionColor = ForeColor;

            foreach (Match match in TokenRegex.Matches(Text))
            {
                var color = ForeColor;

                if (match.Groups[1].Success) // 字符串
                {
                    color = StringColor;
                }
                else if (match.Groups[2].Success || match.Groups[3].Success) // 注释
                {
                    color = CommentColor;
                }
                else if (match.Groups[4].Success) // 数字
                {
                    color = NumberColor;
                }
                else if (match.Groups[5].Success && Keywords.Contains(match.Groups[5].Value)) // 关键字
                {
                    color = KeywordColor;
                }
                else
                {
                    continue;
                }

                Select(match.Index, match.Length);
                SelectionColor = color;
            }

            // 恢复光标
            Select(selStart, selLength);
            SelectionColor = ForeColor;
        }
        finally
        {
            _suppressHighlight = false;
        }
    }

    public void ApplyTheme()
    {
        BorderStyle = BorderStyle.FixedSingle;
        BackColor = Color.White;
        ForeColor = Color.FromArgb(71, 85, 105);
        Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
    }
}
