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
    public partial class FrmDbName : Form
    {
        bool canExit = false;
        public FrmDbName()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DatabaseName = txtDbName.Text;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            canExit = true;
        }
        public string DatabaseName { get; set; }

        private void FrmDbName_FormClosing(object sender, FormClosingEventArgs e)
        {if (!canExit)
            {
                if (txtDbName.Text.Length == 0)
                {
                    MessageBox.Show("Nessun nome di database", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }
            }
            
        }
    }
}
