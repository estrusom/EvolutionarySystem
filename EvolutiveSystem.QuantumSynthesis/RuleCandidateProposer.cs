// EvolutiveSystem.QuantumSynthesis/RuleCandidateProposer.cs
// Data di riferimento: 26 luglio 2025
// Descrizione: Componente responsabile di analizzare lo stato del campo di esistenza
//              e proporre nuove regole MIU candidate per risolvere gap o inefficienze.

using System;
using System.Collections.Generic;
using System.Linq;
using EvolutiveSystem.Common;
using EvolutiveSystem.Logic; // Per RegolaMIU, MiuStateInfo, MiuPattern (futuro)

namespace EvolutiveSystem.QuantumSynthesis
{
    /// <summary>
    /// Contiene una regola MIU candidata e gli stati iniziali di test più appropriati
    /// per valutarne l'efficacia nel MiuSimulationEnvironment.
    /// </summary>
    public class RuleProposal
    {
        public RegolaMIU CandidateRule { get; }
        public List<MiuStateInfo> TestStartingStates { get; }
        public MiuStateInfo TargetAntithesisState { get; } // Lo stato obiettivo che la regola dovrebbe aiutare a raggiungere

        public RuleProposal(RegolaMIU candidateRule, List<MiuStateInfo> testStartingStates, MiuStateInfo targetAntithesisState)
        {
            CandidateRule = candidateRule ?? throw new ArgumentNullException(nameof(candidateRule));
            TestStartingStates = testStartingStates ?? throw new ArgumentNullException(nameof(testStartingStates));
            if (!TestStartingStates.Any()) throw new ArgumentException("La lista degli stati di test non può essere vuota.", nameof(testStartingStates));
            TargetAntithesisState = targetAntithesisState ?? throw new ArgumentNullException(nameof(targetAntithesisState));
        }
    }

    /// <summary>
    /// Il RuleCandidateProposer analizza il campo di esistenza e propone nuove regole
    /// per migliorarlo, risolvendo gap o ottimizzando le derivazioni.
    /// Per ora, la logica di proposta sarà semplice e verrà evoluta in seguito.
    /// </summary>
    public class RuleCandidateProposer
    {
        // Questo sarà il punto in cui il Proposer riceverà informazioni sui gap e le inefficienze.
        // Per ora, non abbiamo un "RuleTaxonomyGenerator" o un "GapAnalyzer",
        // quindi la logica sarà un placeholder.
        // In futuro, questo potrebbe prendere in input i dati dal MiuPatternManager o dal Taxonomy.

        private readonly MIURulesEngine _rulesEngine;

        /// <summary>
        /// Inizializza una nuova istanza di RuleCandidateProposer con il motore di regole MIU.
        /// </summary>
        /// <param name="rulesEngine">Il motore di regole necessario per l'analisi dei fallimenti.</param>
        public RuleCandidateProposer(MIURulesEngine rulesEngine)
        {
            _rulesEngine = rulesEngine ?? throw new ArgumentNullException(nameof(rulesEngine));
        }

