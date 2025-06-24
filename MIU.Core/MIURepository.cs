// File: C:\Progetti\EvolutiveSystem\MIU.Core\MIURepository.cs
// AGGIORNAMENTO 21.6.25: Ricostruzione completa del file MIURepository.cs
// per ripristinare tutti i metodi mancanti e allineare la firma di InsertSearch
// con IMIUDataManager, risolvendo gli errori in Program.cs.

using MasterLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Assicurati che EvolutiveSystem.SQL.Core sia referenziato nel progetto MIU.Core.csproj
using EvolutiveSystem.Common; // Aggiunto per le classi modello spostate

namespace MIU.Core
{
    public class MIURepository : IMIURepository 
    {
        private readonly IMIUDataManager _dataManager;
        private readonly Logger _logger;

        public MIURepository(IMIUDataManager dataManager, Logger logger)
        {
            _dataManager = dataManager;
            _logger = logger;
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] MIURepository istanziato con IMIUDataManager.");
        }

        /// <summary>
        /// Inserisce una nuova entry nella tabella MIU_Searches.
        /// Accetta tutti i parametri, inclusi i dati di lunghezza e conteggio,
        /// e li passa direttamente al data manager.
        /// </summary>
        /// <param name="initialString">La stringa iniziale standard (decompressa).</param>
        /// <param name="targetString">La stringa target standard (decompressa).</param>
        /// <param name="algorithm">L'algoritmo di ricerca usato (es. "BFS", "DFS").</param>
        /// <param name="initialStringLength">Lunghezza della stringa iniziale.</param>
        /// <param name="targetStringLength">Lunghezza della stringa target.</param>
        /// <param name="initialIcount">Conteggio 'I' nella stringa iniziale.</param>
        /// <param name="initialUcount">Conteggio 'U' nella stringa iniziale.</param>
        /// <param name="targetIcount">Conteggio 'I' nella stringa target.</param>
        /// <param name="targetUcount">Conteggio 'U' nella stringa target.</param>
        /// <returns>L'ID della nuova ricerca.</returns>
        public long InsertSearch(
            string initialString,
            string targetString,
            string algorithm,
            int initialStringLength,
            int targetStringLength,
            int initialIcount,
            int initialUcount,
            int targetIcount,
            int targetUcount
            )
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta inserimento Search: {initialString} -> {targetString}");

