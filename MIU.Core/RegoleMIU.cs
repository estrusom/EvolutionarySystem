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
    /// Rappresenta la meta-struttura di apprendimento che osserva le derivazioni MIU.
    /// Inizialmente raccoglie solo statistiche senza influenzare la ricerca.
    /// In futuro, potrà fornire consigli basati sull'esperienza accumulata.
    /// </summary>
    public class EmergingProcesses
    {
        // Contatori per le statistiche di base
        private int _totalSolutionsFound;
        private int _totalSearchesFailed;
        private Dictionary<int, int> _ruleApplicationCounts; // Quante volte ogni regola è stata applicata globalmente (chiave int)
        private Dictionary<int, double> _ruleEffectivenessScores; // Punteggio di efficacia per ogni regola (es. quante volte ha portato a un successo) (chiave int)
        private Dictionary<string, Dictionary<int, int>> _parentChildRuleCounts; // stringa_parent -> (regola_id -> count_di_next_string) (chiave regola int)
        private Dictionary<string, int> _depthReachedCounts; // stringa_compressa -> profondità massima raggiunta

        // Una mappa per tenere traccia delle transizioni che hanno fatto parte di un percorso di successo
        // Chiave: stringa compressa del genitore
        // Valore: Dictionary<int, int> dove la chiave è RuleID e il valore è il conteggio di volte che quella regola
        // ha portato a un figlio che era su un percorso di successo.
        private Dictionary<string, Dictionary<int, int>> _successfulTransitions;

        public EmergingProcesses()
        {
            _totalSolutionsFound = 0;
            _totalSearchesFailed = 0;
            _ruleApplicationCounts = new Dictionary<int, int>();
            _ruleEffectivenessScores = new Dictionary<int, double>();
            _parentChildRuleCounts = new Dictionary<string, Dictionary<int, int>>();
            _depthReachedCounts = new Dictionary<string, int>();
            _successfulTransitions = new Dictionary<string, Dictionary<int, int>>();
        }

        /// <summary>
        /// Inizializza l'Advisor sottoscrivendosi agli eventi del RegoleMIUManager.
        /// </summary>
        public void Initialize()
        {
            RegoleMIUManager.OnSolutionFound += HandleSolutionFound;
            RegoleMIUManager.OnRuleApplied += HandleRuleApplied;
            Console.WriteLine("EmergingProcesses: Sottoscritto agli eventi di RegoleMIUManager.");
        }

        /// <summary>
        /// De-inizializza l'Advisor annullando la sottoscrizione agli eventi.
        /// Utile per evitare memory leak se l'istanza viene distrutta.
        /// </summary>
        public void Deinitialize()
        {
            RegoleMIUManager.OnSolutionFound -= HandleSolutionFound;
            RegoleMIUManager.OnRuleApplied -= HandleRuleApplied;
            Console.WriteLine("EmergingProcesses: Annullata sottoscrizione agli eventi di RegoleMIUManager.");
        }

        /// <summary>
        /// Gestisce l'evento OnSolutionFound, aggiornando le statistiche sulle ricerche
        /// e rinforzando le regole lungo il percorso di successo.
        /// </summary>
        private void HandleSolutionFound(object sender, SolutionFoundEventArgs e)
        {
            if (e.Success)
            {
                _totalSolutionsFound++;
                Console.WriteLine($"EmergingProcesses: Soluzione trovata per {e.InitialString} -> {e.TargetString}. Totale soluzioni: {_totalSolutionsFound}");

                // Logica per analizzare il percorso trovato (e.Path) e aggiornare le statistiche delle regole
                // che hanno contribuito a quel successo. Questo richiede un'analisi più approfondita del percorso.
                // Per ora, l'incremento dell'efficacia avviene in HandleRuleApplied.
                // Qui potremmo implementare un rinforzo specifico per le regole nel percorso di successo.
            }
            else
            {
                _totalSearchesFailed++;
                Console.WriteLine($"EmergingProcesses: Ricerca fallita per {e.InitialString} -> {e.TargetString}. Totale fallimenti: {_totalSearchesFailed}");
            }
        }

        /// <summary>
        /// Gestisce l'evento OnRuleApplied, aggiornando le statistiche sull'applicazione delle regole.
        /// </summary>
        private void HandleRuleApplied(object sender, RuleAppliedEventArgs e)
        {
            // Aggiorna il conteggio generale di applicazione per la regola
            if (_ruleApplicationCounts.ContainsKey(e.AppliedRuleID))
            {
                _ruleApplicationCounts[e.AppliedRuleID]++;
            }
            else
            {
                _ruleApplicationCounts[e.AppliedRuleID] = 1;
            }

            // Aggiorna le statistiche per la transizione parent -> child tramite la regola
            if (!_parentChildRuleCounts.ContainsKey(e.ParentString))
            {
                _parentChildRuleCounts[e.ParentString] = new Dictionary<int, int>();
            }
            if (_parentChildRuleCounts[e.ParentString].ContainsKey(e.AppliedRuleID))
            {
                _parentChildRuleCounts[e.ParentString][e.AppliedRuleID]++;
            }
            else
            {
                _parentChildRuleCounts[e.ParentString][e.AppliedRuleID] = 1;
            }

            // Aggiorna la profondità massima raggiunta per una stringa specifica
            if (_depthReachedCounts.ContainsKey(e.NewString))
            {
                _depthReachedCounts[e.NewString] = Math.Max(_depthReachedCounts[e.NewString], e.CurrentDepth);
            }
            else
            {
                _depthReachedCounts[e.NewString] = e.CurrentDepth;
            }

            // Inizializza o incrementa il punteggio di efficacia per la regola applicata.
            // Questo è il punto in cui la regola "guadagna" esperienza.
            if (_ruleEffectivenessScores.ContainsKey(e.AppliedRuleID))
            {
                _ruleEffectivenessScores[e.AppliedRuleID] += 1.0; // Ogni applicazione è un'esperienza
            }
            else
            {
                _ruleEffectivenessScores[e.AppliedRuleID] = 1.0;
            }
            // Console.WriteLine($"EmergingProcesses: Regola '{e.AppliedRuleName}' applicata da '{e.ParentString}' a '{e.NewString}' (Profondità: {e.CurrentDepth})");
        }

        /// <summary>
        /// Restituisce un riepilogo delle statistiche raccolte.
        /// </summary>
        public string GetStatisticsSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- EmergingProcesses Statistiche ---");
            sb.AppendLine($"Soluzioni Trovate: {_totalSolutionsFound}");
            sb.AppendLine($"Ricerche Fallite: {_totalSearchesFailed}");
            sb.AppendLine("\nConteggio Applicazioni Regole:");
            foreach (var entry in _ruleApplicationCounts.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"  {entry.Key} ({RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == entry.Key)?.Nome ?? "Sconosciuta"}): {entry.Value} volte");
            }
            sb.AppendLine("\nPunteggio Efficacia Regole:");
            foreach (var entry in _ruleEffectivenessScores.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"  {entry.Key} ({RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == entry.Key)?.Nome ?? "Sconosciuta"}): {entry.Value:F2}");
            }
            sb.AppendLine("\n--- Fine Statistiche ---");
            return sb.ToString();
        }

        /// <summary>
        /// Fornisce un ordine preferenziale delle regole basato sull'esperienza accumulata.
        /// Man mano che l'advisor apprende, l'ordine diventerà più "intelligente".
        /// </summary>
        /// <param name="currentCompressedString">La stringa compressa corrente.</param>
        /// <param name="currentDepth">La profondità corrente nella ricerca.</param>
        /// <returns>Una lista di regole MIU ordinate per preferenza.</returns>
        public List<RegolaMIU> GetPreferredRuleOrder(string currentCompressedString, int currentDepth)
        {
            // Ottieni tutte le regole disponibili
            List<RegolaMIU> allRules = RegoleMIUManager.Regole.ToList();

            // Ordina le regole in base al loro punteggio di efficacia.
            // Le regole con punteggio più alto (più "esperte" e "efficaci") vengono prima.
            // Le regole non ancora presenti in _ruleEffectivenessScores (nuove o non ancora applicate)
            // avranno un punteggio di 0.0 per default e andranno in coda, ma il ToList() iniziale
            // mantiene l'ordine originale per quelle con punteggio 0.
            return allRules.OrderByDescending(rule => {
                if (_ruleEffectivenessScores.ContainsKey(rule.ID))
                {
                    return _ruleEffectivenessScores[rule.ID];
                }
                return 0.0; // Punteggio base per regole non ancora applicate o senza esperienza
            }).ToList();
        }

        /// <summary>
        /// Determina se una specifica applicazione di regola dovrebbe essere persistita nel database
        /// per l'analisi dettagliata. Inizialmente restituisce sempre true per acquisire dati.
        /// In futuro, questa logica si evolverà per essere più selettiva.
        /// </summary>
        /// <param name="parentString">La stringa compressa del genitore.</param>
        /// <param name="newString">La stringa compressa generata.</param>
        /// <param name="appliedRuleId">L'ID della regola applicata.</param>
        /// <param name="currentDepth">La profondità corrente dell'applicazione.</param>
        /// <returns>True se l'applicazione dovrebbe essere persistita, false altrimenti.</returns>
        public bool ShouldPersistRuleApplication(string parentString, string newString, int appliedRuleId, int currentDepth)
        {
            // Logica iniziale: registra sempre per acquisire esperienza.
            // In futuro, qui potremmo implementare:
            // - Non loggare se (parent, rule, newString) è già stato visto molte volte e non ha portato a nuovi insight.
            // - Loggare solo se la transizione è "nuova" o "insolita".
            // - Loggare con una certa probabilità basata sull'efficacia della regola o la novità dello stato.
            return true;
        }
    }

    /// <summary>
    /// Fornisce dati per l'evento OnSolutionFound.
    /// Contiene i dettagli completi del risultato di una ricerca di derivazione.
    /// </summary>
    public class SolutionFoundEventArgs : EventArgs
    {
        public string InitialString { get; }
        public string TargetString { get; }
        public bool Success { get; }
        //public List<string> Path { get; } // Il percorso trovato, null se non trovata
        public List<(string CompressedString, int? AppliedRuleID)> Path { get; }
        public long ElapsedTicks { get; }
        public double ElapsedMilliseconds { get; }
        public int StepsTaken { get; } // Numero di passi nella soluzione (profondità del target)
        public int NodesExplored { get; } // Numero totale di nodi visitati
        public int MaxDepthReached { get; } // Profondità massima raggiunta nella ricerca
        public string SearchAlgorithm { get; } //2025.06.04  Aggiunto per MIU_Searches
        public SolutionFoundEventArgs(string initialString, string targetString, bool success, List<(string CompressedString, int? AppliedRuleID)> path, long elapsedTicks, int stepsTaken, int nodesExplored, int maxDepthReached, string searchAlgorithm)
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
            SearchAlgorithm = searchAlgorithm; // 2025.06.04 Aggiunto per MIU_Searches
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
        public int AppliedRuleID { get; } // L'ID della regola applicata
        public string AppliedRuleName { get; } // Il nome della regola applicata
        public int CurrentDepth { get; } // Profondità della nuova stringa nel percorso di ricerca

        public RuleAppliedEventArgs(string parentString, string newString, int appliedRuleID, string appliedRuleName, int currentDepth)
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

        // Istanza di EmergingProcesses che influenza l'ordine delle regole
        private static EmergingProcesses _learningAdvisor = new EmergingProcesses();
        // Contatore globale per i nodi esplorati, resettato all'inizio di ogni ricerca
        private static int _globalNodesExplored;

        // -------------------------------------------------------------------
        // 2. Definizione degli eventi (rimangono invariati)
        // -------------------------------------------------------------------

        public static event EventHandler<SolutionFoundEventArgs> OnSolutionFound;
        public static event EventHandler<RuleAppliedEventArgs> OnRuleApplied;


        /// <summary>
        /// Espone l'istanza di EmergingProcesses per l'accesso esterno (es. da Program.cs).
        /// </summary>
        /// <returns>L'istanza di EmergingProcesses.</returns>
        public static EmergingProcesses GetLearningAdvisor()
        {
            return _learningAdvisor;
        }

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
                    int id = int.Parse(regolaParts[0]);
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
        public static List<(string CompressedString, int? AppliedRuleID)> TrovaDerivazioneBFS(string start, string target, long maxSteps = 100)
        {
            // Le stringhe start e target sono già attese in formato compresso.
            // Se la stringa iniziale compressa è già la target compressa, il percorso è solo la stringa stessa.
            string searchAlgorithmName = "BFS"; // Nome dell'algoritmo per il DB
            if (start == target)
            {
                var path = new List<(string CompressedString, int? AppliedRuleID)> { (start, null) }; // <-- MODIFICATA: usa la tupla
                _globalNodesExplored = 1; // <-- MODIFICATA: usa il contatore globale
                OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                    initialString: start,
                    targetString: target,
                    success: true,
                    path: path, // <-- MODIFICATA: 'path' è ora una lista di tuple
                    elapsedTicks: 0,
                    stepsTaken: 0,
                    nodesExplored: _globalNodesExplored, // <-- MODIFICATA: usa il contatore globale
                    maxDepthReached: 0,
                    searchAlgorithm: searchAlgorithmName // <-- Assicurati che sia presente
                ));
                return path;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            Queue<List<(string CompressedString, int? AppliedRuleID)>> queue = new Queue<List<(string CompressedString, int? AppliedRuleID)>>(); // <-- MODIFICATA: usa la tupla
            queue.Enqueue(new List<(string CompressedString, int? AppliedRuleID)> { (start, null) }); // <-- MODIFICATA: usa la tupla
            HashSet<string> visited = new HashSet<string> { start }; // Lascia questa riga così com'è
            List<(string CompressedString, int? AppliedRuleID)> solutionPath = null; // <-- MODIFICATA: usa la tupla
            int depth = 0;
            _globalNodesExplored = 1; // <-- MODIFICATA: usa il contatore globale


            while (queue.Count > 0 && depth < maxSteps)
            {
                int levelSize = queue.Count;
                depth++;

                for (int i = 0; i < levelSize; i++)
                {
                    List<(string CompressedString, int? AppliedRuleID)> currentPath = queue.Dequeue(); // <-- MODIFICATA: usa la tupla
                    string currentCompressedString = currentPath.Last().CompressedString; // <-- MODIFICATA: accede alla stringa dalla tupla

                    // Chiedi a EmergingProcesses l'ordine preferenziale delle regole
                    // Assicurati che _learningAdvisor sia accessibile (es. tramite GetLearningAdvisor() o rendendolo public static temporaneamente)
                    List<RegolaMIU> orderedRules = _learningAdvisor.GetPreferredRuleOrder(currentCompressedString, depth);

                    foreach (var rule in orderedRules) // Usa l'ordine suggerito dall'advisor
                    {
                        string nextCompressedString;
                        // TryApply ora accetta e restituisce stringhe compresse
                        // Rimosso il controllo !visited.Contains(nextCompressedString) da qui, lo facciamo dopo l'evento OnRuleApplied
                        if (rule.TryApply(currentCompressedString, out nextCompressedString))
                        {
                            // Scatena l'evento di regola applicata PRIMA del controllo visited
                            // per registrare ogni tentativo valido per l'apprendimento.
                            OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs(
                                parentString: currentCompressedString,
                                newString: nextCompressedString,
                                appliedRuleID: rule.ID, // Assicurati che rule.ID sia int
                                appliedRuleName: rule.Nome,
                                currentDepth: depth
                            ));

                            if (!visited.Contains(nextCompressedString)) // <-- Il controllo visited è qui
                            {
                                _globalNodesExplored++; // <-- MODIFICATA: usa il contatore globale

                                if (nextCompressedString == target)
                                {
                                    solutionPath = new List<(string CompressedString, int? AppliedRuleID)>(currentPath); // <-- MODIFICATA: usa la tupla
                                    solutionPath.Add((CompressedString: nextCompressedString, AppliedRuleID: rule.ID)); // <-- MODIFICATA: aggiunge la tupla
                                    stopwatch.Stop();

                                    OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                                        initialString: start,
                                        targetString: target,
                                        success: true,
                                        path: solutionPath, // <-- MODIFICATA: passa il nuovo tipo di path
                                        elapsedTicks: stopwatch.ElapsedTicks,
                                        stepsTaken: solutionPath.Count - 1,
                                        nodesExplored: _globalNodesExplored, // <-- MODIFICATA: usa il contatore globale
                                        maxDepthReached: depth,
                                        searchAlgorithm: searchAlgorithmName // Assicurati che sia presente
                                    ));
                                    return solutionPath;
                                }

                                visited.Add(nextCompressedString);
                                List<(string CompressedString, int? AppliedRuleID)> newPath = new List<(string CompressedString, int? AppliedRuleID)>(currentPath); // <-- MODIFICATA: usa la tupla
                                newPath.Add((CompressedString: nextCompressedString, AppliedRuleID: rule.ID)); // <-- MODIFICATA: aggiunge la tupla
                                queue.Enqueue(newPath);
                            }
                        }
                    }
                }
            }

            stopwatch.Stop(); // Ferma il cronometro
            // Scatena l'evento di soluzione non trovata
            OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                initialString: start,
                targetString: target,
                success: false,
                path: null, // Nessun percorso trovato
                elapsedTicks: stopwatch.ElapsedTicks,
                stepsTaken: (int)maxSteps, // Indica il limite di passi raggiunto
                nodesExplored: _globalNodesExplored, // <-- MODIFICATA: usa il contatore globale
                maxDepthReached: depth,
                searchAlgorithm: searchAlgorithmName // <-- AGGIUNTA: include il parametro searchAlgorithmName
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
        public static List<(string CompressedString, int? AppliedRuleID)> TrovaDerivazioneDFS(string start, string target, long maxDepth, List<(string CompressedString, int? AppliedRuleID)> path = null, HashSet<string> visited = null)
        {
            // Le stringhe start e target sono già attese in formato compresso.
            string searchAlgorithmName = "DFS"; // Lasciala se già presente, altrimenti aggiungila qui
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
                _globalNodesExplored = 0; // <-- AGGIUNTA: Reset del contatore globale per la chiamata iniziale
                path = new List<(string CompressedString, int? AppliedRuleID)> { (currentStart, null) }; // <-- MODIFICATA: usa la tupla
                visited = new HashSet<string> { currentStart };
                _globalNodesExplored = 1; // <-- MODIFICATA: il nodo iniziale è esplorato
            }
            //else
            //{
            //    // Se non è la chiamata iniziale, visited.Count riflette i nodi già esplorati nel ramo
            //    nodesExplored = visited.Count;
            //}

            string currentCompressedString = path.Last().CompressedString; // <-- MODIFICATA: accede alla stringa dalla tupla

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
                    nodesExplored: _globalNodesExplored, // <-- MODIFICATA: usa il contatore globale
                    maxDepthReached: path.Count - 1,
                    searchAlgorithm: searchAlgorithmName // <-- AGGIUNTA: include il parametro searchAlgorithmName
                ));
                return path;
            }

            if (path.Count - 1 >= maxDepth)
            {
                return null;
            }

            foreach (var rule in Regole) // Usa l'ordine suggerito dall'advisor
            {
                string nextCompressedString;
                // TryApply ora accetta e restituisce stringhe compresse
                // Rimosso il controllo !visited.Contains(nextCompressedString) da qui, lo facciamo dopo l'evento OnRuleApplied
                if (rule.TryApply(currentCompressedString, out nextCompressedString))
                {
                    // Scatena l'evento di regola applicata
                    OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs(
                        parentString: currentCompressedString,
                        newString: nextCompressedString,
                        appliedRuleID: rule.ID, // Assicurati che rule.ID sia int
                        appliedRuleName: rule.Nome,
                        currentDepth: path.Count - 1
                    ));

                    if (!visited.Contains(nextCompressedString)) // <-- Il controllo visited è qui
                    {
                        visited.Add(nextCompressedString);
                        _globalNodesExplored++; // <-- MODIFICATA: Incrementa il contatore globale
                        path.Add((CompressedString: nextCompressedString, AppliedRuleID: rule.ID)); // <-- MODIFICATA: aggiunge la tupla

                        // La chiamata ricorsiva deve usare il nuovo tipo di path
                        List<(string CompressedString, int? AppliedRuleID)> result = TrovaDerivazioneDFS(currentStart, currentTarget, maxDepth, path, visited);
                        if (result != null)
                        {
                            stopwatch.Stop();
                            if (!soluzioneTrovata) // Registra il tempo solo alla prima soluzione trovata
                            {
                                elapsedTicks = stopwatch.ElapsedTicks;
                                double tempoMs = (elapsedTicks / frequency) * 1000;
                                numeroPassi = result.Count - 1;
                                soluzioneTrovata = true;
                                Console.WriteLine($"\nSoluzione DFS trovata (profondità {numeroPassi}) in {tempoMs:F2} ms (al ritorno dalla ricorsione, stringa compressa):");
                                foreach (var s in result)
                                {
                                    Console.WriteLine($"  {s.CompressedString}"); // Stampa solo la stringa compressa
                                }
                                // Scatena l'evento di soluzione trovata (al ritorno dalla ricorsione)
                                OnSolutionFound?.Invoke(null, new SolutionFoundEventArgs(
                                    initialString: currentStart,
                                    targetString: currentTarget,
                                    success: true,
                                    path: result, // <-- MODIFICATA: Il percorso è in formato compresso (tupla)
                                    elapsedTicks: elapsedTicks,
                                    stepsTaken: numeroPassi,
                                    nodesExplored: _globalNodesExplored, // <-- MODIFICATA: usa il contatore globale
                                    maxDepthReached: result.Count - 1,
                                    searchAlgorithm: searchAlgorithmName // Assicurati che sia presente
                                ));
                            }
                            return result;
                        }
                        path.RemoveAt(path.Count - 1); // Backtrack
                        visited.Remove(nextCompressedString); // Permetti di visitare di nuovo in altri rami
                    }
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
                    nodesExplored: _globalNodesExplored, // <-- MODIFICATA: usa il contatore globale
                    maxDepthReached: (int)maxDepth,
                    searchAlgorithm: searchAlgorithmName // <-- AGGIUNTA: include il parametro searchAlgorithmName
                ));
            }

            return null;
        }
    }
}
