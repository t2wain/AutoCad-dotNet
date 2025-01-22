using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace AutoCadShared
{
    public static class AcadRead
    {
        public static List<dynamic> LoadBlocks(Document doc)
        {
            using (var tran = doc.TransactionManager.StartTransaction())
            {
                var lst = doc.GetSelection("INSERT")
                    .Select(obj => tran.GetObject(obj.ObjectId, OpenMode.ForRead))
                    .Cast<BlockReference>()
                    .Where(bl => !bl.Name.Contains("*")) // exclude anonymous blocks
                    .Select(bl => 
                    {
                        dynamic o = new ExpandoObject();
                        o.ID = bl.Id;
                        o.PosX = bl.Position.X;
                        o.PosY = bl.Position.Y;
                        o.PosZ = bl.Position.Z;
                        o.Name = bl.Name;
                        o.Name = bl.Layer;
                        o.Rotation = bl.Rotation;
                        o.Attrs = bl.AttributeCollection
                            .Cast<ObjectId>()
                            .Select(attrId => tran.GetObject(attrId, OpenMode.ForRead))
                            .Cast<AttributeReference>()
                            .Select(attref => new { attref.Tag, attref.TextString })
                            .ToList();
                        return o;
                    })
                    .ToList();
                return lst;
            }
        }

        public static List<dynamic> LoadPolyLines(Document doc)
        {
            using (var tran = doc.TransactionManager.StartTransaction())
            {
                return doc.GetSelection("LWPOLYLINE")
                    .Select(obj => tran.GetObject(obj.ObjectId, OpenMode.ForRead))
                    .Cast<Polyline>()
                    .Select(pl =>
                    {
                        dynamic o = new ExpandoObject();
                        o.Id = pl.Id;
                        o.Layer = pl.Layer;
                        o.LineType = pl.Linetype;
                        o.Length = pl.Length;
                        o.Closed = pl.Closed;
                        o.Points = Enumerable.Range(0, pl.NumberOfVertices)
                            .Select(i => pl.GetPoint3dAt(i))
                            .ToList();
                        return o;
                    })
                    .ToList();
            }
        }

        public static List<dynamic> LoadLines(Document doc)
        {
            using (var tran = doc.TransactionManager.StartTransaction())
            {
                return doc.GetSelection("LINE")
                    .Select(obj => tran.GetObject(obj.ObjectId, OpenMode.ForRead))
                    .Cast<Line>()
                    .Select(line => 
                    {
                        dynamic o = new ExpandoObject();
                        o.Id = line.Id;
                        o.StartX = line.StartPoint.X;
                        o.StartY = line.StartPoint.Y;
                        o.EndX = line.EndPoint.X;
                        o.EndY = line.EndPoint.Y;
                        o.Layer = line.Layer;
                        o.LineType = line.Linetype;
                        o.Color = line.Color.ToString();
                        o.Length = line.Length;
                        return o;
                    })
                    .ToList();
            }
        }

        public static List<dynamic> LoadMText(Document doc)
        {
            using (var tran = doc.TransactionManager.StartTransaction())
            {
                return doc.GetSelection("MTEXT")
                    .Select(obj => tran.GetObject(obj.ObjectId, OpenMode.ForRead))
                    .Cast<MText>()
                    .Select(mt =>
                    {
                        dynamic o = new ExpandoObject();
                        o.Id = mt.Id;
                        o.LocX = mt.Location.X;
                        o.LocY = mt.Location.Y;
                        o.Layer = mt.Layer;
                        o.Color = mt.Color.ToString();
                        o.TextStyleName = mt.TextStyleName;
                        o.TextHeight = mt.TextHeight;
                        o.Width = mt.Width;
                        o.Attachment = Convert.ToInt32(mt.Attachment);
                        o.Text = mt.Text;
                        return o;
                    })
                    .ToList();
            }
        }

        static IEnumerable<SelectedObject> GetSelection(this Document doc, string entityType)
        {
            var filter = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, entityType) });
            PromptSelectionResult ssPrompt = doc.Editor.SelectAll(filter);
            if (ssPrompt.Status != PromptStatus.OK) 
                return new List<SelectedObject>();
            else return ssPrompt.Value.Cast<SelectedObject>();
        }
    }
}

