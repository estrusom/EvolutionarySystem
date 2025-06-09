using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions; // Necessario per RegolaMIU.TryApply
using System.Threading.Tasks;
using System.Xml.Serialization;
using MIU.Core.Learning.Interfaces;

// 2025.05.24 aggiunto tempo ms 
// 2025.05.26 Modificato inserimento record con l'aggiornamento dell'indice
// 2025.06.06 Rimosso poichè fa casino cion le transazioni
namespace MIU.Core
{

    /// <summary>
    /// Rappresenta la meta-struttura di apprendimento che osserva le derivazioni MIU.
    /// Raccoglie statistiche e, in futuro, influenzerà la ricerca basandosi sull'esperienza accumulata.
    /// Ora include la capacità di salvare e caricare il proprio stato tramite un'interfaccia.
    /// </summary>
    public class EmergingProcesses : ILearningAdvisor
    {
        // Contatori per le statistiche di base dell'apprendimento
        private int _totalSolutionsFound;
        private int _totalSearchesFailed;
        // Chiave: RuleID (int), Valore: Conteggio applicazioni
        private Dictionary<int, int> _ruleApplicationCounts;
        // Chiave: RuleID (int), Valore: Punteggio di efficacia (double)
        private Dictionary<int, double> _ruleEffectivenessScores;
        // Stringa compressa del genitore -> (RuleID -> conteggio di stringhe figlie generate con quella regola)
        private Dictionary<string, Dictionary<int, int>> _parentChildRuleCounts;
        // Stringa compressa -> Profondità massima raggiunta per quella stringa
        private Dictionary<string, int> _depthReachedCounts;
        // Mappa per tenere traccia delle transizioni che hanno fatto parte di un percorso di successo
        // Chiave: stringa compressa del genitore, Valore: Dictionary<int, int> (RuleID -> conteggio successi)
        private Dictionary<string, Dictionary<int, int>> _successfulTransitions;

        // CAMBIATO: Ora dipende dall'interfaccia ILearningStatePersistence!
        private readonly ILearningStatePersistence _learningStatePersistence;

        /// <summary>
        /// Inizializza una nuova istanza di EmergingProcesses con un riferimento al meccanismo di persistenza.
        /// </summary>
        /// <param name="learningStatePersistence">L'istanza dell'interfaccia ILearningStatePersistence da utilizzare per la persistenza.</param>
        public EmergingProcesses(ILearningStatePersistence learningStatePersistence)
        {
            _learningStatePersistence = learningStatePersistence ?? throw new ArgumentNullException(nameof(learningStatePersistence));

            // Inizializzazione delle strutture dati per l'apprendimento
            _totalSolutionsFound = 0;
            _totalSearchesFailed = 0;
            _ruleApplicationCounts = new Dictionary<int, int>();
            _ruleEffectivenessScores = new Dictionary<int, double>();
            _parentChildRuleCounts = new Dictionary<string, Dictionary<int, int>>();
            _depthReachedCounts = new Dictionary<string, int>();
            _successfulTransitions = new Dictionary<string, Dictionary<int, int>>();
        }

        /// <summary>
        /// Sottoscrive l'Advisor agli eventi di RegoleMIUManager e carica lo stato di apprendimento dal database.
        /// Questo metodo viene chiamato una volta dopo l'inizializzazione del repository.
        /// </summary>
        public void Initialize()
        {
            RegoleMIUManager.OnSolutionFound += HandleSolutionFound;
            RegoleMIUManager.OnRuleApplied += HandleRuleApplied;
            Console.WriteLine("EmergingProcesses: Sottoscritto agli eventi di RegoleMIUManager.");

            // Carica lo stato di apprendimento esistente dal database all'avvio
            LoadState();
        }

