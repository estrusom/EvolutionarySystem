using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutiveSystem.Taxonomy
{
    // File: EvolutiveSystem.Taxonomy/RuleTaxonomy.cs
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Rappresenta una tassonomia di regole MIU.
    /// Contiene un elenco di nodi radice, ciascuno dei quali può avere figli,
    /// formando una struttura ad albero.
    /// </summary>
    public class RuleTaxonomy
    {
        /// <summary>
        /// ID univoco della tassonomia.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome della tassonomia (es. "Tassonomia Efficacia Regole").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descrizione della tassonomia.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Data e ora dell'ultima generazione/aggiornamento della tassonomia.
        /// </summary>
        public DateTime LastGenerated { get; set; }

        /// <summary>
        /// L'elenco dei nodi radice di questa tassonomia.
        /// </summary>
        public List<RuleTaxonomyNode> RootNodes { get; set; } = new List<RuleTaxonomyNode>();

        public RuleTaxonomy()
        {
            LastGenerated = DateTime.UtcNow;
        }
    }
    
}
