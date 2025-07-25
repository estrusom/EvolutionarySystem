// File: EvolutiveSystem.Taxonomy.Antithesis/AntithesisEvents.cs
// Data di riferimento: 24 luglio 2025 (2025.07.24) (Corretto per Namespace e assenza di classe base Event)
// Descrizione: Definizioni degli eventi per la comunicazione delle antitesi identificate.

using System.Collections.Generic;
using EvolutiveSystem.Taxonomy; // CORREZIONE: MiuAbstractPattern si trova qui
using EvolutiveSystem.Common;
using System; // CORREZIONE: EventBus (e non Event) si trova qui

namespace EvolutiveSystem.Taxonomy.Antithesis
{
    /// <summary>
    /// Evento pubblicato quando il TaxonomyOrchestrator identifica gap e/o inefficienze
    /// nel paesaggio MIU. Questo evento segnala la presenza di "antitesi"
    /// che richiedono un'azione correttiva o un'ulteriore esplorazione.
    /// </summary>
    public class AntithesisIdentifiedEvent : EventArgs // CORREZIONE: Rimosso ": Event" in quanto non definita
    {
        /// <summary>
        /// Ottiene la lista dei pattern identificati come "gap" (aree inesplorate o poco esplorate).
        /// </summary>
        public List<MiuAbstractPattern> IdentifiedGaps { get; }

        /// <summary>
        /// Ottiene la lista dei pattern identificati come "inefficienze" (aree problematiche con basso successo o alta complessità).
        /// </summary>
        public List<MiuAbstractPattern> IdentifiedInefficiencies { get; }

        /// <summary>
        /// Costruttore per creare un'istanza di AntithesisIdentifiedEvent.
        /// </summary>
        /// <param name="Gaps">La lista dei pattern identificati come gap.</param>
        /// <param name="inefficiencies">La lista dei pattern identificati come inefficienze.</param>
        public AntithesisIdentifiedEvent(List<MiuAbstractPattern> Gaps, List<MiuAbstractPattern> inefficiencies)
        {
            IdentifiedGaps = Gaps ?? new List<MiuAbstractPattern>();
            IdentifiedInefficiencies = inefficiencies ?? new List<MiuAbstractPattern>();
        }
    }

    // In futuro, qui potremmo aggiungere altri tipi di eventi di antitesi se necessario,
    // ad esempio eventi per specifici tipi di gap o inefficienze che richiedono una gestione più granulare.
}
