using AcadCommon;
using AcadTest;
using Autodesk.AutoCAD.Runtime;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace AutoCadLib
{
    public static class AcadCommands
    {
        [CommandMethod("RunTest")]
        public static void RunTest()
        {
            var config = GetConfig();
            if (config != null)
                Test.Run(config);
        }

        static AcadRunConfig GetConfig()
        {
            var fp = GetConfigFilePath();
            if (File.Exists(fp))
            {
                var xml = FileUtil.ReadFile(fp);
                var config = BlockData.ParseXmlToAcadConfig(xml);
                return config;
            }
            else return null;
        }

        static string GetConfigFilePath()
        {
            var appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            var settings = appConfig.AppSettings.Settings;
            string wf = settings["acadWorkingFolder"].Value;
            if (string.IsNullOrEmpty(wf))
                wf = "ACADSCAN";
            wf = FileUtil.CreateTempFolder(wf);
            return Path.Combine(wf, settings["configXml"].Value);
        }
    }
}
