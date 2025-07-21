using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(Model), Schema = "AI.Ref")]
    public class Model : BaseEntity
    {
        [ForeignKey(nameof(AIService))]
        public Guid AIServiceId { get; set; }

        [StringLength(25)]
        public string Name { get; set; } = null!;

        public bool IsToolEnabled { get; set; }

        public AIService AIService { get; set; } = null!;
    }
}
