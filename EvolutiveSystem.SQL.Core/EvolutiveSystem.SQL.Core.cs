// File: C:\Progetti\EvolutiveSystem\EvolutiveSystem.SQL.Core\EvolutiveSystem.SQL.Core.cs
// creato 5.6.2025 0.59
// Data di riferimento: 20 giugno 2025 (Aggiornato: costruttore con Logger, logging, fix CS1061)
// Questo file è basato sulla versione fornita dall'utente e modificato MINIMAMENTE
// per risolvere gli errori di compilazione e integrare il logger.
// AGGIORNATO 20.06.2025: Rimosse le istruzioni CREATE TABLE IF NOT EXISTS dal metodo InitializeDatabase()
//                       per consentire la creazione manuale delle tabelle da parte dell'utente.

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterLog; // AGGIUNTO: Necessario per la tua classe Logger
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using EvolutiveSystem.Common; // AGGIUNTO: Necessario per File.Exists

namespace EvolutiveSystem.SQL.Core
{
    public class Field
    {
        public string FieldName { get; set; }
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool AutoIncrement { get; set; }
        public string TableName { get; set; } // Nome della tabella a cui appartiene
        public Table ParentTable { get; set; } // Riferimento alla tabella padre

        public Field() { }

        public Field(string fieldName, string dataType, bool isPrimaryKey, bool autoIncrement, Table parentTable)
        {
            FieldName = fieldName;
            DataType = dataType;
            IsPrimaryKey = isPrimaryKey;
            AutoIncrement = autoIncrement;
            ParentTable = parentTable;
            TableName = parentTable?.TableName;
        }
    }

    [Serializable]
    public class Table
    {
        public string TableName { get; set; }
        public List<Field> Fields { get; set; } = new List<Field>();

        // --- INIZIO MODIFICA: Aggiunta la proprietà DataRecords ---
        /// <summary>
        /// Contiene i record di dati della tabella, come lista di dizionari serializzabili.
        /// Questo è il campo che viene svuotato per la "struttura soltanto".
        /// </summary>
        public List<SerializableDictionary<string, object>> DataRecords { get; set; } = new List<SerializableDictionary<string, object>>();
        // --- FINE MODIFICA ---

        // --- INIZIO MODIFICA: Aggiunto [XmlIgnore] per ParentDatabase ---
        [XmlIgnore]
        public Database ParentDatabase { get; set; }
        // --- FINE MODIFICA ---

        public Table()
        {
            // Assicurati che le liste siano inizializzate anche nel costruttore di default
            Fields = new List<Field>();
            DataRecords = new List<SerializableDictionary<string, object>>();
        }

        public Table(string tableName, Database parentDatabase)
        {
            TableName = tableName;
            ParentDatabase = parentDatabase;
            // Assicurati che le liste siano inizializzate anche qui
            Fields = new List<Field>();
            DataRecords = new List<SerializableDictionary<string, object>>();
        }

        public void AddField(Field field)
        {
            if (field != null)
            {
                Fields.Add(field);
                field.ParentTable = this;
                // La riga originale che causava CS1061 era in questo commento. La rimuoviamo completamente.
                // field.TableName = this.TableName; 
            }
        }
    }

    public class Database
    {
        // Contatore statico per generare ID unici per le istanze di Database in memoria.
        // Utilizziamo long per supportare un numero elevato di database.
        private static long _nextDatabaseId = 1;

        /// <summary>
        /// Ottiene o imposta l'ID unico di questo database.
        /// Questo ID è generato al momento della creazione dell'istanza in memoria.
        /// </summary>
        public long DatabaseId { get; set; }
        public string DatabaseName { get; set; }
        public string FilePath { get; set; } // Percorso del file SQLite
        public List<Table> Tables { get; set; } = new List<Table>();

        public Database()
        {
            // --- INIZIO MODIFICA NECESSARIA (Continua) ---
            // Assegna un ID unico al momento della creazione di un'istanza di Database.
            // Interlocked.Increment garantisce che l'operazione sia thread-safe.
            DatabaseId = Interlocked.Increment(ref _nextDatabaseId);
            // --- FINE MODIFICA NECESSARIA (Continua) ---
        }

        public Database(string databaseName, string filePath)
        {
            DatabaseId = Interlocked.Increment(ref _nextDatabaseId);
            DatabaseName = databaseName;
            FilePath = filePath;
        }

