// sostituito 16.6.25 12.02
// sostituito 16.6.2025 12.10
// sostituito 16.6.2025 12.16
//  sostituito 16.6.2025 15.31
// Corretto 17.6.25: Rimossi riferimenti a EvolutiveSystem.Core.
// Aggiunta la definizione di SerializableDictionary direttamente in questo file.
// Corretto l'uso di Database e Table per puntare a EvolutiveSystem.SQL.Core.
// Aggiunta la definizione di SerializableDictionary direttamente in questo file (per compatibilità).
// per risolvere gli errori su 'DataRecords' e pulire codice non funzionale.
// NUOVA MODIFICA 17.6.25: Integrazione di MIURepository nel case 3 per il caricamento delle regole.
// NUOVA MODIFICA 19.6.25: Integrazione di MasterLog.Logger.
// NUOVA MODIFICA 20.6.25: Implementazione della persistenza dei dati di ricerca (Fase 1 roadmap).

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
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public SerializableDictionary() : base() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
    }

    internal class Program
    {
        private const long passi = 10; // Massima profondità di ricerca

        private static Logger _logger; // Istanza del logger
        private static MIURepository _repository; // Repository per la persistenza (reso statico per gli eventi)
        private static long _currentSearchId; // ID della ricerca corrente per correlare eventi (reso statico)


        static void Main(string[] args)
        {
            // Inizializzazione del Logger
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _logger = new Logger(logDirectory, "MIULog", 7); // Conserva gli ultimi 7 giorni di log
            _logger.SwLogLevel = _logger.LOG_INFO | _logger.LOG_DEBUG | _logger.LOG_ERROR | _logger.LOG_WARNING; // Imposta i livelli di log attivi
            _logger.Log(LogLevel.INFO, "Menomale che Gemini ha ripristinato il progeto");
            // Propaga il logger a RegoleMIUManager (che è statico)
            RegoleMIUManager.LoggerInstance = _logger;


            string[,] arrayString =
            {
                {"MI", "MI", "MI", "MI", "MI", "MI", "MIIIIII","MII","MUI", "MI"},
                {"MIU","MII","MIIII","MUI","MUIU","MUIIU", "MUU", "MU","MIU","MIIIIIIIII"}
            };

            string databaseFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";

            int tipotest = 7; // Impostato a 7 per eseguire il test di persistenza con la ricerca BFS specifica
            Process myProc = new Process();
            switch (tipotest)
            {
                case 3:
                    {
                        Random rnd = new Random();
                        databaseFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";
                        SQLiteSchemaLoader _schemaLoader = new SQLiteSchemaLoader(databaseFilePath);

                        // Istanzia MIUDatabaseManager e MIURepository con logger
                        MIUDatabaseManager _dbManager = new MIUDatabaseManager(_schemaLoader, _logger);
                        IMIUDataManager _dataManager = (IMIUDataManager)_dbManager;
                        _repository = new MIURepository(_dataManager, _logger); // Assegna al campo statico

                        List<RegolaMIU> regoleMIUList = _repository.LoadRegoleMIU();
                        RegoleMIUManager.CaricaRegoleDaOggettoRepository(regoleMIUList);

                        RegoleMIUManager.OnRuleApplied += RegoleMIUManager_OnRuleApplied;
                        RegoleMIUManager.OnSolutionFound += RegoleMIUManager_OnSolutionFound;
                        List<string> MIUstringList = _schemaLoader.SQLiteSelect("SELECT StateID, CurrentString, StringLength, Hash, DiscoveryTime_Int, DiscoveryTime_Text, UsageCount FROM MIU_States;");

                        if (RegoleMIUManager.Regole.Count > 0)
                        {
                            foreach (string s in MIUstringList)
                            {
                                string[] MIUstringsSource = s.Split(';');
                                int index = rnd.Next(0, MIUstringsSource.Length - 1);
                                string[] MIUstringDestination = MIUstringList[index].Split(';');

                                // Inserisci la ricerca iniziale e ottieni l'ID
                                _currentSearchId = _repository.InsertSearch(MIUstringsSource[1], MIUstringDestination[1], "BFS"); // Salva la stringa standard per initial/target

                                List<PathStepInfo> miu = RegoleMIUManager.TrovaDerivazioneBFS(_currentSearchId, MIUstringsSource[1], MIUstringDestination[1], passi);
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
                        databaseFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";
                        SQLiteSchemaLoader _schemaLoader = new SQLiteSchemaLoader(databaseFilePath);

                        // Istanzia MIUDatabaseManager e MIURepository con logger
                        MIUDatabaseManager _dbManager = new MIUDatabaseManager(_schemaLoader, _logger);
                        IMIUDataManager _dataManager = (IMIUDataManager)_dbManager;
                        _repository = new MIURepository(_dataManager, _logger); // Assegna al campo statico

                        List<string> regole = _schemaLoader.SQLiteSelect("SELECT ID, Nome, Pattern, Sostituzione, Descrizione FROM RegoleMIU;");

                        string StringIn = "M2U3I4MI";
                        RegoleMIUManager.CaricaRegoleDaOggettoSQLite(regole);

                        // Questo case è un test di applicazione regole senza ricerca e persistenza completa
                        // Quindi non avrà un SearchID correlato automaticamente.

                        string currentTestString = MIUStringConverter.DeflateMIUString(StringIn);
                        bool response0 = RegoleMIUManager.Regole[0].TryApply(currentTestString, out string regola0Output);
                        Console.WriteLine($"String in: {currentTestString} Regola 0: {regola0Output} response: {response0}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 0: {regola0Output} response: {response0}");

                        currentTestString = MIUStringConverter.DeflateMIUString(MIUStringConverter.InflateMIUString(regola0Output));
                        bool response1 = RegoleMIUManager.Regole[1].TryApply(currentTestString, out string regola1Output);
                        Console.WriteLine($"String in: {currentTestString} Regola 1: {regola1Output} response: {response1}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 1: {regola1Output} response: {response1}");

                        currentTestString = MIUStringConverter.DeflateMIUString(MIUStringConverter.InflateMIUString(regola1Output));
                        bool response2 = RegoleMIUManager.Regole[2].TryApply(currentTestString, out string regola2Output);
                        Console.WriteLine($"String in: {currentTestString} Regola 2: {regola2Output} response: {response2}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 2: {regola2Output} response: {response2}");

                        currentTestString = MIUStringConverter.DeflateMIUString(MIUStringConverter.InflateMIUString(regola2Output));
                        bool response3 = RegoleMIUManager.Regole[3].TryApply(currentTestString, out string regola3Output);
                        Console.WriteLine($"String in: {currentTestString} Regola 3: {regola3Output} response: {response3}");
                        _logger.Log(LogLevel.INFO, $"String in: {currentTestString} Regola 3: {regola3Output} response: {response3}");
                    }
                    break;
                case 5:
                    {
                        databaseFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";
                        SQLiteSchemaLoader _schemaLoader = new SQLiteSchemaLoader(databaseFilePath);

                        // Istanzia MIUDatabaseManager e MIURepository con logger
                        MIUDatabaseManager _dbManager = new MIUDatabaseManager(_schemaLoader, _logger);
                        IMIUDataManager _dataManager = (IMIUDataManager)_dbManager;
                        _repository = new MIURepository(_dataManager, _logger); // Assegna al campo statico

                        List<RegolaMIU> regoleMIUList = _repository.LoadRegoleMIU();
                        RegoleMIUManager.CaricaRegoleDaOggettoRepository(regoleMIUList);

                        RegoleMIUManager.OnRuleApplied += RegoleMIUManager_OnRuleApplied;
                        RegoleMIUManager.OnSolutionFound += RegoleMIUManager_OnSolutionFound;

                        long maxProfondita = 10;
                        int cntDwn = 22;
                        while (cntDwn >= 1)
                        {
                            for (int y = 0; y < arrayString.GetLength(1); y++)
                            {
                                // Inserisci la ricerca iniziale e ottieni l'ID
                                _currentSearchId = _repository.InsertSearch(arrayString[0, y], arrayString[1, y], "DFS"); // Salva le stringhe standard
                                RicercaDiDerivazioneDFS(_currentSearchId, arrayString[0, y], arrayString[1, y], maxProfondita);
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
                        databaseFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";
                        SQLiteSchemaLoader _schemaLoader = new SQLiteSchemaLoader(databaseFilePath);

                        // Istanzia MIUDatabaseManager e MIURepository con logger
                        MIUDatabaseManager _dbManager = new MIUDatabaseManager(_schemaLoader, _logger);
                        IMIUDataManager _dataManager = (IMIUDataManager)_dbManager;
                        _repository = new MIURepository(_dataManager, _logger); // Assegna al campo statico

                        List<RegolaMIU> regoleMIUList = _repository.LoadRegoleMIU();
                        RegoleMIUManager.CaricaRegoleDaOggettoRepository(regoleMIUList);

                        RegoleMIUManager.OnRuleApplied += RegoleMIUManager_OnRuleApplied;
                        RegoleMIUManager.OnSolutionFound += RegoleMIUManager_OnSolutionFound;

                        // Stringhe specifiche per il test BFS
                        string testStartStringStandard = "MUUIIIMMMMI";
                        string testTargetStringStandard = "MUMMMMIUMUMMMMIU";

                        // Inserisci la ricerca iniziale e ottieni l'ID
                        _currentSearchId = _repository.InsertSearch(testStartStringStandard, testTargetStringStandard, "BFS"); // Salva le stringhe standard per initial/target

                        _logger.Log(LogLevel.INFO, $"--- Inizio Test Derivazione Specifica (BFS) con persistenza: {testStartStringStandard} -> {testTargetStringStandard} ---");

                        // Le stringhe da passare a TrovaDerivazioneBFS devono essere compresse
                        string compressedStart = MIUStringConverter.InflateMIUString(testStartStringStandard);
                        string compressedTarget = MIUStringConverter.InflateMIUString(testTargetStringStandard);

                        // Richiama TrovaDerivazioneBFS passando l'ID della ricerca
                        List<PathStepInfo> miuPath = RegoleMIUManager.TrovaDerivazioneBFS(_currentSearchId, compressedStart, compressedTarget, passi);

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
        }

        private static void RicercaDiDerivazioneDFS(long searchId, string startString, string targetString, long maxProfondita)
        {
            Console.WriteLine($"Inizio Ricerca DFS da '{startString}' a '{targetString}' (Max Profondità: {maxProfondita})");
            _logger.Log(LogLevel.INFO, $"Inizio Ricerca DFS da '{startString}' a '{targetString}' (Max Profondità: {maxProfondita})");

            // Le stringhe da passare a TrovaDerivazioneDFS devono essere compresse
            string compressedStart = MIUStringConverter.InflateMIUString(startString);
            string compressedTarget = MIUStringConverter.InflateMIUString(targetString);

            List<PathStepInfo> percorsoDFS = RegoleMIUManager.TrovaDerivazioneDFS(searchId, compressedStart, compressedTarget, maxProfondita);

            if (percorsoDFS != null)
            {
                Console.WriteLine("Percorso DFS trovato:");
                _logger.Log(LogLevel.INFO, "Percorso DFS trovato:");
                foreach (var s in percorsoDFS)
                {
                    Console.WriteLine(s.StateStringStandard); // Stampa la stringa standard
                    _logger.Log(LogLevel.INFO, s.StateStringStandard);
                }
            }
            else
            {
                Console.WriteLine($"Nessuna derivazione trovata con DFS entro la profondità {maxProfondita}.");
                _logger.Log(LogLevel.INFO, $"Nessuna derivazione trovata con DFS entro la profondità {maxProfondita}.");
            }
        }
        private static void RicercaDiDerivazioneBFS(long searchId, string inizio, string fine, long passi)
        {
            Console.WriteLine($"inizio: {inizio} fine: {fine} passi: {passi}");
            _logger.Log(LogLevel.INFO, $"Inizio Ricerca BFS da '{inizio}' a '{fine}' (Max Passi: {passi})");

            // Le stringhe da passare a TrovaDerivazioneBFS devono essere compresse
            string compressedInizio = MIUStringConverter.InflateMIUString(inizio);
            string compressedFine = MIUStringConverter.InflateMIUString(fine); // Era un errore qui, deve essere DeflateString

            List<PathStepInfo> miu = RegoleMIUManager.TrovaDerivazioneBFS(searchId, compressedInizio, compressedFine, passi);
            if (miu != null)
            {
                Console.WriteLine("Percorso BFS trovato:");
                _logger.Log(LogLevel.INFO, "Percorso BFS trovato:");
                foreach (PathStepInfo item in miu)
                {
                    Console.WriteLine(item.StateStringStandard); // Stampa la stringa standard
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
