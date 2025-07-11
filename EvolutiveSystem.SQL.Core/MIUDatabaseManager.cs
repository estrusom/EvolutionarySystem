// File: C:\Progetti\EvolutiveSystem\SQL.Core\MIUDatabaseManager.cs
// AGGIORNAMENTO 21.6.25: Ricostruzione completa del file MIUDatabaseManager.cs
// per implementare correttamente tutti i membri dell'interfaccia IMIUDataManager
// e risolvere gli errori di compilazione causati da frammentazioni precedenti.
// AGGIORNATO 20.06.2025: Implementazione del metodo GetTransitionProbabilities per l'aggregazione delle statistiche.
// AGGIORNATO 20.06.2025: Reintegrate le implementazioni complete dei metodi di caricamento/salvataggio delle statistiche di apprendimento,
// in linea con la responsabilità di MIUDatabaseManager come unico punto di accesso diretto al database.
// AGGIORNATO 21.06.2025: Aggiunta l'implementazione del metodo SetJournalMode per incapsulare il comando PRAGMA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using MIU.Core; // Assicurati che questo namespace sia corretto per le classi MIU.Core come RegolaMIU, RuleStatistics, TransitionStatistics
using MasterLog;
using System.Globalization;
using System.Text; // Necessario per StringBuilder in LoadRegoleMIU o altri metodi di utilità
using EvolutiveSystem.Common; // Aggiunto per le classi modello spostate
using System.Threading.Tasks; // NECESSARIO PER I METODI ASINCRONI

namespace EvolutiveSystem.SQL.Core
{
    /// <summary>
    /// Gestore del database MIU. Questa classe fornisce un'interfaccia di alto livello
    /// per la persistenza dei dati relativi al sistema MIU (ricerche, stati, regole, statistiche, configurazione).
    /// Ottiene la ConnectionString da SQLiteSchemaLoader e gestisce le proprie istanze
    /// di SQLiteConnection e SQLiteTransaction per le operazioni di persistenza,
    /// garantendo il controllo delle transazioni e l'uso di query parametrizzate.
    /// Implementa l'interfaccia IMIUDataManager.
    /// </summary>
    public class MIUDatabaseManager : IMIUDataManager  // <- errore cs09535
    {
        private readonly SQLiteSchemaLoader _schemaLoader;
        private readonly Logger _logger;

        /// <summary>
        /// Costruttore di MIUDatabaseManager.
        /// Riceve un'istanza di SQLiteSchemaLoader già configurata e pronta all'uso,
        /// da cui ricaverà la ConnectionString.
        /// </summary>
        /// <param name="schemaLoader">L'istanza di SQLiteSchemaLoader che gestisce la connection string.</param>
        /// <param name="logger">L'istanza del logger per la registrazione degli eventi.</param>
        public MIUDatabaseManager(SQLiteSchemaLoader schemaLoader, Logger logger)
        {
            _schemaLoader = schemaLoader ?? throw new ArgumentNullException(nameof(schemaLoader), "SQLiteSchemaLoader non può essere nullo.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger non può essere nullo.");
            _logger.Log(LogLevel.DEBUG, "MIUDatabaseManager istanziato e ottiene ConnectionString da SQLiteSchemaLoader.");
        }

        // --- Metodi di Persistenza (Implementazione dell'interfaccia IMIUDataManager) ---

