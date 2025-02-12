using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCadShared
{
    /// <summary>
    /// Group convenience methods that depend 
    /// on Database and Transaction
    /// </summary>
    public class AcadDatabase : IDisposable
    {
        #region Init

        Database _acDB;
        Transaction _acTran;

        public AcadDatabase(Database acDB, Transaction acTran)
        {
            this._acDB = acDB;
            this._acTran = acTran;
        }

        public void Dispose()
        {
            this._acDB = null;
            this._acTran = null;
        }

        public Transaction AcadTran => _acTran;

        protected Database AcadDB => _acDB;

        #endregion

        #region ObjectId and DBObject

        /// <summary>
        /// Cast a DBObject to a specific object type
        /// </summary>
        public T CastObject<T>(DBObject obj) where T : DBObject
        {
            if (obj is T)
                return (T)Convert.ChangeType(obj, typeof(T));
            else return default;
        }

        /// <summary>
        /// Get a DBObject given an ObjectId
        /// </summary>
        public DBObject GetDBObject(ObjectId id, OpenMode mode = OpenMode.ForRead) => 
            AcadTran.GetObject(id, mode);

        /// <summary>
        /// Get a DBObject given an ObjectId and cast it to a specific object type
        /// </summary>
        public T GetObject<T>(ObjectId id, OpenMode mode = OpenMode.ForRead) where T : DBObject =>
            CastObject<T>(GetDBObject(id, mode));

        /// <summary>
        /// Get a DBObject given an ObjectId
        /// </summary>
        public IEnumerable<DBObject> GetDBOjects(IEnumerable<ObjectId> objIds,
            OpenMode mode = OpenMode.ForRead) =>
            objIds.Select(obj => GetDBObject(obj, mode));

        #endregion

        #region Object Model

        /// <summary>
        /// Get the BlockTable object
        /// </summary>
        public BlockTable GetBlockTable(OpenMode mode = OpenMode.ForRead) =>
            GetObject<BlockTable>(_acDB.BlockTableId, mode);

        /// <summary>
        /// Get the Model Space BlockTableRecord
        /// </summary>
        public BlockTableRecord GetModelSpaceBlockTableRecord(BlockTable bt,
            OpenMode mode = OpenMode.ForRead) =>
            GetObject<BlockTableRecord>(bt[BlockTableRecord.ModelSpace], mode);

        /// <summary>
        /// Get the Model Space BlockTableRecord
        /// </summary>
        public BlockTableRecord GetModelSpaceBlockTableRecord(OpenMode mode = OpenMode.ForRead) =>
            GetObject<BlockTableRecord>(GetBlockTable(mode)[BlockTableRecord.ModelSpace], mode);

        /// <summary>
        /// Get the Paper Space BlockTableRecord
        /// </summary>
        public BlockTableRecord GetPaperSpaceBlockTableRecord(BlockTable bt,
            OpenMode mode = OpenMode.ForRead) =>
            GetObject<BlockTableRecord>(bt[BlockTableRecord.PaperSpace], mode);

        /// <summary>
        /// Get the Paper Space BlockTableRecord
        /// </summary>
        public BlockTableRecord GetPaperSpaceBlockTableRecord(OpenMode mode = OpenMode.ForRead) =>
            GetObject<BlockTableRecord>(GetBlockTable(mode)[BlockTableRecord.PaperSpace], mode);

        /// <summary>
        /// Get RegAppTable
        /// </summary>
        public RegAppTable GetRegAppTable(OpenMode mode = OpenMode.ForRead) =>
            GetObject<RegAppTable>(_acDB.RegAppTableId, mode);

        public IEnumerable<T> GetEntities<T>(IEnumerable<SelectedObject> selection,
            OpenMode mode = OpenMode.ForRead) where T : DBObject =>
            selection
                .Select(i => i.ObjectId)
                .Select(id => GetObject<T>(id, mode));

        public IEnumerable<T> GetEntities<T>(IEnumerable<ObjectId> objIds,
            OpenMode mode = OpenMode.ForRead) where T : DBObject =>
            objIds.Select(id => GetObject<T>(id, mode));


        #endregion

        #region Add to Dabase

        public void AddEntity(BlockTableRecord btr, Entity ent)
        {

            btr.AppendEntity(ent);
            AcadTran.AddNewlyCreatedDBObject(ent, true);
        }

        /// <summary>
        /// Add xdata to an entity. Be sure an app record
        /// for the app name already existed.
        /// </summary>
        public void AddXData<T>(Entity ent, T xdValue, string appName)
        {
            var rb = new ResultBuffer(
                new TypedValue(1001, appName),
                AcadUtil.BuildXData(xdValue));
            ent.XData = rb;
            rb.Dispose();
        }

        public void AddAppRecord(RegAppTable regAppTbl, string appName)
        {
            if (!regAppTbl.Has(appName))
            {
                var app = new RegAppTableRecord();
                app.Name = appName;
                regAppTbl.Add(app);
                AcadTran.AddNewlyCreatedDBObject(app, true);
            }
        }

        #endregion

    }
}
