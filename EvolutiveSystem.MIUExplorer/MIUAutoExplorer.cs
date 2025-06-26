// EvolutiveSystem.Explorer/MIUAutoExplorer.cs
// Data di riferimento: 26 giugno 2025
// Descrizione: Implementa un esploratore automatico per il sistema MIU,
//              capace di trovare un percorso di derivazione tra due stati.

using System;
using System.Collections.Generic; // Necessario per Queue e HashSet.
using EvolutiveSystem.Common;     // Per MIUState e MIURule.
using System.Linq;                // Per metodi LINQ come Select.

namespace EvolutiveSystem.Explorer
{
    /// <summary>
    /// Rappresenta un nodo nel grafo di esplorazione, utile per ricostruire il percorso.
    /// </summary>
    public class ExplorationNode
    {
        public MIUState State { get; }      // Lo stato MIU rappresentato da questo nodo.
        public ExplorationNode Parent { get; } // Il nodo precedente nel percorso.
        public MIURule RuleApplied { get; } // La regola usata per raggiungere questo stato dal padre.

        /// <summary>
        /// Costruisce un nuovo nodo di esplorazione.
        /// </summary>
        /// <param name="state">Lo stato MIU di questo nodo.</param>
        /// <param name="parent">Il nodo genitore (precedente).</param>
        /// <param name="ruleApplied">La regola applicata per arrivare a questo stato.</param>
        public ExplorationNode(MIUState state, ExplorationNode parent = null, MIURule ruleApplied = null)
        {
            State = state ?? throw new ArgumentNullException(nameof(state), "Lo stato del nodo non può essere nullo.");
            Parent = parent;
            RuleApplied = ruleApplied;
        }
    }

    /// <summary>
    /// L'esploratore automatico del sistema MIU. Utilizza un algoritmo di ricerca
    /// per trovare un percorso di derivazione da uno stato iniziale a uno stato obiettivo.
    /// Questo risolve l'errore 'MIUAutoExplorer' non contiene una definizione di 'GetDerivationPath'.
    /// </summary>
    public class MIUAutoExplorer
    {
        private readonly List<MIURule> _rules; // L'insieme delle regole MIU disponibili.

