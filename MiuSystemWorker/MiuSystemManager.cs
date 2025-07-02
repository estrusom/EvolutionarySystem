// File: MiuSystemWorker/MiuSystemManager.cs
// Nuovo progetto: MiuSystemWorker.csproj
// Questo progetto avrà i riferimenti a MIU.Core.csproj, EvolutiveSystem.Learning.csproj,
// EvolutiveSystem.SQL.Core.csproj, EvolutiveSystem.Common.csproj, MasterLog.csproj.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MasterLog; // Per il logging
using MIU.Core; // Per RegoleMIUManager, MIUStringConverter, PathStepInfo, SolutionFoundEventArgs, RuleAppliedEventArgs
using EvolutiveSystem.Common; // Per RegolaMIU, RuleStatistics, TransitionStatistics
using EvolutiveSystem.Learning; // Per LearningStatisticsManager
using EvolutiveSystem.SQL.Core; // Per SQLiteSchemaLoader, MIUDatabaseManager, MIURepository
// System.Data.SQLite non è più usato direttamente qui per comandi SQL, ma le classi restano per i tipi.

namespace MiuSystemWorker // Nuovo namespace per il progetto wrapper
{
    /// <summary>
    /// Gestisce l'intera logica del sistema MIU, inclusa l'interazione con il database
    /// e l'esecuzione delle ricerche. È incapsulato in un progetto separato per
    /// mantenere il servizio SemanticProcessor leggero e disaccoppiato.
    /// </summary>
    public class MiuSystemManager
    {
        private Logger _logger;
        private string _databaseFilePath;

        // Campi per le statistiche globali in memoria, gestite e sincronizzate da questa classe.
        // Questi campi saranno assegnati alle proprietà statiche di RegoleMIUManager all'inizializzazione.
        private Dictionary<long, RuleStatistics> _internalRuleStatistics;
        private Dictionary<Tuple<string, long>, TransitionStatistics> _internalTransitionStatistics;
        private readonly object _statsLock = new object(); // Lock per l'accesso thread-safe alle statistiche

        /// <summary>
        /// Evento per notificare il servizio SemanticProcessor riguardo eventi importanti
        /// come il successo di una ricerca o la scoperta di una nuova regola (futuro).
        /// </summary>
        public event EventHandler<MiuNotificationEventArgs> OnMiuSystemNotification;

        /// <summary>
        /// Costruttore di MiuSystemManager.
        /// </summary>
        /// <param name="logger">L'istanza del logger fornita dal servizio SemanticProcessor.</param>
        public MiuSystemManager(Logger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Log(LogLevel.DEBUG, "[MiuSystemManager] MiuSystemManager istanziato.");
        }

