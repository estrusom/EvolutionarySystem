// EvolutiveSystem.QuantumSynthesis/MiuSimulationEnvironment.cs
// Data di riferimento: 26 luglio 2025
// Descrizione: Ambiente di simulazione per testare l'impatto di nuove regole MIU,
//              raccogliendo dati grezzi per il calcolo delle metriche di SimulationResult.

using System;
using System.Collections.Generic;
using System.Linq;
using EvolutiveSystem.Common;        // Per MiuStateInfo, RegolaMIU, SimulationResult
using EvolutiveSystem.Explorer;      // Per MIUAutoExplorer, ExplorationNode
using System.Diagnostics;            // Per Stopwatch

namespace EvolutiveSystem.QuantumSynthesis
{
    /// <summary>
    /// Fornisce un ambiente di simulazione per valutare le regole MIU candidate.
    /// Esegue simulazioni mirate e raccoglie dati per il calcolo delle metriche di performance.
    /// La struttura è pensata per essere estensibile per l'aggiunta di nuove "lenti" (metriche).
    /// </summary>
    public class MiuSimulationEnvironment
    {
        private readonly MIUAutoExplorer _explorer;
        private readonly List<RegolaMIU> _baseRules; // Regole base esistenti nel sistema

        /// <summary>
        /// Inizializza un nuovo ambiente di simulazione.
        /// </summary>
        /// <param name="baseRules">Le regole MIU esistenti nel sistema che l'esploratore può utilizzare.</param>
        public MiuSimulationEnvironment(List<RegolaMIU> baseRules)
        {
            _baseRules = baseRules ?? throw new ArgumentNullException(nameof(baseRules), "Le regole base non possono essere nulle.");
            if (!_baseRules.Any())
            {
                throw new ArgumentException("La lista delle regole base non può essere vuota.", nameof(baseRules));
            }
            _explorer = new MIUAutoExplorer(_baseRules);
        }

        /// <summary>
        /// Esegue una simulazione mirata per una regola MIU candidata su un set di stati iniziali di test.
        /// Raccoglie dati grezzi e calcola le metriche di SimulationResult.
        /// Questo è il "laboratorio" che valuta l'impatto della regola candidata.
        /// </summary>
        /// <param name="candidateRule">La regola MIU candidata da valutare.</param>
        /// <param name="testStartingStates">Una lista di stati MIU da cui iniziare le simulazioni di test.</param>
        /// <param name="targetState">Lo stato obiettivo opzionale da raggiungere (ad esempio, l'antitesi risolta).</param>
        /// <param name="maxDepth">Profondità massima di esplorazione per ogni test.</param>
        /// <returns>Un oggetto SimulationResult contenente le metriche aggregate della simulazione.</returns>
        public SimulationResult Simulate(RegolaMIU candidateRule, List<MiuStateInfo> testStartingStates, MiuStateInfo targetState, int maxDepth = 100)
        {
            if (candidateRule == null) throw new ArgumentNullException(nameof(candidateRule), "La regola candidata non può essere nulla.");
            if (testStartingStates == null || !testStartingStates.Any()) throw new ArgumentException("Devono essere forniti stati di partenza per il test.", nameof(testStartingStates));
            if (targetState == null) throw new ArgumentNullException(nameof(targetState), "Lo stato target non può essere nullo.");

            // Combina le regole base con la regola candidata per questa simulazione.
            // La regola candidata viene temporaneamente aggiunta per testarla.
            var simulationRules = new List<RegolaMIU>(_baseRules);
            // Evita di aggiungere duplicati se la regola candidata è già tra le baseRules
            // Correzione: Usiamo r.ID invece di r.RuleID
            if (!simulationRules.Any(r => r.ID == candidateRule.ID))
            {
                simulationRules.Add(candidateRule);
            }

            // Re-inizializza l'esploratore con il set di regole aggiornato per questa simulazione.
            MIUAutoExplorer currentExplorer = new MIUAutoExplorer(simulationRules);

            // Dati grezzi che verranno raccolti durante la simulazione
            List<List<(MiuStateInfo State, RegolaMIU RuleApplied)>> allPathsFound = new List<List<(MiuStateInfo State, RegolaMIU RuleApplied)>>();
            HashSet<string> allDiscoveredStringsSet = new HashSet<string>();
            List<MiuStateInfo> allDiscoveredStatesList = new List<MiuStateInfo>();
            List<int> allDepthsReached = new List<int>();
            int totalRuleApplicationsCount = 0;

            Stopwatch stopwatch = Stopwatch.StartNew();

            bool targetStateReachedInAnyTest = false;

            foreach (var startState in testStartingStates)
            {
                var path = currentExplorer.GetDerivationPath(startState, targetState, maxDepth);

                if (path != null)
                {
                    allPathsFound.Add(path);

                    allDepthsReached.Add(path.Count - 1);

                    if (path.Last().State.CurrentString.Equals(targetState.CurrentString, StringComparison.Ordinal))
                    {
                        targetStateReachedInAnyTest = true;
                    }

                    foreach (var step in path)
                    {
                        if (allDiscoveredStringsSet.Add(step.State.CurrentString))
                        {
                            allDiscoveredStatesList.Add(step.State);
                        }
                    }
                    totalRuleApplicationsCount += (path.Count > 0 ? path.Count - 1 : 0);
                }
                else
                {
                    if (allDiscoveredStringsSet.Add(startState.CurrentString))
                    {
                        allDiscoveredStatesList.Add(startState);
                    }
                    allDepthsReached.Add(0);
                }
            }

            stopwatch.Stop();

            SimulationResult result = new SimulationResult
            {
                ElapsedTime = stopwatch.Elapsed,
                TotalStatesExplored = allDiscoveredStringsSet.Count,
                DiscoveredStates = allDiscoveredStatesList,

                TargetAntithesisResolutionScore = targetStateReachedInAnyTest ? 1.0 : 0.0,
                TotalVariationsGenerated = allDiscoveredStringsSet.Count - testStartingStates.Count,

                MaxDepthReached = allDepthsReached.Any() ? allDepthsReached.Max() : 0,
                AverageDepthOfDiscovery = allDepthsReached.Any() ? allDepthsReached.Average() : 0,

                TotalRuleApplications = totalRuleApplicationsCount,
            };

            CalculatePatternMetrics(allDiscoveredStringsSet, result);
            CalculateTokenBalanceMetrics(allDiscoveredStringsSet, result);
            CalculateStringLengthMetrics(allDiscoveredStringsSet, result);

            return result;
        }

