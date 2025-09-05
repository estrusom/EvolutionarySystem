// File: MIU.Core/IMIUDataManager.cs
// Data di riferimento: 23 giugno 2025 (Aggiornato)
// Questa interfaccia definisce il contratto per la gestione dei dati MIU.
// Questa interfaccia definisce i metodi che devono essere implementati da qualsiasi gestore
// di persistenza per il sistema MIU, garantendo un'astrazione dal meccanismo di storage sottostante.
// Aggiornato 19.06.2025: Allineamento definitivo dei tipi per AppliedRuleID e ParentStateID a long/long?.
// NUOVA MODIFICA 19.6.25 19.21: Aggiunta delle firme dei metodi per la persistenza delle statistiche di apprendimento.
// Data di riferimento: 20 giugno 2025 (Correzione definitiva tipi Dictionary a long)
// Questa interfaccia definisce il contratto per la gestione dei dati MIU.
// NUOVA MODIFICA 21.6.25: Aggiunti nuovi parametri per le caratteristiche delle stringhe ai metodi InsertSearch e UpdateSearch esistenti.
// AGGIORNATO 20.06.2025: Aggiunta firma per il metodo GetTransitionProbabilities per l'aggregazione delle statistiche.
// AGGIORNATO 20.06.2025: Reintegrate le firme dei metodi Load/Save delle statistiche di apprendimento,
// necessarie per la delega da LearningStatisticsManager.
// AGGIORNATO 21.06.2025: Aggiunta firma per SetJournalMode per incapsulare PRAGMA in MIUDatabaseManager.
// AGGIORNATO 23.06.2025: Metodi resi asincroni per I/O e modificata gestione cursore esplorazione.
// 2025.08.22 Aggiorna la stima della profondità media per una specifica regola.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvolutiveSystem.Common; // Per RegolaMIU, RuleStatistics, TransitionStatistics, MiuStateInfo, MIUExplorerCursor

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Interfaccia che definisce le operazioni di gestione dei dati per il sistema MIU.
    /// Questo separa la logica di business MIU (nel Repository) dai dettagli di persistenza.
    /// </summary>
    public interface IMIUDataManager
    {
        // Operazioni per MIU_Searches
        long InsertSearch(
            string initialString,
            string targetString,
            string searchAlgorithm,
            int initialStringLength,
            int targetStringLength,
            int initialIcount,
            int initialUcount,
            int targetIcount,
            int targetUcount
        );
        void UpdateSearch(
            long searchId,
            bool success,
            double flightTimeMs,
            int stepsTaken,
            int nodesExplored,
            int maxDepthReached
        );

        // Operazioni per MIU_States
        //long UpsertMIUState(string miuString);
        /// <summary>
        /// 2025.07.02 1.31
        /// Inserisce o aggiorna uno stato MIU nel database.
        /// </summary>
        /// <param name="miuString">La stringa MIU da inserire/aggiornare.</param>
        /// <returns>Un Tuple dove Item1 è l'ID dello stato e Item2 è true se la stringa è stata appena inserita (nuova), false se esisteva già.</returns>
        // Tuple<long, bool> UpsertMIUStateHistory(string miuString); 2025.08.03

        /// <summary>
        /// Inserisce o aggiorna uno stato MIU completo, inclusi i nuovi campi.
        /// </summary>
        /// <param name="state">L'oggetto MIUStateHistoryDb contenente tutti i dati dello stato.</param>
        /// <returns>Un Tuple dove Item1 è l'ID dello stato e Item2 è true se lo stato è nuovo, false se esisteva già.</returns>
        Tuple<long, bool> UpsertMIUStateHistory(MIUStateHistoryDb state);

        // Reso asincrono per l'I/O del database
        Task<List<MiuStateInfo>> LoadMIUStatesAsync();
        bool SearchExists(string initialString, string targetString);


        // Operazioni per MIU_RuleApplications
        void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long appliedRuleID, int currentDepth);

        // Operazioni per MIU_Paths
        void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, long? appliedRuleID, bool isTarget, bool isSuccess, int depth);

        // Operazioni per RegoleMIU
        List<RegolaMIU> LoadRegoleMIU();
        Task<List<RegolaMIU>> LoadRegoleMIUAsync(); // Nuovo metodo asincrono
        void UpsertRegoleMIU(List<RegolaMIU> regole);
        /// <summary>
        /// 2025.08.22
        /// Aggiorna la stima della profondità media per una specifica regola.
        /// </summary>
        /// <param name="ruleId">L'ID della regola.</param>
        /// <param name="averageDepth">Il valore della profondità media da salvare.</param>
        void UpdateRuleAverageDepth(long ruleId, double averageDepth);
        /// <summary>
        /// Aggiunge o aggiorna una singola regola MIU nel database.
        /// </summary>
        /// <param name="rule">La RegolaMIU da aggiungere o aggiornare.</param>
        Task AddOrUpdateRegolaMIUAsync(RegolaMIU rule);
        // Operazioni per MIUParameterConfigurator (Questi metodi sono ora ridondanti per i puntatori di esplorazione,
        // che vengono gestiti da MIUExplorerCursor, ma potrebbero essere usati per altri parametri generici)
        Dictionary<string, string> LoadMIUParameterConfigurator();
        void SaveMIUParameterConfigurator(Dictionary<string, string> config);

        // Metodi per le statistiche di apprendimento:
        Dictionary<long, RuleStatistics> LoadRuleStatistics();
        void SaveRuleStatistics(Dictionary<long, RuleStatistics> ruleStats);
        Dictionary<Tuple<string, long>, TransitionStatistics> LoadTransitionStatistics();
        void SaveTransitionStatistics(Dictionary<Tuple<string, long>, TransitionStatistics> transitionStats);
        Dictionary<Tuple<string, long>, TransitionStatistics> GetTransitionProbabilities();
        /// </summary>
        /// <param name="initialString">Filtra per la stringa iniziale di una ricerca specifica. Se nullo, non filtra.</param>
        /// <param name="startDate">Filtra le applicazioni avvenute a partire da questa data. Se nullo, non filtra.</param>
        /// <param name="endDate">Filtra le applicazioni avvenute fino a questa data. Se nullo, non filtra.</param>
        /// <param name="maxDepth">Filtra le applicazioni che hanno una profondità minore o uguale a questo valore. Se nullo, non filtra.</param>
        /// <returns>Una lista di oggetti MIUStringTopologyEdge (dati grezzi dell'applicazione di regole).</returns>
        Task<List<MIUStringTopologyEdge>> LoadRawRuleApplicationsForTopologyAsync(
            string initialString = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? maxDepth = null);
        // --- NUOVI METODI PER LA GESTIONE DEL CURSORE DI ESPLORAZIONE (Resi asincroni) ---
        /// <summary>
        /// Carica lo stato del cursore di esplorazione dal database.
        /// </summary>
        /// <returns>Un oggetto MIUExplorerCursor se i parametri esistono, altrimenti un nuovo oggetto con valori di default.</returns>
        Task<MIUExplorerCursor> LoadExplorerCursorAsync();

        /// <summary>
        /// Salva lo stato corrente del cursore di esplorazione nel database.
        /// </summary>
        /// <param name="cursor">L'oggetto MIUExplorerCursor da salvare.</param>
        Task SaveExplorerCursorAsync(MIUExplorerCursor cursor);
        /// <summary>
        /// Resetta i dati specifici dell'esplorazione (ricerche, applicazioni di regole, percorsi, statistiche di apprendimento)
        /// nel database, ma mantiene le regole base, i parametri di configurazione e gli stati MIU generati.
        /// </summary>
        Task ResetExplorationDataAsync();
        /// <summary>
        /// Inserisce o aggiorna un record di ExplorationAnomaly nel database.
        /// Se l'anomalia esiste già (stesso Type, RuleId, ContextPatternHash), viene aggiornata.
        /// Altrimenti, viene inserita come nuovo record.
        /// </summary>
        /// <param name="anomaly">L'oggetto ExplorationAnomaly da salvare.</param>
        void UpsertExplorationAnomaly(ExplorationAnomaly anomaly);

        /// <summary>
        /// Recupera tutte le anomalie di esplorazione persistite nel database.
        /// </summary>
        /// <returns>Una lista di oggetti ExplorationAnomaly.</returns>
        List<ExplorationAnomaly> GetAllExplorationAnomalies();
        /// <summary>
        /// Carica tutte le applicazioni di regole MIU dal database.
        /// </summary>
        /// <returns>Una lista di oggetti MIURuleApplication.</returns>
        List<MIURuleApplication> LoadAllRuleApplications();
        Task<List<MIUState>> GetAllMIUStatesAsync();
        Task<List<MIURuleApplication>> GetAllRuleApplicationsAsync();
        // --- METODI DA AGGIUNGERE PER LA PERSISTENZA DELLA TOPOLOGIA ---

        /// <summary>
        /// Crea un nuovo record in Topology_Runs e restituisce il suo ID.
        /// </summary>
        Task<long> CreateTopologyRunAsync(string description);

        /// <summary>
        /// Salva in blocco una lista di nodi della topologia.
        /// </summary>
        Task SaveTopologyNodesAsync(long topologyRunId, IEnumerable<MIUStringTopologyNode> nodes);

        /// <summary>
        /// Salva in blocco una lista di archi della topologia.
        /// </summary>
        Task SaveTopologyEdgesAsync(long topologyRunId, IEnumerable<MIUStringTopologyEdge> edges);

        /// <summary>
        /// Aggiorna un record esistente in Topology_Runs con i conteggi finali.
        /// </summary>
        Task UpdateTopologyRunCountsAsync(long topologyRunId, int nodeCount, int edgeCount);
    }
}
