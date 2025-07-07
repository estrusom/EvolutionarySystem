// File: C:\Progetti\EvolutiveSystem\MIU.Core\RegoleMIU.cs
// Data di riferimento: 20 giugno 2025 (Aggiornamento Finale)
// Contiene la classe RegoleMIUManager per la gestione delle regole MIU e gli eventi correlati.
// Questo aggiornamento corregge la posizione di PathStepInfo nel namespace MIU.Core
// e qualifica correttamente tutti i riferimenti a RegolaMIU e RuleStatistics da EvolutiveSystem.Common.
// Data di riferimento: 20 giugno 2025 (Aggiornamento per scelta automatica BFS/DFS)
// Aggiunto il metodo TrovaDerivazioneAutomatica per la selezione intelligente dell'algoritmo.
// AGGIORNATO 20.06.2025: Estesa la classe PathStepInfo con proprietà per la persistenza e le statistiche di ricerca,
// e inizializzate correttamente in TrovaDerivazioneDFS e TrovaDerivazioneBFS.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using MasterLog; // Necessary for your Logger class
using EvolutiveSystem.Common;
using System.Threading; // Added for model classes (RegolaMIU, RuleStatistics, TransitionStatistics)
// DO NOT use 'using MIU.Core;' here because we are already in the MIU.Core namespace

namespace MIU.Core
{
    /// <summary>
    /// Data structure for a single step in the solution path.
    /// Contains the state string, the ID of the rule applied to reach it,
    /// and the parent state string.
    /// This class is in the MIU.Core namespace.
    /// </summary>
    public class PathStepInfo
    {
        public string StateStringStandard { get; set; } // The standard (decompressed) MIU string for this state
        public long? AppliedRuleID { get; set; } // The ID of the rule applied to reach this state (null for initial state)
        public string ParentStateStringStandard { get; set; } // The standard (decompressed) MIU string of the parent (null for initial state)

        /// <summary>
        /// Indica la posizione sequenziale (o indice) di questo stato all'interno del percorso di derivazione.
        /// Il primo passo del percorso avrà tipicamente un StepNumber di 0 o 1.
        /// </summary>
        public int StepNumber { get; set; }
        // NUOVE PROPRIETÀ AGGIUNTE PER PERSISTENZA E STATISTICHE
        /// <summary>
        /// L'ID dello stato corrente nel database MIU_States.
        /// </summary>
        public long StateID { get; set; }

        /// <summary>
        /// L'ID dello stato genitore nel database MIU_States. Null per il passo iniziale.
        /// </summary>
        public long? ParentStateID { get; set; }

        /// <summary>
        /// La profondità (numero di passi dal punto di partenza) di questo stato nella ricerca.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Il tempo trascorso (in millisecondi) dall'inizio della ricerca fino al raggiungimento di questo stato.
        /// Questo viene calcolato nel momento della creazione del PathStepInfo.
        /// </summary>
        public double ElapsedMilliseconds { get; set; }

        /// <summary>
        /// Il numero di nodi esplorati dall'algoritmo di ricerca fino al raggiungimento di questo stato.
        /// </summary>
        public int NodesExplored { get; set; }

        /// <summary>
        /// La massima profondità raggiunta dall'algoritmo di ricerca fino al raggiungimento di questo stato.
        /// </summary>
        public int MaxDepthReached { get; set; }
    }

    // EventArgs for the OnSolutionFound event
    public class SolutionFoundEventArgs : EventArgs
    {
        public long SearchID { get; set; } // NEW: Search ID
        public string InitialString { get; set; } // This will be the original COMPRESSED string
        public string TargetString { get; set; } // This will be the target COMPRESSED string
        public bool Success { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public long ElapsedTicks { get; set; }
        public List<PathStepInfo> SolutionPathSteps { get; set; } // list of PathStepInfo
        public int StepsTaken { get; set; } // Number of steps in the solution
        public int NodesExplored { get; set; } // Number of nodes explored during the search
        public int MaxDepthReached { get; set; } // Maximum depth reached
        public bool FromCache { get; set; } // Indicates if the solution was found in cache (added for completeness with other members)
        public string SearchAlgorithmUsed { get; set; } // New: Indicates which algorithm was used (BFS/DFS/Auto)
    }

    // EventArgs for the OnRuleApplied event
    public class RuleAppliedEventArgs : EventArgs
    {
        public long SearchID { get; set; } // Questa è la proprietà mancante!
        public long AppliedRuleID { get; set; }
        public string AppliedRuleName { get; set; }
        public string OriginalString { get; set; } // This will be the STANDARD string
        public string NewString { get; set; } // This will be the STANDARD string
        public int CurrentDepth { get; set; }
    }
    /// <summary>
    /// NUOVA CLASSE: BFSQueueItem
    /// Rappresenta un elemento nella coda di ricerca, includendo lo stato, il percorso e la sua priorità calcolata.
    /// Viene usata per implementare una "coda a priorità" nella BFS intelligente.  
    /// </summary>
    public class BFSQueueItem : IComparable<BFSQueueItem>
    {
        public string CurrentStandard { get; }
        public List<PathStepInfo> CurrentPath { get; }
        public double Priority { get; } // Priorità: valore più alto = priorità più alta

        public BFSQueueItem(string currentStandard, List<PathStepInfo> currentPath, double priority)
        {
            CurrentStandard = currentStandard;
            CurrentPath = currentPath;
            Priority = priority;
        }

