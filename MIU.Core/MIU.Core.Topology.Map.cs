// Creata il 10.6.2025 1.04
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MIU.Core; // CORRETTO: Namespace per RegolaMIU

namespace MIU.Core.Topology.Map // Namespace aggiornato
{
    /// <summary>
    /// Rappresenta la mappa topologica completa dei processi MIU,
    /// composta da una collezione di stati (nodi) e regole (archi).
    /// Questa classe gestisce l'aggiunta e l'accesso agli elementi della mappa.
    /// </summary>
    public class TopologicalMap
    {
        // Utilizziamo un Dictionary per gli stati per un accesso efficiente tramite ID.
        // Un ConcurrentDictionary potrebbe essere usato per scenari multi-threading,
        // ma per ora un Dictionary standard è sufficiente.
        private Dictionary<string, State> _states;

        // Utilizziamo un Dictionary per le regole per un accesso efficiente tramite ID.
        private Dictionary<string, Rule> _rules;

        /// <summary>
        /// Ottiene la collezione di tutti gli stati presenti nella mappa.
        /// È una copia per prevenire modifiche esterne dirette.
        /// </summary>
        public IReadOnlyDictionary<string, State> States => _states;

        /// <summary>
        /// Ottiene la collezione di tutte le regole presenti nella mappa.
        /// È una copia per prevenire modifiche esterne dirette.
        /// </summary>
        public IReadOnlyDictionary<string, Rule> Rules => _rules;

        /// <summary>
        /// Costruttore predefinito per la mappa topologica.
        /// Inizializza le collezioni interne.
        /// </summary>
        public TopologicalMap()
        {
            _states = new Dictionary<string, State>();
            _rules = new Dictionary<string, Rule>();
        }

        /// <summary>
        /// Aggiunge un nuovo stato alla mappa. Se uno stato con lo stesso ID esiste già,
        /// non viene aggiunto, ma la sua VisitCount viene incrementata.
        /// </summary>
        /// <param name="state">Lo stato da aggiungere.</param>
        /// <returns>True se lo stato è stato aggiunto (era nuovo), False se esisteva già.</returns>
        public bool AddOrUpdateState(State state)
        {
            if (state == null)
            {
                Console.WriteLine("Tentativo di aggiungere uno stato nullo.");
                return false;
            }

            if (_states.ContainsKey(state.Id))
            {
                // Stato già presente, aggiorna solo il contatore di visite.
                _states[state.Id].VisitCount++; // <- errore cs1061 su VisitCount
                Console.WriteLine($"Stato '{state.Label}' (ID: {state.Id.Substring(0, Math.Min(10, state.Id.Length))}...) già presente. VisitCount incrementato a {_states[state.Id].VisitCount}."); // <- errore cs1061 su Label
                return false;
            }
            else
            {
                // Stato nuovo, aggiungilo e imposta il VisitCount a 1.
                state.VisitCount = 1; // La prima volta che viene aggiunto, è stato visitato una volta.
                _states.Add(state.Id, state);
                Console.WriteLine($"Nuovo stato '{state.Label}' (ID: {state.Id.Substring(0, Math.Min(10, state.Id.Length))}...) aggiunto alla mappa.");
                return true;
            }
        }

        /// <summary>
        /// Aggiunge una nuova regola (arco) alla mappa. Se una regola con lo stesso ID esiste già,
        /// non viene aggiunta, ma la sua ApplicationCount viene incrementata.
        /// </summary>
        /// <param name="rule">La regola da aggiungere.</param>
        /// <returns>True se la regola è stata aggiunta (era nuova), False se esisteva già.</returns>
        public bool AddOrUpdateRule(Rule rule)
        {
            if (rule == null)
            {
                Console.WriteLine("Tentativo di aggiungere una regola nulla.");
                return false;
            }

            if (_rules.ContainsKey(rule.Id))
            {
                // Regola già presente, aggiorna solo il contatore di applicazioni.
                _rules[rule.Id].ApplicationCount++;
                Console.WriteLine($"Regola '{rule.AppliedMIURule.Nome}' (ID: {rule.Id.Substring(0, Math.Min(10, rule.Id.Length))}...) già presente. ApplicationCount incrementato a {_rules[rule.Id].ApplicationCount}.");  // <- errore cs1061 su Name
                return false;
            }
            else
            {
                // Regola nuova, aggiungila e imposta l'ApplicationCount a 1.
                rule.ApplicationCount = 1; // La prima volta che viene aggiunta, è stata applicata una volta.
                _rules.Add(rule.Id, rule);
                Console.WriteLine($"Nuova regola '{rule.AppliedMIURule.Nome}' (ID: {rule.Id.Substring(0, Math.Min(10, rule.Id.Length))}...) aggiunta alla mappa.");
                return true;
            }
        }

