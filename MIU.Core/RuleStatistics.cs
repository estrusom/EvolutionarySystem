// creato il 15.6.2025 1.56
// File: MIU.Core\RuleStatistics.cs
// Questo file definisce la classe RuleStatistics, un modello di dati
// per memorizzare le statistiche di applicazione e efficacia delle regole MIU.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{
    /// <summary>
    /// Rappresenta le statistiche di apprendimento per una singola regola MIU.
    /// Corrisponde alla tabella 'Learning_RuleStatistics' nel database.
    /// </summary>
    public class RuleStatistics
    {
        // Corrisponde a 'RuleID' nella tabella Learning_RuleStatistics
        public int RuleID { get; set; }

        // Corrisponde a 'ApplicationCount' nella tabella Learning_RuleStatistics
        public int ApplicationCount { get; set; }

        // Corrisponde a 'EffectivenessScore' nella tabella Learning_RuleStatistics
        public double EffectivenessScore { get; set; }

        // Corrisponde a 'LastUpdated' nella tabella Learning_RuleStatistics
        public DateTime LastApplicationTimestamp { get; set; }

        /// <summary>
        /// Costruttore predefinito.
        /// </summary>
        public RuleStatistics()
        {
            LastApplicationTimestamp = DateTime.UtcNow; // Inizializza con l'ora corrente UTC
        }

        /// <summary>
        /// Costruttore per inizializzare RuleStatistics con valori specifici.
        /// </summary>
        public RuleStatistics(int ruleId, int applicationCount, double effectivenessScore, DateTime lastApplicationTimestamp)
        {
            RuleID = ruleId;
            ApplicationCount = applicationCount;
            EffectivenessScore = effectivenessScore;
            LastApplicationTimestamp = lastApplicationTimestamp;
        }

        /// <summary>
        /// Aggiorna le statistiche con una nuova applicazione della regola.
        /// </summary>
        public void IncrementApplicationCount(bool wasSuccessful)
        {
            ApplicationCount++;
            // Logica per l'aggiornamento dell'EffectivenessScore può essere aggiunta qui in futuro.
            // Per ora, aggiorniamo solo il conteggio.
            LastApplicationTimestamp = DateTime.UtcNow;
        }

        // Potresti aggiungere altri metodi o proprietà, come AverageEffectiveness, ecc.
    }
}
