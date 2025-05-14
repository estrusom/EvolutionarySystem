using EvolutiveSystem_01.Properties;
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

namespace EvolutiveSystem_01
{
    public partial class FrmTelegram : Form
    {
        private System.Windows.Forms.ToolTip toolTip;
        private string TelegramGenerate = "";
        private string txtSendData = "";
        private SocketMessageStructure testStruct = new SocketMessageStructure();
        public FrmTelegram()
        {
            InitializeComponent();
            txtDateSend.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            lblDb.Text = "";
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
            }
            #endregion
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

                            }
                            break;
                        case ActionType.FilePath:
                            {
                                gbActionTypeLoadDb.Visible = true;
                                gbActionTypeLoadDb.Dock = DockStyle.Fill;
                            }
                            break;
                        case ActionType.DbName:
                            {

                            }
                            break;
                    }
                }
            }
            testStruct.Command = command;
        }
        #region buttons events
        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            SocketMessageStructure telegramma = new SocketMessageStructure();
            int init = cmbCommand.SelectedItem.ToString().IndexOf("[") + 1;
            int fine = cmbCommand.SelectedItem.ToString().IndexOf("]");
            telegramma.Command = cmbCommand.SelectedItem.ToString().Substring(init, fine - init);
            PropertyInfo[] cmdList = typeof(SocketCommand).GetProperties();
            var method = cmdList.Where(M => M.Name == telegramma.Command);
            if (method.Any())
            {

            }

            telegramma.Data = rtxtBuffer.Text;
            if (DateTime.TryParse(txtDateSend.Text, out DateTime result))
            {
                telegramma.SendingTime = result;
            }
            else
            {
                telegramma.SendingTime = DateTime.Now;
            }
            telegramma.Token = Convert.ToInt32(lblToken.Text);
            if (chkbCRC.Checked)
            {
                telegramma.CRC = telegramma.GetHashCode();
            }
            TelegramGenerate = SocketMessageSerialize.SerializeUTF8(telegramma);
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
            testStruct.Token = Convert.ToInt32(lblToken.Text);
        }
        private void btnSelDb_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                DefaultExt = "*.xml",
                Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*",
                InitialDirectory = Settings.Default.LoadPathDbForCmd.Length == 0 ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : Settings.Default.LoadPathDbForCmd
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtFileDb.Text = ofd.FileName;
                Settings.Default.LoadPathDbForCmd= ofd.FileName; ;
                Settings.Default.Save();
            }
        }
        private void btnOkDb_Click(object sender, EventArgs e)
        {
            lblDb.Text = txtFileDb.Text;
            gbActionTypeLoadDb.Visible = false;
            gbActionTypeLoadDb.Dock = DockStyle.None;
            
        }
        private void bgtnAnnullaDb_Click(object sender, EventArgs e)
        {
            gbActionTypeLoadDb.Visible = false;
            gbActionTypeLoadDb.Dock = DockStyle.None;
        }
        #endregion
        public string TxtSendData { get { return this.txtSendData; } }
    }
}
