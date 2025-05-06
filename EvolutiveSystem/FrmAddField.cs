using ClEvSy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvolutiveSystem
{
    public partial class FrmAddField : Form
    {
        private TreeViewTable structTable;
        private TreeViewField tableField;
        public FrmAddField(TreeViewTable StructTable)
        {
            InitializeComponent();
            this.structTable = StructTable;
        }
        public FrmAddField(TreeViewField StructField)
        {
            InitializeComponent();
            this.tableField = StructField;
        }
        private void FrmAddField_Load(object sender, EventArgs e)
        {
            //n this.Text = this.structTable.Table;
            Type ty = typeof(bool);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(short);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(ushort);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(int);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(uint);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(long);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(ulong);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(string);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(DateTime);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(object);
            cmbDataType.Items.Add(ty.Name);
            ty = typeof(StringBuilder);
            cmbDataType.Items.Add(ty.Name);

            if (this.structTable != null)
            {
                txtTableName.Text = structTable.Table;
                txtId.Text = structTable.Fields != null ? structTable.Fields.Count.ToString() : "0";
                txtTableName.Enabled = false;
                txtId.Enabled = false;
                txtValue.Enabled = false;
                Type type = typeof(TreeViewField);
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
                txtValue.Text = this.tableField.Value.ToString();
                cmbEncrypSel.SelectedItem = this.tableField.EncryptedField == true ? "true" : "false";
                txtRegistry.Text = this.tableField.Registry == ulong.MaxValue ? " " : this.tableField.Registry.ToString();
            }
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            tableField = new TreeViewField()
            {
                Id = int.TryParse(txtId.Text, out int id) ? id : 0,
                Value = txtValue.Text,
                TableName = txtTableName.Text,
                FieldName = txtFieldName.Text,
                EncryptedField = cmbDataType.SelectedItem.ToString().ToUpper() == "TRUE" ? true : false,
                Key = cmbPrymaryKey.SelectedItem.ToString().ToUpper() == "TRUE" ? true : false,
                DataType = cmbDataType.Text,
                Registry = ulong.TryParse(txtRegistry.Text, out ulong registry) ? registry : ulong.MaxValue
            };

        }
        public TreeViewField TableField { get { return tableField; } set { tableField = value; } }
    }
}
