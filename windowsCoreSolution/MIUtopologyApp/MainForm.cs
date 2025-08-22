using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using MasterLog;
using EvolutionarySystem.Core.Models;
using EvolutionarySystem.Data;
using System.Threading.Tasks;

namespace MIUtopologyApp
{
    // Classi per la gestione della topologia.
    // Queste classi restano invariate, in quanto rappresentano il modello dati e non hanno
    // dipendenze esterne.
    public class MemoryUnit
    {
        public string Id { get; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int UsageCount { get; set; }
        public string DetectedPatternHashes_SCSV { get; set; }

        public MemoryUnit(string id, string name, string type, int usageCount, string detectedPatternHashes)
        {
            Id = id;
            Name = name;
            Type = type;
            UsageCount = usageCount;
            DetectedPatternHashes_SCSV = detectedPatternHashes;
        }

        public override string ToString() => $"MU: {Name} (ID: {Id}, Tipo: {Type}, Usage: {UsageCount})";
    }

    public class Link
    {
        public MemoryUnit Source { get; }
        public MemoryUnit Target { get; }
        public string ConnectionType { get; set; }

        public Link(MemoryUnit source, MemoryUnit target, string connectionType)
        {
            Source = source;
            Target = target;
            ConnectionType = connectionType;
        }

        public override string ToString() => $"Collegamento: {Source.Id} -> {Target.Id} (Tipo: {ConnectionType})";
    }

    public class TopologyGraph
    {
        private Dictionary<string, MemoryUnit> _nodes = new Dictionary<string, MemoryUnit>();
        private List<Link> _links = new List<Link>();

        public IReadOnlyCollection<MemoryUnit> Nodes => _nodes.Values;
        public IReadOnlyCollection<Link> Links => _links;

        public void AddNode(MemoryUnit mu)
        {
            if (!_nodes.ContainsKey(mu.Id))
            {
                _nodes[mu.Id] = mu;
            }
        }

        public void AddLink(string sourceId, string targetId, string connectionType)
        {
            if (_nodes.ContainsKey(sourceId) && _nodes.ContainsKey(targetId))
            {
                var sourceMu = _nodes[sourceId];
                var targetMu = _nodes[targetId];
                _links.Add(new Link(sourceMu, targetMu, connectionType));
            }
        }
    }

    public class NodePosition
    {
        public MemoryUnit Node { get; set; }
        public PointF Position { get; set; }
        public RectangleF Bounds { get; set; }
        public bool IsDragging { get; set; }
    }

    //---------------------------------------------------------------------------------
    //  CLASSE PRINCIPALE DEL FORM
    //---------------------------------------------------------------------------------

    public partial class MainForm : Form
    {
        // Variabili per la gestione del grafo e dell'interazione
        private TopologyGraph _topology;
        private List<NodePosition> _nodePositions;
        private PointF _dragOffset;
        private NodePosition _draggingNode;
        private Button btnShow3DGraph;

        // I servizi vengono iniettati tramite il costruttore
        private readonly Logger _logger;
        private readonly IMIUDataService _dataService;
        
        // Costruttore della finestra principale che riceve le dipendenze
        public MainForm(Logger logger, IMIUDataService dataService)
        {
            // Inizializza i servizi iniettati
            _logger = logger;
            _dataService = dataService;

            // Log dell'avvio del form e dell'iniezione delle dipendenze
            _logger.Log(LogLevel.INFO, "MainForm creata e dipendenze iniettate con successo.");

            // Inizializzazione dei componenti e delle proprietà del form
            InitializeComponent();
            this.Text = "Visualizzazione Topologia MIU";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true; // Riduce lo sfarfallio del disegno
            this.ResizeRedraw = true; // Ridisegna il form al ridimensionamento

            // Carica i dati dal database all'avvio del form
            this.Load += MainForm_Load;
            
            // Inizializza il grafo e le posizioni dei nodi
            _topology = new TopologyGraph(); // Inizialmente vuoto
            _nodePositions = new List<NodePosition>();
            
            // Aggiungi gli eventi per il trascinamento
            this.MouseDown += MainForm_MouseDown;
            this.MouseMove += MainForm_MouseMove;
            this.MouseUp += MainForm_MouseUp;
            this.Paint += MainForm_Paint;

            // INTEGRAZIONE: Crea e configura il pulsante da codice
            btnShow3DGraph = new Button();
            btnShow3DGraph.Text = "Visualizza 3D";
            btnShow3DGraph.Name = "btnShow3DGraph"; // Importante per l'identificazione

            // Posiziona il pulsante in alto a destra
            // Assicurati che le dimensioni del form siano già state impostate
            btnShow3DGraph.Size = new Size(120, 30);
            btnShow3DGraph.Location = new Point(this.ClientSize.Width - btnShow3DGraph.Width - 10, 10);
            // Aggiungi il pulsante al form
            this.Controls.Add(btnShow3DGraph);
            btnShow3DGraph.Click += BtnShow3DGraph_Click;
        }

