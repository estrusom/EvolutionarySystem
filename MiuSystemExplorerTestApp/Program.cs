// File: C:\Progetti\EvolutiveSystem\MiuSystemExplorerTestApp\Program.cs
// Data di riferimento: 27 giugno 2025 (Refactoring per integrazione comando Reset Data)
// Descrizione: Punto di ingresso principale dell'applicazione.
//              Inizializza i servizi fondamentali del sistema MIU e avvia il motore di derivazione.
//              La logica di esplorazione e persistenza è delegata a MIUDerivationEngine.
//              Integrazione del MIUTopologyService per la visualizzazione della topologia.
//              Aggiunto comando 'reset data' per la pulizia selettiva del database.

using EvolutiveSystem.SQL.Core; // For SQLiteSchemaLoader, MIUDatabaseManager
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MIU.Core; // For MIURepository, IMIUDataManager, MIUStringConverter, RegoleMIUManager
using MasterLog; // Required for your Logger class
using EvolutiveSystem.Common; // For RegolaMIU, RuleStatistics, TransitionStatistics, MIUExplorerCursor, MIUStringTopologyData
using EvolutiveSystem.Learning; // For LearningStatisticsManager
using EvolutiveSystem.Engine; // For IMIUDataProcessingService, MIUDerivationEngine
using EvolutiveSystem.Services;
using System.Configuration; // For IMIUTopologyService, MIUTopologyService

