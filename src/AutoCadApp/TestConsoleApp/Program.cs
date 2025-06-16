using AcadCommon;
using AcadRun;
using System.Configuration;

namespace TestConsoleApp
{
    internal class Program
    {
        /// <summary>
        /// Run AutoCAD accoreconsole.exe to
        /// read/export blocks in multiple drawings
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // initialize configuration to provide parameters
            // for commands in AutoCAD .NET module to run 
            var config = SetRunConfig();
            FileUtil.ClearDwgExportFiles(config.AcadExportFolderPath);
            SaveConfig(config);

            // Create a list of drawings for command
            // in AutoCAD .NET module to read
            CreateDrawingFiles(config);

            // Run accoreconsole.exe
            var scrArgs = RunScript.GetArgs(config.AcadScriptFilePath);
            RunScript.RunScan(config.AcadProgramFolder, scrArgs, config.ScriptTimeOutMinute);
        }

        static AcadRunConfig SetRunConfig()
        {
            var settings = ConfigurationManager.AppSettings;
            var wf = settings["acadWorkingFolder"];
            if (String.IsNullOrEmpty(wf))
                wf = "ACADSCAN";
                
            // working folder can be created in
            // user's temp directory
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

        /// <summary>
        /// Export config file to working folder to provide
        /// information for commands in AutoCAD .NET module to run
        /// </summary>
        static void SaveConfig(AcadRunConfig config)
        {
            var xml = BlockData.SerializeToXml(config);
            FileUtil.SaveXml(xml,
                Path.Combine(
                    config.AcadExportFolderPath, 
                    ConfigurationManager.AppSettings["configXml"]!
                ));
        }

        /// <summary>
        /// Logic to generate a list of files
        /// and copy it to the working folder
        /// </summary>
        static void CreateDrawingFiles(AcadRunConfig config)
        {
            File.Copy("C:\\devgit\\Data\\drawings.txt", config.DwgFileListPath, true);
        }
    }
}
