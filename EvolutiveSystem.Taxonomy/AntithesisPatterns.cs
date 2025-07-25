// File: EvolutiveSystem.Taxonomy.Antithesis/AntithesisPatterns.cs
// Data di riferimento: 26 luglio 2025
// Descrizione: Definisce le classi concrete per i pattern di Gap e Inefficienza,
//              che ereditano da MiuAbstractPattern.

using EvolutiveSystem.Common; // Per MiuAbstractPattern
using System;

namespace EvolutiveSystem.Taxonomy.Antithesis
{
    /// <summary>
    /// Rappresenta un pattern specifico di "Gap" nel paesaggio MIU.
    /// Eredita da MiuAbstractPattern e aggiunge la semantica di un Gap.
    /// </summary>
    public class GapPattern : MiuAbstractPattern
    {
        /// <summary>
        /// Costruttore per GapPattern.
        /// </summary>
        /// <param name="type">Il tipo del pattern (es. "StringLength").</param>
        /// <param name="value">Il valore del pattern (es. "100").</param>
        /// <param name="nome">Il nome descrittivo del pattern.</param>
        public GapPattern(string type, string value, string nome)
            : base(type, value, nome)
        {
            // Nessuna logica aggiuntiva specifica per GapPattern al momento.
            // Il nome del pattern dovrebbe già indicare che è un Gap.
        }
    }

    /// <summary>
    /// Rappresenta un pattern specifico di "Inefficienza" nel paesaggio MIU.
    /// Eredita da MiuAbstractPattern e aggiunge la semantica di un'Inefficienza.
    /// </summary>
    public class InefficiencyPattern : MiuAbstractPattern
    {
        /// <summary>
        /// Costruttore per InefficiencyPattern.
        /// </summary>
        /// <param name="type">Il tipo del pattern (es. "LowEffectivenessRule").</param>
        /// <param name="value">Il valore del pattern (es. "Rule1").</param>
        /// <param name="nome">Il nome descrittivo del pattern.</param>
        public InefficiencyPattern(string type, string value, string nome)
            : base(type, value, nome)
        {
            // Nessuna logica aggiuntiva specifica per InefficiencyPattern al momento.
            // Il nome del pattern dovrebbe già indicare che è un'Inefficienza.
        }
    }
}
