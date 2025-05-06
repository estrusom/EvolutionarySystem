using EvolutiveSystem;
using EvolutiveSystem.SemanticData;
using EvolutiveSystem.Serialization;
using EvolutiveSystem_01.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ToolTip = System.Windows.Forms.ToolTip;

namespace EvolutiveSystem_01
{
    public partial class FrmEvolutiveSystem : Form
    {

        // La collezione di database (semantiche) gestiti dall'UI.
        private List<Database> loadedDatabases = new List<Database>();
        // Riferimento al database attualmente selezionato o attivo nell'UI
        private Database currentDatabase;
        private ToolTip toolTip;
        public FrmEvolutiveSystem()
        {
            InitializeComponent();
            this.InitializeCustomLogic();
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
            // btnStopProcess.Enabled = false;
            // btnPauseProcess.Enabled = false;
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
            //if (btnStartProcess != null) btnStartProcess.Click += btnStartProcess_Click;
            //if (btnStopProcess != null) btnStopProcess.Click += btnStopProcess_Click;
            //if (btnPauseProcess != null) btnPauseProcess.Click += btnPauseProcess_Click;


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

                treeViewContextMenu.Items.Add(new ToolStripSeparator()); // Separatore

                ToolStripItem deleteItem = treeViewContextMenu.Items.Add("Elimina");
                deleteItem.Tag = "Delete";
                deleteItem.Click += ContextMenuItem_Click;

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
            ulong registryValue = 0; // Placeholder
            object fieldValue = null; // Placeholder

            // Genera un ID semplice per il nuovo campo
            int newFieldId = targetTable.Fields.Count > 0 ? targetTable.Fields.Max(f => f.Id) + 1 : 1;

            // Crea la nuova istanza della classe Field
            Field newField = new Field(newFieldId, newFieldName, newDataType, isKey, isEncrypted, registryValue, targetTable, fieldValue);
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
        /// <summary>
        /// Aggiunge una TabPage per mostrare i *dati* (record) di una Tabella in un DataGridView.
        /// Questa tab viene creata solo quando si seleziona una Tabella.
        /// </summary>
        /// <param name="table">La Tabella i cui dati devono essere visualizzati.</param>
        private void AddTableDataRecordsTab(Table table)
        {
            if (tabControlDetails == null || table == null) return;

            // Controlla se esiste già una TabPage per i dati di questa tabella
            TabPage existingPage = null;
            foreach (TabPage page in tabControlDetails.TabPages)
            {
                if (page.Tag is Tuple<Table, string> pageTag && pageTag.Item1 == table && pageTag.Item2 == "DataRecords")
                {
                    existingPage = page;
                    break;
                }
            }

            if (existingPage != null)
            {
                // Se la pagina esiste, selezionala
                tabControlDetails.SelectedTab = existingPage;
                return; // Non creare una nuova pagina
            }


            // Crea una nuova TabPage per i dati dei record
            TabPage dataRecordsTabPage = new TabPage($"Dati Record: {table.TableName}");
            // Usiamo una Tuple nel Tag per identificare sia la tabella che il tipo di tab
            dataRecordsTabPage.Tag = new Tuple<Table, string>(table, "DataRecords");


            // Crea un nuovo DataGridView per i dati
            DataGridView dgvDataRecords = new DataGridView();
            dgvDataRecords.Dock = DockStyle.Fill; // Riempi la TabPage
            dgvDataRecords.AutoGenerateColumns = false; // Non generare colonne automaticamente
            dgvDataRecords.AllowUserToAddRows = true; // Permetti l'aggiunta di nuove righe
            dgvDataRecords.AllowUserToDeleteRows = true; // Permetti l'eliminazione di righe
            dgvDataRecords.ReadOnly = false; // Permetti la modifica dei dati
            dgvDataRecords.Tag = table; // Associa la tabella al DataGridView

            // Configura le colonne del DataGridView in base ai campi della tabella
            ConfigureDataRecordsDataGridViewColumns(dgvDataRecords, table.Fields);

            // Popola il DataGridView con i dati dei record della tabella
            // *** Nota: La classe Table ha ora una struttura per i "record" (DataRecords). ***
            // *** Dovrai implementare PopulateDataRecordsDataGridView per legare questa struttura al DataGridView. ***
            PopulateDataRecordsDataGridView(dgvDataRecords, table.DataRecords);

            // Aggiungi il DataGridView alla TabPage
            dataRecordsTabPage.Controls.Add(dgvDataRecords);

            // Aggiungi la TabPage al TabControl
            tabControlDetails.TabPages.Add(dataRecordsTabPage);

            // Seleziona la nuova TabPage
            tabControlDetails.SelectedTab = dataRecordsTabPage;

            // *** Aggiunto: Associa il gestore eventi per la validazione della riga ***
            // Questo evento si verifica dopo che l'utente ha finito di modificare una riga (inclusa una nuova riga)
            // e i valori sono stati committati al DataSource.
            dgvDataRecords.RowValidated += DgvDataRecords_RowValidated;

            // *** Qui potresti associare eventi al DataGridView per gestire modifiche, aggiunte, eliminazioni dei DATI ***
            // dgvDataRecords.CellValueChanged += dgvDataRecords_CellValueChanged;
            // dgvDataRecords.UserAddedRow += DgvDataRecords_UserAddedRow; 06/05/25 21:25
            // dgvDataRecords.UserDeletedRow += dgvDataRecords_UserDeletedRow;
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
        /*
        /// <summary>
        /// Popola un DataGridView con i *dati* (record) forniti.
        /// </summary>
        /// <param name="dgv">Il DataGridView da popolare.</param>
        /// <param name="dataRecords">La lista di record da visualizzare (List<Dictionary<string, object>>).</param>
        private void PopulateDataRecordsDataGridView(DataGridView dgv, List<Dictionary<string, object>> dataRecords)
        {
            if (dgv == null || dataRecords == null) return;

            // Usa un BindingList per permettere al DataGridView di rilevare automaticamente le modifiche
            // BindingList<Dictionary<string, object>> non funziona direttamente per il data binding automatico
            // delle colonne con DataPropertyName.
            // Un DataTable è spesso più adatto per legare List<Dictionary<string, object>> a un DataGridView.

            DataTable dt = new DataTable();

            // Crea le colonne del DataTable in base ai campi della tabella (se non già fatto)
            if (dgv.Columns.Count > 0 && dt.Columns.Count == 0)
            {
                foreach (DataGridViewColumn dgvCol in dgv.Columns)
                {
                    dt.Columns.Add(dgvCol.Name, typeof(object)); // Usa object per flessibilità, gestisci tipi specifici in CellFormatting/Parsing
                }
            }

            // Popola il DataTable con i dati dai record
            foreach (var record in dataRecords)
            {
                DataRow row = dt.NewRow();
                foreach (var field in record)
                {
                    // Assicurati che la colonna esista prima di assegnare il valore
                    if (dt.Columns.Contains(field.Key))
                    {
                        row[field.Key] = field.Value ?? DBNull.Value; // Gestisci valori null
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
        */
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

                // Imposta la visibilità e l'abilitazione in base al tipo di oggetto selezionato
                if (selectedObject is Database)
                {
                    if (addTableMenuItem != null) addTableMenuItem.Visible = true; // Puoi aggiungere una tabella a un database
                    if (addFieldMenuItem != null) addFieldMenuItem.Visible = false; // Non puoi aggiungere un campo a un database
                    if (deleteItem != null) deleteItem.Visible = true; // Puoi eliminare un database
                }
                else if (selectedObject is Table)
                {
                    if (addTableMenuItem != null) addTableMenuItem.Visible = false; // Non puoi aggiungere una tabella a una tabella
                    if (addFieldMenuItem != null) addFieldMenuItem.Visible = true; // Puoi aggiungere un campo a una tabella
                    if (deleteItem != null) deleteItem.Visible = true; // Puoi eliminare una tabella
                }
                else if (selectedObject is Field)
                {
                    if (addTableMenuItem != null) addTableMenuItem.Visible = false; // Non puoi aggiungere una tabella a un campo
                    if (addFieldMenuItem != null) addFieldMenuItem.Visible = false; // Non puoi aggiungere un campo a un campo
                    if (deleteItem != null) deleteItem.Visible = true; // Puoi eliminare un campo
                }
                else
                {
                    // Nessuna azione disponibile per nodi sconosciuti
                    if (addTableMenuItem != null) addTableMenuItem.Visible = false;
                    if (addFieldMenuItem != null) addFieldMenuItem.Visible = false;
                    if (deleteItem != null) deleteItem.Visible = false;
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
        #endregion
        #region Datagreedview events
        private void DgvDataRecords_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            // Recupera il DataGridView che ha scatenato l'evento
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;

            // Recupera la tabella associata a questo DataGridView (l'abbiamo salvata nel Tag)
            Table table = dgv.Tag as Table;
            if (table == null)
            {
                AppendToMonitor("Errore: Impossibile trovare la tabella associata al DataGridView nella validazione riga.");
                UpdateStatus("Errore: Tabella non trovata (validazione riga).");
                return;
            }

            // *** Modificato: Controlla se la riga convalidata NON è la riga per i nuovi record (IsNewRow)
            // e se il suo indice è valido e non è l'ultima riga del DGV (che è la nuova riga vuota) ***
            // Quando una nuova riga viene convalidata, diventa una riga di dati regolare,
            // e una nuova riga IsNewRow viene aggiunta in fondo.
            // Quindi, la riga convalidata che ci interessa non avrà IsNewRow = true.
            // Il suo indice sarà l'ultimo indice valido *prima* della riga IsNewRow.
            if (e.RowIndex >= 0 && e.RowIndex < dgv.Rows.Count && !dgv.Rows[e.RowIndex].IsNewRow)
            {
                // Questa condizione si verifica per qualsiasi riga convalidata che non sia la riga "new row"
                // (sia una riga appena aggiunta che una riga esistente modificata).

                // Per distinguere una riga appena aggiunta da una modificata, potremmo aver bisogno
                // di un flag o di confrontare con i dati esistenti nella lista DataRecords.
                // Tuttavia, nel caso più semplice di sola aggiunta, la riga convalidata
                // che non è IsNewRow e il cui indice è l'ultimo valido nel DataTable
                // (che corrisponde a dgv.Rows.Count - 2 se AllowUserToAddRows è true)
                // è la riga appena aggiunta.

                // Recuperiamo il DataTable legato al DataGridView.
                DataTable dt = dgv.DataSource as DataTable;
                if (dt == null)
                {
                    AppendToMonitor("Errore: Impossibile trovare il DataTable associato al DataGridView nella validazione riga.");
                    UpdateStatus("Errore: DataTable non trovato (validazione riga).");
                    return;
                }

                // Controlla se la riga convalidata corrisponde all'ultima riga di dati nel DataTable.
                // Se l'utente ha appena aggiunto una riga, l'indice della riga convalidata (e.RowIndex)
                // nel DGV corrisponderà all'indice dell'ultima riga nel DataTable (dt.Rows.Count - 1).
                if (e.RowIndex == dt.Rows.Count - 1)
                {
                    DataRow validatedDataRow = dt.Rows[e.RowIndex];

                    // Crea un nuovo SerializableDictionary per il nuovo record
                    SerializableDictionary<string, object> newRecord = new SerializableDictionary<string, object>();

                    // Popola il nuovo record con i valori dalla riga convalidata del DataTable
                    foreach (DataColumn col in dt.Columns)
                    {
                        string fieldName = col.ColumnName;
                        // Recupera il valore dalla cella corrispondente nel DataRow. Gestisci DBNull.Value.
                        object cellValue = validatedDataRow[col.ColumnName];
                        newRecord.Add(fieldName, cellValue == DBNull.Value ? null : cellValue);
                    }

                    // Aggiungi il nuovo record alla lista DataRecords della tabella
                    // NOTA: Stiamo aggiungendo il record alla lista originale della tabella.
                    // Il DataTable è solo una vista per il DataGridView.
                    table.DataRecords.Add(newRecord);

                    AppendToMonitor($"Aggiunto nuovo record alla tabella '{table.TableName}' tramite validazione riga.");
                    UpdateStatus($"Record aggiunto alla tabella '{table.TableName}'.");

                    // Potresti voler segnare il database come modificato qui
                    // isDatabaseModified = true;
                }
                // else
                // {
                //     // Se la riga convalidata non è l'ultima riga del DataTable,
                //     // significa che è stata modificata una riga esistente.
                //     // La logica per gestire la modifica di righe esistenti andrebbe qui.
                //     // AppendToMonitor($"Riga esistente convalidata all'indice {e.RowIndex}. Logica di aggiornamento record necessaria.");
                //     // UpdateStatus($"Riga modificata all'indice {e.RowIndex}.");
                // }
            }
        }
        #endregion
        #region datagridview events
        private void dbTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Recupera l'oggetto dati associato al nodo selezionato tramite il Tag
            object selectedObject = e.Node.Tag;

            // Pulisci le TabPages esistenti nel TabControl
            if (tabControlDetails != null)
            {
                tabControlDetails.TabPages.Clear();
            }

            // Aggiorna i controlli UI in base al tipo di oggetto selezionato
            if (selectedObject is Database db)
            {
                AppendToMonitor($"Selezionato Database: {db.DatabaseName}");
                UpdateStatus($"Database selezionato: {db.DatabaseName}");
                currentDatabase = db; // Imposta il database selezionato come corrente per le operazioni globali
                // *** Aggiunto: Aggiunge una TabPage per ogni tabella del database selezionato ***
                foreach (var table in db.Tables)
                {
                    AddTableDataRecordsTab(table);
                }
                lblDbName.Text = currentDatabase.DatabaseName;
                lblTblName.Text = "";
                // *** Qui potresti aggiungere una TabPage con i dettagli del Database ***
                // Esempio placeholder:
                // AddDatabaseDetailsTab(db);
            }
            else if (selectedObject is Table table)
            {
                AppendToMonitor($"Selezionata Tabella: {table.TableName}");
                UpdateStatus($"Tabella selezionata: {table.TableName}");
                currentDatabase = table.ParentDatabase; // Imposta il database padre della tabella come corrente

                // Popola la ListView con le *proprietà* dei campi di questa tabella
                PopulateFieldsListView(table.Fields);

                // *** Qui potresti aggiungere una TabPage con i dettagli della Tabella ***
                // Esempio placeholder:
                // AddTableDetailsTab(table);

                // Aggiunge una TabPage per mostrare i *dati* (record) della tabella
                AddTableDataRecordsTab(table);
                lblDbName.Text = currentDatabase.DatabaseName;
                lblTblName.Text = table.TableName;
            }
            else if (selectedObject is Field field)
            {
                AppendToMonitor($"Selezionato Campo: {field.FieldName}");
                UpdateStatus($"Campo selezionato: {field.FieldName}");

                Table parentTable = field.ParentTable;
                Database parentDatabase = parentTable?.ParentDatabase;

                // Popola la ListView con le *proprietà* dei campi della tabella madre
                if (parentTable != null)
                {
                    currentDatabase = parentDatabase; // Imposta il database padre del campo come corrente
                    PopulateFieldsListView(parentTable.Fields);
                    // Evidenzia il campo selezionato nella ListView
                    HighlightFieldInListView(field);

                    // Aggiunge una TabPage per mostrare i *dati* (record) della tabella madre
                    AddTableDataRecordsTab(parentTable);
                    lblDbName.Text = currentDatabase.DatabaseName;
                    lblTblName.Text = parentTable.TableName;
                }

                // *** Qui potresti aggiungere una TabPage con i dettagli del Campo ***
                // Esempio placeholder:
                // AddFieldDetailsTab(field);
            }
            else
            {
                AppendToMonitor("Selezionato nodo sconosciuto.");
                UpdateStatus("Elemento sconosciuto selezionato.");
                currentDatabase = null; // Nessun database valido selezionato
                // Pulisci la ListView delle proprietà
                if (listViewFields != null) listViewFields.Items.Clear();
            }

            // Opzionale: Aggiorna un'area di dettaglio separata con tutte le proprietà dell'oggetto selezionato
            // DisplayObjectDetails(selectedObject);
            /*
            // Recupera l'oggetto dati associato al nodo selezionato tramite il Tag
            object selectedObject = e.Node.Tag;

            // Pulisci le TabPages esistenti nel TabControl
            if (tabControlDetails != null)
            {
                tabControlDetails.TabPages.Clear();
            }

            // Aggiorna i controlli UI in base al tipo di oggetto selezionato
            if (selectedObject is Database db)
            {
                AppendToMonitor($"Selezionato Database: {db.DatabaseName}");
                UpdateStatus($"Database selezionato: {db.DatabaseName}");

                // *** Qui potresti aggiungere una TabPage con i dettagli del Database ***
                // Esempio placeholder:
                // AddDatabaseDetailsTab(db);
            }
            else if (selectedObject is Table table)
            {
                AppendToMonitor($"Selezionata Tabella: {table.TableName}");
                UpdateStatus($"Tabella selezionata: {table.TableName}");

                // Popola la ListView con le *proprietà* dei campi di questa tabella
                PopulateFieldsListView(table.Fields);

                // *** Qui potresti aggiungere una TabPage con i dettagli della Tabella ***
                // Esempio placeholder:
                // AddTableDetailsTab(table);

                // Aggiunge una TabPage per mostrare i *dati* (record) della tabella
                AddTableDataRecordsTab(table);

            }
            else if (selectedObject is Field field)
            {
                AppendToMonitor($"Selezionato Campo: {field.FieldName}");
                UpdateStatus($"Campo selezionato: {field.FieldName}");

                // Popola la ListView con le *proprietà* dei campi della tabella madre
                if (field.ParentTable != null)
                {
                    PopulateFieldsListView(field.ParentTable.Fields);
                    // Evidenzia il campo selezionato nella ListView
                    HighlightFieldInListView(field);

                    // Aggiunge una TabPage per mostrare i *dati* (record) della tabella madre
                    AddTableDataRecordsTab(field.ParentTable);
                }

                // *** Qui potresti aggiungere una TabPage con i dettagli del Campo ***
                // Esempio placeholder:
                // AddFieldDetailsTab(field);
            }
            else
            {
                AppendToMonitor("Selezionato nodo sconosciuto.");
                UpdateStatus("Elemento sconosciuto selezionato.");
                // Pulisci la ListView delle proprietà
                if (listViewFields != null) listViewFields.Items.Clear();
            }

            // Opzionale: Aggiorna un'area di dettaglio separata con tutte le proprietà dell'oggetto selezionato
            // DisplayObjectDetails(selectedObject);
            */
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
            /*
            if (currentDatabase == null)
            {
                MessageBox.Show("Nessun database da salvare.", "Salva Database", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatus("Nessun database da salvare.");
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*";
                saveFileDialog.Title = "Salva il Database Semantico come file XML";
                saveFileDialog.FileName = currentDatabase.DatabaseName.Replace(" ", "_") + ".xml"; // Nome file suggerito

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        UpdateStatus($"Salvataggio database in {saveFileDialog.FileName}...");
                        DatabaseSerializer.SerializeToXmlFile(currentDatabase, saveFileDialog.FileName);
                        AppendToMonitor($"Database salvato con successo in {saveFileDialog.FileName}");
                        UpdateStatus("Database salvato.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore durante il salvataggio del database: {ex.Message}", "Errore di Salvataggio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        AppendToMonitor($"Errore durante il salvataggio: {ex.Message}");
                        UpdateStatus($"Errore salvataggio: {ex.Message}");
                    }
                }
            }
            */
        }
        /// <summary>
        /// Gestore events per il pulsante "Carica Database".
        /// Permette all'utente di selezionare un file XML e caricarlo.
        /// </summary>
        private void btnLoadDatabase_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*";
                openFileDialog.Title = "Seleziona un file XML del Database Semantico";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        UpdateStatus($"Caricamento database da {openFileDialog.FileName}...");
                        Database loadedDb = DatabaseSerializer.DeserializeFromXmlFile(openFileDialog.FileName);

                        // *** Aggiunto: Controlla se un database con lo stesso ID o nome esiste già ***
                        if (loadedDatabases.Any(db => db.DatabaseId == loadedDb.DatabaseId || db.DatabaseName.Equals(loadedDb.DatabaseName, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show($"Un database con lo stesso ID ({loadedDb.DatabaseId}) o nome ('{loadedDb.DatabaseName}') è già caricato.", "Database Già Caricato", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            UpdateStatus($"Database '{loadedDb.DatabaseName}' già caricato.");
                            return; // Non aggiungere il duplicato
                        }


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
            /*
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*";
                openFileDialog.Title = "Seleziona un file XML del Database Semantico";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        UpdateStatus($"Caricamento database da {openFileDialog.FileName}...");
                        Database loadedDb = DatabaseSerializer.DeserializeFromXmlFile(openFileDialog.FileName);
                        loadedDatabases.Clear();
                        loadedDatabases.Add(loadedDb);
                        currentDatabase = loadedDb;

                        PopulateDatabaseTreeView(); // Aggiorna la TreeView

                        // Abilita i controlli rilevanti (solo pulsanti non gestiti da ContextMenu)
                        // if (btnSaveDatabase != null) btnSaveDatabase.Enabled = true;

                        // Passa il database caricato al motore semantico (se gestito qui)
                        // semanticEngine.LoadSemanticDatabase(currentDatabase);

                        AppendToMonitor($"Database caricato con successo da {openFileDialog.FileName}");
                        UpdateStatus("Database caricato.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore durante il caricamento del database: {ex.Message}", "Errore di Caricamento", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        AppendToMonitor($"Errore durante il caricamento: {ex.Message}");
                        UpdateStatus($"Errore caricamento: {ex.Message}");
                    }
                }
            }
            */
        }
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
                }
            }
        }
        #endregion
    }
}
