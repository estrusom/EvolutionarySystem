// 25.09.04 Descrizione: Definizione dell'enumerazione InefficiencyPatternType
namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Enumera i tipi di pattern di inefficienza che possono essere identificati
    /// nell'applicazione delle regole del sistema formale.
    /// </summary>
    public enum InefficiencyPatternType
    {
        /// <summary>
        /// Nessun pattern di inefficienza rilevato.
        /// </summary>
        None,

        /// <summary>
        /// Indica un "buco nero", una regola che viene applicata molto spesso
        /// ma che raramente porta a uno stato di successo. Consuma risorse senza progredire.
        /// </summary>
        BlackHole,

        /// <summary>
        /// Indica un percorso ciclico, in cui il sistema ritorna a uno stato già visitato.
        /// </summary>
        CyclicPath,

        /// <summary>
        /// Indica una regola che, sebbene valida, è raramente utilizzata e potrebbe essere
        /// un candidato per la potatura (pruning) dall'albero di ricerca.
        /// </summary>
        RarelyUsed
    }
}
