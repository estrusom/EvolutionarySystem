using System;
using System.Collections.Generic;
using System.Linq;
using MIU.Core;
using EvolutiveSystem.SQL.Core;
using System.Diagnostics;
using System.IO;

namespace MIU.TestApp
{
    public class Program
    {
        private static DatabaseManager _genericDbManager;
        private static MIUDatabaseManager _miuDbManager;
        private static MIURepository _miuRepository;
        private static string _dbFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";
        private static long _currentSearchId; // Mantiene l'ID della ricerca corrente per l'handler

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Avvio applicazione MIU System Test...");

            try
            {
                Console.WriteLine("Program: Inizio Inizializzazione Database e Repository.");
                InitializeDatabaseAndRepository();
                Console.WriteLine("Program: Fine Inizializzazione Database e Repository.");

                List<RegolaMIU> regoleIniziali = new List<RegolaMIU>
                {
                    new RegolaMIU(1, "Regola I", "Sostituisce 'I' con 'IU' alla fine di una stringa", "I$", "IU"),
                    new RegolaMIU(2, "Regola II", "Sostituisce 'M' + 'XX' con 'M' + 'X'", "M(.)(.)", "M$1"),
                    new RegolaMIU(3, "Regola III", "Se 'III' appare, può essere sostituito con 'U'", "III", "U"),
                    new RegolaMIU(4, "Regola IV", "Rimuove due 'U' consecutive", "UU", ""),
                };

                RegoleMIUManager.GetLearningAdvisor().Initialize();

                RegoleMIUManager.CaricaRegoleDaOggettoSQLite(regoleIniziali.Select(r => $"{r.ID};{r.Nome};{r.Pattern};{r.Sostituzione};{r.Descrizione}").ToList());
                _miuRepository.UpsertRegoleMIU(regoleIniziali);
                Console.WriteLine("Regole MIU caricate e inserite/aggiornate nel database.");

                Console.WriteLine("Program: Sottoscrizione Eventi.");
                SubscribeToMIUEvents();
                Console.WriteLine("Program: Handler degli eventi sottoscritti.");

                string[] MIUstringsSource = { "MI", "MIIU", "MUI", "M", "MUU", "MIUI", "MIU" };
                string[] MIUstringDestination = { "MIU", "MU", "MUIU", "MUI", "MU", "MII", "MIUIU" };
                int passi = 10;

                Console.WriteLine("Program: Inizio ciclo ricerche.");
                for (int j = 0; j < MIUstringsSource.Length; j++)
                {
                    Console.WriteLine($"Program: Inizio iterazione ricerca {j + 1}.");
                    string startStringCompressed = MIUStringConverter.DeflateMIUString(MIUstringsSource[j]);
                    string targetStringCompressed = MIUStringConverter.DeflateMIUString(MIUstringDestination[j]);
                    string searchAlgorithmName = "DFS"; // Assicurati che questa variabile sia dichiarata qui

                    Console.WriteLine($"\nInizio ricerca {j + 1} da '{MIUstringsSource[j]}' (compressa: '{startStringCompressed}') a '{MIUstringDestination[j]}' (compressa: '{targetStringCompressed}')");

                    List<(string CompressedString, int? AppliedRuleID)> solutionPath = null;
                    Stopwatch searchStopwatch = new Stopwatch();
                    bool searchSuccess = false;

                    try
                    {
                        Console.WriteLine($"Program: Avvio transazione per ricerca {j + 1}.");
                        _genericDbManager.BeginTransaction();

                        _currentSearchId = _miuRepository.InsertSearch(startStringCompressed, targetStringCompressed, searchAlgorithmName);
                        Console.WriteLine($"Program: Ricerca {_currentSearchId} inserita. Avvio algoritmo di ricerca.");

                        searchStopwatch.Start();
                        solutionPath = RegoleMIUManager.TrovaDerivazioneDFS(startStringCompressed, targetStringCompressed, passi);
                        searchStopwatch.Stop();

                        searchSuccess = (solutionPath != null);
                        Console.WriteLine($"Program: Algoritmo di ricerca per ricerca {j + 1} completato. Soluzione trovata: {searchSuccess}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Program: Errore durante l'esecuzione della ricerca {j + 1} (catturato nel Main loop try): {ex.Message}");
                        searchSuccess = false;
                        try
                        {
                            if (_genericDbManager != null) _genericDbManager.RollbackTransaction();
                            Console.WriteLine($"Program: Rollback transazione per ricerca {j + 1} nel catch del Main loop.");
                        }
                        catch (Exception rollbackEx) { Console.WriteLine($"Program: Errore durante il rollback nel Main loop: {rollbackEx.Message}"); }
                    }
                    finally
                    {
                        Console.WriteLine($"Program: Finalizzazione logica per ricerca {j + 1} (blocco finally).");
                        try
                        {
                            RegoleMIUManager.NotificaSoluzioneTrovata(new SolutionFoundEventArgs(
                                initialString: startStringCompressed,
                                targetString: targetStringCompressed,
                                success: searchSuccess,
                                path: solutionPath,
                                elapsedTicks: searchStopwatch.ElapsedTicks,
                                stepsTaken: solutionPath != null ? solutionPath.Count - 1 : (int)passi,
                                nodesExplored: RegoleMIUManager._globalNodesExplored,
                                maxDepthReached: solutionPath != null ? solutionPath.Count - 1 : (int)passi,
                                searchAlgorithm: searchAlgorithmName // Qui searchAlgorithmName è valido
                            ));
                            Console.WriteLine($"Program: Chiamata a HandleSolutionFound per ricerca {j + 1} completata.");
                        }
                        catch (Exception exHandler)
                        {
                            Console.WriteLine($"Program: Errore critico nel finalizzatore della ricerca {j + 1} (HandleSolutionFound): {exHandler.Message}");
                            try
                            {
                                if (_genericDbManager != null) _genericDbManager.RollbackTransaction();
                                Console.WriteLine($"Program: Rollback transazione per ricerca {j + 1} nel catch del finalizzatore.");
                            }
                            catch (Exception rollbackEx) { Console.WriteLine($"Program: Errore durante il rollback nel finalizzatore: {rollbackEx.Message}"); }
                        }
                    }
                    Console.WriteLine($"Program: Fine iterazione ricerca {j + 1}.");
                }
                Console.WriteLine("Program: Fine ciclo ricerche.");

                Console.WriteLine(RegoleMIUManager.GetLearningAdvisor().GetStatisticsSummary());
            }
            catch (Exception ex) // CATCH PER ERRORI CRITICI NELL'APPLICAZIONE (es. problemi DB all'avvio)
            {
                Console.WriteLine($"Program: Errore critico nell'applicazione (catch globale Main): {ex.Message}");
                try
                {
                    if (_genericDbManager != null) _genericDbManager.RollbackTransaction();
                    Console.WriteLine("Program: Rollback transazione nel catch globale Main.");
                }
                catch (Exception rollbackEx) { Console.WriteLine($"Program: Errore durante il rollback nel catch globale: {rollbackEx.Message}"); }
            }
            finally // FINALLY PER LA CHIUSURA DEL DATABASE E DEGLI EVENTI DOPO TUTTE LE RICERCHE
            {
                Console.WriteLine("Program: Inizio de-inizializzazione e chiusura finale.");
                DeinitializeDatabaseAndEvents();
                Console.WriteLine("Applicazione terminata. Premi un tasto per uscire.");
                Console.ReadKey();
            }
        }

