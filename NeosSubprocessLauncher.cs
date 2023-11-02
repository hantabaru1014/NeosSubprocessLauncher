using System;
using System.Collections.Generic;
using ResoniteModLoader;
using FrooxEngine;
using System.IO;
using System.Diagnostics;

namespace NeosSubprocessLauncher
{
    public class NeosSubprocessLauncher : ResoniteMod
    {
        public override string Name => "NeosSubprocessLauncher";
        public override string Author => "hantabaru1014";
        public override string Version => "2.0.0";
        public override string Link => "https://github.com/hantabaru1014/NeosSubprocessLauncher";

        public static readonly string BaseDirPath = Path.Combine(Engine.Current.AppPath, "NeosSubprocessLauncher");
        public static readonly string LogsDirPath = Path.Combine(BaseDirPath, "Logs");
        public static readonly string ConfigPath = Path.Combine(BaseDirPath, "config.json");

        private NSLConfig _config = new NSLConfig();
        private List<LaunchedEntry> _launchedEntries = new List<LaunchedEntry>();

        public override void OnEngineInit()
        {
            Engine.Current.OnShutdown += Engine_OnShutdown;

            CreateDirectories();
            _config = LoadConfigOrDefault(ConfigPath);
            LaunchEntries(_config.Entries);
        }

        private void Engine_OnShutdown()
        {
            foreach (var entry in _launchedEntries)
            {
                if (entry.ProcessEntry.KillOnQuit && !entry.Process.HasExited)
                {
                    try
                    {
                        KillProcessAndChildrens(entry.Process.Id);
                    }
                    catch (Exception ex)
                    {
                        Error($"Failed to kill process: {ex.Message}");
                    }
                }
                if (entry.LogWriter != null)
                {
                    entry.LogWriter.Close();
                }
            }
        }

        private void LaunchEntries(NSLConfig.ProcessEntry[] entries)
        {
            foreach (var entry in entries)
            {
                if (!entry.IsEnabled) continue;

                var psi = new ProcessStartInfo();
                psi.FileName = entry.Path;
                psi.Arguments = entry.Arguments;
                if (!string.IsNullOrEmpty(entry.WorkingDirectory))
                {
                    psi.WorkingDirectory = entry.WorkingDirectory;
                }
                StreamWriter? logWriter = null;

                if (entry.NoGUI)
                {
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                }
                if (entry.UseLog)
                {
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    try
                    {
                        logWriter = new StreamWriter(
                            Path.Combine(LogsDirPath, $"{entry.Name} - {DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss")}.log"),
                            false,
                            System.Text.Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        Error($"Failed to open log file: {ex.Message}");
                    }
                }
                if (entry.Minimized)
                {
                    psi.WindowStyle = ProcessWindowStyle.Minimized;
                }

                try
                {
                    var p = Process.Start(psi);
                    var launchedEntry = new LaunchedEntry(entry, p, logWriter);
                    _launchedEntries.Add(launchedEntry);

                    if (entry.UseLog)
                    {
                        p.OutputDataReceived += (object sender, DataReceivedEventArgs e) => OutputDataReceived(launchedEntry, e);
                        p.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => ErrorDataReceived(launchedEntry, e);
                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();
                    }
                }
                catch (Exception ex)
                {
                    Error($"Failed to launch process: {ex.Message}");
                    continue;
                }

                Msg($"Launch: Name:{entry.Name}, Path:{entry.Path}, Args:{entry.Arguments}");
            }
        }

        private void ErrorDataReceived(LaunchedEntry sender, DataReceivedEventArgs e)
        {
            sender.LogWriter?.WriteLine($"{DateTime.Now.ToString("HH:mm:ss:fff")} [ERROR] {e.Data}");
        }

        private void OutputDataReceived(LaunchedEntry sender, DataReceivedEventArgs e)
        {
            sender.LogWriter?.WriteLine($"{DateTime.Now.ToString("HH:mm:ss:fff")} [INFO] {e.Data}");
        }

        private void CreateDirectories()
        {
            try
            {
                Directory.CreateDirectory(LogsDirPath);
            }
            catch (Exception ex)
            {
                Error($"Failed to create directory: {ex.Message}");
            }
        }

        private NSLConfig LoadConfigOrDefault(string path)
        {
            if (!File.Exists(path))
            {
                var sample = NSLConfig.GetSampleConfig();
                try
                {
                    using (var writer = new StreamWriter(path, false, System.Text.Encoding.UTF8))
                    {
                        writer.Write(sample.Serialize());
                    }
                }
                catch (Exception ex)
                {
                    Error($"Failed to save default config: {ex.Message}");
                }
                return sample;
            }
            try
            {
                using (var reader = new StreamReader(path, System.Text.Encoding.UTF8))
                {
                    return NSLConfig.Deserialize(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Error($"Failed to load config: {ex.Message}");
                return new NSLConfig();
            }
        }

        private static void KillProcessAndChildrens(int pid)
        {
            // HACK: 下記URLのやり方でやりたいが，System.Managementを上手く参照できない
            // https://stackoverflow.com/questions/30249873/process-kill-doesnt-seem-to-kill-the-process

            using (var killer = new Process())
            {
                killer.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "taskkill.exe");
                killer.StartInfo.Arguments = $"/f /t /pid {pid}";
                killer.StartInfo.UseShellExecute = false;
                killer.StartInfo.CreateNoWindow = true;
                killer.Start();
            }
        }
    }
}
