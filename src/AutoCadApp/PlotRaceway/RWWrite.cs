using AutoCadShared;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RacewayDataLib;
using System.Collections.Generic;
using System.Linq;

namespace PlotRaceway
{
    public static class RWWrite
    {
        static IEnumerable<BlockReference> WriteNodeBlock(AcadDatabase acDB, IEnumerable<Node> nodes, BlockTable bt) =>
            nodes.Where(n => bt.Has(n.NodeType))
                .Select(n =>
                {
                    // each block reference is a block table record
                    var btrId = bt[n.NodeType];
                    var bref = new BlockReference(new Point3d(n.X, n.Y, n.Z), btrId);
                    foreach (var attr in bref.AttributeCollection.Cast<ObjectId>())
                    {
                        var attRef = acDB.GetObject<AttributeReference>(attr, OpenMode.ForWrite);
                        UpdateAttr(attRef, n.Tag, n.ID);
                    }
                    return bref;
                });

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

        static IEnumerable<BlockReference> WriteRacewayBlock(AcadDatabase acDB, IEnumerable<Raceway> raceways, BlockTable bt) =>
            raceways.Where(r => bt.Has(r.Type))
                .Select(r =>
                {
                    // each block reference is a block table record
                    var btrId = bt[r.Type];
                    var bref = new BlockReference(new Point3d(0, 0, 0), btrId);
                    foreach (var attr in bref.AttributeCollection.Cast<ObjectId>())
                    {
                        var attRef = acDB.GetObject<AttributeReference>(attr, OpenMode.ForWrite);
                        UpdateAttr(attRef, r.Tag, r.ID);
                        ApplyTransform(attRef, r);
                    }
                    return bref;
                });

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


        public static void WriteNetwork(Document doc, NetworkDB.NetworkData repo)
        {
            using (var acDoc = new AcadDocument(doc))
            using (var acDB = acDoc.GetDatabase())
            {
                var bt = acDB.GetBlockTable();
                var blkNodes = WriteNodeBlock(acDB, repo.Nodes, bt);
                var blkRW = WriteRacewayBlock(acDB, repo.Raceways, bt);
                var blks = blkNodes.Concat(blkRW);

                // model space block table record
                var ms = acDB.GetModelSpaceBlockTableRecord(OpenMode.ForWrite);
                try
                {
                    doc.LockDocument();
                    foreach (var bref in blks)
                    {
                        // attach block reference to model space
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
        }
    }
}
