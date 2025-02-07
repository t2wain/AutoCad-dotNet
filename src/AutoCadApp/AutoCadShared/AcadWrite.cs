using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace AutoCadShared
{
    /// <summary>
    /// Write data to document. Be sure document is locked first.
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

    }
}
