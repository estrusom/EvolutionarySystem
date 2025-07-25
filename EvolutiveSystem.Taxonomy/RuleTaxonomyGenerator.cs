// File: EvolutiveSystem.Taxonomy/RuleTaxonomyGenerator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MasterLog;
using EvolutiveSystem.Common; // Per RuleStatistics, RegolaMIU (se sono ancora in Common)
using EvolutiveSystem.Taxonomy.Antithesis; // AGGIUNGI QUESTA RIGA SE MANCA
using MIU.Core;

namespace EvolutiveSystem.Taxonomy // Questo è il namespace corretto per questo progetto
{
    /// <summary>
    /// Genera una tassonomia delle regole MIU basata su metriche di apprendimento
    /// come l'efficacia e la frequenza di applicazione.
    /// </summary>
    public class RuleTaxonomyGenerator
    {
        private readonly IMIUDataManager _dataManager;
        private readonly Logger _logger;

        // Soglie di esempio per la categorizzazione. Possono essere rese configurabili.
        private const double HIGH_EFFECTIVENESS_THRESHOLD = 0.8; // Efficacia > 80%
        private const double MEDIUM_EFFECTIVENESS_THRESHOLD = 0.5; // Efficacia tra 50% e 80%
        private const int FREQUENTLY_USED_THRESHOLD = 100; // Applicata più di 100 volte
        // 2025.07.18 Dizionario per memorizzare le statistiche sui pattern astratti delle stringhe MIU
        private Dictionary<MiuAbstractPattern, MiuPatternStatistics> _miuPatternStatistics;
        /// <summary>
        /// Costruttore di RuleTaxonomyGenerator.
        /// </summary>
        /// <param name="dataManager">L'istanza del gestore dati per accedere alle statistiche delle regole.</param>
        /// <param name="logger">L'istanza del logger.</param>
        public RuleTaxonomyGenerator(IMIUDataManager dataManager, Logger logger)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _miuPatternStatistics = new Dictionary<MiuAbstractPattern, MiuPatternStatistics>(); // <--- AGGIUNTA: Inizializza il dizionario
            _logger.Log(LogLevel.DEBUG, "RuleTaxonomyGenerator istanziato.");
        }

