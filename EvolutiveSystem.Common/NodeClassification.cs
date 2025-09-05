using System.Collections.Generic;

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta l'analisi tassonomica di un singolo nodo (stringa MIU).
    /// Contiene le proprietà calcolate della stringa.
    /// </summary>
    public class NodeClassification
    {
        public long StateID { get; set; }
        public string CurrentString { get; set; }

        // --- Proprietà Analitiche ---
        public int StringLength { get; set; }
        public int I_Count { get; set; }
        public int U_Count { get; set; }

        /// <summary>
        /// Proprietà derivate o "interessanti" scoperte durante l'analisi.
        /// Esempio: "I_Count_Is_Divisible_By_3" -> "False"
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}