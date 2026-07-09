using System.Data;
using DbLiteDesktop.Forms;
using DbLiteDesktop.Models;
using DbLiteDesktop.Providers;
using DbLiteDesktop.Services;
using DbLiteDesktop.Utils;

namespace DbLiteDesktop;

public partial class MainForm : Form
{
    private const int PreviewPageSize = 100;
    private const string AllFieldsOption = "全部字段";
    private readonly ConfigService _configService = new();
    private readonly PasswordEncryptService _passwordEncryptService = new();
    private readonly SqlGuardService _sqlGuardService = new();
    private readonly ContextMenuStrip _previewCopyMenu = new();
    private QueryHistoryService _queryHistoryService = null!;
    private DbConnectionConfig? _currentConfig;
    private string? _currentPreviewTableName;
    private int _currentPreviewPage = 1;
    private List<string> _currentPreviewColumns = [];
    private string _previewCopyText = string.Empty;

    public MainForm()
    {
        InitializeComponent();
        InitializeServices();
        ApplyTheme();
        tabMain.DrawItem += tabMain_DrawItem;
        Shown += (_, _) => ApplyDefaultSplitterDistance();
    }

    private void InitializeServices()
    {
        _configService.Initialize();
        _queryHistoryService = new QueryHistoryService(_configService.DatabasePath);
        _queryHistoryService.Initialize();
        InitializePreviewSearch();
        InitializePreviewCopyMenu();
        LoadConnections();
        LoadHistory();
    }

    private void InitializePreviewSearch()
    {
        cboPreviewMatch.Items.Clear();
        cboPreviewMatch.Items.AddRange(["包含", "精确"]);
        cboPreviewMatch.SelectedIndex = 0;

        cboPreviewField.Items.Clear();
        cboPreviewField.Items.Add(AllFieldsOption);
        cboPreviewField.SelectedIndex = 0;
    }

    private void InitializePreviewCopyMenu()
    {
        var copyItem = new ToolStripMenuItem("复制");
        copyItem.Click += (_, _) =>
        {
            if (!string.IsNullOrEmpty(_previewCopyText))
            {
                Clipboard.SetText(_previewCopyText);
            }
        };

        _previewCopyMenu.Items.Clear();
        _previewCopyMenu.Items.Add(copyItem);
        gridPreview.MouseDown += gridPreview_MouseDown;
    }

    private void LoadConnections()
    {
        var selectedId = GetSelectedConnection()?.Id;
        var items = _configService.GetConnections();

        cboConnections.Items.Clear();
        foreach (var item in items)
        {
            cboConnections.Items.Add(item);
        }

        if (selectedId.HasValue)
        {
            SelectConnection(selectedId.Value);
        }
        else if (cboConnections.Items.Count > 0)
        {
            cboConnections.SelectedIndex = 0;
        }
    }

    private void LoadHistory()
    {
        gridHistory.DataSource = null;
        gridHistory.DataSource = _queryHistoryService.GetRecent();
    }

    private DbConnectionConfig? GetSelectedConnection()
    {
        return cboConnections.SelectedItem as DbConnectionConfig;
    }

    private void SelectConnection(int connectionId)
    {
        for (var i = 0; i < cboConnections.Items.Count; i++)
        {
            if (cboConnections.Items[i] is DbConnectionConfig item && item.Id == connectionId)
            {
                cboConnections.SelectedIndex = i;
                return;
            }
        }
    }

    private void OpenConnectionForm(DbConnectionConfig? config = null)
    {
        using var form = new ConnectionForm(config);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _configService.SaveConnection(form.ConnectionConfig);
        LoadConnections();
        SelectConnection(form.ConnectionConfig.Id);

        if (form.ConnectAfterSave)
        {
            ConnectSelected();
        }
    }

