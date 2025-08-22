/// File: Program.cs
// Questo file è il punto di ingresso per l'applicazione console e ora
// include un menu interattivo per eseguire le funzioni passo-passo.
// per cominciare da zero l'esplorazione i record da azzerare sono ContinuousExplorer_CurrentSourceId e ContinuousExplorer_CurrentTargetId
using System;
using System.IO;
using System.Threading.Tasks;
using System.Configuration; // Necessario per usare ConfigurationManager
using System.Collections.Generic; // Necessario per Dictionary
using System.Data.SQLite; // Contiene SQLiteConnection
using System.Linq; // Per l'uso di LINQ

// Importa i namespace dai progetti di riferimento
using EvolutiveSystem.Common; // Contiene RegolaMIU
using EvolutiveSystem.SQL.Core; // Contiene SQLiteSchemaLoader e MIUDatabaseManager
using MasterLog;
using MIU.Core;
using EvolutiveSystem.Engine;
using EvolutiveSystem.Learning;
using EvolutiveSystem.Automation;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using MiuSeederTool.Core; // Contiene Logger

public class Program
{
    // Enum per definire le opzioni del menu
    private enum MenuOption
    {
        LoadRules = 1,
        LoadParameters,
        StartMiuDerivationFlow,
        PopolaMIU_States,
        StartManualDFS,
        StartManualBFS,
        BuildTopology,
        DetectAnomaly, // Nuova opzione per la rilevazione delle anomalie
        DeleteTable,
        Exit
    }
    private enum SearchAlgorithmType
    {
        DFS,
        BFS
    }
    protected static Mutex SyncMtxLogger = new Mutex();
    public static Logger logger = null;
    public static IMIUDataManager iMiuDataManagerInstance;
    private static  CancellationTokenSource _cancellationTokenSource;
    private static AnomalyDetectionManager _anomalyDetectionManager; // MODIFICA: Dichiarazione della variabile a livello di classe
    private static EventBus _eventBus; // MODIFICA: Dichiarazione della variabile a livello di classe

