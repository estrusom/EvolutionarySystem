// File: EvolutiveSystem.TaxonomyOrchestration/TaxonomyOrchestrator.cs
// Data di riferimento: 11 luglio 2025
// Descrizione: Implementa la logica di orchestrazione per la rigenerazione della tassonomia,
//              agendo come il "cervello" basato su rete di Petri che decide quando
//              innescare il RuleTaxonomyGenerator in base al flusso di eventi.

using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using EvolutiveSystem.Common; // Per EventBus, AnomalyType
using EvolutiveSystem.Common.Events; // Per RuleAppliedEventArgs, SearchCompletedEvent, AnomalyDetectedEvent
using EvolutiveSystem.Taxonomy; // Per RuleTaxonomyGenerator
using MasterLog;
using MIU.Core; 

namespace EvolutiveSystem.TaxonomyOrchestration // Namespace aggiornato
{
    /// <summary>
    /// Il TaxonomyOrchestrator è il "cervello" basato su rete di Petri
    /// che sa quando far scattare la generazione della tassonomia.
    /// Sottoscrive agli eventi del sistema, genera "token" interni,
    /// e in base a soglie e combinazioni di questi token, decide quando
    /// invocare il RuleTaxonomyGenerator.
    /// </summary>
    public class TaxonomyOrchestrator
    {
        private readonly RuleTaxonomyGenerator _taxonomyGenerator;
        private readonly EventBus _eventBus;
        private readonly Logger _logger;

        // "Luoghi" della nostra rete di Petri semplificata (contatori e flag)
        private int _newRuleApplicationsCount = 0;
        private int _successfulSearchesCount = 0;
        private int _failedSearchesCount = 0; // Contatore per tutti i tipi di fallimento
        private int _anomalyDetectedCount = 0; // Contatore per anomalie rilevate
        private int _newMiuStringsDiscoveredCount = 0; // 25.07.11 Contatore per nuove stringhe MIU scoperte
        private DateTime _lastTaxonomyGenerationTime = DateTime.MinValue;

        // Soglie configurabili per innescare la rigenerazione (i nostri "trigger" del delta di Dirac)
        // Queste sono proprietà pubbliche con getter e setter.
        public int RuleAppThreshold { get; set; }
        public int SuccessSearchThreshold { get; set; }
        public int FailedSearchThreshold { get; set; }
        public int AnomalyThreshold { get; set; }
        public int NewMiuStringThreshold { get; set; }
        public double TimeThresholdHours { get; set; }

