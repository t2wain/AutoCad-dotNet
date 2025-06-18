using AutoCadComShared;
using System.Text.RegularExpressions;

namespace ACadConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AcFactory.Singleton = new AcFactoryImpl();

            using (var app = AcApp.GetInstance())
            using (var doc = app.GetActiveDocument())
            {
                //AcRead.ReadObjectCollection(doc.Database);

                var regEx = new Regex("mto_*", RegexOptions.IgnoreCase);
                var dwg = AcRead.ReadDrawingDTO(doc.Database, doc.FileName, regEx);
            }
        }
    }
}
