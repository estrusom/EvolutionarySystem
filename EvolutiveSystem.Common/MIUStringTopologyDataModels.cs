// EvolutiveSystem.Common/MIUStringTopologyDataModels.cs
// Data di riferimento: 26 giugno 2025 (Aggiornato: Correzione HashCode per .NET Framework 4.8)
// Descrizione: Classi modello per rappresentare i dati della "topologia/topografia" dello spazio degli stati MIU.
//              Questi DTO verranno popolati dal motore e persistiti nel database,
//              rappresentando nodi (stati MIU) e bordi (applicazioni di regole)
//              all'interno del grafo di derivazione.

using System;
using System.Collections.Generic;
using System.Linq; // Necessario per LINQ
using System.Security.Cryptography; // Necessario per SHA256, se non già in MIUStringConverter
using System.Text; // Necessario per Encoding.UTF8

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta un nodo (stato MIU) nella mappa topologica dello spazio degli stati MIU.
    /// Ogni nodo corrisponde a una stringa MIU unica e include metadati
    /// come la profondità e statistiche per la "pesatura".
    /// </summary>
    public class MIUStringTopologyNode
    {
        /// <summary>
        /// ID univoco del nodo. Corrisponde all'StateID nella tabella MIU_States.
        /// Sarà generato dal database.
        /// </summary>
        public long StateID { get; set; }

        /// <summary>
        /// La stringa MIU standard (non compressa) che questo nodo rappresenta.
        /// </summary>
        public string CurrentString { get; set; }

        /// <summary>
        /// La profondità di derivazione dalla stringa iniziale (il "peso" base).
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Posizione 3D suggerita per la visualizzazione sulla mappa fluttuante.
        /// Questi valori possono essere calcolati dopo la generazione della mappa.
        /// </summary>
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        /// <summary>
        /// Statistiche aggiuntive associate a questo stato, per la "pesatura" avanzata.
        /// Potrebbero includere: conteggio delle visite, numero di regole applicabili, ecc.
        /// </summary>
        public Dictionary<string, object> AdditionalStats { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Il timestamp numerico (e.g., UNIX timestamp in secondi o millisecondi) di quando lo stato è stato scoperto.
        /// Utile per ordinamento efficiente e calcoli di decadimento temporale.
        /// </summary>
        public long DiscoveryTimeInt { get; set; }

        /// La rappresentazione testuale del timestamp di scoperta dello stato.
        /// Utile per la visualizzazione o per il parsing preciso.
        /// </summary>
        public string DiscoveryTimeText { get; set; }

        /// <summary>
        /// Costruttore di default.
        /// </summary>
        public MIUStringTopologyNode() { }

        /// <summary>
        /// Costruttore per creare un MIUStringTopologyNode durante l'esplorazione.
        /// L'StateID sarà impostato dopo la persistenza nel database.
        /// </summary>
        /// <param name="currentString">La stringa MIU dello stato.</param>
        /// <param name="depth">La profondità di derivazione.</param>
        public MIUStringTopologyNode(string currentString, int depth)
        {
            CurrentString = currentString;
            Depth = depth;
            // Le posizioni X, Y, Z e le statistiche aggiuntive verranno popolate in seguito.
        }

        // Override di Equals e GetHashCode per confrontare i nodi in base alla loro stringa MIU.
        // Questo è utile per le HashSet durante l'esplorazione.
        public override bool Equals(object obj)
        {
            return obj is MIUStringTopologyNode node && CurrentString == node.CurrentString;
        }

        public override int GetHashCode()
        {
            return CurrentString.GetHashCode();
        }
    }

    /// <summary>
    /// Rappresenta un bordo (transizione/derivazione) nella mappa topologica dello spazio degli stati MIU.
    /// Ogni bordo corrisponde all'applicazione di una regola da uno stato all'altro.
    /// </summary>
    public class MIUStringTopologyEdge
    {
        /// <summary>
        /// ID univoco del bordo. Corrisponde all'ApplicationID nella tabella MIU_RuleApplications.
        /// Sarà generato dal database.
        /// </summary>
        public long ApplicationID { get; set; }

        /// <summary>
        /// L'ID dello stato genitore (SourceNode) da cui parte la derivazione.
        /// Corrisponde all'StateID di un MIUStringTopologyNode.
        /// </summary>
        public long ParentStateID { get; set; }

        /// <summary>
        /// L'ID dello stato figlio (TargetNode) a cui arriva la derivazione.
        /// Corrisponde all'StateID di un MIUStringTopologyNode.
        /// </summary>
        public long NewStateID { get; set; }

        /// <summary>
        /// L'ID della regola MIU che è stata applicata per creare questa transizione.
        /// </summary>
        public long AppliedRuleID { get; set; }

        /// <summary>
        /// Il nome della regola applicata (per facilità di visualizzazione).
        /// </summary>
        public string AppliedRuleName { get; set; }

        /// <summary>
        /// Il "peso" o costo associato a questa transizione.
        /// Può essere basato sulla probabilità di successo, complessità della regola, ecc.
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// La profondità del nodo di destinazione.
        /// </summary>
        public int CurrentDepth { get; set; }

        /// <summary>
        /// L'ID della ricerca a cui appartiene questa specifica applicazione di regola.
        /// </summary>
        public long SearchID { get; set; }

        /// <summary>
        /// Il timestamp preciso in cui questa applicazione di regola è stata registrata.
        /// Usato per il filtraggio temporale e il decadimento del peso.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Costruttore di default.
        /// </summary>
        public MIUStringTopologyEdge() { }

        /// <summary>
        /// Costruttore per creare un MIUStringTopologyEdge durante l'esplorazione.
        /// L'ApplicationID sarà impostato dopo la persistenza nel database.
        /// </summary>
        /// <param name="parentStateID">ID dello stato genitore.</param>
        /// <param name="newStateID">ID dello stato figlio.</param>
        /// <param name="appliedRuleID">ID della regola applicata.</param>
        /// <param name="appliedRuleName">Nome della regola applicata.</param>
        /// <param name="currentDepth">Profondità del nuovo stato.</param>
        /// <param name="weight">Peso della transizione (default 1.0).</param>
        public MIUStringTopologyEdge(long parentStateID, long newStateID, long appliedRuleID, string appliedRuleName, int currentDepth, double weight = 1.0)
        {
            ParentStateID = parentStateID;
            NewStateID = newStateID;
            AppliedRuleID = appliedRuleID;
            AppliedRuleName = appliedRuleName;
            CurrentDepth = currentDepth;
            Weight = weight;
        }

        // Override di Equals e GetHashCode per confrontare i bordi in base a source, target e regola.
        public override bool Equals(object obj)
        {
            return obj is MIUStringTopologyEdge edge &&
                   ParentStateID == edge.ParentStateID &&
                   NewStateID == edge.NewStateID &&
                   AppliedRuleID == edge.AppliedRuleID;
        }

        public override int GetHashCode()
        {
            // Correzione per .NET Framework 4.8: utilizzare una combinazione manuale degli hash
            unchecked // Permette l'overflow senza sollevare eccezioni
            {
                int hash = 17; // Numero primo iniziale
                hash = hash * 23 + ParentStateID.GetHashCode();
                hash = hash * 23 + NewStateID.GetHashCode();
                hash = hash * 23 + AppliedRuleID.GetHashCode();
                return hash;
            }
        }
    }

    /// <summary>
    /// Contenitore principale per tutti i nodi e i bordi che formano la mappa topologica dello spazio degli stati MIU.
    /// Questo oggetto verrà popolato dal motore e poi serializzato e salvato nel database.
    /// </summary>
    public class MIUStringTopologyData
    {
        /// <summary>
        /// L'ID della ricerca associata a questa esplorazione della mappa topologica.
        /// Corrisponde a SearchID nella tabella MIU_Searches.
        /// </summary>
        public long SearchID { get; set; }

        /// <summary>
        /// La stringa MIU iniziale da cui è stata generata la mappa topologica.
        /// </summary>
        public string InitialString { get; set; }

        /// <summary>
        /// La profondità massima fino alla quale è stata esplorata la mappa topologica.
        /// </summary>
        public int MaxDepthExplored { get; set; }

        /// <summary>
        /// La lista di tutti i nodi (stati MIU) nella mappa topologica.
        /// </summary>
        public List<MIUStringTopologyNode> Nodes { get; set; } = new List<MIUStringTopologyNode>();

        /// <summary>
        /// La lista di tutti i bordi (transizioni/applicazioni di regole) nella mappa topologica.
        /// </summary>
        public List<MIUStringTopologyEdge> Edges { get; set; } = new List<MIUStringTopologyEdge>();

        /// <summary>
        /// Il timestamp di quando la mappa topologica è stata generata.
        /// </summary>
        public DateTime GenerationTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Costruttore di default.
        /// </summary>
        public MIUStringTopologyData() { }

        /// <summary>
        /// Costruttore per creare un'istanza di MIUStringTopologyData.
        /// </summary>
        /// <param name="searchID">L'ID della ricerca associata.</param>
        /// <param name="initialString">La stringa iniziale.</param>
        /// <param name="maxDepthExplored">La profondità massima esplorata.</param>
        public MIUStringTopologyData(long searchID, string initialString, int maxDepthExplored)
        {
            SearchID = searchID;
            InitialString = initialString;
            MaxDepthExplored = maxDepthExplored;
        }
    }
}
