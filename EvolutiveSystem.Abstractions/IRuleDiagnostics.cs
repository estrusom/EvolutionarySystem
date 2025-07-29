using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutiveSystem.Abstractions
{
    public enum FailureReason
    {
        NoMatch,
        DidNotReachTarget,
        InfiniteLoopDetected,
        TooManySteps,
        Other
    }
    public interface IRuleFailureDetail
    {
    }
    public interface IAntithesisWithDiagnosticDetails
    {

    }
}
