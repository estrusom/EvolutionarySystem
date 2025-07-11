// File: EvolutiveSystem.Taxonomy/RuleTaxonomyNode.cs
using System;
using System.Collections.Generic;

namespace EvolutiveSystem.Taxonomy // Nuovo Namespace!
{
    /// <summary>
    /// Rappresenta un singolo nodo all'interno di una tassonomia di regole MIU.
    /// Un nodo può categorizzare regole in base a criteri specifici e può avere nodi figli.
    /// </summary>
    public class RuleTaxonomyNode
    {
        /// <summary>
        /// ID univoco del nodo.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// ID del nodo padre (null per i nodi radice).
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// Nome del nodo (es. "Regole ad Alta Efficacia", "Regole Poco Usate").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descrizione del criterio di categorizzazione del nodo.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Elenco degli ID delle regole MIU che appartengono a questa categoria/nodo.
        /// </summary>
        public List<long> RuleIds { get; set; } = new List<long>();

        /// <summary>
        /// Elenco dei nodi figli, che rappresentano sottocategorie.
        /// </summary>
        public List<RuleTaxonomyNode> Children { get; set; } = new List<RuleTaxonomyNode>();

        public RuleTaxonomyNode()
        {
            // Genera un ID temporaneo per i nuovi nodi se non specificato
            // Potrebbe essere sostituito da un ID persistente dal database se salviamo la tassonomia
            Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); // Un modo semplice per un ID temporaneo
        }
    }
}
