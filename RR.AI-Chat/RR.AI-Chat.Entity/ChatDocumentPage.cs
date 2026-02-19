using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(ChatDocumentPage), Schema = "Core")]
    public class ChatDocumentPage : BaseDocumentPage
    {
        public Guid ConversationDocumentId { get; set; }

        public ChatDocument ConversationDocument { get; set; } = null!;
    }
}