        private void BtnShow3DGraph_Click(object? sender, EventArgs e)
        {
            // L'istanza di _topology è già disponibile, quindi la passiamo al nuovo form.
            var graph3DForm = new Graph3DForm(_topology);
            graph3DForm.Show();
        }

        /// <summary>
        /// Gestisce l'evento di caricamento del form.
        /// </summary>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// Carica i dati dal database in modo asincrono.
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                _logger.Log(LogLevel.INFO, "Avvio del caricamento della topologia dal database.");

                // Accede ai dati usando la destructurazione della tupla
                var (paths, ruleApplications, statesHistory) = await _dataService.GetAllDataAsync();

                var stateHistoryMap = statesHistory.ToDictionary(s => s.Id, s => s);

                using (StreamWriter rawData = new StreamWriter("rawData.txt", false))
                {
                    rawData.WriteLine("MIUPaths:");
                    foreach (var path in paths)
                    {
                        rawData.WriteLine($"ID: {path.StateID}, Parent: {path.ParentStateID}, IsTarget: {path.IsTarget}, IsSuccess: {path.IsSuccess}");
                    }
                    rawData.WriteLine("ruleApplications:");
                    foreach (var ruleApp in ruleApplications)
                    {
                        rawData.WriteLine($"NewStateID: {ruleApp.NewStateID}, AppliedRuleID: {ruleApp.AppliedRuleID}");
                    }
                }

                _topology = new TopologyGraph();
                var addedNodes = new Dictionary<int, MemoryUnit>();

                // Popola il grafo con i dati dei percorsi (MIUPath)
                foreach (var path in paths)
                {
                    // Aggiunge i nodi se non esistono già
                    if (!addedNodes.ContainsKey(path.StateID))
                    {
                        var historyData = stateHistoryMap[path.StateID];
                        var node = new MemoryUnit(path.StateID.ToString(), $"Stato {path.StateID}", "Stato", (int)historyData.UsageCount, historyData.DetectedPatternHashes_SCSV);

                        _topology.AddNode(node);
                        addedNodes.Add(path.StateID, node);
                    }

                    if (path.ParentStateID.HasValue && !addedNodes.ContainsKey(path.ParentStateID.Value))
                    {
                        var historyData = stateHistoryMap[path.ParentStateID.Value];
                        var parentNode = new MemoryUnit(path.ParentStateID.Value.ToString(), $"Stato {path.ParentStateID.Value}", "Stato", (int)historyData.UsageCount, historyData.DetectedPatternHashes_SCSV);
                        _topology.AddNode(parentNode);
                        addedNodes.Add(path.ParentStateID.Value, parentNode);
                    }

                    // Aggiunge il collegamento solo se il ParentStateID esiste
                    if (path.ParentStateID.HasValue)
                    {
                        _topology.AddLink(path.ParentStateID.Value.ToString(), path.StateID.ToString(), "Path");
                    }
                }

                // Ora aggiungiamo anche i nodi dalle RuleApplications se non sono già presenti
                foreach (var ruleApp in ruleApplications)
                {
                    // Correzione: uso NewStateID, che è il campo corretto
                    if (!addedNodes.ContainsKey(ruleApp.NewStateID))
                    {
                        var historyData = stateHistoryMap[ruleApp.NewStateID];
                        var node = new MemoryUnit(ruleApp.NewStateID.ToString(), $"Stato {ruleApp.NewStateID}", "Stato",
                    (int)historyData.UsageCount, historyData.DetectedPatternHashes_SCSV);
                        _topology.AddNode(node);
                        addedNodes.Add(ruleApp.NewStateID, node);
                    }
                }

