using EvolutiveSystem.SQL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{
    /// <summary>
    /// Repository per la gestione della persistenza dei dati relativi alle ricerche MIU
    /// e agli stati intermedi nel database. Utilizza MIUDatabaseManager per le operazioni SQLite.
    /// Implementa le interfacce IMIURepository e ILearningStatePersistence definite nel progetto MIU.Core.
    /// </summary>
    public class MIURepository : IMIURepository, ILearningStatePersistence // <<< AGGIUNTO: Implementa entrambe le interfacce
    {
        private readonly MIUDatabaseManager _dbManager;

        /// <summary>
        /// Inizializza una nuova istanza del MIURepository.
        /// </summary>
        /// <param name="dbManager">Un'istanza di MIUDatabaseManager già aperta.</param>
        public MIURepository(MIUDatabaseManager dbManager)
        {
            _dbManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager));
        }

        // <<< Implementazione dei metodi di IMIURepository >>>

        /// <summary>
        /// Inserisce una nuova ricerca nel database.
        /// </summary>
        /// <param name="initialString">La stringa iniziale della ricerca (compressa).</param>
        /// <param name="targetString">La stringa target della ricerca (compressa).</param>
        /// <param name="searchAlgorithm">L'algoritmo di ricerca utilizzato (es. "BFS", "DFS").</param>
        /// <returns>L'ID della ricerca appena inserita.</returns>
        public long InsertSearch(string initialString, string targetString, string searchAlgorithm)
        {
            return _dbManager.InsertSearch(initialString, targetString, searchAlgorithm);
        }

        /// <summary>
        /// Aggiorna una ricerca esistente nel database.
        /// </summary>
        /// <param name="searchId">L'ID della ricerca da aggiornare.</param>
        /// <param name="success">Indica se la ricerca ha avuto successo.</param>
        /// <param name="flightTimeMs">Il tempo di esecuzione della ricerca in millisecondi.</param>
        /// <param name="stepsTaken">Il numero di passi compiuti per trovare la soluzione.</param>
        /// <param name="nodesExplored">Il numero totale di nodi esplorati.</param>
        /// <param name="maxDepthReached">La profondità massima raggiunta durante la ricerca.</param>
        public void UpdateSearch(long searchId, bool success, double flightTimeMs, int stepsTaken, int nodesExplored, int maxDepthReached)
        {
            _dbManager.UpdateSearch(searchId, success, flightTimeMs, stepsTaken, nodesExplored, maxDepthReached);
        }

        /// <summary>
        /// Inserisce o recupera un MIU_State dalla tabella MIU_States.
        /// Se la stringa esiste già, restituisce il suo StateID. Altrimenti, la inserisce e restituisce il nuovo StateID.
        /// </summary>
        /// <param name="miuString">La stringa MIU in formato standard (non compresso).</param>
        /// <returns>Lo StateID della stringa.</returns>
        public long UpsertMIUState(string miuString)
        {
            return _dbManager.UpsertMIUState(miuString);
        }

        /// <summary>
        /// Inserisce un'applicazione di regola nella tabella MIU_RuleApplications.
        /// </summary>
        /// <param name="searchId">L'ID della ricerca a cui appartiene l'applicazione.</param>
        /// <param name="parentStateId">L'ID dello stato genitore.</param>
        /// <param name="newStateId">L'ID del nuovo stato generato.</param>
        /// <param name="appliedRuleId">L'ID della regola applicata.</param>
        /// <param name="currentDepth">La profondità corrente dell'applicazione.</param>
        public void InsertRuleApplication(long searchId, long parentStateId, long newStateId, int appliedRuleId, int currentDepth)
        {
            _dbManager.InsertRuleApplication(searchId, parentStateId, newStateId, appliedRuleId, currentDepth);
        }

        /// <summary>
        /// Inserisce un passo di un percorso di soluzione nella tabella MIU_Paths.
        /// </summary>
        /// <param name="searchId">L'ID della ricerca a cui appartiene il percorso.</param>
        /// <param name="stepNumber">Il numero del passo nel percorso.</param>
        /// <param name="stateId">L'ID dello stato corrente.</param>
        /// <param name="parentStateId">L'ID dello stato genitore (null per il primo passo).</param>
        /// <param name="appliedRuleId">L'ID della regola applicata (null per il primo passo).</param>
        /// <param name="isTarget">True se questo stato è la stringa target.</param>
        /// <param name="isSuccess">True se questo stato fa parte di un percorso di successo finale.</param>
        /// <param name="depth">La profondità dello stato nel percorso.</param>
        public void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, int? appliedRuleId, bool isTarget, bool isSuccess, int depth)
        {
            _dbManager.InsertSolutionPathStep(searchId, stepNumber, stateId, parentStateId, appliedRuleId, isTarget, isSuccess, depth);
        }

        /// <summary>
        /// Carica le regole MIU dal database.
        /// </summary>
        /// <returns>Una lista di oggetti RegolaMIU.</returns>
        public List<RegolaMIU> LoadRegoleMIU()
        {
            return _dbManager.LoadRegoleMIU(); // Delega a MIUDatabaseManager
        }

        /// <summary>
        /// Inserisce o aggiorna le regole MIU nel database.
        /// </summary>
        /// <param name="regole">La lista delle regole da inserire/aggiornare.</param>
        public void UpsertRegoleMIU(List<RegolaMIU> regole)
        {
            _dbManager.UpsertRegoleMIU(regole);
        }

        // <<< Implementazione dei metodi di ILearningStatePersistence >>>

        /// <summary>
        /// Carica le statistiche aggregate delle regole dal database.
        /// </summary>
        /// <returns>Una lista di RuleStatistics.</returns>
        public List<RuleStatistics> LoadRuleStatistics()
        {
            return _dbManager.LoadRuleStatistics();
        }

        /// <summary>
        /// Salva (upsert) le statistiche aggregate delle regole nel database.
        /// </summary>
        /// <param name="ruleStats">La lista di RuleStatistics da salvare.</param>
        public void SaveRuleStatistics(List<RuleStatistics> ruleStats)
        {
            _dbManager.SaveRuleStatistics(ruleStats);
        }

        /// <summary>
        /// Carica le statistiche aggregate delle transizioni (parent-child-rule) dal database.
        /// </summary>
        /// <returns>Una lista di TransitionStatistics.</returns>
        public List<TransitionStatistics> LoadTransitionStatistics()
        {
            return _dbManager.LoadTransitionStatistics();
        }

        /// <summary>
        /// Salva (upsert) le statistiche aggregate delle transizioni nel database.
        /// </summary>
        /// <param name="transitionStats">La lista di TransitionStatistics da salvare.</param>
        public void SaveTransitionStatistics(List<TransitionStatistics> transitionStats)
        {
            _dbManager.SaveTransitionStatistics(transitionStats);
        }
    }
}
