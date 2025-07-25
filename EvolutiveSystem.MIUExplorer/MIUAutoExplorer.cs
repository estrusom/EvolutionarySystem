// EvolutiveSystem.Explorer/MIUAutoExplorer.cs
// Data di riferimento: 26 luglio 2025 (Aggiornato per usare RegolaMIU e MiuStateInfo)
// Descrizione: Implementa un esploratore automatico per il sistema MIU,
//              capace di trovare un percorso di derivazione tra due stati.
//              Refactorizzato per utilizzare RegolaMIU e MiuStateInfo per coerenza.

using System;
using System.Collections.Generic; // Necessario per Queue e HashSet.
using EvolutiveSystem.Common;     // MODIFICA: Per RegolaMIU, MiuStateInfo.
using System.Linq;                // Per metodi LINQ come Select.

namespace EvolutiveSystem.Explorer
{
    /// <summary>
    /// Rappresenta un nodo nel grafo di esplorazione, utile per ricostruire il percorso.
    /// Ora utilizza MiuStateInfo e RegolaMIU.
    /// </summary>
    public class ExplorationNode
    {
        public MiuStateInfo State { get; }      // MODIFICA: Lo stato MIU rappresentato da questo nodo (ora MiuStateInfo).
        public ExplorationNode Parent { get; }   // Il nodo precedente nel percorso.
        public RegolaMIU RuleApplied { get; }    // MODIFICA: La regola usata per raggiungere questo stato dal padre (ora RegolaMIU).
        public int Depth { get; }                // AGGIUNTA: Profondità del nodo nell'esplorazione.

        /// <summary>
        /// Costruisce un nuovo nodo di esplorazione.
        /// </summary>
        /// <param name="state">Lo stato MIU di questo nodo.</param>
        /// <param name="parent">Il nodo genitore (precedente).</param>
        /// <param name="ruleApplied">La regola applicata per arrivare a questo stato.</param>
        /// <param name="depth">La profondità di questo nodo.</param>
        public ExplorationNode(MiuStateInfo state, ExplorationNode parent = null, RegolaMIU ruleApplied = null, int depth = 0) // MODIFICA: Usa MiuStateInfo e RegolaMIU, aggiunto parametro depth
        {
            State = state ?? throw new ArgumentNullException(nameof(state), "Lo stato del nodo non può essere nullo.");
            Parent = parent;
            RuleApplied = ruleApplied;
            Depth = depth; // AGGIUNTA: Inizializza la profondità
        }
    }

    /// <summary>
    /// L'esploratore automatico del sistema MIU. Utilizza un algoritmo di ricerca
    /// per trovare un percorso di derivazione da uno stato iniziale a uno stato obiettivo.
    /// Refactorizzato per utilizzare RegolaMIU e MiuStateInfo.
    /// </summary>
    public class MIUAutoExplorer
    {
        private readonly List<RegolaMIU> _rules; // MODIFICA: L'insieme delle regole MIU disponibili (ora RegolaMIU).

