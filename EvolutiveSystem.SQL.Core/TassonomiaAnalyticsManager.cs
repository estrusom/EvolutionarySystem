// File: EvolutiveSystem.SQL.Core/TassonomiaAnalyticsManager.cs
// Data di riferimento: 24 giugno 2025
// Implementazione della logica per l'analisi della tassonomia del sistema MIU.

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using MasterLog;
//using MIU.Core;
using EvolutiveSystem.Common;
using System.Data;

namespace EvolutiveSystem.SQL.Core
{
    // La classe manager implementa l'interfaccia con il nome corretto
    public class TassonomiaAnalyticsManager : ITassonomiaAnalytics
    {
        private readonly string _databasePath;
        private readonly MasterLog.Logger _logger;

        public TassonomiaAnalyticsManager(string databasePath, Logger logger)
        {
            _databasePath = databasePath;
            _logger = logger;
        }

        /// <summary>
        /// Esegue la query di tassonomia e restituisce i risultati.
        /// </summary>
        /// <returns>Una lista di dizionari, dove ogni dizionario rappresenta una riga dei risultati.</returns>
        public async Task<List<Dictionary<string, object>>> GetTaxonomyResultsAsync()
        {
            var results = new List<Dictionary<string, object>>();

            try
            {
                // La query viene recuperata da una classe statica dedicata,
                // mantenendo la separazione tra logica e dati.
                string query = TassonomiaQueries.TassonomiaQuery;
                _logger.Log(LogLevel.DEBUG, $"[TassonomiaAnalyticsManager] Esecuzione query di tassonomia: {query}");

                using (var connection = new SQLiteConnection($"Data Source={_databasePath};Version=3;"))
                {
                    await connection.OpenAsync();
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        // Qui avviene la vera e propria esecuzione della query
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                }
                                results.Add(row);
                            }
                        }
                    }
                }
                _logger.Log(LogLevel.DEBUG, $"[TassonomiaAnalyticsManager] Trovate {results.Count} righe per la tassonomia.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[TassonomiaAnalyticsManager] Errore durante l'esecuzione della query di tassonomia: {ex.Message}");
                // Ritorna una lista vuota in caso di errore
                return new List<Dictionary<string, object>>();
            }
            return results;
        }
    }
}
