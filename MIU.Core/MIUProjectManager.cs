using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{

    /// <summary>
    /// Gestisce il caricamento del progetto MIU, inclusi i dati del database e le regole.
    /// </summary>
    public class MIUProjectManager
    {
        public EvolutiveSystem.Core.Database CurrentDatabase { get; private set; }

        /// <summary>
        /// Carica il progetto MIU da un file XML.
        /// </summary>
        /// <param name="filePath">Il percorso del file XML del progetto MIU.</param>
        /// <returns>True se il caricamento ha avuto successo, altrimenti false.</returns>
        public bool LoadMIUProject(string filePath)
        {
            try
            {
                // Usa il DatabaseSerializer di EvolutiveSystem.Core per caricare il database
                CurrentDatabase = EvolutiveSystem.Core.DatabaseSerializer.DeserializeFromXmlFile(filePath);
                if (CurrentDatabase != null)
                {
                    CurrentDatabase.FilePath = filePath; // Imposta il percorso del file
                    Console.WriteLine($"Progetto MIU caricato con successo da: {filePath}");

                    // Carica le regole MIU nell'apposito manager
                    RegoleMIUManager.CaricaRegoleDaOggettoDatabase(CurrentDatabase);

                    return true;
                }
                Console.WriteLine($"Errore: Impossibile caricare il database da {filePath}.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il caricamento del progetto MIU: {ex.Message}");
                Console.WriteLine(ex.ToString()); // Per dettagli di debug
                return false;
            }
        }

        /// <summary>
        /// Esempio di funzione del gioco MIU che usa il database caricato.
        /// </summary>
        public void RunMIULogic()
        {
            if (CurrentDatabase == null)
            {
                Console.WriteLine("Nessun database MIU caricato. Carica un progetto prima.");
                return;
            }

            Console.WriteLine($"Esecuzione logica MIU per il database: {CurrentDatabase.DatabaseName}");

            // Esempio: Accedere ai dati del database
            foreach (var table in CurrentDatabase.Tables)
            {
                Console.WriteLine($"  Tabella: {table.TableName}");
                foreach (var field in table.Fields)
                {
                    Console.WriteLine($"    Campo: {field.FieldName}, Tipo: {field.DataType}, Valore: {field.Value}");
                }
                foreach (var record in table.DataRecords)
                {
                    Console.WriteLine("    Record:");
                    foreach (var kvp in record)
                    {
                        Console.WriteLine($"      {kvp.Key}: {kvp.Value}");
                    }
                }
            }

            // Esempio: Applicare le regole MIU
            string initialString = "MIU"; // Stringa di esempio
            string resultString = RegoleMIUManager.ApplicaRegole(initialString);
            Console.WriteLine($"Stringa finale dopo le regole: {resultString}");

            // Qui integrerai la tua logica esistente del gioco MIU,
            // utilizzando CurrentDatabase, le sue Tables, Fields e DataRecords,
            // e le regole caricate tramite RegoleMIUManager.Regole.
            // Ad esempio:
            // var postulatesTable = CurrentDatabase.Tables.FirstOrDefault(t => t.TableName == "Postulates");
            // if (postulatesTable != null) { /* elabora i postulati */ }
        }
    }
}
