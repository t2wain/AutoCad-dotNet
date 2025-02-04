using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;

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
        public static void WriteEntities(Document doc, 
            IEnumerable<Entity> entities, Transaction curTran = null)
        {
            Database db = doc.Database;
            Transaction tran = curTran ?? db.TransactionManager.StartTransaction();
            try 
            {
                // entities are stored in BlockTable / BlockTableRecord
                if (tran.GetObject(db.BlockTableId, OpenMode.ForRead) is BlockTable bt &&
                    tran.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) is BlockTableRecord btr)
                {
                    foreach (Entity ent in entities)
                    {
                        btr.AppendEntity(ent);
                        tran.AddNewlyCreatedDBObject(ent, true);
                    }
                }
                if (curTran == null)
                    tran.Commit();
            }
            catch
            {
                if (curTran == null)
                    tran.Abort();
                throw;
            }
            finally
            {
                if (curTran == null)
                    tran.Dispose();
            }
        }

        /// <summary>
        /// Create an app record for storing xdata to document
        /// </summary>
        /// <param name="curTran">Current transaction</param>
        public static void AddApp(Document doc, string appName, Transaction curTran = null)
        {
            Database db = doc.Database;
            Transaction tran = curTran ?? db.TransactionManager.StartTransaction();
            try
            {
                if (tran.GetObject(db.RegAppTableId, OpenMode.ForRead) is RegAppTable regtbl 
                    && !regtbl.Has(appName))
                {
                    var app = new RegAppTableRecord();
                    app.Name = appName;
                    regtbl.Add(app);
                    tran.AddNewlyCreatedDBObject(app, true);
                    if (curTran == null)
                        tran.Commit();
                }
            }
            catch
            {
                if (curTran == null)
                    tran.Abort();
                throw;
            }
            finally
            { 
                if (curTran == null) 
                    tran.Dispose(); 
            }
        }

        /// <summary>
        /// Add xdata to an entity. Be sure an app record
        /// for the app name already existed.
        /// </summary>
        /// <param name="curTran">Current transaction</param>
        public static void AddXDataToEntity<T>(Document doc, Entity entity, 
            string appName, T xdValue, Transaction curTran = null)
        {
            var data = BuildXData(xdValue);
            Database db = doc.Database;
            Transaction tran = curTran ?? db.TransactionManager.StartTransaction();

            // be sure an app record already existed
            // for the appName
            var rb = new ResultBuffer(
                new TypedValue(1001, appName),
                data);
            try
            {
                entity.XData = rb;
                if (curTran == null)
                    tran.Commit();
            }
            catch
            {
                if (curTran == null)
                    tran.Abort();
                throw;
            }
            finally
            {
                rb.Dispose();
                if (curTran == null)
                    tran.Dispose();
            }
        }

        /// <summary>
        /// Build XData. Only a limited number of data types can be stored
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static TypedValue BuildXData<T>(T xdValue)
        {
            var data = new TypedValue();
            switch (xdValue)
            {
                case int v:
                    data = new TypedValue((int)DxfCode.ExtendedDataInteger32, v);
                    break;
                default:
                    throw new ArgumentException();
            }
            return data;
        }
    }
}
