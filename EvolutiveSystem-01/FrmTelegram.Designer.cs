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
            this.btnSendMsg = new System.Windows.Forms.Button();
            this.gbComponiTelegramma = new System.Windows.Forms.GroupBox();
            this.btnCalcTocken = new System.Windows.Forms.Button();
            this.lblToken = new System.Windows.Forms.Label();
            this.txtBuffer = new System.Windows.Forms.TextBox();
            this.txtDateSend = new System.Windows.Forms.TextBox();
            this.txtCmd = new System.Windows.Forms.TextBox();
            this.chkbCRC = new System.Windows.Forms.CheckBox();
            this.lblDenToken = new System.Windows.Forms.Label();
            this.lblBufferTx = new System.Windows.Forms.Label();
            this.lblSendTime = new System.Windows.Forms.Label();
            this.lblCommand = new System.Windows.Forms.Label();
            this.gbComponiTelegramma.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAnnullaSend
            // 
            this.btnAnnullaSend.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnAnnullaSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnullaSend.Location = new System.Drawing.Point(153, 296);
            this.btnAnnullaSend.Name = "btnAnnullaSend";
            this.btnAnnullaSend.Size = new System.Drawing.Size(75, 31);
            this.btnAnnullaSend.TabIndex = 13;
            this.btnAnnullaSend.Text = "Annulla";
            this.btnAnnullaSend.UseVisualStyleBackColor = true;
            this.btnAnnullaSend.Click += new System.EventHandler(this.btnAnnullaSend_Click);
            // 
            // btnSendMsg
            // 
            this.btnSendMsg.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSendMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendMsg.Location = new System.Drawing.Point(7, 296);
            this.btnSendMsg.Name = "btnSendMsg";
            this.btnSendMsg.Size = new System.Drawing.Size(140, 31);
            this.btnSendMsg.TabIndex = 12;
            this.btnSendMsg.Text = "Prepara telegramma";
            this.btnSendMsg.UseVisualStyleBackColor = true;
            this.btnSendMsg.Click += new System.EventHandler(this.btnSendMsg_Click);
            // 
            // gbComponiTelegramma
            // 
            this.gbComponiTelegramma.Controls.Add(this.btnCalcTocken);
            this.gbComponiTelegramma.Controls.Add(this.lblToken);
            this.gbComponiTelegramma.Controls.Add(this.txtBuffer);
            this.gbComponiTelegramma.Controls.Add(this.txtDateSend);
            this.gbComponiTelegramma.Controls.Add(this.txtCmd);
            this.gbComponiTelegramma.Controls.Add(this.chkbCRC);
            this.gbComponiTelegramma.Controls.Add(this.lblDenToken);
            this.gbComponiTelegramma.Controls.Add(this.lblBufferTx);
            this.gbComponiTelegramma.Controls.Add(this.lblSendTime);
            this.gbComponiTelegramma.Controls.Add(this.lblCommand);
            this.gbComponiTelegramma.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbComponiTelegramma.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbComponiTelegramma.Location = new System.Drawing.Point(0, 0);
            this.gbComponiTelegramma.Name = "gbComponiTelegramma";
            this.gbComponiTelegramma.Size = new System.Drawing.Size(390, 290);
            this.gbComponiTelegramma.TabIndex = 14;
            this.gbComponiTelegramma.TabStop = false;
            this.gbComponiTelegramma.Text = "Parametri telegramma";
            // 
            // btnCalcTocken
            // 
            this.btnCalcTocken.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCalcTocken.Location = new System.Drawing.Point(284, 238);
            this.btnCalcTocken.Name = "btnCalcTocken";
            this.btnCalcTocken.Size = new System.Drawing.Size(83, 28);
            this.btnCalcTocken.TabIndex = 31;
            this.btnCalcTocken.Text = "Calcola";
            this.btnCalcTocken.UseVisualStyleBackColor = true;
            this.btnCalcTocken.Click += new System.EventHandler(this.btnCalcTocken_Click);
            // 
            // lblToken
            // 
            this.lblToken.AutoSize = true;
            this.lblToken.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToken.Location = new System.Drawing.Point(130, 238);
            this.lblToken.Name = "lblToken";
            this.lblToken.Size = new System.Drawing.Size(16, 16);
            this.lblToken.TabIndex = 32;
            this.lblToken.Text = "...";
            // 
            // txtBuffer
            // 
            this.txtBuffer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBuffer.Location = new System.Drawing.Point(11, 122);
            this.txtBuffer.Multiline = true;
            this.txtBuffer.Name = "txtBuffer";
            this.txtBuffer.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBuffer.Size = new System.Drawing.Size(356, 94);
            this.txtBuffer.TabIndex = 30;
            // 
            // txtDateSend
            // 
            this.txtDateSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDateSend.Location = new System.Drawing.Point(158, 73);
            this.txtDateSend.Name = "txtDateSend";
            this.txtDateSend.Size = new System.Drawing.Size(209, 24);
            this.txtDateSend.TabIndex = 29;
            // 
            // txtCmd
            // 
            this.txtCmd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCmd.Location = new System.Drawing.Point(158, 43);
            this.txtCmd.Name = "txtCmd";
            this.txtCmd.Size = new System.Drawing.Size(138, 24);
            this.txtCmd.TabIndex = 28;
            // 
            // chkbCRC
            // 
            this.chkbCRC.AutoSize = true;
            this.chkbCRC.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkbCRC.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkbCRC.Location = new System.Drawing.Point(39, 263);
            this.chkbCRC.Name = "chkbCRC";
            this.chkbCRC.Size = new System.Drawing.Size(108, 20);
            this.chkbCRC.TabIndex = 33;
            this.chkbCRC.Text = "Abilita CRC";
            this.chkbCRC.UseVisualStyleBackColor = true;
            // 
            // lblDenToken
            // 
            this.lblDenToken.AutoSize = true;
            this.lblDenToken.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenToken.Location = new System.Drawing.Point(69, 238);
            this.lblDenToken.Name = "lblDenToken";
            this.lblDenToken.Size = new System.Drawing.Size(55, 16);
            this.lblDenToken.TabIndex = 27;
            this.lblDenToken.Text = "Token:";
            // 
            // lblBufferTx
            // 
            this.lblBufferTx.AutoSize = true;
            this.lblBufferTx.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBufferTx.Location = new System.Drawing.Point(4, 103);
            this.lblBufferTx.Name = "lblBufferTx";
            this.lblBufferTx.Size = new System.Drawing.Size(143, 16);
            this.lblBufferTx.TabIndex = 26;
            this.lblBufferTx.Text = "Buffer trasmissione:";
            // 
            // lblSendTime
            // 
            this.lblSendTime.AutoSize = true;
            this.lblSendTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSendTime.Location = new System.Drawing.Point(66, 73);
            this.lblSendTime.Name = "lblSendTime";
            this.lblSendTime.Size = new System.Drawing.Size(81, 16);
            this.lblSendTime.TabIndex = 25;
            this.lblSendTime.Text = "Data invio:";
            // 
            // lblCommand
            // 
            this.lblCommand.AutoSize = true;
            this.lblCommand.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCommand.Location = new System.Drawing.Point(70, 43);
            this.lblCommand.Name = "lblCommand";
            this.lblCommand.Size = new System.Drawing.Size(77, 16);
            this.lblCommand.TabIndex = 24;
            this.lblCommand.Text = "Comando:";
            // 
            // FrmTelegram
            // 
            this.AcceptButton = this.btnSendMsg;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnAnnullaSend;
            this.ClientSize = new System.Drawing.Size(390, 339);
            this.Controls.Add(this.gbComponiTelegramma);
            this.Controls.Add(this.btnAnnullaSend);
            this.Controls.Add(this.btnSendMsg);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmTelegram";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configurazione telegramma";
            this.gbComponiTelegramma.ResumeLayout(false);
            this.gbComponiTelegramma.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnAnnullaSend;
        private System.Windows.Forms.Button btnSendMsg;
        private System.Windows.Forms.GroupBox gbComponiTelegramma;
        private System.Windows.Forms.Button btnCalcTocken;
        private System.Windows.Forms.Label lblToken;
        private System.Windows.Forms.TextBox txtBuffer;
        private System.Windows.Forms.TextBox txtDateSend;
        private System.Windows.Forms.TextBox txtCmd;
        private System.Windows.Forms.CheckBox chkbCRC;
        private System.Windows.Forms.Label lblDenToken;
        private System.Windows.Forms.Label lblBufferTx;
        private System.Windows.Forms.Label lblSendTime;
        private System.Windows.Forms.Label lblCommand;
    }
}