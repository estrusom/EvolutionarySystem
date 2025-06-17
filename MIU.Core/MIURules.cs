//creata 10.6.2025 10.27
using System.Collections.Generic;

namespace MIU.Core.Rules // Assicurati che questo sia il namespace corretto per le tue regole
{
    /// <summary>
    /// Interfaccia che tutte le regole MIU devono implementare.
    /// Questo permette di trattare le regole in modo polimorfico,
    /// sia quelle definite staticamente che quelle generate dinamicamente con Roslyn.
    /// </summary>
    public interface IRegolaMIU
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        bool IsApplicable(string miuString);
        string Apply(string miuString);
    }

    /// <summary>
    /// Contenitore per l'insieme di tutte le regole MIU disponibili (R1, R2, R3, R4).
    /// Gestisce la collezione di regole implementate dall'interfaccia IRegolaMIU.
    /// </summary>
    public class MIURuleSet
    {
        public List<IRegolaMIU> Rules { get; private set; }

        public MIURuleSet()
        {
            Rules = new List<IRegolaMIU>();
            // A runtime, le regole generate da Roslyn verranno aggiunte qui.
            // Esempio:
            // Rules.Add(new Rule1()); // Se Rule1 implementa IRegolaMIU
        }

        /// <summary>
        /// Aggiunge una regola MIU all'insieme.
        /// </summary>
        /// <param name="rule">La regola da aggiungere, che deve implementare IRegolaMIU.</param>
        public void AddRule(IRegolaMIU rule)
        {
            Rules.Add(rule);
        }
    }
}
