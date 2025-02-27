namespace RR.AI_Chat.Service
{
    public class DocumentStore
    {
        public List<DocumentSession> DocumentSessions { get; set; } = [];
    }

    public class DocumentSession
    {
        public Guid SessionId { get; set; }

        public List<Document> Documents { get; set; } = [];
    }

    public class Document
    {
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }

        public string Name { get; set; } = null!;

        public byte[] Bytes { get; set; } = null!;

        public string Extension { get; set; } = null!;

        public float[] Vector { get; set; } = null!;
    }
}
