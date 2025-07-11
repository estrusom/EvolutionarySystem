// File: EvolutiveSystem.Taxonomy/RuleTaxonomyGenerator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MasterLog;
using EvolutiveSystem.Common; // Per RuleStatistics, RegolaMIU (se sono ancora in Common)
using EvolutiveSystem.Taxonomy;

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

        /// <summary>
        /// Costruttore di RuleTaxonomyGenerator.
        /// </summary>
        /// <param name="dataManager">L'istanza del gestore dati per accedere alle statistiche delle regole.</param>
        /// <param name="logger">L'istanza del logger.</param>
        public RuleTaxonomyGenerator(IMIUDataManager dataManager, Logger logger)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Log(LogLevel.DEBUG, "RuleTaxonomyGenerator istanziato.");
        }

        /// <summary>
        /// Genera una nuova tassonomia delle regole MIU basata sulle statistiche attuali.
        /// </summary>
        /// <returns>Un oggetto RuleTaxonomy contenente la classificazione delle regole.</returns>
        public RuleTaxonomy GenerateRuleTaxonomy() //<- errore cs0246
        {
            _logger.Log(LogLevel.INFO, "Generazione della tassonomia delle regole MIU avviata.");

            var ruleStatistics = _dataManager.LoadRuleStatistics();
            var allRules = _dataManager.LoadRegoleMIU().ToDictionary(r => r.ID); // Carica tutte le regole per nome/descrizione

            var taxonomy = new RuleTaxonomy //<- errore cs0246
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
    }
}
