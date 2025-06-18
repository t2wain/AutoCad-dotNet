using AcadAppLib;
using AutoCadComShared;
using Autodesk.AutoCAD.Interop;

namespace ACadConsoleApp
{
    internal class AcFactoryImpl : AcFactory
    {
        public override AcadApplication GetAutoCadApp() =>
            ComHelper.GetObject("AutoCAD.Application") as AcadApplication;

        public override AcadApplication NewAutoCadApp() =>
            ComHelper.CreateObject("AutoCAD.Application") as AcadApplication;
    }
}
