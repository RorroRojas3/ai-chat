using RR.AI_Chat.Dto;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(ChatDocument), Schema = "Core")]
    public class ChatDocument : BaseDocument
    {
        public Guid ChatId { get; set; }

        public Chat Chat { get; set; } = null!;

        public List<ChatDocumentPage> Pages { get; set; } = [];
    }

    public static class ChatDocumentExtensions
    {
        public static ChatDocumentDto MapToChatDocumentDto(this ChatDocument source)
        {
            return new ChatDocumentDto
            {
                Id = source.Id,
                ChatId = source.ChatId,
                Name = source.Name
            };
        }
    }
}
