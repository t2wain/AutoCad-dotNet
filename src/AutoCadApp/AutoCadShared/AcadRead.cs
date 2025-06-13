using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ACOL = AutoCadShared.AcadCollection;
using AcadCommon.DTO;
using System.IO;


namespace AutoCadShared
{
    /// <summary>
    /// Typical steps to read data from entities in drawing
    /// </summary>
    public static class AcadRead
    {

        #region Read blocks with document

        /// <summary>
        /// Read data from all blocks in the drawing
        /// </summary>
        public static List<BlockDTO> LoadBlocks(Document doc)
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
        public static List<BlockDTO> LoadBlocks(Document doc, string blockName) =>
            LoadBlocks(doc, new[] { blockName });

        /// <summary>
        /// Read data from blocks in the drawing filtered by block names
        /// </summary>
        public static List<BlockDTO> LoadBlocks(Document doc, string[] blockNames)
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
        public static List<BlockDTO> ReadBlocks(this IEnumerable<BlockReference> blocks, AcadDocument acDoc) =>
            ReadBlocks(blocks, acDoc.GetDatabase());

        #endregion

        #region Read Entities

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
                            pl.BlockId,
                            pl.BlockName,
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
                            line.BlockId,
                            line.BlockName,
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
                            mt.BlockId,
                            mt.BlockName,
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

        #region Read All

        /// <summary>
        /// Explore the standard collection of objects in the drawing
        /// </summary>
        public static void ReadObjectCollection(Document doc)
        {
            using (var acDoc = new AcadDocument(doc))
            using (var acDB = acDoc.GetDatabase())
            {
                ReadObjectCollection(acDB);
            }
        }

        /// <summary>
        /// Explore the standard collection of objects in the drawing
        /// </summary>
        public static void ReadObjectCollection(AcadDatabase acDB)
        {
            var lt = acDB.GetObject<LayerTable>(ACOL.GetLayerTable(acDB.AcadDB));
            var ltrs = acDB.GetDBOjects(ACOL.GetLayerTableRecord(lt)).Cast<LayerTableRecord>().ToList();
            var names = ltrs.Select(l => l.Name).ToList();

            var bt = acDB.GetObject<BlockTable>(ACOL.GetBlockTable(acDB.AcadDB));
            var btrs = acDB.GetDBOjects(ACOL.GetBlockTableRecords(bt)).Cast<BlockTableRecord>().ToList();
            names = btrs.Select(b => b.Name).ToList();

            // Model Space
            var btrMS = btrs.Where(btr => btr.Name.Equals(BlockTableRecord.ModelSpace, StringComparison.OrdinalIgnoreCase)).First();
            var msCnt = ACOL.GetEntities(btrMS).Count; // entity count

            // Paper Space
            var btrPS = btrs.Where(btr => btr.Name.Equals(BlockTableRecord.PaperSpace, StringComparison.OrdinalIgnoreCase)).First();
            var psCnt = ACOL.GetEntities(btrPS).Count; // entity count

            // Block templates
            var qbtrs = btrs.Where(btr => !btr.Name.Equals(BlockTableRecord.ModelSpace, StringComparison.OrdinalIgnoreCase)
                && !btr.Name.Equals(BlockTableRecord.PaperSpace, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var btr in qbtrs)
            {
                var data = new
                {
                    btr.Name,
                    btr.Explodable,
                    btr.HasAttributeDefinitions,
                    btr.HasPreviewIcon,
                    btr.IsAnonymous,
                    btr.IsDynamicBlock,
                    btr.IsFromExternalReference,
                    btr.IsFromOverlayReference,
                    btr.IsLayout,
                    btr.Origin,
                    btr.PathName,
                    btr.XrefStatus,
                    AnonymousBlockIds = btr.GetAnonymousBlockIds().Count,
                    BlockReferenceIds = btr.GetBlockReferenceIds(false,false).Count,
                    ErasedBlockReferenceIds = btr.GetErasedBlockReferenceIds().Count,
                };
                    
                var ids = ACOL.GetEntities(btr); // entities defined in block template
                var ents = acDB.GetDBOjects(ids).ToList();
            }
        }

        #endregion

        #region Read blocks with database

        /// <summary>
        /// Scan the document for all BlockReference
        /// </summary>
        public static DrawingDTO ReadAllBlocks(string filePath, Regex nameFilter = null)
        {
            try
            {
                // open the drawing database without
                // loading the drawing to the editor
                using (var acDB = new AcadDatabase(filePath))
                {
                    // get all BlockReference
                    var blks = GetAllBlocks(acDB);
                    // filter blocks by name
                    if (nameFilter != null)
                        blks = blks
                            .Where(blk => nameFilter.IsMatch(blk.Name))
                            .ToList();
                    // read block data
                    var blkData = ReadBlocks(blks, acDB);
                    return new DrawingDTO
                    {
                        FileName = new FileInfo(filePath).Name.Replace(".dwg", ""),
                        FilePath = filePath,
                        MTOBlocks = blkData.ToArray()
                    };
                }
            }
            catch (Exception ex) 
            {
                return CreateDrawingError(ex, filePath);
            }
        }

        /// <summary>
        /// Capture the error from scanning task
        /// </summary>
        public static DrawingDTO CreateDrawingError(Exception ex, string filePath) =>
            new DrawingDTO
            {
                FilePath = filePath,
                FileName = new FileInfo(filePath).Name.Replace(".dwg", ""),
                IsScanError = true,
                ErrorMessage = ex.Message,
            };

        /// <summary>
        /// Retrieve all the BlockReference from model and paper spaces
        /// </summary>
        public static IEnumerable<BlockReference> GetAllBlocks(AcadDatabase acDB)
        {
            var colId = ACOL.GetAllBlocks(acDB).Cast<ObjectId>();
            var colEnt = acDB.GetEntities<BlockReference>(colId).ToList();
            return colEnt
                .Where(bl => !bl.Name.Contains("*")) // exclude anonymous blocks
                .ToList();
        }

        /// <summary>
        /// Read data from a list of blocks
        /// </summary>
        public static List<BlockDTO> ReadBlocks(this IEnumerable<BlockReference> blocks, AcadDatabase acDB) =>
            blocks
                .Select(bl => new BlockDTO
                {
                    Id = bl.Id.ToString(),
                    BlockId = bl.BlockId.ToString(),
                    BlockName = bl.BlockName,
                    BlockTableRecord = bl.BlockTableRecord.ToString(),
                    PositionX = bl.Position.X,
                    PositionY = bl.Position.Y,
                    PositionZ = bl.Position.Z,
                    Name = bl.Name.ToString(),
                    Layer = bl.Layer,
                    Rotation = bl.Rotation,
                    Attrs = acDB.GetEntities<AttributeReference>(bl.AttributeCollection.Cast<ObjectId>())
                        .Select(attref => new BlockAttrDTO { Tag = attref.Tag, TextString = attref.TextString })
                        .ToArray()
                })
                .ToList();

        #endregion
    }
}

