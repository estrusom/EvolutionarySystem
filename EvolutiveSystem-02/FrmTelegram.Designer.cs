namespace EvolutiveSystem_02
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
            this.gbConfigFunzione = new System.Windows.Forms.GroupBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.gbSelCmd.SuspendLayout();
            this.pnlbtn.SuspendLayout();
            this.gbCompilaBufferTx.SuspendLayout();
            this.gbConfigFunzione.SuspendLayout();
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
            this.gbSelCmd.Controls.Add(this.gbConfigFunzione);
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
            this.gbSelCmd.Size = new System.Drawing.Size(1019, 410);
            this.gbSelCmd.TabIndex = 36;
            this.gbSelCmd.TabStop = false;
            this.gbSelCmd.Text = "Seezione comandi e parametri di trasmissione";
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
            this.chkbCRC.Location = new System.Drawing.Point(458, 65);
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
            this.cmbCommand.Size = new System.Drawing.Size(828, 28);
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
            this.pnlbtn.Location = new System.Drawing.Point(0, 687);
            this.pnlbtn.Name = "pnlbtn";
            this.pnlbtn.Size = new System.Drawing.Size(1019, 62);
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
            this.gbCompilaBufferTx.Location = new System.Drawing.Point(0, 503);
            this.gbCompilaBufferTx.Name = "gbCompilaBufferTx";
            this.gbCompilaBufferTx.Size = new System.Drawing.Size(1019, 184);
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
            this.rtxtBuffer.Size = new System.Drawing.Size(1013, 163);
            this.rtxtBuffer.TabIndex = 0;
            this.rtxtBuffer.Text = "";
            // 
            // gbConfigFunzione
            // 
            this.gbConfigFunzione.Controls.Add(this.lblTitle);
            this.gbConfigFunzione.Location = new System.Drawing.Point(50, 173);
            this.gbConfigFunzione.Name = "gbConfigFunzione";
            this.gbConfigFunzione.Size = new System.Drawing.Size(938, 231);
            this.gbConfigFunzione.TabIndex = 49;
            this.gbConfigFunzione.TabStop = false;
            this.gbConfigFunzione.Text = "Configurazione comando";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(7, 24);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(46, 18);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "label1";
            // 
            // FrmTelegram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnAnnullaSend;
            this.ClientSize = new System.Drawing.Size(1019, 749);
            this.Controls.Add(this.gbCompilaBufferTx);
            this.Controls.Add(this.pnlbtn);
            this.Controls.Add(this.gbSelCmd);
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
            this.gbConfigFunzione.ResumeLayout(false);
            this.gbConfigFunzione.PerformLayout();
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
        private System.Windows.Forms.Button btnSendMsg;
        private System.Windows.Forms.Label lblDenDb;
        private System.Windows.Forms.Button btnOkSend;
        private System.Windows.Forms.Label lblDb;
        private System.Windows.Forms.GroupBox gbConfigFunzione;
        private System.Windows.Forms.Label lblTitle;
    }
}