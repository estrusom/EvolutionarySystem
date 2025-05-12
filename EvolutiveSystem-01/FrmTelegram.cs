using SocketManagerInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvolutiveSystem_01
{
    public partial class FrmTelegram : Form
    {
        private string TelegramGenerate = "";
        private string txtSendData = "";
        public FrmTelegram()
        {
            InitializeComponent();
            txtDateSend.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            SocketMessageStructure telegramma = new SocketMessageStructure();
            telegramma.Command = txtCmd.Text;
            telegramma.Data = txtBuffer.Text;
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
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bytes = Encoding.UTF8.GetBytes(TelegramGenerate);
            txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
            Close();
        }
        private void btnAnnullaSend_Click(object sender, EventArgs e)
        {
            Close();
        }
        public string TxtSendData { get { return this.txtSendData; } }

        private void btnCalcTocken_Click(object sender, EventArgs e)
        {
            Random randomGenerator = new Random();
            lblToken.Text = randomGenerator.Next(0, int.MaxValue).ToString();
        }
    }
}
