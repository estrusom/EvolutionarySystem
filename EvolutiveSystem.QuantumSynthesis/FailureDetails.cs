// File: EvolutiveSystem.QuantumSynthesis/FailureDetails.cs

using EvolutiveSystem.Common;
using System;
using System.Collections.Generic;

namespace EvolutiveSystem.QuantumSynthesis
{
    /// <summary>
    /// Contenitore di dati che incapsula le informazioni complete su un fallimento nella derivazione di stringhe.
    /// Serve come input standardizzato per la logica di generazione di nuove regole, consentendo la risoluzione mirata dei gap.
    /// </summary>
    public class FailureDetails
    {
        /// <summary>
        /// Il pattern minimo (in Regex o come stringa) che descrive la causa del fallimento.
        /// Questo è il "mintermine" che non è coperto dalle regole esistenti.
        /// </summary>
        public string FailureReasonPattern { get; set; }

        /// <summary>
        /// La stringa di partenza che ha causato il fallimento.
        /// Serve come contesto completo per la generalizzazione e come test primario.
        /// </summary>
        public string SourceString { get; set; }

        /// <summary>
        /// La stringa che ci si aspettava di ottenere se la derivazione fosse riuscita.
        /// Questo è l'obiettivo della nuova regola.
        /// </summary>
        public string TargetString { get; set; }

        /// <summary>
        /// L'ID della regola esistente che ha fallito, se applicabile.
        /// </summary>
        public long FailedRuleId { get; set; }

        /// <summary>
        /// La lista di tutte le regole esistenti, per un contesto completo.
        /// </summary>
        public List<RegolaMIU> ExistingRules { get; set; }

        public FailureDetails(string failureReasonPattern, string sourceString, string targetString, long failedRuleId, List<RegolaMIU> existingRules)
        {
            FailureReasonPattern = failureReasonPattern ?? throw new ArgumentNullException(nameof(failureReasonPattern));
            SourceString = sourceString ?? throw new ArgumentNullException(nameof(sourceString));
            TargetString = targetString ?? throw new ArgumentNullException(nameof(targetString));
            FailedRuleId = failedRuleId;
            ExistingRules = existingRules ?? throw new ArgumentNullException(nameof(existingRules));
        }
    }
}