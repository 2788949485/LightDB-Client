namespace DbLiteDesktop.Forms;

partial class ConnectionForm
{
    private System.ComponentModel.IContainer components = null!;
    private TableLayoutPanel layout = null!;
    private Label lblName = null!;
    private TextBox txtName = null!;
    private Label lblDbType = null!;
    private ComboBox cboDbType = null!;
    private Label lblHost = null!;
    private TextBox txtHost = null!;
    private Label lblPort = null!;
    private NumericUpDown numPort = null!;
    private Label lblDatabaseName = null!;
    private TextBox txtDatabaseName = null!;
    private Label lblUsername = null!;
    private TextBox txtUsername = null!;
    private Label lblPassword = null!;
    private TextBox txtPassword = null!;
    private Label lblFilePath = null!;
    private TextBox txtFilePath = null!;
    private Button btnBrowse = null!;
    private FlowLayoutPanel buttonPanel = null!;
    private Button btnTest = null!;
    private Button btnSave = null!;
    private Button btnConnect = null!;
    private Button btnCancel = null!;

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
        layout = new TableLayoutPanel();
        lblName = new Label();
        txtName = new TextBox();
        lblDbType = new Label();
        cboDbType = new ComboBox();
        lblHost = new Label();
        txtHost = new TextBox();
        lblPort = new Label();
        numPort = new NumericUpDown();
        lblDatabaseName = new Label();
        txtDatabaseName = new TextBox();
        lblUsername = new Label();
        txtUsername = new TextBox();
        lblPassword = new Label();
        txtPassword = new TextBox();
        lblFilePath = new Label();
        txtFilePath = new TextBox();
        btnBrowse = new Button();
        buttonPanel = new FlowLayoutPanel();
        btnTest = new Button();
        btnSave = new Button();
        btnConnect = new Button();
        btnCancel = new Button();
        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        SuspendLayout();
        //
        // layout
        //
        layout.ColumnCount = 3;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
        layout.Dock = DockStyle.Fill;
        layout.Padding = new Padding(12);
        layout.RowCount = 8;
        for (var i = 0; i < 7; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        }

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        //
        // labels and inputs
        //
        lblName.Text = "连接名称";
        lblName.TextAlign = ContentAlignment.MiddleLeft;
        lblName.Dock = DockStyle.Fill;
        txtName.Dock = DockStyle.Fill;
        layout.Controls.Add(lblName, 0, 0);
        layout.Controls.Add(txtName, 1, 0);
        layout.SetColumnSpan(txtName, 2);

        lblDbType.Text = "数据库类型";
        lblDbType.TextAlign = ContentAlignment.MiddleLeft;
        lblDbType.Dock = DockStyle.Fill;
        cboDbType.Dock = DockStyle.Fill;
        cboDbType.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDbType.SelectedIndexChanged += cboDbType_SelectedIndexChanged;
        layout.Controls.Add(lblDbType, 0, 1);
        layout.Controls.Add(cboDbType, 1, 1);
        layout.SetColumnSpan(cboDbType, 2);

        lblHost.Text = "主机地址";
        lblHost.TextAlign = ContentAlignment.MiddleLeft;
        lblHost.Dock = DockStyle.Fill;
        txtHost.Dock = DockStyle.Fill;
        layout.Controls.Add(lblHost, 0, 2);
        layout.Controls.Add(txtHost, 1, 2);
        layout.SetColumnSpan(txtHost, 2);

        lblPort.Text = "端口";
        lblPort.TextAlign = ContentAlignment.MiddleLeft;
        lblPort.Dock = DockStyle.Fill;
        numPort.Dock = DockStyle.Left;
        numPort.Maximum = 65535;
        numPort.Minimum = 1;
        numPort.Value = 3306;
        numPort.Width = 120;
        layout.Controls.Add(lblPort, 0, 3);
        layout.Controls.Add(numPort, 1, 3);

        lblDatabaseName.Text = "数据库名称";
        lblDatabaseName.TextAlign = ContentAlignment.MiddleLeft;
        lblDatabaseName.Dock = DockStyle.Fill;
        txtDatabaseName.Dock = DockStyle.Fill;
        layout.Controls.Add(lblDatabaseName, 0, 4);
        layout.Controls.Add(txtDatabaseName, 1, 4);
        layout.SetColumnSpan(txtDatabaseName, 2);

        lblUsername.Text = "用户名";
        lblUsername.TextAlign = ContentAlignment.MiddleLeft;
        lblUsername.Dock = DockStyle.Fill;
        txtUsername.Dock = DockStyle.Fill;
        layout.Controls.Add(lblUsername, 0, 5);
        layout.Controls.Add(txtUsername, 1, 5);
        layout.SetColumnSpan(txtUsername, 2);

        lblPassword.Text = "密码";
        lblPassword.TextAlign = ContentAlignment.MiddleLeft;
        lblPassword.Dock = DockStyle.Fill;
        txtPassword.Dock = DockStyle.Fill;
        txtPassword.UseSystemPasswordChar = true;
        layout.Controls.Add(lblPassword, 0, 6);
        layout.Controls.Add(txtPassword, 1, 6);
        layout.SetColumnSpan(txtPassword, 2);

        lblFilePath.Text = "SQLite 文件";
        lblFilePath.TextAlign = ContentAlignment.MiddleLeft;
        lblFilePath.Dock = DockStyle.Fill;
        txtFilePath.Dock = DockStyle.Fill;
        btnBrowse.Text = "选择文件";
        btnBrowse.Width = 80;
        btnBrowse.Click += btnBrowse_Click;
        layout.Controls.Add(lblFilePath, 0, 7);
        layout.Controls.Add(txtFilePath, 1, 7);
        layout.Controls.Add(btnBrowse, 2, 7);

        //
        // button panel
        //
        buttonPanel.Dock = DockStyle.Bottom;
        buttonPanel.FlowDirection = FlowDirection.RightToLeft;
        buttonPanel.AutoSize = true;
        buttonPanel.Padding = new Padding(12, 0, 12, 12);

        btnCancel.Text = "取消";
        btnCancel.AutoSize = true;
        btnCancel.Click += btnCancel_Click;

        btnConnect.Text = "连接";
        btnConnect.AutoSize = true;
        btnConnect.Click += btnConnect_Click;

        btnSave.Text = "保存连接";
        btnSave.AutoSize = true;
        btnSave.Click += btnSave_Click;

        btnTest.Text = "测试连接";
        btnTest.AutoSize = true;
        btnTest.Click += btnTest_Click;

        buttonPanel.Controls.Add(btnCancel);
        buttonPanel.Controls.Add(btnConnect);
        buttonPanel.Controls.Add(btnSave);
        buttonPanel.Controls.Add(btnTest);

        //
        // ConnectionForm
        //
        AcceptButton = btnConnect;
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(560, 390);
        Controls.Add(layout);
        Controls.Add(buttonPanel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ConnectionForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "数据库连接";
        ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