        // --- NUOVI METODI: INIZIALIZZAZIONE E GESTIONE EVENTI ---

        private static void InitializeDatabaseAndRepository()
        {
            Console.WriteLine($"Program: Inizializzazione database SQLite a: {_dbFilePath}");
            _genericDbManager = new DatabaseManager(_dbFilePath); // Passa il percorso assoluto
            _genericDbManager.OpenConnection();
            Console.WriteLine("Program: Connessione al database generico aperta con successo.");

            _miuDbManager = new MIU.Core.MIUDatabaseManager(_genericDbManager);
            // La chiamata a _miuDbManager.CreateTables() è stata rimossa come richiesto.

            _miuRepository = new MIURepository(_miuDbManager);
            Console.WriteLine("Program: MIURepository inizializzato.");
        }

        private static void SubscribeToMIUEvents()
        {
            RegoleMIUManager.OnSolutionFound += HandleSolutionFound; // Sottoscrivi qui l'evento OnSolutionFound
            RegoleMIUManager.OnRuleApplied += HandleRuleApplied; // Solo OnRuleApplied
            Console.WriteLine("Program: Sottoscritto agli eventi RegoleMIUManager.");
        }

        private static void DeinitializeDatabaseAndEvents()
        {
            RegoleMIUManager.OnSolutionFound -= HandleSolutionFound; // Annulla sottoscrizione OnSolutionFound
            RegoleMIUManager.OnRuleApplied -= HandleRuleApplied;
            Console.WriteLine("Program: Annullata sottoscrizione agli eventi RegoleMIUManager.");

            if (_genericDbManager != null)
            {
                _genericDbManager.CloseConnection();
                Console.WriteLine("Program: Connessione al database chiusa.");
            }
        }

