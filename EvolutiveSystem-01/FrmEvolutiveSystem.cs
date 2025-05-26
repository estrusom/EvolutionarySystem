/*
/*
    https://icon-icons.com/it/categoria/Applicazione/4
    https://icons8.it/icon/set/button-cancel/ios
	https://convertico.com/
 */
/* changelog
 2025.05.14 nel form FrmTelegram è stata aggiunto una combobox da cui scegliere il comando da generare
 2025.05.22 Aggiunto: Riferimento alla tabella attualmente selezionata nella TreeView
 2025.05.24 aggiunti tipi floatin point
 2025.05.25 aggiunta gestione per autoincremento indici
*/
using AsyncSocketServer;
using EvolutiveSystem;
using EvolutiveSystem.Core;
using EvolutiveSystem_01.Properties;
using MasterLog;
using MessaggiErrore;
using MIU.Core;
using SocketManagerInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ToolTip = System.Windows.Forms.ToolTip;

namespace EvolutiveSystem_01
{
    public partial class FrmEvolutiveSystem : Form
    {
        #region istanza classi socket server
        private AsyncSocketListener asl;
        protected AsyncSocketThread scktThrd;
        private SocketMessageStructure response = null;
        protected Thread thSocket;
        #endregion
        private Logger _logger;
        protected Mutex SyncMtxLogger = new Mutex();
        private int swDebug = 0;
        protected string _path = "";
        private string _serviceName = "SemanticProcessor";
        private bool _isAdmin = false;
        // La collezione di database (semantiche) gestiti dall'UI.
        private List<Database> loadedDatabases = new List<Database>();
        // Riferimento al database attualmente selezionato o attivo nell'UI
        private Database currentDatabase;
        private ToolTip toolTip;
        // 2025.05.22 Aggiunto: Riferimento alla tabella attualmente selezionata nella TreeView
        private Table _currentActiveTableInUI;
        // 2025.05.22 Aggiunto: Riferimento al DataGridView dei record attualmente visualizzato
        private DataGridView _currentActiveDataGridView;
        private DataGridView dgvDataRecords;

