namespace EvolutiveSystem_01
{
    partial class FrmTelegram
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmTelegram));
            this.btnAnnullaSend = new System.Windows.Forms.Button();
            this.gbSelCmd = new System.Windows.Forms.GroupBox();
            this.lblMonitor = new System.Windows.Forms.Label();
            this.rtxtMonitor = new System.Windows.Forms.RichTextBox();
            this.lblIdSel = new System.Windows.Forms.Label();
            this.lblDenDbIdSel = new System.Windows.Forms.Label();
            this.lblDb = new System.Windows.Forms.Label();
            this.lblDenDb = new System.Windows.Forms.Label();
            this.chkbCRC = new System.Windows.Forms.CheckBox();
            this.btnCalcTocken = new System.Windows.Forms.Button();
            this.lblToken = new System.Windows.Forms.Label();
            this.lblDenCalcolaTocken = new System.Windows.Forms.Label();
            this.cmbCommand = new System.Windows.Forms.ComboBox();
            this.txtDateSend = new System.Windows.Forms.TextBox();
            this.lblSendTime = new System.Windows.Forms.Label();
            this.lblCommand = new System.Windows.Forms.Label();
            this.pnlbtn = new System.Windows.Forms.Panel();
            this.btnOkSend = new System.Windows.Forms.Button();
            this.btnSendMsg = new System.Windows.Forms.Button();
            this.gbCompilaBufferTx = new System.Windows.Forms.GroupBox();
            this.rtxtBuffer = new System.Windows.Forms.RichTextBox();
            this.gbActionTypeLoadDb = new System.Windows.Forms.GroupBox();
            this.bgtnAnnullaDb = new System.Windows.Forms.Button();
            this.btnOkDb = new System.Windows.Forms.Button();
            this.btnSelDb = new System.Windows.Forms.Button();
            this.txtFileDb = new System.Windows.Forms.TextBox();
            this.lblDenDbName = new System.Windows.Forms.Label();
            this.gbActionSaveDatabase = new System.Windows.Forms.GroupBox();
            this.lblId = new System.Windows.Forms.Label();
            this.lblDbSelect = new System.Windows.Forms.Label();
            this.lblDenIdSel = new System.Windows.Forms.Label();
            this.chbSavePerID = new System.Windows.Forms.CheckBox();
            this.btnAnnulaSaveDB = new System.Windows.Forms.Button();
            this.btnOkSaveDB = new System.Windows.Forms.Button();
            this.btnSaveDb = new System.Windows.Forms.Button();
            this.txtDbSave = new System.Windows.Forms.TextBox();
            this.lblDenSelDb = new System.Windows.Forms.Label();
            this.gbSelRequest = new System.Windows.Forms.GroupBox();
            this.lblRqId = new System.Windows.Forms.Label();
            this.lblRqDbSel = new System.Windows.Forms.Label();
            this.lblDenRqDbSel = new System.Windows.Forms.Label();
            this.chbRqPerId = new System.Windows.Forms.CheckBox();
            this.btnSelRqDb = new System.Windows.Forms.Button();
            this.txtDbRq = new System.Windows.Forms.TextBox();
            this.dbStructReqAnnulla = new System.Windows.Forms.Button();
            this.BtnDbStructRecOk = new System.Windows.Forms.Button();
            this.rbFull = new System.Windows.Forms.RadioButton();
            this.rbStructureOnly = new System.Windows.Forms.RadioButton();
            this.rbListOnly = new System.Windows.Forms.RadioButton();
            this.gbSelCmd.SuspendLayout();
            this.pnlbtn.SuspendLayout();
            this.gbCompilaBufferTx.SuspendLayout();
            this.gbActionTypeLoadDb.SuspendLayout();
            this.gbActionSaveDatabase.SuspendLayout();
            this.gbSelRequest.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAnnullaSend
            // 
            this.btnAnnullaSend.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnAnnullaSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnullaSend.Location = new System.Drawing.Point(228, 3);
            this.btnAnnullaSend.Name = "btnAnnullaSend";
            this.btnAnnullaSend.Size = new System.Drawing.Size(95, 53);
            this.btnAnnullaSend.TabIndex = 13;
            this.btnAnnullaSend.Text = "Annulla";
            this.btnAnnullaSend.UseVisualStyleBackColor = true;
            this.btnAnnullaSend.Click += new System.EventHandler(this.btnAnnullaSend_Click);
            // 
            // gbSelCmd
            // 
            this.gbSelCmd.Controls.Add(this.lblMonitor);
            this.gbSelCmd.Controls.Add(this.rtxtMonitor);
            this.gbSelCmd.Controls.Add(this.lblIdSel);
            this.gbSelCmd.Controls.Add(this.lblDenDbIdSel);
            this.gbSelCmd.Controls.Add(this.lblDb);
            this.gbSelCmd.Controls.Add(this.lblDenDb);
            this.gbSelCmd.Controls.Add(this.chkbCRC);
            this.gbSelCmd.Controls.Add(this.btnCalcTocken);
            this.gbSelCmd.Controls.Add(this.lblToken);
            this.gbSelCmd.Controls.Add(this.lblDenCalcolaTocken);
            this.gbSelCmd.Controls.Add(this.cmbCommand);
            this.gbSelCmd.Controls.Add(this.txtDateSend);
            this.gbSelCmd.Controls.Add(this.lblSendTime);
            this.gbSelCmd.Controls.Add(this.lblCommand);
            this.gbSelCmd.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbSelCmd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSelCmd.Location = new System.Drawing.Point(0, 0);
            this.gbSelCmd.Name = "gbSelCmd";
            this.gbSelCmd.Size = new System.Drawing.Size(818, 203);
            this.gbSelCmd.TabIndex = 36;
            this.gbSelCmd.TabStop = false;
            this.gbSelCmd.Text = "Seezione comandi e parametri di trasmissione";
            // 
            // lblMonitor
            // 
            this.lblMonitor.AutoSize = true;
            this.lblMonitor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMonitor.Location = new System.Drawing.Point(591, 110);
            this.lblMonitor.Name = "lblMonitor";
            this.lblMonitor.Size = new System.Drawing.Size(66, 18);
            this.lblMonitor.TabIndex = 48;
            this.lblMonitor.Text = "Monitor";
            // 
            // rtxtMonitor
            // 
            this.rtxtMonitor.Location = new System.Drawing.Point(591, 136);
            this.rtxtMonitor.Name = "rtxtMonitor";
            this.rtxtMonitor.Size = new System.Drawing.Size(166, 52);
            this.rtxtMonitor.TabIndex = 47;
            this.rtxtMonitor.Text = "";
            // 
            // lblIdSel
            // 
            this.lblIdSel.AutoSize = true;
            this.lblIdSel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblIdSel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIdSel.Location = new System.Drawing.Point(160, 166);
            this.lblIdSel.Name = "lblIdSel";
            this.lblIdSel.Size = new System.Drawing.Size(47, 22);
            this.lblIdSel.TabIndex = 46;
            this.lblIdSel.Text = "none";
            // 
            // lblDenDbIdSel
            // 
            this.lblDenDbIdSel.AutoSize = true;
            this.lblDenDbIdSel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenDbIdSel.Location = new System.Drawing.Point(29, 166);
            this.lblDenDbIdSel.Name = "lblDenDbIdSel";
            this.lblDenDbIdSel.Size = new System.Drawing.Size(102, 18);
            this.lblDenDbIdSel.TabIndex = 45;
            this.lblDenDbIdSel.Text = "Database Id:";
            // 
            // lblDb
            // 
            this.lblDb.AutoSize = true;
            this.lblDb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDb.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDb.Location = new System.Drawing.Point(160, 136);
            this.lblDb.Name = "lblDb";
            this.lblDb.Size = new System.Drawing.Size(47, 22);
            this.lblDb.TabIndex = 44;
            this.lblDb.Text = "none";
            // 
            // lblDenDb
            // 
            this.lblDenDb.AutoSize = true;
            this.lblDenDb.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenDb.Location = new System.Drawing.Point(47, 136);
            this.lblDenDb.Name = "lblDenDb";
            this.lblDenDb.Size = new System.Drawing.Size(84, 18);
            this.lblDenDb.TabIndex = 43;
            this.lblDenDb.Text = "Database:";
            // 
            // chkbCRC
            // 
            this.chkbCRC.AutoSize = true;
            this.chkbCRC.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkbCRC.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkbCRC.Location = new System.Drawing.Point(601, 65);
            this.chkbCRC.Name = "chkbCRC";
            this.chkbCRC.Size = new System.Drawing.Size(156, 22);
            this.chkbCRC.TabIndex = 42;
            this.chkbCRC.Text = "Calcolo del CRC";
            this.chkbCRC.UseVisualStyleBackColor = true;
            // 
            // btnCalcTocken
            // 
            this.btnCalcTocken.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCalcTocken.Location = new System.Drawing.Point(458, 100);
            this.btnCalcTocken.Name = "btnCalcTocken";
            this.btnCalcTocken.Size = new System.Drawing.Size(43, 39);
            this.btnCalcTocken.TabIndex = 41;
            this.btnCalcTocken.Text = "...";
            this.btnCalcTocken.UseVisualStyleBackColor = true;
            this.btnCalcTocken.Click += new System.EventHandler(this.btnCalcTocken_Click);
            // 
            // lblToken
            // 
            this.lblToken.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblToken.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToken.Location = new System.Drawing.Point(160, 100);
            this.lblToken.Name = "lblToken";
            this.lblToken.Size = new System.Drawing.Size(288, 28);
            this.lblToken.TabIndex = 40;
            this.lblToken.Text = "0";
            // 
            // lblDenCalcolaTocken
            // 
            this.lblDenCalcolaTocken.AutoSize = true;
            this.lblDenCalcolaTocken.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenCalcolaTocken.Location = new System.Drawing.Point(62, 100);
            this.lblDenCalcolaTocken.Name = "lblDenCalcolaTocken";
            this.lblDenCalcolaTocken.Size = new System.Drawing.Size(69, 18);
            this.lblDenCalcolaTocken.TabIndex = 39;
            this.lblDenCalcolaTocken.Text = "Tocken:";
            // 
            // cmbCommand
            // 
            this.cmbCommand.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbCommand.FormattingEnabled = true;
            this.cmbCommand.Location = new System.Drawing.Point(160, 29);
            this.cmbCommand.Name = "cmbCommand";
            this.cmbCommand.Size = new System.Drawing.Size(607, 28);
            this.cmbCommand.TabIndex = 38;
            this.cmbCommand.SelectedIndexChanged += new System.EventHandler(this.cmbCommand_SelectedIndexChanged);
            // 
            // txtDateSend
            // 
            this.txtDateSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDateSend.Location = new System.Drawing.Point(160, 65);
            this.txtDateSend.Name = "txtDateSend";
            this.txtDateSend.Size = new System.Drawing.Size(291, 27);
            this.txtDateSend.TabIndex = 37;
            // 
            // lblSendTime
            // 
            this.lblSendTime.AutoSize = true;
            this.lblSendTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSendTime.Location = new System.Drawing.Point(43, 65);
            this.lblSendTime.Name = "lblSendTime";
            this.lblSendTime.Size = new System.Drawing.Size(88, 18);
            this.lblSendTime.TabIndex = 36;
            this.lblSendTime.Text = "Data invio:";
            // 
            // lblCommand
            // 
            this.lblCommand.AutoSize = true;
            this.lblCommand.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCommand.Location = new System.Drawing.Point(45, 29);
            this.lblCommand.Name = "lblCommand";
            this.lblCommand.Size = new System.Drawing.Size(86, 18);
            this.lblCommand.TabIndex = 35;
            this.lblCommand.Text = "Comando:";
            // 
            // pnlbtn
            // 
            this.pnlbtn.Controls.Add(this.btnOkSend);
            this.pnlbtn.Controls.Add(this.btnAnnullaSend);
            this.pnlbtn.Controls.Add(this.btnSendMsg);
            this.pnlbtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlbtn.Location = new System.Drawing.Point(0, 584);
            this.pnlbtn.Name = "pnlbtn";
            this.pnlbtn.Size = new System.Drawing.Size(818, 62);
            this.pnlbtn.TabIndex = 37;
            // 
            // btnOkSend
            // 
            this.btnOkSend.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOkSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOkSend.Location = new System.Drawing.Point(147, 3);
            this.btnOkSend.Name = "btnOkSend";
            this.btnOkSend.Size = new System.Drawing.Size(75, 53);
            this.btnOkSend.TabIndex = 14;
            this.btnOkSend.Text = "ok";
            this.btnOkSend.UseVisualStyleBackColor = true;
            this.btnOkSend.Click += new System.EventHandler(this.btnOkSend_Click);
            // 
            // btnSendMsg
            // 
            this.btnSendMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendMsg.Location = new System.Drawing.Point(3, 3);
            this.btnSendMsg.Name = "btnSendMsg";
            this.btnSendMsg.Size = new System.Drawing.Size(140, 53);
            this.btnSendMsg.TabIndex = 12;
            this.btnSendMsg.Text = "Prepara telegramma";
            this.btnSendMsg.UseVisualStyleBackColor = true;
            this.btnSendMsg.Click += new System.EventHandler(this.btnSendMsg_Click);
            // 
            // gbCompilaBufferTx
            // 
            this.gbCompilaBufferTx.Controls.Add(this.rtxtBuffer);
            this.gbCompilaBufferTx.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gbCompilaBufferTx.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbCompilaBufferTx.Location = new System.Drawing.Point(0, 400);
            this.gbCompilaBufferTx.Name = "gbCompilaBufferTx";
            this.gbCompilaBufferTx.Size = new System.Drawing.Size(818, 184);
            this.gbCompilaBufferTx.TabIndex = 38;
            this.gbCompilaBufferTx.TabStop = false;
            this.gbCompilaBufferTx.Text = "Telegramma compilato";
            // 
            // rtxtBuffer
            // 
            this.rtxtBuffer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtxtBuffer.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtxtBuffer.Location = new System.Drawing.Point(3, 18);
            this.rtxtBuffer.Name = "rtxtBuffer";
            this.rtxtBuffer.Size = new System.Drawing.Size(812, 163);
            this.rtxtBuffer.TabIndex = 0;
            this.rtxtBuffer.Text = "";
            // 
            // gbActionTypeLoadDb
            // 
            this.gbActionTypeLoadDb.Controls.Add(this.bgtnAnnullaDb);
            this.gbActionTypeLoadDb.Controls.Add(this.btnOkDb);
            this.gbActionTypeLoadDb.Controls.Add(this.btnSelDb);
            this.gbActionTypeLoadDb.Controls.Add(this.txtFileDb);
            this.gbActionTypeLoadDb.Controls.Add(this.lblDenDbName);
            this.gbActionTypeLoadDb.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbActionTypeLoadDb.Location = new System.Drawing.Point(232, 225);
            this.gbActionTypeLoadDb.Name = "gbActionTypeLoadDb";
            this.gbActionTypeLoadDb.Size = new System.Drawing.Size(659, 135);
            this.gbActionTypeLoadDb.TabIndex = 39;
            this.gbActionTypeLoadDb.TabStop = false;
            this.gbActionTypeLoadDb.Text = "Selezione database";
            this.gbActionTypeLoadDb.Visible = false;
            // 
            // bgtnAnnullaDb
            // 
            this.bgtnAnnullaDb.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bgtnAnnullaDb.Location = new System.Drawing.Point(578, 98);
            this.bgtnAnnullaDb.Name = "bgtnAnnullaDb";
            this.bgtnAnnullaDb.Size = new System.Drawing.Size(75, 31);
            this.bgtnAnnullaDb.TabIndex = 44;
            this.bgtnAnnullaDb.Text = "Annulla";
            this.bgtnAnnullaDb.UseVisualStyleBackColor = true;
            this.bgtnAnnullaDb.Click += new System.EventHandler(this.bgtnAnnullaDb_Click);
            // 
            // btnOkDb
            // 
            this.btnOkDb.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOkDb.Location = new System.Drawing.Point(509, 98);
            this.btnOkDb.Name = "btnOkDb";
            this.btnOkDb.Size = new System.Drawing.Size(65, 31);
            this.btnOkDb.TabIndex = 43;
            this.btnOkDb.Text = "Ok";
            this.btnOkDb.UseVisualStyleBackColor = true;
            this.btnOkDb.Click += new System.EventHandler(this.btnOkDb_Click);
            // 
            // btnSelDb
            // 
            this.btnSelDb.Location = new System.Drawing.Point(614, 44);
            this.btnSelDb.Name = "btnSelDb";
            this.btnSelDb.Size = new System.Drawing.Size(43, 39);
            this.btnSelDb.TabIndex = 42;
            this.btnSelDb.Text = "...";
            this.btnSelDb.UseVisualStyleBackColor = true;
            this.btnSelDb.Click += new System.EventHandler(this.btnSelDb_Click);
            // 
            // txtFileDb
            // 
            this.txtFileDb.Location = new System.Drawing.Point(15, 44);
            this.txtFileDb.Name = "txtFileDb";
            this.txtFileDb.Size = new System.Drawing.Size(593, 22);
            this.txtFileDb.TabIndex = 1;
            // 
            // lblDenDbName
            // 
            this.lblDenDbName.AutoSize = true;
            this.lblDenDbName.Location = new System.Drawing.Point(12, 25);
            this.lblDenDbName.Name = "lblDenDbName";
            this.lblDenDbName.Size = new System.Drawing.Size(121, 16);
            this.lblDenDbName.TabIndex = 0;
            this.lblDenDbName.Text = "Database name:";
            // 
            // gbActionSaveDatabase
            // 
            this.gbActionSaveDatabase.Controls.Add(this.gbSelRequest);
            this.gbActionSaveDatabase.Controls.Add(this.lblId);
            this.gbActionSaveDatabase.Controls.Add(this.lblDbSelect);
            this.gbActionSaveDatabase.Controls.Add(this.lblDenIdSel);
            this.gbActionSaveDatabase.Controls.Add(this.chbSavePerID);
            this.gbActionSaveDatabase.Controls.Add(this.btnAnnulaSaveDB);
            this.gbActionSaveDatabase.Controls.Add(this.btnOkSaveDB);
            this.gbActionSaveDatabase.Controls.Add(this.btnSaveDb);
            this.gbActionSaveDatabase.Controls.Add(this.txtDbSave);
            this.gbActionSaveDatabase.Controls.Add(this.lblDenSelDb);
            this.gbActionSaveDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbActionSaveDatabase.Location = new System.Drawing.Point(65, 260);
            this.gbActionSaveDatabase.Name = "gbActionSaveDatabase";
            this.gbActionSaveDatabase.Size = new System.Drawing.Size(659, 134);
            this.gbActionSaveDatabase.TabIndex = 40;
            this.gbActionSaveDatabase.TabStop = false;
            this.gbActionSaveDatabase.Text = "Salva database";
            this.gbActionSaveDatabase.Visible = false;
            // 
            // lblId
            // 
            this.lblId.AutoSize = true;
            this.lblId.Location = new System.Drawing.Point(213, 103);
            this.lblId.Name = "lblId";
            this.lblId.Size = new System.Drawing.Size(19, 16);
            this.lblId.TabIndex = 52;
            this.lblId.Text = "...";
            this.lblId.Visible = false;
            // 
            // lblDbSelect
            // 
            this.lblDbSelect.AutoSize = true;
            this.lblDbSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDbSelect.Location = new System.Drawing.Point(192, 77);
            this.lblDbSelect.Name = "lblDbSelect";
            this.lblDbSelect.Size = new System.Drawing.Size(41, 18);
            this.lblDbSelect.TabIndex = 51;
            this.lblDbSelect.Text = "none";
            // 
            // lblDenIdSel
            // 
            this.lblDenIdSel.AutoSize = true;
            this.lblDenIdSel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenIdSel.Location = new System.Drawing.Point(15, 77);
            this.lblDenIdSel.Name = "lblDenIdSel";
            this.lblDenIdSel.Size = new System.Drawing.Size(175, 18);
            this.lblDenIdSel.TabIndex = 50;
            this.lblDenIdSel.Text = "Database selezionate:";
            // 
            // chbSavePerID
            // 
            this.chbSavePerID.AutoSize = true;
            this.chbSavePerID.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chbSavePerID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chbSavePerID.Location = new System.Drawing.Point(15, 103);
            this.chbSavePerID.Name = "chbSavePerID";
            this.chbSavePerID.Size = new System.Drawing.Size(167, 22);
            this.chbSavePerID.TabIndex = 48;
            this.chbSavePerID.Text = "Salvataggio per ID";
            this.chbSavePerID.UseVisualStyleBackColor = true;
            this.chbSavePerID.CheckedChanged += new System.EventHandler(this.chbSavePerID_CheckedChanged);
            // 
            // btnAnnulaSaveDB
            // 
            this.btnAnnulaSaveDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnulaSaveDB.Location = new System.Drawing.Point(565, 96);
            this.btnAnnulaSaveDB.Name = "btnAnnulaSaveDB";
            this.btnAnnulaSaveDB.Size = new System.Drawing.Size(90, 31);
            this.btnAnnulaSaveDB.TabIndex = 47;
            this.btnAnnulaSaveDB.Text = "Annulla";
            this.btnAnnulaSaveDB.UseVisualStyleBackColor = true;
            this.btnAnnulaSaveDB.Click += new System.EventHandler(this.btnAnnulaSaveDB_Click);
            // 
            // btnOkSaveDB
            // 
            this.btnOkSaveDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOkSaveDB.Location = new System.Drawing.Point(496, 96);
            this.btnOkSaveDB.Name = "btnOkSaveDB";
            this.btnOkSaveDB.Size = new System.Drawing.Size(65, 31);
            this.btnOkSaveDB.TabIndex = 46;
            this.btnOkSaveDB.Text = "Ok";
            this.btnOkSaveDB.UseVisualStyleBackColor = true;
            this.btnOkSaveDB.Click += new System.EventHandler(this.btnOkSaveDB_Click);
            // 
            // btnSaveDb
            // 
            this.btnSaveDb.Location = new System.Drawing.Point(614, 44);
            this.btnSaveDb.Name = "btnSaveDb";
            this.btnSaveDb.Size = new System.Drawing.Size(43, 39);
            this.btnSaveDb.TabIndex = 45;
            this.btnSaveDb.Text = "...";
            this.btnSaveDb.UseVisualStyleBackColor = true;
            this.btnSaveDb.Click += new System.EventHandler(this.btnSaveDb_Click);
            // 
            // txtDbSave
            // 
            this.txtDbSave.Location = new System.Drawing.Point(15, 44);
            this.txtDbSave.Name = "txtDbSave";
            this.txtDbSave.Size = new System.Drawing.Size(593, 22);
            this.txtDbSave.TabIndex = 44;
            // 
            // lblDenSelDb
            // 
            this.lblDenSelDb.AutoSize = true;
            this.lblDenSelDb.Location = new System.Drawing.Point(12, 25);
            this.lblDenSelDb.Name = "lblDenSelDb";
            this.lblDenSelDb.Size = new System.Drawing.Size(121, 16);
            this.lblDenSelDb.TabIndex = 43;
            this.lblDenSelDb.Text = "Database name:";
            // 
            // gbSelRequest
            // 
            this.gbSelRequest.Controls.Add(this.lblRqId);
            this.gbSelRequest.Controls.Add(this.lblRqDbSel);
            this.gbSelRequest.Controls.Add(this.lblDenRqDbSel);
            this.gbSelRequest.Controls.Add(this.chbRqPerId);
            this.gbSelRequest.Controls.Add(this.btnSelRqDb);
            this.gbSelRequest.Controls.Add(this.txtDbRq);
            this.gbSelRequest.Controls.Add(this.dbStructReqAnnulla);
            this.gbSelRequest.Controls.Add(this.BtnDbStructRecOk);
            this.gbSelRequest.Controls.Add(this.rbFull);
            this.gbSelRequest.Controls.Add(this.rbStructureOnly);
            this.gbSelRequest.Controls.Add(this.rbListOnly);
            this.gbSelRequest.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSelRequest.Location = new System.Drawing.Point(167, 25);
            this.gbSelRequest.Name = "gbSelRequest";
            this.gbSelRequest.Size = new System.Drawing.Size(718, 144);
            this.gbSelRequest.TabIndex = 41;
            this.gbSelRequest.TabStop = false;
            this.gbSelRequest.Text = "Definizione richieste info al DB";
            this.gbSelRequest.Visible = false;
            // 
            // lblRqId
            // 
            this.lblRqId.AutoSize = true;
            this.lblRqId.Location = new System.Drawing.Point(192, 107);
            this.lblRqId.Name = "lblRqId";
            this.lblRqId.Size = new System.Drawing.Size(19, 16);
            this.lblRqId.TabIndex = 54;
            this.lblRqId.Text = "...";
            this.lblRqId.Visible = false;
            // 
            // lblRqDbSel
            // 
            this.lblRqDbSel.AutoSize = true;
            this.lblRqDbSel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRqDbSel.Location = new System.Drawing.Point(191, 85);
            this.lblRqDbSel.Name = "lblRqDbSel";
            this.lblRqDbSel.Size = new System.Drawing.Size(41, 18);
            this.lblRqDbSel.TabIndex = 53;
            this.lblRqDbSel.Text = "none";
            // 
            // lblDenRqDbSel
            // 
            this.lblDenRqDbSel.AutoSize = true;
            this.lblDenRqDbSel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenRqDbSel.Location = new System.Drawing.Point(17, 85);
            this.lblDenRqDbSel.Name = "lblDenRqDbSel";
            this.lblDenRqDbSel.Size = new System.Drawing.Size(175, 18);
            this.lblDenRqDbSel.TabIndex = 52;
            this.lblDenRqDbSel.Text = "Database selezionate:";
            // 
            // chbRqPerId
            // 
            this.chbRqPerId.AutoSize = true;
            this.chbRqPerId.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chbRqPerId.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chbRqPerId.Location = new System.Drawing.Point(18, 107);
            this.chbRqPerId.Name = "chbRqPerId";
            this.chbRqPerId.Size = new System.Drawing.Size(150, 22);
            this.chbRqPerId.TabIndex = 51;
            this.chbRqPerId.Text = "Richiesta per ID";
            this.chbRqPerId.UseVisualStyleBackColor = true;
            this.chbRqPerId.CheckedChanged += new System.EventHandler(this.chbRqPerId_CheckedChanged);
            // 
            // btnSelRqDb
            // 
            this.btnSelRqDb.Location = new System.Drawing.Point(610, 21);
            this.btnSelRqDb.Name = "btnSelRqDb";
            this.btnSelRqDb.Size = new System.Drawing.Size(43, 39);
            this.btnSelRqDb.TabIndex = 50;
            this.btnSelRqDb.Text = "...";
            this.btnSelRqDb.UseVisualStyleBackColor = true;
            this.btnSelRqDb.Click += new System.EventHandler(this.btnSelRqDb_Click);
            // 
            // txtDbRq
            // 
            this.txtDbRq.Location = new System.Drawing.Point(13, 21);
            this.txtDbRq.Name = "txtDbRq";
            this.txtDbRq.Size = new System.Drawing.Size(593, 22);
            this.txtDbRq.TabIndex = 49;
            // 
            // dbStructReqAnnulla
            // 
            this.dbStructReqAnnulla.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dbStructReqAnnulla.Location = new System.Drawing.Point(563, 97);
            this.dbStructReqAnnulla.Name = "dbStructReqAnnulla";
            this.dbStructReqAnnulla.Size = new System.Drawing.Size(90, 31);
            this.dbStructReqAnnulla.TabIndex = 48;
            this.dbStructReqAnnulla.Text = "Annulla";
            this.dbStructReqAnnulla.UseVisualStyleBackColor = true;
            this.dbStructReqAnnulla.Click += new System.EventHandler(this.dbStructReqAnnulla_Click);
            // 
            // BtnDbStructRecOk
            // 
            this.BtnDbStructRecOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnDbStructRecOk.Location = new System.Drawing.Point(492, 97);
            this.BtnDbStructRecOk.Name = "BtnDbStructRecOk";
            this.BtnDbStructRecOk.Size = new System.Drawing.Size(65, 31);
            this.BtnDbStructRecOk.TabIndex = 47;
            this.BtnDbStructRecOk.Text = "Ok";
            this.BtnDbStructRecOk.UseVisualStyleBackColor = true;
            this.BtnDbStructRecOk.Click += new System.EventHandler(this.BtnDbStructRecOk_Click);
            // 
            // rbFull
            // 
            this.rbFull.AutoSize = true;
            this.rbFull.Checked = true;
            this.rbFull.Location = new System.Drawing.Point(274, 61);
            this.rbFull.Name = "rbFull";
            this.rbFull.Size = new System.Drawing.Size(63, 20);
            this.rbFull.TabIndex = 2;
            this.rbFull.TabStop = true;
            this.rbFull.Tag = "Full";
            this.rbFull.Text = "Tutto";
            this.rbFull.UseVisualStyleBackColor = true;
            // 
            // rbStructureOnly
            // 
            this.rbStructureOnly.AutoSize = true;
            this.rbStructureOnly.Location = new System.Drawing.Point(133, 61);
            this.rbStructureOnly.Name = "rbStructureOnly";
            this.rbStructureOnly.Size = new System.Drawing.Size(119, 20);
            this.rbStructureOnly.TabIndex = 1;
            this.rbStructureOnly.Tag = "StructureOnly";
            this.rbStructureOnly.Text = "Solo struttura";
            this.rbStructureOnly.UseVisualStyleBackColor = true;
            // 
            // rbListOnly
            // 
            this.rbListOnly.AutoSize = true;
            this.rbListOnly.Location = new System.Drawing.Point(18, 61);
            this.rbListOnly.Name = "rbListOnly";
            this.rbListOnly.Size = new System.Drawing.Size(93, 20);
            this.rbListOnly.TabIndex = 0;
            this.rbListOnly.Tag = "ListOnly";
            this.rbListOnly.Text = "Solo lista";
            this.rbListOnly.UseVisualStyleBackColor = true;
            // 
            // FrmTelegram
            // 
            this.AcceptButton = this.btnOkDb;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnAnnullaSend;
            this.ClientSize = new System.Drawing.Size(818, 646);
            this.Controls.Add(this.gbActionSaveDatabase);
            this.Controls.Add(this.gbCompilaBufferTx);
            this.Controls.Add(this.pnlbtn);
            this.Controls.Add(this.gbSelCmd);
            this.Controls.Add(this.gbActionTypeLoadDb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmTelegram";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configurazione telegramma";
            this.Load += new System.EventHandler(this.FrmTelegram_Load);
            this.gbSelCmd.ResumeLayout(false);
            this.gbSelCmd.PerformLayout();
            this.pnlbtn.ResumeLayout(false);
            this.gbCompilaBufferTx.ResumeLayout(false);
            this.gbActionTypeLoadDb.ResumeLayout(false);
            this.gbActionTypeLoadDb.PerformLayout();
            this.gbActionSaveDatabase.ResumeLayout(false);
            this.gbActionSaveDatabase.PerformLayout();
            this.gbSelRequest.ResumeLayout(false);
            this.gbSelRequest.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnAnnullaSend;
        private System.Windows.Forms.GroupBox gbSelCmd;
        private System.Windows.Forms.ComboBox cmbCommand;
        private System.Windows.Forms.TextBox txtDateSend;
        private System.Windows.Forms.Label lblSendTime;
        private System.Windows.Forms.Label lblCommand;
        private System.Windows.Forms.Panel pnlbtn;
        private System.Windows.Forms.GroupBox gbCompilaBufferTx;
        private System.Windows.Forms.RichTextBox rtxtBuffer;
        private System.Windows.Forms.Label lblDenCalcolaTocken;
        private System.Windows.Forms.Button btnCalcTocken;
        private System.Windows.Forms.Label lblToken;
        private System.Windows.Forms.CheckBox chkbCRC;
        private System.Windows.Forms.GroupBox gbActionTypeLoadDb;
        private System.Windows.Forms.Button btnSelDb;
        private System.Windows.Forms.TextBox txtFileDb;
        private System.Windows.Forms.Label lblDenDbName;
        private System.Windows.Forms.Button btnSendMsg;
        private System.Windows.Forms.Button bgtnAnnullaDb;
        private System.Windows.Forms.Button btnOkDb;
        private System.Windows.Forms.Label lblDenDb;
        private System.Windows.Forms.Button btnOkSend;
        private System.Windows.Forms.GroupBox gbActionSaveDatabase;
        private System.Windows.Forms.Button btnSaveDb;
        private System.Windows.Forms.TextBox txtDbSave;
        private System.Windows.Forms.Label lblDenSelDb;
        private System.Windows.Forms.CheckBox chbSavePerID;
        private System.Windows.Forms.Button btnAnnulaSaveDB;
        private System.Windows.Forms.Button btnOkSaveDB;
        private System.Windows.Forms.Label lblDenIdSel;
        private System.Windows.Forms.Label lblDbSelect;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.Label lblIdSel;
        private System.Windows.Forms.Label lblDenDbIdSel;
        private System.Windows.Forms.Label lblDb;
        private System.Windows.Forms.GroupBox gbSelRequest;
        private System.Windows.Forms.RadioButton rbFull;
        private System.Windows.Forms.RadioButton rbStructureOnly;
        private System.Windows.Forms.RadioButton rbListOnly;
        private System.Windows.Forms.Button dbStructReqAnnulla;
        private System.Windows.Forms.Button BtnDbStructRecOk;
        private System.Windows.Forms.RichTextBox rtxtMonitor;
        private System.Windows.Forms.Label lblMonitor;
        private System.Windows.Forms.Button btnSelRqDb;
        private System.Windows.Forms.TextBox txtDbRq;
        private System.Windows.Forms.CheckBox chbRqPerId;
        private System.Windows.Forms.Label lblRqDbSel;
        private System.Windows.Forms.Label lblDenRqDbSel;
        private System.Windows.Forms.Label lblRqId;
    }
}