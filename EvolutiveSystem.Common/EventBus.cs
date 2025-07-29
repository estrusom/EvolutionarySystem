// File: EvolutiveSystem.Common/EventBus.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<Type, List<DelegateSubscription>> _handlers = new Dictionary<Type, List<DelegateSubscription>>();
        private readonly Logger _logger;
        private readonly object _lock = new object(); // Per la sicurezza dei thread

        public EventBus(Logger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Log(LogLevel.DEBUG, "EventBus istanziato.");
        }


        private class DelegateSubscription
        {
            // L'handler originale (es. Func<TEvent, Task> o Action<TEvent>)
            public Delegate OriginalHandler { get; }
            // Il delegato wrapper che viene effettivamente memorizzato e invocato nel bus
            public Func<object, Task> WrappedHandler { get; }

            public DelegateSubscription(Delegate originalHandler, Func<object, Task> wrappedHandler)
            {
                OriginalHandler = originalHandler;
                WrappedHandler = wrappedHandler;
            }
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
                    // INIZIO MODIFICA: La lista ora contiene DelegateSubscription
                    _handlers[eventType] = new List<DelegateSubscription>();
                }
                // INIZIO MODIFICA: Crea il wrapper e aggiungilo come DelegateSubscription
                Func<object, Task> wrapped = async (e) => await handler((TEvent)e);
                _handlers[eventType].Add(new DelegateSubscription(handler, wrapped));
                _logger.Log(LogLevel.DEBUG, $"Sottoscritto handler asincrono per evento di tipo {eventType.Name}."); // Modificato il messaggio di log
                                                                                                                     // FINE MODIFICA
            }
        }
        /// <summary>
        /// Disiscrive un handler asincrono da un tipo specifico di evento.
        /// </summary>
        /// <typeparam name="TEvent">Il tipo di evento dal quale disiscriversi.</typeparam>
        /// <param name="handlerToRemove">Il metodo asincrono da rimuovere dalla sottoscrizione.</param>
        public void Unsubscribe<TEvent>(Func<TEvent, Task> handlerToRemove) where TEvent : class // Uniformato a 'where TEvent : class'
        {
            Type eventType = typeof(TEvent);
            if (_handlers.TryGetValue(eventType, out var subscriptions))
            {
                lock (_lock) // Il lock deve essere sull'oggetto _lock del bus, non sulla lista temporanea
                {
                    // Troviamo l'handler wrapper basandoci sull'handler originale fornito
                    // Usiamo FirstOrDefault per trovare la prima corrispondenza
                    var subscriptionToRemove = subscriptions.FirstOrDefault(s => s.OriginalHandler.Equals(handlerToRemove));
                    if (subscriptionToRemove != null)
                    {
                        subscriptions.Remove(subscriptionToRemove);
                        _logger.Log(LogLevel.DEBUG, $"Disiscritto handler asincrono per evento di tipo {eventType.Name}.");
                    }
                }
            }
        }
        /// <summary>
        /// Disiscrive un handler sincrono da un tipo specifico di evento.
        /// </summary>
        /// <typeparam name="TEvent">Il tipo di evento dal quale disiscriversi.</typeparam>
        /// <param name="handlerToRemove">Il metodo sincrono da rimuovere dalla sottoscrizione.</param>
        public void Unsubscribe<TEvent>(Action<TEvent> handlerToRemove) where TEvent : class // Uniformato a 'where TEvent : class'
        {
            Type eventType = typeof(TEvent);
            if (_handlers.TryGetValue(eventType, out var subscriptions))
            {
                lock (_lock) // Il lock deve essere sull'oggetto _lock del bus
                {
                    // Troviamo l'handler wrapper basandoci sull'handler originale fornito
                    var subscriptionToRemove = subscriptions.FirstOrDefault(s => s.OriginalHandler.Equals(handlerToRemove));
                    if (subscriptionToRemove != null)
                    {
                        subscriptions.Remove(subscriptionToRemove);
                        _logger.Log(LogLevel.DEBUG, $"Disiscritto handler sincrono per evento di tipo {eventType.Name}.");
                    }
                }
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
            // INIZIO MODIFICA: La lista ora contiene DelegateSubscription
            List<DelegateSubscription> handlersToInvoke;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(eventType, out handlersToInvoke))
                {
                    _logger.Log(LogLevel.DEBUG, $"Nessun handler sottoscritto per evento di tipo {eventType.Name}.");
                    return;
                }
                // Creiamo una copia per evitare modifiche alla lista durante l'iterazione
                handlersToInvoke = new List<DelegateSubscription>(handlersToInvoke);
            }

            _logger.Log(LogLevel.DEBUG, $"Pubblicando evento di tipo {eventType.Name}. Numero di handler: {handlersToInvoke.Count}.");

            // INIZIO MODIFICA: Ciclo su DelegateSubscription e invoca il WrappedHandler
            foreach (var subscription in handlersToInvoke)
            {
                try
                {
                    await subscription.WrappedHandler(eventMessage);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.ERROR, $"Errore durante l'esecuzione dell'handler per l'evento {eventType.Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }
            // FINE MODIFICA
        }
        /// <summary>
        /// Sottoscrive un handler sincrono a un tipo specifico di evento.
        /// </summary>
        /// <typeparam name="TEvent">Il tipo di evento da sottoscrivere.</typeparam>
        /// <param name="handler">Il delegato sincrono che gestirà l'evento.</param>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class // Uniformato a 'where TEvent : class'
        {
            lock (_lock)
            {
                Type eventType = typeof(TEvent);
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<DelegateSubscription>();
                }

                // Creiamo il delegato wrapper: l'Action viene avvolto in un Func<object, Task> che restituisce Task.CompletedTask
                Func<object, Task> wrapped = (e) =>
                {
                    handler((TEvent)e); // Esegue l'handler sincrono
                    return Task.CompletedTask; // Restituisce un Task già completato
                };
                _handlers[eventType].Add(new DelegateSubscription(handler, wrapped));
                _logger.Log(LogLevel.DEBUG, $"Sottoscritto handler sincrono per evento di tipo {eventType.Name}.");
            }
        }
    }
}
