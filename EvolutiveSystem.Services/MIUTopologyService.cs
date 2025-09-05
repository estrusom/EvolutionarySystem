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
using EvolutiveSystem.Taxonomy; // Per RuleTaxonomyGenerator
using MasterLog; // Per Logger
using MIU.Core; // Per IMIUDataManager, MIUStringConverter (dall'aggiunta precedente)

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
        private readonly RuleTaxonomyGenerator _taxonomyGenerator; // Nuovo campo per il RuleTaxonomyGenerator
        private readonly Logger _logger;

        /// <summary>
        /// Costruttore per MIUTopologyService.
        /// </summary>
        /// <param name="dataManager">Istanza del gestore dati per l'accesso al database.</param>
        /// <param name="learningStatsManager">Istanza del gestore statistiche di apprendimento per i pesi.</param>
        /// <param name="taxonomyGenerator">Istanza del generatore di tassonomia per l'analisi dei pattern.</param>
        /// <param name="logger">Istanza del logger.</param>
        public MIUTopologyService(IMIUDataManager dataManager, LearningStatisticsManager learningStatsManager, RuleTaxonomyGenerator taxonomyGenerator, Logger logger)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _learningStatsManager = learningStatsManager ?? throw new ArgumentNullException(nameof(learningStatsManager));
            _taxonomyGenerator = taxonomyGenerator ?? throw new ArgumentNullException(nameof(taxonomyGenerator)); // Aggiunto per il costruttore
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
                var allRules = await _dataManager.LoadRegoleMIUAsync(); // Usiamo la versione asincrona se disponibile
                var ruleLookup = allRules.ToDictionary(r => r.ID, r => r.Nome);
                var transitionProbabilities = await _learningStatsManager.GetTransitionProbabilitiesAsync();
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Caricate {ruleLookup.Count} regole MIU per lookup.");

                /*
                // 2. Carica le statistiche di transizione aggregate (per i pesi)
                var transitionProbabilities = await _learningStatsManager.GetTransitionProbabilitiesAsync();
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Caricate {transitionProbabilities.Count} statistiche di transizione.");

                // 3. Carica tutti gli stati (nodi) che rientrano nel filtro temporale
                var allMiuStates = await _dataManager.LoadMIUStatesAsync();
                var filteredStates = allMiuStates.Where(s =>
                {
                    DateTime discoveryTime;
                    if (!DateTime.TryParse(s.DiscoveryTime_Text, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out discoveryTime))
                    {
                        _logger.Log(LogLevel.WARNING, $"[MIUTopologyService] Impossibile parsare DiscoveryTime_Text '{s.DiscoveryTime_Text}' per StateID {s.StateID}. Stato escluso.");
                        return false;
                    }

                    bool matchesDateRange = true;
                    if (startDate.HasValue && discoveryTime < startDate.Value) matchesDateRange = false;
                    if (endDate.HasValue && discoveryTime > endDate.Value) matchesDateRange = false;

                    return matchesDateRange;
                }).ToList();
                
                foreach (var s in filteredStates)
                {
                    var node = new MIUStringTopologyNode
                    {
                        StateID = s.StateID,
                        CurrentString = s.CurrentString,
                        Depth = -1,
                        DiscoveryTimeInt = s.DiscoveryTime_Int,
                        DiscoveryTimeText = s.DiscoveryTime_Text,
                        AdditionalStats = { { "UsageCount", s.UsageCount } }
                    };
                    nodesDict[node.StateID] = node;
                }
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Caricati {nodesDict.Count} stati (nodi) basati sul filtro temporale.");
                */

                // 3. Carica tutti gli stati (nodi) che rientrano nel filtro temporale
                var allMiuStates = await _dataManager.LoadMIUStatesAsync(); // <-- ECCO LA RIGA MANCANTE, ORA INCLUSA
                var filteredStates = new List<MiuStateInfo>();
                const string expectedDateFormat = "yyyy-MM-dd HH:mm:ss";

                foreach (var s in allMiuStates)
                {
                    if (string.IsNullOrWhiteSpace(s.DiscoveryTime_Text))
                    {
                        _logger.Log(LogLevel.WARNING, $"[MIUTopologyService] DiscoveryTime_Text è vuoto per StateID {s.StateID}. Stato escluso.");
                        continue;
                    }

                    if (DateTime.TryParseExact(s.DiscoveryTime_Text, expectedDateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime discoveryTime))
                    {
                        bool matchesDateRange = true;
                        if (startDate.HasValue && discoveryTime < startDate.Value) matchesDateRange = false;
                        if (endDate.HasValue && discoveryTime > endDate.Value) matchesDateRange = false;

                        if (matchesDateRange)
                        {
                            filteredStates.Add(s);
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.WARNING, $"[MIUTopologyService] Impossibile parsare DiscoveryTime_Text '{s.DiscoveryTime_Text}' per StateID {s.StateID} con il formato atteso '{expectedDateFormat}'. Stato escluso.");
                    }
                }

                // Aggiungiamo i nodi filtrati al dizionario
                foreach (var s in filteredStates)
                {
                    var node = new MIUStringTopologyNode
                    {
                        StateID = s.StateID,
                        CurrentString = s.CurrentString,
                        Depth = -1,
                        DiscoveryTimeInt = s.DiscoveryTime_Int,
                        DiscoveryTimeText = s.DiscoveryTime_Text,
                        AdditionalStats = { { "UsageCount", s.UsageCount } }
                    };
                    nodesDict[node.StateID] = node;
                }
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Caricati {nodesDict.Count} stati (nodi) basati sul filtro temporale.");
                // 4. Carica le applicazioni di regole (bordi)
                var rawEdges = await _dataManager.LoadRawRuleApplicationsForTopologyAsync(
                    initialString, startDate, endDate, maxDepth
                );
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Caricati {rawEdges.Count} raw edges dal data manager.");

                var finalEdges = new List<MIUStringTopologyEdge>();
                foreach (var ra in rawEdges)
                {
                    if (nodesDict.ContainsKey(ra.ParentStateID) && nodesDict.ContainsKey(ra.NewStateID))
                    {
                        string ruleName = ruleLookup.TryGetValue(ra.AppliedRuleID, out string name) ? name : "Sconosciuta";

                        var edge = new MIUStringTopologyEdge
                        {
                            ApplicationID = ra.ApplicationID,
                            SearchID = ra.SearchID,
                            ParentStateID = ra.ParentStateID,
                            NewStateID = ra.NewStateID,
                            AppliedRuleID = ra.AppliedRuleID,
                            AppliedRuleName = ruleName,
                            CurrentDepth = ra.CurrentDepth,
                            Timestamp = ra.Timestamp,
                            Weight = 0.0
                        };
                        finalEdges.Add(edge);

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

                // 5. Perfezionare la matematica della topologia e applicare i pesi
                foreach (var edge in finalEdges)
                {
                    if (nodesDict.TryGetValue(edge.ParentStateID, out var parentNode))
                    {
                        string parentCompressedString = MIUStringConverter.DeflateMIUString(parentNode.CurrentString);
                        var key = Tuple.Create(parentCompressedString, edge.AppliedRuleID);

                        if (transitionProbabilities.TryGetValue(key, out TransitionStatistics stats))
                        {
                            // --- Nuova Formula per il Peso dell'Arco ---
                            // La nuova formula si basa sul SuccessRate (efficacia) e sull'ApplicationCount (frequenza)
                            // La formula precedente non aveva una base matematica solida.
                            // Invece di usare un fattore di decadimento arbitrario,
                            // usiamo un punteggio normalizzato basato su metriche esistenti.

                            double successRatio = (double)stats.SuccessfulCount / stats.ApplicationCount;
                            double frequencyBonus = Math.Log(stats.ApplicationCount + 1, 2); // Logaritmo per attenuare l'effetto di conteggi molto grandi

                            // Il peso dell'arco riflette l'utilità e la frequenza della transizione
                            edge.Weight = successRatio * frequencyBonus;
                            // Assicura un peso minimo per evitare valori nulli
                            edge.Weight = Math.Max(0.01, edge.Weight);
                        }
                        else
                        {
                            edge.Weight = 0.01; // Peso minimo se non ci sono statistiche per la transizione
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.WARNING, $"[MIUTopologyService] Arco {edge.ApplicationID} ha un ParentStateID {edge.ParentStateID} non trovato nel dizionario dei nodi.");
                        edge.Weight = 0.0;
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"[MIUTopologyService] Pesi applicati ai bordi.");

                // 6. Finalizza la collezione di nodi, includendo solo quelli connessi da un bordo
                var connectedNodeIds = new HashSet<long>();
                foreach (var edge in finalEdges)
                {
                    connectedNodeIds.Add(edge.ParentStateID);
                    connectedNodeIds.Add(edge.NewStateID);
                }
                //topologyData.Nodes = nodesDict.Values .Where(node => connectedNodeIds.Contains(node.StateID) && node.Depth != -1).ToList();
                topologyData.Nodes = nodesDict.Values.Where(node => connectedNodeIds.Contains(node.StateID)).ToList();
                topologyData.Edges = finalEdges;
                _logger.Log(LogLevel.INFO, $"[MIUTopologyService] Topologia MIU caricata con {topologyData.Nodes.Count} nodi e {topologyData.Edges.Count} bordi.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MIUTopologyService] Errore durante il caricamento della topologia MIU: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return new MIUStringTopologyData();
            }
            return topologyData;
        }

        // Questo metodo carica le regole in modo asincrono, utile per il nuovo codice
        public async Task<List<RegolaMIU>> LoadRegoleMIUAsync()
        {
            var regole = new List<RegolaMIU>();
            try
            {
                regole = _dataManager.LoadRegoleMIU();
                _logger.Log(LogLevel.DEBUG, "[MIUTopologyService] RegoleMIU caricate dal database.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MIUTopologyService] Errore caricamento RegoleMIU: {ex.Message}. Restituisco lista vuota.");
            }
            return await Task.FromResult(regole);
        }
    }
}
