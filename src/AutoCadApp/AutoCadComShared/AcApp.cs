using Autodesk.AutoCAD.Interop;
using System;

namespace AutoCadComShared
{
    public class AcApp : IDisposable
    {
        protected static AcApp APP;

        protected AcadApplication mAcadApp = null;
        protected bool mCreateNew = false;
        protected bool mIsCreatedNew = false;
        protected bool mVisible = false;

        public static AcApp GetInstance()
        {
            return AcApp.GetInstance(false, false);
        }

        public static AcApp GetInstance(bool pCreateNew, bool pVisible)
        {
            if (APP == null)
            {
                APP = new AcApp(pCreateNew, pVisible);
                APP.GetApplication();
            }
            return APP;
        }

        public static void DisposeSingleton()
        {
            AcApp.APP = null;
        }

        protected AcApp(AcFactory fact) : this(false, false) { }

        protected AcApp(bool pCreateNew, bool pVisible) 
        {
            this.mCreateNew = pCreateNew;
            this.mVisible = pVisible;
        }

        public static void CloseApp()
        {
            if (APP != null)
                APP.CloseAcadApp();
        }

        public bool IsAcadActiveDocument => 
            this.IsAcadCreated && this.GetApplication().Documents.Count > 0;

        public bool IsAcadCreated => this.GetApplication() != null;

        public IAcadApplication GetApplication() => this.GetApplication(this.mVisible);

        public virtual IAcadApplication GetApplication(Boolean pVisible) 
        {
            if (this.mAcadApp == null)
            {
                try
                {
                    this.mAcadApp = AcFactory.Singleton.GetAutoCadApp();
                    this.mIsCreatedNew = false;
                }
                catch { }

                if (this.mAcadApp == null && this.mCreateNew)
                {
                    this.mAcadApp = AcFactory.Singleton.NewAutoCadApp();
                    this.mAcadApp.Visible = pVisible;
                    this.mIsCreatedNew = true;
                }
            }
            return this.mAcadApp;
        }

        public void CloseAcadApp() 
        {
            if (this.mIsCreatedNew && this.mAcadApp != null)
                this.mAcadApp.Application.Quit();
            this.mAcadApp = null;
        }

        public AcDoc GetActiveDocument() 
        {
            AcDoc lDoc = null;
            if (this.IsAcadActiveDocument) 
            {
                IAcadDocument lAcadDoc = this.GetApplication().ActiveDocument;
                lDoc = new AcDoc(lAcadDoc, AcApp.GetInstance());
            }
            return lDoc;
        }

        public AcDoc LoadDocument(string pFileName) 
        {
            IAcadDocument lAcadDoc = this.GetApplication().Documents.Open(pFileName, true);
            AcDoc lDoc = new AcDoc(lAcadDoc, AcApp.GetInstance());
            return lDoc;
        }

        public AcDoc CreateDocument() 
        {
            IAcadDocument lAcadDoc = this.GetApplication().Documents.Add();
            AcDoc lDoc = new AcDoc(lAcadDoc, AcApp.GetInstance());
            return lDoc;
        }

        public virtual void Dispose() 
        {
            this.CloseAcadApp();
        }
    }
}
