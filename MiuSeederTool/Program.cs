// File: MiuSeederTool/Program.cs
// Data di riferimento: 07 luglio 2025 (Aggiornamento per menu semplificato)
// Descrizione: Punto di ingresso dell'applicazione per il tool di seeding MIU.
// Aggiornato per consolidare i comandi 'initdb_bfs' e 'initdb_dfs' in un unico 'initdb'.

using MasterLog;
using MiuSeederTool.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace MiuSeederTool
{
    class Program
    {
        private static SeederDbAccess _dbAccess;
        private static MiuStringHelper _miuStringHelper;
        private static MiuDerivationSeeder _miuDerivationSeeder;
        private static Logger _logger;

        private static string _databaseFilePath;
        private static int _maxStringLength;
        private static int _targetDerivableCountInitDb;
        private static int _maxStringsToGenerateForSeeding;

        static async Task Main(string[] args)
        {
            Console.WriteLine("--- PROGRAMMA AVVIATO: Inizializzazione ---");

            LoadConfiguration();

            string logDirectoryPath = AppDomain.CurrentDomain.BaseDirectory + "\\logs";

            _logger = new Logger(logDirectoryPath, "MiuSeederToolLog");
            _logger.SwLogLevel = _logger.LOG_INFO | _logger.LOG_DEBUG | _logger.LOG_WARNING | _logger.LOG_ERROR | _logger.LOG_SOCKET | _logger.LOG_SERVICE | _logger.LOG_SERVICE_EVENT | _logger.LOG_ENANCED_DEBUG | _logger.LOG_INTERNAL_TEST;

            _logger.Log(LogLevel.INFO, "MiuSeederTool Application avviata. Logger inizializzato.", false);
            Console.WriteLine($"Logger inizializzato. File di log atteso in: {Path.Combine(logDirectoryPath, _logger.GetNome())}");

            _dbAccess = new SeederDbAccess(_databaseFilePath, _logger);
            _logger.Log(LogLevel.INFO, "SeederDbAccess inizializzato.", false);

            // Carica le regole e inizializza MiuStringHelper
            var miuRules = await _dbAccess.LoadRegoleMIUAsync();
            _logger.Log(LogLevel.INFO, $"Caricate {miuRules.Count()} regole MIU dal database.", false);

            _miuStringHelper = new MiuStringHelper(miuRules, _logger);
            _logger.Log(LogLevel.INFO, "MiuStringHelper inizializzato.", false);

            _miuDerivationSeeder = new MiuDerivationSeeder(_dbAccess, _miuStringHelper, _logger, _maxStringLength);
            _logger.Log(LogLevel.INFO, "MiuDerivationSeeder inizializzato.", false);


            Console.WriteLine($"Config: DatabaseFilePath = {_databaseFilePath}");
            Console.WriteLine($"Config: MaxStringLength = {_maxStringLength}");
            Console.WriteLine($"Config: TargetDerivableCountInitDb = {_targetDerivableCountInitDb}");
            Console.WriteLine($"Config: MaxStringsToGenerateForSeeding = {_maxStringsToGenerateForSeeding}");

            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
                _logger.Log(LogLevel.INFO, "Operazione annullata dall'utente.", false);
                Console.WriteLine("Operazione annullata. Attendere il completamento...");
            };

            while (true)
            {
                // *** MENU AGGIORNATO E SEMPLIFICATO ***
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("MiuSeederTool - Strumento per il Seeding del Database MIU");
                Console.WriteLine("Comandi:");
                Console.WriteLine("  'initdb <num_total_strings> <max_derivation_depth>'");
                Console.WriteLine($"    (Svuota e ripopola il DB con un mix di stringhe derivabili (BFS/DFS) e casuali)");
                Console.WriteLine($"    Esempio: 'initdb 1000 300'");
                Console.WriteLine("      - num_total_strings: Numero totale di stringhe da generare nel database.");
                Console.WriteLine("      - max_derivation_depth: Profondità massima per la generazione delle stringhe derivabili e di soluzione.");
                Console.WriteLine("  'add_derivable <num_to_add> <max_derivation_depth>'");
                Console.WriteLine($"    (Aggiunge a quelle esistenti) Esempio: 'add_derivable 150 10' (Aggiunge fino a 150 stringhe derivabili, max lunghezza 60)");
                Console.WriteLine("      - num_to_add: Numero di stringhe derivabili da tentare di aggiungere.");
                Console.WriteLine("      - max_derivation_depth: Profondità massima per la generazione delle nuove stringhe derivabili.");
                Console.WriteLine("  'seed <num_records_to_seed> <generation_depth>'");
                Console.WriteLine("    Esempio: 'seed 333 10'");
                Console.WriteLine("      - num_records_to_seed: Numero di record esistenti da aggiornare con una stringa derivabile (NON modifica SeedingType).");
                Console.WriteLine("      - generation_depth: Profondità a cui generare la nuova stringa derivata (es. 5-15).");
                Console.WriteLine("  'exit' - Esce dall'applicazione.");
                Console.WriteLine("------------------------------------------");

                Console.Write("> ");
                string commandLine = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(commandLine)) continue;

                string[] parts = commandLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToLower();

                try
                {
                    switch (command)
                    {
                        case "initdb": // Unico case per l'inizializzazione completa
                            if (parts.Length != 3)
                            {
                                Console.WriteLine($"Uso: {command} <num_total_strings> <max_derivation_depth>");
                                break;
                            }
                            int numTotalStrings = int.Parse(parts[1]);
                            int maxDerivationDepth = int.Parse(parts[2]);

                            Console.WriteLine("Svuotamento della tabella MIU_States...");
                            await _dbAccess.ClearMiuStatesTableAsync();
                            // Ora passiamo un algoritmo arbitrario (es. BFS) poiché la logica interna di InitializeFullDatabaseAsync
                            // gestisce la combinazione BFS/DFS per i SolutionPath.
                            await _miuDerivationSeeder.InitializeFullDatabaseAsync(numTotalStrings, maxDerivationDepth, cancellationTokenSource.Token, PathFindingAlgorithm.BFS);
                            Console.WriteLine("Inizializzazione completata. Controlla il database per i tipi di seeding.");
                            break;

                        case "add_derivable":
                            if (parts.Length != 3)
                            {
                                Console.WriteLine("Uso: add_derivable <num_to_add> <max_derivation_depth>");
                                break;
                            }
                            int numToAdd = int.Parse(parts[1]);
                            int addDerivationDepth = int.Parse(parts[2]);
                            Console.WriteLine("Funzionalità 'add_derivable' non ancora implementata come comando separato.");
                            break;

                        case "seed":
                            if (parts.Length != 3)
                            {
                                Console.WriteLine("Uso: seed <num_records_to_seed> <generation_depth>");
                                break;
                            }
                            int numRecordsToSeed = int.Parse(parts[1]);
                            int generationDepth = int.Parse(parts[2]);
                            Console.WriteLine("Funzionalità 'seed' non più supportata direttamente. Usa 'initdb' per ripopolare.");
                            break;

                        case "exit":
                            Console.WriteLine("Uscita dall'applicazione.");
                            return;

                        default:
                            Console.WriteLine("Comando non riconosciuto.");
                            break;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Errore: Formato numerico non valido. Assicurati di inserire numeri interi.");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operazione annullata.");
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.ERROR, $"Errore durante l'esecuzione del comando: {ex.Message}", false);
                    Console.WriteLine($"Errore: {ex.Message}");
                }
            }
        }

        private static void LoadConfiguration()
        {
            _databaseFilePath = ConfigurationManager.AppSettings["DatabaseFilePath"] ?? "miu_data.db";
            if (!int.TryParse(ConfigurationManager.AppSettings["MaxStringLength"], out _maxStringLength))
            {
                _maxStringLength = 30;
            }
            if (!int.TryParse(ConfigurationManager.AppSettings["TargetDerivableCountInitDb"], out _targetDerivableCountInitDb))
            {
                _targetDerivableCountInitDb = 150;
            }
            if (!int.TryParse(ConfigurationManager.AppSettings["MaxStringsToGenerateForSeeding"], out _maxStringsToGenerateForSeeding))
            {
                _maxStringsToGenerateForSeeding = 3000;
            }
        }
    }
}
