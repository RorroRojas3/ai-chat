using RR.AI_Chat.Dto;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(ChatDocument), Schema = "Core")]
    public class ChatDocument : BaseDocument
    {
        public Guid ConversationId { get; set; }

        public Chat Conversation { get; set; } = null!;

        public List<ChatDocumentPage> Pages { get; set; } = [];
    }

    public static class ChatDocumentExtensions
    {
        public static ChatDocumentDto MapToChatDocumentDto(this ChatDocument source)
        {
            return new ChatDocumentDto
            {
                Id = source.Id,
                ConversationId = source.ConversationId,
                Name = source.Name
            };
        }
    }
}
