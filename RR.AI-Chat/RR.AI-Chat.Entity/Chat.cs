using RR.AI_Chat.Dto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(Chat), Schema = "Core")]
    public class Chat : BaseModifiedEntity
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(Project))]
        public Guid? ProjectId { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = "New Chat";

        public long InputTokens { get; set; }

        public long OutputTokens { get; set; }

        public long TotalTokens => InputTokens + OutputTokens;

        public User User { get; set; } = null!;

        public Project? Project { get; set; }

        public List<ChatDocument> Documents { get; set; } = [];
    }

    public static class ChatExtensions
    {
        public static ChatDto MapToChatDto(this Chat source)
        {
            return new ChatDto
            {
                Id = source.Id,
                ProjectId = source.ProjectId,
                Name = source.Name,
                DateCreated = source.DateCreated,
                DateModified = source.DateModified
            };
        }
    }
}
