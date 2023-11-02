using System;
using Newtonsoft.Json;

namespace NeosSubprocessLauncher
{
    public class NSLConfig
    {
        public class ProcessEntry
        {
            /// <summary>
            /// この設定を使用するかどうか
            /// </summary>
            public bool IsEnabled { get; set; } = true;
            /// <summary>
            /// 管理名．空の場合はプロセスのexe名を使用．
            /// </summary>
            public string Name { get; set; } = string.Empty;
            /// <summary>
            /// プロセスのパス
            /// </summary>
            public string Path { get; set; } = string.Empty;
            /// <summary>
            /// 起動引数
            /// </summary>
            public string Arguments { get; set; } = string.Empty;
            /// <summary>
            /// 作業ディレクトリ. null or emptyでデフォルト．
            /// </summary>
            public string? WorkingDirectory { get; set; } = null;
            /// <summary>
            /// GUIがある場合に最小化状態で起動する
            /// </summary>
            public bool Minimized { get; set; }
            /// <summary>
            /// CUIプログラムである場合にウィンドウを表示しない
            /// </summary>
            public bool NoGUI { get; set; }
            /// <summary>
            /// 標準出力をログとして記録する
            /// </summary>
            public bool UseLog { get; set; }
            /// <summary>
            /// neos終了時にプロセスを終了するか
            /// </summary>
            public bool KillOnQuit { get; set; }
        }

        public ProcessEntry[] Entries { get; set; } = new ProcessEntry[0];

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static NSLConfig Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<NSLConfig>(json) ?? throw new ArgumentException("Invalid json");
        }

        public static NSLConfig GetSampleConfig()
        {
            var config = new NSLConfig
            {
                Entries = new ProcessEntry[2]
            {
                new() {
                    IsEnabled = true,
                    Name = "CUI Sample",
                    Path = "C:\\Windows\\system32\\cmd.exe",
                    Arguments = "/c dir",
                    WorkingDirectory = NeosSubprocessLauncher.BaseDirPath,
                    NoGUI = true,
                    UseLog = true,
                    KillOnQuit = true,
                },
                new() {
                    IsEnabled = true,
                    Name = "GUI Sample",
                    Path = "C:\\Windows\\system32\\notepad.exe",
                    Minimized = true,
                }
            }
            };
            return config;
        }
    }
}