namespace MiuSystemExplorerTestApp // Your specific test app namespace
{
    // Left here for compatibility, but not directly used for RuleStatistics
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public SerializableDictionary() : base() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
    }

    internal class Program
    {
        private static Logger _logger; // Logger instance
        private static MIURepository _repository; // Repository for persistence

        // Fields for configuration parameters loaded from DB, with default values
        private static long _configuredMaxDepth = 10;
        private static long _configuredMaxSteps = 10;

        private static LearningStatisticsManager _learningStatsManager; // LearningStatisticsManager instance
        private static IMIUDataProcessingService _miuDerivationEngine; // Derivation engine instance
        private static IMIUTopologyService _miuTopologyService; // Topology service instance
        private static IMIUDataManager _dataManager; // Reference to the data manager for reset functionality
        private static EventBus eventBus = null;

        static async Task Main(string[] args) // Main made asynchronous
        {
            // Logger Initialization
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            _logger = new Logger(logDirectory, "MIULog", 7); // Retain last 7 days of logs
            //_logger.SwLogLevel = _logger.LOG_INFO | _logger.LOG_DEBUG | _logger.LOG_ERROR | _logger.LOG_WARNING; // Set active log levels
            _logger.SwLogLevel = _logger.LOG_INFO | _logger.LOG_ERROR | _logger.LOG_WARNING; // Set active log levels

            _logger.Log(LogLevel.INFO, "MIU Application started.");

            try
            {
                // Common repository initialization and statistics loading
                //string databaseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "miudb.sqlite"); // Modified path to be more flexible
                string databaseFilePath = ConfigurationManager.AppSettings["FolderDBFile"];
                SQLiteSchemaLoader _schemaLoader = new SQLiteSchemaLoader(databaseFilePath, _logger);
                _schemaLoader.InitializeDatabase();
                _logger.Log(LogLevel.INFO, "Database initialized via SQLiteSchemaLoader.");

                MIUDatabaseManager _dbManager = new MIUDatabaseManager(_schemaLoader, _logger);
                _dataManager = _dbManager; // Assign to static field for broader access
                _repository = new MIURepository(_dataManager, _logger);
                _logger.Log(LogLevel.INFO, "MIUDatabaseManager and MIURepository instantiated.");
                eventBus = new EventBus(_logger);

                // LearningStatisticsManager Initialization
                _learningStatsManager = new LearningStatisticsManager(_dataManager, _logger);
                _logger.Log(LogLevel.INFO, "LearningStatisticsManager instantiated.");

                // Ensure RegoleMIUManager has the logger set, as it is static.
                RegoleMIUManager.LoggerInstance = _logger;

                // Load configuration parameters from the database and set them in RegoleMIUManager
                Dictionary<string, string> configParams = _repository.LoadMIUParameterConfigurator();

                long parsedDepth = _configuredMaxDepth;
                if (configParams.TryGetValue("ProfonditaDiRicerca", out string depthStr) && long.TryParse(depthStr, out parsedDepth))
                {
                    _configuredMaxDepth = parsedDepth;
                    _logger.Log(LogLevel.INFO, $"[Program INFO] Loaded ProfonditaDiRicerca from DB: {_configuredMaxDepth}");
                }
                else
                {
                    _logger.Log(LogLevel.WARNING, $"[Program WARNING] Parameter 'ProfonditaDiRicerca' not found or invalid. Using default value: {_configuredMaxDepth}");
                }

                long parsedSteps = _configuredMaxSteps;
                if (configParams.TryGetValue("MassimoPassiRicerca", out string stepsStr) && long.TryParse(stepsStr, out parsedSteps))
                {
                    _configuredMaxSteps = parsedSteps;
                    _logger.Log(LogLevel.INFO, $"[Program INFO] Loaded MassimoPassiRicerca from DB: {_configuredMaxSteps}");
                }
                else
                {
                    _logger.Log(LogLevel.WARNING, $"[Program WARNING] Parameter 'MassimoPassiRicerca' not found or invalid. Using default value: {_configuredMaxSteps}");
                }

                RegoleMIUManager.MaxProfonditaRicerca = _configuredMaxDepth;
                RegoleMIUManager.MassimoPassiRicerca = _configuredMaxSteps;
                _logger.Log(LogLevel.INFO, $"[Program INFO] RegoleMIUManager set with MaxProfonditaRicerca: {RegoleMIUManager.MaxProfonditaRicerca} and MassimoPassiRicerca: {RegoleMIUManager.MassimoPassiRicerca}");

                // Load and save example rules if DB is empty
                var existingRules = _dataManager.LoadRegoleMIU();
                if (existingRules == null || !existingRules.Any())
                {
                    _logger.Log(LogLevel.INFO, "No MIU rules found in the database. Loading example rules.");
                    var defaultRules = new List<RegolaMIU>
                    {
                        new RegolaMIU(1, "Regola 1 (Append U)", "Stringhe Mu -> Stringhe MUU", "u$", "uu"),
                        new RegolaMIU(2, "Regola 2 (MMxx)", "Mix -> MMxx", "M(i+)", "M$1$1"),
                        new RegolaMIU(3, "Regola 3 (III to U)", "III", "U", "Sostituisce 'III' con 'U'"),
                        new RegolaMIU(4, "Regola 4 (UU to Empty)", "UU", "", "Elimina 'UU'")
                    };
                    _dataManager.UpsertRegoleMIU(defaultRules);
                    _logger.Log(LogLevel.INFO, $"{defaultRules.Count} example rules inserted into the database.");
                }

                // Derivation Engine Initialization
                _miuDerivationEngine = new MIUDerivationEngine(_dataManager, _learningStatsManager, _logger, eventBus);

                // Subscribe to engine events for console feedback
                _miuDerivationEngine.OnExplorationStatusChanged += MiuDerivationEngine_OnExplorationStatusChanged;
                _miuDerivationEngine.OnNodesExploredCountChanged += MiuDerivationEngine_OnNodesExploredCountChanged;

                // NEW: Topology Service Initialization
                _miuTopologyService = new MIUTopologyService(_dataManager, _learningStatsManager, _logger);
                _logger.Log(LogLevel.INFO, "MIUTopologyService instantiated.");


                Console.WriteLine("------------------------------------------");
                Console.WriteLine("MIU Explorer System Started");
                Console.WriteLine("Commands:");
                Console.WriteLine("  'start <initial_string>' (e.g., 'start MI') - Starts exploration.");
                Console.WriteLine("  'start <initial_string> <target_string>' (e.g., 'start MI MU') - Starts exploration with target.");
                Console.WriteLine("  'stop' - Stops current exploration.");
                Console.WriteLine("  'status' - Shows current engine status.");
                Console.WriteLine("  'show topology [initialString] [startDate] [endDate] [maxDepth]' - Shows filtered topology.");
                Console.WriteLine("  'reset data' - Clears exploration-specific data in the database."); // NEW COMMAND
                Console.WriteLine("  'exit' - Exits the application.");
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
                            Console.WriteLine("Error: Specify the initial string. Example: start MI");
                            continue;
                        }
                        string initialString = parts[1].ToUpperInvariant();
                        string targetString = (parts.Length > 2) ? parts[2].ToUpperInvariant() : null;

                        if (_miuDerivationEngine.IsExplorationRunning)
                        {
                            Console.WriteLine("The engine is already running. Stop the current exploration first.");
                        }
                        else
                        {
                            // Start the derivation engine with the initial string and target
                            await _miuDerivationEngine.StartExplorationAsync(initialString, targetString);
                            Console.WriteLine("Start command sent to the engine. Exploration is in the background...");
                        }
                    }
                    else if (command == "stop")
                    {
                        if (_miuDerivationEngine.IsExplorationRunning)
                        {
                            _miuDerivationEngine.StopExploration();
                            Console.WriteLine("Stop command sent to the engine. Waiting for clean shutdown.");
                        }
                        else
                        {
                            Console.WriteLine("The engine is not running.");
                        }
                    }
                    else if (command == "status")
                    {
                        MIUExplorerCursor currentCursor = await _miuDerivationEngine.GetCurrentExplorerCursorAsync();
                        Console.WriteLine($"Engine Status: {(_miuDerivationEngine.IsExplorationRunning ? "Running" : "Inactive")}");
                        Console.WriteLine($"  Last Cursor (DB): Source ID={currentCursor.CurrentSourceIndex}, Target ID={currentCursor.CurrentTargetIndex}, Last Update={currentCursor.LastExplorationTimestamp}");
                    }
                    else if (command.StartsWith("show topology")) // COMMAND FOR TOPOLOGY
                    {
                        string[] parts = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string initialStrFilter = null;
                        DateTime? startDateFilter = null;
                        DateTime? endDateFilter = null;
                        int? maxDepthFilter = null;

                        // Parsing parameters: show topology [initialString] [startDate] [endDate] [maxDepth]
                        // Simplified: assumes parameters are in order.
                        if (parts.Length > 2) initialStrFilter = parts[2].ToUpperInvariant(); // e.g., "show topology MI"
                        if (parts.Length > 3 && DateTime.TryParse(parts[3], out DateTime sd)) startDateFilter = sd;
                        if (parts.Length > 4 && DateTime.TryParse(parts[4], out DateTime ed)) endDateFilter = ed;
                        if (parts.Length > 5 && int.TryParse(parts[5], out int md)) maxDepthFilter = md;

                        Console.WriteLine("Loading topology...");
                        MIUStringTopologyData topology = await _miuTopologyService.LoadMIUStringTopologyAsync(
                            initialStrFilter, startDateFilter, endDateFilter, maxDepthFilter
                        );
                        Console.WriteLine($"Topology Loaded: {topology.Nodes.Count} Nodes, {topology.Edges.Count} Edges.");
                        if (!string.IsNullOrEmpty(topology.InitialString))
                        {
                            Console.WriteLine($"  Topology filtered for initial string: '{topology.InitialString}'.");
                        }
                        // You can add more details here, e.g., first 5 nodes or edges
                    }
                    else if (command == "reset data") // NEW COMMAND: Reset data
                    {
                        Console.Write("Are you sure you want to clear all exploration data (searches, rule applications, paths, learning stats)? (yes/no): ");
                        string confirmation = Console.ReadLine()?.ToLowerInvariant().Trim();
                        if (confirmation == "yes")
                        {
                            await _dataManager.ResetExplorationDataAsync();
                            Console.WriteLine("Exploration data has been reset.");
                            _logger.Log(LogLevel.INFO, "[Program] Exploration data reset by user command.");
                        }
                        else
                        {
                            Console.WriteLine("Reset cancelled.");
                        }
                    }
                    else if (command != "exit")
                    {
                        Console.WriteLine("Command not recognized.");
                    }

                } while (command != "exit");

                // On application shutdown, ensure the engine is stopped
                if (_miuDerivationEngine.IsExplorationRunning)
                {
                    _miuDerivationEngine.StopExploration();
                    _logger.Log(LogLevel.INFO, "Waiting for derivation engine to shut down...");
                    // You might want to add a wait here to ensure the task has completed
                }

                _logger.Log(LogLevel.INFO, "MIU Application terminated.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Unhandled critical error in application: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Console.WriteLine($"Critical error: {ex.Message}");
                Console.WriteLine("Check the log file for details.");
            }
            finally
            {
                // Ensure the logger is properly disposed if necessary
            }
        }

        // --- Event handlers for feedback from MIUDerivationEngine ---
        private static void MiuDerivationEngine_OnExplorationStatusChanged(object sender, string e)
        {
            Console.WriteLine($"[Engine Status] {e}");
            _logger.Log(LogLevel.INFO, $"[Engine Status Event] {e}");
        }

        private static void MiuDerivationEngine_OnNodesExploredCountChanged(object sender, int e)
        {
            // You might want to limit print frequency to avoid console flooding
            if (e % 100 == 0 || e == 1) // Print every 100 nodes or on the first node
            {
                Console.WriteLine($"[Engine Progress] Nodes Explored: {e}");
            }
            _logger.Log(LogLevel.DEBUG, $"[Engine Progress Event] Nodes Explored: {e}");
        }
    }
}
