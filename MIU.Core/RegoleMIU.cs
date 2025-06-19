// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\RegoleMIU.cs
// Data di riferimento: 4 giugno 2025
// aggiornato 19.6.25 9.46
// Contiene la classe RegoleMIUManager per la gestione delle regole MIU e gli eventi correlati.
// CORREZIONE 17.6.25: Rimossa la direttiva 'using EvolutiveSystem.SQL.Core;' per eliminare la dipendenza inversa.
// MODIFICA 17.6.25: Adattato TrovaDerivazioneBFS e TrovaDerivazioneDFS per operare su stringhe standard (decompresse).
// NUOVA MODIFICA 19.6.25: Integrazione di MasterLog.Logger per sostituire Console.WriteLine e aggiunta LoggerInstance.
// CORREZIONE 20.6.25: AGGIUNTA EFFETTIVA DELLA PROPRIETA' LoggerInstance MANCANTE.
// NUOVA MODIFICA 20.6.25: Aggiornamento SolutionFoundEventArgs e logica BFS/DFS per persistenza completa.
// MODIFICA 20.6.25: Implementazione della gestione interna di MaxProfonditaRicerca e MassimoPassiRicerca.
// NUOVA MODIFICA 21.6.25: Aggiunta logica di ordinamento delle regole basata su RuleStatistics.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using MasterLog; // Necessario per la tua classe Logger

namespace MIU.Core
{
    /// <summary>
    /// Struttura dati per un singolo passo nel percorso della soluzione.
    /// Contiene la stringa dello stato, l'ID della regola applicata per raggiungerlo
    /// e la stringa dello stato genitore.
    /// </summary>
    public class PathStepInfo
    {
        public string StateStringStandard { get; set; } // La stringa MIU standard (decompressa) per questo stato
        public long? AppliedRuleID { get; set; } // L'ID della regola applicata per arrivare a questo stato (null per lo stato iniziale)
        public string ParentStateStringStandard { get; set; } // La stringa MIU standard (decompressa) del genitore (null per lo stato iniziale)
    }

    // EventArgs per l'evento OnSolutionFound
    public class SolutionFoundEventArgs : EventArgs
    {
        public long SearchID { get; set; } // NUOVO: ID della ricerca
        public string InitialString { get; set; } // Questa sarà la stringa COMPRESSA originale
        public string TargetString { get; set; } // Questa sarà la stringa COMPRESSA target
        public bool Success { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public long ElapsedTicks { get; set; }
        public List<PathStepInfo> SolutionPathSteps { get; set; } // NUOVO TIPO: lista di PathStepInfo
        public int StepsTaken { get; set; } // Numero di passi nella soluzione
        public int NodesExplored { get; set; } // Numero di nodi esplorati durante la ricerca
        public int MaxDepthReached { get; set; } // Profondità massima raggiunta
        public bool FromCache { get; set; } // Indica se la soluzione è stata trovata in cache (aggiunto per completezza con altri membri)
    }

    // EventArgs per l'evento OnRuleApplied
    public class RuleAppliedEventArgs : EventArgs
    {
        public long AppliedRuleID { get; set; }
        public string AppliedRuleName { get; set; }
        public string OriginalString { get; set; } // Questa sarà la stringa STANDARD
        public string NewString { get; set; } // Questa sarà la stringa STANDARD
        public int CurrentDepth { get; set; }
    }

    public static class RegoleMIUManager
    {
        // Proprietà statica per l'istanza del logger
        public static Logger LoggerInstance { get; set; }

        // Proprietà statiche per i parametri di configurazione (MaxDepth e MaxSteps)
        /// <summary>
        /// Profondità massima consentita per le ricerche in profondità (DFS).
        /// Viene impostata dall'orchestratore all'avvio.
        /// </summary>
        public static long MaxProfonditaRicerca { get; set; }

        /// <summary>
        /// Numero massimo di passi/nodi da esplorare per le ricerche in ampiezza (BFS).
        /// Viene impostato dall'orchestratore all'avvio.
        /// </summary>
        public static long MassimoPassiRicerca { get; set; }

        // NUOVO: Proprietà statica per accedere alle RuleStatistics caricate dall'orchestratore
        /// <summary>
        /// Riferimento al dizionario delle RuleStatistics correnti, caricato da Program.cs.
        /// Utilizzato per l'ordinamento delle regole in base alla loro efficacia.
        /// </summary>
        public static Dictionary<long, RuleStatistics> CurrentRuleStatistics { get; set; }


