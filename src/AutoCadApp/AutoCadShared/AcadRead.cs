using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
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
            using (var tran = doc.TransactionManager.StartTransaction())
            {
                return doc.GetSelection(DxfEntity.INSERT)
                    .Select(obj => tran.GetObject(obj.ObjectId, OpenMode.ForRead))
                    .Cast<BlockReference>()
                    .Where(bl => !bl.Name.Contains("*")) // exclude anonymous blocks
                    .ReadBlocks(tran);
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
            using (var tran = doc.TransactionManager.StartTransaction())
            {
                return doc.GetBlockSelection(blockNames)
                    .Select(obj => tran.GetObject(obj.ObjectId, OpenMode.ForRead))
                    .Cast<BlockReference>()
                    .ReadBlocks(tran);
            }
        }

        /// <summary>
        /// Read data from a list of blocks
        /// </summary>
        static List<dynamic> ReadBlocks(this IEnumerable<BlockReference> blocks, Transaction tran) =>
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
                    Attrs = bl.AttributeCollection
                            .Cast<ObjectId>()
                            .Select(attrId => tran.GetObject(attrId, OpenMode.ForRead))
                            .Cast<AttributeReference>()
                            .Select(attref => new { attref.Tag, attref.TextString })
                            .ToList()
                })
                .ToList();

        /// <summary>
        /// Read data from all polylines in the drawing
        /// </summary>
        public static List<dynamic> LoadPolyLines(Document doc)
        {
            using (var tran = doc.TransactionManager.StartTransaction())
            {
                return doc.GetSelection(DxfEntity.LWPOLYLINE)
                    .Select(obj => tran.GetObject(obj.ObjectId, OpenMode.ForRead))
                    .Cast<Polyline>()
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
            using (var tran = doc.TransactionManager.StartTransaction())
            {
                return doc.GetSelection(DxfEntity.LINE)
                    .Select(obj => tran.GetObject(obj.ObjectId, OpenMode.ForRead))
                    .Cast<Line>()
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
            using (var tran = doc.TransactionManager.StartTransaction())
            {
                return doc.GetSelection(DxfEntity.MTEXT)
                    .Select(obj => tran.GetObject(obj.ObjectId, OpenMode.ForRead))
                    .Cast<MText>()
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

        #region Entity Selection

        /// <summary>
        /// Select block entities by block names
        /// </summary>
        public static IEnumerable<SelectedObject> GetBlockSelection(this Document doc, string[] blockNames)
        {
            var q = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Operator, "<AND"),
                new TypedValue((int)DxfCode.Start, DxfEntity.INSERT.ToString()),
                new TypedValue((int)DxfCode.Operator, "<OR"),
            }
            .Concat(blockNames.Select(n => new TypedValue((int)DxfCode.BlockName, n)))
            .Concat(new TypedValue[]
            {
                new TypedValue((int)DxfCode.Operator, "OR>"),
                new TypedValue((int)DxfCode.Operator, "AND>")
            });

            var filter = new SelectionFilter(q.ToArray());
            return doc.GetSelection(filter);
        }

        /// <summary>
        /// Get a selection of all entities of certain type in the drawing
        /// </summary>
        public static IEnumerable<SelectedObject> GetSelection(this Document doc, DxfEntity dxfEntity) =>
            doc.GetSelection(new SelectionFilter(
                new TypedValue[] 
                { 
                    new TypedValue((int)DxfCode.Start, dxfEntity.ToString())
                }
            ));

        /// <summary>
        /// Get a selection of all entities of certain type in the drawing
        /// </summary>
        public static IEnumerable<SelectedObject> GetSelection(this Document doc, SelectionFilter filter)
        {
            // select entities based on filter
            PromptSelectionResult ssPrompt = doc.Editor.SelectAll(filter);

            if (ssPrompt.Status != PromptStatus.OK)
                return Array.Empty<SelectedObject>();
            else return ssPrompt.Value.Cast<SelectedObject>();
        }

        #endregion
    }
}

