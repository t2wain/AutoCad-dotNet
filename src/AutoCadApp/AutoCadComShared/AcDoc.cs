using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.Interop.Common;

namespace AutoCadComShared
{
    public class AcDoc : AcDocBase
    {

        protected IAcadDocument mDoc;
        protected AcApp mApp;

        public  AcDoc(IAcadDocument pDoc, AcApp pApp) 
        {
            this.mDoc = pDoc;
            this.mApp = pApp;
        }

        public IAcadDocument AcadDocument => this.mDoc;

        public string FileName => this.mDoc.Name;

        public override string Name => this.FileName;

        public override string FullName => this.mDoc.FullName;

        public AcadDatabase Database => this.mDoc.Database;

        public override void Close(bool pSaveChanges) => 
            this.mDoc.Close(pSaveChanges);
        
        public override void Dispose() 
        {
            if (this.mDoc != null)
                this.mDoc = null;
            this.mApp = null;
            base.Dispose();
        }

    }
}
