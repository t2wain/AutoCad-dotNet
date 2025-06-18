using Autodesk.AutoCAD.Interop;

namespace AutoCadComShared
{
    public abstract class AcFactory
    {
        public static AcFactory Singleton { get; set; }

        public abstract AcadApplication GetAutoCadApp();
        public abstract AcadApplication NewAutoCadApp();
    }
}