        /// <summary>
        /// De-inizializza l'Advisor annullando la sottoscrizione agli eventi e salvando lo stato.
        /// Utile per evitare memory leak e per garantire la persistenza finale.
        /// </summary>
        public void Deinitialize()
        {
            RegoleMIUManager.OnSolutionFound -= HandleSolutionFound;
            RegoleMIUManager.OnRuleApplied -= HandleRuleApplied;
            Console.WriteLine("EmergingProcesses: Annullata sottoscrizione agli eventi di RegoleMIUManager.");

            // Salva lo stato di apprendimento aggiornato nel database alla chiusura
            SaveState();
        }

        /// <summary>
        /// Carica lo stato di apprendimento (statistiche delle regole, ecc.) dal database.
        /// Questo ripristina la "memoria" del sistema da esecuzioni precedenti.
        /// </summary>
        public void LoadState()
        {
            Console.WriteLine("EmergingProcesses: Tentativo di caricamento dello stato di apprendimento dal database...");
            try
            {
                var ruleStats = _learningStatePersistence.LoadRuleStatistics(); // Usa l'interfaccia
                foreach (var stat in ruleStats)
                {
                    _ruleApplicationCounts[stat.RuleID] = stat.ApplicationCount;
                    _ruleEffectivenessScores[stat.RuleID] = stat.EffectivenessScore;
                }

                var transitionStats = _learningStatePersistence.LoadTransitionStatistics(); // Usa l'interfaccia
                foreach (var stat in transitionStats)
                {
                    if (!_parentChildRuleCounts.ContainsKey(stat.ParentStringCompressed))
                    {
                        _parentChildRuleCounts[stat.ParentStringCompressed] = new Dictionary<int, int>();
                    }
                    _parentChildRuleCounts[stat.ParentStringCompressed][stat.AppliedRuleID] = stat.ApplicationCount;

                    if (stat.SuccessfulCount > 0)
                    {
                        if (!_successfulTransitions.ContainsKey(stat.ParentStringCompressed))
                        {
                            _successfulTransitions[stat.ParentStringCompressed] = new Dictionary<int, int>();
                        }
                        _successfulTransitions[stat.ParentStringCompressed][stat.AppliedRuleID] = stat.SuccessfulCount;
                    }
                }

                Console.WriteLine($"EmergingProcesses: Stato di apprendimento caricato con successo. Caricate {ruleStats.Count} statistiche regole e {transitionStats.Count} statistiche transizioni.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EmergingProcesses: Errore durante il caricamento dello stato di apprendimento: {ex.Message}");
                // Inizializza a zero in caso di errore per evitare stati nulli
                _totalSolutionsFound = 0;
                _totalSearchesFailed = 0;
                _ruleApplicationCounts.Clear();
                _ruleEffectivenessScores.Clear();
                _parentChildRuleCounts.Clear();
                _depthReachedCounts.Clear();
                _successfulTransitions.Clear();
            }
        }