    private void TestSelectedConnection()
    {
        var config = GetSelectedConnection();
        if (config is null)
        {
            MessageBox.Show("请先选择连接。", "提示");
            return;
        }

        try
        {
            var provider = DatabaseProviderFactory.Create(config.DbType);
            provider.TestConnection(config, GetPassword(config));
            MessageBox.Show("连接测试成功。", "提示");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "连接测试失败");
        }
    }

    private void ConnectSelected()
    {
        var config = GetSelectedConnection();
        if (config is null)
        {
            MessageBox.Show("请先选择连接。", "提示");
            return;
        }

        try
        {
            var provider = DatabaseProviderFactory.Create(config.DbType);
            provider.TestConnection(config, GetPassword(config));
            _currentConfig = config;
            LoadTables();
            lblStatus.Text = $"已连接：{config.Name}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "连接失败");
        }
    }

    private void LoadTables()
    {
        if (_currentConfig is null)
        {
            return;
        }

        var provider = DatabaseProviderFactory.Create(_currentConfig.DbType);
        var tables = provider.GetTables(_currentConfig, GetPassword(_currentConfig));

        treeTables.Nodes.Clear();
        foreach (var table in tables)
        {
            treeTables.Nodes.Add(table);
        }
    }

    private void LoadColumnsForTable(string tableName)
    {
        if (_currentConfig is null)
        {
            return;
        }

        try
        {
            var provider = DatabaseProviderFactory.Create(_currentConfig.DbType);
            var columns = provider.GetColumns(_currentConfig, GetPassword(_currentConfig), tableName);

            gridColumns.DataSource = null;
            gridColumns.DataSource = columns;
            _currentPreviewColumns = columns.Select(column => column.Name).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
            BindPreviewFields();
            txtSql.Text = provider.BuildPreviewSql(tableName, 100);
            _currentPreviewTableName = tableName;
            _currentPreviewPage = 1;
            txtPreviewKeyword.Clear();
            cboPreviewMatch.SelectedIndex = 0;
            LoadPreviewPage();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "读取字段失败");
        }
    }

    private void BindPreviewFields()
    {
        cboPreviewField.Items.Clear();
        cboPreviewField.Items.Add(AllFieldsOption);

        foreach (var column in _currentPreviewColumns)
        {
            cboPreviewField.Items.Add(column);
        }

        cboPreviewField.SelectedIndex = 0;
    }

    private void LoadPreviewPage()
    {
        if (_currentConfig is null || string.IsNullOrWhiteSpace(_currentPreviewTableName))
        {
            return;
        }

        try
        {
            var provider = DatabaseProviderFactory.Create(_currentConfig.DbType);
            var sql = BuildPreviewSql(provider);
            var result = provider.ExecuteQuery(_currentConfig, GetPassword(_currentConfig), sql, PreviewPageSize);

            gridPreview.DataSource = null;
            gridPreview.DataSource = result;
            lblPreviewPage.Text = $"第 {_currentPreviewPage} 页";
            btnPrevPage.Enabled = _currentPreviewPage > 1;
            btnNextPage.Enabled = result.Rows.Count >= PreviewPageSize;
            tabMain.SelectedTab = tabPreview;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "加载数据预览失败");
        }
    }

    private string BuildPreviewSql(IDatabaseProvider provider)
    {
        if (string.IsNullOrWhiteSpace(_currentPreviewTableName))
        {
            return string.Empty;
        }

        var selectedColumn = cboPreviewField.SelectedItem?.ToString();
        if (string.Equals(selectedColumn, AllFieldsOption, StringComparison.Ordinal))
        {
            selectedColumn = null;
        }

        var parseResult = PreviewSearchInputParser.Parse(
            txtPreviewKeyword.Text,
            _currentPreviewColumns,
            selectedColumn,
            string.Equals(cboPreviewMatch.SelectedItem?.ToString(), "精确", StringComparison.Ordinal)
        );

        if (!parseResult.Success)
        {
            throw new InvalidOperationException(parseResult.ErrorMessage);
        }

        if (string.IsNullOrWhiteSpace(parseResult.Keyword))
        {
            return provider.BuildPagedPreviewSql(_currentPreviewTableName, _currentPreviewPage, PreviewPageSize);
        }

        var exactMatch = parseResult.FieldName is not null
            ? true
            : parseResult.ExactMatch;

        return provider.BuildFilteredPreviewSql(
            _currentPreviewTableName,
            _currentPreviewColumns,
            parseResult.FieldName,
            parseResult.Keyword,
            exactMatch,
            _currentPreviewPage,
            PreviewPageSize
        );
    }

    private void RunSql()
    {
        if (_currentConfig is null)
        {
            MessageBox.Show("请先连接数据库。", "提示");
            return;
        }

        var sql = txtSql.Text.Trim();
        if (!_sqlGuardService.IsReadonlySql(sql))
        {
            MessageBox.Show("当前工具只允许执行只读 SQL。", "提示");
            return;
        }

        var startedAt = DateTime.UtcNow;

        try
        {
            var provider = DatabaseProviderFactory.Create(_currentConfig.DbType);
            var result = provider.ExecuteQuery(_currentConfig, GetPassword(_currentConfig), sql, 1000);
            var duration = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;

            gridResults.DataSource = null;
            gridResults.DataSource = result;
            lblStatus.Text = $"查询成功，返回 {result.Rows.Count} 行，耗时 {duration} ms";

            _queryHistoryService.Add(new QueryHistoryItem
            {
                ConnectionId = _currentConfig.Id,
                DbType = _currentConfig.DbType,
                DatabaseName = GetDisplayDatabaseName(_currentConfig),
                SqlText = sql,
                Success = true,
                DurationMs = duration,
                RowCount = result.Rows.Count
            });

            LoadHistory();
            tabMain.SelectedTab = tabSql;
        }
        catch (Exception ex)
        {
            var duration = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
            lblStatus.Text = $"查询失败：{ex.Message}";

            _queryHistoryService.Add(new QueryHistoryItem
            {
                ConnectionId = _currentConfig.Id,
                DbType = _currentConfig.DbType,
                DatabaseName = GetDisplayDatabaseName(_currentConfig),
                SqlText = sql,
                Success = false,
                ErrorMessage = ex.Message,
                DurationMs = duration,
                RowCount = 0
            });

            LoadHistory();
            MessageBox.Show(ex.Message, "查询失败");
        }
    }

    private string GetPassword(DbConnectionConfig config)
    {
        return string.Equals(config.DbType, "sqlite", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : _passwordEncryptService.Decrypt(config.PasswordEncrypted ?? string.Empty);
    }

    private static string GetDisplayDatabaseName(DbConnectionConfig config)
    {
        return config.DatabaseName ?? config.FilePath ?? config.Name;
    }

    private void Disconnect()
    {
        _currentConfig = null;
        _currentPreviewTableName = null;
        _currentPreviewPage = 1;
        _currentPreviewColumns = [];
        treeTables.Nodes.Clear();
        gridColumns.DataSource = null;
        gridResults.DataSource = null;
        gridPreview.DataSource = null;
        lblPreviewPage.Text = "第 1 页";
        lblStatus.Text = "未连接";
        txtPreviewKeyword.Clear();
        cboPreviewField.Items.Clear();
        cboPreviewField.Items.Add(AllFieldsOption);
        cboPreviewField.SelectedIndex = 0;
    }

    private void ApplyTheme()
    {
        var pageBackColor = Color.FromArgb(238, 246, 245);
        var cardBackColor = Color.White;
        var accentColor = Color.FromArgb(3, 105, 161);
        var borderColor = Color.FromArgb(203, 213, 225);
        var textColor = Color.FromArgb(15, 23, 42);
        var subtleTextColor = Color.FromArgb(71, 85, 105);

        BackColor = pageBackColor;
        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        txtPreviewKeyword.PlaceholderText = "输入值，或直接输入 字段名=数据";
        txtSql.PlaceholderText = "请输入只读 SQL，例如：SELECT * FROM your_table LIMIT 100";

        lblAppTitle.Font = new Font("Microsoft YaHei UI", 12.5F, FontStyle.Bold, GraphicsUnit.Point);
        lblAppTitle.ForeColor = textColor;
        lblAppTitle.Text = "DB Lite Desktop";
        lblAppTitle.AutoSize = true;
        lblAppTitle.Location = new Point(0, 12);
        lblAppSubtitle.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        lblAppSubtitle.ForeColor = subtleTextColor;
        lblAppSubtitle.Text = string.Empty;
        lblAppSubtitle.Visible = false;

        lblTablesTitle.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
        lblTablesTitle.ForeColor = textColor;
        lblTablesSubtitle.ForeColor = subtleTextColor;
        lblTablesSubtitle.Text = "连接成功后显示当前数据库中的表。";

        ApplyPanelChrome(headerPanel, cardBackColor, borderColor);
        ApplyPanelChrome(navigationPanel, cardBackColor, borderColor);
        ApplyPanelChrome(workspacePanel, cardBackColor, borderColor);
        ApplyPanelChrome(headerActionsPanel, Color.Transparent, Color.Transparent);

        lblConnection.ForeColor = subtleTextColor;
        lblConnection.TextAlign = ContentAlignment.MiddleLeft;
        StyleComboBox(cboConnections);
        cboConnections.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        StyleHeaderButton(btnNewConnection, false);
        StyleHeaderButton(btnEditConnection, false);
        StyleHeaderButton(btnDeleteConnection, false);
        StyleHeaderButton(btnTestConnection, false);
        StyleHeaderButton(btnRefresh, false);
        StyleHeaderButton(btnConnect, true, accentColor);
        StyleHeaderButton(btnDisconnect, false);

        splitContainer.BackColor = pageBackColor;

        treeTables.BackColor = cardBackColor;
        treeTables.BorderStyle = BorderStyle.None;
        treeTables.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        treeTables.ForeColor = textColor;
        treeTables.FullRowSelect = true;
        treeTables.HotTracking = false;
        treeTables.Indent = 18;
        treeTables.ItemHeight = 30;
        treeTables.ShowLines = false;
        treeTables.ShowPlusMinus = false;
        treeTables.ShowRootLines = false;

        tabMain.Appearance = TabAppearance.Normal;
        tabMain.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabMain.Padding = new Point(12, 8);
        tabMain.SizeMode = TabSizeMode.Fixed;

        StyleGrid(gridColumns);
        StyleGrid(gridResults);
        StyleGrid(gridHistory);
        StyleGrid(gridPreview);

        StyleActionButton(btnRunSql, accentColor);
        StyleActionButton(btnPrevPage);
        StyleActionButton(btnNextPage);
        StyleActionButton(btnApplyPreviewFilter, accentColor);
        StyleGhostButton(btnClearSql);
        StyleGhostButton(btnCopySql);
        StyleGhostButton(btnResetPreviewFilter);

        lblStatus.BackColor = Color.FromArgb(248, 250, 252);
        lblStatus.ForeColor = subtleTextColor;
        lblStatus.Padding = new Padding(12, 0, 0, 0);
        lblPreviewPage.ForeColor = textColor;
        lblPreviewTip.ForeColor = subtleTextColor;
        lblPreviewTip.Text = "支持 字段名=数据";

        previewSearchPanel.BackColor = Color.FromArgb(248, 250, 252);
        previewSearchPanel.Margin = new Padding(0);
        previewButtonPanel.BackColor = Color.White;
        sqlButtonPanel.BackColor = Color.White;
        sqlLayout.BackColor = cardBackColor;
        previewLayout.BackColor = cardBackColor;
        tabMain.BackColor = cardBackColor;

        StyleComboBox(cboPreviewField);
        StyleComboBox(cboPreviewMatch);
        StyleTextInput(txtPreviewKeyword);
        StyleTextInput(txtSql);
        txtSql.BackColor = Color.FromArgb(248, 250, 252);
        txtSql.ForeColor = textColor;
        txtSql.BorderStyle = BorderStyle.FixedSingle;
        txtSql.Margin = new Padding(0, 0, 0, 10);

        AlignPreviewSearchControls();
    }

    private static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.GridColor = Color.FromArgb(226, 232, 240);
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        grid.ColumnHeadersHeight = 40;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        grid.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
        grid.RowTemplate.Height = 34;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
    }

    private static void StyleActionButton(Button button, Color? accentColor = null)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = accentColor ?? Color.FromArgb(3, 105, 161);
        button.ForeColor = Color.White;
        button.Padding = new Padding(10, 4, 10, 4);
        button.Margin = new Padding(0, 0, 10, 0);
        button.MinimumSize = new Size(0, 34);
        button.Cursor = Cursors.Hand;
    }

    private static void StyleGhostButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);
        button.FlatAppearance.BorderSize = 1;
        button.BackColor = Color.White;
        button.ForeColor = Color.FromArgb(3, 105, 161);
        button.Padding = new Padding(10, 4, 10, 4);
        button.Margin = new Padding(0, 0, 10, 0);
        button.MinimumSize = new Size(0, 34);
        button.Cursor = Cursors.Hand;
    }

    private static void StyleHeaderButton(Button button, bool emphasize, Color? accentColor = null)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = emphasize ? 0 : 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        button.Margin = new Padding(0, 0, 6, 0);
        button.Padding = new Padding(8, 3, 8, 3);
        button.MinimumSize = new Size(0, 34);
        button.ForeColor = emphasize ? Color.White : Color.FromArgb(15, 23, 42);
        button.BackColor = emphasize ? accentColor ?? Color.FromArgb(3, 105, 161) : Color.FromArgb(248, 250, 252);
        button.Cursor = Cursors.Hand;
    }

    private static void ApplyPanelChrome(Panel panel, Color backColor, Color borderColor)
    {
        panel.BackColor = backColor;
        panel.Paint += (_, args) =>
        {
            if (borderColor == Color.Transparent)
            {
                return;
            }

            using var pen = new Pen(borderColor, 1);
            var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            args.Graphics.DrawRectangle(pen, rect);
        };
    }

    private static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.BackColor = Color.White;
        comboBox.ForeColor = Color.FromArgb(30, 41, 59);
        comboBox.IntegralHeight = false;
    }

    private static void StyleTextInput(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = Color.White;
        textBox.ForeColor = Color.FromArgb(15, 23, 42);
    }

    private void AlignPreviewSearchControls()
    {
        previewSearchPanel.Height = 54;

        cboConnections.Height = 34;

        cboPreviewField.Height = 34;
        cboPreviewMatch.Height = 34;
        txtPreviewKeyword.Height = 34;

        btnApplyPreviewFilter.AutoSize = false;
        btnApplyPreviewFilter.Height = 34;

        btnResetPreviewFilter.AutoSize = false;
        btnResetPreviewFilter.Height = 34;
    }

    private void ApplyDefaultSplitterDistance()
    {
        if (splitContainer.Width <= 0)
        {
            return;
        }

        var target = Math.Min(240, Math.Max(220, splitContainer.Width / 4));
        var maxAllowed = splitContainer.Width - splitContainer.Panel2MinSize - splitContainer.SplitterWidth;
        splitContainer.SplitterDistance = Math.Max(splitContainer.Panel1MinSize, Math.Min(target, maxAllowed));
    }

    private void tabMain_DrawItem(object? sender, DrawItemEventArgs e)
    {
        var tabPage = tabMain.TabPages[e.Index];
        var bounds = e.Bounds;
        var selected = e.Index == tabMain.SelectedIndex;
        var backgroundColor = Color.White;
        var textColor = selected ? Color.FromArgb(3, 105, 161) : Color.FromArgb(51, 65, 85);

        using var background = new SolidBrush(backgroundColor);
        using var linePen = new Pen(Color.FromArgb(226, 232, 240), 1);
        using var accentPen = new Pen(Color.FromArgb(3, 105, 161), 3);
        e.Graphics.FillRectangle(background, bounds);
        e.Graphics.DrawLine(linePen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);

        if (selected)
        {
            e.Graphics.DrawLine(accentPen, bounds.Left + 10, bounds.Bottom - 2, bounds.Right - 10, bounds.Bottom - 2);
        }

        TextRenderer.DrawText(
            e.Graphics,
            tabPage.Text,
            new Font("Microsoft YaHei UI", 9F, selected ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point),
            bounds,
            textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
        );
    }

    private void btnNewConnection_Click(object? sender, EventArgs e)
    {
        OpenConnectionForm();
    }

    private void btnEditConnection_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedConnection();
        if (selected is null)
        {
            MessageBox.Show("请先选择连接。", "提示");
            return;
        }

        OpenConnectionForm(selected);
    }

    private void btnDeleteConnection_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedConnection();
        if (selected is null)
        {
            MessageBox.Show("请先选择连接。", "提示");
            return;
        }

        var result = MessageBox.Show(
            $"确定删除连接“{selected.Name}”吗？",
            "确认删除",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (result != DialogResult.Yes)
        {
            return;
        }

        _configService.DeleteConnection(selected.Id);
        if (_currentConfig?.Id == selected.Id)
        {
            Disconnect();
        }

        LoadConnections();
    }

    private void btnTestConnection_Click(object? sender, EventArgs e)
    {
        TestSelectedConnection();
    }

    private void btnConnect_Click(object? sender, EventArgs e)
    {
        ConnectSelected();
    }

    private void btnRefresh_Click(object? sender, EventArgs e)
    {
        LoadConnections();
        LoadHistory();

        if (_currentConfig is not null)
        {
            SelectConnection(_currentConfig.Id);
            LoadTables();
        }
    }

    private void btnDisconnect_Click(object? sender, EventArgs e)
    {
        Disconnect();
    }

    private void treeTables_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node is not null)
        {
            LoadColumnsForTable(e.Node.Text);
        }
    }

    private void btnRunSql_Click(object? sender, EventArgs e)
    {
        RunSql();
    }

    private void btnClearSql_Click(object? sender, EventArgs e)
    {
        txtSql.Clear();
    }

    private void btnCopySql_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtSql.Text))
        {
            Clipboard.SetText(txtSql.Text);
        }
    }

    private void btnPrevPage_Click(object? sender, EventArgs e)
    {
        if (_currentPreviewPage <= 1)
        {
            return;
        }

        _currentPreviewPage--;
        LoadPreviewPage();
    }

    private void btnNextPage_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentPreviewTableName))
        {
            return;
        }

        _currentPreviewPage++;
        LoadPreviewPage();
    }

    private void btnApplyPreviewFilter_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentPreviewTableName))
        {
            MessageBox.Show("请先选择表。", "提示");
            return;
        }

        _currentPreviewPage = 1;
        LoadPreviewPage();
    }

    private void btnResetPreviewFilter_Click(object? sender, EventArgs e)
    {
        txtPreviewKeyword.Clear();
        cboPreviewField.SelectedIndex = 0;
        cboPreviewMatch.SelectedIndex = 0;
        _currentPreviewPage = 1;
        LoadPreviewPage();
    }

    private void gridHistory_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (gridHistory.Rows[e.RowIndex].DataBoundItem is QueryHistoryItem item)
        {
            txtSql.Text = item.SqlText;
            tabMain.SelectedTab = tabSql;
        }
    }

    private void gridPreview_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        var hit = gridPreview.HitTest(e.X, e.Y);

        if (hit.Type == DataGridViewHitTestType.ColumnHeader && hit.ColumnIndex >= 0)
        {
            _previewCopyText = gridPreview.Columns[hit.ColumnIndex].HeaderText;
            _previewCopyMenu.Show(gridPreview, new Point(e.X, e.Y));
            return;
        }

        if (hit.Type == DataGridViewHitTestType.Cell && hit.RowIndex >= 0 && hit.ColumnIndex >= 0)
        {
            var value = gridPreview.Rows[hit.RowIndex].Cells[hit.ColumnIndex].Value;
            _previewCopyText = value?.ToString() ?? string.Empty;
            _previewCopyMenu.Show(gridPreview, new Point(e.X, e.Y));
        }
    }

}