        /// <summary>
        /// Genera una nuova tassonomia delle regole MIU basata sulle statistiche attuali.
        /// </summary>
        /// <returns>Un oggetto RuleTaxonomy contenente la classificazione delle regole.</returns>
        public RuleTaxonomy GenerateRuleTaxonomy() 
        {
            _logger.Log(LogLevel.INFO, "Generazione della tassonomia delle regole MIU avviata.");

            var ruleStatistics = _dataManager.LoadRuleStatistics();
            var allRules = _dataManager.LoadRegoleMIU().ToDictionary(r => r.ID); // Carica tutte le regole per nome/descrizione

            var taxonomy = new RuleTaxonomy 
            {
                Id = 1, // ID fisso per la tassonomia principale delle regole
                Name = "Tassonomia Efficacia e Uso Regole MIU",
                Description = "Classificazione delle regole basata sull'efficacia storica e la frequenza di applicazione.",
                LastGenerated = DateTime.UtcNow
            };

            // Creazione dei nodi radice per l'efficacia
            var highEffectivenessNode = new RuleTaxonomyNode { Name = "Alta Efficacia", Description = $"Regole con efficacia >= {HIGH_EFFECTIVENESS_THRESHOLD * 100}%" };
            var mediumEffectivenessNode = new RuleTaxonomyNode { Name = "Media Efficacia", Description = $"Regole con efficacia tra {MEDIUM_EFFECTIVENESS_THRESHOLD * 100}% e {HIGH_EFFECTIVENESS_THRESHOLD * 100}%" };
            var lowEffectivenessNode = new RuleTaxonomyNode { Name = "Bassa Efficacia", Description = $"Regole con efficacia < {MEDIUM_EFFECTIVENESS_THRESHOLD * 100}%" };
            var unknownEffectivenessNode = new RuleTaxonomyNode { Name = "Efficacia Sconosciuta", Description = "Regole senza statistiche di applicazione." };

            // Aggiungi i nodi radice alla tassonomia
            taxonomy.RootNodes.Add(highEffectivenessNode);
            taxonomy.RootNodes.Add(mediumEffectivenessNode);
            taxonomy.RootNodes.Add(lowEffectivenessNode);
            taxonomy.RootNodes.Add(unknownEffectivenessNode);


            // Popola i nodi con le regole e crea sottocategorie per la frequenza d'uso
            foreach (var stat in ruleStatistics.Values)
            {
                RuleTaxonomyNode targetNode;
                if (stat.ApplicationCount == 0)
                {
                    targetNode = unknownEffectivenessNode;
                }
                else if (stat.EffectivenessScore >= HIGH_EFFECTIVENESS_THRESHOLD)
                {
                    targetNode = highEffectivenessNode;
                }
                else if (stat.EffectivenessScore >= MEDIUM_EFFECTIVENESS_THRESHOLD)
                {
                    targetNode = mediumEffectivenessNode;
                }
                else
                {
                    targetNode = lowEffectivenessNode;
                }

                // Aggiungi la regola all'elenco di RuleIds del nodo principale
                targetNode.RuleIds.Add(stat.RuleID);

                // Creazione di sottocategorie per la frequenza d'uso
                RuleTaxonomyNode frequencyNode = null;
                if (stat.ApplicationCount >= FREQUENTLY_USED_THRESHOLD)
                {
                    frequencyNode = targetNode.Children.FirstOrDefault(n => n.Name == "Usate Frequentemente");
                    if (frequencyNode == null)
                    {
                        frequencyNode = new RuleTaxonomyNode { Name = "Usate Frequentemente", Description = $"Applicate >= {FREQUENTLY_USED_THRESHOLD} volte", ParentId = targetNode.Id };
                        targetNode.Children.Add(frequencyNode);
                    }
                }
                else if (stat.ApplicationCount > 0)
                {
                    frequencyNode = targetNode.Children.FirstOrDefault(n => n.Name == "Usate Raramente");
                    if (frequencyNode == null)
                    {
                        frequencyNode = new RuleTaxonomyNode { Name = "Usate Raramente", Description = $"Applicate < {FREQUENTLY_USED_THRESHOLD} volte", ParentId = targetNode.Id };
                        targetNode.Children.Add(frequencyNode);
                    }
                }
                else // ApplicationCount == 0
                {
                    frequencyNode = targetNode.Children.FirstOrDefault(n => n.Name == "Mai Usate");
                    if (frequencyNode == null)
                    {
                        frequencyNode = new RuleTaxonomyNode { Name = "Mai Usate", Description = "Non ancora applicate.", ParentId = unknownEffectivenessNode.Id };
                        targetNode.Children.Add(frequencyNode);
                    }
                }

                // Aggiungi la regola anche al nodo di frequenza (se esiste)
                if (frequencyNode != null)
                {
                    if (!frequencyNode.RuleIds.Contains(stat.RuleID)) // Evita duplicati se una regola può finire in più sottocategorie per qualche ragione
                    {
                        frequencyNode.RuleIds.Add(stat.RuleID);
                    }
                }
            }

            // Per le regole che non hanno statistiche (es. nuove regole appena inserite)
            // Assicurati che ogni regola sia inclusa da qualche parte
            foreach (var ruleEntry in allRules)
            {
                if (!ruleStatistics.ContainsKey(ruleEntry.Key))
                {
                    // Se una regola non ha statistiche, la mettiamo nel nodo "Efficacia Sconosciuta"
                    if (!unknownEffectivenessNode.RuleIds.Contains(ruleEntry.Key))
                    {
                        unknownEffectivenessNode.RuleIds.Add(ruleEntry.Key);
                    }
                    var neverUsedNode = unknownEffectivenessNode.Children.FirstOrDefault(n => n.Name == "Mai Usate");
                    if (neverUsedNode == null)
                    {
                        neverUsedNode = new RuleTaxonomyNode { Name = "Mai Usate", Description = "Non ancora applicate.", ParentId = unknownEffectivenessNode.Id };
                        unknownEffectivenessNode.Children.Add(neverUsedNode);
                    }
                    if (!neverUsedNode.RuleIds.Contains(ruleEntry.Key))
                    {
                        neverUsedNode.RuleIds.Add(ruleEntry.Key);
                    }
                }
            }


            _logger.Log(LogLevel.INFO, $"Tassonomia delle regole MIU generata con {taxonomy.RootNodes.Count} nodi radice.");
            return taxonomy;
        }
        /// <summary>
        /// Analizza una stringa MIU e ne estrae un set di pattern astratti.
        /// Questa è la fase iniziale di "sfocatura intelligente" per il modulo Taxonomy.
        /// </summary>
        /// <param name="miuStringStandard">La stringa MIU in formato standard (decompresso).</param>
        /// <returns>Una lista di MiuAbstractPattern identificati nella stringa.</returns>
        private List<MiuAbstractPattern> AnalyzeMiuStringForPatterns(string miuStringStandard)
        {
            List<MiuAbstractPattern> patterns = new List<MiuAbstractPattern>();

            // Esempio 1: Pattern basato sulla lunghezza della stringa
            // MODIFICA: Utilizza la classe concreta InefficiencyPattern
            patterns.Add(new InefficiencyPattern("StringLength", miuStringStandard.Length.ToString(), $"Lunghezza: {miuStringStandard.Length}"));

            // Esempio 2: Pattern basato sulla parità del conteggio dei caratteri 'I'
            // Assicurati che MIUStringConverter sia accessibile (dovrebbe essere in EvolutiveSystem.Common)
            int iCount = MIUStringConverter.CountChar(miuStringStandard, 'I'); 
            // MODIFICA: Utilizza la classe concreta InefficiencyPattern
            patterns.Add(new InefficiencyPattern("ICountParity", (iCount % 2 == 0) ? "Even" : "Odd", $"Conteggio 'I' {(iCount % 2 == 0 ? "Pari" : "Dispari")}"));

            // Esempio 3: Pattern basato sulla presenza di sottostringhe chiave
            if (miuStringStandard.Contains("MIU"))
            {
                // MODIFICA: Utilizza la classe concreta InefficiencyPattern
                patterns.Add(new InefficiencyPattern("ContainsMIU", "True", "Contiene 'MIU'"));
            }
            if (miuStringStandard.Contains("MUU"))
            {
                // MODIFICA: Utilizza la classe concreta InefficiencyPattern
                patterns.Add(new InefficiencyPattern("ContainsMUU", "True", "Contiene 'MUU'"));
            }

            // In futuro, qui potremmo aggiungere pattern più complessi (es. regex, struttura interna)
            // o usare un sistema plug-in per definire i pattern.

            _logger.Log(LogLevel.ENANCED_DEBUG, $"[RuleTaxonomyGenerator] Analizzata stringa '{miuStringStandard.Substring(0, Math.Min(miuStringStandard.Length, 30))}...' per pattern. Trovati: {string.Join(", ", patterns.Select(p => p.ToString()))}");

            return patterns;
        }
        /// <summary>
        /// Aggiorna le statistiche per i pattern astratti di una stringa MIU scoperta.
        /// Questo metodo sarà chiamato dall'Orchestrator quando una nuova stringa viene scoperta
        /// o quando fa parte di un percorso di soluzione.
        /// </summary>
        /// <param name="discoveredString">La stringa MIU scoperta (standard).</param>
        /// <param name="isSolutionPathStep">Indica se la stringa fa parte di un percorso di soluzione (per SuccessCount).</param>
        /// <param name="depth">La profondità a cui la stringa è stata scoperta.</param>
        public void UpdatePatternStatistics(string discoveredString, bool isSolutionPathStep, int depth)
        {
            List<MiuAbstractPattern> patterns = AnalyzeMiuStringForPatterns(discoveredString);

            foreach (var pattern in patterns)
            {
                if (!_miuPatternStatistics.TryGetValue(pattern, out MiuPatternStatistics stats))
                {
                    stats = new MiuPatternStatistics(pattern);
                    _miuPatternStatistics.Add(pattern, stats);
                    _logger.Log(LogLevel.DEBUG, $"[RuleTaxonomyGenerator] Nuovo pattern tracciato: {pattern}");
                }

                stats.DiscoveryCount++;
                stats.TotalDepth += depth;
                if (isSolutionPathStep)
                {
                    stats.SuccessCount++;
                }
                stats.LastUpdated = DateTime.UtcNow;
            }

            _logger.Log(LogLevel.INFO, $"[RuleTaxonomyGenerator] Statistiche pattern aggiornate per '{discoveredString.Substring(0, Math.Min(discoveredString.Length, 30))}...'. Totale pattern tracciati: {_miuPatternStatistics.Count}");
        }
        /// <summary>
        /// Metodo per recuperare le statistiche dei pattern. Utile per debug o per future analisi.
        /// </summary>
        public IReadOnlyDictionary<MiuAbstractPattern, MiuPatternStatistics> GetMiuPatternStatistics()
        {
            return _miuPatternStatistics;
        }
        /// <summary>
        /// 2025.07.24
        /// Identifica i "gap" nell'esplorazione del paesaggio MIU basandosi sulle statistiche dei pattern.
        /// Un gap è un pattern poco o per nulla scoperto, che indica un'area inesplorata.
        /// </summary>
        /// <param name="gapThreshold">La soglia di DiscoveryCount al di sotto della quale un pattern è considerato un gap.</param>
        /// <returns>Una lista di MiuAbstractPattern che rappresentano i gap identificati.</returns>
        public List<MiuAbstractPattern> IdentifyGaps(long gapThreshold = 5) // Soglia di default, può essere configurabile
        {
            _logger.Log(LogLevel.INFO, $"[RuleTaxonomyGenerator] Avvio identificazione dei gap (soglia DiscoveryCount < {gapThreshold}).");

            List<MiuAbstractPattern> gaps = new List<MiuAbstractPattern>();

            foreach (var entry in _miuPatternStatistics)
            {
                var pattern = entry.Key;
                var stats = entry.Value;

                // Un gap è un pattern che è stato scoperto un numero di volte inferiore alla soglia
                // Escludiamo i pattern con DiscoveryCount == 0 perché non sono stati affatto osservati
                // e la loro identificazione come "gap" richiederebbe un set di tutti i pattern possibili,
                // cosa che va oltre lo scopo attuale e la disponibilità di dati.
                if (stats.DiscoveryCount > 0 && stats.DiscoveryCount < gapThreshold)
                {
                    // MODIFICA: Utilizza la classe concreta GapPattern
                    gaps.Add(new GapPattern(pattern.Type, pattern.Value, pattern.Nome));
                    _logger.Log(LogLevel.DEBUG, $"[RuleTaxonomyGenerator] Gap identificato: {pattern} (DiscoveryCount: {stats.DiscoveryCount})");
                }
            }

            _logger.Log(LogLevel.INFO, $"[RuleTaxonomyGenerator] Identificazione dei gap completata. Trovati {gaps.Count} gap.");
            return gaps;
        }
        /// <summary>
        /// 2025.07.24
        /// Identifica le "inefficienze" nell'esplorazione del paesaggio MIU basandosi sulle statistiche dei pattern.
        /// Un'inefficienza è un pattern spesso incontrato, ma raramente parte di un percorso di successo,
        /// o che porta a percorsi di soluzione molto lunghi.
        /// </summary>
        /// <param name="minDiscoveryCount">La soglia minima di DiscoveryCount per considerare un pattern (evita pattern rari).</param>
        /// <param name="maxSuccessRatio">La soglia massima per il rapporto SuccessCount/DiscoveryCount per considerare un'inefficienza.</param>
        /// <param name="maxAverageDepthForEfficiency">La profondità media massima desiderabile per un pattern efficiente.</param>
        /// <returns>Una lista di MiuAbstractPattern che rappresentano le inefficienze identificate.</returns>
        public List<MiuAbstractPattern> IdentifyInefficiencies(long minDiscoveryCount = 10, double maxSuccessRatio = 0.2, double maxAverageDepthForEfficiency = 10.0)
        {
            _logger.Log(LogLevel.INFO, $"[RuleTaxonomyGenerator] Avvio identificazione delle inefficienze (soglie: MinDiscovery={minDiscoveryCount}, MaxSuccessRatio={maxSuccessRatio * 100:N0}%, MaxAvgDepth={maxAverageDepthForEfficiency:N1}).");

            List<MiuAbstractPattern> inefficiencies = new List<MiuAbstractPattern>();

            foreach (var entry in _miuPatternStatistics)
            {
                var pattern = entry.Key;
                var stats = entry.Value;

                // Consideriamo solo i pattern che sono stati scoperti abbastanza volte per avere statistiche significative
                if (stats.DiscoveryCount >= minDiscoveryCount)
                {
                    double successRatio = (double)stats.SuccessCount / stats.DiscoveryCount;

                    bool isInefficient = false;
                    string inefficiencyReason = "";

                    // Condizione 1: Basso rapporto di successo
                    if (successRatio < maxSuccessRatio)
                    {
                        isInefficient = true;
                        inefficiencyReason = $"Basso Success Ratio ({successRatio:P2})";
                    }
                    // Condizione 2: Profondità media troppo alta per i successi (se ci sono stati successi)
                    // Questo indica che, pur portando a soluzioni, sono soluzioni difficili/lunghe
                    else if (stats.SuccessCount > 0 && stats.AverageDepth > maxAverageDepthForEfficiency)
                    {
                        isInefficient = true;
                        inefficiencyReason = $"Alta Profondità Media ({stats.AverageDepth:N1})";
                    }

                    if (isInefficient)
                    {
                        // MODIFICA: Utilizza la classe concreta InefficiencyPattern
                        inefficiencies.Add(new InefficiencyPattern(pattern.Type, pattern.Value, $"{pattern.Nome} - {inefficiencyReason}"));
                        _logger.Log(LogLevel.DEBUG, $"[RuleTaxonomyGenerator] Inefficienza identificata: {pattern} ({inefficiencyReason}, DC: {stats.DiscoveryCount}, SR: {successRatio:P2}, AvgDepth: {stats.AverageDepth:N1})");
                    }
                }
            }

            _logger.Log(LogLevel.INFO, $"[RuleTaxonomyGenerator] Identificazione delle inefficienze completata. Trovate {inefficiencies.Count} inefficienze.");
            return inefficiencies;
        }
    }
}
