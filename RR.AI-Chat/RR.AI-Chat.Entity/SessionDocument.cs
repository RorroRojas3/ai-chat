namespace RR.AI_Chat.Entity
{
    public class SessionDocument : BaseDocument
    {
        public Guid SessionId { get; set; }

        public Session Session { get; set; } = null!;

        public List<SessionDocumentPage> Pages { get; set; } = [];
    }
}
