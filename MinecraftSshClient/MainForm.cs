using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinecraftSshClient
{
    public class MainForm : Form
    {
        private readonly TextBox txtHost = new() { Width = 260, Text = "2.56.99.112" };
        private readonly TextBox txtPort = new() { Width = 80, Text = "22" };
        private readonly TextBox txtUser = new() { Width = 180, Text = "mc-ctrl", ReadOnly = true };
        private readonly TextBox txtKeyPath = new() { Width = 420 };
        private readonly TextBox txtVersion = new() { Width = 140, Text = "1.20.4" };
        private readonly TextBox txtLog = new()
        {
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            ReadOnly = true,
            WordWrap = false,
            Width = 760,
            Height = 320
        };

        public MainForm()
        {
            Text = "Minecraft SSH Client";
            Width = 860;
            Height = 620;
            StartPosition = FormStartPosition.CenterScreen;

            txtKeyPath.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".ssh",
                "id_ed25519"
            );

            var lblHost = new Label { Text = "Server", AutoSize = true };
            var lblPort = new Label { Text = "Port", AutoSize = true };
            var lblUser = new Label { Text = "User", AutoSize = true };
            var lblKey = new Label { Text = "Private Key", AutoSize = true };
            var lblVersion = new Label { Text = "Minecraft Version", AutoSize = true };

            var btnBrowse = new Button { Text = "Browse..." };
            btnBrowse.Click += (_, _) => BrowseKey();

            var btnStart = new Button { Text = "Start", Width = 120 };
            var btnStop = new Button { Text = "Stop", Width = 120 };
            var btnVersion = new Button { Text = "Set Version", Width = 120 };

            btnStart.Click += async (_, _) => await RunCommandAsync("start");
            btnStop.Click += async (_, _) => await RunCommandAsync("stop");
            btnVersion.Click += async (_, _) => await RunCommandAsync($"version {txtVersion.Text.Trim()}");

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 7,
                Padding = new Padding(12),
                AutoSize = true
            };

            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            grid.Controls.Add(lblHost, 0, 0);
            grid.Controls.Add(txtHost, 1, 0);

            grid.Controls.Add(lblPort, 0, 1);
            grid.Controls.Add(txtPort, 1, 1);

            grid.Controls.Add(lblUser, 0, 2);
            grid.Controls.Add(txtUser, 1, 2);

            grid.Controls.Add(lblKey, 0, 3);
            grid.Controls.Add(txtKeyPath, 1, 3);
            grid.Controls.Add(btnBrowse, 2, 3);

            grid.Controls.Add(lblVersion, 0, 4);
            grid.Controls.Add(txtVersion, 1, 4);

            var buttonFlow = new FlowLayoutPanel { AutoSize = true };
            buttonFlow.Controls.Add(btnStart);
            buttonFlow.Controls.Add(btnStop);
            buttonFlow.Controls.Add(btnVersion);
            grid.Controls.Add(buttonFlow, 1, 5);

            grid.Controls.Add(txtLog, 0, 6);
            grid.SetColumnSpan(txtLog, 4);

            Controls.Add(grid);
        }

        private void BrowseKey()
        {
            using var dialog = new OpenFileDialog();
            dialog.Title = "Choose private key";
            dialog.Filter = "All files|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtKeyPath.Text = dialog.FileName;
            }
        }

        private async Task RunCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text) ||
                string.IsNullOrWhiteSpace(txtPort.Text) ||
                string.IsNullOrWhiteSpace(txtKeyPath.Text))
            {
                MessageBox.Show("Please fill in server, port, and key path.");
                return;
            }

            if (!int.TryParse(txtPort.Text.Trim(), out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Invalid SSH port.");
                return;
            }

            if (!File.Exists(txtKeyPath.Text))
            {
                MessageBox.Show("Private key file not found.");
                return;
            }

            if (command.StartsWith("version ", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(txtVersion.Text))
            {
                MessageBox.Show("Please enter a Minecraft version.");
                return;
            }

            AppendLog($"> {command}");

            var sshPath = ResolveSshPath();
            if (sshPath == null)
            {
                AppendLog("ssh.exe not found. Install the OpenSSH Client feature on Windows.");
                AppendLog("");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = sshPath,
                Arguments = $"-p {port} -i \"{txtKeyPath.Text}\" -o BatchMode=yes -o StrictHostKeyChecking=accept-new {txtUser.Text}@{txtHost.Text} \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = new Process { StartInfo = psi };
                var output = new StringBuilder();
                var error = new StringBuilder();

                process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
                process.ErrorDataReceived += (_, e) => { if (e.Data != null) error.AppendLine(e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                if (output.Length > 0) AppendLog(output.ToString().TrimEnd());
                if (error.Length > 0) AppendLog(error.ToString().TrimEnd());

                AppendLog($"Exit code: {process.ExitCode}");
                AppendLog("");
            }
            catch (Exception ex)
            {
                AppendLog($"Error: {ex.Message}");
                AppendLog("");
            }
        }

        private static string? ResolveSshPath()
        {
            var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var candidate = Path.Combine(windowsDir, "System32", "OpenSSH", "ssh.exe");
            if (File.Exists(candidate)) return candidate;

            return "ssh.exe";
        }

        private void AppendLog(string text)
        {
            txtLog.AppendText(text + Environment.NewLine);
        }
    }
}
