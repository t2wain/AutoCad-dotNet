
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using AutoCadShared;

namespace AutoCadLib
{
    public static class AcadCommands
    {
        [CommandMethod("RunTest")]
        public static void RunTest()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            AcadTest.Read(doc);
        }
    }

}
