using Autodesk.AutoCAD.Interop.Common;
using ACOL = AutoCadComShared.AcCol;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using AcadCommon.DTO;
using System.IO;
using System.Text.RegularExpressions;

namespace AutoCadComShared
{
    public static class AcRead
    {
        /// <summary>
        /// Explore the standard collection of objects in the drawing
        /// </summary>
        public static void ReadObjectCollection(AcadDatabase acDB)
        {
            // layers
            var lt = ACOL.GetLayerTable(acDB);
            var ltrs = ACOL.GetLayerTableRecord(lt);
            var names = ltrs.Select(l => l.Name).ToList();

            // block definition
            var bt = ACOL.GetBlockTable(acDB);
            var btrs = ACOL.GetBlockTableRecords(bt);
            names = btrs.Select(b => b.Name).ToList();

            // Model Space
            var btrMS = acDB.ModelSpace;
            var msCnt = btrMS.Count; // entity count

            // Paper Space
            var btrPS = acDB.PaperSpace;
            var psCnt = btrPS.Count; // entity count

            #region Block templates

            var qbtrs = btrs.Where(btr => 
                !btr.Name.Equals(btrMS.Name, StringComparison.OrdinalIgnoreCase)
                && !btr.Name.Equals(btrPS.Name, StringComparison.OrdinalIgnoreCase)
                && btr.Name.StartsWith("MTO_")
            ).ToList();

            var lst = new List<object>();
            foreach (var btr in qbtrs)
            {
                var data = new
                {
                    btr.ObjectName,
                    btr.ObjectID,
                    btr.Name,
                    btr.Explodable,
                    btr.IsDynamicBlock,
                    btr.IsLayout,
                    btr.Origin,
                    btr.IsXRef,
                };
                lst.Add(data);
            }

            #endregion

            #region Block references

            // BlockReference
            var blkRefs = ACOL.GetBlockRefModelAndPaper(acDB);
            
            lst.Clear();
            foreach (var blkRef in blkRefs) 
            {
                var data = ReadBlockRef(blkRef);
                lst.Add(data);
            }

            #endregion
        }

        /// <summary>
        /// Explore data in block reference
        /// </summary>
        public static object ReadBlockRef(IAcadBlockReference blkRef)
        {
            var loc = blkRef.InsertionPoint as double[];
            var data = new
            {
                Id = blkRef.ObjectID,
                blkRef.OwnerID,
                blkRef.InsertionPoint,
                blkRef.InsUnits,
                blkRef.InsUnitsFactor,
                PositionX = loc[0],
                PositionY = loc[1],
                PositionZ = loc[2],
                blkRef.ObjectName, 
                blkRef.EffectiveName, 
                blkRef.EntityName,
                blkRef.IsDynamicBlock,
                blkRef.EntityType,
                Name = blkRef.Name.ToString(),
                blkRef.Layer,
                blkRef.Rotation
            };

            return data;
        }

        #region Read drawing

        /// <summary>
        /// Scan the document for all BlockReference
        /// in ModelSpace and PaperSpace
        /// </summary>
        public static DrawingDTO ReadDrawingDTO(AcadDatabase acDB, string filePath, Regex nameFilter = null)
        {
            try
            {
                // get all BlockReference
                var blks = ACOL.GetBlockRefModelAndPaper(acDB);
                // filter blocks by name
                if (nameFilter != null)
                    blks = blks
                        .Where(blk => nameFilter.IsMatch(blk.Name))
                        .ToList();
                // read block data
                var blkData = ReadBlockDTO(blks);
                return new DrawingDTO
                {
                    FileName = new FileInfo(filePath).Name.Replace(".dwg", ""),
                    FilePath = filePath,
                    MTOBlocks = blkData.ToArray()
                };
            }
            catch (Exception ex)
            {
                return CreateDrawingError(ex, filePath);
            }
        }

        /// <summary>
        /// Read attributes in block
        /// </summary>
        public static List<BlockAttrDTO> ReadBlockAttrDTO(IAcadBlockReference blockRef)
        {
            var lst = new List<BlockAttrDTO>();
            if (blockRef.HasAttributes)
            {
                var atts = (blockRef.GetAttributes() as IEnumerable).Cast<IAcadAttributeReference>();
                lst = atts.Select(att => 
                    new BlockAttrDTO {
                        Tag = att.TagString, 
                        TextString = att.TextString 
                    }
                ).ToList();
            }
            return lst;
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
        /// Read data from a list of blocks
        /// </summary>
        public static List<BlockDTO> ReadBlockDTO(IEnumerable<IAcadBlockReference> blocks) =>
             (from bl in blocks
                let loc = bl.InsertionPoint as double[]
                select new BlockDTO
                {
                    Id = bl.ObjectID.ToString(),
                    BlockId = bl.OwnerID.ToString(),
                    BlockName = bl.EffectiveName,
                    BlockTableRecord = bl.ObjectName,
                    PositionX = loc[0],
                    PositionY = loc[1],
                    PositionZ = loc[2],
                    Name = bl.Name.ToString(),
                    Layer = bl.Layer,
                    Rotation = bl.Rotation,
                    Attrs = ReadBlockAttrDTO(bl).ToArray()
                }).ToList();

        #endregion
    }
}
