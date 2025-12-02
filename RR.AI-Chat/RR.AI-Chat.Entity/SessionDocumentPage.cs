using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(SessionDocumentPage), Schema = "Core")]
    public class SessionDocumentPage : BaseDocumentPage
    {
        public Guid SessionDocumentId { get; set; }

        public SessionDocument SessionDocument { get; set; } = null!;
    }
}
