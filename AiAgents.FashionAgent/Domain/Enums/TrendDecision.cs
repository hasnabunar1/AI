using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiAgents.FashionAgent.Domain.Enums
{
    public enum TrendDecision
    {
        Recommend,      // Visoka vjerovatnoća da je trending - preporuči korisniku
        Archive,        // Niska vjerovatnoća - arhiviraj
        PendingReview   // Srednja vjerovatnoća - potrebna ručna provjera
    }
}