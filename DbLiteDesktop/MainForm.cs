using System.Data;
using DbLiteDesktop.Forms;
using DbLiteDesktop.Models;
using DbLiteDesktop.Providers;
using DbLiteDesktop.Services;

namespace DbLiteDesktop;

public partial class MainForm : Form
{
    private const int PreviewPageSize = 100;
    private readonly ConfigService _configService = new();
    private readonly PasswordEncryptService _passwordEncryptService = new();
    private readonly SqlGuardService _sqlGuardService = new();
    private QueryHistoryService _queryHistoryService = null!;
    private DbConnectionConfig? _currentConfig;
    private string? _currentPreviewTableName;
    private int _currentPreviewPage = 1;

    public MainForm()
    {
        InitializeComponent();
        InitializeServices();
        ApplyTheme();
    }

    private void InitializeServices()
    {
        _configService.Initialize();
        _queryHistoryService = new QueryHistoryService(_configService.DatabasePath);
        _queryHistoryService.Initialize();
        LoadConnections();
        LoadHistory();
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
            txtSql.Text = provider.BuildPreviewSql(tableName, 100);
            _currentPreviewTableName = tableName;
            _currentPreviewPage = 1;
            LoadPreviewPage();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "读取字段失败");
        }
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
            var sql = provider.BuildPagedPreviewSql(_currentPreviewTableName, _currentPreviewPage, PreviewPageSize);
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
        treeTables.Nodes.Clear();
        gridColumns.DataSource = null;
        gridResults.DataSource = null;
        gridPreview.DataSource = null;
        lblPreviewPage.Text = "第 1 页";
        lblStatus.Text = "未连接";
    }

    private void ApplyTheme()
    {
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        toolStrip.BackColor = Color.White;
        toolStrip.GripStyle = ToolStripGripStyle.Hidden;
        toolStrip.Padding = new Padding(8, 4, 8, 4);
        toolStrip.RenderMode = ToolStripRenderMode.System;

        splitContainer.BackColor = Color.FromArgb(226, 232, 240);
        treeTables.BackColor = Color.White;
        treeTables.BorderStyle = BorderStyle.None;

        tabMain.Appearance = TabAppearance.Normal;

        StyleGrid(gridColumns);
        StyleGrid(gridResults);
        StyleGrid(gridHistory);
        StyleGrid(gridPreview);

        StyleActionButton(btnRunSql);
        StyleActionButton(btnPrevPage);
        StyleActionButton(btnNextPage);
        StyleGhostButton(btnClearSql);
        StyleGhostButton(btnCopySql);

        lblStatus.BackColor = Color.White;
        lblStatus.ForeColor = Color.FromArgb(71, 85, 105);
        lblPreviewTip.ForeColor = Color.FromArgb(71, 85, 105);
    }

    private static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
    }

    private static void StyleActionButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Color.FromArgb(37, 99, 235);
        button.ForeColor = Color.White;
        button.Padding = new Padding(10, 4, 10, 4);
        button.Margin = new Padding(0, 8, 10, 0);
    }

    private static void StyleGhostButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);
        button.FlatAppearance.BorderSize = 1;
        button.BackColor = Color.White;
        button.ForeColor = Color.FromArgb(30, 64, 175);
        button.Padding = new Padding(10, 4, 10, 4);
        button.Margin = new Padding(0, 8, 10, 0);
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
}