        /// <summary>
        /// Propone una nuova regola MIU candidata e un set di stati iniziali per testarla.
        /// Per questa prima versione, la proposta è molto semplice/simulata.
        /// In futuro, questa logica sarà guidata dall'analisi di gap e inefficienze.
        /// </summary>
        /// <param name="currentFieldOfExistence">Una rappresentazione (anche se semplice per ora) dell'attuale campo di esistenza.</param>
        /// <returns>Un oggetto RuleProposal contenente la regola candidata e i test case.</returns>
        public RuleProposal ProposeRule(object currentFieldOfExistence = null) // Tipo generico per ora
        {
            // --- Logica di Proposta Semplice/Placeholder ---
            // Questo è il punto che verrà ampiamente sviluppato in futuro.
            // Le strategie potrebbero includere:
            // 1. Mutazioni di regole esistenti.
            // 2. Combinazioni di regole.
            // 3. Generazione casuale di regole con vincoli.
            // 4. Proposte basate sull'analisi di MiuPattern che "mancano" (gap).

            // Esempio di una regola candidata molto semplice per il test:
            // Regola "UI -> UII"
            // Obiettivo: generare 'MUU' da 'MU' (ipotetico gap o obiettivo)

            // Creiamo una regola di esempio (ID 5 per distinguere dalle 4 base)
            long newRuleId = 5; // Useremo un ID fisso per i test, poi sarà generato.
            RegolaMIU candidateRule = new RegolaMIU(
                id: newRuleId,
                nome: "AddIIfUIIsPresent",
                descrizione: "Aggiunge una 'I' dopo 'U' se la stringa inizia con 'M' e contiene 'UII'. Proposta per testare l'espansione.",
                pattern: "^M(U+)I$", // Modificato per un test più significativo
                sostituzione: "M$1II"
            );

            // Definiamo un set di stati iniziali di test e uno stato target
            // Questi stati dovrebbero essere scelti per evidenziare il comportamento della regola candidata.
            List<MiuStateInfo> testStates = new List<MiuStateInfo>
            {
                new MiuStateInfo("MUI"),     // Stringa che dovrebbe essere modificata dalla regola
                new MiuStateInfo("MUII"),    // Stringa che dovrebbe essere modificata
                new MiuStateInfo("MI")       // Stringa che non dovrebbe essere modificata (test negativo)
            };

            // Lo stato obiettivo: cosa ci aspetteremmo se la regola funzionasse come previsto su MUI?
            MiuStateInfo targetState = new MiuStateInfo("MUII"); // Un obiettivo plausibile per la regola.

            Console.WriteLine($"Proposta Regola ID: {candidateRule.ID}, Nome: {candidateRule.Nome}");
            Console.WriteLine($"Test su stati: {string.Join(", ", testStates.Select(s => s.CurrentString))}");
            Console.WriteLine($"Obiettivo del test: {targetState.CurrentString}");

            return new RuleProposal(candidateRule, testStates, targetState);
        }
        #region public methods
        /// <summary>
        /// Esegue un'analisi ricorsiva (a mintermini) confrontando una stringa fallita con il suo obiettivo.
        /// Il suo scopo è identificare il pattern di stringa minimo e non coperto che ha causato il fallimento,
        /// fornendo un input strutturato per il processo di generazione di nuove regole.
        /// </summary>
        /// <param name="sourceString">La stringa di partenza che ha causato il fallimento.</param>
        /// <param name="targetString">La stringa che ci si aspettava di ottenere.</param>
        /// <param name="existingRules">L'insieme di tutte le regole MIU attive nel sistema.</param>
        /// <returns>Un oggetto FailureDetails contenente la diagnosi completa del fallimento.</returns>
        public FailureDetails AnalyzeFailures(string source, string target, List<RegolaMIU> regole)
        {
            // Coda per la ricerca in ampiezza
            var queue = new Queue<string>();
            queue.Enqueue(source);

            // Insieme per tenere traccia delle stringhe già visitate
            var visited = new HashSet<string> { source };

            while (queue.Count > 0)
            {
                var currentString = queue.Dequeue();

                // Se la stringa corrente corrisponde al target, abbiamo finito
                if (currentString == target)
                {
                    // Successo! La stringa target è raggiungibile.
                    // Ho aggiunto i parametri mancanti.
                    return new FailureDetails(source, target, "", 0L, new List<RegolaMIU>());
                }

                // Applica ogni regola disponibile
                foreach (var regola in regole)
                {
                    string newString;

                    // Qui usi il tuo metodo TryApply per un singolo passo
                    if (regola.TryApply(currentString, out newString))
                    {
                        // Se la nuova stringa non è già stata visitata, la aggiungiamo alla coda
                        if (!visited.Contains(newString))
                        {
                            visited.Add(newString);
                            queue.Enqueue(newString);
                        }
                    }
                }
            }

            // Se la coda si svuota senza raggiungere il target, il percorso non esiste
            // Ho aggiunto i parametri mancanti anche qui.
            return new FailureDetails(source, target, "Mintermine irriducibile", 0L, new List<RegolaMIU>());
        }
        #endregion 
    }
}