            // Delega l'operazione al data manager, passando tutti i parametri ricevuti
            long searchId = _dataManager.InsertSearch(
                initialString,
                targetString,
                algorithm,
                initialStringLength,
                targetStringLength,
                initialIcount,
                initialUcount,
                targetIcount,
                targetUcount
            );
            _logger.Log(LogLevel.DEBUG, $"Search inserita tramite DataManager. ID: {searchId}.");
            return searchId;
        }

        /// <summary>
        /// Aggiorna una ricerca MIU esistente con i risultati finali.
        /// Delega al data manager.
        /// </summary>
        public void UpdateSearch(long searchId, bool success, double flightTimeMs, int stepsTaken, int nodesExplored, int maxDepthReached)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta aggiornamento Search ID: {searchId}");
            _dataManager.UpdateSearch(searchId, success, flightTimeMs, stepsTaken, nodesExplored, maxDepthReached);
            _logger.Log(LogLevel.DEBUG, $"Search '{searchId}' aggiornata tramite DataManager.");
        }

        /// <summary>
        /// Inserisce o aggiorna uno stato MIU. Delega al data manager.
        /// </summary>
        /// <param name="miuString">La stringa MIU standard.</param>
        /// <returns>L'ID dello stato MIU nel database.</returns>
        public long UpsertMIUState(string miuString)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta UpsertMIUState per '{miuString}'.");
            return _dataManager.UpsertMIUState(miuString);
        }

        /// <summary>
        /// Registra un'applicazione di una regola MIU. Delega al data manager.
        /// </summary>
        public void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long appliedRuleID, int currentDepth)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta InsertRuleApplication per SearchID={searchId}, RuleID={appliedRuleID}.");
            _dataManager.InsertRuleApplication(searchId, parentStateId, newStateId, appliedRuleID, currentDepth);
        }

        /// <summary>
        /// Registra un passo del percorso della soluzione. Delega al data manager.
        /// </summary>
        public void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, long? appliedRuleID, bool isTarget, bool isSuccess, int depth)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta InsertSolutionPathStep per SearchID={searchId}, Step={stepNumber}.");
            _dataManager.InsertSolutionPathStep(searchId, stepNumber, stateId, parentStateId, appliedRuleID, isTarget, isSuccess, depth);
        }

        /// <summary>
        /// Carica tutte le regole MIU. Delega al data manager.
        /// </summary>
        /// <returns>Una lista di oggetti RegolaMIU.</returns>
        public List<RegolaMIU> LoadRegoleMIU()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta LoadRegoleMIU.");
            return _dataManager.LoadRegoleMIU();
        }

        /// <summary>
        /// Inserisce o aggiorna un elenco di regole MIU. Delega al data manager.
        /// </summary>
        public void UpsertRegoleMIU(List<RegolaMIU> regole)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta UpsertRegoleMIU per {regole?.Count ?? 0} regole.");
            _dataManager.UpsertRegoleMIU(regole);
        }

        /// <summary>
        /// Carica i parametri di configurazione. Delega al data manager.
        /// </summary>
        /// <returns>Un dizionario con chiave=NomeParametro e valore=ValoreParametro.</returns>
        public Dictionary<string, string> LoadMIUParameterConfigurator()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta LoadMIUParameterConfigurator.");
            return _dataManager.LoadMIUParameterConfigurator();
        }

        /// <summary>
        /// Salva i parametri di configurazione. Delega al data manager.
        /// </summary>
        public void SaveMIUParameterConfigurator(Dictionary<string, string> config)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta SaveMIUParameterConfigurator per {config?.Count ?? 0} parametri.");
            _dataManager.SaveMIUParameterConfigurator(config);
        }

        /// <summary>
        /// Carica le statistiche delle regole. Delega al data manager.
        /// </summary>
        /// <returns>Un dizionario di RuleStatistics.</returns>
        public Dictionary<long, RuleStatistics> LoadRuleStatistics()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta LoadRuleStatistics.");
            return _dataManager.LoadRuleStatistics();
        }

        /// <summary>
        /// Salva le statistiche delle regole. Delega al data manager.
        /// </summary>
        public void SaveRuleStatistics(Dictionary<long, RuleStatistics> ruleStats)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta SaveRuleStatistics per {ruleStats?.Count ?? 0} statistiche.");
            _dataManager.SaveRuleStatistics(ruleStats);
        }

        /// <summary>
        /// Carica le statistiche di transizione. Delega al data manager.
        /// </summary>
        /// <returns>Un dizionario di TransitionStatistics.</returns>
        public Dictionary<Tuple<string, long>, TransitionStatistics> LoadTransitionStatistics()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta LoadTransitionStatistics.");
            return _dataManager.LoadTransitionStatistics();
        }

        /// <summary>
        /// Salva le statistiche di transizione. Delega al data manager.
        /// </summary>
        public void SaveTransitionStatistics(Dictionary<Tuple<string, long>, TransitionStatistics> transitionStats)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta SaveTransitionStatistics per {transitionStats?.Count ?? 0} statistiche.");
            _dataManager.SaveTransitionStatistics(transitionStats);
        }
        /// <summary>
        /// Carica tutti gli stati MIU dal database in modo asincrono.
        /// Delega al data manager.
        /// </summary>
        /// <returns>Un oggetto Task che rappresenta l'operazione asincrona, con un risultato di tipo List<MiuStateInfo>.</returns>
        public Task<List<MiuStateInfo>> LoadMIUStatesAsync()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta LoadMIUStatesAsync.");
            // Delega la chiamata al metodo asincrono dell'IMIUDataManager iniettato
            return _dataManager.LoadMIUStatesAsync();
        }
    }
}
