// File: EvolutiveSystem.Taxonomy.Antithesis/TaxonomyAntithesisPublisher.cs
// Data di riferimento: 24 luglio 2025 (2025.07.24) (Corretto per Namespace EventBus)
// Descrizione: Componente responsabile della pubblicazione degli eventi di antitesi.

using System.Collections.Generic;
using System.Threading.Tasks;
using MasterLog;
using EvolutiveSystem.Common; // CORREZIONE: EventBus si trova qui
using EvolutiveSystem.Taxonomy; // CORREZIONE: MiuAbstractPattern si trova qui

namespace EvolutiveSystem.Taxonomy.Antithesis
{
    /// <summary>
    /// Componente responsabile della pubblicazione degli eventi relativi
    /// all'identificazione di antitesi (gap e inefficienze) nel sistema MIU.
    /// </summary>
    public class TaxonomyAntithesisPublisher
    {
        private readonly EventBus _eventBus; // CORREZIONE: Tipo EventBus (non IEventBus se non definita)
        private readonly Logger _logger;

        /// <summary>
        /// Costruttore di TaxonomyAntithesisPublisher.
        /// </summary>
        /// <param name="eventBus">L'istanza dell'Event Bus per pubblicare gli eventi.</param>
        /// <param name="logger">L'istanza del logger.</param>
        public TaxonomyAntithesisPublisher(EventBus eventBus, Logger logger) // CORREZIONE: Tipo EventBus
        {
            _eventBus = eventBus ?? throw new System.ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _logger.Log(LogLevel.DEBUG, "TaxonomyAntithesisPublisher istanziato.");
        }

        /// <summary>
        /// Pubblica un evento che segnala l'identificazione di gap e inefficienze.
        /// </summary>
        /// <param name="gaps">La lista dei pattern identificati come gap.</param>
        /// <param name="inefficiencies">La lista dei pattern identificati come inefficienze.</param>
        public async Task PublishAntitheses(List<MiuAbstractPattern> gaps, List<MiuAbstractPattern> inefficiencies)
        {
            var antithesisEvent = new AntithesisIdentifiedEvent(gaps, inefficiencies);
            await _eventBus.Publish(antithesisEvent); // MODIFICA: Aggiunto 'await'
            _logger.Log(LogLevel.INFO, $"[TaxonomyAntithesisPublisher] Pubblicato AntithesisIdentifiedEvent: {gaps.Count} gap, {inefficiencies.Count} inefficienze.");
        }
    }
}
