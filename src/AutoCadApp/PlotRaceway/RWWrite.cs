using AutoCadShared;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RacewayDataLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlotRaceway
{
    public static class RWWrite
    {
        #region Node block

        /// <summary>
        /// Create node block references
        /// </summary>
        /// <param name="attachBTR">Attached entity to model_space BloctTableRecord</param>
        static IEnumerable<ObjectId> BuildNodeBlock(AcadDatabase acDB, IEnumerable<Node> nodes, 
            BlockTable bt, BlockEnum blockName, LayerEnum layerName, BlockTableRecord attachBTR) =>
            nodes.Where(n => bt.Has(blockName.ToString()))
                .Select(n => BuildNodeBlock(acDB, n, bt, blockName, layerName, attachBTR));

        /// <summary>
        /// Create node block references
        /// </summary>
        /// <param name="attachBTR">Attached entity to model_space BloctTableRecord</param>
        static ObjectId BuildNodeBlock(AcadDatabase acDB, Node node, 
            BlockTable bt, BlockEnum blockName, LayerEnum layerName, BlockTableRecord attachBTR)
        {

            // each block reference is a block table record
            // contains the entities defined for the block reference
            var btrId = bt[blockName.ToString()];

            // create a new instance of a block based
            // on the block table record
            using (var bref = new BlockReference(new Point3d(0, 0, 0), btrId))
            {
                bref.Layer = layerName.ToString();
                var blkId = acDB.AddEntity(attachBTR, bref);
                var transforms = new AcadWrite.EntityTransforms
                {
                    Scale = Matrix3d.Scaling(0.2, bref.Position),
                    Displacement = Matrix3d.Displacement(new Point3d(node.X, node.Y, node.Z).GetAsVector())
                };
                AcadWrite.ApplyTransform(bref, transforms);

                // move the attributes to the node position
                transforms.Scale = Matrix3d.Identity;
                var lstAttIds = AddAttributes(acDB, bref, (node.Tag, node.ID), transforms);
                return blkId;
            }
        }

        #endregion

        #region Raceway block

        /// <summary>
        /// Create raceway block references
        /// </summary>
        /// <param name="attachBTR">Attached entity to model_space BloctTableRecord</param>
        static IEnumerable<ObjectId> BuildRacewayBlock(AcadDatabase acDB, IEnumerable<Raceway> raceways, 
            BlockTable bt, BlockEnum blockName, LayerEnum layerName, BlockTableRecord attachBTR) =>
            raceways.Where(r => bt.Has(blockName.ToString()))
                .Select(r => BuildRacewayBlock(acDB, r, bt, blockName, layerName, attachBTR));

        /// <summary>
        /// Create raceway block reference
        /// </summary>
        /// <param name="attachBTR">Attached entity to model_space BloctTableRecord</param>
        static ObjectId BuildRacewayBlock(AcadDatabase acDB, Raceway raceway, 
            BlockTable bt, BlockEnum blockName, LayerEnum layerName, BlockTableRecord attachBTR)
        {
            // each block definition is a block table record
            // contains the entities defined for the block definition
            var btrId = bt[blockName.ToString()];

            // create a new instance of a block reference based
            // on block definition
            using (var bref = new BlockReference(new Point3d(0, 0, 0), btrId))
            {
                bref.Layer = layerName.ToString();
                var blkId = acDB.AddEntity(attachBTR, bref);
                var trans = GetTransform(raceway);
                AcadWrite.ApplyTransform(bref, trans);

                // create attributes for the block reference
                // move the attributes to the raceway position
                trans.Scale = Matrix3d.Identity;
                var lstAttIds = AddAttributes(acDB, bref, (raceway.Tag, raceway.ID), trans);
                return blkId;
            }
        }
        
        /// <summary>
        /// Plot the raceway network on certain layers
        /// based on existing block definitions
        /// </summary>
        static IEnumerable<ObjectId> BuildRaceway(this IEnumerable<Raceway> raceway, 
            AcadDatabase acDB, BlockTable bt, BlockTableRecord attachBTR)
        {
            IEnumerable<ObjectId> lstBlk = Array.Empty<ObjectId>();

            // create tray and raceway node block on certain layers
            if (raceway.GetTray() is IEnumerable<Raceway> trays && trays.Count() > 0)
            {
                var rwNodes = trays.GetNodes();
                var blkRwNodes = BuildNodeBlock(acDB, rwNodes, bt, BlockEnum.RwNode, LayerEnum.RwNode, attachBTR);
                var blkRw = BuildRacewayBlock(acDB, trays, bt, BlockEnum.Raceway, LayerEnum.Raceway, attachBTR);
                lstBlk = lstBlk.Concat(blkRwNodes).Concat(blkRw);
            }

            // create jump block on certain layers
            if (raceway.GetJump() is IEnumerable<Raceway> jumps && jumps.Count() > 0)
            {
                var blkJump = BuildRacewayBlock(acDB, jumps, bt, BlockEnum.Drop, LayerEnum.Drop, attachBTR);
                lstBlk = lstBlk.Concat(blkJump);
            }

            // create drop block and equip node block on certain layers
            if (raceway.GetDrop() is IEnumerable<Raceway> drops && drops.Count() > 0)
            {
                var rwEqNodes = drops.GetNodes().GetEquipNodes();
                var blkEqNodes = BuildNodeBlock(acDB, rwEqNodes, bt, BlockEnum.EquipNode, LayerEnum.EquipNode, attachBTR);
                var blkDrop = BuildRacewayBlock(acDB, drops, bt, BlockEnum.Drop, LayerEnum.Drop, attachBTR);
                lstBlk = lstBlk.Concat(blkEqNodes).Concat(blkDrop);
            }

            return lstBlk;
        }

        #endregion

        #region Common

        /// <summary>
        /// Add attributes to new block reference
        /// based on its block definition
        /// </summary>
        static IEnumerable<ObjectId>AddAttributes(AcadDatabase acDB, 
            BlockReference bref, (string Tag, int ID) data, AcadWrite.EntityTransforms transforms)
        {
            var lstAtt = AcadWrite.AddAttributes(acDB, bref);
            var lstIds = new List<ObjectId>();
            foreach (var att in lstAtt)
            {
                switch (att.Tag)
                {
                    case "NAME":
                        att.TextString = data.Tag;
                        break;
                    case "ID":
                        att.TextString = data.ID.ToString();
                        break;
                }
                att.FieldLength = att.TextString.Length;
                AcadWrite.ApplyTransform(att, transforms);
                lstIds.Add(att.ObjectId);
                att.Dispose();
            }
            return lstIds;
        }

        /// <summary>
        /// Get block transform based on raceway data
        /// </summary>
        static AcadWrite.EntityTransforms GetTransform(Raceway raceway)
        {
            // position of default raceway block
            var p0 = new Point3d(0, 0, 0); // insertion point at origin
            var vb = new Vector3d(1, 0, 0); // unit lenth of 1 in x-axis

            // calculate vector of raceway
            var fn = raceway.FromNode;
            var tn = raceway.ToNode;
            var pf = new Point3d(new double[] { fn.X, fn.Y, fn.Z });
            var pt = new Point3d(new double[] { tn.X, tn.Y, tn.Z });
            var vr = pt - pf; // raceway vector

            return AcadWrite.GetTransforms(p0, vb, pf, vr);
        }

        public static void ExecuteQuery(AcadDatabase acDB, IEnumerable<ObjectId> qEntities)
        {
            try
            {
                // execute LINQ query
                var lstIds = qEntities.ToList();
                acDB.AcadTran.Commit();
            }
            catch
            {
                acDB.AcadTran.Abort();
                throw;
            }
        }

        #endregion

        public static void WriteNetwork(Document doc, IEnumerable<Raceway> raceways)
        {
            doc.LockDocument();
            using (var acDoc = new AcadDocument(doc))
            using (var acDB = acDoc.GetDatabase())
            {
                var bt = acDB.GetBlockTable();
                var ms = acDB.GetModelSpaceBlockTableRecord(OpenMode.ForWrite);
                var qEntities = raceways.BuildRaceway(acDB, bt, ms);
                ExecuteQuery(acDB, qEntities);
            }
            AcadZoom.ZoomExtents(doc);
        }
    }
   
}