                _logger.Log(LogLevel.INFO, $"Caricamento dati completato. Trovati {_topology.Nodes.Count} nodi e {_topology.Links.Count} collegamenti.");

                // Genera il layout del grafo e forza il ridisegno
                GenerateCircularLayout();
                this.Invalidate();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"Errore durante il caricamento dei dati dal database: {ex.Message}");
                _logger.Log(LogLevel.ERROR, $"Stack Trace: {ex.StackTrace}");

                MessageBox.Show(
                    "Impossibile caricare i dati dal database. Controlla il log per i dettagli.",
                    "Errore di Database",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // Metodo per generare un layout circolare dei nodi
        private void GenerateCircularLayout()
        {
            _nodePositions.Clear();
            int nodeCount = _topology.Nodes.Count;
            if (nodeCount == 0) return;

            float centerX = this.ClientSize.Width / 2f;
            float centerY = this.ClientSize.Height / 2f;
            float radius = Math.Min(centerX, centerY) - 50; // Raggio del cerchio

            float angleStep = (float)(2 * Math.PI / nodeCount);

            for (int i = 0; i < nodeCount; i++)
            {
                float angle = i * angleStep;
                float x = centerX + radius * (float)Math.Cos(angle);
                float y = centerY + radius * (float)Math.Sin(angle);

                _nodePositions.Add(new NodePosition
                {
                    Node = _topology.Nodes.ElementAt(i),
                    Position = new PointF(x, y),
                });
            }
        }

        // Metodo di disegno principale, chiamato da Invalidate()
        private void MainForm_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Disegna i collegamenti
            using (var pen = new Pen(Color.Gray, 2))
            {
                foreach (var link in _topology.Links)
                {
                    var sourcePos = _nodePositions.FirstOrDefault(np => np.Node.Id == link.Source.Id);
                    var targetPos = _nodePositions.FirstOrDefault(np => np.Node.Id == link.Target.Id);

                    if (sourcePos != null && targetPos != null)
                    {
                        g.DrawLine(pen, sourcePos.Position, targetPos.Position);
                    }
                }
            }

            // Disegna i nodi e le etichette
            float nodeSize = 30f;
            using (var brush = new SolidBrush(Color.FromArgb(52, 152, 219))) // Blu chiaro
            using (var textBrush = new SolidBrush(Color.White))
            using (var labelBrush = new SolidBrush(Color.Black))
            using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
            using (var labelFont = new Font("Segoe UI", 9))
            {
                foreach (var nodePos in _nodePositions)
                {
                    // Disegna il cerchio
                    g.FillEllipse(brush, nodePos.Position.X - nodeSize / 2, nodePos.Position.Y - nodeSize / 2, nodeSize, nodeSize);

                    // Disegna l'etichetta del nodo (ID)
                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;
                    g.DrawString(nodePos.Node.Id, font, textBrush, nodePos.Position, stringFormat);

                    // Disegna il nome completo sotto il nodo
                    string name = nodePos.Node.Name;
                    SizeF textSize = g.MeasureString(name, labelFont);
                    g.DrawString(name, labelFont, labelBrush, nodePos.Position.X - textSize.Width / 2, nodePos.Position.Y + nodeSize / 2 + 5);

                    // Aggiorna i limiti del nodo per il trascinamento
                    nodePos.Bounds = new RectangleF(nodePos.Position.X - nodeSize, nodePos.Position.Y - nodeSize, nodeSize * 2, nodeSize * 2);
                }
            }
        }

        // Evento di pressione del mouse
        private void MainForm_MouseDown(object? sender, MouseEventArgs e)
        {
            _draggingNode = _nodePositions.FirstOrDefault(np => np.Bounds.Contains(e.Location));
            if (_draggingNode != null)
            {
                _draggingNode.IsDragging = true;
                _dragOffset = new PointF(e.X - _draggingNode.Position.X, e.Y - _draggingNode.Position.Y);
            }
        }

        // Evento di movimento del mouse
        private void MainForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_draggingNode != null && _draggingNode.IsDragging)
            {
                _draggingNode.Position = new PointF(e.X - _dragOffset.X, e.Y - _dragOffset.Y);
                this.Invalidate();
            }
        }

        // Evento di rilascio del mouse
        private void MainForm_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_draggingNode != null)
            {
                _draggingNode.IsDragging = false;
                _draggingNode = null;
            }
        }
    }
}