        /// <summary>
        /// Salva lo stato di apprendimento corrente (statistiche delle regole, ecc.) nel database.
        /// Questo assicura che l'esperienza accumulata venga preservata per esecuzioni future.
        /// </summary>
        public void SaveState()
        {
            Console.WriteLine("EmergingProcesses: Tentativo di salvataggio dello stato di apprendimento nel database...");
            try
            {
                // Salva le statistiche delle regole
                var ruleStatsToSave = _ruleApplicationCounts.Select(kvp => new RuleStatistics
                {
                    RuleID = kvp.Key,
                    ApplicationCount = kvp.Value,
                    EffectivenessScore = _ruleEffectivenessScores.ContainsKey(kvp.Key) ? _ruleEffectivenessScores[kvp.Key] : 0.0,
                    LastUpdated = DateTime.UtcNow.ToString("o")
                }).ToList();
                _learningStatePersistence.SaveRuleStatistics(ruleStatsToSave); // Usa l'interfaccia

                // Salva le statistiche delle transizioni
                var transitionStatsToSave = new List<TransitionStatistics>();
                foreach (var parentEntry in _parentChildRuleCounts)
                {
                    string parentString = parentEntry.Key;
                    foreach (var ruleEntry in parentEntry.Value)
                    {
                        int ruleId = ruleEntry.Key;
                        int appCount = ruleEntry.Value;
                        int successCount = _successfulTransitions.ContainsKey(parentString) && _successfulTransitions[parentString].ContainsKey(ruleId)
                            ? _successfulTransitions[parentString][ruleId]
                            : 0;

                        transitionStatsToSave.Add(new TransitionStatistics
                        {
                            ParentStringCompressed = parentString,
                            AppliedRuleID = ruleId,
                            ApplicationCount = appCount,
                            SuccessfulCount = successCount,
                            LastUpdated = DateTime.UtcNow.ToString("o")
                        });
                    }
                }
                _learningStatePersistence.SaveTransitionStatistics(transitionStatsToSave); // Usa l'interfaccia

                Console.WriteLine("EmergingProcesses: Stato di apprendimento salvato con successo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EmergingProcesses: Errore durante il salvataggio dello stato di apprendimento: {ex.Message}");
            }
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

                // Rinforzo delle regole lungo il percorso di successo
                if (e.Path != null && e.Path.Count > 1)
                {
                    for (int i = 1; i < e.Path.Count; i++) // Inizia dal secondo elemento, perché il primo è la stringa iniziale (senza regola applicata per raggiungerla)
                    {
                        string parentString = e.Path[i - 1].CompressedString;
                        int? appliedRuleID = e.Path[i].AppliedRuleID;

                        if (appliedRuleID.HasValue)
                        {
                            // Incrementa il conteggio dei successi per questa transizione (parent, rule)
                            if (!_successfulTransitions.ContainsKey(parentString))
                            {
                                _successfulTransitions[parentString] = new Dictionary<int, int>();
                            }
                            if (_successfulTransitions[parentString].ContainsKey(appliedRuleID.Value))
                            {
                                _successfulTransitions[parentString][appliedRuleID.Value]++;
                            }
                            else
                            {
                                _successfulTransitions[parentString][appliedRuleID.Value] = 1;
                            }

                            // Potremmo anche aumentare il punteggio di efficacia della regola in modo più significativo qui
                            // rispetto a una semplice applicazione. Esempio: +10 per un successo nel percorso.
                            if (_ruleEffectivenessScores.ContainsKey(appliedRuleID.Value))
                            {
                                _ruleEffectivenessScores[appliedRuleID.Value] += 10.0; // Punti bonus per successo
                            }
                            else
                            {
                                _ruleEffectivenessScores[appliedRuleID.Value] = 10.0;
                            }
                        }
                    }
                }
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
            // Ogni applicazione è un'esperienza. Punti "base" per l'applicazione.
            if (_ruleEffectivenessScores.ContainsKey(e.AppliedRuleID))
            {
                _ruleEffectivenessScores[e.AppliedRuleID] += 1.0;
            }
            else
            {
                _ruleEffectivenessScores[e.AppliedRuleID] = 1.0;
            }
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
            sb.AppendLine("\nConteggio Applicazioni Regole (totale):");
            foreach (var entry in _ruleApplicationCounts.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"  {entry.Key} ({RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == entry.Key)?.Nome ?? "Sconosciuta"}): {entry.Value} volte");
            }
            sb.AppendLine("\nPunteggio Efficacia Regole (accumulato):");
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
        /// Le regole con punteggio di efficacia più alto vengono prioritarie.
        /// </summary>
        /// <param name="currentCompressedString">La stringa compressa corrente.</param>
        /// <param name="currentDepth">La profondità corrente nella ricerca.</param>
        /// <returns>Una lista di regole MIU ordinate per preferenza.</returns>
        public List<RegolaMIU> GetPreferredRuleOrder(string currentCompressedString, int currentDepth)
        {
            List<RegolaMIU> allRules = RegoleMIUManager.Regole.ToList();

            // Ordina le regole: prima quelle con un'alta efficacia, poi quelle con meno esperienza.
            return allRules.OrderByDescending(rule => {
                // Calcola il punteggio combinato (es. efficacia + un bonus per le transizioni di successo)
                double score = 0.0;

                // Punteggio base di efficacia della regola
                if (_ruleEffectivenessScores.ContainsKey(rule.ID))
                {
                    score += _ruleEffectivenessScores[rule.ID];
                }

                // Aggiungi un bonus se questa regola ha portato a successi specifici da questa stringa genitore
                if (_successfulTransitions.ContainsKey(currentCompressedString) &&
                    _successfulTransitions[currentCompressedString].ContainsKey(rule.ID))
                {
                    // Più successi = punteggio più alto
                    score += _successfulTransitions[currentCompressedString][rule.ID] * 50.0; // Peso maggiore per i successi diretti
                }

                return score;
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
            // In futuro, qui potremmo implementare filtri più intelligenti basati sulla "rilevanza" o "novità" della transizione.
            return true;
        }

        /// <summary>
        /// Espone il conteggio totale dei nodi esplorati dall'ultima ricerca.
        /// </summary>
        /// <returns>Il conteggio totale dei nodi esplorati.</returns>
        public int GetTotalNodesExplored()
        {
            // Questa variabile è statica in RegoleMIUManager, quindi la leggiamo direttamente da lì.
            return RegoleMIUManager._globalNodesExplored;
        }

        void ILearningAdvisor.NotifyRuleApplied(string parentStringCompressed, string newStringCompressed, int appliedRuleID, int currentDepth, bool isSuccessPath)
        {
            // Troviamo il nome della regola usando l'ID
            // Assicurati che RegoleMIUManager.Regole sia accessibile.
            // Se RegoleMIUManager.Regole è null o vuoto, il nome sarà "Sconosciuta".
            string appliedRuleName = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == appliedRuleID)?.Nome ?? "Sconosciuta";

            // Creiamo un'istanza di RuleAppliedEventArgs con l'ordine corretto dei parametri,
            // inclusi parentStringCompressed, newStringCompressed, il nome della regola e isSuccessPath.
            var args = new RuleAppliedEventArgs(
                parentString: parentStringCompressed,       // 1° parametro (stringa)
                newString: newStringCompressed,             // 2° parametro (stringa)
                appliedRuleID: appliedRuleID,               // 3° parametro (int)
                appliedRuleName: appliedRuleName,           // 4° parametro (stringa)
                currentDepth: currentDepth,                 // 5° parametro (int)
                isSuccessPath: isSuccessPath                // 6° parametro (bool)
            );

            // Chiamiamo il metodo privato esistente HandleRuleApplied con l'istanza di EventArgs.
            HandleRuleApplied(this, args);
        }

        void ILearningAdvisor.Initialize()
        {
            Initialize();
        }

        List<RegolaMIU> ILearningAdvisor.GetPreferredRuleOrder(string currentCompressedString, int currentDepth)
        {
            // ATTENZIONE: Questo è un placeholder!
            // In una vera implementazione, la logica di EmergingProcesses qui
            // ordinerebbe le RegoleMIU in base a pattern di apprendimento.
            // Per ora, restituisce tutte le regole disponibili dall'RegoleMIUManager
            // in un ordine semplice (es. per ID) per evitare la NotImplementedException.
            return RegoleMIUManager.Regole.OrderBy(r => r.ID).ToList();
        }

        void ILearningAdvisor.Deinitialize()
        {
            Deinitialize();
        }

        string ILearningAdvisor.GetStatisticsSummary()
        {
            // ATTENZIONE: Questo è un placeholder!
            // In una vera implementazione, restituirebbe un riepilogo delle statistiche di apprendimento.
            return "Riepilogo statistiche di apprendimento: [Implementazione in corso].";
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
        public List<(string CompressedString, int? AppliedRuleID)> Path { get; } // MODIFICA QUI
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
        public bool IsSuccessPath { get; } // <-- NUOVA PROPRIETÀ AGGIUNTA QUI

        // MODIFICA IL COSTRUTTORE QUI SOTTO:
        public RuleAppliedEventArgs(string parentString, string newString, int appliedRuleID, string appliedRuleName, int currentDepth, bool isSuccessPath) // <-- AGGIUNTO isSuccessPath
        {
            ParentString = parentString;
            NewString = newString;
            AppliedRuleID = appliedRuleID;
            AppliedRuleName = appliedRuleName;
            CurrentDepth = currentDepth;
            IsSuccessPath = isSuccessPath; // <-- ASSEGNAZIONE NUOVA
        }
    }

    public static partial class RegoleMIUManager
    {
        private static List<RegolaMIU> _regole = new List<RegolaMIU>();
        public static IReadOnlyList<RegolaMIU> Regole => _regole.AsReadOnly();

        // Istanza di EmergingProcesses che influenza l'ordine delle regole
        //private static EmergingProcesses _learningAdvisor = new EmergingProcesses();
        //private static EmergingProcesses _learningAdvisor; 
        private static ILearningAdvisor _learningAdvisor;
        // Contatore globale per i nodi esplorati, resettato all'inizio di ogni ricerca
        // private static int _globalNodesExplored; 2025.06.06 Modi
        public static int _globalNodesExplored;

        // -------------------------------------------------------------------
        // 2. Definizione degli eventi (rimangono invariati)
        // -------------------------------------------------------------------

        public static event EventHandler<SolutionFoundEventArgs> OnSolutionFound;
        public static event EventHandler<RuleAppliedEventArgs> OnRuleApplied;

        /// <summary>
        /// Imposta l'istanza di ILearningAdvisor che il manager utilizzerà.
        /// Questo metodo DEVE essere chiamato all'avvio dell'applicazione per inizializzare l'advisor.
        /// </summary>
        /// <param name="advisor">L'istanza dell'advisor di apprendimento da utilizzare.</param>
        public static void SetLearningAdvisor(ILearningAdvisor advisor)
        {
            if (advisor == null)
            {
                throw new ArgumentNullException(nameof(advisor), "L'istanza di ILearningAdvisor non può essere null.");
            }
            _learningAdvisor = advisor;
            Console.WriteLine("RegoleMIUManager: Learning Advisor impostato.");
        }

        /// <summary>
        /// Scatena l'evento OnSolutionFound con i dati della soluzione.
        /// Questo metodo è un helper per Program.cs per notificare le soluzioni.
        /// </summary>
        public static void NotificaSoluzioneTrovata(SolutionFoundEventArgs args)
        {
            OnSolutionFound?.Invoke(null, args);
        }

        /// <summary>
        /// Espone l'istanza di EmergingProcesses per l'accesso esterno (es. da Program.cs).
        /// </summary>
        /// <returns>L'istanza di EmergingProcesses.</returns>
        public static ILearningAdvisor GetLearningAdvisor()
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
                //string compressedCurrent = MIUStringConverter.InflateMIUString(currentString);
                string compressedCurrent = MIUStringConverter.DeflateMIUString(currentString);
                string compressedNew;

                // TryApply ora accetta e restituisce stringhe compresse.
                if (rule.TryApply(compressedCurrent, out compressedNew))
                {
                    //currentString = MIUStringConverter.DeflateMIUString(compressedNew); // Decomprimi per la visualizzazioneù
                    currentString = MIUStringConverter.InflateMIUString(compressedNew); // Decomprimi per la visualizzazione
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
        /// L'ordine di applicazione delle regole è influenzato da EmergingProcesses.
        /// </summary>
        /// <param name="start">La stringa iniziale (formato compresso).</param>
        /// <param name="target">La stringa target (formato compresso).</param>
        /// <param name="maxSteps">La profondità massima della ricerca per evitare cicli infiniti.</param>
        /// <returns>Una lista di stringhe che rappresentano il percorso di derivazione (formato compresso),
        /// o null se la stringa target non viene raggiunta entro la profondità massima.</returns>
        public static List<(string CompressedString, int? AppliedRuleID)> TrovaDerivazioneBFS(string start, string target, long maxSteps = 100)
        {
            // Le stringhe start e target sono già attese in formato compresso.
            string searchAlgorithmName = "BFS"; // Nome dell'algoritmo per il DB
            _globalNodesExplored = 0; // Reset del contatore globale per ogni nuova ricerca

            if (start == target)
            {
                // Il percorso iniziale ha la stringa di partenza e un RuleID null (nessuna regola applicata per raggiungerla)
                var path = new List<(string CompressedString, int? AppliedRuleID)> { (start, null) };
                _globalNodesExplored = 1; // Un nodo esplorato
                // RIMOSSA: OnSolutionFound?.Invoke(...)
                return path;
            }
                
            // RIMOSSA: Stopwatch stopwatch = Stopwatch.StartNew(); // Avvia il cronometro per misurare il tempo di esecuzione
            // Coda per la BFS, memorizza i percorsi come liste di tuple (stringa compressa, RuleID)
            Queue<List<(string CompressedString, int? AppliedRuleID)>> queue = new Queue<List<(string CompressedString, int? AppliedRuleID)>>();
            queue.Enqueue(new List<(string CompressedString, int? AppliedRuleID)> { (start, null) }); // Aggiungi il percorso iniziale alla coda
            HashSet<string> visited = new HashSet<string> { start }; // Set per tenere traccia delle stringhe già visitate (solo stringhe compresse)
            List<(string CompressedString, int? AppliedRuleID)> solutionPath = null; // Il percorso della soluzione, se trovato
            int depth = 0; // Profondità corrente della ricerca
            _globalNodesExplored = 1; // Contatore globale per i nodi esplorati (inizia con la stringa di partenza)

            // Loop principale della BFS
            while (queue.Count > 0 && depth < maxSteps)
            {
                int levelSize = queue.Count; // Numero di nodi a questo livello
                depth++; // Incrementa la profondità per il prossimo livello

                // Esplora tutti i nodi a questo livello
                for (int i = 0; i < levelSize; i++)
                {
                    List<(string CompressedString, int? AppliedRuleID)> currentPath = queue.Dequeue(); // Preleva il percorso corrente dalla coda
                    string currentCompressedString = currentPath.Last().CompressedString; // L'ultima stringa nel percorso è lo stato corrente
                    
                    // Chiedi a EmergingProcesses l'ordine preferenziale delle regole
                    List<RegolaMIU> orderedRules = _learningAdvisor.GetPreferredRuleOrder(currentCompressedString, depth);

                    foreach (var rule in orderedRules) // Usa l'ordine suggerito dall'advisor
                    {
                        string nextCompressedString;
                        // TryApply ora accetta e restituisce stringhe compresse
                        if (rule.TryApply(currentCompressedString, out nextCompressedString))
                        {
                            // Scatena l'evento di regola applicata PRIMA del controllo visited per registrare ogni tentativo valido
                            // Scatena l'evento di regola applicata PRIMA del controllo visited per registrare ogni tentativo valido
                            OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs(
                                parentString: currentCompressedString,
                                newString: nextCompressedString, // <-- CORRETTO: Usa 'nextCompressedString'
                                appliedRuleID: rule.ID,
                                appliedRuleName: rule.Nome,
                                currentDepth: depth, // <-- CORRETTO: Usa la variabile 'depth'
                                isSuccessPath: false
                            ));

                            if (!visited.Contains(nextCompressedString)) // Controlla visited qui per la logica BFS
                            {
                                _globalNodesExplored++; // Incrementa il contatore globale

                                if (nextCompressedString == target)
                                {
                                    solutionPath = new List<(string CompressedString, int? AppliedRuleID)>(currentPath); // Copia il percorso corrente
                                    solutionPath.Add((CompressedString: nextCompressedString, AppliedRuleID: rule.ID)); // Aggiungi la stringa target e la regola al percorso
                                    // RIMOSSA: stopwatch.Stop(); // Ferma il cronometro

                                    // RIMOSSA: OnSolutionFound?.Invoke(...)
                                    return solutionPath; // Restituisci il percorso trovato
                                }

                                visited.Add(nextCompressedString);
                                List<(string CompressedString, int? AppliedRuleID)> newPath = new List<(string CompressedString, int? AppliedRuleID)>(currentPath); // Crea un nuovo percorso
                                newPath.Add((CompressedString: nextCompressedString, AppliedRuleID: rule.ID)); // Aggiungi la nuova stringa e la regola al nuovo percorso
                                queue.Enqueue(newPath); // Aggiungi il nuovo percorso alla coda per l'esplorazione futura
                            }
                        }
                    }
                }
            }

            // Se la ricerca termina senza trovare la stringa target
            // RIMOSSA: stopwatch.Stop(); // Ferma il cronometro
            // RIMOSSA: OnSolutionFound?.Invoke(...)

            return null; // Nessuna soluzione trovata
        }

        /// <summary>
/// Esegue una ricerca in profondità (DFS) per trovare una derivazione
/// dalla stringa iniziale alla stringa target usando le regole MIU.
/// Opera interamente con stringhe compresse per efficienza.
/// L'ordine di applicazione delle regole è influenzato da EmergingProcesses.
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
    string searchAlgorithmName = "DFS"; // Nome dell'algoritmo per il DB

    string currentStart = start; // Usa direttamente start, che è già compresso
    string currentTarget = target; // Usa direttamente target, che è già compresso

    // RIMOSSA: double frequency = Stopwatch.Frequency;
    // RIMOSSA: Stopwatch stopwatch = Stopwatch.StartNew();
    // RIMOSSA: List<(string CompressedString, int? AppliedRuleID)> solutionPath = null;
    // RIMOSSA: long elapsedTicks = 0;
    // RIMOSSA: bool soluzioneTrovata = false;
    // int numeroPassi = 0;

    if (path == null)
    {
        _globalNodesExplored = 0; // Reset del contatore globale per la chiamata iniziale
        path = new List<(string CompressedString, int? AppliedRuleID)> { (currentStart, null) }; // Inizializza con RuleID null
        visited = new HashSet<string> { currentStart };
        _globalNodesExplored = 1; // Il nodo iniziale è esplorato
    }
    // else {
    //     _globalNodesExplored = visited.Count; // Questo non è accurato per DFS ricorsiva, _globalNodesExplored è già globale
    // }


    string currentCompressedString = path.Last().CompressedString; // Preleva la stringa compressa

    if (currentCompressedString == currentTarget)
    {
        return path;
    }

    if (path.Count - 1 >= maxDepth)
    {
        return null;
    }

    // Chiedi a EmergingProcesses l'ordine preferenziale delle regole
    List<RegolaMIU> orderedRules = _learningAdvisor.GetPreferredRuleOrder(currentCompressedString, path.Count - 1); // La profondità del nodo attuale

    foreach (var rule in orderedRules) // Usa l'ordine suggerito dall'advisor
    {
        string nextCompressedString;
        if (rule.TryApply(currentCompressedString, out nextCompressedString))
        {
            // Scatena l'evento di regola applicata
            OnRuleApplied?.Invoke(null, new RuleAppliedEventArgs(
                parentString: currentCompressedString,
                newString: nextCompressedString,
                appliedRuleID: rule.ID,
                appliedRuleName: rule.Nome,
                currentDepth: path.Count, // <-- CORREZIONE QUI: Usa path.Count per la profondità del NUOVO nodo
                isSuccessPath: false
            ));

            if (!visited.Contains(nextCompressedString)) // Controlla visited qui per la logica DFS
            {
                visited.Add(nextCompressedString);
                _globalNodesExplored++; // Incrementa il contatore globale
                path.Add((CompressedString: nextCompressedString, AppliedRuleID: rule.ID)); // Aggiungi la stringa e la regola al percorso

                List<(string CompressedString, int? AppliedRuleID)> result = TrovaDerivazioneDFS(currentStart, currentTarget, maxDepth, path, visited);
                if (result != null)
                {
                    return result;
                }
                path.RemoveAt(path.Count - 1); // Backtrack
                visited.Remove(nextCompressedString); // Permetti di visitare di nuovo in altri rami
            }
        }
    }
    return null; // Nessuna soluzione trovata in questo ramo
}

    }
}
