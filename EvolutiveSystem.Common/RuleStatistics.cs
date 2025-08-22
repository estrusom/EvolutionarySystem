// File: EvolutiveSystem.Common/RuleStatistics.cs
// Data di riferimento: 21 giugno 2025
// Descrizione: Classe modello per le statistiche di apprendimento delle regole MIU.

using System;

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta le statistiche di apprendimento per una specifica regola MIU.
    /// Utilizzato per calcolare l'efficacia delle regole.
    /// </summary>
    public class RuleStatistics
    {
        /// <summary>
        /// L'ID univoco della regola MIU.
        /// </summary>
        public long RuleID { get; set; }

        /// <summary>
        /// Il numero totale di volte che questa regola è stata applicata (tentata).
        /// </summary>
        public int ApplicationCount { get; set; }

        /// <summary>
        /// Il numero di volte che l'applicazione di questa regola ha portato a una soluzione.
        /// </summary>
        public int SuccessfulCount { get; set; }

        /// <summary>
        /// Il punteggio di efficacia della regola, calcolato come SuccessRatio.
        /// Un valore più alto indica una regola più efficace.
        /// </summary>
        public double EffectivenessScore { get; set; } // Calcolato automaticamente 2025.06.23 set da privato a pubblico

        /// <summary>
        /// Il timestamp dell'ultima applicazione o aggiornamento di questa statistica.
        /// </summary>
        public DateTime LastApplicationTimestamp { get; set; }

        /// <summary>
        /// La somma totale delle profondità a cui questa regola è stata applicata.
        /// </summary>
        public double TotalDepthSum { get; set; }

        /// <summary>
        /// Il calcolo della stima di profondità media.
        /// </summary>
        public double AverageDepth => ApplicationCount > 0 ? TotalDepthSum / ApplicationCount : 0;

        public RuleStatistics()
        {
            // Inizializza i conteggi a zero
            ApplicationCount = 0;
            SuccessfulCount = 0;
            EffectivenessScore = 0.0;
            LastApplicationTimestamp = DateTime.MinValue; // O DateTime.UtcNow
        }

        /// <summary>
        /// Ricalcola l'EffectivenessScore basandosi sui conteggi correnti.
        /// Questo metodo dovrebbe essere chiamato ogni volta che ApplicationCount o SuccessfulCount cambiano.
        /// </summary>
        public void RecalculateEffectiveness()
        {
            if (ApplicationCount > 0)
            {
                EffectivenessScore = (double)SuccessfulCount / ApplicationCount;
            }
            else
            {
                EffectivenessScore = 0.0; // Evita divisione per zero se la regola non è mai stata applicata
            }
        }
    }
}
