using EvolutiveSystem.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
// 2025.05.24 aggiunto tempo ms 
// 2025.05.26 Modificato inserimento record con l'aggiornamento dell'indice
namespace MIU.Core
{
    /// <summary>
    /// Fornisce dati per l'evento OnSolutionFound.
    /// Contiene i dettagli completi del risultato di una ricerca di derivazione.
    /// </summary>
    public class SolutionFoundEventArgs : EventArgs
    {
        public string InitialString { get; }
        public string TargetString { get; }
        public bool Success { get; }
        public List<string> Path { get; } // Il percorso trovato, null se non trovata
        public long ElapsedTicks { get; }
        public double ElapsedMilliseconds { get; }
        public int StepsTaken { get; } // Numero di passi nella soluzione (profondità del target)
        public int NodesExplored { get; } // Numero totale di nodi visitati
        public int MaxDepthReached { get; } // Profondità massima raggiunta nella ricerca

        public SolutionFoundEventArgs(string initialString, string targetString, bool success, List<string> path, long elapsedTicks, int stepsTaken, int nodesExplored, int maxDepthReached)
        {
            InitialString = initialString;
            TargetString = targetString;
            Success = success;
            Path = path;
            ElapsedTicks = elapsedTicks;
            ElapsedMilliseconds = (elapsedTicks / (double)Stopwatch.Frequency) * 1000;
            StepsTaken = stepsTaken;
            NodesExplored = nodesExplored;
            MaxDepthReached = maxDepthReached;
        }
    }
    /// <summary>
    /// Fornisce dati per l'evento OnRuleApplied.
    /// Contiene informazioni su ogni nuova stringa generata tramite l'applicazione di una regola.
    /// </summary>
    public class RuleAppliedEventArgs : EventArgs
    {
        public string ParentString { get; } // La stringa da cui è stata derivata nextString
        public string NewString { get; } // La nuova stringa generata
        public string AppliedRuleID { get; } // L'ID della regola applicata
        public string AppliedRuleName { get; } // Il nome della regola applicata
        public int CurrentDepth { get; } // Profondità della nuova stringa nel percorso di ricerca

        public RuleAppliedEventArgs(string parentString, string newString, string appliedRuleID, string appliedRuleName, int currentDepth)
        {
            ParentString = parentString;
            NewString = newString;
            AppliedRuleID = appliedRuleID;
            AppliedRuleName = appliedRuleName;
            CurrentDepth = currentDepth;
        }
    }

    public static partial class RegoleMIUManager
    {
        private static List<RegolaMIU> _regole = new List<RegolaMIU>();
        public static IReadOnlyList<RegolaMIU> Regole => _regole.AsReadOnly();

        // -------------------------------------------------------------------
        // 2. Definizione degli eventi (rimangono invariati)
        // -------------------------------------------------------------------

        public static event EventHandler<SolutionFoundEventArgs> OnSolutionFound;
        public static event EventHandler<RuleAppliedEventArgs> OnRuleApplied;

        public static void CaricaRegoleDaOggettoSQLite(List<string> regoleData)
        {
            _regole.Clear();

            if (regoleData == null || !regoleData.Any())
            {
                Console.WriteLine("La lista di regole fornita è nulla o vuota. Nessuna regola MIU caricata.");
                return;
            }

            foreach (var record in regoleData)
            {
                string[] regolaParts = record.Split(';');
                if (regolaParts.Length >= 5)
                {
                    string id = regolaParts[0];
                    string nome = regolaParts[1];
                    string pattern = regolaParts[2];
                    string sostituzione = regolaParts[3];
                    string descrizione = regolaParts[4];
                    _regole.Add(new RegolaMIU(id, nome, descrizione, pattern, sostituzione));
                }
                else
                {
                    Console.WriteLine($"Avviso: Record di regola incompleto trovato: '{record}'. Questo record è stato saltato.");
                }
            }
            Console.WriteLine($"Caricate {_regole.Count} regole MIU dalla lista SQLite.");
        }

