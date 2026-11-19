using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

internal static class Setup
{
    private const string Version = "1.0.0";
    private const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\CuePane";

    [STAThread]
    private static int Main(string[] args)
    {
        try {
            if (args.Length == 2 && args[0] == "--extract") {
                ExtractPayload(args[1]);
                return 0;
            }
            if (MessageBox.Show("Install CuePane for this Windows account?", "CuePane Setup",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK) return 0;

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs", "CuePane");
            ExtractPayload(directory);
            string application = Path.Combine(directory, "CuePane.exe");
            string shortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "CuePane.lnk");
            dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
            dynamic link = shell.CreateShortcut(shortcut);
            link.TargetPath = application;
            link.WorkingDirectory = directory;
            link.Save();

            string powershell = Path.Combine(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe");
            string uninstall = Path.Combine(directory, "Uninstall.ps1");
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(UninstallKey)) {
                key.SetValue("DisplayName", "CuePane");
                key.SetValue("DisplayVersion", Version);
                key.SetValue("DisplayIcon", application);
                key.SetValue("InstallLocation", directory);
                key.SetValue("UninstallString", Quote(powershell) + " -NoProfile -ExecutionPolicy Bypass -File " + Quote(uninstall));
                key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            }
            MessageBox.Show("CuePane is installed. Open it from the Start menu.", "CuePane Setup",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return 0;
        } catch (Exception error) {
            if (args.Length == 0)
                MessageBox.Show(error.Message, "CuePane Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    private static string Quote(string value) { return "\"" + value + "\""; }

    private static void ExtractPayload(string directory)
    {
        Directory.CreateDirectory(directory);
        Extract("CuePane.exe", directory);
        Extract("Uninstall.ps1", directory);
    }

    private static void Extract(string name, string directory)
    {
        string target = Path.Combine(directory, name);
        string pending = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".tmp");
        try {
            using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            using (var output = new FileStream(pending, FileMode.CreateNew, FileAccess.Write)) {
                if (input == null) throw new InvalidDataException("Missing installer payload: " + name);
                input.CopyTo(output);
            }
            if (File.Exists(target)) File.Replace(pending, target, null);
            else File.Move(pending, target);
        } finally {
            File.Delete(pending);
        }
    }
}
