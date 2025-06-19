// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core.tester\Program.cs
// Data di riferimento: 21 giugno 2025
// AGGIORNAMENTO 21.6.25: Assegnazione definitiva delle RuleStatistics caricate a RegoleMIUManager.CurrentRuleStatistics.

using EvolutiveSystem.SQL.Core; // Per SQLiteSchemaLoader, Database, Table
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MIU.Core; // Per MIURepository, RuleStatistics, TransitionStatistics, IMIUDataManager, PathStepInfo
using MasterLog; // Necessario per la tua classe Logger

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
        // RIMOSSO: La costante 'passi' non è più hardcoded.
        // private const long passi = 10; // Massima profondità di ricerca per BFS (esempi preesistenti)

        private static Logger _logger; // Istanza del logger
        private static MIURepository _repository; // Repository per la persistenza (reso statico per gli eventi)
        private static long _currentSearchId; // ID della ricerca corrente per correlare eventi (reso statico)
        private static Dictionary<long, RuleStatistics> _ruleStatistics; // DICTIONARY AGGIUNTO: Statistiche delle regole (RuleID è long)

        // NUOVO: Campi per i parametri di configurazione caricati dal DB, con valori predefiniti
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
            var configParams = _repository.LoadMIUParameterConfigurator();
            if (configParams.TryGetValue("ProfonditaDiRicerca", out string depthStr) && long.TryParse(depthStr, out long parsedDepth))
            {
                _configuredMaxDepth = parsedDepth;
                _logger.Log(LogLevel.INFO, $"[Program INFO] Caricato ProfonditaDiRicerca dal DB: {_configuredMaxDepth}");
            }
            else
            {
                _logger.Log(LogLevel.WARNING, $"[Program WARNING] Parametro 'ProfonditaDiRicerca' non trovato o non valido. Usato valore predefinito: {_configuredMaxDepth}");
            }

            if (configParams.TryGetValue("MassimoPassiRicerca", out string stepsStr) && long.TryParse(stepsStr, out long parsedSteps))
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
                _ruleStatistics = new Dictionary<long, RuleStatistics>();
            }
            _logger.Log(LogLevel.INFO, $"[Program INFO] Caricate {_ruleStatistics.Count} RuleStatistics all'avvio.");

            // QUESTA È L'UNICA RIGA AGGIUNTA
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

                        List<RegolaMIU> regoleMIUList = _repository.LoadRegoleMIU();
                        RegoleMIUManager.CaricaRegoleDaOggettoRepository(regoleMIUList);

                        List<string> MIUstringList = _schemaLoader.SQLiteSelect("SELECT StateID, CurrentString, StringLength, Hash, DiscoveryTime_Int, DiscoveryTime_Text, UsageCount FROM MIU_States;");

                        if (RegoleMIUManager.Regole.Count > 0)
                        {
                            foreach (string s in MIUstringList)
                            {
                                string[] MIUstringsSource = s.Split(';');
                                int index = rnd.Next(0, MIUstringList.Count - 1); // Corretto l'indice massimo
                                string[] MIUstringDestination = MIUstringList[index].Split(';');

                                // Inserisci la ricerca iniziale e ottieni l'ID
                                _currentSearchId = _repository.InsertSearch(MIUstringsSource[1], MIUstringDestination[1], "BFS"); // Salva la stringa standard per initial/target

                                // Le stringhe da passare a TrovaDerivazioneBFS devono essere compresse
                                string compressedSource = MIUStringConverter.InflateMIUString(MIUstringsSource[1]);
                                string compressedTarget = MIUStringConverter.InflateMIUString(MIUstringDestination[1]);

                                // Rimosso l'argomento 'passi'. RegoleMIUManager ora usa la sua proprietà statica.
                                List<PathStepInfo> miu = RegoleMIUManager.TrovaDerivazioneBFS(_currentSearchId, compressedSource, compressedTarget);
                            }
                        }
                        else
                        {
                            Console.WriteLine("[DEBUG] Nessuna regola MIU caricata dal repository. Controllare i dati del database.");
                            _logger.Log(LogLevel.DEBUG, "[DEBUG] Nessuna regola MIU caricata dal repository. Controllare i dati del database.");
                        }
                    }
                    break;
                case 4:
                    {
                        List<string> regole = _schemaLoader.SQLiteSelect("SELECT ID, Nome, Pattern, Sostituzione, Descrizione FROM RegoleMIU;");
                        string StringIn = "M2U3I4MI";
                        RegoleMIUManager.CaricaRegoleDaOggettoSQLite(regole);

                        // Questo case è un test di applicazione regole senza ricerca e persistenza completa
                        // Quindi non avrà un SearchID correlato automaticamente.

                        string currentTestString = MIUStringConverter.DeflateMIUString(StringIn);
                        string regola0Output = string.Empty; // FIX: Inizializza la variabile
                        bool response0 = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == 0)?.TryApply(currentTestString, out regola0Output) ?? false;
                        Console.WriteLine($"String in: {currentTestString} Regola 0: {regola0Output} response: {response0}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 0: {regola0Output} response: {response0}");

                        currentTestString = MIUStringConverter.DeflateMIUString(MIUStringConverter.InflateMIUString(regola0Output ?? string.Empty));
                        string regola1Output = string.Empty; // FIX: Inizializza la variabile
                        bool response1 = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == 1)?.TryApply(currentTestString, out regola1Output) ?? false;
                        Console.WriteLine($"String in: {currentTestString} Regola 1: {regola1Output} response: {response1}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 1: {regola1Output} response: {response1}");

                        currentTestString = MIUStringConverter.DeflateMIUString(MIUStringConverter.InflateMIUString(regola1Output ?? string.Empty));
                        string regola2Output = string.Empty; // FIX: Inizializza la variabile
                        bool response2 = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == 2)?.TryApply(currentTestString, out regola2Output) ?? false;
                        Console.WriteLine($"String in: {currentTestString} Regola 2: {regola2Output} response: {response2}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 2: {regola2Output} response: {response2}");

                        currentTestString = MIUStringConverter.DeflateMIUString(MIUStringConverter.InflateMIUString(regola2Output ?? string.Empty));
                        string regola3Output = string.Empty; // FIX: Inizializza la variabile
                        bool response3 = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == 3)?.TryApply(currentTestString, out regola3Output) ?? false;
                        Console.WriteLine($"String in: {currentTestString} Regola 3: {regola3Output} response: {response3}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 3: {regola3Output} response: {response3}");
                    }
                    break;
                case 5:
                    {
                        List<RegolaMIU> regoleMIUList = _repository.LoadRegoleMIU();
                        RegoleMIUManager.CaricaRegoleDaOggettoRepository(regoleMIUList);

                        // RIMOSSO: La variabile locale 'maxProfondita' non è più usata.
                        // long maxProfondita = 10;
                        int cntDwn = 22;
                        while (cntDwn >= 1)
                        {
                            for (int y = 0; y < arrayString.GetLength(1); y++)
                            {
                                // Inserisci la ricerca iniziale e ottieni l'ID
                                _currentSearchId = _repository.InsertSearch(arrayString[0, y], arrayString[1, y], "DFS"); // Salva le stringhe standard

                                // Le stringhe da passare a TrovaDerivazioneDFS devono essere compresse
                                string compressedStart = MIUStringConverter.InflateMIUString(arrayString[0, y]);
                                string compressedTarget = MIUStringConverter.InflateMIUString(arrayString[1, y]);
                                // Rimosso l'argomento 'maxProfondita'. RegoleMIUManager ora usa la sua proprietà statica.
                                RicercaDiDerivazioneDFS(_currentSearchId, compressedStart, compressedTarget);
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
                case 7: // Il tuo test specifico per BFS con persistenza
                    {
                        Random rnd = new Random();

                        List<RegolaMIU> regoleMIUList = _repository.LoadRegoleMIU();
                        RegoleMIUManager.CaricaRegoleDaOggettoRepository(regoleMIUList);

                        // Stringhe specifiche per il test BFS
                        string testStartStringStandard = "MUUIIIMMMMI";
                        string testTargetStringStandard = "MUMMMMIUMUMMMMIU";

                        // Inserisci la ricerca iniziale e ottieni l'ID
                        _currentSearchId = _repository.InsertSearch(testStartStringStandard, testTargetStringStandard, "BFS"); // Salva le stringhe standard per initial/target

                        _logger.Log(LogLevel.INFO, $"--- Inizio Test Derivazione Specifica (BFS) con persistenza: {testStartStringStandard} -> {testTargetStringStandard} ---");

                        // Le stringhe da passare a TrovaDerivazioneBFS devono essere compresse
                        string compressedSource = MIUStringConverter.InflateMIUString(testStartStringStandard);
                        string compressedTarget = MIUStringConverter.InflateMIUString(testTargetStringStandard);

                        // Richiama TrovaDerivazioneBFS passando l'ID della ricerca
                        // Rimosso l'argomento 'passi'. RegoleMIUManager ora usa la sua proprietà statica.
                        List<PathStepInfo> miuPath = RegoleMIUManager.TrovaDerivazioneBFS(_currentSearchId, compressedSource, compressedTarget);

                        if (miuPath != null)
                        {
                            _logger.Log(LogLevel.INFO, $"\n--- Percorso trovato per '{testStartStringStandard}' -> '{testTargetStringStandard}': ---");
                            foreach (PathStepInfo step in miuPath)
                            {
                                // Recupera la stringa standard per il log
                                string logMessage = $"Stato: {step.StateStringStandard}";
                                if (step.AppliedRuleID.HasValue)
                                {
                                    RegolaMIU appliedRule = RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == step.AppliedRuleID.Value);
                                    logMessage += $", Regola Applicata: {appliedRule?.Nome ?? "Sconosciuta"} (ID: {step.AppliedRuleID.Value})";
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
                            _logger.Log(LogLevel.INFO, $"\n--- Nessun percorso trovato per '{testStartStringStandard}' -> '{testTargetStringStandard}' ---");
                        }
                        _logger.Log(LogLevel.INFO, "--- Fine Test Derivazione Specifica ---");
                    }
                    break;
            }
            Console.WriteLine("premi un tasto");
            Console.ReadKey();

            // Salva le statistiche delle regole alla fine dell'esecuzione dell'applicazione
            if (_ruleStatistics != null)
            {
                _repository.SaveRuleStatistics(_ruleStatistics);
                _logger.Log(LogLevel.INFO, $"[Program INFO] Salvate {_ruleStatistics.Count} RuleStatistics alla chiusura.");
            }
        }

        private static void RegoleMIUManager_OnSolutionFound(object sender, SolutionFoundEventArgs e)
        {
            // Converti la lista di PathStepInfo in una stringa leggibile per il log/console
            string pathString = "N/A";
            if (e.SolutionPathSteps != null && e.SolutionPathSteps.Any())
            {
                pathString = string.Join(" -> ", e.SolutionPathSteps.Select(step => step.StateStringStandard));
            }

            string message = $"SearchID: {e.SearchID} ElapsedMilliseconds: {e.ElapsedMilliseconds} ElapsedTicks: {e.ElapsedTicks} InitialString: {e.InitialString} MaxDepthReached: {e.MaxDepthReached} NodesExplored: {e.NodesExplored} Path: {pathString} StepsTaken: {e.StepsTaken} Success: {e.Success} TargetString: {e.TargetString}";
            Console.WriteLine(message);
            _logger.Log(e.Success ? LogLevel.INFO : LogLevel.WARNING, message);

            // Persistenza dei dati della ricerca complessiva
            _repository.UpdateSearch(e.SearchID, e.Success, e.ElapsedMilliseconds, e.StepsTaken, e.NodesExplored, e.MaxDepthReached);

            // Aggiorna SuccessfulCount e EffectivenessScore per le regole nel percorso di successo
            if (e.Success && e.SolutionPathSteps != null)
            {
                foreach (PathStepInfo step in e.SolutionPathSteps)
                {
                    if (step.AppliedRuleID.HasValue)
                    {
                        long ruleId = step.AppliedRuleID.Value; // RuleID è long
                        if (_ruleStatistics.ContainsKey(ruleId))
                        {
                            _ruleStatistics[ruleId].SuccessfulCount++;
                            // Ricalcola l'EffectivenessScore
                            _ruleStatistics[ruleId].RecalculateEffectiveness();
                            _ruleStatistics[ruleId].LastApplicationTimestamp = DateTime.Now; // Aggiorna anche l'ultimo utilizzo
                            _logger.Log(LogLevel.DEBUG, $"[Apprendimento] Regola {ruleId} ({RegoleMIUManager.Regole.FirstOrDefault(r => r.ID == ruleId)?.Nome ?? "Sconosciuta"}) SuccessfulCount incrementato a {_ruleStatistics[ruleId].SuccessfulCount}. Effectiveness: {_ruleStatistics[ruleId].EffectivenessScore:F4}");
                        }
                        else
                        {
                            // Questo caso non dovrebbe verificarsi spesso se tutte le regole sono caricate,
                            // ma lo gestiamo creando una nuova entry (con ApplicationCount = 0 per coerenza)
                            _logger.Log(LogLevel.WARNING, $"[Apprendimento] Regola {ruleId} trovata nel percorso di successo ma non in _ruleStatistics. Creazione nuova entry.");
                            _ruleStatistics[ruleId] = new RuleStatistics
                            {
                                RuleID = ruleId,
                                ApplicationCount = 0, // Verrà incrementato da OnRuleApplied
                                SuccessfulCount = 1,
                                LastApplicationTimestamp = DateTime.Now
                            };
                            _ruleStatistics[ruleId].RecalculateEffectiveness(); // Ricalcola anche se ApplicationCount è 0
                        }
                    }
                }
            }

            // Persistenza dei passi del percorso della soluzione (solo se la ricerca ha avuto successo)
            if (e.Success && e.SolutionPathSteps != null)
            {
                for (int i = 0; i < e.SolutionPathSteps.Count; i++)
                {
                    PathStepInfo currentStep = e.SolutionPathSteps[i];

                    // Upsert dello stato corrente
                    long currentStateId = _repository.UpsertMIUState(currentStep.StateStringStandard);

                    // Upsert dello stato genitore (se esiste)
                    long? parentStateId = null;
                    if (currentStep.ParentStateStringStandard != null)
                    {
                        parentStateId = _repository.UpsertMIUState(currentStep.ParentStateStringStandard);
                    }

                    // Determina se questo passo è il target
                    bool isTarget = (currentStep.StateStringStandard == MIUStringConverter.DeflateMIUString(e.TargetString));

                    // Inserisce il passo del percorso
                    _repository.InsertSolutionPathStep(
                        e.SearchID,
                        i + 1, // StepNumber
                        currentStateId,
                        parentStateId,
                        currentStep.AppliedRuleID,
                        isTarget,
                        e.Success, // isSuccess si riferisce al successo dell'intera ricerca
                        i // Depth
                    );
                }
            }
        }

        private static void RegoleMIUManager_OnRuleApplied(object sender, RuleAppliedEventArgs e)
        {
            string message = $"AppliedRuleID: {e.AppliedRuleID} AppliedRuleName: {e.AppliedRuleName} OriginalString: {e.OriginalString} NewString: {e.NewString} CurrentDepth: {e.CurrentDepth}";
            Console.WriteLine(message);
            _logger.Log(LogLevel.DEBUG, message);

            // Persistenza dell'applicazione della regola
            // Ottieni gli ID degli stati coinvolti
            long parentStateId = _repository.UpsertMIUState(e.OriginalString);
            long newStateId = _repository.UpsertMIUState(e.NewString);

            _repository.InsertRuleApplication(
                _currentSearchId, // Usa l'ID della ricerca corrente
                parentStateId,
                newStateId,
                e.AppliedRuleID,
                e.CurrentDepth
            );

            // Aggiorna ApplicationCount per la regola applicata
            long ruleId = e.AppliedRuleID; // RuleID è long
            if (!_ruleStatistics.ContainsKey(ruleId))
            {
                // Se la regola non è ancora presente nelle statistiche caricate, la aggiungiamo.
                // Questo può accadere se una nuova regola viene introdotta o se il database era vuoto.
                _ruleStatistics[ruleId] = new RuleStatistics { RuleID = ruleId };
                _logger.Log(LogLevel.DEBUG, $"[Apprendimento] Creata nuova entry RuleStatistics per regola {ruleId}.");
            }
            _ruleStatistics[ruleId].ApplicationCount++;
            _ruleStatistics[ruleId].LastApplicationTimestamp = DateTime.Now; // Aggiorna l'ultimo utilizzo
            _logger.Log(LogLevel.DEBUG, $"[Apprendimento] Regola {ruleId} ({e.AppliedRuleName}) ApplicationCount incrementato a {_ruleStatistics[ruleId].ApplicationCount}.");
            // Nota: EffectivenessScore non ricalcolato qui, solo a fine ricerca quando si sa se la regola ha contribuito a una soluzione.
        }

        // Metodo per la ricerca DFS (esistente, non modificato per le statistiche in questo step)
        private static void RicercaDiDerivazioneDFS(long searchId, string startStringCompressed, string targetStringCompressed)
        {
            Console.WriteLine($"Inizio Ricerca DFS da '{startStringCompressed}' a '{targetStringCompressed}' (Max Profondità: {RegoleMIUManager.MaxProfonditaRicerca})"); // Modificato log
            _logger.Log(LogLevel.INFO, $"Inizio Ricerca DFS da '{startStringCompressed}' a '{targetStringCompressed}' (Max Profondità: {RegoleMIUManager.MaxProfonditaRicerca})"); // Modificato log

            List<PathStepInfo> percorsoDFS = RegoleMIUManager.TrovaDerivazioneDFS(searchId, startStringCompressed, targetStringCompressed);

            if (percorsoDFS != null)
            {
                Console.WriteLine("Percorso DFS trovato:");
                _logger.Log(LogLevel.INFO, "Percorso DFS trovato:");
                foreach (var s in percorsoDFS)
                {
                    Console.WriteLine(s.StateStringStandard);
                    _logger.Log(LogLevel.INFO, s.StateStringStandard);
                }
            }
            else
            {
                Console.WriteLine($"Nessuna derivazione trovata con DFS entro la profondità {RegoleMIUManager.MaxProfonditaRicerca}."); // Modificato log
                _logger.Log(LogLevel.INFO, $"Nessuna derivazione trovata con DFS entro la profondità {RegoleMIUManager.MaxProfonditaRicerca}."); // Modificato log
            }
        }

        // Metodo per la ricerca BFS (esistente, non modificato per le statistiche in questo step)
        private static void RicercaDiDerivazioneBFS(long searchId, string inizioCompressed, string fineCompressed)
        {
            Console.WriteLine($"inizio: {inizioCompressed} fine: {fineCompressed} passi: {RegoleMIUManager.MassimoPassiRicerca}"); // Modificato log
            _logger.Log(LogLevel.INFO, $"Inizio Ricerca BFS da '{inizioCompressed}' a '{fineCompressed}' (Max Passi: {RegoleMIUManager.MassimoPassiRicerca})"); // Modificato log

            List<PathStepInfo> miu = RegoleMIUManager.TrovaDerivazioneBFS(searchId, inizioCompressed, fineCompressed);
            if (miu != null)
            {
                Console.WriteLine("Percorso BFS trovato:");
                _logger.Log(LogLevel.INFO, "Percorso BFS trovato:");
                foreach (PathStepInfo item in miu)
                {
                    Console.WriteLine(item.StateStringStandard);
                    _logger.Log(LogLevel.INFO, item.StateStringStandard);
                }
            }
            else
            {
                Console.WriteLine("FAIL! Nessun percorso BFS trovato.");
                _logger.Log(LogLevel.WARNING, "FAIL! Nessun percorso BFS trovato.");
            }
        }
    }
}
