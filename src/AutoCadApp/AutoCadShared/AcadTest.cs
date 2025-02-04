using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace AutoCadShared
{
    public static class AcadTest
    {
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

        public static void Write<T>(Document doc, List<(Entity entity, T xdValue)> entities, string appName) 
        { 
            doc.LockDocument();
            using (var tran = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    var lst = entities.Select(e => e.entity);
                    AcadWrite.WriteEntities(doc, lst, tran);
                    AcadWrite.AddApp(doc, appName, tran);
                    foreach (var ent in entities)
                    {
                        AcadWrite.AddXDataToEntity(doc, ent.entity, appName, ent.xdValue, tran);
                    }
                    tran.Commit();
                }
                catch
                {
                    tran.Abort();
                    throw;
                }
            }
        }
    }
}
