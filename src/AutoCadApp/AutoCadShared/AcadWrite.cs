using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace AutoCadShared
{
    /// <summary>
    /// Typical steps to write data to document. 
    /// Be sure document is locked first.
    /// </summary>
    public static class AcadWrite
    {
        /// <summary>
        /// Write new entities to document
        /// </summary>
        /// <param name="curTran">Existing transaction</param>
        public static void WriteEntities(AcadDatabase acDB, IEnumerable<Entity> entities,
            string appName = null)
        {
            var lst = entities.Select<Entity, (Entity, object)>(e => (e, null));
            WriteEntitiesWithXData(acDB, lst, null);
        }

        /// <summary>
        /// Write new entities to document with XData value
        /// </summary>
        /// <typeparam name="T">Data type of XData</typeparam>
        /// <param name="entities">Tuples of Entity and it's XData value</param>
        /// <param name="appName">App name for XData</param>
        public static void WriteEntitiesWithXData<T>(AcadDatabase acDB, IEnumerable<(Entity, T)> entities,
            string appName = null)
        {
            var bt = acDB.GetBlockTable();
            var btr = acDB.GetModelSpaceBlockTableRecord(bt, OpenMode.ForWrite);
            foreach ((Entity ent, T xdValue) in entities)
            {
                acDB.AddEntity(btr, ent);
                // add xdata to entity
                if (!string.IsNullOrEmpty(appName) && xdValue != null)
                    acDB.AddXData(ent, xdValue, appName);
            }
        }

        /// <summary>
        /// Create an app record for storing xdata to document
        /// </summary>
        /// <param name="curTran">Current transaction</param>
        public static void AddApp(AcadDatabase acDB, string appName)
        {
            var regtbl = acDB.GetRegAppTable(OpenMode.ForWrite);
            acDB.AddAppRecord(regtbl, appName);
        }

        #region Block Reference

        /// <summary>
        /// Create a block reference based on exsiting block definition.
        /// Each block definition is a block table record
        /// </summary>
        public static BlockReference CreateBlockReference(BlockTable bt, string blkName)
        {
            // existing block definition is defined as block table record
            var btrId = bt[blkName];

            // create a new instance of a block based
            // on the block table record
            var bref = new BlockReference(new Point3d(0, 0, 0), btrId);
            return bref;
        }

        /// <summary>
        /// Add attributes to new block reference
        /// based on its block definition
        /// </summary>
        public static IEnumerable<AttributeReference> AddAttributes(AcadDatabase acDB, BlockReference newBlkRef)
        {
            // Entities and attributes defined in the block definition
            // are stored in its block table record
            using (var btr = acDB.GetObject<BlockTableRecord>(newBlkRef.BlockTableRecord))
            {
                var lstAtt = new List<AttributeReference>();
                foreach (var objId in btr.Cast<ObjectId>())
                {
                    var objDB = acDB.GetDBObject(objId);
                    if (objDB is AttributeDefinition attDef && !attDef.Constant)
                    {
                        var attRef = CreateAttrRef(attDef);
                        var attId = newBlkRef.AttributeCollection.AppendAttribute(attRef);
                        lstAtt.Add(attRef);
                    }
                }
                return lstAtt;
            }
        }

        /// <summary>
        /// Create an attribute reference based on attribute definition
        /// </summary>
        public static AttributeReference CreateAttrRef(AttributeDefinition attDef)
        {
            var att = new AttributeReference();
            att.SetPropertiesFrom(attDef);
            att.Invisible = attDef.Invisible;
            att.Position += attDef.Position.GetAsVector();
            att.Height = attDef.Height;
            att.Rotation = attDef.Rotation;
            att.FieldLength = attDef.FieldLength;
            att.Tag = attDef.Tag;
            return att;
        }

        #endregion

        #region Transform Entity

        public class EntityTransforms
        {
            public Matrix3d Rotation { get; set; } = Matrix3d.Identity;
            public Matrix3d Scale { get; set; } = Matrix3d.Identity;
            public Matrix3d Displacement { get; set; } = Matrix3d.Identity;
        }

        public static EntityTransforms GetTransforms( 
            Point3d fromPosition, Vector3d fromSizeDirection, 
            Point3d toPosition, Vector3d toSizeDirection)
        {
            // rotate
            var axis = fromSizeDirection.CrossProduct(toSizeDirection);
            var angle = fromSizeDirection.GetAngleTo(toSizeDirection);
            var rotate = Matrix3d.Rotation(angle, axis, fromPosition);

            // lengthen
            var scale = Matrix3d.Scaling(toSizeDirection.Length / fromSizeDirection.Length, fromPosition);

            // move
            var move = Matrix3d.Displacement(toPosition - fromPosition);

            return new EntityTransforms { Rotation = rotate, Scale = scale, Displacement = move };
        }

        public static void ApplyTransform(Entity ent, EntityTransforms transforms)
        {
            ent.TransformBy(transforms.Rotation);
            ent.TransformBy(transforms.Scale);
            ent.TransformBy(transforms.Displacement);
        }

        #endregion
    }
}
