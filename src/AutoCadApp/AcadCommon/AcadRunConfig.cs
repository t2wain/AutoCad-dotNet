namespace AcadCommon
{
    public class AcadRunConfig
    {
        public string AcadProgramFolder { get; set; }
        public string AcadScriptFilePath { get; set; }
        public string AcadExportFolderPath { get; set; }
        public int ScriptTimeOutMinute { get; set; } = 1;
        public int RunCommand { get; set; }
        public string DwgFileListPath { get; set; }
    }
}
