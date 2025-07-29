// EvolutiveSystem.QuantumSynthesis/QuantumSynthesisOrchestrator.cs
// Data di riferimento: 28 luglio 2025
// Descrizione: Orchestratore principale del processo di sintesi quantistica.
//              Coordina la proposta, simulazione e valutazione di nuove regole
//              in risposta alle antitesi identificate nel sistema.

using System;
using System.Collections.Generic; // Necessario per List<T>
using System.Linq; // Necessario per SelectMany e altre operazioni LINQ
using System.Threading.Tasks;
using EvolutiveSystem.Common;
using EvolutiveSystem.Taxonomy; // Necessario per MiuAbstractPattern
using EvolutiveSystem.Taxonomy.Antithesis; // CORREZIONE: Per AntithesisIdentifiedEvent
using MIU.Core;
using MasterLog;

namespace EvolutiveSystem.QuantumSynthesis
{
    public class QuantumSynthesisOrchestrator : IDisposable
    {
        private readonly EventBus _eventBus;
        private readonly RuleCandidateProposer _proposer;
        private readonly RuleCandidateEvaluator _evaluator;
        private readonly IMIUDataManager _dataManager; // <---------- COME  LA METIAMO?
        private readonly Logger _logger;
        private readonly IMiuSimulationEnvironment _miuSimulationEnvironment;

        public QuantumSynthesisOrchestrator(
            EventBus eventBus,
            RuleCandidateProposer proposer,
            RuleCandidateEvaluator evaluator,
            IMIUDataManager dataManager,
            IMiuSimulationEnvironment miuSimulationEnvironment,
            Logger logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _proposer = proposer ?? throw new ArgumentNullException(nameof(proposer));
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // CORREZIONE: Sottoscrizione all'evento corretto: AntithesisIdentifiedEvent
            _eventBus.Subscribe<AntithesisIdentifiedEvent>(HandleAntithesisIdentified);
            _logger.Log(LogLevel.INFO, "QuantumSynthesisOrchestrator inizializzato e sottoscritto agli eventi di antitesi identificate.");
        }