        /// <summary>
        /// Inizializza il sistema MIU, stabilendo la connessione al database
        /// e caricando le statistiche iniziali. Questo metodo è chiamato dal servizio
        /// SemanticProcessor quando la UI ne dà il comando.
        /// </summary>
        /// <param name="databaseFilePath">Il percorso UNC al file del database SQLite.</param>
        public void InitializeMiuSystem(string databaseFilePath)
        {
            if (string.IsNullOrEmpty(databaseFilePath))
            {
                throw new ArgumentException("Il percorso del file database non può essere nullo o vuoto.", nameof(databaseFilePath));
            }
            _databaseFilePath = databaseFilePath;

            _logger.Log(LogLevel.INFO, $"[MiuSystemManager] Inizializzazione sistema MIU con database: {_databaseFilePath}");

            // Creazione di istanze locali per l'inizializzazione.
            SQLiteSchemaLoader initSchemaLoader = null;
            MIUDatabaseManager initDataManager = null;
            MIURepository initRepository = null;
            LearningStatisticsManager initLearningStatsManager = null;

            try
            {
                initSchemaLoader = new SQLiteSchemaLoader(_databaseFilePath, _logger);
                initSchemaLoader.InitializeDatabase(); // Assicura che il file DB esista e sia accessibile.

                initDataManager = new MIUDatabaseManager(initSchemaLoader, _logger);

                // *** NUOVO: Delega l'impostazione della modalità WAL al data manager ***
                // Questo garantisce che MIUDatabaseManager sia l'unica entità a eseguire direttamente comandi PRAGMA.
                initDataManager.SetJournalMode("WAL");
                // *** FINE NUOVO ***

                initRepository = new MIURepository(initDataManager, _logger);
                initLearningStatsManager = new LearningStatisticsManager(initDataManager, _logger);

                // Carica le regole e assegna a RegoleMIUManager (che è statico)
                RegoleMIUManager.CaricaRegoleDaOggettoRepository(initRepository.LoadRegoleMIU());

                // Carica le statistiche iniziali e le assegna alle proprietà statiche di RegoleMIUManager
                // Le statistiche interne a MiuSystemManager (_internalRuleStatistics, _internalTransitionStatistics)
                // fungono da "cache" e fonte di verità per le proprietà statiche di RegoleMIUManager.
                _internalRuleStatistics = initLearningStatsManager.LoadRuleStatistics() ?? new Dictionary<long, RuleStatistics>();
                _internalTransitionStatistics = initLearningStatsManager.GetTransitionProbabilities() ?? new Dictionary<Tuple<string, long>, TransitionStatistics>();

                lock (_statsLock) // Proteggi l'accesso alle statistiche statiche di RegoleMIUManager
                {
                    RegoleMIUManager.CurrentRuleStatistics = _internalRuleStatistics;
                    RegoleMIUManager.CurrentTransitionStatistics = _internalTransitionStatistics;
                }

                // Sottoscrivi agli eventi di RegoleMIUManager (questi aggiorneranno _internalRuleStatistics/_internalTransitionStatistics)
                // L'assegnazione del logger a RegoleMIUManager è fatta qui.
                RegoleMIUManager.OnRuleApplied += RegoleMIUManager_OnRuleApplied;
                RegoleMIUManager.OnSolutionFound += RegoleMIUManager_OnSolutionFound;
                RegoleMIUManager.LoggerInstance = _logger;

                // Carica i parametri di configurazione per RegoleMIUManager (MaxProfonditaRicerca, MassimoPassiRicerca)
                var configParams = initRepository.LoadMIUParameterConfigurator();
                if (configParams.TryGetValue("ProfonditaDiRicerca", out string depthStr) && long.TryParse(depthStr, out long parsedDepth))
                {
                    RegoleMIUManager.MaxProfonditaRicerca = parsedDepth;
                }
                else { RegoleMIUManager.MaxProfonditaRicerca = 10; } // Valore predefinito
                if (configParams.TryGetValue("MassimoPassiRicerca", out string stepsStr) && long.TryParse(stepsStr, out long parsedSteps))
                {
                    RegoleMIUManager.MassimoPassiRicerca = parsedSteps;
                }
                else { RegoleMIUManager.MassimoPassiRicerca = 10; } // Valore predefinito

                _logger.Log(LogLevel.INFO, "[MiuSystemManager] Sistema MIU inizializzato con successo.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MiuSystemManager ERROR] Errore durante l'inizializzazione del sistema MIU: {ex.Message}");
                throw; // Rilancia l'eccezione per informare il servizio chiamante
            }
        }

        /// <summary>
        /// Spegne il sistema MIU, salvando le statistiche finali e rilasciando le risorse.
        /// Questo metodo è chiamato dal servizio SemanticProcessor quando la UI ne dà il comando.
        /// </summary>
        public void ShutdownMiuSystem()
        {
            _logger.Log(LogLevel.INFO, "[MiuSystemManager] Spegnimento sistema MIU.");
            // Creazione di istanze locali per lo spegnimento per salvare le statistiche.
            SQLiteSchemaLoader shutdownSchemaLoader = null;
            MIUDatabaseManager shutdownDataManager = null;
            LearningStatisticsManager shutdownLearningStatsManager = null;

            try
            {
                // Disiscrivi dagli eventi per prevenire memory leak o chiamate dopo la dismissione
                RegoleMIUManager.OnRuleApplied -= RegoleMIUManager_OnRuleApplied;
                RegoleMIUManager.OnSolutionFound -= RegoleMIUManager_OnSolutionFound;

                shutdownSchemaLoader = new SQLiteSchemaLoader(_databaseFilePath, _logger);
                shutdownDataManager = new MIUDatabaseManager(shutdownSchemaLoader, _logger);
                shutdownLearningStatsManager = new LearningStatisticsManager(shutdownDataManager, _logger);

                // Salva le statistiche finali utilizzando le istanze locali
                if (_internalRuleStatistics != null)
                {
                    shutdownLearningStatsManager.SaveRuleStatistics(_internalRuleStatistics);
                }
                if (_internalTransitionStatistics != null)
                {
                    shutdownLearningStatsManager.SaveTransitionStatistics(_internalTransitionStatistics);
                }
                _logger.Log(LogLevel.INFO, "[MiuSystemManager] Statistiche finali salvate.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MiuSystemManager ERROR] Errore durante lo spegnimento del sistema MIU: {ex.Message}");
            }
            finally
            {
                // Resetta le proprietà statiche di RegoleMIUManager per evitare riferimenti a oggetti dismessi.
                // Questo è importante se il servizio dovesse essere riavviato e RegoleMIUManager riutilizzato.
                lock (_statsLock)
                {
                    RegoleMIUManager.CurrentRuleStatistics = null;
                    RegoleMIUManager.CurrentTransitionStatistics = null;
                }
                RegoleMIUManager.LoggerInstance = null;
            }
            _logger.Log(LogLevel.INFO, "[MiuSystemManager] Sistema MIU spento con successo.");
        }

