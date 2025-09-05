// File: C:\Progetti\EvolutiveSystem\Learning\LearningStatisticsManager.cs
// Data di riferimento: 20 giugno 2025
// Descrizione: Questa classe è responsabile della gestione (caricamento, salvataggio, aggregazione)
// delle statistiche di apprendimento (RuleStatistics e TransitionStatistics).
// Migliora la separazione delle responsabilità e la testabilità del sistema.

using System;
using System.Collections.Generic;
using System.Linq; // Necessario per Linq, se usato in futuro
using MasterLog;
using EvolutiveSystem.Common; // Per RuleStatistics, TransitionStatistics
using MIU.Core;
using System.Threading.Tasks; // Per IMIUDataManager

namespace EvolutiveSystem.Learning // Nuovo namespace suggerito
{
    /// <summary>
    /// Gestisce le statistiche di apprendimento per il sistema evolutivo.
    /// Si interfaccia con il data manager per ottenere i dati grezzi e poi li aggrega o salva.
    /// </summary>
    public class LearningStatisticsManager: ILearningStatisticsManager
    {
        private readonly IMIUDataManager _dataManager;
        private readonly Logger _logger;

        /// <summary>
        /// Costruttore di LearningStatisticsManager.
        /// Riceve un'istanza di IMIUDataManager per accedere ai dati.
        /// </summary>
        /// <param name="dataManager">L'istanza del gestore dati (MIUDatabaseManager implementa questa interfaccia).</param>
        /// <param name="logger">L'istanza del logger per la registrazione degli eventi.</param>
        public LearningStatisticsManager(IMIUDataManager dataManager, Logger logger)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager), "IMIUDataManager non può essere nullo.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger non può essere nullo.");
            _logger.Log(LogLevel.DEBUG, "[LearningStatisticsManager DEBUG] LearningStatisticsManager istanziato.");
        }

        /// <summary>
        /// Carica le statistiche di apprendimento delle regole dal database tramite il data manager.
        /// </summary>
        /// <returns>Un dizionario di RuleStatistics, con chiave=RuleID.</returns>
        public Dictionary<long, RuleStatistics> LoadRuleStatistics()
        {
            _logger.Log(LogLevel.DEBUG, "[LearningStatisticsManager DEBUG] Richiesta LoadRuleStatistics.");
            try
            {
                // Delega la logica di caricamento diretta a IMIUDataManager
                return _dataManager.LoadRuleStatistics();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[LearningStatisticsManager] Errore caricamento RuleStatistics: {ex.Message}. Restituisco dizionario vuoto.");
                return new Dictionary<long, RuleStatistics>();
            }
        }

        /// <summary>
        /// Salva (upsert) le statistiche delle regole di apprendimento nel database tramite il data manager.
        /// </summary>
        public void SaveRuleStatistics(Dictionary<long, RuleStatistics> ruleStats)
        {
            _logger.Log(LogLevel.DEBUG, $"[LearningStatisticsManager DEBUG] Richiesta SaveRuleStatistics per {ruleStats?.Count ?? 0} statistiche.");
            try
            {
                // Delega la logica di salvataggio diretta a IMIUDataManager
                _dataManager.SaveRuleStatistics(ruleStats);
                _logger.Log(LogLevel.DEBUG, $"[LearningStatisticsManager] Salvate {ruleStats.Count} RuleStatistics tramite il data manager.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[LearningStatisticsManager] Errore salvataggio RuleStatistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Carica le statistiche di transizione di apprendimento dal database tramite il data manager.
        /// </summary>
        /// <returns>Un dizionario di TransitionStatistics, con chiave (ParentStringCompressed, AppliedRuleID).</returns>
        public Dictionary<Tuple<string, long>, TransitionStatistics> LoadTransitionStatistics()
        {
            _logger.Log(LogLevel.DEBUG, "[LearningStatisticsManager DEBUG] Richiesta LoadTransitionStatistics.");
            try
            {
                // Delega la logica di caricamento diretta a IMIUDataManager
                return _dataManager.LoadTransitionStatistics();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[LearningStatisticsManager] Errore caricamento TransitionStatistics: {ex.Message}. Restituisco dizionario vuoto.");
                return new Dictionary<Tuple<string, long>, TransitionStatistics>();
            }
        }

        // Nuovo metodo asincrono
        public async Task<Dictionary<Tuple<string, long>, TransitionStatistics>> GetTransitionProbabilitiesAsync()
        {
            return await Task.Run(() => GetTransitionProbabilities());
        }

        /// <summary>
        /// Salva (upsert) le statistiche di transizione di apprendimento nel database tramite il data manager.
        /// </summary>
        public void SaveTransitionStatistics(Dictionary<Tuple<string, long>, TransitionStatistics> transitionStats)
        {
            _logger.Log(LogLevel.DEBUG, $"[LearningStatisticsManager DEBUG] Richiesta SaveTransitionStatistics per {transitionStats?.Count ?? 0} statistiche.");
            try
            {
                // Delega la logica di salvataggio diretta a IMIUDataManager
                _dataManager.SaveTransitionStatistics(transitionStats);
                _logger.Log(LogLevel.DEBUG, $"[LearningStatisticsManager] Salvate {transitionStats.Count} TransitionStatistics tramite il data manager.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[LearningStatisticsManager] Errore salvataggio TransitionStatistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Carica le statistiche di transizione aggregate (conteggi totali e di successo)
        /// dal database tramite il data manager, calcolando la probabilità di successo per ogni transizione.
        /// Questo metodo rappresenta la "topografia pesata e dinamica" basata su dati storici reali.
        /// </summary>
        /// <returns>Un dizionario di TransitionStatistics, con chiave (ParentStringCompressed, AppliedRuleID).</returns>
        public Dictionary<Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics> GetTransitionProbabilities()
        {
            _logger.Log(LogLevel.DEBUG, "[LearningStatisticsManager DEBUG] Richiesta GetTransitionProbabilities.");
            try
            {
                // Ora delega la richiesta di dati aggregati direttamente a IMIUDataManager.
                // IMIUDataManager (implementato da MIUDatabaseManager) sarà responsabile dell'esecuzione
                // della query SQL di aggregazione.
                return _dataManager.GetTransitionProbabilities();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[LearningStatisticsManager] Errore durante il recupero delle probabilità di transizione dal data manager: {ex.Message}. Restituisco dizionario vuoto.");
                return new Dictionary<Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics>();
            }
        }
    }
}