    public static async Task Main(string[] args)
    {
        //MIUDatabaseManager miuDataManagerInstance = null; // Dichiarazione qui per scope
        LearningStatisticsManager learningStatsManager = null;
        MIUDerivationEngine miuDerivationEngine = null;
        IMIURepository miuRepositoryInstance = null;
        List<RegolaMIU> regolaMIUList;
        MiuContinuousExplorerScheduler _continuousScheduler;
        Dictionary<string, string> configParams = null;

        Console.WriteLine("Avvio dell'applicazione per il test del database...");

        // 1. INIZIALIZZAZIONE DEL LOGGER
        int swDebug = Convert.ToInt32(ConfigurationManager.AppSettings["DebugLev"]);
        string _path = ConfigurationManager.AppSettings["FolderLOG"]; // Percorso di log simulato
        logger = new Logger(_path, "MIU_Tester", SyncMtxLogger);
        logger.SwLogLevel = swDebug;
        logger.Log(LogLevel.INFO, "Start TESTER");
        logger.Log(LogLevel.DEBUG, $"Esecuzione del metodo Main: {nameof(Main)}");
        _cancellationTokenSource = new CancellationTokenSource();

        // 2. CONFIGURAZIONE DELLA CONNESSIONE AL DATABASE
        string dbFileName = "C:\\progetti\\EvolutionarySystem\\Database\\miu_data.db";
        Console.WriteLine($"Apertura della connessione al database in {dbFileName}...");
        SQLiteConnection dbConnection = new SQLiteConnection($"Data Source={dbFileName}");
        Console.WriteLine("Connessione al database aperta con successo.");

        // Il caricatore dello schema viene inizializzato con il nome del file del database
        // e il logger.
        Console.WriteLine("Inizializzazione dello schema del database...");
        SQLiteSchemaLoader schemaLoader = new SQLiteSchemaLoader(dbFileName, logger);
        Console.WriteLine("Schema del database inizializzato.");

        try
        {
            // 3. APERTURA DEL DATABASE - IL PRIMO PASSO FONDAMENTALE
            Console.WriteLine($"Apertura della connessione al database in {dbFileName}...");
            await dbConnection.OpenAsync();
            Console.WriteLine("Connessione al database aperta con successo.");

            // 4. INIZIALIZZAZIONE DELLO SCHEMA
            Console.WriteLine("Inizializzazione dello schema del database...");
            // Non c'è bisogno di re-inizializzare schemaLoader qui se dbFileName è già corretto.
            // schemaLoader = new EvolutiveSystem.SQL.Core.SQLiteSchemaLoader(dbConnection.FileName, logger);
            schemaLoader.InitializeDatabase();
            Console.WriteLine("Schema del database inizializzato.");

            // 5. CREAZIONE DELLE ISTANZE DEI COMPONENTI
            // Le istanze delle tue classi reali
            _eventBus = new EventBus(logger);
            iMiuDataManagerInstance = new MIUDatabaseManager(schemaLoader, logger);
            learningStatsManager = new LearningStatisticsManager(iMiuDataManagerInstance, logger);
            _anomalyDetectionManager = new AnomalyDetectionManager(iMiuDataManagerInstance, logger, _eventBus); // MODIFICA: Inizializza AnomalyDetectionManager
                        
            RegoleMIUManager.LoggerInstance = logger;
            miuDerivationEngine = new MIUDerivationEngine(iMiuDataManagerInstance, learningStatsManager, logger, _eventBus);
            miuRepositoryInstance = new MIU.Core.MIURepository(iMiuDataManagerInstance, logger);

            // MODIFICA: Iscrivi un listener di debug all'evento di anomalia
            _eventBus.Subscribe<AnomalyDetectedEvent>(
                (eventData) =>
                {
                    logger.Log(LogLevel.INFO, $"[Program] Ascoltatore di debug: Rilevata anomalia di tipo '{eventData.Type}' con ID '{eventData.AnomalyId}'.");
                });

            regolaMIUList = new List<RegolaMIU>();

            bool exitProgram = false;
            while (!exitProgram)
            {
                DisplayMenu();
                string input = Console.ReadLine();
                if (int.TryParse(input, out int choice))
                {
                    switch ((MenuOption)choice)
                    {
                        case MenuOption.LoadRules:
                            {
                                logger.Log(LogLevel.INFO, "LoadRules");
                                regolaMIUList = iMiuDataManagerInstance.LoadRegoleMIU();
                                foreach (RegolaMIU r in regolaMIUList)
                                {
                                    string msg = $"id: {r.ID} Nome: {r.Nome} Descrizione: {r.Descrizione} Pattern: {r.Pattern} Sostituzione: {r.Sostituzione}";
                                    logger.Log(LogLevel.INFO, msg);
                                    Console.WriteLine(msg);
                                }
                            }
                            break;
                        case MenuOption.LoadParameters:
                            {
                                logger.Log(LogLevel.INFO, "LoadParameters");
                                configParams = iMiuDataManagerInstance.LoadMIUParameterConfigurator();
                                foreach (var s in configParams)
                                {
                                    string msg = $"Key: {s.Key} Value:_ {s.Value}";
                                    logger.Log(LogLevel.INFO, msg);
                                    Console.WriteLine(msg);
                                }
                                if(configParams.TryGetValue("ProfonditaDiRicerca", out var p))
                                {
                                    RegoleMIUManager.MaxProfonditaRicerca = p != null ? Convert.ToInt32(p) : 0;
                                }
                                if (configParams.TryGetValue("MaxNodiDaEsplorare", out var r))
                                {
                                    RegoleMIUManager.MaxNodiDaEsplorare = r != null ? Convert.ToInt32(r) : 0;
                                }
                                if (configParams.TryGetValue("MaxStringLength", out var l))
                                {
                                    RegoleMIUManager.MAX_STRING_LENGTH = l != null ? Convert.ToInt32(l) : 0;
                                }
                                //RegoleMIUManager.MassimoPassiRicerca = maxSteps;

                            }
                            break;
                        case MenuOption.StartMiuDerivationFlow:
                            _continuousScheduler = new MiuContinuousExplorerScheduler(miuDerivationEngine, iMiuDataManagerInstance, miuRepositoryInstance, logger, configParams);
                            _continuousScheduler.StartScheduler();
                            _continuousScheduler.ProgressUpdated -= _continuousScheduler_ProgressUpdated;
                            _continuousScheduler.ExplorationCompleted -= _continuousScheduler_ExplorationCompleted;
                            _continuousScheduler.ExplorationError -= _continuousScheduler_ExplorationError;
                            _continuousScheduler.NewMiuStringDiscovered -= _continuousScheduler_NewMiuStringDiscovered;
                                                                                                                   // ...
                            _continuousScheduler.ProgressUpdated += _continuousScheduler_ProgressUpdated;
                            _continuousScheduler.ExplorationCompleted += _continuousScheduler_ExplorationCompleted;
                            _continuousScheduler.ExplorationError += _continuousScheduler_ExplorationError;
                            _continuousScheduler.NewMiuStringDiscovered += _continuousScheduler_NewMiuStringDiscovered;
                            break;
                        case MenuOption.PopolaMIU_States: //legge un file con stringhe MIU e le carica in MIU_States
                            {
                                SeederDbAccess sda = new SeederDbAccess(dbFileName, logger);
                                SeederMiuState smt = new SeederMiuState()
                                {
                                    //StateID = 0,
                                    CurrentString = "",
                                    StringLength = 0,
                                    DeflateString = "",
                                    Hash = "",
                                    DiscoveryTime_Int = 0,
                                    DiscoveryTime_Text = "",
                                    UsageCount = 0,
                                    SeedingType = SeedingType.SolutionPath,

                                };
                                await sda.ClearMiuStatesTableAsync();
                                using (StreamReader sr = new StreamReader("C:\\progetti\\EvolutionarySystem\\Database\\MIUstring.txt"))
                                {
                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        if (!string.IsNullOrWhiteSpace(line))
                                        {
                                            //smt.StateID++;
                                            smt.CurrentString = line.Trim();
                                            smt.StringLength = smt.CurrentString.Length;
                                            smt.DeflateString = MIUStringConverter.DeflateMIUString(smt.CurrentString);
                                            smt.Hash = MIUStringConverter.ComputeHash(smt.CurrentString);
                                            smt.DiscoveryTime_Int = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                                            smt.DiscoveryTime_Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                                            smt.UsageCount = 0;
                                            smt.SeedingType = SeedingType.SolutionPath;
                                        }
                                        string msgIns = $"{smt.StateID} {smt.CurrentString}  {smt.StringLength} {smt.DeflateString} {smt.Hash} {smt.DiscoveryTime_Int} {smt.DiscoveryTime_Text} {smt.UsageCount} {smt.SeedingType}";
                                        Console.WriteLine(msgIns);
                                        logger.Log(LogLevel.INFO, msgIns);
                                        await sda.InsertMiuStateAsync(smt);
                                    }
                                }
                            }
                            break;
                        case MenuOption.Exit:
                            exitProgram = true;
                            Console.WriteLine("Uscita dal programma. Arrivederci!");
                            break;
                        case MenuOption.StartManualDFS:
                            await StartManualDerivation(miuDerivationEngine, SearchAlgorithmType.DFS);
                            break;
                        case MenuOption.StartManualBFS:
                            await StartManualDerivation(miuDerivationEngine, SearchAlgorithmType.BFS);
                            break;
                        // *** AGGIUNTA DEL 13-08-2025 ***
                        case MenuOption.DetectAnomaly:
                            
                            // 💡 Nuova opzione per testare la rilevazione di un'anomalia
                            // 2025/08/15: Chiamata al metodo corretto per gestire l'anomalia
                            await HandleDetectAnomaly(_anomalyDetectionManager);
                            break;
                        case MenuOption.BuildTopology:
                            {
                                // MODIFICA: Chiamata al metodo corretto BuildTopology sul manager
                                await _anomalyDetectionManager.BuildTopology();
                                Console.WriteLine("Costruzione della topologia completata.");
                            }
                            break;
                        case MenuOption.DeleteTable:
                            await iMiuDataManagerInstance.ResetExplorationDataAsync();
                            break;
                        default:
                            Console.WriteLine("Scelta non valida. Riprova.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Input non valido. Inserisci un numero.");
                }
                Console.WriteLine("\nPremi un tasto per continuare...");
                Console.ReadKey();
                Console.Clear(); // Pulisce la console per il prossimo menu
            }
        }
        catch (Exception ex)
        {
            // Gestione e log degli errori
            logger.Log(LogLevel.ERROR, $"Errore grave durante l'esecuzione del Main: {ex.Message}");
            Console.WriteLine($"Errore: {ex.Message}");
        }
        finally
        {
            // 6. CHIUSURA SICURA DELLA CONNESSIONE
            if (dbConnection != null && dbConnection.State == System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Chiusura della connessione al database.");
                dbConnection.Close();
            }
        }
    }
    /// <summary>
    /// Metodo che gestisce la rilevazione di un'anomalia.
    /// Simula il rilevamento di un'anomalia e la passa al manager.
    /// </summary>
    /// <param name="manager">L'istanza dell'AnomalyDetectionManager.</param>
    private static async Task HandleDetectAnomaly(AnomalyDetectionManager manager)
    {
        Console.WriteLine("\n--- Rilevamento Anomalia Simulato ---");
        // MODIFICA: Utilizzo di un valore di AnomalyType valido
        manager.DetectAndHandleAnomaly(
            AnomalyType.ExcessiveLengthGeneration,
            null,
            null,
            "Simulazione",
            1.5,
            "Valore anomalo simulato per la metrica di esplorazione."
        );
        Console.WriteLine("Anomalia simulata inviata al manager.");
        await Task.CompletedTask; // Permette di usare await
     }