        /// <summary>
        /// Inizializza una nuova istanza dell'esploratore con un set di regole.
        /// </summary>
        /// <param name="rules">La lista delle regole MIU da utilizzare per l'esplorazione.</param>
        /// <exception cref="ArgumentNullException">Lanciata se la lista di regole è nulla.</exception>
        /// <exception cref="ArgumentException">Lanciata se la lista di regole è vuota.</exception>
        public MIUAutoExplorer(List<RegolaMIU> rules) // MODIFICA: Accetta List<RegolaMIU> nel costruttore.
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules), "Le regole non possono essere nulle.");
            if (_rules.Count == 0)
            {
                throw new ArgumentException("La lista delle regole non può essere vuota.", nameof(rules));
            }
        }

        /// <summary>
        /// Trova un percorso di derivazione dallo stato iniziale a quello obiettivo
        /// utilizzando una ricerca in ampiezza (BFS).
        /// Questo metodo è stato refactorizzato per utilizzare MiuStateInfo e RegolaMIU.
        /// </summary>
        /// <param name="startStateInfo">Lo stato MIU di partenza (MiuStateInfo).</param>
        /// <param name="targetStateInfo">Lo stato MIU obiettivo (MiuStateInfo).</param>
        /// <param name="maxDepth">La profondità massima di ricerca per prevenire loop infiniti o ricerche eccessivamente lunghe.</param>
        /// <returns>
        /// Una lista di tuple (MiuStateInfo State, RegolaMIU RuleApplied) che rappresentano il percorso dal target al start,
        /// oppure null se nessun percorso viene trovato entro la profondità massima.
        /// </returns>
        public List<(MiuStateInfo State, RegolaMIU RuleApplied)> GetDerivationPath(MiuStateInfo startStateInfo, MiuStateInfo targetStateInfo, int maxDepth = 1000) // MODIFICA: Usa MiuStateInfo e RegolaMIU
        {
            // Validazione degli input.
            if (startStateInfo == null) throw new ArgumentNullException(nameof(startStateInfo));
            if (targetStateInfo == null) throw new ArgumentNullException(nameof(targetStateInfo));

            // Se lo stato iniziale è già lo stato obiettivo.
            if (startStateInfo.CurrentString.Equals(targetStateInfo.CurrentString, StringComparison.Ordinal))
            {
                Console.WriteLine($"Stato iniziale '{startStateInfo.CurrentString}' è già lo stato obiettivo.");
                return new List<(MiuStateInfo State, RegolaMIU RuleApplied)> { (startStateInfo, null) };
            }

            // Coda per la ricerca in ampiezza (BFS).
            Queue<ExplorationNode> queue = new Queue<ExplorationNode>();
            // HashSet per tenere traccia delle stringhe degli stati già visitati e prevenire loop.
            HashSet<string> visitedStrings = new HashSet<string>();

            // Inizializza la coda con il nodo di partenza.
            ExplorationNode initialNode = new ExplorationNode(startStateInfo, depth: 0);
            queue.Enqueue(initialNode);
            visitedStrings.Add(startStateInfo.CurrentString);

            Console.WriteLine($"Inizio ricerca da '{startStateInfo.CurrentString}' a '{targetStateInfo.CurrentString}' (Max Profondità: {maxDepth})");

            while (queue.Any()) // Continua finché ci sono nodi da esplorare.
            {
                ExplorationNode currentNode = queue.Dequeue(); // Prendi il nodo corrente dalla coda.

                // Verifica la profondità dopo aver estratto il nodo, prima di espanderlo
                if (currentNode.Depth >= maxDepth)
                {
                    // Non loggare qui, il log di "Nessun percorso trovato" è gestito più avanti
                    continue; // Salta l'espansione di questo nodo se ha già raggiunto o superato la profondità massima
                }

                // Per ogni regola disponibile, prova ad applicarla.
                foreach (RegolaMIU rule in _rules)
                {
                    string newString;
                    // Utilizza il metodo TryApply della RegolaMIU
                    if (rule.TryApply(currentNode.State.CurrentString, out newString))
                    {
                        // Se la nuova stringa è uguale alla stringa corrente, la regola non ha avuto effetto utile
                        if (newString.Equals(currentNode.State.CurrentString, StringComparison.Ordinal))
                        {
                            continue; // Salta questa applicazione, non ha prodotto un nuovo stato
                        }

                        // Se il nuovo stato non è stato ancora visitato.
                        if (visitedStrings.Add(newString))
                        {
                            // Crea un nuovo MiuStateInfo per la nuova stringa
                            MiuStateInfo newStateInfo = new MiuStateInfo(newString);

                            ExplorationNode newNode = new ExplorationNode(newStateInfo, currentNode, rule, currentNode.Depth + 1);

                            // Se il nuovo stato è lo stato obiettivo, abbiamo trovato un percorso!
                            if (newStateInfo.CurrentString.Equals(targetStateInfo.CurrentString, StringComparison.Ordinal))
                            {
                                Console.WriteLine($"Percorso trovato! Obiettivo '{targetStateInfo.CurrentString}' raggiunto a profondità {newNode.Depth}.");
                                return ReconstructPath(newNode); // Ricostruisci e restituisci il percorso.
                            }

                            queue.Enqueue(newNode); // Aggiungi il nuovo nodo alla coda per l'esplorazione futura.
                        }
                    }
                }
            }

            // Questo return è cruciale per risolvere l'errore CS0161.
            Console.WriteLine($"Nessun percorso trovato dopo aver esplorato tutti gli stati raggiungibili o raggiunta la profondità massima di {maxDepth}.");
            return null; // Nessun percorso trovato.
        }

        /// <summary>
        /// Ricostruisce il percorso dallo stato finale al nodo iniziale risalendo i genitori.
        /// </summary>
        /// <param name="targetNode">Il nodo che rappresenta lo stato obiettivo trovato.</param>
        /// <returns>Una lista ordinata di (MiuStateInfo State, RegolaMIU RuleApplied) dal nodo iniziale al nodo obiettivo.</returns>
        private List<(MiuStateInfo State, RegolaMIU RuleApplied)> ReconstructPath(ExplorationNode targetNode)
        {
            List<(MiuStateInfo State, RegolaMIU RuleApplied)> path = new List<(MiuStateInfo State, RegolaMIU RuleApplied)>();
            ExplorationNode current = targetNode;

            // Risali dal nodo obiettivo al nodo iniziale.
            while (current != null)
            {
                // Aggiungi il nodo corrente all'inizio della lista.
                path.Insert(0, (current.State, current.RuleApplied));
                current = current.Parent;
            }

            // La prima tupla nel percorso avrà RuleApplied = null (stato iniziale).
            return path;
        }
    }
}