        public void AddTable(Table table)
        {
            if (table != null)
            {
                Tables.Add(table);
                table.ParentDatabase = this;
                // RIGA CORRETTA: La riga errata è stata rimossa nella classe Table.AddField
            }
        }
    }

    public class SQLiteSchemaLoader
    {
        const string select = "SELECT";
        const string from = "FROM";

        private readonly string _connectionString;
        private readonly Logger _logger; // AGGIUNTO: Campo per l'istanza del logger
        private readonly string _databaseFilePath; // NUOVO: per il controllo esistenza file

        /// <summary>
        /// Costruttore.
        /// </summary>
        /// <param name="databaseFilePath">Percorso completo al file del database SQLite.</param>
        /// <param name="logger">L'istanza del logger per la registrazione degli eventi.</param>
        public SQLiteSchemaLoader(string databaseFilePath, Logger logger) // MODIFICATO: Accetta ora il Logger
        {
            _databaseFilePath = databaseFilePath; // Salva il percorso del file
            _connectionString = $"Data Source={databaseFilePath};Version=3;";
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger non può essere nullo."); // Inizializza il logger
            _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] Costruttore chiamato. ConnectionString: {_connectionString}"); // Aggiunto log

            // Rimuoviamo la logica di creazione delle tabelle da qui,
            // poiché l'utente preferisce crearle manualmente tramite SQLiteStudio.
        }

