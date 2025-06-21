// File: C:\Progetti\EvolutiveSystem\MIU.Core.tester\Program.cs
// Data di riferimento: 20 giugno 2025 (Aggiornamento Finale)
// CORREZIONE 20.6.25: Posizione corretta di PathStepInfo (in MIU.Core, non in Common).
//                    Reintroduzione di configParams.
//                    Qualificazione precisa di tutti i tipi.
// NUOVA CORREZIONE 20.6.25: Correzione delle firme degli event handler per SolutionFoundEventArgs
//                        e RuleAppliedEventArgs (non sono annidati in RegoleMIUManager).
// AGGIORNATO 20.06.2025: Integrazione di LearningStatisticsManager per le TransitionStatistics
//                        e gestione degli aggiornamenti delle statistiche in caso di successo.
//                        Corretti i problemi di metodi di estensione (.Last()) e accesso alle proprietà di PathStepInfo.

// Data di riferimento: 20 giugno 2025
// Questo file contiene la logica principale per testare il sistema MIU,
// inclusa la selezione automatica dell'algoritmo di ricerca (BFS/DFS)
// e la persistenza dei dati delle ricerche e delle statistiche delle regole.


using EvolutiveSystem.SQL.Core; // Per SQLiteSchemaLoader, MIUDatabaseManager
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq; // NECESSARIO per i metodi di estensione LINQ come .Last() e .Any()
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MIU.Core; // Per MIURepository, IMIUDataManager, MIUStringConverter, RegoleMIUManager, SolutionFoundEventArgs, RuleAppliedEventArgs, PathStepInfo
using MasterLog; // Necessario per la tua classe Logger
using EvolutiveSystem.Common; // Per RegolaMIU, RuleStatistics, TransitionStatistics
using EvolutiveSystem.Learning; // NUOVO: Per LearningStatisticsManager

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
        private static MIURepository _repository; // Repository per la persistenza (reso statico per gli eventi)
        private static long _currentSearchId; // ID della ricerca corrente per correlare eventi (reso statico)

        // DICHIARAZIONE AGGIORNATA E QUALIFICATA (RuleStatistics è in Common)
        private static System.Collections.Generic.Dictionary<long, EvolutiveSystem.Common.RuleStatistics> _ruleStatistics; // Statistiche delle regole (RuleID è long)
        // NUOVO: Statistiche delle transizioni per la topografia dinamica
        private static System.Collections.Generic.Dictionary<Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics> _transitionStatistics;

        // Campi per i parametri di configurazione caricati dal DB, con valori predefiniti
        private static long _configuredMaxDepth = 10; // Valore predefinito per ProfonditaDiRicerca
        private static long _configuredMaxSteps = 10; // Valore predefinito per MassimoPassiRicerca

        // NUOVO: Istanza di LearningStatisticsManager per gestire le statistiche di apprendimento
        private static LearningStatisticsManager _learningStatsManager;


        static void Main(string[] args)
        {
            // Inizializzazione del Logger (mantengo la tua implementazione originale)
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _logger = new Logger(logDirectory, "MIULog", 7); // Conserva gli ultimi 7 giorni di log
            _logger.SwLogLevel = _logger.LOG_INFO | _logger.LOG_DEBUG | _logger.LOG_ERROR | _logger.LOG_WARNING; // Imposta i livelli di log attivi

            // Propaga il logger a RegoleMIUManager (che è statico)
            RegoleMIUManager.LoggerInstance = _logger;
            _logger.Log(LogLevel.INFO, "Applicazione avviata."); // Messaggio di log iniziale

            string[,] arrayString =
            {
                {"MI", "MI", "MI", "MI", "MI", "MI", "MIIIIII","MII","MUI", "MI"},
                {"MIU","MII","MIIII","MUI","MUIU","MUIIU", "MUU", "MU","MIU","MIIIIIIIII"}
            };

            string databaseFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";

            int tipotest = 7; // Impostato a 7 per eseguire il test di persistenza con la ricerca BFS specifica

            // Inizializzazione comune del repository e caricamento delle statistiche
            SQLiteSchemaLoader _schemaLoader = new SQLiteSchemaLoader(databaseFilePath, _logger);
            // Assicurati che SQLiteSchemaLoader contenga un metodo pubblico InitializeDatabase()
            _schemaLoader.InitializeDatabase(); // <- errore cs1061
            _logger.Log(LogLevel.INFO, "Database inizializzato tramite SQLiteSchemaLoader.");

            MIUDatabaseManager _dbManager = new MIUDatabaseManager(_schemaLoader, _logger);
            IMIUDataManager _dataManager = _dbManager;
            _repository = new MIURepository(_dataManager, _logger); // Assegna al campo statico
            _logger.Log(LogLevel.INFO, "MIUDatabaseManager e MIURepository istanziati.");

            // NUOVO: Inizializzazione di LearningStatisticsManager
            _learningStatsManager = new LearningStatisticsManager(_dataManager, _logger);
            _logger.Log(LogLevel.INFO, "LearningStatisticsManager istanziato.");

            // Carica i parametri di configurazione dal database
            System.Collections.Generic.Dictionary<string, string> configParams = _repository.LoadMIUParameterConfigurator();

            long parsedDepth = _configuredMaxDepth; // Inizializza con il valore predefinito
            if (configParams.TryGetValue("ProfonditaDiRicerca", out string depthStr) && long.TryParse(depthStr, out parsedDepth))
            {
                _configuredMaxDepth = parsedDepth;
                _logger.Log(LogLevel.INFO, $"[Program INFO] Caricato ProfonditaDiRicerca dal DB: {_configuredMaxDepth}");
            }
            else
            {
                _logger.Log(LogLevel.WARNING, $"[Program WARNING] Parametro 'ProfonditaDiRicerca' non trovato o non valido. Usato valore predefinito: {_configuredMaxDepth}");
            }

            long parsedSteps = _configuredMaxSteps; // Inizializza con il valore predefinito
            if (configParams.TryGetValue("MassimoPassiRicerca", out string stepsStr) && long.TryParse(stepsStr, out parsedSteps))
            {
                _configuredMaxSteps = parsedSteps;
                _logger.Log(LogLevel.INFO, $"[Program INFO] Caricato MassimoPassiRicerca dal DB: {_configuredMaxSteps}");
            }
            else
            {
                _logger.Log(LogLevel.WARNING, $"[Program WARNING] Parametro 'MassimoPassiRicerca' non trovato o non valido. Usato valore predefinito: {_configuredMaxSteps}");
            }

            // Imposta le proprietà statiche in RegoleMIUManager con i valori caricati
            RegoleMIUManager.MaxProfonditaRicerca = _configuredMaxDepth;
            RegoleMIUManager.MassimoPassiRicerca = _configuredMaxSteps;
            _logger.Log(LogLevel.INFO, $"[Program INFO] RegoleMIUManager impostato con MaxProfonditaRicerca: {RegoleMIUManager.MaxProfonditaRicerca} e MassimoPassiRicerca: {RegoleMIUManager.MassimoPassiRicerca}");


            // Carica le statistiche delle regole all'avvio dell'applicazione (tramite _repository, che userà dataManager)
            _ruleStatistics = _repository.LoadRuleStatistics();
            if (_ruleStatistics == null)
            {
                _ruleStatistics = new System.Collections.Generic.Dictionary<long, EvolutiveSystem.Common.RuleStatistics>();
            }
            _logger.Log(LogLevel.INFO, $"[Program INFO] Caricate {_ruleStatistics.Count} RuleStatistics all'avvio.");

            // Carica le statistiche di transizione aggregate tramite LearningStatisticsManager
            _transitionStatistics = _learningStatsManager.GetTransitionProbabilities();
            if (_transitionStatistics == null)
            {
                _transitionStatistics = new System.Collections.Generic.Dictionary<Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics>();
            }
            _logger.Log(LogLevel.INFO, $"[Program INFO] Caricate {_transitionStatistics.Count} TransitionStatistics all'avvio.");

            // Imposta le proprietà statiche in RegoleMIUManager
            RegoleMIUManager.CurrentRuleStatistics = _ruleStatistics;
            RegoleMIUManager.CurrentTransitionStatistics = _transitionStatistics;


            // Collega gli handler degli eventi
            RegoleMIUManager.OnRuleApplied += RegoleMIUManager_OnRuleApplied;
            RegoleMIUManager.OnSolutionFound += RegoleMIUManager_OnSolutionFound;


            Process myProc = new Process(); // Questa variabile non è usata e può essere rimossa se non serve altrove.
            switch (tipotest)
            {
                case 3:
                    {
                        Random rnd = new Random();

                        System.Collections.Generic.List<EvolutiveSystem.Common.RegolaMIU> regoleMIUList = _repository.LoadRegoleMIU();
                        RegoleMIUManager.CaricaRegoleDaOggettoRepository(regoleMIUList);

                        System.Collections.Generic.List<string> MIUstringList = _schemaLoader.SQLiteSelect("SELECT StateID, CurrentString, StringLength, Hash, DiscoveryTime_Int, DiscoveryTime_Text, UsageCount FROM MIU_States;");

                        if (RegoleMIUManager.Regole.Count > 0)
                        {
                            for (int i = 0; i < 5; i++) // Limit to 5 iterations for quicker testing
                            {
                                int indexSource = rnd.Next(0, MIUstringList.Count);
                                int indexTarget = rnd.Next(0, MIUstringList.Count);
                                string[] MIUstringsSource = MIUstringList[indexSource].Split(';');
                                string[] MIUstringDestination = MIUstringList[indexTarget].Split(';');

                                // Inserisci la ricerca iniziale e ottieni l'ID
                                string currentInitialStringCase3 = MIUstringsSource[1]; // Stringa standard
                                string currentTargetStringCase3 = MIUstringDestination[1]; // Stringa standard

                                _currentSearchId = _repository.InsertSearch(
                                    currentInitialStringCase3,
                                    currentTargetStringCase3,
                                    "AUTO-RANDOM", // Changed search type for tracking
                                    currentInitialStringCase3.Length,
                                    currentTargetStringCase3.Length,
                                    MIUStringConverter.CountChar(currentInitialStringCase3, 'I'), // Usa CountChar
                                    MIUStringConverter.CountChar(currentInitialStringCase3, 'U'), // Usa CountChar
                                    MIUStringConverter.CountChar(currentTargetStringCase3, 'I'), // Usa CountChar
                                    MIUStringConverter.CountChar(currentTargetStringCase3, 'U')  // Usa CountChar
                                );

                                // Le stringhe da passare a TrovaDerivazione devono essere compresse
                                string compressedSource = MIUStringConverter.DeflateMIUString(MIUstringsSource[1]);
                                string compressedTarget = MIUStringConverter.DeflateMIUString(MIUstringDestination[1]);

                                // CHIAMA IL METODO INTELLIGENTE
                                System.Collections.Generic.List<MIU.Core.PathStepInfo> miu = RegoleMIUManager.TrovaDerivazioneAutomatica(_currentSearchId, compressedSource, compressedTarget);
                            }
                        }
                        else
                        {
                            Console.WriteLine("[DEBUG] No MIU rules loaded from repository. Check database data.");
                            _logger.Log(LogLevel.DEBUG, "[DEBUG] No MIU rules loaded from repository. Check database data.");
                        }
                    }
                    break;
                case 4:
                    {
                        System.Collections.Generic.List<string> regole = _schemaLoader.SQLiteSelect("SELECT ID, Nome, Pattern, Sostituzione, Descrizione FROM RegoleMIU;");
                        string StringIn = "M2U3I4MI";
                        RegoleMIUManager.CaricaRegoleDaOggettoSQLite(regole);

                        string currentTestString = MIUStringConverter.InflateMIUString(StringIn);
                        string regola0Output = string.Empty;
                        bool response0 = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == 0)?.TryApply(currentTestString, out regola0Output) ?? false;
                        Console.WriteLine($"String in: {currentTestString} Regola 0: {regola0Output} response: {response0}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 0: {regola0Output} response: {response0}");

                        currentTestString = MIUStringConverter.DeflateMIUString(MIUStringConverter.InflateMIUString(regola0Output ?? string.Empty));
                        string regola1Output = string.Empty;
                        bool response1 = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == 1)?.TryApply(currentTestString, out regola1Output) ?? false;
                        Console.WriteLine($"String in: {currentTestString} Regola 1: {regola1Output} response: {response1}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 1: {regola1Output} response: {response1}");

                        currentTestString = MIUStringConverter.DeflateMIUString(MIUStringConverter.InflateMIUString(regola1Output ?? string.Empty));
                        string regola2Output = string.Empty;
                        bool response2 = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == 2)?.TryApply(currentTestString, out regola2Output) ?? false;
                        Console.WriteLine($"String in: {currentTestString} Regola 2: {regola2Output} response: {response2}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 2: {regola2Output} response: {response2}");

                        currentTestString = MIUStringConverter.DeflateMIUString(MIUStringConverter.InflateMIUString(regola2Output ?? string.Empty));
                        string regola3Output = string.Empty;
                        bool response3 = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == 3)?.TryApply(currentTestString, out regola3Output) ?? false;
                        Console.WriteLine($"String in: {currentTestString} Regola 3: {regola3Output} response: {response3}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 3: {regola3Output} response: {response3}");
                    }
                    break;
                case 5:
                    {
                        Random rnd = new Random();

                        System.Collections.Generic.List<EvolutiveSystem.Common.RegolaMIU> regoleMIUList = _repository.LoadRegoleMIU();
                        RegoleMIUManager.CaricaRegoleDaOggettoRepository(regoleMIUList);

                        int cntDwn = 2; // Reduced for quicker testing
                        while (cntDwn >= 1)
                        {
                            for (int y = 0; y < arrayString.GetLength(1); y++)
                            {
                                // Insert initial search and get ID
                                string currentInitialStringCase5 = arrayString[0, y]; // Standard string
                                string currentTargetStringCase5 = arrayString[1, y]; // Standard string

                                _currentSearchId = _repository.InsertSearch(
                                    currentInitialStringCase5,
                                    currentTargetStringCase5,
                                    "AUTO-ARRAY", // Changed search type for tracking
                                    currentInitialStringCase5.Length,
                                    currentTargetStringCase5.Length,
                                    MIUStringConverter.CountChar(currentInitialStringCase5, 'I'), // Usa CountChar
                                    MIUStringConverter.CountChar(currentInitialStringCase5, 'U'), // Usa CountChar
                                    MIUStringConverter.CountChar(currentTargetStringCase5, 'I'), // Usa CountChar
                                    MIUStringConverter.CountChar(currentTargetStringCase5, 'U')  // Usa CountChar
                                );

                                // Compressed strings to pass to the intelligent derivation method
                                string compressedStart = MIUStringConverter.DeflateMIUString(arrayString[0, y]);
                                string compressedTarget = MIUStringConverter.DeflateMIUString(arrayString[1, y]);

                                // CHIAMA IL METODO INTELLIGENTE
                                System.Collections.Generic.List<MIU.Core.PathStepInfo> miu = RegoleMIUManager.TrovaDerivazioneAutomatica(_currentSearchId, compressedStart, compressedTarget);
                            }
                            cntDwn--;
                        }
                    }
                    break;
                case 6:
                    {
                        Random r = new Random();
                        for (int i = 0; i < 10; i++)
                        {
                            Console.Write(r.Next(1, 3));
                            _logger.Log(LogLevel.INFO, $"Random number: {r.Next(1, 3)}");
                            Console.WriteLine("Hit any key");
                            Console.ReadKey();
                        }
                    }
                    break;
                case 7: // Your specific BFS persistence test
                    {
                        Random rnd = new Random();

                        System.Collections.Generic.List<EvolutiveSystem.Common.RegolaMIU> regoleMIUList = _repository.LoadRegoleMIU();
                        RegoleMIUManager.CaricaRegoleDaOggettoRepository(regoleMIUList);

                        // Specific strings for BFS test
                        string testStartStringStandard = "MUUIIIMMMMI";
                        string testTargetStringStandard = "MUMMMMIUMUMMMMIU";

                        // Insert initial search and get ID
                        _currentSearchId = _repository.InsertSearch(
                            testStartStringStandard,
                            testTargetStringStandard,
                            "AUTO-SPECIFIC", // Changed search type for tracking
                            testStartStringStandard.Length,
                            testTargetStringStandard.Length,
                            MIUStringConverter.CountChar(testStartStringStandard, 'I'), // Usa CountChar
                            MIUStringConverter.CountChar(testStartStringStandard, 'U'), // Usa CountChar
                            MIUStringConverter.CountChar(testTargetStringStandard, 'I'), // Usa CountChar
                            MIUStringConverter.CountChar(testTargetStringStandard, 'U')  // Usa CountChar
                        );

                        _logger.Log(LogLevel.INFO, $"--- Starting Specific Derivation Test (Automatic choice) with persistence: {testStartStringStandard} -> {testTargetStringStandard} ---");

                        // Compressed strings to pass to the intelligent derivation method
                        string compressedSource = MIUStringConverter.DeflateMIUString(testStartStringStandard);
                        string compressedTarget = MIUStringConverter.DeflateMIUString(testTargetStringStandard);

                        // CALL THE INTELLIGENT METHOD
                        System.Collections.Generic.List<MIU.Core.PathStepInfo> miuPath = RegoleMIUManager.TrovaDerivazioneAutomatica(_currentSearchId, compressedSource, compressedTarget);

                        if (miuPath != null && miuPath.Any()) // Correzione per .Any() e Last()
                        {
                            _logger.Log(LogLevel.INFO, $"\n--- Path found for '{testStartStringStandard}' -> '{testTargetStringStandard}': ---");
                            foreach (MIU.Core.PathStepInfo step in miuPath)
                            {
                                // Retrieve standard string for logging
                                string logMessage = $"State: {step.StateStringStandard}";
                                if (step.AppliedRuleID.HasValue)
                                {
                                    EvolutiveSystem.Common.RegolaMIU appliedRule = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == step.AppliedRuleID.Value);
                                    logMessage += $", Applied Rule: {appliedRule?.Nome ?? "Unknown"} (ID: {step.AppliedRuleID.Value})";
                                }
                                if (step.ParentStateStringStandard != null)
                                {
                                    logMessage += $", Parent: {step.ParentStateStringStandard}";
                                }
                                _logger.Log(LogLevel.INFO, logMessage);
                            }
                            // Aggiorna la ricerca nel DB solo se miuPath non è nullo e non è vuoto
                            _repository.UpdateSearch(_currentSearchId, true,
                                miuPath.Last().ElapsedMilliseconds, // <- errore cs1061 Assicurati che PathStepInfo abbia ElapsedMilliseconds
                                miuPath.Count - 1,
                                miuPath.Last().NodesExplored, // <- errore cs1061 Assicurati che PathStepInfo abbia NodesExplored
                                miuPath.Last().MaxDepthReached); // <- errore cs1061 Assicurati che PathStepInfo abbia MaxDepthReached

                            // AGGIORNAMENTO DELLE STATISTICHE DI APPRENDIMENTO DOPO IL SUCCESSO
                            foreach (MIU.Core.PathStepInfo step in miuPath)
                            {
                                if (step.AppliedRuleID.HasValue && step.ParentStateStringStandard != null)
                                {
                                    // Aggiorna RuleStatistics (il campo _ruleStatistics, che poi verrà salvato da _repository)
                                    if (_ruleStatistics.TryGetValue(step.AppliedRuleID.Value, out RuleStatistics ruleStats))
                                    {
                                        ruleStats.ApplicationCount++;
                                        ruleStats.SuccessfulCount++;
                                        ruleStats.LastApplicationTimestamp = DateTime.Now;
                                        ruleStats.RecalculateEffectiveness(); // Assicurati che RecalculateEffectiveness() sia pubblico
                                    }
                                    else
                                    {
                                        _ruleStatistics[step.AppliedRuleID.Value] = new EvolutiveSystem.Common.RuleStatistics
                                        {
                                            RuleID = step.AppliedRuleID.Value,
                                            ApplicationCount = 1,
                                            SuccessfulCount = 1,
                                            LastApplicationTimestamp = DateTime.Now
                                        };
                                        _ruleStatistics[step.AppliedRuleID.Value].RecalculateEffectiveness();
                                    }

                                    // Aggiorna TransitionStatistics (il campo _transitionStatistics)
                                    var parentCompressed = MIUStringConverter.DeflateMIUString(step.ParentStateStringStandard);
                                    var transitionKey = Tuple.Create(parentCompressed, step.AppliedRuleID.Value);

                                    if (_transitionStatistics.TryGetValue(transitionKey, out TransitionStatistics transitionStats))
                                    {
                                        transitionStats.ApplicationCount++;
                                        transitionStats.SuccessfulCount++;
                                        transitionStats.LastUpdated = DateTime.Now;
                                        // SuccessRate viene ricalcolato automaticamente dalla proprietà get
                                    }
                                    else
                                    {
                                        _transitionStatistics[transitionKey] = new EvolutiveSystem.Common.TransitionStatistics
                                        {
                                            ParentStringCompressed = parentCompressed,
                                            AppliedRuleID = step.AppliedRuleID.Value,
                                            ApplicationCount = 1,
                                            SuccessfulCount = 1,
                                            LastUpdated = DateTime.Now
                                        };
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.INFO, $"\n--- No path found for '{testStartStringStandard}' -> '{testTargetStringStandard}' ---");
                            // Aggiorna la ricerca nel DB come fallita
                            _repository.UpdateSearch(_currentSearchId, false, 0, -1, 0, 0); // Valori default per fallimento
                        }
                        _logger.Log(LogLevel.INFO, "--- End Specific Derivation Test ---");
                    }
                    break;
            }
            Console.WriteLine("press any key");
            Console.ReadKey();

            // Salva le RuleStatistics tramite il repository (che gestisce _ruleStatistics)
            if (_ruleStatistics != null)
            {
                _repository.SaveRuleStatistics(_ruleStatistics);
                _logger.Log(LogLevel.INFO, $"[Program INFO] Saved {_ruleStatistics.Count} RuleStatistics at shutdown.");
            }
            // Salva le TransitionStatistics tramite LearningStatisticsManager
            if (_transitionStatistics != null)
            {
                _learningStatsManager.SaveTransitionStatistics(_transitionStatistics);
                _logger.Log(LogLevel.INFO, $"[Program INFO] Saved {_transitionStatistics.Count} TransitionStatistics at shutdown.");
            }
        }

        private static void RegoleMIUManager_OnSolutionFound(object sender, MIU.Core.SolutionFoundEventArgs e)
        {
            string pathString = "N/A";
            if (e.SolutionPathSteps != null && e.SolutionPathSteps.Any())
            {
                pathString = string.Join(" -> ", e.SolutionPathSteps.Select(step => step.StateStringStandard));
            }

            string message = $"SearchID: {e.SearchID} ElapsedMilliseconds: {e.ElapsedMilliseconds} ElapsedTicks: {e.ElapsedTicks} InitialString: {e.InitialString} MaxDepthReached: {e.MaxDepthReached} NodesExplored: {e.NodesExplored} Path: {pathString} StepsTaken: {e.StepsTaken} Success: {e.Success} TargetString: {e.TargetString} Algorithm: {e.SearchAlgorithmUsed}"; // ADDED ALG.
            Console.WriteLine(message);
            _logger.Log(e.Success ? LogLevel.INFO : LogLevel.WARNING, message);

            // _repository.UpdateSearch(e.SearchID, e.Success, e.ElapsedMilliseconds, e.StepsTaken, e.NodesExplored, e.MaxDepthReached);
            // Questo update è stato spostato nel Main per una gestione centralizzata dopo la ricerca completa.
            // Se lo lasci qui, lo farai due volte.

            if (e.Success && e.SolutionPathSteps != null)
            {
                foreach (MIU.Core.PathStepInfo step in e.SolutionPathSteps)
                {
                    // Aggiornamento RuleStatistics per ogni regola applicata nel percorso di successo
                    if (step.AppliedRuleID.HasValue)
                    {
                        long ruleId = step.AppliedRuleID.Value;
                        if (_ruleStatistics.TryGetValue(ruleId, out RuleStatistics ruleStats))
                        {
                            ruleStats.SuccessfulCount++;
                            ruleStats.ApplicationCount++; // Incrementa anche qui, se OnRuleApplied non è sufficiente
                            ruleStats.RecalculateEffectiveness();
                            ruleStats.LastApplicationTimestamp = DateTime.Now;
                            _logger.Log(LogLevel.DEBUG, $"[Learning] Rule {ruleId} ({RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == ruleId)?.Nome ?? "Unknown"}) SuccessfulCount incremented to {ruleStats.SuccessfulCount}. Effectiveness: {ruleStats.EffectivenessScore:F4}");
                        }
                        else
                        {
                            _logger.Log(LogLevel.WARNING, $"[Learning] Rule {ruleId} found in successful path but not in _ruleStatistics. Creating new entry.");
                            _ruleStatistics[ruleId] = new EvolutiveSystem.Common.RuleStatistics
                            {
                                RuleID = ruleId,
                                ApplicationCount = 1, // È stata applicata per trovare la soluzione
                                SuccessfulCount = 1,
                                LastApplicationTimestamp = DateTime.Now
                            };
                            _ruleStatistics[ruleId].RecalculateEffectiveness();
                        }

                        // Aggiornamento TransitionStatistics per ogni transizione nel percorso di successo
                        if (step.ParentStateStringStandard != null)
                        {
                            var parentCompressed = MIUStringConverter.DeflateMIUString(step.ParentStateStringStandard);
                            var transitionKey = Tuple.Create(parentCompressed, step.AppliedRuleID.Value);

                            if (_transitionStatistics.TryGetValue(transitionKey, out TransitionStatistics transitionStats))
                            {
                                transitionStats.SuccessfulCount++;
                                transitionStats.ApplicationCount++; // Incrementa anche qui, se OnRuleApplied non è sufficiente
                                transitionStats.LastUpdated = DateTime.Now;
                                // SuccessRate viene ricalcolato automaticamente dalla proprietà get
                            }
                            else
                            {
                                _transitionStatistics[transitionKey] = new EvolutiveSystem.Common.TransitionStatistics
                                {
                                    ParentStringCompressed = parentCompressed,
                                    AppliedRuleID = step.AppliedRuleID.Value,
                                    ApplicationCount = 1,
                                    SuccessfulCount = 1,
                                    LastUpdated = DateTime.Now
                                };
                            }
                        }
                    }
                }
            }
        }

        private static void RegoleMIUManager_OnRuleApplied(object sender, MIU.Core.RuleAppliedEventArgs e)
        {
            string message = $"AppliedRuleID: {e.AppliedRuleID} AppliedRuleName: {e.AppliedRuleName} OriginalString: {e.OriginalString} NewString: {e.NewString} CurrentDepth: {e.CurrentDepth}";
            Console.WriteLine(message);
            _logger.Log(LogLevel.DEBUG, message);

            long parentStateId = _repository.UpsertMIUState(e.OriginalString);
            long newStateId = _repository.UpsertMIUState(e.NewString);

            _repository.InsertRuleApplication(
                _currentSearchId,
                parentStateId,
                newStateId,
                e.AppliedRuleID,
                e.CurrentDepth
            );

            long ruleId = e.AppliedRuleID;
            if (!_ruleStatistics.ContainsKey(ruleId))
            {
                _logger.Log(LogLevel.WARNING, $"[Learning] Rule {ruleId} not found in _ruleStatistics. Creating new entry.");
                _ruleStatistics[ruleId] = new EvolutiveSystem.Common.RuleStatistics { RuleID = ruleId };
                _logger.Log(LogLevel.DEBUG, $"[Learning] Created new RuleStatistics entry for rule {ruleId}.");
            }
            _ruleStatistics[ruleId].ApplicationCount++; // Incrementa il conteggio di applicazioni ogni volta che la regola viene tentata
            _ruleStatistics[ruleId].LastApplicationTimestamp = DateTime.Now;
            _ruleStatistics[ruleId].RecalculateEffectiveness(); // Ricalcola anche qui se vuoi un valore aggiornato in tempo reale
            _logger.Log(LogLevel.DEBUG, $"[Learning] Rule {ruleId} ({RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == ruleId)?.Nome ?? "Unknown"}) ApplicationCount incremented to {_ruleStatistics[ruleId].ApplicationCount}.");

            // Aggiornamento TransitionStatistics in OnRuleApplied (per tutte le applicazioni, anche quelle fallite)
            if (e.OriginalString != null) // OriginalString è lo stato genitore
            {
                var parentCompressed = MIUStringConverter.DeflateMIUString(e.OriginalString);
                var transitionKey = Tuple.Create(parentCompressed, e.AppliedRuleID);

                if (_transitionStatistics.TryGetValue(transitionKey, out TransitionStatistics transitionStats))
                {
                    transitionStats.ApplicationCount++;
                    transitionStats.LastUpdated = DateTime.Now;
                }
                else
                {
                    _transitionStatistics[transitionKey] = new EvolutiveSystem.Common.TransitionStatistics
                    {
                        ParentStringCompressed = parentCompressed,
                        AppliedRuleID = e.AppliedRuleID,
                        ApplicationCount = 1,
                        SuccessfulCount = 0, // Inizialmente 0, incrementato solo in OnSolutionFound
                        LastUpdated = DateTime.Now
                    };
                }
            }
        }

        // Metodo per DFS search (existing, not modified for statistics in this step)
        // This method is now effectively a wrapper to allow direct calls if needed,
        // but the main entry point will be TrovaDerivazioneAutomatica.
        private static void RicercaDiDerivazioneDFS(long searchId, string startStringCompressed, string targetStringCompressed)
        {
            // This method now directly calls RegoleMIUManager.TrovaDerivazioneDFS
            // which in turn invokes the OnSolutionFound event with the specified algorithm.
            RegoleMIUManager.TrovaDerivazioneDFS(searchId, startStringCompressed, targetStringCompressed);
        }

        // Method for BFS search (existing, not modified for statistics in this step)
        // This method is now effectively a wrapper to allow direct calls if needed,
        // but the main entry point will be TrovaDerivazioneAutomatica.
        private static void RicercaDiDerivazioneBFS(long searchId, string inizioCompressed, string fineCompressed)
        {
            // This method now directly calls RegoleMIUManager.TrovaDerivazioneBFS
            // which in turn invokes the OnSolutionFound event with the specified algorithm.
            RegoleMIUManager.TrovaDerivazioneBFS(searchId, inizioCompressed, fineCompressed);
        }
    }
}
