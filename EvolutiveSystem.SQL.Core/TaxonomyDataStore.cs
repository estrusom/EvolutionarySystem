// File: EvolutiveSystem.SQL.Data/TaxonomyDataStore.cs
// Data di riferimento: 2 settembre 2025
// Questa classe implementa l'interfaccia IRuleTaxonomyDataStore.
// La sua responsabilità è gestire la persistenza dei dati della tassonomia in un database.
// Le tabelle del database devono essere create esternamente tramite script SQL.

using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using EvolutiveSystem.SQL.Core;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization; // Aggiungiamo i riferimenti necessari per la serializzazione XML
using System.IO;

namespace EvolutiveSystem.SQL.Data
{
    /// <summary>
    /// Implementazione concreta per la persistenza della tassonomia.
    /// Si aspetta che lo schema del database sia già presente.
    /// </summary>
    public class TaxonomyDataStore : IRuleTaxonomyDataStore
    {
        private readonly string _connectionString;

        public TaxonomyDataStore(string connectionString)
        {
            _connectionString = connectionString;
            // La creazione delle tabelle è gestita da script SQL esterni.
        }

        /// <summary>
        /// Salva la tassonomia generata nel database.
        /// Serializza l'oggetto tassonomia in una stringa XML prima di salvarlo.
        /// Questo mantiene il disaccoppiamento tra i progetti e supporta il formato XML.
        /// </summary>
        /// <param name="taxonomy">L'oggetto tassonomia da salvare.</param>
        public async Task SaveTaxonomyAsync(object taxonomy)
        {
            if (taxonomy == null)
            {
                throw new ArgumentNullException(nameof(taxonomy));
            }

            // Serializza l'oggetto taxonomy in XML
            string taxonomyXml;
            try
            {
                var serializer = new XmlSerializer(taxonomy.GetType());
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, taxonomy);
                    taxonomyXml = writer.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Impossibile serializzare l'oggetto tassonomia in XML.", ex);
            }

            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync();

                string insertSql = @"
                    INSERT INTO RuleTaxonomy (Data, LastGenerated)
                    VALUES (@Data, @LastGenerated);";

                using (var cmd = new SQLiteCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Data", taxonomyXml);
                    cmd.Parameters.AddWithValue("@LastGenerated", DateTime.UtcNow.ToString("o"));

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
