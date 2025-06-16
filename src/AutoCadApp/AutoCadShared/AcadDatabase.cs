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
        bool _disposeAll = false;

        /// <summary>
        /// Open the drawing database without loading the
        /// drawing to the editor
        /// </summary>
        public AcadDatabase(string filePath,
            FileOpenMode mode = FileOpenMode.OpenForReadAndReadShare)
        {
            _acDB = new Database(false, true);
            _acDB.ReadDwgFile(filePath, mode, true, string.Empty);
            _acTran = _acDB.TransactionManager.StartTransaction();

            // ensure the database and transaction will be dispose.
            _disposeAll = true;
        }

        public AcadDatabase(Database acDB, Transaction acTran)
        {
            this._acDB = acDB;
            this._acTran = acTran;
        }

        public void Dispose()
        {
            if (_disposeAll)
            {
                this._acTran.Dispose();
                this._acDB.Dispose();
            }
            this._acDB = null;
            this._acTran = null;
        }

        public Transaction AcadTran => _acTran;

        public Database AcadDB => _acDB;

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
        public IEnumerable<DBObject> GetDBOjects(ObjectIdCollection objIds,
            OpenMode mode = OpenMode.ForRead) =>
            objIds.Cast<ObjectId>()
                .Select(oid => GetDBObject(oid, mode))
                .Where(obj => obj != null)
                .ToList();

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
            GetEntities<T>(selection.Select(i => i.ObjectId), mode);

        public IEnumerable<T> GetEntities<T>(IEnumerable<ObjectId> objIds,
            OpenMode mode = OpenMode.ForRead) where T : DBObject =>
            objIds.Select(id => GetObject<T>(id, mode))
                .Where(o => o != null).ToList();


        #endregion

        #region Add to Dabase

        public ObjectId AddEntity(BlockTableRecord btr, Entity ent)
        {

            var id = btr.AppendEntity(ent);
            AcadTran.AddNewlyCreatedDBObject(ent, true);
            return id;
        }

        public void AddEntityToModelSpace(Entity ent) => 
            AddEntity(GetModelSpaceBlockTableRecord(), ent);

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
