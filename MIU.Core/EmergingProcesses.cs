//creata 11.6.2025 1.42
using System;
using System.Collections.Generic;
using System.Linq;
using MIU.Core; // Per IRegolaMIU, RegolaMIU, Compresser
using MIU.Core.Rules; // Per Rule
using MIU.Core.Topology.Map; // Per TopologicalMap, State, Rule
// using MIU.Core.Persistence; //<- cs0234 Per ILearningStatePersistence
using System.Threading.Tasks; // Per Task

namespace MIU.Core.Topology.Map
{
    /// <summary>
    /// Classe responsabile della gestione dei processi emergenti nell'ambiente MIU.
    /// Utilizza una mappa topologica per tracciare stati e transizioni,
    /// e implementa un fattore di invecchiamento (aging factor) per le statistiche.
    /// </summary>
    public class EmergingProcesses
    {
        private readonly ILearningStatePersistence _persistence;
        private readonly Compresser _compresser; // <- cs0246
        private readonly object _lock = new object(); // Blocco per operazioni thread-safe

        /// <summary>
        /// La mappa topologica che memorizza stati, regole e statistiche di apprendimento.
        /// </summary>
        public TopologicalMap CurrentMap { get; private set; }

        /// <summary>
        /// Fattore di invecchiamento applicato alle statistiche di apprendimento.
        /// Un valore vicino a 1.0 (es. 0.99) significa un lento decadimento,
        /// un valore più basso (es. 0.8) un decadimento più rapido.
        /// </summary>
        public double AgingFactor { get; private set; }

        /// <summary>
        /// Intervallo (in millisecondi) tra le applicazioni dell'aging factor.
        /// </summary>
        public int AgingIntervalMs { get; private set; }

        private DateTime _lastAgingApplicationTime;

        // Delegati per eventi di apprendimento
        public delegate void SolutionFoundEventHandler(string solution, int depth);
        public event SolutionFoundEventHandler OnSolutionFound;

        public delegate void RuleAppliedEventHandler(string parentString, RegolaMIU appliedRule, string newString, int depth);
        public event RuleAppliedEventHandler OnRuleApplied;

        /// <summary>
        /// Costruttore per EmergingProcesses.
        /// </summary>
        /// <param name="persistence">Meccanismo di persistenza per lo stato di apprendimento.</param>
        /// <param name="compresser">Compressore per le stringhe MIU.</param>
        /// <param name="agingFactor">Fattore di invecchiamento (es. 0.99) per le statistiche.</param>
        /// <param name="agingIntervalMs">Intervallo in millisecondi per l'applicazione dell'aging factor.</param>
        public EmergingProcesses(ILearningStatePersistence persistence, Compresser compresser, double agingFactor, int agingIntervalMs) // <- cs0246
        {
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _compresser = compresser ?? throw new ArgumentNullException(nameof(compresser));

            if (agingFactor <= 0 || agingFactor > 1.0)
                throw new ArgumentOutOfRangeException(nameof(agingFactor), "L'aging factor deve essere compreso tra 0 (escluso) e 1 (incluso).");
            if (agingIntervalMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(agingIntervalMs), "L'aging interval deve essere maggiore di zero.");

            AgingFactor = agingFactor;
            AgingIntervalMs = agingIntervalMs;

            CurrentMap = new TopologicalMap();
            _lastAgingApplicationTime = DateTime.UtcNow;

            // Tentativo di caricare lo stato esistente all'avvio
            LoadLearningStateAsync().Wait(); // Si blocca in modo sincrono per l'inizializzazione
        }