    private static void RegoleMIUManager_OnNewMiuStringDiscoveredInternal(object sender, NewMiuStringDiscoveredEventArgs e)
    {
        string msg = $"[SemanticProcessorService] Nuova stringa MIU scoperta: '{e.DiscoveredString}'. StateID: {e.StateID}, IsTrulyNewToDatabase: {e.IsTrulyNewToDatabase}";
        logger.Log(LogLevel.INFO, msg);
        Console.WriteLine(msg);
    }

    private static void RegoleMIUManager_OnRuleApplied(object sender, RuleAppliedEventArgs e)
    {
        string msg = $"[SemanticProcessorService] Regola applicata: {e.AppliedRuleName} su '{e.NewString}' -> '{e.OriginalString}'. SearchID: {e.SearchID}, CurrentDepth: {e.CurrentDepth}";
        logger.Log(LogLevel.INFO, msg);
        Console.WriteLine(msg);
    }

    private static void RegoleMIUManager_OnSolutionFound(object sender, SolutionFoundEventArgs e)
    {
        if (e.Success)
        {
            string msg = $"[SemanticProcessorService] Soluzione trovata: {e.TargetString}. SearchID: {e.SearchID}, Steps: {e.StepsTaken}, Nodes: {e.NodesExplored}, Max Depth: {e.MaxDepthReached}";
            logger.Log(LogLevel.INFO, msg);
            Console.WriteLine(msg);
        }
        else
        {
            string msg = $"[SemanticProcessorService] Nessuna soluzione trovata. SearchID: {e.SearchID}, Steps: {e.StepsTaken}, Nodes: {e.NodesExplored}, Max Depth: {e.MaxDepthReached}";
            logger.Log(LogLevel.WARNING, msg);
            Console.WriteLine(msg);
        }
    }

