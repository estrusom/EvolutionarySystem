// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\MIURepository.cs
// Data di riferimento: 20 giugno 2025
// Contiene la classe MIURepository per la gestione della persistenza dei dati MIU e delle statistiche.
// AGGIORNAMENTO: Utilizza il nome di tabella corretto 'Learning_RuleStatistics'
// e gestisce la colonna 'LastUpdated' come TEXT per i timestamp, leggendo e scrivendo come stringa.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization; // Necessario per CultureInfo.InvariantCulture e DateTimeStyles
using System.Linq;
using EvolutiveSystem.SQL.Core; // Assicurati che questo riferimento sia corretto per la tua libreria SQL
using MasterLog; // Necessario per il Logger

namespace MIU.Core
{
    public class MIURepository
    {
        private readonly IMIUDataManager _dataManager;
        private readonly Logger _logger;

        public MIURepository(IMIUDataManager dataManager, Logger logger)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] MIURepository istanziato con IMIUDataManager.");
        }

        #region Regole MIU

        /// <summary>
        /// Carica tutte le regole MIU dal database.
        /// </summary>
        /// <returns>Una lista di oggetti RegolaMIU.</returns>
        public List<RegolaMIU> LoadRegoleMIU()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta caricamento RegoleMIU.");
            List<RegolaMIU> regole = new List<RegolaMIU>();
            try
            {
                // Assumiamo che la tabella si chiami 'RegoleMIU' e contenga le colonne specificate
                DataTable dt = _dataManager.ExecuteSelect("SELECT ID, Nome, Pattern, Sostituzione, Descrizione FROM RegoleMIU;");

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        long id = Convert.ToInt64(row["ID"]);
                        string nome = row["Nome"].ToString();
                        string pattern = row["Pattern"].ToString();
                        string sostituzione = row["Sostituzione"].ToString();
                        string descrizione = row["Descrizione"].ToString();
                        regole.Add(new RegolaMIU(id, nome, descrizione, pattern, sostituzione));
                    }
                    _logger.Log(LogLevel.DEBUG, "RegoleMIU caricate dal database.");
                }
                else
                {
                    _logger.Log(LogLevel.WARNING, "Nessuna regola MIU trovata nel database.");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nel caricamento delle regole MIU: {ex.Message}");
            }
            return regole;
        }

        #endregion

        #region Stati MIU

        /// <summary>
        /// Inserisce o aggiorna uno stato MIU nel database.
        /// </summary>
        /// <param name="miuStringStandard">La stringa MIU standard.</param>
        /// <returns>L'ID dello stato inserito/aggiornato.</returns>
        public long UpsertMIUState(string miuStringStandard)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta Upsert MIU State: {miuStringStandard}");
            long stateId;
            try
            {
                // Controlla se lo stato esiste già
                DataTable dt = _dataManager.ExecuteSelect($"SELECT StateID FROM MIU_States WHERE CurrentString = '{miuStringStandard.Replace("'", "''")}';");

                if (dt != null && dt.Rows.Count > 0)
                {
                    // Stato esistente, aggiorna l'UsageCount
                    stateId = Convert.ToInt64(dt.Rows[0]["StateID"]);
                    _dataManager.ExecuteNonQuery($"UPDATE MIU_States SET UsageCount = UsageCount + 1 WHERE StateID = {stateId};");
                    _logger.Log(LogLevel.DEBUG, $"MIUState '{miuStringStandard}' aggiornato. ID: {stateId}");
                }
                else
                {
                    // Nuovo stato, inserisci
                    long hash = miuStringStandard.GetHashCode(); // GetHashCode su stringa è un int, non un long. Se la colonna Hash è TEXT, va bene.
                                                                  // Se Hash fosse INTEGER/BIGINT, sarebbe meglio usare un algoritmo hash più robusto o un convert.
                    int stringLength = miuStringStandard.Length;
                    DateTime discoveryTime = DateTime.Now;
                    long discoveryTimeInt = new DateTimeOffset(discoveryTime).ToUnixTimeSeconds();
                    string discoveryTimeText = discoveryTime.ToString("yyyy-MM-dd HH:mm:ss");

                    _dataManager.ExecuteNonQuery($"INSERT INTO MIU_States (CurrentString, StringLength, Hash, DiscoveryTime_Int, DiscoveryTime_Text, UsageCount) VALUES ('{miuStringStandard.Replace("'", "''")}', {stringLength}, '{hash}', {discoveryTimeInt}, '{discoveryTimeText}', 1);");

                    // Recupera l'ID appena inserito
                    stateId = _dataManager.ExecuteScalar<long>("SELECT last_insert_rowid();");
                    _logger.Log(LogLevel.DEBUG, $"MIUState '{miuStringStandard}' inserito. ID: {stateId}");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nell'Upsert di MIU State '{miuStringStandard}': {ex.Message}");
                throw; // Rilancia l'eccezione per gestione a monte
            }
            return stateId;
        }

        #endregion

        #region Statistiche Regole (Learning Strategy)

        /// <summary>
        /// Carica le statistiche di tutte le regole dal database.
        /// Se una regola non ha statistiche, ne crea una nuova con valori predefiniti.
        /// </summary>
        /// <returns>Un dizionario di RuleStatistics, con RuleID come chiave.</returns>
        public Dictionary<long, RuleStatistics> LoadRuleStatistics()
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta caricamento RuleStatistics.");
            Dictionary<long, RuleStatistics> stats = new Dictionary<long, RuleStatistics>();
            try
            {
                // UTILIZZIAMO IL NOME DI TABELLA CORRETTO: Learning_RuleStatistics
                // Colonne: RuleID (PK), ApplicationCount, SuccessfulCount, EffectivenessScore, LastUpdated (TEXT)
                DataTable dt = _dataManager.ExecuteSelect("SELECT RuleID, ApplicationCount, SuccessfulCount, EffectivenessScore, LastUpdated FROM Learning_RuleStatistics;");

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        long ruleId = Convert.ToInt64(row["RuleID"]);
                        int appCount = Convert.ToInt32(row["ApplicationCount"]);
                        int succCount = Convert.ToInt32(row["SuccessfulCount"]);
                        // Utilizza CultureInfo.InvariantCulture per la conversione robusta di double
                        double effectiveness = Convert.ToDouble(row["EffectivenessScore"], CultureInfo.InvariantCulture);
                        string lastUpdatedText = row["LastUpdated"].ToString();
                        
                        DateTime lastAppDateTime;
                        // Prova a parsare la stringa nel formato atteso.
                        // DateTimeStyles.None è sufficiente per il formato esatto, ma potresti considerare altre opzioni
                        // come DateTimeStyles.AssumeLocal o DateTimeStyles.AdjustToUniversal se la gestione del fuso orario fosse critica.
                        if (!DateTime.TryParseExact(lastUpdatedText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastAppDateTime))
                        {
                            // Fallback se il parsing fallisce (es. formato non standard o dati corrotti)
                            lastAppDateTime = DateTime.MinValue; 
                            _logger.Log(LogLevel.WARNING, $"[Repository WARNING] Impossibile parsare LastUpdated '{lastUpdatedText}' per RuleID {ruleId}. Impostato a DateTime.MinValue.");
                        }

                        stats[ruleId] = new RuleStatistics
                        {
                            RuleID = ruleId,
                            ApplicationCount = appCount,
                            SuccessfulCount = succCount,
                            EffectivenessScore = effectiveness,
                            LastApplicationTimestamp = lastAppDateTime
                        };
                    }
                    _logger.Log(LogLevel.DEBUG, "RuleStatistics caricate dal database.");
                }
                else
                {
                    _logger.Log(LogLevel.WARNING, "Nessuna RuleStatistics trovata nel database. Verranno create nuove entry all'occorrenza.");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nel caricamento delle RuleStatistics: {ex.Message}");
            }
            return stats;
        }

        /// <summary>
        /// Salva le statistiche di tutte le regole nel database.
        /// </summary>
        /// <param name="ruleStats">Il dizionario di RuleStatistics da salvare.</param>
        public void SaveRuleStatistics(Dictionary<long, RuleStatistics> ruleStats)
        {
            _logger.Log(LogLevel.DEBUG, "[Repository DEBUG] Richiesta salvataggio RuleStatistics.");
            if (ruleStats == null)
            {
                _logger.Log(LogLevel.WARNING, "Tentativo di salvare RuleStatistics nullo.");
                return;
            }

            try
            {
                // Inizia una transazione per assicurare l'atomicità
                _dataManager.ExecuteNonQuery("BEGIN TRANSACTION;");

                foreach (var entry in ruleStats)
                {
                    RuleStatistics rs = entry.Value;
                    // Converte DateTime in stringa nel formato 'yyyy-MM-dd HH:mm:ss' per la colonna TEXT LastUpdated
                    string lastApplicationTimestampText = rs.LastApplicationTimestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    // UTILIZZIAMO IL NOME DI TABELLA CORRETTO: Learning_RuleStatistics
                    DataTable dt = _dataManager.ExecuteSelect($"SELECT RuleID FROM Learning_RuleStatistics WHERE RuleID = {rs.RuleID};");

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        // Aggiorna
                        _dataManager.ExecuteNonQuery($@"
                            UPDATE Learning_RuleStatistics SET
                                ApplicationCount = {rs.ApplicationCount},
                                SuccessfulCount = {rs.SuccessfulCount},
                                EffectivenessScore = {rs.EffectivenessScore.ToString(CultureInfo.InvariantCulture)}, -- Usa InvariantCulture per i double
                                LastUpdated = '{lastApplicationTimestampText}'
                            WHERE RuleID = {rs.RuleID};"
                        );
                    }
                    else
                    {
                        // Inserisci
                        _dataManager.ExecuteNonQuery($@"
                            INSERT INTO Learning_RuleStatistics (RuleID, ApplicationCount, SuccessfulCount, EffectivenessScore, LastUpdated)
                            VALUES ({rs.RuleID}, {rs.ApplicationCount}, {rs.SuccessfulCount}, {rs.EffectivenessScore.ToString(CultureInfo.InvariantCulture)}, '{lastApplicationTimestampText}');"
                        );
                    }
                }
                _dataManager.ExecuteNonQuery("COMMIT;");
                _logger.Log(LogLevel.DEBUG, $"RuleStatistics salvate nel database. Numero di entry: {ruleStats.Count}");
            }
            catch (Exception ex)
            {
                _dataManager.ExecuteNonQuery("ROLLBACK;"); // Esegue il rollback in caso di errore
                _logger.Log(LogLevel.ERROR, $"Errore nel salvataggio delle RuleStatistics: {ex.Message}");
            }
        }

        #endregion

        #region Ricerche (Search)

        /// <summary>
        /// Inserisce una nuova entry di ricerca nel database.
        /// </summary>
        /// <param name="initialString">La stringa iniziale della ricerca (standard).</param>
        /// <param name="targetString">La stringa target della ricerca (standard).</param>
        /// <param name="algorithm">L'algoritmo usato (BFS/DFS).</param>
        /// <returns>L'ID della ricerca appena inserita.</returns>
        public long InsertSearch(string initialString, string targetString, string algorithm)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta inserimento Search: {initialString} -> {targetString}");
            long searchId = -1;
            try
            {
                DateTime startTime = DateTime.Now;
                long startTimeInt = new DateTimeOffset(startTime).ToUnixTimeSeconds();
                string startTimeText = startTime.ToString("yyyy-MM-dd HH:mm:ss");

                _dataManager.ExecuteNonQuery($@"
                    INSERT INTO Searches (InitialString, TargetString, Algorithm, StartTime_Int, StartTime_Text, Success, ElapsedMilliseconds, StepsTaken, NodesExplored, MaxDepthReached)
                    VALUES ('{initialString.Replace("'", "''")}', '{targetString.Replace("'", "''")}', '{algorithm}', {startTimeInt}, '{startTimeText}', 0, 0, 0, 0, 0);"
                );
                searchId = _dataManager.ExecuteScalar<long>("SELECT last_insert_rowid();");
                _logger.Log(LogLevel.DEBUG, $"Search inserita: Initial='{initialString}', Target='{targetString}', Algo='{algorithm}'. ID: {searchId}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nell'inserimento della Search: {ex.Message}");
            }
            return searchId;
        }

        /// <summary>
        /// Aggiorna i dettagli di una ricerca esistente.
        /// </summary>
        public void UpdateSearch(long searchId, bool success, long elapsedMilliseconds, int stepsTaken, int nodesExplored, int maxDepthReached)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta aggiornamento Search ID: {searchId}");
            try
            {
                _dataManager.ExecuteNonQuery($@"
                    UPDATE Searches SET
                        Success = {(success ? 1 : 0)},
                        ElapsedMilliseconds = {elapsedMilliseconds},
                        StepsTaken = {stepsTaken},
                        NodesExplored = {nodesExplored},
                        MaxDepthReached = {maxDepthReached}
                    WHERE SearchID = {searchId};"
                );
                _logger.Log(LogLevel.DEBUG, $"Search ID {searchId} aggiornata. Successo: {success}, Tempo: {elapsedMilliseconds}ms.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nell'aggiornamento della Search ID {searchId}: {ex.Message}");
            }
        }

        #endregion

        #region Applicazioni Regole (Rule Applications)

        /// <summary>
        /// Inserisce un'applicazione di regola nel database.
        /// </summary>
        public void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long appliedRuleId, int depth)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta inserimento Rule Application: SearchID={searchId}, Parent={parentStateId}, New={newStateId}, Rule={appliedRuleId}.");
            try
            {
                DateTime applicationTime = DateTime.Now;
                long applicationTimeInt = new DateTimeOffset(applicationTime).ToUnixTimeSeconds();
                string applicationTimeText = applicationTime.ToString("yyyy-MM-dd HH:mm:ss");

                _dataManager.ExecuteNonQuery($@"
                    INSERT INTO RuleApplications (SearchID, ParentStateID, NewStateID, AppliedRuleID, ApplicationTime_Int, ApplicationTime_Text, Depth)
                    VALUES ({searchId}, {parentStateId}, {newStateId}, {appliedRuleId}, {applicationTimeInt}, '{applicationTimeText}', {depth});"
                );
                _logger.Log(LogLevel.DEBUG, $"Applicazione Regola inserita: SearchID={searchId}, Parent={parentStateId}, New={newStateId}, Rule={appliedRuleId}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nell'inserimento dell'applicazione regola per SearchID {searchId}: {ex.Message}");
            }
        }

        #endregion

        #region Passi del Percorso della Soluzione (Solution Path Steps)

        /// <summary>
        /// Inserisce un passo nel percorso di una soluzione trovata.
        /// </summary>
        public void InsertSolutionPathStep(long searchId, int stepNumber, long currentStateId, long? parentStateId, long? appliedRuleId, bool isTarget, bool isSuccess, int depth)
        {
            _logger.Log(LogLevel.DEBUG, $"[Repository DEBUG] Richiesta inserimento Solution Path Step: SearchID={searchId}, Step={stepNumber}, StateID={currentStateId}.");
            try
            {
                string parentStateIdValue = parentStateId.HasValue ? parentStateId.Value.ToString() : "NULL";
                string appliedRuleIdValue = appliedRuleId.HasValue ? appliedRuleId.Value.ToString() : "NULL";

                _dataManager.ExecuteNonQuery($@"
                    INSERT INTO SolutionPathSteps (SearchID, StepNumber, CurrentStateID, ParentStateID, AppliedRuleID, IsTarget, IsSuccess, Depth)
                    VALUES ({searchId}, {stepNumber}, {currentStateId}, {parentStateIdValue}, {appliedRuleIdValue}, {(isTarget ? 1 : 0)}, {(isSuccess ? 1 : 0)}, {depth});"
                );
                _logger.Log(LogLevel.DEBUG, $"Passo del percorso della soluzione inserito: SearchID={searchId}, Step={stepNumber}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore nell'inserimento del passo del percorso della soluzione per SearchID {searchId}, Step {stepNumber}: {ex.Message}");
            }
        }

        #endregion
    }
}
