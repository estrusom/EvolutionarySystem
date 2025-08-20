// EvolutiveSystem.QuantumSynthesis/AntithesisDefinition.cs
// Data di riferimento: 30 luglio 2025
// Descrizione: Implementazione concreta di un'antitesi con dettagli diagnostici.

using System;
using System.Collections.Generic;
using EvolutiveSystem.Abstractions; // Necessario per IAntithesisWithDiagnosticDetails e IRuleFailureDetail

namespace EvolutiveSystem.QuantumSynthesis
{
    /// <summary>
    /// Rappresenta una specifica antitesi identificata nel sistema MIU,
    /// inclusi i pattern di inefficienza o gap e i dettagli diagnostici dei fallimenti delle regole.
    /// Questa classe è l'input principale per il processo di sintesi quantistica.
    /// </summary>
    public class AntithesisDefinition : IAntithesisWithDiagnosticDetails
    {
        public string AntithesisId { get; }
        public string Description { get; }
        public IReadOnlyList<IBaseMiuPattern> IdentifiedAntithesisPatterns { get; }
        public IReadOnlyList<IRuleFailureDetail> FailureDetails { get; }
        public IReadOnlyList<string> TargetMiuStrings { get; } // Le stringhe che si volevano raggiungere ma non derivate

        /// <summary>
        /// Inizializza una nuova istanza della classe AntithesisDefinition.
        /// </summary>
        /// <param name="antithesisId">ID univoco dell'antitesi.</param>
        /// <param name="description">Descrizione dell'antitesi.</param>
        /// <param name="identifiedAntithesisPatterns">Pattern di gap o inefficienza identificati.</param>
        /// <param name="failureDetails">Dettagli diagnostici sui fallimenti delle regole.</param>
        /// <param name="targetMiuStrings">Le stringhe MIU bersaglio che il sistema non è riuscito a derivare.</param>
        /// <exception cref="ArgumentNullException">Lanciata se gli argomenti essenziali sono null.</exception>
        public AntithesisDefinition(
            string antithesisId,
            string description,
            IReadOnlyList<IBaseMiuPattern> identifiedAntithesisPatterns,
            IReadOnlyList<IRuleFailureDetail> failureDetails,
            IReadOnlyList<string> targetMiuStrings)
        {
            AntithesisId = antithesisId ?? throw new ArgumentNullException(nameof(antithesisId));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IdentifiedAntithesisPatterns = identifiedAntithesisPatterns ?? throw new ArgumentNullException(nameof(identifiedAntithesisPatterns));
            FailureDetails = failureDetails ?? throw new ArgumentNullException(nameof(failureDetails));
            TargetMiuStrings = targetMiuStrings ?? throw new ArgumentNullException(nameof(targetMiuStrings));
        }
    }
}