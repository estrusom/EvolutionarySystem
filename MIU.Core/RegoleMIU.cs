// File: C:\Progetti\EvolutiveSystem\MIU.Core\RegoleMIU.cs
// Data di riferimento: 20 giugno 2025 (Aggiornamento Finale)
// Contiene la classe RegoleMIUManager per la gestione delle regole MIU e gli eventi correlati.
// Questo aggiornamento corregge la posizione di PathStepInfo nel namespace MIU.Core
// e qualifica correttamente tutti i riferimenti a RegolaMIU e RuleStatistics da EvolutiveSystem.Common.
// Data di riferimento: 20 giugno 2025 (Aggiornamento per scelta automatica BFS/DFS)
// Aggiunto il metodo TrovaDerivazioneAutomatica per la selezione intelligente dell'algoritmo.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using MasterLog; // Necessary for your Logger class
using EvolutiveSystem.Common; // Added for model classes (RegolaMIU, RuleStatistics, TransitionStatistics)
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
        public long AppliedRuleID { get; set; }
        public string AppliedRuleName { get; set; }
        public string OriginalString { get; set; } // This will be the STANDARD string
        public string NewString { get; set; } // This will be the STANDARD string
        public int CurrentDepth { get; set; }
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


        // Static collection of all available MIU rules.
        // Explicit qualification for RegolaMIU
        public static System.Collections.Generic.List<EvolutiveSystem.Common.RegolaMIU> Regole { get; private set; } = new System.Collections.Generic.List<EvolutiveSystem.Common.RegolaMIU>();

        // Events to notify when a solution is found or a rule is applied.
        public static event EventHandler<SolutionFoundEventArgs> OnSolutionFound;
        public static event EventHandler<RuleAppliedEventArgs> OnRuleApplied;

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
                            NewString = newStringStandard,
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
        public static List<PathStepInfo> TrovaDerivazioneDFS(long searchId, string startStringCompressed, string targetStringCompressed)
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
                ParentStateStringStandard = null // No parent for initial state
            };
            stack.Push((startStringStandard, new System.Collections.Generic.List<PathStepInfo> { initialPathStep }));
            visitedStandard.Add(startStringStandard);

            int nodesExplored = 0;
            int maxDepthReached = 0;

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
                    LoggerInstance?.Log(LogLevel.INFO, $"[DFS] Solution found: '{startStringCompressed}' -> '{targetStringCompressed}'. Steps: {currentPath.Count - 1}, Nodes explored: {nodesExplored}. Time: {stopwatch.ElapsedMilliseconds} ms.");
                    return currentPath; // Returns path in PathStepInfo
                }

                if (currentPath.Count - 1 >= MaxProfonditaRicerca) continue; // Maximum depth reached

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
                    if (rule.TryApply(currentStandard, out string newStringStandard))
                    {
                        OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs
                        {
                            AppliedRuleID = rule.ID,
                            AppliedRuleName = rule.Nome,
                            OriginalString = currentStandard, // STANDARD string
                            NewString = newStringStandard,    // STANDARD string
                            CurrentDepth = currentPath.Count - 1 // Current depth
                        });

                        if (!visitedStandard.Contains(newStringStandard))
                        {
                            visitedStandard.Add(newStringStandard);
                            // Create a new step for the path
                            var newPathStep = new PathStepInfo
                            {
                                StateStringStandard = newStringStandard,
                                AppliedRuleID = rule.ID,
                                ParentStateStringStandard = currentStandard
                            };
                            System.Collections.Generic.List<PathStepInfo> newPath = new System.Collections.Generic.List<PathStepInfo>(currentPath) { newPathStep };
                            stack.Push((newStringStandard, newPath));
                            LoggerInstance?.Log(LogLevel.DEBUG, $"[DFS] Added new state: '{newStringStandard}' (from '{currentStandard}' with rule '{(rule.Nome)}'). Depth: {currentPath.Count}. Queue: {stack.Count}");
                        }
                        else
                        {
                            // LoggerInstance?.Log(LogLevel.DEBUG, $"[DFS] State '{newStringStandard}' already visited. Skipping.");
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
            LoggerInstance?.Log(LogLevel.INFO, $"[DFS] No solution found: '{startStringCompressed}' -> '{targetStringCompressed}'. Nodes explored: {nodesExplored}, Max Depth: {maxDepthReached}. Time: {stopwatch.ElapsedMilliseconds} ms.");
            return null; // No derivation found
        }


        /// <summary>
        /// Implementation of Breadth-First Search (BFS) to find the shortest derivation.
        /// Operates on STANDARD strings internally, but accepts/returns a list of PathStepInfo.
        /// Uses the static property MassimoPassiRicerca for the step limit.
        /// </summary>
        /// <param name="searchId">The ID of the current search for persistence.</param>
        public static List<PathStepInfo> TrovaDerivazioneBFS(long searchId, string startStringCompressed, string targetStringCompressed)
        {
            // Decompress initial and target strings for internal search
            string startStringStandard = MIUStringConverter.InflateMIUString(startStringCompressed);
            string targetStringStandard = MIUStringConverter.InflateMIUString(targetStringCompressed);

            // Queue for BFS: (current standard state, list of PathStepInfo up to here)
            System.Collections.Generic.Queue<(string currentStandard, System.Collections.Generic.List<PathStepInfo> currentPath)> queue = new System.Collections.Generic.Queue<(string, System.Collections.Generic.List<PathStepInfo>)>();
            System.Collections.Generic.HashSet<string> visitedStandard = new System.Collections.Generic.HashSet<string>(); // To track already visited standard states
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Add initial state to path
            var initialPathStep = new PathStepInfo
            {
                StateStringStandard = startStringStandard,
                AppliedRuleID = null, // No rule applied for initial state
                ParentStateStringStandard = null // No parent for initial state
            };
            queue.Enqueue((startStringStandard, new System.Collections.Generic.List<PathStepInfo> { initialPathStep }));
            visitedStandard.Add(startStringStandard);

            int nodesExplored = 0;
            int maxDepthReached = 0;

            LoggerInstance?.Log(LogLevel.DEBUG, $"[BFS] Starting search from '{startStringStandard}' to '{targetStringStandard}' (Max steps: {MassimoPassiRicerca})");

            while (queue.Count > 0)
            {
                nodesExplored++;
                var (currentStandard, currentPath) = queue.Dequeue();
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
                        ElapsedTicks = stopwatch.ElapsedTicks,
                        FromCache = false, // Always consider false for now
                        SolutionPathSteps = currentPath, // Complete path ready
                        StepsTaken = currentPath.Count - 1,
                        NodesExplored = nodesExplored,
                        MaxDepthReached = maxDepthReached,
                        SearchAlgorithmUsed = "BFS" // Specify the algorithm used
                    });
                    LoggerInstance?.Log(LogLevel.INFO, $"[BFS] Solution found: '{startStringStandard}' -> '{targetStringStandard}'. Steps: {currentPath.Count - 1}, Nodes explored: {nodesExplored}. Time: {stopwatch.ElapsedMilliseconds} ms.");
                    return currentPath; // Returns path in PathStepInfo
                }

                if (currentPath.Count - 1 >= MassimoPassiRicerca)
                {
                    // LoggerInstance?.Log(LogLevel.DEBUG, $"[BFS] Maximum depth reached ({MassimoPassiRicerca}) for '{currentStandard}'. Skipping exploration.");
                    continue; // Maximum depth reached
                }

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
                    if (rule.TryApply(currentStandard, out string newStringStandard))
                    {
                        OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs
                        {
                            AppliedRuleID = rule.ID,
                            AppliedRuleName = rule.Nome,
                            OriginalString = currentStandard, // STANDARD string
                            NewString = newStringStandard,    // STANDARD string
                            CurrentDepth = currentPath.Count - 1 // Current depth
                        });

                        if (!visitedStandard.Contains(newStringStandard))
                        {
                            visitedStandard.Add(newStringStandard);
                            // Create a new step for the path
                            var newPathStep = new PathStepInfo
                            {
                                StateStringStandard = newStringStandard,
                                AppliedRuleID = rule.ID,
                                ParentStateStringStandard = currentStandard
                            };
                            System.Collections.Generic.List<PathStepInfo> newPath = new System.Collections.Generic.List<PathStepInfo>(currentPath) { newPathStep };
                            queue.Enqueue((newStringStandard, newPath));
                            LoggerInstance?.Log(LogLevel.DEBUG, $"[BFS] Added new state: '{newStringStandard}' (from '{currentStandard}' with rule '{(rule.Nome)}'). Depth: {currentPath.Count}. Queue: {queue.Count}");
                        }
                        else
                        {
                            // LoggerInstance?.Log(LogLevel.DEBUG, $"[BFS] State '{newStringStandard}' already visited. Skipping.");
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
                SearchAlgorithmUsed = "BFS" // Specify the algorithm used
            });
            LoggerInstance?.Log(LogLevel.INFO, $"[BFS] No solution found: '{startStringStandard}' -> '{targetStringCompressed}'. Nodes explored: {nodesExplored}, Max Depth: {maxDepthReached}. Time: {stopwatch.ElapsedMilliseconds} ms.");
            return null; // No derivation found
        }


        /// <summary>
        /// Intelligent method to choose and start the derivation search (BFS or DFS)
        /// based on string length heuristics.
        /// </summary>
        /// <param name="searchId">The ID of the current search for persistence.</param>
        /// <param name="startStringCompressed">The compressed starting string.</param>
        /// <param name="targetStringCompressed">The compressed target string.</param>
        /// <returns>The list of PathStepInfo that constitutes the solution, or null if not found.</returns>
        public static List<PathStepInfo> TrovaDerivazioneAutomatica(long searchId, string startStringCompressed, string targetStringCompressed)
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
                resultPath = TrovaDerivazioneDFS(searchId, startStringCompressed, targetStringCompressed);
            }
            else
            {
                chosenAlgorithm = "BFS (Automatic)";
                LoggerInstance?.Log(LogLevel.INFO, $"[AutoSearch] Target string is not significantly longer. Chosen algorithm BFS.");
                resultPath = TrovaDerivazioneBFS(searchId, startStringCompressed, targetStringCompressed);
            }

            // The OnSolutionFound event is already invoked by the BFS/DFS methods,
            // so we do not invoke it here to avoid duplicates.
            // However, we can add a log to summarize the choice.

            return resultPath;
        }

    }
}
