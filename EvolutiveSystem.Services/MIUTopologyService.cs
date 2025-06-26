// File: EvolutiveSystem.Services/MIUTopologyService.cs
// Data di riferimento: 27 giugno 2025 (Aggiornato con caricamento archi finale)
// Descrizione: Implementazione del servizio di gestione e costruzione della topologia
//              dello spazio degli stati MIU. Questo servizio assembla e pesa i dati
//              per la visualizzazione dinamica, ora utilizzando il metodo efficiente
//              LoadRawRuleApplicationsForTopologyAsync da IMIUDataManager.

using System;
using System.Collections.Generic;
using System.Linq; // Per LINQ (Where, Select, ToDictionary, Any)
using System.Threading.Tasks;
using EvolutiveSystem.Common; // Per MIUStringTopologyData, MIUStringTopologyNode, MIUStringTopologyEdge, MiuStateInfo, RegolaMIU, RuleStatistics, TransitionStatistics
using EvolutiveSystem.Learning; // Per LearningStatisticsManager
using MasterLog; // Per Logger
using MIU.Core; // Per IMIUDataManager, MIUStringConverter (dall'aggiunta precedente)
// Assicurati che MIU.Core sia referenziato nel progetto EvolutiveSystem.Services per IMIUDataManager e MIUStringConverter

namespace EvolutiveSystem.Services // Namespace specifico per il progetto EvolutiveSystem.Services
{
    /// <summary>
    /// Servizio responsabile di caricare, assemblare e arricchire i dati
    /// per la visualizzazione della topologia dello spazio degli stati MIU.
    /// Applica logiche di pesatura e filtraggio temporale.
    /// </summary>
    public class MIUTopologyService : IMIUTopologyService
    {
        private readonly IMIUDataManager _dataManager;
        private readonly LearningStatisticsManager _learningStatsManager;
        private readonly Logger _logger;

