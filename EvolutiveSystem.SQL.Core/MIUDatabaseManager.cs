using EvolutiveSystem.SQL.Core;
using MIU.Core; // Assicurati che MIU.Core sia presente se RegolaMIU è lì
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core // <--- MODIFICATA: La namespace dovrebbe essere MIU.Core
{
    /// <summary>
    /// Manager per la gestione della persistenza dei dati specifici del gioco MIU
    /// nelle tabelle SQLite (MIU_States, MIU_Searches, MIU_Paths, MIU_RuleApplications, RegoleMIU).
    /// Si interfaccia con DatabaseManager per l'esecuzione dei comandi SQL e la gestione delle transazioni.
    /// </summary>
    public class MIUDatabaseManager // <--- MODIFICATO: Il nome della classe
    {
        private readonly DatabaseManager _dbManager; // Riferimento al DatabaseManager

        /// <summary>
        /// Inizializza una nuova istanza di MIUDatabaseManager.
        /// </summary>
        /// <param name="dbManager">Un'istanza di DatabaseManager già aperta e valida.</param>
        public MIUDatabaseManager(DatabaseManager dbManager) // <--- MODIFICATO: Il nome del costruttore
        {
            _dbManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager));
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
            string deflateString = MIUStringConverter.DeflateMIUString(miuStringStandard); // <--- ASSICURATI CHE MIUStringConverter SIA ACCESSIBILE
            string hash = GetStringHash(miuStringStandard);

            // Cerca se la stringa esiste già
            string selectSql = "SELECT StateID FROM MIU_States WHERE CurrentString = @CurrentString;";
            object result = _dbManager.ExecuteScalar(selectSql, new SQLiteParameter("@CurrentString", miuStringStandard));

            if (result != null)
            {
                stateId = (long)result;
                // Incrementa UsageCount
                string updateSql = "UPDATE MIU_States SET UsageCount = UsageCount + 1 WHERE StateID = @StateID;";
                _dbManager.ExecuteNonQuery(updateSql, new SQLiteParameter("@StateID", stateId));
            }
            else
            {
                // Inserisci la nuova stringa
                string insertSql = @"
                    INSERT INTO MIU_States (CurrentString, StringLength, DeflateString, Hash, DiscoveryTime_Int, DiscoveryTime_Text, UsageCount)
                    VALUES (@CurrentString, @StringLength, @DeflateString, @Hash, @DiscoveryTime_Int, @DiscoveryTime_Text, @UsageCount);
                    SELECT last_insert_rowid();";

                stateId = (long)_dbManager.ExecuteScalar(insertSql,
                    new SQLiteParameter("@CurrentString", miuStringStandard),
                    new SQLiteParameter("@StringLength", miuStringStandard.Length),
                    new SQLiteParameter("@DeflateString", deflateString),
                    new SQLiteParameter("@Hash", hash),
                    new SQLiteParameter("@DiscoveryTime_Int", DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                    new SQLiteParameter("@DiscoveryTime_Text", DateTime.UtcNow.ToString("o")),
                    new SQLiteParameter("@UsageCount", 1)
                );
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
            string insertSql = @"
                INSERT INTO MIU_Searches (InitialString, TargetString, SearchAlgorithm, StartTime, Outcome, StepsTaken, MaxDepth, NodesExplored, EndTime)
                VALUES (@InitialString, @TargetString, @SearchAlgorithm, @StartTime, NULL, NULL, NULL, NULL, NULL);
                SELECT last_insert_rowid();";

            long searchId = (long)_dbManager.ExecuteScalar(insertSql,
                new SQLiteParameter("@InitialString", initialStringCompressed),
                new SQLiteParameter("@TargetString", targetStringCompressed),
                new SQLiteParameter("@SearchAlgorithm", searchAlgorithm),
                new SQLiteParameter("@StartTime", DateTime.UtcNow.ToString("o"))
            );
            Console.WriteLine($"MIUDatabaseManager: Ricerca {searchId} inserita.");
            return searchId;
        }

        /// <summary>
        /// Aggiorna una ricerca esistente nella tabella MIU_Searches.
        /// </summary>
        public void UpdateSearch(long searchId, bool success, double flightTimeMs, int stepsTaken, int nodesExplored, int maxDepthReached)
        {
            string updateSql = @"
                UPDATE MIU_Searches
                SET Outcome = @Outcome, EndTime = @EndTime, StepsTaken = @StepsTaken,
                    NodesExplored = @NodesExplored, MaxDepth = @MaxDepth
                WHERE SearchID = @SearchID;";

            _dbManager.ExecuteNonQuery(updateSql,
                new SQLiteParameter("@Outcome", success ? "Success" : "Failure"),
                new SQLiteParameter("@EndTime", DateTime.UtcNow.ToString("o")),
                new SQLiteParameter("@StepsTaken", stepsTaken),
                new SQLiteParameter("@NodesExplored", nodesExplored),
                new SQLiteParameter("@MaxDepth", maxDepthReached),
                new SQLiteParameter("@SearchID", searchId)
            );
            Console.WriteLine($"MIUDatabaseManager: Ricerca {searchId} aggiornata.");
        }

        /// <summary>
        /// Inserisce un'applicazione di regola nella tabella MIU_RuleApplications.
        /// </summary>
        public void InsertRuleApplication(long searchId, long parentStateId, long newStateId, int appliedRuleId, int currentDepth)
        {
            string insertSql = @"
                INSERT INTO MIU_RuleApplications (SearchID, ParentStateID, NewStateID, AppliedRuleID, CurrentDepth, Timestamp)
                VALUES (@SearchID, @ParentStateID, @NewStateID, @AppliedRuleID, @CurrentDepth, @Timestamp);";

            _dbManager.ExecuteNonQuery(insertSql,
                new SQLiteParameter("@SearchID", searchId),
                new SQLiteParameter("@ParentStateID", parentStateId),
                new SQLiteParameter("@NewStateID", newStateId),
                new SQLiteParameter("@AppliedRuleID", appliedRuleId),
                new SQLiteParameter("@CurrentDepth", currentDepth),
                new SQLiteParameter("@Timestamp", DateTime.UtcNow.ToString("o"))
            );
            // Console.WriteLine($"MIUDatabaseManager: Applicazione regola per ricerca {searchId} registrata.");
        }

        /// <summary>
        /// Inserisce un passo di un percorso di soluzione nella tabella MIU_Paths.
        /// </summary>
        public void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, int? appliedRuleId, bool isTarget, bool isSuccess, int depth)
        {
            string insertSql = @"
                INSERT INTO MIU_Paths (SearchID, StepNumber, StateID, ParentStateID, AppliedRuleID, IsTarget, IsSuccess, Depth)
                VALUES (@SearchID, @StepNumber, @StateID, @ParentStateID, @AppliedRuleID, @IsTarget, @IsSuccess, @Depth);";

            _dbManager.ExecuteNonQuery(insertSql,
                new SQLiteParameter("@SearchID", searchId),
                new SQLiteParameter("@StepNumber", stepNumber),
                new SQLiteParameter("@StateID", stateId),
                new SQLiteParameter("@ParentStateID", (object)parentStateId ?? DBNull.Value),
                new SQLiteParameter("@AppliedRuleID", (object)appliedRuleId ?? DBNull.Value),
                new SQLiteParameter("@IsTarget", isTarget),
                new SQLiteParameter("@IsSuccess", isSuccess),
                new SQLiteParameter("@Depth", depth)
            );
            // Console.WriteLine($"MIUDatabaseManager: Passo percorso soluzione per ricerca {searchId} registrato.");
        }

        /// <summary>
        /// Inserisce o aggiorna i dettagli delle regole nella tabella RegoleMIU.
        /// Questo dovrebbe essere chiamato una volta all'avvio dell'applicazione.
        /// </summary>
        public void UpsertRegoleMIU(List<RegolaMIU> regole)
        {
            foreach (var rule in regole)
            {
                string sql = @"
                    INSERT OR REPLACE INTO RegoleMIU (ID, Nome, Descrizione, Pattern, Sostituzione)
                    VALUES (@ID, @Nome, @Descrizione, @Pattern, @Sostituzione);";

                _dbManager.ExecuteNonQuery(sql,
                    new SQLiteParameter("@ID", rule.ID),
                    new SQLiteParameter("@Nome", rule.Nome),
                    new SQLiteParameter("@Descrizione", (object)rule.Descrizione ?? DBNull.Value),
                    new SQLiteParameter("@Pattern", (object)rule.Pattern ?? DBNull.Value),
                    new SQLiteParameter("@Sostituzione", (object)rule.Sostituzione ?? DBNull.Value)
                );
            }
            Console.WriteLine($"MIUDatabaseManager: {regole.Count} regole MIU inserite/aggiornate.");
        }
    }
}
