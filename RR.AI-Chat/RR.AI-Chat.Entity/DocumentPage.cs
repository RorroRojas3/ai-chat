﻿using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(DocumentPage), Schema = "AI")]
    public class DocumentPage : BaseEntity
    {
        [ForeignKey(nameof(Document))]
        public Guid DocumentId { get; set; }

        public int Number { get; set; }

        public string Text { get; set; } = null!;

        [Column(TypeName = "float[]")]
        public float[] Vector { get; set; } = null!;

        public Document Document { get; set; } = null!;
    }
}
