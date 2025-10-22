using System.ComponentModel;

namespace RR.AI_Chat.Dto.Enums
{
    public enum DocumentFormats
    {
        [Description("application/pdf")]
        Pdf = 1,
        
        [Description("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
        Word = 2,

        [Description("text/markdown")]
        Markdown = 3
    }
}
