using AcadCommon.DTO;
using System.IO;
using System.Xml.Serialization;

namespace AcadCommon
{
    public static class BlockData
    {
        public static string SerializeToXml(DrawingDTO dwg)
        {
            XmlSerializer x = new XmlSerializer(typeof(DrawingDTO));
            var sb = new StringWriter();
            x.Serialize(sb, dwg);
            sb.Flush();
            return sb.ToString();
        }

        public static DrawingDTO ParseXmlToDrawing(string xml)
        {
            XmlSerializer x = new XmlSerializer(typeof(DrawingDTO));
            return x.Deserialize(new StringReader(xml)) as DrawingDTO;
        }

        public static AcadRunConfig ParseXmlToAcadConfig(string xml)
        {
            XmlSerializer x = new XmlSerializer(typeof(AcadRunConfig));
            return x.Deserialize(new StringReader(xml)) as AcadRunConfig;
        }

        public static string SerializeToXml(AcadRunConfig config)
        {
            XmlSerializer x = new XmlSerializer(typeof(AcadRunConfig));
            var sb = new StringWriter();
            x.Serialize(sb, config);
            sb.Flush();
            return sb.ToString();
        }

    }
}
