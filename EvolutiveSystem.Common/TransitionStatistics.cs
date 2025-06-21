// File: EvolutiveSystem.Common/TransitionStatistics.cs
// Data di riferimento: 21 giugno 2025
// Descrizione: Classe modello per le statistiche di apprendimento delle transizioni tra stati MIU.

using System;

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta le statistiche di apprendimento per una specifica transizione da uno stato
    /// (rappresentato dalla sua stringa compressa) tramite l'applicazione di una regola.
    /// Utilizzato per calcolare la probabilità di successo di una transizione specifica.
    /// </summary>
    public class TransitionStatistics
    {
        /// <summary>
        /// La stringa MIU dello stato genitore (in formato compresso) da cui è partita la transizione.
        /// </summary>
        public string ParentStringCompressed { get; set; }

        /// <summary>
        /// L'ID della regola applicata per questa transizione.
        /// </summary>
        public long AppliedRuleID { get; set; }

        /// <summary>
        /// Il numero totale di volte che questa transizione specifica è stata tentata.
        /// </summary>
        public int ApplicationCount { get; set; }

        /// <summary>
        /// Il numero di volte che questa transizione specifica ha portato a una soluzione.
        /// </summary>
        public int SuccessfulCount { get; set; }

        /// <summary>
        /// La probabilità di successo per questa transizione, calcolata come SuccessfulCount / ApplicationCount.
        /// Un valore più alto indica una transizione più promettente.
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

        /// <summary>
        /// Il timestamp dell'ultima volta che questa statistica di transizione è stata aggiornata.
        /// </summary>
        public DateTime LastUpdated { get; set; }

        public TransitionStatistics()
        {
            ApplicationCount = 0;
            SuccessfulCount = 0;
            LastUpdated = DateTime.MinValue; // O DateTime.UtcNow
        }
    }
}
