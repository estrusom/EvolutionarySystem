// File: EvolutiveSystem.Common/Events/SearchCompletedEvent.cs
using System;

namespace EvolutiveSystem.Common.Events
{
    /// <summary>
    /// Evento scatenato quando una ricerca MIU completa il suo ciclo.
    /// Contiene l'esito finale della ricerca (successo o vari tipi di fallimento).
    /// </summary>
    public class SearchCompletedEvent
    {
        public long SearchId { get; }
        public string InitialString { get; }
        public string TargetString { get; }
        public string Outcome { get; } // Es: "Success", "Failed: Depth Limit", "Failed: Timeout", "Failed: No Rules"
        public int StepsTaken { get; }
        public int NodesExplored { get; }
        public double ElapsedMilliseconds { get; }
        public DateTime Timestamp { get; }

        /// <summary>
        /// Costruttore per SearchCompletedEvent.
        /// </summary>
        /// <param name="searchId">L'ID univoco della ricerca.</param>
        /// <param name="initialString">La stringa iniziale della ricerca.</param>
        /// <param name="targetString">La stringa target della ricerca.</param>
        /// <param name="outcome">L'esito della ricerca (Successo o tipo di fallimento).</param>
        /// <param name="stepsTaken">Il numero di passi (applicazioni di regole) eseguiti.</param>
        /// <param name="nodesExplored">Il numero di nodi (stati) esplorati.</param>
        /// <param name="elapsedMilliseconds">Il tempo impiegato dalla ricerca in millisecondi.</param>
        public SearchCompletedEvent(long searchId, string initialString, string targetString, string outcome, int stepsTaken, int nodesExplored, double elapsedMilliseconds)
        {
            SearchId = searchId;
            InitialString = initialString;
            TargetString = targetString;
            Outcome = outcome;
            StepsTaken = stepsTaken;
            NodesExplored = nodesExplored;
            ElapsedMilliseconds = elapsedMilliseconds;
            Timestamp = DateTime.UtcNow; // Registra il momento dell'evento
        }
    }
}