        public static string ApplicaRegole(string input)
        {
            // Questo metodo ApplicaRegole è progettato per operare su stringhe MIU in formato standard (non compresso).
            // Comprime l'input, applica le regole (che internamente lavorano con stringhe compresse),
            // e poi decomprime il risultato per la visualizzazione.
            string currentString = input;
            Console.WriteLine($"\nApplicazione regole a: '{input}'");

            if (!_regole.Any())
            {
                Console.WriteLine("Nessuna regola MIU caricata. La stringa non verrà modificata.");
                return input;
            }

            foreach (var rule in _regole)
            {
                string oldString = currentString;
                string newString;

                // Comprimi la stringa corrente prima di passarla a TryApply
                string compressedCurrent = MIUStringConverter.InflateMIUString(currentString);
                string compressedNew;

                // TryApply ora accetta una stringa compressa e restituisce una stringa compressa.
                if (rule.TryApply(compressedCurrent, out compressedNew))
                {
                    currentString = MIUStringConverter.DeflateMIUString(compressedNew); // Decomprimi per la visualizzazione
                    Console.WriteLine($"  Regola '{rule.Nome}' (Pattern: '{rule.Pattern}', Sostituzione: '{rule.Sostituzione ?? "NULL"}') applicata.");
                    Console.WriteLine($"    Risultato parziale: '{currentString}' (decompresso)");
                }
                else
                {
                    Console.WriteLine($"  Regola '{rule.Nome}' non applicata a '{oldString}'.");
                }
            }

            Console.WriteLine($"Risultato finale dopo l'applicazione delle regole: '{currentString}' (decompresso)");
            return currentString;
        }

        /// <summary>
        /// Esegue una ricerca in ampiezza (BFS) per trovare una derivazione
        /// dalla stringa iniziale alla stringa target usando le regole MIU.
        /// Opera interamente con stringhe compresse per efficienza.
        /// </summary>
        /// <param name="start">La stringa iniziale (formato compresso).</param>
        /// <param name="target">La stringa target (formato compresso).</param>
        /// <param name="maxSteps">La profondità massima della ricerca per evitare cicli infiniti.</param>
        /// <returns>Una lista di stringhe che rappresentano il percorso di derivazione (formato compresso),
        /// o null se la stringa target non viene raggiunta entro la profondità massima.</returns>
        public static List<string> TrovaDerivazioneBFS(string start, string target, long maxSteps = 100)
        {
            // Le stringhe start e target sono già attese in formato compresso.
            // Se la stringa iniziale compressa è già la target compressa, il percorso è solo la stringa stessa.
            if (start == target)
            {
                var path = new List<string> { start };
                OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                    initialString: start,
                    targetString: target,
                    success: true,
                    path: path,
                    elapsedTicks: 0,
                    stepsTaken: 0,
                    nodesExplored: 1,
                    maxDepthReached: 0
                ));
                return path;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            Queue<List<string>> queue = new Queue<List<string>>();
            queue.Enqueue(new List<string> { start }); // Inserisci la stringa compressa
            HashSet<string> visited = new HashSet<string> { start }; // Visited set con stringhe compresse
            List<string> solutionPath = null;
            int depth = 0;
            int nodesExplored = 1;

