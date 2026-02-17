using RR.AI_Chat.Dto;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(ChatDocument), Schema = "Core")]
    public class ChatDocument : BaseDocument
    {
        public Guid SessionId { get; set; }

        public Session Session { get; set; } = null!;

        public List<ChatDocumentPage> Pages { get; set; } = [];
    }

    public static class SessionDocumentExtensions
    {
        public static SessionDocumentDto MapToSessionDocumentDto(this ChatDocument source)
        {
            return new SessionDocumentDto
            {
                Id = source.Id,
                SessionId = source.SessionId,
                Name = source.Name
            };
        }
    }
}