        /// <summary>
        /// Costruttore di TaxonomyOrchestrator.
        /// Riceve le dipendenze necessarie tramite Dependency Injection.
        /// </summary>
        /// <param name="taxonomyGenerator">L'istanza del generatore di tassonomie.</param>
        /// <param name="eventBus">L'istanza dell'Event Bus per sottoscrivere agli eventi.</param>
        /// <param name="logger">L'istanza del logger.</param>
        public TaxonomyOrchestrator(RuleTaxonomyGenerator taxonomyGenerator, EventBus eventBus, Logger logger)
        {
            _taxonomyGenerator = taxonomyGenerator ?? throw new ArgumentNullException(nameof(taxonomyGenerator));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.Log(LogLevel.DEBUG, "TaxonomyOrchestrator istanziato. Sottoscrizione agli eventi in corso.");

            // Assegna i valori di default DESIDERATI alle proprietà
            RuleAppThreshold = 500;
            SuccessSearchThreshold = 20;
            FailedSearchThreshold = 50;
            AnomalyThreshold = 5;
            NewMiuStringThreshold = 100;
            TimeThresholdHours = 24.0;

            _logger.Log(LogLevel.INFO, $"[TaxonomyOrchestrator] Soglie inizializzate con default: RuleApp={RuleAppThreshold}, SuccessSearch={SuccessSearchThreshold}, FailedSearch={FailedSearchThreshold}, Anomaly={AnomalyThreshold}, NewMiuString={NewMiuStringThreshold}, Time={TimeThresholdHours}h.");

            // Sottoscrizione agli eventi rilevanti
            _eventBus.Subscribe<RuleAppliedEventArgs>(HandleRuleAppliedEvent);
            _eventBus.Subscribe<SearchCompletedEvent>(HandleSearchCompletedEvent);
            _eventBus.Subscribe<AnomalyDetectedEvent>(HandleAnomalyDetectedEvent);
            _eventBus.Subscribe<NewMiuStringDiscoveredEventArgs>(HandleNewMiuStringDiscoveredEvent);


            // Inizializza il tempo dell'ultima generazione all'avvio dell'orchestratore
            _lastTaxonomyGenerationTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Handler per l'evento RuleAppliedEventArgs. Incrementa il contatore delle applicazioni di regole.
        /// </summary>
        /// <param name="e">I dati dell'evento RuleAppliedEventArgs.</param>
        private Task HandleRuleAppliedEvent(RuleAppliedEventArgs e)
        {
            _newRuleApplicationsCount++;
            _logger.Log(LogLevel.DEBUG, $"RuleAppliedEvent ricevuto. Contatore applicazioni: {_newRuleApplicationsCount}");
            CheckAndGenerateTaxonomy(); // Controlla se è il momento di generare la tassonomia
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler per l'evento SearchCompletedEvent. Incrementa i contatori delle ricerche completate.
        /// </summary>
        /// <param name="e">I dati dell'evento SearchCompletedEvent.</param>
        private Task HandleSearchCompletedEvent(SearchCompletedEvent e)
        {
            if (e.Outcome == "Success")
            {
                _successfulSearchesCount++;
                _logger.Log(LogLevel.DEBUG, $"SearchCompletedEvent (Successo) ricevuto. Contatore successi: {_successfulSearchesCount}");
            }
            else
            {
                _failedSearchesCount++;
                _logger.Log(LogLevel.DEBUG, $"SearchCompletedEvent (Fallimento: {e.Outcome}) ricevuto. Contatore fallimenti: {_failedSearchesCount}");
            }
            CheckAndGenerateTaxonomy(); // Controlla se è il momento di generare la tassonomia
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler per l'evento AnomalyDetectedEvent. Incrementa il contatore delle anomalie.
        /// </summary>
        /// <param name="e">I dati dell'evento AnomalyDetectedEvent.</param>
        private Task HandleAnomalyDetectedEvent(AnomalyDetectedEvent e)
        {
            _anomalyDetectedCount++;
            _logger.Log(LogLevel.WARNING, $"AnomalyDetectedEvent ({e.Type}) ricevuto. Contatore anomalie: {_anomalyDetectedCount}");
            CheckAndGenerateTaxonomy(); // Controlla se è il momento di generare la tassonomia
            return Task.CompletedTask;
        }
        /// <summary>
        /// Handler per l'evento NewMiuStringDiscoveredEventArgs.
        /// Incrementa il contatore delle nuove stringhe MIU scoperte.
        /// </summary>
        /// <param name="e">I dati dell'evento NewMiuStringDiscoveredEventArgs.</param>
        private Task HandleNewMiuStringDiscoveredEvent(NewMiuStringDiscoveredEventArgs e)
        {
            // Incrementa il contatore solo se la stringa è veramente nuova per il database,
            // per evitare di triggerare la tassonomia per ogni singola scoperta durante un'esplorazione,
            // ma solo quando c'è un'aggiunta significativa al knowledge base.
            // Questo è importante per evitare di generare tassonomie troppo frequentemente
            if (DateTime.Now > Convert.ToDateTime("11/7/2025"))
            {
            // indipendentemente dal fatto che sia veramente nuova per il database.
            // Questo riflette l'attività di esplorazione all'interno del paesaggio MIU esistente.
            _newMiuStringsDiscoveredCount++;

                _logger.Log(LogLevel.INFO, $"NewMiuStringDiscoveredEvent ricevuto (Stringa: '{e.DiscoveredString.Substring(0, Math.Min(e.DiscoveredString.Length, 50))}...'). Contatore nuove stringhe (esplorate): {_newMiuStringsDiscoveredCount}. IsTrulyNewToDatabase: {e.IsTrulyNewToDatabase}");

                // Controlla se una delle soglie è stata raggiunta
                CheckAndGenerateTaxonomy();
            }
            else
            {
                if (e.IsTrulyNewToDatabase)
                {
                    _newMiuStringsDiscoveredCount++;
                    _logger.Log(LogLevel.INFO, $"NewMiuStringDiscoveredEvent ricevuto (Nuova per DB: '{e.DiscoveredString}'). Contatore nuove stringhe: {_newMiuStringsDiscoveredCount}");
                    CheckAndGenerateTaxonomy(); // Controlla se è il momento di generare la tassonomia
                }
                else
                {
                    _logger.Log(LogLevel.DEBUG, $"NewMiuStringDiscoveredEvent ricevuto (Non nuova per DB: '{e.DiscoveredString}'). Contatore non incrementato.");
                }
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// Controlla se le condizioni per la rigenerazione della tassonomia sono soddisfatte.
        /// Questa è la logica della "rete di Petri" semplificata che decide il "collasso della funzione d'onda".
        /// </summary>
        public void CheckAndGenerateTaxonomy()
        {
            bool shouldGenerate = false;
            string triggerReason = "Nessuna";

            // Condizione 1: Abbastanza nuove applicazioni di regole
            if (_newRuleApplicationsCount >= RuleAppThreshold)
            {
                shouldGenerate = true;
                triggerReason = $"Soglia applicazioni ({RuleAppThreshold}) raggiunta.";
            }

            // Condizione 2: Abbastanza nuove ricerche di successo
            if (_successfulSearchesCount >= SuccessSearchThreshold)
            {
                shouldGenerate = true;
                triggerReason = $"Soglia ricerche di successo ({SuccessSearchThreshold}) raggiunta.";
            }

            // Condizione 3: Abbastanza nuove ricerche fallite (il fallimento è informazione!)
            if (_failedSearchesCount >= FailedSearchThreshold)
            {
                shouldGenerate = true;
                triggerReason = $"Soglia ricerche fallite ({FailedSearchThreshold}) raggiunta.";
            }

            // Condizione 4: Abbastanza anomalie rilevate (segnalano problemi che la tassonomia potrebbe aiutare a risolvere)
            if (_anomalyDetectedCount >= AnomalyThreshold)
            {
                shouldGenerate = true;
                triggerReason = $"Soglia anomalie ({AnomalyThreshold}) raggiunta.";
            }

            // Condizione 5: Tempo trascorso (fallback per evitare stasi del sistema)
            if ((DateTime.UtcNow - _lastTaxonomyGenerationTime).TotalHours >= TimeThresholdHours)
            {
                shouldGenerate = true;
                triggerReason = $"Soglia temporale ({TimeThresholdHours} ore) raggiunta.";
            }

            if (shouldGenerate)
            {
                _logger.Log(LogLevel.INFO, $"Condizioni per la rigenerazione della tassonomia soddisfatte. Motivo: {triggerReason}. Avvio generazione...");
                try
                {
                    _taxonomyGenerator.GenerateRuleTaxonomy(); // Invocazione del generatore di tassonomie
                    _logger.Log(LogLevel.INFO, "Rigenerazione tassonomia completata.");
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.ERROR, $"Errore durante la rigenerazione della tassonomia: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
                finally
                {
                    // Resetta i contatori e il timestamp dopo ogni tentativo di generazione
                    _lastTaxonomyGenerationTime = DateTime.UtcNow;
                    _newRuleApplicationsCount = 0;
                    _successfulSearchesCount = 0;
                    _failedSearchesCount = 0;
                    _anomalyDetectedCount = 0;
                    _logger.Log(LogLevel.INFO, "Contatori di trigger per la tassonomia resettati.");
                }
            }
        }
    }
}
