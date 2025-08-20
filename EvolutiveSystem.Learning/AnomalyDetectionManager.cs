// File: EvolutiveSystem.Learning/AnomalyDetectionManager.cs
// Data di riferimento: 11 luglio 2025
// Descrizione: Gestisce la rilevazione e la persistenza delle anomalie di esplorazione nel sistema MIU.
//              Pubblica AnomalyDetectedEvent sull'EventBus quando un'anomalia viene identificata.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterLog; // Per il Logger
using EvolutiveSystem.Common; // Per IMIUDataManager, ExplorationAnomaly, AnomalyType, EventBus
using EvolutiveSystem.Common.Events; // Per AnomalyDetectedEvent

namespace EvolutiveSystem.Learning // Namespace corretto per questo progetto
{
    /// <summary>
    /// Gestisce la rilevazione e la persistenza delle anomalie di esplorazione nel sistema MIU.
    /// Pubblica eventi di anomalia sull'EventBus.
    /// </summary>
    public class AnomalyDetectionManager
    {
        private readonly IMIUDataManager _dataManager;
        private readonly Logger _logger;
        private readonly EventBus _eventBus;

        /// <summary>
        /// Costruttore dell'AnomalyDetectionManager.
        /// </summary>
        /// <param name="dataManager">L'istanza del gestore dati per la persistenza delle anomalie.</param>
        /// <param name="logger">L'istanza del logger.</param>
        /// <param name="eventBus">L'istanza dell'EventBus per pubblicare gli eventi di anomalia.</param>
        public AnomalyDetectionManager(IMIUDataManager dataManager, Logger logger, EventBus eventBus)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger.Log(LogLevel.DEBUG, "AnomalyDetectionManager istanziato con EventBus.");
        }
        /// <summary>
        /// Simula la costruzione di una topologia di sistema.
        /// Questo metodo è stato aggiunto per supportare il menu di Program.cs.
        /// </summary>
        public async Task BuildTopology()
        {
            _logger.Log(LogLevel.INFO, "[AnomalyDetectionManager] Inizio della costruzione della topologia del sistema...");
            // TODO: Aggiungere qui la logica effettiva per costruire la topologia.
            // Ad esempio, si potrebbe leggere la configurazione dal database,
            // mappare i nodi e le relazioni, ecc.
            await Task.Delay(100); // Simulazione di un'operazione asincrona
            _logger.Log(LogLevel.INFO, "[AnomalyDetectionManager] Costruzione della topologia completata.");
        }
        /// <summary>
        /// Rileva e gestisce un'anomalia di esplorazione.
        /// Questo metodo dovrebbe essere chiamato quando un comportamento anomalo viene identificato.
        /// </summary>
        /// <param name="type">Il tipo di anomalia.</param>
        /// <param name="ruleId">L'ID della regola MIU associata (nullable).</param>
        /// <param name="contextPatternHash">L'hash del pattern di contesto (nullable).</param>
        /// <param name="contextPatternSample">Un esempio del pattern di contesto (nullable).</param>
        /// <param name="value">Un valore numerico associato all'anomalia (es. profondità, conteggio).</param>
        /// <param name="description">Una descrizione testuale dell'anomalia.</param>
        public void DetectAndHandleAnomaly(
            AnomalyType type,
            long? ruleId,
            int? contextPatternHash,
            string contextPatternSample,
            double value,
            string description)
        {
            _logger.Log(LogLevel.WARNING, $"[AnomalyDetectionManager] Anomalia rilevata: Tipo={type}, Regola={ruleId}, Descrizione='{description}'");

            // Qui potresti voler recuperare un'anomalia esistente dal DB
            // per aggiornarla invece di crearne sempre una nuova,
            // specialmente per anomalie ricorrenti (es. cicli infiniti).
            // Per semplicità, per ora, creeremo o aggiorneremo direttamente.

            var anomaly = new ExplorationAnomaly
            {
                Type = type,
                RuleId = ruleId,
                ContextPatternHash = contextPatternHash,
                ContextPatternSample = contextPatternSample,
                Count = 1, // Questo sarà incrementato dalla logica di Upsert se esiste già
                AverageValue = value,
                AverageDepth = value, // Per ora, usiamo lo stesso valore per semplicità
                LastDetected = DateTime.UtcNow,
                Description = description,
                IsNewCategory = true // Potrebbe essere false se l'anomalia è già nota
            };

            // Persisti l'anomalia nel database (Upsert)
            _dataManager.UpsertExplorationAnomaly(anomaly);
            _logger.Log(LogLevel.INFO, $"[AnomalyDetectionManager] Anomalia persistita nel DB. ID: {anomaly.Id}");

            // Pubblicazione dell'evento sull'EventBus
            // Creiamo l'istanza di AnomalyDetectedEvent con i dati rilevanti dalla ExplorationAnomaly appena creata/aggiornata.
            var eventMessage = new AnomalyDetectedEvent(
                anomaly.Id, // Usiamo l'ID generato/aggiornato dal database
                anomaly.Type,
                anomaly.RuleId,
                anomaly.ContextPatternHash,
                anomaly.Description
            );

            // Pubblichiamo l'evento sull'EventBus in modo asincrono ("fire and forget")
            _ = Task.Run(async () =>
            {
                try
                {
                    await _eventBus.Publish(eventMessage);
                    _logger.Log(LogLevel.DEBUG, $"[AnomalyDetectionManager] Pubblicato AnomalyDetectedEvent: Tipo={eventMessage.Type}, ID Anomalia={eventMessage.AnomalyId}");
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.ERROR, $"[AnomalyDetectionManager] Errore durante la pubblicazione di AnomalyDetectedEvent: {ex.Message}");
                }
            });
        }

        // ... Altri metodi dell'AnomalyDetectionManager (es. per caricare anomalie, analizzare dati) ...
    }
}