        // Collezione statica di tutte le regole MIU disponibili.
        public static List<RegolaMIU> Regole { get; private set; } = new List<RegolaMIU>();

        // Eventi per notificare la soluzione trovata o l'applicazione di una regola.
        public static event EventHandler<SolutionFoundEventArgs> OnSolutionFound;
        public static event EventHandler<RuleAppliedEventArgs> OnRuleApplied;

        /// <summary>
        /// Carica le regole MIU da una lista di stringhe formattate come output SQLiteSelect.
        /// Questo metodo è progettato per interfacciarsi con il formato stringa di SQLiteSchemaLoader.
        /// ATTENZIONE: Questo metodo assume un formato stringa specifico e non è robusto a cambiamenti.
        /// </summary>
        /// <param name="regoleRawData">Lista di stringhe, ogni stringa rappresenta una riga di dati delimitata da ';'.</param>
        public static void CaricaRegoleDaOggettoSQLite(List<string> regoleRawData)
        {
            Regole.Clear(); // Pulisce le regole esistenti prima di caricare le nuove

            foreach (string riga in regoleRawData)
            {
                string[] campi = riga.Split(';');
                if (campi.Length >= 5) // Assicurati che ci siano abbastanza campi
                {
                    try
                    {
                        // Assumiamo l'ordine: ID, Nome, Pattern, Sostituzione, Descrizione
                        long id = Convert.ToInt64(campi[0]);
                        string nome = campi[1].Trim();
                        string pattern = campi[2].Trim();
                        string sostituzione = campi[3].Trim();
                        string descrizione = campi[4].Trim();

                        Regole.Add(new RegolaMIU(id, nome, descrizione, pattern, sostituzione));
                    }
                    catch (Exception ex)
                    {
                        // Usa il LoggerInstance per il log degli errori
                        LoggerInstance?.Log(LogLevel.ERROR, $"[RegoleMIUManager ERROR] Errore nel parsing di una riga regola: {riga}. Dettagli: {ex.Message}");
                    }
                }
            }
            LoggerInstance?.Log(LogLevel.DEBUG, $"[RegoleMIUManager DEBUG] Caricate {Regole.Count} regole da oggetto SQLite.");
        }

        /// <summary>
        /// Carica le regole MIU da una lista di oggetti RegolaMIU.
        /// Questo metodo è pensato per essere utilizzato con l'output di MIURepository.LoadRegoleMIU().
        /// </summary>
        /// <param name="regoleMIU">Lista di oggetti RegolaMIU.</param>
        public static void CaricaRegoleDaOggettoRepository(List<RegolaMIU> regoleMIU)
        {
            Regole.Clear(); // Pulisce le regole esistenti prima di caricare le nuove
            Regole.AddRange(regoleMIU); // Aggiunge tutte le regole dalla lista fornita
            LoggerInstance?.Log(LogLevel.DEBUG, $"[RegoleMIUManager DEBUG] Caricate {Regole.Count} regole da oggetto Repository.");
        }


        /// <summary>
        /// Applica le regole MIU a una stringa data in un ciclo, mostrando tutti i passaggi.
        /// Questa funzione opera su stringhe STANDARD (decompresse).
        /// </summary>
        /// <param name="initialStringStandard">La stringa iniziale standard (decompressa) a cui applicare le regole.</param>
        public static void ApplicaRegole(string initialStringStandard)
        {
            LoggerInstance?.Log(LogLevel.INFO, $"Stringa iniziale: {initialStringStandard}");
            string currentStringStandard = initialStringStandard;
            int step = 0;
            bool appliedAnyRule;

            do
            {
                appliedAnyRule = false;
                // MODIFICA: Ordina le regole prima di applicarle
                var orderedRules = Regole.OrderByDescending(rule =>
                {
                    if (CurrentRuleStatistics != null && CurrentRuleStatistics.TryGetValue(rule.ID, out RuleStatistics stats))
                    {
                        return stats.EffectivenessScore;
                    }
                    return 0.0; // Punteggio predefinito per regole senza statistiche
                })
                .ThenByDescending(rule =>
                {
                    if (CurrentRuleStatistics != null && CurrentRuleStatistics.TryGetValue(rule.ID, out RuleStatistics stats))
                    {
                        return stats.ApplicationCount;
                    }
                    return 0; // Conteggio predefinito per regole senza statistiche
                })
                .ToList();


                foreach (var rule in orderedRules) // Usa le regole ordinate
                {
                    if (rule.TryApply(currentStringStandard, out string newStringStandard))
                    {
                        LoggerInstance?.Log(LogLevel.INFO, $"Passo {step + 1}: Applicata Regola '{rule.Nome}' ({rule.ID}) a '{currentStringStandard}' -> '{newStringStandard}'");
                        OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs
                        {
                            AppliedRuleID = rule.ID,
                            AppliedRuleName = rule.Nome,
                            OriginalString = currentStringStandard, // Stringa STANDARD
                            NewString = newStringStandard,    // Stringa STANDARD
                            CurrentDepth = step
                        });
                        currentStringStandard = newStringStandard;
                        appliedAnyRule = true;
                        step++;
                        break; // Applica una sola regola per passo per test
                    }
                }
            } while (appliedAnyRule);

