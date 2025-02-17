using System.IO;

namespace RacewayDataLib
{
    /// <summary>
    /// Provide configuration data of the raceway data file path
    /// </summary>
    public class DataConfig
    {
        /// <summary>
        /// Initialize default file name and path (see source code)
        /// </summary>
        public DataConfig() : this("C:\\devgit\\Data\\Raceway\\CSV\\J6327") { }

        public DataConfig(string folderPath)
        {
            this.DataFolderPath = folderPath;
            this.RacewayFile = Path.Combine(DataFolderPath, "raceways.csv");
            this.NodeFile = Path.Combine(DataFolderPath, "nodes.csv");
            this.SegSystemFile = Path.Combine(DataFolderPath, "seg_systems.csv");
            this.CableFile = Path.Combine(DataFolderPath, "cables.csv");
            this.RouteFile = Path.Combine(DataFolderPath, "routes.csv");
            this.PreferRouteFile = Path.Combine(DataFolderPath, "prefer_routes.csv");
        }

        public string DataFolderPath { get; set; }
        public string RacewayFile { get; set; }
        public string NodeFile { get; set; } 
        public string SegSystemFile { get; set; } 
        public string CableFile { get; set; } 
        public string RouteFile { get; set; } 
        public string PreferRouteFile { get; set; }  

    }
}