        /// <summary>
        /// Inizializza il database SQLite, creando il file del database se non esiste.
        /// Non crea le tabelle, la loro gestione è demandata all'utente.
        /// Questo metodo deve essere chiamato all'avvio dell'applicazione.
        /// </summary>
        public void InitializeDatabase()
        {
            _logger.Log(LogLevel.INFO, "[SQLiteSchemaLoader INFO] Inizializzazione del database...");

            try
            {
                // Se il file del database non esiste, lo crea.
                if (!File.Exists(_databaseFilePath))
                {
                    SQLiteConnection.CreateFile(_databaseFilePath);
                    _logger.Log(LogLevel.INFO, $"[SQLiteSchemaLoader INFO] Database file creato: {_databaseFilePath}");
                }

                // Apre e chiude la connessione per assicurarsi che il file sia accessibile.
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    _logger.Log(LogLevel.DEBUG, "[SQLiteSchemaLoader DEBUG] Connessione database aperta e chiusa per InitializeDatabase (solo verifica accesso).");
                    // Nessuna creazione di tabelle qui.
                }
                _logger.Log(LogLevel.INFO, "[SQLiteSchemaLoader INFO] Inizializzazione del database completata. Le tabelle devono essere create manualmente.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[SQLiteSchemaLoader ERROR] Errore durante l'inizializzazione del database: {ex.Message}");
                // Rilancia l'eccezione, poiché un errore qui impedirebbe il funzionamento dell'applicazione.
                throw;
            }
        }


        public Database LoadSchema()
        {
            _logger.Log(LogLevel.DEBUG, "[SQLiteSchemaLoader DEBUG] Caricamento schema database..."); // Aggiunto log
            var database = new Database(System.IO.Path.GetFileNameWithoutExtension(_connectionString.Split(';')[0].Split('=')[1]), _connectionString.Split(';')[0].Split('=')[1]);

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                _logger.Log(LogLevel.DEBUG, "[SQLiteSchemaLoader DEBUG] Connessione database aperta per LoadSchema."); // Aggiunto log

                // Ottieni la lista delle tabelle
                using (var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tableName = reader.GetString(0);
                        var table = new Table(tableName, database);
                        database.AddTable(table);
                        LoadTableSchema(connection, table); // Carica le informazioni sulle colonne per ogni tabella
                        _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] Caricata tabella: {tableName}"); // Aggiunto log
                    }
                }
            }
            _logger.Log(LogLevel.DEBUG, "[SQLiteSchemaLoader DEBUG] Schema database caricato."); // Aggiunto log
            return database;
        }

        private void LoadTableSchema(SQLiteConnection connection, Table table)
        {
            _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] Caricamento schema per tabella: {table.TableName}"); // Aggiunto log
            using (var command = new SQLiteCommand($"PRAGMA table_info('{table.TableName}')", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    var dataType = reader.GetString(2);
                    var notNull = reader.GetInt32(3) == 1;
                    var isPrimaryKey = reader.GetInt32(5) == 1;
                    bool autoIncrement = dataType.ToUpper().Contains("INTEGER") && isPrimaryKey;

                    var field = new Field(columnName, dataType, isPrimaryKey, autoIncrement, table);
                    table.AddField(field);
                    _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] Aggiunto campo: {columnName} ({dataType}) a {table.TableName}"); // Aggiunto log
                }
            }
        }

        public List<Dictionary<string, object>> LoadTableData(string tableName)
        {
            _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] Caricamento dati per tabella: {tableName}"); // Aggiunto log
            var data = new List<Dictionary<string, object>>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = $"SELECT * FROM `{tableName}`";
                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        data.Add(row);
                    }
                }
            }
            _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] Dati caricati per tabella: {tableName}. Righe: {data.Count}"); // Aggiunto log
            return data;
        }

        // Questo metodo è quello usato da MIUDatabaseManager
        public List<string> SQLiteSelect(string SqlSelect)
        {
            List<string> results = new List<string>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] Esecuzione SELECT: {SqlSelect}"); // Aggiunto log
                    using (var command = new SQLiteCommand(SqlSelect, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                List<string> rowValues = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    rowValues.Add(reader[i].ToString());
                                }
                                results.Add(string.Join(";", rowValues));
                            }
                        }
                    }
                    _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] SELECT completata. Righe: {results.Count}"); // Aggiunto log
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[SQLiteSchemaLoader ERROR] Errore in SQLiteSelect: {ex.Message}. Query: '{SqlSelect}'"); // Aggiunto log
                throw; // Rilancia l'eccezione per gestione a livello superiore
            }
            return results;
        }

        public int SQLiteUpdate(string SqlUpdate)
        {
            int ret = 0;
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] Esecuzione UPDATE: {SqlUpdate}"); // Aggiunto log
                    using (var command = new SQLiteCommand(SqlUpdate, connection))
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        ret = command.ExecuteNonQuery();
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] UPDATE completato. Righe modificate: {ret}"); // Aggiunto log
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[SQLiteSchemaLoader ERROR] Errore in SQLiteUpdate: {ex.Message}. Query: '{SqlUpdate}'"); // Aggiunto log
                throw;
            }
            return ret;
        }

        public long SQLiteInsert(string SqlInsert)
        {
            long newRowId = -1;
            try
            {
                using (var connection = new SQLiteConnection(this._connectionString))
                {
                    connection.Open();
                    _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] Esecuzione INSERT: {SqlInsert}"); // Aggiunto log
                    using (var command = new SQLiteCommand(SqlInsert, connection))
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        command.ExecuteNonQuery();

                        using (var idCommand = new SQLiteCommand("SELECT last_insert_rowid()", connection))
                        {
                            newRowId = (long)idCommand.ExecuteScalar();
                        }
                    }
                    _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] INSERT completato. Nuova riga ID: {newRowId}"); // Aggiunto log
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[SQLiteSchemaLoader ERROR] Errore in SQLiteInsert: {ex.Message}"); // Aggiunto log
                throw;
            }
            return newRowId;
        }

        // Questo metodo sembra non essere utilizzato altrove, ma lo mantengo invariato per non introdurre regressioni.
        public SQLiteDataReader SQLiteSelect(string SqlSelect, out List<string> FieldName)
        {
            _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] Esecuzione SELECT con out FieldName: {SqlSelect}"); // Aggiunto log
            List<string> fieldName = new List<string>();
            SQLiteDataReader request = null;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                SQLiteCommand cmdStructView = new SQLiteCommand()
                {
                    Connection = connection,
                    CommandType = System.Data.CommandType.Text,
                    CommandText = SqlSelect,
                };
                string[] sqlPar = SqlSelect.Split(',');
                foreach (string s in sqlPar)
                {
                    if (s.ToUpper().Contains(select))
                    {
                        int i = s.IndexOf(select) + select.Length;
                        int f = s.Length;
                        fieldName.Add(s.Substring(i, f - i).Trim());
                    }
                    else if (s.ToUpper().Contains(from))
                    {
                        int f = s.IndexOf(from);
                        fieldName.Add(s.Substring(0, f).Trim());
                    }
                    else
                    {
                        fieldName.Add(s.Trim());
                    }
                }
                request = cmdStructView.ExecuteReader();
            }
            FieldName = fieldName;
            _logger.Log(LogLevel.DEBUG, $"[SQLiteSchemaLoader DEBUG] SELECT con out FieldName completata."); // Aggiunto log
            return request;
        }

        public string ConnectionString { get { return this._connectionString; } }
    }
}
