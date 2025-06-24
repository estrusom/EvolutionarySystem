// --- File 1: EvolutiveSystem.Common/MIUExplorerCursor.cs ---
// Data di riferimento: 21 giugno 2025
// Descrizione: Classe modello per rappresentare lo stato del cursore di esplorazione
//              delle stringhe MIU.

using System;

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta lo stato del cursore per l'esplorazione sistematica
    /// delle coppie di stringhe MIU (origine, destinazione) dalla tabella MIU_States.
    /// Questo stato viene salvato e caricato dalla tabella MIUParameterConfigurator.
    /// </summary>
    public class MIUExplorerCursor
    {
        /// <summary>
        /// L'indice della stringa di origine corrente nell'elenco di MIU_States.
        /// </summary>
        public long CurrentSourceIndex { get; set; }

        /// <summary>
        /// L'indice della stringa di destinazione corrente nell'elenco di MIU_States,
        /// relativa all'CurrentSourceIndex.
        /// </summary>
        public long CurrentTargetIndex { get; set; }

        /// <summary>
        /// Il timestamp dell'ultima volta che il cursore è stato aggiornato/utilizzato.
        /// </summary>
        public DateTime LastExplorationTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Costruttore di default.
        /// </summary>
        public MIUExplorerCursor() { }

        /// <summary>
        /// Costruttore con parametri.
        /// </summary>
        public MIUExplorerCursor(int sourceIndex, int targetIndex, DateTime lastTimestamp)
        {
            CurrentSourceIndex = sourceIndex;
            CurrentTargetIndex = targetIndex;
            LastExplorationTimestamp = lastTimestamp;
        }
    }
}
