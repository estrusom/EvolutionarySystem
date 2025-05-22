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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSocketClient));
            this.chiudiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxTools = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmDecodificaComando = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmDeserializzaComando = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmDeserializzaComandoClose = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlCommand = new System.Windows.Forms.Panel();
            this.pnlCmdSocket = new System.Windows.Forms.Panel();
            this.btnAttrezzi = new System.Windows.Forms.Button();
            this.btnCommand = new System.Windows.Forms.Button();
            this.pnlGestSocket = new System.Windows.Forms.Panel();
            this.btnCloseConnection = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tssDenComStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssComStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.spcDebug = new System.Windows.Forms.SplitContainer();
            this.gbComRx = new System.Windows.Forms.GroupBox();
            this.rtbBufferRx = new System.Windows.Forms.RichTextBox();
            this.gbSendData = new System.Windows.Forms.GroupBox();
            this.txtSendData = new System.Windows.Forms.TextBox();
            this.gbConnectTo = new System.Windows.Forms.GroupBox();
            this.txtIPport = new System.Windows.Forms.TextBox();
            this.lblIpPort = new System.Windows.Forms.Label();
            this.txtIPaddress = new System.Windows.Forms.TextBox();
            this.lblIpAddress = new System.Windows.Forms.Label();
            this.spcOutput = new System.Windows.Forms.SplitContainer();
            this.richTextBoxDebug = new System.Windows.Forms.RichTextBox();
            this.tsmClrRx = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmClrTx = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxTools.SuspendLayout();
            this.pnlCommand.SuspendLayout();
            this.pnlCmdSocket.SuspendLayout();
            this.pnlGestSocket.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcDebug)).BeginInit();
            this.spcDebug.Panel1.SuspendLayout();
            this.spcDebug.Panel2.SuspendLayout();
            this.spcDebug.SuspendLayout();
            this.gbComRx.SuspendLayout();
            this.gbSendData.SuspendLayout();
            this.gbConnectTo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcOutput)).BeginInit();
            this.spcOutput.Panel1.SuspendLayout();
            this.spcOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // chiudiToolStripMenuItem
            // 
            this.chiudiToolStripMenuItem.Name = "chiudiToolStripMenuItem";
            this.chiudiToolStripMenuItem.Size = new System.Drawing.Size(225, 24);
            this.chiudiToolStripMenuItem.Text = "Chiudi";
            // 
            // ctxTools
            // 
            this.ctxTools.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctxTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmDecodificaComando,
            this.tsmDeserializzaComando,
            this.toolStripSeparator2,
            this.tsmClrRx,
            this.tsmClrTx,
            this.toolStripSeparator3,
            this.tsmDeserializzaComandoClose});
            this.ctxTools.Name = "ctxTools";
            this.ctxTools.Size = new System.Drawing.Size(251, 164);
            // 
            // tsmDecodificaComando
            // 
            this.tsmDecodificaComando.Name = "tsmDecodificaComando";
            this.tsmDecodificaComando.Size = new System.Drawing.Size(251, 24);
            this.tsmDecodificaComando.Text = "Decodifica comando";
            this.tsmDecodificaComando.Click += new System.EventHandler(this.tsmDecodificaComando_Click);
            // 
            // tsmDeserializzaComando
            // 
            this.tsmDeserializzaComando.Name = "tsmDeserializzaComando";
            this.tsmDeserializzaComando.Size = new System.Drawing.Size(251, 24);
            this.tsmDeserializzaComando.Text = "Deserializza comando";
            this.tsmDeserializzaComando.Click += new System.EventHandler(this.tsmDeserializzaComando_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(248, 6);
            // 
            // tsmDeserializzaComandoClose
            // 
            this.tsmDeserializzaComandoClose.Name = "tsmDeserializzaComandoClose";
            this.tsmDeserializzaComandoClose.Size = new System.Drawing.Size(251, 24);
            this.tsmDeserializzaComandoClose.Text = "Close";
            // 
            // pnlCommand
            // 
            this.pnlCommand.Controls.Add(this.pnlCmdSocket);
            this.pnlCommand.Controls.Add(this.pnlGestSocket);
            this.pnlCommand.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCommand.Location = new System.Drawing.Point(0, 0);
            this.pnlCommand.Name = "pnlCommand";
            this.pnlCommand.Size = new System.Drawing.Size(1334, 49);
            this.pnlCommand.TabIndex = 1;
            // 
            // pnlCmdSocket
            // 
            this.pnlCmdSocket.Controls.Add(this.btnAttrezzi);
            this.pnlCmdSocket.Controls.Add(this.btnCommand);
            this.pnlCmdSocket.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlCmdSocket.Location = new System.Drawing.Point(168, 0);
            this.pnlCmdSocket.Name = "pnlCmdSocket";
            this.pnlCmdSocket.Size = new System.Drawing.Size(200, 49);
            this.pnlCmdSocket.TabIndex = 4;
            // 
            // btnAttrezzi
            // 
            this.btnAttrezzi.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.tools_disk_19657;
            this.btnAttrezzi.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAttrezzi.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnAttrezzi.Location = new System.Drawing.Point(53, 0);
            this.btnAttrezzi.Name = "btnAttrezzi";
            this.btnAttrezzi.Size = new System.Drawing.Size(53, 49);
            this.btnAttrezzi.TabIndex = 7;
            this.btnAttrezzi.UseVisualStyleBackColor = true;
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
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(57, 6);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssDenComStatus,
            this.tssComStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 866);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1334, 25);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tssDenComStatus
            // 
            this.tssDenComStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tssDenComStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.tssDenComStatus.Name = "tssDenComStatus";
            this.tssDenComStatus.Size = new System.Drawing.Size(185, 19);
            this.tssDenComStatus.Text = "Stato comunicazione socket";
            // 
            // tssComStatus
            // 
            this.tssComStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.tssComStatus.ForeColor = System.Drawing.Color.Red;
            this.tssComStatus.Name = "tssComStatus";
            this.tssComStatus.Size = new System.Drawing.Size(42, 19);
            this.tssComStatus.Text = "Close";
            this.tssComStatus.ToolTipText = "Close";
            // 
            // spcDebug
            // 
            this.spcDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcDebug.Location = new System.Drawing.Point(0, 49);
            this.spcDebug.Name = "spcDebug";
            // 
            // spcDebug.Panel1
            // 
            this.spcDebug.Panel1.Controls.Add(this.gbComRx);
            this.spcDebug.Panel1.Controls.Add(this.gbSendData);
            this.spcDebug.Panel1.Controls.Add(this.gbConnectTo);
            // 
            // spcDebug.Panel2
            // 
            this.spcDebug.Panel2.Controls.Add(this.spcOutput);
            this.spcDebug.Size = new System.Drawing.Size(1334, 817);
            this.spcDebug.SplitterDistance = 888;
            this.spcDebug.TabIndex = 4;
            // 
            // gbComRx
            // 
            this.gbComRx.Controls.Add(this.rtbBufferRx);
            this.gbComRx.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbComRx.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbComRx.Location = new System.Drawing.Point(0, 452);
            this.gbComRx.Name = "gbComRx";
            this.gbComRx.Size = new System.Drawing.Size(888, 361);
            this.gbComRx.TabIndex = 9;
            this.gbComRx.TabStop = false;
            this.gbComRx.Text = "Ricezione dati";
            // 
            // rtbBufferRx
            // 
            this.rtbBufferRx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbBufferRx.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbBufferRx.Location = new System.Drawing.Point(3, 23);
            this.rtbBufferRx.Name = "rtbBufferRx";
            this.rtbBufferRx.Size = new System.Drawing.Size(882, 335);
            this.rtbBufferRx.TabIndex = 0;
            this.rtbBufferRx.Text = "";
            // 
            // gbSendData
            // 
            this.gbSendData.Controls.Add(this.txtSendData);
            this.gbSendData.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbSendData.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSendData.Location = new System.Drawing.Point(0, 115);
            this.gbSendData.Name = "gbSendData";
            this.gbSendData.Size = new System.Drawing.Size(888, 337);
            this.gbSendData.TabIndex = 8;
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
            this.txtSendData.Size = new System.Drawing.Size(882, 311);
            this.txtSendData.TabIndex = 1;
            // 
            // gbConnectTo
            // 
            this.gbConnectTo.Controls.Add(this.txtIPport);
            this.gbConnectTo.Controls.Add(this.lblIpPort);
            this.gbConnectTo.Controls.Add(this.txtIPaddress);
            this.gbConnectTo.Controls.Add(this.lblIpAddress);
            this.gbConnectTo.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbConnectTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbConnectTo.Location = new System.Drawing.Point(0, 0);
            this.gbConnectTo.Name = "gbConnectTo";
            this.gbConnectTo.Size = new System.Drawing.Size(888, 115);
            this.gbConnectTo.TabIndex = 7;
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
            this.txtIPaddress.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSendData_KeyPress);
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
            // spcOutput
            // 
            this.spcOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcOutput.Location = new System.Drawing.Point(0, 0);
            this.spcOutput.Name = "spcOutput";
            this.spcOutput.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcOutput.Panel1
            // 
            this.spcOutput.Panel1.Controls.Add(this.richTextBoxDebug);
            this.spcOutput.Size = new System.Drawing.Size(442, 817);
            this.spcOutput.SplitterDistance = 386;
            this.spcOutput.TabIndex = 0;
            // 
            // richTextBoxDebug
            // 
            this.richTextBoxDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxDebug.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxDebug.Name = "richTextBoxDebug";
            this.richTextBoxDebug.Size = new System.Drawing.Size(442, 386);
            this.richTextBoxDebug.TabIndex = 0;
            this.richTextBoxDebug.Text = "";
            // 
            // tsmClrRx
            // 
            this.tsmClrRx.Name = "tsmClrRx";
            this.tsmClrRx.Size = new System.Drawing.Size(251, 24);
            this.tsmClrRx.Text = "Cancella ricezione dati";
            this.tsmClrRx.Click += new System.EventHandler(this.tsmClrRx_Click);
            // 
            // tsmClrTx
            // 
            this.tsmClrTx.Name = "tsmClrTx";
            this.tsmClrTx.Size = new System.Drawing.Size(250, 24);
            this.tsmClrTx.Text = "Cancella trasmissione dati";
            this.tsmClrTx.Click += new System.EventHandler(this.tsmClrTx_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(248, 6);
            // 
            // FrmSocketClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 891);
            this.Controls.Add(this.spcDebug);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.pnlCommand);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSocketClient";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Client socket";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSocketClient_FormClosing);
            this.Load += new System.EventHandler(this.FrmSocketClient_Load);
            this.ctxTools.ResumeLayout(false);
            this.pnlCommand.ResumeLayout(false);
            this.pnlCmdSocket.ResumeLayout(false);
            this.pnlGestSocket.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.spcDebug.Panel1.ResumeLayout(false);
            this.spcDebug.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcDebug)).EndInit();
            this.spcDebug.ResumeLayout(false);
            this.gbComRx.ResumeLayout(false);
            this.gbSendData.ResumeLayout(false);
            this.gbSendData.PerformLayout();
            this.gbConnectTo.ResumeLayout(false);
            this.gbConnectTo.PerformLayout();
            this.spcOutput.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcOutput)).EndInit();
            this.spcOutput.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStripMenuItem chiudiToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip ctxTools;
        private System.Windows.Forms.Panel pnlCommand;
        private System.Windows.Forms.Panel pnlCmdSocket;
        private System.Windows.Forms.Button btnAttrezzi;
        private System.Windows.Forms.Button btnCommand;
        private System.Windows.Forms.Panel pnlGestSocket;
        private System.Windows.Forms.Button btnCloseConnection;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tssDenComStatus;
        private System.Windows.Forms.ToolStripStatusLabel tssComStatus;
        private System.Windows.Forms.SplitContainer spcDebug;
        private System.Windows.Forms.GroupBox gbComRx;
        private System.Windows.Forms.RichTextBox rtbBufferRx;
        private System.Windows.Forms.GroupBox gbSendData;
        private System.Windows.Forms.TextBox txtSendData;
        private System.Windows.Forms.GroupBox gbConnectTo;
        private System.Windows.Forms.TextBox txtIPport;
        private System.Windows.Forms.Label lblIpPort;
        private System.Windows.Forms.TextBox txtIPaddress;
        private System.Windows.Forms.Label lblIpAddress;
        private System.Windows.Forms.SplitContainer spcOutput;
        private System.Windows.Forms.ToolStripMenuItem tsmDecodificaComando;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem tsmDeserializzaComandoClose;
        private System.Windows.Forms.RichTextBox richTextBoxDebug;
        private System.Windows.Forms.ToolStripMenuItem tsmDeserializzaComando;
        private System.Windows.Forms.ToolStripMenuItem tsmClrRx;
        private System.Windows.Forms.ToolStripMenuItem tsmClrTx;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}