        /// <summary>
        /// Costruttore per MIUTopologyService.
        /// </summary>
        /// <param name="dataManager">Istanza del gestore dati per l'accesso al database.</param>
        /// <param name="learningStatsManager">Istanza del gestore statistiche di apprendimento per i pesi.</param>
        /// <param name="logger">Istanza del logger.</param>
        public MIUTopologyService(IMIUDataManager dataManager, LearningStatisticsManager learningStatsManager, Logger logger)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _learningStatsManager = learningStatsManager ?? throw new ArgumentNullException(nameof(learningStatsManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.Log(LogLevel.INFO, "[MIUTopologyService] Servizio di topologia inizializzato.");
        }

        /// <summary>
        /// Carica i dati della topologia dello spazio degli stati MIU, inclusi nodi, bordi e pesi.
        /// Consente il filtraggio temporale e per profondità, per supportare visualizzazioni dinamiche e "a film".
        /// </summary>
        /// <param name="initialString">La stringa iniziale della ricerca per cui caricare la topologia.
        ///                               Se nullo, carica la topologia aggregata di tutte le ricerche rilevanti.</param>
        /// <param name="startDate">Data di inizio opzionale per filtrare gli eventi (nodi e bordi) in base al timestamp.</param>
        /// <param name="endDate">Data di fine opzionale per filtrare gli eventi.</param>
        /// <param name="maxDepth">Profondità massima opzionale per limitare l'esplorazione del grafo.</param>
        /// <returns>Un oggetto MIUStringTopologyData contenente nodi e bordi della topologia.</returns>
        public async Task<MIUStringTopologyData> LoadMIUStringTopologyAsync(
            string initialString = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? maxDepth = null
        )
        {
            _logger.Log(LogLevel.INFO, $"[MIUTopologyService] Caricamento topologia: Initial='{initialString ?? "Tutte"}', Start='{startDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}', End='{endDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}', MaxDepth='{maxDepth?.ToString() ?? "N/A"}'.");

            var topologyData = new MIUStringTopologyData();
            var nodesDict = new Dictionary<long, MIUStringTopologyNode>();

            try
            {
                // 1. Carica tutte le regole MIU per associare RuleID a RuleName
                // Nota: RegoleMIUManager è statico.
                // Carichiamo le regole per popolare la cache interna di RegoleMIUManager se non già fatto.
                // Oppure, se RegoleMIUManager.Regole è già popolato, lo usiamo direttamente.
                var allRules = RegoleMIUManager.Regole.ToDictionary(r => r.ID, r => r.Nome);
                if (!allRules.Any())
                {
                    // Se RegoleMIUManager.Regole non è popolato, caricalo dal DB.
                    var loadedRules = _dataManager.LoadRegoleMIU();
                    RegoleMIUManager.CaricaRegoleDaOggettoRepository(loadedRules);
                    allRules = RegoleMIUManager.Regole.ToDictionary(r => r.ID, r => r.Nome);
                }
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Caricate {allRules.Count} regole MIU per lookup.");


                // 2. Carica le statistiche di transizione aggregate (per i pesi)
                var transitionProbabilities = _learningStatsManager.GetTransitionProbabilities();
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Caricate {transitionProbabilities.Count} statistiche di transizione.");

                // 3. Carica tutti gli stati (nodi) che rientrano nel filtro temporale (per DiscoveryTime)
                var allMiuStates = await _dataManager.LoadMIUStatesAsync();

                var filteredStates = allMiuStates.Where(s =>
                {
                    DateTime discoveryTime;
                    // Tenta di parsare DiscoveryTime_Text, se fallisce usa DateTime.MinValue per escludere
                    if (!DateTime.TryParse(s.DiscoveryTime_Text, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out discoveryTime))
                    {
                        _logger.Log(LogLevel.WARNING, $"[MIUTopologyService] Impossibile parsare DiscoveryTime_Text '{s.DiscoveryTime_Text}' per StateID {s.StateID}. Stato escluso.");
                        return false; // Escludi se il tempo non è parsabile
                    }

                    bool matchesDateRange = true;
                    if (startDate.HasValue && discoveryTime < startDate.Value)
                        matchesDateRange = false;
                    if (endDate.HasValue && discoveryTime > endDate.Value)
                        matchesDateRange = false;

                    return matchesDateRange;
                }).ToList();

                foreach (var s in filteredStates)
                {
                    var node = new MIUStringTopologyNode
                    {
                        StateID = s.StateID,
                        CurrentString = s.CurrentString,
                        Depth = -1, // La profondità verrà calcolata dagli archi (se presenti)
                        DiscoveryTimeInt = s.DiscoveryTime_Int,
                        DiscoveryTimeText = s.DiscoveryTime_Text,
                        AdditionalStats = { { "UsageCount", s.UsageCount } }
                    };
                    nodesDict[node.StateID] = node;
                }
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Caricati {nodesDict.Count} stati (nodi) basati sul filtro temporale.");


                // 4. Carica le applicazioni di regole (bordi) usando il nuovo metodo di IMIUDataManager
                // Questo metodo carica direttamente gli archi con SearchID, Timestamp, CurrentDepth
                var rawEdges = await _dataManager.LoadRawRuleApplicationsForTopologyAsync(
                    initialString, startDate, endDate, maxDepth
                );
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Caricati {rawEdges.Count} raw edges dal data manager.");

                // Filtra e prepara gli archi finali, assicurandoti che i nodi associati esistano
                var finalEdges = new List<MIUStringTopologyEdge>();
                foreach (var ra in rawEdges)
                {
                    // Assicurati che i nodi genitore e figlio esistano nel nostro set filtrato.
                    // Se un nodo non è nel nodesDict, significa che non rientra nel filtro temporale dei nodi.
                    if (nodesDict.ContainsKey(ra.ParentStateID) && nodesDict.ContainsKey(ra.NewStateID))
                    {
                        string ruleName = allRules.TryGetValue(ra.AppliedRuleID, out string name) ? name : "Sconosciuta";

                        var edge = new MIUStringTopologyEdge
                        {
                            ApplicationID = ra.ApplicationID,
                            SearchID = ra.SearchID, // Ora disponibile direttamente da LoadRawRuleApplicationsForTopologyAsync
                            ParentStateID = ra.ParentStateID,
                            NewStateID = ra.NewStateID,
                            AppliedRuleID = ra.AppliedRuleID,
                            AppliedRuleName = ruleName,
                            CurrentDepth = ra.CurrentDepth,
                            Timestamp = ra.Timestamp, // Ora disponibile direttamente
                            Weight = 0.0 // Calcolato sotto
                        };
                        finalEdges.Add(edge);

                        // Aggiorna la profondità del nodo di destinazione se questa derivazione ha una profondità minore
                        if (nodesDict.TryGetValue(edge.NewStateID, out var targetNode))
                        {
                            if (targetNode.Depth == -1 || edge.CurrentDepth < targetNode.Depth)
                            {
                                targetNode.Depth = edge.CurrentDepth;
                            }
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Ignorato arco (AppID: {ra.ApplicationID}) perché uno o entrambi i nodi (Parent: {ra.ParentStateID}, New: {ra.NewStateID}) non rientrano nei filtri sui nodi.");
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Finalizzati {finalEdges.Count} bordi dopo il filtering dei nodi.");


                // 5. Applica i pesi ai bordi
                foreach (var edge in finalEdges)
                {
                    // Assicurati che il nodo genitore esista in nodesDict per ottenere la stringa
                    if (nodesDict.TryGetValue(edge.ParentStateID, out var parentNode))
                    {
                        string parentCompressedString = MIUStringConverter.DeflateMIUString(parentNode.CurrentString);
                        var key = Tuple.Create(parentCompressedString, edge.AppliedRuleID);

                        if (transitionProbabilities.TryGetValue(key, out TransitionStatistics stats))
                        {
                            // Formula di pesatura (SuccessRate + bonus per frequenza + decadimento temporale)
                            double timeDecayFactor = 1.0;
                            // Il decadimento temporale si basa sulla "freschezza" dell'applicazione della regola
                            TimeSpan ageFromEndDate = (endDate.HasValue ? endDate.Value : DateTime.UtcNow) - edge.Timestamp;

                            // Un esempio più robusto di decadimento, usando la normalizzazione dell'età.
                            // Supponiamo che gli eventi molto vecchi (es. > 1 anno) abbiano un peso molto basso.
                            double daysOld = ageFromEndDate.TotalDays;
                            if (daysOld > 0)
                            {
                                timeDecayFactor = Math.Max(0.01, 1.0 - (daysOld / 365.0)); // Decadimento lineare su 1 anno
                            }
                            else
                            {
                                timeDecayFactor = 1.0; // Eventi recenti o futuri non decadono
                            }


                            // Formula combinata: SuccessRate, Frequenza, Recenza
                            edge.Weight = stats.SuccessRate * (1.0 + (stats.ApplicationCount / 50.0)) * timeDecayFactor;
                            // Assicura un peso minimo per evitare divisioni per zero o pesi negativi
                            edge.Weight = Math.Max(0.01, edge.Weight);
                        }
                        else
                        {
                            edge.Weight = 0.01; // Peso minimo se non ci sono statistiche per la transizione specifica
                        }
                    }
                    else
                    {
                        // Questo caso non dovrebbe accadere se il filtering iniziale dei nodi è corretto,
                        // ma per sicurezza logghiamo.
                        _logger.Log(LogLevel.WARNING, $"[MIUTopologyService] Arco {edge.ApplicationID} ha un ParentStateID {edge.ParentStateID} non trovato nel dizionario dei nodi filtrati durante il calcolo del peso.");
                        edge.Weight = 0.0; // Nessun peso se il nodo genitore non è valido
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Pesi applicati ai bordi.");

                // 6. Finalizza la collezione di nodi, includendo solo quelli effettivamente connessi da un bordo
                // e quelli che hanno una profondità valida.
                var connectedNodeIds = new HashSet<long>();
                foreach (var edge in finalEdges)
                {
                    connectedNodeIds.Add(edge.ParentStateID);
                    connectedNodeIds.Add(edge.NewStateID);
                }
                topologyData.Nodes = nodesDict.Values
                    .Where(node => connectedNodeIds.Contains(node.StateID) && node.Depth != -1) // Includi solo nodi connessi e con profondità calcolata
                    .ToList();
                topologyData.Edges = finalEdges;

                // Popola i dati della ricerca di riferimento (se specificata)
                if (!string.IsNullOrEmpty(initialString))
                {
                    // Carichiamo la stringa iniziale della ricerca dal database.
                    // Idealmente, questo dovrebbe essere un metodo specifico in IMIUDataManager
                    // per caricare i dettagli di una singola SearchID (es. GetSearchById).
                    // Per ora, lo facciamo trovando uno stato iniziale che corrisponda,
                    // che è un po' un workaround ma sufficiente per il display.
                    var searchRecord = (await _dataManager.LoadMIUStatesAsync())
                                     .FirstOrDefault(s => s.CurrentString == initialString);

                    if (searchRecord != null)
                    {
                        topologyData.InitialString = initialString;
                        // Nota: SearchID e MaxDepthExplored provengono da MIU_Searches, non da MIU_States.
                        // Il MIUTopologyService non ha ancora accesso diretto ai dati completi di una singola ricerca.
                        // Questo potrebbe essere un perfezionamento futuro:
                        // aggiungere GetSearchInfo(initialString, targetString) a IMIUDataManager.
                        // Per ora, questi campi rimarranno non popolati o useranno placeholder se non essenziali.
                    }
                }

                _logger.Log(LogLevel.INFO, $"[MIUTopologyService] Topologia MIU caricata con {topologyData.Nodes.Count} nodi e {topologyData.Edges.Count} bordi.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MIUTopologyService] Errore durante il caricamento della topologia MIU: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return new MIUStringTopologyData(); // Restituisce un oggetto vuoto in caso di errore
            }
            return topologyData;
        }

        // Rimosso il metodo temporaneo LoadAllRuleApplicationsTemporarily
        // Rimosso il metodo temporaneo GetInitialStringForSearch
    }
}
