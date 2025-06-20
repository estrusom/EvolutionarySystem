// File: C:\Progetti\EvolutiveSystem\MIU.Core\MIURepository.cs
// Data di riferimento: 21 giugno 2025 (Versione definitiva allineata allo schema MIU_Searches)
// Modifiche per Fase 2.3.2 - Inserimento dati nelle colonne esistenti di MIU_Searches.
// CORREZIONI: Nome tabella 'MIU_Searches', colonne 'SearchAlgorithm', 'Outcome', 'MaxDepth'.

using EvolutiveSystem.Common; // Per RegolaMIU, RuleStatistics, MIUParameterConfigurator, Search, RuleApplication, MIUState, PathStepInfo
using EvolutiveSystem.SQL.Core; // Per IMIUDataManager
using MasterLog; // Per Logger
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{
    /// <summary>
    /// Repository class for managing MIU-related data persistence.
    /// Interacts with the IMIUDataManager to perform CRUD operations on the SQLite database.
    /// </summary>
    public class MIURepository
    {
        private readonly IMIUDataManager _dataManager;
        private readonly Logger _logger;

        /// <summary>
        /// Constructor for MIURepository.
        /// </summary>
        /// <param name="dataManager">An implementation of IMIUDataManager for database access.</param>
        /// <param name="logger">An instance of the Logger for logging messages.</param>
        public MIURepository(IMIUDataManager dataManager, Logger logger)
        {
            _dataManager = dataManager;
            _logger = logger;
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] MIURepository istanziato con IMIUDataManager.");
        }

        /// <summary>
        /// Loads MIU rules from the database.
        /// </summary>
        /// <returns>A list of RegolaMIU objects.</returns>
        public List<RegolaMIU> LoadRegoleMIU()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta LoadRegoleMIU.");
            List<RegolaMIU> regole = new List<RegolaMIU>();
            try
            {
                List<string> rawData = _dataManager.SQLiteSelect("SELECT ID, Nome, Pattern, Sostituzione, Descrizione FROM RegoleMIU;");
                foreach (string row in rawData)
                {
                    string[] fields = row.Split(';');
                    if (fields.Length >= 5)
                    {
                        regole.Add(new RegolaMIU(
                            Convert.ToInt64(fields[0]),
                            fields[1],
                            fields[4], // Descrizione
                            fields[2], // Pattern
                            fields[3]  // Sostituzione
                        ));
                    }
                }
                _logger.Log(LogLevel.DEBUG, "RegoleMIU caricate dal database.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nel caricamento delle RegoleMIU: {ex.Message}");
            }
            return regole;
        }

        /// <summary>
        /// Loads RuleStatistics from the database.
        /// </summary>
        /// <returns>A dictionary of RuleStatistics, keyed by RuleID.</returns>
        public Dictionary<long, RuleStatistics> LoadRuleStatistics()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta LoadRuleStatistics.");
            Dictionary<long, RuleStatistics> stats = new Dictionary<long, RuleStatistics>();
            try
            {
                List<string> rawData = _dataManager.SQLiteSelect("SELECT RuleID, ApplicationCount, SuccessfulCount, EffectivenessScore, LastApplicationTimestamp FROM RuleStatistics;");
                foreach (string row in rawData)
                {
                    string[] fields = row.Split(';');
                    if (fields.Length >= 5)
                    {
                        long ruleId = Convert.ToInt64(fields[0]);
                        stats[ruleId] = new RuleStatistics
                        {
                            RuleID = ruleId,
                            ApplicationCount = Convert.ToInt64(fields[1]),
                            SuccessfulCount = Convert.ToInt64(fields[2]),
                            EffectivenessScore = Convert.ToDouble(fields[3], System.Globalization.CultureInfo.InvariantCulture), // Ensure correct double parsing
                            LastApplicationTimestamp = DateTime.Parse(fields[4])
                        };
                    }
                }
                _logger.Log(LogLevel.DEBUG, "RuleStatistics caricate dal database.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nel caricamento delle RuleStatistics: {ex.Message}");
            }
            return stats;
        }

        /// <summary>
        /// Saves or updates RuleStatistics in the database.
        /// </summary>
        /// <param name="stats">Dictionary of RuleStatistics to save.</param>
        public void SaveRuleStatistics(Dictionary<long, RuleStatistics> stats)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta SaveRuleStatistics per {stats.Count} statistiche.");
            try
            {
                foreach (var entry in stats)
                {
                    RuleStatistics ruleStats = entry.Value;
                    string query = $@"
                        INSERT OR REPLACE INTO RuleStatistics (
                            RuleID, ApplicationCount, SuccessfulCount, EffectivenessScore, LastApplicationTimestamp
                        ) VALUES (
                            {ruleStats.RuleID}, {ruleStats.ApplicationCount}, {ruleStats.SuccessfulCount},
                            {ruleStats.EffectivenessScore.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                            '{ruleStats.LastApplicationTimestamp.ToString("yyyy-MM-dd HH:mm:ss")}'
                        );";
                    _dataManager.SQLiteQuery(query);
                }
                _logger.Log(LogLevel.DEBUG, $"Salvate {stats.Count} RuleStatistics nel database.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nel salvataggio delle RuleStatistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads MIU parameter configurator settings from the database.
        /// </summary>
        /// <returns>A dictionary of parameter names and their values.</returns>
        public Dictionary<string, string> LoadMIUParameterConfigurator()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta LoadMIUParameterConfigurator.");
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            try
            {
                List<string> rawData = _dataManager.SQLiteSelect("SELECT Name, Value FROM MIUParameterConfigurator;");
                foreach (string row in rawData)
                {
                    string[] fields = row.Split(';');
                    if (fields.Length >= 2)
                    {
                        parameters[fields[0]] = fields[1];
                    }
                }
                _logger.Log(LogLevel.DEBUG, "Parametri da MIUParameterConfigurator caricati.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nel caricamento dei parametri di configurazione: {ex.Message}");
            }
            return parameters;
        }

        /// <summary>
        /// Inserts a new search record into the database.
        /// Uses the MIU_Searches table.
        /// </summary>
        /// <param name="initialString">The initial string of the search (standard format).</param>
        /// <param name="targetString">The target string of the search (standard format).</param>
        /// <param name="algoUsed">The algorithm used for the search (e.g., 'BFS', 'DFS', 'AUTO').</param>
        /// <param name="initialStringLength">Length of the initial string.</param>
        /// <param name="targetStringLength">Length of the target string.</param>
        /// <param name="initialIcount">Count of 'I' in the initial string.</param>
        /// <param name="initialUcount">Count of 'U' in the initial string.</param>
        /// <param name="targetIcount">Count of 'I' in the target string.</param>
        /// <param name="targetUcount">Count of 'U' in the target string.</param>
        /// <returns>The ID of the newly inserted search record.</returns>
        // MODIFIED: Corrected table name to MIU_Searches and included all characteristic parameters.
        public long InsertSearch(
            string initialString,
            string targetString,
            string algoUsed, // Corrisponde a SearchAlgorithm
            int initialStringLength,
            int targetStringLength,
            int initialIcount,
            int initialUcount,
            int targetIcount,
            int targetUcount
            )
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta inserimento Search: {initialString} -> {targetString}");
            long searchId = -1;
            try
            {
                // Corretto nome tabella a MIU_Searches e nomi colonne
                string query = $@"
                    INSERT INTO MIU_Searches (
                        InitialString, TargetString, SearchAlgorithm, StartTime,
                        InitialStringLength, TargetStringLength, InitialIcount, InitialUcount, TargetIcount, TargetUcount
                    ) VALUES (
                        '{_dataManager.SanitizeString(initialString)}',
                        '{_dataManager.SanitizeString(targetString)}',
                        '{_dataManager.SanitizeString(algoUsed)}', -- Mappato a SearchAlgorithm
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
                        {initialStringLength}, {targetStringLength}, {initialIcount}, {initialUcount}, {targetIcount}, {targetUcount}
                    );
                    SELECT last_insert_rowid();"; // Ottiene l'ID dell'ultima riga inserita

                searchId = _dataManager.SQLiteQuery(query);
                _logger.Log(LogLevel.DEBUG, $"Search inserita: Initial='{initialString}', Target='{targetString}', Algo='{algoUsed}'. ID: {searchId}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nell'inserimento della Search: {ex.Message}");
            }
            return searchId;
        }

        /// <summary>
        /// Updates an existing search record with results.
        /// Uses the MIU_Searches table.
        /// </summary>
        /// <param name="searchId">The ID of the search record to update.</param>
        /// <param name="success">Whether the search was successful.</param>
        /// <param name="elapsedMilliseconds">Time taken for the search in milliseconds.</param>
        /// <param name="stepsTaken">Number of steps in the solution path.</param>
        /// <param name="nodesExplored">Number of nodes explored during the search.</param>
        /// <param name="maxDepthReached">Maximum depth reached during the search.</param>
        public void UpdateSearch(long searchId, bool success, long elapsedMilliseconds, int stepsTaken, int nodesExplored, int maxDepthReached)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta aggiornamento Search ID: {searchId}");
            try
            {
                // Corretto nome tabella a MIU_Searches e nomi colonne
                string query = $@"
                    UPDATE MIU_Searches SET
                        Outcome = '{ (success ? "Success" : "Failure") }', -- Mappato 'success' boolean a 'Outcome' text
                        ElapsedMilliseconds = {elapsedMilliseconds},
                        StepsTaken = {stepsTaken},
                        NodesExplored = {nodesExplored},
                        MaxDepth = {maxDepthReached}, -- Mappato 'MaxDepthReached' a 'MaxDepth'
                        EndTime = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' -- Aggiunto EndTime
                    WHERE SearchID = {searchId};"; // Usare SearchID per la clausola WHERE

                _dataManager.SQLiteQuery(query);
                _logger.Log(LogLevel.DEBUG, $"Search '{searchId}' aggiornata: Success={success}, ElapsedMilliseconds={elapsedMilliseconds}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nell'aggiornamento della Search ID {searchId}: {ex.Message}");
            }
        }


        /// <summary>
        /// Inserts or updates an MIU state in the MIU_States table.
        /// </summary>
        /// <param name="stateString">The standard (decompressed) MIU string.</param>
        /// <returns>The ID of the MIU state.</returns>
        public long UpsertMIUState(string stateString)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta UpsertMIUState per '{stateString}'.");
            long stateId = -1;
            string compressedString = MIUStringConverter.InflateMIUString(stateString); // Questa variabile non è usata dopo la compressione
            string stringHash = MIUStringConverter.CalculateHash(stateString);

            try
            {
                // Prima, prova a trovare lo stato
                List<string> existing = _dataManager.SQLiteSelect($"SELECT StateID, CurrentString, UsageCount FROM MIU_States WHERE Hash = '{stringHash}';");

                if (existing != null && existing.Any())
                {
                    // Lo stato esiste, aggiorna UsageCount
                    string[] fields = existing[0].Split(';');
                    stateId = Convert.ToInt64(fields[0]);
                    long usageCount = Convert.ToInt64(fields[2]) + 1;
                    string updateQuery = $"UPDATE MIU_States SET UsageCount = {usageCount} WHERE StateID = {stateId};";
                    _dataManager.SQLiteQuery(updateQuery);
                    _logger.Log(LogLevel.DEBUG, $"MIUState '{stateString}' aggiornato. ID: {stateId}");
                }
                else
                {
                    // Lo stato non esiste, inserisci nuovo
                    string insertQuery = $@"
                        INSERT INTO MIU_States (
                            CurrentString, StringLength, Hash, DiscoveryTime_Int, DiscoveryTime_Text, UsageCount
                        ) VALUES (
                            '{_dataManager.SanitizeString(stateString)}',
                            {stateString.Length},
                            '{stringHash}',
                            {new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()},
                            '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
                            1
                        );
                        SELECT last_insert_rowid();"; // Ottiene l'ID della riga appena inserita
                    stateId = _dataManager.SQLiteQuery(insertQuery);
                    _logger.Log(LogLevel.DEBUG, $"MIUState '{stateString}' inserito. ID: {stateId}");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nell'upsert di MIUState '{stateString}': {ex.Message}");
            }
            return stateId;
        }

        /// <summary>
        /// Inserts a record of a rule application during a search.
        /// </summary>
        /// <param name="searchId">The ID of the current search.</param>
        /// <param name="parentStateId">The ID of the state before the rule was applied.</param>
        /// <param name="newStateId">The ID of the state after the rule was applied.</param>
        /// <param name="ruleId">The ID of the rule that was applied.</param>
        /// <param name="depth">The depth of the application in the search tree.</param>
        public void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long ruleId, int depth)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta InsertRuleApplication per SearchID={searchId}, RuleID={ruleId}.");
            try
            {
                string query = $@"
                    INSERT INTO RuleApplications (
                        SearchID, RuleID, ParentStateID, NewStateID, ApplicationTime, Depth
                    ) VALUES (
                        {searchId}, {ruleId}, {parentStateId}, {newStateId},
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', {depth}
                    );";
                _dataManager.SQLiteQuery(query);
                _logger.Log(LogLevel.DEBUG, $"Applicazione Regola inserita: SearchID={searchId}, Parent={parentStateId}, New={newStateId}, Rule={ruleId}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nell'inserimento dell'applicazione della regola per SearchID {searchId}, RuleID {ruleId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Inserts a step into the solution path for a given search.
        /// Uses the SolutionPaths table.
        /// </summary>
        /// <param name="searchId">The ID of the search.</param>
        /// <param name="stepNumber">The sequential number of the step in the path.</param>
        /// <param name="stateId">The ID of the MIU state at this step.</param>
        /// <param name="parentStateId">The ID of the parent MIU state (can be null for initial step).</param>
        /// <param name="appliedRuleId">The ID of the rule applied to reach this state (can be null for initial step).</param>
        /// <param name="isTarget">Indicates if this state is the target string.</param>
        /// <param name="solutionFound">Indicates if a solution was found for this search.</param>
        /// <param name="depth">The depth of this state in the search tree.</param>
        public void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, long? appliedRuleId, bool isTarget, bool solutionFound, int depth)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta InsertSolutionPathStep per SearchID={searchId}, Step={stepNumber}.");
            try
            {
                string parentIdValue = parentStateId.HasValue ? parentStateId.Value.ToString() : "NULL";
                string ruleIdValue = appliedRuleId.HasValue ? appliedRuleId.Value.ToString() : "NULL";

                string query = $@"
                    INSERT INTO SolutionPaths (
                        SearchID, StepNumber, StateID, ParentStateID, AppliedRuleID,
                        IsTarget, SolutionFound, Depth, Timestamp
                    ) VALUES (
                        {searchId}, {stepNumber}, {stateId}, {parentIdValue}, {ruleIdValue},
                        {Convert.ToInt32(isTarget)}, {Convert.ToInt32(solutionFound)}, {depth},
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'
                    );";
                _dataManager.SQLiteQuery(query);
                _logger.Log(LogLevel.DEBUG, $"Passo Path soluzione inserito: SearchID={searchId}, Step={stepNumber}, StateID={stateId}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nell'inserimento del passo della soluzione per SearchID {searchId}, Step {stepNumber}: {ex.Message}");
            }
        }
    }
}
