// File: EvolutiveSystem.Common/Events/RulesEvolvedEvent.cs
// Data di riferimento: 25 luglio 2025
// Descrizione: Definisce l'evento pubblicato quando il SynthesisEngine
//              ha generato e integrato con successo nuove regole nel sistema.

using System;
using System.Collections.Generic;
using EvolutiveSystem.Common; // Per RegolaMIU

namespace EvolutiveSystem.Common.Events
{
    /// <summary>
    /// Evento pubblicato quando il sistema ha evoluto le sue regole,
    /// ad esempio generando nuove regole o modificando quelle esistenti.
    /// </summary>
    public class RulesEvolvedEvent : EventArgs
    {
        /// <summary>
        /// Ottiene la lista delle nuove regole MIU che sono state generate e integrate.
        /// </summary>
        public IReadOnlyList<RegolaMIU> NewRules { get; }

        /// <summary>
        /// Inizializza una nuova istanza della classe RulesEvolvedEvent.
        /// </summary>
        /// <param name="newRules">La lista delle regole MIU appena generate e integrate.</param>
        public RulesEvolvedEvent(IReadOnlyList<RegolaMIU> newRules)
        {
            NewRules = newRules ?? new List<RegolaMIU>();
        }
    }
}
