namespace RR.AI_Chat.Service
{
    public class DocumentStore
    {
        public List<DocumentSession> DocumentSessions { get; set; } = [];
    }

    public class DocumentSession
    {
        public Guid SessionId { get; set; }
    }
}
