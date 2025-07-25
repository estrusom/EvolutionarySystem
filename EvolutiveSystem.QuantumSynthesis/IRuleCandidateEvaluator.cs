// File: EvolutiveSystem.QuantumSynthesis/IRuleCandidateEvaluator.cs
// Data di riferimento: 25 luglio 2025
// Descrizione: Interfaccia per il componente responsabile di valutare le regole candidate
//              basandosi sui risultati della simulazione e sull'Antitesi originale.

using System.Threading.Tasks;
using EvolutiveSystem.Common; // Per RegolaMIU
using EvolutiveSystem.Taxonomy; // Per MiuAbstractPattern (base per GapPattern/InefficiencyPattern)

namespace EvolutiveSystem.QuantumSynthesis
{
    /// <summary>
    /// Definisce il contratto per un componente che valuta l'efficacia di una regola MIU candidata.
    /// Questa valutazione si basa sui risultati di una simulazione e sull'antitesi originale,
    /// assegnando un punteggio di "fitness" per il "collasso della funzione d'onda".
    /// </summary>
    public interface IRuleCandidateEvaluator
    {
        /// <summary>
        /// Valuta una singola regola MIU candidata basandosi sui risultati della simulazione
        /// e sull'antitesi che ha innescato la generazione della regola.
        /// </summary>
        /// <param name="candidateRule">La RegolaMIU candidata da valutare.</param>
        /// <param name="simulationResult">I risultati della simulazione ottenuti con la regola candidata.</param>
        /// <param name="originalAntithesis">L'Antitesi (GapPattern o InefficiencyPattern) che ha innescato il processo.</param>
        /// <returns>Il punteggio di fitness della regola candidata (valore più alto = migliore).</returns>
        Task<double> EvaluateCandidateAsync(RegolaMIU candidateRule, SimulationResult simulationResult, MiuAbstractPattern originalAntithesis);
    }
}
