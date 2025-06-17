//creata 10.6.2025 10.04
using System;
using System.Collections.Generic;
using MIU.Core; // CORRETTO: Namespace per RegolaMIU

namespace MIU.Core.Topology.Map // Namespace per la classe Rule
{
    /// <summary>
    /// Rappresenta una "regola" (un collegamento o arco diretto) nella mappa topologica.
    /// Unisce uno stato di origine a uno stato di destinazione tramite l'applicazione di una RegolaMIU.
    /// Contiene metadati e metriche quantitative sull'applicazione della regola.
    /// </summary>
    public class Rule
    {
        /// <summary>
        /// Identificatore unico per l'istanza della regola (potrebbe essere un GUID o un hash combinato).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// L'ID dello stato di origine da cui la regola è stata applicata.
        /// </summary>
        public string SourceStateId { get; set; }

        /// <summary>
        /// L'ID dello stato di destinazione raggiunto applicando la regola.
        /// </summary>
        public string TargetStateId { get; set; }

        /// <summary>
        /// Riferimento alla RegolaMIU effettivamente applicata (es. R1, R2, R3, R4).
        /// Questo ci permette di recuperare i dettagli della regola base.
        /// </summary>
        public RegolaMIU AppliedMIURule { get; set; }

        /// <summary>
        /// Unix timestamp: quando questa specifica applicazione della regola è avvenuta.
        /// </summary>
        public long ApplicationTimestamp { get; set; }

        /// <summary>
        /// Indica la frequenza con cui questa specifica transizione (Source -> Target tramite questa regola) è stata osservata.
        /// </summary>
        public int ApplicationCount { get; set; }

        /// <summary>
        /// Il costo intrinseco di applicazione di questa regola (es. costo computazionale, complessità).
        /// </summary>
        public double ApplicationCost { get; set; }

        /// <summary>
        /// Indica se questa applicazione della regola ha portato a uno stato "nuovo" (non ancora visitato)
        /// o a uno stato già conosciuto.
        /// </summary>
        public bool LeadsToNewState { get; set; }

        /// <summary>
        /// Profondità del nodo target relativa all'albero di esplorazione, utile per visualizzare la "lunghezza" del percorso.
        /// </summary>
        public int Depth { get; set; }

        // Costruttore per inizializzare una nuova istanza di Rule.
        public Rule(string sourceStateId, string targetStateId, RegolaMIU appliedMIURule, long applicationTimestamp, bool leadsToNewState, int depth)
        {
            if (string.IsNullOrEmpty(sourceStateId))
                throw new ArgumentNullException(nameof(sourceStateId), "L'ID dello stato di origine non può essere null o vuoto.");
            if (string.IsNullOrEmpty(targetStateId))
                throw new ArgumentNullException(nameof(targetStateId), "L'ID dello stato di destinazione non può essere null o vuoto.");
            if (appliedMIURule == null)
                throw new ArgumentNullException(nameof(appliedMIURule), "La RegolaMIU applicata non può essere null.");

            // L'ID della regola può essere una combinazione degli ID degli stati e dell'ID della regola MIU
            // per garantirne l'unicità per una specifica transizione.
            Id = $"{sourceStateId}-{appliedMIURule.ID}-{targetStateId}"; // Usiamo rule.ID qui

            SourceStateId = sourceStateId;
            TargetStateId = targetStateId;
            AppliedMIURule = appliedMIURule;
            ApplicationTimestamp = applicationTimestamp;
            LeadsToNewState = leadsToNewState;
            Depth = depth;

            // Inizializzazione delle metriche
            ApplicationCount = 1; // Ogni volta che istanziamo, è stata applicata almeno una volta
            // Assumiamo che la RegolaMIU abbia una proprietà per il costo intrinseco, altrimenti useremo un valore predefinito
            // Se RegolaMIU non ha 'Cost', puoi commentare questa riga o usare un valore fisso es. 1.0
            ApplicationCost = 1.0; // Valore predefinito, da aggiornare se RegolaMIU ha una proprietà 'Cost'
        }

        // Sovrascrivi Equals e GetHashCode per consentire un confronto corretto
        // e l'uso in collezioni, basato sull'Id.
        public override bool Equals(object obj)
        {
            return obj is Rule other && Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"Regola: {AppliedMIURule.Nome} ({SourceStateId.Substring(0, Math.Min(10, SourceStateId.Length))}... -> {TargetStateId.Substring(0, Math.Min(10, TargetStateId.Length))}...), Costo: {ApplicationCost}";
        }
    }
}
