// File: MIU.Core/IMIUTopologyService.cs
// Data di riferimento: 26 giugno 2025
// Descrizione: Interfaccia che definisce il contratto per il servizio di gestione
//              e costruzione della topologia dello spazio degli stati MIU.
//              Questo servizio sarà responsabile di assemblare i dati della topologia
//              da fonti granulari e di applicare la logica di "pesatura" e "fluttuazione" temporale.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvolutiveSystem.Common; // Per MIUStringTopologyData, MIUStringTopologyNode, MIUStringTopologyEdge

namespace MIU.Core
{
    /// <summary>
    /// Definisce le operazioni per la gestione e il caricamento della topologia
    /// dello spazio degli stati del sistema MIU.
    /// </summary>
    public interface IMIUTopologyService
    {
        /// <summary>
        /// Carica i dati della topologia dello spazio degli stati MIU, inclusi nodi, bordi e pesi.
        /// Consente il filtraggio temporale e per profondità, per supportare visualizzazioni dinamiche e "a film".
        /// Questa operazione assembla i dati da diverse fonti di persistenza granulare.
        /// </summary>
        /// <param name="initialString">La stringa iniziale della ricerca per cui caricare la topologia.
        ///                               Se nullo, carica la topologia aggregata di tutte le ricerche rilevanti.</param>
        /// <param name="startDate">Data di inizio opzionale per filtrare gli eventi (nodi e bordi) in base al timestamp.</param>
        /// <param name="endDate">Data di fine opzionale per filtrare gli eventi.</param>
        /// <param name="maxDepth">Profondità massima opzionale per limitare l'esplorazione del grafo.</param>
        /// <returns>Un oggetto MIUStringTopologyData contenente nodi e bordi della topologia.</returns>
        Task<MIUStringTopologyData> LoadMIUStringTopologyAsync(
            string initialString = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? maxDepth = null
        );

        // Potrebbero essere aggiunti qui altri metodi in futuro, ad esempio:
        // - Metodi per l'analisi dei vuoti nella topologia.
        // - Metodi per l'identificazione di cicli o pattern negli "infiniti".
        // - Metodi per calcolare metriche specifiche sulla topologia.
    }
}
