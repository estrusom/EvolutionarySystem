// File: EvolutiveSystem.Engine/MIUDerivationEngine.cs
// Data di riferimento: 27 giugno 2025 (Aggiornato con Logica di Derivazione e Persistenza)
// Descrizione: Implementazione del motore di derivazione del sistema MIU.
//              Questa classe è responsabile dell'esplorazione dello spazio degli stati,
//              del popolamento del database e della gestione dei cursori di esplorazione,
//              operando su un thread separato.
//              Ora gestisce la registrazione delle ricerche, la persistenza di stati e applicazioni di regole,
//              e l'aggiornamento delle statistiche di apprendimento.

using System;
using System.Collections.Generic; // Necessario per Dictionary e List
using System.Linq; // Necessario per LINQ (.Any(), .FirstOrDefault())
using System.Threading;
using System.Threading.Tasks;
using MasterLog; // Per il Logger
using EvolutiveSystem.Common; // Per MIUExplorerCursor, RegolaMIU, RuleStatistics, TransitionStatistics, MiuStateInfo
using MIU.Core; // Per IMIUDataManager, RegoleMIUManager, SolutionFoundEventArgs, RuleAppliedEventArgs, PathStepInfo, MIUStringConverter
using EvolutiveSystem.Learning; // Per LearningStatisticsManager

namespace EvolutiveSystem.Engine // Namespace specifico per questo nuovo progetto
{
    /// <summary>
    /// Motore di derivazione per il sistema MIU.
    /// Implementa IMIUDataProcessingService per orchestrare l'esplorazione dello spazio degli stati,
    /// la persistenza dei dati nel database e la gestione del cursore di esplorazione.
    /// Opera su un thread separato per non bloccare l'applicazione.
    /// </summary>
    public class MIUDerivationEngine : IMIUDataProcessingService
    {
        private readonly IMIUDataManager _dataManager;
        // RegoleMIUManager rimane statico, quindi non lo iniettiamo direttamente.
        // I suoi metodi saranno chiamati staticamente.
        private readonly Logger _logger;
        private readonly LearningStatisticsManager _learningStatsManager; // Gestisce il caricamento/salvataggio delle statistiche

        private CancellationTokenSource _cancellationTokenSource;
        private Task _explorationTask;

        private long _currentSearchId; // ID della ricerca corrente gestito dal motore
        private Dictionary<long, RuleStatistics> _ruleStatistics; // Statistiche delle regole in memoria
        private Dictionary<Tuple<string, long>, TransitionStatistics> _transitionStatistics; // Statistiche delle transizioni in memoria

        public bool IsExplorationRunning { get; private set; }

        public event EventHandler<string> OnExplorationStatusChanged;
        public event EventHandler<int> OnNodesExploredCountChanged;

