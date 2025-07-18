// File: EvolutiveSystem.Common/EventBus.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MasterLog; // Per il Logger

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Un semplice Event Bus in-memory per la pubblicazione e sottoscrizione di eventi.
    /// Permette un disaccoppiamento tra publisher e subscriber.
    /// Può gestire qualsiasi classe come tipo di evento, inclusi gli EventArgs personalizzati.
    /// </summary>
    public class EventBus
    {
        // Dizionario per memorizzare i delegati sottoscritti per ogni tipo di evento
        private readonly Dictionary<Type, List<Func<object, Task>>> _handlers = new Dictionary<Type, List<Func<object, Task>>>();
        private readonly Logger _logger;
        private readonly object _lock = new object(); // Per la sicurezza dei thread

        public EventBus(Logger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Log(LogLevel.DEBUG, "EventBus istanziato.");
        }

        /// <summary>
        /// Sottoscrive un handler a un tipo specifico di evento.
        /// </summary>
        /// <typeparam name="TEvent">Il tipo di evento da sottoscrivere (es. RuleAppliedEventArgs).</typeparam>
        /// <param name="handler">Il delegato asincrono che gestirà l'evento.</param>
        public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class
        {
            lock (_lock)
            {
                Type eventType = typeof(TEvent);
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<Func<object, Task>>();
                }
                _handlers[eventType].Add(async (e) => await handler((TEvent)e));
                _logger.Log(LogLevel.DEBUG, $"Sottoscritto handler per evento di tipo {eventType.Name}.");
            }
        }

        /// <summary>
        /// Pubblica un evento sull'Event Bus. Tutti gli handler sottoscritti per quel tipo di evento verranno invocati.
        /// </summary>
        /// <typeparam name="TEvent">Il tipo di evento da pubblicare (es. RuleAppliedEventArgs).</typeparam>
        /// <param name="eventMessage">L'istanza dell'evento da pubblicare.</param>
        public async Task Publish<TEvent>(TEvent eventMessage) where TEvent : class
        {
            Type eventType = typeof(TEvent);
            List<Func<object, Task>> handlersToInvoke;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(eventType, out handlersToInvoke))
                {
                    _logger.Log(LogLevel.DEBUG, $"Nessun handler sottoscritto per evento di tipo {eventType.Name}.");
                    return;
                }
                // Creiamo una copia per evitare modifiche alla lista durante l'iterazione
                handlersToInvoke = new List<Func<object, Task>>(handlersToInvoke);
            }

            _logger.Log(LogLevel.DEBUG, $"Pubblicando evento di tipo {eventType.Name}. Numero di handler: {handlersToInvoke.Count}.");

            foreach (var handler in handlersToInvoke)
            {
                try
                {
                    await handler(eventMessage);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.ERROR, $"Errore durante l'esecuzione dell'handler per l'evento {eventType.Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    // Non rilanciamo l'eccezione qui per non bloccare altri handler
                }
            }
        }
    }
}
