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
            var lst = AcadRead.LoadBlocks(doc, BlockEnum.Raceway.ToString());
            lst = AcadRead.LoadBlocks(doc, BlockEnum.Drop.ToString());
            lst = AcadRead.LoadBlocks(doc, BlockEnum.EquipNode.ToString());
            lst = AcadRead.LoadBlocks(doc, BlockEnum.RwNode.ToString());

            lst = AcadRead.LoadBlocks(doc, new[] 
            {
                BlockEnum.Raceway.ToString(),
                BlockEnum.Drop.ToString(),
                BlockEnum.EquipNode.ToString(),
                BlockEnum.RwNode.ToString()
            });
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
                var sel = AcadUtil.GetBlockSelection(doc, new string[] { BlockEnum.Raceway.ToString() });
                IEnumerable<BlockReference> lst = db.GetEntities<BlockReference>(sel).ToList();

                sel = AcadUtil.GetBlockSelection(doc, new string[] { BlockEnum.Drop.ToString() });
                lst = db.GetEntities<BlockReference>(sel).ToList();

                sel = AcadUtil.GetBlockSelection(doc, new string[] { BlockEnum.EquipNode.ToString() });
                lst = db.GetEntities<BlockReference>(sel).ToList();

                sel = AcadUtil.GetBlockSelection(doc, new string[] { BlockEnum.RwNode.ToString() });
                lst = db.GetEntities<BlockReference>(sel).ToList();

                sel = AcadUtil.GetBlockSelection(doc, new string[] 
                {
                    BlockEnum.Raceway.ToString(),
                    BlockEnum.Drop.ToString(),
                    BlockEnum.EquipNode.ToString(),
                    BlockEnum.RwNode.ToString()
                });
                lst = db.GetEntities<BlockReference>(sel).ToList();
            }
        }


    }
}
