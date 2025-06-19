/*
    https://icon-icons.com/it/categoria/Applicazione/4
    https://icons8.it/icon/set/button-cancel/ios
	https://convertico.com/
 */
using AsyncSocketServer;
using EvolutiveSystem.SQL.Core;
using EvolutiveSystem_02.Properties;
using MasterLog;
using MessaggiErrore;
using MIU.Core;
using SocketManagerInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure.Interception;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EvolutiveSystem_02
{
    public partial class FrmEvolutiveSystem : Form
    {
        #region istanza classi socket server
        private AsyncSocketListener asl;
        protected AsyncSocketThread scktThrd;
        private SocketMessageStructure response = null;
        protected Thread thSocket;
        #endregion
        #region dichiarazioni per il logger
        Logger _logger = null;
        protected Mutex SyncMtxLogger = new Mutex();
        private int swDebug = 0;
        protected string _path = "";
        #endregion
        private bool _isAdmin = false;  
        private System.Windows.Forms.ToolTip toolTip;
        #region dichiarazioni SQLite
        private SQLiteSchemaLoader _schemaLoader;
        private Database _currentDatabaseSchema;
        #endregion
        public FrmEvolutiveSystem()
        {
            InitializeComponent();
            swDebug = Convert.ToInt32(ConfigurationManager.AppSettings["DebugLev"]);
            _path = ConfigurationManager.AppSettings["FolderLOG"];
            _logger = new Logger(_path, "UIEvSystem", SyncMtxLogger);
            _logger.SwLogLevel = swDebug;
            _logger.Log(LogLevel.INFO, string.Format("Log path:{0}", _logger.GetPercorsoCompleto()));
            InitializeCustomLogic();
            StartServerSocket();
            StartThread();
            tabCtrlDBgrid.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.tabCtrlDBgrid.DrawItem += TabCtrlDBgrid_DrawItem;
        }
        #region private methods
        private void InitializeCustomLogic()
        {
            toolTip = new System.Windows.Forms.ToolTip();
            _isAdmin = IsRunningAsAdministrator();
            if (!_isAdmin)
            {
                AppendToMonitor("L'applicazione non è in esecuzione con privilegi amministrativi.");
                UpdateStatus("Regular User");
            }
            else
            {
                AppendToMonitor("L'applicazione è in esecuzione con privilegi amministrativi.");
                UpdateStatus("Administrator");
            }
            #region configurazione bottoni
            toolTip.SetToolTip(btnLoadDatabase, "Carica un database semantico 'miu_data.db'."); // *** Aggiunto ToolTip ***
            toolTip.SetToolTip(btnRicaricaDB, "Carica il database semantico predefinito."); // *** Aggiunto ToolTip ***
            toolTip.SetToolTip(btnConfigTest, "Creazione stringhe da inserire nella tabella MIU_States per l'esecuzione del test"); // *** Aggiunto ToolTip ***
            toolTip.SetToolTip(btnSocket, "Client socket e attivazione comandi"); // *** Aggiunto ToolTip ***
            #endregion


        }
        #region methods per socket server
        /// <summary>
        /// Enabling the server socket
        /// </summary>
        private void StartServerSocket()
        {
            asl = new AsyncSocketListener(ConfigurationManager.AppSettings["RemotePortList"], _logger);
            asl.Echo = false;
            asl.SwDebug = swDebug;
            asl.DataFromSocket += Asl_DataFromSocket;
            asl.ErrorFromSocket += Asl_ErrorFromSocket;
            asl.TokenSocket = 0x7FFFFFFF;
            AppendToMonitor(string.Format($"Socket server in ascolto sul {asl.SrvIpAddress[0]} : {asl.SrvPort.ToString()}"));
        }
        /// <summary>
        /// Avvio thread per server socket asincrono
        /// </summary>
        private void StartThread()
        {
            scktThrd = new AsyncSocketThread();
            scktThrd.Log = _logger;
            scktThrd.AsyncSocketListener = asl;
            scktThrd.Interval = 100;
            thSocket = new Thread(scktThrd.AsyncSocket);
        }
        /// <summary>
        /// Metodo che viene eseguito sul thread della UI per mostrare il contenuto del BufferDati
        /// in una MessageBox.
        /// </summary>
        /// <param name="message">L'oggetto SocketMessageStructure ricevuto.</param>
        private string MessageFromSocket(SocketMessageStructure message)
        {
            // *** Questo codice viene eseguito sul thread della UI! È sicuro aggiornare i controlli UI qui. ***
            string messageText = "";
            if (message != null)
            {
                string bufferContent = "BufferDati è nullo o vuoto.";
                if (message.BufferDati != null)
                {
                    // Converti l'XElement BufferDati in una stringa per mostrarlo
                    // Puoi usare ToString() per ottenere la rappresentazione XML dell'elemento e del suo contenuto.
                    bufferContent = message.BufferDati.ToString(SaveOptions.DisableFormatting); // Disabilita formattazione per stringa compatta
                    // O ToString(SaveOptions.None) per una stringa indentata e leggibile
                }

                string messageType = message.MessageType ?? "N/A"; // Usa MessageType se disponibile
                string command = message.Command ?? "N/A"; // Usa Command se disponibile
                string token = message.Token ?? "N/A"; // Usa Token se disponibile

                // Costruisci il messaggio da mostrare nella MessageBox
                messageText = $"Messaggio Ricevuto:\n" +
                                        $"- Tipo: {messageType}\n" +
                                        $"- Comando: {command}\n" +
                                        $"- Token: {token}\n" +
                                        $"- BufferDati:\n{bufferContent}";

                // Mostra la MessageBox (questa operazione deve avvenire sul thread della UI)


                // TODO: Qui potresti anche aggiungere la logica per processare il messaggio ricevuto
                // in base al suo MessageType (Info, Warning, Error) o Command,
                // aggiornando l'area di monitoraggio o altri controlli UI.
                // Esempio: AppendToMonitor($"[{messageType}] {message.BufferDati?.Value}"); // Se vuoi solo il valore testuale nel monitor
                // O una logica di dispatching più complessa...
            }
            return messageText;
        }
        #endregion
        /// <summary>
        /// Carica la struttura del database nella treeview
        /// </summary>
        /// <param name="database"></param>
        private void PopolaTreeView(Database database)
        {
            treeViewDatabase.Nodes.Clear(); // Pulisce eventuali nodi precedenti

            if (database != null)
            {
                TreeNode databaseNode = new TreeNode(database.DatabaseName);
                databaseNode.Tag = database; // Puoi taggare il nodo con l'oggetto database

                foreach (Table table in database.Tables)
                {
                    TreeNode tableNode = new TreeNode(table.TableName);
                    tableNode.Tag = table; // Taggare il nodo con l'oggetto tabella
                    databaseNode.Nodes.Add(tableNode);

                    foreach (Field field in table.Fields)
                    {
                        TreeNode fieldNode = new TreeNode($"{field.FieldName} ({field.DataType})");
                        fieldNode.Tag = field; // Taggare il nodo con l'oggetto campo
                        tableNode.Nodes.Add(fieldNode);
                    }
                }
                treeViewDatabase.Nodes.Add(databaseNode);
                databaseNode.ExpandAll(); // Espande tutti i nodi per visualizzare l'intera struttura
            }
        }
        /// <summary>
        /// Verifica se l'applicazione corrente è in esecuzione con privilegi amministrativi.
        /// </summary>
        /// <returns>True se l'applicazione è amministratore, altrimenti False.</returns>
        private bool IsRunningAsAdministrator()
        {
            // Ottiene l'identità Windows dell'utente corrente.
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            // Crea un oggetto WindowsPrincipal basato sull'identità.
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            // Controlla se l'utente è membro del gruppo Administrators.
            // Questo è il modo standard per verificare i privilegi amministrativi su Windows.
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        /// <summary>
        /// Aggiorna il testo nella StatusStrip.
        /// </summary>
        /// <param name="statusText">Il testo da visualizzare nella StatusStrip.</param>
        private void UpdateStatus(string statusText)
        {
            // Assicurati che la StatusStrip e la ToolStripStatusLabel esistano nel designer
            //if (statusStrip != null && statusStrip != null)
            if (statusStrip != null) 
            {
                // Usa InvokeRequired per aggiornare la UI da un thread diverso, se necessario
                if (statusStrip.InvokeRequired)
                {
                    statusStrip.Invoke((MethodInvoker)delegate {
                        toolStripStatusLabel.Text = statusText;
                    });
                }
                else
                {
                    toolStripStatusLabel.Text = statusText;
                }
            }
            else
            {
                // Fallback alla console se i controlli non sono disponibili
                Console.WriteLine($"STATUS: {statusText}");
            }
        }
        /// <summary>
        /// Aggiunge un messaggio all'area di monitoraggio (es. una RichTextBox chiamata 'evolutionMonitor').
        /// Gestisce l'aggiornamento da thread diversi.
        /// </summary>
        private void AppendToMonitor(string message)
        {
            // Assumendo che tu abbia un controllo RichTextBox chiamato 'evolutionMonitor'
            // Usa InvokeRequired per aggiornare la UI da un thread diverso
            if (evolutionMonitor != null) // Aggiunto controllo null
            {
                if (evolutionMonitor.InvokeRequired)
                {
                    evolutionMonitor.Invoke((MethodInvoker)delegate {
                        evolutionMonitor.AppendText(message + Environment.NewLine);
                        // Opzionale: scorri automaticamente verso il basso
                        // evolutionMonitor.ScrollToCaret();
                    });
                }
                else
                {
                    evolutionMonitor.AppendText(message + Environment.NewLine);
                    // Opzionale: scorri automaticamente verso il basso
                    // evolutionMonitor.ScrollToCaret();
                }
            }
            else
            {
                // Fallback alla console se i controlli non sono disponibili
                Console.WriteLine(message);
            }
        }
        #endregion
        #region Form events
        private void FrmEvolutiveSystem_Load(object sender, EventArgs e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                if (Settings.Default.DBFileName.Length > 0) toolStripDbDefaulLabel.Text = Settings.Default.DBFileName;
                thSocket.Start();
            }
            catch (Exception ex)
            {
                string errMsg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, errMsg);
                MessageBox.Show(errMsg, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void FrmEvolutiveSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                scktThrd.StopThread = true;
                asl.CloseServerSocket(3);
                asl.allDone.Reset();
            }
            catch (Exception ex)
            {
                string errMsg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, errMsg);
            }
        }

        #endregion
        #region buttons events
        private void btnLoadDatabase_Click(object sender, EventArgs e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (Settings.Default.DBFileName.Length > 0)
                {
                    openFileDialog.FileName = Settings.Default.DBFileName;
                }
                openFileDialog.Filter = "SQLite Database (*.db;*.sqlite)|*.db;*.sqlite|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.DBFileName = openFileDialog.FileName;
                    Settings.Default.Save();
                    string databaseFilePath = openFileDialog.FileName;
                    _schemaLoader = new SQLiteSchemaLoader(databaseFilePath, _logger);
                    _currentDatabaseSchema = _schemaLoader.LoadSchema();
                    PopolaTreeView(_currentDatabaseSchema); // Metodo per popolare la TreeView con lo schema
                    AppendToMonitor(Settings.Default.DBFileName);
                }
            }
            catch (Exception ex)
            {
                string msg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, msg);
                MessageBox.Show(msg, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendToMonitor(msg);
            }
        }
        private void btnRicaricaDB_Click(object sender, EventArgs e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                if (Settings.Default.DBFileName.Length > 0)
                {
                    if (File.Exists(Settings.Default.DBFileName))
                    {
                        string databaseFilePath = Settings.Default.DBFileName;
                        _schemaLoader = new SQLiteSchemaLoader(databaseFilePath, _logger);
                        _currentDatabaseSchema = _schemaLoader.LoadSchema();
                        PopolaTreeView(_currentDatabaseSchema); // Metodo per popolare la TreeView con lo schema
                        AppendToMonitor(Settings.Default.DBFileName);
                    }
                    else
                    {
                        string msg = $"Il file {Settings.Default.DBFileName} non esiste più";
                        _logger.Log(LogLevel.WARNING, msg);
                        AppendToMonitor(msg);
                        MessageBox.Show(msg, "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }catch (Exception ex)
            {
                string msg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, msg);
                MessageBox.Show(msg, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendToMonitor(msg);
            }
        }
        private void btnConfigTest_Click(object sender, EventArgs e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                if (_schemaLoader != null)
                {
                    string select = "SELECT ID, NomeParametro, ValoreParametro, Descrizione FROM MIUParameterConfigurator ";
                    List<string> field = _schemaLoader.SQLiteSelect(select);
                    FrmGeneraStringheTest frmGeneraStringheTest = new FrmGeneraStringheTest(field, _schemaLoader.ConnectionString);
                    frmGeneraStringheTest.ShowDialog();
                }
                else
                {
                    string msg = "Il database non è ancora connesso";
                    _logger.Log(LogLevel.WARNING, msg);
                    AppendToMonitor(msg);
                    MessageBox.Show(msg, "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                string msg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, msg);
                MessageBox.Show(msg, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendToMonitor(msg);
            }
        }
        private void btnSocket_Click(object sender, EventArgs e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                if (Settings.Default.DBFileName.Length > 0)
                {
                    FrmSocketClient fSocket = new FrmSocketClient(_logger, asl, Settings.Default.DBFileName);
                    fSocket.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Per configurare la comunicazione co 'Semantic Servic' occorre una connesione a un database!", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }catch (Exception ex)
            {
                string msg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, msg);
                MessageBox.Show(msg, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendToMonitor(msg);
            }
        }

        #endregion
        #region  treeview events
        private void treeViewDatabase_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Table selectedTable)
            {
                string tableName = selectedTable.TableName;
                TabPage existingTabPage = null;

                foreach (TabPage tabPage in tabCtrlDBgrid.TabPages)
                {
                    if (tabPage.Text == tableName)
                    {
                        existingTabPage = tabPage;
                        break;
                    }
                }

                if (existingTabPage == null)
                {
                    TabPage tabPage = new TabPage(tableName);
                    tabCtrlDBgrid.TabPages.Add(tabPage);

                    List<Dictionary<string, object>> tableData = _schemaLoader.LoadTableData(tableName);

                    DataGridView dataGridView = new DataGridView();
                    dataGridView.Dock = DockStyle.Fill;
                    tabPage.Controls.Add(dataGridView);

                    if (tableData != null && tableData.Count > 0)
                    {
                        if (tableData.Count > 0)
                        {
                            foreach (var key in tableData[0].Keys)
                            {
                                dataGridView.Columns.Add(key, key);
                            }

                            foreach (var rowData in tableData)
                            {
                                dataGridView.Rows.Add(rowData.Values.ToArray());
                            }
                        }
                    }
                    else
                    {
                        Label lblNoData = new Label();
                        lblNoData.Text = "Nessun dato da visualizzare.";
                        lblNoData.Dock = DockStyle.Fill;
                        tabPage.Controls.Add(lblNoData);
                    }

                    tabCtrlDBgrid.SelectedTab = tabPage;
                }
                else
                {
                    tabCtrlDBgrid.SelectedTab = existingTabPage;
                }
            }
            else if (e.Node.Tag is Field selectedField)
            {
                // Popola la ListView con le proprietà del campo
                listViewFields.Items.Clear();
                listViewFields.Columns.Clear();
                listViewFields.Columns.Add("Proprietà", 100);
                listViewFields.Columns.Add("Valore", 150);
                listViewFields.Items.Add(new ListViewItem(new string[] { "Nome", selectedField.FieldName }));
                listViewFields.Items.Add(new ListViewItem(new string[] { "Tipo Dati", selectedField.DataType }));
                listViewFields.Items.Add(new ListViewItem(new string[] { "Chiave Primaria", selectedField.IsPrimaryKey.ToString() }));
                listViewFields.Items.Add(new ListViewItem(new string[] { "Autoincremento", selectedField.AutoIncrement.ToString() }));
                listViewFields.Items.Add(new ListViewItem(new string[] { "Tabella", selectedField.TableName }));

                // Seleziona la TabPage della tabella padre
                if (selectedField.ParentTable != null)
                {
                    string parentTableName = selectedField.ParentTable.TableName;
                    TabPage tabPageToSelect = null;
                    foreach (TabPage tabPage in tabCtrlDBgrid.TabPages)
                    {
                        if (tabPage.Text == parentTableName)
                        {
                            tabPageToSelect = tabPage;
                            break;
                        }
                    }

                    // Se la TabPage della tabella esiste, la selezioniamo
                    if (tabPageToSelect != null)
                    {
                        tabCtrlDBgrid.SelectedTab = tabPageToSelect;
                    }
                    else
                    {
                        // Se la TabPage non esiste ancora, potremmo volerla creare.
                        // Possiamo riutilizzare la logica dal blocco 'if (e.Node.Tag is Table ...)'
                        // per creare e selezionare la TabPage.
                        string tableName = parentTableName;
                        TabPage newTabPage = new TabPage(tableName);
                        tabCtrlDBgrid.TabPages.Add(newTabPage);

                        List<Dictionary<string, object>> tableData = _schemaLoader.LoadTableData(tableName);
                        DataGridView dataGridView = new DataGridView();
                        dataGridView.Dock = DockStyle.Fill;
                        newTabPage.Controls.Add(dataGridView);

                        if (tableData != null && tableData.Count > 0)
                        {
                            foreach (var key in tableData[0].Keys)
                            {
                                dataGridView.Columns.Add(key, key);
                            }
                            foreach (var rowData in tableData)
                            {
                                dataGridView.Rows.Add(rowData.Values.ToArray());
                            }
                        }
                        else
                        {
                            Label lblNoData = new Label();
                            lblNoData.Text = "Nessun dato da visualizzare.";
                            lblNoData.Dock = DockStyle.Fill;
                            newTabPage.Controls.Add(lblNoData);
                        }
                        tabCtrlDBgrid.SelectedTab = newTabPage;
                    }
                }
            }
            /*
            if (e.Node.Tag is Table selectedTable)
            {
                string tableName = selectedTable.TableName;
                TabPage existingTabPage = null;

                foreach (TabPage tabPage in tabCtrlDBgrid.TabPages)
                {
                    if (tabPage.Text == tableName)
                    {
                        existingTabPage = tabPage;
                        break;
                    }
                }

                if (existingTabPage == null)
                {
                    TabPage tabPage = new TabPage(tableName)
                    {
                        
                        ForeColor = Color.Blue,
                        Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold),
                    };
                    tabCtrlDBgrid.TabPages.Add(tabPage);

                    List<Dictionary<string, object>> tableData = _schemaLoader.LoadTableData(tableName);

                    DataGridView dataGridView = new DataGridView();
                    dataGridView.Dock = DockStyle.Fill;
                    tabPage.Controls.Add(dataGridView);

                    if (tableData != null && tableData.Count > 0)
                    {
                        if (tableData.Count > 0)
                        {
                            foreach (var key in tableData[0].Keys)
                            {
                                dataGridView.Columns.Add(key, key);
                            }

                            foreach (var rowData in tableData)
                            {
                                dataGridView.Rows.Add(rowData.Values.ToArray());
                            }
                        }
                    }
                    else
                    {
                        Label lblNoData = new Label();
                        lblNoData.Text = "Nessun dato da visualizzare.";
                        lblNoData.Dock = DockStyle.Fill;
                        tabPage.Controls.Add(lblNoData);
                    }

                    tabCtrlDBgrid.SelectedTab = tabPage;
                }
                else
                {
                    tabCtrlDBgrid.SelectedTab = existingTabPage;
                }

            }
            else if (e.Node.Tag is Field selectedField)
            {
                // Pulisci la ListView prima di mostrare le nuove proprietà
                listViewFields.Items.Clear();
                listViewFields.Columns.Clear();

                // Aggiungi le colonne alla ListView (una per la proprietà, una per il valore)
                listViewFields.Columns.Add("Proprietà", 100);
                listViewFields.Columns.Add("Valore", 150);

                // Popola la ListView con le proprietà del campo selezionato
                listViewFields.Items.Add(new ListViewItem(new string[] { "Nome", selectedField.FieldName }));
                listViewFields.Items.Add(new ListViewItem(new string[] { "Tipo Dati", selectedField.DataType }));
                listViewFields.Items.Add(new ListViewItem(new string[] { "Chiave Primaria", selectedField.IsPrimaryKey.ToString() }));
                listViewFields.Items.Add(new ListViewItem(new string[] { "Autoincremento", selectedField.AutoIncrement.ToString() }));
                listViewFields.Items.Add(new ListViewItem(new string[] { "Tabella", selectedField.TableName }));
                // Potresti aggiungere altre proprietà se lo desideri

                // Assicurati che la ListView sia visibile e abbia dati
                if (listViewFields.Items.Count > 0)
                {
                    // Potresti voler adattare le colonne alla larghezza del contenuto
                    // listViewFields.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
            */
        }
        private void TabCtrlDBgrid_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle r = e.Bounds;
            TabPage tp = this.tabCtrlDBgrid.TabPages[e.Index];
            Color backgroundColor = Color.LightSteelBlue;
            Font tabFont = new Font("Segoe UI", 10, FontStyle.Bold); // Font più piccolo per stare in altezze standard
            Color textColor = Color.Green;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center; // Allinea verticalmente il testo

            using (Brush backBrush = new SolidBrush(backgroundColor))
            {
                g.FillRectangle(backBrush, r);
            }

            using (Pen borderPen = new Pen(Color.Gray))
            {
                g.DrawRectangle(borderPen, r);
            }

            using (Brush textBrush = new SolidBrush(textColor))
            {
                g.DrawString(tp.Text, tabFont, textBrush, r, sf);
            }
        }
        #endregion
        #region Async socket serve events
        private void Asl_ErrorFromSocket(object sender, string e)
        {
            evolutionMonitor.AppendText(e);
            _logger.Log(LogLevel.ERROR, e);
        }
        private void Asl_DataFromSocket(object sender, SocketMessageStructure e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            Socket Handler = sender as Socket;
            try
            {
                string txtSendData = "";
                response = new SocketMessageStructure
                {
                    Command = null,
                    CRC = 0,
                    MessageType = e.MessageType,
                    SendingTime = DateTime.Now,
                    Token = e.Token,
                    BufferDati = new XElement("BufferDati",
                    new XElement("DataRx", e.SendingTime),
                    new XElement("Status", "OK"))
                };
                string telegramGenerate = SocketMessageSerializer.SerializeUTF8(response);
                _logger.Log(LogLevel.DEBUG, telegramGenerate);
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                txtSendData = SocketMessageSerializer.Base64Start + Convert.ToBase64String(bytes, 0, bytes.Length) + SocketMessageSerializer.Base64End;
                asl.Send(Handler, txtSendData);
                _logger.Log(MasterLog.LogLevel.DEBUG, string.Format("{0} funzione: {1}", Handler.Connected, thisMethod.Name));

                if (this.InvokeRequired)
                {

                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        // Questo codice all'interno del delegate viene eseguito sul thread della UI.
                        evolutionMonitor.AppendText(MessageFromSocket(e) + Environment.NewLine);
                    });
                }
                else
                {
                    evolutionMonitor.AppendText(MessageFromSocket(e) + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                string errMsg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, errMsg);
            }
        }
        #endregion
    }
}
