using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace AutoCadShared
{
    public static class AcadTest
    {
        public static Document ActiveDocument => 
            Application.DocumentManager.MdiActiveDocument;

        public static void Read(Document doc)
        {
            var lst = AcadRead.LoadPolyLines(doc);
            lst = AcadRead.LoadLines(doc);
            lst = AcadRead.LoadBlocks(doc);
            lst = AcadRead.LoadMText(doc);

            lst = AcadRead.LoadBlocks(doc, "Raceway");
            lst = AcadRead.LoadBlocks(doc, "Drop");
            lst = AcadRead.LoadBlocks(doc, "EquipNode");
            lst = AcadRead.LoadBlocks(doc, "RwNode");

            lst = AcadRead.LoadBlocks(doc, new[] { "Raceway", "Drop", "EquipNode", "RwNode" });
        }

        public static void Read2(Document doc)
        {
            using (var adoc = new AcadDocument(doc))
            using (var db = adoc.GetDatabase())
            {
                IEnumerable<Entity> lst = adoc.GetPolylines().ToList();
                lst = adoc.Getlines().ToList();
                lst = adoc.GetBlocks().ToList();
                lst = adoc.GetEntities<MText>(DxfEntity.MTEXT).ToList();

                var sel = AcadUtil.GetBlockSelection(doc, new string[] { "Raceway" });
                lst = db.GetEntities<BlockReference>(sel).ToList();

                sel = AcadUtil.GetBlockSelection(doc, new string[] { "Drop" });
                lst = db.GetEntities<BlockReference>(sel).ToList();

                sel = AcadUtil.GetBlockSelection(doc, new string[] { "EquipNode" });
                lst = db.GetEntities<BlockReference>(sel).ToList();

                sel = AcadUtil.GetBlockSelection(doc, new string[] { "RwNode" });
                lst = db.GetEntities<BlockReference>(sel).ToList();

                sel = AcadUtil.GetBlockSelection(doc, new string[] { "Raceway", "Drop", "EquipNode", "RwNode" });
                lst = db.GetEntities<BlockReference>(sel).ToList();
            }
        }

        public static void Write<T>(Document doc, IEnumerable<(Entity entity, T xdValue)> entities, string appName) 
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
    }
}
