//2025.05.24 aggiunti tipi floating point
//2025.05.25 aggiunto checkBox per autoincremento indici
using EvolutiveSystem.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvolutiveSystem
{
    public partial class FrmAddField : Form
    {
        List<string> AdmittedAutoInc = new List<string>();
        private Table structTable;
        private Field tableField;
        public FrmAddField(Table StructTable)
        {
            InitializeComponent();
            this.structTable = StructTable;
        }
        public FrmAddField(Field StructField)
        {
            InitializeComponent();
            this.tableField = StructField;
        }
        public FrmAddField(Table StructTable, Field StructField)
        {
            InitializeComponent();
            this.structTable = StructTable;
            this.tableField = StructField;
        }
        private void FrmAddField_Load(object sender, EventArgs e)
        {
            //n this.Text = this.structTable.Table;
            Type ty = typeof(bool);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(short);
            AdmittedAutoInc.Add(ty.Name);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(ushort);
            AdmittedAutoInc.Add(ty.Name);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(int);
            AdmittedAutoInc.Add(ty.Name);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(uint);
            AdmittedAutoInc.Add(ty.Name);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(long);
            AdmittedAutoInc.Add(ty.Name);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(ulong);
            AdmittedAutoInc.Add(ty.Name);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(string);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(DateTime);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(object);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(StringBuilder);
            cmbDataType.Items.Add(ty.Name);
            //2025.05.24 aggiunti tipi floatin point
            ty = typeof(double);
            AdmittedAutoInc.Add(ty.Name);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(decimal);
            AdmittedAutoInc.Add(ty.Name);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(Single);
            AdmittedAutoInc.Add(ty.Name);
            cmbDataType.Items.Add(ty.Name);


            if (this.structTable != null)
            {
                txtTableName.Text = tableField.TableName;
                txtId.Text = structTable.Fields != null ? structTable.Fields.Count.ToString() : "0";
                txtTableName.Enabled = false;
                txtId.Enabled = false;
                txtValue.Enabled = false;
                Type type = typeof(Field);
                PropertyInfo[] pinfo = type.GetProperties();
                foreach (Control ctrlFrm in this.Controls)
                {
                    if (ctrlFrm.GetType().Name == typeof(TextBox).Name)
                    {
                        var f = pinfo.Where(F => F.Name == ctrlFrm.Tag.ToString());
                        if (f.Any())
                        {
                            //ctrlFrm.Text = f.First().GetValue();
                        }
                    }
                }
                foreach (PropertyInfo pinfo2 in pinfo)
                {

                }
            }
            else if(this.tableField != null)
            {
                txtTableName.Text = this.tableField.TableName;
                txtId.Text = this.tableField.Id.ToString();
                txtTableName.Enabled = false;
                txtId.Enabled = false;
                txtValue.Enabled = false;
                txtFieldName.Text = this.tableField.FieldName;
                cmbDataType.SelectedItem = this.tableField.DataType;
                cmbPrymaryKey.SelectedItem = this.tableField.Key == true ? "true" : "false";
                chkAutoincremento.Checked = this.tableField.PrimaryKeyAutoIncrement;
                txtValue.Text = this.tableField.Value == null ? "" : this.tableField.Value.ToString();
                cmbEncrypSel.SelectedItem = this.tableField.EncryptedField == true ? "true" : "false";
                txtRegistry.Text = this.tableField.Registry == ulong.MaxValue ? " " : this.tableField.Registry.ToString();
            }
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            tableField = new Field()
            {
                Id = int.TryParse(txtId.Text, out int id) ? id : 0,
                Value = txtValue.Text,
                TableName = txtTableName.Text,
                FieldName = txtFieldName.Text,
                EncryptedField = cmbDataType.SelectedItem.ToString().ToUpper() == "TRUE" ? true : false,
                Key = cmbPrymaryKey.SelectedItem.ToString().ToUpper() == "TRUE" ? true : false,
                DataType = cmbDataType.Text,
                Registry = ulong.TryParse(txtRegistry.Text, out ulong registry) ? registry : ulong.MaxValue,
                PrimaryKeyAutoIncrement = chkAutoincremento.Checked
            };

        }
        public Field Field { get { return this.tableField; } set { this.tableField = value; } }

        private void cmbPrymaryKey_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            if ((cmb.SelectedItem.ToString().ToUpper() == "TRUE") && AutoIncAdmitted(cmbDataType.SelectedItem.ToString())) chkAutoincremento.Enabled= true; else chkAutoincremento.Enabled= false;
        }

        private void cmbDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            Console.WriteLine(cmb.SelectedItem);

            chkAutoincremento.Enabled = AutoIncAdmitted(cmb.SelectedItem.ToString());
        }
        private bool AutoIncAdmitted(string element)
        {
            bool bRet = false;
            var v = AdmittedAutoInc.Where(SEL => SEL.Equals(element));
            if (v.Any())
            {
                bRet = true;
            }
            return bRet;
        }
    }
}
