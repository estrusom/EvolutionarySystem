namespace EvolutiveSystem_02
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmEvolutiveSystem));
            this.panelCommands = new System.Windows.Forms.Panel();
            this.gbAnalysis = new System.Windows.Forms.GroupBox();
            this.btnAnalysis = new System.Windows.Forms.Button();
            this.gbSocketServer = new System.Windows.Forms.GroupBox();
            this.btnSocket = new System.Windows.Forms.Button();
            this.gbServiceManager = new System.Windows.Forms.GroupBox();
            this.btnServiceStop = new System.Windows.Forms.Button();
            this.btnServicePause = new System.Windows.Forms.Button();
            this.btnServiceStart = new System.Windows.Forms.Button();
            this.gbTestConfig = new System.Windows.Forms.GroupBox();
            this.btnConfigTest = new System.Windows.Forms.Button();
            this.gbFileManager = new System.Windows.Forms.GroupBox();
            this.btnCloseAllDatabase = new System.Windows.Forms.Button();
            this.btnRicaricaDB = new System.Windows.Forms.Button();
            this.btnSaveDatabase = new System.Windows.Forms.Button();
            this.btnLoadDatabase = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelDen = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripDbDefaulLabelDen = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripDbDefaulLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.spcMain = new System.Windows.Forms.SplitContainer();
            this.spcLeft = new System.Windows.Forms.SplitContainer();
            this.treeViewDatabase = new System.Windows.Forms.TreeView();
            this.listViewFields = new System.Windows.Forms.ListView();
            this.evolutionMonitor = new System.Windows.Forms.RichTextBox();
            this.tabCtrlDBgrid = new System.Windows.Forms.TabControl();
            this.panelCommands.SuspendLayout();
            this.gbAnalysis.SuspendLayout();
            this.gbSocketServer.SuspendLayout();
            this.gbServiceManager.SuspendLayout();
            this.gbTestConfig.SuspendLayout();
            this.gbFileManager.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcMain)).BeginInit();
            this.spcMain.Panel1.SuspendLayout();
            this.spcMain.Panel2.SuspendLayout();
            this.spcMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcLeft)).BeginInit();
            this.spcLeft.Panel1.SuspendLayout();
            this.spcLeft.Panel2.SuspendLayout();
            this.spcLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelCommands
            // 
            this.panelCommands.Controls.Add(this.gbAnalysis);
            this.panelCommands.Controls.Add(this.gbSocketServer);
            this.panelCommands.Controls.Add(this.gbServiceManager);
            this.panelCommands.Controls.Add(this.gbTestConfig);
            this.panelCommands.Controls.Add(this.gbFileManager);
            this.panelCommands.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCommands.Location = new System.Drawing.Point(0, 0);
            this.panelCommands.Name = "panelCommands";
            this.panelCommands.Size = new System.Drawing.Size(1782, 94);
            this.panelCommands.TabIndex = 1;
            // 
            // gbAnalysis
            // 
            this.gbAnalysis.Controls.Add(this.btnAnalysis);
            this.gbAnalysis.Dock = System.Windows.Forms.DockStyle.Left;
            this.gbAnalysis.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbAnalysis.Location = new System.Drawing.Point(833, 0);
            this.gbAnalysis.Name = "gbAnalysis";
            this.gbAnalysis.Size = new System.Drawing.Size(136, 94);
            this.gbAnalysis.TabIndex = 10;
            this.gbAnalysis.TabStop = false;
            this.gbAnalysis.Text = "Analisi dati";
            // 
            // btnAnalysis
            // 
            this.btnAnalysis.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAnalysis.BackgroundImage")));
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
            this.gbSocketServer.Location = new System.Drawing.Point(725, 0);
            this.gbSocketServer.Name = "gbSocketServer";
            this.gbSocketServer.Size = new System.Drawing.Size(108, 94);
            this.gbSocketServer.TabIndex = 9;
            this.gbSocketServer.TabStop = false;
            this.gbSocketServer.Text = "Socket client";
            // 
            // btnSocket
            // 
            this.btnSocket.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSocket.BackgroundImage")));
            this.btnSocket.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSocket.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSocket.Location = new System.Drawing.Point(3, 18);
            this.btnSocket.Name = "btnSocket";
            this.btnSocket.Size = new System.Drawing.Size(74, 73);
            this.btnSocket.TabIndex = 5;
            this.btnSocket.UseVisualStyleBackColor = true;
            this.btnSocket.Click += new System.EventHandler(this.btnSocket_Click);
            // 
            // gbServiceManager
            // 
            this.gbServiceManager.Controls.Add(this.btnServiceStop);
            this.gbServiceManager.Controls.Add(this.btnServicePause);
            this.gbServiceManager.Controls.Add(this.btnServiceStart);
            this.gbServiceManager.Dock = System.Windows.Forms.DockStyle.Left;
            this.gbServiceManager.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbServiceManager.Location = new System.Drawing.Point(483, 0);
            this.gbServiceManager.Name = "gbServiceManager";
            this.gbServiceManager.Size = new System.Drawing.Size(242, 94);
            this.gbServiceManager.TabIndex = 8;
            this.gbServiceManager.TabStop = false;
            this.gbServiceManager.Text = "Service manager";
            // 
            // btnServiceStop
            // 
            this.btnServiceStop.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnServiceStop.BackgroundImage")));
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
            this.btnServicePause.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnServicePause.BackgroundImage")));
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
            this.btnServiceStart.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnServiceStart.BackgroundImage")));
            this.btnServiceStart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnServiceStart.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnServiceStart.Location = new System.Drawing.Point(3, 18);
            this.btnServiceStart.Name = "btnServiceStart";
            this.btnServiceStart.Size = new System.Drawing.Size(74, 73);
            this.btnServiceStart.TabIndex = 5;
            this.btnServiceStart.UseVisualStyleBackColor = true;
            // 
            // gbTestConfig
            // 
            this.gbTestConfig.Controls.Add(this.btnConfigTest);
            this.gbTestConfig.Dock = System.Windows.Forms.DockStyle.Left;
            this.gbTestConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbTestConfig.Location = new System.Drawing.Point(310, 0);
            this.gbTestConfig.Name = "gbTestConfig";
            this.gbTestConfig.Size = new System.Drawing.Size(173, 94);
            this.gbTestConfig.TabIndex = 5;
            this.gbTestConfig.TabStop = false;
            this.gbTestConfig.Text = "Configurazione test";
            // 
            // btnConfigTest
            // 
            this.btnConfigTest.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnConfigTest.BackgroundImage")));
            this.btnConfigTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnConfigTest.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnConfigTest.Location = new System.Drawing.Point(3, 18);
            this.btnConfigTest.Name = "btnConfigTest";
            this.btnConfigTest.Size = new System.Drawing.Size(74, 73);
            this.btnConfigTest.TabIndex = 6;
            this.btnConfigTest.UseVisualStyleBackColor = true;
            this.btnConfigTest.Click += new System.EventHandler(this.btnConfigTest_Click);
            // 
            // gbFileManager
            // 
            this.gbFileManager.Controls.Add(this.btnCloseAllDatabase);
            this.gbFileManager.Controls.Add(this.btnRicaricaDB);
            this.gbFileManager.Controls.Add(this.btnSaveDatabase);
            this.gbFileManager.Controls.Add(this.btnLoadDatabase);
            this.gbFileManager.Dock = System.Windows.Forms.DockStyle.Left;
            this.gbFileManager.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbFileManager.Location = new System.Drawing.Point(0, 0);
            this.gbFileManager.Name = "gbFileManager";
            this.gbFileManager.Size = new System.Drawing.Size(310, 94);
            this.gbFileManager.TabIndex = 4;
            this.gbFileManager.TabStop = false;
            this.gbFileManager.Text = "File";
            // 
            // btnCloseAllDatabase
            // 
            this.btnCloseAllDatabase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCloseAllDatabase.BackgroundImage")));
            this.btnCloseAllDatabase.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCloseAllDatabase.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnCloseAllDatabase.Location = new System.Drawing.Point(225, 18);
            this.btnCloseAllDatabase.Name = "btnCloseAllDatabase";
            this.btnCloseAllDatabase.Size = new System.Drawing.Size(74, 73);
            this.btnCloseAllDatabase.TabIndex = 8;
            this.btnCloseAllDatabase.UseVisualStyleBackColor = true;
            // 
            // btnRicaricaDB
            // 
            this.btnRicaricaDB.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRicaricaDB.BackgroundImage")));
            this.btnRicaricaDB.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRicaricaDB.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnRicaricaDB.Location = new System.Drawing.Point(151, 18);
            this.btnRicaricaDB.Name = "btnRicaricaDB";
            this.btnRicaricaDB.Size = new System.Drawing.Size(74, 73);
            this.btnRicaricaDB.TabIndex = 7;
            this.btnRicaricaDB.UseVisualStyleBackColor = true;
            this.btnRicaricaDB.Click += new System.EventHandler(this.btnRicaricaDB_Click);
            // 
            // btnSaveDatabase
            // 
            this.btnSaveDatabase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSaveDatabase.BackgroundImage")));
            this.btnSaveDatabase.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSaveDatabase.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSaveDatabase.Location = new System.Drawing.Point(77, 18);
            this.btnSaveDatabase.Name = "btnSaveDatabase";
            this.btnSaveDatabase.Size = new System.Drawing.Size(74, 73);
            this.btnSaveDatabase.TabIndex = 6;
            this.btnSaveDatabase.UseVisualStyleBackColor = true;
            // 
            // btnLoadDatabase
            // 
            this.btnLoadDatabase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnLoadDatabase.BackgroundImage")));
            this.btnLoadDatabase.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLoadDatabase.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnLoadDatabase.Location = new System.Drawing.Point(3, 18);
            this.btnLoadDatabase.Name = "btnLoadDatabase";
            this.btnLoadDatabase.Size = new System.Drawing.Size(74, 73);
            this.btnLoadDatabase.TabIndex = 5;
            this.btnLoadDatabase.UseVisualStyleBackColor = true;
            this.btnLoadDatabase.Click += new System.EventHandler(this.btnLoadDatabase_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelDen,
            this.toolStripStatusLabel,
            this.toolStripDbDefaulLabelDen,
            this.toolStripDbDefaulLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 1004);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1782, 26);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabelDen
            // 
            this.toolStripStatusLabelDen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabelDen.ForeColor = System.Drawing.Color.Blue;
            this.toolStripStatusLabelDen.Name = "toolStripStatusLabelDen";
            this.toolStripStatusLabelDen.Size = new System.Drawing.Size(136, 20);
            this.toolStripStatusLabelDen.Text = "Messaggi di stato:";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.ForeColor = System.Drawing.Color.Green;
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(18, 20);
            this.toolStripStatusLabel.Text = "...";
            // 
            // toolStripDbDefaulLabelDen
            // 
            this.toolStripDbDefaulLabelDen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripDbDefaulLabelDen.ForeColor = System.Drawing.Color.Blue;
            this.toolStripDbDefaulLabelDen.Name = "toolStripDbDefaulLabelDen";
            this.toolStripDbDefaulLabelDen.Size = new System.Drawing.Size(156, 20);
            this.toolStripDbDefaulLabelDen.Text = "Database predefinito";
            // 
            // toolStripDbDefaulLabel
            // 
            this.toolStripDbDefaulLabel.ForeColor = System.Drawing.Color.Green;
            this.toolStripDbDefaulLabel.Name = "toolStripDbDefaulLabel";
            this.toolStripDbDefaulLabel.Size = new System.Drawing.Size(18, 20);
            this.toolStripDbDefaulLabel.Text = "...";
            // 
            // spcMain
            // 
            this.spcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcMain.Location = new System.Drawing.Point(0, 94);
            this.spcMain.Name = "spcMain";
            // 
            // spcMain.Panel1
            // 
            this.spcMain.Panel1.Controls.Add(this.spcLeft);
            this.spcMain.Panel1.Controls.Add(this.evolutionMonitor);
            // 
            // spcMain.Panel2
            // 
            this.spcMain.Panel2.Controls.Add(this.tabCtrlDBgrid);
            this.spcMain.Size = new System.Drawing.Size(1782, 910);
            this.spcMain.SplitterDistance = 594;
            this.spcMain.TabIndex = 3;
            // 
            // spcLeft
            // 
            this.spcLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcLeft.Location = new System.Drawing.Point(0, 0);
            this.spcLeft.Name = "spcLeft";
            this.spcLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcLeft.Panel1
            // 
            this.spcLeft.Panel1.Controls.Add(this.treeViewDatabase);
            // 
            // spcLeft.Panel2
            // 
            this.spcLeft.Panel2.Controls.Add(this.listViewFields);
            this.spcLeft.Size = new System.Drawing.Size(594, 732);
            this.spcLeft.SplitterDistance = 380;
            this.spcLeft.TabIndex = 5;
            // 
            // treeViewDatabase
            // 
            this.treeViewDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewDatabase.Location = new System.Drawing.Point(0, 0);
            this.treeViewDatabase.Name = "treeViewDatabase";
            this.treeViewDatabase.Size = new System.Drawing.Size(594, 380);
            this.treeViewDatabase.TabIndex = 0;
            this.treeViewDatabase.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewDatabase_AfterSelect);
            // 
            // listViewFields
            // 
            this.listViewFields.AllowColumnReorder = true;
            this.listViewFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewFields.FullRowSelect = true;
            this.listViewFields.GridLines = true;
            this.listViewFields.HideSelection = false;
            this.listViewFields.LabelEdit = true;
            this.listViewFields.Location = new System.Drawing.Point(0, 0);
            this.listViewFields.Name = "listViewFields";
            this.listViewFields.Size = new System.Drawing.Size(594, 348);
            this.listViewFields.TabIndex = 5;
            this.listViewFields.UseCompatibleStateImageBehavior = false;
            this.listViewFields.View = System.Windows.Forms.View.Details;
            // 
            // evolutionMonitor
            // 
            this.evolutionMonitor.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.evolutionMonitor.Location = new System.Drawing.Point(0, 732);
            this.evolutionMonitor.Name = "evolutionMonitor";
            this.evolutionMonitor.Size = new System.Drawing.Size(594, 178);
            this.evolutionMonitor.TabIndex = 4;
            this.evolutionMonitor.Text = "";
            // 
            // tabCtrlDBgrid
            // 
            this.tabCtrlDBgrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCtrlDBgrid.Location = new System.Drawing.Point(0, 0);
            this.tabCtrlDBgrid.Name = "tabCtrlDBgrid";
            this.tabCtrlDBgrid.SelectedIndex = 0;
            this.tabCtrlDBgrid.Size = new System.Drawing.Size(1184, 910);
            this.tabCtrlDBgrid.TabIndex = 0;
            // 
            // FrmEvolutiveSystem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1782, 1030);
            this.Controls.Add(this.spcMain);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.panelCommands);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmEvolutiveSystem";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Evolutive DB - test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmEvolutiveSystem_FormClosing);
            this.Load += new System.EventHandler(this.FrmEvolutiveSystem_Load);
            this.panelCommands.ResumeLayout(false);
            this.gbAnalysis.ResumeLayout(false);
            this.gbSocketServer.ResumeLayout(false);
            this.gbServiceManager.ResumeLayout(false);
            this.gbTestConfig.ResumeLayout(false);
            this.gbFileManager.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.spcMain.Panel1.ResumeLayout(false);
            this.spcMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcMain)).EndInit();
            this.spcMain.ResumeLayout(false);
            this.spcLeft.Panel1.ResumeLayout(false);
            this.spcLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcLeft)).EndInit();
            this.spcLeft.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelCommands;
        private System.Windows.Forms.GroupBox gbFileManager;
        private System.Windows.Forms.Button btnCloseAllDatabase;
        private System.Windows.Forms.Button btnRicaricaDB;
        private System.Windows.Forms.Button btnSaveDatabase;
        private System.Windows.Forms.Button btnLoadDatabase;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.SplitContainer spcMain;
        private System.Windows.Forms.SplitContainer spcLeft;
        private System.Windows.Forms.RichTextBox evolutionMonitor;
        private System.Windows.Forms.TreeView treeViewDatabase;
        private System.Windows.Forms.TabControl tabCtrlDBgrid;
        private System.Windows.Forms.ListView listViewFields;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelDen;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripDbDefaulLabelDen;
        private System.Windows.Forms.ToolStripStatusLabel toolStripDbDefaulLabel;
        private System.Windows.Forms.GroupBox gbAnalysis;
        private System.Windows.Forms.Button btnAnalysis;
        private System.Windows.Forms.GroupBox gbSocketServer;
        private System.Windows.Forms.Button btnSocket;
        private System.Windows.Forms.GroupBox gbServiceManager;
        private System.Windows.Forms.Button btnServiceStop;
        private System.Windows.Forms.Button btnServicePause;
        private System.Windows.Forms.Button btnServiceStart;
        private System.Windows.Forms.GroupBox gbTestConfig;
        private System.Windows.Forms.Button btnConfigTest;
    }
}

