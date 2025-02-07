using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCadShared
{
    public class AcadDocument : IDisposable
    {
        #region Init

        public AcadDocument(Document doc) : this(doc, null) { }

        public AcadDocument(Document doc, Transaction tran)
        {
            this._doc = doc;
            this._extTran = tran;
        }

        public void Dispose()
        {
            CleanUp();
            _doc = null;
            _extTran = null;
        }

        public void CleanUp()
        {
            if (_tran != null)
            {
                _tran.Dispose();
                _tran = null;
            }
            if (_db != null)
            {
                _db.Dispose();
                _db = null;
            }
        }

        #endregion

        #region States

        Document _doc = null;
        Transaction _extTran = null;
        Transaction _tran = null;

        public Transaction GetTransaction() {
            if (_extTran !=  null)
                return _extTran;
            
            if (_tran == null)
                this._tran = GetNewTransaction();

            return _tran;
        }

        AcadDatabase _db = null;
        public AcadDatabase GetDatabase()
        {
            if( _db == null)
                _db = new AcadDatabase(_doc.Database, GetTransaction());
            return _db;
        }

        public AcadDatabase GetDatabase(Transaction tran) => 
            new AcadDatabase(_doc.Database, tran);

        #endregion

        #region Common

        public static AcadDocument ActiveDocument =>
            new AcadDocument(Application.DocumentManager.MdiActiveDocument);

        public Transaction GetNewTransaction() => 
            this._doc.TransactionManager.StartTransaction();

        #endregion

        #region Get Entities by Selection

        public IEnumerable<T> GetEntities<T>(DxfEntity entType, 
            OpenMode mode = OpenMode.ForRead) where T : DBObject =>
            GetDatabase().GetEntities<T>(AcadUtil.GetSelection(_doc, entType), mode);

        public IEnumerable<BlockReference> GetBlocks(OpenMode mode = OpenMode.ForRead) =>
            GetEntities<BlockReference>(DxfEntity.INSERT, mode)
                .Where(bl => !bl.Name.Contains("*")); // exclude anonymous blocks;

        public IEnumerable<AttributeReference> GetBlockAttributes(BlockReference block, 
            OpenMode mode = OpenMode.ForRead) =>
            GetDatabase().GetEntities<AttributeReference>(block.AttributeCollection.Cast<ObjectId>(), mode);

        public IEnumerable<Polyline> GetPolylines(OpenMode mode = OpenMode.ForRead) =>
            GetEntities<Polyline>(DxfEntity.POLYLINE, mode);

        public IEnumerable<Line> Getlines(OpenMode mode = OpenMode.ForRead) =>
            GetEntities<Line>(DxfEntity.LIGHT, mode);

        #endregion

    }
}
