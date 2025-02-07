using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCadShared
{
    /// <summary>
    /// Learn to read data from entities in drawing
    /// </summary>
    public static class AcadRead
    {

        #region Read Entity

        /// <summary>
        /// Read data from all blocks in the drawing
        /// </summary>
        public static List<dynamic> LoadBlocks(Document doc)
        {
            using (var acDoc = new AcadDocument(doc))
            {
                return acDoc.GetBlocks()
                    .ReadBlocks(acDoc);
            }
        }

        /// <summary>
        /// Read data from blocks in the drawing filtered by block name
        /// </summary>
        public static List<dynamic> LoadBlocks(Document doc, string blockName) =>
            LoadBlocks(doc, new[] { blockName });

        /// <summary>
        /// Read data from blocks in the drawing filtered by block names
        /// </summary>
        public static List<dynamic> LoadBlocks(Document doc, string[] blockNames)
        {
            using (var acDoc = new AcadDocument(doc))
            {
                var sel = AcadUtil.GetBlockSelection(doc, blockNames);
                return acDoc.GetDatabase().GetEntities<BlockReference>(sel)
                    .ReadBlocks(acDoc);
            }
        }

        /// <summary>
        /// Read data from a list of blocks
        /// </summary>
        static List<dynamic> ReadBlocks(this IEnumerable<BlockReference> blocks, AcadDocument acDoc) =>
            blocks
                .Select(bl => (dynamic)new
                {
                    bl.Id,
                    PosX = bl.Position.X,
                    PosY = bl.Position.Y,
                    PosZ = bl.Position.Z,
                    bl.Name,
                    bl.Layer,
                    bl.Rotation,
                    Attrs = acDoc.GetBlockAttributes(bl)
                        .Select(attref => new { attref.Tag, attref.TextString })
                        .ToList()
                })
                .ToList();

        /// <summary>
        /// Read data from all polylines in the drawing
        /// </summary>
        public static List<dynamic> LoadPolyLines(Document doc)
        {
            using (var acDoc = new AcadDocument(doc))
            {
                return acDoc.GetPolylines()
                    .Select(pl => (dynamic) new
                        {
                            pl.Id,
                            pl.Layer,
                            pl.Linetype,
                            pl.Length,
                            pl.Closed,
                            Points = Enumerable.Range(0, pl.NumberOfVertices)
                                .Select(i => pl.GetPoint3dAt(i))
                                .ToList()
                        })
                    .ToList();
            }
        }

        /// <summary>
        /// Read data from all lines in the drawing
        /// </summary>
        public static List<dynamic> LoadLines(Document doc)
        {
            using (var acDoc = new AcadDocument(doc))
            {
                return acDoc.Getlines()
                    .Select(line => (dynamic) new 
                        {
                            line.Id,
                            StartX = line.StartPoint.X,
                            StartY = line.StartPoint.Y,
                            EndX = line.EndPoint.X,
                            EndY = line.EndPoint.Y,
                            line.Layer,
                            line.Linetype,
                            Color = line.Color.ToString(),
                            line.Length,
                        })
                    .ToList();
            }
        }

        /// <summary>
        /// Read data from all MText in the drawing
        /// </summary>
        public static List<dynamic> LoadMText(Document doc)
        {
            using (var acDoc = new AcadDocument(doc))
            {
                return acDoc.GetEntities<MText>(DxfEntity.MTEXT)
                    .Select(mt => (dynamic) new 
                        {
                            mt.Id,
                            LocX = mt.Location.X,
                            LocY = mt.Location.Y,
                            mt.Layer,
                            Color = mt.Color.ToString(),
                            mt.TextStyleName,
                            mt.TextHeight,
                            mt.Width,
                            Attachment = Convert.ToInt32(mt.Attachment),
                            mt.Text,
                        })
                    .ToList();
            }
        }

        #endregion
    }
}