        // Implementazione di IComparable per ordinare gli elementi nella coda a priorità.
        // Ordina in ordine decrescente di priorità (il più promettente prima).
        // Se le priorità sono uguali, favorisce percorsi più brevi.
        public int CompareTo(BFSQueueItem other)
        {
            if (other == null) return 1;

            // Confronta per priorità (decrescente: il più alto punteggio viene prima)
            int result = other.Priority.CompareTo(this.Priority);
            if (result == 0)
            {
                // Se le priorità sono uguali, usa la profondità del percorso (crescente: percorsi più brevi prima)
                result = this.CurrentPath.Count.CompareTo(other.CurrentPath.Count);
                if (result == 0)
                {
                    // Ulteriore tie-breaker: confronto lessicografico delle stringhe per garantire un ordine deterministico
                    result = string.Compare(this.CurrentStandard, other.CurrentStandard, StringComparison.Ordinal);
                }
            }
            return result;
        }
    }
    public static class RegoleMIUManager
    {
        // Static property for the logger instance
        public static Logger LoggerInstance { get; set; }

        // Static properties for configuration parameters (MaxDepth and MaxSteps)
        /// <summary>
        /// Maximum allowed depth for Depth-First Searches (DFS).
        /// Set by the orchestrator at startup.
        /// </summary>
        public static long MaxProfonditaRicerca { get; set; }

        /// <summary>
        /// Maximum number of steps/nodes to explore for Breadth-First Searches (BFS).
        /// Set by the orchestrator at startup.
        /// </summary>
        public static long MassimoPassiRicerca { get; set; }

        // NEW: Static property to access current RuleStatistics loaded by the orchestrator
        /// <summary>
        /// Reference to the current RuleStatistics dictionary, loaded from Program.cs.
        /// Used for sorting rules based on their effectiveness.
        /// Explicit qualification for RuleStatistics.
        /// </summary>
        public static System.Collections.Generic.Dictionary<long, EvolutiveSystem.Common.RuleStatistics> CurrentRuleStatistics { get; set; }
        /// <summary>
        /// 2025.06.20 15.27
        /// Reference to the current TransitionStatistics dictionary, loaded from Program.cs.
        /// Used for sorting rules based on specific transition effectiveness.
        /// </summary>
        public static System.Collections.Generic.Dictionary<Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics> CurrentTransitionStatistics { get; set; }
        // Static collection of all available MIU rules.
        // Explicit qualification for RegolaMIU
        public static System.Collections.Generic.List<EvolutiveSystem.Common.RegolaMIU> Regole { get; private set; } = new System.Collections.Generic.List<EvolutiveSystem.Common.RegolaMIU>();

        // Events to notify when a solution is found or a rule is applied.
        public static event EventHandler<SolutionFoundEventArgs> OnSolutionFound;
        public static event EventHandler<RuleAppliedEventArgs> OnRuleApplied;
        public static event EventHandler<NewMiuStringDiscoveredEventArgs> OnNewMiuStringDiscoveredInternal; // NEW EVENT: Per notificare la scoperta di nuove stringhe

        /// <summary>
        /// Loads MIU rules from a list of strings formatted as SQLiteSelect output.
        /// This method is designed to interface with the string format of SQLiteSchemaLoader.
        /// WARNING: This method assumes a specific string format and is not robust to changes.
        /// </summary>
        /// <param name="regoleRawData">List of strings, each string represents a data row delimited by ';'.</param>
        public static void CaricaRegoleDaOggettoSQLite(System.Collections.Generic.List<string> regoleRawData)
        {
            Regole.Clear(); // Clears existing rules before loading new ones

            foreach (string riga in regoleRawData)
            {
                string[] campi = riga.Split(';');
                if (campi.Length >= 5) // Ensure there are enough fields
                {
                    try
                    {
                        // Assume order: ID, Nome, Pattern, Sostituzione, Descrizione
                        long id = Convert.ToInt64(campi[0]);
                        string nome = campi[1].Trim();
                        string pattern = campi[2].Trim();
                        string sostituzione = campi[3].Trim();
                        string descrizione = campi[4].Trim();

                        // Explicit qualification for RegolaMIU
                        Regole.Add(new EvolutiveSystem.Common.RegolaMIU(id, nome, descrizione, pattern, sostituzione));
                    }
                    catch (Exception ex)
                    {
                        // Use LoggerInstance for error logging
                        LoggerInstance?.Log(LogLevel.ERROR, $"[RegoleMIUManager ERROR] Error parsing rule row: {riga}. Details: {ex.Message}");
                    }
                }
            }
            LoggerInstance?.Log(LogLevel.DEBUG, $"[RegoleMIUManager DEBUG] Loaded {Regole.Count} rules from SQLite object.");
        }

        /// <summary>
        /// Loads MIU rules from a list of RegolaMIU objects.
        /// This method is intended to be used with the output of MIURepository.LoadRegoleMIU().
        /// </summary>
        /// <param name="regoleMIU">List of RegolaMIU objects.</param>
        // Explicit qualification for RegolaMIU in the parameter
        public static void CaricaRegoleDaOggettoRepository(System.Collections.Generic.List<EvolutiveSystem.Common.RegolaMIU> regoleMIU)
        {
            Regole.Clear(); // Clears existing rules before loading new ones
            Regole.AddRange(regoleMIU); // Adds all rules from the provided list
            LoggerInstance?.Log(LogLevel.DEBUG, $"[RegoleMIUManager DEBUG] Loaded {Regole.Count} rules from Repository object.");
        }


