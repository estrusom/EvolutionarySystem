// File: EvolutiveSystem.Engine/IMIUDataProcessingService.cs
// Data di riferimento: 26 giugno 2025
// Descrizione: Interfaccia che definisce il contratto per il servizio di elaborazione dati MIU,
//              responsabile dell'esplorazione dello spazio degli stati,
//              del popolamento del database e dell'aggiornamento delle statistiche di apprendimento,
//              operando potenzialmente su un thread separato.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvolutiveSystem.Common; // Per MIUExplorerCursor, RuleStatistics, TransitionStatistics, MiuStateInfo, RegolaMIU

namespace EvolutiveSystem.Engine // ATTENZIONE: Questo sarà il namespace del nuovo progetto EvolutiveSystem.Engine
{
    /// <summary>
    /// Definisce le operazioni per l'elaborazione dei dati del sistema MIU,
    /// inclusa la derivazione di nuove stringhe, il popolamento del database
    /// e l'aggiornamento delle statistiche di apprendimento.
    /// </summary>
    public interface IMIUDataProcessingService
    {
        /// <summary>
        /// Avvia l'esplorazione dello spazio degli stati MIU come processo in background (thread).
        /// Il motore continua a generare stati e ad applicare regole, persistendo i dati
        /// e aggiornando le statistiche nel database.
        /// </summary>
        /// <param name="initialString">La stringa MIU iniziale da cui iniziare l'esplorazione.</param>
        /// <param name="targetString">La stringa MIU target da raggiungere (opzionale).</param>
        Task StartExplorationAsync(string initialString, string targetString = null);

        /// <summary>
        /// Richiede l'interruzione dell'esplorazione corrente.
        /// </summary>
        void StopExploration();

        /// <summary>
        /// Indica se il motore di esplorazione è attualmente in esecuzione.
        /// </summary>
        bool IsExplorationRunning { get; }

        /// <summary>
        /// Recupera lo stato attuale del cursore di esplorazione, indicando l'ultimo progresso.
        /// </summary>
        /// <returns>L'oggetto MIUExplorerCursor che rappresenta l'ultimo stato noto.</returns>
        Task<MIUExplorerCursor> GetCurrentExplorerCursorAsync();

        // Eventi per notificare lo stato dell'esplorazione (opzionale, utile per UI)
        event EventHandler<string> OnExplorationStatusChanged;
        event EventHandler<int> OnNodesExploredCountChanged;
    }
}
    