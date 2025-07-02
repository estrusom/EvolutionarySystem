// File: EvolutiveSystem.Automation/MiuContinuousExplorerScheduler.cs
// Data di riferimento: 01 luglio 2025 (Classe MiuContinuousExplorerScheduler Completa)

using System;
using System.Collections.Generic;
using System.Linq; // Required for OrderBy, FirstOrDefault, Any
using System.Threading; // Required for CancellationToken, CancellationTokenSource, ManualResetEvent
using System.Threading.Tasks; // Required for Task, Task.Run, Task.Delay
using MasterLog; // For Logger, LogLevel
using MIU.Core; // For MiuStateInfo, IMIUDataManager, IMIURepository (interfaces)
using EvolutiveSystem.Engine; // For MIUDerivationEngine
using EvolutiveSystem.Common; // For MIUDerivationEngine (concrete class) - Assicurati che questo sia il namespace corretto per MIUDerivationEngine

namespace EvolutiveSystem.Automation // This is the new project's namespace
{
    // --- CLASSI EVENTARGS ---
    // Queste classi trasportano i dati specifici per ogni tipo di evento.

    /// <summary>
    /// Argomenti per l'evento di aggiornamento del progresso dell'esplorazione MIU.
    /// </summary>
    public class MiuExplorationProgressEventArgs : EventArgs
    {
        public long CurrentSourceId { get; } // ID della stringa sorgente corrente
        public long CurrentTargetId { get; } // ID della stringa target corrente
        public int TotalSources { get; }     // Numero totale di stringhe sorgente
        public int TotalTargetsPerSource { get; } // Numero totale di stringhe target per ogni sorgente
        public string CurrentSourceString { get; }
        public string CurrentTargetString { get; }
        public string CurrentStatus { get; }
        public int ExploredPairsCount { get; } // Numero di coppie (source, target) esplorate finora
        public int TotalNewMiuStringsFound { get; } // Numero totale di nuove stringhe MIU trovate
        public int NodesExploredInCurrentEngineWave { get; } // Nodi esplorati dal motore nell'ultima wave

        public MiuExplorationProgressEventArgs(
            long currentSourceId, long currentTargetId, int totalSources, int totalTargetsPerSource,
            string currentSourceString, string currentTargetString, string currentStatus,
            int exploredPairsCount, int totalNewMiuStringsFound, int nodesExploredInCurrentEngineWave)
        {
            CurrentSourceId = currentSourceId;
            CurrentTargetId = currentTargetId;
            TotalSources = totalSources;
            TotalTargetsPerSource = totalTargetsPerSource;
            CurrentSourceString = currentSourceString;
            CurrentTargetString = currentTargetString;
            CurrentStatus = currentStatus;
            ExploredPairsCount = exploredPairsCount;
            TotalNewMiuStringsFound = totalNewMiuStringsFound;
            NodesExploredInCurrentEngineWave = nodesExploredInCurrentEngineWave;
        }
    }

    /// <summary>
    /// Argomenti per l'evento di completamento dell'esplorazione MIU.
    /// </summary>
    public class MiuExplorationCompletedEventArgs : EventArgs
    {
        public bool IsSuccessful { get; }
        public string FinalMessage { get; }
        public int TotalPairsExplored { get; }
        public int TotalNewMiuStringsFound { get; }

        public MiuExplorationCompletedEventArgs(bool isSuccessful, string finalMessage, int totalPairsExplored, int totalNewMiuStringsFound)
        {
            IsSuccessful = isSuccessful;
            FinalMessage = finalMessage;
            TotalPairsExplored = totalPairsExplored;
            TotalNewMiuStringsFound = totalNewMiuStringsFound;
        }
    }

