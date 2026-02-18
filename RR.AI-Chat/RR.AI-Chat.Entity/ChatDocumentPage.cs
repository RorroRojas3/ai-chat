using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(ChatDocumentPage), Schema = "Core")]
    public class ChatDocumentPage : BaseDocumentPage
    {
        public Guid ChatDocumentId { get; set; }

        public ChatDocument ChatDocument { get; set; } = null!;
    }
}
