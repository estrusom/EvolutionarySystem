using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class Table
    {
        public string TableName { get; set; }
        public List<Field> Fields { get; set; } = new List<Field>();
        public Database ParentDatabase { get; set; } // Riferimento al database padre
                                                     // Potremmo anche tenere qui una lista dei dati (record) della tabella in memoria,
                                                     // ma per grandi tabelle sarebbe inefficiente. Potremmo caricarli on-demand.
                                                     // public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();

        public Table() { }

        public Table(string tableName, Database parentDatabase)
        {
            TableName = tableName;
            ParentDatabase = parentDatabase;
        }

        public void AddField(Field field)
        {
            if (field != null)
            {
                Fields.Add(field);
                field.ParentTable = this;
                field.TableName = this.TableName;
            }
        }
    }

    public class Database
    {
        public string DatabaseName { get; set; }
        public string FilePath { get; set; } // Percorso del file SQLite
        public List<Table> Tables { get; set; } = new List<Table>();

        public Database() { }

        public Database(string databaseName, string filePath)
        {
            DatabaseName = databaseName;
            FilePath = filePath;
        }

        public void AddTable(Table table)
        {
            if (table != null)
            {
                Tables.Add(table);
                table.ParentDatabase = this;
            }
        }
    }

    public class SQLiteSchemaLoader
    {
        const string select = "SELECT";
        const string from = "FROM";

        private readonly string _connectionString;

        public SQLiteSchemaLoader(string databaseFilePath)
        {
            _connectionString = $"Data Source={databaseFilePath};Version=3;";
        }

        public Database LoadSchema()
        {
            var database = new Database(System.IO.Path.GetFileNameWithoutExtension(_connectionString.Split(';')[0].Split('=')[1]), _connectionString.Split(';')[0].Split('=')[1]);

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

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
                    }
                }
            }

            return database;
        }
        private void LoadTableSchema(SQLiteConnection connection, Table table)
        {
            using (var command = new SQLiteCommand($"PRAGMA table_info('{table.TableName}')", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var columnName = reader.GetString(1);
                    var dataType = reader.GetString(2);
                    var notNull = reader.GetInt32(3) == 1;
                    var isPrimaryKey = reader.GetInt32(5) == 1;
                    // SQLite non ha un flag booleano diretto per l'autoincremento in PRAGMA table_info.
                    // Spesso è dedotto dal tipo INTEGER PRIMARY KEY.
                    bool autoIncrement = dataType.ToUpper().Contains("INTEGER") && isPrimaryKey;

                    var field = new Field(columnName, dataType, isPrimaryKey, autoIncrement, table);
                    table.AddField(field);
                }
            }
        }
        public List<Dictionary<string, object>> LoadTableData(string tableName)
        {
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
            return data;
        }
        public List<string> SQLiteSelect(string SqlSelect)
        {
            List<string> fieldName = new List<string>();
            List<string> outPut = new List<string>();

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
                foreach(string s in sqlPar)
                {
                    if (s.ToUpper().Contains(select))
                    {
                        int i = s.IndexOf(select) + select.Length;
                        int f = s.Length;
                        fieldName.Add(s.Substring(i, f - i).Trim());
                    }
                    else if(s.ToUpper().Contains(from))
                    {
                        int f = s.IndexOf(from);
                        fieldName.Add(s.Substring(0, f).Trim());
                    }
                    else
                    {
                        fieldName.Add(s.Trim());
                    }
                }
                SQLiteDataReader request = cmdStructView.ExecuteReader();
                object[] structView = new object[fieldName.Count];
                StringBuilder sb = new StringBuilder();
                while (request.Read()) 
                {
                    int r = request.GetValues(structView);
                    for (int l = 0; l < structView.Count(); l++)
                    {
                        sb.Append($"{structView[l]};");
                    }
                    int i = sb.ToString().LastIndexOf(';');
                    outPut.Add(sb.ToString().Substring(0,i-1));
                    sb.Clear();
                }
            }
            return outPut;
        }
        public int SQLiteUpdate(string SqlUpdate)
        {
            int ret = 0;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                SQLiteCommand cmdUpdate = new SQLiteCommand(SqlUpdate, connection)
                {
                    CommandType = System.Data.CommandType.Text
                };
                ret = cmdUpdate.ExecuteNonQuery();
                return ret;
            }
        }
        public SQLiteDataReader SQLiteSelect(string SqlSelect, out List<string> FieldName)
        {
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
            return request;
        }
        public string ConnectionString { get { return this._connectionString; } }
    }
}
