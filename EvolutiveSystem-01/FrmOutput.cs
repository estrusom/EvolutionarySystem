using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvolutiveSystem_01
{
    public partial class FrmOutput : Form
    {
        string output = "";
        public FrmOutput(string Output)
        {
            InitializeComponent();
            this.output = Output;
        }
        private void FrmOutput_Load(object sender, EventArgs e)
        {
            rctOutput.Text = this.output;
        }
    }
}