    /// <summary>
    /// Argomenti per l'evento di errore durante l'esplorazione MIU.
    /// </summary>
    public class MiuExplorationErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; }
        public Exception Exception { get; }

        public MiuExplorationErrorEventArgs(string errorMessage, Exception exception)
        {
            ErrorMessage = errorMessage;
            Exception = exception;
        }
    }

    /// <summary>
    /// Manages the continuous MIU exploration loop, iterating through (source, target) pairs
    /// from the knowledge base and persisting progress.
    /// </summary>
    public class MiuContinuousExplorerScheduler
    {
        // --- CAMPI ESISTENTI ---
        private readonly MIUDerivationEngine _miuDerivationEngine;
        private readonly IMIUDataManager _miuDataManager;
        private readonly IMIURepository _miuRepositoryInstance; // Used to save configuration parameters
        private readonly Logger _logger;
        private readonly Dictionary<string, string> _configParam; // The in-memory configuration parameter dictionary
        private int _currentNodesExploredInEngineWave = 0; // Contatore dei nodi esplorati dal motore nella singola wave

        // Fields for Task and cancellation management
        private CancellationTokenSource _cancellationTokenSource;
        private Task _schedulerTask;
        private volatile bool _isSchedulerRunning = false; // Indica se lo scheduler è in esecuzione

        // Constants for persistent cursor keys
        private const string ContinuousExplorerSourceIdKey = "ContinuousExplorer_CurrentSourceId";
        private const string ContinuousExplorerTargetIdKey = "ContinuousExplorer_CurrentTargetId";

        // --- NUOVI CAMPI PER LA GESTIONE PAUSA/RIPRESA ---
        // Inizializzato a 'true' (segnale) per permettere l'esecuzione iniziale senza attese.
        private ManualResetEvent _pauseEvent = new ManualResetEvent(true);
        private volatile bool _isPaused = false; // Indica se lo scheduler è in pausa

        // --- NUOVI EVENTI PUBBLICI ---
        // Questi eventi possono essere sottoscritti dal SemanticProcessorService.
        public event EventHandler<MiuExplorationProgressEventArgs> ProgressUpdated;
        public event EventHandler<MiuExplorationCompletedEventArgs> ExplorationCompleted;
        public event EventHandler<MiuExplorationErrorEventArgs> ExplorationError;
        public event EventHandler<NewMiuStringFoundEventArgs> NewMiuStringFound; // Evento per nuove scoperte

        // --- Contatori per gli eventi ---
        private int _totalExploredPairs = 0;
        private int _totalNewMiuStringsFound = 0;


        /// <summary>
        /// Constructor for the continuous exploration scheduler.
        /// </summary>
        /// <param name="miuDerivationEngine">Instance of the MIU derivation engine.</param>
        /// <param name="miuDataManager">Instance of the MIU data manager.</param>
        /// <param name="miuRepositoryInstance">Instance of the MIU repository for saving configuration.</param>
        /// <param name="logger">Instance of the logger.</param>
        /// <param name="configParam">The in-memory configuration parameter dictionary of the service.</param>
        public MiuContinuousExplorerScheduler(
            MIUDerivationEngine miuDerivationEngine,
            IMIUDataManager miuDataManager,
            IMIURepository miuRepositoryInstance,
            Logger logger,
            Dictionary<string, string> configParam)
        {
            _miuDerivationEngine = miuDerivationEngine ?? throw new ArgumentNullException(nameof(miuDerivationEngine));
            _miuDataManager = miuDataManager ?? throw new ArgumentNullException(nameof(miuDataManager));
            _miuRepositoryInstance = miuRepositoryInstance ?? throw new ArgumentNullException(nameof(miuRepositoryInstance));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configParam = configParam ?? throw new ArgumentNullException(nameof(configParam));

            _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Initialized.");

            _miuDerivationEngine.OnExplorationStatusChanged += _miuDerivationEngine_OnExplorationStatusChanged;
            _miuDerivationEngine.OnNodesExploredCountChanged += _miuDerivationEngine_OnNodesExploredCountChanged;
            _miuDerivationEngine.OnNewStringDiscovered += _miuDerivationEngine_OnNewStringDiscovered; ; // 2025.07.02
            // *** Sottoscrizione all'evento del MIUDerivationEngine ***
            // Assicurati che MIUDerivationEngine abbia un evento NewStringDiscovered o simile.
            // Se il tuo MIUDerivationEngine non ha un evento, dovrai implementarlo lì.
            // Esempio: _miuDerivationEngine.NewStringDiscovered += HandleNewStringDiscoveredByEngine;
            // Se MIUDerivationEngine non espone un evento, dovrai trovare un altro modo
            // per ottenere il conteggio delle nuove stringhe dopo ogni StartExplorationAsync.
            // Per ora, userò un placeholder per questo.
            // Se MIUDerivationEngine ha un evento, decommenta e implementa HandleNewStringDiscoveredByEngine.
            // _miuDerivationEngine.NewStringDiscovered += HandleNewStringDiscoveredByEngine; 
        }
        private void _miuDerivationEngine_OnNewStringDiscovered(object sender, NewMiuStringFoundEventArgs e)
        {
            _totalNewMiuStringsFound++; // Incrementa il contatore totale delle nuove stringhe trovate
            OnNewMiuStringFound(e); // <- errore cs1503 **** Solleva l'evento pubblico dello scheduler
            _logger.Log(LogLevel.INFO, $"[MiuContinuousExplorerScheduler] Nuova stringa MIU scoperta dal motore: '{e.NewMiuString}'. (Totale: {_totalNewMiuStringsFound})");
        }

        /// <summary>
        /// Gestisce l'evento OnExplorationStatusChanged dal MIUDerivationEngine.
        /// Utilizzato principalmente per il logging dettagliato dello stato del motore.
        /// Non genera direttamente un evento verso il SemanticProcessorService, ma informa lo scheduler
        /// sullo stato interno della singola esplorazione del motore.
        /// </summary>
        private void _miuDerivationEngine_OnNodesExploredCountChanged(object sender, int e)
        {
            // Aggiorna il campo privato che tiene traccia dei nodi esplorati dal motore nella wave attuale.
            _currentNodesExploredInEngineWave = e;
            _logger.Log(LogLevel.DEBUG, $"[MiuContinuousExplorerScheduler] Received from Engine: Nodes Explored = {e}");
        }
        /// <summary>
        /// Gestisce l'evento OnNodesExploredCountChanged dal MIUDerivationEngine.
        /// Aggiorna il contatore interno dei nodi esplorati per la wave corrente del motore.
        /// Questo valore verrà poi utilizzato dall'evento ProgressUpdated dello scheduler.
        /// </summary>
        private void _miuDerivationEngine_OnExplorationStatusChanged(object sender, string e)
        {
            // Logga il messaggio di stato ricevuto dal motore.
            _logger.Log(LogLevel.DEBUG, $"[MiuContinuousExplorerScheduler] Received from Engine: Status = '{e}'");
            // Questo handler non aggiorna _currentNodesExploredInEngineWave, che è compito dell'altro handler.
        }

        // *** Metodo per gestire l'evento del MIUDerivationEngine e sollevare il proprio evento ***
        // private void HandleNewStringDiscoveredByEngine(object sender, NewMiuStringFoundEventArgs e)
        // {
        //     _totalNewMiuStringsFound++; // Incrementa il contatore totale
        //     OnNewMiuStringFound(e); // Solleva l'evento dello scheduler
        // }


        /// <summary>
        /// Starts the continuous exploration loop in the background.
        /// </summary>
        public void StartScheduler()
        {
            if (_isSchedulerRunning)
            {
                _logger.Log(LogLevel.WARNING, "[MiuContinuousExplorerScheduler] Scheduler is already running.");
                return;
            }
            if (_miuDerivationEngine == null)
            {
                _logger.Log(LogLevel.ERROR, "[MiuContinuousExplorerScheduler] MIUDerivationEngine not initialized. Cannot start.");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _isSchedulerRunning = true;
            _isPaused = false; // Assicurati che non sia in pausa all'avvio
            _pauseEvent.Set(); // Assicurati che il segnale sia impostato per iniziare a correre

            // Reset dei contatori all'avvio di un nuovo ciclo completo
            _totalExploredPairs = 0;
            _totalNewMiuStringsFound = 0;

            _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Starting continuous scheduling Task.");
            _schedulerTask = Task.Run(async () => await ExplorationLoop(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// Stops the continuous exploration loop.
        /// </summary>
        public void StopScheduler()
        {
            if (!_isSchedulerRunning)
            {
                _logger.Log(LogLevel.WARNING, "[MiuContinuousExplorerScheduler] Scheduler is not running or already stopped.");
                return;
            }

            _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Requesting scheduler shutdown.");
            _cancellationTokenSource?.Cancel(); // Signals cancellation

            // Se lo scheduler è in pausa, sbloccalo per permettere al task di terminare
            if (_isPaused)
            {
                _pauseEvent.Set();
                _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Scheduler sbloccato dalla pausa per consentire l'arresto.");
            }

            try
            {
                // Attendi che il task di esplorazione termini (con un timeout per evitare blocchi indefiniti)
                _schedulerTask?.Wait(TimeSpan.FromSeconds(30));
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    _logger.Log(LogLevel.ERROR, $"[MiuContinuousExplorerScheduler] Errore durante l'attesa di arresto del task: {ex.Message}");
                    OnExplorationError(new MiuExplorationErrorEventArgs("Errore durante l'arresto del task.", ex));
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Attesa di arresto cancellata.");
            }
            finally
            {
                _isSchedulerRunning = false;
                _isPaused = false; // Reset dello stato di pausa
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _schedulerTask = null;
                _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Scheduler arrestato e risorse rilasciate.");
                // Notifica completamento/arresto con i totali accumulati
                OnExplorationCompleted(new MiuExplorationCompletedEventArgs(true, "Scheduler arrestato dall'utente.", _totalExploredPairs, _totalNewMiuStringsFound));
            }
        }

        /// <summary>
        /// Returns true if the scheduler is currently running.
        /// </summary>
        public bool IsRunning => _isSchedulerRunning;

        /// <summary>
        /// Returns true if the scheduler is currently paused.
        /// </summary>
        public bool IsPaused => _isPaused; // Nuova proprietà per lo stato di pausa

        /// <summary>
        /// Mette in pausa l'esecuzione dello scheduler.
        /// </summary>
        public void PauseScheduler()
        {
            if (!_isSchedulerRunning)
            {
                _logger.Log(LogLevel.WARNING, "[MiuContinuousExplorerScheduler] Impossibile mettere in pausa: lo scheduler non è in esecuzione.");
                return;
            }
            if (_isPaused)
            {
                _logger.Log(LogLevel.WARNING, "[MiuContinuousExplorerScheduler] Scheduler già in pausa.");
                return;
            }

            _isPaused = true;
            _pauseEvent.Reset(); // Imposta il segnale a 'non segnalato', bloccando i thread che chiamano WaitOne()
            _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Scheduler messo in pausa.");
        }

        /// <summary>
        /// Riprende l'esecuzione dello scheduler.
        /// </summary>
        public void ResumeScheduler()
        {
            if (!_isSchedulerRunning)
            {
                _logger.Log(LogLevel.WARNING, "[MiuContinuousExplorerScheduler] Impossibile riprendere: lo scheduler non è in esecuzione.");
                return;
            }
            if (!_isPaused)
            {
                _logger.Log(LogLevel.WARNING, "[MiuContinuousExplorerScheduler] Scheduler non in pausa.");
                return;
            }

            _isPaused = false;
            _pauseEvent.Set(); // Imposta il segnale a 'segnalato', sbloccando i thread in attesa
            _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Scheduler ripreso.");
        }

        /// <summary>
        /// Main loop for continuous exploration (nested loop logic).
        /// This loop iterates through all possible (source, target) pairs from the MIU_States table.
        /// Its progress is persisted via cursors in the configuration database.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the loop.</param>
        private async Task ExplorationLoop(CancellationToken cancellationToken)
        {
            _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Continuous (pairwise) exploration loop started.");

            try
            {
                while (!cancellationToken.IsCancellationRequested) // The outer loop (implicitly) continues until cancelled
                {
                    // --- CONTROLLO PAUSA ALL'INIZIO DEL CICLO ESTERNO ---
                    _pauseEvent.WaitOne();
                    if (cancellationToken.IsCancellationRequested) break;

                    cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation at the beginning of each iteration

                    // Load all known MIU states from the database
                    // Reload at the beginning of each full "pass" to update the list if new states were discovered
                    List<MiuStateInfo> allMiuStates = await _miuDataManager.LoadMIUStatesAsync();

                    if (allMiuStates == null || !allMiuStates.Any())
                    {
                        _logger.Log(LogLevel.WARNING, "[MiuContinuousExplorerScheduler] No MIU states found in the database. Cannot start pairwise loop. Waiting...");
                        _isSchedulerRunning = false; // Stop the scheduler if no states exist
                        OnExplorationCompleted(new MiuExplorationCompletedEventArgs(false, "Nessuno stato MIU trovato nel database.", _totalExploredPairs, _totalNewMiuStringsFound));
                        return; // Exit the Task.Run method
                    }

                    // Order states by ID to ensure consistent progression
                    allMiuStates = allMiuStates.OrderBy(s => s.StateID).ToList();

                    // Load persistent cursors from configuration
                    long currentSourceId = 0;
                    long currentTargetId = 0;

                    if (_configParam.TryGetValue(ContinuousExplorerSourceIdKey, out string sourceIdStr) && long.TryParse(sourceIdStr, out long parsedSourceId))
                    {
                        currentSourceId = parsedSourceId;
                    }
                    if (_configParam.TryGetValue(ContinuousExplorerTargetIdKey, out string targetIdStr) && long.TryParse(targetIdStr, out long parsedTargetId))
                    {
                        currentTargetId = parsedTargetId;
                    }

                    _logger.Log(LogLevel.INFO, $"[MiuContinuousExplorerScheduler] Resuming from SourceId: {currentSourceId}, TargetId: {currentTargetId}");

                    bool completedAllPairsInThisPass = true; // Flag to track if all pairs were processed in this pass

                    // Outer loop: selects the SOURCE string
                    foreach (var sourceState in allMiuStates)
                    {
                        // --- CONTROLLO PAUSA ALL'INIZIO DEL CICLO ESTERNO DI FOREACH ---
                        _pauseEvent.WaitOne();
                        if (cancellationToken.IsCancellationRequested) break;

                        cancellationToken.ThrowIfCancellationRequested();

                        // If resuming from a specific point, skip already processed sources
                        if (sourceState.StateID < currentSourceId)
                        {
                            continue;
                        }
                        // If we moved to a new source (or resumed after a full pass),
                        // reset the target cursor for this new source, and update the source.
                        // This handles both initial startup and resuming after completing a source.
                        if (sourceState.StateID > currentSourceId)
                        {
                            currentSourceId = sourceState.StateID; // Update source cursor
                            currentTargetId = 0; // Reset target for the new source
                            completedAllPairsInThisPass = false; // We are starting a new pass
                        }
                        // Save cursors before starting the inner loop for this source
                        // This covers the case where the service is stopped in the middle of the inner loop.
                        _configParam[ContinuousExplorerSourceIdKey] = currentSourceId.ToString();
                        _configParam[ContinuousExplorerTargetIdKey] = currentTargetId.ToString();
                        _miuRepositoryInstance.SaveMIUParameterConfigurator(_configParam); // Persist the updated dictionary

                        _logger.Log(LogLevel.INFO, $"[MiuContinuousExplorerScheduler] Outer loop: Exploring from SOURCE: '{sourceState.CurrentString}' (ID: {sourceState.StateID}).");

                        // Inner loop: selects the TARGET string
                        foreach (var targetState in allMiuStates)
                        {
                            // --- CONTROLLO PAUSA ALL'INIZIO DEL CICLO INTERNO DI FOREACH ---
                            _pauseEvent.WaitOne();
                            if (cancellationToken.IsCancellationRequested) break;

                            cancellationToken.ThrowIfCancellationRequested();

                            // Skip already processed targets for this source (if not at first execution)
                            if (targetState.StateID <= currentTargetId && sourceState.StateID == currentSourceId)
                            {
                                continue;
                            }

                            // Avoid exploring from a string to itself (if desired)
                            if (sourceState.StateID == targetState.StateID)
                            {
                                continue;
                            }

                            completedAllPairsInThisPass = false; // We are processing a pair, so not all pairs are finished yet

                            _logger.Log(LogLevel.INFO, $"[MiuContinuousExplorerScheduler] Inner loop: Exploring from '{sourceState.CurrentString}' (ID: {sourceState.StateID}) to TARGET: '{targetState.CurrentString}' (ID: {targetState.StateID}).");

                            // 1. Check if the MIU engine is busy from another exploration (e.g., launched by CmdMIUexploration)
                            while (_miuDerivationEngine.IsExplorationRunning && !cancellationToken.IsCancellationRequested)
                            {
                                // --- CONTROLLO PAUSA DURANTE L'ATTESA DEL MOTORE MIU ---
                                _pauseEvent.WaitOne();
                                if (cancellationToken.IsCancellationRequested) break;

                                _logger.Log(LogLevel.DEBUG, "[MiuContinuousExplorerScheduler] MIU Engine currently running. Waiting for it to free up.");
                                await Task.Delay(5000, cancellationToken); // Wait 5 seconds before retrying
                            }
                            cancellationToken.ThrowIfCancellationRequested();

                            // 2. Start MIU exploration and await its completion
                            _logger.Log(LogLevel.INFO, $"[MiuContinuousExplorerScheduler] Starting MIU DerivationEngine for '{sourceState.CurrentString}' -> '{targetState.CurrentString}'.");

                            // *** Qui dovresti sottoscrivere agli eventi del MIUDerivationEngine se vuoi ricevere notifiche
                            //     specifiche dall'esplorazione di una singola coppia (es. NewMiuStringFound) ***
                            // Se MIUDerivationEngine ha un evento NewStringDiscovered, puoi gestirlo nel costruttore
                            // di questo scheduler e poi sollevare l'evento NewMiuStringFound dello scheduler.

                            await _miuDerivationEngine.StartExplorationAsync(sourceState.CurrentString, targetState.CurrentString);
                            _logger.Log(LogLevel.INFO, $"[MiuContinuousExplorerScheduler] Exploration wave completed for '{sourceState.CurrentString}' -> '{targetState.CurrentString}'.");

                            // Aggiorna i contatori per l'evento di progresso
                            _totalExploredPairs++; // Incrementa il contatore delle coppie esplorate

                            // Se _miuDerivationEngine.StartExplorationAsync restituisce il numero di nuove stringhe trovate, aggiorna _totalNewMiuStringsFound
                            // O se MIUDerivationEngine solleva un evento NewStringDiscovered, il contatore _totalNewMiuStringsFound
                            // verrà aggiornato dal metodo HandleNewStringDiscoveredByEngine (se implementato e sottoscritto).
                            // Esempio: int newStringsThisWave = _miuDerivationEngine.GetNewStringsCountFromLastExploration();
                            // _totalNewMiuStringsFound += newStringsThisWave;

                            // Solleva un evento di progresso dopo ogni coppia (source, target) esplorata
                            OnProgressUpdated(new MiuExplorationProgressEventArgs(
                                sourceState.StateID,
                                targetState.StateID,
                                allMiuStates.Count,
                                allMiuStates.Count, // Assumendo che ogni sorgente possa avere tutte le altre come target
                                sourceState.CurrentString,
                                targetState.CurrentString,
                                $"Esplorando coppia ({sourceState.StateID}, {targetState.StateID})",
                                _totalExploredPairs,
                                _totalNewMiuStringsFound,
                                _currentNodesExploredInEngineWave
                            ));

                            // 3. Update the Target cursor and persist it
                            currentTargetId = targetState.StateID;
                            _configParam[ContinuousExplorerSourceIdKey] = currentSourceId.ToString(); // Reconfirm source, for safety
                            _configParam[ContinuousExplorerTargetIdKey] = currentTargetId.ToString();
                            _miuRepositoryInstance.SaveMIUParameterConfigurator(_configParam); // Persist the updated dictionary

                            // --- CONTROLLO PAUSA DOPO L'AGGIORNAMENTO DEL CURSORE ---
                            _pauseEvent.WaitOne();
                            if (cancellationToken.IsCancellationRequested) break;

                            cancellationToken.ThrowIfCancellationRequested();

                            // Small pause between derivations
                            await Task.Delay(1000, cancellationToken); // 1 second pause
                        }
                        _logger.Log(LogLevel.INFO, $"[MiuContinuousExplorerScheduler] All targets explored for SOURCE: '{sourceState.CurrentString}'.");
                        // L'evento di progresso per la sorgente è già stato sollevato dopo ogni coppia.
                        // Puoi aggiungere qui un log o una notifica specifica se desideri un riepilogo per ogni sorgente completata.
                    }

                    if (completedAllPairsInThisPass)
                    {
                        _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] All SOURCE-TARGET pairs in the database have been explored.");
                        // Reset cursors to start from the beginning in the next outer loop iteration (if the scheduler is not stopped)
                        currentSourceId = 0;
                        currentTargetId = 0;
                        _configParam[ContinuousExplorerSourceIdKey] = currentSourceId.ToString();
                        _configParam[ContinuousExplorerTargetIdKey] = currentTargetId.ToString();
                        _miuRepositoryInstance.SaveMIUParameterConfigurator(_configParam);
                        _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Resetting cursors to restart pairwise exploration from the beginning.");

                        // --- CONTROLLO PAUSA PRIMA DI RIPARTIRE CON UN NUOVO CICLO COMPLETO ---
                        _pauseEvent.WaitOne();
                        if (cancellationToken.IsCancellationRequested) break;

                        await Task.Delay(5000, cancellationToken); // Pause before restarting the full cycle
                    }
                    else
                    {
                        // If we reached here, it means we processed some pairs but didn't finish all,
                        // so the outer while loop will continue to the next pass.
                        _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Continuing to the next pass of pairwise exploration.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Continuous exploration loop cancelled.");
                OnExplorationCompleted(new MiuExplorationCompletedEventArgs(true, "Esplorazione cancellata dall'utente.", _totalExploredPairs, _totalNewMiuStringsFound));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MiuContinuousExplorerScheduler] Critical error in the continuous loop: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                OnExplorationError(new MiuExplorationErrorEventArgs("Errore critico nell'esplorazione continua.", ex));
            }
            finally
            {
                _isSchedulerRunning = false;
                _isPaused = false; // Assicurati che lo stato di pausa sia resettato alla terminazione
                _logger.Log(LogLevel.INFO, "[MiuContinuousExplorerScheduler] Continuous exploration scheduler terminated.");
                // OnExplorationCompleted è già chiamato nel catch (OperationCanceledException) e nel finally di StopScheduler.
                // Se il loop termina normalmente (es. tutti i cicli completati senza cancellazione),
                // potresti voler chiamare OnExplorationCompleted qui con un messaggio di successo.
                // Per ora, l'evento di completamento è gestito dalle clausole di uscita specifiche.
            }
        }

        // --- METODI PROTETTI PER SOLLEVARE GLI EVENTI ---
        // Chiamati internamente dalla logica dello scheduler per notificare gli eventi.
        protected virtual void OnProgressUpdated(MiuExplorationProgressEventArgs e)
        {
            ProgressUpdated?.Invoke(this, e);
        }

        protected virtual void OnExplorationCompleted(MiuExplorationCompletedEventArgs e)
        {
            ExplorationCompleted?.Invoke(this, e);
        }

        protected virtual void OnExplorationError(MiuExplorationErrorEventArgs e)
        {
            ExplorationError?.Invoke(this, e);
        }

        protected virtual void OnNewMiuStringFound(NewMiuStringFoundEventArgs e)
        {
            NewMiuStringFound?.Invoke(this, e);
        }
    }
}
