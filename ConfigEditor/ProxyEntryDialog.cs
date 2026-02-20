using System.Diagnostics;

namespace ConfigEditor;

public class ProxyEntryDialog : Form
{
    private static readonly string[] EndpointOptions =
    {
        "socks-local.antizapret:8118",
        "socks-world.antizapret:8118"
    };

    private readonly ProxyEntry _entry;
    private readonly Dictionary<string, (string Username, string Password)> _endpointCredentials;

    private TextBox txtAppNames = null!;
    private ComboBox cmbRunningApps = null!;
    private Button btnAddApp = null!;
    private ComboBox cmbEndpoint = null!;
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private CheckBox chkTcp = null!;
    private CheckBox chkUdp = null!;
    private Button btnOk = null!;
    private Button btnCancel = null!;

    public ProxyEntryDialog(ProxyEntry entry, Dictionary<string, (string Username, string Password)>? endpointCredentials = null)
    {
        _entry = entry;
        _endpointCredentials = endpointCredentials ?? new();
        InitializeComponent();
        LoadEntryData();
        LoadRunningProcesses();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Text = "Прокси-конфигурация";
        Size = new Size(480, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 9F);

        int y = 16;
        int labelX = 16;
        int inputX = 170;
        int inputW = 280;

        // App Names
        var lblApps = new Label { Text = "Приложения (appNames):", Location = new Point(labelX, y + 2), AutoSize = true };
        txtAppNames = new TextBox { Location = new Point(inputX, y), Size = new Size(inputW, 24) };
        y += 32;

        // Running processes dropdown
        var lblRunning = new Label { Text = "Запущенные процессы:", Location = new Point(labelX, y + 2), AutoSize = true };
        cmbRunningApps = new ComboBox
        {
            Location = new Point(inputX, y),
            Size = new Size(200, 24),
            DropDownStyle = ComboBoxStyle.DropDown
        };
        btnAddApp = new Button
        {
            Text = "← Добавить",
            Location = new Point(inputX + 206, y - 1),
            Size = new Size(74, 26)
        };
        btnAddApp.Click += BtnAddApp_Click;
        y += 36;

        // Endpoint
        var lblEndpoint = new Label { Text = "Прокси-сервер:", Location = new Point(labelX, y + 2), AutoSize = true };
        cmbEndpoint = new ComboBox
        {
            Location = new Point(inputX, y),
            Size = new Size(inputW, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbEndpoint.Items.AddRange(EndpointOptions);
        cmbEndpoint.SelectedIndexChanged += CmbEndpoint_SelectedIndexChanged;
        y += 36;

        // Username
        var lblUser = new Label { Text = "Имя пользователя:", Location = new Point(labelX, y + 2), AutoSize = true };
        txtUsername = new TextBox { Location = new Point(inputX, y), Size = new Size(inputW, 24) };
        y += 32;

        // Password
        var lblPass = new Label { Text = "Пароль:", Location = new Point(labelX, y + 2), AutoSize = true };
        txtPassword = new TextBox { Location = new Point(inputX, y), Size = new Size(inputW, 24) };
        y += 40;

        // Protocols
        var lblProtocols = new Label { Text = "Протоколы:", Location = new Point(labelX, y + 2), AutoSize = true };
        chkTcp = new CheckBox { Text = "TCP", Location = new Point(inputX, y), AutoSize = true };
        chkUdp = new CheckBox { Text = "UDP", Location = new Point(inputX + 80, y), AutoSize = true };
        y += 44;

        // Buttons
        btnOk = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(260, y),
            Size = new Size(90, 32)
        };
        btnOk.Click += BtnOk_Click;

        btnCancel = new Button
        {
            Text = "Отмена",
            DialogResult = DialogResult.Cancel,
            Location = new Point(360, y),
            Size = new Size(90, 32)
        };

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        Controls.AddRange(new Control[]
        {
            lblApps, txtAppNames,
            lblRunning, cmbRunningApps, btnAddApp,
            lblEndpoint, cmbEndpoint,
            lblUser, txtUsername,
            lblPass, txtPassword,
            lblProtocols, chkTcp, chkUdp,
            btnOk, btnCancel
        });

        ResumeLayout(false);
        PerformLayout();
    }

    private void LoadEntryData()
    {
        txtAppNames.Text = string.Join(", ", _entry.AppNames);

        // Set endpoint selection
        var idx = Array.IndexOf(EndpointOptions, _entry.Socks5ProxyEndpoint);
        cmbEndpoint.SelectedIndex = idx >= 0 ? idx : 0;

        txtUsername.Text = _entry.Username;
        txtPassword.Text = _entry.Password;

        // For new entries (empty credentials), auto-fill from remembered credentials
        if (string.IsNullOrEmpty(_entry.Username) || string.IsNullOrEmpty(_entry.Password))
        {
            AutoFillCredentials();
        }

        chkTcp.Checked = _entry.SupportedProtocols.Contains("TCP");
        chkUdp.Checked = _entry.SupportedProtocols.Contains("UDP");
    }

    private void LoadRunningProcesses()
    {
        try
        {
            var processNames = Process.GetProcesses()
                .Select(p => p.ProcessName)
                .Distinct()
                .OrderBy(n => n)
                .ToArray();

            cmbRunningApps.Items.Clear();
            cmbRunningApps.Items.AddRange(processNames);

            if (cmbRunningApps.Items.Count > 0)
                cmbRunningApps.SelectedIndex = 0;
        }
        catch
        {
            // Could not enumerate processes
        }
    }

    private void BtnAddApp_Click(object? sender, EventArgs e)
    {
        var processName = cmbRunningApps.Text.Trim();
        if (string.IsNullOrEmpty(processName))
            return;

        var current = txtAppNames.Text.Trim();
        if (string.IsNullOrEmpty(current))
        {
            txtAppNames.Text = processName;
        }
        else
        {
            // Add only if not already present
            var existing = current.Split(',').Select(s => s.Trim()).ToList();
            if (!existing.Contains(processName, StringComparer.OrdinalIgnoreCase))
            {
                txtAppNames.Text = current + ", " + processName;
            }
        }
    }

    private void CmbEndpoint_SelectedIndexChanged(object? sender, EventArgs e)
    {
        AutoFillCredentials();
    }

    private void AutoFillCredentials()
    {
        var endpoint = cmbEndpoint.SelectedItem?.ToString();
        if (endpoint != null && _endpointCredentials.TryGetValue(endpoint, out var creds))
        {
            txtUsername.Text = creds.Username;
            txtPassword.Text = creds.Password;
        }
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        // Validate
        if (!chkTcp.Checked && !chkUdp.Checked)
        {
            MessageBox.Show("Выберите хотя бы один протокол (TCP или UDP).",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        var appNamesText = txtAppNames.Text.Trim();
        if (string.IsNullOrEmpty(appNamesText))
        {
            MessageBox.Show("Укажите хотя бы одно приложение.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        // Update entry
        _entry.AppNames = appNamesText
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        _entry.Socks5ProxyEndpoint = cmbEndpoint.SelectedItem?.ToString() ?? EndpointOptions[0];
        _entry.Username = txtUsername.Text.Trim();
        _entry.Password = txtPassword.Text.Trim();

        _entry.SupportedProtocols = new List<string>();
        if (chkTcp.Checked) _entry.SupportedProtocols.Add("TCP");
        if (chkUdp.Checked) _entry.SupportedProtocols.Add("UDP");
    }
}
