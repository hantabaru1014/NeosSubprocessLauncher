using System.Diagnostics;
using System.IO;

namespace NeosSubprocessLauncher
{
    public class LaunchedEntry
    {
        public NSLConfig.ProcessEntry ProcessEntry { get; set; }
        public Process Process { get; set; }
        public StreamWriter? LogWriter { get; set; }

        public LaunchedEntry(NSLConfig.ProcessEntry processEntry, Process process, StreamWriter? logWriter = null)
        {
            ProcessEntry = processEntry;
            Process = process;
            LogWriter = logWriter;
        }
    }
}
