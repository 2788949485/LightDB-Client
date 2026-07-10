namespace DbLiteDesktop;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private TableLayoutPanel mainLayout = null!;
    private Panel headerPanel = null!;
    private TableLayoutPanel headerLayout = null!;
    private Panel headerInfoPanel = null!;
    private Label lblAppTitle = null!;
    private Label lblAppSubtitle = null!;
    private Panel headerActionsPanel = null!;
    private TableLayoutPanel headerActionsLayout = null!;
    private Label lblConnection = null!;
    private ComboBox cboConnections = null!;
    private Button btnNewConnection = null!;
    private Button btnEditConnection = null!;
    private Button btnDeleteConnection = null!;
    private Button btnTestConnection = null!;
    private Button btnConnect = null!;
    private Button btnRefresh = null!;
    private Button btnDisconnect = null!;
    private SplitContainer splitContainer = null!;
    private Panel navigationPanel = null!;
    private TableLayoutPanel navigationLayout = null!;
    private Label lblTablesTitle = null!;
    private Label lblTablesSubtitle = null!;
    private TreeView treeTables = null!;
    private Panel workspacePanel = null!;
    private TabControl tabMain = null!;
    private TabPage tabColumns = null!;
    private TabPage tabPreview = null!;
    private TabPage tabSql = null!;
    private TabPage tabHistory = null!;
    private DataGridView gridColumns = null!;
    private DataGridView gridPreview = null!;
    private DataGridView gridResults = null!;
    private DataGridView gridHistory = null!;
    private TableLayoutPanel previewLayout = null!;
    private TableLayoutPanel previewSearchPanel = null!;
    private Label lblPreviewField = null!;
    private ComboBox cboPreviewField = null!;
    private Label lblPreviewMatch = null!;
    private ComboBox cboPreviewMatch = null!;
    private Label lblPreviewKeyword = null!;
    private TextBox txtPreviewKeyword = null!;
    private Button btnApplyPreviewFilter = null!;
    private Button btnResetPreviewFilter = null!;
    private FlowLayoutPanel previewButtonPanel = null!;
    private Button btnPrevPage = null!;
    private Button btnNextPage = null!;
    private Label lblPreviewPage = null!;
    private Label lblPreviewTip = null!;
    private TableLayoutPanel sqlLayout = null!;
    private TextBox txtSql = null!;
    private FlowLayoutPanel sqlButtonPanel = null!;
    private Button btnRunSql = null!;
    private Button btnClearSql = null!;
    private Button btnCopySql = null!;
    private Label lblStatus = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        mainLayout = new TableLayoutPanel();
        headerPanel = new Panel();
        headerLayout = new TableLayoutPanel();
        headerInfoPanel = new Panel();
        lblAppTitle = new Label();
        lblAppSubtitle = new Label();
        headerActionsPanel = new Panel();
        headerActionsLayout = new TableLayoutPanel();
        lblConnection = new Label();
        cboConnections = new ComboBox();
        btnNewConnection = new Button();
        btnEditConnection = new Button();
        btnDeleteConnection = new Button();
        btnTestConnection = new Button();
        btnConnect = new Button();
        btnRefresh = new Button();
        btnDisconnect = new Button();
        splitContainer = new SplitContainer();
        navigationPanel = new Panel();
        navigationLayout = new TableLayoutPanel();
        lblTablesTitle = new Label();
        lblTablesSubtitle = new Label();
        treeTables = new TreeView();
        workspacePanel = new Panel();
        tabMain = new TabControl();
        tabColumns = new TabPage();
        gridColumns = new DataGridView();
        tabPreview = new TabPage();
        previewLayout = new TableLayoutPanel();
        previewSearchPanel = new TableLayoutPanel();
        lblPreviewField = new Label();
        cboPreviewField = new ComboBox();
        lblPreviewMatch = new Label();
        cboPreviewMatch = new ComboBox();
        lblPreviewKeyword = new Label();
        txtPreviewKeyword = new TextBox();
        btnApplyPreviewFilter = new Button();
        btnResetPreviewFilter = new Button();
        gridPreview = new DataGridView();
        previewButtonPanel = new FlowLayoutPanel();
        btnPrevPage = new Button();
        btnNextPage = new Button();
        lblPreviewPage = new Label();
        lblPreviewTip = new Label();
        tabSql = new TabPage();
        sqlLayout = new TableLayoutPanel();
        txtSql = new TextBox();
        sqlButtonPanel = new FlowLayoutPanel();
        btnRunSql = new Button();
        btnClearSql = new Button();
        btnCopySql = new Button();
        gridResults = new DataGridView();
        lblStatus = new Label();
        tabHistory = new TabPage();
        gridHistory = new DataGridView();
        mainLayout.SuspendLayout();
        headerPanel.SuspendLayout();
        headerLayout.SuspendLayout();
        headerInfoPanel.SuspendLayout();
        headerActionsPanel.SuspendLayout();
        headerActionsLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        navigationPanel.SuspendLayout();
        navigationLayout.SuspendLayout();
        workspacePanel.SuspendLayout();
        tabMain.SuspendLayout();
        tabColumns.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridColumns).BeginInit();
        tabPreview.SuspendLayout();
        previewLayout.SuspendLayout();
        previewSearchPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridPreview).BeginInit();
        previewButtonPanel.SuspendLayout();
        tabSql.SuspendLayout();
        sqlLayout.SuspendLayout();
        sqlButtonPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridResults).BeginInit();
        tabHistory.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridHistory).BeginInit();
        SuspendLayout();
        //
        // mainLayout
        //
        mainLayout.ColumnCount = 1;
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayout.Dock = DockStyle.Fill;
        mainLayout.Padding = new Padding(20, 20, 20, 20);
        mainLayout.RowCount = 2;
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayout.Controls.Add(headerPanel, 0, 0);
        mainLayout.Controls.Add(splitContainer, 0, 1);
        //
        // headerPanel
        //
        headerPanel.Controls.Add(headerLayout);
        headerPanel.Dock = DockStyle.Fill;
        headerPanel.Margin = new Padding(0, 0, 0, 16);
        headerPanel.Padding = new Padding(20, 12, 20, 12);
        //
        // headerLayout
        //
        headerLayout.ColumnCount = 2;
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        headerLayout.Dock = DockStyle.Fill;
        headerLayout.RowCount = 1;
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        headerLayout.Controls.Add(headerInfoPanel, 0, 0);
        headerLayout.Controls.Add(headerActionsPanel, 1, 0);
        //
        // headerInfoPanel
        //
        headerInfoPanel.Controls.Add(lblAppSubtitle);
        headerInfoPanel.Controls.Add(lblAppTitle);
        headerInfoPanel.Dock = DockStyle.Fill;
        //
        // lblAppTitle
        //
        lblAppTitle.Dock = DockStyle.Fill;
        lblAppTitle.TextAlign = ContentAlignment.MiddleLeft;
        lblAppTitle.Text = "DB Lite Desktop";
        //
        // lblAppSubtitle
        //
        lblAppSubtitle.AutoSize = true;
        lblAppSubtitle.Location = new Point(0, 30);
        lblAppSubtitle.Text = "轻量只读数据库客户端";
        //
        // headerActionsPanel
        //
        headerActionsPanel.Controls.Add(headerActionsLayout);
        headerActionsPanel.Dock = DockStyle.Fill;
        //
        // headerActionsLayout
        //
        headerActionsLayout.ColumnCount = 9;
        headerActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52F));
        headerActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
        headerActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
        headerActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
        headerActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
        headerActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
        headerActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
        headerActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
        headerActionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
        headerActionsLayout.Dock = DockStyle.Right;
        headerActionsLayout.Location = new Point(326, 0);
        headerActionsLayout.Margin = new Padding(0);
        headerActionsLayout.RowCount = 1;
        headerActionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        headerActionsLayout.Size = new Size(804, 48);
        headerActionsLayout.Controls.Add(lblConnection, 0, 0);
        headerActionsLayout.Controls.Add(cboConnections, 1, 0);
        headerActionsLayout.Controls.Add(btnNewConnection, 2, 0);
        headerActionsLayout.Controls.Add(btnEditConnection, 3, 0);
        headerActionsLayout.Controls.Add(btnDeleteConnection, 4, 0);
        headerActionsLayout.Controls.Add(btnTestConnection, 5, 0);
        headerActionsLayout.Controls.Add(btnConnect, 6, 0);
        headerActionsLayout.Controls.Add(btnRefresh, 7, 0);
        headerActionsLayout.Controls.Add(btnDisconnect, 8, 0);
        //
        // header actions
        //
        lblConnection.Anchor = AnchorStyles.Left;
        lblConnection.AutoSize = true;
        lblConnection.Text = "连接";
        cboConnections.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        cboConnections.DropDownStyle = ComboBoxStyle.DropDownList;
        cboConnections.Margin = new Padding(0, 0, 10, 0);
        btnNewConnection.Dock = DockStyle.Fill;
        btnNewConnection.Margin = new Padding(0, 0, 8, 0);
        btnNewConnection.Text = "新建";
        btnNewConnection.Click += btnNewConnection_Click;
        btnEditConnection.Dock = DockStyle.Fill;
        btnEditConnection.Margin = new Padding(0, 0, 8, 0);
        btnEditConnection.Text = "编辑";
        btnEditConnection.Click += btnEditConnection_Click;
        btnDeleteConnection.Dock = DockStyle.Fill;
        btnDeleteConnection.Margin = new Padding(0, 0, 8, 0);
        btnDeleteConnection.Text = "删除";
        btnDeleteConnection.Click += btnDeleteConnection_Click;
        btnTestConnection.Dock = DockStyle.Fill;
        btnTestConnection.Margin = new Padding(0, 0, 8, 0);
        btnTestConnection.Text = "测试";
        btnTestConnection.Click += btnTestConnection_Click;
        btnConnect.Dock = DockStyle.Fill;
        btnConnect.Margin = new Padding(0, 0, 8, 0);
        btnConnect.Text = "连接";
        btnConnect.Click += btnConnect_Click;
        btnRefresh.Dock = DockStyle.Fill;
        btnRefresh.Margin = new Padding(0, 0, 8, 0);
        btnRefresh.Text = "刷新";
        btnRefresh.Click += btnRefresh_Click;
        btnDisconnect.Dock = DockStyle.Fill;
        btnDisconnect.Margin = new Padding(0);
        btnDisconnect.Text = "断开";
        btnDisconnect.Click += btnDisconnect_Click;
        //
        // splitContainer
        //
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel1;
        splitContainer.IsSplitterFixed = false;
        splitContainer.Panel1.Controls.Add(navigationPanel);
        splitContainer.Panel1.Padding = new Padding(0, 0, 8, 0);
        splitContainer.Panel2.Controls.Add(workspacePanel);
        splitContainer.Panel2.Padding = new Padding(8, 0, 0, 0);
        splitContainer.SplitterWidth = 8;
        //
        // navigationPanel
        //
        navigationPanel.Controls.Add(navigationLayout);
        navigationPanel.Dock = DockStyle.Fill;
        navigationPanel.Padding = new Padding(20);
        //
        // navigationLayout
        //
        navigationLayout.ColumnCount = 1;
        navigationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        navigationLayout.Dock = DockStyle.Fill;
        navigationLayout.RowCount = 3;
        navigationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        navigationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        navigationLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        navigationLayout.Controls.Add(lblTablesTitle, 0, 0);
        navigationLayout.Controls.Add(lblTablesSubtitle, 0, 1);
        navigationLayout.Controls.Add(treeTables, 0, 2);
        //
        // navigation labels
        //
        lblTablesTitle.Dock = DockStyle.Fill;
        lblTablesTitle.Text = "数据表导航";
        lblTablesTitle.TextAlign = ContentAlignment.MiddleLeft;
        lblTablesSubtitle.Dock = DockStyle.Fill;
        lblTablesSubtitle.Text = "连接成功后显示当前数据库中的表。";
        lblTablesSubtitle.TextAlign = ContentAlignment.MiddleLeft;
        //
        // treeTables
        //
        treeTables.Dock = DockStyle.Fill;
        treeTables.HideSelection = false;
        treeTables.AfterSelect += treeTables_AfterSelect;
        //
        // workspacePanel
        //
        workspacePanel.Controls.Add(tabMain);
        workspacePanel.Dock = DockStyle.Fill;
        workspacePanel.Padding = new Padding(20, 20, 20, 20);
        //
        // tabMain
        //
        tabMain.Controls.Add(tabColumns);
        tabMain.Controls.Add(tabPreview);
        tabMain.Controls.Add(tabSql);
        tabMain.Controls.Add(tabHistory);
        tabMain.Dock = DockStyle.Fill;
        tabMain.ItemSize = new Size(116, 32);
        tabMain.Multiline = false;
        tabMain.SizeMode = TabSizeMode.Fixed;
        //
        // tabColumns
        //
        tabColumns.Controls.Add(gridColumns);
        tabColumns.Padding = new Padding(8);
        tabColumns.Text = "字段信息";
        //
        // gridColumns
        //
        gridColumns.AllowUserToAddRows = false;
        gridColumns.AllowUserToDeleteRows = false;
        gridColumns.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridColumns.Dock = DockStyle.Fill;
        gridColumns.ReadOnly = true;
        gridColumns.RowHeadersVisible = false;
        //
        // tabPreview
        //
        tabPreview.Controls.Add(previewLayout);
        tabPreview.Padding = new Padding(8);
        tabPreview.Text = "数据预览";
        //
        // previewLayout
        //
        previewLayout.ColumnCount = 1;
        previewLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        previewLayout.Dock = DockStyle.Fill;
        previewLayout.RowCount = 3;
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        previewLayout.Controls.Add(previewSearchPanel, 0, 0);
        previewLayout.Controls.Add(gridPreview, 0, 1);
        previewLayout.Controls.Add(previewButtonPanel, 0, 2);
        //
        // previewSearchPanel
        //
        previewSearchPanel.ColumnCount = 8;
        previewSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
        previewSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        previewSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
        previewSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
        previewSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));
        previewSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        previewSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));
        previewSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108F));
        previewSearchPanel.Dock = DockStyle.Fill;
        previewSearchPanel.Padding = new Padding(16, 14, 16, 14);
        previewSearchPanel.RowCount = 1;
        previewSearchPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        previewSearchPanel.Controls.Add(lblPreviewField, 0, 0);
        previewSearchPanel.Controls.Add(cboPreviewField, 1, 0);
        previewSearchPanel.Controls.Add(lblPreviewMatch, 2, 0);
        previewSearchPanel.Controls.Add(cboPreviewMatch, 3, 0);
        previewSearchPanel.Controls.Add(lblPreviewKeyword, 4, 0);
        previewSearchPanel.Controls.Add(txtPreviewKeyword, 5, 0);
        previewSearchPanel.Controls.Add(btnApplyPreviewFilter, 6, 0);
        previewSearchPanel.Controls.Add(btnResetPreviewFilter, 7, 0);
        //
        // preview search controls
        //
        lblPreviewField.Anchor = AnchorStyles.Left;
        lblPreviewField.AutoSize = true;
        lblPreviewField.Text = "字段";
        cboPreviewField.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        cboPreviewField.DropDownStyle = ComboBoxStyle.DropDownList;
        cboPreviewField.Margin = new Padding(0, 0, 14, 0);
        lblPreviewMatch.Anchor = AnchorStyles.Left;
        lblPreviewMatch.AutoSize = true;
        lblPreviewMatch.Text = "方式";
        cboPreviewMatch.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        cboPreviewMatch.DropDownStyle = ComboBoxStyle.DropDownList;
        cboPreviewMatch.Margin = new Padding(0, 0, 14, 0);
        lblPreviewKeyword.Anchor = AnchorStyles.Left;
        lblPreviewKeyword.AutoSize = true;
        lblPreviewKeyword.Text = "值";
        txtPreviewKeyword.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtPreviewKeyword.Margin = new Padding(0, 0, 14, 0);
        btnApplyPreviewFilter.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        btnApplyPreviewFilter.Margin = new Padding(0, 0, 12, 0);
        btnApplyPreviewFilter.Text = "查询";
        btnApplyPreviewFilter.Click += btnApplyPreviewFilter_Click;
        btnResetPreviewFilter.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        btnResetPreviewFilter.Margin = new Padding(0);
        btnResetPreviewFilter.Text = "重置";
        btnResetPreviewFilter.Click += btnResetPreviewFilter_Click;
        //
        // gridPreview
        //
        gridPreview.AllowUserToAddRows = false;
        gridPreview.AllowUserToDeleteRows = false;
        gridPreview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridPreview.Dock = DockStyle.Fill;
        gridPreview.ReadOnly = true;
        gridPreview.RowHeadersVisible = false;
        //
        // previewButtonPanel
        //
        previewButtonPanel.Controls.Add(btnPrevPage);
        previewButtonPanel.Controls.Add(btnNextPage);
        previewButtonPanel.Controls.Add(lblPreviewPage);
        previewButtonPanel.Controls.Add(lblPreviewTip);
        previewButtonPanel.Dock = DockStyle.Fill;
        previewButtonPanel.FlowDirection = FlowDirection.LeftToRight;
        previewButtonPanel.Padding = new Padding(12, 8, 12, 0);
        previewButtonPanel.WrapContents = false;
        //
        // preview footer controls
        //
        btnPrevPage.AutoSize = true;
        btnPrevPage.Text = "上一页";
        btnPrevPage.Click += btnPrevPage_Click;
        btnNextPage.AutoSize = true;
        btnNextPage.Text = "下一页";
        btnNextPage.Click += btnNextPage_Click;
        lblPreviewPage.AutoSize = true;
        lblPreviewPage.Margin = new Padding(16, 10, 16, 0);
        lblPreviewPage.Text = "第 1 页";
        lblPreviewTip.AutoSize = true;
        lblPreviewTip.Margin = new Padding(0, 10, 0, 0);
        lblPreviewTip.Text = "支持 字段名=数据";
        //
        // tabSql
        //
        tabSql.Controls.Add(sqlLayout);
        tabSql.Padding = new Padding(8);
        tabSql.Text = "SQL 查询";
        //
        // sqlLayout
        //
        sqlLayout.ColumnCount = 1;
        sqlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sqlLayout.Dock = DockStyle.Fill;
        sqlLayout.RowCount = 4;
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 146F));
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        sqlLayout.Controls.Add(txtSql, 0, 0);
        sqlLayout.Controls.Add(sqlButtonPanel, 0, 1);
        sqlLayout.Controls.Add(gridResults, 0, 2);
        sqlLayout.Controls.Add(lblStatus, 0, 3);
        //
        // txtSql
        //
        txtSql.Dock = DockStyle.Fill;
        txtSql.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
        txtSql.Multiline = true;
        txtSql.ScrollBars = ScrollBars.Both;
        txtSql.WordWrap = false;
        //
        // sqlButtonPanel
        //
        sqlButtonPanel.Controls.Add(btnRunSql);
        sqlButtonPanel.Controls.Add(btnClearSql);
        sqlButtonPanel.Controls.Add(btnCopySql);
        sqlButtonPanel.Dock = DockStyle.Fill;
        sqlButtonPanel.FlowDirection = FlowDirection.LeftToRight;
        sqlButtonPanel.Padding = new Padding(12, 8, 12, 0);
        //
        // sql buttons
        //
        btnRunSql.AutoSize = true;
        btnRunSql.Text = "执行";
        btnRunSql.Click += btnRunSql_Click;
        btnClearSql.AutoSize = true;
        btnClearSql.Text = "清空";
        btnClearSql.Click += btnClearSql_Click;
        btnCopySql.AutoSize = true;
        btnCopySql.Text = "复制 SQL";
        btnCopySql.Click += btnCopySql_Click;
        //
        // gridResults
        //
        gridResults.AllowUserToAddRows = false;
        gridResults.AllowUserToDeleteRows = false;
        gridResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridResults.Dock = DockStyle.Fill;
        gridResults.ReadOnly = true;
        gridResults.RowHeadersVisible = false;
        //
        // lblStatus
        //
        lblStatus.Dock = DockStyle.Fill;
        lblStatus.Text = "未连接";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        //
        // tabHistory
        //
        tabHistory.Controls.Add(gridHistory);
        tabHistory.Padding = new Padding(8);
        tabHistory.Text = "查询历史";
        //
        // gridHistory
        //
        gridHistory.AllowUserToAddRows = false;
        gridHistory.AllowUserToDeleteRows = false;
        gridHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridHistory.Dock = DockStyle.Fill;
        gridHistory.ReadOnly = true;
        gridHistory.RowHeadersVisible = false;
        gridHistory.CellDoubleClick += gridHistory_CellDoubleClick;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 760);
        Controls.Add(mainLayout);
        MinimumSize = new Size(1120, 680);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "DB Lite Desktop";
        mainLayout.ResumeLayout(false);
        headerPanel.ResumeLayout(false);
        headerLayout.ResumeLayout(false);
        headerInfoPanel.ResumeLayout(false);
        headerInfoPanel.PerformLayout();
        headerActionsPanel.ResumeLayout(false);
        headerActionsLayout.ResumeLayout(false);
        headerActionsLayout.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        navigationPanel.ResumeLayout(false);
        navigationLayout.ResumeLayout(false);
        workspacePanel.ResumeLayout(false);
        tabMain.ResumeLayout(false);
        tabColumns.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridColumns).EndInit();
        tabPreview.ResumeLayout(false);
        previewLayout.ResumeLayout(false);
        previewSearchPanel.ResumeLayout(false);
        previewSearchPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridPreview).EndInit();
        previewButtonPanel.ResumeLayout(false);
        previewButtonPanel.PerformLayout();
        tabSql.ResumeLayout(false);
        sqlLayout.ResumeLayout(false);
        sqlLayout.PerformLayout();
        sqlButtonPanel.ResumeLayout(false);
        sqlButtonPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridResults).EndInit();
        tabHistory.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridHistory).EndInit();
        ResumeLayout(false);
    }
}