        /// <summary>
        /// Applies MIU rules to a given string in a loop, showing all steps.
        /// This function operates on STANDARD (decompressed) strings.
        /// </summary>
        /// <param name="initialStringStandard">The initial standard (decompressed) string to apply rules to.</param>
        public static void ApplicaRegole(string initialStringStandard)
        {
            LoggerInstance?.Log(LogLevel.INFO, $"Initial string: {initialStringStandard}");
            string currentStringStandard = initialStringStandard;
            int step = 0;
            bool appliedAnyRule;

            do
            {
                appliedAnyRule = false;
                // MODIFICATION: Sort rules before applying them
                var orderedRules = Regole.OrderByDescending(rule =>
                {
                    // Explicit qualification for RuleStatistics
                    if (CurrentRuleStatistics != null && CurrentRuleStatistics.TryGetValue(rule.ID, out EvolutiveSystem.Common.RuleStatistics stats))
                    {
                        return stats.EffectivenessScore;
                    }
                    return 0.0; // Default score for rules without statistics
                })
                .ThenByDescending(rule =>
                {
                    // Explicit qualification for RuleStatistics
                    if (CurrentRuleStatistics != null && CurrentRuleStatistics.TryGetValue(rule.ID, out EvolutiveSystem.Common.RuleStatistics stats))
                    {
                        return stats.ApplicationCount;
                    }
                    return 0; // Default count for rules without statistics
                })
                .ToList();


                foreach (var rule in orderedRules) // Use sorted rules
                {
                    // TryApply operates on STANDARD strings
                    if (rule.TryApply(currentStringStandard, out string newStringStandard))
                    {
                        OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs
                        {
                            AppliedRuleID = rule.ID,
                            AppliedRuleName = rule.Nome,
                            OriginalString = currentStringStandard, // STANDARD string
                            NewString = newStringStandard,    // STANDARD string
                            CurrentDepth = step
                        });
                        currentStringStandard = newStringStandard;
                        appliedAnyRule = true;
                        step++;
                        break; // Apply only one rule per step for testing
                    }
                }
            } while (appliedAnyRule);

            LoggerInstance?.Log(LogLevel.INFO, $"Final result after {step} steps: {currentStringStandard}");
        }


