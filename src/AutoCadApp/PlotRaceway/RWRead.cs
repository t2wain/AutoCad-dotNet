using AutoCadShared;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace PlotRaceway
{
    public static class RWRead
    {
        /// <summary>
        /// Read data of entity
        /// </summary>
        /// <param name="doc"></param>
        public static void Read(Document doc)
        {
            var lst = AcadRead.LoadBlocks(doc, "Raceway");
            lst = AcadRead.LoadBlocks(doc, "Drop");
            lst = AcadRead.LoadBlocks(doc, "EquipNode");
            lst = AcadRead.LoadBlocks(doc, "RwNode");

            lst = AcadRead.LoadBlocks(doc, new[] { "Raceway", "Drop", "EquipNode", "RwNode" });
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
                var sel = AcadUtil.GetBlockSelection(doc, new string[] { "Raceway" });
                IEnumerable<Entity> lst = db.GetEntities<BlockReference>(sel).ToList();

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


    }
}
