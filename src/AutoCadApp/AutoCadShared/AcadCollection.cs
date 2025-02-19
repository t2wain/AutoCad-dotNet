using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace AutoCadShared
{
    /// <summary>
    /// Navigating the AutoCAD collections.
    /// From ObjectId, retrieve DBObject, then cast to correct entity types
    /// </summary>
    public static class AcadCollection
    {
        #region Standard Table

        /// <summary>
        /// Get BlockTable
        /// </summary>
        public static ObjectId GetBlockTable(Database acDB) => acDB.BlockTableId;

        /// <summary>
        /// Get DimStyleTable
        /// </summary>
        public static ObjectId GetDimStyleTable(Database acDB) => acDB.DimStyleTableId;

        /// <summary>
        /// Get LayerTable
        /// </summary>
        public static ObjectId GetLayerTable(Database acDB) => acDB.LayerTableId;

        /// <summary>
        /// Get LinetypeTable
        /// </summary>
        public static ObjectId GetLinetypeTable(Database acDB) => acDB.LinetypeTableId;

        /// <summary>
        /// Get RegAppTable
        /// </summary>
        public static ObjectId GetRegAppTable(Database acDB) => acDB.RegAppTableId;

        /// <summary>
        /// Get TextStyleTable
        /// </summary>
        public static ObjectId GetTextStyleTable(Database acDB) => acDB.TextStyleTableId;

        #endregion

        /// <summary>
        /// Get all BlockTableRecord
        /// </summary>
        public static ObjectIdCollection GetBlockTableRecords(BlockTable bt) =>
            GetObjectIds(bt.Cast<ObjectId>());

        /// <summary>
        /// Get all entities of a BlockTableRecord
        /// </summary>
        public static ObjectIdCollection GetEntities(BlockTableRecord btr) =>
            GetObjectIds(btr.Cast<ObjectId>());

        /// <summary>
        /// Get all LayerTableRecord
        /// </summary>
        public static ObjectIdCollection GetLayerTableRecord(LayerTable ltr) =>
            GetObjectIds(ltr.Cast<ObjectId>());

        public static ObjectIdCollection GetObjectIds(IEnumerable<ObjectId> objIds) =>
            GetObjectIds(objIds, new ObjectIdCollection());

        public static ObjectIdCollection GetObjectIds(IEnumerable<ObjectId> objIds, ObjectIdCollection colId) =>
            objIds.Aggregate(colId, (col, id) =>
            {
                if (!col.Contains(id))
                    col.Add(id);
                return col;
            });
    }
}