        public FrmEvolutiveSystem()
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                InitializeComponent();
                swDebug = Convert.ToInt32(ConfigurationManager.AppSettings["DebugLev"]);
                _path = ConfigurationManager.AppSettings["FolderLOG"];
                _logger = new Logger(_path, "UIEvSystem", SyncMtxLogger);
                _logger.SwLogLevel = swDebug;
                _logger.Log(LogLevel.INFO, string.Format("Log path:{0}", _logger.GetPercorsoCompleto()));
                this.InitializeCustomLogic();
                this.StartServerSocket();
                this.StartThread();
            }
            catch (Exception ex)
            {
                string errMsg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, errMsg);
                MessageBox.Show(errMsg, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #region private methods
        /// <summary>
        /// Inizializza logica o stato personalizzato all'avvio del Form.
        /// </summary>
        private void InitializeCustomLogic()
        {
            toolTip = new ToolTip();
            loadedDatabases = new List<Database>();
            // Inizializza il motore semantico (se gestito direttamente nella UI)
            // semanticEngine = new SemanticEngine();
            // semanticEngine.OnProcessUpdate += SemanticEngine_OnProcessUpdate; // Sottoscrivi agli eventi di aggiornamento
            // semanticEngine.OnPhaseChange += SemanticEngine_OnPhaseChange; // Sottoscrivi agli eventi di cambio fase

            // Imposta lo stato iniziale dei controlli di supervisione e dettagli
            // btnServiceStop.Enabled = false;
            // btnServicePause.Enabled = false;
            // lblCurrentPhase.Text = "Stato: Inattivo"; // Se hai una label per la fase
            // evolutionMonitor.Clear();
            // lblSelectedElementType.Text = ""; // Se hai etichette per i dettagli
            // lblElementName.Text = ""; // Se hai etichette per i dettagli

            // Pulisci e configura il TabControl all'avvio
            if (tabControlDetails != null)
            {
                tabControlDetails.TabPages.Clear();
            }

            // Configura le colonne della ListView per i campi (RIPRISTINATO)
            ConfigureFieldsListView();
            _isAdmin = IsRunningAsAdministrator();
            if (!_isAdmin)
            {
                AppendToMonitor("L'applicazione non è in esecuzione con privilegi amministrativi.");
                UpdateStatus("Eseguire come amministratore per controllare il servizio.");
            }
            else
            {
                AppendToMonitor("L'applicazione è in esecuzione con privilegi amministrativi.");
                UpdateStatus("Controllo servizio abilitato.");
            }

            UpdateStatus("Pronto."); // Imposta lo stato iniziale nella StatusStrip

            // Associa gli eventi dei pulsanti globali nel pannello comandi (assicurati che i controlli Button esistano nel designer e siano rinominati)
            if (btnAddDatabase != null)
            {
                btnAddDatabase.Click += btnAddDatabase_Click;
                toolTip.SetToolTip(btnAddDatabase, "Aggiunge un nuovo database semantico vuoto."); // *** Aggiunto ToolTip ***
            }

            if (btnLoadDatabase != null)
            {
                btnLoadDatabase.Click += btnLoadDatabase_Click;
                toolTip.SetToolTip(btnLoadDatabase, "Carica uno o più database semantici da file XML.");
            }
            if (btnSaveDatabase != null)
            {
                btnSaveDatabase.Click += btnSaveDatabase_Click;
                toolTip.SetToolTip(btnSaveDatabase, "Salva tutti i database semantici caricati in file XML.");
            }
            // *** Aggiunto: Associa l'evento per il pulsante "Chiudi Tutti i Database" ***
            if (btnCloseAllDatabases != null)
            {
                btnCloseAllDatabases.Click += BtnCloseAllDatabases_Click;
                toolTip.SetToolTip(btnCloseAllDatabases, "Chiude tutti i database semantici caricati."); // Aggiunto ToolTip
            }
            if (btnRicaricaDB != null)
            {
                btnRicaricaDB.Click += BtnRicaricaDB_Click;
                toolTip.SetToolTip(btnRicaricaDB, "Riapre l'ultimo database chiuso"); // Aggiunto ToolTip
            }
            if (btnAnalysis != null)
            {
                btnAnalysis.Click += BtnAnalysis_Click;
                toolTip.SetToolTip(btnAnalysis, "Analisi dati database"); // Aggiunto ToolTip
            }
            if(btnServiceStart != null)
            {
                btnServiceStart.Click += BtnServiceStart_Click;
                toolTip.SetToolTip(btnServiceStart, "Avvia il servizio del server semantico."); // Aggiunto ToolTip
            }
            if(btnServicePause != null)
            {
                btnServicePause.Click += BtnServicePause_Click;
                toolTip.SetToolTip(btnServicePause, "Sospensione del servizio server semantico."); // Aggiunto ToolTip
            }
            if(btnServiceStop != null)
            {
                btnServiceStop.Click += BtnServiceStop_Click;
                toolTip.SetToolTip(btnServiceStop, "Arresto del servizio server semantico."); // Aggiunto ToolTip
            }
            if (btnSocket != null)
            {
                btnSocket.Click += BtnSocket_Click;
                toolTip.SetToolTip(btnSocket, "Avvio client socket."); // Aggiunto ToolTip
            }
            //if (btnStartProcess != null) btnStartProcess.Click += btnStartProcess_Click;
            //if (btnServiceStop != null) btnServiceStop.Click += btnStopProcess_Click;
            //if (btnServicePause != null) btnServicePause.Click += btnPauseProcess_Click;


            // Associa gli eventi della TreeView (assicurati che il controllo TreeView esista nel designer)
            /*
             * generati da generatore di eventi delle proprietà
            if (dbTreeView != null)
            {
                dbTreeView.AfterSelect += dbTreeView_AfterSelect; // Gestisce la selezione con click sinistro
                dbTreeView.NodeMouseClick += dbTreeView_NodeMouseClick; // Gestisce il click del mouse su un nodo (incluso il destro)
            }
            */

            // Configura il ContextMenu (assicurati che il controllo ContextMenuStrip esista nel designer)
            ConfigureTreeViewContextMenu();
        }
        /// <summary>
        /// Configura le voci del ContextMenu.
        /// </summary>
        private void ConfigureTreeViewContextMenu()
        {
            if (treeViewContextMenu != null)
            {
                // Pulisci voci esistenti (se configurato nel designer, potresti non volerlo fare)
                treeViewContextMenu.Items.Clear();

                // Aggiungi le voci del menu. Useremo i Tag delle voci per identificarle.
                ToolStripItem addTableMenuItem = treeViewContextMenu.Items.Add("Aggiungi Tabella");
                addTableMenuItem.Tag = "AddTable";
                addTableMenuItem.Click += ContextMenuItem_Click;

                ToolStripItem addFieldMenuItem = treeViewContextMenu.Items.Add("Aggiungi Campo");
                addFieldMenuItem.Tag = "AddField";
                addFieldMenuItem.Click += ContextMenuItem_Click;

                //treeViewContextMenu.Items.Add(new ToolStripSeparator()); // Separatore

                ToolStripItem deleteItem = treeViewContextMenu.Items.Add("Elimina");
                deleteItem.Tag = "Delete";
                deleteItem.Click += ContextMenuItem_Click;

                ToolStripItem modifyItem = treeViewContextMenu.Items.Add("Modifica");
                modifyItem.Tag = "Modify";
                modifyItem.Click += ContextMenuItem_Click;

                // Puoi aggiungere altre voci qui se necessario
            }
        }
        /// <summary>
        /// Logica per aggiungere una nuova tabella al database selezionato.
        /// </summary>
        /// <param name="targetDatabase">Il Database a cui aggiungere la tabella.</param>
        private void AddTableToDatabase(Database targetDatabase)
        {
            if (targetDatabase == null) return;

            // *** Qui dovresti ottenere il nome della nuova tabella dall'utente (es. tramite un Input Dialog) ***
            string newTableName = "NuovaTabella_" + (targetDatabase.Tables.Count + 1); // Placeholder con nome univoco semplice

            // Genera un ID semplice per la nuova tabella
            int newTableId = targetDatabase.Tables.Count > 0 ? targetDatabase.Tables.Max(t => t.TableId) + 1 : 1;
            FrmAddTable ftblName = new FrmAddTable(targetDatabase.DatabaseName, newTableName);
            if (ftblName.ShowDialog() == DialogResult.OK)
            {
                newTableName = ftblName.TableName;
                // Crea la nuova istanza della classe Table
                Table newTable = new Table(newTableId, newTableName, targetDatabase);

                // Aggiungi la nuova tabella alla lista del database
                targetDatabase.AddTable(newTable); // Usa il metodo AddTable per impostare il ParentDatabase

                // Aggiungi un nuovo nodo alla TreeView per la nuova tabella
                TreeNode dbNode = dbTreeView.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Tag == targetDatabase);
                if (dbNode != null)
                {
                    TreeNode newTableNode = new TreeNode(newTable.TableName);
                    newTableNode.Tag = newTable; // Associa l'oggetto Table al nodo
                    dbNode.Nodes.Add(newTableNode); // Aggiungi il nodo alla TreeView sotto il database selezionato

                    // Espandi il nodo del database per mostrare la nuova tabella
                    dbNode.Expand();

                    AppendToMonitor($"Aggiunta nuova tabella '{newTableName}' al database '{targetDatabase.DatabaseName}'.");
                    UpdateStatus($"Tabella '{newTableName}' aggiunta.");
                }
            }
        }
        /// <summary>
        /// Logica per aggiungere un nuovo campo alla tabella selezionata.
        /// </summary>
        /// <param name="targetTable">La Tabella a cui aggiungere il campo.</param>
        private void AddFieldToTable(Table targetTable)
        {
            if (targetTable == null) return;

            // *** Qui dovresti ottenere i dettagli del nuovo campo dall'utente (es. tramite un Input Dialog o un Form dedicato) ***
            string newFieldName = "NuovoCampo_" + (targetTable.Fields.Count + 1); // Placeholder con nome univoco semplice
            string newDataType = "string"; // Placeholder
            bool isKey = false; // Placeholder
            bool isEncrypted = false; // Placeholder
            bool autoIncrement = false;
            ulong registryValue = 0; // Placeholder
            object fieldValue = null; // Placeholder

            // Genera un ID semplice per il nuovo campo
            int newFieldId = targetTable.Fields.Count > 0 ? targetTable.Fields.Max(f => f.Id) + 1 : 1;

            // Crea la nuova istanza della classe Field
            Field newField = new Field(newFieldId, autoIncrement, newFieldName, newDataType, isKey, isEncrypted, registryValue, targetTable, fieldValue);
            FrmAddField frmAddField = new FrmAddField(newField);
            if (frmAddField.ShowDialog() == DialogResult.OK)
            {
                newField.DataType = frmAddField.Field.DataType;
                newField.Id = newFieldId;
                newField.ElementType = frmAddField.Field.ElementType;
                newField.EncryptedField = frmAddField.Field.EncryptedField;
                newField.FieldName = frmAddField.Field.FieldName;
                newField.Key = frmAddField.Field.Key;
                newField.Value = frmAddField.Field.Value;
                newField.Registry = frmAddField.Field.Registry;
                newField.Value = frmAddField.Field.Value;
                newField.PrimaryKeyAutoIncrement = frmAddField.Field.PrimaryKeyAutoIncrement;
            }
            // Aggiungi il nuovo campo alla lista della tabella
            targetTable.AddField(newField); // Usa il metodo AddField per impostare il ParentTable

            // Aggiungi un nuovo nodo alla TreeView per il nuovo campo
            TreeNode tableNode = dbTreeView.Nodes.Cast<TreeNode>().SelectMany(dbNode => dbNode.Nodes.Cast<TreeNode>()).FirstOrDefault(tNode => tNode.Tag == targetTable);
            if (tableNode != null)
            {
                TreeNode newFieldNode = new TreeNode($"{newField.FieldName} ({newField.DataType})");
                newFieldNode.Tag = newField; // Associa l'oggetto Field al nodo
                tableNode.Nodes.Add(newFieldNode); // Aggiungi il nodo campo sotto la tabella selezionata

                // Espandi il nodo della tabella per mostrare il nuovo campo
                tableNode.Expand();

                // Aggiorna la ListView per mostrare il nuovo campo aggiunto (proprietà)
                PopulateFieldsListView(targetTable.Fields);

                // Aggiorna il DataGridView dei dati record se la tab della tabella è attualmente visibile
                UpdateDataRecordsDataGridView(targetTable);

                AppendToMonitor($"Aggiunto nuovo campo '{newFieldName}' alla tabella '{targetTable.TableName}'.");
                UpdateStatus($"Campo '{newFieldName}' aggiunto.");
            }
        }
        /// <summary>
        /// Logica per eliminare l'elemento selezionato (Database, Tabella o Campo).
        /// </summary>
        /// <param name="selectedObject">L'oggetto dati da eliminare.</param>
        /// <param name="selectedNode">Il nodo TreeView corrispondente all'oggetto da eliminare.</param>
        private void DeleteSelectedItem(object selectedObject, TreeNode selectedNode)
        {
            if (selectedObject == null || selectedNode == null) return;

            // *** Qui dovresti chiedere conferma all'utente prima di eliminare! ***
            // Esempio:
            // DialogResult confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare '{selectedNode.Text}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            // if (confirmResult == DialogResult.No) return;


            if (selectedObject is Database dbToDelete)
            {
                // Logica per eliminare un Database
                if (loadedDatabases.Contains(dbToDelete))
                {
                    loadedDatabases.Remove(dbToDelete);
                    currentDatabase = loadedDatabases.FirstOrDefault(); // Imposta un nuovo database corrente o null
                    dbTreeView.Nodes.Remove(selectedNode); // Rimuovi il nodo dalla TreeView
                    AppendToMonitor($"Database '{dbToDelete.DatabaseName}' eliminato.");
                    UpdateStatus($"Database '{dbToDelete.DatabaseName}' eliminato.");
                    // Pulisci le tab dei dettagli/dati
                    if (tabControlDetails != null) tabControlDetails.TabPages.Clear();
                }
            }
            else if (selectedObject is Table tableToDelete)
            {
                // Logica per eliminare una Tabella
                if (tableToDelete.ParentDatabase != null && tableToDelete.ParentDatabase.Tables.Contains(tableToDelete))
                {
                    tableToDelete.ParentDatabase.Tables.Remove(tableToDelete);
                    selectedNode.Parent.Nodes.Remove(selectedNode); // Rimuovi il nodo dalla TreeView
                    AppendToMonitor($"Tabella '{tableToDelete.TableName}' eliminata dal database '{tableToDelete.ParentDatabase.DatabaseName}'.");
                    UpdateStatus($"Tabella '{tableToDelete.TableName}' eliminata.");
                    // Rimuovi la TabPage corrispondente ai dati record se esiste
                    RemoveTableDataRecordsTab(tableToDelete);
                    // Pulisci la ListView delle proprietà se mostrava i campi di questa tabella
                    if (listViewFields != null && listViewFields.Items.Cast<ListViewItem>().Any() && listViewFields.Items[0].Tag is Field firstFieldInList && firstFieldInList.ParentTable == tableToDelete)
                    {
                        listViewFields.Items.Clear();
                    }
                }
            }
            else if (selectedObject is Field fieldToDelete)
            {
                // Logica per eliminare un Campo
                if (fieldToDelete.ParentTable != null && fieldToDelete.ParentTable.Fields.Contains(fieldToDelete))
                {
                    Table parentTable = fieldToDelete.ParentTable;
                    fieldToDelete.ParentTable.Fields.Remove(fieldToDelete);
                    selectedNode.Parent.Nodes.Remove(selectedNode); // Rimuovi il nodo dalla TreeView
                    AppendToMonitor($"Campo '{fieldToDelete.FieldName}' eliminato dalla tabella '{fieldToDelete.ParentTable.TableName}'.");
                    UpdateStatus($"Campo '{fieldToDelete.FieldName}' eliminato.");
                    // Aggiorna la ListView delle proprietà per riflettere l'eliminazione
                    if (parentTable != null)
                    {
                        PopulateFieldsListView(parentTable.Fields);
                    }
                    // Aggiorna il DataGridView dei dati record se la tab della tabella madre è visibile
                    UpdateDataRecordsDataGridView(parentTable);
                }
            }
        }
        /// <summary>
        /// Logica per m,odificare un campo
        /// </summary>
        private void ModifySelectedItem(Field targeField, Table targetTable)
        {
            //Field newField = new Field(newFieldId, autoIncrement, newFieldName, newDataType, isKey, isEncrypted, registryValue, targetTable, fieldValue);

            FrmAddField frmAddField = new FrmAddField(targeField);
            if (frmAddField.ShowDialog() == DialogResult.OK) 
            {
                targeField.DataType = frmAddField.Field.DataType;
                targeField.Id = frmAddField.Field.Id;
                targeField.ElementType = frmAddField.Field.ElementType;
                targeField.EncryptedField = frmAddField.Field.EncryptedField;
                targeField.FieldName = frmAddField.Field.FieldName;
                targeField.Key = frmAddField.Field.Key;
                targeField.Value = frmAddField.Field.Value;
                targeField.Registry = frmAddField.Field.Registry;
                targeField.Value = frmAddField.Field.Value;
                targeField.PrimaryKeyAutoIncrement = frmAddField.Field.PrimaryKeyAutoIncrement;
            }
            TreeNode tableNode = dbTreeView.Nodes.Cast<TreeNode>().SelectMany(dbNode => dbNode.Nodes.Cast<TreeNode>()).FirstOrDefault(tNode => tNode.Tag == targetTable);
            if (tableNode != null)
            {
                TreeNode newFieldNode = new TreeNode($"{targeField.FieldName} ({targeField.DataType})");
                newFieldNode.Tag = targeField; // Associa l'oggetto Field al nodo
                //tableNode.Nodes.Add(newFieldNode); // Aggiungi il nodo campo sotto la tabella selezionata

                // Espandi il nodo della tabella per mostrare il nuovo campo
                tableNode.Expand();

                // Aggiorna la ListView per mostrare il nuovo campo aggiunto (proprietà)
                PopulateFieldsListView(targetTable.Fields);

                // Aggiorna il DataGridView dei dati record se la tab della tabella è attualmente visibile
                UpdateDataRecordsDataGridView(targetTable);

                AppendToMonitor($"Aggiunto nuovo campo '{targeField}' alla tabella '{targetTable.TableName}'.");
                UpdateStatus($"Campo '{targeField}' modificato.");
            }
        }
        /// <summary>
        /// Configura le colonne della ListView per visualizzare le proprietà della classe Field.
        /// RIPRISTINATO: Questa ListView mostra le *proprietà* dei campi, non i dati dei record.
        /// </summary>
        private void ConfigureFieldsListView()
        {
            // Assicurati che il controllo ListView esista nel designer e sia rinominato 'listViewFields'
            if (listViewFields != null)
            {
                listViewFields.View = View.Details; // Imposta la visualizzazione a dettagli (griglia)
                listViewFields.Columns.Clear(); // Pulisci eventuali colonne esistenti

                // Aggiungi le colonne per le proprietà di Field
                listViewFields.Columns.Add("ID", 50, HorizontalAlignment.Left);
                listViewFields.Columns.Add("Nome Campo", 150, HorizontalAlignment.Left);
                listViewFields.Columns.Add("Tipo Dato", 100, HorizontalAlignment.Left);
                listViewFields.Columns.Add("Chiave", 60, HorizontalAlignment.Center);
                listViewFields.Columns.Add("Criptato", 70, HorizontalAlignment.Center);
                listViewFields.Columns.Add("Registry", 100, HorizontalAlignment.Left);
                listViewFields.Columns.Add("Tipo Elemento", 100, HorizontalAlignment.Left);
                listViewFields.Columns.Add("(Incremento automatico)", 100, HorizontalAlignment.Left);
                // Il valore del campo non viene mostrato in questa ListView, ma nei dati del DataGridView

                // Opzionale: Permetti all'utente di riordinare le colonne
                // listViewFields.ColumnClick += ListViewFields_ColumnClick;
            }
        }
        /// <summary>
        /// Aggiorna il testo nella StatusStrip.
        /// </summary>
        /// <param name="statusText">Il testo da visualizzare nella StatusStrip.</param>
        private void UpdateStatus(string statusText)
        {
            // Assicurati che la StatusStrip e la ToolStripStatusLabel esistano nel designer
            if (toolStripStatusLabel != null && statusStrip != null)
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
        /// <summary>
        /// Popola la ListView con le *proprietà* dei campi forniti.
        /// RIPRISTINATO: Questa ListView mostra le *proprietà* dei campi, non i dati dei record.
        /// </summary>
        /// <param name="fields">La lista di Field le cui proprietà devono essere visualizzate.</param>
        private void PopulateFieldsListView(List<Field> fields)
        {
             if (listViewFields != null)
             {
                 listViewFields.Items.Clear(); // Pulisci gli elementi esistenti

                 if (fields != null)
                 {
                     foreach (var field in fields)
                     {
                         // Crea un nuovo ListViewItem per ogni campo, mostrando le sue proprietà
                         ListViewItem item = new ListViewItem(field.Id.ToString()); // Primo elemento (colonna ID)
                         item.SubItems.Add(field.FieldName);
                         item.SubItems.Add(field.DataType);
                         item.SubItems.Add(field.Key.ToString());
                         item.SubItems.Add(field.EncryptedField.ToString());
                         item.SubItems.Add(field.Registry.ToString());
                         item.SubItems.Add(field.ElementType.ToString()); // Mostra il tipo logico
                         item.SubItems.Add(field.PrimaryKeyAutoIncrement.ToString());
                         item.Tag = field; // Associa l'oggetto Field all'elemento della ListView
                         listViewFields.Items.Add(item); // Aggiungi l'elemento alla ListView
                     }
                 }
             }
        }
        /// <summary>
        /// Aggiorna il DataGridView dei dati per una specifica Tabella se la sua TabPage è attualmente visibile.
        /// Utile dopo aver aggiunto o eliminato un record (non un campo) tramite il DataGridView stesso.
        /// </summary>
        /// <param name="table">La Tabella il cui DataGridView dei dati deve essere aggiornato.</param>
        private void UpdateDataRecordsDataGridView(Table table)
        {
            if (tabControlDetails == null || table == null) return;

            // Trova la TabPage dei dati associata a questa tabella
            TabPage dataRecordsTabPage = null;
            foreach (TabPage page in tabControlDetails.TabPages)
            {
                if (page.Tag is Tuple<Table, string> pageTag && pageTag.Item1 == table && pageTag.Item2 == "DataRecords")
                {
                    dataRecordsTabPage = page;
                    break;
                }
            }

            // Se la TabPage esiste e contiene un DataGridView, aggiornalo
            if (dataRecordsTabPage != null && dataRecordsTabPage.Controls.Count > 0 && dataRecordsTabPage.Controls[0] is DataGridView dgv)
            {
                // Aggiorna il DataSource del DataGridView
                // Richiede che la classe Table abbia una proprietà per i dati dei record
                // PopulateDataRecordsDataGridView(dgv, table.DataRecords); // Assumendo una proprietà DataRecords

                // Placeholder: Aggiorna solo la visualizzazione (potrebbe non bastare se il DataSource non è BindingList)
                dgv.Refresh();

                AppendToMonitor($"DataGridView dati record per tabella '{table.TableName}' aggiornato.");
            }
        }
        /// <summary>
        /// Rimuove la TabPage dei dati record associata a una specifica Tabella, se esiste.
        /// </summary>
        /// <param name="table">La Tabella la cui TabPage dei dati record deve essere rimossa.</param>
        private void RemoveTableDataRecordsTab(Table table)
        {
            if (tabControlDetails == null || table == null) return;

            // Trova la TabPage associata a questa tabella e di tipo "DataRecords"
            TabPage pageToRemove = null;
            foreach (TabPage page in tabControlDetails.TabPages)
            {
                if (page.Tag is Tuple<Table, string> pageTag && pageTag.Item1 == table && pageTag.Item2 == "DataRecords")
                {
                    pageToRemove = page;
                    break;
                }
            }

            // Rimuovi la TabPage se trovata
            if (pageToRemove != null)
            {
                tabControlDetails.TabPages.Remove(pageToRemove);
                AppendToMonitor($"TabPage dati record per tabella '{table.TableName}' rimossa.");
            }
        }
        /// <summary>
        /// Popola la TreeView con la struttura del database corrente (currentDatabase).
        /// (Assumendo che tu abbia un controllo TreeView chiamato 'dbTreeView')
        /// </summary>
        private void PopulateDatabaseTreeView()
        {
            if (dbTreeView != null)
            {
                dbTreeView.Nodes.Clear(); // Pulisce i nodi esistenti prima di ripopolare

                // Itera su TUTTI i database nella lista loadedDatabases
                foreach (var database in loadedDatabases)
                {
                    TreeNode dbNode = new TreeNode(database.DatabaseName);
                    dbNode.Tag = database; // Associa l'oggetto Database al nodo
                    dbTreeView.Nodes.Add(dbNode);

                    foreach (var table in database.Tables) // Itera sulle tabelle del database corrente
                    {
                        TreeNode tableNode = new TreeNode(table.TableName);
                        tableNode.Tag = table; // Associa l'oggetto Table al nodo
                        dbNode.Nodes.Add(tableNode);

                        foreach (var field in table.Fields) // Itera sui campi della tabella corrente
                        {
                            TreeNode fieldNode = new TreeNode($"{field.FieldName} ({field.DataType})");
                            fieldNode.Tag = field; // Associa l'oggetto Field al nodo
                            tableNode.Nodes.Add(fieldNode);
                        }
                    }
                    dbNode.ExpandAll(); // Espandi tutti i nodi per mostrare l'intera struttura di questo database
                }
            }
        }
        /// <summary>
        /// Popola la TreeView con la struttura del database corrente (currentDatabase).
        /// (Assumendo che tu abbia un controllo TreeView chiamato 'dbTreeView')
        /// </summary>
        private void PopulateDatabaseTreeView1()
        {
            if (dbTreeView != null)
            {
                dbTreeView.Nodes.Clear();

                if (currentDatabase != null)
                {
                    TreeNode dbNode = new TreeNode(currentDatabase.DatabaseName);
                    dbNode.Tag = currentDatabase; // Associa l'oggetto Database al nodo
                    dbTreeView.Nodes.Add(dbNode);

                    foreach (var table in currentDatabase.Tables)
                    {
                        TreeNode tableNode = new TreeNode(table.TableName);
                        tableNode.Tag = table; // Associa l'oggetto Table al nodo
                        dbNode.Nodes.Add(tableNode);

                        foreach (var field in table.Fields)
                        {
                            TreeNode fieldNode = new TreeNode($"{field.FieldName} ({field.DataType})");
                            fieldNode.Tag = field; // Associa l'oggetto Field al nodo
                            tableNode.Nodes.Add(fieldNode);
                        }
                    }
                    dbNode.ExpandAll(); // Espandi tutti i nodi per mostrare l'intera struttura
                }
            }
        }
        //private Type GetTypeForFieldType(string fieldType)
        //{
        //    if (fieldType.Equals("int", StringComparison.OrdinalIgnoreCase)) return typeof(int);
        //    if (fieldType.Equals("bool", StringComparison.OrdinalIgnoreCase)) return typeof(bool);
        //    if (fieldType.Equals("double", StringComparison.OrdinalIgnoreCase)) return typeof(double);
        //    // Aggiungi altri tipi se necessario (es. DateTime, decimal)
        //    return typeof(string); // Default a stringa
        //}
        // Funzione helper per ottenere il tipo CLR dal tipo di campo stringa
        // Questa funzione assume che la tua classe Field abbia una proprietà 'DataType' di tipo stringa.
        private Type GetTypeForFieldType(string dataType)
        {
            if (dataType.Equals("int", StringComparison.OrdinalIgnoreCase)) return typeof(int);
            if (dataType.Equals("bool", StringComparison.OrdinalIgnoreCase)) return typeof(bool);
            if (dataType.Equals("double", StringComparison.OrdinalIgnoreCase)) return typeof(double);
            // Aggiungi altri tipi se necessario (es. DateTime, decimal)
            return typeof(string); // Default a stringa
        }
        /// <summary>
        /// Aggiunge una nuova TabPage al tabControlDetails per visualizzare i record di una tabella,
        /// o seleziona una tab esistente se già aperta.
        /// </summary>
        /// <param name="tableName">Il nome della tabella da visualizzare.</param>
        /// <param name="database">Il database a cui appartiene la tabella.</param>
        private void AddTableDataRecordsTab(string tableName, Database database)
        {
            // Cerca se la tab per questa tabella e database esiste già
            foreach (TabPage page in tabControlDetails.TabPages)
            {
                // Controlla se il testo della tab (nome tabella) corrisponde
                // E se l'oggetto Database nel Tag della tab è la stessa istanza del database passato
                if (page.Text.Equals(tableName, StringComparison.OrdinalIgnoreCase) && page.Tag is Database existingDb && existingDb == database)
                {
                    tabControlDetails.SelectedTab = page; // Seleziona la tab esistente
                                                          // Aggiorna _currentActiveDataGridView al DataGridView della tab esistente
                    DataGridView existingDgv = page.Controls[0] as DataGridView;
                    if (existingDgv != null)
                    {
                        _currentActiveDataGridView = existingDgv;
                        // Assicurati che _currentActiveTableInUI sia aggiornata correttamente
                        _currentActiveTableInUI = database.Tables.FirstOrDefault(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                    }
                    AppendToMonitor($"Tab '{tableName}' già aperta per il database '{database.DatabaseName}', selezionata.");
                    UpdateStatus($"Tab '{tableName}' già aperta, selezionata.");
                    return; // Tab già aperta, esci
                }
            }

            // Se la tab non esiste, creala
            TabPage dataRecordsTabPage = new TabPage(tableName);
            dataRecordsTabPage.Name = tableName;
            dataRecordsTabPage.Tag = database; // Associa l'intera istanza del database alla TabPage

            dgvDataRecords = new DataGridView();
            dgvDataRecords.Dock = DockStyle.Fill;
            dgvDataRecords.AllowUserToAddRows = true;
            dgvDataRecords.AllowUserToDeleteRows = true;
            dgvDataRecords.ReadOnly = false; // Permetti la modifica
            dgvDataRecords.AutoGenerateColumns = true; // Genera colonne automaticamente

            // Trova la tabella nel database usando la tua classe Table
            Table table = database.Tables.FirstOrDefault(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));

            if (table != null)
            {
                // Crea una DataTable per il DataGridView
                DataTable dt = new DataTable(tableName);

                // Aggiungi le colonne alla DataTable basandoti sui CAMPI (Fields) della tabella
                // Utilizza la tua classe Field e la sua proprietà DataType
                foreach (var field in table.Fields)
                {
                    // Determina il tipo di colonna in base al tipo di campo usando field.DataType
                    Type columnType = GetTypeForFieldType(field.DataType); // Usa la tua proprietà DataType

                    DataColumn dc = new DataColumn(field.FieldName, columnType);
                    dc.AllowDBNull = true; // O false, a seconda della tua logica di business
                    dt.Columns.Add(dc);
                }

                // Popola la DataTable con i record esistenti
                // Utilizza la tua List<SerializableDictionary<string, object>> DataRecords
                foreach (var recordDict in table.DataRecords) // Corretto: 'recordDict' è già il SerializableDictionary
                {
                    DataRow dr = dt.NewRow();
                    foreach (var field in table.Fields) // Itera sui campi per ottenere i nomi attesi
                    {
                        string fieldName = field.FieldName;
                        // Accedi ai valori del dizionario direttamente su 'recordDict'
                        if (recordDict.ContainsKey(fieldName) && dt.Columns.Contains(fieldName)) // Corretto: recordDict.ContainsKey
                        {
                            try
                            {
                                Type targetType = dt.Columns[fieldName].DataType;
                                object value = recordDict[fieldName]; // Corretto: recordDict[fieldName]
                                                                      // Gestisci DBNull.Value per i null
                                if (value == null)
                                {
                                    dr[fieldName] = DBNull.Value;
                                }
                                else
                                {
                                    object convertedValue = Convert.ChangeType(value, targetType);
                                    dr[fieldName] = convertedValue;
                                }
                            }
                            catch (FormatException)
                            {
                                dr[fieldName] = DBNull.Value; // Assegna DBNull.Value per valori non validi
                                AppendToMonitor($"Avviso: Errore di formato per il campo '{fieldName}' nel record. Valore non valido.");
                            }
                            catch (InvalidCastException)
                            {
                                dr[fieldName] = DBNull.Value; // Assegna DBNull.Value per valori non validi
                                AppendToMonitor($"Avviso: Errore di cast per il campo '{fieldName}' nel record. Valore non valido.");
                            }
                            catch (Exception ex) // Cattura altri errori generici durante la conversione
                            {
                                dr[fieldName] = DBNull.Value;
                                AppendToMonitor($"Errore generico durante la conversione per il campo '{fieldName}': {ex.Message}");
                            }
                        }
                        else if (dt.Columns.Contains(fieldName))
                        {
                            dr[fieldName] = DBNull.Value; // Imposta a DBNull se il record non ha il valore
                        }
                    }
                    dt.Rows.Add(dr);
                }

                dgvDataRecords.DataSource = dt;

                // Associa i gestori eventi per il DataGridView
                dgvDataRecords.DataError += DgvDataRecords_DataError;
                dgvDataRecords.UserAddedRow += DgvDataRecords_UserAddedRow;
                dgvDataRecords.UserDeletedRow += DgvDataRecords_UserDeletedRow;
                dgvDataRecords.UserDeletingRow += DgvDataRecords_UserDeletingRow;
                dgvDataRecords.CellValueChanged += DgvDataRecords_CellValueChanged;
                dgvDataRecords.RowValidated += DgvDataRecords_RowValidated;

                dataRecordsTabPage.Controls.Add(dgvDataRecords);

                // Aggiungi la TabPage al TabControl
                tabControlDetails.TabPages.Add(dataRecordsTabPage);

                // Seleziona la nuova TabPage
                tabControlDetails.SelectedTab = dataRecordsTabPage;

                // FONDAMENTALE: Aggiorna _currentActiveDataGridView e _currentActiveTableInUI qui
                _currentActiveDataGridView = dgvDataRecords;
                _currentActiveTableInUI = table; // Imposta la tabella attiva in UI

                UpdateStatus($"Tab '{tableName}' aperta.");
            }
            else
            {
                MessageBox.Show($"Tabella '{tableName}' non trovata nel database '{database.DatabaseName}'.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataRecordsTabPage.Dispose(); // Rimuovi la tab creata se la tabella non è stata trovata
            }
        }

        private void DgvDataRecords_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            string primaryKeyFieldName = "";
            if (dgv.DataSource == null || !(dgv.DataSource is DataTable dt)) return;
            if (_currentActiveTableInUI == null) return;

            // *** CORREZIONE: Accedi alla riga tramite dgv.Rows[e.RowIndex] ***
            // Ignora gli eventi che non sono per righe di dati valide (es. header)
            if (e.RowIndex < 0) return;

            // Recupera la riga del DataGridView
            DataGridViewRow validatedRow = dgv.Rows[e.RowIndex];

            // Se la riga è la riga "nuova riga" (placeholder), ignorala.
            // Dobbiamo usare validatedRow.IsNewRow, non e.Row.IsNewRow
            if (validatedRow.IsNewRow)
            {
                return;
            }

            DataRow dr = null;
            try
            {
                // Recupera la DataRow corrispondente alla riga validata nel DataGridView.
                dr = dt.Rows[e.RowIndex];
            }
            catch (IndexOutOfRangeException)
            {
                AppendToMonitor($"Avviso: Impossibile trovare DataRow per l'indice {e.RowIndex} in RowValidated.");
                return;
            }

            SerializableDictionary<string, object> recordToUpdate = null;
            int recordIndex = e.RowIndex;

            // ===================================================================================================
            // Logica per distinguere Nuovi Record da Record Esistenti Modificati
            // ===================================================================================================
            // Questa condizione si verifica quando una riga è stata appena aggiunta al DataTable
            // e non ancora presente nella nostra lista _currentActiveTableInUI.DataRecords.
            // L'indice della riga validata (e.RowIndex) dovrebbe essere uguale al numero attuale di record nella lista.
            if (recordIndex >= 0 && recordIndex < dt.Rows.Count && recordIndex == _currentActiveTableInUI.DataRecords.Count)
            {
                // È una nuova riga che è stata appena commessa alla DataTable.
                // Dobbiamo creare un nuovo SerializableDictionary e popolarlo con TUTTI i valori della riga.
                recordToUpdate = new SerializableDictionary<string, object>();
                #region *** INIZIO MODIFICHE PER L'AUTOINCREMENTO ***
                Field autoIncrementPrimaryKeyField = _currentActiveTableInUI.Fields.FirstOrDefault(f => f.Key && f.PrimaryKeyAutoIncrement);

                if (autoIncrementPrimaryKeyField != null)
                {
                    primaryKeyFieldName = autoIncrementPrimaryKeyField.FieldName;
                    Type primaryKeyType = GetTypeForFieldType(autoIncrementPrimaryKeyField.DataType);
                    object nextId = null;

                    // Trova il valore massimo corrente della chiave primaria
                    if (_currentActiveTableInUI.DataRecords.Any() && _currentActiveTableInUI.DataRecords.First().ContainsKey(primaryKeyFieldName))
                    {
                        dynamic maxId = null;
                        foreach (var record in _currentActiveTableInUI.DataRecords)
                        {
                            if (record.ContainsKey(primaryKeyFieldName) && record[primaryKeyFieldName] != null)
                            {
                                dynamic currentValue = Convert.ChangeType(record[primaryKeyFieldName], primaryKeyType);
                                if (maxId == null || currentValue > maxId)
                                {
                                    maxId = currentValue;
                                }
                            }
                        }

                        // Calcola il prossimo ID
                        if (maxId != null)
                        {
                            if (primaryKeyType == typeof(int)) nextId = (int)maxId + 1;
                            else if (primaryKeyType == typeof(uint)) nextId = (uint)maxId + 1;
                            else if (primaryKeyType == typeof(long)) nextId = (long)maxId + 1;
                            else if (primaryKeyType == typeof(ulong)) nextId = (ulong)maxId + 1;
                            // Aggiungi altri tipi numerici se necessario (short, ushort, etc.)
                        }
                        else
                        {
                            // Se non ci sono record, inizia da 1 (o un altro valore iniziale se preferisci)
                            if (primaryKeyType == typeof(int) || primaryKeyType == typeof(uint) || primaryKeyType == typeof(long) || primaryKeyType == typeof(ulong))
                            {
                                nextId = Convert.ChangeType(1, primaryKeyType);
                            }
                        }

                        // Imposta il valore autoincrementale nel nuovo record
                        if (nextId != null)
                        {
                            recordToUpdate[primaryKeyFieldName] = nextId;
                        }
                    }
                    else
                    {
                        // Se non ci sono record e la tabella ha una chiave primaria autoincrementale, inizia da 1
                        if (primaryKeyType == typeof(int) || primaryKeyType == typeof(uint) || primaryKeyType == typeof(long) || primaryKeyType == typeof(ulong))
                        {
                            recordToUpdate[primaryKeyFieldName] = Convert.ChangeType(1, primaryKeyType);
                        }
                    }
                }
                #endregion *** FINE MODIFICHE PER L'AUTOINCREMENTO ***
                // Popola il nuovo record con i valori dalla riga del DataGridView
                foreach (DataGridViewColumn column in dgv.Columns)
                {
                    // Ignora le colonne non visibili o che non corrispondono a un campo dati
                    if (!column.Visible || string.IsNullOrEmpty(column.DataPropertyName))
                    {
                        continue;
                    }

                    string fieldName = column.DataPropertyName; // Usa DataPropertyName per il nome del campo
                    object cellValue = validatedRow.Cells[column.Index].Value; // Accedi alla cella tramite validatedRow

                    // Converti DBNull.Value in null per il tuo dizionario
                    if (cellValue == DBNull.Value)
                    {
                        recordToUpdate[fieldName] = null;
                    }
                    else
                    {
                        // Tenta di convertire il valore al tipo di campo atteso
                        Field correspondingField = _currentActiveTableInUI.Fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                        if (correspondingField != null)
                        {
                            try
                            {
                                Type targetType = GetTypeForFieldType(correspondingField.DataType);
                                object convertedValue = Convert.ChangeType(cellValue, targetType);
                                recordToUpdate[fieldName] = convertedValue;
                            }
                            catch (Exception ex)
                            {
                                AppendToMonitor($"Errore di conversione per il campo '{fieldName}' durante l'aggiunta del nuovo record in RowValidated: {ex.Message}");
                                recordToUpdate[fieldName] = null; // In caso di errore, imposta a null
                            }
                        }
                        else
                        {
                            // Se il campo non è trovato nella definizione della tabella, salva il valore così com'è
                            recordToUpdate[fieldName] = cellValue;
                        }
                    }
                }
                // ===============================================================================================
                // NUOVO: Controllo Unicità Chiave Primaria per Nuovi Record
                // Usiamo _currentActiveTableInUI.PrimaryKeyFieldName che è già stato popolato.
                // ===============================================================================================
                primaryKeyFieldName = _currentActiveTableInUI.PrimaryKeyFieldName;

                if (!string.IsNullOrEmpty(primaryKeyFieldName)) // Se una chiave primaria è definita per la tabella
                {
                    object newPkValue = recordToUpdate.ContainsKey(primaryKeyFieldName) ? recordToUpdate[primaryKeyFieldName] : null;

                    bool isDuplicate = false;
                    foreach (var existingRecord in _currentActiveTableInUI.DataRecords)
                    {
                        object existingPkValue = existingRecord.ContainsKey(primaryKeyFieldName) ? existingRecord[primaryKeyFieldName] : null;

                        // Confronta i valori della chiave primaria. Usiamo Equals per un confronto robusto.
                        if (Equals(newPkValue, existingPkValue))
                        {
                            isDuplicate = true;
                            break; // Trovato un duplicato
                        }
                    }

                    if (isDuplicate)
                    {
                        MessageBox.Show($"Impossibile aggiungere il record: il valore per la chiave primaria '{primaryKeyFieldName}' = '{newPkValue}' esiste già.", "Errore di integrità", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        AppendToMonitor($"Errore: Tentativo di aggiungere un record con chiave primaria duplicata nella tabella '{_currentActiveTableInUI.TableName}'. Valore: '{newPkValue}'.");

                        // Annulla l'aggiunta della riga nel DataGridView e nel DataTable
                        if (dt.Rows.Count > e.RowIndex && dt.Rows[e.RowIndex].RowState == DataRowState.Added)
                        {
                            dt.Rows[e.RowIndex].Delete(); // Segna la riga per la cancellazione
                            dt.AcceptChanges(); // Applica la cancellazione
                        }
                        // Potresti anche voler riportare il focus sulla riga o sulla cella problematica
                        return; // Esce dal metodo per prevenire l'aggiunta del record
                    }
                }
                _currentActiveTableInUI.DataRecords.Add(recordToUpdate);
                AppendToMonitor($"Nuovo record completo aggiunto alla tabella '{_currentActiveTableInUI.TableName}' tramite RowValidated.");

                // Decidi qui se vuoi salvare il database automaticamente dopo ogni nuovo record.
                // SaveDatabase();
            }
            else
            {
                // Se non è una nuova riga (cioè è una riga esistente che è stata modificata),
                // allora la sua gestione spetta a CellValueChanged.
                // Non facciamo nulla qui per le modifiche di righe esistenti.
                UpdateStatus($"Riga esistente all'indice {e.RowIndex} validata. Le modifiche sono gestite da CellValueChanged.");
            }
        }
        #region eventi datagrid view
        private void DgvDataRecords_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Ignora gli eventi che non sono per righe/colonne di dati valide (es. header o fuori dai limiti)
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            DataGridView dgv = (DataGridView)sender;
            if (dgv.DataSource == null || !(dgv.DataSource is DataTable dt)) return;
            if (_currentActiveTableInUI == null) return;

            // *** IMPORTANTE: Ignora la riga "nuova riga" (placeholder) ***
            // Se l'evento si scatena per la riga vuota in fondo (dgv.NewRowIndex),
            // significa che l'utente sta digitando in essa, ma la DataRow corrispondente
            // non è ancora stata aggiunta alla DataTable. Ignoriamo per ora.
            // La gestione dell'aggiunta del record completo avverrà in RowValidated.
            if (e.RowIndex == dgv.NewRowIndex)
            {
                return;
            }

            // A questo punto, la riga non è il placeholder, quindi dovrebbe esistere nel DataTable.
            // Dobbiamo assicurarci che esista anche nel nostro _currentActiveTableInUI.DataRecords.
            // Se e.RowIndex è maggiore o uguale al numero di record, significa che c'è un disallineamento
            // (teoricamente non dovrebbe accadere se RowValidated funziona correttamente).
            if (e.RowIndex >= _currentActiveTableInUI.DataRecords.Count)
            {
                AppendToMonitor($"Errore: Tentativo di modificare un record non ancora presente nella lista interna all'indice {e.RowIndex} in CellValueChanged. Questo non dovrebbe accadere dopo RowValidated.");
                return;
            }

            // Ottieni il SerializableDictionary corrispondente al record che stiamo modificando
            SerializableDictionary<string, object> recordToUpdate = _currentActiveTableInUI.DataRecords[e.RowIndex];

            // ===================================================================================================
            // Aggiorna il valore della singola cella modificata nel SerializableDictionary
            // ===================================================================================================
            string fieldName = dgv.Columns[e.ColumnIndex].DataPropertyName; // Usa DataPropertyName per il nome del campo
            object cellValue = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            // Converti DBNull.Value in null per il tuo dizionario
            if (cellValue == DBNull.Value)
            {
                recordToUpdate[fieldName] = null;
            }
            else
            {
                // Tenta di convertire il valore al tipo di campo atteso
                Field correspondingField = _currentActiveTableInUI.Fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                if (correspondingField != null)
                {
                    try
                    {
                        Type targetType = GetTypeForFieldType(correspondingField.DataType);
                        object convertedValue = Convert.ChangeType(cellValue, targetType);
                        recordToUpdate[fieldName] = convertedValue;
                    }
                    catch (Exception ex)
                    {
                        AppendToMonitor($"Errore di conversione per il campo '{fieldName}' in CellValueChanged: {ex.Message}");
                        recordToUpdate[fieldName] = null; // In caso di errore, imposta a null
                    }
                }
                else
                {
                    // Se il campo non è trovato, salva il valore così com'è
                    recordToUpdate[fieldName] = cellValue;
                }
            }
            UpdateStatus($"Cella '{fieldName}' nella riga {e.RowIndex} aggiornata.");

            // Decidi qui se vuoi salvare il database automaticamente dopo ogni modifica di cella.
            // ATTENZIONE: Questo può portare a salvataggi molto frequenti (ogni singola battitura).
            // È spesso preferibile salvare solo quando l'utente esce dalla riga (RowValidated)
            // o quando preme un pulsante "Salva".
            // SaveDatabase();
        }

        private void DgvDataRecords_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            // Verifica che ci sia una tabella attiva nell'UI.
            if (_currentActiveTableInUI == null)
            {
                AppendToMonitor("Errore: Impossibile eliminare record, nessuna tabella attiva in UI.");
                UpdateStatus("Errore: Nessuna tabella attiva.");
                e.Cancel = true; // Annulla l'eliminazione
                return;
            }

            // Verifica che la tabella attiva abbia una chiave primaria definita.
            // Questa proprietà dovrebbe essere stata popolata dopo il caricamento del database.
            if (string.IsNullOrEmpty(_currentActiveTableInUI.PrimaryKeyFieldName))
            {
                MessageBox.Show($"La tabella '{_currentActiveTableInUI.TableName}' non ha una chiave primaria definita. Impossibile eliminare il record.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true; // Annulla l'eliminazione
                return;
            }

            DataGridView dgv = (DataGridView)sender;
            // L'evento UserDeletingRow si verifica *prima* che la riga venga rimossa dalla DataTable.
            // Quindi, possiamo ancora accedere ai dati della riga che sta per essere eliminata.
            // e.Row è la riga che sta per essere eliminata.
            DataGridViewRow rowToDelete = e.Row;

            // Ottieni il nome del campo chiave primaria dalla tua tabella attiva
            string primaryKeyFieldName = _currentActiveTableInUI.PrimaryKeyFieldName;

            // Verifica che la colonna della chiave primaria esista nella riga del DataGridView
            if (!dgv.Columns.Contains(primaryKeyFieldName))
            {
                MessageBox.Show($"La colonna della chiave primaria '{primaryKeyFieldName}' non è stata trovata nella riga del DataGridView. Impossibile eliminare il record.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true; // Annulla l'eliminazione
                return;
            }

            // Recupera il valore della chiave primaria dalla riga che sta per essere eliminata
            object primaryKeyValue = rowToDelete.Cells[primaryKeyFieldName].Value;

            if (primaryKeyValue == null || primaryKeyValue == DBNull.Value)
            {
                MessageBox.Show($"Il valore della chiave primaria '{primaryKeyFieldName}' è nullo nella riga da eliminare. Impossibile identificare il record.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true; // Annulla l'eliminazione
                return;
            }

            // Chiedi conferma all'utente prima di eliminare
            DialogResult confirmResult = MessageBox.Show(
                $"Sei sicuro di voler eliminare il record con {primaryKeyFieldName} = '{primaryKeyValue}' dalla tabella '{_currentActiveTableInUI.TableName}'?",
                "Conferma Eliminazione",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.No)
            {
                e.Cancel = true; // L'utente ha annullato
                AppendToMonitor("Eliminazione record annullata dall'utente.");
                UpdateStatus("Eliminazione annullata.");
                return;
            }

            // Ora, usa primaryKeyValue per trovare e rimuovere il SerializableDictionary corrispondente
            // dalla tua lista _currentActiveTableInUI.DataRecords.
            bool removedFromDataRecords = false;

            // Utilizziamo RemoveAll per rimuovere il record dalla lista DataRecords.
            // La logica di confronto deve gestire i tipi di chiave primaria (int, string, ecc.).
            removedFromDataRecords = _currentActiveTableInUI.DataRecords.RemoveAll(recordDict =>
            {
                if (recordDict.TryGetValue(primaryKeyFieldName, out object valueInDict))
                {
                    // Confronta il valore della chiave primaria della riga con quello nel dizionario.
                    // Gestisce i valori nulli e DBNull.Value per un confronto robusto.
                    if (primaryKeyValue == DBNull.Value || primaryKeyValue == null)
                    {
                        return valueInDict == null || valueInDict == DBNull.Value;
                    }
                    else
                    {
                        // Confronta i valori convertendoli a stringa per una generalizzazione.
                        // Questo funziona bene per int, string, Guid.
                        return primaryKeyValue.ToString().Equals(valueInDict?.ToString(), StringComparison.OrdinalIgnoreCase);
                    }
                }
                return false; // Il campo chiave primaria non è presente nel dizionario del record
            }) > 0; // RemoveAll restituisce il numero di elementi rimossi

            if (removedFromDataRecords)
            {
                AppendToMonitor($"Record con chiave '{primaryKeyFieldName}' = '{primaryKeyValue}' eliminato con successo dalla lista DataRecords in memoria.");
                UpdateStatus("Record eliminato.");
                e.Cancel = false; // Permetti al DGV di completare la sua eliminazione visiva
            }
            else
            {
                MessageBox.Show($"Errore: Record con chiave '{primaryKeyFieldName}' = '{primaryKeyValue}' non trovato o non rimosso dalla lista DataRecords in memoria.", "Errore di Eliminazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true; // Annulla l'eliminazione della riga nel DGV se non è stata rimossa dalla lista
                AppendToMonitor("Errore: Record non rimosso dalla lista DataRecords in memoria.");
                UpdateStatus("Errore eliminazione.");
            }

            // IMPORTANTE: Dopo aver modificato i dati in memoria (_currentActiveTableInUI.DataRecords),
            // dovresti salvare il database su disco per rendere persistenti le modifiche.
            // Questo non è gestito da questo metodo. Potresti chiamare un metodo SaveDatabase() qui
            // o in un evento successivo come UserDeletedRow (se lo riattivi per questo scopo).
            // Esempio: SaveDatabase();
        }
        private void DgvDataRecords_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            string errorMessage = $"Errore di Data Binding nella riga {e.RowIndex}, colonna '{e.ColumnIndex}': {e.Exception.Message}";
            AppendToMonitor(errorMessage);
            UpdateStatus($"Errore dati: {e.Exception.Message}");
        }
        private void DgvDataRecords_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            AppendToMonitor("Nuova riga utente aggiunta (in attesa di validazione).");
            UpdateStatus("Nuova riga aggiunta.");
        }
        private void DgvDataRecords_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (_currentActiveTableInUI == null)
            {
                AppendToMonitor("Errore: Impossibile eliminare record, nessuna tabella attiva in UI.");
                UpdateStatus("Errore: Nessuna tabella attiva.");
                return;
            }

            DataGridView dgv = (DataGridView)sender;
            if (dgv.DataSource == null || !(dgv.DataSource is DataTable dt)) return;

            // Ricostruisci DataRecords dalla DataTable aggiornata.
            // Questo è un approccio semplice per garantire la sincronizzazione.
            _currentActiveTableInUI.DataRecords.Clear();
            foreach (DataRow dr in dt.Rows)
            {
                if (dr.RowState != DataRowState.Deleted)
                {
                    // Crea un nuovo SerializableDictionary per ogni riga della DataTable
                    SerializableDictionary<string, object> newRecordDict = new SerializableDictionary<string, object>();
                    foreach (DataColumn dc in dt.Columns)
                    {
                        string fieldName = dc.ColumnName;
                        object cellValue = dr[fieldName];
                        newRecordDict[fieldName] = (cellValue == DBNull.Value) ? null : cellValue; // Corretto: newRecordDict[fieldName]
                    }
                    _currentActiveTableInUI.DataRecords.Add(newRecordDict);
                }
            }

            AppendToMonitor($"Record eliminato dalla tabella '{_currentActiveTableInUI.TableName}'. Lista DataRecords sincronizzata.");
            UpdateStatus("Record eliminato.");
        }
        #endregion
        /// <summary>
        /// Recupera il DataGridView associato a una specifica tabella.
        /// Presuppone che il DataGridView sia l'unico controllo di tipo DataGridView
        /// all'interno della TabPage dedicata a quella tabella.
        /// </summary>
        /// <param name="table">La tabella per cui recuperare il DataGridView.</param>
        /// <returns>Il DataGridView associato alla tabella, o null se non trovato.</returns>
        private DataGridView GetDataGridViewForTable(Table table)
        {
            // Itera attraverso tutte le TabPage nel tabControlDetails
            foreach (TabPage page in tabControlDetails.TabPages)
            {
                // Controlla se il Tag della TabPage corrisponde alla tabella usando TableId
                if (page.Tag is Table pageTable && pageTable.TableId == table.TableId) // *** CORREZIONE QUI: Usiamo TableId ***
                {
                    // Se la TabPage è quella giusta, cerca un DataGridView al suo interno
                    foreach (Control control in page.Controls)
                    {
                        if (control is DataGridView dgv)
                        {
                            return dgv; // Trovato il DataGridView
                        }
                    }
                }
            }
            return null; // DataGridView non trovato per la tabella specificata
        }
        /// <summary>
        /// Ottiene il GUID di un record dalla tabella attiva in base all'indice della riga selezionata nella griglia.
        /// Si basa sulla variabile _currentActiveTableInUI che rappresenta la tabella attualmente visualizzata.
        /// </summary>
        /// <param name="selectedIndex">L'indice della riga selezionata nella griglia (che corrisponde all'indice in DataRecords).</param>
        /// <returns>Il GUID del record corrispondente, o Guid.Empty se non trovato o non valido.</returns>
        public Guid GetGuidFromSelectedIndex(int selectedIndex)
        {
            // Usa _currentActiveTableInUI che è già impostata dal dbTreeView_AfterSelect
            // Questo assicura che stiamo lavorando sulla tabella che è attualmente visualizzata e attiva.
            if (_currentActiveTableInUI == null || _currentActiveTableInUI.DataRecords == null || selectedIndex < 0 || selectedIndex >= _currentActiveTableInUI.DataRecords.Count)
            {
                // Utilizza AppendToMonitor per messaggi di debug/errore nell'UI
                AppendToMonitor($"Errore: Indice selezionato non valido ({selectedIndex}) o tabella attiva non caricata/non valida.");
                return Guid.Empty; // Restituisce un GUID vuoto per indicare un errore
            }

            // 1. Ottieni il SerializableDictionary corrispondente all'indice selezionato
            SerializableDictionary<string, object> selectedRecord = _currentActiveTableInUI.DataRecords[selectedIndex];

            // 2. Cerca il valore del campo GUID nel dizionario
            //    Cerca un campo con nome "Guid" (case-insensitive) tra i Fields della tabella.
            //    Se non trova un campo specifico, usa "Guid" come fallback.
            string guidKey = _currentActiveTableInUI.Fields.FirstOrDefault(f => f.FieldName.Equals("Guid", StringComparison.OrdinalIgnoreCase))?.FieldName ?? "Guid";

            if (selectedRecord.TryGetValue(guidKey, out object guidValue))
            {
                if (guidValue is Guid currentGuid)
                {
                    // Il valore è già un Guid
                    return currentGuid;
                }
                else if (guidValue is string guidString)
                {
                    // Il valore è una stringa, prova a parsare
                    if (Guid.TryParse(guidString, out Guid parsedGuid))
                    {
                        return parsedGuid;
                    }
                }
            }

            AppendToMonitor($"Avviso: Il campo '{guidKey}' non è stato trovato o non è un GUID valido per il record all'indice {selectedIndex} nella tabella '{_currentActiveTableInUI.TableName}'.");
            return Guid.Empty; // Se il GUID non è trovato o non è valido, restituisce Guid.Empty
        }
        /// <summary>
        /// Configura le colonne per un DataGridView che mostra i *dati* dei record.
        /// Le colonne corrispondono ai nomi dei campi della tabella.
        /// </summary>
        /// <param name="dgv">Il DataGridView da configurare.</param>
        /// <param name="fields">La lista di Field che definiscono le colonne.</param>
        private void ConfigureDataRecordsDataGridViewColumns(DataGridView dgv, List<Field> fields)
        {
            if (dgv == null || fields == null) return;

            dgv.Columns.Clear(); // Pulisci eventuali colonne esistenti

            // Aggiungi una colonna per ogni campo nella tabella
            foreach (var field in fields)
            {
                // Crea una colonna in base al nome del campo
                // Il tipo di colonna (testo, checkbox, ecc.) dovrebbe idealmente
                // dipendere dal DataType del campo, ma per semplicità usiamo TextboxColumn per ora.
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = field.FieldName; // L'header è il nome del campo
                                                     // DataPropertyName dovrebbe corrispondere alla chiave nel Dictionary<string, object>
                column.DataPropertyName = field.FieldName;
                column.Name = field.FieldName; // Nome interno della colonna
                column.Width = 100; // Larghezza di default
                                    // Potresti voler impostare ReadOnly = field.ReadOnlyProperty se ne avessi una

                dgv.Columns.Add(column);
            }
        }
        /// <summary>
        /// Popola un DataGridView con i *dati* (record) forniti.
        /// Accetta una lista di SerializableDictionary per i dati dei record.
        /// </summary>
        /// <param name="dgv">Il DataGridView da popolare.</param>
        /// <param name="dataRecords">La lista di record da visualizzare (List<SerializableDictionary<string, object>>).</param>
        private void PopulateDataRecordsDataGridView(DataGridView dgv, List<SerializableDictionary<string, object>> dataRecords) // *** Modificato il tipo del parametro ***
        {
            if (dgv == null || dataRecords == null) return;

            // Usa un DataTable per legare List<SerializableDictionary<string, object>> a un DataGridView.
            DataTable dt = new DataTable();

            // Crea le colonne del DataTable in base ai campi della tabella (se non già fatto)
            // Le colonne del DataGridView dovrebbero già essere state configurate in ConfigureDataRecordsDataGridViewColumns
            // in base ai Field della Table. Usiamo i nomi delle colonne del DataGridView per il DataTable.
            if (dgv.Columns.Count > 0 && dt.Columns.Count == 0)
            {
                foreach (DataGridViewColumn dgvCol in dgv.Columns)
                {
                    // Usa object per flessibilità, gestisci tipi specifici in CellFormatting/Parsing
                    dt.Columns.Add(dgvCol.Name, typeof(object));
                }
            }

            // Popola il DataTable con i dati dai record
            foreach (var record in dataRecords)
            {
                DataRow row = dt.NewRow();
                // Itera sulle colonne del DataTable (che corrispondono ai nomi dei campi)
                foreach (DataColumn col in dt.Columns)
                {
                    string fieldName = col.ColumnName;
                    // Cerca il valore per questo campo nel record (SerializableDictionary)
                    if (record.ContainsKey(fieldName))
                    {
                        // Assegna il valore al DataRow. Gestisci valori null.
                        row[fieldName] = record[fieldName] ?? DBNull.Value;
                    }
                    else
                    {
                        // Se il campo non è presente nel record, imposta DBNull.Value
                        row[fieldName] = DBNull.Value;
                    }
                }
                dt.Rows.Add(row);
            }

            dgv.DataSource = dt; // Lega il DataTable al DataGridView

            // *** Dovrai gestire gli eventi CellFormatting e CellParsing del DataGridView
            // *** per convertire correttamente i tipi di dati tra object e la visualizzazione/editing.
            // dgv.CellFormatting += dgvDataRecords_CellFormatting;
            // dgv.CellParsing += dgvDataRecords_CellParsing;
        }
        /// <summary>
        /// Evidenzia un campo specifico nella ListView (se presente).
        /// Per evidenziare la riga delle proprietà del campo selezionato.
        /// </summary>
        /// <param name="fieldToHighlight">Il Field da evidenziare.</param>
        private void HighlightFieldInListView(Field fieldToHighlight)
        {
            if (listViewFields != null && fieldToHighlight != null)
            {
                // Cerca l'elemento nella ListView il cui Tag corrisponde al campo da evidenziare
                foreach (ListViewItem item in listViewFields.Items)
                {
                    if (item.Tag is Field itemField && itemField.Id == fieldToHighlight.Id)
                    {
                        item.Selected = true; // Seleziona l'elemento
                        listViewFields.EnsureVisible(item.Index); // Assicurati che sia visibile
                                                                  // Opzionale: cambia il colore di sfondo o il font per maggiore enfasi
                                                                  // item.BackColor = Color.Yellow;
                        break; // Esci una volta trovato
                    }
                }
            }
        }
        /// <summary>
        /// Aggiorna la visibilità e l'abilitazione delle voci del ContextMenu
        /// in base al tipo di nodo selezionato.
        /// </summary>
        /// <param name="selectedNode">Il nodo attualmente selezionato nella TreeView.</param>
        private void UpdateTreeViewContextMenu(TreeNode selectedNode)
        {
            if (treeViewContextMenu != null && selectedNode != null)
            {
                // Recupera l'oggetto dati associato al nodo
                object selectedObject = selectedNode.Tag;

                // Trova le voci del menu tramite il loro Tag
                ToolStripItem addTableMenuItem = treeViewContextMenu.Items.Cast<ToolStripItem>().FirstOrDefault(item => item.Tag?.ToString() == "AddTable");
                ToolStripItem addFieldMenuItem = treeViewContextMenu.Items.Cast<ToolStripItem>().FirstOrDefault(item => item.Tag?.ToString() == "AddField");
                ToolStripItem deleteItem = treeViewContextMenu.Items.Cast<ToolStripItem>().FirstOrDefault(item => item.Tag?.ToString() == "Delete");
                ToolStripItem modifyItem = treeViewContextMenu.Items.Cast<ToolStripItem>().FirstOrDefault(item => item.Tag?.ToString() == "Modify");

                // Imposta la visibilità e l'abilitazione in base al tipo di oggetto selezionato
                if (selectedObject is Database)
                {
                    if (addTableMenuItem != null) addTableMenuItem.Visible = true; // Puoi aggiungere una tabella a un database
                    if (addFieldMenuItem != null) addFieldMenuItem.Visible = false; // Non puoi aggiungere un campo a un database
                    if (deleteItem != null) deleteItem.Visible = true; // Puoi eliminare un database
                    if (modifyItem != null) modifyItem.Visible = false;
                }
                else if (selectedObject is Table)
                {
                    if (addTableMenuItem != null) addTableMenuItem.Visible = false; // Non puoi aggiungere una tabella a una tabella
                    if (addFieldMenuItem != null) addFieldMenuItem.Visible = true; // Puoi aggiungere un campo a una tabella
                    if (deleteItem != null) deleteItem.Visible = true; // Puoi eliminare una tabella
                    if (modifyItem != null) modifyItem.Visible = false;
                }
                else if (selectedObject is Field)
                {
                    if (addTableMenuItem != null) addTableMenuItem.Visible = false; // Non puoi aggiungere una tabella a un campo
                    if (addFieldMenuItem != null) addFieldMenuItem.Visible = false; // Non puoi aggiungere un campo a un campo
                    if (deleteItem != null) deleteItem.Visible = true; // Puoi eliminare un campo
                    if (modifyItem!= null) modifyItem.Visible = true; // Puoi modificare un campo
                }
                else
                {
                    // Nessuna azione disponibile per nodi sconosciuti
                    if (addTableMenuItem != null) addTableMenuItem.Visible = false;
                    if (addFieldMenuItem != null) addFieldMenuItem.Visible = false;
                    if (deleteItem != null) deleteItem.Visible = false;
                    if (modifyItem != null) modifyItem.Visible = false;
                }
            }
        }
        /// <summary>
        /// Metodo di utilità per trovare un nodo nella TreeView in base al suo Tag.
        /// </summary>
        /// <param name="nodes">La collezione di nodi in cui cercare.</param>
        /// <param name="tagToFind">Il Tag (oggetto) da cercare.</param>
        /// <returns>Il TreeNode trovato, o null se non trovato.</returns>
        private TreeNode FindNodeByTag(TreeNodeCollection nodes, object tagToFind)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag == tagToFind)
                {
                    return node;
                }
                // Cerca ricorsivamente nei nodi figli
                TreeNode foundNode = FindNodeByTag(node.Nodes, tagToFind);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            return null;
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
        #region method gestione scm
        // --- Metodo Helper per ottenere l'istanza di ServiceController ---
        /// <summary>
        /// Recupera l'istanza di ServiceController per il servizio specificato.
        /// </summary>
        /// <param name="serviceName">Il nome del servizio Windows.</param>
        /// <returns>L'oggetto ServiceController, o null se il servizio non è trovato.</returns>
        private ServiceController GetServiceController(string serviceName)
        {
            try
            {
                // Ottiene tutti i servizi installati sul computer locale
                ServiceController[] scServices = ServiceController.GetServices();

                // Cerca il servizio con il nome specificato
                var service = scServices.FirstOrDefault(sc => sc.ServiceName == serviceName);

                return service;
            }
            catch (Exception ex)
            {
                // Gestisci eventuali errori (es. permessi insufficienti)
                AppendToMonitor($"Errore nel recuperare ServiceController per '{serviceName}': {ex.Message}");
                UpdateStatus($"Errore ServiceController: {ex.Message}");
                return null;
            }
        }
        // --- Metodo per aggiornare lo stato di abilitazione dei pulsanti di controllo servizio ---
        /// <summary>
        /// Aggiorna lo stato di abilitazione dei pulsanti Start, Pause, Stop
        /// in base allo stato corrente del servizio Windows.
        /// </summary>
        private void UpdateServiceControlButtonsState()
        {
            // Assicurati che i pulsanti esistano nel designer
            if (btnServiceStart == null || btnServicePause == null || btnServiceStop == null)
            {
                // Controlli non ancora inizializzati o non presenti
                return;
            }

            ServiceController service = GetServiceController(_serviceName);

            // Se il servizio non è trovato o ci sono errori, disabilita tutto
            if (service == null)
            {
                btnServiceStart.Enabled = false;
                btnServicePause.Enabled = false;
                btnServiceStop.Enabled = false;
                UpdateStatus($"Servizio '{_serviceName}' non trovato.");
                return;
            }

            // Aggiorna lo stato dei pulsanti in base allo stato del servizio
            switch (service.Status)
            {
                case ServiceControllerStatus.Running:
                    btnServiceStart.Enabled = false; // Già avviato
                    btnServicePause.Enabled = service.CanPauseAndContinue; // Abilita pausa solo se supportata
                    btnServiceStop.Enabled = true; // Può essere fermato
                    UpdateStatus($"Servizio '{_serviceName}' in esecuzione.");
                    break;
                case ServiceControllerStatus.Stopped:
                    btnServiceStart.Enabled = true; // Può essere avviato
                    btnServicePause.Enabled = false; // Non può essere messo in pausa
                    btnServiceStop.Enabled = false; // Già fermo
                    UpdateStatus($"Servizio '{_serviceName}' fermo.");
                    break;
                case ServiceControllerStatus.Paused:
                    btnServiceStart.Enabled = false; // Non può essere avviato (è in pausa)
                    btnServicePause.Enabled = service.CanPauseAndContinue; // Può essere ripreso se supportato
                    btnServiceStop.Enabled = true; // Può essere fermato
                    UpdateStatus($"Servizio '{_serviceName}' in pausa.");
                    break;
                case ServiceControllerStatus.StartPending:
                case ServiceControllerStatus.StopPending:
                case ServiceControllerStatus.ContinuePending:
                case ServiceControllerStatus.PausePending:
                    // Durante le transizioni, disabilita tutti i pulsanti per evitare operazioni concorrenti
                    btnServiceStart.Enabled = false;
                    btnServicePause.Enabled = false;
                    btnServiceStop.Enabled = false;
                    UpdateStatus($"Servizio '{_serviceName}' in transizione: {service.Status}");
                    break;
                default:
                    // Stato sconosciuto o errore
                    btnServiceStart.Enabled = false;
                    btnServicePause.Enabled = false;
                    btnServiceStop.Enabled = false;
                    UpdateStatus($"Servizio '{_serviceName}' stato sconosciuto: {service.Status}");
                    break;
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
        #endregion
        #endregion
        #region form events
        private void FrmEvolutiveSystem_Load(object sender, EventArgs e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
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
            /*
            XElement bufferDatiContent = new XElement("BufferDati");

            if (!string.IsNullOrWhiteSpace(localIpAddress))
            {
                bufferDatiContent.Add(new XElement("UiIpAddress", localIpAddress));
            }
            bufferDatiContent.Add(new XElement("UiPort", uiListenPort.ToString()));
            */
        }
        #endregion
        #region Tab control events
        private void tabControlDetails_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Ottieni la tab attualmente selezionata dal TabControl.
            TabPage selectedPage = tabControlDetails.SelectedTab;

            // Verifica se la pagina selezionata esiste e se contiene controlli.
            if (selectedPage != null && selectedPage.Controls.Count > 0)
            {
                // Cerca il DataGridView all'interno dei controlli della pagina.
                // Assumiamo che il DataGridView sia il primo controllo aggiunto alla TabPage,
                // o che sia l'unico controllo di tipo DataGridView.
                DataGridView dgv = selectedPage.Controls[0] as DataGridView;

                // Se il controllo trovato è effettivamente un DataGridView, assegnarlo alla variabile globale.
                if (dgv != null)
                {
                    _currentActiveDataGridView = dgv; // Aggiorna la variabile globale
                }
                else
                {
                    // Se la pagina selezionata non contiene un DataGridView, imposta a null.
                    _currentActiveDataGridView = null;
                }
            }
            else
            {
                // Se non c'è una pagina selezionata o la pagina è vuota, imposta a null.
                _currentActiveDataGridView = null;
            }
        }
        #endregion
        #region dbTreeView events
        private void dbTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Recupera l'oggetto dati associato al nodo selezionato tramite il Tag
            object selectedObject = e.Node.Tag;

            // Inizializza a null. Saranno impostati correttamente da AddTableDataRecordsTab o logica specifica.
            _currentActiveTableInUI = null;
            _currentActiveDataGridView = null;

            // *** DIFFERENZA CHIAVE: Rimosso tabControlDetails.TabPages.Clear(); ***
            // Questo permette alle schede di rimanere aperte quando si cambia selezione nella TreeView.
            // La logica di AddTableDataRecordsTab si occuperà di creare nuove schede o selezionare quelle esistenti.

            // Aggiorna i controlli UI in base al tipo di oggetto selezionato
            if (selectedObject is Database db)
            {
                AppendToMonitor($"Selezionato Database: {db.DatabaseName}");
                UpdateStatus($"Database selezionato: {db.DatabaseName}");
                currentDatabase = db; // Imposta il database selezionato come corrente per le operazioni globali

                lblDbName.Text = currentDatabase.DatabaseName;
                lblTblName.Text = "";

                // *** DIFFERENZA: Gestione dell'apertura delle schede per un Database ***
                // Invece di aprire TUTTE le tabelle, suggerisco di aprire una scheda di riepilogo
                // o la prima tabella, per evitare un eccessivo numero di schede.
                // Se non ci sono tab aperte e il database ha tabelle, apri la prima.
                if (tabControlDetails.TabPages.Count == 0 && db.Tables.Any())
                {
                    AddTableDataRecordsTab(db.Tables.First().TableName, db);
                }
                // Se preferisci ancora aprire tutte le tabelle, il codice sarebbe:
                // foreach (var table in db.Tables)
                // {
                //     AddTableDataRecordsTab(table.TableName, db); // Passa anche il database
                // }
            }
            else if (selectedObject is Table table)
            {
                AppendToMonitor($"Selezionata Tabella: {table.TableName}");
                UpdateStatus($"Tabella selezionata: {table.TableName}");
                currentDatabase = table.ParentDatabase; // Imposta il database padre della tabella come corrente

                // Popola la ListView con le *proprietà* dei campi di questa tabella
                PopulateFieldsListView(table.Fields);

                lblDbName.Text = currentDatabase.DatabaseName;
                lblTblName.Text = table.TableName;

                // *** DIFFERENZA: Chiamata a AddTableDataRecordsTab con nome tabella e database ***
                // Questo metodo si occuperà di creare/selezionare la tab e di impostare
                // _currentActiveDataGridView e _currentActiveTableInUI.
                AddTableDataRecordsTab(table.TableName, currentDatabase);

                // *** DIFFERENZA: Rimosso l'assegnazione diretta qui ***
                // _currentActiveDataGridView = GetDataGridViewForTable(table);
                // L'assegnazione avviene all'interno di AddTableDataRecordsTab.
            }
            else if (selectedObject is Field field)
            {
                AppendToMonitor($"Selezionato Campo: {field.FieldName}");
                UpdateStatus($"Campo selezionato: {field.FieldName}");

                Table parentTable = field.ParentTable;
                Database parentDatabase = parentTable?.ParentDatabase;

                if (parentTable != null && parentDatabase != null) // Assicurati che siano validi
                {
                    currentDatabase = parentDatabase; // Imposta il database padre del campo come corrente
                    PopulateFieldsListView(parentTable.Fields);
                    HighlightFieldInListView(field); // Evidenzia il campo selezionato nella ListView

                    lblDbName.Text = currentDatabase.DatabaseName;
                    lblTblName.Text = parentTable.TableName;

                    // *** DIFFERENZA: Chiamata a AddTableDataRecordsTab con nome tabella e database ***
                    // Questo metodo si occuperà di creare/selezionare la tab e di impostare
                    // _currentActiveDataGridView e _currentActiveTableInUI.
                    AddTableDataRecordsTab(parentTable.TableName, currentDatabase);

                    // *** DIFFERENZA: Rimosso l'assegnazione diretta qui ***
                    // _currentActiveDataGridView = GetDataGridViewForTable(parentTable);
                    // L'assegnazione avviene all'interno di AddTableDataRecordsTab.
                }
                else
                {
                    _currentActiveTableInUI = null;
                    _currentActiveDataGridView = null;
                    AppendToMonitor("Errore: Il nodo campo non ha un nodo padre valido di tipo Tabella o Database.");
                    UpdateStatus("Nessuna tabella valida selezionata.");
                    tabControlDetails.TabPages.Clear(); // In questo caso, pulisci se c'è un errore grave
                    listViewFields.Items.Clear(); // Pulisce la ListView dei campi
                }
            }
            else
            {
                _currentActiveTableInUI = null;
                _currentActiveDataGridView = null;
                AppendToMonitor("Nessun oggetto valido selezionato nella TreeView.");
                UpdateStatus("Nessuna selezione valida.");
                tabControlDetails.TabPages.Clear(); // Pulisce le schede se non c'è una selezione valida
                listViewFields.Items.Clear(); // Pulisce la ListView dei campi
            }
        }
        /// <summary>
        /// Gestore events per il click del mouse su un nodo della TreeView (incluso il tasto destro).
        /// Mostra il ContextMenu in base al nodo selezionato.
        /// </summary>
        private void dbTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Se è stato premuto il tasto destro del mouse
            if (e.Button == MouseButtons.Right)
            {
                // Seleziona il nodo su cui è stato fatto click con il tasto destro
                dbTreeView.SelectedNode = e.Node;

                // Aggiorna lo stato del ContextMenu prima di mostrarlo
                UpdateTreeViewContextMenu(e.Node);

                // Mostra il ContextMenu nella posizione del mouse
                if (treeViewContextMenu != null)
                {
                    treeViewContextMenu.Show(dbTreeView, e.Location);
                }
            }
        }
        #endregion 
        #region buttons events
        private void btnAddDatabase_Click(object sender, EventArgs e)
        {
            string newDbName = string.Empty;

            // Placeholder attuale: per ora, usa un nome di default se non hai ancora integrato il tuo form
            // Rimuovi o commenta questa parte una volta integrato il tuo form di input
            newDbName = "NuovoDatabase_" + (loadedDatabases.Count + 1);
            // MessageBox.Show("Placeholder per l'input del nome database. Integra qui il tuo form personalizzato.", "Nota", MessageBoxButtons.OK, MessageBoxIcon.Information);

            FrmDbName frmDbName = new FrmDbName(newDbName);
            frmDbName.ShowDialog();
            if (frmDbName.DialogResult == DialogResult.OK)
            {
                newDbName = frmDbName.DatabaseName;
                // Se l'utente ha annullato o inserito un nome vuppo (dopo aver integrato il tuo form), esci
                if (string.IsNullOrWhiteSpace(newDbName))
                {
                    UpdateStatus("Creazione database annullata o nome non valido.");
                    return;
                }

                // Controlla se esiste già un database con lo stesso nome
                if (loadedDatabases.Any(db => db.DatabaseName.Equals(newDbName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"Esiste già un database con il nome '{newDbName}'. Scegli un nome diverso.", "Nome Database Esistente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    UpdateStatus($"Nome database '{newDbName}' già esistente.");
                    return;
                }


                // Genera un ID semplice per il nuovo database (assicurati che sia univoco tra i database caricati)
                int newDbId = loadedDatabases.Count > 0 ? loadedDatabases.Max(db => db.DatabaseId) + 1 : 1;
                // Potresti voler implementare una logica di generazione ID più robusta se prevedi di caricare/salvare database multipli.


                // Crea la nuova istanza della classe Database
                Database newDatabase = new Database(newDbId, newDbName);

                // Aggiungi il nuovo database alla lista dei database caricati
                loadedDatabases.Add(newDatabase);

                // Imposta il nuovo database come quello corrente (se gestisci un solo database alla volta)
                // Se gestisci database multipli, potresti voler selezionare il nuovo database nella TreeView
                currentDatabase = newDatabase;


                // Aggiorna la TreeView per mostrare il nuovo database
                PopulateDatabaseTreeView();

                AppendToMonitor($"Aggiunto nuovo database '{newDbName}'.");
                UpdateStatus($"Database '{newDbName}' aggiunto.");

                // Seleziona automaticamente il nuovo nodo nella TreeView
                if (dbTreeView != null)
                {
                    // Trova il nodo corrispondente al nuovo database e selezionalo
                    TreeNode newNode = FindNodeByTag(dbTreeView.Nodes, newDatabase);
                    if (newNode != null)
                    {
                        dbTreeView.SelectedNode = newNode;
                    }
                }
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// Gestore events per il pulsante "Salva Database".
        /// Permette all'utente di salvare il database corrente in un file XML.
        /// </summary>
        private void btnSaveDatabase_Click(object sender, EventArgs e)
        {
            if (loadedDatabases == null || loadedDatabases.Count == 0)
            {
                MessageBox.Show("Nessun database da salvare.", "Salva Database", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatus("Nessun database da salvare.");
                return;
            }

            // *** INIZIO: Logica per selezionare la directory di salvataggio ***
            // Usiamo FolderBrowserDialog per permettere all'utente di scegliere una cartella.
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Seleziona la cartella dove salvare i database.";
                // folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyDocuments; // Opzionale: imposta una cartella iniziale
                // folderBrowserDialog.SelectedPath = "C:\\YourDefaultSavePath"; // Opzionale: imposta un percorso di default
                if (Settings.Default.PathDb.Length > 0)
                {
                    folderBrowserDialog.SelectedPath = Settings.Default.PathDb;
                }
                else
                {
                    string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    folderBrowserDialog.SelectedPath = dbPath;
                }
                folderBrowserDialog.Description = string.Format("Percorso db: {0}", folderBrowserDialog.SelectedPath);
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string saveDirectory = folderBrowserDialog.SelectedPath;
                    Settings.Default.PathDb = folderBrowserDialog.SelectedPath;
                    Settings.Default.Save();

                    // *** Fine: Logica per selezionare la directory di salvataggio ***

                    AppendToMonitor($"Salvataggio di {loadedDatabases.Count} database nella cartella: {saveDirectory}...");
                    UpdateStatus($"Salvataggio in corso in {saveDirectory}...");

                    int savedCount = 0;
                    foreach (var databaseToSave in loadedDatabases)
                    {
                        // Costruisci il percorso completo del file per ogni database.
                        // Puoi usare il nome del database per il nome del file.
                        string fileName = databaseToSave.DatabaseName.Replace(" ", "_") + ".xml";
                        string filePath = Path.Combine(saveDirectory, fileName);

                        try
                        {
                            DatabaseSerializer.SerializeToXmlFile(databaseToSave, filePath);
                            AppendToMonitor($"Database '{databaseToSave.DatabaseName}' salvato con successo in {filePath}");
                            savedCount++;
                        }
                        catch (Exception ex)
                        {
                            // Continua a salvare gli altri database anche in caso di errore con uno
                            AppendToMonitor($"Errore durante il salvataggio del database '{databaseToSave.DatabaseName}': {ex.Message}");
                            MessageBox.Show($"Errore durante il salvataggio del database '{databaseToSave.DatabaseName}': {ex.Message}", "Errore di Salvataggio Parziale", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    if (savedCount == loadedDatabases.Count)
                    {
                        UpdateStatus($"Tutti i {savedCount} database salvati con successo.");
                    }
                    else
                    {
                        UpdateStatus($"Salvataggio completato con {savedCount} di {loadedDatabases.Count} database salvati.");
                    }
                }
                else
                {
                    // L'utente ha annullato la finestra di dialogo
                    UpdateStatus("Salvataggio annullato dall'utente.");
                    AppendToMonitor("Salvataggio annullato.");
                }
            }
        }
        /// <summary>
        /// Gestore events per il pulsante "Carica Database".
        /// Permette all'utente di selezionare un file XML e caricarlo.
        /// </summary>
        private void btnLoadDatabase_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (Settings.Default.PathDb.Length > 0 && Settings.Default.DBName.Length > 0) 
                {
                    openFileDialog.FileName = Path.Combine(Settings.Default.DBName, Settings.Default.PathDb);
                }
                else
                {
                    string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                openFileDialog.Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*";
                openFileDialog.Title = "Seleziona un file XML del Database Semantico";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Settings.Default.PathDb = Path.GetFullPath(openFileDialog.FileName);
                        Settings.Default.DBName = Path.GetFileName(openFileDialog.FileName);
                        Settings.Default.Save();

                        
                        UpdateStatus($"Caricamento database da {openFileDialog.FileName}...");
                        Database loadedDb = DatabaseSerializer.DeserializeFromXmlFile(openFileDialog.FileName);

                        // *** Aggiunto: Controlla se un database con lo stesso ID o nome esiste già ***
                        if (loadedDatabases.Any(db => db.DatabaseId == loadedDb.DatabaseId || db.DatabaseName.Equals(loadedDb.DatabaseName, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show($"Un database con lo stesso ID ({loadedDb.DatabaseId}) o nome ('{loadedDb.DatabaseName}') è già caricato.", "Database Già Caricato", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            UpdateStatus($"Database '{loadedDb.DatabaseName}' già caricato.");
                            return; // Non aggiungere il duplicato
                        }

                        // ===================================================================================================
                        // INSERIMENTO CRUCIALE: Popolare PrimaryKeyFieldName per ogni tabella
                        // ===================================================================================================
                        foreach (Table table in loadedDb.Tables)
                        {
                            table.SetPrimaryKeyFieldName();
                            // Opzionale per debug: Console.WriteLine($"Tabella '{table.TableName}': Chiave Primaria = '{table.PrimaryKeyFieldName ?? "Non definita"}'");
                        }
                        // ===================================================================================================

                        // Aggiungi il database caricato alla lista esistente
                        loadedDatabases.Add(loadedDb);

                        // Imposta il database caricato come quello corrente (opzionale, dipende dalla gestione)
                        currentDatabase = loadedDb;

                        // Aggiorna la TreeView per mostrare TUTTI i database caricati
                        PopulateDatabaseTreeView();

                        // Abilita i controlli rilevanti (solo pulsanti non gestiti da ContextMenu)
                        // if (btnSaveDatabase != null) btnSaveDatabase.Enabled = true;

                        // Passa il database caricato al motore semantico (se gestito qui)
                        // semanticEngine.LoadSemanticDatabase(currentDatabase); // Potrebbe dover gestire database multipli

                        AppendToMonitor($"Database caricato con successo da {openFileDialog.FileName}");
                        UpdateStatus("Database caricato.");

                        // Seleziona automaticamente il nodo del database caricato nella TreeView
                        if (dbTreeView != null)
                        {
                            TreeNode newNode = FindNodeByTag(dbTreeView.Nodes, loadedDb);
                            if (newNode != null)
                            {
                                dbTreeView.SelectedNode = newNode;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore durante il caricamento del database: {ex.Message}", "Errore di Caricamento", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        AppendToMonitor($"Errore durante il caricamento: {ex.Message}");
                        UpdateStatus($"Errore caricamento: {ex.Message}");
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCloseAllDatabases_Click(object sender, EventArgs e)
        {
            // *** Chiedi conferma all'utente prima di chiudere tutti i database ***
            DialogResult confirmResult = MessageBox.Show(
                "Sei sicuro di voler chiudere tutti i database caricati? Le modifiche non salvate andranno perse.",
                "Conferma Chiusura",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                // Cancella la lista dei database caricati
                loadedDatabases.Clear();
                // Resetta il database corrente
                currentDatabase = null;

                // Aggiorna l'UI
                if (dbTreeView != null)
                {
                    dbTreeView.Nodes.Clear(); // Pulisci la TreeView
                }
                if (tabControlDetails != null)
                {
                    tabControlDetails.TabPages.Clear(); // Pulisci le tab dei dettagli/dati
                }
                if (listViewFields != null)
                {
                    listViewFields.Items.Clear(); // Pulisci la ListView dei campi
                }

                // Aggiorna lo stato
                AppendToMonitor("Tutti i database caricati sono stati chiusi.");
                UpdateStatus("Tutti i database chiusi.");

                // Potresti voler disabilitare alcuni pulsanti qui (es. Salva, Avvia)
                // if (btnSaveDatabase != null) btnSaveDatabase.Enabled = false;
                // if (btnStartProcess != null) btnStartProcess.Enabled = false;
            }
            else
            {
                UpdateStatus("Chiusura database annullata.");
                AppendToMonitor("Chiusura database annullata dall'utente.");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void BtnRicaricaDB_Click(object sender, EventArgs e)
        {
            try
            {
                // Cancella la lista dei database caricati
                loadedDatabases.Clear();
                // Resetta il database corrente
                currentDatabase = null;

                // Aggiorna l'UI
                if (dbTreeView != null)
                {
                    dbTreeView.Nodes.Clear(); // Pulisci la TreeView
                }
                if (tabControlDetails != null)
                {
                    tabControlDetails.TabPages.Clear(); // Pulisci le tab dei dettagli/dati
                }
                if (listViewFields != null)
                {
                    listViewFields.Items.Clear(); // Pulisci la ListView dei campi
                }
                if (Settings.Default.PathDb.Length > 0 && Settings.Default.DBName.Length > 0) 
                {
                    string PathFileDb = Path.Combine(Settings.Default.PathDb, Settings.Default.DBName);
                    Database loadedDb = DatabaseSerializer.DeserializeFromXmlFile(PathFileDb);
                    // *** Aggiunto: Controlla se un database con lo stesso ID o nome esiste già ***
                    if (loadedDatabases.Any(db => db.DatabaseId == loadedDb.DatabaseId || db.DatabaseName.Equals(loadedDb.DatabaseName, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"Un database con lo stesso ID ({loadedDb.DatabaseId}) o nome ('{loadedDb.DatabaseName}') è già caricato.", "Database Già Caricato", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        UpdateStatus($"Database '{loadedDb.DatabaseName}' già caricato.");
                        return; // Non aggiungere il duplicato
                    }

                    // ===================================================================================================
                    // INSERIMENTO CRUCIALE: Popolare PrimaryKeyFieldName per ogni tabella
                    // ===================================================================================================
                    foreach (Table table in loadedDb.Tables)
                    {
                        table.SetPrimaryKeyFieldName();
                        // Opzionale per debug: Console.WriteLine($"Tabella '{table.TableName}': Chiave Primaria = '{table.PrimaryKeyFieldName ?? "Non definita"}'");
                    }
                    // ===================================================================================================

                    // Aggiungi il database caricato alla lista esistente
                    loadedDatabases.Add(loadedDb);

                    // Imposta il database caricato come quello corrente (opzionale, dipende dalla gestione)
                    currentDatabase = loadedDb;

                    // Aggiorna la TreeView per mostrare TUTTI i database caricati
                    PopulateDatabaseTreeView();

                    // Abilita i controlli rilevanti (solo pulsanti non gestiti da ContextMenu)
                    // if (btnSaveDatabase != null) btnSaveDatabase.Enabled = true;

                    // Passa il database caricato al motore semantico (se gestito qui)
                    // semanticEngine.LoadSemanticDatabase(currentDatabase); // Potrebbe dover gestire database multipli

                    AppendToMonitor($"Database caricato con successo da {PathFileDb}");
                    UpdateStatus("Database caricato.");

                    // Seleziona automaticamente il nodo del database caricato nella TreeView
                    if (dbTreeView != null)
                    {
                        TreeNode newNode = FindNodeByTag(dbTreeView.Nodes, loadedDb);
                        if (newNode != null)
                        {
                            dbTreeView.SelectedNode = newNode;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Nessun db attivo", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante il caricamento del database: {ex.Message}", "Errore di Caricamento", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AppendToMonitor($"Errore durante il caricamento: {ex.Message}");
                    UpdateStatus($"Errore caricamento: {ex.Message}");
                }
        }
        /// <summary>
        /// Avvio servizio server semantico
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void BtnServiceStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_isAdmin)
                {
                    MessageBox.Show("È necessario eseguire l'applicazione come amministratore per avviare il servizio.", "Permessi Insufficienti", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    UpdateStatus("Avvio servizio non consentito (privilegi insufficienti).");
                    return;
                }
                // *** Modificato: Usa ServiceController per avviare il servizio Windows ***
                ServiceController service = GetServiceController(_serviceName);

                if (service != null)
                {
                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        AppendToMonitor($"Avvio del servizio '{_serviceName}'...");
                        UpdateStatus($"Avvio servizio '{_serviceName}'...");
                        service.Start();
                        // Opzionale: attendi che lo stato cambi
                        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10)); // Attendi fino a 10 secondi
                        AppendToMonitor($"Servizio '{_serviceName}' avviato.");
                        UpdateStatus($"Servizio '{_serviceName}' avviato.");
                    }
                    else
                    {
                        AppendToMonitor($"Servizio '{_serviceName}' è già in stato: {service.Status}");
                        UpdateStatus($"Servizio '{_serviceName}' già in esecuzione.");
                    }
                }
                else
                {
                    AppendToMonitor($"Errore: Servizio '{_serviceName}' non trovato.");
                    UpdateStatus($"Errore: Servizio '{_serviceName}' non trovato.");
                    MessageBox.Show($"Servizio '{_serviceName}' non trovato sul sistema.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // *** Aggiunto: Aggiorna lo stato dei pulsanti dopo l'operazione ***
                UpdateServiceControlButtonsState();
            }
            catch (Exception ex)
            {
                // Gestisci eventuali errori nell'interazione con il servizio
                MessageBox.Show($"Errore durante l'avvio del servizio: {ex.Message}", "Errore Controllo Servizio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendToMonitor($"Errore nell'avvio del servizio '{_serviceName}': {ex.Message}");
                UpdateStatus($"Errore avvio servizio: {ex.Message}");
                // *** Aggiunto: Aggiorna lo stato dei pulsanti anche in caso di errore ***
                UpdateServiceControlButtonsState();
            }
        }
        /// <summary>
        /// Sospensione del servizio server semantico
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void BtnServicePause_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_isAdmin)
                {
                    MessageBox.Show("È necessario eseguire l'applicazione come amministratore per avviare il servizio.", "Permessi Insufficienti", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    UpdateStatus("Avvio servizio non consentito (privilegi insufficienti).");
                    return;
                }
                // *** Modificato: Usa ServiceController per mettere in pausa/riprendere il servizio Windows ***
                ServiceController service = GetServiceController(_serviceName);

                if (service != null)
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        if (service.CanPauseAndContinue) // Verifica se il servizio supporta pausa/ripresa
                        {
                            AppendToMonitor($"Messa in pausa del servizio '{_serviceName}'...");
                            UpdateStatus($"Pausa servizio '{_serviceName}'...");
                            service.Pause();
                            // Opzionale: attendi che lo stato cambi
                            service.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromSeconds(10)); // Attendi fino a 10 secondi
                            AppendToMonitor($"Servizio '{_serviceName}' in pausa.");
                            UpdateStatus($"Servizio '{_serviceName}' in pausa.");
                        }
                        else
                        {
                            AppendToMonitor($"Servizio '{_serviceName}' non supporta pausa/ripresa.");
                            UpdateStatus($"Servizio non supporta pausa.");
                            MessageBox.Show($"Il servizio '{_serviceName}' non supporta le operazioni di pausa e ripresa.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else if (service.Status == ServiceControllerStatus.Paused)
                    {
                        AppendToMonitor($"Ripresa del servizio '{_serviceName}'...");
                        UpdateStatus($"Ripresa servizio '{_serviceName}'...");
                        service.Continue();
                        // Opzionale: attendi che lo stato cambi
                        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10)); // Attendi fino a 10 secondi
                        AppendToMonitor($"Servizio '{_serviceName}' ripreso.");
                        UpdateStatus($"Servizio '{_serviceName}' ripreso.");
                    }
                    else
                    {
                        AppendToMonitor($"Servizio '{_serviceName}' non può essere messo in pausa/ripreso dallo stato corrente: {service.Status}");
                        UpdateStatus($"Impossibile mettere in pausa/riprendere.");
                    }
                }
                else
                {
                    AppendToMonitor($"Errore: Servizio '{_serviceName}' non trovato.");
                    UpdateStatus($"Errore: Servizio '{_serviceName}' non trovato.");
                    MessageBox.Show($"Servizio '{_serviceName}' non trovato sul sistema.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // *** Aggiunto: Aggiorna lo stato dei pulsanti dopo l'operazione ***
                UpdateServiceControlButtonsState();
            }
            catch (Exception ex)
            {
                // Gestisci errori
                MessageBox.Show($"Errore durante la pausa/ripresa del servizio: {ex.Message}", "Errore Controllo Servizio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendToMonitor($"Errore nella pausa/ripresa del servizio '{_serviceName}': {ex.Message}");
                UpdateStatus($"Errore pausa/ripresa servizio: {ex.Message}");
                // *** Aggiunto: Aggiorna lo stato dei pulsanti anche in caso di errore ***
                UpdateServiceControlButtonsState();
            }
        }
        /// <summary>
        /// Arresto del servizio del server semantico
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void BtnServiceStop_Click(object sender, EventArgs e)
        {
            if (!_isAdmin)
            {
                MessageBox.Show("È necessario eseguire l'applicazione come amministratore per avviare il servizio.", "Permessi Insufficienti", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateStatus("Avvio servizio non consentito (privilegi insufficienti).");
                return;
            }
            try
            {
                // *** Modificato: Usa ServiceController per fermare il servizio Windows ***
                ServiceController service = GetServiceController(_serviceName);

                if (service != null)
                {
                    if (service.Status != ServiceControllerStatus.Stopped)
                    {
                        AppendToMonitor($"Arresto del servizio '{_serviceName}'...");
                        UpdateStatus($"Arresto servizio '{_serviceName}'...");
                        service.Stop();
                        // Opzionale: attendi che lo stato cambi
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10)); // Attendi fino a 10 secondi
                        AppendToMonitor($"Servizio '{_serviceName}' fermato.");
                        UpdateStatus($"Servizio '{_serviceName}' fermato.");
                    }
                    else
                    {
                        AppendToMonitor($"Servizio '{_serviceName}' è già fermo.");
                        UpdateStatus($"Servizio '{_serviceName}' già fermo.");
                    }
                }
                else
                {
                    AppendToMonitor($"Errore: Servizio '{_serviceName}' non trovato.");
                    UpdateStatus($"Errore: Servizio '{_serviceName}' non trovato.");
                    MessageBox.Show($"Servizio '{_serviceName}' non trovato sul sistema.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // *** Aggiunto: Aggiorna lo stato dei pulsanti dopo l'operazione ***
                UpdateServiceControlButtonsState();
            }
            catch (Exception ex)
            {
                // Gestisci errori
                MessageBox.Show($"Errore durante l'arresto del servizio: {ex.Message}", "Errore Controllo Servizio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendToMonitor($"Errore nell'arresto del servizio '{_serviceName}': {ex.Message}");
                UpdateStatus($"Errore arresto servizio: {ex.Message}");
                // *** Aggiunto: Aggiorna lo stato dei pulsanti anche in caso di errore ***
                UpdateServiceControlButtonsState();
            }
        }
        /// <summary>
        /// Avvio del client socket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void BtnSocket_Click(object sender, EventArgs e)
        {
            FrmSocketClient fSocket = new FrmSocketClient(_logger, asl);
            fSocket.ShowDialog();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void BtnAnalysis_Click(object sender, EventArgs e)
        {
            Point p = PointToScreen(new Point(gbAnalysis.Left + btnAnalysis.Left + btnAnalysis.Width, btnAnalysis.Top + btnAnalysis.Height));
            //Point p = PointToScreen(new Point(panelCommands.Left + panelCommands.Left + btnAnalysis.Width, btnAnalysis.Top + btnAnalysis.Height));
            AnalysisContextMenu.Show(p);
        }

        #endregion
        #region context menu events
        /// <summary>
        /// Gestore generico per i click sulle voci del ContextMenu della TreeView.
        /// </summary>
        private void ContextMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null && dbTreeView.SelectedNode != null)
            {
                // Recupera l'azione dal Tag della voce di menu
                string action = menuItem.Tag?.ToString();
                // Recupera l'oggetto dati associato al nodo selezionato
                object selectedObject = dbTreeView.SelectedNode.Tag;
                object parrentSelectedObject = dbTreeView.SelectedNode.Parent.Tag;          
                switch (action)
                {
                    case "AddTable":
                        if (selectedObject is Database selectedDbForTable)
                        {
                            // Logica per aggiungere una nuova tabella
                            AddTableToDatabase(selectedDbForTable);
                        }
                        break;
                    case "AddField":
                        if (selectedObject is Table selectedTableForField)
                        {
                            // Logica per aggiungere un nuovo campo
                            AddFieldToTable(selectedTableForField);
                        }
                        break;
                    case "Delete":
                        // Logica per eliminare l'elemento selezionato
                        DeleteSelectedItem(selectedObject, dbTreeView.SelectedNode);
                        break;
                    // Aggiungi altri casi per altre azioni del menu
                    case "Modify":
                        {

                            if (parrentSelectedObject is Table parrentSelectedTableForField)
                            {
                                if (selectedObject is Field selectedTableForFieldM)
                                {
                                    ModifySelectedItem(selectedTableForFieldM, parrentSelectedTableForField);
                                }
                            }


                            //if (selectedObject is Field selectedTableForFieldM)
                            //{
                            //    ModifySelectedItem(selectedTableForFieldM);
                            //}
                        }
                        break;
                }
            }
        }
        /// <summary>
        /// Gestione context menu analisi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmSpeedAnalysis_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            Table esplorazioneMIUTable = currentDatabase.Tables.FirstOrDefault(t => t.TableName == "EsplorazioneMIU");
            Dictionary<(string iniziale, string target), TempoAnalisiRisultati> risultatiAnalisi = DataAnalysis.AnalizzaTempi(esplorazioneMIUTable.DataRecords);
            foreach (var entry in risultatiAnalisi)
            {
                var coppiaStringhe = entry.Key;
                var statistiche = entry.Value;
                sb.Append(($"Analisi per: Iniziale='{coppiaStringhe.iniziale}', Target='{coppiaStringhe.target}'") + Environment.NewLine);
                sb.Append($"  Min Tempo: {statistiche.Min?.ToString("F2") ?? "N/A"} ms" + Environment.NewLine);
                sb.Append($"  Max Tempo: {statistiche.Max?.ToString("F2") ?? "N/A"} ms" + Environment.NewLine);
                sb.Append($"  Media Tempo: {statistiche.Media?.ToString("F2") ?? "N/A"} ms" + Environment.NewLine);
                sb.Append($"  Dev. Std: {statistiche.DeviazioneStandard?.ToString("F2") ?? "N/A"} ms" + Environment.NewLine);
                // Qui puoi aggiungere le righe alla tua griglia nel form di analisi
                //Console.WriteLine($"Analisi per: Iniziale='{coppiaStringhe.iniziale}', Target='{coppiaStringhe.target}'");
                //Console.WriteLine($"  Min Tempo: {statistiche.Min?.ToString("F2") ?? "N/A"} ms");
                //Console.WriteLine($"  Max Tempo: {statistiche.Max?.ToString("F2") ?? "N/A"} ms");
                //Console.WriteLine($"  Media Tempo: {statistiche.Media?.ToString("F2") ?? "N/A"} ms");
                //Console.WriteLine($"  Dev. Std: {statistiche.DeviazioneStandard?.ToString("F2") ?? "N/A"} ms");
            }
            FrmOutput outputForm = new FrmOutput(sb.ToString());
            outputForm.Show();

        }

        #endregion
        #region Async socket serve events
        private void Asl_ErrorFromSocket(object sender, string e)
        {
            evolutionMonitor.AppendText(e);
            _logger.Log(LogLevel.ERROR, e);
        }

        private void Asl_DataFromSocket(object sender, SocketManagerInfo.SocketMessageStructure e)
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
            }catch(Exception ex)
            {
                string errMsg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, errMsg);
            }
        }
        #endregion
    }
}
