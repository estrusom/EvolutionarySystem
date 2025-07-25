// File: EvolutiveSystem.QuantumSynthesis/IRuleCandidateProposer.cs
// Data di riferimento: 25 luglio 2025
// Descrizione: Interfaccia per il componente responsabile di proporre RegoleMIU candidate
//              in risposta a un'Antitesi (Gap o Inefficienza).

using System.Collections.Generic;
using System.Threading.Tasks;
using EvolutiveSystem.Common; // Per RegolaMIU
using EvolutiveSystem.Taxonomy; // Per MiuAbstractPattern (base per GapPattern/InefficiencyPattern)

namespace EvolutiveSystem.QuantumSynthesis
{
    /// <summary>
    /// Definisce il contratto per un componente che propone un insieme di regole MIU candidate
    /// basandosi su un'antitesi identificata. Queste regole formano la "superposizione"
    /// prima del collasso della funzione d'onda.
    /// </summary>
    public interface IRuleCandidateProposer
    {
        /// <summary>
        /// Propone una lista di regole MIU candidate per risolvere una specifica antitesi.
        /// </summary>
        /// <param name="antithesis">Il pattern di antitesi (GapPattern o InefficiencyPattern) che innesca la proposta.</param>
        /// <returns>Una lista di RegolaMIU candidate.</returns>
        Task<List<RegolaMIU>> ProposeCandidatesAsync(MiuAbstractPattern antithesis);
    }
}
