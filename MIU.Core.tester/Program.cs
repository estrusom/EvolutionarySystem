// File: C:\Progetti\EvolutiveSystem\MIU.Core.tester\Program.cs
// Data di riferimento: 27 giugno 2025 (Refactoring per integrazione MIUDerivationEngine)
// Descrizione: Punto di ingresso principale dell'applicazione.
//              Inizializza i servizi fondamentali del sistema MIU e avvia il motore di derivazione.
//              La logica di esplorazione e persistenza è ora delegata a MIUDerivationEngine.

using EvolutiveSystem.SQL.Core; // Per SQLiteSchemaLoader, MIUDatabaseManager
using System;
using System.Collections.Generic;
using System.Diagnostics; // Per Process, StopWatch (se ancora in uso, altrimenti può essere rimosso)
using System.IO;
using System.Linq; // Necessario per LINQ
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MIU.Core; // Per MIURepository, IMIUDataManager, MIUStringConverter, RegoleMIUManager
using MasterLog; // Necessario per la tua classe Logger
using EvolutiveSystem.Common; // Per RegolaMIU, RuleStatistics, TransitionStatistics, MIUExplorerCursor
using EvolutiveSystem.Learning; // Per LearningStatisticsManager
using EvolutiveSystem.Engine; // Per IMIUDataProcessingService, MIUDerivationEngine (il nostro motore)