        /// <summary>
        /// Inserisce una nuova ricerca MIU nel database con le caratteristiche delle stringhe.
        /// </summary>
        /// <returns>L'ID della ricerca appena inserita, o -1 in caso di errore.</returns>
        public long InsertSearch(
            string initialString,
            string targetString,
            string searchAlgorithm,
            int initialStringLength,
            int targetStringLength,
            int initialIcount,
            int initialUcount,
            int targetIcount,
            int targetUcount
        )
        {
            long lastId = -1;
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string sql = @"
                        INSERT INTO MIU_Searches (
                            InitialString, TargetString, SearchAlgorithm,
                            InitialStringLength, TargetStringLength,
                            InitialIcount, InitialUcount, TargetIcount, TargetUcount,
                            StartTime, Outcome, StepsTaken, NodesExplored, MaxDepth, ElapsedMilliseconds
                        ) VALUES (
                            @initialString, @targetString, @searchAlgorithm,
                            @initialStringLength, @targetStringLength,
                            @initialIcount, @initialUcount, @targetIcount, @targetUcount,
                            @startTime, @outcome, @stepsTaken, @nodesExplored, @maxDepth, @elapsedMilliseconds
                        )";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@initialString", initialString);
                        command.Parameters.AddWithValue("@targetString", targetString);
                        command.Parameters.AddWithValue("@searchAlgorithm", searchAlgorithm);
                        command.Parameters.AddWithValue("@initialStringLength", initialStringLength);
                        command.Parameters.AddWithValue("@targetStringLength", targetStringLength);
                        command.Parameters.AddWithValue("@initialIcount", initialIcount);
                        command.Parameters.AddWithValue("@initialUcount", initialUcount);
                        command.Parameters.AddWithValue("@targetIcount", targetIcount);
                        command.Parameters.AddWithValue("@targetUcount", targetUcount);
                        command.Parameters.AddWithValue("@startTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
                        command.Parameters.AddWithValue("@outcome", "Pending");
                        command.Parameters.AddWithValue("@stepsTaken", 0);
                        command.Parameters.AddWithValue("@nodesExplored", 0);
                        command.Parameters.AddWithValue("@maxDepth", 0);
                        command.Parameters.AddWithValue("@elapsedMilliseconds", 0.0);

                        command.ExecuteNonQuery();

                        using (var idCommand = new SQLiteCommand("SELECT last_insert_rowid()", connection))
                        {
                            lastId = (long)idCommand.ExecuteScalar();
                        }
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"Search inserita: Initial='{initialString}', Target='{targetString}', Algo='{searchAlgorithm}'. ID: {lastId}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore in InsertSearch: {ex.Message}");
            }
            return lastId;
        }

        /// <summary>
        /// Aggiorna una ricerca MIU esistente con i risultati finali.
        /// </summary>
        public void UpdateSearch(long searchId, bool success, double flightTimeMs, int stepsTaken, int nodesExplored, int maxDepthReached)
        {
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string sql = @"
                        UPDATE MIU_Searches
                        SET Outcome = @outcome,
                            EndTime = @endTime,
                            StepsTaken = @stepsTaken,
                            NodesExplored = @nodesExplored,
                            MaxDepth = @maxDepth,
                            ElapsedMilliseconds = @elapsedMilliseconds
                        WHERE SearchID = @searchId";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@outcome", success ? "Success" : "Failed");
                        command.Parameters.AddWithValue("@endTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
                        command.Parameters.AddWithValue("@stepsTaken", stepsTaken);
                        command.Parameters.AddWithValue("@nodesExplored", nodesExplored);
                        command.Parameters.AddWithValue("@maxDepth", maxDepthReached);
                        command.Parameters.AddWithValue("@elapsedMilliseconds", flightTimeMs);
                        command.Parameters.AddWithValue("@searchId", searchId);
                        command.ExecuteNonQuery();
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"Search '{searchId}' aggiornata: Success={success}, ElapsedMilliseconds={flightTimeMs}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore in UpdateSearch: {ex.Message}");
            }
        }

        /// <summary>
        /// Inserisce o aggiorna uno stato MIU (una stringa MIU).
        /// Se lo stato esiste già, ne incrementa l'uso. Altrimenti, lo inserisce.
        /// </summary>
        /// <param name="miuString">La stringa MIU standard (non compressa).</param>
        /// <returns>L'ID dello stato MIU nel database.</returns>
        public Tuple<long, bool> UpsertMIUState(string miuString)
        {
            long stateId = -1;
            bool isNewString = false; // Flag per indicare se la stringa è nuova
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string selectSql = "SELECT StateID FROM MIU_States WHERE CurrentString = @miuString";
                    using (var selectCommand = new SQLiteCommand(selectSql, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@miuString", miuString);
                        object result = selectCommand.ExecuteScalar();
                        if (result != null)
                        {
                            stateId = (long)result;
                            string updateSql = "UPDATE MIU_States SET UsageCount = UsageCount + 1, DiscoveryTime_Int = @timeInt, DiscoveryTime_Text = @timeText WHERE StateID = @stateId";
                            using (var updateCommand = new SQLiteCommand(updateSql, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@timeInt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                                updateCommand.Parameters.AddWithValue("@timeText", DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss"));
                                updateCommand.Parameters.AddWithValue("@stateId", stateId);
                                updateCommand.ExecuteNonQuery();
                            }
                            _logger.Log(LogLevel.DEBUG, $"MIUState '{miuString}' aggiornato. ID: {stateId}");
                        }
                        else
                        {
                            int stringLength = miuString.Length;
                            long discoveryTimeInt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            string discoveryTimeText = DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss");

                            // Utilizza MIUStringConverter per la compressione
                            string insertSql = "INSERT INTO MIU_States (CurrentString, StringLength, DeflateString, Hash, DiscoveryTime_Int, DiscoveryTime_Text, UsageCount) VALUES (@currentString, @stringLength, @deflateString, @hash, @timeInt, @timeText, 1); SELECT last_insert_rowid();";
                            using (var insertCommand = new SQLiteCommand(insertSql, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@currentString", miuString);
                                insertCommand.Parameters.AddWithValue("@stringLength", stringLength);
                                insertCommand.Parameters.AddWithValue("@deflateString", MIUStringConverter.DeflateMIUString(miuString)); // Modificato: Ora chiama DeflateMIUString
                                insertCommand.Parameters.AddWithValue("@hash", miuString.GetHashCode().ToString()); // Simple hash for now
                                insertCommand.Parameters.AddWithValue("@timeInt", discoveryTimeInt);
                                insertCommand.Parameters.AddWithValue("@timeText", discoveryTimeText);
                                stateId = (long)insertCommand.ExecuteScalar();
                                _logger.Log(LogLevel.DEBUG, $"MIUState inserito: '{miuString}'. ID: {stateId}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore in UpsertMIUState: {ex.Message}");
            }
            return Tuple.Create(stateId, isNewString);
        }

        /// <summary>
        /// Registra un'applicazione di una regola MIU come parte di una ricerca.
        /// </summary>
        public void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long appliedRuleID, int currentDepth)
        {
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string sql = $"INSERT INTO MIU_RuleApplications (SearchID, ParentStateID, NewStateID, AppliedRuleID, CurrentDepth, Timestamp) VALUES (@searchId, @parentStateId, @newStateId, @appliedRuleID, @currentDepth, @timestamp)";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@searchId", searchId);
                        command.Parameters.AddWithValue("@parentStateId", parentStateId);
                        command.Parameters.AddWithValue("@newStateId", newStateId);
                        command.Parameters.AddWithValue("@appliedRuleID", appliedRuleID);
                        command.Parameters.AddWithValue("@currentDepth", currentDepth);
                        command.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"Applicazione Regola inserita: SearchID={searchId}, Parent={parentStateId}, New={newStateId}, Rule={appliedRuleID}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore in InsertRuleApplication: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra un passo del percorso della soluzione di una ricerca MIU.
        /// </summary>
        public void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, long? appliedRuleID, bool isTarget, bool isSuccess, int depth)
        {
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string sql = $"INSERT INTO MIU_Paths (SearchID, StepNumber, StateID, ParentStateID, AppliedRuleID, IsTarget, IsSuccess, Depth) VALUES (@searchId, @stepNumber, @stateId, @parentStateID, @appliedRuleID, @isTarget, @isSuccess, @depth)";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@searchId", searchId);
                        command.Parameters.AddWithValue("@stepNumber", stepNumber);
                        command.Parameters.AddWithValue("@stateId", stateId);
                        command.Parameters.AddWithValue("@parentStateID", parentStateId.HasValue ? (object)parentStateId.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@appliedRuleID", appliedRuleID.HasValue ? (object)appliedRuleID.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@isTarget", isTarget ? 1 : 0);
                        command.Parameters.AddWithValue("@isSuccess", isSuccess ? 1 : 0);
                        command.Parameters.AddWithValue("@depth", depth);
                        command.ExecuteNonQuery();
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"Passo Path soluzione inserito: SearchID={searchId}, Step={stepNumber}, StateID={stateId}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore in InsertSolutionPathStep: {ex.Message}");
            }
        }

        /// <summary>
        /// Carica tutte le regole MIU dal database.
        /// </summary>
        /// <returns>Una lista di oggetti RegolaMIU.</returns>
        public List<RegolaMIU> LoadRegoleMIU()
        {
            List<RegolaMIU> regole = new List<RegolaMIU>();
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT ID, Nome, Pattern, Sostituzione, Descrizione FROM RegoleMIU";
                    using (var command = new SQLiteCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                long id = reader.GetInt64(reader.GetOrdinal("ID"));
                                string nome = reader.GetString(reader.GetOrdinal("Nome"));
                                string pattern = reader.GetString(reader.GetOrdinal("Pattern"));
                                string sostituzione = reader.GetString(reader.GetOrdinal("Sostituzione"));
                                string descrizione = reader.GetString(reader.GetOrdinal("Descrizione"));

                                regole.Add(new RegolaMIU(id, nome, descrizione, pattern, sostituzione));
                            }
                            catch (InvalidCastException ex)
                            {
                                _logger.Log(LogLevel.ERROR, $"Errore di cast durante la lettura di una colonna in LoadRegoleMIU.");
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string colName = reader.GetName(i);
                                    string colTypeName = reader.GetDataTypeName(i);
                                    object colValue = reader.GetValue(i);
                                    _logger.Log(LogLevel.ERROR, $"  Colonna: '{colName}', Tipo DB: '{colTypeName}', Valore: '{colValue ?? "NULL"}' (Tipo runtime: {colValue?.GetType().Name ?? "NULL"})");
                                }
                                _logger.Log(LogLevel.ERROR, $"  Dettagli eccezione: {ex.Message}");
                                throw;
                            }
                        }
                    }
                }
                _logger.Log(LogLevel.DEBUG, "RegoleMIU caricate dal database.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore generale caricamento RegoleMIU: {ex.Message}. Restituisco lista vuota.");
                return new List<RegolaMIU>();
            }
            return regole;
        }

        /// <summary>
        /// Inserisce o aggiorna un elenco di regole MIU nel database.
        /// Utilizza una transazione per garantire l'atomicità dell'operazione.
        /// </summary>
        public void UpsertRegoleMIU(List<RegolaMIU> regole)
        {
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (var regola in regole)
                        {
                            string sql = "INSERT OR REPLACE INTO RegoleMIU (ID, Nome, Pattern, Sostituzione, Descrizione) VALUES (@id, @nome, @pattern, @sostituzione, @descrizione)";
                            using (var command = new SQLiteCommand(sql, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@id", regola.ID);
                                command.Parameters.AddWithValue("@nome", regola.Nome);
                                command.Parameters.AddWithValue("@pattern", regola.Pattern);
                                command.Parameters.AddWithValue("@sostituzione", regola.Sostituzione);
                                command.Parameters.AddWithValue("@descrizione", regola.Descrizione);
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"Upserted {regole.Count} RegoleMIU al database.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore salvataggio RegoleMIU: {ex.Message}");
            }
        }

        /// <summary>
        /// Carica i parametri di configurazione dal database.
        /// </summary>
        /// <returns>Un dizionario con chiave=NomeParametro e valore=ValoreParametro.</returns>
        public Dictionary<string, string> LoadMIUParameterConfigurator()
        {
            var config = new Dictionary<string, string>();
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT NomeParametro, ValoreParametro FROM MIUParameterConfigurator";
                    using (var command = new SQLiteCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string nomeParametro = reader.GetString(reader.GetOrdinal("NomeParametro"));
                            string valoreParametro = reader.GetString(reader.GetOrdinal("ValoreParametro"));
                            config[nomeParametro] = valoreParametro;
                        }
                    }
                }
                _logger.Log(LogLevel.DEBUG, "Parametri da MIUParameterConfigurator caricati.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore caricamento MIUParameterConfigurator: {ex.Message}. Restituisco configurazione vuota.");
                return new Dictionary<string, string>();
            }
            return config;
        }

        /// <summary>
        /// Salva i parametri di configurazione.
        /// </summary>
        public void SaveMIUParameterConfigurator(Dictionary<string, string> config)
        {
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (var entry in config)
                        {
                            string sql = "INSERT OR REPLACE INTO MIUParameterConfigurator (NomeParametro, ValoreParametro) VALUES (@nomeParametro, @valoreParametro)";
                            using (var command = new SQLiteCommand(sql, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@nomeParametro", entry.Key);
                                command.Parameters.AddWithValue("@valoreParametro", entry.Value);
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"Salvati {config.Count} parametri in MIUParameterConfigurator.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore salvataggio MIUParameterConfigurator: {ex.Message}");
            }
        }

        /// <summary>
        /// Carica le statistiche di apprendimento delle regole dal database.
        /// Implementazione necessaria per IMIUDataManager.
        /// </summary>
        /// <returns>Un dizionario di RuleStatistics, con chiave=RuleID.</returns>
        public Dictionary<long, RuleStatistics> LoadRuleStatistics()
        {
            var ruleStats = new Dictionary<long, RuleStatistics>();
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT RuleID, ApplicationCount, SuccessfulCount, EffectivenessScore, LastUpdated FROM Learning_RuleStatistics";
                    using (var command = new SQLiteCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long ruleId = reader.GetInt64(reader.GetOrdinal("RuleID"));
                            int appCount = reader.GetInt32(reader.GetOrdinal("ApplicationCount"));
                            int succCount = reader.GetInt32(reader.GetOrdinal("SuccessfulCount"));
                            double effScore = reader.GetDouble(reader.GetOrdinal("EffectivenessScore"));
                            if (!reader.IsDBNull(reader.GetOrdinal("LastUpdated")))
                            {
                                DateTime lastUpdated = DateTime.MinValue;
                                if (DateTime.TryParse(reader.GetString(reader.GetOrdinal("LastUpdated")), out lastUpdated))
                                {
                                    ruleStats[ruleId] = new RuleStatistics
                                    {
                                        RuleID = ruleId,
                                        ApplicationCount = appCount,
                                        SuccessfulCount = succCount,
                                        EffectivenessScore = effScore,
                                        LastApplicationTimestamp = lastUpdated
                                    };
                                }
                            }
                        }
                    }
                }
                _logger.Log(LogLevel.DEBUG, "RuleStatistics caricate dal database.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore caricamento RuleStatistics: {ex.Message}. Restituisco dizionario vuoto.");
                return new Dictionary<long, RuleStatistics>();
            }
            return ruleStats;
        }

        /// <summary>
        /// Salva (upsert) le statistiche delle regole di apprendimento nel database.
        /// Implementazione necessaria per IMIUDataManager.
        /// </summary>
        public void SaveRuleStatistics(Dictionary<long, RuleStatistics> ruleStats)
        {
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (var entry in ruleStats)
                        {
                            var stats = entry.Value;
                            string sql = "INSERT OR REPLACE INTO Learning_RuleStatistics (RuleID, ApplicationCount, SuccessfulCount, EffectivenessScore, LastUpdated) VALUES (@ruleId, @appCount, @succCount, @effScore, @lastUpdated)";
                            using (var command = new SQLiteCommand(sql, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@ruleId", stats.RuleID);
                                command.Parameters.AddWithValue("@appCount", stats.ApplicationCount);
                                command.Parameters.AddWithValue("@succCount", stats.SuccessfulCount);
                                // Converte il double in stringa usando CultureInfo.InvariantCulture per evitare problemi di formattazione decimale
                                command.Parameters.AddWithValue("@effScore", stats.EffectivenessScore.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                command.Parameters.AddWithValue("@lastUpdated", stats.LastApplicationTimestamp.ToString("yyyy/MM/dd HH:mm:ss"));
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"Salvate {ruleStats.Count} RuleStatistics nel database.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore salvataggio RuleStatistics: {ex.Message}");
            }
        }
        /// <summary>
        /// Carica le statistiche di transizione di apprendimento dal database.
        /// Implementazione necessaria per IMIUDataManager.
        /// </summary>
        /// <returns>Un dizionario di TransitionStatistics, con chiave (ParentStringCompressed, AppliedRuleID).</returns>
        public Dictionary<Tuple<string, long>, TransitionStatistics> LoadTransitionStatistics()
        {
            var transitionStats = new Dictionary<Tuple<string, long>, TransitionStatistics>();
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT ParentStringCompressed, AppliedRuleID, ApplicationCount, SuccessfulCount, LastUpdated FROM Learning_TransitionStatistics";
                    using (var command = new SQLiteCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string parentString = reader.GetString(reader.GetOrdinal("ParentStringCompressed"));
                            long appliedRuleId = reader.GetInt64(reader.GetOrdinal("AppliedRuleID"));
                            int appCount = reader.GetInt32(reader.GetOrdinal("ApplicationCount"));
                            int succCount = reader.GetInt32(reader.GetOrdinal("SuccessfulCount"));

                            DateTime lastUpdated = DateTime.MinValue;
                            if (DateTime.TryParse(reader.GetString(reader.GetOrdinal("LastUpdated")), out lastUpdated))
                            {
                                var key = Tuple.Create(parentString, appliedRuleId);
                                transitionStats[key] = new TransitionStatistics
                                {
                                    ParentStringCompressed = parentString,
                                    AppliedRuleID = appliedRuleId,
                                    ApplicationCount = appCount,
                                    SuccessfulCount = succCount,
                                    LastUpdated = lastUpdated
                                };
                            }
                        }
                    }
                }
                _logger.Log(LogLevel.DEBUG, "TransitionStatistics caricate dal database.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore caricamento TransitionStatistics: {ex.Message}. Restituisco dizionario vuoto.");
                return new Dictionary<Tuple<string, long>, TransitionStatistics>();
            }
            return transitionStats;
        }

        /// <summary>
        /// Salva (upsert) le statistiche di transizione di apprendimento nel database.
        /// Implementazione necessaria per IMIUDataManager.
        /// </summary>
        public void SaveTransitionStatistics(Dictionary<Tuple<string, long>, TransitionStatistics> transitionStats)
        {
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (var entry in transitionStats)
                        {
                            var stats = entry.Value;
                            string sql = "INSERT OR REPLACE INTO Learning_TransitionStatistics (ParentStringCompressed, AppliedRuleID, ApplicationCount, SuccessfulCount, LastUpdated) VALUES (@parentStringCompressed, @appliedRuleId, @appCount, @succCount, @lastUpdated)";
                            using (var command = new SQLiteCommand(sql, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@parentStringCompressed", stats.ParentStringCompressed);
                                command.Parameters.AddWithValue("@appliedRuleId", stats.AppliedRuleID);
                                command.Parameters.AddWithValue("@appCount", stats.ApplicationCount);
                                command.Parameters.AddWithValue("@succCount", stats.SuccessfulCount);
                                command.Parameters.AddWithValue("@lastUpdated", stats.LastUpdated.ToString("yyyy/MM/dd HH:mm:ss")); // Non c'è più @effScore qui
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"Salvate {transitionStats.Count} TransitionStatistics nel database.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore salvataggio TransitionStatistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Carica le statistiche di transizione aggregate (conteggi totali e di successo)
        /// dal database, calcolando la probabilità di successo per ogni transizione.
        /// Questo metodo esegue una query SQL complessa che aggrega i dati delle applicazioni di regole
        /// e dei risultati delle ricerche per fornire una "topografia pesata e dinamica"
        /// basata su dati storici reali.
        /// </summary>
        /// <returns>Un dizionario di TransitionStatistics, con chiave (ParentStringCompressed, AppliedRuleID).</returns>
        public Dictionary<Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics> GetTransitionProbabilities()
        {
            var transitionProbabilities = new Dictionary<Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics>();
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string sql = @"
                        SELECT
                            MS.DeflateString AS ParentStringCompressed,
                            AR.AppliedRuleID,
                            COUNT(AR.ApplicationID) AS TotalApplications,
                            SUM(CASE WHEN S.Outcome = 'Success' THEN 1 ELSE 0 END) AS SuccessfulApplications
                        FROM
                            MIU_RuleApplications AS AR
                        JOIN
                            MIU_Searches AS S ON AR.SearchID = S.SearchID
                        JOIN
                            MIU_States AS MS ON AR.ParentStateID = MS.StateID
                        GROUP BY
                            MS.DeflateString,
                            AR.AppliedRuleID;";

                    using (var command = new SQLiteCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string parentStringCompressed = reader.GetString(reader.GetOrdinal("ParentStringCompressed"));
                            long appliedRuleId = reader.GetInt64(reader.GetOrdinal("AppliedRuleID"));
                            int totalApplications = reader.GetInt32(reader.GetOrdinal("TotalApplications"));
                            int successfulApplications = reader.GetInt32(reader.GetOrdinal("SuccessfulApplications"));

                            var key = Tuple.Create(parentStringCompressed, appliedRuleId);

                            transitionProbabilities[key] = new EvolutiveSystem.Common.TransitionStatistics
                            {
                                ParentStringCompressed = parentStringCompressed,
                                AppliedRuleID = appliedRuleId,
                                ApplicationCount = totalApplications,
                                SuccessfulCount = successfulApplications,
                                LastUpdated = DateTime.Now // Imposta la data dell'ultima aggregazione
                            };
                        }
                    }
                }
                _logger.Log(LogLevel.INFO, $"[MIUDatabaseManager] Caricate {transitionProbabilities.Count} statistiche di transizione aggregate.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MIUDatabaseManager] Errore durante il caricamento delle probabilità di transizione: {ex.Message}. Restituisco dizionario vuoto.");
                return new Dictionary<Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics>();
            }
            return transitionProbabilities;
        }

        /// <summary>
        /// Imposta la modalità di journaling del database (es. WAL, DELETE, TRUNCATE).
        /// Questo metodo incapsula l'esecuzione del comando PRAGMA, garantendo che
        /// tutte le interazioni SQL dirette avvengano all'interno di MIUDatabaseManager.
        /// </summary>
        /// <param name="mode">La modalità di journaling da impostare (es. "WAL").</param>
        public void SetJournalMode(string mode)
        {
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string sql = $"PRAGMA journal_mode={mode};";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                        _logger.Log(LogLevel.INFO, $"[MIUDatabaseManager] Modalità WAL impostata a '{mode}' per il database.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MIUDatabaseManager ERROR] Errore nell'impostazione della modalità WAL a '{mode}': {ex.Message}");
                throw; // Rilancia l'eccezione per gestione a livello superiore
            }
        }

        /// <summary>
        /// Carica tutti gli stati MIU dal database in modo asincrono.
        /// Implementazione di IMIUDataManager.LoadMIUStatesAsync().
        /// </summary>
        /// <returns>Un oggetto Task che rappresenta l'operazione asincrona, con un risultato di tipo List<MiuStateInfo>.</returns>
        public async Task<List<EvolutiveSystem.Common.MiuStateInfo>> LoadMIUStatesAsync() // <-- CAMBIATO DA LoadMIUStates()
        {
            var states = new List<EvolutiveSystem.Common.MiuStateInfo>();
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    await connection.OpenAsync(); // <-- UTILIZZA await OpenAsync()
                    string sql = "SELECT StateID, CurrentString, StringLength, DeflateString, Hash, DiscoveryTime_Int, DiscoveryTime_Text, UsageCount FROM MIU_States";
                    using (var command = new SQLiteCommand(sql, connection))
                    using (var reader = await command.ExecuteReaderAsync()) // <-- UTILIZZA await ExecuteReaderAsync()
                    {
                        while (await reader.ReadAsync()) // <-- UTILIZZA await ReadAsync()
                        {
                            states.Add(new EvolutiveSystem.Common.MiuStateInfo
                            {
                                StateID = reader.GetInt64(reader.GetOrdinal("StateID")),
                                CurrentString = reader.GetString(reader.GetOrdinal("CurrentString")),
                                StringLength = reader.GetInt32(reader.GetOrdinal("StringLength")),
                                DeflateString = reader.GetString(reader.GetOrdinal("DeflateString")),
                                Hash = reader.GetString(reader.GetOrdinal("Hash")),
                                DiscoveryTime_Int = reader.GetInt64(reader.GetOrdinal("DiscoveryTime_Int")),
                                DiscoveryTime_Text = reader.GetString(reader.GetOrdinal("DiscoveryTime_Text")),
                                UsageCount = reader.GetInt32(reader.GetOrdinal("UsageCount"))
                            });
                        }
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"Caricati {states.Count} stati MIU dal database in modo asincrono."); // <-- AGGIUNTO "in modo asincrono."
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore caricamento MIU_States asincrono: {ex.Message}. Restituisco lista vuota."); // <-- AGGIUNTO "asincrono:"
                return new List<EvolutiveSystem.Common.MiuStateInfo>();
            }
            return states;
        }

        /// <summary>
        /// Verifica se una ricerca con una specifica coppia (initial, target) esiste già nel database
        /// e ha un esito non "Pending".
        /// </summary>
        /// <param name="initialString">La stringa iniziale standard (decompressa).</param>
        /// <param name="targetString">La stringa target standard (decompressa).</param>
        /// <returns>True se la ricerca esiste e non è in stato "Pending", False altrimenti.</returns>
        public bool SearchExists(string initialString, string targetString)
        {
            bool exists = false;
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT COUNT(*) FROM MIU_Searches WHERE InitialString = @initialString AND TargetString = @targetString AND Outcome != 'Pending'";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@initialString", initialString);
                        command.Parameters.AddWithValue("@targetString", targetString);
                        long count = (long)command.ExecuteScalar();
                        exists = count > 0;
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"Verifica SearchExists per '{initialString}'->'{targetString}': {exists}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore in SearchExists per '{initialString}'->'{targetString}': {ex.Message}. Restituisco false.");
            }
            return exists;
        }
        /// <summary>
        /// Carica lo stato del cursore di esplorazione dal database (tabella MIUParameterConfigurator) in modo asincrono.
        /// Implementazione di IMIUDataManager.LoadExplorerCursorAsync().
        /// </summary>
        /// <returns>Un oggetto Task che rappresenta l'operazione asincrona, con un risultato di tipo MIUExplorerCursor.</returns>
        public async Task<EvolutiveSystem.Common.MIUExplorerCursor> LoadExplorerCursorAsync() // <-- MODIFICATO FIRMA A ASYNC TASK
        {
            _logger.Log(LogLevel.DEBUG, "[MIUDatabaseManager] Caricamento cursore esplorazione asincrono..."); // <-- AGGIUNTO "asincrono"
            var cursor = new EvolutiveSystem.Common.MIUExplorerCursor(); // Inizializza con i valori di default

            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    await connection.OpenAsync(); // <-- UTILIZZA await OpenAsync()
                    string sql = "SELECT NomeParametro, ValoreParametro FROM MIUParameterConfigurator WHERE NomeParametro IN ('Explorer_CurrentSourceIndex', 'Explorer_CurrentTargetIndex', 'Explorer_LastExplorationTimestamp')";
                    using (var command = new SQLiteCommand(sql, connection))
                    using (var reader = await command.ExecuteReaderAsync()) // <-- UTILIZZA await ExecuteReaderAsync()
                    {
                        var parameters = new Dictionary<string, string>();
                        while (await reader.ReadAsync()) // <-- UTILIZZA await ReadAsync()
                        {
                            parameters[reader.GetString(0)] = reader.GetString(1);
                        }

                        if (parameters.TryGetValue("Explorer_CurrentSourceIndex", out string sourceIndexStr) && long.TryParse(sourceIndexStr, out long sourceIndex)) // <-- CAMBIATO A long.TryParse
                        {
                            cursor.CurrentSourceIndex = sourceIndex;
                        }
                        if (parameters.TryGetValue("Explorer_CurrentTargetIndex", out string targetIndexStr) && long.TryParse(targetIndexStr, out long targetIndex)) // <-- CAMBIATO A long.TryParse
                        {
                            cursor.CurrentTargetIndex = targetIndex;
                        }
                        if (parameters.TryGetValue("Explorer_LastExplorationTimestamp", out string timestampStr) && DateTime.TryParse(timestampStr, out DateTime timestamp))
                        {
                            cursor.LastExplorationTimestamp = timestamp;
                        }
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"[MIUDatabaseManager] Cursore esplorazione caricato asincrono: Source={cursor.CurrentSourceIndex}, Target={cursor.CurrentTargetIndex}."); // <-- AGGIUNTO "asincrono"
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MIUDatabaseManager ERROR] Errore durante il caricamento del cursore di esplorazione asincrono: {ex.Message}. Utilizzo valori di default."); // <-- AGGIUNTO "asincrono"
                                                                                                                                                                                           // In caso di errore, restituisce il cursore con i valori di default (0, 0, DateTime.UtcNow)
            }
            return cursor;
        }
        /// <summary>
        /// Salva lo stato corrente del cursore di esplorazione nel database (tabella MIUParameterConfigurator) in modo asincrono.
        /// Implementazione di IMIUDataManager.SaveExplorerCursorAsync().
        /// </summary>
        /// <param name="cursor">L'oggetto MIUExplorerCursor da salvare.</param>
        /// <returns>Un oggetto Task che rappresenta l'operazione asincrona.</returns>
        public async Task SaveExplorerCursorAsync(EvolutiveSystem.Common.MIUExplorerCursor cursor) // <-- MODIFICATO FIRMA A ASYNC TASK
        {
            _logger.Log(LogLevel.DEBUG, "[MIUDatabaseManager] Salvataggio cursore esplorazione asincrono..."); // <-- AGGIUNTO "asincrono"
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    await connection.OpenAsync(); // <-- UTILIZZA await OpenAsync()
                    using (var transaction = connection.BeginTransaction())
                    {
                        // Inserisce o aggiorna ogni parametro del cursore
                        string sql = "INSERT OR REPLACE INTO MIUParameterConfigurator (NomeParametro, ValoreParametro) VALUES (@nomeParametro, @valoreParametro)";

                        using (var command = new SQLiteCommand(sql, connection, transaction))
                        {
                            command.Parameters.Add(new SQLiteParameter("@nomeParametro", "Explorer_CurrentSourceIndex"));
                            command.Parameters.Add(new SQLiteParameter("@valoreParametro", cursor.CurrentSourceIndex.ToString()));
                            await command.ExecuteNonQueryAsync(); // <-- UTILIZZA await ExecuteNonQueryAsync()

                            command.Parameters.Clear();
                            command.Parameters.Add(new SQLiteParameter("@nomeParametro", "Explorer_CurrentTargetIndex"));
                            command.Parameters.Add(new SQLiteParameter("@valoreParametro", cursor.CurrentTargetIndex.ToString()));
                            await command.ExecuteNonQueryAsync(); // <-- UTILIZZA await ExecuteNonQueryAsync()

                            command.Parameters.Clear();
                            command.Parameters.Add(new SQLiteParameter("@nomeParametro", "Explorer_LastExplorationTimestamp"));
                            command.Parameters.Add(new SQLiteParameter("@valoreParametro", cursor.LastExplorationTimestamp.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
                            await command.ExecuteNonQueryAsync(); // <-- UTILIZZA await ExecuteNonQueryAsync()
                        }
                        transaction.Commit();
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"[MIUDatabaseManager] Cursore esplorazione salvato asincrono: Source={cursor.CurrentSourceIndex}, Target={cursor.CurrentTargetIndex}."); // <-- AGGIUNTO "asincrono"
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MIUDatabaseManager ERROR] Errore durante il salvataggio del cursore di esplorazione asincrono: {ex.Message}"); // <-- AGGIUNTO "asincrono"
                                                                                                                                                              // In caso di errore, la transazione verrà automaticamente annullata se non commessa.
            }
        }
        /// <summary>
        /// Carica le applicazioni di regole (archi della topologia) con opzioni di filtro.
        /// Questo metodo è progettato per supportare il caricamento efficiente dei dati
        /// grezzi necessari per costruire la topologia MIU.
        /// </summary>
        /// <param name="initialString">Filtra per la stringa iniziale di una ricerca specifica. Se nullo, non filtra.</param>
        /// <param name="startDate">Filtra le applicazioni avvenute a partire da questa data. Se nullo, non filtra.</param>
        /// <param name="endDate">Filtra le applicazioni avvenute fino a questa data. Se nullo, non filtra.</param>
        /// <param name="maxDepth">Filtra le applicazioni che hanno una profondità minore o uguale a questo valore. Se nullo, non filtra.</param>
        /// <returns>Una lista di oggetti MIUStringTopologyEdge (dati grezzi dell'applicazione di regole).</returns>
        public async Task<List<MIUStringTopologyEdge>> LoadRawRuleApplicationsForTopologyAsync(
            string initialString = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? maxDepth = null)
        {
            var edges = new List<MIUStringTopologyEdge>();
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    await connection.OpenAsync();

                    // Costruiamo dinamicamente la query SQL in base ai parametri di filtro.
                    // Join con MIU_Searches per filtrare per InitialString.
                    // Non joiniamo con MIU_States qui, perché le stringhe vengono inflate solo nel servizio di topologia.
                    // Le colonne 'Timestamp' e 'CurrentDepth' sono direttamente da MIU_RuleApplications.
                    StringBuilder sql = new StringBuilder();
                    sql.Append(@"
                        SELECT
                            RA.ApplicationID,
                            RA.SearchID,
                            RA.ParentStateID,
                            RA.NewStateID,
                            RA.AppliedRuleID,
                            RA.CurrentDepth,
                            RA.Timestamp -- La colonna Timestamp da MIU_RuleApplications
                        FROM MIU_RuleApplications AS RA
                        JOIN MIU_Searches AS S ON RA.SearchID = S.SearchID
                        WHERE 1=1 "); // Clauola WHERE fittizia per semplificare l'aggiunta di condizioni

                    // Aggiungi condizioni WHERE in base ai parametri
                    if (!string.IsNullOrEmpty(initialString))
                    {
                        sql.Append(" AND S.InitialString = @initialString");
                    }
                    if (startDate.HasValue)
                    {
                        // SQLite compara le stringhe ISO 8601 correttamente
                        sql.Append(" AND RA.Timestamp >= @startDate");
                    }
                    if (endDate.HasValue)
                    {
                        sql.Append(" AND RA.Timestamp <= @endDate");
                    }
                    if (maxDepth.HasValue)
                    {
                        sql.Append(" AND RA.CurrentDepth <= @maxDepth");
                    }

                    using (var command = new SQLiteCommand(sql.ToString(), connection))
                    {
                        if (!string.IsNullOrEmpty(initialString))
                        {
                            command.Parameters.AddWithValue("@initialString", initialString);
                        }
                        if (startDate.HasValue)
                        {
                            // Formatta la data in un formato compatibile con SQLite (ISO 8601)
                            command.Parameters.AddWithValue("@startDate", startDate.Value.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture));
                        }
                        if (endDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@endDate", endDate.Value.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture));
                        }
                        if (maxDepth.HasValue)
                        {
                            command.Parameters.AddWithValue("@maxDepth", maxDepth.Value);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                DateTime timestamp;
                                // Tenta di parsare la stringa del timestamp. Se fallisce, usa DateTime.MinValue.
                                if (!DateTime.TryParse(reader.GetString(reader.GetOrdinal("Timestamp")), CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
                                {
                                    timestamp = DateTime.MinValue;
                                    _logger.Log(LogLevel.WARNING, $"[MIUDatabaseManager] Impossibile parsare timestamp per ApplicationID {reader.GetInt64(reader.GetOrdinal("ApplicationID"))}.");
                                }

                                edges.Add(new MIUStringTopologyEdge
                                {
                                    ApplicationID = reader.GetInt64(reader.GetOrdinal("ApplicationID")),
                                    SearchID = reader.GetInt64(reader.GetOrdinal("SearchID")),
                                    ParentStateID = reader.GetInt64(reader.GetOrdinal("ParentStateID")),
                                    NewStateID = reader.GetInt64(reader.GetOrdinal("NewStateID")),
                                    AppliedRuleID = reader.GetInt64(reader.GetOrdinal("AppliedRuleID")),
                                    CurrentDepth = reader.GetInt32(reader.GetOrdinal("CurrentDepth")),
                                    Timestamp = timestamp,
                                    // AppliedRuleName e Weight verranno impostati in MIUTopologyService
                                    AppliedRuleName = null, // Verrà popolato in MIUTopologyService
                                    Weight = 0.0 // Verrà popolato in MIUTopologyService
                                });
                            }
                        }
                    }
                }
                _logger.Log(LogLevel.INFO, $"[MIUDatabaseManager] Caricate {edges.Count} applicazioni di regole per la topologia con filtri.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MIUDatabaseManager ERROR] Errore durante il caricamento delle applicazioni di regole per la topologia: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return new List<MIUStringTopologyEdge>(); // Restituisce lista vuota in caso di errore
            }
            return edges;
        }
        /// <summary>
        /// Resetta i dati specifici dell'esplorazione (ricerche, applicazioni di regole, percorsi, statistiche di apprendimento)
        /// nel database, ma mantiene le regole base, i parametri di configurazione e gli stati MIU generati.
        /// </summary>
        public async Task ResetExplorationDataAsync()
        {
            _logger.Log(LogLevel.INFO, "[MIUDatabaseManager] Inizio reset selettivo dei dati di esplorazione.");
            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    await connection.OpenAsync();

                    // Iniziamo una transazione per assicurare che tutte le eliminazioni siano atomiche
                    using (var transaction = connection.BeginTransaction())
                    {
                        // Array delle tabelle da pulire
                        string[] tablesToClear = new string[]
                        {
                            "MIU_Searches",
                            "MIU_RuleApplications",
                            "MIU_Paths",
                            "MIU_Actions", // Inserita come richiesto, se esiste
                            "Learning_TransitionStatistics",
                            "Learning_RuleStatistics"
                        };

                        foreach (var tableName in tablesToClear)
                        {
                            string sql = $"DELETE FROM {tableName};";
                            using (var command = new SQLiteCommand(sql, connection, transaction))
                            {
                                int rowsAffected = await command.ExecuteNonQueryAsync();
                                _logger.Log(LogLevel.DEBUG, $"[MIUDatabaseManager] Eliminati {rowsAffected} record dalla tabella '{tableName}'.");
                                string resetSequenceSql = $"UPDATE sqlite_sequence SET seq = 0 WHERE name = '{tableName}';";
                                using (var cmd = new SQLiteCommand(resetSequenceSql, connection, transaction))
                                {
                                    await cmd.ExecuteNonQueryAsync();
                                    _logger.Log(LogLevel.DEBUG, $"[MIUDatabaseManager] Sequenza AUTOINCREMENT resettata per '{tableName}'.");
                                }
                            }
                        }
                        transaction.Commit();
                    }
                }
                _logger.Log(LogLevel.INFO, "[MIUDatabaseManager] Reset selettivo dei dati di esplorazione completato con successo.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MIUDatabaseManager ERROR] Errore durante il reset dei dati di esplorazione: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw; // Rilancia l'eccezione
            }
        }
        /// <summary>
        /// Inserisce o aggiorna un record di ExplorationAnomaly nel database.
        /// Se l'anomalia esiste già (stesso Type, RuleId, ContextPatternHash), viene aggiornata.
        /// Altrimenti, viene inserita come nuovo record.
        /// </summary>
        /// <param name="anomaly">L'oggetto ExplorationAnomaly da salvare.</param>
        public void UpsertExplorationAnomaly(ExplorationAnomaly anomaly)
        {
            // La query INSERT OR REPLACE è specifica di SQLite e gestisce l'upsert.
            // Se un record con la stessa combinazione di (Type, RuleId, ContextPatternHash) esiste,
            // viene rimpiazzato. Altrimenti, viene inserito un nuovo record.
            string sql = @"
            INSERT OR REPLACE INTO ExplorationAnomalies (
                Id, Type, RuleId, ContextPatternHash, ContextPatternSample,
                Count, AverageValue, AverageDepth, LastDetected, Description, IsNewCategory, CreatedDate
            ) VALUES (
                -- Se l'anomalia ha già un Id (cioè è stata letta dal DB), lo usiamo.
                -- Altrimenti, passiamo NULL per far sì che AUTOINCREMENT generi un nuovo Id.
                (SELECT Id FROM ExplorationAnomalies WHERE Type = @Type AND RuleId = @RuleId AND ContextPatternHash = @ContextPatternHash),
                @Type, @RuleId, @ContextPatternHash, @ContextPatternSample,
                @Count, @AverageValue, @AverageDepth, @LastDetected, @Description, @IsNewCategory, @CreatedDate
            );";

            try // Inizio blocco try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString)) // Inizio using (connection)
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection)) // Inizio using (command)
                    {
                        // Gestione dei valori nullable (RuleId, ContextPatternHash, ContextPatternSample)
                        // e conversione dei tipi per il database.
                        command.Parameters.AddWithValue("@Id", anomaly.Id == 0 ? (object)DBNull.Value : anomaly.Id);
                        command.Parameters.AddWithValue("@Type", (int)anomaly.Type);
                        command.Parameters.AddWithValue("@RuleId", anomaly.RuleId.HasValue ? (object)anomaly.RuleId.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@ContextPatternHash", anomaly.ContextPatternHash.HasValue ? (object)anomaly.ContextPatternHash.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@ContextPatternSample", anomaly.ContextPatternSample ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Count", anomaly.Count);
                        command.Parameters.AddWithValue("@AverageValue", anomaly.AverageValue);
                        command.Parameters.AddWithValue("@AverageDepth", anomaly.AverageDepth);
                        // Formato ISO 8601 con millisecondi per precisione e compatibilità SQLite
                        command.Parameters.AddWithValue("@LastDetected", anomaly.LastDetected.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("@Description", anomaly.Description);
                        command.Parameters.AddWithValue("@IsNewCategory", anomaly.IsNewCategory ? 1 : 0); // SQLite usa 0/1 per booleani
                        command.Parameters.AddWithValue("@CreatedDate", anomaly.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture));

                        command.ExecuteNonQuery();

                        // Se l'anomalia era nuova (Id == 0 prima dell'insert), aggiorniamo l'Id dell'oggetto C#
                        // con quello generato dal database. Questo è utile se l'oggetto viene riutilizzato in memoria.
                        if (anomaly.Id == 0)
                        {
                            anomaly.Id = connection.LastInsertRowId;
                        }
                    } // Fine using (command)
                } // Fine using (connection)
            } // Fine blocco try
            catch (Exception ex) // Inizio blocco catch
            {
                _logger.Log(LogLevel.ERROR, $"Errore durante l'upsert dell'anomalia di esplorazione: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                // Non rilanciamo l'eccezione qui per permettere al chiamante di continuare,
                // ma il log è essenziale per il debug.
            } // Fine blocco catch
        } // Fine del metodo UpsertExplorationAnomaly
        /// <summary>
        /// Recupera tutte le anomalie di esplorazione persistite nel database.
        /// </summary>
        /// <returns>Una lista di oggetti ExplorationAnomaly.</returns>
        public List<ExplorationAnomaly> GetAllExplorationAnomalies()
        {
            List<ExplorationAnomaly> anomalies = new List<ExplorationAnomaly>();
            string sql = "SELECT Id, Type, RuleId, ContextPatternHash, ContextPatternSample, Count, AverageValue, AverageDepth, LastDetected, Description, IsNewCategory, CreatedDate FROM ExplorationAnomalies;";

            try
            {
                using (var connection = new SQLiteConnection(_schemaLoader.ConnectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                anomalies.Add(new ExplorationAnomaly
                                {
                                    Id = reader.GetInt64(reader.GetOrdinal("Id")),
                                    Type = (AnomalyType)reader.GetInt32(reader.GetOrdinal("Type")),
                                    RuleId = reader.IsDBNull(reader.GetOrdinal("RuleId")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("RuleId")),
                                    ContextPatternHash = reader.IsDBNull(reader.GetOrdinal("ContextPatternHash")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ContextPatternHash")),
                                    ContextPatternSample = reader.IsDBNull(reader.GetOrdinal("ContextPatternSample")) ? null : reader.GetString(reader.GetOrdinal("ContextPatternSample")),
                                    Count = reader.GetInt32(reader.GetOrdinal("Count")),
                                    AverageValue = reader.GetDouble(reader.GetOrdinal("AverageValue")),
                                    AverageDepth = reader.GetDouble(reader.GetOrdinal("AverageDepth")),
                                    // Usa CultureInfo.InvariantCulture e DateTimeStyles.None per un parsing robusto
                                    LastDetected = DateTime.Parse(reader.GetString(reader.GetOrdinal("LastDetected")), CultureInfo.InvariantCulture, DateTimeStyles.None),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    IsNewCategory = reader.GetInt32(reader.GetOrdinal("IsNewCategory")) == 1, // Converti 0/1 in bool
                                    CreatedDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedDate")), CultureInfo.InvariantCulture, DateTimeStyles.None)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore durante il recupero delle anomalie di esplorazione: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return new List<ExplorationAnomaly>(); // Restituisce una lista vuota in caso di errore
            }
            return anomalies;
        }
    }
}
