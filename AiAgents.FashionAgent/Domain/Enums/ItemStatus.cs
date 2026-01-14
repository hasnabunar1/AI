using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiAgents.FashionAgent.Domain.Enums
{
    public enum ItemStatus
    {
        Queued,         // Čeka na obradu
        Processing,     // U obradi
        Recommended,    // Preporučen korisnicima
        Archived,       // Arhiviran (nije trending)
        PendingReview,  // Čeka ručnu provjeru
        Reviewed        // Ručno pregledan
    }
}