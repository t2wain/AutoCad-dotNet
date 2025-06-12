using AcadCommon;
using System.Configuration;

namespace TestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SetRunConfig();
            var config = SetRunConfig();
            CreateDrawingFiles(config);
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

            var xml = BlockData.SerializeToXml(config);
            FileUtil.SaveXml(xml, Path.Combine(wf, settings["configXml"]!));

            return config;
        }

        static void CreateDrawingFiles(AcadRunConfig config)
        {
            File.Copy("C:\\devgit\\Data\\drawings.txt", config.DwgFileListPath, true);
        }
    }
}