        /// <summary>
        /// Esegue una ricerca MIU in un task in background.
        /// Questo metodo incapsula la logica di ricerca effettiva per il servizio,
        /// creando le proprie istanze di data access per ogni esecuzione.
        /// </summary>
        /// <param name="startCompressed">La stringa di partenza compressa.</param>
        /// <param name="targetCompressed">La stringa target compressa.</param>
        /// <param name="token">Token di cancellazione per consentire l'interruzione della ricerca.</param>
        public async Task PerformMiuSearchTask(string startCompressed, string targetCompressed, CancellationToken token)
        {
            // Creazione di istanze locali per il repository e il data manager specifici per questo Task.
            // Questo garantisce isolamento delle connessioni DB per le operazioni di INSERT/UPDATE.
            SQLiteSchemaLoader taskSchemaLoader = null;
            MIUDatabaseManager taskDataManager = null;
            MIURepository taskRepository = null;

            long searchId = -1; // Inizializza con un valore non valido

            try
            {
                // Inizializza le proprie istanze di data access per questo Task
                taskSchemaLoader = new SQLiteSchemaLoader(_databaseFilePath, _logger);
                // Non chiamiamo InitializeDatabase() qui; dovrebbe essere chiamato una volta all'avvio del servizio.
                taskDataManager = new MIUDatabaseManager(taskSchemaLoader, _logger);
                taskRepository = new MIURepository(taskDataManager, _logger);

                string startStandard = MIUStringConverter.InflateMIUString(startCompressed);
                string targetStandard = MIUStringConverter.InflateMIUString(targetCompressed);

                // Inserisci la ricerca iniziale e ottieni l'ID dal repository locale al Task
                searchId = taskRepository.InsertSearch(
                    startStandard, targetStandard, "SERVICE_AUTO",
                    startStandard.Length, targetStandard.Length,
                    MIUStringHelper.CountI(startStandard), MIUStringHelper.CountU(startStandard),
                    MIUStringHelper.CountI(targetStandard), MIUStringHelper.CountU(targetStandard)
                );
                _logger.Log(LogLevel.INFO, $"[MiuSystemManager] Avvio ricerca MIU SearchID: {searchId} per '{startCompressed}' -> '{targetCompressed}'.");

                // Passa l'ID della ricerca al metodo di ricerca statico.
                // RegoleMIUManager userà i suoi eventi (OnRuleApplied, OnSolutionFound) per notificare
                // il MiuSystemManager di questo Task che gestirà l'aggiornamento delle statistiche in memoria
                // e la persistenza sul DB tramite il suo taskRepository.
                List<PathStepInfo> resultPath = RegoleMIUManager.TrovaDerivazioneAutomatica(searchId, startCompressed, targetCompressed, CancellationToken.None, taskDataManager); // MODIFIED: Passa taskDataManager

                token.ThrowIfCancellationRequested(); // Controlla se è stata richiesta la cancellazione

                if (resultPath != null)
                {
                    _logger.Log(LogLevel.INFO, $"[MiuSystemManager] Ricerca MIU SearchID: {searchId} completata con successo.");
                    OnMiuSystemNotification?.Invoke(this, new MiuNotificationEventArgs("MIU_SEARCH_SUCCESS", $"Ricerca '{startCompressed}' -> '{targetCompressed}' completata con successo."));
                }
                else
                {
                    _logger.Log(LogLevel.WARNING, $"[MiuSystemManager] Ricerca MIU SearchID: {searchId} non trovata.");
                    OnMiuSystemNotification?.Invoke(this, new MiuNotificationEventArgs("MIU_SEARCH_FAILED", $"Ricerca '{startCompressed}' -> '{targetCompressed}' non trovata."));
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log(LogLevel.WARNING, $"[MiuSystemManager] Ricerca MIU SearchID: {searchId} annullata.");
                // Aggiorna lo stato della ricerca a "Cancelled" nel DB tramite il repository locale
                if (taskRepository != null && searchId != -1) // Assicurati che taskRepository sia stato inizializzato
                    taskRepository.UpdateSearch(searchId, false, 0, -1, 0, 0);
                OnMiuSystemNotification?.Invoke(this, new MiuNotificationEventArgs("MIU_SEARCH_CANCELLED", $"Ricerca '{startCompressed}' -> '{targetCompressed}' annullata."));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MiuSystemManager ERROR] Errore nel thread di ricerca MIU SearchID: {searchId}: {ex.Message}");
                // Aggiorna lo stato della ricerca a "Failed" nel DB tramite il repository locale
                if (taskRepository != null && searchId != -1) // Assicurati che taskRepository sia stato inizializzato
                    taskRepository.UpdateSearch(searchId, false, 0, -1, 0, 0);
                OnMiuSystemNotification?.Invoke(this, new MiuNotificationEventArgs("MIU_SEARCH_ERROR", $"Errore durante la ricerca '{startCompressed}' -> '{targetCompressed}': {ex.Message}"));
            }
            finally
            {
                // Rilascia le risorse locali se necessario (per SQLite, basta che le connessioni siano chiuse nei using)
            }
        }

