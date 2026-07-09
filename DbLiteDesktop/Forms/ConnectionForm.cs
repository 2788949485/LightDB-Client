using DbLiteDesktop.Models;
using DbLiteDesktop.Providers;
using DbLiteDesktop.Services;

namespace DbLiteDesktop.Forms;

public partial class ConnectionForm : Form
{
    private readonly PasswordEncryptService _passwordEncryptService = new();

    public DbConnectionConfig ConnectionConfig { get; private set; }
    public bool ConnectAfterSave { get; private set; }

    public ConnectionForm(DbConnectionConfig? existing = null)
    {
        InitializeComponent();
        ConnectionConfig = existing is null
            ? new DbConnectionConfig { DbType = "sqlite" }
            : Clone(existing);
        LoadFromModel();
        ApplyTheme();
    }

    private static DbConnectionConfig Clone(DbConnectionConfig config)
    {
        return new DbConnectionConfig
        {
            Id = config.Id,
            Name = config.Name,
            DbType = config.DbType,
            Host = config.Host,
            Port = config.Port,
            DatabaseName = config.DatabaseName,
            Username = config.Username,
            PasswordEncrypted = config.PasswordEncrypted,
            FilePath = config.FilePath,
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt
        };
    }

    private void LoadFromModel()
    {
        cboDbType.Items.Clear();
        cboDbType.Items.AddRange(["sqlite", "mysql"]);
        cboDbType.SelectedItem = string.IsNullOrWhiteSpace(ConnectionConfig.DbType)
            ? "sqlite"
            : ConnectionConfig.DbType.ToLowerInvariant();

        txtName.Text = ConnectionConfig.Name;
        txtHost.Text = ConnectionConfig.Host ?? string.Empty;
        numPort.Value = ConnectionConfig.Port is > 0 ? ConnectionConfig.Port.Value : 3306;
        txtDatabaseName.Text = ConnectionConfig.DatabaseName ?? string.Empty;
        txtUsername.Text = ConnectionConfig.Username ?? string.Empty;
        txtFilePath.Text = ConnectionConfig.FilePath ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(ConnectionConfig.PasswordEncrypted) &&
            !string.Equals(ConnectionConfig.DbType, "sqlite", StringComparison.OrdinalIgnoreCase))
        {
            txtPassword.Text = _passwordEncryptService.Decrypt(ConnectionConfig.PasswordEncrypted);
        }

        UpdateFieldVisibility();
    }

    private void UpdateFieldVisibility()
    {
        var isSqlite = SelectedDbType == "sqlite";

        lblHost.Visible = txtHost.Visible = !isSqlite;
        lblPort.Visible = numPort.Visible = !isSqlite;
        lblDatabaseName.Visible = txtDatabaseName.Visible = !isSqlite;
        lblUsername.Visible = txtUsername.Visible = !isSqlite;
        lblPassword.Visible = txtPassword.Visible = !isSqlite;

        lblFilePath.Visible = txtFilePath.Visible = btnBrowse.Visible = isSqlite;
    }

    private string SelectedDbType =>
        (cboDbType.SelectedItem?.ToString() ?? "sqlite").ToLowerInvariant();

    private DbConnectionConfig BuildConfigFromInputs()
    {
        var dbType = SelectedDbType;

        return new DbConnectionConfig
        {
            Id = ConnectionConfig.Id,
            Name = txtName.Text.Trim(),
            DbType = dbType,
            Host = dbType == "mysql" ? txtHost.Text.Trim() : null,
            Port = dbType == "mysql" ? (int)numPort.Value : null,
            DatabaseName = dbType == "mysql" ? txtDatabaseName.Text.Trim() : null,
            Username = dbType == "mysql" ? txtUsername.Text.Trim() : null,
            PasswordEncrypted = dbType == "mysql"
                ? _passwordEncryptService.Encrypt(txtPassword.Text)
                : null,
            FilePath = dbType == "sqlite" ? txtFilePath.Text.Trim() : null,
            CreatedAt = ConnectionConfig.CreatedAt,
            UpdatedAt = ConnectionConfig.UpdatedAt
        };
    }

    private bool ValidateInputs(out string message)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            message = "请输入连接名称。";
            return false;
        }

        if (SelectedDbType == "sqlite")
        {
            if (string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                message = "请选择 SQLite 数据库文件。";
                return false;
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text) ||
                string.IsNullOrWhiteSpace(txtDatabaseName.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                message = "请填写 MySQL 的主机、数据库名称和用户名。";
                return false;
            }
        }

        message = string.Empty;
        return true;
    }

    private void SaveAndClose(bool connectAfterSave)
    {
        if (!ValidateInputs(out var message))
        {
            MessageBox.Show(message, "提示");
            return;
        }

        ConnectionConfig = BuildConfigFromInputs();
        ConnectAfterSave = connectAfterSave;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void TestCurrentConnection()
    {
        if (!ValidateInputs(out var message))
        {
            MessageBox.Show(message, "提示");
            return;
        }

        var config = BuildConfigFromInputs();
        var provider = DatabaseProviderFactory.Create(config.DbType);
        var password = config.DbType == "sqlite" ? string.Empty : txtPassword.Text;

        provider.TestConnection(config, password);
        MessageBox.Show("连接测试成功。", "提示");
    }

    private void ApplyTheme()
    {
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        foreach (var button in new[] { btnTest, btnSave, btnConnect })
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Color.FromArgb(37, 99, 235);
            button.ForeColor = Color.White;
            button.Padding = new Padding(10, 4, 10, 4);
        }

        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        btnCancel.BackColor = Color.White;
        btnCancel.ForeColor = Color.FromArgb(51, 65, 85);

        btnBrowse.FlatStyle = FlatStyle.Flat;
        btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);
        btnBrowse.BackColor = Color.White;
        btnBrowse.ForeColor = Color.FromArgb(30, 64, 175);
    }

    private void cboDbType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateFieldVisibility();
    }

    private void btnBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "SQLite Database|*.db;*.sqlite;*.sqlite3|All Files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            txtFilePath.Text = dialog.FileName;
        }
    }

    private void btnTest_Click(object? sender, EventArgs e)
    {
        try
        {
            TestCurrentConnection();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "连接测试失败");
        }
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        SaveAndClose(false);
    }

    private void btnConnect_Click(object? sender, EventArgs e)
    {
        SaveAndClose(true);
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
