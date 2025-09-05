// File: EvolutiveSystem.SQL.Core/ITassonomiaInterfaces.cs
// Data di riferimento: 1 settembre 2025
// Questo file contiene le interfacce per la logica di accesso al database,
// garantendo il disaccoppiamento dei progetti.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvolutiveSystem.SQL.Core
{
    /// <summary>
    /// Questa interfaccia definisce il contratto per l'analisi della tassonomia.
    /// Il suo unico scopo è astrarre la logica di accesso al database per le query di reportistica.
    /// </summary>
    public interface ITassonomiaAnalytics
    {
        /// <summary>
        /// Esegue la query di tassonomia e restituisce i risultati.
        /// </summary>
        /// <returns>Una lista di dizionari, dove ogni dizionario rappresenta una riga dei risultati.</returns>
        Task<List<Dictionary<string, object>>> GetTaxonomyResultsAsync();
    }

    /// <summary>
    /// Interfaccia che definisce il contratto per la persistenza della tassonomia.
    /// </summary>
    public interface IRuleTaxonomyDataStore
    {
        /// <summary>
        /// Salva la tassonomia generata.
        /// </summary>
        /// <param name="taxonomy">L'oggetto tassonomia da salvare (il tipo esatto non è definito qui per mantenere il disaccoppiamento).</param>
        Task SaveTaxonomyAsync(object taxonomy);
    }
}
