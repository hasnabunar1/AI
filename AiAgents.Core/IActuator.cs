using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AiAgents.Core
{
    public interface IActuator<TAction, TResult>
    {
        Task<TResult> ExecuteAsync(TAction action, CancellationToken ct);
    }
}