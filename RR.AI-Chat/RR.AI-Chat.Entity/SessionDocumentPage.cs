using System;
using System.Collections.Generic;
using System.Text;

namespace RR.AI_Chat.Entity
{
    public class SessionDocumentPage : BaseDocumentPage
    {
        public Guid SessionDocumentId { get; set; }

        public SessionDocument SessionDocument { get; set; } = null!;
    }
}
