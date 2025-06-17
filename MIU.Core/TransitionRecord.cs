// creata 10.6.2025 23.19
using System;

namespace MIU.Core.Topology.Map
{
    /// <summary>
    /// Rappresenta un record di transizione tra due stati (stringhe)
    /// a seguito dell'applicazione di una regola MIU.
    /// Utilizzato per tracciare il percorso e raccogliere dati per l'apprendimento.
    /// </summary>
    public class TransitionRecord
    {
        public string StartString { get; }      // Stringa prima dell'applicazione della regola
        public string EndString { get; }        // Stringa dopo l'applicazione della regola
        public string AppliedRuleId { get; }    // L'ID/Nome della regola applicata (stringa)
        public bool IsSuccess { get; }          // Indica se l'applicazione della regola ha avuto successo
        public double Cost { get; }             // Il costo associato all'applicazione della regola
        public DateTime Timestamp { get; }      // Quando è avvenuta la transizione

        public TransitionRecord(string startString, string endString, string appliedRuleId, bool isSuccess, double cost)
        {
            StartString = startString;
            EndString = endString;
            AppliedRuleId = appliedRuleId;
            IsSuccess = isSuccess;
            Cost = cost;
            Timestamp = DateTime.Now;
        }
    }
}