            LoggerInstance?.Log(LogLevel.INFO, $"Risultato finale dopo {step} passi: {currentStringStandard}");
        }

        /// <summary>
        /// Implementazione della ricerca in profondità (DFS) per trovare una derivazione.
        /// Opera su stringhe STANDARD internamente, ma accetta/restituisce stringhe COMPRESSE.
        /// Utilizza la proprietà statica MaxProfonditaRicerca per il limite di profondità.
        /// </summary>
        /// <param name="searchId">L'ID della ricerca corrente per la persistenza.</param>
        public static List<PathStepInfo> TrovaDerivazioneDFS(long searchId, string startStringCompressed, string targetStringCompressed)
        {
            // Decomprimi le stringhe iniziali e target per la ricerca interna
            string startStringStandard = MIUStringConverter.DeflateMIUString(startStringCompressed);
            string targetStringStandard = MIUStringConverter.DeflateMIUString(targetStringCompressed);

            // Stack per la DFS: (stato corrente standard, percorso di PathStepInfo fino a qui)
            Stack<(string currentStandard, List<PathStepInfo> currentPath)> stack = new Stack<(string, List<PathStepInfo>)>();
            HashSet<string> visitedStandard = new HashSet<string>(); // Per tracciare gli stati standard già visitati
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Aggiungi lo stato iniziale al percorso
            var initialPathStep = new PathStepInfo
            {
                StateStringStandard = startStringStandard,
                AppliedRuleID = null, // Nessuna regola applicata per lo stato iniziale
                ParentStateStringStandard = null // Nessun genitore per lo stato iniziale
            };
            stack.Push((startStringStandard, new List<PathStepInfo> { initialPathStep }));
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
                    // Il percorso è già in PathStepInfo con stringhe standard
                    OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs
                    {
                        SearchID = searchId, // Passa l'ID della ricerca
                        InitialString = startStringCompressed, // Stringa COMPRESSA originale
                        TargetString = targetStringCompressed, // Stringa COMPRESSA target
                        Success = true,
                        ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                        FromCache = false, // Considera sempre false per ora
                        ElapsedTicks = stopwatch.ElapsedTicks,
                        SolutionPathSteps = currentPath, // Percorso completo già pronto
                        StepsTaken = currentPath.Count - 1,
                        NodesExplored = nodesExplored,
                        MaxDepthReached = maxDepthReached
                    });
                    LoggerInstance?.Log(LogLevel.INFO, $"[DFS] Soluzione trovata: '{startStringCompressed}' -> '{targetStringCompressed}'. Passi: {currentPath.Count - 1}, Nodi esplorati: {nodesExplored}. Tempo: {stopwatch.ElapsedMilliseconds} ms.");
                    return currentPath; // Restituisce percorso in PathStepInfo
                }

                if (currentPath.Count - 1 >= MaxProfonditaRicerca) continue; // Profondità massima raggiunta

                // MODIFICA: Ordina le regole prima di applicarle
                var orderedRules = Regole.OrderByDescending(rule =>
                {
                    if (CurrentRuleStatistics != null && CurrentRuleStatistics.TryGetValue(rule.ID, out RuleStatistics stats))
                    {
                        return stats.EffectivenessScore;
                    }
                    return 0.0; // Punteggio predefinito per regole senza statistiche
                })
                .ThenByDescending(rule =>
                {
                    if (CurrentRuleStatistics != null && CurrentRuleStatistics.TryGetValue(rule.ID, out RuleStatistics stats))
                    {
                        return stats.ApplicationCount;
                    }
                    return 0; // Conteggio predefinito per regole senza statistiche
                })
                .ToList();