        // --- Event Handlers per RegoleMIUManager (metodi d'istanza) ---
        // Questi metodi d'istanza aggiornano le statistiche in memoria _internalRuleStatistics/_internalTransitionStatistics
        // e persistono le applicazioni di regole.
        // Essendo chiamati dagli eventi statici di RegoleMIUManager, devono gestire l'accesso al database.
        // Poiché i metodi di ricerca ora creano il proprio repository, gli event handler non possono usare un _repository a livello di classe.
        // Devono creare il proprio repository o l'evento deve passare il repository.
        // Per semplicità e coerenza con l'attuale architettura degli eventi statici di RegoleMIUManager,
        // questi event handler ora creeranno il proprio set di MIUDatabaseManager e MIURepository per ogni evento.
        // Questo potrebbe essere inefficiente se gli eventi sono molto frequenti, ma garantisce la thread-safety
        // e il disaccoppiamento richiesto, e il database SQLite è efficiente nel gestire molte piccole connessioni.

        private void RegoleMIUManager_OnRuleApplied(object sender, RuleAppliedEventArgs e)
        {
            // Creazione di istanze locali di data access per questo evento specifico.
            // Questo garantisce che ogni aggiornamento al DB avvenga su una connessione isolata,
            // cruciale per la thread-safety e per l'assenza di riferimenti al repository "principale" del Task.
            SQLiteSchemaLoader eventSchemaLoader = new SQLiteSchemaLoader(_databaseFilePath, _logger);
            MIUDatabaseManager eventDataManager = new MIUDatabaseManager(eventSchemaLoader, _logger);
            MIURepository eventRepository = new MIURepository(eventDataManager, _logger);

            lock (_statsLock) // Protegge l'accesso alle statistiche in memoria _internalRuleStatistics/_internalTransitionStatistics
            {
                // 1. Aggiorna RuleStatistics in memoria
                if (!_internalRuleStatistics.TryGetValue(e.AppliedRuleID, out RuleStatistics ruleStats))
                {
                    ruleStats = new RuleStatistics { RuleID = e.AppliedRuleID };
                    _internalRuleStatistics[e.AppliedRuleID] = ruleStats;
                }
                ruleStats.ApplicationCount++;
                ruleStats.LastApplicationTimestamp = DateTime.Now;
                ruleStats.RecalculateEffectiveness();

                // 2. Aggiorna TransitionStatistics in memoria
                var parentCompressed = MIUStringConverter.DeflateMIUString(e.OriginalString);
                var transitionKey = Tuple.Create(parentCompressed, e.AppliedRuleID);
                if (!_internalTransitionStatistics.TryGetValue(transitionKey, out TransitionStatistics transitionStats))
                {
                    transitionStats = new TransitionStatistics
                    {
                        ParentStringCompressed = parentCompressed,
                        AppliedRuleID = e.AppliedRuleID,
                        SuccessfulCount = 0 // Inizialmente 0, incrementato solo a soluzione trovata
                    };
                    _internalTransitionStatistics[transitionKey] = transitionStats;
                }
                transitionStats.ApplicationCount++;
                transitionStats.LastUpdated = DateTime.Now;

                // 3. Persisti l'applicazione della regola nel database utilizzando il repository locale all'evento
                long parentStateId = eventRepository.UpsertMIUState(e.OriginalString).Item1;
                long newStateId = eventRepository.UpsertMIUState(e.NewString).Item1;

                eventRepository.InsertRuleApplication(
                    e.SearchID, // <- errore 1061
                    parentStateId,
                    newStateId,
                    e.AppliedRuleID,
                    e.CurrentDepth
                );
            }
        }