        /// <summary>
        /// Implementation of Depth-First Search (DFS) to find a derivation.
        /// Operates on STANDARD strings internally, but accepts/returns COMPRESSED strings.
        /// Uses the static property MaxProfonditaRicerca for the depth limit.
        /// </summary>
        /// <param name="searchId">The ID of the current search for persistence.</param>
        public static List<PathStepInfo> TrovaDerivazioneDFS(long searchId, string startStringCompressed, string targetStringCompressed, IMIUDataManager dataManager) // MODIFIED SIGNATURE
        {
            // Decompress initial and target strings for internal search
            string startStringStandard = MIUStringConverter.InflateMIUString(startStringCompressed);
            string targetStringStandard = MIUStringConverter.InflateMIUString(targetStringCompressed);

            // Stack for DFS: (current standard state, list of PathStepInfo up to here)
            System.Collections.Generic.Stack<(string currentStandard, System.Collections.Generic.List<PathStepInfo> currentPath)> stack = new System.Collections.Generic.Stack<(string, System.Collections.Generic.List<PathStepInfo>)>();
            System.Collections.Generic.HashSet<string> visitedStandard = new System.Collections.Generic.HashSet<string>(); // To track already visited standard states
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Add initial state to path
            var initialPathStep = new PathStepInfo
            {
                StateStringStandard = startStringStandard,
                AppliedRuleID = null, // No rule applied for initial state
                ParentStateStringStandard = null, // No parent for initial state
                // Inizializzazione delle nuove proprietà per il passo iniziale
                StateID = -1, // Verrà aggiornato dal repository
                ParentStateID = null, // Verrà aggiornato dal repository
                Depth = 0,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                NodesExplored = 0,
                MaxDepthReached = 0
            };
            stack.Push((startStringStandard, new System.Collections.Generic.List<PathStepInfo> { initialPathStep }));
            visitedStandard.Add(startStringStandard);

            int nodesExplored = 0;
            int maxDepthReached = 0;

            LoggerInstance?.Log(LogLevel.INFO, $"[DFS] Starting search from '{startStringStandard}' to '{targetStringStandard}' (Max Depth: {MaxProfonditaRicerca})", true); // truncate = true
            
            while (stack.Count > 0)
            {
                nodesExplored++;
                var (currentStandard, currentPath) = stack.Pop();
                maxDepthReached = Math.Max(maxDepthReached, currentPath.Count - 1); // Depth is path length - 1

                if (currentStandard == targetStringStandard)
                {
                    stopwatch.Stop();
                    // The path is already in PathStepInfo with standard strings
                    OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs
                    {
                        SearchID = searchId, // Pass the search ID
                        InitialString = startStringCompressed, // Original COMPRESSED string
                        TargetString = targetStringCompressed, // Target COMPRESSED string
                        Success = true,
                        ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                        FromCache = false, // Always consider false for now
                        ElapsedTicks = stopwatch.ElapsedTicks,
                        SolutionPathSteps = currentPath, // Complete path ready
                        StepsTaken = currentPath.Count - 1,
                        NodesExplored = nodesExplored,
                        MaxDepthReached = maxDepthReached,
                        SearchAlgorithmUsed = "DFS" // Specify the algorithm used
                    });
                    LoggerInstance?.Log(LogLevel.INFO, $"[DFS] Solution found: '{startStringCompressed}' -> '{targetStringCompressed}'. Steps: {currentPath.Count - 1}, Nodes explored: {nodesExplored}. Time: {stopwatch.ElapsedMilliseconds} ms.", true);
                    return currentPath; // Returns path in PathStepInfo
                }

                if (currentPath.Count - 1 >= MaxProfonditaRicerca)
                {
                    LoggerInstance?.Log(LogLevel.INFO, $"[DFS] Maximum depth reached ({MaxProfonditaRicerca}) for '{currentStandard}'. Pruning this branch.", true);
                    continue; // Maximum depth reached
                }
                // MODIFICA CRUCIALE: Ordina le regole prima di applicarle,
                // dando priorità alle statistiche di transizione specifiche,
                // poi alle statistiche generali della regola.
                var orderedRules = Regole.OrderByDescending(rule =>
                {
                    double score = 0.0; // Inizializza il punteggio per questa regola

                    // 1. Priorità massima: SuccessRate della transizione specifica (currentStandard -> rule)
                    if (CurrentTransitionStatistics != null)
                    {
                        // La chiave per le TransitionStatistics richiede la stringa genitore COMPRESSA
                        var transitionKey = Tuple.Create(MIUStringConverter.DeflateMIUString(currentStandard), rule.ID);
                        if (CurrentTransitionStatistics.TryGetValue(transitionKey, out EvolutiveSystem.Common.TransitionStatistics transitionStats))
                        {
                            // Assegna un punteggio elevato basato su SuccessRate e ApplicationCount della transizione.
                            // Moltiplichiamo il SuccessRate per un fattore alto (es. 1000) per assicurare che abbia la precedenza.
                            // Aggiungiamo ApplicationCount per dare preferenza a statistiche più "solide".
                            score = transitionStats.SuccessRate * 1000.0 + transitionStats.ApplicationCount;
                            // LoggerInstance?.Log(LogLevel.DEBUG, $"[DFS-Sort] Rule {rule.ID} ({rule.Nome}) from '{MIUStringConverter.DeflateMIUString(currentStandard)}': Transition Score = {score:F4}");
                        }
                    }

                    // 2. Seconda priorità: EffectivenessScore della regola generale (se la transizione specifica non ha dato un punteggio elevato)
                    // Questo viene considerato solo se la score della transizione specifica è ancora 0 o non presente
                    if (score == 0.0 && CurrentRuleStatistics != null && CurrentRuleStatistics.TryGetValue(rule.ID, out EvolutiveSystem.Common.RuleStatistics ruleStats))
                    {
                        score = ruleStats.EffectivenessScore;
                        // LoggerInstance?.Log(LogLevel.DEBUG, $"[DFS-Sort] Rule {rule.ID} ({rule.Nome}): General Rule Score = {score:F4}");
                    }

                    return score; // Restituisce il punteggio calcolato
                })
                .ThenByDescending(rule =>
                {
                    // Come terzo criterio di ordinamento, in caso di parità nei punteggi precedenti,
                    // usiamo il conteggio delle applicazioni della regola generale.
                    // Questo aiuta a rompere i legami in modo deterministico e favorire regole più usate.
                    if (CurrentRuleStatistics != null && CurrentRuleStatistics.TryGetValue(rule.ID, out EvolutiveSystem.Common.RuleStatistics ruleStats))
                    {
                        return ruleStats.ApplicationCount;
                    }
                    return 0; // Valore predefinito se le statistiche non sono disponibili
                })
                .ToList();

                foreach (var rule in orderedRules) // Use sorted rules
                {
                    // TryApply operates on STANDARD strings
                    if (rule.TryApply(currentStandard, out string newStringStandard))
                    {
                        OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs
                        {
                            SearchID = searchId, // <<-- QUESTA RIGA È DA AGGIUNGERE/CORREGGERE QUI!
                            AppliedRuleID = rule.ID,
                            AppliedRuleName = rule.Nome,
                            OriginalString = currentStandard, // STANDARD string
                            NewString = newStringStandard,    // STANDARD string
                            CurrentDepth = currentPath.Count - 1 // Current depth
                        });

                        // NEW: Tentativo di inserire/aggiornare la stringa nel database e ottenere il flag isNewToDatabase
                        Tuple<long, bool> upsertResult = dataManager.UpsertMIUState(MIUStringConverter.DeflateMIUString(newStringStandard));
                        long newStateId = upsertResult.Item1;
                        bool isNewToDatabase = upsertResult.Item2;

                        if (!visitedStandard.Contains(newStringStandard))
                        {
                            visitedStandard.Add(newStringStandard);
                            OnNewMiuStringDiscoveredInternal?.Invoke(null, new NewMiuStringDiscoveredEventArgs
                            {
                                SearchID = searchId,
                                DiscoveredString = newStringStandard, // La stringa standard scoperta
                                IsTrulyNewToDatabase = isNewToDatabase, // <- errore cs103 Indica se è anche nuova per il DB
                                StateID = newStateId // <- errore cs103  L'ID assegnato dal database
                            });
                            // Create a new step for the path
                            var newPathStep = new PathStepInfo
                            {
                                StateStringStandard = newStringStandard,
                                AppliedRuleID = rule.ID,
                                ParentStateStringStandard = currentStandard,
                                // Inizializzazione delle nuove proprietà per ogni nuovo passo
                                StateID = -1, // Verrà aggiornato dal repository
                                ParentStateID = (currentPath.LastOrDefault()?.StateID), // Inizializza con l'ID dello stato genitore se PathStepInfo ha ElapsedMilliseconds
                                Depth = currentPath.Count, // La profondità è la dimensione del percorso (0-indexed)
                                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                                NodesExplored = nodesExplored,
                                MaxDepthReached = maxDepthReached
                            };
                            System.Collections.Generic.List<PathStepInfo> newPath = new System.Collections.Generic.List<PathStepInfo>(currentPath) { newPathStep };
                            stack.Push((newStringStandard, newPath));
                            LoggerInstance?.Log(LogLevel.DEBUG, $"[DFS] Added new state: '{newStringStandard}' (from '{currentStandard}' with rule '{(rule.Nome)}'). Depth: {currentPath.Count}. Stack: {stack.Count}", true);
                        }
                        else
                        {
                            LoggerInstance?.Log(LogLevel.DEBUG, $"[DFS] State '{newStringStandard}' already visited. Skipping.", true);
                        }
                    }
                }
            }

