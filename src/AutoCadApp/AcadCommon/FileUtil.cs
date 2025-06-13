using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AcadCommon
{
    public static class FileUtil
    {

        public static string CreateTempFolder(string folderName)
        {
            if (folderName.Contains("\\"))
                return folderName;

            var fp = Path.Combine(Path.GetTempPath(), folderName);
            if (!Directory.Exists(fp))
                Directory.CreateDirectory(fp);
            return fp;
        }

        public static IEnumerable<FileInfo> GetDwgExportFiles(string exportFolderPath)
        {
            if (Directory.Exists(exportFolderPath))
            {
                return Directory.GetFiles(exportFolderPath)
                    .Select(fi => new FileInfo(fi))
                    .Where(fi => fi.Extension == ".xml" | fi.Extension == ".txt")
                    .ToList();
            }
            return Enumerable.Empty<FileInfo>();
        }

        public static void ClearDwgExportFiles(string exportFolderPath)
        {
            foreach (var fi in GetDwgExportFiles(exportFolderPath))
                fi.Delete();
        }

        public static string GetDwgExportFilePath(string dwgFilePath, string exportFolderPath)
        {
            var fi = new FileInfo(dwgFilePath);
            var name = fi.Name.Replace(fi.Extension, ".xml");
            return Path.Combine(exportFolderPath, name);
        }

        public static void SaveXml(string data, string fileName) =>
            File.WriteAllText(fileName, data, System.Text.Encoding.UTF8);

        public static string ReadFile(string fileName) => File.ReadAllText(fileName);

    }
}
