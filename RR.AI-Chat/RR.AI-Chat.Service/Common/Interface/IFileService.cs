using RR.AI_Chat.Dto;

namespace RR.AI_Chat.Service.Common.Interface
{
    public interface IFileService
    {
        List<DocumentExtractorDto> ExtractText(byte[] bytes, string fileName);
    }
}
