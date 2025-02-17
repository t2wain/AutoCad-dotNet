using AcadTest;
using Autodesk.AutoCAD.Runtime;

namespace AutoCadLib
{
    public static class AcadCommands
    {
        [CommandMethod("RunTest")]
        public static void RunTest()
        {
            Test.Run();
        }
    }

}
