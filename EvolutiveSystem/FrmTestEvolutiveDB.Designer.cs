namespace EvolutiveSystem
{
    partial class FrmTestEvolutiveDB
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmTestEvolutiveDB));
            this.pnlCmd = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAddDb = new System.Windows.Forms.Button();
            this.bnlBtn = new System.Windows.Forms.Panel();
            this.spcDBtest = new System.Windows.Forms.SplitContainer();
            this.spcDefField = new System.Windows.Forms.SplitContainer();
            this.trvDB = new System.Windows.Forms.TreeView();
            this.lwField = new System.Windows.Forms.ListView();
            this.pnlDetails = new System.Windows.Forms.Panel();
            this.lblTblName = new System.Windows.Forms.Label();
            this.lblDbName = new System.Windows.Forms.Label();
            this.lblDenTblName = new System.Windows.Forms.Label();
            this.lblDenDbName = new System.Windows.Forms.Label();
            this.tbcDatabase = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.ctxFieldManager = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmAddField = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmRemoveTable = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmFmClose = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxTableManager = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmAddTable = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmRemoveDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmTmClose = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxEditField = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmEditField = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmDelField = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlCmd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcDBtest)).BeginInit();
            this.spcDBtest.Panel1.SuspendLayout();
            this.spcDBtest.Panel2.SuspendLayout();
            this.spcDBtest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcDefField)).BeginInit();
            this.spcDefField.Panel1.SuspendLayout();
            this.spcDefField.Panel2.SuspendLayout();
            this.spcDefField.SuspendLayout();
            this.pnlDetails.SuspendLayout();
            this.tbcDatabase.SuspendLayout();
            this.ctxFieldManager.SuspendLayout();
            this.ctxTableManager.SuspendLayout();
            this.ctxEditField.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlCmd
            // 
            this.pnlCmd.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCmd.Controls.Add(this.btnSave);
            this.pnlCmd.Controls.Add(this.btnAddDb);
            this.pnlCmd.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCmd.Location = new System.Drawing.Point(0, 0);
            this.pnlCmd.Name = "pnlCmd";
            this.pnlCmd.Size = new System.Drawing.Size(1346, 68);
            this.pnlCmd.TabIndex = 4;
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImage = global::EvolutiveSystem.Properties.Resources.icons8_save_close_48;
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSave.Location = new System.Drawing.Point(71, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(71, 66);
            this.btnSave.TabIndex = 1;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnAddDb
            // 
            this.btnAddDb.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAddDb.BackgroundImage")));
            this.btnAddDb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddDb.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnAddDb.Location = new System.Drawing.Point(0, 0);
            this.btnAddDb.Name = "btnAddDb";
            this.btnAddDb.Size = new System.Drawing.Size(71, 66);
            this.btnAddDb.TabIndex = 0;
            this.btnAddDb.UseVisualStyleBackColor = true;
            this.btnAddDb.Click += new System.EventHandler(this.btnAddDb_Click);
            // 
            // bnlBtn
            // 
            this.bnlBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bnlBtn.Location = new System.Drawing.Point(0, 824);
            this.bnlBtn.Name = "bnlBtn";
            this.bnlBtn.Size = new System.Drawing.Size(1346, 41);
            this.bnlBtn.TabIndex = 5;
            // 
            // spcDBtest
            // 
            this.spcDBtest.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.spcDBtest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcDBtest.Location = new System.Drawing.Point(0, 68);
            this.spcDBtest.Name = "spcDBtest";
            // 
            // spcDBtest.Panel1
            // 
            this.spcDBtest.Panel1.Controls.Add(this.spcDefField);
            // 
            // spcDBtest.Panel2
            // 
            this.spcDBtest.Panel2.Controls.Add(this.tbcDatabase);
            this.spcDBtest.Size = new System.Drawing.Size(1346, 756);
            this.spcDBtest.SplitterDistance = 457;
            this.spcDBtest.TabIndex = 6;
            // 
            // spcDefField
            // 
            this.spcDefField.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.spcDefField.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcDefField.Location = new System.Drawing.Point(0, 0);
            this.spcDefField.Name = "spcDefField";
            this.spcDefField.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcDefField.Panel1
            // 
            this.spcDefField.Panel1.Controls.Add(this.trvDB);
            // 
            // spcDefField.Panel2
            // 
            this.spcDefField.Panel2.Controls.Add(this.lwField);
            this.spcDefField.Panel2.Controls.Add(this.pnlDetails);
            this.spcDefField.Size = new System.Drawing.Size(457, 756);
            this.spcDefField.SplitterDistance = 343;
            this.spcDefField.TabIndex = 0;
            // 
            // trvDB
            // 
            this.trvDB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trvDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.trvDB.Location = new System.Drawing.Point(0, 0);
            this.trvDB.Name = "trvDB";
            this.trvDB.Size = new System.Drawing.Size(455, 341);
            this.trvDB.TabIndex = 0;
            this.trvDB.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.trvDB_NodeMouseClick);
            // 
            // lwField
            // 
            this.lwField.AllowColumnReorder = true;
            this.lwField.FullRowSelect = true;
            this.lwField.GridLines = true;
            this.lwField.HideSelection = false;
            this.lwField.LabelEdit = true;
            this.lwField.Location = new System.Drawing.Point(0, 63);
            this.lwField.Name = "lwField";
            this.lwField.Size = new System.Drawing.Size(455, 344);
            this.lwField.TabIndex = 1;
            this.lwField.UseCompatibleStateImageBehavior = false;
            this.lwField.View = System.Windows.Forms.View.Details;
            // 
            // pnlDetails
            // 
            this.pnlDetails.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.pnlDetails.Controls.Add(this.lblTblName);
            this.pnlDetails.Controls.Add(this.lblDbName);
            this.pnlDetails.Controls.Add(this.lblDenTblName);
            this.pnlDetails.Controls.Add(this.lblDenDbName);
            this.pnlDetails.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDetails.Location = new System.Drawing.Point(0, 0);
            this.pnlDetails.Name = "pnlDetails";
            this.pnlDetails.Size = new System.Drawing.Size(455, 63);
            this.pnlDetails.TabIndex = 0;
            // 
            // lblTblName
            // 
            this.lblTblName.AutoSize = true;
            this.lblTblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTblName.ForeColor = System.Drawing.Color.SeaGreen;
            this.lblTblName.Location = new System.Drawing.Point(172, 29);
            this.lblTblName.Name = "lblTblName";
            this.lblTblName.Size = new System.Drawing.Size(27, 25);
            this.lblTblName.TabIndex = 3;
            this.lblTblName.Text = "...";
            // 
            // lblDbName
            // 
            this.lblDbName.AutoSize = true;
            this.lblDbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDbName.ForeColor = System.Drawing.Color.Green;
            this.lblDbName.Location = new System.Drawing.Point(172, 4);
            this.lblDbName.Name = "lblDbName";
            this.lblDbName.Size = new System.Drawing.Size(27, 25);
            this.lblDbName.TabIndex = 2;
            this.lblDbName.Text = "...";
            // 
            // lblDenTblName
            // 
            this.lblDenTblName.AutoSize = true;
            this.lblDenTblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenTblName.ForeColor = System.Drawing.Color.MediumBlue;
            this.lblDenTblName.Location = new System.Drawing.Point(34, 29);
            this.lblDenTblName.Name = "lblDenTblName";
            this.lblDenTblName.Size = new System.Drawing.Size(132, 25);
            this.lblDenTblName.TabIndex = 1;
            this.lblDenTblName.Text = "Nome tabella:";
            // 
            // lblDenDbName
            // 
            this.lblDenDbName.AutoSize = true;
            this.lblDenDbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenDbName.ForeColor = System.Drawing.Color.MediumBlue;
            this.lblDenDbName.Location = new System.Drawing.Point(10, 4);
            this.lblDenDbName.Name = "lblDenDbName";
            this.lblDenDbName.Size = new System.Drawing.Size(156, 25);
            this.lblDenDbName.TabIndex = 0;
            this.lblDenDbName.Text = "Database name:";
            // 
            // tbcDatabase
            // 
            this.tbcDatabase.Controls.Add(this.tabPage1);
            this.tbcDatabase.Controls.Add(this.tabPage2);
            this.tbcDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbcDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbcDatabase.Location = new System.Drawing.Point(0, 0);
            this.tbcDatabase.Name = "tbcDatabase";
            this.tbcDatabase.SelectedIndex = 0;
            this.tbcDatabase.Size = new System.Drawing.Size(883, 754);
            this.tbcDatabase.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPage1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPage1.Location = new System.Drawing.Point(4, 31);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(875, 719);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 31);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(875, 719);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // ctxFieldManager
            // 
            this.ctxFieldManager.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctxFieldManager.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmAddField,
            this.tsmRemoveTable,
            this.toolStripSeparator1,
            this.tsmFmClose});
            this.ctxFieldManager.Name = "ctxFieldManager";
            this.ctxFieldManager.Size = new System.Drawing.Size(190, 82);
            // 
            // tsmAddField
            // 
            this.tsmAddField.Name = "tsmAddField";
            this.tsmAddField.Size = new System.Drawing.Size(189, 24);
            this.tsmAddField.Text = "Aggiungi campo";
            this.tsmAddField.Click += new System.EventHandler(this.tsmAddField_Click);
            // 
            // tsmRemoveTable
            // 
            this.tsmRemoveTable.Name = "tsmRemoveTable";
            this.tsmRemoveTable.Size = new System.Drawing.Size(189, 24);
            this.tsmRemoveTable.Text = "Rimuovi tabella";
            this.tsmRemoveTable.Click += new System.EventHandler(this.tsmRemoveTable_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(186, 6);
            // 
            // tsmFmClose
            // 
            this.tsmFmClose.Name = "tsmFmClose";
            this.tsmFmClose.Size = new System.Drawing.Size(189, 24);
            this.tsmFmClose.Text = "Chiudi";
            // 
            // ctxTableManager
            // 
            this.ctxTableManager.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctxTableManager.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmAddTable,
            this.tsmRemoveDatabase,
            this.toolStripSeparator2,
            this.tsmTmClose});
            this.ctxTableManager.Name = "ctxTableManager";
            this.ctxTableManager.Size = new System.Drawing.Size(200, 82);
            // 
            // tsmAddTable
            // 
            this.tsmAddTable.Name = "tsmAddTable";
            this.tsmAddTable.Size = new System.Drawing.Size(199, 24);
            this.tsmAddTable.Text = "Aggiungi tabella";
            this.tsmAddTable.Click += new System.EventHandler(this.tsmAddTable_Click);
            // 
            // tsmRemoveDatabase
            // 
            this.tsmRemoveDatabase.Name = "tsmRemoveDatabase";
            this.tsmRemoveDatabase.Size = new System.Drawing.Size(199, 24);
            this.tsmRemoveDatabase.Text = "Cancella database";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(196, 6);
            // 
            // tsmTmClose
            // 
            this.tsmTmClose.Name = "tsmTmClose";
            this.tsmTmClose.Size = new System.Drawing.Size(199, 24);
            this.tsmTmClose.Text = "Chiudi";
            // 
            // ctxEditField
            // 
            this.ctxEditField.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctxEditField.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmEditField,
            this.tsmDelField,
            this.toolStripSeparator3,
            this.toolStripMenuItem1});
            this.ctxEditField.Name = "ctx";
            this.ctxEditField.Size = new System.Drawing.Size(188, 82);
            // 
            // tsmEditField
            // 
            this.tsmEditField.Name = "tsmEditField";
            this.tsmEditField.Size = new System.Drawing.Size(187, 24);
            this.tsmEditField.Text = "Modifica campo";
            this.tsmEditField.Click += new System.EventHandler(this.tsmEditField_Click);
            // 
            // tsmDelField
            // 
            this.tsmDelField.Name = "tsmDelField";
            this.tsmDelField.Size = new System.Drawing.Size(187, 24);
            this.tsmDelField.Text = "Cancella campo";
            this.tsmDelField.Click += new System.EventHandler(this.tsmDelField_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(184, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(187, 24);
            this.toolStripMenuItem1.Text = "Chiudi";
            // 
            // FrmTestEvolutiveDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1346, 865);
            this.Controls.Add(this.spcDBtest);
            this.Controls.Add(this.bnlBtn);
            this.Controls.Add(this.pnlCmd);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmTestEvolutiveDB";
            this.Text = "Evolutive DB - test";
            this.Load += new System.EventHandler(this.FrmTestEvolutiveDB_Load);
            this.pnlCmd.ResumeLayout(false);
            this.spcDBtest.Panel1.ResumeLayout(false);
            this.spcDBtest.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcDBtest)).EndInit();
            this.spcDBtest.ResumeLayout(false);
            this.spcDefField.Panel1.ResumeLayout(false);
            this.spcDefField.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcDefField)).EndInit();
            this.spcDefField.ResumeLayout(false);
            this.pnlDetails.ResumeLayout(false);
            this.pnlDetails.PerformLayout();
            this.tbcDatabase.ResumeLayout(false);
            this.ctxFieldManager.ResumeLayout(false);
            this.ctxTableManager.ResumeLayout(false);
            this.ctxEditField.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel pnlCmd;
        private System.Windows.Forms.Panel bnlBtn;
        private System.Windows.Forms.SplitContainer spcDBtest;
        private System.Windows.Forms.SplitContainer spcDefField;
        private System.Windows.Forms.Button btnAddDb;
        private System.Windows.Forms.TreeView trvDB;
        private System.Windows.Forms.ContextMenuStrip ctxFieldManager;
        private System.Windows.Forms.ToolStripMenuItem tsmAddField;
        private System.Windows.Forms.ToolStripMenuItem tsmRemoveTable;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsmFmClose;
        private System.Windows.Forms.ContextMenuStrip ctxTableManager;
        private System.Windows.Forms.ToolStripMenuItem tsmAddTable;
        private System.Windows.Forms.ToolStripMenuItem tsmRemoveDatabase;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem tsmTmClose;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Panel pnlDetails;
        private System.Windows.Forms.Label lblDenDbName;
        private System.Windows.Forms.Label lblDenTblName;
        private System.Windows.Forms.Label lblDbName;
        private System.Windows.Forms.Label lblTblName;
        private System.Windows.Forms.ListView lwField;
        private System.Windows.Forms.ContextMenuStrip ctxEditField;
        private System.Windows.Forms.ToolStripMenuItem tsmEditField;
        private System.Windows.Forms.ToolStripMenuItem tsmDelField;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.TabControl tbcDatabase;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
    }
}