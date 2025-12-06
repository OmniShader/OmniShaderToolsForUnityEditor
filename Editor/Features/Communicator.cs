//  Copyright (c) 2025-present amlovey
// 
using System.Diagnostics;
using System.IO;
using OmniShader.Common;

namespace OmniShader.Editor
{
    public class Communicator
    {
        public static string Execute(string command, params string[] arguments)
        {
            var exe = GetOSExePath();
            if (!File.Exists(exe))
            {
                return null;
            }

            var startInfo = new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = string.Format("{0} {1}", command, string.Join(" ", arguments)),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(exe)
            };

            OSUtils.Log(string.Format("{0} {1}", command, string.Join(" ", arguments)));

            var process = Process.Start(startInfo);
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        public static void TryEnsureExecuableIfNeeds()
        {
#if UNITY_EDITOR_WIN
            return;
#else
            var exe = GetOSExePath();
            if (!File.Exists(exe))
            {
                return;
            }

            var startInfo = new ProcessStartInfo()
            {
                FileName = "chmod",
                Arguments = string.Format("+x {0}", exe),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(exe)
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();
#endif
        }

        private static string GetOSExePath()
        {
            var folder = Unpacker.GetOmniShaderFolder();
            var platform = "mac";
            var exeName = "os";
#if UNITY_STANDALONE_WIN
            platform = "win";
            exeName = "os.exe";
#endif
            return Path.Combine(folder, platform, exeName);
        }
    }
}