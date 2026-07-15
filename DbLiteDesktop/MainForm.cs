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
    // SqlGuardService 改为静态类，无需实例化
    private readonly DataExportService _dataExportService = new();
    private readonly ContextMenuStrip _previewCopyMenu = new();
    private QueryHistoryService _queryHistoryService = null!;
    private DbConnectionConfig? _currentConfig;
    private IDatabaseProvider? _currentProvider;
    private string? _currentPreviewTableName;
    private int _currentPreviewPage = 1;
    private List<string> _currentPreviewColumns = [];
    private string _previewCopyText = string.Empty;
    private bool _isLoading;

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
            _currentProvider = provider;
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
        if (_currentConfig is null || _currentProvider is null)
        {
            return;
        }

        SetLoading(true);
        Task.Run(() =>
        {
            try
            {
                var tables = _currentProvider.GetTables(_currentConfig, GetPassword(_currentConfig));
                Invoke(() =>
                {
                    treeTables.Nodes.Clear();
                    foreach (var table in tables)
                    {
                        treeTables.Nodes.Add(table);
                    }
                    SetLoading(false);
                });
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    SetLoading(false);
                    MessageBox.Show(ex.Message, "读取表列表失败");
                });
            }
        });
    }

    private void SetLoading(bool loading)
    {
        _isLoading = loading;
        lblStatus.Text = loading ? "加载中..." : lblStatus.Text;
        Cursor = loading ? Cursors.WaitCursor : Cursors.Default;
        UseWaitCursor = loading;
    }

    private void LoadColumnsForTable(string tableName)
    {
        if (_currentConfig is null || _currentProvider is null)
        {
            return;
        }

        SetLoading(true);
        Task.Run(() =>
        {
            try
            {
                var columns = _currentProvider.GetColumns(_currentConfig, GetPassword(_currentConfig), tableName);
                Invoke(() =>
                {
                    gridColumns.DataSource = null;
                    gridColumns.DataSource = columns;
                    _currentPreviewColumns = columns.Select(column => column.Name).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
                    BindPreviewFields();
                    txtSql.Text = _currentProvider.BuildPreviewSql(tableName, 100);
                    _currentPreviewTableName = tableName;
                    _currentPreviewPage = 1;
                    txtPreviewKeyword.Clear();
                    cboPreviewMatch.SelectedIndex = 0;
                    LoadPreviewPage();
                    SetLoading(false);
                });
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    SetLoading(false);
                    MessageBox.Show(ex.Message, "读取字段失败");
                });
            }
        });
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
        if (_currentConfig is null || _currentProvider is null || string.IsNullOrWhiteSpace(_currentPreviewTableName))
        {
            return;
        }

        SetLoading(true);
        Task.Run(() =>
        {
            try
            {
                var sql = string.Empty;
                Invoke(() => { sql = BuildPreviewSql(_currentProvider); });

                var result = _currentProvider.ExecuteQuery(_currentConfig, GetPassword(_currentConfig), sql, PreviewPageSize);

                Invoke(() =>
                {
                    gridPreview.SuspendLayout();
                    try
                    {
                        gridPreview.DataSource = null;
                        gridPreview.DataSource = result;
                        gridPreview.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    }
                    finally
                    {
                        gridPreview.ResumeLayout();
                    }

                    lblPreviewPage.Text = $"第 {_currentPreviewPage} 页";
                    btnPrevPage.Enabled = _currentPreviewPage > 1;
                    btnNextPage.Enabled = result.Rows.Count >= PreviewPageSize;
                    tabMain.SelectedTab = tabPreview;
                    SetLoading(false);
                });
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    SetLoading(false);
                    MessageBox.Show(ex.Message, "加载数据预览失败");
                });
            }
        });
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
        if (_currentConfig is null || _currentProvider is null)
        {
            MessageBox.Show("请先连接数据库。", "提示");
            return;
        }

        var sql = txtSql.Text.Trim();
        if (!SqlGuardService.IsReadonlySql(sql))
        {
            MessageBox.Show("当前工具只允许执行只读 SQL。", "提示");
            return;
        }

        SetLoading(true);
        var startedAt = DateTime.UtcNow;
        var config = _currentConfig;
        var provider = _currentProvider;
        var password = GetPassword(config);

        Task.Run(() =>
        {
            try
            {
                var result = provider.ExecuteQuery(config, password, sql, 1000);
                var duration = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;

                Invoke(() =>
                {
                    gridResults.SuspendLayout();
                    try
                    {
                        gridResults.DataSource = null;
                        gridResults.DataSource = result;
                        gridResults.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    }
                    finally
                    {
                        gridResults.ResumeLayout();
                    }

                    lblStatus.Text = $"查询成功，返回 {result.Rows.Count} 行，耗时 {duration} ms";

                    _queryHistoryService.Add(new QueryHistoryItem
                    {
                        ConnectionId = config.Id,
                        DbType = config.DbType,
                        DatabaseName = GetDisplayDatabaseName(config),
                        SqlText = sql,
                        Success = true,
                        DurationMs = duration,
                        RowCount = result.Rows.Count
                    });

                    LoadHistory();
                    tabMain.SelectedTab = tabSql;
                    SetLoading(false);
                });
            }
            catch (Exception ex)
            {
                var duration = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
                Invoke(() =>
                {
                    lblStatus.Text = $"查询失败：{ex.Message}";

                    _queryHistoryService.Add(new QueryHistoryItem
                    {
                        ConnectionId = config.Id,
                        DbType = config.DbType,
                        DatabaseName = GetDisplayDatabaseName(config),
                        SqlText = sql,
                        Success = false,
                        ErrorMessage = ex.Message,
                        DurationMs = duration,
                        RowCount = 0
                    });

                    LoadHistory();
                    SetLoading(false);
                    MessageBox.Show(ex.Message, "查询失败");
                });
            }
        });
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
        _currentProvider = null;
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

    private bool _themeApplied;

    private void ApplyTheme()
    {
        if (_themeApplied)
        {
            return;
        }

        _themeApplied = true;
        var pageBackColor = Color.FromArgb(245, 247, 250);
        var cardBackColor = Color.White;
        var chromeBackColor = Color.FromArgb(251, 252, 253);
        var accentColor = Color.FromArgb(59, 130, 246);
        var accentHoverColor = Color.FromArgb(37, 99, 235);
        var borderColor = Color.FromArgb(226, 232, 240);
        var textColor = Color.FromArgb(15, 23, 42);
        var subtleTextColor = Color.FromArgb(100, 116, 139);

        BackColor = pageBackColor;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        txtPreviewKeyword.PlaceholderText = "输入值，或直接输入 字段名=数据";
        txtSql.PlaceholderText = "请输入只读 SQL，例如：SELECT * FROM your_table LIMIT 100";

        lblAppTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point);
        lblAppTitle.ForeColor = accentColor;
        lblAppTitle.Text = "🗄️ DB Lite Desktop";
        lblAppTitle.AutoSize = false;
        lblAppTitle.Dock = DockStyle.Fill;
        lblAppTitle.TextAlign = ContentAlignment.MiddleLeft;
        lblAppSubtitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        lblAppSubtitle.ForeColor = subtleTextColor;
        // lblAppSubtitle.Text = "✨ 轻量级只读数据库客户端";
        lblAppSubtitle.Visible = true;
        lblAppSubtitle.AutoSize = true;
        lblAppSubtitle.Location = new Point(0, 38);
        lblAppSubtitle.Margin = new Padding(0);

        lblTablesTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
        lblTablesTitle.ForeColor = textColor;
        lblTablesTitle.Text = "📊 数据表导航";
        lblTablesSubtitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        lblTablesSubtitle.ForeColor = subtleTextColor;
        lblTablesSubtitle.Text = "连接成功后显示当前数据库中的表";

        ApplyPanelChrome(headerPanel, cardBackColor, borderColor);
        ApplyPanelChrome(navigationPanel, cardBackColor, borderColor);
        ApplyPanelChrome(workspacePanel, cardBackColor, borderColor);
        ApplyPanelChrome(headerActionsPanel, cardBackColor, Color.Transparent);
        ApplyPanelChrome(previewSearchPanel, chromeBackColor, borderColor);
        ApplyPanelChrome(previewButtonPanel, chromeBackColor, borderColor);
        ApplyPanelChrome(sqlButtonPanel, chromeBackColor, borderColor);

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

        treeTables.BackColor = chromeBackColor;
        treeTables.BorderStyle = BorderStyle.None;
        treeTables.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        treeTables.ForeColor = textColor;
        treeTables.FullRowSelect = true;
        treeTables.HotTracking = true;
        treeTables.Indent = 20;
        treeTables.ItemHeight = 32;
        treeTables.ShowLines = false;
        treeTables.ShowPlusMinus = true;
        treeTables.ShowRootLines = false;
        treeTables.HideSelection = false;
        treeTables.DrawMode = TreeViewDrawMode.OwnerDrawText;
        treeTables.DrawNode += (sender, e) => {
            if (e.Node == null) return;
            
            var bounds = e.Bounds;
            var isSelected = (e.State & TreeNodeStates.Selected) != 0;
            var isHot = (e.State & TreeNodeStates.Hot) != 0;
            
            if (isSelected || isHot) {
                using var brush = new SolidBrush(
                    isSelected ? Color.FromArgb(219, 234, 254) : Color.FromArgb(241, 245, 249)
                );
                e.Graphics.FillRectangle(brush, new Rectangle(0, bounds.Top, treeTables.Width, bounds.Height));
                
                if (isSelected) {
                    using var pen = new Pen(Color.FromArgb(59, 130, 246), 2);
                    e.Graphics.DrawLine(pen, 0, bounds.Bottom - 1, treeTables.Width, bounds.Bottom - 1);
                }
            }
            
            using var boldFont = isSelected || isHot
                ? new Font(treeTables.Font, FontStyle.Bold)
                : null;

            TextRenderer.DrawText(
                e.Graphics,
                e.Node.Text,
                boldFont ?? treeTables.Font,
                new Rectangle(bounds.X + 4, bounds.Y, bounds.Width, bounds.Height),
                isSelected ? Color.FromArgb(15, 23, 42) : Color.FromArgb(71, 85, 105),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter
            );
        };

        tabMain.Appearance = TabAppearance.Normal;
        tabMain.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabMain.Padding = new Point(16, 10);
        tabMain.SizeMode = TabSizeMode.Fixed;
        tabMain.ItemSize = new Size(120, 36);

        StyleGrid(gridColumns);
        StyleGrid(gridResults);
        StyleGrid(gridHistory);
        StyleGrid(gridPreview);

        StyleActionButton(btnRunSql, accentColor);
        StyleGhostButton(btnPrevPage);
        StyleGhostButton(btnNextPage);
        StyleActionButton(btnApplyPreviewFilter, accentColor);
        StyleGhostButton(btnClearSql);
        StyleGhostButton(btnCopySql);
        StyleGhostButton(btnExportResults);
        StyleGhostButton(btnResetPreviewFilter);
        StyleGhostButton(btnRowCount);
        StyleGhostButton(btnExportPreview);

        lblStatus.BackColor = chromeBackColor;
        lblStatus.ForeColor = subtleTextColor;
        lblStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        lblStatus.Padding = new Padding(16, 0, 0, 0);
        lblStatus.BorderStyle = BorderStyle.None;
        lblPreviewPage.ForeColor = textColor;
        lblPreviewPage.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold, GraphicsUnit.Point);
        lblPreviewTip.ForeColor = subtleTextColor;
        lblPreviewTip.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        lblPreviewTip.Text = "💡 支持 字段名=数据 快捷搜索";

        previewSearchPanel.BackColor = chromeBackColor;
        previewSearchPanel.Margin = new Padding(0);
        previewButtonPanel.BackColor = chromeBackColor;
        sqlButtonPanel.BackColor = chromeBackColor;
        sqlLayout.BackColor = cardBackColor;
        previewLayout.BackColor = cardBackColor;
        tabMain.BackColor = cardBackColor;
        headerPanel.BackColor = cardBackColor;
        navigationPanel.BackColor = cardBackColor;
        workspacePanel.BackColor = cardBackColor;

        lblPreviewField.ForeColor = subtleTextColor;
        lblPreviewMatch.ForeColor = subtleTextColor;
        lblPreviewKeyword.ForeColor = subtleTextColor;
        lblPreviewField.TextAlign = ContentAlignment.MiddleLeft;
        lblPreviewMatch.TextAlign = ContentAlignment.MiddleLeft;
        lblPreviewKeyword.TextAlign = ContentAlignment.MiddleLeft;
        lblPreviewField.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point);
        lblPreviewMatch.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point);
        lblPreviewKeyword.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point);

        StyleComboBox(cboPreviewField);
        StyleComboBox(cboPreviewMatch);
        StyleTextInput(txtPreviewKeyword);
        txtSql.ApplyTheme();
        txtSql.PlaceholderText = "请输入只读 SQL，例如：SELECT * FROM your_table LIMIT 100";

        AlignPreviewSearchControls();
    }

    private void StyleGrid(DataGridView grid)
    {
        // 性能优化：启用双缓冲
        typeof(DataGridView).InvokeMember(
            "DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
            null,
            grid,
            new object[] { true }
        );

        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        
        // 性能优化：减少不必要的重绘
        grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        
        grid.GridColor = Color.FromArgb(241, 245, 249);
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 12, 8, 12);
        grid.ColumnHeadersHeight = 44;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        grid.DefaultCellStyle.Padding = new Padding(8, 8, 8, 8);
        grid.RowTemplate.Height = 36;
        grid.RowTemplate.Resizable = DataGridViewTriState.False;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        grid.ScrollBars = ScrollBars.Both;
        grid.AllowUserToResizeRows = false;
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        
        // 性能优化：设置列宽模式以提升滚动性能
        if (ReferenceEquals(grid, gridPreview) || ReferenceEquals(grid, gridResults))
        {
            // 加载时按内容计算一次列宽，滚动时保持固定。
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        }
        else
        {
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }

    private static void StyleActionButton(Button button, Color? accentColor = null)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = accentColor ?? Color.FromArgb(59, 130, 246);
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        button.Padding = new Padding(16, 8, 16, 8);
        button.Margin = new Padding(0, 0, 10, 0);
        button.MinimumSize = new Size(0, 36);
        button.Cursor = Cursors.Hand;
        
        button.MouseEnter += (_, _) => button.BackColor = accentColor ?? Color.FromArgb(37, 99, 235);
        button.MouseLeave += (_, _) => button.BackColor = accentColor ?? Color.FromArgb(59, 130, 246);
    }

    private static void StyleGhostButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        button.FlatAppearance.BorderSize = 1;
        button.BackColor = Color.White;
        button.ForeColor = Color.FromArgb(71, 85, 105);
        button.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        button.Padding = new Padding(14, 7, 14, 7);
        button.Margin = new Padding(0, 0, 10, 0);
        button.MinimumSize = new Size(0, 35);
        button.Cursor = Cursors.Hand;
        
        button.MouseEnter += (_, _) => {
            button.BackColor = Color.FromArgb(248, 250, 252);
            button.FlatAppearance.BorderColor = Color.FromArgb(148, 163, 184);
        };
        button.MouseLeave += (_, _) => {
            button.BackColor = Color.White;
            button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        };
    }

    private static void StyleHeaderButton(Button button, bool emphasize, Color? accentColor = null)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = emphasize ? 0 : 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        button.Margin = new Padding(3, 6, 3, 6);
        button.Padding = new Padding(12, 6, 12, 6);
        button.MinimumSize = new Size(0, 36);
        button.Font = new Font("Segoe UI", 8.75F, emphasize ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point);
        button.ForeColor = emphasize ? Color.White : Color.FromArgb(71, 85, 105);
        button.BackColor = emphasize ? accentColor ?? Color.FromArgb(59, 130, 246) : Color.White;
        button.Cursor = Cursors.Hand;
        
        if (emphasize) {
            button.MouseEnter += (_, _) => button.BackColor = accentColor ?? Color.FromArgb(37, 99, 235);
            button.MouseLeave += (_, _) => button.BackColor = accentColor ?? Color.FromArgb(59, 130, 246);
        } else {
            button.MouseEnter += (_, _) => {
                button.BackColor = Color.FromArgb(248, 250, 252);
                button.ForeColor = Color.FromArgb(15, 23, 42);
            };
            button.MouseLeave += (_, _) => {
                button.BackColor = Color.White;
                button.ForeColor = Color.FromArgb(71, 85, 105);
            };
        }
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
        comboBox.ForeColor = Color.FromArgb(71, 85, 105);
        comboBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        comboBox.IntegralHeight = false;
    }

    private static void StyleTextInput(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = Color.White;
        textBox.ForeColor = Color.FromArgb(71, 85, 105);
        textBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
    }

    private void AlignPreviewSearchControls()
    {
        cboConnections.Height = 36;

        cboPreviewField.Height = 36;
        cboPreviewMatch.Height = 36;
        txtPreviewKeyword.Height = 36;

        btnApplyPreviewFilter.AutoSize = false;
        btnApplyPreviewFilter.Height = 36;

        btnResetPreviewFilter.AutoSize = false;
        btnResetPreviewFilter.Height = 36;
    }

    private void ApplyDefaultSplitterDistance()
    {
        if (splitContainer.Width <= 0)
        {
            return;
        }

        splitContainer.Panel1MinSize = 220;
        splitContainer.Panel2MinSize = Math.Min(760, Math.Max(560, splitContainer.Width / 2));

        var target = Math.Min(240, Math.Max(220, splitContainer.Width / 4));
        var maxAllowed = splitContainer.Width - splitContainer.Panel2MinSize - splitContainer.SplitterWidth;
        if (maxAllowed < splitContainer.Panel1MinSize)
        {
            splitContainer.Panel2MinSize = Math.Max(420, splitContainer.Width - splitContainer.Panel1MinSize - splitContainer.SplitterWidth);
            maxAllowed = splitContainer.Width - splitContainer.Panel2MinSize - splitContainer.SplitterWidth;
        }

        splitContainer.SplitterDistance = Math.Max(splitContainer.Panel1MinSize, Math.Min(target, maxAllowed));
    }

    private void tabMain_DrawItem(object? sender, DrawItemEventArgs e)
    {
        var tabPage = tabMain.TabPages[e.Index];
        var bounds = e.Bounds;
        var selected = e.Index == tabMain.SelectedIndex;
        var backgroundColor = Color.White;
        var textColor = selected ? Color.FromArgb(59, 130, 246) : Color.FromArgb(100, 116, 139);
        var hot = e.State == DrawItemState.HotLight;

        using var background = new SolidBrush(backgroundColor);
        using var linePen = new Pen(Color.FromArgb(226, 232, 240), 1);
        using var accentPen = new Pen(Color.FromArgb(59, 130, 246), 2);
        using var hotBrush = new SolidBrush(Color.FromArgb(248, 250, 252));
        
        e.Graphics.FillRectangle(background, bounds);
        
        if (hot && !selected) {
            e.Graphics.FillRectangle(hotBrush, bounds);
        }
        
        if (selected) {
            var accentBounds = new Rectangle(bounds.Left + 16, bounds.Bottom - 3, bounds.Width - 32, 2);
            e.Graphics.FillRectangle(accentPen.Brush, accentBounds);
        }

        e.Graphics.DrawLine(linePen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);

        using var selectedFont = selected ? new Font(tabMain.Font, FontStyle.Bold) : null;
        TextRenderer.DrawText(
            e.Graphics,
            tabPage.Text,
            selectedFont ?? tabMain.Font,
            bounds,
            hot && !selected ? Color.FromArgb(15, 23, 42) : textColor,
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

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // Ctrl+Enter: 执行 SQL
        if (keyData == (Keys.Control | Keys.Enter))
        {
            RunSql();
            return true;
        }

        // F5: 刷新表列表
        if (keyData == Keys.F5)
        {
            if (_currentConfig is not null)
            {
                LoadTables();
            }
            else
            {
                LoadConnections();
                LoadHistory();
            }
            return true;
        }

        // Escape: 清空搜索框
        if (keyData == Keys.Escape)
        {
            if (tabMain.SelectedTab == tabPreview)
            {
                txtPreviewKeyword.Clear();
                cboPreviewField.SelectedIndex = 0;
                cboPreviewMatch.SelectedIndex = 0;
            }
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
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

    private void btnExportResults_Click(object? sender, EventArgs e)
    {
        ExportGridData(gridResults, "查询结果");
    }

    private void GridResults_SortCompare(object? sender, DataGridViewSortCompareEventArgs e)
    {
        if (decimal.TryParse(e.CellValue1?.ToString(), out var n1) &&
            decimal.TryParse(e.CellValue2?.ToString(), out var n2))
        {
            e.SortResult = n1.CompareTo(n2);
        }
        else
        {
            e.SortResult = string.Compare(
                e.CellValue1?.ToString(),
                e.CellValue2?.ToString(),
                StringComparison.Ordinal
            );
        }
        e.Handled = true;
    }

    private void btnExportPreview_Click(object? sender, EventArgs e)
    {
        ExportGridData(gridPreview, "数据预览");
    }

    private void ExportGridData(DataGridView grid, string defaultFileName)
    {
        if (grid.DataSource is not DataTable table || table.Rows.Count == 0)
        {
            MessageBox.Show("没有可导出的数据。", "提示");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}",
            Filter = "CSV 文件|*.csv|JSON 文件|*.json",
            DefaultExt = "csv"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var format = dialog.FilterIndex == 2
                ? DataExportService.ExportFormat.Json
                : DataExportService.ExportFormat.Csv;

            _dataExportService.Export(table, dialog.FileName, format);
            MessageBox.Show($"导出成功：{dialog.FileName}", "提示");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "导出失败");
        }
    }

    private void btnRowCount_Click(object? sender, EventArgs e)
    {
        if (_currentConfig is null || _currentProvider is null || string.IsNullOrWhiteSpace(_currentPreviewTableName))
        {
            MessageBox.Show("请先选择表。", "提示");
            return;
        }

        var tableName = _currentPreviewTableName;
        SetLoading(true);

        Task.Run(() =>
        {
            try
            {
                var count = _currentProvider.GetRowCount(_currentConfig, GetPassword(_currentConfig), tableName);
                Invoke(() =>
                {
                    SetLoading(false);
                    lblStatus.Text = $"表 {tableName} 共 {count:N0} 行";
                });
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    SetLoading(false);
                    MessageBox.Show(ex.Message, "行数统计失败");
                });
            }
        });
    }
}
