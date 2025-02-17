using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCadShared
{
    /// <summary>
    /// Common entity type (DxfCode.Start)
    /// used for entity filtering
    /// </summary>
    public enum DxfEntity
    {
        ATTDEF,
        ATTRIB,// AttributeReference
        BODY,
        CIRCLE,
        DIMENSION,
        ELLIPSE,
        HATCH,
        HELIX,
        IMAGE,
        INSERT, // BlockReference
        LEADER,
        LIGHT,
        LINE,
        LWPOLYLINE,
        MESH,
        MLINE,
        MLEADERSTYLE,
        MLEADER,
        MTEXT,
        OLEFRAME,
        OLE2FRAME,
        POINT,
        POLYLINE,
        RAY,
        REGION,
        SECTION,
        SEQEND,
        SHAPE,
        SOLID,
        SPLINE,
        SUN,
        SURFACE,
        TABLE,
        TEXT,
        TOLERANCE,
        TRACE,
        UNDERLAY,
        VERTEX,
        VIEWPORT,
        WIPEOUT,
        XLINE,
    }

    /// <summary>
    /// Common DxfCode used for entity filtering
    /// </summary>
    public enum DxfCodeFilter
    {
        /// <summary>
        /// 0 : string : see DxfEntity
        /// </summary>
        Start = DxfCode.Start,

        /// <summary>
        /// 2 : string
        /// </summary>
        BlockName = DxfCode.BlockName,

        /// <summary>
        /// 8 : string : "Layer 0"
        /// </summary>
        LayerName = DxfCode.LayerName,

        /// <summary>
        /// 60 : int : 0 = visible, 1 = invisible
        /// </summary>
        Visibility = DxfCode.Visibility,

        /// <summary>
        /// int : Use 0 or omitted = model space, 1 = paper space.
        /// </summary>
        ModelPaperSpace = 67,

        /// <summary>
        /// 62 : int
        /// Numeric index values ranging from 0 to 256.
        /// Zero indicates BYBLOCK. 256 indicates BYLAYER.
        /// A negative value indicates that the layer is turned off.
        /// </summary>
        Color = DxfCode.Color,

        Operator = DxfCode.Operator,

        Text = DxfCode.Text,
    }

    /// <summary>
    /// Symbol names and strings in selection filters can include wild-card patterns.
    /// 
    /// # (pound) : Matches any single numeric digit
    /// @ (at) : Matches any single alphabetic character
    /// . (period) : Matches any single non-alphanumeric character
    /// * (asterisk) : Matches any character sequence, including an empty one, and it can be used anywhere in the search pattern: at the beginning, middle, or end
    /// ? (question mark) : Matches any single character
    /// ~ (tilde) : If it is the first character in the pattern, it matches anything except the pattern
    /// [...] : Matches any one of the characters enclosed
    /// [~...] : Matches any single character not enclosed
    /// - (hyphen) : Used inside brackets to specify a range for a single character
    /// , (comma) : Separates two patterns
    /// `(reverse quote) : Escapes special characters (reads next character literally)
    /// </summary>
    public enum DxfCodeText
    {
        /// <summary>
        /// 2 : string
        /// </summary>
        BlockName = DxfCode.BlockName,

        /// <summary>
        /// 8 : string : "Layer 0"
        /// </summary>
        LayerName = DxfCode.LayerName,

        Text = DxfCode.Text,

    }

    /// <summary>
    /// Common operator (DxfCode.Operator) 
    /// used for entity filtering
    /// </summary>
    public static class DxfCodeOperator
    {
        public const string EQ = "=";
        public const string NEQ = "!=";
        public const string NEQ1 = "/=";
        public const string NEQ2 = "<>";
        public const string LT = "<";
        public const string LTE = "<=";
        public const string GT = ">";
        public const string GTE = ">=";
        public const string AND1 = "<AND";
        public const string AND2 = "AND>";
        public const string OR1 = "<OR";
        public const string OR2 = "OR>";
        public const string XOR1 = "<XOR";
        public const string XOR2 = "XOR>";
        public const string NOT1 = "<NOT";
        public const string NOT2 = "NOT>";
    }

    public static class DxfCodeUtil
    {
        public static IDictionary<string, int> GetDict()
        {
            var t = typeof(DxfCode);
            var names = Enum.GetNames(t);
            return names.Aggregate(new Dictionary<string, int>(), (dict, n) => 
            { 
                dict.Add(n, (int)Enum.Parse(t, n)); 
                return dict; 
            });
        }
    }

}
