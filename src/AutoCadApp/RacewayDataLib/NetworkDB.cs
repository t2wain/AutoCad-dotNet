using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RacewayDataLib
{
    /// <summary>
    /// Read and save raceway data from and to CSV files
    /// </summary>
    public static class NetworkDB
    {
        public class NetworkData
        {
            public IEnumerable<Raceway> Raceways { get; set; } = new List<Raceway>();
            public IEnumerable<Cable> Cables { get; set; } = new List<Cable>();
            public IEnumerable<Route> Routes { get; set; } = new List<Route>();
            public IEnumerable<SegSystem> Systems { get; set; } = new List<SegSystem>();
            public IEnumerable<Node> Nodes { get; set; } = new List<Node>();
        }

        #region Filter raceways

        public static double CalcLength(this Raceway r) =>
            Math.Sqrt(Math.Pow(r.FromNode.X - r.ToNode.X, 2) 
                + Math.Pow(r.FromNode.Y - r.ToNode.Y, 2) 
                + Math.Pow(r.FromNode.Z - r.ToNode.Z, 2));

        /// <summary>
        /// Filter raceway
        /// </summary>
        public static IEnumerable<Raceway> GetTray(this IEnumerable<Raceway> raceways) =>
            raceways.Where(r => r.Type == "TRAY");

        public static IEnumerable<Raceway> GetJump(this IEnumerable<Raceway> raceways) =>
            raceways.Where(r => r.Type == "SPECIAL" 
                && r.IsDrop == 0);

        public static IEnumerable<Raceway> GetDrop(this IEnumerable<Raceway> raceways) =>
            raceways.Where(r => r.Type == "SPECIAL" 
                && r.IsDrop == 1);

        /// <summary>
        /// Filter raceway
        /// </summary>
        public static IEnumerable<Raceway> SelectSystem(this IEnumerable<Raceway> raceways, int sysId) =>
            raceways.Where(r => sysId == 0 || r.Systems.Contains(sysId));

        #endregion

        #region Filter nodes

        public static IEnumerable<Node> GetEquipNodes(this IEnumerable<Node> nodes) =>
            nodes.Where(n => n.NodeType == "EQUIPMENT").ToList();

        public static IEnumerable<Node> GetEquipNodes(this IEnumerable<Cable> cables, IEnumerable<Node> nodeLookup) =>
            nodeLookup.GetNodes(cables.GetNodeIds());

        public static IEnumerable<Node> GetNodes(this IEnumerable<Raceway> raceways)
        {
            var lstNodes = raceways.SelectMany<Raceway, Node>(r => new Node[] { r.FromNode, r.ToNode });
            return lstNodes.Aggregate(new Dictionary<int, Node>(), (d, n) =>
            {
                if (!d.ContainsKey(n.ID))
                    d.Add(n.ID, n);
                return d;
            }).Values;
        }

        static IEnumerable<int> GetNodeIds(this IEnumerable<Cable> cables) =>
            cables.SelectMany<Cable, int>(c => new int[] { c.FromNodeID, c.ToNodeID }).Distinct();

        static IEnumerable<Node> GetNodes(this IEnumerable<Node> nodeLookup, IEnumerable<int> nodeIds) =>
            nodeLookup.Join(nodeIds, n => n.ID, id => id, (n, id) => n).ToList();

        #endregion

        #region Read Data from CSV files

        /// <summary>
        /// Load a complete set of raceway data from CSV files
        /// </summary>
        public static NetworkData LoadData(DataConfig files)
        {
            var data = new NetworkData
            { 
                Raceways = ReadRaceways(files.RacewayFile),
                Cables = ReadCables(files.CableFile),
                Routes = ReadRoutes(files.RouteFile),
                Systems = ReadSegSystems(files.SegSystemFile),
                Nodes = ReadNodes(files.NodeFile),
            };

            var dnode = data.Nodes.ToDictionary(n => n.ID);
            foreach (var rw in data.Raceways)
            {
                if (dnode.TryGetValue(rw.FromNodeID, out var n))
                    rw.FromNode = n;
                if (dnode.TryGetValue(rw.ToNodeID, out n))
                    rw.ToNode = n;
            }

            return data;
        }

        public static IEnumerable<Raceway> ReadRaceways(string path)
        {
            using (TextReader rd = new StreamReader(path))
            using (var csv = new CsvReader(rd, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<RacewayMap>();
                csv.Context.Configuration.HasHeaderRecord = true;
                return csv.GetRecords<Raceway>().ToList();
            }
        }

        public static IEnumerable<Cable> ReadCables(string path)
        {
            using (TextReader rd = new StreamReader(path))
            using (var csv = new CsvReader(rd, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<CableMap>();
                csv.Context.Configuration.HasHeaderRecord = true;
                return csv.GetRecords<Cable>().ToList();
            }
        }

        public static IEnumerable<Route> ReadRoutes(string path)
        {
            using (TextReader rd = new StreamReader(path))
            using (var csv = new CsvReader(rd, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<RouteMap>();
                csv.Context.Configuration.HasHeaderRecord = true;
                return csv.GetRecords<Route>().ToList();
            }
        }

        public static IEnumerable<SegSystem> ReadSegSystems(string path)
        {
            using (TextReader rd = new StreamReader(path))
            using (var csv = new CsvReader(rd, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<SegSystemMap>();
                csv.Context.Configuration.HasHeaderRecord = true;
                return csv.GetRecords<SegSystem>().ToList();
            }
        }

        public static IEnumerable<Node> ReadNodes(string path)
        {
            using (TextReader rd = new StreamReader(path))
            using (var csv = new CsvReader(rd, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<NodeMap>();
                csv.Context.Configuration.HasHeaderRecord = true;
                return csv.GetRecords<Node>().ToList();
            }
        }

        #endregion

        #region Save Data to CSV files

        public static void SaveRaceways(string path, IEnumerable<Raceway> data)
        {
            SaveData(path, data, typeof(RacewayMap));
        }

        public static void SaveCables(string path, IEnumerable<Cable> data)
        {
            SaveData(path, data, typeof(CableMap));
        }

        public static void SaveRoutes(string path, IEnumerable<Route> data)
        {
            SaveData(path, data, typeof(RouteMap));
        }

        public static void SaveSegSystems(string path, IEnumerable<SegSystem> data)
        {
            SaveData(path, data, typeof(SegSystemMap));
        }

        public static void SaveNodes(string path, IEnumerable<Node> data)
        {
            SaveData(path, data, typeof(NodeMap));
        }

        static void SaveData(string path, System.Collections.IEnumerable data, Type classMap)
        {
            using (TextWriter rd = new StreamWriter(path))
            using (var csv = new CsvWriter(rd, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap(classMap);
                csv.Context.Configuration.HasHeaderRecord = true;
                csv.WriteRecords(data);
            }
        }

        #endregion
    }
}