            while (queue.Count > 0 && depth < maxSteps)
            {
                int levelSize = queue.Count;
                depth++;

                for (int i = 0; i < levelSize; i++)
                {
                    List<string> currentPath = queue.Dequeue();
                    string currentCompressedString = currentPath.Last(); // La stringa corrente è compressa

                    foreach (var rule in Regole)
                    {
                        string nextCompressedString;
                        // TryApply ora accetta e restituisce stringhe compresse
                        if (rule.TryApply(currentCompressedString, out nextCompressedString) && !visited.Contains(nextCompressedString))
                        {
                            nodesExplored++;

                            if (nextCompressedString == target)
                            {
                                solutionPath = new List<string>(currentPath);
                                solutionPath.Add(nextCompressedString);
                                stopwatch.Stop();

                                OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                                    initialString: start,
                                    targetString: target,
                                    success: true,
                                    path: solutionPath, // Il percorso è in formato compresso
                                    elapsedTicks: stopwatch.ElapsedTicks,
                                    stepsTaken: solutionPath.Count - 1,
                                    nodesExplored: nodesExplored,
                                    maxDepthReached: depth
                                ));
                                return solutionPath;
                            }

                            visited.Add(nextCompressedString);
                            List<string> newPath = new List<string>(currentPath);
                            newPath.Add(nextCompressedString);
                            queue.Enqueue(newPath);

                            OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs(
                                parentString: currentCompressedString,
                                newString: nextCompressedString,
                                appliedRuleID: rule.ID,
                                appliedRuleName: rule.Nome,
                                currentDepth: depth
                            ));
                        }
                    }
                }
            }

            stopwatch.Stop();
            OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                initialString: start,
                targetString: target,
                success: false,
                path: null,
                elapsedTicks: stopwatch.ElapsedTicks,
                stepsTaken: (int)maxSteps,
                nodesExplored: nodesExplored,
                maxDepthReached: depth
            ));

            return null;
        }

        /// <summary>
        /// Esegue una ricerca in profondità (DFS) per trovare una derivazione
        /// dalla stringa iniziale alla stringa target usando le regole MIU.
        /// Opera interamente con stringhe compresse per efficienza.
        /// </summary>
        /// <param name="start">La stringa iniziale (formato compresso).</param>
        /// <param name="target">La stringa target (formato compresso).</param>
        /// <param name="maxDepth">La profondità massima della ricerca per evitare cicli infiniti.</param>
        /// <param name="path">La lista corrente del percorso (usata per la ricorsione, con stringhe compresse).</param>
        /// <param name="visited">Un set di stringhe già visitate per evitare cicli (con stringhe compresse).</param>
        /// <returns>Una lista di stringhe che rappresentano il percorso di derivazione (formato compresso),
        /// o null se la stringa target non viene raggiunta entro la profondità massima.</returns>
        public static List<string> TrovaDerivazioneDFS(string start, string target, long maxDepth, List<string> path = null, HashSet<string> visited = null)
        {
            // Le stringhe start e target sono già attese in formato compresso.
            string currentStart = start; // Usa direttamente start, che è già compresso
            string currentTarget = target; // Usa direttamente target, che è già compresso

            double frequency = Stopwatch.Frequency;
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<string> solutionPath = null;
            long elapsedTicks = 0;
            bool soluzioneTrovata = false;
            int numeroPassi = 0;
            int nodesExplored = 0;

            if (path == null)
            {
                path = new List<string> { currentStart };
                visited = new HashSet<string> { currentStart };
                nodesExplored = 1;
            }
            else
            {
                // Se non è la chiamata iniziale, visited.Count riflette i nodi già esplorati nel ramo
                nodesExplored = visited.Count;
            }

            string currentCompressedString = path.Last(); // La stringa corrente è compressa

            if (currentCompressedString == currentTarget)
            {
                stopwatch.Stop();
                elapsedTicks = stopwatch.ElapsedTicks;
                double tempoMs = (elapsedTicks / frequency) * 1000;
                numeroPassi = path.Count - 1;
                soluzioneTrovata = true;
                Console.WriteLine($"\nSoluzione DFS trovata (profondità {path.Count - 1}) in {tempoMs:F2} ms (stringa compressa):");
                foreach (var s in path)
                {
                    Console.WriteLine($"  {s}");
                }
                OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                    initialString: currentStart,
                    targetString: currentTarget,
                    success: true,
                    path: path, // Il percorso è in formato compresso
                    elapsedTicks: elapsedTicks,
                    stepsTaken: numeroPassi,
                    nodesExplored: nodesExplored,
                    maxDepthReached: path.Count - 1
                ));
                return path;
            }

            if (path.Count - 1 >= maxDepth)
            {
                return null;
            }

            foreach (var rule in Regole)
            {
                string nextCompressedString;
                // TryApply ora accetta e restituisce stringhe compresse
                if (rule.TryApply(currentCompressedString, out nextCompressedString) && !visited.Contains(nextCompressedString))
                {
                    visited.Add(nextCompressedString);
                    path.Add(nextCompressedString);

                    OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs(
                        parentString: currentCompressedString,
                        newString: nextCompressedString,
                        appliedRuleID: rule.ID,
                        appliedRuleName: rule.Nome,
                        currentDepth: path.Count - 1
                    ));

                    List<string> result = TrovaDerivazioneDFS(currentStart, currentTarget, maxDepth, path, visited); // Passa start/target compressi
                    if (result != null)
                    {
                        stopwatch.Stop();
                        if (!soluzioneTrovata)
                        {
                            elapsedTicks = stopwatch.ElapsedTicks;
                            double tempoMs = (elapsedTicks / frequency) * 1000;
                            numeroPassi = result.Count - 1;
                            soluzioneTrovata = true;
                            Console.WriteLine($"\nSoluzione DFS trovata (profondità {numeroPassi}) in {tempoMs:F2} ms (al ritorno dalla ricorsione, stringa compressa):");
                            foreach (var s in result)
                            {
                                Console.WriteLine($"  {s}");
                            }
                            OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                                initialString: currentStart,
                                targetString: currentTarget,
                                success: true,
                                path: result, // Il percorso è in formato compresso
                                elapsedTicks: elapsedTicks,
                                stepsTaken: numeroPassi,
                                nodesExplored: nodesExplored,
                                maxDepthReached: result.Count - 1
                            ));
                        }
                        return result;
                    }
                    path.RemoveAt(path.Count - 1);
                    visited.Remove(nextCompressedString); // Rimuovi per permettere esplorazione in altri rami DFS
                }
            }

            if (path.Count == 1)
            {
                stopwatch.Stop();
                elapsedTicks = stopwatch.ElapsedTicks;
                double tempoMsFallimento = (elapsedTicks / frequency) * 1000;
                int numeroPassiFallimento = (int)maxDepth;

                Console.WriteLine($"\nRicerca DFS da '{start}' a '{target}' fallita entro la profondità massima di {maxDepth} in {tempoMsFallimento:F2} ms.");

                OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                    initialString: currentStart,
                    targetString: currentTarget,
                    success: false,
                    path: null,
                    elapsedTicks: elapsedTicks,
                    stepsTaken: numeroPassiFallimento,
                    nodesExplored: nodesExplored,
                    maxDepthReached: (int)maxDepth
                ));
            }

            return null;
        }
    }

    /*
    public static List<string> TrovaDerivazioneDFS(string start, string target, long maxDepth, List<string> path = null, HashSet<string> visited = null, Database database = null)
    {
        double frequency = Stopwatch.Frequency;
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<string> solutionPath = null;
        long elapsedTicks = 0;
        bool soluzioneTrovata = false;
        int numeroPassi = 0;

        if (path == null)
        {
            path = new List<string> { start };
        }
        if (visited == null)
        {
            visited = new HashSet<string> { start };
        }

        string currentString = path.Last();

        if (currentString == target)
        {
            stopwatch.Stop();
            elapsedTicks = stopwatch.ElapsedTicks;
            double tempoMs = (elapsedTicks / frequency) * 1000;
            numeroPassi = path.Count - 1;
            soluzioneTrovata = true;
            Console.WriteLine($"\nSoluzione DFS trovata (profondità {path.Count - 1}) in {tempoMs:F2} ms:");
            foreach (var s in path)
            {
                Console.WriteLine($"  {s}");
            }
            // Memorizza il risultato nel database se fornito
            if (database != null && database.Tables.Any(t => t.TableName == "EsplorazioneMIU"))
            {
                var tabellaEsplorazione = database.Tables.First(t => t.TableName == "EsplorazioneMIU");
                var record = new SerializableDictionary<string, object>
            {
                { "StringaIniziale", start },
                { "StringaTarget", target },
                { "NumeroPassi", numeroPassi },
                { "LimitePassi", maxDepth },
                { "TicOrologio",(ulong) elapsedTicks },
                { "TempoMs", tempoMs },
                { "SoluzioneTrovata", soluzioneTrovata }
            };
                tabellaEsplorazione.AddRecord(record);
                //Console.WriteLine($"Risultato DFS da '{start}' a '{target}' memorizzato.");
            }
            return path;
        }

        if (path.Count - 1 >= maxDepth)
        {
            return null;
        }

        foreach (var rule in Regole)
        {
            string nextString;
            if (rule.TryApply(currentString, out nextString) && !visited.Contains(nextString))
            {
                visited.Add(nextString);
                path.Add(nextString);
                List<string> result = TrovaDerivazioneDFS(start, target, maxDepth, path, visited, database);
                if (result != null)
                {
                    stopwatch.Stop();
                    if (!soluzioneTrovata) // Registra il tempo solo alla prima soluzione trovata
                    {
                        elapsedTicks = stopwatch.ElapsedTicks;
                        double tempoMs = (elapsedTicks / frequency) * 1000;
                        numeroPassi = result.Count - 1;
                        soluzioneTrovata = true;
                        Console.WriteLine($"\nSoluzione DFS trovata (profondità {numeroPassi}) in {tempoMs:F2} ms (al ritorno dalla ricorsione):");
                        foreach (var s in result)
                        {
                            Console.WriteLine($"  {s}");
                        }
                        if (database != null && database.Tables.Any(t => t.TableName == "EsplorazioneMIU"))
                        {
                            var tabellaEsplorazione = database.Tables.First(t => t.TableName == "EsplorazioneMIU");
                            var record = new SerializableDictionary<string, object>
                        {
                            { "StringaIniziale", start },
                            { "StringaTarget", target },
                            { "NumeroPassi", numeroPassi },
                            { "LimitePassi", maxDepth },
                            { "TicOrologio",(ulong) elapsedTicks },
                            { "TempoMs", tempoMs },
                            { "SoluzioneTrovata", soluzioneTrovata }
                        };
                            tabellaEsplorazione.AddRecord(record);
                            //Console.WriteLine($"Risultato DFS da '{start}' a '{target}' memorizzato (al ritorno).");
                        }
                    }
                    return result;
                }
                path.RemoveAt(path.Count - 1); // Backtrack
                visited.Remove(nextString); // Permetti di visitare di nuovo in altri rami
            }
        }

        return null; // Nessuna soluzione trovata in questo ramo
    }
    */
}
