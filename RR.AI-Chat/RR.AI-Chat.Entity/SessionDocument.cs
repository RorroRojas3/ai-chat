using RR.AI_Chat.Dto;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(SessionDocument), Schema = "Core")]
    public class SessionDocument : BaseDocument
    {
        public Guid SessionId { get; set; }

        public Session Session { get; set; } = null!;

        public List<SessionDocumentPage> Pages { get; set; } = [];
    }

    public static class SessionDocumentExtensions
    {
        public static SessionDocumentDto MapToSessionDocumentDto(this SessionDocument source)
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
