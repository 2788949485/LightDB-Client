namespace DbLiteDesktop;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private ToolStrip toolStrip = null!;
    private ToolStripLabel lblConnection = null!;
    private ToolStripComboBox cboConnections = null!;
    private ToolStripButton btnNewConnection = null!;
    private ToolStripButton btnEditConnection = null!;
    private ToolStripButton btnDeleteConnection = null!;
    private ToolStripButton btnTestConnection = null!;
    private ToolStripButton btnConnect = null!;
    private ToolStripButton btnRefresh = null!;
    private ToolStripButton btnDisconnect = null!;
    private SplitContainer splitContainer = null!;
    private TreeView treeTables = null!;
    private TabControl tabMain = null!;
    private TabPage tabColumns = null!;
    private TabPage tabSql = null!;
    private TabPage tabHistory = null!;
    private TabPage tabPreview = null!;
    private DataGridView gridColumns = null!;
    private DataGridView gridResults = null!;
    private DataGridView gridHistory = null!;
    private DataGridView gridPreview = null!;
    private TextBox txtSql = null!;
    private FlowLayoutPanel sqlButtonPanel = null!;
    private FlowLayoutPanel previewButtonPanel = null!;
    private Button btnRunSql = null!;
    private Button btnClearSql = null!;
    private Button btnCopySql = null!;
    private Button btnPrevPage = null!;
    private Button btnNextPage = null!;
    private Label lblStatus = null!;
    private Label lblPreviewPage = null!;
    private Label lblPreviewTip = null!;
    private TableLayoutPanel sqlLayout = null!;
    private TableLayoutPanel previewLayout = null!;

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
        toolStrip = new ToolStrip();
        lblConnection = new ToolStripLabel();
        cboConnections = new ToolStripComboBox();
        btnNewConnection = new ToolStripButton();
        btnEditConnection = new ToolStripButton();
        btnDeleteConnection = new ToolStripButton();
        btnTestConnection = new ToolStripButton();
        btnConnect = new ToolStripButton();
        btnRefresh = new ToolStripButton();
        btnDisconnect = new ToolStripButton();
        splitContainer = new SplitContainer();
        treeTables = new TreeView();
        tabMain = new TabControl();
        tabColumns = new TabPage();
        gridColumns = new DataGridView();
        tabSql = new TabPage();
        sqlLayout = new TableLayoutPanel();
        txtSql = new TextBox();
        sqlButtonPanel = new FlowLayoutPanel();
        btnRunSql = new Button();
        btnClearSql = new Button();
        btnCopySql = new Button();
        gridResults = new DataGridView();
        lblStatus = new Label();
        tabPreview = new TabPage();
        previewLayout = new TableLayoutPanel();
        gridPreview = new DataGridView();
        previewButtonPanel = new FlowLayoutPanel();
        btnPrevPage = new Button();
        btnNextPage = new Button();
        lblPreviewPage = new Label();
        lblPreviewTip = new Label();
        tabHistory = new TabPage();
        gridHistory = new DataGridView();
        toolStrip.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        tabMain.SuspendLayout();
        tabColumns.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridColumns).BeginInit();
        tabSql.SuspendLayout();
        sqlLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridResults).BeginInit();
        tabHistory.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridHistory).BeginInit();
        SuspendLayout();
        //
        // toolStrip
        //
        toolStrip.Items.AddRange(
        [
            lblConnection,
            cboConnections,
            btnNewConnection,
            btnEditConnection,
            btnDeleteConnection,
            btnTestConnection,
            btnConnect,
            btnRefresh,
            btnDisconnect
        ]);
        toolStrip.Location = new Point(0, 0);
        toolStrip.Name = "toolStrip";
        toolStrip.Size = new Size(1184, 25);
        //
        // tool strip items
        //
        lblConnection.Text = "连接";
        cboConnections.DropDownStyle = ComboBoxStyle.DropDownList;
        cboConnections.AutoSize = false;
        cboConnections.Width = 220;
        btnNewConnection.Text = "新建";
        btnEditConnection.Text = "编辑";
        btnDeleteConnection.Text = "删除";
        btnTestConnection.Text = "测试";
        btnConnect.Text = "连接";
        btnRefresh.Text = "刷新";
        btnDisconnect.Text = "断开";

        btnNewConnection.Click += btnNewConnection_Click;
        btnEditConnection.Click += btnEditConnection_Click;
        btnDeleteConnection.Click += btnDeleteConnection_Click;
        btnTestConnection.Click += btnTestConnection_Click;
        btnConnect.Click += btnConnect_Click;
        btnRefresh.Click += btnRefresh_Click;
        btnDisconnect.Click += btnDisconnect_Click;
        //
        // splitContainer
        //
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 25);
        splitContainer.Name = "splitContainer";
        splitContainer.Panel1.Controls.Add(treeTables);
        splitContainer.Panel2.Controls.Add(tabMain);
        splitContainer.Size = new Size(1184, 636);
        splitContainer.SplitterDistance = 280;
        //
        // treeTables
        //
        treeTables.Dock = DockStyle.Fill;
        treeTables.HideSelection = false;
        treeTables.AfterSelect += treeTables_AfterSelect;
        //
        // tabMain
        //
        tabMain.Controls.Add(tabColumns);
        tabMain.Controls.Add(tabPreview);
        tabMain.Controls.Add(tabSql);
        tabMain.Controls.Add(tabHistory);
        tabMain.Dock = DockStyle.Fill;
        //
        // tabColumns
        //
        tabColumns.Controls.Add(gridColumns);
        tabColumns.Text = "字段信息";
        tabColumns.Padding = new Padding(6);
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
        // tabSql
        //
        tabSql.Controls.Add(sqlLayout);
        tabSql.Text = "SQL 查询";
        tabSql.Padding = new Padding(6);
        //
        // sqlLayout
        //
        sqlLayout.ColumnCount = 1;
        sqlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sqlLayout.Dock = DockStyle.Fill;
        sqlLayout.RowCount = 4;
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        sqlLayout.Controls.Add(txtSql, 0, 0);
        sqlLayout.Controls.Add(sqlButtonPanel, 0, 1);
        sqlLayout.Controls.Add(gridResults, 0, 2);
        sqlLayout.Controls.Add(lblStatus, 0, 3);
        //
        // txtSql
        //
        txtSql.Dock = DockStyle.Fill;
        txtSql.Multiline = true;
        txtSql.ScrollBars = ScrollBars.Both;
        txtSql.WordWrap = false;
        txtSql.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
        //
        // sqlButtonPanel
        //
        sqlButtonPanel.Dock = DockStyle.Fill;
        sqlButtonPanel.FlowDirection = FlowDirection.LeftToRight;
        sqlButtonPanel.Controls.Add(btnRunSql);
        sqlButtonPanel.Controls.Add(btnClearSql);
        sqlButtonPanel.Controls.Add(btnCopySql);
        //
        // sql buttons
        //
        btnRunSql.Text = "执行";
        btnRunSql.AutoSize = true;
        btnRunSql.Click += btnRunSql_Click;
        btnClearSql.Text = "清空";
        btnClearSql.AutoSize = true;
        btnClearSql.Click += btnClearSql_Click;
        btnCopySql.Text = "复制 SQL";
        btnCopySql.AutoSize = true;
        btnCopySql.Click += btnCopySql_Click;
        //
        // gridResults
        //
        gridResults.AllowUserToAddRows = false;
        gridResults.AllowUserToDeleteRows = false;
        gridResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
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
        // tabPreview
        //
        tabPreview.Controls.Add(previewLayout);
        tabPreview.Text = "数据预览";
        tabPreview.Padding = new Padding(6);
        //
        // previewLayout
        //
        previewLayout.ColumnCount = 1;
        previewLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        previewLayout.Dock = DockStyle.Fill;
        previewLayout.RowCount = 2;
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        previewLayout.Controls.Add(gridPreview, 0, 0);
        previewLayout.Controls.Add(previewButtonPanel, 0, 1);
        //
        // gridPreview
        //
        gridPreview.AllowUserToAddRows = false;
        gridPreview.AllowUserToDeleteRows = false;
        gridPreview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        gridPreview.Dock = DockStyle.Fill;
        gridPreview.ReadOnly = true;
        gridPreview.RowHeadersVisible = false;
        //
        // previewButtonPanel
        //
        previewButtonPanel.Dock = DockStyle.Fill;
        previewButtonPanel.FlowDirection = FlowDirection.LeftToRight;
        previewButtonPanel.WrapContents = false;
        previewButtonPanel.Controls.Add(btnPrevPage);
        previewButtonPanel.Controls.Add(btnNextPage);
        previewButtonPanel.Controls.Add(lblPreviewPage);
        previewButtonPanel.Controls.Add(lblPreviewTip);
        //
        // preview controls
        //
        btnPrevPage.Text = "上一页";
        btnPrevPage.AutoSize = true;
        btnPrevPage.Click += btnPrevPage_Click;
        btnNextPage.Text = "下一页";
        btnNextPage.AutoSize = true;
        btnNextPage.Click += btnNextPage_Click;
        lblPreviewPage.AutoSize = true;
        lblPreviewPage.Margin = new Padding(16, 10, 16, 0);
        lblPreviewPage.Text = "第 1 页";
        lblPreviewTip.AutoSize = true;
        lblPreviewTip.Margin = new Padding(0, 10, 0, 0);
        lblPreviewTip.Text = "每页 100 条";
        //
        // tabHistory
        //
        tabHistory.Controls.Add(gridHistory);
        tabHistory.Text = "查询历史";
        tabHistory.Padding = new Padding(6);
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
        ClientSize = new Size(1184, 661);
        Controls.Add(splitContainer);
        Controls.Add(toolStrip);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "DB Lite Desktop";
        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        tabMain.ResumeLayout(false);
        tabColumns.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridColumns).EndInit();
        tabSql.ResumeLayout(false);
        sqlLayout.ResumeLayout(false);
        sqlLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridResults).EndInit();
        tabHistory.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridHistory).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
