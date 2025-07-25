// EvolutiveSystem.QuantumSynthesis/RuleCandidateEvaluator.cs
// Data di riferimento: 26 luglio 2025
// Descrizione: Componente responsabile di valutare i risultati di una simulazione
//              e decidere se una regola candidata è adatta per essere integrata nel sistema.

using System;
using System.Collections.Generic;
using System.Linq;
using EvolutiveSystem.Common; // Per RegolaMIU, SimulationResult
// Non includiamo MiuSimulationEnvironment qui, lo riceveremo nel costruttore
// per favorire la Dependency Injection e la testabilità.

namespace EvolutiveSystem.QuantumSynthesis
{
    /// <summary>
    /// Rappresenta il verdetto dell'evaluator su una regola candidata.
    /// </summary>
    public class EvaluationResult
    {
        public RegolaMIU EvaluatedRule { get; }
        public SimulationResult SimulationOutcome { get; }
        public bool IsAccepted { get; }
        public double Score { get; } // Punteggio complessivo della regola, basato sulle metriche
        public string Reason { get; } // Motivazione dell'accettazione/rifiuto

        public EvaluationResult(RegolaMIU rule, SimulationResult outcome, bool isAccepted, double score, string reason)
        {
            EvaluatedRule = rule ?? throw new ArgumentNullException(nameof(rule));
            SimulationOutcome = outcome ?? throw new ArgumentNullException(nameof(outcome));
            IsAccepted = isAccepted;
            Score = score;
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }

    /// <summary>
    /// Il RuleCandidateEvaluator valuta le regole MIU candidate basandosi sui risultati
    /// delle simulazioni fornite dal MiuSimulationEnvironment. Implementa i criteri
    /// per il "collasso della funzione d'onda".
    /// </summary>
    public class RuleCandidateEvaluator
    {
        private readonly MiuSimulationEnvironment _simulationEnvironment;
        // In futuro, potremmo aggiungere soglie e pesi per le metriche,
        // o un riferimento a un MiuPatternManager per regole più complesse.

        /// <summary>
        /// Inizializza un nuovo RuleCandidateEvaluator con un'istanza di MiuSimulationEnvironment.
        /// </summary>
        /// <param name="simulationEnvironment">L'ambiente di simulazione da utilizzare per i test.</param>
        public RuleCandidateEvaluator(MiuSimulationEnvironment simulationEnvironment)
        {
            _simulationEnvironment = simulationEnvironment ?? throw new ArgumentNullException(nameof(simulationEnvironment), "MiuSimulationEnvironment non può essere nullo.");
        }

        /// <summary>
        /// Valuta una regola MIU candidata eseguendo simulazioni e analizzando i risultati.
        /// Questo è il cuore del "collasso della funzione d'onda".
        /// </summary>
        /// <param name="proposal">La proposta di regola da valutare, inclusi gli stati di test.</param>
        /// <returns>Un EvaluationResult che indica se la regola è stata accettata e perché.</returns>
        public EvaluationResult Evaluate(RuleProposal proposal)
        {
            if (proposal == null) throw new ArgumentNullException(nameof(proposal), "La proposta di regola non può essere nulla.");

            Console.WriteLine($"Valutazione della regola candidata ID: {proposal.CandidateRule.ID} ('{proposal.CandidateRule.Nome}')");
            Console.WriteLine($"Simulazione con stati di partenza: {string.Join(", ", proposal.TestStartingStates.Select(s => s.CurrentString))}");
            Console.WriteLine($"Obiettivo: {proposal.TargetAntithesisState.CurrentString}");

            // Esegui la simulazione usando il MiuSimulationEnvironment
            SimulationResult simulationOutcome = _simulationEnvironment.Simulate(
                proposal.CandidateRule,
                proposal.TestStartingStates,
                proposal.TargetAntithesisState
            );

            // --- Logica di Valutazione del "Collasso della Funzione d'Onda" ---
            // Questa è la parte dove definisci i tuoi criteri di accettazione.
            // Per ora, useremo una logica molto semplice, che verrà raffinata.

            bool isAccepted = false;
            double evaluationScore = 0.0;
            string reason = "Regola non accettata: ";

            // Criterio 1: La regola deve aver contribuito a raggiungere l'obiettivo antitetico
            if (simulationOutcome.TargetAntithesisResolutionScore > 0.0)
            {
                evaluationScore += simulationOutcome.TargetAntithesisResolutionScore * 0.6; // Peso maggiore
                reason = "Regola ha contribuito alla risoluzione dell'antitesi. ";
            }
            else
            {
                reason += "Non ha risolto l'antitesi target. ";
            }

            // Criterio 2: La regola deve aver generato una buona diversità di pattern (placeholder)
            // Stima molto semplice di un punteggio positivo per la diversità
            if (simulationOutcome.UniquePatternCount > proposal.TestStartingStates.Count) // Ha generato nuove stringhe
            {
                evaluationScore += simulationOutcome.PatternDiversityScore * 0.2; // Aggiungi un bonus per la diversità
                reason += $"Generati {simulationOutcome.UniquePatternCount - proposal.TestStartingStates.Count} nuovi stati unici. ";
            }
            else
            {
                reason += "Nessuna variazione significativa. ";
            }


            // Criterio 3: Il bilanciamento dei token non deve essere peggiorato drasticamente (placeholder)
            // Se il bilanciamento è decente, aggiungiamo un piccolo bonus.
            if (simulationOutcome.TokenBalanceScore > 0.5) // Esempio: un punteggio superiore a 0.5 è "decente"
            {
                evaluationScore += simulationOutcome.TokenBalanceScore * 0.1;
                reason += "Bilanciamento token decente. ";
            }
            else
            {
                reason += "Bilanciamento token non ottimale. ";
            }

            // Criterio 4: La regola non deve creare stringhe eccessivamente lunghe o corte (placeholder)
            // Se la varianza della lunghezza è contenuta (non esplode)
            if (simulationOutcome.StringLengthVariance < 100.0 && simulationOutcome.AverageStringLength < 20.0) // Esempi di soglie
            {
                evaluationScore += 0.05; // Piccolo bonus
                reason += "Lunghezze stringhe controllate. ";
            }
            else
            {
                reason += "Lunghezze stringhe estreme. ";
            }


            // Decidi se accettare in base al punteggio finale
            const double AcceptanceThreshold = 0.6; // Soglia arbitraria per iniziare
            if (evaluationScore >= AcceptanceThreshold)
            {
                isAccepted = true;
                reason = "Regola accettata: " + reason;
                Console.WriteLine($"Regola {proposal.CandidateRule.ID} ACCETTATA con score {evaluationScore:F2}.");
            }
            else
            {
                reason = "Regola RIFIUTATA: " + reason;
                Console.WriteLine($"Regola {proposal.CandidateRule.ID} RIFIUTATA con score {evaluationScore:F2}.");
            }

            Console.WriteLine($"Dettagli: {reason}");
            Console.WriteLine($"Tempo simulazione: {simulationOutcome.ElapsedTime.TotalMilliseconds:F2} ms, Stati esplorati: {simulationOutcome.TotalStatesExplored}");

            return new EvaluationResult(proposal.CandidateRule, simulationOutcome, isAccepted, evaluationScore, reason);
        }
    }
}