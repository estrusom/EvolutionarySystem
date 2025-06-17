// creato il 15.6.2025 1.56
// File: MIU.Core\TransitionStatistics.cs
// Questo file definisce la classe TransitionStatistics, un modello di dati
// per memorizzare le statistiche di transizione tra stati MIU.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{
    /// <summary>
    /// Rappresenta le statistiche di apprendimento per una specifica transizione tra stati MIU,
    /// tramite l'applicazione di una determinata regola.
    /// Corrisponde alla tabella 'Learning_TransitionStatistics' nel database.
    /// </summary>
    public class TransitionStatistics
    {
        // Corrisponde a 'ParentStringCompressed' nella tabella Learning_TransitionStatistics
        public string ParentStringCompressed { get; set; }

        // Corrisponde a 'AppliedRuleID' nella tabella Learning_TransitionStatistics
        public int AppliedRuleID { get; set; }

        // Corrisponde a 'ApplicationCount' nella tabella Learning_TransitionStatistics
        public int ApplicationCount { get; set; }

        // Corrisponde a 'SuccessfulCount' nella tabella Learning_TransitionStatistics
        public int SuccessfulCount { get; set; }

        // Corrisponde a 'LastUpdated' nella tabella Learning_TransitionStatistics
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Costruttore predefinito.
        /// </summary>
        public TransitionStatistics()
        {
            LastUpdated = DateTime.UtcNow; // Inizializza con l'ora corrente UTC
        }

        /// <summary>
        /// Costruttore per inizializzare TransitionStatistics con valori specifici.
        /// </summary>
        public TransitionStatistics(string parentStringCompressed, int appliedRuleId, int applicationCount, int successfulCount, DateTime lastUpdated)
        {
            ParentStringCompressed = parentStringCompressed;
            AppliedRuleID = appliedRuleId;
            ApplicationCount = applicationCount;
            SuccessfulCount = successfulCount;
            LastUpdated = lastUpdated;
        }

        /// <summary>
        /// Aggiorna le statistiche per questa transizione.
        /// </summary>
        public void IncrementCounts(bool wasSuccessful)
        {
            ApplicationCount++;
            if (wasSuccessful)
            {
                SuccessfulCount++;
            }
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Calcola il tasso di successo per questa transizione.
        /// </summary>
        public double SuccessRate
        {
            get
            {
                if (ApplicationCount == 0)
                {
                    return 0.0;
                }
                return (double)SuccessfulCount / ApplicationCount;
            }
        }
    }
}