    private static void _continuousScheduler_NewMiuStringDiscovered(object sender, NewMiuStringDiscoveredEventArgs e)
    {
        // Verifica che la stringa sia veramente nuova per il database.
        // L'analisi ha senso solo per stati appena scoperti o aggiornati.
        if (!e.IsTrulyNewToDatabase)
        {
            return;
        }

        string logMessage = $"[SemanticProcessorService] Nuova stringa MIU scoperta dallo scheduler: '{e.DiscoveredString}'. StateID: {e.StateID}, IsTrulyNewToDatabase: {e.IsTrulyNewToDatabase}. Avvio calcolo tassonomia.";
        logger.Log(LogLevel.INFO, logMessage, true);

        try
        {
            // Ottiene l'istanza statica del data manager.
            var miuDataManager = iMiuDataManagerInstance;
            if (miuDataManager == null)
            {
                logger.Log(LogLevel.ERROR, "[SemanticProcessorService] L'istanza di IMIUDataManager non è stata inizializzata. Impossibile procedere.");
                return;
            }

            // Carica le applicazioni di regole dal database.
            var allRuleApplications = miuDataManager.LoadAllRuleApplications();

            if (allRuleApplications == null || !allRuleApplications.Any())
            {
                logger.Log(LogLevel.WARNING, "[SemanticProcessorService] Nessuna applicazione di regole trovata per l'analisi. Ignoro l'evento.");
                return;
            }

            // Calcola l'in-degree (numero di archi in entrata) per ogni nodo (stringa MIU).
            var inDegree = allRuleApplications
                .GroupBy(ra => ra.NewStateID)
                .ToDictionary(g => g.Key, g => g.Count());

            // Calcola l'out-degree (numero di archi in uscita) per ogni nodo (stringa MIU).
            var outDegree = allRuleApplications
                .GroupBy(ra => ra.ParentStateID)
                .ToDictionary(g => g.Key, g => g.Count());

            // Identifica e logga le anomalie (es. Trappole e Collettori).
            var traps = outDegree
                .Where(kvp => kvp.Value == 0)
                .Select(kvp => kvp.Key);

            var collectors = inDegree
                .Where(kvp => kvp.Value > 1 && (!outDegree.ContainsKey(kvp.Key) || outDegree[kvp.Key] == 0))
                .Select(kvp => kvp.Key);

            string anomalyMessage = "[SemanticProcessorService] Analisi Tassonomia completata.";
            if (traps.Any())
            {
                anomalyMessage += $" Trovate Trappole (out-degree = 0): {string.Join(", ", traps)}";
                foreach (var trapId in traps)
                {
                    miuDataManager.UpsertExplorationAnomaly(new ExplorationAnomaly(
                    type: AnomalyType.DeadEndString,
                    ruleId: null,
                    contextPatternHash: null,
                    contextPatternSample: null,
                    description: $"Trap found for StateID {trapId} (out-degree = 0)"
                ));
                }
            }
            if (collectors.Any())
            {
                anomalyMessage += $" Trovati Collettori (in-degree > 1 e out-degree = 0): {string.Join(", ", collectors)}";
            }

            logger.Log(LogLevel.INFO, anomalyMessage, true);

            // Ulteriori log di dettaglio sui gradi del nodo appena scoperto
            int discoveredInDegree;
            inDegree.TryGetValue(e.StateID, out discoveredInDegree);

            int discoveredOutDegree;
            outDegree.TryGetValue(e.StateID, out discoveredOutDegree);

            logger.Log(LogLevel.INFO, $"[SemanticProcessorService] Dettagli per la nuova stringa (ID: {e.StateID}, '{e.DiscoveredString}'): In-Degree = {discoveredInDegree}, Out-Degree = {discoveredOutDegree}", true);

            // TODO: Qui puoi inserire la logica per salvare i risultati della tassonomia in una nuova tabella.
            // Esempio: miuDataManager.SaveTaxonomyResult(e.StateID, discoveredInDegree, discoveredOutDegree);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.ERROR, $"[SemanticProcessorService] Errore nel calcolo della tassonomia per la stringa '{e.DiscoveredString}': {ex.Message}");
        }
    }

