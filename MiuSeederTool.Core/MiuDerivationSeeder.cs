// File: MiuSeederTool.Core/MiuDerivationSeeder.cs
// Descrizione: Classe responsabile della generazione e del seeding delle stringhe MIU nel database.
// Aggiornato per implementare la raccolta e prioritizzazione delle stringhe SolutionPath da BFS e DFS.
// Corretto errore CS0103 relativo a HashCode.Combine.
// Target Framework: .NET Framework 4.8

using MasterLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiuSeederTool.Core
{
    /// <summary>
    /// Enumera i tipi di algoritmo di ricerca che possono essere utilizzati per trovare percorsi.
    /// </summary>
    public enum PathFindingAlgorithm
    {
        BFS,
        DFS
    }

    /// <summary>
    /// Rappresenta un singolo passo in un percorso di derivazione MIU.
    /// Contiene la stringa dello stato, l'ID della regola applicata per raggiungerlo,
    /// e la stringa dello stato genitore.
    /// </summary>
    public class PathStepInfo
    {
        public string StateString { get; set; } // La stringa MIU per questo stato
        public long? AppliedRuleID { get; set; } // L'ID della regola applicata per raggiungere questo stato (null per lo stato iniziale)
        public string ParentStateString { get; set; } // La stringa dello stato genitore (null per lo stato iniziale)
        public int Depth { get; set; } // La profondità (numero di passi dal punto di partenza) di questo stato

        // Override di Equals e GetHashCode per permettere a HashSet di trattare PathStepInfo
        // come unico basandosi solo su StateString.
        public override bool Equals(object obj)
        {
            return obj is PathStepInfo info &&
                   StateString == info.StateString;
        }

        public override int GetHashCode()
        {
            // Correzione per l'errore CS0103: HashCode.Combine non disponibile.
            // Utilizza l'hash code della StateString direttamente.
            return StateString.GetHashCode();
        }
    }

    /// <summary>
    /// Gestisce la generazione e l'inizializzazione del database con stringhe MIU.
    /// </summary>
    public class MiuDerivationSeeder
    {
        private readonly SeederDbAccess _dbAccess;
        private readonly MiuStringHelper _miuStringHelper;
        private readonly Logger _logger;
        private readonly Random _random;
        private readonly int _globalMaxStringLength;

        // Stringhe iniziali da cui iniziare la derivazione per il master pool
        private static readonly List<string> InitialSeedStrings = new List<string>
        {
            "MI",
            "MII",
            "MIU",
            "MIIII",
            "MUI",
            "MIIIIU",
            "MIIIIIIII",
            "MUUII",
            "MUUIIU"
        };

        /// <summary>
        /// Inizializza una nuova istanza della classe MiuDerivationSeeder.
        /// </summary>
        /// <param name="dbAccess">L'istanza per l'accesso al database.</param>
        /// <param name="miuStringHelper">L'istanza per l'applicazione delle regole MIU.</param>
        /// <param name="logger">L'istanza del logger.</param>
        /// <param name="globalMaxStringLength">La lunghezza massima consentita per le stringhe MIU.</param>
        public MiuDerivationSeeder(SeederDbAccess dbAccess, MiuStringHelper miuStringHelper, Logger logger, int globalMaxStringLength)
        {
            _dbAccess = dbAccess ?? throw new ArgumentNullException(nameof(dbAccess));
            _miuStringHelper = miuStringHelper ?? throw new ArgumentNullException(nameof(miuStringHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _random = new Random();
            _globalMaxStringLength = globalMaxStringLength;

            _logger.Log(LogLevel.INFO, "[MiuDerivationSeeder] Initialized.", false);
        }

        /// <summary>
        /// Inizializza completamente il database con stringhe MIU di vari tipi.
        /// </summary>
        /// <param name="numTotalStrings">Il numero totale di stringhe da generare.</param>
        /// <param name="maxDerivationDepthForSeeding">La profondità massima di derivazione da esplorare per il seeding.</param>
        /// <param name="cancellationToken">Token per annullare l'operazione.</param>
        /// <param name="pathAlgorithm">L'algoritmo da usare per trovare i percorsi di soluzione (BFS o DFS). Nota: ora entrambi vengono usati internamente per la fase SolutionPath.</param>
        public async Task InitializeFullDatabaseAsync(int numTotalStrings, int maxDerivationDepthForSeeding, CancellationToken cancellationToken, PathFindingAlgorithm pathAlgorithm)
        {
            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] Starting full database initialization. Total strings target: {numTotalStrings}. Primary Path algorithm for logging: {pathAlgorithm}.", false);
            Console.WriteLine($"Starting full database initialization. Total strings target: {numTotalStrings} (using {pathAlgorithm} for paths)...");

            // Valori target per i tipi di seeding. Aumentato targetSolutionPathCount per maggiore diversità.
            int targetDerivableCount = 20;
            int targetSolutionPathCount = 500; // Aumentato da 150 a 500

            List<SeederMiuState> statesToInsert = new List<SeederMiuState>();
            HashSet<string> uniqueStringsTracker = new HashSet<string>(); // Traccia tutte le stringhe uniche inserite nel DB

            int poolGenerationLimit = Math.Max(targetDerivableCount * 5, targetSolutionPathCount * 5); // Aumentato il moltiplicatore per il pool
            poolGenerationLimit = Math.Max(poolGenerationLimit, numTotalStrings * 2);

            HashSet<string> masterDerivablePool = new HashSet<string>();
            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] Generating master pool of derivable strings (max length {_globalMaxStringLength}, depth {maxDerivationDepthForSeeding}, pool limit {poolGenerationLimit})...", false);
            Console.WriteLine($"Generating master pool of derivable strings...");

            foreach (string startString in InitialSeedStrings)
            {
                if (cancellationToken.IsCancellationRequested) break;

                HashSet<string> currentBranchDerivables = await GenerateLimitedUniqueStringsAsync(
                    startString,
                    maxDerivationDepthForSeeding,
                    poolGenerationLimit,
                    _globalMaxStringLength,
                    cancellationToken
                );
                foreach (string s in currentBranchDerivables)
                {
                    masterDerivablePool.Add(s);
                    if (masterDerivablePool.Count >= poolGenerationLimit) break;
                }
                if (masterDerivablePool.Count >= poolGenerationLimit) break;
            }
            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] Master pool generated with {masterDerivablePool.Count} unique derivable strings.", false);
            Console.WriteLine($"Master pool generated with {masterDerivablePool.Count} unique derivable strings.");

            if (masterDerivablePool.Count == 0)
            {
                _logger.Log(LogLevel.WARNING, "[MiuDerivationSeeder] Master pool of derivable strings is empty. Cannot guarantee Derivable or SolutionPath types.", false);
                Console.WriteLine("Master pool of derivable strings is empty. Cannot guarantee Derivable or SolutionPath types.");
            }

            // --- FASE 2: Preparazione di stringhe SolutionPath (Tipo 2) ---
            // Le stringhe target vengono prese dal masterDerivablePool, esclusa la stringa "MI" se presente
            List<string> candidateSolutionTargets = masterDerivablePool
                                                    .Where(s => s.Length <= _globalMaxStringLength && s != "MI") // Escludi "MI" come target per evitare percorsi di lunghezza 0
                                                    .OrderBy(s => _random.Next()) // Ordina casualmente per diversificare i target
                                                    .ToList();

            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] Preparing {targetSolutionPathCount} 'SolutionPath' strings from {candidateSolutionTargets.Count} candidates by combining BFS and DFS results.", false);
            Console.WriteLine($"Preparing {targetSolutionPathCount} 'SolutionPath' strings by combining BFS and DFS results.");

            // HashSet per raccogliere tutte le stringhe SolutionPath uniche trovate da BFS e DFS
            HashSet<string> bfsDiscoveredSolutionPaths = new HashSet<string>();
            HashSet<string> dfsDiscoveredSolutionPaths = new HashSet<string>();

            int pathSearchAttempts = 0;
            int maxPathSearchAttempts = candidateSolutionTargets.Count * 2; // Tentativi per ogni target (BFS + DFS)

            foreach (string target in candidateSolutionTargets)
            {
                if (cancellationToken.IsCancellationRequested || pathSearchAttempts >= maxPathSearchAttempts) break;
                if (uniqueStringsTracker.Contains(target)) continue; // Se il target è già stato aggiunto come altro tipo

                string randomStartString = masterDerivablePool.ElementAt(_random.Next(masterDerivablePool.Count));
                // Assicurati che startString e target non siano uguali per evitare percorsi vuoti
                if (randomStartString == target)
                {
                    // Se sono uguali, prova a selezionare un'altra startString casuale
                    // oppure salta questo target se non si trova una startString diversa dopo pochi tentativi
                    int retryCount = 0;
                    while (randomStartString == target && retryCount < 10 && masterDerivablePool.Count > 1)
                    {
                        randomStartString = masterDerivablePool.ElementAt(_random.Next(masterDerivablePool.Count));
                        retryCount++;
                    }
                    if (randomStartString == target) continue; // Se non si è trovata una startString diversa, salta
                }


                // Ricerca con BFS
                List<PathStepInfo> bfsPath = await FindSolutionPathInternally(randomStartString, target, _globalMaxStringLength, maxDerivationDepthForSeeding * 5, cancellationToken, PathFindingAlgorithm.BFS);
                pathSearchAttempts++;
                if (bfsPath != null && bfsPath.Any())
                {
                    foreach (var step in bfsPath)
                    {
                        bfsDiscoveredSolutionPaths.Add(step.StateString);
                    }
                    _logger.Log(LogLevel.DEBUG, $"[MiuDerivationSeeder] BFS path found for '{randomStartString}' -> '{target}'. Length: {bfsPath.Count}. Total BFS found: {bfsDiscoveredSolutionPaths.Count}", false);
                }
                else
                {
                    _logger.Log(LogLevel.DEBUG, $"[MiuDerivationSeeder] No BFS path found for '{randomStartString}' -> '{target}'.", false);
                }

                if (cancellationToken.IsCancellationRequested) break;

                // Ricerca con DFS
                List<PathStepInfo> dfsPath = await FindSolutionPathInternally(randomStartString, target, _globalMaxStringLength, maxDerivationDepthForSeeding * 5, cancellationToken, PathFindingAlgorithm.DFS);
                pathSearchAttempts++;
                if (dfsPath != null && dfsPath.Any())
                {
                    foreach (var step in dfsPath)
                    {
                        dfsDiscoveredSolutionPaths.Add(step.StateString);
                    }
                    _logger.Log(LogLevel.DEBUG, $"[MiuDerivationSeeder] DFS path found for '{randomStartString}' -> '{target}'. Length: {dfsPath.Count}. Total DFS found: {dfsDiscoveredSolutionPaths.Count}", false);
                }
                else
                {
                    _logger.Log(LogLevel.DEBUG, $"[MiuDerivationSeeder] No DFS path found for '{randomStartString}' -> '{target}'.", false);
                }
            }

            // Calcola le stringhe uniche e comuni tra i risultati di BFS e DFS
            var uniqueToBfs = bfsDiscoveredSolutionPaths.Except(dfsDiscoveredSolutionPaths).ToList();
            var uniqueToDfs = dfsDiscoveredSolutionPaths.Except(bfsDiscoveredSolutionPaths).ToList();
            var commonSolutionPaths = bfsDiscoveredSolutionPaths.Intersect(dfsDiscoveredSolutionPaths).ToList();

            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] SolutionPath candidates: Unique to BFS: {uniqueToBfs.Count}, Unique to DFS: {uniqueToDfs.Count}, Common: {commonSolutionPaths.Count}.", false);
            Console.WriteLine($"SolutionPath candidates: Unique to BFS: {uniqueToBfs.Count}, Unique to DFS: {uniqueToDfs.Count}, Common: {commonSolutionPaths.Count}.");

            // Popola la lista statesToInsert con SolutionPath, dando priorità alla diversità
            int actualSolutionPathCount = 0;

            // 1. Aggiungi le stringhe uniche a BFS
            foreach (string s in uniqueToBfs.OrderBy(x => _random.Next()))
            {
                if (cancellationToken.IsCancellationRequested || actualSolutionPathCount >= targetSolutionPathCount) break;
                if (uniqueStringsTracker.Add(s))
                {
                    statesToInsert.Add(new SeederMiuState
                    {
                        CurrentString = s,
                        StringLength = s.Length,
                        DeflateString = SeederDbAccess.CompressMiuString(s),
                        Hash = SeederDbAccess.CalculateSha256Hash(s),
                        DiscoveryTime_Int = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                        DiscoveryTime_Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        UsageCount = 0,
                        SeedingType = SeedingType.SolutionPath
                    });
                    actualSolutionPathCount++;
                }
            }

            // 2. Aggiungi le stringhe uniche a DFS
            foreach (string s in uniqueToDfs.OrderBy(x => _random.Next()))
            {
                if (cancellationToken.IsCancellationRequested || actualSolutionPathCount >= targetSolutionPathCount) break;
                if (uniqueStringsTracker.Add(s))
                {
                    statesToInsert.Add(new SeederMiuState
                    {
                        CurrentString = s,
                        StringLength = s.Length,
                        DeflateString = SeederDbAccess.CompressMiuString(s),
                        Hash = SeederDbAccess.CalculateSha256Hash(s),
                        DiscoveryTime_Int = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                        DiscoveryTime_Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        UsageCount = 0,
                        SeedingType = SeedingType.SolutionPath
                    });
                    actualSolutionPathCount++;
                }
            }

            // 3. Completa con le stringhe comuni, se necessario
            foreach (string s in commonSolutionPaths.OrderBy(x => _random.Next()))
            {
                if (cancellationToken.IsCancellationRequested || actualSolutionPathCount >= targetSolutionPathCount) break;
                if (uniqueStringsTracker.Add(s))
                {
                    statesToInsert.Add(new SeederMiuState
                    {
                        CurrentString = s,
                        StringLength = s.Length,
                        DeflateString = SeederDbAccess.CompressMiuString(s),
                        Hash = SeederDbAccess.CalculateSha256Hash(s),
                        DiscoveryTime_Int = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                        DiscoveryTime_Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        UsageCount = 0,
                        SeedingType = SeedingType.SolutionPath
                    });
                    actualSolutionPathCount++;
                }
            }

            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] Prepared {actualSolutionPathCount} unique 'SolutionPath' strings.", false);
            Console.WriteLine($"Prepared {actualSolutionPathCount} unique 'SolutionPath' strings.");

            // --- FASE 3: Preparazione di stringhe Derivable (Tipo 1) ---
            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] Preparing {targetDerivableCount} 'Derivable' strings.", false);
            Console.WriteLine($"Preparing {targetDerivableCount} 'Derivable' strings.");

            int actualDerivableCount = 0;
            // Ordina casualmente le stringhe nel masterDerivablePool per selezionare un sottoinsieme diverso
            foreach (string derivableString in masterDerivablePool.OrderBy(s => _random.Next()))
            {
                if (cancellationToken.IsCancellationRequested || actualDerivableCount >= targetDerivableCount) break;

                // Aggiungi solo se non è già stata aggiunta come SolutionPath
                if (uniqueStringsTracker.Add(derivableString))
                {
                    statesToInsert.Add(new SeederMiuState
                    {
                        CurrentString = derivableString,
                        StringLength = derivableString.Length,
                        DeflateString = SeederDbAccess.CompressMiuString(derivableString),
                        Hash = SeederDbAccess.CalculateSha256Hash(derivableString),
                        DiscoveryTime_Int = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                        DiscoveryTime_Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        UsageCount = 0,
                        SeedingType = SeedingType.Derivable
                    });
                    actualDerivableCount++;
                    _logger.Log(LogLevel.DEBUG, $"[MiuDerivationSeeder] String '{derivableString}' prepared as 'Derivable'. Count: {actualDerivableCount}/{targetDerivableCount}", false);
                }
            }
            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] Prepared {actualDerivableCount} unique 'Derivable' strings.", false);
            Console.WriteLine($"Prepared {actualDerivableCount} unique 'Derivable' strings.");

            // --- FASE 4: Preparazione di stringhe Random per raggiungere il totale ---
            int currentTotalStrings = statesToInsert.Count;
            int randomStringsNeeded = numTotalStrings - currentTotalStrings;

            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] Preparing {randomStringsNeeded} 'Random' strings to reach total of {numTotalStrings}.", false);
            Console.WriteLine($"Preparing {randomStringsNeeded} 'Random' strings to reach total of {numTotalStrings}.");

            int actualRandomCount = 0;
            int randomAttempts = 0;
            int maxRandomAttempts = randomStringsNeeded * 5; // Limite per evitare loop infiniti se non si trovano abbastanza stringhe uniche

            while (actualRandomCount < randomStringsNeeded && randomAttempts < maxRandomAttempts && !cancellationToken.IsCancellationRequested)
            {
                string randomString = GenerateRandomMiuLikeString(_globalMaxStringLength);
                if (uniqueStringsTracker.Add(randomString))
                {
                    statesToInsert.Add(new SeederMiuState
                    {
                        CurrentString = randomString,
                        StringLength = randomString.Length,
                        DeflateString = SeederDbAccess.CompressMiuString(randomString),
                        Hash = SeederDbAccess.CalculateSha256Hash(randomString),
                        DiscoveryTime_Int = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                        DiscoveryTime_Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        UsageCount = 0,
                        SeedingType = SeedingType.Random
                    });
                    actualRandomCount++;
                    _logger.Log(LogLevel.DEBUG, $"[MiuDerivationSeeder] String '{randomString}' prepared as 'Random'. Count: {actualRandomCount}/{randomStringsNeeded}", false);
                }
                randomAttempts++;
            }
            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] Prepared {actualRandomCount} unique 'Random' strings.", false);
            Console.WriteLine($"Prepared {actualRandomCount} unique 'Random' strings.");

            // --- FASE 5: Inserimento finale di tutte le stringhe nel database ---
            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] Inserting all {statesToInsert.Count} unique strings into the database.", false);
            Console.WriteLine($"Inserting all {statesToInsert.Count} unique strings into the database.");

            // Ordina la lista finale per garantire un ordine di inserimento coerente (es. per hash o lunghezza)
            // L'ordine non è più topologico, ma deve essere deterministico per il debugging
            // e per la coerenza degli ID auto-incrementanti dopo lo svuotamento.
            // Un semplice ordinamento per hash della stringa può garantire un ordine "casuale ma ripetibile".
            List<SeederMiuState> finalOrderedStates = statesToInsert.OrderBy(s => s.Hash).ToList();

            int insertedFinalCount = 0;
            foreach (SeederMiuState state in finalOrderedStates)
            {
                if (cancellationToken.IsCancellationRequested) break;

                _logger.Log(LogLevel.DEBUG, $"[MiuDerivationSeeder] DEBUG (Pre-InsertDbAccess): String '{state.CurrentString}'. SeedingType (oggetto): {state.SeedingType}. Valore int: {(int)state.SeedingType}", false);

                await _dbAccess.InsertMiuStateAsync(state);
                insertedFinalCount++;
                _logger.Log(LogLevel.DEBUG, $"[MiuDerivationSeeder] Inserito: '{state.CurrentString}' (Tipo: {state.SeedingType}) ({insertedFinalCount}/{statesToInsert.Count})", false);
            }

            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder] Full database initialization completed. Inserted {insertedFinalCount} total strings.", false);
            Console.WriteLine($"Full database initialization completed. Inserted {insertedFinalCount} total strings.");
            Console.WriteLine($"Final counts: Derivable: {actualDerivableCount}, SolutionPath: {actualSolutionPathCount}, Random: {actualRandomCount}.");
        }

        /// <summary>
        /// Genera un numero specificato di stringhe MIU uniche derivate da una stringa iniziale,
        /// esplorando fino a una profondità massima e rispettando una lunghezza massima.
        /// Restituisce le stringhe man mano che vengono scoperte, fino al limite maxStringsToCollect.
        /// Questo metodo è progettato per popolare il database iniziale con diverse stringhe.
        /// </summary>
        /// <param name="initialString">La stringa MIU iniziale (es. "MI").</param>
        /// <param name="maxDepth">La profondità massima da esplorare per trovare stringhe.</param>
        /// <param name="maxStringsToCollect">Il numero massimo di stringhe uniche da raccogliere.</param>
        /// <param name="maxLength">La lunghezza massima consentita per le stringhe generate.</param>
        /// <param name="cancellationToken">Token per annullare l'operazione.</param>
        /// <returns>Un HashSet di stringhe MIU uniche trovate che rispettano la lunghezza massima.</returns>
        public async Task<HashSet<string>> GenerateLimitedUniqueStringsAsync(string initialString, int maxDepth, int maxStringsToCollect, int maxLength, CancellationToken cancellationToken)
        {
            HashSet<string> collectedUniqueStrings = new HashSet<string>();
            if (maxStringsToCollect <= 0) return collectedUniqueStrings;
            if (maxDepth < 0) return collectedUniqueStrings;

            if (initialString.Length > maxLength)
            {
                _logger.Log(LogLevel.DEBUG, $"[MiuDerivationSeeder.GenerateLimitedUniqueStringsAsync] Stringa iniziale '{initialString}' (lunghezza {initialString.Length}) supera maxLength {maxLength}. Non verrà esplorata.", false);
                return collectedUniqueStrings;
            }

            Queue<(string currentString, int depth)> queue = new Queue<(string, int)>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue((currentString: initialString, depth: 0));
            visited.Add(initialString);

            // Aggiungi la stringa iniziale se valida e non è già stata raccolta
            if (!string.IsNullOrEmpty(initialString) && initialString.Length <= maxLength && collectedUniqueStrings.Count < maxStringsToCollect)
            {
                collectedUniqueStrings.Add(initialString);
            }


            while (queue.Any() && collectedUniqueStrings.Count < maxStringsToCollect && !cancellationToken.IsCancellationRequested)
            {
                var (current, currentDepth) = queue.Dequeue();

                if (currentDepth >= maxDepth)
                {
                    continue;
                }

                var derivedStrings = _miuStringHelper.ApplyAllRules(current);

                foreach (var derived in derivedStrings)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    if (derived.Length > maxLength)
                    {
                        _logger.Log(LogLevel.DEBUG, $"[MiuDerivationSeeder.GenerateLimitedUniqueStringsAsync] Scartata stringa '{derived}' (lunghezza {derived.Length}) perché supera maxLength {maxLength}.", false);
                        continue;
                    }

                    if (!visited.Contains(derived))
                    {
                        visited.Add(derived);
                        collectedUniqueStrings.Add(derived);
                        if (collectedUniqueStrings.Count >= maxStringsToCollect) break;

                        queue.Enqueue((currentString: derived, depth: currentDepth + 1));
                    }
                }
                await Task.Delay(1); // Piccolo delay per non bloccare il thread principale
            }

            _logger.Log(LogLevel.INFO, $"[MiuDerivationSeeder.GenerateLimitedUniqueStringsAsync] Trovate {collectedUniqueStrings.Count} stringhe uniche fino a profondità {maxDepth} e lunghezza massima {maxLength} (max {maxStringsToCollect} richieste) partendo da '{initialString}'.", false);
            return collectedUniqueStrings;
        }

        /// <summary>
        /// Trova un percorso di derivazione da una stringa iniziale a una stringa target utilizzando l'algoritmo specificato.
        /// Restituisce una lista di PathStepInfo che rappresentano il percorso, o null se non trovato.
        /// Questo metodo è stato adattato per usare PathStepInfo e gestire BFS/DFS.
        /// </summary>
        /// <param name="startString">La stringa di partenza.</param>
        /// <param name="targetString">La stringa target da raggiungere.</param>
        /// <param name="maxLength">La lunghezza massima consentita per le stringhe nel percorso.</param>
        /// <param name="maxDepth">La profondità massima di ricerca.</param>
        /// <param name="cancellationToken">Token per annullare l'operazione.</param>
        /// <param name="algorithm">L'algoritmo di ricerca del percorso da utilizzare (BFS o DFS).</param>
        /// <returns>Una lista di PathStepInfo che rappresentano il percorso, o null se non trovato.</returns>
        private async Task<List<PathStepInfo>> FindSolutionPathInternally(string startString, string targetString, int maxLength, int maxDepth, CancellationToken cancellationToken, PathFindingAlgorithm algorithm)
        {
            _logger.Log(LogLevel.INFO, $"[FindSolutionPathInternally] Starting {algorithm} search from '{startString}' to '{targetString}' (Max Length: {maxLength}, Max Depth: {maxDepth}).", false);

            if (startString == targetString)
            {
                _logger.Log(LogLevel.INFO, $"[FindSolutionPathInternally] Start string is already target string: '{startString}'.", false);
                return new List<PathStepInfo> { new PathStepInfo { StateString = startString, Depth = 0, AppliedRuleID = null, ParentStateString = null } };
            }

            // Verifica che startString e targetString non siano null o vuote
            if (string.IsNullOrEmpty(startString) || string.IsNullOrEmpty(targetString))
            {
                _logger.Log(LogLevel.WARNING, $"[FindSolutionPathInternally] Start or target string is null/empty. Start: '{startString}', Target: '{targetString}'. Returning null.", false);
                return null;
            }


            if (algorithm == PathFindingAlgorithm.BFS)
            {
                Queue<(string currentString, List<PathStepInfo> path)> queue = new Queue<(string, List<PathStepInfo>)>();
                HashSet<string> visited = new HashSet<string>();

                queue.Enqueue((currentString: startString, path: new List<PathStepInfo> { new PathStepInfo { StateString = startString, Depth = 0, AppliedRuleID = null, ParentStateString = null } }));
                visited.Add(startString);

                while (queue.Any() && !cancellationToken.IsCancellationRequested)
                {
                    var (current, path) = queue.Dequeue();

                    // Controlla la profondità massima
                    if (path.Count - 1 >= maxDepth) // current depth is path.Count - 1
                    {
                        continue;
                    }

                    var derivedStrings = _miuStringHelper.ApplyAllRules(current);

                    foreach (var derived in derivedStrings)
                    {
                        if (cancellationToken.IsCancellationRequested) return null;

                        if (derived.Length > maxLength)
                        {
                            _logger.Log(LogLevel.DEBUG, $"[BFS-Internal] Derived string '{derived}' (length {derived.Length}) exceeds max length {maxLength}. Skipping.", false);
                            continue;
                        }

                        if (derived == targetString)
                        {
                            // Per il target, non abbiamo un RuleID o ParentStateString diretto dallo step del target stesso,
                            // ma il PathStepInfo del target è l'ultimo del percorso.
                            // Possiamo migliorare catturando la regola applicata per arrivare al target.
                            // Per ora, manteniamo null per RuleID e ParentStateString per il target stesso.
                            var newPathStep = new PathStepInfo { StateString = derived, Depth = path.Count, AppliedRuleID = null, ParentStateString = current };
                            List<PathStepInfo> newPath = new List<PathStepInfo>(path) { newPathStep };
                            _logger.Log(LogLevel.INFO, $"[BFS-Internal] Path found for '{targetString}'! Path length: {newPath.Count - 1}.", false);
                            return newPath;
                        }

                        if (visited.Add(derived))
                        {
                            // Qui potremmo passare il RuleID della regola che ha generato 'derived' da 'current'
                            var newPathStep = new PathStepInfo { StateString = derived, Depth = path.Count, AppliedRuleID = null, ParentStateString = current };
                            List<PathStepInfo> newPath = new List<PathStepInfo>(path) { newPathStep };
                            queue.Enqueue((currentString: derived, path: newPath));
                            _logger.Log(LogLevel.DEBUG, $"[BFS-Internal] Enqueued '{derived}' (depth: {newPath.Count - 1}). Queue size: {queue.Count}.", false);
                        }
                    }
                    await Task.Delay(1); // Piccolo delay per non bloccare il thread principale
                }
            }
            else if (algorithm == PathFindingAlgorithm.DFS)
            {
                // Implementazione DFS
                Stack<(string currentString, List<PathStepInfo> path)> stack = new Stack<(string, List<PathStepInfo>)>();
                HashSet<string> visited = new HashSet<string>();

                stack.Push((currentString: startString, path: new List<PathStepInfo> { new PathStepInfo { StateString = startString, Depth = 0, AppliedRuleID = null, ParentStateString = null } }));
                visited.Add(startString);

                while (stack.Any() && !cancellationToken.IsCancellationRequested)
                {
                    var (current, path) = stack.Pop();

                    // Controlla la profondità massima
                    if (path.Count - 1 >= maxDepth) // current depth is path.Count - 1
                    {
                        continue;
                    }

                    var derivedStrings = _miuStringHelper.ApplyAllRules(current);

                    // Per DFS, push in reverse order to explore the first derived path deeper
                    // L'ordine di ApplyAllRules è determinato dall'ordine delle regole.
                    // Invertire l'ordine qui può influenzare quale "primo" percorso viene trovato.
                    foreach (var derived in derivedStrings.Reverse())
                    {
                        if (cancellationToken.IsCancellationRequested) return null;

                        if (derived.Length > maxLength)
                        {
                            _logger.Log(LogLevel.DEBUG, $"[DFS-Internal] Derived string '{derived}' (length {derived.Length}) exceeds max length {maxLength}. Skipping.", false);
                            continue;
                        }

                        if (derived == targetString)
                        {
                            var newPathStep = new PathStepInfo { StateString = derived, Depth = path.Count, AppliedRuleID = null, ParentStateString = current };
                            List<PathStepInfo> newPath = new List<PathStepInfo>(path) { newPathStep };
                            _logger.Log(LogLevel.INFO, $"[DFS-Internal] Path found for '{targetString}'! Path length: {newPath.Count - 1}.", false);
                            return newPath;
                        }

                        if (visited.Add(derived))
                        {
                            var newPathStep = new PathStepInfo { StateString = derived, Depth = path.Count, AppliedRuleID = null, ParentStateString = current };
                            List<PathStepInfo> newPath = new List<PathStepInfo>(path) { newPathStep };
                            stack.Push((currentString: derived, path: newPath));
                            _logger.Log(LogLevel.DEBUG, $"[DFS-Internal] Pushed '{derived}' (depth: {newPath.Count - 1}). Stack size: {stack.Count}.", false);
                        }
                    }
                    await Task.Delay(1); // Piccolo delay per non bloccare il thread principale
                }
            }

            _logger.Log(LogLevel.INFO, $"[FindSolutionPathInternally] No solution found from '{startString}' to '{targetString}' using {algorithm} within limits.", false);
            return null; // Percorso non trovato
        }

        /// <summary>
        /// Genera una stringa casuale "MIU-like" di lunghezza casuale.
        /// </summary>
        /// <param name="maxLength">La lunghezza massima per la stringa casuale.</param>
        /// <returns>Una stringa casuale composta da M, I, U.</returns>
        private string GenerateRandomMiuLikeString(int maxLength)
        {
            int length = _random.Next(1, maxLength + 1);
            StringBuilder sb = new StringBuilder();
            char[] chars = { 'M', 'I', 'U' };

            sb.Append('M'); // Le stringhe MIU iniziano sempre con 'M'
            if (length > 1)
            {
                for (int i = 1; i < length; i++)
                {
                    sb.Append(chars[_random.Next(chars.Length)]);
                }
            }
            return sb.ToString();
        }
    }
}
