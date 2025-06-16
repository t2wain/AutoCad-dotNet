using AcadCommon;
using AcadTest;
using Autodesk.AutoCAD.Runtime;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace AutoCadLib
{
    /// <summary>
    /// AutoCAD .NET command
    /// </summary>
    public static class AcadCommands
    {
        [CommandMethod("RunTest")]
        public static void RunTest()
        {
            // Custom command cannot accept parameters.
            // Utilize configuration file instead to pass parameters.
            var config = GetConfig();
            if (config != null)
                Test.Run(config);
        }

        /// <summary>
        /// Get run configuration
        /// </summary>
        static AcadRunConfig GetConfig()
        {
            // get file path to the xml run configuration
            var fp = GetConfigFilePath();
            if (File.Exists(fp))
            {
                var xml = FileUtil.ReadFile(fp);
                var config = BlockData.ParseXmlToAcadConfig(xml);
                return config;
            }
            else return null;
        }

        /// <summary>
        /// Read xml app setting configuration file
        /// </summary>
        /// <returns>File path to the xml run configuration</returns>
        static string GetConfigFilePath()
        {
            var appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            var settings = appConfig.AppSettings.Settings;
            string wf = settings["acadWorkingFolder"].Value;
            if (string.IsNullOrEmpty(wf))
                wf = "ACADSCAN";

            // Ensure working folder created.
            // Working folder could be created n user's temp folder. 
            wf = FileUtil.CreateTempFolder(wf);

            return Path.Combine(wf, settings["configXml"].Value);
        }
    }
}
