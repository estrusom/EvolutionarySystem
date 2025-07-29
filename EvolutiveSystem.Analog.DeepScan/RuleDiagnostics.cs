// EvolutiveSystem.Analog.DeepScan/RuleDiagnostics.cs
// Data di riferimento: 29 luglio 2025
// Descrizione: Definizioni per i dettagli sui fallimenti delle regole e le ragioni diagnostiche.
// Questo modulo è il cuore della "Mappa di Karnaugh analogica" per la diagnosi dei problemi.

using EvolutiveSystem.Abstractions;
using EvolutiveSystem.Common;
using System; // Necessario per il tipo RegolaMIU

namespace EvolutiveSystem.Analog.DeepScan
{

    /// <summary>
    /// Contiene i dettagli diagnostici completi di una specifica RegolaMIU quando fallisce su una particolare stringa.
    /// Queste informazioni sono l'input grezzo per l'analisi profonda del RuleCandidateProposer (la "Mappa di Karnaugh analogica").
    /// </summary>
    public class RuleFailureDetail : IRuleFailureDetail
    {
        /// <summary>La regola MIU che è fallita.</summary>
        public RegolaMIU FailedRule { get; }

        /// <summary>La stringa MIU iniziale su cui la regola è stata applicata o tentata.</summary>
        public string InitialString { get; }

        /// <summary>La stringa MIU risultante dopo l'applicazione della regola. Sarà null se la regola non ha trovato un match (NoMatch).</summary>
        public string ResultingString { get; }

        /// <summary>La ragione specifica del fallimento della regola.</summary>
        public FailureReason Reason { get; }

        /// <summary>Un messaggio diagnostico opzionale che fornisce ulteriori dettagli sul fallimento.</summary>
        public string DiagnosticMessage { get; }

        /// <summary>
        /// Inizializza una nuova istanza della classe RuleFailureDetail.
        /// </summary>
        /// <param name="failedRule">La regola MIU che ha fallito.</param>
        /// <param name="initialString">La stringa su cui la regola è stata tentata.</param>
        /// <param name="resultingString">La stringa ottenuta dopo il tentativo (null se nessun match).</param>
        /// <param name="reason">La ragione del fallimento.</param>
        /// <param name="diagnosticMessage">Un messaggio aggiuntivo sul fallimento (opzionale).</param>
        /// <exception cref="ArgumentNullException">Lanciata se failedRule o initialString sono null.</exception>
        public RuleFailureDetail(RegolaMIU failedRule, string initialString, string resultingString, FailureReason reason, string diagnosticMessage = null)
        {
            FailedRule = failedRule ?? throw new ArgumentNullException(nameof(failedRule));
            InitialString = initialString ?? throw new ArgumentNullException(nameof(initialString));
            ResultingString = resultingString; // Può essere null
            Reason = reason;
            DiagnosticMessage = diagnosticMessage;
        }
    }
}