            stopwatch.Stop();
            OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs
            {
                SearchID = searchId, // Pass the search ID
                InitialString = startStringCompressed, // Original COMPRESSED string
                TargetString = targetStringCompressed, // Target COMPRESSED string
                Success = false,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ElapsedTicks = stopwatch.ElapsedTicks,
                FromCache = false, // Always consider false for now
                SolutionPathSteps = null, // Null path if not found
                StepsTaken = -1,
                NodesExplored = nodesExplored,
                MaxDepthReached = maxDepthReached,
                SearchAlgorithmUsed = "DFS" // Specify the algorithm used
            });
            LoggerInstance?.Log(LogLevel.INFO, $"[DFS] No solution found: '{startStringStandard}' -> '{targetStringCompressed}'. Nodes explored: {nodesExplored}, Max Depth: {maxDepthReached}. Time: {stopwatch.ElapsedMilliseconds} ms.", true);
            return null; // No derivation found
        }


        /// <summary>
        /// Implementation of Breadth-First Search (BFS) to find the shortest derivation,
        /// enhanced with heuristic pruning and priority queue based on learning statistics.
        /// Operates on STANDARD strings internally.
        /// </summary>
        /// <param name="searchId">The ID of the current search for persistence.</param>
        /// <param name="startStringCompressed">The compressed starting string.</param>
        /// <param name="targetStringCompressed">The compressed target string.</param>
        /// <param name="cancellationToken">A cancellation token for early termination.</param>
        /// <returns>The list of PathStepInfo that constitutes the solution, or null if not found.</returns>
        public static List<PathStepInfo> TrovaDerivazioneBFS(long searchId, string startStringCompressed, string targetStringCompressed, CancellationToken cancellationToken, IMIUDataManager dataManager) // MODIFIED SIGNATURE
        {
            string startStringStandard = MIUStringConverter.InflateMIUString(startStringCompressed);
            string targetStringStandard = MIUStringConverter.InflateMIUString(targetStringCompressed);

            // *** MODIFICA A: Sostituisci Queue con List<BFSQueueItem> per simulare una coda a priorità ***
            // Questa lista conterrà gli elementi da esplorare, ordinati per priorità.
            List<BFSQueueItem> priorityQueue = new List<BFSQueueItem>();
            // ********************************************************************************************

            HashSet<string> visitedStandard = new HashSet<string>(); // Per tracciare gli stati già visitati
                                                                     // La dictionary `paths` non è più strettamente necessaria qui come per il BFS puro,
                                                                     // poiché ogni BFSQueueItem contiene già il suo `currentPath`.
                                                                     // Tuttavia, se si volesse tenere traccia del miglior percorso per uno stato già visitato
                                                                     // in un A* completo, si potrebbe volerla estendere per PathStepInfo e priorità.
                                                                     // Per ora, manterremo un comportamento simile al tuo BFS originale (non riesplorare stati visitati).

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Add initial state to path
            var initialPathStep = new PathStepInfo
            {
                StateStringStandard = startStringStandard,
                AppliedRuleID = null, // No rule applied for initial state
                ParentStateStringStandard = null, // No parent for initial state
                StateID = dataManager.UpsertMIUState(startStringCompressed).Item1, // NEW: Ottieni l'ID dal DB per la stringa iniziale
                ParentStateID = null, // Verrà aggiornato dal repository
                Depth = 0,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                NodesExplored = 0, // Questi campi verranno aggiornati in MIUDerivationEngine.HandleRuleApplied
                MaxDepthReached = 0  // Questi campi verranno aggiornati in MIUDerivationEngine.HandleRuleApplied
            };

            // *** MODIFICA B: Calcola la priorità iniziale e aggiungi a priorityQueue ***
            // Per lo stato iniziale, non c'è una regola applicata o un genitore, quindi passiamo null/-1.
            double initialPriority = CalculatePriority(null, -1, 0); // La funzione CalculatePriority verrà aggiunta al passo 3
            priorityQueue.Add(new BFSQueueItem(startStringStandard, new List<PathStepInfo> { initialPathStep }, initialPriority));
            visitedStandard.Add(startStringStandard);
            // *****************************************************************************

            int nodesExplored = 0;
            int maxDepthReached = 0;

            LoggerInstance?.Log(LogLevel.INFO, $"[BFS-Intelligent] Starting search from '{startStringStandard}' to '{targetStringStandard}' (Max steps: {MassimoPassiRicerca}, Max Depth: {MaxProfonditaRicerca})", true);

            while (priorityQueue.Count > 0) // Il ciclo ora opera sulla nostra lista a priorità
            {
                // Controlla la richiesta di cancellazione
                cancellationToken.ThrowIfCancellationRequested();

                // *** MODIFICA C: Estrai l'elemento con la priorità più alta dalla lista ***
                // Questo ordina la lista ad ogni iterazione per simulare il comportamento di una coda a priorità.
                // In un'implementazione reale ad alte prestazioni, useresti una vera struttura dati PriorityQueue ottimizzata.
                // Ordiniamo per priorità decrescente, poi per profondità crescente (percorsi più brevi a parità di priorità).
                BFSQueueItem bestItem = priorityQueue.OrderByDescending(item => item.Priority).ThenBy(item => item.CurrentPath.Count).FirstOrDefault();
                if (bestItem == null) break; // Non dovrebbe accadere se Count > 0

                priorityQueue.Remove(bestItem); // Rimuovi l'elemento estratto dalla lista

                string currentStandard = bestItem.CurrentStandard;
                List<PathStepInfo> currentPath = bestItem.CurrentPath;
                // ***************************************************************

                nodesExplored++; // Conta i nodi effettivamente esplorati (estratti dalla coda)
                maxDepthReached = Math.Max(maxDepthReached, currentPath.Count - 1); // Aggiorna la profondità massima raggiunta

                if (currentStandard == targetStringStandard)
                {
                    stopwatch.Stop();
                    // Soluzione trovata
                    OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs
                    {
                        SearchID = searchId,
                        InitialString = startStringCompressed,
                        TargetString = targetStringCompressed,
                        Success = true,
                        ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                        ElapsedTicks = stopwatch.ElapsedTicks,
                        FromCache = false,
                        SolutionPathSteps = currentPath,
                        StepsTaken = currentPath.Count - 1,
                        NodesExplored = nodesExplored,
                        MaxDepthReached = maxDepthReached,
                        SearchAlgorithmUsed = "BFS-Intelligent" // Aggiorna il nome dell'algoritmo
                    });
                    LoggerInstance?.Log(LogLevel.INFO, $"[BFS-Intelligent] Solution found: '{startStringStandard}' -> '{targetStringCompressed}'. Steps: {currentPath.Count - 1}, Nodes explored: {nodesExplored}, Max Depth: {maxDepthReached}. Time: {stopwatch.ElapsedMilliseconds} ms.", true);
                    return currentPath;
                }

                // Limite di passi di esplorazione raggiunto (ora `nodesExplored` si riferisce ai nodi *estratti* dalla coda)
                if (nodesExplored >= MassimoPassiRicerca)
                {
                    LoggerInstance?.Log(LogLevel.INFO, $"[BFS-Intelligent] Maximum exploration steps reached ({MassimoPassiRicerca}). Terminating search.");
                    break; // Esci dal loop
                }

                // Limite di profondità raggiunto
                if (currentPath.Count - 1 >= MaxProfonditaRicerca)
                {
                    LoggerInstance?.Log(LogLevel.INFO, $"[BFS-Intelligent] Maximum depth reached ({MaxProfonditaRicerca}) for '{currentStandard}'. Pruning this branch.", true);
                    continue; // Salta l'esplorazione di questo ramo troppo profondo
                }

                // La tua logica precedente di "orderedRules" qui non è più necessaria nello stesso modo,
                // perché l'ordinamento degli stati da esplorare è ora gestito dalla `priorityQueue`
                // basata sull'euristica CalculatePriority.
                // Qui semplicemente iteriamo sulle regole disponibili. Se vuoi un ordinamento secondario
                // delle regole da applicare *da un singolo stato*, potresti mantenerlo, ma la priorità
                // complessiva del grafo sarà decisa dalla coda. Per ora, iteriamo sulle regole nell'ordine predefinito.
                foreach (var rule in Regole)
                {
                    // Tenta di applicare la regola
                    if (rule.TryApply(currentStandard, out string newStringStandard))
                    {
                        // Invoca l'evento OnRuleApplied per la persistenza e l'aggiornamento delle statistiche
                        OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs
                        {
                            SearchID = searchId,
                            AppliedRuleID = rule.ID,
                            AppliedRuleName = rule.Nome,
                            OriginalString = currentStandard,
                            NewString = newStringStandard,
                            CurrentDepth = currentPath.Count - 1 // Profondità della stringa originale
                        });

                        // NEW: Tentativo di inserire/aggiornare la stringa nel database e ottenere il flag isNewToDatabase
                        Tuple<long, bool> upsertResult = dataManager.UpsertMIUState(MIUStringConverter.DeflateMIUString(newStringStandard));
                        long newStateId = upsertResult.Item1;
                        bool isNewToDatabase = upsertResult.Item2;

                        // Se la stringa è già stata visitata, salta.
                        // In un A* completo, qui si dovrebbe verificare se il nuovo percorso per lo stato visitato
                        // è migliore (ha un costo cumulativo inferiore) rispetto a quello già trovato,
                        // e aggiornare il percorso se necessario. Per ora, manteniamo la logica di non riesplorazione.
                        if (!visitedStandard.Contains(newStringStandard))
                        {
                            visitedStandard.Add(newStringStandard);
                            // NEW: Scatena l'evento per contare le "nuove stringhe scoperte"
                            OnNewMiuStringDiscoveredInternal?.Invoke(null, new NewMiuStringDiscoveredEventArgs
                            {
                                SearchID = searchId,
                                DiscoveredString = newStringStandard, // La stringa standard scoperta
                                IsTrulyNewToDatabase = isNewToDatabase, // Indica se è anche nuova per il DB
                                StateID = newStateId // L'ID assegnato dal database
                            });
                            // Crea un nuovo passo per il percorso
                            var newPathStep = new PathStepInfo
                            {
                                StateStringStandard = newStringStandard,
                                AppliedRuleID = rule.ID,
                                ParentStateStringStandard = currentStandard,
                                StateID = dataManager.UpsertMIUState(startStringCompressed).Item1, // NEW: Ottieni l'ID dal DB per la stringa iniziale
                                ParentStateID = (currentPath.LastOrDefault()?.StateID), // Inizializza con l'ID dello stato genitore
                                Depth = currentPath.Count, // La profondità è la dimensione del percorso (0-indexed)
                                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                                NodesExplored = nodesExplored, // Questo conteggio è per l'evento, non per il passo singolo
                                MaxDepthReached = maxDepthReached // Questo conteggio è per l'evento, non per il passo singolo
                            };

                            List<PathStepInfo> newPath = new List<PathStepInfo>(currentPath) { newPathStep };

                            // *** MODIFICA D: Calcola la priorità per il nuovo stato e aggiungilo alla coda a priorità ***
                            double newPriority = CalculatePriority(currentStandard, rule.ID, newPathStep.Depth); // La funzione CalculatePriority verrà aggiunta al passo 3
                            priorityQueue.Add(new BFSQueueItem(newStringStandard, newPath, newPriority));
                            // **********************************************************************************************

                            LoggerInstance?.Log(LogLevel.INFO, $"[BFS-Intelligent] Added new state: '{newStringStandard}' (from '{currentStandard}' with rule '{rule.Nome}'). Depth: {newPathStep.Depth}. Priority: {newPriority:F4}. Queue Size: {priorityQueue.Count}", true);
                        }
                        else
                        {
                            LoggerInstance?.Log(LogLevel.INFO, $"[BFS-Intelligent] State '{newStringStandard}' already visited. Skipping.", true);
                        }
                    }
                }
            }

            stopwatch.Stop();
            OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs
            {
                SearchID = searchId,
                InitialString = startStringCompressed,
                TargetString = targetStringCompressed,
                Success = false,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ElapsedTicks = stopwatch.ElapsedTicks,
                FromCache = false,
                SolutionPathSteps = null,
                StepsTaken = -1,
                NodesExplored = nodesExplored,
                MaxDepthReached = maxDepthReached,
                SearchAlgorithmUsed = "BFS-Intelligent"
            });
            LoggerInstance?.Log(LogLevel.INFO, $"[BFS-Intelligent] No solution found: '{startStringStandard}' -> '{targetStringCompressed}'. Nodes explored: {nodesExplored}, Max Depth: {maxDepthReached}. Time: {stopwatch.ElapsedMilliseconds} ms.");
            return null;
        }
        /// <summary>
        /// Calcola la priorità di un nuovo stato da esplorare.
        /// Utilizza le statistiche di apprendimento per informare l'euristica.
        /// Un punteggio più alto indica una priorità più alta (il nodo dovrebbe essere esplorato prima).
        /// </summary>
        /// <param name="parentStandardString">La stringa genitore da cui è stato derivato il nuovo stato (per le statistiche di transizione). Può essere null per lo stato iniziale.</param>
        /// <param name="appliedRuleId">L'ID della regola applicata per derivare il nuovo stato. Può essere -1 per lo stato iniziale.</param>
        /// <param name="newDepth">La profondità del nuovo stato nel grafo di esplorazione.</param>
        /// <returns>Il punteggio di priorità.</returns>
        private static double CalculatePriority(string parentStandardString, long appliedRuleId, int newDepth)
        {
            // Base priority: Favorisce nodi meno profondi
            // Più è profondo il nodo, minore è la sua priorità. Iniziamo con un valore base alto per priorità e sottraiamo la profondità.
            // Questo emula in parte il comportamento di BFS (preferire percorsi brevi), ma lo modifichiamo con le euristiche.
            double priority = 1000.0; // Punteggio base arbitrario
            priority -= newDepth * 10.0; // Penalità per la profondità (può essere calibrata)

            double transitionSuccessRate = 0.0;
            long transitionApplicationCount = 0;
            double ruleEffectivenessScore = 0.0;
            long ruleApplicationCount = 0;

            // Tentativo di ottenere statistiche di transizione specifiche
            // Convertiamo la stringa standard del genitore in compressa per la chiave delle statistiche
            if (parentStandardString != null && appliedRuleId != -1 && CurrentTransitionStatistics != null)
            {
                var transitionKey = Tuple.Create(MIUStringConverter.DeflateMIUString(parentStandardString), appliedRuleId);
                if (CurrentTransitionStatistics.TryGetValue(transitionKey, out EvolutiveSystem.Common.TransitionStatistics transitionStats))
                {
                    transitionSuccessRate = transitionStats.SuccessRate;
                    transitionApplicationCount = transitionStats.ApplicationCount;
                }
            }

            // Tentativo di ottenere statistiche della regola generale
            if (appliedRuleId != -1 && CurrentRuleStatistics != null && CurrentRuleStatistics.TryGetValue(appliedRuleId, out EvolutiveSystem.Common.RuleStatistics ruleStats))
            {
                ruleEffectivenessScore = ruleStats.EffectivenessScore;
                ruleApplicationCount = ruleStats.ApplicationCount;
            }

            // Pesi Euristici (possono essere ottimizzati tramite machine learning in futuro)
            const double WEIGHT_TRANSITION_SUCCESS = 500.0; // Forte peso per il successo della transizione specifica
            const double WEIGHT_RULE_EFFECTIVENESS = 50.0;  // Moderato per l'efficacia generale della regola
            const double WEIGHT_APPLICATION_COUNT_TRANSITION = 0.5; // Piccolo peso per applicazioni della transizione (dati più robusti)
            const double WEIGHT_APPLICATION_COUNT_RULE = 0.05;    // Ancora più piccolo per applicazioni della regola generale
            const double EXPLORATION_BONUS = 0.001;        // Piccolo bonus per percorsi nuovi/meno esplorati (esplorazione vs. sfruttamento)

            // Euristica principale: Basata sulla SuccessRate della transizione e della regola
            if (transitionApplicationCount > 0) // Se abbiamo statistiche sulla transizione specifica (è stata provata almeno una volta)
            {
                priority += transitionSuccessRate * WEIGHT_TRANSITION_SUCCESS;
                priority += transitionApplicationCount * WEIGHT_APPLICATION_COUNT_TRANSITION;
                // LoggerInstance?.Log(LogLevel.DEBUG, $"[CalculatePriority] State at depth {newDepth} from '{parentStandardString}' via Rule {appliedRuleId}: Transition Score Part = {transitionSuccessRate * WEIGHT_TRANSITION_SUCCESS + transitionApplicationCount * WEIGHT_APPLICATION_COUNT_TRANSITION:F4}");
            }
            else if (ruleApplicationCount > 0) // Altrimenti, se ci sono statistiche sulla regola generale
            {
                priority += ruleEffectivenessScore * WEIGHT_RULE_EFFECTIVENESS;
                priority += ruleApplicationCount * WEIGHT_APPLICATION_COUNT_RULE;
                // LoggerInstance?.Log(LogLevel.DEBUG, $"[CalculatePriority] State at depth {newDepth} from '{parentStandardString}' via Rule {appliedRuleId}: General Rule Score Part = {ruleEffectivenessScore * WEIGHT_RULE_EFFECTIVENESS + ruleApplicationCount * WEIGHT_APPLICATION_COUNT_RULE:F4}");
            }
            else // Per transizioni/regole completamente nuove (non viste o con successo nullo)
            {
                // Diamo un piccolo bonus di esplorazione per assicurarci che vengano comunque tentate,
                // prevenendo che la ricerca si blocchi solo su percorsi "noti".
                priority += EXPLORATION_BONUS;
                // LoggerInstance?.Log(LogLevel.DEBUG, $"[CalculatePriority] State at depth {newDepth} from '{parentStandardString}' via Rule {appliedRuleId}: Exploration Bonus Part = {EXPLORATION_BONUS:F4}");
            }

            // --- PENALITÀ CRUCIALE PER LA LUNGHEZZA DELLA STRINGA (LA "NATURA ABBORRISCE L'INFINITO") ---
            // Questa è la potatura più diretta per controllare la crescita delle stringhe e la loro complessità.
            const int STRING_LENGTH_PENALTY_THRESHOLD = 20; // Soglia arbitraria: oltre 20 caratteri, inizia la penalità. Puoi calibrare.
            const double STRING_LENGTH_PENALTY_FACTOR = 5.0; // Quanto penalizzare per ogni carattere oltre la soglia. Valore più alto = penalità più aggressiva.

            if (parentStandardString != null) // Non applicare al nodo iniziale
            {
                // Usiamo la lunghezza della stringa STANDARD, non compressa, perché è quella che conta per la complessità.
                int currentStringLength = parentStandardString.Length;
                if (currentStringLength > STRING_LENGTH_PENALTY_THRESHOLD)
                {
                    priority -= (currentStringLength - STRING_LENGTH_PENALTY_THRESHOLD) * STRING_LENGTH_PENALTY_FACTOR;
                    LoggerInstance?.Log(LogLevel.INFO, $"[CalculatePriority] Penalized long string (len: {currentStringLength}) at depth {newDepth} with penalty: {(currentStringLength - STRING_LENGTH_PENALTY_THRESHOLD) * STRING_LENGTH_PENALTY_FACTOR:F2}");
                }
            }

            // Ulteriore penalità potenziale per un numero eccessivo di 'I' o 'U' se generano cicli o ramificazioni inutili.
            // Puoi aggiungere qui logiche specifiche per le tue regole MIU.
            // Esempio (attualmente commentato):
            // const int I_COUNT_PENALTY_THRESHOLD = 15;
            // if (parentStandardString != null && MIUStringConverter.CountChar(parentStandardString, 'I') > I_COUNT_PENALTY_THRESHOLD)
            // {
            //     priority -= (MIUStringConverter.CountChar(parentStandardString, 'I') - I_COUNT_PENALTY_THRESHOLD) * 10.0;
            // }


            LoggerInstance?.Log(LogLevel.INFO, $"[CalculatePriority] Calculated priority for state at depth {newDepth} from '{parentStandardString}' via Rule {appliedRuleId}: Total Priority = {priority:F4}");

            return priority;
        }
        /// <summary>
        /// Intelligent method to choose and start the derivation search (BFS or DFS)
        /// based on string length heuristics.
        /// </summary>
        /// <param name="searchId">The ID of the current search for persistence.</param>
        /// <param name="startStringCompressed">The compressed starting string.</param>
        /// <param name="targetStringCompressed">The compressed target string.</param>
        /// <param name="cancellationToken"> Un token di cancellazione che può essere usato per richiedere l'interruzione anticipata della ricerca.</param>
        /// <returns>The list of PathStepInfo that constitutes the solution, or null if not found.</returns>
        public static List<PathStepInfo> TrovaDerivazioneAutomatica(long searchId, string startStringCompressed, string targetStringCompressed, CancellationToken cancellationToken, IMIUDataManager dataManager) // MODIFIED: Usa IMIUDataManager - MODIFIED SIGNATURE: Aggiunto dataManager
        {
            LoggerInstance?.Log(LogLevel.INFO, $"[AutoSearch] Automatic search requested from '{startStringCompressed}' to '{targetStringCompressed}'.");

            string startStringStandard = MIUStringConverter.InflateMIUString(startStringCompressed);
            string targetStringStandard = MIUStringConverter.InflateMIUString(targetStringCompressed);

            // Heuristic: If the target string is significantly longer than the initial string (e.g., > 1.5 times length), prefer DFS.
            // Otherwise, prefer BFS for the shortest path.
            bool useDFS = targetStringStandard.Length > (startStringStandard.Length * 1.5);

            List<PathStepInfo> resultPath = null;
            string chosenAlgorithm = "";

            if (useDFS)
            {
                chosenAlgorithm = "DFS (Automatic)";
                LoggerInstance?.Log(LogLevel.INFO, $"[AutoSearch] Target string is longer. Chosen algorithm DFS.");
                resultPath = TrovaDerivazioneDFS(searchId, startStringCompressed, targetStringCompressed, dataManager); // <- error cs1501  NEW: Passa dataManager
            }
            else
            {
                chosenAlgorithm = "BFS (Automatic)";
                LoggerInstance?.Log(LogLevel.INFO, $"[AutoSearch] Target string is not significantly longer. Chosen algorithm BFS.");
                resultPath = TrovaDerivazioneBFS(searchId, startStringCompressed, targetStringCompressed, cancellationToken, dataManager); // <- error cs1501  NEW: Passa dataManager
            }

            // The OnSolutionFound event is already invoked by the BFS/DFS methods,
            // so we do not invoke it here to avoid duplicates.
            // However, we can add a log to summarize the choice.

            return resultPath;
        }

    }
}
