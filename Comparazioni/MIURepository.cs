// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\MIURepository.cs
// Data di riferimento: 20 giugno 2025 (Correzione definitiva tipi Dictionary a long)
// Questa classe funge da Repository per il sistema MIU, inclusi i metodi per le statistiche di apprendimento.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterLog; // Necessario per la tua classe Logger

namespace MIU.Core
{
    /// <summary>
    /// Repository per il sistema MIU. Fornisce un'interfaccia di alto livello
    /// per le operazioni CRUD (Create, Read, Update, Delete) sulle entità del dominio MIU.
    /// Delega le operazioni di accesso al database a un'implementazione di IMIUDataManager,
    /// garantendo la separazione delle responsabilità e l'inversione di dipendenza.
    /// </summary>
    public class MIURepository
    {
        private readonly IMIUDataManager _dbManager; // Ora dipende dall'interfaccia
        private readonly Logger _logger; // Campo per l'istanza del logger

        /// <summary>
        /// Costruttore di MIURepository.
        /// Inietta l'istanza dell'implementazione di IMIUDataManager.
        /// </summary>
        /// <param name="dbManager">L'istanza dell'implementazione di IMIUDataManager.</param>
        /// <param name="logger">L'istanza del logger per la registrazione degli eventi.</param>
        public MIURepository(IMIUDataManager dbManager, Logger logger)
        {
            _dbManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager), "IMIUDataManager non può essere nullo.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger non può essere nullo.");
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] MIURepository istanziato con IMIUDataManager.");
        }

        // --- Metodi per le Entità del Dominio MIU ---

        /// <summary>
        /// Inserisce una nuova ricerca MIU.
        /// </summary>
        /// <returns>L'ID della ricerca appena inserita.</returns>
        public long InsertSearch(string initialString, string targetString, string searchAlgorithm)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta inserimento Search: {initialString} -> {targetString}");
            return _dbManager.InsertSearch(initialString, targetString, searchAlgorithm);
        }

        /// <summary>
        /// Aggiorna i dettagli di una ricerca esistente.
        /// </summary>
        public void UpdateSearch(long searchId, bool success, double flightTimeMs, int stepsTaken, int nodesExplored, int maxDepthReached)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta aggiornamento Search ID: {searchId}");
            _dbManager.UpdateSearch(searchId, success, flightTimeMs, stepsTaken, nodesExplored, maxDepthReached);
        }

        /// <summary>
        /// Inserisce o aggiorna uno stato MIU nel database.
        /// </summary>
        /// <param name="miuString">La stringa MIU standard dello stato.</param>
        /// <returns>L'ID dello stato MIU.</returns>
        public long UpsertMIUState(string miuString)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta Upsert MIU State: {miuString}");
            return _dbManager.UpsertMIUState(miuString);
        }

        /// <summary>
        /// Registra un'applicazione di una regola durante una ricerca.
        /// </summary>
        public void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long appliedRuleID, int currentDepth)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta inserimento Rule Application: SearchID={searchId}, Parent={parentStateId}, New={newStateId}, Rule={appliedRuleID}.");
            _dbManager.InsertRuleApplication(searchId, parentStateId, newStateId, appliedRuleID, currentDepth);
        }

        /// <summary>
        /// Registra un passo nel percorso della soluzione di una ricerca.
        /// </summary>
        public void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, long? appliedRuleID, bool isTarget, bool isSuccess, int depth)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta inserimento Path Step: SearchID={searchId}, Step={stepNumber}, StateID={stateId}.");
            _dbManager.InsertSolutionPathStep(searchId, stepNumber, stateId, parentStateId, appliedRuleID, isTarget, isSuccess, depth);
        }

        /// <summary>
        /// Carica tutte le regole MIU dal database.
        /// </summary>
        /// <returns>Una lista di oggetti RegolaMIU.</returns>
        public List<RegolaMIU> LoadRegoleMIU()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta caricamento RegoleMIU.");
            return _dbManager.LoadRegoleMIU();
        }

        /// <summary>
        /// Inserisce o aggiorna un elenco di regole MIU nel database.
        /// </summary>
        public void UpsertRegoleMIU(List<RegolaMIU> regole)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta Upsert di {regole.Count} RegoleMIU.");
            _dbManager.UpsertRegoleMIU(regole);
        }

        /// <summary>
        /// Carica i parametri di configurazione del sistema.
        /// </summary>
        /// <returns>Un dizionario di parametri.</returns>
        public Dictionary<string, string> LoadMIUParameterConfigurator()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta caricamento MIU Parameter Configurator.");
            return _dbManager.LoadMIUParameterConfigurator();
        }

        /// <summary>
        /// Salva i parametri di configurazione del sistema.
        /// </summary>
        public void SaveMIUParameterConfigurator(Dictionary<string, string> config)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta salvataggio di {config.Count} MIU Parameter Configurator.");
            _dbManager.SaveMIUParameterConfigurator(config);
        }

        // Metodi per le statistiche di apprendimento (Chiave Dictionary ora 'long' e Tuple.Item2 a 'long')
        /// <summary>
        /// Carica le statistiche di apprendimento delle regole.
        /// </summary>
        /// <returns>Un dizionario di RuleStatistics.</returns>
        public Dictionary<long, RuleStatistics> LoadRuleStatistics() // Modificato il tipo della chiave a long
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta caricamento RuleStatistics.");
            return _dbManager.LoadRuleStatistics();
        }

        /// <summary>
        /// Salva le statistiche di apprendimento delle regole.
        /// </summary>
        public void SaveRuleStatistics(Dictionary<long, RuleStatistics> ruleStats) // Modificato il tipo della chiave a long
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta salvataggio di {ruleStats.Count} RuleStatistics.");
            _dbManager.SaveRuleStatistics(ruleStats);
        }

        /// <summary>
        /// Carica le statistiche delle transizioni di apprendimento.
        /// </summary>
        /// <returns>Un dizionario di TransitionStatistics.</returns>
        public Dictionary<Tuple<string, long>, TransitionStatistics> LoadTransitionStatistics() // Modificato il tipo della chiave a long
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta caricamento TransitionStatistics.");
            return _dbManager.LoadTransitionStatistics();
        }

        /// <summary>
        /// Salva le statistiche delle transizioni di apprendimento.
        /// </summary>
        public void SaveTransitionStatistics(Dictionary<Tuple<string, long>, TransitionStatistics> transitionStats) // Modificato il tipo della chiave a long
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta salvataggio di {transitionStats.Count} TransitionStatistics.");
            _dbManager.SaveTransitionStatistics(transitionStats);
        }
    }
}
