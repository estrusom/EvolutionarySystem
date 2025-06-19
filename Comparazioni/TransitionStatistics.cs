// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\TransitionStatistics.cs
// Data di riferimento: 20 giugno 2025
// Contiene la definizione della classe TransitionStatistics per tracciare le transizioni tra stati MIU.

using System;

namespace MIU.Core
{
    /// <summary>
    /// Classe per memorizzare le statistiche di apprendimento per una specifica transizione tra stati MIU.
    /// Questo può essere usato per ottimizzare ulteriormente la ricerca path-specific.
    /// </summary>
    public class TransitionStatistics
    {
        public long OriginalStateID { get; set; } // ID dello stato di partenza
        public long NewStateID { get; set; }      // ID dello stato di arrivo
        public long RuleID { get; set; }          // ID della regola applicata per questa transizione (precedentemente AppliedRuleID)
        public int Count { get; set; }            // Numero di volte che questa transizione specifica è avvenuta
        public int SuccessfulCount { get; set; }  // Numero di volte che questa transizione ha portato a un successo
        public double Probability { get; set; }   // Probabilità di successo di questa transizione (può essere ricalcolata)

        public TransitionStatistics()
        {
            Count = 0;
            SuccessfulCount = 0;
            Probability = 0.0;
        }

        /// <summary>
        /// Ricalcola la probabilità di successo per questa transizione.
        /// </summary>
        public void RecalculateProbability()
        {
            // Anche qui si potrebbe applicare Laplace Smoothing
            Probability = (Count == 0) ? 0.0 : (double)SuccessfulCount / Count;
        }
    }
}