        /// <summary>
        /// Costruttore del motore di derivazione.
        /// </summary>
        /// <param name="dataManager">L'istanza del gestore dati per la persistenza.</param>
        /// <param name="learningStatsManager">L'istanza del gestore delle statistiche di apprendimento.</param>
        /// <param name="logger">L'istanza del logger per la registrazione.</param>
        public MIUDerivationEngine(IMIUDataManager dataManager, LearningStatisticsManager learningStatsManager, Logger logger)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _learningStatsManager = learningStatsManager ?? throw new ArgumentNullException(nameof(learningStatsManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            IsExplorationRunning = false;

            // Assicurati che RegoleMIUManager abbia il logger impostato, dato che è statico.
            // Questo potrebbe essere fatto anche in Program.cs all'avvio.
            RegoleMIUManager.LoggerInstance = _logger;

            _logger.Log(LogLevel.INFO, "[MIUDerivationEngine] Motore di derivazione inizializzato.");
        }

        /// <summary>
        /// Avvia l'esplorazione dello spazio degli stati MIU come processo in background (Task).
        /// </summary>
        /// <param name="initialString">La stringa MIU iniziale da cui iniziare l'esplorazione.</param>
        /// <param name="targetString">La stringa MIU target da raggiungere (opzionale).</param>
        public Task StartExplorationAsync(string initialString, string targetString = null)
        {
            if (IsExplorationRunning)
            {
                _logger.Log(LogLevel.WARNING, "[MIUDerivationEngine] L'esplorazione è già in corso.");
                return Task.CompletedTask;
            }

            _logger.Log(LogLevel.INFO, $"[MIUDerivationEngine] Avvio esplorazione da '{initialString}' verso '{targetString ?? "ignoto"}'...");
            OnExplorationStatusChanged?.Invoke(this, "Avvio esplorazione...");

            _cancellationTokenSource = new CancellationTokenSource();
            IsExplorationRunning = true;

            _explorationTask = Task.Run(async () =>
            {
                try
                {
                    // 1. Carica le regole MIU
                    var regoleMIUList = _dataManager.LoadRegoleMIU();
                    RegoleMIUManager.CaricaRegoleDaOggettoRepository(regoleMIUList);
                    _logger.Log(LogLevel.INFO, $"[MIUDerivationEngine] Caricate {RegoleMIUManager.Regole.Count} regole MIU.");

                    // 2. Carica le statistiche di apprendimento all'avvio della ricerca
                    _ruleStatistics = _learningStatsManager.LoadRuleStatistics();
                    _transitionStatistics = _learningStatsManager.LoadTransitionStatistics();

                    // Imposta le statistiche in RegoleMIUManager (che è statico)
                    RegoleMIUManager.CurrentRuleStatistics = _ruleStatistics;
                    RegoleMIUManager.CurrentTransitionStatistics = _transitionStatistics;
                    _logger.Log(LogLevel.INFO, $"[MIUDerivationEngine] Caricate {_ruleStatistics.Count} RuleStatistics e {_transitionStatistics.Count} TransitionStatistics.");

                    // 3. Iscrizione agli eventi di RegoleMIUManager
                    RegoleMIUManager.OnRuleApplied += HandleRuleApplied;
                    RegoleMIUManager.OnSolutionFound += HandleSolutionFound;

                    // 4. Carica il cursore di esplorazione esistente o inizializza
                    MIUExplorerCursor cursor = await _dataManager.LoadExplorerCursorAsync();
                    _logger.Log(LogLevel.DEBUG, $"[MIUDerivationEngine] Cursore caricato: Source={cursor.CurrentSourceIndex}, Target={cursor.CurrentTargetIndex}, LastTimestamp={cursor.LastExplorationTimestamp}");

                    long initialStringStateId = -1;

                    // Se non ci sono ID salvati nel cursore, persistiamo la stringa iniziale
                    if (cursor.CurrentSourceIndex <= 0)
                    {
                        initialStringStateId = _dataManager.UpsertMIUState(initialString);
                        cursor.CurrentSourceIndex = initialStringStateId;
                        _logger.Log(LogLevel.INFO, $"[MIUDerivationEngine] Inizializzato stato di partenza: '{initialString}' (ID: {initialStringStateId}).");
                    }
                    else // Se c'è un cursore salvato, usiamo la sua stringa iniziale
                    {
                        var states = await _dataManager.LoadMIUStatesAsync();
                        var sourceStateInfo = states.FirstOrDefault(s => s.StateID == cursor.CurrentSourceIndex);
                        if (sourceStateInfo != null)
                        {
                            initialString = sourceStateInfo.CurrentString;
                            initialStringStateId = sourceStateInfo.StateID;
                            _logger.Log(LogLevel.INFO, $"[MIUDerivationEngine] Ripresa esplorazione da stato salvato: '{initialString}' (ID: {initialStringStateId}).");
                        }
                        else
                        {
                            _logger.Log(LogLevel.WARNING, $"[MIUDerivationEngine] Impossibile trovare stato con ID {cursor.CurrentSourceIndex}. Riavvio da '{initialString}'.");
                            initialStringStateId = _dataManager.UpsertMIUState(initialString);
                            cursor.CurrentSourceIndex = initialStringStateId;
                        }
                    }

                    // 5. Inserisci la ricerca nel database e ottieni l'ID corrente
                    _currentSearchId = _dataManager.InsertSearch(
                        initialString,
                        targetString,
                        "AutomaticDerivationEngine", // Tipo di algoritmo controllato dal motore
                        initialString.Length,
                        targetString?.Length ?? 0, // Gestisce targetString nullo
                        MIUStringConverter.CountChar(initialString, 'I'),
                        MIUStringConverter.CountChar(initialString, 'U'),
                        targetString != null ? MIUStringConverter.CountChar(targetString, 'I') : 0,
                        targetString != null ? MIUStringConverter.CountChar(targetString, 'U') : 0
                    );
                    _logger.Log(LogLevel.INFO, $"[MIUDerivationEngine] Nuova ricerca registrata con ID: {_currentSearchId}");


                    // Le stringhe da passare a TrovaDerivazioneAutomatica devono essere compresse
                    string compressedInitial = MIUStringConverter.DeflateMIUString(initialString);
                    string compressedTarget = targetString != null ? MIUStringConverter.DeflateMIUString(targetString) : null;

                    // 6. Avvia la derivazione usando RegoleMIUManager (statico)
                    // Il RegoleMIUManager chiamerà gli eventi HandleRuleApplied e HandleSolutionFound.
                    _logger.Log(LogLevel.INFO, $"[MIUDerivationEngine] Chiamata a RegoleMIUManager.TrovaDerivazioneAutomatica per SearchID: {_currentSearchId}");
                    List<PathStepInfo> miuPath = RegoleMIUManager.TrovaDerivazioneAutomatica(
                        _currentSearchId,
                        compressedInitial,
                        compressedTarget,
                        _cancellationTokenSource.Token // Passa il CancellationToken
                    );

                    // Dopo che TrovaDerivazioneAutomatica è terminato (o annullato), aggiorna la ricerca finale
                    // La logica di UpdateSearch e il salvataggio finale delle statistiche si trova in HandleSolutionFound
                    // o in un blocco finally dopo la chiamata a TrovaDerivazioneAutomatica per garantire che avvenga.

                    // Se la soluzione non è stata trovata o annullata (e OnSolutionFound non ha aggiornato),
                    // assicurati di aggiornare comunque la ricerca.
                    // Se miuPath è null o vuoto, la soluzione non è stata trovata.
                    // NOTA: OnSolutionFound dovrebbe già occuparsi di UpdateSearch.
                    // Questa parte è più che altro un fallback per assicurarsi che la ricerca venga chiusa
                    // anche se l'evento OnSolutionFound non dovesse essere scatenato (es. per un errore imprevisto).
                    if (miuPath == null || !miuPath.Any())
                    {
                        _logger.Log(LogLevel.INFO, $"[MIUDerivationEngine] Nessun percorso restituito o esplorazione annullata per ricerca ID: {_currentSearchId}. Se OnSolutionFound non ha agito, lo stato finale verrà gestito qui.");
                        // Questa riga è ridondante se OnSolutionFound è sempre chiamato, ma garantisce fallback.
                        // _dataManager.UpdateSearch(_currentSearchId, false, 0, 0, 0, 0); // Aggiorna come fallita/incompleta
                    }
                    else
                    {
                        _logger.Log(LogLevel.INFO, $"[MIUDerivationEngine] Percorso trovato con {miuPath.Count} passi per ricerca ID: {_currentSearchId}.");
                    }

                    // Salva lo stato finale del cursore dopo ogni sessione di esplorazione
                    cursor.CurrentSourceIndex = initialStringStateId; // O l'ultimo stato esplorato se vuoi ripartire da lì
                    cursor.LastExplorationTimestamp = DateTime.UtcNow;
                    await _dataManager.SaveExplorerCursorAsync(cursor);

                    // Salva le statistiche di apprendimento alla fine della sessione
                    _learningStatsManager.SaveRuleStatistics(_ruleStatistics);
                    _learningStatsManager.SaveTransitionStatistics(_transitionStatistics);
                    _logger.Log(LogLevel.INFO, "[MIUDerivationEngine] Statistiche di apprendimento salvate.");
                }
                catch (OperationCanceledException)
                {
                    _logger.Log(LogLevel.INFO, "[MIUDerivationEngine] Task di esplorazione annullato.");
                    OnExplorationStatusChanged?.Invoke(this, "Esplorazione annullata.");
                    // In caso di annullamento, assicurati che la ricerca sia marcata come fallita/annullata.
                    _dataManager.UpdateSearch(_currentSearchId, false, 0, 0, 0, 0); // O un flag "Cancelled"
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.ERROR, $"[MIUDerivationEngine] Errore critico durante l'esplorazione: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    OnExplorationStatusChanged?.Invoke(this, $"Errore: {ex.Message}");
                    // In caso di errore, assicurati che la ricerca sia marcata come fallita.
                    _dataManager.UpdateSearch(_currentSearchId, false, 0, 0, 0, 0);
                }
                finally
                {
                    IsExplorationRunning = false;
                    // Disiscrizione dagli eventi per evitare memory leak o chiamate a oggetto dismesso
                    RegoleMIUManager.OnRuleApplied -= HandleRuleApplied;
                    RegoleMIUManager.OnSolutionFound -= HandleSolutionFound;
                    OnExplorationStatusChanged?.Invoke(this, "Motore inattivo.");
                }
            }, _cancellationTokenSource.Token);

            return _explorationTask;
        }

        /// <summary>
        /// Richiede l'interruzione dell'esplorazione corrente.
        /// </summary>
        public void StopExploration()
        {
            if (IsExplorationRunning && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _logger.Log(LogLevel.INFO, "[MIUDerivationEngine] Richiesta interruzione esplorazione...");
                _cancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Recupera lo stato attuale del cursore di esplorazione.
        /// </summary>
        /// <returns>L'oggetto MIUExplorerCursor che rappresenta l'ultimo stato noto.</returns>
        public async Task<MIUExplorerCursor> GetCurrentExplorerCursorAsync()
        {
            return await _dataManager.LoadExplorerCursorAsync();
        }

        /// <summary>
        /// Gestisce l'evento OnRuleApplied dal RegoleMIUManager.
        /// Questo metodo persiste l'applicazione della regola e aggiorna le statistiche grezze.
        /// </summary>
        private void HandleRuleApplied(object sender, RuleAppliedEventArgs e)
        {
            string message = $"AppliedRuleID: {e.AppliedRuleID} AppliedRuleName: {e.AppliedRuleName} OriginalString: {e.OriginalString} NewString: {e.NewString} CurrentDepth: {e.CurrentDepth}";
            _logger.Log(LogLevel.DEBUG, $"[MIUDerivationEngine - Rule Applied] {message}");

            // Persistenza dello stato originale e del nuovo stato
            long parentStateId = _dataManager.UpsertMIUState(e.OriginalString);
            long newStateId = _dataManager.UpsertMIUState(e.NewString);

            // Persistenza dell'applicazione della regola
            _dataManager.InsertRuleApplication(
                _currentSearchId, // Usa l'ID della ricerca corrente gestito dal motore
                parentStateId,
                newStateId,
                e.AppliedRuleID,
                e.CurrentDepth
            );

            // Aggiornamento delle RuleStatistics
            if (!_ruleStatistics.ContainsKey(e.AppliedRuleID))
            {
                _logger.Log(LogLevel.WARNING, $"[MIUDerivationEngine - Learning] Rule {e.AppliedRuleID} not found in _ruleStatistics. Creating new entry.");
                _ruleStatistics[e.AppliedRuleID] = new RuleStatistics { RuleID = e.AppliedRuleID };
            }
            _ruleStatistics[e.AppliedRuleID].ApplicationCount++;
            _ruleStatistics[e.AppliedRuleID].LastApplicationTimestamp = DateTime.Now;
            _ruleStatistics[e.AppliedRuleID].RecalculateEffectiveness();
            _logger.Log(LogLevel.DEBUG, $"[MIUDerivationEngine - Learning] Rule {e.AppliedRuleID} ({RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == e.AppliedRuleID)?.Nome ?? "Unknown"}) ApplicationCount incremented to {_ruleStatistics[e.AppliedRuleID].ApplicationCount}.");

            // Aggiornamento TransitionStatistics
            if (e.OriginalString != null)
            {
                var parentCompressed = MIUStringConverter.DeflateMIUString(e.OriginalString);
                var transitionKey = Tuple.Create(parentCompressed, e.AppliedRuleID);

                if (!_transitionStatistics.ContainsKey(transitionKey))
                {
                    _transitionStatistics[transitionKey] = new TransitionStatistics
                    {
                        ParentStringCompressed = parentCompressed,
                        AppliedRuleID = e.AppliedRuleID,
                        ApplicationCount = 0, // Verrà incrementato sotto
                        SuccessfulCount = 0,
                        LastUpdated = DateTime.Now
                    };
                    _logger.Log(LogLevel.WARNING, $"[MIUDerivationEngine - Learning] Transition {parentCompressed} -> Rule {e.AppliedRuleID} not found in _transitionStatistics. Creating new entry.");
                }
                _transitionStatistics[transitionKey].ApplicationCount++;
                _transitionStatistics[transitionKey].LastUpdated = DateTime.Now;
            }
        }

        /// <summary>
        /// Gestisce l'evento OnSolutionFound dal RegoleMIUManager.
        /// Questo metodo finalizza la registrazione della ricerca e aggiorna le statistiche di successo.
        /// </summary>
        private void HandleSolutionFound(object sender, SolutionFoundEventArgs e)
        {
            string pathString = "N/A";
            if (e.SolutionPathSteps != null && e.SolutionPathSteps.Any())
            {
                pathString = string.Join(" -> ", e.SolutionPathSteps.Select(step => step.StateStringStandard));
            }

            string message = $"SearchID: {e.SearchID} Success: {e.Success} InitialString: {e.InitialString} TargetString: {e.TargetString} ElapsedMilliseconds: {e.ElapsedMilliseconds} StepsTaken: {e.StepsTaken} NodesExplored: {e.NodesExplored} MaxDepthReached: {e.MaxDepthReached} Algorithm: {e.SearchAlgorithmUsed} Path: {pathString}";
            _logger.Log(e.Success ? LogLevel.INFO : LogLevel.WARNING, $"[MIUDerivationEngine - Solution Found] {message}");
            OnExplorationStatusChanged?.Invoke(this, e.Success ? "Soluzione trovata!" : "Soluzione non trovata.");


            // Aggiorna la ricerca nel DB
            _dataManager.UpdateSearch(e.SearchID, e.Success, e.ElapsedMilliseconds, e.StepsTaken, e.NodesExplored, e.MaxDepthReached);

            if (e.Success && e.SolutionPathSteps != null)
            {
                foreach (PathStepInfo step in e.SolutionPathSteps)
                {
                    if (step.AppliedRuleID.HasValue)
                    {
                        long ruleId = step.AppliedRuleID.Value;
                        if (_ruleStatistics.TryGetValue(ruleId, out RuleStatistics ruleStats))
                        {
                            ruleStats.SuccessfulCount++;
                            ruleStats.RecalculateEffectiveness();
                            _logger.Log(LogLevel.DEBUG, $"[MIUDerivationEngine - Learning] Rule {ruleId} ({RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == ruleId)?.Nome ?? "Unknown"}) SuccessfulCount incremented to {ruleStats.SuccessfulCount}. Effectiveness: {ruleStats.EffectivenessScore:F4}");
                        }
                        else
                        {
                            _logger.Log(LogLevel.WARNING, $"[MIUDerivationEngine - Learning] Rule {ruleId} in successful path not found in _ruleStatistics. This should not happen if HandleRuleApplied works correctly.");
                            // Questo caso indica una potenziale incongruenza, la regola dovrebbe essere stata aggiunta in HandleRuleApplied.
                        }

                        // Aggiorna TransitionStatistics solo per i passi della soluzione di successo
                        if (step.ParentStateStringStandard != null)
                        {
                            var parentCompressed = MIUStringConverter.DeflateMIUString(step.ParentStateStringStandard);
                            var transitionKey = Tuple.Create(parentCompressed, step.AppliedRuleID.Value);

                            if (_transitionStatistics.TryGetValue(transitionKey, out TransitionStatistics transitionStats))
                            {
                                transitionStats.SuccessfulCount++;
                                // SuccessRate viene ricalcolato automaticamente dalla proprietà get
                                _logger.Log(LogLevel.DEBUG, $"[MIUDerivationEngine - Learning] Transition {parentCompressed} -> Rule {ruleId} SuccessfulCount incremented.");
                            }
                            else
                            {
                                _logger.Log(LogLevel.WARNING, $"[MIUDerivationEngine - Learning] Transition {parentCompressed} -> Rule {ruleId} in successful path not found in _transitionStatistics. This should not happen.");
                                // Anche qui, potenziale incongruenza
                            }
                        }

                        // Inserisce i passi del percorso nel DB
                        _dataManager.InsertSolutionPathStep(
                            e.SearchID,
                            step.StepNumber,
                            step.StateID,
                            step.ParentStateID,
                            step.AppliedRuleID,
                            step.StateStringStandard == e.TargetString, // True se è lo stato target
                            e.Success, // true se la ricerca è riuscita
                            step.Depth
                        );
                    }
                }
            }
        }
    }
}
