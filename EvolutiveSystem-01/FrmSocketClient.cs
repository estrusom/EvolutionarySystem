using AsyncSocketServer;
using EvolutiveSystem.UI.Forms;
using EvolutiveSystem_01.Properties;
using MasterLog;
using MessaggiErrore;
using SocketManager;
using SocketManagerInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ToolTip = System.Windows.Forms.ToolTip;

namespace EvolutiveSystem_01
{
    public partial class FrmSocketClient : Form
    {
        private AsyncSocketListener asl;
        private Logger _logger;
        private SocketClient sk;
        private ToolTip toolTip;
        string xmlSv = "";
        public FrmSocketClient(Logger logger, AsyncSocketListener Asl)
        {
            InitializeComponent();
            this.asl = Asl;
            this._logger = logger;
            sk = new SocketClient(_logger);
        }
        #region form events
        private void FrmSocketClient_Load(object sender, EventArgs e)
        {
            #region configura bottoni
            toolTip = new ToolTip();
            if (btnConnect != null) 
            {
                btnConnect.Click += BtnConnect_Click;
                toolTip.SetToolTip(btnConnect, "Connette al socket server."); // *** Aggiunto ToolTip ***
            }
            if(btnSend != null)
            {
                btnSend.Click += BtnSend_Click;
                toolTip.SetToolTip(btnSend, "Invia messagio al socket server tramite socket."); // *** Aggiunto ToolTip ***
            }
            if(btnCloseConnection != null)
            {
                btnCloseConnection.Click += BtnCloseConnection_Click;
                toolTip.SetToolTip(btnCloseConnection, "Chiusura connessione col socket."); // *** Aggiunto ToolTip ***
            }
            if (btnCommand != null)
            {
                btnCommand.Click += BtnCommand_Click;
                toolTip.SetToolTip(btnCommand, "Composizione del comando per il server semantico."); // *** Aggiunto ToolTip ***
            }
            if (btnAttrezzi != null)
            {
                btnAttrezzi.Click += BtnAttrezzi_Click;
                toolTip.SetToolTip(btnAttrezzi, "Attrezzi d'uso generico per debuging del sistema."); // *** Aggiunto ToolTip ***
            }
            #endregion
            #region gestione textbox
            if (Settings.Default.LastIPaddress.Length == 0)
            {
                txtIPaddress.Text = ConfigurationManager.AppSettings["SocketAddrAssetMngm"];
            }
            else
            {
                txtIPaddress.Text = Settings.Default.LastIPaddress;
                
            }
            if (Settings.Default.lastIPport == 0)
            {
                txtIPport.Text= ConfigurationManager.AppSettings["SocketPortAssetMngm"];
            }
            else
            {
                txtIPport.Text = Settings.Default.lastIPport.ToString();
            }
            #endregion
            this.btnConnect.Enabled = true;
            this.btnSend.Enabled = false;
            this.btnCloseConnection.Enabled = false;
            this.statusStrip1.ShowItemToolTips = true;
            this.tssDenComStatus.Text = "Stato comunicazione socket";
        }
        private void FrmSocketClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            if (sk != null)
            {
                try
                {
                    sk.CloseSocket();
                }
                catch (SocketException sex)
                {
                    string msg = ClsMessaggiErrore.CustomMsg(sex, thisMethod);
                    _logger.Log(LogLevel.ERROR, msg);
                }
                catch (Exception ex)
                {
                    string msg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                    _logger.Log(LogLevel.ERROR, msg);
                }
            }
        }
        #endregion
        #region buttons events
        private void BtnAttrezzi_Click(object sender, EventArgs e)
        {
            Point p = PointToScreen(new Point(pnlCmdSocket.Left + btnAttrezzi.Left + btnAttrezzi.Width, btnAttrezzi.Top + btnAttrezzi.Height));
            ctxTools.Show(p);
        }
        private void BtnCommand_Click(object sender, EventArgs e)
        {
            FrmTelegram setTelegram = new FrmTelegram(asl);
            setTelegram.ShowDialog();
            if (setTelegram.DialogResult == DialogResult.OK)
            {
                txtSendData.Text = setTelegram.TxtSendData;
            }

        }
        private void BtnCloseConnection_Click(object sender, EventArgs e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                if (sk.isConnect)
                {
                    sk.CloseSocket();
                    tssComStatus.Text = "Close";
                    tssComStatus.ForeColor = Color.Blue;
                    tssComStatus.ToolTipText = "Close";
                }
            }
            catch (Exception ex)
            {
                string errMsg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, errMsg);
                MessageBox.Show(errMsg, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnConnect.Enabled = true;
                this.btnSend.Enabled = false;
                this.btnCloseConnection.Enabled = false;
            }
        }
        private void BtnSend_Click(object sender, EventArgs e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                if (sk.isConnect)
                {
                    if (txtSendData.Text.Length > 0)
                    {
                        sk.SendString(txtSendData.Text);
                        
                        string rxData = sk.ReceiveMessage().Trim();
                        ASCIIEncoding dencoding = new ASCIIEncoding();
                        int init = rxData.IndexOf(SocketMessageSerializer.Base64Start) + SocketMessageSerializer.Base64Start.Length;
                        int end = rxData.IndexOf(SocketMessageSerializer.Base64End);
                        xmlSv = Encoding.UTF8.GetString(Convert.FromBase64String(rxData.Substring(init, end - init)));
                        rtbBufferRx.AppendText(xmlSv + Environment.NewLine);
                        sk.CloseSocket();
                        btnSend.Enabled = false;
                        btnConnect.Enabled = true;
                        btnCloseConnection.Enabled= false;
                        tssComStatus.Text = "Close";
                        tssComStatus.ForeColor = Color.Blue;
                        tssComStatus.ToolTipText = "Close";

                    }
                    else
                    {
                        MessageBox.Show("Nessun dato da trasmettere", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("La comunicazzione è chiusa", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }catch (Exception ex)
            {
                string errMsg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, errMsg);
                MessageBox.Show(errMsg, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                sk.ConnectToServer(txtIPaddress.Text, Convert.ToInt32(txtIPport.Text));
                tssComStatus.ToolTipText = $"End Point{sk.SocketAddress.ToString()}:{sk.SocketPort}";
                 this.btnConnect.Enabled = false;
                this.btnSend.Enabled = true;
                this.btnCloseConnection.Enabled = true;
                tssComStatus.Text = "Open";
                tssComStatus.ForeColor = Color.Green;
            }catch (Exception ex)
            {
                string msg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, msg);
                MessageBox.Show(msg, "ERRORE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
                
        }
        #endregion
        #region context menu events
        private void tsmDecodificaComando_Click(object sender, EventArgs e)
        {
            InputBoxForm inputBox = new InputBoxForm("decodifica Base64 UTF8", "Inserire il messagio");
            inputBox.ShowDialog();
            if (inputBox.DialogResult == DialogResult.OK)
            {
                SocketCommand sk = new SocketCommand();
                ASCIIEncoding dencoding = new ASCIIEncoding();
                int init = inputBox.InputValue.IndexOf(sk.Base64Start) + sk.Base64Start.Length;
                int end = inputBox.InputValue.IndexOf(sk.Base64End);
                xmlSv = Encoding.UTF8.GetString(Convert.FromBase64String(inputBox.InputValue.Substring(init, end - init)));
                richTextBoxDebug.Text = xmlSv;
            }
        }
        private void tsmDeserializzaComando_Click(object sender, EventArgs e)
        {
            if (xmlSv.Length > 0)
            {
                SocketMessageStructure Telegram = SocketMessageSerializer.DeserializeUTF8(xmlSv);
                richTextBoxDebug.AppendText(Environment.NewLine);
                richTextBoxDebug.AppendText("********************************************************" + Environment.NewLine);
                richTextBoxDebug.AppendText(string.Format("Command: {0}", Telegram.Command) + Environment.NewLine);
                richTextBoxDebug.AppendText(string.Format("Data trasmissione: {0}", Telegram.SendingTime) + Environment.NewLine);
                richTextBoxDebug.AppendText(string.Format("Dati: {0}", Telegram.BufferDati) + Environment.NewLine);
                richTextBoxDebug.AppendText(string.Format("Tocken: {0}", Telegram.Token) + Environment.NewLine);
                richTextBoxDebug.AppendText(string.Format("CRC: {0}", Telegram.CRC) + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("Nessun dato da deserializzare", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void tsmClrRx_Click(object sender, EventArgs e)
        {
            rtbBufferRx.Text = "";
        }
        private void tsmClrTx_Click(object sender, EventArgs e)
        {
            txtSendData.Text = "";
        }
        #endregion
        #region textbox events
        private void txtIPaddress_Leave(object sender, EventArgs e)
        {
            Settings.Default.LastIPaddress = txtIPaddress.Text;
            Settings.Default.Save();
        }
        private void txtIPport_Leave(object sender, EventArgs e)
        {
            Settings.Default.lastIPport = Convert.ToInt32(txtIPport.Text);
            Settings.Default.Save();
        }
        private void txtIPaddress_Enter(object sender, EventArgs e)
        {
            txtIPaddress.SelectionStart = 0;
            txtIPaddress.SelectionLength = txtIPaddress.TextLength;
        }
        private void txtIPport_Enter(object sender, EventArgs e)
        {
            txtIPport.SelectionStart = 0;
            txtIPport.SelectionLength = txtIPaddress.TextLength;

        }
        private void txtSendData_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == '\r')
            {
                if (this.btnSend.Enabled)
                {
                    if (txtSendData.Text.Length > 0)
                    {
                        sk.SendString(txtSendData.Text);
                        rtbBufferRx.AppendText(sk.ReceiveMessage().Trim() + Environment.NewLine);
                    }
                    else
                    {
                        MessageBox.Show("Nessun dato da trasmettere", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("La comunicazzione è chiusa", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        #endregion
    }
}
