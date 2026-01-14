using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiAgents.Core
{
    public interface IPolicy<TPercept, TAction>
    {
        Task<TAction> DecideAsync(TPercept percept, CancellationToken ct);
    }
}