        /// <summary>
        /// Gestisce l'evento OnSolutionFound e salva i dati nel database.
        /// </summary>
        private static void HandleSolutionFound(object sender, SolutionFoundEventArgs e)
        {
            // Usa _currentSearchId per i log, e.SearchAlgorithm per il nome dell'algoritmo
            Console.WriteLine($"Program: HandleSolutionFound avviato per ricerca {_currentSearchId}. Successo: {e.Success}. Algoritmo: {e.SearchAlgorithm}.");
            try
            {
                _miuRepository.UpdateSearch(
                    _currentSearchId, // Usa l'ID della ricerca corrente
                    e.Success,
                    e.ElapsedMilliseconds,
                    e.StepsTaken,
                    e.NodesExplored,
                    e.MaxDepthReached
                );
                Console.WriteLine($"Program: Ricerca {_currentSearchId} aggiornata in DB.");

                if (e.Success && e.Path != null)
                {
                    Console.WriteLine($"Program: Inizio salvataggio percorso per ricerca {_currentSearchId}.");
                    long? parentStateId = null;
                    for (int i = 0; i < e.Path.Count; i++)
                    {
                        var step = e.Path[i];
                        long currentStateId = _miuRepository.UpsertMIUState(MIUStringConverter.InflateMIUString(step.CompressedString));

                        parentStateId = (i > 0) ? (long?)_miuRepository.UpsertMIUState(MIUStringConverter.InflateMIUString(e.Path[i - 1].CompressedString)) : null;
                        int? appliedRuleId = (i > 0) ? step.AppliedRuleID : null;

                        _miuRepository.InsertSolutionPathStep(
                            _currentSearchId,
                            i,
                            currentStateId,
                            parentStateId,
                            appliedRuleId,
                            step.CompressedString == e.TargetString,
                            e.Success,
                            i
                        );
                    }
                    Console.WriteLine($"Program: Percorso per ricerca {_currentSearchId} salvato in DB.");
                }

                _genericDbManager.CommitTransaction();
                Console.WriteLine($"Program: Transazione per ricerca {_currentSearchId} commessa.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program: Errore FATALE in HandleSolutionFound per ricerca {_currentSearchId}: {ex.Message}");
                try
                {
                    if (_genericDbManager != null) _genericDbManager.RollbackTransaction();
                    Console.WriteLine($"Program: Rollback transazione per ricerca {_currentSearchId} nel catch di HandleSolutionFound.");
                }
                catch (Exception rollbackEx) { Console.WriteLine($"Program: Errore durante il rollback in HandleSolutionFound: {rollbackEx.Message}"); }
            }
        }

        /// <summary>
        /// Gestisce l'evento OnRuleApplied e salva i dettagli delle applicazioni di regole nel database.
        /// Questo metodo si basa sulla transazione avviata per la ricerca principale.
        /// </summary>
        private static void HandleRuleApplied(object sender, RuleAppliedEventArgs e)
        {
            // Questa funzione deve essere robusta e non lanciare eccezioni in modo incontrollato
            // dato che è un handler di evento chiamato ripetutamente.
            // Qualsiasi errore qui potrebbe interrompere il ciclo di ricerca.
            try
            {
                // Console.WriteLine($"Program: Rule Applied - Parent: {e.ParentString}, New: {e.NewString}, RuleID: {e.AppliedRuleID}, Depth: {e.CurrentDepth}"); // ABILITA SOLO PER DEBUG INTENSO
                EmergingProcesses learningAdvisor = RegoleMIUManager.GetLearningAdvisor();

                string parentStandardString = MIUStringConverter.InflateMIUString(e.ParentString);
                string newStandardString = MIUStringConverter.InflateMIUString(e.NewString);

                long parentStateId = _miuRepository.UpsertMIUState(parentStandardString);
                long newStateId = _miuRepository.UpsertMIUState(newStandardString);

                if (learningAdvisor.ShouldPersistRuleApplication(e.ParentString, e.NewString, e.AppliedRuleID, e.CurrentDepth))
                {
                    _miuRepository.InsertRuleApplication(
                        _currentSearchId, // Usa l'ID della ricerca corrente
                        parentStateId,
                        newStateId,
                        e.AppliedRuleID,
                        e.CurrentDepth
                    );
                    // Console.WriteLine($"Program: Rule application for search {_currentSearchId} persisted."); // ABILITA SOLO PER DEBUG INTENSO
                }
                else
                {
                    // Console.WriteLine($"Program: Rule application for search {_currentSearchId} NOT persisted by advisor."); // ABILITA SOLO PER DEBUG INTENSO
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program: Errore in HandleRuleApplied (RuleID: {e.AppliedRuleID}, Depth: {e.CurrentDepth}): {ex.Message}");
                // IMPORTANTE: Non fare rollback qui! Questo handler è parte di una transazione più grande.
                // Un errore qui significa che la transazione della ricerca principale potrebbe essere già in uno stato problematico,
                // e il rollback verrà gestito da HandleSolutionFound o dal catch generale del Main.
            }
        }
    }
}
