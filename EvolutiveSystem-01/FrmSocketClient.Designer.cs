namespace EvolutiveSystem_01
{
    partial class FrmSocketClient
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSocketClient));
            this.pnlCommand = new System.Windows.Forms.Panel();
            this.pnlCmdSocket = new System.Windows.Forms.Panel();
            this.btnCommand = new System.Windows.Forms.Button();
            this.pnlGestSocket = new System.Windows.Forms.Panel();
            this.btnCloseConnection = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.gpCoponiComanado = new System.Windows.Forms.GroupBox();
            this.btnAnnullaSend = new System.Windows.Forms.Button();
            this.btnSendMsg = new System.Windows.Forms.Button();
            this.lblToken = new System.Windows.Forms.Label();
            this.txtBuffer = new System.Windows.Forms.TextBox();
            this.txtDateSend = new System.Windows.Forms.TextBox();
            this.txtCmd = new System.Windows.Forms.TextBox();
            this.chkbCRC = new System.Windows.Forms.CheckBox();
            this.lblDenToken = new System.Windows.Forms.Label();
            this.lblBufferTx = new System.Windows.Forms.Label();
            this.lblSendTime = new System.Windows.Forms.Label();
            this.lblCommand = new System.Windows.Forms.Label();
            this.gbConnectTo = new System.Windows.Forms.GroupBox();
            this.txtIPport = new System.Windows.Forms.TextBox();
            this.lblIpPort = new System.Windows.Forms.Label();
            this.txtIPaddress = new System.Windows.Forms.TextBox();
            this.lblIpAddress = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tssDenComStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssComStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.gbSendData = new System.Windows.Forms.GroupBox();
            this.txtSendData = new System.Windows.Forms.TextBox();
            this.gbComRx = new System.Windows.Forms.GroupBox();
            this.rtbBufferRx = new System.Windows.Forms.RichTextBox();
            this.btnCalcTocken = new System.Windows.Forms.Button();
            this.pnlCommand.SuspendLayout();
            this.pnlCmdSocket.SuspendLayout();
            this.pnlGestSocket.SuspendLayout();
            this.gpCoponiComanado.SuspendLayout();
            this.gbConnectTo.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.gbSendData.SuspendLayout();
            this.gbComRx.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlCommand
            // 
            this.pnlCommand.Controls.Add(this.pnlCmdSocket);
            this.pnlCommand.Controls.Add(this.pnlGestSocket);
            this.pnlCommand.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCommand.Location = new System.Drawing.Point(0, 0);
            this.pnlCommand.Name = "pnlCommand";
            this.pnlCommand.Size = new System.Drawing.Size(1123, 49);
            this.pnlCommand.TabIndex = 1;
            // 
            // pnlCmdSocket
            // 
            this.pnlCmdSocket.Controls.Add(this.btnCommand);
            this.pnlCmdSocket.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlCmdSocket.Location = new System.Drawing.Point(168, 0);
            this.pnlCmdSocket.Name = "pnlCmdSocket";
            this.pnlCmdSocket.Size = new System.Drawing.Size(200, 49);
            this.pnlCmdSocket.TabIndex = 4;
            // 
            // btnCommand
            // 
            this.btnCommand.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.make_15198;
            this.btnCommand.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCommand.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnCommand.Location = new System.Drawing.Point(0, 0);
            this.btnCommand.Name = "btnCommand";
            this.btnCommand.Size = new System.Drawing.Size(53, 49);
            this.btnCommand.TabIndex = 6;
            this.btnCommand.UseVisualStyleBackColor = true;
            // 
            // pnlGestSocket
            // 
            this.pnlGestSocket.Controls.Add(this.btnCloseConnection);
            this.pnlGestSocket.Controls.Add(this.btnSend);
            this.pnlGestSocket.Controls.Add(this.btnConnect);
            this.pnlGestSocket.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlGestSocket.Location = new System.Drawing.Point(0, 0);
            this.pnlGestSocket.Name = "pnlGestSocket";
            this.pnlGestSocket.Size = new System.Drawing.Size(168, 49);
            this.pnlGestSocket.TabIndex = 3;
            // 
            // btnCloseConnection
            // 
            this.btnCloseConnection.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.close_14776;
            this.btnCloseConnection.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCloseConnection.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnCloseConnection.Location = new System.Drawing.Point(106, 0);
            this.btnCloseConnection.Name = "btnCloseConnection";
            this.btnCloseConnection.Size = new System.Drawing.Size(53, 49);
            this.btnCloseConnection.TabIndex = 5;
            this.btnCloseConnection.UseVisualStyleBackColor = true;
            // 
            // btnSend
            // 
            this.btnSend.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.iconfinder_send_4341325_120524;
            this.btnSend.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSend.Location = new System.Drawing.Point(53, 0);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(53, 49);
            this.btnSend.TabIndex = 4;
            this.btnSend.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            this.btnConnect.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.screenshot;
            this.btnConnect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnConnect.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnConnect.Location = new System.Drawing.Point(0, 0);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(53, 49);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.UseVisualStyleBackColor = true;
            // 
            // gpCoponiComanado
            // 
            this.gpCoponiComanado.Controls.Add(this.btnCalcTocken);
            this.gpCoponiComanado.Controls.Add(this.btnAnnullaSend);
            this.gpCoponiComanado.Controls.Add(this.btnSendMsg);
            this.gpCoponiComanado.Controls.Add(this.lblToken);
            this.gpCoponiComanado.Controls.Add(this.txtBuffer);
            this.gpCoponiComanado.Controls.Add(this.txtDateSend);
            this.gpCoponiComanado.Controls.Add(this.txtCmd);
            this.gpCoponiComanado.Controls.Add(this.chkbCRC);
            this.gpCoponiComanado.Controls.Add(this.lblDenToken);
            this.gpCoponiComanado.Controls.Add(this.lblBufferTx);
            this.gpCoponiComanado.Controls.Add(this.lblSendTime);
            this.gpCoponiComanado.Controls.Add(this.lblCommand);
            this.gpCoponiComanado.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gpCoponiComanado.Location = new System.Drawing.Point(415, 323);
            this.gpCoponiComanado.Name = "gpCoponiComanado";
            this.gpCoponiComanado.Size = new System.Drawing.Size(374, 327);
            this.gpCoponiComanado.TabIndex = 2;
            this.gpCoponiComanado.TabStop = false;
            this.gpCoponiComanado.Text = "Composizione comando ";
            this.gpCoponiComanado.Visible = false;
            // 
            // btnAnnullaSend
            // 
            this.btnAnnullaSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnullaSend.Location = new System.Drawing.Point(158, 286);
            this.btnAnnullaSend.Name = "btnAnnullaSend";
            this.btnAnnullaSend.Size = new System.Drawing.Size(75, 31);
            this.btnAnnullaSend.TabIndex = 11;
            this.btnAnnullaSend.Text = "Annulla";
            this.btnAnnullaSend.UseVisualStyleBackColor = true;
            this.btnAnnullaSend.Click += new System.EventHandler(this.btnAnnullaSend_Click);
            // 
            // btnSendMsg
            // 
            this.btnSendMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendMsg.Location = new System.Drawing.Point(12, 286);
            this.btnSendMsg.Name = "btnSendMsg";
            this.btnSendMsg.Size = new System.Drawing.Size(140, 31);
            this.btnSendMsg.TabIndex = 10;
            this.btnSendMsg.Text = "Prepara telegramma";
            this.btnSendMsg.UseVisualStyleBackColor = true;
            this.btnSendMsg.Click += new System.EventHandler(this.btnSendMsg_Click);
            // 
            // lblToken
            // 
            this.lblToken.AutoSize = true;
            this.lblToken.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToken.Location = new System.Drawing.Point(137, 228);
            this.lblToken.Name = "lblToken";
            this.lblToken.Size = new System.Drawing.Size(16, 16);
            this.lblToken.TabIndex = 8;
            this.lblToken.Text = "...";
            // 
            // txtBuffer
            // 
            this.txtBuffer.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBuffer.Location = new System.Drawing.Point(12, 115);
            this.txtBuffer.Multiline = true;
            this.txtBuffer.Name = "txtBuffer";
            this.txtBuffer.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBuffer.Size = new System.Drawing.Size(356, 94);
            this.txtBuffer.TabIndex = 7;
            // 
            // txtDateSend
            // 
            this.txtDateSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDateSend.Location = new System.Drawing.Point(159, 66);
            this.txtDateSend.Name = "txtDateSend";
            this.txtDateSend.Size = new System.Drawing.Size(209, 22);
            this.txtDateSend.TabIndex = 6;
            // 
            // txtCmd
            // 
            this.txtCmd.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCmd.Location = new System.Drawing.Point(159, 36);
            this.txtCmd.Name = "txtCmd";
            this.txtCmd.Size = new System.Drawing.Size(138, 22);
            this.txtCmd.TabIndex = 5;
            // 
            // chkbCRC
            // 
            this.chkbCRC.AutoSize = true;
            this.chkbCRC.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkbCRC.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkbCRC.Location = new System.Drawing.Point(40, 256);
            this.chkbCRC.Name = "chkbCRC";
            this.chkbCRC.Size = new System.Drawing.Size(108, 20);
            this.chkbCRC.TabIndex = 9;
            this.chkbCRC.Text = "Abilita CRC";
            this.chkbCRC.UseVisualStyleBackColor = true;
            // 
            // lblDenToken
            // 
            this.lblDenToken.AutoSize = true;
            this.lblDenToken.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenToken.Location = new System.Drawing.Point(93, 231);
            this.lblDenToken.Name = "lblDenToken";
            this.lblDenToken.Size = new System.Drawing.Size(55, 16);
            this.lblDenToken.TabIndex = 3;
            this.lblDenToken.Text = "Token:";
            // 
            // lblBufferTx
            // 
            this.lblBufferTx.AutoSize = true;
            this.lblBufferTx.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBufferTx.Location = new System.Drawing.Point(5, 96);
            this.lblBufferTx.Name = "lblBufferTx";
            this.lblBufferTx.Size = new System.Drawing.Size(143, 16);
            this.lblBufferTx.TabIndex = 2;
            this.lblBufferTx.Text = "Buffer trasmissione:";
            // 
            // lblSendTime
            // 
            this.lblSendTime.AutoSize = true;
            this.lblSendTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSendTime.Location = new System.Drawing.Point(67, 66);
            this.lblSendTime.Name = "lblSendTime";
            this.lblSendTime.Size = new System.Drawing.Size(81, 16);
            this.lblSendTime.TabIndex = 1;
            this.lblSendTime.Text = "Data invio:";
            // 
            // lblCommand
            // 
            this.lblCommand.AutoSize = true;
            this.lblCommand.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCommand.Location = new System.Drawing.Point(71, 36);
            this.lblCommand.Name = "lblCommand";
            this.lblCommand.Size = new System.Drawing.Size(77, 16);
            this.lblCommand.TabIndex = 0;
            this.lblCommand.Text = "Comando:";
            // 
            // gbConnectTo
            // 
            this.gbConnectTo.Controls.Add(this.txtIPport);
            this.gbConnectTo.Controls.Add(this.lblIpPort);
            this.gbConnectTo.Controls.Add(this.txtIPaddress);
            this.gbConnectTo.Controls.Add(this.lblIpAddress);
            this.gbConnectTo.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbConnectTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbConnectTo.Location = new System.Drawing.Point(0, 49);
            this.gbConnectTo.Name = "gbConnectTo";
            this.gbConnectTo.Size = new System.Drawing.Size(1123, 115);
            this.gbConnectTo.TabIndex = 2;
            this.gbConnectTo.TabStop = false;
            this.gbConnectTo.Text = "Collegamento al server";
            // 
            // txtIPport
            // 
            this.txtIPport.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtIPport.Location = new System.Drawing.Point(104, 73);
            this.txtIPport.Name = "txtIPport";
            this.txtIPport.Size = new System.Drawing.Size(157, 27);
            this.txtIPport.TabIndex = 3;
            this.txtIPport.Enter += new System.EventHandler(this.txtIPport_Enter);
            this.txtIPport.Leave += new System.EventHandler(this.txtIPport_Leave);
            // 
            // lblIpPort
            // 
            this.lblIpPort.AutoSize = true;
            this.lblIpPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIpPort.Location = new System.Drawing.Point(37, 73);
            this.lblIpPort.Name = "lblIpPort";
            this.lblIpPort.Size = new System.Drawing.Size(60, 20);
            this.lblIpPort.TabIndex = 2;
            this.lblIpPort.Text = "IP Port";
            // 
            // txtIPaddress
            // 
            this.txtIPaddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtIPaddress.Location = new System.Drawing.Point(104, 29);
            this.txtIPaddress.Name = "txtIPaddress";
            this.txtIPaddress.Size = new System.Drawing.Size(656, 27);
            this.txtIPaddress.TabIndex = 1;
            this.txtIPaddress.Enter += new System.EventHandler(this.txtIPaddress_Enter);
            this.txtIPaddress.Leave += new System.EventHandler(this.txtIPaddress_Leave);
            // 
            // lblIpAddress
            // 
            this.lblIpAddress.AutoSize = true;
            this.lblIpAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIpAddress.Location = new System.Drawing.Point(6, 29);
            this.lblIpAddress.Name = "lblIpAddress";
            this.lblIpAddress.Size = new System.Drawing.Size(91, 20);
            this.lblIpAddress.TabIndex = 0;
            this.lblIpAddress.Text = "IP Address";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssDenComStatus,
            this.tssComStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 688);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1123, 25);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tssDenComStatus
            // 
            this.tssDenComStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tssDenComStatus.Name = "tssDenComStatus";
            this.tssDenComStatus.Size = new System.Drawing.Size(142, 19);
            this.tssDenComStatus.Text = "toolStripStatusLabel1";
            // 
            // tssComStatus
            // 
            this.tssComStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.tssComStatus.ForeColor = System.Drawing.Color.Red;
            this.tssComStatus.Name = "tssComStatus";
            this.tssComStatus.Size = new System.Drawing.Size(42, 19);
            this.tssComStatus.Text = "Close";
            // 
            // gbSendData
            // 
            this.gbSendData.Controls.Add(this.txtSendData);
            this.gbSendData.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSendData.Location = new System.Drawing.Point(0, 164);
            this.gbSendData.Name = "gbSendData";
            this.gbSendData.Size = new System.Drawing.Size(1123, 255);
            this.gbSendData.TabIndex = 5;
            this.gbSendData.TabStop = false;
            this.gbSendData.Text = "Dati da trasmettere";
            // 
            // txtSendData
            // 
            this.txtSendData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSendData.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSendData.Location = new System.Drawing.Point(3, 23);
            this.txtSendData.Multiline = true;
            this.txtSendData.Name = "txtSendData";
            this.txtSendData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSendData.Size = new System.Drawing.Size(1117, 229);
            this.txtSendData.TabIndex = 1;
            this.txtSendData.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSendData_KeyPress);
            // 
            // gbComRx
            // 
            this.gbComRx.Controls.Add(this.rtbBufferRx);
            this.gbComRx.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gbComRx.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbComRx.Location = new System.Drawing.Point(0, 433);
            this.gbComRx.Name = "gbComRx";
            this.gbComRx.Size = new System.Drawing.Size(1123, 255);
            this.gbComRx.TabIndex = 6;
            this.gbComRx.TabStop = false;
            this.gbComRx.Text = "Ricezione dati";
            // 
            // rtbBufferRx
            // 
            this.rtbBufferRx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbBufferRx.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbBufferRx.Location = new System.Drawing.Point(3, 23);
            this.rtbBufferRx.Name = "rtbBufferRx";
            this.rtbBufferRx.Size = new System.Drawing.Size(1117, 229);
            this.rtbBufferRx.TabIndex = 0;
            this.rtbBufferRx.Text = "";
            // 
            // btnCalcTocken
            // 
            this.btnCalcTocken.Location = new System.Drawing.Point(293, 231);
            this.btnCalcTocken.Name = "btnCalcTocken";
            this.btnCalcTocken.Size = new System.Drawing.Size(75, 23);
            this.btnCalcTocken.TabIndex = 8;
            this.btnCalcTocken.Text = "Calcola";
            this.btnCalcTocken.UseVisualStyleBackColor = true;
            this.btnCalcTocken.Click += new System.EventHandler(this.btnCalcTocken_Click);
            // 
            // FrmSocketClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1123, 713);
            this.Controls.Add(this.gpCoponiComanado);
            this.Controls.Add(this.gbComRx);
            this.Controls.Add(this.gbSendData);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.gbConnectTo);
            this.Controls.Add(this.pnlCommand);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSocketClient";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Client socket";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSocketClient_FormClosing);
            this.Load += new System.EventHandler(this.FrmSocketClient_Load);
            this.pnlCommand.ResumeLayout(false);
            this.pnlCmdSocket.ResumeLayout(false);
            this.pnlGestSocket.ResumeLayout(false);
            this.gpCoponiComanado.ResumeLayout(false);
            this.gpCoponiComanado.PerformLayout();
            this.gbConnectTo.ResumeLayout(false);
            this.gbConnectTo.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.gbSendData.ResumeLayout(false);
            this.gbSendData.PerformLayout();
            this.gbComRx.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel pnlCommand;
        private System.Windows.Forms.GroupBox gbConnectTo;
        private System.Windows.Forms.TextBox txtIPport;
        private System.Windows.Forms.Label lblIpPort;
        private System.Windows.Forms.TextBox txtIPaddress;
        private System.Windows.Forms.Label lblIpAddress;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.GroupBox gbSendData;
        private System.Windows.Forms.TextBox txtSendData;
        private System.Windows.Forms.GroupBox gbComRx;
        private System.Windows.Forms.RichTextBox rtbBufferRx;
        private System.Windows.Forms.ToolStripStatusLabel tssDenComStatus;
        private System.Windows.Forms.ToolStripStatusLabel tssComStatus;
        private System.Windows.Forms.Panel pnlGestSocket;
        private System.Windows.Forms.Button btnCloseConnection;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Panel pnlCmdSocket;
        private System.Windows.Forms.Button btnCommand;
        private System.Windows.Forms.GroupBox gpCoponiComanado;
        private System.Windows.Forms.Label lblCommand;
        private System.Windows.Forms.Label lblSendTime;
        private System.Windows.Forms.Label lblBufferTx;
        private System.Windows.Forms.Label lblDenToken;
        private System.Windows.Forms.CheckBox chkbCRC;
        private System.Windows.Forms.TextBox txtBuffer;
        private System.Windows.Forms.TextBox txtDateSend;
        private System.Windows.Forms.TextBox txtCmd;
        private System.Windows.Forms.Label lblToken;
        private System.Windows.Forms.Button btnAnnullaSend;
        private System.Windows.Forms.Button btnSendMsg;
        private System.Windows.Forms.Button btnCalcTocken;
    }
}