        /// <summary>
        /// Aggiorna le statistiche di apprendimento quando una regola viene applicata.
        /// Questo include l'aggiornamento della mappa topologica (stati e regole/archi)
        /// e le statistiche di nodi e archi.
        /// </summary>
        /// <param name="parentString">La stringa MIU prima dell'applicazione della regola.</param>
        /// <param name="appliedRule">La regola MIU applicata.</param>
        /// <param name="newString">La stringa MIU risultante dopo l'applicazione.</param>
        /// <param name="depth">La profondità corrente nell'albero di esplorazione.</param>
        /// <param name="isNewState">Indica se la newString è uno stato precedentemente non visitato.</param>
        public void UpdateLearningStateOnRuleApplication(string parentString, RegolaMIU appliedRule, string newString, int depth, bool isNewState)
        {
            lock (_lock)
            {
                // 1. Applica l'aging factor se è trascorso l'intervallo
                ApplyAgingFactorIfNeeded();

                string parentCompressed = _compresser.Compress(parentString);
                string newCompressed = _compresser.Compress(newString);

                // Aggiorna gli stati nella mappa topologica
                State sourceState = new State(parentString, depth);
                CurrentMap.AddOrUpdateState(sourceState);

                State targetState = new State(newString, depth, sourceState.Id, appliedRule.ID.ToString());
                CurrentMap.AddOrUpdateState(targetState);

                // Aggiorna le regole (archi) nella mappa topologica
                Rule topologicalRule = new Rule(
                    sourceState.Id,
                    targetState.Id,
                    appliedRule,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    isNewState,
                    depth
                );
                CurrentMap.AddOrUpdateRule(topologicalRule);

                // Aggiorna le statistiche della regola (nodo)
                RuleStatistics ruleStats = CurrentMap.GetOrCreateRuleStatistics(appliedRule.ID); // <- cs1061
                ruleStats.ApplicationCount++;
                ruleStats.LastApplicationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // <- cs1061

                // Aggiorna le statistiche della transizione (arco)
                TransitionStatistics transitionStats = CurrentMap.GetOrCreateTransitionStatistics(parentCompressed, appliedRule.ID); // <- cs1061
                transitionStats.ApplicationCount++;
                //transitionStats.LastApplicationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // <- cs0161
                transitionStats.LastUpdated = DateTime.UtcNow.ToString("o"); // <--- Usa LastUpdated
                transitionStats.SuccessRate = (transitionStats.SuccessCount / (double)transitionStats.ApplicationCount) * 100.0; // <- cs1061
                transitionStats.AverageCost = ((transitionStats.AverageCost * (transitionStats.ApplicationCount - 1)) + topologicalRule.ApplicationCost) / transitionStats.ApplicationCount; // <- cs1061

                // Aggiorna la profondità massima raggiunta per la stringa compressa
                CurrentMap.UpdateMaxDepthReachedForString(newCompressed, depth); // <- cs1061

                OnRuleApplied?.Invoke(parentString, appliedRule, newString, depth);
            }
        }

        /// <summary>
        /// Aggiorna le statistiche quando una soluzione (obiettivo MIU) viene trovata.
        /// </summary>
        /// <param name="solvedString">La stringa MIU che rappresenta la soluzione.</param>
        /// <param name="depth">La profondità a cui la soluzione è stata trovata.</param>
        /// <param name="rulesAppliedInPath">Le regole applicate per raggiungere la soluzione nel percorso.</param>
        public void UpdateLearningStateOnSolutionFound(string solvedString, int depth, List<Tuple<string, RegolaMIU, string>> rulesAppliedInPath)
        {
            lock (_lock)
            {
                // Applica l'aging factor se necessario
                ApplyAgingFactorIfNeeded();

                // Incrementa i contatori di successo per le regole e transizioni che hanno portato alla soluzione
                foreach (var step in rulesAppliedInPath)
                {
                    string parentCompressed = _compresser.Compress(step.Item1);
                    RegolaMIU appliedRule = step.Item2;
                    string newCompressed = _compresser.Compress(step.Item3);

                    // Aggiorna le statistiche della regola (nodo)
                    RuleStatistics ruleStats = CurrentMap.GetOrCreateRuleStatistics(appliedRule.ID); // <- cs1061
                    ruleStats.SuccessCount++; // <- cs1061
                    // Aggiorna l'efficacia della regola basata sul numero di successi
                    ruleStats.EffectivenessScore = (ruleStats.SuccessCount / (double)ruleStats.ApplicationCount) * 100.0;

                    // Aggiorna le statistiche della transizione (arco)
                    TransitionStatistics transitionStats = CurrentMap.GetOrCreateTransitionStatistics(parentCompressed, appliedRule.ID);
                    transitionStats.SuccessCount++;
                    transitionStats.SuccessRate = (transitionStats.SuccessCount / (double)transitionStats.ApplicationCount) * 100.0;
                }

                // Invocazione dell'evento
                OnSolutionFound?.Invoke(solvedString, depth);
            }
        }

        /// <summary>
        /// Applica l'aging factor a tutte le statistiche di regole e transizioni.
        /// Riduce l'ApplicationCount, SuccessCount, EffectivenessScore e SuccessRate
        /// per dare più peso alle interazioni recenti.
        /// </summary>
        private void ApplyAgingFactorIfNeeded()
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan timeSinceLastAging = now - _lastAgingApplicationTime;