        /// <summary>
        /// Tenta di recuperare uno stato dalla mappa dato il suo ID.
        /// </summary>
        /// <param name="stateId">L'ID dello stato da cercare.</param>
        /// <param name="state">Lo stato trovato, o null se non trovato.</param>
        /// <returns>True se lo stato è stato trovato, False altrimenti.</returns>
        public bool TryGetState(string stateId, out State state)
        {
            return _states.TryGetValue(stateId, out state);
        }

        /// <summary>
        /// Tenta di recuperare una regola dalla mappa dato il suo ID.
        /// </summary>
        /// <param name="ruleId">L'ID della regola da cercare.</param>
        /// <param name="rule">La regola trovata, o null se non trovata.</param>
        /// <returns>True se la regola è stata trovata, False altrimenti.</returns>
        public bool TryGetRule(string ruleId, out Rule rule)
        {
            return _rules.TryGetValue(ruleId, out rule);
        }

        /// <summary>
        /// Restituisce tutti gli stati che sono raggiungibili direttamente da uno stato sorgente.
        /// </summary>
        /// <param name="sourceStateId">L'ID dello stato sorgente.</param>
        /// <returns>Una lista di stati di destinazione.</returns>
        public IEnumerable<State> GetStatesReachableFrom(string sourceStateId)
        {
            // Trova tutte le regole che partono dallo stato sorgente
            var outgoingRules = _rules.Values.Where(r => r.SourceStateId == sourceStateId);

            // Per ogni regola, recupera lo stato di destinazione
            foreach (var rule in outgoingRules)
            {
                if (_states.TryGetValue(rule.TargetStateId, out State targetState))
                {
                    yield return targetState;
                }
            }
        }

        /// <summary>
        /// Restituisce tutte le regole che partono da uno stato sorgente.
        /// </summary>
        /// <param name="sourceStateId">L'ID dello stato sorgente.</param>
        /// <returns>Una lista di regole in uscita.</returns>
        public IEnumerable<Rule> GetOutgoingRules(string sourceStateId)
        {
            return _rules.Values.Where(r => r.SourceStateId == sourceStateId);
        }

        /// <summary>
        /// Restituisce tutte le regole che arrivano ad uno stato di destinazione.
        /// </summary>
        /// <param name="targetStateId">L'ID dello stato di destinazione.</param>
        /// <returns>Una lista di regole in ingresso.</returns>
        public IEnumerable<Rule> GetIncomingRules(string targetStateId)
        {
            return _rules.Values.Where(r => r.TargetStateId == targetStateId);
        }

        /// <summary>
        /// Resetta la mappa, rimuovendo tutti gli stati e le regole.
        /// </summary>
        public void Clear()
        {
            _states.Clear();
            _rules.Clear();
            Console.WriteLine("Mappa topologica resettata: tutti gli stati e le regole sono stati rimossi.");
        }

        /// <summary>
        /// Restituisce il numero totale di stati nella mappa.
        /// </summary>
        public int StateCount => _states.Count;

        /// <summary>
        /// Restituisce il numero totale di regole (archi) nella mappa.
        /// </summary>
        public int RuleCount => _rules.Count;

        public override string ToString()
        {
            return $"Mappa Topologica: {StateCount} stati e {RuleCount} regole.";
        }
        //2025.06.12
        // NUOVA COLLEZIONE: Per memorizzare le statistiche delle regole
        // Usiamo ConcurrentDictionary per la sicurezza dei thread, se l'applicazione è multi-threaded.
        // L'ID della regola (Guid) è la chiave, RuleStatistics è il valore.
        private readonly ConcurrentDictionary<Guid, RuleStatistics> _ruleStatistics = new ConcurrentDictionary<Guid, RuleStatistics>();

        // NUOVO METODO: Per ottenere o creare statistiche di una regola
        public RuleStatistics GetOrCreateRuleStatistics(Guid ruleId)
        {
            // GetOrAdd è un metodo thread-safe di ConcurrentDictionary.
            // Se la chiave esiste, restituisce il valore esistente.
            // Se la chiave non esiste, aggiunge un nuovo RuleStatistics e lo restituisce.
            return _ruleStatistics.GetOrAdd(ruleId, new RuleStatistics(ruleId));
        }

    }
}
