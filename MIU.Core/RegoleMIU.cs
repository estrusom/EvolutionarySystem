using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MIU.Core
{
    /// <summary>
    /// Gestisce il caricamento e l'accesso alle regole MIU.
    /// </summary>
    public static class RegoleMIUManager
    {
        // Questa è la lista privata dove vengono memorizzate le regole caricate.
        private static List<RegolaMIU> _regole = new List<RegolaMIU>();

        // Espone le regole caricate come una lista di sola lettura
        public static IReadOnlyList<RegolaMIU> Regole => _regole.AsReadOnly();

        /// <summary>
        /// Carica le regole MIU da un oggetto Database già popolato.
        /// Questo metodo si aspetta che l'oggetto Database sia già stato caricato
        /// dal file XML (es. MIUProject.xml) utilizzando le tue classi esistenti.
        /// </summary>
        /// <param name="database">L'istanza dell'oggetto Database contenente i dati.</param>
        public static void CaricaRegoleDaOggettoDatabase(Database database)
        {
            if (database == null)
            {
                Console.WriteLine("L'oggetto Database fornito è nullo. Impossibile caricare le regole MIU.");
                _regole.Clear();
                return;
            }

            _regole.Clear(); // Pulisci le regole esistenti prima di caricare le nuove

            // Trova la tabella "RegoleMIU" all'interno dell'oggetto Database
            Table regoleMIUTable = database.Tables.FirstOrDefault(t => t.TableName == "RegoleMIU");

            if (regoleMIUTable == null)
            {
                Console.WriteLine("Tabella 'RegoleMIU' non trovata nell'oggetto Database fornito.");
                return;
            }

            if (regoleMIUTable.DataRecords == null || !regoleMIUTable.DataRecords.Any())
            {
                Console.WriteLine("Nessun record di dati trovato nella tabella 'RegoleMIU'.");
                return;
            }

            // Itera su ogni record di dati (SerializableDictionaryOfStringObject)
            foreach (var record in regoleMIUTable.DataRecords)
            {
                // Estrai i valori usando il metodo GetValue del SerializableDictionaryOfStringObject
                string id = record.GetValue("ID");
                string nome = record.GetValue("Nome");
                string descrizione = record.GetValue("Descrizione");
                string pattern = record.GetValue("Pattern");
                string sostituzione = record.GetValue("Sostituzione");

                // Aggiungi la regola solo se i campi essenziali sono presenti
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(nome))
                {
                    _regole.Add(new RegolaMIU(id, nome, descrizione, pattern, sostituzione));
                }
                else
                {
                    Console.WriteLine($"Avviso: Record incompleto trovato nella tabella 'RegoleMIU'. ID: {id ?? "N/A"}, Nome: {nome ?? "N/A"}. Questo record è stato saltato.");
                }
            }

            Console.WriteLine($"Caricate {_regole.Count} regole MIU dall'oggetto Database.");
        }

        /// <summary>
        /// Applica tutte le regole MIU caricate a una stringa di input.
        /// Ogni regola viene applicata sequenzialmente.
        /// </summary>
        /// <param name="input">La stringa a cui applicare le regole.</param>
        /// <returns>La stringa risultante dopo l'applicazione di tutte le regole.</returns>
        public static string ApplicaRegole(string input)
        {
            string result = input;
            Console.WriteLine($"\nApplicazione regole a: '{input}'");

            if (!_regole.Any())
            {
                Console.WriteLine("Nessuna regola MIU caricata. La stringa non verrà modificata.");
                return input;
            }

            foreach (var rule in _regole)
            {
                if (!string.IsNullOrEmpty(rule.Pattern))
                {
                    try
                    {
                        // Utilizza Regex.Replace per applicare il pattern e la sostituzione
                        // La sostituzione può essere null, in quel caso Replace rimuove il match
                        string oldResult = result;
                        result = Regex.Replace(result, rule.Pattern, rule.Sostituzione ?? "");
                        if (oldResult != result)
                        {
                            Console.WriteLine($"  Regola '{rule.Nome}' (Pattern: '{rule.Pattern}', Sostituzione: '{rule.Sostituzione ?? "NULL"}') applicata.");
                            Console.WriteLine($"    Risultato parziale: '{result}'");
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"  Errore nella regola '{rule.Nome}': Pattern '{rule.Pattern}' non valido. {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Errore generico nell'applicazione della regola '{rule.Nome}': {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"  Avviso: Regola '{rule.Nome}' ha un pattern vuoto. Saltata.");
                }
            }
            Console.WriteLine($"Risultato finale dopo l'applicazione delle regole: '{result}'");
            return result;
        }
    }
    // ====================================================================================
    // CLASSE PER LA SERIALIZZAZIONE/DESERIALIZZAZIONE DEL DATABASE (IL TUO DatabaseSerializer)
    // Questa è una versione semplificata per l'esempio. Tu userai la tua.
    // ====================================================================================

    public static class DatabaseSerializer
    {
        public static Database DeserializeFromXmlFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Errore: Il file XML '{filePath}' non è stato trovato.");
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Database));
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    // Aggiungi qui eventuali logica di validazione o gestione degli errori
                    // che il tuo DatabaseSerializer originale potrebbe avere.
                    return (Database)serializer.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la deserializzazione del Database da XML: {ex.Message}");
                Console.WriteLine(ex.ToString()); // Per dettagli di debug
                return null;
            }
        }
    }
    /// <summary>
    /// Fornisce un metodo per caricare le regole MIU da un file XML.
    /// </summary>
    public class MiuRulesLoader
    {
        /// <summary>
        /// Carica le regole MIU dal file XML specificato.
        /// </summary>
        /// <param name="xmlFilePath">Il percorso completo del file XML contenente il database.</param>
        /// <returns>True se il caricamento è avvenuto con successo, altrimenti False.</returns>
        public bool LoadMiuRulesFromFile(string xmlFilePath)
        {
            Console.WriteLine($"Tentativo di caricare le regole MIU da: {xmlFilePath}");

            // PASSO 1: Carica il database dall'XML usando il tuo DatabaseSerializer
            Database loadedDb = DatabaseSerializer.DeserializeFromXmlFile(xmlFilePath);

            if (loadedDb == null)
            {
                Console.WriteLine("Caricamento del database fallito. Le regole MIU non sono state caricate.");
                return false;
            }

            Console.WriteLine($"Database '{loadedDb.DatabaseName}' caricato con successo da XML.");

            // PASSO 2: Passa l'oggetto Database caricato al RegoleMIUManager per estrarre le regole
            RegoleMIUManager.CaricaRegoleDaOggettoDatabase(loadedDb);

            // Verifica se delle regole sono state effettivamente caricate
            if (RegoleMIUManager.Regole.Any())
            {
                Console.WriteLine($"Caricamento delle regole MIU completato. Numero di regole: {RegoleMIUManager.Regole.Count}");
                return true;
            }
            else
            {
                Console.WriteLine("Nessuna regola MIU è stata trovata o caricata dal database.");
                return false;
            }
        }
    }

}
