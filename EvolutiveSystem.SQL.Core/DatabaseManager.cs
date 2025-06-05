using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutiveSystem.SQL.Core
{
    /// <summary>
    /// Gestore centrale per la connessione e le transazioni del database SQLite.
    /// Fornisce una connessione persistente e metodi per eseguire comandi SQL
    /// all'interno di transazioni.
    /// </summary>
    public class DatabaseManager : IDisposable
    {
        private readonly string _connectionString;
        private SQLiteConnection _connection;
        private SQLiteTransaction _currentTransaction;

        /// <summary>
        /// Inizializza una nuova istanza di DatabaseManager.
        /// </summary>
        /// <param name="databaseFilePath">Il percorso completo del file database SQLite.</param>
        public DatabaseManager(string databaseFilePath)
        {
            _connectionString = $"Data Source={databaseFilePath};Version=3;";
            Console.WriteLine($"DatabaseManager: Inizializzato con connection string: {_connectionString}");
        }

        /// <summary>
        /// Apre la connessione al database.
        /// </summary>
        public void OpenConnection()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                _connection = new SQLiteConnection(_connectionString);
                _connection.Open();
                Console.WriteLine("DatabaseManager: Connessione al database aperta.");
                // Qui potresti chiamare un metodo per assicurarti che le tabelle esistano,
                // ma per ora lo lascio alla logica di inizializzazione esterna o al repository.
            }
        }

        /// <summary>
        /// Chiude la connessione al database.
        /// </summary>
        public void CloseConnection()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                if (_currentTransaction != null)
                {
                    // Se c'è una transazione attiva, la annulliamo per evitare blocchi
                    Console.WriteLine("DatabaseManager: Rollback della transazione attiva prima di chiudere la connessione.");
                    _currentTransaction.Rollback();
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
                _connection.Close();
                _connection.Dispose();
                _connection = null;
                Console.WriteLine("DatabaseManager: Connessione al database chiusa.");
            }
        }

        /// <summary>
        /// Inizia una nuova transazione. Se una transazione è già attiva, lancia un'eccezione.
        /// </summary>
        /// <returns>La transazione SQLite appena iniziata.</returns>
        public SQLiteTransaction BeginTransaction()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                throw new InvalidOperationException("La connessione al database non è aperta. Chiamare OpenConnection() prima di BeginTransaction().");
            }
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("Una transazione è già attiva. Commit o Rollback prima di iniziarne una nuova.");
            }
            _currentTransaction = _connection.BeginTransaction();
            Console.WriteLine("DatabaseManager: Transazione avviata.");
            return _currentTransaction;
        }

        /// <summary>
        /// Esegue il commit della transazione corrente.
        /// </summary>
        public void CommitTransaction()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("Nessuna transazione attiva da commettere.");
            }
            _currentTransaction.Commit();
            _currentTransaction.Dispose();
            _currentTransaction = null;
            Console.WriteLine("DatabaseManager: Transazione commessa.");
        }

        /// <summary>
        /// Esegue il rollback della transazione corrente.
        /// </summary>
        public void RollbackTransaction()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("Nessuna transazione attiva da annullare.");
            }
            _currentTransaction.Rollback();
            _currentTransaction.Dispose();
            _currentTransaction = null;
            Console.WriteLine("DatabaseManager: Transazione annullata (rollback).");
        }

        /// <summary>
        /// Esegue un comando SQL che non restituisce righe (INSERT, UPDATE, DELETE, CREATE TABLE).
        /// </summary>
        /// <param name="sql">La stringa SQL da eseguire.</param>
        /// <param name="parameters">Un array di parametri SQLite.</param>
        /// <returns>Il numero di righe influenzate.</returns>
        public int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (var command = _connection.CreateCommand())
            {
                command.Transaction = _currentTransaction; // Associa il comando alla transazione corrente
                command.CommandText = sql;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Esegue un comando SQL che restituisce un singolo valore scalare (es. COUNT, ID autoincrementato).
        /// </summary>
        /// <param name="sql">La stringa SQL da eseguire.</param>
        /// <param name="parameters">Un array di parametri SQLite.</param>
        /// <returns>Il valore scalare restituito dal comando.</returns>
        public object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            using (var command = _connection.CreateCommand())
            {
                command.Transaction = _currentTransaction; // Associa il comando alla transazione corrente
                command.CommandText = sql;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Esegue un comando SQL che restituisce un set di risultati (SELECT).
        /// Il chiamante è responsabile di disporre del reader.
        /// </summary>
        /// <param name="sql">La stringa SQL da eseguire.</param>
        /// <param name="parameters">Un array di parametri SQLite.</param>
        /// <returns>Un SQLiteDataReader contenente i risultati.</returns>
        public SQLiteDataReader ExecuteReader(string sql, params SQLiteParameter[] parameters)
        {
            using (var command = _connection.CreateCommand())
            {
                command.Transaction = _currentTransaction; // Associa il comando alla transazione corrente
                command.CommandText = sql;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                // CommandBehavior.CloseConnection assicura che la connessione venga chiusa
                // quando il reader viene chiuso o disposto.
                // Tuttavia, dato che la connessione è gestita dal DatabaseManager,
                // è meglio non usare CloseConnection qui per mantenerla aperta per altre operazioni.
                return command.ExecuteReader();
            }
        }

        /// <summary>
        /// Implementazione di IDisposable per garantire la chiusura della connessione.
        /// </summary>
        public void Dispose()
        {
            CloseConnection();
        }
    }
}
