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
    public static partial class RegoleMIUManager // Usiamo partial per poter estendere la classe in più file se necessario
    {
        // Questa è la lista privata dove vengono memorizzate le regole caricate.
        private static List<RegolaMIU> _regole = new List<RegolaMIU>();

        // Espone le regole caricate come una lista di sola lettura
        public static IReadOnlyList<RegolaMIU> Regole => _regole.AsReadOnly();
        /// <summary>
        /// Evento scatenato quando una ricerca di derivazione (BFS/DFS) si conclude,
        /// sia con successo che con fallimento.
        /// </summary>
        public static event EventHandler<SolutionFoundEventArgs> OnSolutionFound;
        /// <summary>
        /// Evento scatenato ogni volta che una regola viene applicata con successo
        /// e genera una nuova stringa non ancora visitata durante una ricerca.
        /// </summary>
        public static event EventHandler<RuleAppliedEventArgs> OnRuleApplied;
        /// <summary>
        /// Carica le regole MIU da una lista di stringhe, dove ogni stringa
        /// rappresenta una regola con campi separati da punto e virgola.
        /// </summary>
        /// <param name="Regole">Una lista di stringhe contenenti i dati delle regole.</param>
        public static void CaricaRegoleDaOggettoSQLite(List<string> Regole)
        {
            _regole.Clear(); // Pulisci le regole esistenti prima di caricare le nuove
            foreach (var record in Regole)
            {                //// Estrai i valori accedendo direttamente al dizionario.
                string[] regola = record.Split(';');
                string id = regola[0];
                string nome = regola[1];
                string pattern = regola[2];
                string sostituzione = regola[3];
                string descrizione = regola[4];
                _regole.Add(new RegolaMIU(id, nome, descrizione, pattern, sostituzione));
            }
        }
        /// <summary>
        /// Applica tutte le regole MIU caricate a una stringa di input, iterando su di esse.
        /// </summary>
        /// <param name="input">La stringa a cui applicare le regole.</param>
        /// <returns>La stringa risultante dopo l'applicazione delle regole.</returns>
        public static string ApplicaRegole(string input)
        {
            string currentString = input; // Inizializza currentString qui
            if (!_regole.Any())
            {
                return input;
            }

            // Iteriamo su ogni regola e tentiamo di applicarla.
            // Puoi decidere se applicare ogni regola una sola volta o iterare finché non ci sono più cambiamenti.
            // Per ora, applichiamo ogni regola una volta in sequenza.
            foreach (var rule in _regole)
            {
                string oldString = currentString;
                string newString; // Dichiarata qui per l'out parameter

                if (rule.TryApply(currentString, out newString))
                {
                    currentString = newString; // Aggiorna la stringa corrente se la regola è stata applicata
                }
            }

            Console.WriteLine($"Risultato finale dopo l'applicazione delle regole: '{currentString}'");
            return currentString;
        }

        /// <summary>
        /// Esegue una ricerca in ampiezza (BFS) per trovare una derivazione
        /// dalla stringa iniziale alla stringa target usando le regole MIU.
        /// </summary>
        /// <param name="start">La stringa iniziale.</param>
        /// <param name="target">La stringa target.</param>
        /// <param name="maxDepth">La profondità massima della ricerca per evitare cicli infiniti.</param>
        /// <returns>Una lista di stringhe che rappresentano il percorso di derivazione,
        /// o null se la stringa target non viene raggiunta entro la profondità massima.</returns>
        public static List<string> TrovaDerivazioneBFS(string start, string target, long maxSteps = 100)
        {
            double frequency = Stopwatch.Frequency; // Ottieni la frequenza una volta per calcolo tempo ms
            if (start == target)
            {
                var path = new List<string> { start };
                // Scatena l'evento di soluzione trovata immediatamente
                OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                    initialString: start,
                    targetString: target,
                    success: true,
                    path: path,
                    elapsedTicks: 0, // Tempo quasi nullo
                    stepsTaken: 0,
                    nodesExplored: 1, // Solo il nodo iniziale
                    maxDepthReached: 0
                ));
                return path;
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            Queue<List<string>> queue = new Queue<List<string>>();
            queue.Enqueue(new List<string> { start });
            HashSet<string> visited = new HashSet<string> { start };
            List<string> solutionPath = null;
            int depth = 0;
            int nodesExplored = 1; // Contatore per i nodi esplorati (inizia con la stringa di partenza)

            while (queue.Count > 0 && depth < maxSteps)
            {
                int levelSize = queue.Count;
                depth++;

                for (int i = 0; i < levelSize; i++)
                {
                    List<string> currentPath = queue.Dequeue();
                    string currentString = currentPath.Last();
                    // Console.WriteLine($"queue.Count: {queue.Count} i: {i}");
                    foreach (var rule in Regole)
                    {
                        string nextString;
                        if (rule.TryApply(currentString, out nextString) && !visited.Contains(nextString))
                        {
                            nodesExplored++; // Incrementa il contatore dei nodi esplorati
                            if (nextString == target)
                            {
                                solutionPath = new List<string>(currentPath);
                                solutionPath.Add(nextString);
                                stopwatch.Stop();
                                OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                                    initialString: start,
                                    targetString: target,
                                    success: true,
                                    path: solutionPath,
                                    elapsedTicks: stopwatch.ElapsedTicks,
                                    stepsTaken: solutionPath.Count - 1, // Numero di passi = lunghezza del percorso - 1
                                    nodesExplored: nodesExplored,
                                    maxDepthReached: depth
                                ));
                                return solutionPath;
                            }

                            visited.Add(nextString);
                            List<string> newPath = new List<string>(currentPath);
                            newPath.Add(nextString);
                            queue.Enqueue(newPath);
                            // Scatena l'evento di regola applicata
                            OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs(
                                parentString: currentString,
                                newString: nextString,
                                appliedRuleID: rule.Id,
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
                path: null, // Nessun percorso trovato
                elapsedTicks: stopwatch.ElapsedTicks,
                stepsTaken: (int)maxSteps, // Indica il limite di passi raggiunto
                nodesExplored: nodesExplored,
                maxDepthReached: depth
            ));
            return null;
        }

        /// <summary>
        /// Esegue una ricerca in profondità (DFS) per trovare una derivazione
        /// dalla stringa iniziale alla stringa target usando le regole MIU.
        /// </summary>
        /// <param name="start">La stringa iniziale.</param>
        /// <param name="target">La stringa target.</param>
        /// <param name="maxDepth">La profondità massima della ricerca per evitare cicli infiniti.</param>
        /// <param name="path">La lista corrente del percorso (usata per la ricorsione).</param>
        /// <param name="visited">Un set di stringhe già visitate per evitare cicli.</param>
        /// <param name="database">L'oggetto Database per la memorizzazione dei risultati (opzionale).</param>
        /// <returns>Una lista di stringhe che rappresentano il percorso di derivazione,
        /// o null se la stringa target non viene raggiunta entro la profondità massima.</returns>
        public static List<string> TrovaDerivazioneDFS(string start, string target, long maxDepth, List<string> path = null, HashSet<string> visited = null)
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
                // Memorizza il risultato (successo) nel database se fornito
                //if (database != null && database.Tables.Any(t => t.TableName == "EsplorazioneMIU"))
                //{
                //    var tabellaEsplorazione = database.Tables.First(t => t.TableName == "EsplorazioneMIU");
                //    var record = new SerializableDictionary<string, object>
                //{
                //    { "StringaIniziale", start },
                //    { "StringaTarget", target },
                //    { "NumeroPassi", numeroPassi },
                //    { "LimitePassi", maxDepth },
                //    { "TicOrologio",(ulong) elapsedTicks },
                //    { "TempoMs", tempoMs },
                //    { "SoluzioneTrovata", soluzioneTrovata }
                //};
                //    tabellaEsplorazione.AddRecord(record);
                //    //Console.WriteLine($"Risultato DFS (successo) da '{start}' a '{target}' memorizzato.");
                //}
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
                    List<string> result = TrovaDerivazioneDFS(start, target, maxDepth, path, visited);
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
                            //if (database != null && database.Tables.Any(t => t.TableName == "EsplorazioneMIU"))
                            //{
                            //    var tabellaEsplorazione = database.Tables.First(t => t.TableName == "EsplorazioneMIU");
                            //    var record = new SerializableDictionary<string, object>
                            //{
                            //    { "StringaIniziale", start },
                            //    { "StringaTarget", target },
                            //    { "NumeroPassi", numeroPassi },
                            //    { "LimitePassi", maxDepth },
                            //    { "TicOrologio",(ulong) elapsedTicks },
                            //    { "TempoMs", tempoMs },
                            //    { "SoluzioneTrovata", soluzioneTrovata }
                            //};
                            //    tabellaEsplorazione.AddRecord(record);
                            //    //Console.WriteLine($"Risultato DFS (successo) da '{start}' a '{target}' memorizzato (al ritorno).");
                            //}
                        }
                        return result;
                    }
                    path.RemoveAt(path.Count - 1); // Backtrack
                    visited.Remove(nextString); // Permetti di visitare di nuovo in altri rami
                }
            }

            // Se la ricerca arriva qui, significa che non è stata trovata una soluzione in questo ramo.
            // Memorizziamo il fallimento se siamo alla chiamata iniziale (path.Count == 1) e abbiamo raggiunto la profondità massima.
            if (path.Count == 1)
            {
                stopwatch.Stop();
                elapsedTicks = stopwatch.ElapsedTicks;
                double tempoMsFallimento = (elapsedTicks / frequency) * 1000;
                bool soluzioneTrovataFallimento = false;
                int numeroPassiFallimento = (int)maxDepth; // Indica la profondità massima raggiunta

                Console.WriteLine($"\nRicerca DFS da '{start}' a '{target}' fallita entro la profondità massima di {maxDepth} in {tempoMsFallimento:F2} ms.");

                //if (database != null && database.Tables.Any(t => t.TableName == "EsplorazioneMIU"))
                //{
                //    var tabellaEsplorazione = database.Tables.First(t => t.TableName == "EsplorazioneMIU");
                //    var recordFallimento = new SerializableDictionary<string, object>
                //{
                //    { "StringaIniziale", start },
                //    { "StringaTarget", target },
                //    { "NumeroPassi", numeroPassiFallimento },
                //    { "LimitePassi", maxDepth },
                //    { "TicOrologio", (ulong)elapsedTicks },
                //    { "TempoMs", tempoMsFallimento },
                //    { "SoluzioneTrovata", soluzioneTrovataFallimento }
                //};
                //    tabellaEsplorazione.AddRecord(recordFallimento);
                //    //Console.WriteLine($"Risultato DFS (fallito) da '{start}' a '{target}' memorizzato.");
                //}
            }

            return null; // Nessuna soluzione trovata in questo ramo
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
