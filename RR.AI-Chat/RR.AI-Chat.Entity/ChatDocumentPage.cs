using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(ChatDocumentPage), Schema = "Core")]
    public class ChatDocumentPage : BaseDocumentPage
    {
        public Guid SessionDocumentId { get; set; }

        public ChatDocument SessionDocument { get; set; } = null!;
    }
}