        private void RegoleMIUManager_OnSolutionFound(object sender, SolutionFoundEventArgs e)
        {
            // Creazione di istanze locali di data access per questo evento specifico.
            // Stesso principio di OnRuleApplied per garantire thread-safety e isolamento.
            SQLiteSchemaLoader eventSchemaLoader = new SQLiteSchemaLoader(_databaseFilePath, _logger);
            MIUDatabaseManager eventDataManager = new MIUDatabaseManager(eventSchemaLoader, _logger);
            MIURepository eventRepository = new MIURepository(eventDataManager, _logger);

            lock (_statsLock) // Protegge l'accesso alle statistiche in memoria _internalRuleStatistics/_internalTransitionStatistics
            {
                // 1. Aggiorna RuleStatistics e TransitionStatistics per le regole/transizioni di successo
                if (e.Success && e.SolutionPathSteps != null)
                {
                    foreach (PathStepInfo step in e.SolutionPathSteps)
                    {
                        if (step.AppliedRuleID.HasValue)
                        {
                            long ruleId = step.AppliedRuleID.Value;
                            if (_internalRuleStatistics.TryGetValue(ruleId, out RuleStatistics ruleStats))
                            {
                                ruleStats.SuccessfulCount++;
                                ruleStats.RecalculateEffectiveness();
                                ruleStats.LastApplicationTimestamp = DateTime.Now;
                            }
                            // OnRuleApplied dovrebbe aver già creato l'entry se non esistente.

                            if (step.ParentStateStringStandard != null)
                            {
                                var parentCompressed = MIUStringConverter.DeflateMIUString(step.ParentStateStringStandard);
                                var transitionKey = Tuple.Create(parentCompressed, step.AppliedRuleID.Value);
                                if (_internalTransitionStatistics.TryGetValue(transitionKey, out TransitionStatistics transitionStats))
                                {
                                    transitionStats.SuccessfulCount++;
                                    transitionStats.LastUpdated = DateTime.Now;
                                }
                            }
                        }
                    }
                }

                // 2. Aggiorna il record della ricerca nel database con i risultati finali
                eventRepository.UpdateSearch(e.SearchID, e.Success, e.ElapsedMilliseconds, e.StepsTaken, e.NodesExplored, e.MaxDepthReached);

                _logger.Log(LogLevel.INFO, $"[MiuSystemManager] Ricerca ID {e.SearchID} completata. Successo: {e.Success}.");
            }

            // 3. Notifica il servizio SemanticProcessor
            string notificationMessage = e.Success
                ? $"Evviva! Il Sistema MIU ha trovato una derivazione per '{e.InitialString}' -> '{e.TargetString}' in {e.StepsTaken} passi."
                : $"Ricerca per '{e.InitialString}' -> '{e.TargetString}' fallita.";
            OnMiuSystemNotification?.Invoke(this, new MiuNotificationEventArgs(e.Success ? "MIU_SOLUTION_FOUND" : "MIU_NO_SOLUTION", notificationMessage));
        }
    }
}
