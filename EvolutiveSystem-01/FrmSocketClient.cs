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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ToolTip = System.Windows.Forms.ToolTip;

namespace EvolutiveSystem_01
{
    public partial class FrmSocketClient : Form
    {
        private string TelegramGenerate = "";
        private Logger _logger;
        private SocketClient sk;
        private ToolTip toolTip;
        public FrmSocketClient(Logger logger)
        {
            InitializeComponent();
            this._logger = logger;
            sk = new SocketClient(_logger);
        }
        private void FrmSocketClient_Load(object sender, EventArgs e)
        {
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
            this.btnConnect.Enabled = true;
            this.btnSend.Enabled = false;
            this.btnCloseConnection.Enabled = false;
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
        #region buttons events
        private void BtnCommand_Click(object sender, EventArgs e)
        {
            pnlCommand.Enabled = false;
                        
            gpCoponiComanado.Visible = true;
            gpCoponiComanado.Select();
            gpCoponiComanado.BringToFront();

            gpCoponiComanado.Left = (this.Width - gpCoponiComanado.Width) / 2;
            gpCoponiComanado.Top = (this.Height - gpCoponiComanado.Height) / 2;
            txtDateSend.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }
        private void btnCalcTocken_Click(object sender, EventArgs e)
        {
            Random randomGenerator = new Random();
            lblToken.Text = randomGenerator.Next(0, int.MaxValue).ToString();
        }
        private void BtnCloseConnection_Click(object sender, EventArgs e)
        {
            if (sk.isConnect)
            {
                sk.CloseSocket();
                this.btnConnect.Enabled = true;
                this.btnSend.Enabled = false;
                this.btnCloseConnection.Enabled = false;
                tssComStatus.Text = "Close";
                tssComStatus.ForeColor = Color.Green;
            }
        }
        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (sk.isConnect)
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
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            //if (!sk.isConnect)
            {
                sk.ConnectToServer(txtIPaddress.Text, Convert.ToInt32(txtIPport.Text));
                this.btnConnect.Enabled = false;
                this.btnSend.Enabled = true;
                this.btnCloseConnection.Enabled = true;
                tssComStatus.Text = "Open";
                tssComStatus.ForeColor = Color.Green;
            }
                
        }
        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            SocketMessageStructure telegramma = new SocketMessageStructure();
            telegramma.Command = txtCmd.Text;
            telegramma.Data = txtDateSend.Text;
            if (DateTime.TryParse(txtDateSend.Text,out DateTime result))
            {
                telegramma.SendingTime = result;
            }
            else
            {
                telegramma.SendingTime = DateTime.Now;
            }
            telegramma.Token = Convert.ToInt32(lblToken.Text);
            if(chkbCRC.Checked)
            {
                telegramma.CRC = telegramma.GetHashCode();
            }
            TelegramGenerate = SocketMessageSerialize.Serilaize(telegramma);
            pnlCommand.Enabled = true;
            gpCoponiComanado.Visible = false;
            txtSendData.Text = TelegramGenerate;
        }
        private void btnAnnullaSend_Click(object sender, EventArgs e)
        {
            pnlCommand.Enabled = true;
            gpCoponiComanado.Visible = false;
        }
        #endregion
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
    }
}
