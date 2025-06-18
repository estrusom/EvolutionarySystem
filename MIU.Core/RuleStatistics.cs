// creato il 15.6.2025 1.56
// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\RuleStatistics.cs
// Questo file definisce la classe RuleStatistics, un modello di dati
// per memorizzare le statistiche di applicazione e efficacia delle regole MIU.
// Data: 20 giugno 2025
// Descrizione: Classe per la memorizzazione delle statistiche di apprendimento per ciascuna regola MIU.




using System;

namespace MIU.Core
{
    /// <summary>
    /// Rappresenta le statistiche di apprendimento per una singola regola MIU.
    /// Utilizzata per calcolare l'efficacia delle regole.
    /// </summary>
    public class RuleStatistics
    {
        /// <summary>
        /// L'ID univoco della regola a cui si riferiscono queste statistiche.
        /// Corrisponde all'ID della classe RegolaMIU.
        /// Nota: Utilizziamo 'long' per allineare al tipo di ID di RegolaMIU.
        /// </summary>
        public long RuleID { get; set; }

        /// <summary>
        /// Il numero totale di volte in cui questa regola è stata applicata.
        /// </summary>
        public int ApplicationCount { get; set; }

        /// <summary>
        /// Il numero di volte in cui questa regola è stata applicata come parte
        /// di un percorso di derivazione che ha portato a una soluzione di successo.
        /// </summary>
        public int SuccessfulCount { get; set; }

        /// <summary>
        /// Il punteggio di efficacia della regola, calcolato come SuccessfulCount / ApplicationCount.
        /// Un valore più alto indica una regola più efficace nelle derivazioni di successo.
        /// </summary>
        public double EffectivenessScore { get; set; }

        /// <summary>
        /// Timestamp dell'ultima volta in cui queste statistiche sono state aggiornate o la regola è stata applicata.
        /// </summary>
        public DateTime LastApplicationTimestamp { get; set; }

        /// <summary>
        /// Costruttore predefinito.
        /// Inizializza i contatori a zero e il punteggio a 0.0.
        /// </summary>
        public RuleStatistics()
        {
            ApplicationCount = 0;
            SuccessfulCount = 0;
            EffectivenessScore = 0.0;
            LastApplicationTimestamp = DateTime.MinValue; // Valore predefinito
        }

        /// <summary>
        /// Ricalcola l'EffectivenessScore basandosi sugli ApplicationCount e SuccessfulCount attuali.
        /// Da chiamare ogni volta che SuccessfulCount o ApplicationCount vengono modificati.
        /// </summary>
        public void RecalculateEffectiveness()
        {
            if (ApplicationCount > 0)
            {
                EffectivenessScore = (double)SuccessfulCount / ApplicationCount;
            }
            else
            {
                EffectivenessScore = 0.0; // Evita divisione per zero
            }
        }
    }
}
