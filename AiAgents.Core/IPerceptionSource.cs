using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiAgents.Core
{
    public interface IPerceptionSource<TPercept>
    {
        Task<TPercept?> PerceiveAsync(CancellationToken ct);
        bool HasNext();
    }
}