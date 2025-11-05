using RR.AI_Chat.Dto;
using RR.AI_Chat.Service.Common.Interface;
using System.Text;

namespace RR.AI_Chat.Service
{
    public class CommonFileService : IFileService
    {
        public List<DocumentExtractorDto> ExtractText(byte[] bytes, string fileName)
        {
            var text = Encoding.UTF8.GetString(bytes);

            return
            [
                new DocumentExtractorDto
                {
                    PageNumber = 1,
                    PageText = text
                }
            ];
        }
    }
}