    private static void _continuousScheduler_ExplorationError(object sender, MiuExplorationErrorEventArgs e)
    {
        string msg = $"[SemanticProcessorService] Errore nell'Esplorazione Continua: {e.ErrorMessage}. Eccezione: {e.Exception?.Message}";
        logger.Log(LogLevel.ERROR, msg);
        Console.WriteLine(msg);
    }

    private static void _continuousScheduler_ExplorationCompleted(object sender, MiuExplorationCompletedEventArgs e)
    {
        string msg = $"[SemanticProcessorService] Esplorazione Continua Completata. Successo: {e.IsSuccessful}, Messaggio: '{e.FinalMessage}'. Coppie Esplorate: {e.TotalPairsExplored}, Nuove Stringhe Totali: {e.TotalNewMiuStringsFound}";
        logger.Log(LogLevel.INFO, msg);
        Console.WriteLine(msg);
    }

    private static void _continuousScheduler_ProgressUpdated(object sender, MiuExplorationProgressEventArgs e)
    {
        string msg = $"[SemanticProcessorService] Progresso Esplorazione: Coppia {e.ExploredPairsCount} (S:{e.CurrentSourceId} T:{e.CurrentTargetId}) - Nuove Stringhe: {e.TotalNewMiuStringsFound} - Nodi Motore: {e.NodesExploredInCurrentEngineWave}";
        logger.Log(LogLevel.INFO, msg);
        Console.WriteLine(msg);
    }
    private static async Task StartManualDerivation(MIUDerivationEngine engine, SearchAlgorithmType algorithmType)
    {
        List<MIU.Core.PathStepInfo> pathStepInfos = new List<MIU.Core.PathStepInfo>();
        Console.WriteLine($"\n--- Avvia Derivazione {algorithmType} ---");
        Console.Write("Inserisci la stringa di partenza: ");
        string startString = Console.ReadLine();
        Console.Write("Inserisci la stringa di destinazione: ");
        string targetString = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(startString) || string.IsNullOrWhiteSpace(targetString))
        {
            Console.WriteLine("Stringa di partenza o di destinazione non valide.");
            return;
        }

