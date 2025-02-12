using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCadShared
{
    public static class AcadUtil
    {
        #region Entity Filtering

        /// <summary>
        /// Select block entities by block names
        /// </summary>
        public static IEnumerable<SelectedObject> GetBlockSelection(Document doc, string[] blockNames)
        {
            var q = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Operator, DxfCodeOperator.AND1),
                new TypedValue((int)DxfCode.Start, DxfEntity.INSERT.ToString()),
                new TypedValue((int)DxfCode.Operator, DxfCodeOperator.OR1),
            }
            .Concat(blockNames.Select(n => new TypedValue((int)DxfCode.BlockName, n)))
            .Concat(new TypedValue[]
            {
                new TypedValue((int)DxfCode.Operator, DxfCodeOperator.OR1),
                new TypedValue((int)DxfCode.Operator, DxfCodeOperator.AND1)
            });

            var filter = new SelectionFilter(q.ToArray());
            return GetSelection(doc, filter);
        }

        /// <summary>
        /// Get a selection of all entities of certain type in the drawing
        /// </summary>
        public static IEnumerable<SelectedObject> GetSelection(Document doc, DxfEntity dxfEntity) =>
            GetSelection(doc, new SelectionFilter(
                new TypedValue[]
                {
                    new TypedValue((int)DxfCode.Start, dxfEntity.ToString())
                }
            ));

        /// <summary>
        /// Get a selection of all entities of certain type in the drawing.
        /// return IEnumerable for use with LINQ
        /// </summary>
        public static IEnumerable<SelectedObject> GetSelection(Document doc, SelectionFilter filter)
        {
            // select entities based on filter
            PromptSelectionResult ssPrompt = doc.Editor.SelectAll(filter);

            if (ssPrompt.Status != PromptStatus.OK)
                return Array.Empty<SelectedObject>();
            // ssPrompt.Value return non-generic ICollection : IEnumberable
            // Cast<> convert to generic IEnumerable for use with most LINQ functions
            else return ssPrompt.Value.Cast<SelectedObject>();
        }

        #endregion

        /// <summary>
        /// Build XData. Only a limited number of data types can be stored
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static TypedValue BuildXData<T>(T xdValue)
        {
            var data = new TypedValue();
            switch (xdValue)
            {
                case int v:
                    data = new TypedValue((int)DxfCode.ExtendedDataInteger32, v);
                    break;
                default:
                    throw new ArgumentException();
            }
            return data;
        }

    }
}
