using System.Diagnostics;
using System.IO;
using System;

namespace AcadRun
{
    public static class RunScript
    {
        public static string AppName = "accoreconsole.exe";

        public static string GetArgs(string scriptPath, string filePath = null)
        {
            var arg = $"/s {scriptPath}";
            if (!string.IsNullOrEmpty(filePath) )
                arg = $"/i {filePath} /readonly {arg}";
            return arg ;
        }

        public static (int ExitCode, string Ouptput) RunScan(string cadFolderPath, string args, int timeOutMinute)
        {
            var exeName = Path.Combine(cadFolderPath, AppName);
            return CaptureConsoleAppOutput(exeName, args,
                Convert.ToInt32(TimeSpan.FromMinutes(timeOutMinute).TotalMilliseconds));
        }

        static (int ExitCode, string Ouptput) CaptureConsoleAppOutput(string exeName, string arguments, int timeoutMilliseconds)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = exeName;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                try
                {
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();

                    bool exited = process.WaitForExit(timeoutMilliseconds);
                    int exitCode = 0;
                    if (exited)
                    {
                        exitCode = process.ExitCode;
                    }
                    else
                    {
                        exitCode = -1;
                    }

                    return (exitCode, output);
                }
                catch (Exception e)
                {
                    return (1, e.Message.Substring(0, 25));
                }

            }
        }
    }
}
