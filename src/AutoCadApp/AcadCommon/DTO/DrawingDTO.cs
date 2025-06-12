namespace AcadCommon.DTO
{
    public class DrawingDTO
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public BlockDTO[] MTOBlocks { get; set; }
        public bool IsScanError { get; set; }
        public string ErrorMessage { get; set; }
    }
}
