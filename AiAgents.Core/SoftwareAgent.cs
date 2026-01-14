using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AiAgents.Core
{
    public abstract class SoftwareAgent<TPercept, TAction, TResult, TExperience>
    {
        protected readonly IPerceptionSource<TPercept> PerceptionSource;
        protected readonly IPolicy<TPercept, TAction> Policy;
        protected readonly IActuator<TAction, TResult> Actuator;
        protected readonly ILearningComponent<TExperience>? LearningComponent;

        protected SoftwareAgent(
            IPerceptionSource<TPercept> perceptionSource,
            IPolicy<TPercept, TAction> policy,
            IActuator<TAction, TResult> actuator,
            ILearningComponent<TExperience>? learningComponent = null)
        {
            PerceptionSource = perceptionSource;
            Policy = policy;
            Actuator = actuator;
            LearningComponent = learningComponent;
        }

        public abstract Task<TResult?> StepAsync(CancellationToken ct);
    }
}