                foreach (var rule in orderedRules) // Usa le regole ordinate
                {
                    // TryApply opera su stringhe STANDARD
                    if (rule.TryApply(currentStandard, out string newStringStandard))
                    {
                        OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs
                        {
                            AppliedRuleID = rule.ID,
                            AppliedRuleName = rule.Nome,
                            OriginalString = currentStandard, // Stringa STANDARD
                            NewString = newStringStandard,    // Stringa STANDARD
                            CurrentDepth = currentPath.Count - 1 // Profondità corrente
                        });

                        if (!visitedStandard.Contains(newStringStandard))
                        {
                            visitedStandard.Add(newStringStandard);
                            // Crea un nuovo passo per il percorso
                            var newPathStep = new PathStepInfo
                            {
                                StateStringStandard = newStringStandard,
                                AppliedRuleID = rule.ID,
                                ParentStateStringStandard = currentStandard
                            };
                            List<PathStepInfo> newPath = new List<PathStepInfo>(currentPath) { newPathStep };
                            stack.Push((newStringStandard, newPath));
                        }
                    }
                }
            }

            stopwatch.Stop();
            OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs
            {
                SearchID = searchId, // Passa l'ID della ricerca
                InitialString = startStringCompressed, // Stringa COMPRESSA originale
                TargetString = targetStringCompressed, // Stringa COMPRESSA target
                Success = false,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ElapsedTicks = stopwatch.ElapsedTicks,
                FromCache = false, // Considera sempre false per ora
                SolutionPathSteps = null, // Percorso nullo se non trovato
                StepsTaken = -1,
                NodesExplored = nodesExplored,
                MaxDepthReached = maxDepthReached
            });
            LoggerInstance?.Log(LogLevel.INFO, $"[DFS] Nessuna soluzione trovata: '{startStringCompressed}' -> '{targetStringCompressed}'. Nodi esplorati: {nodesExplored}, Prof. Max: {maxDepthReached}. Tempo: {stopwatch.ElapsedMilliseconds} ms.");
            return null; // Nessuna derivazione trovata
        }


        /// <summary>
        /// Implementazione della ricerca in ampiezza (BFS) per trovare la derivazione più breve.
        /// Opera su stringhe STANDARD internamente, ma accetta/restituisce una lista di PathStepInfo.
        /// Utilizza la proprietà statica MassimoPassiRicerca per il limite di passi.
        /// </summary>
        /// <param name="searchId">L'ID della ricerca corrente per la persistenza.</param>
        public static List<PathStepInfo> TrovaDerivazioneBFS(long searchId, string startStringCompressed, string targetStringCompressed)
        {
            // Decomprimi le stringhe iniziali e target per la ricerca interna
            string startStringStandard = MIUStringConverter.DeflateMIUString(startStringCompressed);
            string targetStringStandard = MIUStringConverter.DeflateMIUString(targetStringCompressed);

            // Coda per la BFS: (stato corrente standard, percorso di PathStepInfo fino a qui)
            Queue<(string currentStandard, List<PathStepInfo> currentPath)> queue = new Queue<(string, List<PathStepInfo>)>();
            HashSet<string> visitedStandard = new HashSet<string>(); // Per tracciare gli stati standard già visitati
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Aggiungi lo stato iniziale al percorso
            var initialPathStep = new PathStepInfo
            {
                StateStringStandard = startStringStandard,
                AppliedRuleID = null, // Nessuna regola applicata per lo stato iniziale
                ParentStateStringStandard = null // Nessun genitore per lo stato iniziale
            };
            queue.Enqueue((startStringStandard, new List<PathStepInfo> { initialPathStep }));
            visitedStandard.Add(startStringStandard);

            int nodesExplored = 0;
            int maxDepthReached = 0;

            LoggerInstance?.Log(LogLevel.DEBUG, $"[BFS] Inizio ricerca da '{startStringStandard}' a '{targetStringStandard}' (Max passi: {MassimoPassiRicerca})");

            while (queue.Count > 0)
            {
                nodesExplored++;
                var (currentStandard, currentPath) = queue.Dequeue();
                maxDepthReached = Math.Max(maxDepthReached, currentPath.Count - 1); // Depth is path length - 1

                if (currentStandard == targetStringStandard)
                {
                    stopwatch.Stop();
                    // Il percorso è già in PathStepInfo con stringhe standard
                    OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs
                    {
                        SearchID = searchId, // Passa l'ID della ricerca
                        InitialString = startStringCompressed, // Stringa COMPRESSA originale
                        TargetString = targetStringCompressed, // Stringa COMPRESSA target
                        Success = true,
                        ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                        ElapsedTicks = stopwatch.ElapsedTicks,
                        FromCache = false, // Considera sempre false per ora
                        SolutionPathSteps = currentPath, // Percorso completo già pronto
                        StepsTaken = currentPath.Count - 1,
                        NodesExplored = nodesExplored,
                        MaxDepthReached = maxDepthReached
                    });
                    LoggerInstance?.Log(LogLevel.INFO, $"[BFS] Soluzione trovata: '{startStringStandard}' -> '{targetStringStandard}'. Passi: {currentPath.Count - 1}, Nodi esplorati: {nodesExplored}. Tempo: {stopwatch.ElapsedMilliseconds} ms.");
                    return currentPath; // Restituisce percorso in PathStepInfo
                }

                if (currentPath.Count - 1 >= MassimoPassiRicerca)
                {
                    // LoggerInstance?.Log(LogLevel.DEBUG, $"[BFS] Raggiunta profondità massima ({MassimoPassiRicerca}) per '{currentStandard}'. Saltando esplorazione.");
                    continue; // Profondità massima raggiunta
                }

                // MODIFICA: Ordina le regole prima di applicarle
                var orderedRules = Regole.OrderByDescending(rule =>
                {
                    if (CurrentRuleStatistics != null && CurrentRuleStatistics.TryGetValue(rule.ID, out RuleStatistics stats))
                    {
                        return stats.EffectivenessScore;
                    }
                    return 0.0; // Punteggio predefinito per regole senza statistiche
                })
                .ThenByDescending(rule =>
                {
                    if (CurrentRuleStatistics != null && CurrentRuleStatistics.TryGetValue(rule.ID, out RuleStatistics stats))
                    {
                        return stats.ApplicationCount;
                    }
                    return 0; // Conteggio predefinito per regole senza statistiche
                })
                .ToList();


                foreach (var rule in orderedRules) // Usa le regole ordinate
                {
                    // TryApply opera su stringhe STANDARD
                    if (rule.TryApply(currentStandard, out string newStringStandard))
                    {
                        OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs
                        {
                            AppliedRuleID = rule.ID,
                            AppliedRuleName = rule.Nome,
                            OriginalString = currentStandard, // Stringa STANDARD
                            NewString = newStringStandard,    // Stringa STANDARD
                            CurrentDepth = currentPath.Count - 1 // Profondità corrente
                        });

                        if (!visitedStandard.Contains(newStringStandard))
                        {
                            visitedStandard.Add(newStringStandard);
                            // Crea un nuovo passo per il percorso
                            var newPathStep = new PathStepInfo
                            {
                                StateStringStandard = newStringStandard,
                                AppliedRuleID = rule.ID,
                                ParentStateStringStandard = currentStandard
                            };
                            List<PathStepInfo> newPath = new List<PathStepInfo>(currentPath) { newPathStep };
                            queue.Enqueue((newStringStandard, newPath));
                            LoggerInstance?.Log(LogLevel.DEBUG, $"[BFS] Aggiunto nuovo stato: '{newStringStandard}' (da '{currentStandard}' con regola '{(rule.Nome)}'). Profondità: {currentPath.Count}. Coda: {queue.Count}");
                        }
                        else
                        {
                            // LoggerInstance?.Log(LogLevel.DEBUG, $"[BFS] Stato '{newStringStandard}' già visitato. Saltando.");
                        }
                    }
                }
            }

            stopwatch.Stop();
            OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs
            {
                SearchID = searchId, // Passa l'ID della ricerca
                InitialString = startStringCompressed, // Stringa COMPRESSA originale
                TargetString = targetStringCompressed, // Stringa COMPRESSA target
                Success = false,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ElapsedTicks = stopwatch.ElapsedTicks,
                FromCache = false, // Considera sempre false per ora
                SolutionPathSteps = null, // Percorso nullo se non trovato
                StepsTaken = -1,
                NodesExplored = nodesExplored,
                MaxDepthReached = maxDepthReached
            });
            LoggerInstance?.Log(LogLevel.INFO, $"[BFS] Nessuna soluzione trovata: '{startStringStandard}' -> '{targetStringStandard}'. Nodi esplorati: {nodesExplored}, Prof. Max: {maxDepthReached}. Tempo: {stopwatch.ElapsedMilliseconds} ms.");
            return null; // Nessuna derivazione trovata
        }
    }
}
