// File: MiuSeederTool.Core/SeederDbAccess.cs

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MasterLog; // Necessario per LogLevel

namespace MiuSeederTool.Core
{
    /// <summary>
    /// Provides data access methods for the MIU_States database.
    /// Handles database creation, state management, and rule loading.
    /// </summary>
    public class SeederDbAccess
    {
        private readonly string _connectionString;
        private readonly Logger _logger; // Ora è l'istanza del logger

        /// <summary>
        /// Initializes a new instance of SeederDbAccess.
        /// </summary>
        /// <param name="databaseFilePath">The full path to the SQLite database file.</param>
        /// <param name="logger">The Logger instance for logging.</param>
        public SeederDbAccess(string databaseFilePath, Logger logger)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("Database file path cannot be null or empty.", nameof(databaseFilePath));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = $"Data Source={databaseFilePath};Version=3;";

            InitializeDatabase(databaseFilePath);
            _logger.Log(LogLevel.DEBUG, $"[SeederDbAccess] Initialized for database: {databaseFilePath}"); // Log a livello DEBUG
        }

        /// <summary>
        /// Ensures the database file and necessary tables exist.
        /// </summary>
        /// <param name="databaseFilePath">The path to the database file.</param>
        private void InitializeDatabase(string databaseFilePath)
        {
            if (!File.Exists(databaseFilePath))
            {
                _logger.Log(LogLevel.INFO, $"[SeederDbAccess] Database file not found. Creating new database at: {databaseFilePath}"); // Log a livello INFO
                SQLiteConnection.CreateFile(databaseFilePath);
            }

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string createStatesTableSql = @"
                    CREATE TABLE IF NOT EXISTS MIU_States (
                        StateID INTEGER PRIMARY KEY AUTOINCREMENT,
                        CurrentString TEXT NOT NULL UNIQUE,
                        StringLength INTEGER NOT NULL,
                        DeflateString TEXT NOT NULL UNIQUE,
                        Hash TEXT NOT NULL UNIQUE,
                        DiscoveryTime_Int INTEGER NOT NULL,
                        DiscoveryTime_Text TEXT NOT NULL,
                        UsageCount INTEGER NOT NULL,
                        SeedingType INTEGER NOT NULL DEFAULT 0 -- *** AGGIUNTA QUESTA COLONNA FONDAMENTALE ***
                    );";
                using (var command = new SQLiteCommand(createStatesTableSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                string createRulesTableSql = @"
                    CREATE TABLE IF NOT EXISTS MIU_Rules (
                        RuleID INTEGER PRIMARY KEY,
                        RuleName TEXT NOT NULL UNIQUE,
                        Pattern TEXT NOT NULL,
                        Replacement TEXT NOT NULL
                    );";
                using (var command = new SQLiteCommand(createRulesTableSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                PopulateMiuRules(connection);
            }
        }

        /// <summary>
        /// Populates the MIU_Rules table with the four Hofstadter MIU rules if it's empty.
        /// </summary>
        /// <param name="connection">The active SQLiteConnection.</param>
        private void PopulateMiuRules(SQLiteConnection connection)
        {
            string checkRulesCountSql = "SELECT COUNT(*) FROM MIU_Rules;";
            using (var command = new SQLiteCommand(checkRulesCountSql, connection))
            {
                long count = (long)command.ExecuteScalar();
                if (count == 0)
                {
                    _logger.Log(LogLevel.INFO, "[SeederDbAccess] MIU_Rules table is empty. Populating with default rules."); // Log a livello INFO
                    var rules = new List<SeederMiuRule>
                    {
                        new SeederMiuRule { RuleID = 1, RuleName = "Regola I", Pattern = "xI", Replacement = "xIU" },
                        new SeederMiuRule { RuleID = 2, RuleName = "Regola II (Hofstadter)", Pattern = "Mx", Replacement = "Mxx" },
                        new SeederMiuRule { RuleID = 3, RuleName = "Regola III", Pattern = "III", Replacement = "U" },
                        new SeederMiuRule { RuleID = 4, RuleName = "Regola IV", Pattern = "UU", Replacement = "" }
                    };

                    foreach (var rule in rules)
                    {
                        string insertSql = "INSERT INTO MIU_Rules (RuleID, RuleName, Pattern, Replacement) VALUES (@RuleID, @RuleName, @Pattern, @Replacement);";
                        using (var insertCommand = new SQLiteCommand(insertSql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@RuleID", rule.RuleID);
                            insertCommand.Parameters.AddWithValue("@RuleName", rule.RuleName);
                            insertCommand.Parameters.AddWithValue("@Pattern", rule.Pattern);
                            insertCommand.Parameters.AddWithValue("@Replacement", rule.Replacement);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                    _logger.Log(LogLevel.DEBUG, $"[SeederDbAccess] Loaded {rules.Count} MIU rules."); // Log a livello DEBUG
                }
                else
                {
                    _logger.Log(LogLevel.DEBUG, $"[SeederDbAccess] Loaded {count} MIU rules."); // Log a livello DEBUG
                }
            }
        }

        /// <summary>
        /// Loads all MIU rules from the database.
        /// </summary>
        /// <returns>An enumerable collection of SeederMiuRule objects.</returns>
        public async Task<IEnumerable<SeederMiuRule>> LoadRegoleMIUAsync()
        {
            return await Task.Run(() =>
            {
                var rules = new List<SeederMiuRule>();
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string selectSql = "SELECT RuleID, RuleName, Pattern, Replacement FROM MIU_Rules ORDER BY RuleID;";
                    using (var command = new SQLiteCommand(selectSql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                rules.Add(new SeederMiuRule
                                {
                                    RuleID = reader.GetInt32(0),
                                    RuleName = reader.GetString(1),
                                    Pattern = reader.GetString(2),
                                    Replacement = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
                return rules;
            });
        }

        /// <summary>
        /// Inserts a new MIU state into the database.
        /// </summary>
        /// <param name="state">The SeederMiuState object to insert. It must have all fields populated.</param>
        public async Task InsertMiuStateAsync(SeederMiuState state)
        {
            // Assicurati che la stringa non sia nulla o vuota e che la lunghezza sia valida
            if (string.IsNullOrWhiteSpace(state.CurrentString) || state.StringLength <= 0)
            {
                _logger.Log(LogLevel.ERROR, $"[SeederDbAccess] Tentativo di inserire una stringa MIU non valida: '{state.CurrentString}' (Length: {state.StringLength}). Inserimento annullato.", true);
                return;
            }

            // Calcola hash e deflate se non già presenti (dovrebbero esserlo, ma per sicurezza)
            if (string.IsNullOrEmpty(state.DeflateString))
            {
                state.DeflateString = CompressMiuString(state.CurrentString);
            }
            if (string.IsNullOrEmpty(state.Hash))
            {
                state.Hash = CalculateSha256Hash(state.CurrentString);
            }

            await Task.Run(() =>
            {
                try // Aggiunto blocco try-catch per gestire eccezioni all'interno del Task.Run
                {
                    using (var connection = new SQLiteConnection(_connectionString))
                    {
                        connection.Open();
                        string insertSql = @"
                    INSERT OR IGNORE INTO MIU_States (
                        CurrentString, StringLength, DeflateString, Hash,
                        DiscoveryTime_Int, DiscoveryTime_Text, UsageCount, SeedingType
                    )
                    VALUES (
                        @CurrentString, @StringLength, @DeflateString, @Hash,
                        @DiscoveryTime_Int, @DiscoveryTime_Text, @UsageCount, @SeedingType
                    );
                ";
                        using (var command = new SQLiteCommand(insertSql, connection))
                        {
                            command.Parameters.AddWithValue("@CurrentString", state.CurrentString);
                            command.Parameters.AddWithValue("@StringLength", state.StringLength);
                            command.Parameters.AddWithValue("@DeflateString", state.DeflateString);
                            command.Parameters.AddWithValue("@Hash", state.Hash);
                            command.Parameters.AddWithValue("@DiscoveryTime_Int", state.DiscoveryTime_Int);
                            command.Parameters.AddWithValue("@DiscoveryTime_Text", state.DiscoveryTime_Text);
                            command.Parameters.AddWithValue("@UsageCount", state.UsageCount);
                            command.Parameters.AddWithValue("@SeedingType", (int)state.SeedingType); // Assicurati che questo sia @SeedingType
                            _logger.Log(LogLevel.DEBUG, $"[SeederDbAccess] DEBUG: Inserimento di '{state.CurrentString}'. Valore SeedingType (oggetto): {state.SeedingType}. Valore int: {(int)state.SeedingType}", false);
                            command.ExecuteNonQuery();
                        }
                    }
                    // Log di successo per l'inserimento
                    _logger.Log(LogLevel.DEBUG, $"[SeederDbAccess] Inserita nuova stringa MIU: '{state.CurrentString}' (Type: {state.SeedingType}).", true);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.ERROR, $"[SeederDbAccess] Errore durante l'inserimento dello stato MIU '{state.CurrentString}': {ex.Message}{Environment.NewLine}{ex.StackTrace}", true);
                    // Stampa anche sulla console per visibilità immediata
                    Console.WriteLine($"[ERRORE DB] Fallimento inserimento per '{state.CurrentString}': {ex.Message}");
                    // Non rilanciare qui, dato che sei dentro Task.Run e non c'è un await diretto per catturarlo fuori facilmente.
                    // Il log e la stampa su console sono sufficienti per la diagnosi.
                }
            });
        }


        /// <summary>
        /// Updates the string and related properties for a specific MIU state by ID.
        /// </summary>
        /// <param name="stateId">The ID of the state to update.</param>
        /// <param name="newString">The new string value.</param>
        public async Task UpdateMiuStateStringAsync(long stateId, string newString)
        {
            await Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string updateSql = @"UPDATE MIU_States SET 
                                            CurrentString = @NewString, 
                                            StringLength = @StringLength,
                                            DeflateString = @DeflateString,
                                            Hash = @Hash,
                                            DiscoveryTime_Int = @DiscoveryTime_Int,
                                            DiscoveryTime_Text = @DiscoveryTime_Text
                                            WHERE StateID = @StateID;";
                    using (var command = new SQLiteCommand(updateSql, connection))
                    {
                        long unixTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                        string readableTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                        command.Parameters.AddWithValue("@NewString", newString);
                        command.Parameters.AddWithValue("@StringLength", newString.Length);
                        command.Parameters.AddWithValue("@DeflateString", CompressMiuString(newString));
                        command.Parameters.AddWithValue("@Hash", CalculateSha256Hash(newString));
                        command.Parameters.AddWithValue("@DiscoveryTime_Int", unixTimestamp);
                        command.Parameters.AddWithValue("@DiscoveryTime_Text", readableTime);
                        command.Parameters.AddWithValue("@StateID", stateId);
                        command.ExecuteNonQuery();
                    }
                }
            });
        }

        /// <summary>
        /// Loads all MIU states from the database.
        /// </summary>
        /// <returns>An enumerable collection of SeederMiuState objects.</returns>
        public async Task<IEnumerable<SeederMiuState>> LoadAllMiuStatesAsync()
        {
            return await Task.Run(() =>
            {
                var states = new List<SeederMiuState>();
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    // Assicurati di selezionare la colonna SeedingType
                    string selectSql = "SELECT StateID, CurrentString, StringLength, DeflateString, Hash, DiscoveryTime_Int, DiscoveryTime_Text, UsageCount, SeedingType FROM MIU_States;";
                    using (var command = new SQLiteCommand(selectSql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                states.Add(new SeederMiuState
                                {
                                    StateID = reader.GetInt64(0),
                                    CurrentString = reader.GetString(1),
                                    StringLength = reader.GetInt32(2),
                                    DeflateString = reader.GetString(3),
                                    Hash = reader.GetString(4),
                                    DiscoveryTime_Int = reader.GetInt64(5),
                                    DiscoveryTime_Text = reader.GetString(6),
                                    UsageCount = reader.GetInt32(7),
                                    SeedingType = (SeedingType)reader.GetInt32(8) // Leggi il SeedingType
                                });
                            }
                        }
                    }
                }
                return states;
            });
        }

        /// <summary>
        /// Loads all MIU State IDs from the database.
        /// </summary>
        /// <returns>An enumerable collection of State IDs.</returns>
        public async Task<IEnumerable<long>> LoadAllMiuStateIDsAsync()
        {
            return await Task.Run(() =>
            {
                var ids = new List<long>();
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string selectSql = "SELECT StateID FROM MIU_States;";
                    using (var command = new SQLiteCommand(selectSql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ids.Add(reader.GetInt64(0));
                            }
                        }
                    }
                }
                return ids;
            });
        }

        /// <summary>
        /// Clears all records from the MIU_States table.
        /// </summary>
        public async Task ClearMiuStatesTableAsync()
        {
            await Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string deleteSql = "DELETE FROM MIU_States;";
                    using (var command = new SQLiteCommand(deleteSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    string resetSequenceSql = "DELETE FROM sqlite_sequence WHERE name='MIU_States';";
                    using (var command = new SQLiteCommand(resetSequenceSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            });
        }
        /// <summary>
        /// Updates the SeedingType for a specific MIU state by ID.
        /// </summary>
        /// <param name="stateId">The ID of the state to update.</param>
        /// <param name="seedingType">The new SeedingType value.</param>
        public async Task UpdateMiuStateSeedingTypeAsync(long stateId, SeedingType seedingType)
        {
            await Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string updateSql = @"UPDATE MIU_States SET SeedingType = @SeedingType WHERE StateID = @StateID;";
                    using (var command = new SQLiteCommand(updateSql, connection))
                    {
                        command.Parameters.AddWithValue("@SeedingType", (int)seedingType);
                        command.Parameters.AddWithValue("@StateID", stateId);
                        command.ExecuteNonQuery();
                    }
                }
            });
        }

        /// <summary>
        /// Compresses a MIU string by replacing repeating characters (like 'IIII' with 'I4').
        /// </summary>
        /// <param name="miuString">The MIU string to compress.</param>
        /// <returns>The compressed string.</returns>
        public static string CompressMiuString(string miuString)
        {
            if (string.IsNullOrEmpty(miuString)) return miuString;

            StringBuilder compressed = new StringBuilder();
            char? lastChar = null;
            int count = 0;

            foreach (char c in miuString)
            {
                if (c == lastChar)
                {
                    count++;
                }
                else
                {
                    if (lastChar.HasValue)
                    {
                        compressed.Append(lastChar.Value);
                        if (count > 1)
                        {
                            compressed.Append(count);
                        }
                    }
                    lastChar = c;
                    count = 1;
                }
            }

            if (lastChar.HasValue)
            {
                compressed.Append(lastChar.Value);
                if (count > 1)
                {
                    compressed.Append(count);
                }
            }

            return compressed.ToString();
        }

        /// <summary>
        /// Inflates a compressed MIU string back to its original form.
        /// </summary>
        /// <param name="compressedString">The compressed MIU string.</param>
        /// <returns>The inflated string.</returns>
        public static string InflateMiuString(string compressedString)
        {
            if (string.IsNullOrEmpty(compressedString)) return compressedString;

            StringBuilder inflated = new StringBuilder();
            for (int i = 0; i < compressedString.Length; i++)
            {
                char c = compressedString[i];
                if (char.IsLetter(c))
                {
                    int count = 0;
                    int j = i + 1;
                    while (j < compressedString.Length && char.IsDigit(compressedString[j]))
                    {
                        count = count * 10 + (compressedString[j] - '0');
                        j++;
                    }

                    if (count == 0) // No number after character, means count is 1
                    {
                        inflated.Append(c);
                    }
                    else
                    {
                        for (int k = 0; k < count; k++)
                        {
                            inflated.Append(c);
                        }
                    }
                    i = j - 1; // Move index to the last digit read or the character itself
                }
                else
                {
                    // Should not happen with valid compressed strings, but handle defensively
                    inflated.Append(c);
                }
            }
            return inflated.ToString();
        }

        /// <summary>
        /// Calculates the SHA256 hash of a string.
        /// </summary>
        /// <param name="rawString">The string to hash.</param>
        /// <returns>The SHA256 hash as a hexadecimal string.</returns>
        public static string CalculateSha256Hash(string rawString)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawString));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
