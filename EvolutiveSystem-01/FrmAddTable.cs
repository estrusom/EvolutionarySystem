using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvolutiveSystem
{
    public partial class FrmAddTable : Form
    {
        bool canExit = false;
        string dbSelect = "";
        string tableName = "";
        public FrmAddTable(string Dbselect,string TableName)
        {
            InitializeComponent();
            this.dbSelect = Dbselect;
            this.tableName = TableName;
        }
        private void FrmAddTable_Load(object sender, EventArgs e)
        {
            lblDb.Text = this.dbSelect;
            txtTblName.Text = this.tableName;
            //txtTblName.SelectedText = this.tableName;
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            TableName = txtTblName.Text;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            canExit = true;
        }
        public string TableName { get; set; }
        private void FrmAddTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!canExit)
            {
                if (txtTblName.Text.Length == 0)
                {
                    MessageBox.Show("Nessun nome di tabella", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }
            }
        }
    }
}
