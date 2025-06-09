using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{
    /// <summary>
    /// Modello per la persistenza delle statistiche aggregate di una singola regola MIU.
    /// Utilizzato da EmergingProcesses e dal livello di persistenza.
    /// </summary>
    public class RuleStatistics
    {
        public int RuleID { get; set; }
        public int ApplicationCount { get; set; }
        public double EffectivenessScore { get; set; }
        public string LastUpdated { get; set; } // Aggiunto per tracciare l'ultimo aggiornamento nel DB
    }

    /// <summary>
    /// Modello per la persistenza delle statistiche aggregate delle transizioni.
    /// Utilizzato da EmergingProcesses e dal livello di persistenza.
    /// </summary>
    public class TransitionStatistics
    {
        public string ParentStringCompressed { get; set; }
        public int AppliedRuleID { get; set; }
        public int ApplicationCount { get; set; } // Quante volte questa transizione è stata applicata
        public int SuccessfulCount { get; set; } // Quante volte questa transizione ha fatto parte di un percorso di successo
        public string LastUpdated { get; set; } // Aggiunto per tracciare l'ultimo aggiornamento nel DB
    }
}
