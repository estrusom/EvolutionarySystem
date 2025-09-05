using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using EvolutiveSystem.Common;
using MasterLog;

namespace EvolutiveSystem.Taxonomy
{
    public class NodeTaxonomyGenerator
    {
        private readonly Logger _logger;
        private readonly string _basePath;

        public NodeTaxonomyGenerator(string basePath, Logger logger)
        {
            _basePath = basePath;
            _logger = logger;
        }

        public List<NodeClassification> GenerateNodeTaxonomy()
        {
            _logger.Log(LogLevel.INFO, "Inizio della generazione della tassonomia dei nodi...");
            var nodeTaxonomy = new List<NodeClassification>();

            string topologyFilePath = Path.Combine(_basePath, "output", "topology.xml");

            if (!File.Exists(topologyFilePath))
            {
                _logger.Log(LogLevel.ERROR, $"File di topologia non trovato in '{topologyFilePath}'. Eseguire prima la costruzione della topologia.");
                return null;
            }

            // Carichiamo la topologia dal file XML
            MIUStringTopologyData topologyData;
            try
            {
                var serializer = new XmlSerializer(typeof(MIUStringTopologyData));
                using (var reader = new StreamReader(topologyFilePath))
                {
                    topologyData = (MIUStringTopologyData)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore durante la lettura del file XML della topologia: {ex.Message}");
                return null;
            }

            if (topologyData == null || topologyData.Nodes == null)
            {
                _logger.Log(LogLevel.WARNING, "Il file di topologia è vuoto o corrotto.");
                return null;
            }

            // Analizziamo ogni nodo e creiamo la classificazione
            foreach (var node in topologyData.Nodes)
            {
                var classification = new NodeClassification
                {
                    StateID = node.StateID,
                    CurrentString = node.CurrentString,
                    StringLength = node.CurrentString.Length,
                    I_Count = node.CurrentString.Count(c => c == 'I'),
                    U_Count = node.CurrentString.Count(c => c == 'U')
                };

                // Aggiungiamo una proprietà derivata interessante
                classification.Properties.Add("I_Count_Is_Divisible_By_3", (classification.I_Count > 0 && classification.I_Count % 3 == 0).ToString());

                nodeTaxonomy.Add(classification);
            }

            _logger.Log(LogLevel.INFO, $"Tassonomia dei nodi generata. {nodeTaxonomy.Count} nodi classificati.");
            return nodeTaxonomy;
        }

        public async Task SaveNodeTaxonomyAsXmlAsync(List<NodeClassification> taxonomy, string filePath)
        {
            _logger.Log(LogLevel.INFO, $"Salvataggio della tassonomia dei nodi in: {filePath}");
            try
            {
                var serializer = new XmlSerializer(typeof(List<NodeClassification>));
                await Task.Run(() =>
                {
                    using (var writer = new StreamWriter(filePath))
                    {
                        serializer.Serialize(writer, taxonomy);
                    }
                });
                _logger.Log(LogLevel.INFO, "Salvataggio completato con successo.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore durante il salvataggio della tassonomia dei nodi: {ex.Message}");
            }
        }
    }
}