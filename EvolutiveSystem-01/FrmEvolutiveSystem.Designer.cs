namespace EvolutiveSystem_01
{
    partial class FrmEvolutiveSystem
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmEvolutiveSystem));
            this.panelCommands = new System.Windows.Forms.Panel();
            this.gbFileManager = new System.Windows.Forms.GroupBox();
            this.btnRicaricaDB = new System.Windows.Forms.Button();
            this.btnCloseAllDatabases = new System.Windows.Forms.Button();
            this.btnLoadDatabase = new System.Windows.Forms.Button();
            this.btnSaveDatabase = new System.Windows.Forms.Button();
            this.btnAddDatabase = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripStatusLabelDen = new System.Windows.Forms.ToolStripLabel();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripLabel();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.splitContainerLeft = new System.Windows.Forms.SplitContainer();
            this.dbTreeView = new System.Windows.Forms.TreeView();
            this.listViewFields = new System.Windows.Forms.ListView();
            this.evolutionMonitor = new System.Windows.Forms.RichTextBox();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.lblTblName = new System.Windows.Forms.Label();
            this.lblDbName = new System.Windows.Forms.Label();
            this.lblDenTblName = new System.Windows.Forms.Label();
            this.lblDenDbName = new System.Windows.Forms.Label();
            this.tabControlDetails = new System.Windows.Forms.TabControl();
            this.CmdStructDb = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.treeViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AnalysisContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmSpeedAnalysis = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmAnalysisClose = new System.Windows.Forms.ToolStripMenuItem();
            this.gbAnalysis = new System.Windows.Forms.GroupBox();
            this.btnAnalysis = new System.Windows.Forms.Button();
            this.gbSocketServer = new System.Windows.Forms.GroupBox();
            this.btnSocket = new System.Windows.Forms.Button();
            this.gbServiceManager = new System.Windows.Forms.GroupBox();
            this.btnServiceStop = new System.Windows.Forms.Button();
            this.btnServicePause = new System.Windows.Forms.Button();
            this.btnServiceStart = new System.Windows.Forms.Button();
            this.panelCommands.SuspendLayout();
            this.gbFileManager.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).BeginInit();
            this.splitContainerLeft.Panel1.SuspendLayout();
            this.splitContainerLeft.Panel2.SuspendLayout();
            this.splitContainerLeft.SuspendLayout();
            this.pnlDetail.SuspendLayout();
            this.tabControlDetails.SuspendLayout();
            this.AnalysisContextMenu.SuspendLayout();
            this.gbAnalysis.SuspendLayout();
            this.gbSocketServer.SuspendLayout();
            this.gbServiceManager.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelCommands
            // 
            this.panelCommands.Controls.Add(this.gbAnalysis);
            this.panelCommands.Controls.Add(this.gbSocketServer);
            this.panelCommands.Controls.Add(this.gbServiceManager);
            this.panelCommands.Controls.Add(this.gbFileManager);
            this.panelCommands.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCommands.Location = new System.Drawing.Point(0, 0);
            this.panelCommands.Name = "panelCommands";
            this.panelCommands.Size = new System.Drawing.Size(1646, 94);
            this.panelCommands.TabIndex = 0;
            // 
            // gbFileManager
            // 
            this.gbFileManager.Controls.Add(this.btnRicaricaDB);
            this.gbFileManager.Controls.Add(this.btnCloseAllDatabases);
            this.gbFileManager.Controls.Add(this.btnLoadDatabase);
            this.gbFileManager.Controls.Add(this.btnSaveDatabase);
            this.gbFileManager.Controls.Add(this.btnAddDatabase);
            this.gbFileManager.Dock = System.Windows.Forms.DockStyle.Left;
            this.gbFileManager.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbFileManager.Location = new System.Drawing.Point(0, 0);
            this.gbFileManager.Name = "gbFileManager";
            this.gbFileManager.Size = new System.Drawing.Size(400, 94);
            this.gbFileManager.TabIndex = 4;
            this.gbFileManager.TabStop = false;
            this.gbFileManager.Text = "File";
            // 
            // btnRicaricaDB
            // 
            this.btnRicaricaDB.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.button_reload_15002;
            this.btnRicaricaDB.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRicaricaDB.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnRicaricaDB.Location = new System.Drawing.Point(299, 18);
            this.btnRicaricaDB.Name = "btnRicaricaDB";
            this.btnRicaricaDB.Size = new System.Drawing.Size(74, 73);
            this.btnRicaricaDB.TabIndex = 8;
            this.btnRicaricaDB.UseVisualStyleBackColor = true;
            // 
            // btnCloseAllDatabases
            // 
            this.btnCloseAllDatabases.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.businessapplication_database_database_accepteitheracceptthedatabase_connect_connectdatabase_negocios_aplicacion_basededato_2310__1_;
            this.btnCloseAllDatabases.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCloseAllDatabases.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnCloseAllDatabases.Location = new System.Drawing.Point(225, 18);
            this.btnCloseAllDatabases.Name = "btnCloseAllDatabases";
            this.btnCloseAllDatabases.Size = new System.Drawing.Size(74, 73);
            this.btnCloseAllDatabases.TabIndex = 7;
            this.btnCloseAllDatabases.UseVisualStyleBackColor = true;
            // 
            // btnLoadDatabase
            // 
            this.btnLoadDatabase.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.business_application_download_downloaddatabase_thedatabase_23201;
            this.btnLoadDatabase.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLoadDatabase.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnLoadDatabase.Location = new System.Drawing.Point(151, 18);
            this.btnLoadDatabase.Name = "btnLoadDatabase";
            this.btnLoadDatabase.Size = new System.Drawing.Size(74, 73);
            this.btnLoadDatabase.TabIndex = 6;
            this.btnLoadDatabase.UseVisualStyleBackColor = true;
            // 
            // btnSaveDatabase
            // 
            this.btnSaveDatabase.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.businessapplication_database_loaddatabase_db_negocios_aplicacion_2318;
            this.btnSaveDatabase.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSaveDatabase.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSaveDatabase.Location = new System.Drawing.Point(77, 18);
            this.btnSaveDatabase.Name = "btnSaveDatabase";
            this.btnSaveDatabase.Size = new System.Drawing.Size(74, 73);
            this.btnSaveDatabase.TabIndex = 5;
            this.btnSaveDatabase.UseVisualStyleBackColor = true;
            // 
            // btnAddDatabase
            // 
            this.btnAddDatabase.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.business_application_addthedatabase_add_insert_database_db_2313;
            this.btnAddDatabase.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddDatabase.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnAddDatabase.Location = new System.Drawing.Point(3, 18);
            this.btnAddDatabase.Name = "btnAddDatabase";
            this.btnAddDatabase.Size = new System.Drawing.Size(74, 73);
            this.btnAddDatabase.TabIndex = 4;
            this.btnAddDatabase.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelDen,
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 872);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1646, 26);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "toolStrip1";
            // 
            // toolStripStatusLabelDen
            // 
            this.toolStripStatusLabelDen.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabelDen.ForeColor = System.Drawing.Color.Blue;
            this.toolStripStatusLabelDen.Name = "toolStripStatusLabelDen";
            this.toolStripStatusLabelDen.Size = new System.Drawing.Size(148, 23);
            this.toolStripStatusLabelDen.Text = "Messaggi di stato:";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabel.ForeColor = System.Drawing.Color.Green;
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(22, 23);
            this.toolStripStatusLabel.Text = "...";
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 94);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.splitContainerLeft);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.BackColor = System.Drawing.Color.GhostWhite;
            this.splitContainerMain.Panel2.Controls.Add(this.tabControlDetails);
            this.splitContainerMain.Size = new System.Drawing.Size(1646, 778);
            this.splitContainerMain.SplitterDistance = 548;
            this.splitContainerMain.TabIndex = 2;
            // 
            // splitContainerLeft
            // 
            this.splitContainerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerLeft.Location = new System.Drawing.Point(0, 0);
            this.splitContainerLeft.Name = "splitContainerLeft";
            this.splitContainerLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerLeft.Panel1
            // 
            this.splitContainerLeft.Panel1.BackColor = System.Drawing.Color.Azure;
            this.splitContainerLeft.Panel1.Controls.Add(this.dbTreeView);
            // 
            // splitContainerLeft.Panel2
            // 
            this.splitContainerLeft.Panel2.BackColor = System.Drawing.Color.MintCream;
            this.splitContainerLeft.Panel2.Controls.Add(this.listViewFields);
            this.splitContainerLeft.Panel2.Controls.Add(this.evolutionMonitor);
            this.splitContainerLeft.Panel2.Controls.Add(this.pnlDetail);
            this.splitContainerLeft.Size = new System.Drawing.Size(548, 778);
            this.splitContainerLeft.SplitterDistance = 387;
            this.splitContainerLeft.TabIndex = 0;
            // 
            // dbTreeView
            // 
            this.dbTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dbTreeView.Location = new System.Drawing.Point(0, 0);
            this.dbTreeView.Name = "dbTreeView";
            this.dbTreeView.Size = new System.Drawing.Size(548, 387);
            this.dbTreeView.TabIndex = 0;
            this.dbTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.dbTreeView_AfterSelect);
            this.dbTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.dbTreeView_NodeMouseClick);
            // 
            // listViewFields
            // 
            this.listViewFields.AllowColumnReorder = true;
            this.listViewFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewFields.FullRowSelect = true;
            this.listViewFields.GridLines = true;
            this.listViewFields.HideSelection = false;
            this.listViewFields.LabelEdit = true;
            this.listViewFields.Location = new System.Drawing.Point(0, 64);
            this.listViewFields.Name = "listViewFields";
            this.listViewFields.Size = new System.Drawing.Size(548, 197);
            this.listViewFields.TabIndex = 4;
            this.listViewFields.UseCompatibleStateImageBehavior = false;
            this.listViewFields.View = System.Windows.Forms.View.Details;
            // 
            // evolutionMonitor
            // 
            this.evolutionMonitor.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.evolutionMonitor.Location = new System.Drawing.Point(0, 261);
            this.evolutionMonitor.Name = "evolutionMonitor";
            this.evolutionMonitor.Size = new System.Drawing.Size(548, 126);
            this.evolutionMonitor.TabIndex = 3;
            this.evolutionMonitor.Text = "";
            // 
            // pnlDetail
            // 
            this.pnlDetail.Controls.Add(this.lblTblName);
            this.pnlDetail.Controls.Add(this.lblDbName);
            this.pnlDetail.Controls.Add(this.lblDenTblName);
            this.pnlDetail.Controls.Add(this.lblDenDbName);
            this.pnlDetail.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDetail.Location = new System.Drawing.Point(0, 0);
            this.pnlDetail.Name = "pnlDetail";
            this.pnlDetail.Size = new System.Drawing.Size(548, 64);
            this.pnlDetail.TabIndex = 0;
            // 
            // lblTblName
            // 
            this.lblTblName.AutoSize = true;
            this.lblTblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTblName.ForeColor = System.Drawing.Color.SeaGreen;
            this.lblTblName.Location = new System.Drawing.Point(162, 35);
            this.lblTblName.Name = "lblTblName";
            this.lblTblName.Size = new System.Drawing.Size(27, 25);
            this.lblTblName.TabIndex = 4;
            this.lblTblName.Text = "...";
            // 
            // lblDbName
            // 
            this.lblDbName.AutoSize = true;
            this.lblDbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDbName.ForeColor = System.Drawing.Color.Green;
            this.lblDbName.Location = new System.Drawing.Point(162, 7);
            this.lblDbName.Name = "lblDbName";
            this.lblDbName.Size = new System.Drawing.Size(27, 25);
            this.lblDbName.TabIndex = 3;
            this.lblDbName.Text = "...";
            // 
            // lblDenTblName
            // 
            this.lblDenTblName.AutoSize = true;
            this.lblDenTblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenTblName.ForeColor = System.Drawing.Color.MediumBlue;
            this.lblDenTblName.Location = new System.Drawing.Point(27, 35);
            this.lblDenTblName.Name = "lblDenTblName";
            this.lblDenTblName.Size = new System.Drawing.Size(132, 25);
            this.lblDenTblName.TabIndex = 2;
            this.lblDenTblName.Text = "Nome tabella:";
            // 
            // lblDenDbName
            // 
            this.lblDenDbName.AutoSize = true;
            this.lblDenDbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenDbName.ForeColor = System.Drawing.Color.MediumBlue;
            this.lblDenDbName.Location = new System.Drawing.Point(3, 7);
            this.lblDenDbName.Name = "lblDenDbName";
            this.lblDenDbName.Size = new System.Drawing.Size(156, 25);
            this.lblDenDbName.TabIndex = 1;
            this.lblDenDbName.Text = "Database name:";
            // 
            // tabControlDetails
            // 
            this.tabControlDetails.Controls.Add(this.CmdStructDb);
            this.tabControlDetails.Controls.Add(this.tabPage2);
            this.tabControlDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlDetails.Location = new System.Drawing.Point(0, 0);
            this.tabControlDetails.Name = "tabControlDetails";
            this.tabControlDetails.SelectedIndex = 0;
            this.tabControlDetails.Size = new System.Drawing.Size(1094, 778);
            this.tabControlDetails.TabIndex = 0;
            this.tabControlDetails.SelectedIndexChanged += new System.EventHandler(this.tabControlDetails_SelectedIndexChanged);
            // 
            // CmdStructDb
            // 
            this.CmdStructDb.Location = new System.Drawing.Point(4, 25);
            this.CmdStructDb.Name = "CmdStructDb";
            this.CmdStructDb.Padding = new System.Windows.Forms.Padding(3);
            this.CmdStructDb.Size = new System.Drawing.Size(1086, 749);
            this.CmdStructDb.TabIndex = 0;
            this.CmdStructDb.Text = "tabPage1";
            this.CmdStructDb.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1086, 744);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // treeViewContextMenu
            // 
            this.treeViewContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.treeViewContextMenu.Name = "treeViewContextMenu";
            this.treeViewContextMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // AnalysisContextMenu
            // 
            this.AnalysisContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.AnalysisContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmSpeedAnalysis,
            this.toolStripSeparator1,
            this.tsmAnalysisClose});
            this.AnalysisContextMenu.Name = "contextMenuStrip1";
            this.AnalysisContextMenu.Size = new System.Drawing.Size(257, 58);
            // 
            // tsmSpeedAnalysis
            // 
            this.tsmSpeedAnalysis.Name = "tsmSpeedAnalysis";
            this.tsmSpeedAnalysis.Size = new System.Drawing.Size(256, 24);
            this.tsmSpeedAnalysis.Text = "Analisi flutuazione velocità";
            this.tsmSpeedAnalysis.Click += new System.EventHandler(this.tsmSpeedAnalysis_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(253, 6);
            // 
            // tsmAnalysisClose
            // 
            this.tsmAnalysisClose.Name = "tsmAnalysisClose";
            this.tsmAnalysisClose.Size = new System.Drawing.Size(256, 24);
            this.tsmAnalysisClose.Text = "Close";
            // 
            // gbAnalysis
            // 
            this.gbAnalysis.Controls.Add(this.btnAnalysis);
            this.gbAnalysis.Dock = System.Windows.Forms.DockStyle.Left;
            this.gbAnalysis.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbAnalysis.Location = new System.Drawing.Point(767, 0);
            this.gbAnalysis.Name = "gbAnalysis";
            this.gbAnalysis.Size = new System.Drawing.Size(136, 94);
            this.gbAnalysis.TabIndex = 13;
            this.gbAnalysis.TabStop = false;
            this.gbAnalysis.Text = "Analisi dati";
            // 
            // btnAnalysis
            // 
            this.btnAnalysis.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.data_analysis_icon_icons_com_52842;
            this.btnAnalysis.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAnalysis.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnAnalysis.Location = new System.Drawing.Point(3, 18);
            this.btnAnalysis.Name = "btnAnalysis";
            this.btnAnalysis.Size = new System.Drawing.Size(74, 73);
            this.btnAnalysis.TabIndex = 5;
            this.btnAnalysis.UseVisualStyleBackColor = true;
            // 
            // gbSocketServer
            // 
            this.gbSocketServer.Controls.Add(this.btnSocket);
            this.gbSocketServer.Dock = System.Windows.Forms.DockStyle.Left;
            this.gbSocketServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSocketServer.Location = new System.Drawing.Point(631, 0);
            this.gbSocketServer.Name = "gbSocketServer";
            this.gbSocketServer.Size = new System.Drawing.Size(136, 94);
            this.gbSocketServer.TabIndex = 12;
            this.gbSocketServer.TabStop = false;
            this.gbSocketServer.Text = "Socket client";
            // 
            // btnSocket
            // 
            this.btnSocket.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.connect_23039;
            this.btnSocket.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSocket.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSocket.Location = new System.Drawing.Point(3, 18);
            this.btnSocket.Name = "btnSocket";
            this.btnSocket.Size = new System.Drawing.Size(74, 73);
            this.btnSocket.TabIndex = 5;
            this.btnSocket.UseVisualStyleBackColor = true;
            // 
            // gbServiceManager
            // 
            this.gbServiceManager.Controls.Add(this.btnServiceStop);
            this.gbServiceManager.Controls.Add(this.btnServicePause);
            this.gbServiceManager.Controls.Add(this.btnServiceStart);
            this.gbServiceManager.Dock = System.Windows.Forms.DockStyle.Left;
            this.gbServiceManager.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbServiceManager.Location = new System.Drawing.Point(400, 0);
            this.gbServiceManager.Name = "gbServiceManager";
            this.gbServiceManager.Size = new System.Drawing.Size(231, 94);
            this.gbServiceManager.TabIndex = 11;
            this.gbServiceManager.TabStop = false;
            this.gbServiceManager.Text = "Service manager";
            // 
            // btnServiceStop
            // 
            this.btnServiceStop.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.Stop1Hot_26966;
            this.btnServiceStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnServiceStop.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnServiceStop.Location = new System.Drawing.Point(151, 18);
            this.btnServiceStop.Name = "btnServiceStop";
            this.btnServiceStop.Size = new System.Drawing.Size(80, 73);
            this.btnServiceStop.TabIndex = 7;
            this.btnServiceStop.UseVisualStyleBackColor = true;
            // 
            // btnServicePause
            // 
            this.btnServicePause.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.PausePressed_26932;
            this.btnServicePause.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnServicePause.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnServicePause.Location = new System.Drawing.Point(77, 18);
            this.btnServicePause.Name = "btnServicePause";
            this.btnServicePause.Size = new System.Drawing.Size(74, 73);
            this.btnServicePause.TabIndex = 6;
            this.btnServicePause.UseVisualStyleBackColor = true;
            // 
            // btnServiceStart
            // 
            this.btnServiceStart.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.start256_24877;
            this.btnServiceStart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnServiceStart.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnServiceStart.Location = new System.Drawing.Point(3, 18);
            this.btnServiceStart.Name = "btnServiceStart";
            this.btnServiceStart.Size = new System.Drawing.Size(74, 73);
            this.btnServiceStart.TabIndex = 5;
            this.btnServiceStart.UseVisualStyleBackColor = true;
            // 
            // FrmEvolutiveSystem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1646, 898);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.panelCommands);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmEvolutiveSystem";
            this.Text = "Evolutive DB - test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmEvolutiveSystem_FormClosing);
            this.Load += new System.EventHandler(this.FrmEvolutiveSystem_Load);
            this.panelCommands.ResumeLayout(false);
            this.gbFileManager.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainerLeft.Panel1.ResumeLayout(false);
            this.splitContainerLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).EndInit();
            this.splitContainerLeft.ResumeLayout(false);
            this.pnlDetail.ResumeLayout(false);
            this.pnlDetail.PerformLayout();
            this.tabControlDetails.ResumeLayout(false);
            this.AnalysisContextMenu.ResumeLayout(false);
            this.gbAnalysis.ResumeLayout(false);
            this.gbSocketServer.ResumeLayout(false);
            this.gbServiceManager.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelCommands;
        private System.Windows.Forms.ToolStrip statusStrip;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.SplitContainer splitContainerLeft;
        private System.Windows.Forms.TreeView dbTreeView;
        private System.Windows.Forms.Panel pnlDetail;
        private System.Windows.Forms.Label lblDenDbName;
        private System.Windows.Forms.Label lblDenTblName;
        private System.Windows.Forms.Label lblDbName;
        private System.Windows.Forms.Label lblTblName;
        private System.Windows.Forms.TabControl tabControlDetails;
        private System.Windows.Forms.TabPage CmdStructDb;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ContextMenuStrip treeViewContextMenu;
        private System.Windows.Forms.ToolStripLabel toolStripStatusLabelDen;
        private System.Windows.Forms.ToolStripLabel toolStripStatusLabel;
        private System.Windows.Forms.RichTextBox evolutionMonitor;
        private System.Windows.Forms.ListView listViewFields;
        private System.Windows.Forms.GroupBox gbFileManager;
        private System.Windows.Forms.Button btnCloseAllDatabases;
        private System.Windows.Forms.Button btnLoadDatabase;
        private System.Windows.Forms.Button btnSaveDatabase;
        private System.Windows.Forms.Button btnAddDatabase;
        private System.Windows.Forms.Button btnRicaricaDB;
        private System.Windows.Forms.ContextMenuStrip AnalysisContextMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmSpeedAnalysis;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsmAnalysisClose;
        private System.Windows.Forms.GroupBox gbAnalysis;
        private System.Windows.Forms.Button btnAnalysis;
        private System.Windows.Forms.GroupBox gbSocketServer;
        private System.Windows.Forms.Button btnSocket;
        private System.Windows.Forms.GroupBox gbServiceManager;
        private System.Windows.Forms.Button btnServiceStop;
        private System.Windows.Forms.Button btnServicePause;
        private System.Windows.Forms.Button btnServiceStart;
    }
}

