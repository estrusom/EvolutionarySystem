// EvolutiveSystem.Common/MIUExplorerCursor.cs
// Data di riferimento: 26 giugno 2025
// Descrizione: Aggiornamenti minimi alla classe MIUExplorerCursor per aggiungere
//              le proprietà State e Predecessor, necessarie per il funzionamento
//              dell'esploratore MIU e la ricostruzione del percorso.

using System;
// Non servono altri using per questa versione minima.

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta il cursore per l'esplorazione del sistema MIU.
    /// Questa versione aggiunge la capacità di fungere da "nodo" in un percorso di derivazione,
    /// tracciando lo stato MIU corrente e il cursore che lo ha preceduto.
    /// </summary>
    public class MIUExplorerCursor
    {
        // --- Proprietà esistenti per la Persistenza del Progresso Generale ---
        /// <summary>
        /// L'indice della stringa di origine corrente.
        /// </summary>
        public long CurrentSourceIndex { get; set; }

        /// <summary>
        /// L'indice della stringa di destinazione corrente.
        /// </summary>
        public long CurrentTargetIndex { get; set; }

        /// <summary>
        /// Il timestamp dell'ultima esplorazione.
        /// </summary>
        public DateTime LastExplorationTimestamp { get; set; } = DateTime.UtcNow;

        // --- NUOVE PROPRIETÀ: State e Predecessor ---
        /// <summary>
        /// Ottiene lo stato MIU che questo cursore rappresenta in un dato punto del percorso.
        /// </summary>
        public MIUState State { get; }

        /// <summary>
        /// Ottiene il cursore precedente a questo nel percorso di derivazione.
        /// È null se questo è il punto di partenza del percorso.
        /// </summary>
        public MIUExplorerCursor Predecessor { get; }

        /// <summary>
        /// Costruttore di default per la serializzazione o per l'uso generico.
        /// Le nuove proprietà State e Predecessor non vengono impostate qui.
        /// </summary>
        public MIUExplorerCursor()
        {
        }

        /// <summary>
        /// Costruttore per la persistenza del progresso (usa indici e timestamp).
        /// </summary>
        public MIUExplorerCursor(long sourceIndex, long targetIndex, DateTime lastTimestamp)
            : this() // Chiama il costruttore di default
        {
            CurrentSourceIndex = sourceIndex;
            CurrentTargetIndex = targetIndex;
            LastExplorationTimestamp = lastTimestamp;
        }

        /// <summary>
        /// NUOVO Costruttore: per creare un cursore come nodo di un percorso.
        /// Accetta lo stato MIU corrente e il cursore che lo ha preceduto.
        /// </summary>
        /// <param name="state">Lo stato MIU che questo cursore rappresenta.</param>
        /// <param name="predecessor">Il cursore precedente nel percorso. Può essere null se è il primo stato.</param>
        /// <exception cref="ArgumentNullException">Lanciata se lo stato fornito è null.</exception>
        public MIUExplorerCursor(MIUState state, MIUExplorerCursor predecessor) : this()
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state), "Lo stato non può essere null per un nodo di esplorazione.");
            }
            State = state;
            Predecessor = predecessor;
        }
    }
}
