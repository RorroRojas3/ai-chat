namespace RR.AI_Chat.Dto
{
    public class FileDataDto
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = [];
        public string ContentType { get; set; } = string.Empty;
        public long Length { get; set; }
    }
}