            if (timeSinceLastAging.TotalMilliseconds >= AgingIntervalMs)
            {
                Console.WriteLine($"Applicando l'aging factor ({AgingFactor}) alle statistiche...");

                // Applica l'aging factor alle statistiche delle regole
                foreach (var ruleStats in CurrentMap.RuleNodesStatistics.Values)
                {
                    ruleStats.ApplicationCount = (int)Math.Max(1, ruleStats.ApplicationCount * AgingFactor);
                    ruleStats.SuccessCount = (int)Math.Max(0, ruleStats.SuccessCount * AgingFactor);
                    ruleStats.EffectivenessScore *= AgingFactor;
                }

                // Applica l'aging factor alle statistiche delle transizioni
                foreach (var transitionStats in CurrentMap.TransitionEdgesStatistics.Values)
                {
                    transitionStats.ApplicationCount = (int)Math.Max(1, transitionStats.ApplicationCount * AgingFactor);
                    transitionStats.SuccessCount = (int)Math.Max(0, transitionStats.SuccessCount * AgingFactor);
                    //transitionStats.SuccessRate *= AgingFactor;
                    transitionStats.AverageCost *= AgingFactor; // Anche il costo medio può invecchiare
                    transitionStats.LastUpdated = DateTime.UtcNow.ToString("o"); // <--- Aggiorna LastUpdated
                }

                // Aggiorna i VisitCount degli stati
                foreach (var state in CurrentMap.States.Values)
                {
                    state.VisitCount = (int)Math.Max(1, state.VisitCount * AgingFactor);
                }

                // Aggiorna gli ApplicationCount delle regole (archi) della mappa
                foreach (var rule in CurrentMap.Rules.Values)
                {
                    rule.ApplicationCount = (int)Math.Max(1, rule.ApplicationCount * AgingFactor);
                }

                _lastAgingApplicationTime = now;
                Console.WriteLine("Aging factor applicato.");
            }
        }


        /// <summary>
        /// Carica lo stato di apprendimento persistente.
        /// </summary>
        public async Task LoadLearningStateAsync()
        {
            lock (_lock)
            {
                try
                {
                    TopologicalMap loadedMap = _persistence.LoadLearningState();
                    if (loadedMap != null)
                    {
                        CurrentMap = loadedMap;
                        Console.WriteLine("Stato di apprendimento caricato con successo dalla persistenza.");
                    }
                    else
                    {
                        Console.WriteLine("Nessuno stato di apprendimento precedente trovato. Inizializzazione di una nuova mappa.");
                        CurrentMap = new TopologicalMap();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore durante il caricamento dello stato di apprendimento: {ex.Message}. Inizializzazione di una nuova mappa.");
                    CurrentMap = new TopologicalMap();
                }
                _lastAgingApplicationTime = DateTime.UtcNow; // Resetta il timer dopo il caricamento
            }
            await Task.CompletedTask; // Rende il metodo asincrono
        }

        /// <summary>
        /// Salva lo stato di apprendimento corrente.
        /// </summary>
        public async Task SaveLearningStateAsync()
        {
            lock (_lock)
            {
                try
                {
                    _persistence.SaveLearningState(CurrentMap);
                    Console.WriteLine("Stato di apprendimento salvato con successo nella persistenza.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore durante il salvataggio dello stato di apprendimento: {ex.Message}");
                }
            }
            await Task.CompletedTask; // Rende il metodo asincrono
        }

        /// <summary>
        /// Recupera le statistiche di una regola specifica.
        /// </summary>
        /// <param name="ruleId">L'ID numerico della regola.</param>
        /// <returns>Le statistiche della regola, o null se non trovate.</returns>
        public RuleStatistics GetRuleStatistics(int ruleId)
        {
            CurrentMap.RuleNodesStatistics.TryGetValue(ruleId, out RuleStatistics stats);
            return stats;
        }

        /// <summary>
        /// Recupera le statistiche di una transizione specifica.
        /// </summary>
        /// <param name="parentStringCompressed">La stringa compressa dello stato genitore.</param>
        /// <param name="appliedRuleId">L'ID numerico della regola applicata.</param>
        /// <returns>Le statistiche della transizione, o null se non trovate.</returns>
        public TransitionStatistics GetTransitionStatistics(string parentStringCompressed, int appliedRuleId)
        {
            CurrentMap.TransitionEdgesStatistics.TryGetValue(Tuple.Create(parentStringCompressed, appliedRuleId), out TransitionStatistics stats);
            return stats;
        }

        /// <summary>
        /// Metodo per pulire completamente lo stato di apprendimento e la mappa topologica.
        /// </summary>
        public void ClearLearningState()
        {
            lock (_lock)
            {
                CurrentMap.Clear(); // Pulisce tutti gli stati, le regole e le statistiche
                Console.WriteLine("Stato di apprendimento e mappa topologica completamente resettati.");
            }
        }
    }
}
