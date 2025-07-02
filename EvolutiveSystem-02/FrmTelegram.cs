using EvolutiveSystem_02.Properties;
using EvolutiveSystem_02;
using SocketManagerInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Security.Cryptography;
using System.Xml.Linq;
using AsyncSocketServer;
using System.Net;

namespace EvolutiveSystem_02
{
    public partial class FrmTelegram : Form
    {
        private AsyncSocketListener asl;
        private System.Windows.Forms.ToolTip toolTip;
        private string TelegramGenerate = "";
        private string txtSendData = "";
        private SocketMessageStructure testStruct = new SocketMessageStructure();
        private CommandConfig cmdCnf;
        private SocketMessageStructure telegramma;
        private string _connectionString = "";
        private string startString = "";
        private string endString = "";
        public FrmTelegram(AsyncSocketListener Asl, string ConnectionString)
        {
            InitializeComponent();
            this.asl = Asl;
            this.telegramma = new SocketMessageStructure();
            this.cmdCnf = new CommandConfig();
            cmdCnf.ExecuteCmdSync += CmdCnf_ExecuteCmdSync;
            cmdCnf.ExecuteOpenDB += CmdCnf_ExecuteOpenDB;
            cmdCnf.ExecuteMIUexploration += CmdCnf_ExecuteMIUexploration;
            //cmdCnf.ExecuteSaveDB += CmdCnf_ExecuteSaveDB;
            //cmdCnf.ExecuteDBStruct += CmdCnf_ExecuteDBStruct;
            txtDateSend.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            lblDb.Text = "";
            this._connectionString = ConnectionString;
        }

