// EvolutiveSystem.Logic/MIURulesEngine.cs
// Data di riferimento: 26 giugno 2025
// Descrizione: Gestisce l'applicazione delle regole MIU a uno stato dato.
//              Fornisce metodi per trovare le regole applicabili e per derivare nuovi stati.

using System;
using System.Collections.Generic; // Per List<T>
using System.Linq;                // Per LINQ (ad es. Where)
using EvolutiveSystem.Common;     // Per MIUState e MIURule

namespace EvolutiveSystem.Logic
{
    /// <summary>
    /// Motore di regole per il sistema MIU.
    /// Questo motore incapsula la logica per determinare quali regole sono applicabili
    /// a un dato stato e per applicare tali regole per generare nuovi stati.
    /// L'errore CS1501 sul GetHashCode in questo file suggeriva un uso improprio
    /// delle funzioni di hashing su oggetti MIUState. La soluzione è garantire che
    /// il codice non tenti di passare argomenti a GetHashCode.
    /// </summary>
    public class MIURulesEngine
    {
        private readonly List<MIURule> _allRules; // Tutte le regole disponibili nel sistema.

        /// <summary>
        /// Inizializza una nuova istanza del MIURulesEngine con un set di regole.
        /// </summary>
        /// <param name="rules">La lista delle regole MIU da utilizzare. Non può essere nulla o vuota.</param>
        /// <exception cref="ArgumentNullException">Lanciata se la lista di regole è nulla.</exception>
        /// <exception cref="ArgumentException">Lanciata se la lista di regole è vuota.</exception>
        public MIURulesEngine(List<MIURule> rules)
        {
            _allRules = rules ?? throw new ArgumentNullException(nameof(rules), "La lista delle regole non può essere nulla.");
            if (_allRules.Count == 0)
            {
                throw new ArgumentException("La lista delle regole non può essere vuota.", nameof(rules));
            }
        }

        /// <summary>
        /// Trova tutte le regole MIU applicabili a un dato stato.
        /// </summary>
        /// <param name="state">Lo stato MIU da analizzare.</param>
        /// <returns>Una lista di regole MIU che possono essere applicate allo stato dato.</returns>
        /// <exception cref="ArgumentNullException">Lanciata se lo stato è nullo.</exception>
        public List<MIURule> GetApplicableRules(MIUState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state), "Lo stato non può essere nullo per determinare le regole applicabili.");
            }

            // Utilizza LINQ per filtrare le regole che sono applicabili allo stato corrente.
            // Qui NON si chiama GetHashCode con argomenti; l'uguaglianza e l'hashing di MIUState
            // sono gestiti internamente dalla sua implementazione di Equals e GetHashCode.
            return _allRules.Where(rule => rule.IsApplicable(state)).ToList();
        }

        /// <summary>
        /// Applica una specifica regola MIU a un dato stato per derivare un nuovo stato.
        /// </summary>
        /// <param name="state">Lo stato MIU di partenza.</param>
        /// <param name="rule">La regola MIU da applicare.</param>
        /// <returns>Il nuovo stato MIU derivato.</returns>
        /// <exception cref="ArgumentNullException">Lanciata se lo stato o la regola sono nulli.</exception>
        /// <exception cref="InvalidOperationException">Lanciata se la regola non è applicabile allo stato fornito.</exception>
        public MIUState ApplyRule(MIUState state, MIURule rule)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state), "Lo stato non può essere nullo per applicare una regola.");
            }
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule), "La regola non può essere nulla per essere applicata.");
            }

            // Verifica esplicita se la regola è applicabile.
            // Anche se Apply() della regola gestisce l'eccezione, è buona pratica verificarlo qui
            // per fornire un feedback più immediato se la pre-condizione non è soddisfatta.
            if (!rule.IsApplicable(state))
            {
                throw new InvalidOperationException($"La regola '{rule.Name}' non è applicabile allo stato '{state.CurrentString}'.");
            }

            return rule.Apply(state);
        }

        /// <summary>
        /// Deriva tutti i possibili stati successivi da un dato stato applicando tutte le regole applicabili.
        /// </summary>
        /// <param name="state">Lo stato MIU di partenza.</param>
        /// <returns>Una lista di stati MIU derivati.</returns>
        /// <exception cref="ArgumentNullException">Lanciata se lo stato è nullo.</exception>
        public List<MIUState> GetAllNextStates(MIUState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state), "Lo stato non può essere nullo per derivare stati successivi.");
            }

            List<MIUState> nextStates = new List<MIUState>();
            foreach (MIURule rule in _allRules)
            {
                if (rule.IsApplicable(state))
                {
                    try
                    {
                        MIUState newState = rule.Apply(state);
                        nextStates.Add(newState);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Questo blocco catch è principalmente per scopi di debugging.
                        // Non dovrebbe essere raggiunto se IsApplicable è corretto.
                        Console.WriteLine($"Errore inaspettato durante l'applicazione della regola '{rule.Name}' allo stato '{state.CurrentString}': {ex.Message}");
                    }
                }
            }
            return nextStates;
        }
    }
}
