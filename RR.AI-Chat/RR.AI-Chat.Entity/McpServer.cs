using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(McpServer), Schema = "AI")]
    public class McpServer : BaseEntity
    {
        [StringLength(250)]
        public string Name { get; set; } = null!;

        [StringLength(250)]
        public string Command { get; set; } = null!;

        public List<string> Arguments { get; set; } = null!;

        [StringLength(2000)]
        public string? WorkingDirectory { get; set; }
    }
}