        private void FrmTelegram_Load(object sender, EventArgs e)
        {
            toolTip = new System.Windows.Forms.ToolTip();
            toolTip.SetToolTip(btnCalcTocken, "Calcola il valore del token."); // Aggiunto ToolTip
            #region 25.05.14 nel form FrmTelegram è stata aggiunto una combobox da cui scegliere il comando da generare
            PropertyInfo[] cmdList = typeof(SocketCommand).GetProperties();
            foreach (var item in cmdList)
            {
                if(item.CustomAttributes.Count() != 0)
                {
                    StringBuilder cmbItem = new StringBuilder();
                    var attrIgn = item.CustomAttributes.First().NamedArguments.Where(A => A.MemberName == "IgnoraComand");
                    if (!attrIgn.Any())
                    {
                        AddItemsCombo(item);
                    }
                    else
                    {
                        
                        if ((bool)attrIgn.First().TypedValue.Value == false)
                        {
                            AddItemsCombo(item);
                            /*
                            var attr = item.CustomAttributes.First().NamedArguments.Where(A => A.MemberName == "AddToCombobox");
                            if (attr.Any())
                            {
                                if ((bool)attr.First().TypedValue.Value)
                                {
                                    Console.WriteLine(item.CustomAttributes);
                                    cmbItem.Append(string.Format("[{0}]", item.Name));
                                }
                            }
                            var desc = item.CustomAttributes.First().NamedArguments.Where(D => D.MemberName == "Description");
                            if (desc.Any())
                            {
                                cmbItem.Append(string.Format(" {0}", desc.First().TypedValue.Value));
                            }
                            cmbCommand.Items.Add(cmbItem.ToString());
                            */
                        }
                    }
                }
            }
            #endregion
            lblDb.Text = this._connectionString;
        }
        private void AddItemsCombo(PropertyInfo item)
        {
            StringBuilder cmbItem = new StringBuilder();
            var attr = item.CustomAttributes.First().NamedArguments.Where(A => A.MemberName == "AddToCombobox");
            if (attr.Any())
            {
                if ((bool)attr.First().TypedValue.Value)
                {
                    Console.WriteLine(item.CustomAttributes);
                    cmbItem.Append(string.Format("[{0}]", item.Name));
                }
            }
            var desc = item.CustomAttributes.First().NamedArguments.Where(D => D.MemberName == "Description");
            if (desc.Any())
            {
                cmbItem.Append(string.Format(" {0}", desc.First().TypedValue.Value));
            }
            cmbCommand.Items.Add(cmbItem.ToString());
        }
        private void cmbCommand_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox cmb = sender as System.Windows.Forms.ComboBox;
            Console.WriteLine(cmb.SelectedItem);
            PropertyInfo[] cmdList = typeof(SocketCommand).GetProperties();
            int init = cmb.SelectedItem.ToString().IndexOf("[") + 1;
            int fine = cmb.SelectedItem.ToString().IndexOf("]");
            string command = cmb.SelectedItem.ToString().Substring(init, fine - init );
            var acti = cmdList.Where(A => A.Name == command);
            if (acti.Any())
            {
                var action = acti.First().CustomAttributes.First().NamedArguments.Where(ACT => ACT.MemberName == "SelectAction");
                if (action.Any())
                {
                    switch ((ActionType) Convert.ToInt32( action.First().TypedValue.Value) )
                    {
                        case ActionType.None:
                            {
                                gbConfigFunzione.Controls.Clear();
                            }
                        break;
                        case ActionType.DefStartStopString:
                            {
                                btnSendMsg.Enabled = false;
                                btnOkSend.Enabled = false;
                                btnAnnullaSend.Enabled = false;
                                lblTitle.Text = cmbCommand.SelectedItem.ToString();
                                Label labelStart = new Label()
                                {
                                    Name = "lblStartString",
                                    Top = 50,
                                    Left = 10,
                                    Width = 100,
                                    Text = "Start string:",
                                    Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
                                    Enabled = true
                                };
                                gbConfigFunzione.Controls.Add(labelStart);
                                System.Windows.Forms.TextBox txtStart = new System.Windows.Forms.TextBox()
                                {
                                    Name = "txtStartString",
                                    Top = 50,
                                    Left= 110,
                                    Width = 100,
                                    Font = new Font(this.Font.FontFamily, 10, FontStyle.Regular),
                                    Enabled = true
                                };
                                gbConfigFunzione.Controls.Add(txtStart);
                                Label labelEnd = new Label()
                                {
                                    Name = "lblEndString",
                                    Top = 80,
                                    Left = 10,
                                    Width = 100,
                                    Text = "End string:",
                                    Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
                                    Enabled = true
                                };
                                gbConfigFunzione.Controls.Add(labelEnd);
                                System.Windows.Forms.TextBox txtEnd = new System.Windows.Forms.TextBox()
                                {
                                    Name = "txtEndString",
                                    Top = 80,
                                    Left = 110,
                                    Width = 100,
                                    Font = new Font(this.Font.FontFamily, 10, FontStyle.Regular),
                                    Enabled = true
                                };
                                gbConfigFunzione.Controls.Add(txtEnd);
                                System.Windows.Forms.Button btnOk = new System.Windows.Forms.Button()
                                {
                                    Name = "btnOk",
                                    Top = 150,
                                    Width = 80,
                                    Height = 35,
                                    Left = 10,
                                    Font = new Font(this.Font.FontFamily, 10, FontStyle.Regular),
                                    Text = "OK"
                                };
                                gbConfigFunzione.Controls.Add(btnOk);
                                System.Windows.Forms.Button btnAnnulla = new System.Windows.Forms.Button()
                                {
                                    Name = "btnAnnulla",
                                    Top = 150,
                                    Width = 80,
                                    Height = 35,
                                    Left = 100,
                                    Font = new Font(this.Font.FontFamily, 10, FontStyle.Regular),
                                    Text = "Annula"
                                };
                                gbConfigFunzione.Controls.Add(btnAnnulla);
                                btnOk.Click += BtnOk_Click;
                                btnAnnulla.Click += BtnAnnulla_Click;
                            }
                            break;
                    //    case ActionType.FilePath:
                    //        {
                    //            gbSelCmd.Enabled = false;
                    //            pnlbtn.Enabled = false;
                    //            gbActionTypeLoadDb.BringToFront();
                    //            gbActionTypeLoadDb.Visible = true;
                    //            gbActionTypeLoadDb.Dock = DockStyle.Fill;
                    //        }
                    //        break;
                    //    case ActionType.DbName:
                    //        {
                    //            gbSelCmd.Enabled = false;
                    //            pnlbtn.Enabled = false;
                    //            gbActionSaveDatabase.Visible = true;
                    //            gbActionSaveDatabase.Dock = DockStyle.Fill;
                    //        }
                    //        break;
                    //    case ActionType.DbRequest:
                    //        {
                    //            gbSelCmd.Enabled = false;
                    //            pnlbtn.Enabled = false;
                    //            gbSelRequest.Visible = true;
                    //            gbSelRequest.Dock = DockStyle.Fill;
                    //        }
                    //        break;
                    }
                }
            }
            testStruct.Command = command;
        }
        #region buttons events
        private void BtnAnnulla_Click(object sender, EventArgs e)
        {
            this.startString = "";
            this.endString = "";
            btnSendMsg.Enabled = true;
            btnOkSend.Enabled = true;
            btnAnnullaSend.Enabled = true;
            gbConfigFunzione.Controls.Clear();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            foreach (Control control in gbConfigFunzione.Controls)
            {
                if(control.Name == "txtStartString")
                {
                    this.startString = control.Text;
                }
                if (control.Name == "txtEndString")
                {
                    this.endString = control.Text;
                }
            }
            btnSendMsg.Enabled = true;
            btnOkSend.Enabled = true;
            btnAnnullaSend.Enabled = true;
            gbConfigFunzione.Controls.Clear();
        }

        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            rtxtBuffer.Clear();
            int init = cmbCommand.SelectedItem.ToString().IndexOf("[") + 1;
            int fine = cmbCommand.SelectedItem.ToString().IndexOf("]");
            telegramma.Command = cmbCommand.SelectedItem.ToString().Substring(init, fine - init);
            PropertyInfo[] cmdList = typeof(SocketCommand).GetProperties();
            var method = cmdList.Where(M => M.Name == telegramma.Command);
            if (method.Any())
            {
                Type tyCommandHandlers = typeof(CommandConfig);
                ClsCustomBinder myCustomBinder = new ClsCustomBinder();

                MethodInfo myMethod = tyCommandHandlers.GetMethod(
                    method.First().Name, // Nome del metodo
                    BindingFlags.Public | BindingFlags.Instance, // Cerca metodi pubblici d'istanza
                    myCustomBinder, // Usa il tuo binder per la risoluzione
                    new Type[] { typeof(string) }, // Tipi dei parametri (verifica che siano corretti!)
                    null // Modificatori
                );
                if (myMethod != null)
                {
                    object a = myMethod.Invoke(cmdCnf, new Object[] { method.First().Name });
                }
            }
            if (DateTime.TryParse(txtDateSend.Text, out DateTime result))
            {
                telegramma.SendingTime = result;
            }
            else
            {
                telegramma.SendingTime = DateTime.Now;
            }
            telegramma.Token = lblToken.Text;
            if (chkbCRC.Checked)
            {
                telegramma.CRC = telegramma.GetHashCode();
            }
            TelegramGenerate = SocketMessageSerializer.SerializeUTF8(telegramma);
            rtxtBuffer.AppendText(TelegramGenerate);
        }
        private void btnOkSend_Click(object sender, EventArgs e)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bytes = Encoding.UTF8.GetBytes(TelegramGenerate);
            this.txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
            Close();
        }

        private void btnAnnullaSend_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void btnCalcTocken_Click(object sender, EventArgs e)
        {
            Random randomGenerator = new Random();
            lblToken.Text = randomGenerator.Next(0, int.MaxValue).ToString();
            testStruct.Token = lblToken.Text;
        }
        #endregion
        #region eventi di configurazione UI per comandi
        private void CmdCnf_ExecuteOpenDB(object sender, string e)
        {

            XElement filePathElement = new XElement("FilePath", _connectionString);
            XElement bufferContent = new XElement("BufferDati", // Nome descrittivo, NON "BufferDati"
                                            filePathElement);
            telegramma.BufferDati = bufferContent;

        }
        private void CmdCnf_ExecuteCmdSync(object sender, string e)
        {
            string localIpAddress = "";
            if (asl.SrvIpAddress.Count > 0)
            {
                localIpAddress = asl.SrvIpAddress[asl.SrvIpAddress.Count - 1].ToString();
            }

            XElement bufferDatiContent = new XElement("BufferDati"); // Crea l'elemento <BufferDati> vuoto

            // Aggiungi l'elemento UiIpAddress solo se l'IP locale è stato trovato
            if (!string.IsNullOrWhiteSpace(localIpAddress))
            {
                // Aggiunge <UiIpAddress>...</UiIpAddress> come figlio di bufferDatiContent
                bufferDatiContent.Add(new XElement("UiIpAddress", localIpAddress));
           }
            else
            {
            }

            // Aggiunge l'elemento UiPort (assumendo che uiListenPort sia sempre valido)
            // Aggiunge <UiPort>...</UiPort> come figlio di bufferDatiContent
            bufferDatiContent.Add(new XElement("UiPort", asl.SrvPort.ToString()));
            telegramma.BufferDati = bufferDatiContent;
        }
        private void CmdCnf_ExecuteMIUexploration(object sender, string e)
        {
            XElement BufferDati = new XElement("BufferDati",
                    new XElement("MIUstring",
                    new XElement("StringStart", this.startString),
                    new XElement("EndString", this.endString)));
            telegramma.BufferDati = BufferDati;
        }

        #endregion
        public string TxtSendData { get { return this.txtSendData; } }
    }
}
