using AcadCommon;
using AcadCommon.DTO;
using AutoCadShared;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using PlotRaceway;
using RacewayDataLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AcadTest
{
    public static class Test
    {
        public static void Run(AcadRunConfig config)
        {
            // During debug, change this value
            // at break point to run other test
            var testNo = config.RunCommand;
            switch (testNo)
            {
                case 1:
                    Read2(ActiveDocument);
                    break;
                case 2:
                    AcadRead.ReadObjectCollection(ActiveDocument);
                    break;
                case 3:
                    RWRead.Read(ActiveDocument);
                    break;
                case 4:
                    RWRead.Read2(ActiveDocument);
                    break;
                case 5:
                    PlotRW(ActiveDocument);
                    break;
                case 6:
                    ReadBlockInFile(config);
                    break;
                default:
                    Read(ActiveDocument);
                    break;
            }
        }

        public static Document ActiveDocument =>
            Application.DocumentManager.MdiActiveDocument;

        /// <summary>
        /// Read data of entity
        /// </summary>
        /// <param name="doc"></param>
        public static void Read(Document doc)
        {
            var lst = AcadRead.LoadPolyLines(doc);
            lst = AcadRead.LoadLines(doc);
            lst = (dynamic)AcadRead.LoadBlocks(doc);
            lst = AcadRead.LoadMText(doc);
        }

        /// <summary>
        /// Select entity using filtering
        /// </summary>
        /// <param name="doc"></param>
        public static void Read2(Document doc)
        {
            using (var adoc = new AcadDocument(doc))
            using (var db = adoc.GetDatabase())
            {
                IEnumerable<Entity> lst = adoc.GetPolylines().ToList();
                lst = adoc.Getlines().ToList();
                lst = adoc.GetBlocks().ToList();
                lst = adoc.GetEntities<MText>(DxfEntity.MTEXT).ToList();
            }
        }

        /// <summary>
        /// Steps to write data
        /// </summary>
        public static void Write<T>(Document doc,
            IEnumerable<(Entity entity, T xdValue)> entities, string appName)
        {
            doc.LockDocument();
            using (var adoc = new AcadDocument(doc))
            using (var db = adoc.GetDatabase())
            {
                try
                {
                    AcadWrite.AddApp(db, appName);
                    AcadWrite.WriteEntitiesWithXData(db, entities, appName);
                    db.AcadTran.Commit();
                }
                catch
                {
                    db.AcadTran.Abort();
                    throw;
                }
            }
        }

        public static void PlotRW(Document doc)
        {
            var config = new DataConfig();
            var repo = NetworkDB.LoadData(config);
            var lstRw = repo.Raceways.Where(rw => rw.Systems.Contains(6)).ToList();
            RWWrite.WriteNetwork(doc, lstRw);
        }

        public static void ReadBlockInFile(AcadRunConfig config)
        {
            if (!File.Exists(config.DwgFileListPath))
                return;

            var lstFileName = File.ReadLines(config.DwgFileListPath)
                .Where(f => File.Exists(f));
            var regEx = new Regex("mto_*", RegexOptions.IgnoreCase);
            foreach (var filePath in lstFileName)
            {
                var dwg = AcadRead.ReadAllBlocks(filePath, regEx);
                if (Directory.Exists(config.AcadExportFolderPath))
                {
                    var xml = BlockData.SerializeToXml(dwg);
                    var xf = Path.Combine(config.AcadExportFolderPath, $"{dwg.FileName}.xml");
                    FileUtil.SaveXml(xml, xf);
                }
            }
        }
    }
}
