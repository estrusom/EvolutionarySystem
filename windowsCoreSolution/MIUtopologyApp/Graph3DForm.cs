using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MIUtopologyApp
{
    public partial class Graph3DForm : Form
    {
        private readonly TopologyGraph _topology;
        private bool _isGraphDrawn = false;

        public Graph3DForm(TopologyGraph Topology)
        {
            InitializeComponent();
            _topology = Topology;
            // Chiama il metodo di inizializzazione all'avvio del form
            this.Load += async (sender, e) =>
            {
                await InitializeWebViewAndDrawGraph();
            };
        }

        private async Task InitializeWebViewAndDrawGraph()
        {
            try
            {
                // Assicura l'inizializzazione di WebView2
                await webViewBrowser.EnsureCoreWebView2Async(null);

                // Collega l'evento di navigazione completata
                webViewBrowser.CoreWebView2.NavigationCompleted += async (sender, e) =>
                {
                    if (e.IsSuccess && !_isGraphDrawn)
                    {
                        // Quando la navigazione è completata, disegna il grafico
                        await DrawGraph();
                        _isGraphDrawn = true;
                    }
                };

                // Percorso del file HTML
                string htmlFilePath = Path.Combine(Application.StartupPath, "htmlResources", "graph.html");

                // Naviga verso il file HTML
                if (File.Exists(htmlFilePath))
                {
                    webViewBrowser.CoreWebView2.Navigate($"file:///{htmlFilePath}");
                }
                else
                {
                    MessageBox.Show("Il file 'graph.html' non è stato trovato.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'inizializzazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DrawGraph()
        {
            if (_topology == null || _topology.Nodes.Count == 0)
            {
                return;
            }

            var graphData = new
            {
                nodes = _topology.Nodes.Select(n => new { id = n.Id, name = n.Name, type = n.Type }).ToList(),
                links = _topology.Links.Select(l => new { source = l.Source.Id, target = l.Target.Id, type = l.ConnectionType }).ToList()
            };

            string jsonData = JsonSerializer.Serialize(graphData);
            string script = $"updateGraph({jsonData});";

            await webViewBrowser.CoreWebView2.ExecuteScriptAsync(script);
        }

        private void btnSource_Click(object sender, EventArgs e)
        {
            SaveGraphToFile();
        }
        private void SaveGraphToFile()
        {
            if (_topology == null || _topology.Nodes.Count == 0)
            {
                MessageBox.Show("Nessun dato del grafico da salvare.", "Informazione", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Prepara i dati del grafico come stringa JSON
            var graphData = new
            {
                nodes = _topology.Nodes.Select(n => new { id = n.Id, name = n.Name, type = n.Type }).ToList(),
                links = _topology.Links.Select(l => new { source = l.Source.Id, target = l.Target.Id, type = l.ConnectionType }).ToList()
            };
            string jsonData = JsonSerializer.Serialize(graphData, new JsonSerializerOptions { WriteIndented = true });

            // Leggi il template HTML dal file esistente
            string htmlTemplatePath = Path.Combine(Application.StartupPath, "htmlResources", "graph.html");
            if (!File.Exists(htmlTemplatePath))
            {
                MessageBox.Show("Il file 'graph.html' non è stato trovato.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string htmlContent = File.ReadAllText(htmlTemplatePath);

            // Inietta i dati JSON nel template HTML, prima del tag di chiusura del body
            string scriptInjection = $"<script>window.initialGraphData = {jsonData};</script>";
            htmlContent = htmlContent.Replace("</body>", scriptInjection + "\n</body>");

            // Mostra una finestra di dialogo per salvare il file
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "File HTML (*.html)|*.html";
                saveFileDialog.Title = "Salva il grafico 3D";
                saveFileDialog.FileName = "moebius_graph.html";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, htmlContent);
                        MessageBox.Show($"Grafico salvato con successo in:\n{saveFileDialog.FileName}", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore durante il salvataggio del file: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}