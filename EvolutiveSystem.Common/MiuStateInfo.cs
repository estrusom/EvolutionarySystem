// File: EvolutiveSystem.Common/MiuStateInfo.cs
// Data di riferimento: 21 giugno 2025
// Descrizione: Classe modello per rappresentare un singolo stato MIU dal database.

using System;

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta un record della tabella MIU_States.
    /// </summary>
    public class MiuStateInfo
    {
        public long StateID { get; set; }
        public string CurrentString { get; set; } // La stringa MIU standard (non compressa)
        public int StringLength { get; set; }
        public string DeflateString { get; set; } // La stringa MIU compressa
        public string Hash { get; set; }
        public long DiscoveryTime_Int { get; set; }
        public string DiscoveryTime_Text { get; set; }
        public int UsageCount { get; set; }
    }
}
