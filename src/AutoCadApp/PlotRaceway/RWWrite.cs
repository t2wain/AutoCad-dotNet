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

        static IEnumerable<BlockReference> BuildNodeBlock(AcadDatabase acDB, 
            IEnumerable<Node> nodes, BlockTable bt, BlockEnum blockName, LayerEnum layerName) =>
            nodes.Where(n => bt.Has(n.NodeType))
                .Select(n => BuildNodeBlock(acDB, n, bt, blockName, layerName));
        
        static BlockReference BuildNodeBlock(AcadDatabase acDB,
            Node node, BlockTable bt, BlockEnum blockName, LayerEnum layerName)
        {

            // each block reference is a block table record
            // contains the entities defined for the block reference
            var btrId = bt[blockName.ToString()];

            // create a new instance of a block based
            // on the block table record
            var bref = new BlockReference(new Point3d(node.X, node.Y, node.Z), btrId);
            bref.Layer = layerName.ToString();

            foreach (var attr in bref.AttributeCollection.Cast<ObjectId>())
            {
                var attRef = acDB.GetObject<AttributeReference>(attr, OpenMode.ForWrite);
                UpdateAttr(attRef, node.Tag, node.ID);
            }
            return bref;
        }

        #endregion

        #region Raceway block

        static IEnumerable<BlockReference> BuildRacewayBlock(AcadDatabase acDB, 
            IEnumerable<Raceway> raceways, BlockTable bt, BlockEnum blockName, LayerEnum layerName) =>
            raceways.Where(r => bt.Has(r.Type))
                .Select(r => BuildRacewayBlock(acDB, r, bt, blockName, layerName));

        static BlockReference BuildRacewayBlock(AcadDatabase acDB,
            Raceway raceway, BlockTable bt, BlockEnum blockName, LayerEnum layerName)
        {
            // each block reference is a block table record
            // contains the entities defined for the block reference
            var btrId = bt[blockName.ToString()];

            // create a new instance of a block based
            // on the block table record
            var bref = new BlockReference(new Point3d(0, 0, 0), btrId);
            bref.Layer = layerName.ToString();

            foreach (var attr in bref.AttributeCollection.Cast<ObjectId>())
            {
                var attRef = acDB.GetObject<AttributeReference>(attr, OpenMode.ForWrite);
                UpdateAttr(attRef, raceway.Tag, raceway.ID);
                ApplyTransform(attRef, raceway);
            }
            return bref;
        }
        
        static IEnumerable<BlockReference> BuildRaceway(this IEnumerable<Raceway> raceway, 
            AcadDatabase acDB, BlockTable bt)
        {
            IEnumerable<BlockReference> lstBlk = Array.Empty<BlockReference>();

            if (raceway.GetTray() is IEnumerable<Raceway> trays && trays.Count() > 0)
            {
                var rwNodes = trays.GetNodes();
                var blkRwNodes = BuildNodeBlock(acDB, rwNodes, bt, BlockEnum.RwNode, LayerEnum.RwNode);
                var blkRw = BuildRacewayBlock(acDB, trays, bt, BlockEnum.Raceway, LayerEnum.Raceway);
                lstBlk = lstBlk.Concat(blkRwNodes).Concat(blkRw);
            }

            if (raceway.GetJump() is IEnumerable<Raceway> jumps && jumps.Count() > 0)
            {
                var blkJump = BuildRacewayBlock(acDB, jumps, bt, BlockEnum.Drop, LayerEnum.Drop);
                lstBlk = lstBlk.Concat(blkJump);
            }

            if (raceway.GetDrop() is IEnumerable<Raceway> drops && drops.Count() > 0)
            {
                var rwEqNodes = drops.GetNodes().GetEquipNodes();
                var blkEqNodes = BuildNodeBlock(acDB, rwEqNodes, bt, BlockEnum.EquipNode, LayerEnum.EquipNode);
                var blkDrop = BuildRacewayBlock(acDB, drops, bt, BlockEnum.Drop, LayerEnum.Drop);
                lstBlk = lstBlk.Concat(blkEqNodes).Concat(blkDrop);
            }

            return lstBlk;
        }

        #endregion

        #region Common

        static void UpdateAttr(AttributeReference attRef, string tag, int id)
        {
            switch (attRef.Tag)
            {
                case "NAME":
                    attRef.TextString = tag;
                    break;
                case "ID":
                    attRef.TextString = id.ToString();
                    break;
            }
        }

        static void ApplyTransform(Entity ent, Raceway raceway)
        {
            var p0 = new Point3d(0, 0, 0);
            var vb = new Vector3d(1, 0, 0);

            var fn = raceway.FromNode;
            var tn = raceway.ToNode;
            var pf = new Point3d(new double[] { fn.X, fn.Y, fn.Z });
            var pt = new Point3d(new double[] { tn.X, tn.Y, tn.Z });
            var vr = pt - pf;

            var axis = vb.CrossProduct(vr);
            var angle = vb.GetAngleTo(vr);

            ent.TransformBy(Matrix3d.Rotation(angle, axis, p0));
            ent.TransformBy(Matrix3d.Scaling(vr.Length, new Point3d(0, 0, 0)));
            ent.TransformBy(Matrix3d.Displacement(pf - p0));
        }

        static void SaveData(AcadDatabase acDB, IEnumerable<BlockReference> lstBlk)
        {
            // model space block table record
            var ms = acDB.GetModelSpaceBlockTableRecord(OpenMode.ForWrite);
            try
            {
                foreach (var bref in lstBlk)
                {
                    // attach block instance to model space
                    ms.AppendEntity(bref);
                    acDB.AcadTran.AddNewlyCreatedDBObject(bref, true);
                    bref.Dispose();
                }
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
                var lstBlk = raceways.BuildRaceway(acDB, bt);
                SaveData(acDB, lstBlk);
            }
        }
    }
   
}