        /// <summary>
        /// Inizializza una nuova istanza dell'esploratore con un set di regole.
        /// </summary>
        /// <param name="rules">La lista delle regole MIU da utilizzare per l'esplorazione.</param>
        /// <exception cref="ArgumentNullException">Lanciata se la lista di regole è nulla.</exception>
        /// <exception cref="ArgumentException">Lanciata se la lista di regole è vuota.</exception>
        public MIUAutoExplorer(List<MIURule> rules)
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
        /// </summary>
        /// <param name="startState">Lo stato MIU di partenza.</param>
        /// <param name="targetState">Lo stato MIU obiettivo.</param>
        /// <param name="maxDepth">La profondità massima di ricerca per prevenire loop infiniti o ricerche eccessivamente lunghe.</param>
        /// <returns>
        /// Una lista di tuple (Stato, Regola) che rappresentano il percorso dal target al start,
        /// oppure null se nessun percorso viene trovato entro la profondità massima.
        /// La tupla finale sarà (TargetState, null). La prima tupla sarà (StartState, RuleUsedToReachIt).
        /// </returns>
        public List<(MIUState State, MIURule RuleApplied)> GetDerivationPath(MIUState startState, MIUState targetState, int maxDepth = 1000)
        {
            // Validazione degli input.
            if (startState == null) throw new ArgumentNullException(nameof(startState));
            if (targetState == null) throw new ArgumentNullException(nameof(targetState));

            // Se lo stato iniziale è già lo stato obiettivo, il percorso è solo lo stato iniziale.
            if (startState.Equals(targetState))
            {
                Console.WriteLine($"Stato iniziale '{startState.CurrentString}' è già lo stato obiettivo.");
                return new List<(MIUState State, MIURule RuleApplied)> { (startState, null) };
            }

            // Coda per la ricerca in ampiezza (BFS).
            Queue<ExplorationNode> queue = new Queue<ExplorationNode>();
            // HashSet per tenere traccia degli stati già visitati e prevenire loop.
            HashSet<MIUState> visitedStates = new HashSet<MIUState>();

            // Inizializza la coda con il nodo di partenza.
            ExplorationNode initialNode = new ExplorationNode(startState);
            queue.Enqueue(initialNode);
            visitedStates.Add(startState);

            int currentDepth = 0; // Contatore per la profondità di ricerca.

            Console.WriteLine($"Inizio ricerca da '{startState.CurrentString}' a '{targetState.CurrentString}' (Max Profondità: {maxDepth})");

            while (queue.Any()) // Continua finché ci sono nodi da esplorare.
            {
                int levelSize = queue.Count; // Numero di nodi a questa profondità.
                currentDepth++;

                if (currentDepth > maxDepth)
                {
                    Console.WriteLine($"Raggiunta la profondità massima di {maxDepth}. Nessun percorso trovato.");
                    return null; // Profondità massima raggiunta, interrompi la ricerca.
                }

                for (int i = 0; i < levelSize; i++)
                {
                    ExplorationNode currentNode = queue.Dequeue(); // Prendi il nodo corrente dalla coda.
                    // Console.WriteLine($"Esplorando stato: {currentNode.State.CurrentString} (Profondità: {currentDepth -1})"); // Debugging

                    // Per ogni regola disponibile, prova ad applicarla.
                    foreach (MIURule rule in _rules)
                    {
                        if (rule.IsApplicable(currentNode.State))
                        {
                            MIUState newState = rule.Apply(currentNode.State); // Applica la regola.

                            // Se il nuovo stato non è stato ancora visitato.
                            if (visitedStates.Add(newState)) // Add restituisce true se l'elemento è nuovo e viene aggiunto.
                            {
                                ExplorationNode newNode = new ExplorationNode(newState, currentNode, rule); // Crea un nuovo nodo.

                                // Se il nuovo stato è lo stato obiettivo, abbiamo trovato un percorso!
                                if (newState.Equals(targetState))
                                {
                                    Console.WriteLine($"Percorso trovato! Obiettivo '{targetState.CurrentString}' raggiunto.");
                                    return ReconstructPath(newNode); // Ricostruisci e restituisci il percorso.
                                }

                                queue.Enqueue(newNode); // Aggiungi il nuovo nodo alla coda per l'esplorazione futura.
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Nessun percorso trovato dopo aver esplorato tutti gli stati raggiungibili.");
            return null; // Nessun percorso trovato.
        }

        /// <summary>
        /// Ricostruisce il percorso dallo stato finale al nodo iniziale risalendo i genitori.
        /// </summary>
        /// <param name="targetNode">Il nodo che rappresenta lo stato obiettivo trovato.</param>
        /// <returns>Una lista ordinata di (Stato, Regola) dal nodo iniziale al nodo obiettivo.</returns>
        private List<(MIUState State, MIURule RuleApplied)> ReconstructPath(ExplorationNode targetNode)
        {
            List<(MIUState State, MIURule RuleApplied)> path = new List<(MIUState State, MIURule RuleApplied)>();
            ExplorationNode current = targetNode;

            // Risali dal nodo obiettivo al nodo iniziale.
            while (current != null)
            {
                // Aggiungi il nodo corrente all'inizio della lista.
                path.Insert(0, (current.State, current.RuleApplied));
                current = current.Parent;
            }

            // La prima tupla nel percorso avrà RuleApplied = null (stato iniziale).
            // Dobbiamo rimuoverla o gestire che il primo elemento non ha una regola che lo precede.
            // L'implementazione attuale include (StartState, null) che è corretto per indicare l'inizio.
            return path;
        }
    }
}