        /// <summary>
        /// Metodo handler per l'evento AntithesisIdentifiedEvent.
        /// Qui inizia il processo di sintesi per ogni antitesi rilevata.
        /// </summary>
        /// <param name="e">I dati dell'evento di antitesi identificate.</param>
        private async Task HandleAntithesisIdentified(AntithesisIdentifiedEvent e) // CORREZIONE: Tipo di evento
        {
            _logger.Log(LogLevel.INFO, $"Antitesi identificate ricevute: {e.IdentifiedGaps.Count} gap, {e.IdentifiedInefficiencies.Count} inefficienze.");
            Console.WriteLine($"[Orchestrator] Antitesi identificate: {e.IdentifiedGaps.Count} gap, {e.IdentifiedInefficiencies.Count} inefficienze.");

            // Combiniamo tutti i pattern di antitesi da processare
            var allAntithesisPatterns = new List<MiuAbstractPattern>();
            allAntithesisPatterns.AddRange(e.IdentifiedGaps);
            allAntithesisPatterns.AddRange(e.IdentifiedInefficiencies);

            if (allAntithesisPatterns.Count == 0)
            {
                _logger.Log(LogLevel.INFO, "Nessun pattern di antitesi da processare in questo evento.");
                return;
            }

            foreach (var antithesisPattern in allAntithesisPatterns)
            {
                try
                {
                    _logger.Log(LogLevel.INFO, $"Elaborazione antitesi pattern: Tipo={antithesisPattern.GetType().Name}, ID={antithesisPattern.ID}");
                    Console.WriteLine($"[Orchestrator] Elaborazione antitesi pattern: {antithesisPattern.GetType().Name} (ID: {antithesisPattern.ID})");

                    // Passo 1: Proporre una regola candidata basata sul pattern di antitesi specifico
                    // Il Proposer userà antithesisPattern per generare una proposta mirata.
                    RuleProposal proposal = _proposer.ProposeRule(antithesisPattern);
                    _logger.Log(LogLevel.INFO, $"Proposta una regola candidata: ID {proposal.CandidateRule.ID}, Nome: {proposal.CandidateRule.Nome}");
                    Console.WriteLine($"[Orchestrator] Proposta: {proposal.CandidateRule.Nome}");

                    // Passo 2: Valutare la regola candidata
                    EvaluationResult evaluationResult = _evaluator.Evaluate(proposal);
                    _logger.Log(LogLevel.INFO, $"Valutazione completata per regola ID {evaluationResult.EvaluatedRule.ID}: Accettata={evaluationResult.IsAccepted}, Score={evaluationResult.Score:F2}");
                    Console.WriteLine($"[Orchestrator] Valutazione per '{evaluationResult.EvaluatedRule.Nome}': Accettata={evaluationResult.IsAccepted}, Score={evaluationResult.Score:F2}");

                    // Passo 3: Gestire il risultato dell'evaluazione
                    if (evaluationResult.IsAccepted)
                    {
                        _logger.Log(LogLevel.INFO, $"Regola ID {evaluationResult.EvaluatedRule.ID} accettata. Persistenza in corso...");
                        Console.WriteLine($"[Orchestrator] Regola ACCETTATA: {evaluationResult.EvaluatedRule.Nome}. Tentativo di persistenza.");

                        #region --- INIZIO LOGICA DI PERSISTENZA E AGGIORNAMENTO CACHE (FINALMENTE CORRETTA) ---
                        // 1. Salva la regola nel database. Ora AddOrUpdateRegolaMIUAsync salverà TUTTI i campi.
                        await _dataManager.AddOrUpdateRegolaMIUAsync(evaluationResult.EvaluatedRule);
                        _logger.Log(LogLevel.INFO, $"Regola ID {evaluationResult.EvaluatedRule.ID} persistita con successo (tramite DataManager).");
                        Console.WriteLine($"[Orchestrator] Regola ID {evaluationResult.EvaluatedRule.ID} persistita.");

                        // 2. Sincronizza la cache in memoria.
                        // Cerca la regola nella cache RegoleMIUManager.Regole.
                        var existingRuleInCache = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == evaluationResult.EvaluatedRule.ID);
                        if (existingRuleInCache == null)
                        {
                            // Se la regola non esiste in cache (è nuova), aggiungila.
                            RegoleMIUManager.Regole.Add(evaluationResult.EvaluatedRule);
                            _logger.Log(LogLevel.INFO, $"[Orchestrator] Aggiunta nuova regola ID {evaluationResult.EvaluatedRule.ID} ({evaluationResult.EvaluatedRule.Nome}) alla cache in memoria.");
                        }
                        else
                        {
                            // Se la regola esiste già in cache, non dobbiamo fare nulla per le proprietà immutabili.
                            // L'oggetto 'evaluationResult.EvaluatedRule' è la versione più recente.
                            // L'oggetto 'existingRuleInCache' è quello che aggiorneremo per StimaProfonditaMedia più avanti.
                            _logger.Log(LogLevel.DEBUG, $"[Orchestrator] Regola ID {evaluationResult.EvaluatedRule.ID} ({evaluationResult.EvaluatedRule.Nome}) già presente in cache. L'istanza è aggiornata per le proprietà immutabili.");
                        }
                        #endregion --- FINE LOGICA DI PERSISTENZA E AGGIORNAMENTO CACHE ---

                        #region --- INIZIO LOGICA DI CALCOLO E AGGIORNAMENTO STIMAPROFONDITAMEDIA (LA TUA LOGICA ORIGINALE) ---
                        _logger.Log(LogLevel.INFO, $"Avvio simulazione per calcolare StimaProfonditaMedia per regola ID: {evaluationResult.EvaluatedRule.ID}");
                        Console.WriteLine($"[Orchestrator] Avvio simulazione per calcolare StimaProfonditaMedia...");

                        string initialStringForSimulation = "MI";
                        int maxStepsForSimulation = 100;

                        // Esegui la simulazione per ottenere il risultato
                        // Questa parte è essenziale e deve rimanere.
                        SimulationResult outcome = await _miuSimulationEnvironment.SimulateExplorationAsync(
                            new List<RegolaMIU> { evaluationResult.EvaluatedRule }, // Passiamo la regola appena accettata
                            initialStringForSimulation,
                            maxStepsForSimulation
                        );

                        double successThreshold = 0.8;

                        if (outcome.TargetAntithesisResolutionScore >= successThreshold)
                        {
                            _logger.Log(LogLevel.INFO, $"Simulazione riuscita per regola '{evaluationResult.EvaluatedRule.Nome}' (ID: {evaluationResult.EvaluatedRule.ID}). Risoluzione Antitesi: {outcome.TargetAntithesisResolutionScore:F2}. Profondità media di scoperta: {outcome.AverageDepthOfDiscovery:F2}.");
                            Console.WriteLine($"[Orchestrator] Simulazione riuscita. Risoluzione: {outcome.TargetAntithesisResolutionScore:F2}, Profondità di scoperta: {outcome.AverageDepthOfDiscovery:F2}.");

                            // Recupera l'istanza della regola dalla cache per aggiornare StimaProfonditaMedia.
                            // È FONDAMENTALE MODIFICARE L'OGGETTO GIÀ PRESENTE NELLA LISTA STATICA.
                            RegolaMIU ruleToUpdateStima = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == evaluationResult.EvaluatedRule.ID);

                            if (ruleToUpdateStima == null)
                            {
                                // Questo non dovrebbe accadere se la logica precedente è corretta, ma è una safety net.
                                _logger.Log(LogLevel.ERROR, $"RegolaMIU ID {evaluationResult.EvaluatedRule.ID} NON TROVATA nella cache di RegoleMIUManager per aggiornare StimaProfonditaMedia dopo simulazione. Problema critico di sincronizzazione interna. La stima non sarà aggiornata per questa sessione.");
                            }
                            else
                            {
                                double oldStima = ruleToUpdateStima.StimaProfonditaMedia;
                                double newAverageDepth = outcome.AverageDepthOfDiscovery;

                                double updatedStima;

                                // --- LA TUA LOGICA ORIGINALE PER L'AGGIORNAMENTO DELLA STIMA ---
                                if (oldStima == 0.0)
                                {
                                    updatedStima = newAverageDepth;
                                }
                                else
                                {
                                    updatedStima = (oldStima + newAverageDepth) / 2.0; // Media semplice
                                }
                                // --- FINE TUA LOGICA ORIGINALE ---

                                ruleToUpdateStima.StimaProfonditaMedia = updatedStima; // Applica la stima calcolata all'oggetto in cache

                                // Salva la regola nel database con la StimaProfonditaMedia aggiornata.
                                // Poiché 'ruleToUpdateStima' è l'oggetto preso dalla cache, la cache è già aggiornata.
                                await _dataManager.AddOrUpdateRegolaMIUAsync(ruleToUpdateStima);
                                _logger.Log(LogLevel.INFO, $"Aggiornata StimaProfonditaMedia per regola '{evaluationResult.EvaluatedRule.Nome}' (ID: {evaluationResult.EvaluatedRule.ID}) da {oldStima:F2} a {updatedStima:F2}.");
                                Console.WriteLine($"[Orchestrator] StimaProfonditaMedia aggiornata a: {updatedStima:F2}.");
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.WARNING, $"Simulazione fallita (TargetAntithesisResolutionScore insufficiente: {outcome.TargetAntithesisResolutionScore:F2}) per la regola '{evaluationResult.EvaluatedRule.Nome}' (ID: {evaluationResult.EvaluatedRule.ID}). StimaProfonditaMedia non aggiornata.");
                            Console.WriteLine($"[Orchestrator] Simulazione fallita per la regola. Stima non aggiornata.");
                        }
                        #endregion --- FINE LOGICA DI CALCOLO E AGGIORNAMENTO STIMAPROFONDITAMEDIA ---
                    }
                    else
                    {
                        _logger.Log(LogLevel.WARNING, $"Regola ID {evaluationResult.EvaluatedRule.ID} rifiutata. Motivo: {evaluationResult.Reason}");
                        Console.WriteLine($"[Orchestrator] Regola RIFIUTATA: {evaluationResult.EvaluatedRule.Nome}. Motivo: {evaluationResult.Reason}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.ERROR, $"Errore durante l'elaborazione di un pattern di antitesi ({antithesisPattern.GetType().Name} - ID: {antithesisPattern.ID}): {ex.Message} - StackTrace: {ex.StackTrace}");
                    Console.WriteLine($"[Orchestrator] ERRORE durante la sintesi per un pattern: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            // CORREZIONE: Disiscrizione dall'evento corretto: AntithesisIdentifiedEvent
            _eventBus.Unsubscribe<AntithesisIdentifiedEvent>(HandleAntithesisIdentified);
            _logger.Log(LogLevel.INFO, "QuantumSynthesisOrchestrator disiscritto dagli eventi di antitesi identificate.");
            GC.SuppressFinalize(this);
        }
    }
}