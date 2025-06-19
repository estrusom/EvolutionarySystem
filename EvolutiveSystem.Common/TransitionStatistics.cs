// creato il 15.6.2025 1.56
// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\TransitionStatistics.cs
// Questo file definisce la classe TransitionStatistics, un modello di dati
// per memorizzare le statistiche di transizione tra stati MIU.
// Data: 20 giugno 2025
// Descrizione: Classe per la memorizzazione delle statistiche di apprendimento per le transizioni (stato genitore -> regola applicata).
using System;
namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta le statistiche di apprendimento per una specifica transizione.
    /// Una transizione è definita da uno stato iniziale (ParentStringCompressed)
    /// e una regola applicata (AppliedRuleID) per raggiungere un nuovo stato.
    /// </summary>
    public class TransitionStatistics
    {
        /// <summary>
        /// La stringa MIU compressa dello stato genitore da cui è avvenuta la transizione.
        /// </summary>
        public string ParentStringCompressed { get; set; }

        /// <summary>
        /// L'ID della regola applicata per effettuare questa transizione.
        /// </summary>
        public long AppliedRuleID { get; set; }

        /// <summary>
        /// Il numero totale di volte in cui questa transizione specifica
        /// (stato genitore + regola applicata) è stata tentata.
        /// </summary>
        public int ApplicationCount { get; set; }

        /// <summary>
        /// Il numero di volte in cui questa transizione specifica ha portato
        /// a un nuovo stato che è stato parte di un percorso di soluzione di successo.
        /// </summary>
        public int SuccessfulCount { get; set; }

        /// <summary>
        /// Timestamp dell'ultima volta in cui queste statistiche sono state aggiornate.
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Costruttore predefinito.
        /// Inizializza i contatori a zero.
        /// </summary>
        public TransitionStatistics()
        {
            ApplicationCount = 0;
            SuccessfulCount = 0;
            LastUpdated = DateTime.MinValue; // Valore predefinito
        }

        /// <summary>
        /// Calcola il tasso di successo di questa transizione.
        /// </summary>
        public double SuccessRate
        {
            get
            {
                if (ApplicationCount > 0)
                {
                    return (double)SuccessfulCount / ApplicationCount;
                }
                return 0.0; // Evita divisione per zero
            }
        }
    }
}
