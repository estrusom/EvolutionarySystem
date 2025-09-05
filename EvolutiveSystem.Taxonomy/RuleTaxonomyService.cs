// File: RuleTaxonomyService.cs
// Data di riferimento: 1 settembre 2025
// Questa classe gestisce la generazione e il salvataggio della tassonomia delle regole.

using EvolutiveSystem.Common.Contracts;
using EvolutiveSystem.Taxonomy.Contracts;
using EvolutiveSystem.Taxonomy.Models;
using EvolutiveSystem.SQL.Core;
using EvolutiveSystem.SQL.Data;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EvolutiveSystem.Taxonomy
{
    public class RuleTaxonomyService : IRuleTaxonomyService
    {
        private readonly IRuleTaxonomyDataStore _dataStore;

        /// <summary>
        /// Costruttore che accetta l'interfaccia di persistenza come dipendenza.
        /// </summary>
        /// <param name="dataStore">L'istanza del data store per salvare la tassonomia.</param>
        public RuleTaxonomyService(IRuleTaxonomyDataStore dataStore)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        }

        /// <summary>
        /// Genera una tassonomia di regole a partire da un insieme di stati e la salva.
        /// </summary>
        /// <param name="states">Gli stati di partenza per la generazione della tassonomia.</param>
        public async Task GenerateRuleTaxonomy(ICollection<State> states)
        {
            // TODO: Inserire qui la logica complessa per generare la tassonomia
            // a partire dagli stati. Per ora, creiamo un oggetto di esempio.
            var taxonomy = new RuleTaxonomy
            {
                TaxonomyId = Guid.NewGuid(),
                GenerationDate = DateTime.UtcNow,
                RootNode = new RuleTaxonomyNode()
            };

            // Utilizziamo il data store per salvare la tassonomia.
            // L'interfaccia _dataStore nasconde la complessità della logica di salvataggio.
            Console.WriteLine("Avvio del salvataggio della tassonomia...");
            await _dataStore.SaveTaxonomyAsync(taxonomy);
            Console.WriteLine("Tassonomia salvata con successo!");
        }
    }
}
