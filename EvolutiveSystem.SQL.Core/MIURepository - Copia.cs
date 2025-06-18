using MIU.Core;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EvolutiveSystem.SQL.Core
{
    internal class MIURepository
    {
        private readonly SQLiteConnection _connection;
        private SQLiteTransaction _transaction; // La transazione deve essere passata o gestita esternamente

        /// <summary>
        /// Inizializza una nuova istanza di MIURepository.
        /// </summary>
        /// <param name="connection">Una connessione SQLite aperta e valida.</param>
        /// <param name="transaction">La transazione SQLite corrente in cui operare. Può essere null inizialmente.</param>
        public MIURepository(SQLiteConnection connection, SQLiteTransaction transaction = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _transaction = transaction;
        }

        /// <summary>
        /// Imposta la transazione corrente per le operazioni del repository.
        /// </summary>
        /// <param name="transaction">La transazione da utilizzare.</param>
        public void SetTransaction(SQLiteTransaction transaction)
        {
            _transaction = transaction;
        }

        /// <summary>
        /// Inserisce o recupera un MIU_State dalla tabella MIU_States.
        /// Se la stringa esiste già, restituisce il suo StateID. Altrimenti, la inserisce e restituisce il nuovo StateID.
        /// </summary>
        /// <param name="miuStringStandard">La stringa MIU in formato standard (non compresso).</param>
        /// <returns>Lo StateID della stringa.</returns>
        public long UpsertMIUState(string miuStringStandard)
        {
            long stateId;
            string deflateString = MIUStringConverter.DeflateMIUString(miuStringStandard);
            string hash = GetStringHash(miuStringStandard);

            using (var command = _connection.CreateCommand())
            {
                command.Transaction = _transaction;

                // Cerca se la stringa esiste già
                command.CommandText = "SELECT StateID FROM MIU_States WHERE CurrentString = @CurrentString;";
                command.Parameters.AddWithValue("@CurrentString", miuStringStandard);
                object result = command.ExecuteScalar();

                if (result != null)
                {
                    stateId = (long)result;
                    // Incrementa UsageCount
                    using (var updateCommand = _connection.CreateCommand())
                    {
                        updateCommand.Transaction = _transaction;
                        updateCommand.CommandText = "UPDATE MIU_States SET UsageCount = UsageCount + 1 WHERE StateID = @StateID;";
                        updateCommand.Parameters.AddWithValue("@StateID", stateId);
                        updateCommand.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Inserisci la nuova stringa
                    command.CommandText = @"
                        INSERT INTO MIU_States (CurrentString, StringLength, DeflateString, Hash, DiscoveryTime_Int, DiscoveryTime_Text, UsageCount)
                        VALUES (@CurrentString, @StringLength, @DeflateString, @Hash, @DiscoveryTime_Int, @DiscoveryTime_Text, @UsageCount);
                        SELECT last_insert_rowid();";

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@CurrentString", miuStringStandard);
                    command.Parameters.AddWithValue("@StringLength", miuStringStandard.Length);
                    command.Parameters.AddWithValue("@DeflateString", deflateString);
                    command.Parameters.AddWithValue("@Hash", hash);
                    command.Parameters.AddWithValue("@DiscoveryTime_Int", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    command.Parameters.AddWithValue("@DiscoveryTime_Text", DateTime.UtcNow.ToString("o"));
                    command.Parameters.AddWithValue("@UsageCount", 1);

                    stateId = (long)command.ExecuteScalar();
                }
            }
            return stateId;
        }

        /// <summary>
        /// Genera un hash SHA256 per una stringa.
        /// </summary>
        private string GetStringHash(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Inserisce una nuova ricerca nella tabella MIU_Searches.
        /// </summary>
        /// <param name="initialStringCompressed">La stringa iniziale compressa.</param>
        /// <param name="targetStringCompressed">La stringa target compressa.</param>
        /// <param name="searchAlgorithm">L'algoritmo di ricerca utilizzato (es. "BFS", "DFS").</param>
        /// <returns>L'ID della ricerca appena inserita.</returns>
        public long InsertSearch(string initialStringCompressed, string targetStringCompressed, string searchAlgorithm)
        {
            using (var command = _connection.CreateCommand())
            {
                command.Transaction = _transaction;
                command.CommandText = @"
                    INSERT INTO MIU_Searches (InitialString, TargetString, SearchAlgorithm, StartTime, Outcome, StepsTaken, MaxDepth, NodesExplored, EndTime)
                    VALUES (@InitialString, @TargetString, @SearchAlgorithm, @StartTime, NULL, NULL, NULL, NULL, NULL);
                    SELECT last_insert_rowid();";

                command.Parameters.AddWithValue("@InitialString", initialStringCompressed);
                command.Parameters.AddWithValue("@TargetString", targetStringCompressed);
                command.Parameters.AddWithValue("@SearchAlgorithm", searchAlgorithm);
                command.Parameters.AddWithValue("@StartTime", DateTime.UtcNow.ToString("o"));

                long searchId = (long)command.ExecuteScalar();
                Console.WriteLine($"MIURepository: Ricerca {searchId} inserita.");
                return searchId;
            }
        }

        /// <summary>
        /// Aggiorna una ricerca esistente nella tabella MIU_Searches.
        /// </summary>
        public void UpdateSearch(long searchId, bool success, double flightTimeMs, int stepsTaken, int nodesExplored, int maxDepthReached)
        {
            using (var command = _connection.CreateCommand())
            {
                command.Transaction = _transaction;
                command.CommandText = @"
                    UPDATE MIU_Searches
                    SET Outcome = @Outcome, EndTime = @EndTime, StepsTaken = @StepsTaken,
                        NodesExplored = @NodesExplored, MaxDepth = @MaxDepth
                    WHERE SearchID = @SearchID;";

                command.Parameters.AddWithValue("@Outcome", success ? "Success" : "Failure");
                command.Parameters.AddWithValue("@EndTime", DateTime.UtcNow.ToString("o"));
                command.Parameters.AddWithValue("@StepsTaken", stepsTaken);
                command.Parameters.AddWithValue("@NodesExplored", nodesExplored);
                command.Parameters.AddWithValue("@MaxDepth", maxDepthReached);
                command.Parameters.AddWithValue("@SearchID", searchId);

                command.ExecuteNonQuery();
                Console.WriteLine($"MIURepository: Ricerca {searchId} aggiornata.");
            }
        }

        /// <summary>
        /// Inserisce un'applicazione di regola nella tabella MIU_RuleApplications.
        /// </summary>
        public void InsertRuleApplication(long searchId, long parentStateId, long newStateId, int appliedRuleId, int currentDepth)
        {
            using (var command = _connection.CreateCommand())
            {
                command.Transaction = _transaction;
                command.CommandText = @"
                    INSERT INTO MIU_RuleApplications (SearchID, ParentStateID, NewStateID, AppliedRuleID, CurrentDepth, Timestamp)
                    VALUES (@SearchID, @ParentStateID, @NewStateID, @AppliedRuleID, @CurrentDepth, @Timestamp);";

                command.Parameters.AddWithValue("@SearchID", searchId);
                command.Parameters.AddWithValue("@ParentStateID", parentStateId);
                command.Parameters.AddWithValue("@NewStateID", newStateId);
                command.Parameters.AddWithValue("@AppliedRuleID", appliedRuleId);
                command.Parameters.AddWithValue("@CurrentDepth", currentDepth);
                command.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("o"));

                command.ExecuteNonQuery();
                // Console.WriteLine($"MIURepository: Applicazione regola per ricerca {searchId} registrata.");
            }
        }

        /// <summary>
        /// Inserisce un passo di un percorso di soluzione nella tabella MIU_Paths.
        /// </summary>
        public void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, int? appliedRuleId, bool isTarget, bool isSuccess, int depth)
        {
            using (var command = _connection.CreateCommand())
            {
                command.Transaction = _transaction;
                command.CommandText = @"
                    INSERT INTO MIU_Paths (SearchID, StepNumber, StateID, ParentStateID, AppliedRuleID, IsTarget, IsSuccess, Depth)
                    VALUES (@SearchID, @StepNumber, @StateID, @ParentStateID, @AppliedRuleID, @IsTarget, @IsSuccess, @Depth);";

                command.Parameters.AddWithValue("@SearchID", searchId);
                command.Parameters.AddWithValue("@StepNumber", stepNumber);
                command.Parameters.AddWithValue("@StateID", stateId);
                command.Parameters.AddWithValue("@ParentStateID", (object)parentStateId ?? DBNull.Value);
                command.Parameters.AddWithValue("@AppliedRuleID", (object)appliedRuleId ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsTarget", isTarget);
                command.Parameters.AddWithValue("@IsSuccess", isSuccess);
                command.Parameters.AddWithValue("@Depth", depth);

                command.ExecuteNonQuery();
                // Console.WriteLine($"MIURepository: Passo percorso soluzione per ricerca {searchId} registrato.");
            }
        }

        /// <summary>
        /// Inserisce o aggiorna i dettagli delle regole nella tabella RegoleMIU.
        /// Questo dovrebbe essere chiamato una volta all'avvio dell'applicazione.
        /// </summary>
        public void UpsertRegoleMIU(List<RegolaMIU> regole)
        {
            // Nota: Questa funzione opera con una sua transazione interna,
            // non con _transaction della classe, per poter essere chiamata
            // indipendentemente da una ricerca attiva.
            using (var transaction = _connection.BeginTransaction())
            {
                foreach (var rule in regole)
                {
                    using (var command = _connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = @"
                            INSERT OR REPLACE INTO RegoleMIU (ID, Nome, Descrizione, Pattern, Sostituzione)
                            VALUES (@ID, @Nome, @Descrizione, @Pattern, @Sostituzione);";

                        command.Parameters.AddWithValue("@ID", rule.ID);
                        command.Parameters.AddWithValue("@Nome", rule.Nome);
                        command.Parameters.AddWithValue("@Descrizione", (object)rule.Descrizione ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Pattern", (object)rule.Pattern ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Sostituzione", (object)rule.Sostituzione ?? DBNull.Value);
                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
                Console.WriteLine($"MIURepository: {regole.Count} regole MIU inserite/aggiornate.");
            }
        }
    }
}
