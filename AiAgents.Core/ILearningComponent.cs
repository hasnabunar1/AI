using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiAgents.Core
{
    public interface ILearningComponent<TExperience>
    {
        Task LearnAsync(TExperience experience, CancellationToken ct);
        Task<bool> ShouldRetrainAsync(CancellationToken ct);
    }
}