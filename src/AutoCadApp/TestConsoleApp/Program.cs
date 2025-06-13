using AcadCommon;
using AcadRun;
using System.Configuration;

namespace TestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = SetRunConfig();
            FileUtil.ClearDwgExportFiles(config.AcadExportFolderPath);
            SaveConfig(config);
            CreateDrawingFiles(config);

            var scrArgs = RunScript.GetArgs(config.AcadScriptFilePath);
            RunScript.RunScan(config.AcadProgramFolder, scrArgs, config.ScriptTimeOutMinute);
        }

        static AcadRunConfig SetRunConfig()
        {
            var settings = ConfigurationManager.AppSettings;
            var wf = settings["acadWorkingFolder"];
            if (String.IsNullOrEmpty(wf))
                wf = "ACADSCAN";
                
            wf = FileUtil.CreateTempFolder(wf);

            var config = new AcadRunConfig
            {
                AcadProgramFolder = settings["acadProgramFolder"],
                AcadScriptFilePath = settings["acadScriptFilePath"],
                AcadExportFolderPath = wf,
                RunCommand = Int32.Parse(settings["runCommand"] ?? "0"),
                ScriptTimeOutMinute = Int32.Parse(settings["scriptTimeOutMinute"] ?? "2"),
                DwgFileListPath = Path.Combine(wf, settings["dwgFileList"]!),
            };

            return config;
        }

        static void SaveConfig(AcadRunConfig config)
        {
            var xml = BlockData.SerializeToXml(config);
            FileUtil.SaveXml(xml,
                Path.Combine(
                    config.AcadExportFolderPath, 
                    ConfigurationManager.AppSettings["configXml"]!
                ));
        }

        static void CreateDrawingFiles(AcadRunConfig config)
        {
            File.Copy("C:\\devgit\\Data\\drawings.txt", config.DwgFileListPath, true);
        }
    }
}
