// File: IMIUDataService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using EvolutionarySystem.Core.Models;

namespace EvolutionarySystem.Core.Models
{
    /// <summary>
    /// Interfaccia per la gestione dei dati necessari per la costruzione del grafo topologico.
    /// Questo approccio permette la separazione delle responsabilità e facilita i test.
    /// </summary>
    public interface IMIUDataService
    {
        /// <summary>
        /// Recupera tutti i percorsi MIU e le regole applicate dal database.
        /// </summary>
        /// <returns>Un oggetto contenente liste di MIUPath e MIURuleApplication.</returns>
        Task<(List<MIUPath> Paths, List<MIURuleApplication> RuleApplications)> GetAllDataAsync();
    }
}
