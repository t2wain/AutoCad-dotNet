using Autodesk.AutoCAD.Interop.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutoCadComShared
{
    public static class AcCol
    {
        #region Standard Table

        /// <summary>
        /// Get BlockTable
        /// </summary>
        public static IAcadBlocks GetBlockTable(IAcadDatabase acDB) => acDB.Blocks;

        /// <summary>
        /// Get DimStyleTable
        /// </summary>
        public static IAcadDimStyles GetDimStyleTable(IAcadDatabase acDB) => acDB.DimStyles;

        /// <summary>
        /// Get LayerTable
        /// </summary>
        public static IAcadLayers GetLayerTable(IAcadDatabase acDB) => acDB.Layers;

        /// <summary>
        /// Get LinetypeTable
        /// </summary>
        public static IAcadLineTypes GetLinetypeTable(IAcadDatabase acDB) => acDB.Linetypes;

        /// <summary>
        /// Get RegAppTable
        /// </summary>
        public static IAcadRegisteredApplications GetRegAppTable(IAcadDatabase acDB) => acDB.RegisteredApplications;

        /// <summary>
        /// Get TextStyleTable
        /// </summary>
        public static IAcadTextStyles GetTextStyleTable(IAcadDatabase acDB) => acDB.TextStyles;

        #endregion

        /// <summary>
        /// Get all BlockTableRecord
        /// </summary>
        public static List<IAcadBlock> GetBlockTableRecords(IAcadBlocks bt) => 
            GetItems<IAcadBlock>(bt.GetEnumerator());

        /// <summary>
        /// Get all entities of a BlockTableRecord
        /// </summary>
        public static List<IAcadEntity> GetBlocks(IAcadBlock btr) => 
            GetItems<IAcadEntity>(btr.GetEnumerator());  

        /// <summary>
        /// Get all BlockReference from ModelSpace and PaperSpace
        /// </summary>
        public static List<IAcadBlockReference> GetBlockRefModelAndPaper(AcadDatabase acDB, Regex nameFilter = null)
        {
            var qMs = GetItems<IAcadBlockReference>(acDB.ModelSpace.GetEnumerator());
            var qPs = GetItems<IAcadBlockReference>(acDB.PaperSpace.GetEnumerator());
            var qAll = qMs.Concat(qPs);
            if (nameFilter != null)
                qAll = qAll.Where(b => nameFilter.IsMatch(b.Name));

            var dAll = new Dictionary<long, IAcadBlockReference>();
            qAll.Aggregate(dAll, (d, e) =>
            {
                if (!d.ContainsKey(e.ObjectID))
                    d.Add(e.ObjectID, e);
                return d;
            });

            return dAll.Values.ToList();
        }

        /// <summary>
        /// Get all LayerTableRecord
        /// </summary>
        public static List<IAcadLayer> GetLayerTableRecord(IAcadLayers ltr) => 
            GetItems<IAcadLayer>(ltr.GetEnumerator());

        static List<T> GetItems<T>(IEnumerator items)
        {
            var lst = new List<T>();
            while (items.MoveNext())
            {
                if (items.Current is T item)
                    lst.Add(item);
            }
            return lst;
        }
    }
}
