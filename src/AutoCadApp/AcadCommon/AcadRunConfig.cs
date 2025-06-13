namespace AcadCommon
{
    public class AcadRunConfig
    {
        /// <summary>
        /// Directory where AutoCAD installed
        /// </summary>
        public string AcadProgramFolder { get; set; }
        /// <summary>
        /// Script file to run by accoreconsole.exe
        /// </summary>
        public string AcadScriptFilePath { get; set; }
        /// <summary>
        /// Working folder for accoreconsole.exe
        /// </summary>
        public string AcadExportFolderPath { get; set; }
        public int ScriptTimeOutMinute { get; set; } = 1;
        /// <summary>
        /// Task to run by the .NET AutoCAD module
        /// </summary>
        public int RunCommand { get; set; }
        /// <summary>
        /// File with a list of drawings for process by accoreconsole.exe
        /// </summary>
        public string DwgFileListPath { get; set; }
    }
}
