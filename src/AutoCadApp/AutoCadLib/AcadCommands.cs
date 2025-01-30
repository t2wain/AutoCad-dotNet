
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
            var lst = AcadRead.LoadPolyLines(doc);
            lst = AcadRead.LoadLines(doc);
            lst = AcadRead.LoadBlocks(doc);
            lst = AcadRead.LoadMText(doc);

            lst = AcadRead.LoadBlocks(doc, "Raceway");
            lst = AcadRead.LoadBlocks(doc, "Drop");
            lst = AcadRead.LoadBlocks(doc, "EquipNode");
            lst = AcadRead.LoadBlocks(doc, "RwNode");

            lst = AcadRead.LoadBlocks(doc, new[] { "Raceway", "Drop", "EquipNode", "RwNode" });
        }
    }

}