namespace MIU.Core.tester
{
    // Lasciata qui per compatibilità, ma non direttamente usata per RuleStatistics
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public SerializableDictionary() : base() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
    }

    internal class Program
    {
        private static Logger _logger; // Istanza del logger
        private static MIURepository _repository; // Repository per la persistenza
        // Rimosso: private static long _currentSearchId; // Ora gestito internamente dal MIUDerivationEngine

        // Rimosso: Le statistiche non sono più campi statici di Program.cs,
        // sono gestite da LearningStatisticsManager e passate a RegoleMIUManager dal DerivationEngine.
        // private static System.Collections.Generic.Dictionary<long, EvolutiveSystem.Common.RuleStatistics> _ruleStatistics;
        // private static System.Collections.Generic.Dictionary<Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics> _transitionStatistics;

        // Campi per i parametri di configurazione caricati dal DB, con valori predefiniti
        private static long _configuredMaxDepth = 10; // Valore predefinito per ProfonditaDiRicerca
        private static long _configuredMaxSteps = 10; // Valore predefinito per MassimoPassiRicerca

        private static LearningStatisticsManager _learningStatsManager; // Istanza di LearningStatisticsManager
        private static IMIUDataProcessingService _miuDerivationEngine; // Istanza del motore di derivazione

        static async Task Main(string[] args) // Main reso asincrono
        {
            // Inizializzazione del Logger
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            _logger = new Logger(logDirectory, "MIULog", 7); // Conserva gli ultimi 7 giorni di log
            _logger.SwLogLevel = _logger.LOG_INFO | _logger.LOG_DEBUG | _logger.LOG_ERROR | _logger.LOG_WARNING; // Imposta i livelli di log attivi

            _logger.Log(LogLevel.INFO, "Applicazione MIU avviata.");

            try
            {
                // Inizializzazione comune del repository e caricamento delle statistiche
                string databaseFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";
                SQLiteSchemaLoader _schemaLoader = new SQLiteSchemaLoader(databaseFilePath, _logger);
                _schemaLoader.InitializeDatabase();
                _logger.Log(LogLevel.INFO, "Database inizializzato tramite SQLiteSchemaLoader.");

                MIUDatabaseManager _dbManager = new MIUDatabaseManager(_schemaLoader, _logger);
                IMIUDataManager _dataManager = _dbManager;
                _repository = new MIURepository(_dataManager, _logger); // Assegna al campo statico
                _logger.Log(LogLevel.INFO, "MIUDatabaseManager e MIURepository istanziati.");

                // Inizializzazione di LearningStatisticsManager
                _learningStatsManager = new LearningStatisticsManager(_dataManager, _logger);
                _logger.Log(LogLevel.INFO, "LearningStatisticsManager istanziato.");

                // Assicurati che RegoleMIUManager abbia il logger impostato, dato che è statico.
                RegoleMIUManager.LoggerInstance = _logger;


                // Carica i parametri di configurazione dal database e impostali in RegoleMIUManager
                System.Collections.Generic.Dictionary<string, string> configParams = _repository.LoadMIUParameterConfigurator();

                long parsedDepth = _configuredMaxDepth;
                if (configParams.TryGetValue("ProfonditaDiRicerca", out string depthStr) && long.TryParse(depthStr, out parsedDepth))
                {
                    _configuredMaxDepth = parsedDepth;
                    _logger.Log(LogLevel.INFO, $"[Program INFO] Caricato ProfonditaDiRicerca dal DB: {_configuredMaxDepth}");
                }
                else
                {
                    _logger.Log(LogLevel.WARNING, $"[Program WARNING] Parametro 'ProfonditaDiRicerca' non trovato o non valido. Usato valore predefinito: {_configuredMaxDepth}");
                }

                long parsedSteps = _configuredMaxSteps;
                if (configParams.TryGetValue("MassimoPassiRicerca", out string stepsStr) && long.TryParse(stepsStr, out parsedSteps))
                {
                    _configuredMaxSteps = parsedSteps;
                    _logger.Log(LogLevel.INFO, $"[Program INFO] Caricato MassimoPassiRicerca dal DB: {_configuredMaxSteps}");
                }
                else
                {
                    _logger.Log(LogLevel.WARNING, $"[Program WARNING] Parametro 'MassimoPassiRicerca' non trovato o non valido. Usato valore predefinito: {_configuredMaxSteps}");
                }

                RegoleMIUManager.MaxProfonditaRicerca = _configuredMaxDepth;
                RegoleMIUManager.MassimoPassiRicerca = _configuredMaxSteps;
                _logger.Log(LogLevel.INFO, $"[Program INFO] RegoleMIUManager impostato con MaxProfonditaRicerca: {RegoleMIUManager.MaxProfonditaRicerca} e MassimoPassiRicerca: {RegoleMIUManager.MassimoPassiRicerca}");

                // Carica e salva le regole di esempio se il DB è vuoto
                var existingRules = _dataManager.LoadRegoleMIU();
                if (existingRules == null || !existingRules.Any())
                {
                    _logger.Log(LogLevel.INFO, "Nessuna regola MIU trovata nel database. Caricamento regole di esempio.");
                    var defaultRules = new List<RegolaMIU>
                    {
                        new RegolaMIU(1, "Regola 1 (Append U)", "Stringhe Mu -> Stringhe MUU", "u$", "uu"),
                        new RegolaMIU(2, "Regola 2 (MMxx)", "Mix -> MMxx", "M(i+)", "M$1$1"),
                        new RegolaMIU(3, "Regola 3 (III to U)", "III -> U", "III", "U"),
                        new RegolaMIU(4, "Regola 4 (UU to Empty)", "UU", "", "Elimina 'UU'")
                    };
                    _dataManager.UpsertRegoleMIU(defaultRules);
                    _logger.Log(LogLevel.INFO, $"{defaultRules.Count} regole di esempio inserite nel database.");
                }

                // Inizializzazione del Motore di Derivazione
                _miuDerivationEngine = new MIUDerivationEngine(_dataManager, _learningStatsManager, _logger);

                // Iscrizione agli eventi del motore per feedback sulla console
                _miuDerivationEngine.OnExplorationStatusChanged += MiuDerivationEngine_OnExplorationStatusChanged;
                _miuDerivationEngine.OnNodesExploredCountChanged += MiuDerivationEngine_OnNodesExploredCountChanged;

                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Sistema Esploratore MIU Avviato");
                Console.WriteLine("Comandi:");
                Console.WriteLine("  'start <stringa_iniziale>' (es. 'start MI') - Avvia esplorazione.");
                Console.WriteLine("  'start <stringa_iniziale> <stringa_target>' (es. 'start MI MU') - Avvia esplorazione con target.");
                Console.WriteLine("  'stop' - Ferma l'esplorazione corrente.");
                Console.WriteLine("  'status' - Mostra lo stato corrente del motore.");
                Console.WriteLine("  'exit' - Esce dall'applicazione.");
                Console.WriteLine("------------------------------------------");

                string command;
                do
                {
                    Console.Write("> ");
                    command = Console.ReadLine()?.ToLowerInvariant().Trim();

                    if (command == null) continue;

                    if (command.StartsWith("start"))
                    {
                        string[] parts = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2)
                        {
                            Console.WriteLine("Errore: specifica la stringa iniziale. Es: start MI");
                            continue;
                        }
                        string initialString = parts[1].ToUpperInvariant();
                        string targetString = (parts.Length > 2) ? parts[2].ToUpperInvariant() : null;

                        if (_miuDerivationEngine.IsExplorationRunning)
                        {
                            Console.WriteLine("Il motore è già in esecuzione. Ferma prima l'esplorazione corrente.");
                        }
                        else
                        {
                            // Avvia il motore di derivazione con la stringa iniziale e target
                            await _miuDerivationEngine.StartExplorationAsync(initialString, targetString);
                            Console.WriteLine("Comando di avvio inviato al motore. L'esplorazione è in background...");
                        }
                    }
                    else if (command == "stop")
                    {
                        if (_miuDerivationEngine.IsExplorationRunning)
                        {
                            _miuDerivationEngine.StopExploration();
                            Console.WriteLine("Comando di stop inviato al motore. Attendi l'arresto pulito.");
                        }
                        else
                        {
                            Console.WriteLine("Il motore non è in esecuzione.");
                        }
                    }
                    else if (command == "status")
                    {
                        MIUExplorerCursor currentCursor = await _miuDerivationEngine.GetCurrentExplorerCursorAsync();
                        Console.WriteLine($"Stato Motore: {(_miuDerivationEngine.IsExplorationRunning ? "In Esecuzione" : "Inattivo")}");
                        Console.WriteLine($"  Ultimo Cursore (DB): Source ID={currentCursor.CurrentSourceIndex}, Target ID={currentCursor.CurrentTargetIndex}, Ultimo Aggiornamento={currentCursor.LastExplorationTimestamp}");
                    }
                    else if (command != "exit")
                    {
                        Console.WriteLine("Comando non riconosciuto.");
                    }

                } while (command != "exit");

                // Alla chiusura dell'applicazione, assicurati di fermare il motore
                if (_miuDerivationEngine.IsExplorationRunning)
                {
                    _miuDerivationEngine.StopExploration();
                    _logger.Log(LogLevel.INFO, "Attendendo l'arresto del motore di derivazione...");
                    // Puoi aggiungere un'attesa qui se vuoi essere sicuro che il task sia terminato
                    // _miuDerivationEngine._explorationTask?.Wait(5000); // Esempio: attende fino a 5 secondi
                }

                _logger.Log(LogLevel.INFO, "Applicazione MIU terminata.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore critico non gestito nell'applicazione: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Console.WriteLine($"Errore critico: {ex.Message}");
                Console.WriteLine("Controlla il file di log per i dettagli.");
            }
            finally
            {
                // Assicurati che il logger venga smaltito correttamente se necessario
            }
        }

        // --- Event handlers per il feedback dal MIUDerivationEngine ---
        private static void MiuDerivationEngine_OnExplorationStatusChanged(object sender, string e)
        {
            Console.WriteLine($"[Motore Stato] {e}");
            _logger.Log(LogLevel.INFO, $"[Motore Stato Event] {e}");
        }

        private static void MiuDerivationEngine_OnNodesExploredCountChanged(object sender, int e)
        {
            // Potresti voler limitare la frequenza di stampa per non inondare la console
            if (e % 100 == 0 || e == 1) // Stampa ogni 100 nodi o al primo nodo
            {
                Console.WriteLine($"[Motore Progresso] Nodi Esplorati: {e}");
            }
            _logger.Log(LogLevel.DEBUG, $"[Motore Progresso Event] Nodi Esplorati: {e}");
        }

        // --- Metodi e Handler degli eventi di RegoleMIUManager RIMOSSI da Program.cs ---
        // I metodi RegoleMIUManager_OnRuleApplied e RegoleMIUManager_OnSolutionFound
        // sono stati spostati e rinominati in MIUDerivationEngine (HandleRuleApplied, HandleSolutionFound)
        // e sono ora gestiti internamente dal motore.
        // Le chiamate dirette a RegoleMIUManager.TrovaDerivazioneAutomatica sono state rimosse
        // a favore dell'avvio del MIUDerivationEngine.
    }
}
