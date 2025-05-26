using EvolutiveSystem.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
// 2025.05.24 aggiunto tempo ms 
// 2025.05.26 Modificato inserimento record con l'aggiornamento dell'indice
namespace MIU.Core
{

    public static partial class RegoleMIUManager // Usiamo partial per poter estendere la classe in più file se necessario
    {
        //private static int _esplorazioneIdCounter = 1; // Contatore statico per l'ID di esplorazione
        // Questa è la lista privata dove vengono memorizzate le regole caricate.
        private static List<RegolaMIU> _regole = new List<RegolaMIU>();

        // Espone le regole caricate come una lista di sola lettura
        public static IReadOnlyList<RegolaMIU> Regole => _regole.AsReadOnly();

        /// <summary>
        /// Carica le regole MIU da un oggetto Database già popolato (di tipo EvolutiveSystem.Core.Database).
        /// Questo metodo si aspetta che l'oggetto Database sia già stato caricato
        /// dal file XML (es. MIUProject.xml) utilizzando il DatabaseSerializer di EvolutiveSystem.Core.
        /// </summary>
        /// <param name="database">L'istanza dell'oggetto Database contenente i dati.</param>
        public static void CaricaRegoleDaOggettoDatabase(EvolutiveSystem.Core.Database database) // Modificato il tipo del parametro
        {
            if (database == null)
            {
                Console.WriteLine("L'oggetto Database fornito è nullo. Impossibile caricare le regole MIU.");
                _regole.Clear();
                return;
            }

            _regole.Clear(); // Pulisci le regole esistenti prima di caricare le nuove

            // Trova la tabella "RegoleMIU" all'interno dell'oggetto Database
            EvolutiveSystem.Core.Table regoleMIUTable = database.Tables.FirstOrDefault(t => t.TableName == "RegoleMIU");

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

            // Itera su ogni record di dati (EvolutiveSystem.Core.SerializableDictionary<string, object>)
            foreach (var record in regoleMIUTable.DataRecords)
            {
                // Estrai i valori accedendo direttamente al dizionario.
                // È importante gestire il casting da 'object' a 'string' e i valori null.
                string id = record.ContainsKey("ID") ? record["ID"]?.ToString() : null;
                string nome = record.ContainsKey("Nome") ? record["Nome"]?.ToString() : null;
                string descrizione = record.ContainsKey("Descrizione") ? record["Descrizione"]?.ToString() : null;
                string pattern = record.ContainsKey("Pattern") ? record["Pattern"]?.ToString() : null;
                string sostituzione = record.ContainsKey("Sostituzione") ? record["Sostituzione"]?.ToString() : null;

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

            // Inizializza _esplorazioneIdCounter leggendo l'ultimo ID dalla tabella EsplorazioneMIU
            //InizializzaEsplorazioneIdCounter(database);
        }
        ///// <summary>
        ///// Inizializza il contatore degli ID per le esplorazioni leggendo l'ID massimo dalla tabella 'EsplorazioneMIU'
        ///// e impostando il contatore al valore massimo + 1.
        ///// </summary>
        ///// <param name="database">L'oggetto Database contenente la tabella 'EsplorazioneMIU'.</param>
        //private static void InizializzaEsplorazioneIdCounter(Database database)
        //{
        //    if (database != null && database.Tables.Any(t => t.TableName == "EsplorazioneMIU"))
        //    {
        //        var tabellaEsplorazione = database.Tables.First(t => t.TableName == "EsplorazioneMIU");
        //        if (tabellaEsplorazione.DataRecords != null && tabellaEsplorazione.DataRecords.Any())
        //        {
        //            ulong maxId = 0;
        //            foreach (var record in tabellaEsplorazione.DataRecords)
        //            {
        //                if (record.ContainsKey("ID") && record["ID"] is IConvertible)
        //                {
        //                    ulong currentId;
        //                    try
        //                    {
        //                        currentId = Convert.ToUInt64(record["ID"]);
        //                        if (currentId > maxId)
        //                        {
        //                            maxId = currentId;
        //                        }
        //                    }
        //                    catch (FormatException)
        //                    {
        //                        Console.WriteLine("Avviso: Trovato un ID non valido nella tabella EsplorazioneMIU. Questo record verrà ignorato per il calcolo dell'ID massimo.");
        //                    }
        //                }
        //            }
        //            _esplorazioneIdCounter = (int)(maxId + 1);
        //            Console.WriteLine($"Contatore ID esplorazione inizializzato a: {_esplorazioneIdCounter}");
        //        }
        //        else
        //        {
        //            Console.WriteLine("Tabella 'EsplorazioneMIU' vuota. Il contatore ID esplorazione rimane a 1.");
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("Tabella 'EsplorazioneMIU' non trovata. Il contatore ID esplorazione rimane a 1.");
        //    }
        //}
        /// <summary>
        /// Applica tutte le regole MIU caricate a una stringa di input, iterando su di esse.
        /// </summary>
        /// <param name="input">La stringa a cui applicare le regole.</param>
        /// <returns>La stringa risultante dopo l'applicazione delle regole.</returns>
        public static string ApplicaRegole(string input)
        {
            string currentString = input; // Inizializza currentString qui
            Console.WriteLine($"\nApplicazione regole a: '{input}'");

            if (!_regole.Any())
            {
                Console.WriteLine("Nessuna regola MIU caricata. La stringa non verrà modificata.");
                return input;
            }

            // Iteriamo su ogni regola e tentiamo di applicarla.
            // Puoi decidere se applicare ogni regola una sola volta o iterare finché non ci sono più cambiamenti.
            // Per ora, applichiamo ogni regola una volta in sequenza.
            foreach (var rule in _regole)
            {
                string oldString = currentString;
                string newString; // Dichiarata qui per l'out parameter

                if (rule.TryApply(currentString, out newString))
                {
                    currentString = newString; // Aggiorna la stringa corrente se la regola è stata applicata
                    Console.WriteLine($"  Regola '{rule.Nome}' (Pattern: '{rule.Pattern}', Sostituzione: '{rule.Sostituzione ?? "NULL"}') applicata.");
                    Console.WriteLine($"    Risultato parziale: '{currentString}'");
                }
                else
                {
                    // La regola non è stata applicata o il pattern non ha trovato corrispondenze
                    Console.WriteLine($"  Regola '{rule.Nome}' non applicata a '{oldString}'.");
                }
            }

            Console.WriteLine($"Risultato finale dopo l'applicazione delle regole: '{currentString}'");
            return currentString;
        }

        /// <summary>
        /// Esegue una ricerca in ampiezza (BFS) per trovare una derivazione
        /// dalla stringa iniziale alla stringa target usando le regole MIU.
        /// </summary>
        /// <param name="start">La stringa iniziale.</param>
        /// <param name="target">La stringa target.</param>
        /// <param name="maxDepth">La profondità massima della ricerca per evitare cicli infiniti.</param>
        /// <returns>Una lista di stringhe che rappresentano il percorso di derivazione,
        /// o null se la stringa target non viene raggiunta entro la profondità massima.</returns>
        public static List<string> TrovaDerivazioneBFS(string start, string target, long maxSteps = 100, Database database = null)
        {
            double frequency = Stopwatch.Frequency; // Ottieni la frequenza una volta per calcolo tempo ms
            if (start == target)
            {
                return new List<string> { start };
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            Queue<List<string>> queue = new Queue<List<string>>();
            queue.Enqueue(new List<string> { start });
            HashSet<string> visited = new HashSet<string> { start };
            List<string> solutionPath = null;
            int depth = 0;
            long elapsedTicks = 0; // Dichiarazione qui

            while (queue.Count > 0 && depth < maxSteps)
            {
                int levelSize = queue.Count;
                depth++;

                for (int i = 0; i < levelSize; i++)
                {
                    List<string> currentPath = queue.Dequeue();
                    string currentString = currentPath.Last();
                    foreach (var rule in Regole)
                    {
                        string nextString;
                        if (rule.TryApply(currentString, out nextString) && !visited.Contains(nextString))
                        {
                            if (nextString == target)
                            {
                                solutionPath = new List<string>(currentPath);
                                solutionPath.Add(nextString);
                                stopwatch.Stop();
                                elapsedTicks = stopwatch.ElapsedTicks;
                                double tempoMs = (elapsedTicks / frequency) * 1000; // Calcola il tempo in ms
                                int numeroPassi = solutionPath.Count - 1;
                                bool soluzioneTrovata = true;
                                Console.WriteLine($"\nSoluzione trovata a profondità {depth} in {tempoMs:F2} ms:");
                                foreach (var s in solutionPath)
                                {
                                    Console.WriteLine($"  {s}");
                                }
                                // Memorizza il risultato nel database se fornito (senza controllo duplicati)
                                if (database != null && database.Tables.Any(t => t.TableName == "EsplorazioneMIU"))
                                {
                                    var tabellaEsplorazione = database.Tables.First(t => t.TableName == "EsplorazioneMIU");
                                    var record = new SerializableDictionary<string, object>
                                {
                                    //{ "ID", _esplorazioneIdCounter++ }, // Autoincremento ID
                                    { "StringaIniziale", start },
                                    { "StringaTarget", target },
                                    { "NumeroPassi", numeroPassi },
                                    { "LimitePassi", maxSteps },
                                    { "TicOrologio",(ulong) elapsedTicks },
                                    { "TempoMs", tempoMs }, // Salva il tempo in ms
                                    { "SoluzioneTrovata", soluzioneTrovata }
                                };
                                    //tabellaEsplorazione.DataRecords.Add(record);  //<- è questo che devo modificare?
                                    // *** MODIFICA QUI: Usa AddRecord per l'autoincremento ***
                                    tabellaEsplorazione.AddRecord(record); //2025.05.26 così inserisco un record e incremento l'indice

                                    //Console.WriteLine($"Risultato BFS da '{start}' a '{target}' memorizzato (ID: {_esplorazioneIdCounter - 1}).");
                                }
                                return solutionPath;
                            }

                            visited.Add(nextString);
                            List<string> newPath = new List<string>(currentPath);
                            newPath.Add(nextString);
                            queue.Enqueue(newPath);
                        }
                    }
                }
            }

            stopwatch.Stop();
            elapsedTicks = stopwatch.ElapsedTicks;
            double tempoMsFallimento = (elapsedTicks / frequency) * 1000; // Calcola il tempo in ms per il fallimento
            int numeroPassiFinale = (int)maxSteps;
            bool soluzioneTrovataFinale = solutionPath != null;

            Console.WriteLine($"\nStringa target '{target}' non raggiunta entro la profondità massima di {maxSteps} in {tempoMsFallimento:F2} ms.");
            if (database != null && database.Tables.Any(t => t.TableName == "EsplorazioneMIU"))
            {
                var tabellaEsplorazione = database.Tables.First(t => t.TableName == "EsplorazioneMIU");
                // Memorizza il risultato (fallito) nel database se fornito (senza controllo duplicati)
                var recordFallimento = new SerializableDictionary<string, object>
            {
                //{ "ID", _esplorazioneIdCounter++ }, // Autoincremento ID
                { "StringaIniziale", start },
                { "StringaTarget", target },
                { "NumeroPassi", numeroPassiFinale },
                { "LimitePassi", maxSteps },
                { "TicOrologio", (ulong)elapsedTicks },
                { "TempoMs", tempoMsFallimento }, // Salva il tempo in ms per il fallimento
                { "SoluzioneTrovata", soluzioneTrovataFinale }
            };
                tabellaEsplorazione.AddRecord(recordFallimento); //2025.05.26 così inserisco un record e incremento l'indice
                //Console.WriteLine($"Risultato BFS (fallito) da '{start}' a '{target}' memorizzato (ID: {_esplorazioneIdCounter - 1}).");
            }
            return null;
        }
    }
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