        string deflateStartString = MIUStringConverter.DeflateMIUString(startString);
        string deflateTargetStrin = MIUStringConverter.DeflateMIUString(targetString);
        Console.WriteLine($"\nAvvio della ricerca {algorithmType} da '{startString}' a '{targetString}'...");
        RegoleMIUManager.OnSolutionFound += RegoleMIUManager_OnSolutionFound;
        RegoleMIUManager.OnRuleApplied += RegoleMIUManager_OnRuleApplied;
        RegoleMIUManager.OnNewMiuStringDiscoveredInternal += RegoleMIUManager_OnNewMiuStringDiscoveredInternal;

        // CORREZIONE: Chiama direttamente i nuovi metodi che hai appena creato
        string searchId = null;
        if (algorithmType == SearchAlgorithmType.BFS)
        {
            //pathStepInfos = RegoleMIUManager.TrovaDerivazioneBFS(long.MaxValue, deflateStartString, deflateTargetStrin, _cancellationTokenSource.Token, iMiuDataManagerInstance, engine);
        }
        else if (algorithmType == SearchAlgorithmType.DFS)
        {
            // pathStepInfos = RegoleMIUManager.TrovaDerivazioneDFS(long.MaxValue, deflateStartString, deflateTargetStrin, iMiuDataManagerInstance, _cancellationTokenSource.Token);
        }

        if (searchId != null)
        {
            Console.WriteLine($"Ricerca avviata con SearchID: {searchId}");
        }
        else
        {
            Console.WriteLine("Impossibile avviare la ricerca. Controlla i log per i dettagli.");
        }
    }
    /// <summary>
    /// Mostra le opzioni del menu all'utente.
    /// </summary>
    private static void DisplayMenu()
    {
        Console.WriteLine("--- MENU PRINCIPALE ---");
        Console.WriteLine("1. Carica e visualizza Regole MIU");
        Console.WriteLine("2. Carica e visualizza Parametri di Configurazione");
        Console.WriteLine("3. Avvia Flusso di Derivazione MIU");
        Console.WriteLine("4. Carica tabella MIU_States");
        Console.WriteLine("5. Avvia Derivazione Manuale (DFS)");
        Console.WriteLine("6. Avvia Derivazione Manuale (BFS)");
        Console.WriteLine("7. Costruisci la Topologia del Sistema"); // 2025/07/15: Etichetta aggiornata
        Console.WriteLine("8. Simula Rilevamento Anomalia"); // 2025/07/15: Etichetta aggiornata
        Console.WriteLine("9 Cancella dati tabelle");
        Console.WriteLine("10. Esci");
        Console.Write("Scegli un'opzione: ");
    }
}
