namespace AcadCommon.DTO
{
    public class BlockDTO
    {
        public string Id { get; set; }
        public string BlockId {get; set;}
        public string BlockName {get; set;}
        public string BlockTableRecord {get; set;}
        public double PositionX {get; set;}
        public double PositionY {get; set;}
        public double PositionZ {get; set;}
        public string Name {get; set;}
        public string Layer {get; set;}
        public double Rotation { get; set; }
        public BlockAttrDTO[] Attrs {get; set;}

    }
}