        /// <summary>
        /// Lente 1: Calcola le metriche relative ai pattern.
        /// </summary>
        /// <param name="discoveredStrings">Tutte le stringhe MIU uniche scoperte durante la simulazione.</param>
        /// <param name="result">L'oggetto SimulationResult da popolare.</param>
        private void CalculatePatternMetrics(HashSet<string> discoveredStrings, SimulationResult result)
        {
            result.UniquePatternCount = discoveredStrings.Count;

            result.PatternOccurrenceCounts = new Dictionary<MiuAbstractPattern, int>();
            result.PatternDiversityScore = (double)discoveredStrings.Count / (discoveredStrings.Count > 0 ? discoveredStrings.Max(s => s.Length) : 1);
            result.AverageVariationDepth = 0.0;
        }

        /// <summary>
        /// Lente 2: Calcola le metriche relative al bilanciamento dei token 'M', 'I', 'U'.
        /// </summary>
        /// <param name="discoveredStrings">Tutte le stringhe MIU uniche scoperte durante la simulazione.</param>
        /// <param name="result">L'oggetto SimulationResult da popolare.</param>
        private void CalculateTokenBalanceMetrics(HashSet<string> discoveredStrings, SimulationResult result)
        {
            if (!discoveredStrings.Any()) return;

            long totalM = 0;
            long totalI = 0;
            long totalU = 0;

            foreach (var s in discoveredStrings)
            {
                totalM += s.Count(c => c == 'M');
                totalI += s.Count(c => c == 'I');
                totalU += s.Count(c => c == 'U');
            }

            result.TokenCounts = new Dictionary<string, int>
            {
                { "M", (int)totalM },
                { "I", (int)totalI },
                { "U", (int)totalU }
            };

            long totalTokens = totalM + totalI + totalU;
            if (totalTokens > 0)
            {
                result.M_Ratio = (double)totalM / totalTokens;
                result.I_Ratio = (double)totalI / totalTokens;
                result.U_Ratio = (double)totalU / totalTokens;

                double idealRatio = 1.0 / 3.0;
                result.TokenBalanceScore = 1.0 - (
                    Math.Abs(result.M_Ratio - idealRatio) +
                    Math.Abs(result.I_Ratio - idealRatio) +
                    Math.Abs(result.U_Ratio - idealRatio)
                ) / 2.0;
            }
            else
            {
                result.M_Ratio = 0;
                result.I_Ratio = 0;
                result.U_Ratio = 0;
                result.TokenBalanceScore = 0;
            }
        }

        /// <summary>
        /// Lente 3: Calcola le metriche relative alla lunghezza delle stringhe.
        /// </summary>
        /// <param name="discoveredStrings">Tutte le stringhe MIU uniche scoperte durante la simulazione.</param>
        /// <param name="result">L'oggetto SimulationResult da popolare.</param>
        private void CalculateStringLengthMetrics(HashSet<string> discoveredStrings, SimulationResult result)
        {
            if (!discoveredStrings.Any()) return;

            var lengths = discoveredStrings.Select(s => (double)s.Length).ToList();
            result.AverageStringLength = lengths.Average();

            double sumOfSquaresOfDifferences = lengths.Sum(x => Math.Pow(x - result.AverageStringLength, 2));
            result.StringLengthVariance = sumOfSquaresOfDifferences / lengths.Count;
        }
    }
}