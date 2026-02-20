using System.Diagnostics;
using System.Text.Json;
using Microsoft.Win32;

namespace ConfigEditor;

public partial class MainForm : Form
{
    private static readonly string ConfigFileName = "app-config.json";
    private static readonly string ProxiFyreExe = "ProxiFyre.exe";
    private static readonly string DriverMsi = "Windows.Packet.Filter.3.6.2.1.x64.msi";

    private AppConfig _config = new();
    private string _configPath = "";
    private string _appDir = "";

    private ListBox lstProxies = null!;
    private Button btnAdd = null!;
    private Button btnEdit = null!;
    private Button btnDelete = null!;
    private Button btnSave = null!;
    private Button btnLaunch = null!;
    private Button btnCheckDriver = null!;
    private Label lblStatus = null!;

    public MainForm()
    {
        InitializeComponent();
        ResolveAppDirectory();
        CheckDriverOnStartup();
        LoadConfig();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Text = "AntizapretSOCKS5 — Редактор конфигурации";
        Size = new Size(620, 500);
        MinimumSize = new Size(520, 420);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        var exeIcon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        if (exeIcon != null)
            Icon = exeIcon;

        var lblTitle = new Label
        {
            Text = "Список прокси-конфигураций:",
            Location = new Point(12, 12),
            AutoSize = true
        };

        lstProxies = new ListBox
        {
            Location = new Point(12, 34),
            Size = new Size(440, 340),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            IntegralHeight = false
        };

        btnAdd = new Button
        {
            Text = "Добавить",
            Location = new Point(462, 34),
            Size = new Size(130, 32),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnAdd.Click += BtnAdd_Click;

        btnEdit = new Button
        {
            Text = "Редактировать",
            Location = new Point(462, 72),
            Size = new Size(130, 32),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnEdit.Click += BtnEdit_Click;

        btnDelete = new Button
        {
            Text = "Удалить",
            Location = new Point(462, 110),
            Size = new Size(130, 32),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnDelete.Click += BtnDelete_Click;

        btnSave = new Button
        {
            Text = "💾 Сохранить конфиг",
            Location = new Point(462, 170),
            Size = new Size(130, 40),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        btnSave.Click += BtnSave_Click;

        btnLaunch = new Button
        {
            Text = "▶ Запустить ProxiFyre",
            Location = new Point(462, 220),
            Size = new Size(130, 40),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.FromArgb(46, 139, 87),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        btnLaunch.Click += BtnLaunch_Click;

        btnCheckDriver = new Button
        {
            Text = "🔍 Проверить драйвер",
            Location = new Point(462, 280),
            Size = new Size(130, 32),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnCheckDriver.Click += BtnCheckDriver_Click;

        lblStatus = new Label
        {
            Text = "",
            Location = new Point(12, 380),
            Size = new Size(580, 40),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ForeColor = Color.Gray
        };

        Controls.AddRange(new Control[]
        {
            lblTitle, lstProxies,
            btnAdd, btnEdit, btnDelete,
            btnSave, btnLaunch, btnCheckDriver,
            lblStatus
        });

        lstProxies.DoubleClick += BtnEdit_Click;

        ResumeLayout(false);
        PerformLayout();
    }

    private void ResolveAppDirectory()
    {
        // Look for ProxyAPP folder relative to the executable or in parent directories
        var exeDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(exeDir, "..", "ProxyAPP"),
            Path.Combine(exeDir, "ProxyAPP"),
            Path.Combine(exeDir, "..", "..", "ProxyAPP"),
            exeDir
        };

        foreach (var dir in candidates)
        {
            var full = Path.GetFullPath(dir);
            if (File.Exists(Path.Combine(full, ConfigFileName)))
            {
                _appDir = full;
                _configPath = Path.Combine(full, ConfigFileName);
                return;
            }
        }

        // Folder not found automatically — ask the user to select it
        var result = MessageBox.Show(
            "Папка ProxyAPP с файлом app-config.json не найдена автоматически.\n\n" +
            "Укажите расположение папки вручную?",
            "Папка не найдена",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Выберите папку ProxyAPP (содержащую ProxiFyre.exe и app-config.json)",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _appDir = dialog.SelectedPath;
                _configPath = Path.Combine(_appDir, ConfigFileName);
                return;
            }
        }

        // Fallback: use ProxyAPP next to the exe directory
        _appDir = Path.GetFullPath(Path.Combine(exeDir, "..", "ProxyAPP"));
        _configPath = Path.Combine(_appDir, ConfigFileName);
    }

    private void LoadConfig()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                SetStatus($"Конфигурация загружена из: {_configPath}");
            }
            else
            {
                _config = new AppConfig();
                SetStatus($"Файл конфигурации не найден. Будет создан: {_configPath}");
            }
        }
        catch (Exception ex)
        {
            _config = new AppConfig();
            SetStatus($"Ошибка загрузки: {ex.Message}");
        }

        RefreshProxyList();
    }

    private void RefreshProxyList()
    {
        lstProxies.Items.Clear();
        foreach (var proxy in _config.Proxies)
        {
            lstProxies.Items.Add(proxy);
        }
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        var entry = new ProxyEntry();
        using var dialog = new ProxyEntryDialog(entry, GetEndpointCredentials());
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _config.Proxies.Add(entry);
            SyncCredentials(entry);
            RefreshProxyList();
            SetStatus("Прокси-конфигурация добавлена.");
        }
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (lstProxies.SelectedIndex < 0)
        {
            MessageBox.Show("Выберите конфигурацию для редактирования.", "Внимание",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var entry = _config.Proxies[lstProxies.SelectedIndex];
        using var dialog = new ProxyEntryDialog(entry, GetEndpointCredentials());
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            SyncCredentials(entry);
            RefreshProxyList();
            SetStatus("Прокси-конфигурация обновлена.");
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (lstProxies.SelectedIndex < 0)
        {
            MessageBox.Show("Выберите конфигурацию для удаления.", "Внимание",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show("Удалить выбранную конфигурацию?", "Подтверждение",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _config.Proxies.RemoveAt(lstProxies.SelectedIndex);
            RefreshProxyList();
            SetStatus("Прокси-конфигурация удалена.");
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            var dir = Path.GetDirectoryName(_configPath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(_config, options);
            File.WriteAllText(_configPath, json);

            SetStatus($"Конфигурация сохранена в: {_configPath}");
            MessageBox.Show("Конфигурация успешно сохранена!", "Сохранено",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnLaunch_Click(object? sender, EventArgs e)
    {
        var exePath = Path.Combine(_appDir, ProxiFyreExe);
        if (!File.Exists(exePath))
        {
            MessageBox.Show($"Файл {ProxiFyreExe} не найден по пути:\n{exePath}",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = _appDir,
                UseShellExecute = true
            };
            Process.Start(psi);
            SetStatus("ProxiFyre.exe запущен.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка запуска: {ex.Message}", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnCheckDriver_Click(object? sender, EventArgs e)
    {
        CheckDriverAndPromptInstall();
    }

    private void CheckDriverOnStartup()
    {
        if (!IsWindowsPacketFilterInstalled())
        {
            var result = MessageBox.Show(
                "Библиотека Windows Packet Filter не обнаружена.\n\n" +
                "Она необходима для работы ProxiFyre.\n" +
                "Установить сейчас?",
                "Требуется установка драйвера",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                InstallDriver();
            }
        }
    }

    private void CheckDriverAndPromptInstall()
    {
        if (IsWindowsPacketFilterInstalled())
        {
            SetStatus("✅ Windows Packet Filter установлен.");
            MessageBox.Show("Windows Packet Filter установлен и готов к работе.",
                "Драйвер найден", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            var result = MessageBox.Show(
                "Windows Packet Filter не установлен.\nУстановить сейчас?",
                "Драйвер не найден", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                InstallDriver();
            }
        }
    }

    private static bool IsWindowsPacketFilterInstalled()
    {
        try
        {
            // Check for WinpkFilter service in the registry
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\WinpkFilter");
            if (key != null)
                return true;

            // Also check for NDISRD driver
            using var key2 = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\NDISRD");
            if (key2 != null)
                return true;

            // Check installed products in registry
            using var uninstallKey = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (uninstallKey != null)
            {
                foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                {
                    using var subKey = uninstallKey.OpenSubKey(subKeyName);
                    var displayName = subKey?.GetValue("DisplayName") as string;
                    if (displayName != null &&
                        displayName.Contains("Windows Packet Filter", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        catch
        {
            // On non-Windows or without permissions, return false
            return false;
        }
    }

    private void InstallDriver()
    {
        // Look for the driver MSI in Driver folder (support version updates)
        var exeDir = AppContext.BaseDirectory;
        var driverDirs = new[]
        {
            Path.GetFullPath(Path.Combine(exeDir, "..", "Driver")),
            Path.GetFullPath(Path.Combine(exeDir, "Driver")),
            Path.GetFullPath(Path.Combine(exeDir, "..", "..", "Driver")),
        };

        string? msiPath = null;
        foreach (var dir in driverDirs)
        {
            if (!Directory.Exists(dir)) continue;

            // Try exact name first, then fall back to any MSI with "Packet.Filter" in the name
            var exact = Path.Combine(dir, DriverMsi);
            if (File.Exists(exact))
            {
                msiPath = exact;
                break;
            }

            var fallback = Directory.GetFiles(dir, "*.msi")
                .FirstOrDefault(f => f.Contains("Packet.Filter", StringComparison.OrdinalIgnoreCase));
            if (fallback != null)
            {
                msiPath = fallback;
                break;
            }
        }

        if (msiPath == null)
        {
            MessageBox.Show(
                $"Файл установщика драйвера не найден:\n{DriverMsi}\n\n" +
                "Убедитесь, что папка Driver находится рядом с программой.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "msiexec",
                Arguments = $"/i \"{msiPath}\"",
                UseShellExecute = true,
                Verb = "runas" // Run as administrator
            };
            var process = Process.Start(psi);
            process?.WaitForExit();

            if (IsWindowsPacketFilterInstalled())
            {
                SetStatus("✅ Драйвер успешно установлен.");
                MessageBox.Show("Windows Packet Filter успешно установлен!",
                    "Установка завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                SetStatus("⚠ Установка драйвера не завершена.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка установки драйвера: {ex.Message}",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private Dictionary<string, (string Username, string Password)> GetEndpointCredentials()
    {
        var map = new Dictionary<string, (string, string)>();
        foreach (var proxy in _config.Proxies)
        {
            if (!string.IsNullOrEmpty(proxy.Username) || !string.IsNullOrEmpty(proxy.Password))
            {
                map[proxy.Socks5ProxyEndpoint] = (proxy.Username, proxy.Password);
            }
        }
        return map;
    }

    private void SyncCredentials(ProxyEntry source)
    {
        foreach (var proxy in _config.Proxies)
        {
            if (proxy != source &&
                proxy.Socks5ProxyEndpoint == source.Socks5ProxyEndpoint)
            {
                proxy.Username = source.Username;
                proxy.Password = source.Password;
            }
        }
    }

    private void SetStatus(string message)
    {
        lblStatus.Text = message;
    }
}
