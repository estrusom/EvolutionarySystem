using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using EvolutiveSystem.Common;
using EvolutiveSystem.Learning;
using MasterLog; // Corretto il namespace del logger

namespace EvolutiveSystem.Taxonomy
{
    public class RuleTaxonomyGenerator
    {
        private readonly ILearningStatisticsManager _learningStatsManager;
        private readonly Logger _logger; // Usiamo la classe concreta Logger come negli altri file

        public RuleTaxonomyGenerator(ILearningStatisticsManager learningStatsManager, Logger logger)
        {
            _learningStatsManager = learningStatsManager;
            _logger = logger;
        }
        /* prima di 25.09.05
        public InefficiencyPatternType ClassifyRuleTransition(TransitionStatistics stats)
        {
            // La tua classe TransitionStatistics ha 'TotalApplications'
            if (stats.SuccessRate < 0.1 && stats.ApplicationCount > 20)
            {
                return InefficiencyPatternType.BlackHole;
            }

            return InefficiencyPatternType.None;
        }
        */
        // dopo 25.09.05
        public InefficiencyPatternType ClassifyRuleTransition(TransitionStatistics stats)
        {
            // Criterio #1: BlackHole (Regola inefficiente e molto usata)
            // Se ha un tasso di successo bassissimo ma il sistema insiste a usarla.
            if (stats.SuccessRate < 0.1 && stats.ApplicationCount > 20)
            {
                return InefficiencyPatternType.BlackHole;
            }

            // Criterio #2: RarelyUsed (Regola usata pochissimo)
            // Se è stata applicata solo poche volte in totale.
            // Potrebbe non essere inutile, ma è certamente rara.
            if (stats.ApplicationCount < 5)
            {
                return InefficiencyPatternType.RarelyUsed;
            }

            // Criterio #3: CyclicPath (Percorso Ciclico)
            // Questa è la più difficile da rilevare solo con le statistiche.
            // Una buona approssimazione è una transizione con un tasso di successo pari a ZERO
            // ma con un numero medio di tentativi. Significa che OGNI volta
            // che è stata applicata, ha portato a uno stato già noto.
            if (stats.SuccessRate == 0 && stats.ApplicationCount > 5)
            {
                return InefficiencyPatternType.CyclicPath;
            }

            // Se nessuna delle condizioni sopra è vera, la transizione è considerata "normale".
            return InefficiencyPatternType.None;
        }
        // Metodo reso SINCRONO per allinearsi con GetTransitionProbabilities()
        public Dictionary<string, InefficiencyPatternType> GenerateFullTaxonomy()
        {
            _logger.Log(LogLevel.INFO, "Inizio della generazione della tassonomia completa delle regole...");
            var taxonomyResults = new Dictionary<string, InefficiencyPatternType>();

            // Chiamata al metodo SINCRONO esistente
            var transitionStats = _learningStatsManager.GetTransitionProbabilities();

            if (transitionStats == null || !transitionStats.Any())
            {
                // Ho sostituito WARN con INFO
                _logger.Log(LogLevel.INFO, "Nessuna statistica di transizione trovata. La tassonomia sarà vuota.");
                return taxonomyResults;
            }

            foreach (var statEntry in transitionStats)
            {
                var transitionInfo = statEntry.Key;
                var stats = statEntry.Value;

                var pattern = ClassifyRuleTransition(stats);

                string ruleKey = $"Da '{transitionInfo.Item1}' (Regola {transitionInfo.Item2})";
                taxonomyResults[ruleKey] = pattern;
            }

            _logger.Log(LogLevel.INFO, $"Generazione della tassonomia completata. {taxonomyResults.Count} transizioni uniche classificate.");
            return taxonomyResults;
        }

        public async Task SaveTaxonomyAsXmlAsync(Dictionary<string, InefficiencyPatternType> taxonomy, string filePath)
        {
            _logger.Log(LogLevel.INFO, $"Salvataggio della tassonomia in XML in corso in: {filePath}...");
            try
            {
                var serializableList = taxonomy.Select(kvp => new SerializableTaxonomyItem
                {
                    Transition = kvp.Key,
                    Pattern = kvp.Value.ToString()
                }).ToList();

                var serializer = new XmlSerializer(typeof(List<SerializableTaxonomyItem>));

                await Task.Run(() =>
                {
                    using (var writer = new StreamWriter(filePath))
                    {
                        serializer.Serialize(writer, serializableList);
                    }
                });

                _logger.Log(LogLevel.INFO, "Salvataggio della tassonomia in XML completato con successo.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore durante il salvataggio della tassonomia in XML: {ex.Message}");
            }
        }
    }

    [Serializable]
    public class SerializableTaxonomyItem
    {
        public string Transition { get; set; }
        public string Pattern { get; set; }
    }
}

