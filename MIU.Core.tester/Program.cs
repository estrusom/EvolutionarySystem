// File: C:\Progetti\EvolutiveSystem\MIU.Core.tester\Program.cs
// Data di riferimento: 20 giugno 2025 (Aggiornamento Finale)
// CORREZIONE 20.6.25: Posizione corretta di PathStepInfo (in MIU.Core, non in Common).
//                     Reintroduzione di configParams.
//                     Qualificazione precisa di tutti i tipi.
// NUOVA CORREZIONE 20.6.25: Correzione delle firme degli event handler per SolutionFoundEventArgs
//                           e RuleAppliedEventArgs (non sono annidati in RegoleMIUManager).

// Data di riferimento: 20 giugno 2025
// Questo file contiene la logica principale per testare il sistema MIU,
// inclusa la selezione automatica dell'algoritmo di ricerca (BFS/DFS)
// e la persistenza dei dati delle ricerche e delle statistiche delle regole.


using EvolutiveSystem.SQL.Core; // Per SQLiteSchemaLoader, MIUDatabaseManager
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MIU.Core; // Per MIURepository, IMIUDataManager, MIUStringConverter, RegoleMIUManager, SolutionFoundEventArgs, RuleAppliedEventArgs, PathStepInfo
using MasterLog; // Necessario per la tua classe Logger
using EvolutiveSystem.Common; // Per RegolaMIU, RuleStatistics, TransitionStatistics


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

        // Campi per i parametri di configurazione caricati dal DB, con valori predefiniti
        private static long _configuredMaxDepth = 10; // Valore predefinito per ProfonditaDiRicerca
        private static long _configuredMaxSteps = 10; // Valore predefinito per MassimoPassiRicerca

        static void Main(string[] args)
        {
            // Inizializzazione del Logger
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _logger = new Logger(logDirectory, "MIULog", 7); // Conserva gli ultimi 7 giorni di log
            _logger.SwLogLevel = _logger.LOG_INFO | _logger.LOG_DEBUG | _logger.LOG_ERROR | _logger.LOG_WARNING; // Imposta i livelli di log attivi

            // Propaga il logger a RegoleMIUManager (che è statico)
            RegoleMIUManager.LoggerInstance = _logger;


            string[,] arrayString =
            {
                {"MI", "MI", "MI", "MI", "MI", "MI", "MIIIIII","MII","MUI", "MI"},
                {"MIU","MII","MIIII","MUI","MUIU","MUIIU", "MUU", "MU","MIU","MIIIIIIIII"}
            };

            string databaseFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";

            int tipotest = 7; // Impostato a 7 per eseguire il test di persistenza con la ricerca BFS specifica

            // Inizializzazione comune del repository e caricamento delle statistiche
            SQLiteSchemaLoader _schemaLoader = new SQLiteSchemaLoader(databaseFilePath, _logger);
            MIUDatabaseManager _dbManager = new MIUDatabaseManager(_schemaLoader, _logger);
            IMIUDataManager _dataManager = (IMIUDataManager)_dbManager;
            _repository = new MIURepository(_dataManager, _logger); // Assegna al campo statico

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


            // Carica le statistiche delle regole all'avvio dell'applicazione
            _ruleStatistics = _repository.LoadRuleStatistics();
            if (_ruleStatistics == null)
            {
                _ruleStatistics = new System.Collections.Generic.Dictionary<long, EvolutiveSystem.Common.RuleStatistics>();
            }
            _logger.Log(LogLevel.INFO, $"[Program INFO] Caricate {_ruleStatistics.Count} RuleStatistics all'avvio.");

            RegoleMIUManager.CurrentRuleStatistics = _ruleStatistics;


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
                                    MIUStringConverter.CountChar(currentInitialStringCase3, 'I'),
                                    MIUStringConverter.CountChar(currentInitialStringCase3, 'U'),
                                    MIUStringConverter.CountChar(currentTargetStringCase3, 'I'),
                                    MIUStringConverter.CountChar(currentTargetStringCase3, 'U')
                                );

                                // Le stringhe da passare a TrovaDerivazione devono essere compresse
                                string compressedSource = MIUStringConverter.InflateMIUString(MIUstringsSource[1]);
                                string compressedTarget = MIUStringConverter.InflateMIUString(MIUstringDestination[1]);

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

                        string currentTestString = MIUStringConverter.DeflateMIUString(StringIn);
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
                                    MIUStringConverter.CountChar(currentInitialStringCase5, 'I'),
                                    MIUStringConverter.CountChar(currentInitialStringCase5, 'U'),
                                    MIUStringConverter.CountChar(currentTargetStringCase5, 'I'),
                                    MIUStringConverter.CountChar(currentTargetStringCase5, 'U')
                                );

                                // Compressed strings to pass to the intelligent derivation method
                                string compressedStart = MIUStringConverter.InflateMIUString(arrayString[0, y]);
                                string compressedTarget = MIUStringConverter.InflateMIUString(arrayString[1, y]);

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
                            MIUStringConverter.CountChar(testStartStringStandard, 'I'),
                            MIUStringConverter.CountChar(testStartStringStandard, 'U'),
                            MIUStringConverter.CountChar(testTargetStringStandard, 'I'),
                            MIUStringConverter.CountChar(testTargetStringStandard, 'U')
                        );

                        _logger.Log(LogLevel.INFO, $"--- Starting Specific Derivation Test (Automatic choice) with persistence: {testStartStringStandard} -> {testTargetStringStandard} ---");

                        // Compressed strings to pass to the intelligent derivation method
                        string compressedSource = MIUStringConverter.InflateMIUString(testStartStringStandard);
                        string compressedTarget = MIUStringConverter.InflateMIUString(testTargetStringStandard);

                        // CALL THE INTELLIGENT METHOD
                        System.Collections.Generic.List<MIU.Core.PathStepInfo> miuPath = RegoleMIUManager.TrovaDerivazioneAutomatica(_currentSearchId, compressedSource, compressedTarget);

                        if (miuPath != null)
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
                        }
                        else
                        {
                            _logger.Log(LogLevel.INFO, $"\n--- No path found for '{testStartStringStandard}' -> '{testTargetStringStandard}' ---");
                        }
                        _logger.Log(LogLevel.INFO, "--- End Specific Derivation Test ---");
                    }
                    break;
            }
            Console.WriteLine("press any key");
            Console.ReadKey();

            // Save rule statistics at application shutdown
            if (_ruleStatistics != null)
            {
                _repository.SaveRuleStatistics(_ruleStatistics);
                _logger.Log(LogLevel.INFO, $"[Program INFO] Saved {_ruleStatistics.Count} RuleStatistics at shutdown.");
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

            _repository.UpdateSearch(e.SearchID, e.Success, e.ElapsedMilliseconds, e.StepsTaken, e.NodesExplored, e.MaxDepthReached);

            if (e.Success && e.SolutionPathSteps != null)
            {
                foreach (MIU.Core.PathStepInfo step in e.SolutionPathSteps)
                {
                    if (step.AppliedRuleID.HasValue)
                    {
                        long ruleId = step.AppliedRuleID.Value;
                        if (_ruleStatistics.ContainsKey(ruleId))
                        {
                            _ruleStatistics[ruleId].SuccessfulCount++;
                            _ruleStatistics[ruleId].RecalculateEffectiveness();
                            _ruleStatistics[ruleId].LastApplicationTimestamp = DateTime.Now;
                            _logger.Log(LogLevel.DEBUG, $"[Learning] Rule {ruleId} ({RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == ruleId)?.Nome ?? "Unknown"}) SuccessfulCount incremented to {_ruleStatistics[ruleId].SuccessfulCount}. Effectiveness: {_ruleStatistics[ruleId].EffectivenessScore:F4}");
                        }
                        else
                        {
                            _logger.Log(LogLevel.WARNING, $"[Learning] Rule {ruleId} found in successful path but not in _ruleStatistics. Creating new entry.");
                            _ruleStatistics[ruleId] = new EvolutiveSystem.Common.RuleStatistics
                            {
                                RuleID = ruleId,
                                ApplicationCount = 0,
                                SuccessfulCount = 1,
                                LastApplicationTimestamp = DateTime.Now
                            };
                            _ruleStatistics[ruleId].RecalculateEffectiveness();
                        }
                    }
                }
            }

            if (e.Success && e.SolutionPathSteps != null)
            {
                for (int i = 0; i < e.SolutionPathSteps.Count; i++)
                {
                    MIU.Core.PathStepInfo currentStep = e.SolutionPathSteps[i];

                    long currentStateId = _repository.UpsertMIUState(currentStep.StateStringStandard);

                    long? parentStateId = null;
                    if (currentStep.ParentStateStringStandard != null)
                    {
                        parentStateId = _repository.UpsertMIUState(currentStep.ParentStateStringStandard);
                    }

                    bool isTarget = (currentStep.StateStringStandard == MIUStringConverter.DeflateMIUString(e.TargetString));

                    _repository.InsertSolutionPathStep(
                        e.SearchID,
                        i + 1,
                        currentStateId,
                        parentStateId,
                        currentStep.AppliedRuleID,
                        isTarget,
                        e.Success,
                        i
                    );
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
                _logger.Log(LogLevel.WARNING, $"[Learning] Rule {ruleId} found in _ruleStatistics but not in _ruleStatistics. Creating new entry.");
                _ruleStatistics[ruleId] = new EvolutiveSystem.Common.RuleStatistics { RuleID = ruleId };
                _logger.Log(LogLevel.DEBUG, $"[Learning] Created new RuleStatistics entry for rule {ruleId}.");
            }
            _ruleStatistics[ruleId].ApplicationCount++;
            _ruleStatistics[ruleId].LastApplicationTimestamp = DateTime.Now;
            _logger.Log(LogLevel.DEBUG, $"[Learning] Rule {ruleId} ({RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == ruleId)?.Nome ?? "Unknown"}) ApplicationCount incremented to {_ruleStatistics[ruleId].ApplicationCount}.");
        }

        // Method for DFS search (existing, not modified for statistics in this step)
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
