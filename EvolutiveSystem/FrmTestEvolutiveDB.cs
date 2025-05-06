using ClEvSy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.CompilerServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EvolutiveSystem
{
    public partial class FrmTestEvolutiveDB : Form
    {
        //private List<TreeViewMng> treeViewMngs = new List<TreeViewMng>();
        //private TreeViewMng treeSelect = new TreeViewMng();
        private List<TreeRoot> treePath = new List<TreeRoot>();
        private List<TreeviewDatabase> databases = new List<TreeviewDatabase>();
        private TreeviewDatabase dbSel = null;
        private TreeViewTable tableSel = null;
        private TreeViewField fieldSel = null;
        public string dbPathFileName = @"C:\Progetti\EvolutiveSystem\xml\";
        public FrmTestEvolutiveDB()
        {
            InitializeComponent();
        }
        private void FrmTestEvolutiveDB_Load(object sender, EventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            #region ricerca file database
            string[] folderlist = Directory.GetFiles(dbPathFileName, "*.xml", SearchOption.AllDirectories);
            foreach (string folder in folderlist)
            {
                xmlDoc.Load(folder);
                TreeviewDatabase Database = TreeViewMngDeserialize(xmlDoc);
                databases.Add(Database);
            }
            #endregion
            DataGridView dataGrid = null;
            trvDB.Nodes.Clear();
            int index = 0;
            tbcDatabase.TabPages.Clear();
            databases.ForEach(delegate (TreeviewDatabase tdb)
            {
                Type tydb = tdb.GetType();
                tbcDatabase.TabPages.Add(index.ToString(), tdb.DatabaseName);
                tbcDatabase.Font = new Font(this.Font.FontFamily, 12);
                dataGrid = new DataGridView();
                dataGrid.CellBeginEdit += DataGrid_CellBeginEdit;
                dataGrid.CellEndEdit += DataGrid_CellEndEdit;
                dataGrid.Dock = DockStyle.Fill;
                tbcDatabase.TabPages[index].Controls.Add(dataGrid);
#if DEBUG
                Console.WriteLine(tdb.DatabaseName);
#endif
                trvDB.Nodes.Add(tydb.Name, tdb.DatabaseName);
                tdb.Tables.ForEach(delegate (TreeViewTable ttb)
                {
                    Type tytb = ttb.GetType();
#if DEBUG
                    Console.WriteLine(ttb.Table);
#endif
                    trvDB.Nodes[index].Nodes.Insert(ttb.Id, tytb.Name, ttb.Table);
                    ttb.Fields.ForEach(delegate (TreeViewField ttf)
                    {
                        Type tyf = ttf.GetType();
#if DEBUG
                        Console.WriteLine(ttf.FieldName);
#endif
                        if (dataGrid.Columns.Count < ttb.Fields.Count) 
                        {
                            dataGrid.Columns.Add(tyf.Name, ttf.FieldName);
                            dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Blue;
                        }
                        trvDB.Nodes[index].Nodes[ttb.Id].Nodes.Insert(ttf.Id, tyf.Name, ttf.FieldName);
                    });
                });
                index++;
            });
            foreach (DataGridViewTextBoxColumn item in dataGrid.Columns)
            {
                item.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader | DataGridViewAutoSizeColumnMode.DisplayedCells;
            }
        }

        private void DataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void DataGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            
        }

        #region BUTTON
        private void btnAddDb_Click(object sender, EventArgs e)
        {
            // aggiungi database
            FrmDbName fdbName = new FrmDbName();
            fdbName.ShowDialog();
            if (fdbName.DialogResult == DialogResult.OK) 
            {
                TreeviewDatabase treeViewDb = new TreeviewDatabase()
                {
                    DatabaseName = fdbName.DatabaseName,
                    DatabaseId = databases.Count() + 1,
                    ChiSonoIo = typeof(TreeviewDatabase).Name
                };
                databases.Add(treeViewDb);
                trvDB.Nodes.Add(databases.Count.ToString(), fdbName.DatabaseName);
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            foreach (TreeviewDatabase db in databases)
            {
                string xml = TreeViewMngSerialize(db);
                XmlDocument xd = new XmlDocument();
                xd.InnerXml = xml;
                string fileName = string.Format("{0}.xml", Path.Combine(dbPathFileName, db.DatabaseName));
                xd.Save(fileName);
            }
        }
        #endregion
        #region getstione struttura ad albero
        
        //private void cercaNodoMain(TreeNodeCollection item, string serachNode)
        //{
        //    Console.ForegroundColor = ConsoleColor.Red;
        //    Console.WriteLine(serachNode);
        //    Console.ForegroundColor = ConsoleColor.White;
        //    TreeNode n = cercaNodo(item, serachNode);
        //}
        //private TreeNode cercaNodo(TreeNodeCollection item, string serachNode)
        //{
        //    TreeNode foundNode = null;
        //    foreach (TreeNode n in item)
        //    {
        //        if(n.Nodes.Count > 0)
        //        {
        //            Console.ForegroundColor = ConsoleColor.Blue;
        //            Console.WriteLine(n.Text);
        //            Console.ForegroundColor = ConsoleColor.White;

        //            cercaNodo(n.Nodes, serachNode);
        //        }
        //        else
        //        {
        //            Console.ForegroundColor= ConsoleColor.Green;
        //            Console.WriteLine(n.Text);
        //            Console.ForegroundColor = ConsoleColor.White;
        //            foundNode = n;
        //        }
        //    }
        //    return foundNode;
        //}
        
        /*
        private string NodoPrincipale(TreeNode treeNode)
        {
            string ret = "";
            if (treeNode.Parent!= null)
            {
                if (treeNode.Parent.Parent != null)
                {
                    //Console.WriteLine("NODE PARENT: {0} NODE PARENT PARENT: {1}", e.Node.Parent.Text, e.Node.Parent.Parent.Text);
                    ret = treeNode.Parent.Parent.Text.Trim();
                }
                else
                {
                    ret = treeNode.Parent.Text.Trim();
                }
            }
            else
            {
                ret = treeNode.Text.Trim();
            }
            return ret;
        }
        */
        private string NodoPrincipale1(TreeNode treeNode)
        {
            string ret = "";
            if (treeNode.Parent != null)
            {
                TreeRoot tr = new TreeRoot()
                {
                    Name = treeNode.Text,
                    Index = treePath.Count + 1,
                    IoChiSono = treeNode.Name
                };
                treePath.Add(tr);
                ret = NodoPrincipale1(treeNode.Parent);
            }
            else
            {
                TreeRoot tr = new TreeRoot()
                {
                    Name = treeNode.Text,
                    Index = treePath.Count + 1,
                    IoChiSono = treeNode.Name
                };
                treePath.Add(tr);
                ret = treeNode.Text;
            }
            return ret;
        }
        private void trvDB_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Modifica database
                var v = databases.Where(TR => TR.DatabaseName == e.Node.Text);
                if (v.Any())
                {
                    // aggiungi tabella
                    TreeviewDatabase DBSelect = v.First() as TreeviewDatabase;
                    Point p = PointToScreen(e.Location);
                    ctxTableManager.Show(p);
                }
                else
                {
                    if (e.Node.Name== "TreeViewTable")
                    {
                        Point p = PointToScreen(e.Location);
                        ctxFieldManager.Show(p);
                    }
                    else
                    {
                        Point p = PointToScreen(e.Location);
                        ctxEditField.Show(p);
                    }
                }
            }
            else
            {
                this.treePath.Clear();
                string nodoPrincipale = NodoPrincipale1(e.Node);
                lwField.Clear();
                lwField.Columns.Clear();
                lwField.Items.Clear();
                lblTblName.Text = "";
                foreach (var item in this.treePath)
                {

                    string sw = item.IoChiSono;
                    switch (sw)
                    {
                        case "TreeviewDatabase":
                            {
                                lblDbName.Text = nodoPrincipale;
                            }
                            break;
                        case "TreeViewTable":
                            {
                                lblTblName.Text = item.Name;
                                this.databases.ForEach(delegate (TreeviewDatabase tdb)
                                {
                                    Console.WriteLine(tdb.Tables.Count);
                                    var tbl = tdb.Tables.Where(TBL => TBL.Table == item.Name);
                                    if (tbl.Any())
                                    {
                                        if (tbl.First().Fields.Count() > 0)
                                        {
                                            Type tyFld = tbl.First().Fields[0].GetType();
                                            PropertyInfo[] pInfo = tyFld.GetProperties();
                                            lwField.Columns.Add(pInfo[0].Name.Trim(), 30, HorizontalAlignment.Left);
                                            lwField.Columns.Add(pInfo[1].Name.Trim(), 80, HorizontalAlignment.Left);
                                            lwField.Columns.Add(pInfo[4].Name.Trim(), 80, HorizontalAlignment.Left);
                                            lwField.Columns.Add(pInfo[2].Name.Trim(), 80, HorizontalAlignment.Left);
                                            lwField.Columns.Add(pInfo[6].Name.Trim(), 80, HorizontalAlignment.Left);
                                            for (int i = 0; lwField.Columns.Count > i; i++)
                                            {
                                                lwField.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                                                lwField.Columns[i].TextAlign = HorizontalAlignment.Left;
                                            }
                                            tbl.First().Fields.ForEach(delegate (TreeViewField fld)
                                            {
                                                string f = string.Format("{0} {1} {2}", pInfo[1].Name, pInfo[4].Name, pInfo[0]);
                                                ListViewItem item1 = new ListViewItem(fld.Id.ToString(), fld.Id);
                                                item1.SubItems.Add(fld.FieldName);
                                                item1.SubItems.Add(fld.DataType);
                                                item1.SubItems.Add(fld.Key == false ? "NO" : "SI");
                                                item1.SubItems.Add(fld.EncryptedField == false ? "NO" : "SI");
                                                lwField.Items.Add(item1);
                                            });
                                        }
                                    }
                                });
                                //var tbl = this.databases.Where(DB => DB.Tables.Where(TBL => TBL.Table == item.Name).Any());
                                //if (tbl.Any())
                                //{
                                //    tbl.First().Tables
                                //}
                            }
                            break;
                        case "TreeViewField":
                            {

                            }
                            break;
                    }
                }
            }
        }
        #region ramo gestione tabelle
        private void tsmAddTable_Click(object sender, EventArgs e)
        {
            // aggiungi tabella
            if(trvDB.SelectedNode != null)
            {
                FrmAddTable ftblName = new FrmAddTable(trvDB.SelectedNode.Text);
                if (ftblName.ShowDialog() == DialogResult.OK)
                {
                    int index = 0;
                    string dbSel = NodoPrincipale1(trvDB.SelectedNode);
                    var dbs = this.databases.Where(DBS => DBS.DatabaseName == trvDB.SelectedNode.Text);
                    if (dbs.Any() )
                    {
                        index = dbs.First().Tables.Count + 1; 
                    }
                    trvDB.Nodes[trvDB.SelectedNode.Name].Nodes.Insert(index,typeof(TreeViewTable).Name, ftblName.TableName);
                    var db = databases.Where(TB => TB.DatabaseName == trvDB.SelectedNode.Text);
                    if (db.Any())
                    {
                        TreeviewDatabase lDbCls = db.First() as TreeviewDatabase;
                        TreeViewTable ltrvt = new TreeViewTable()
                        {
                            Table = ftblName.TableName,
                            TableId = lDbCls.Tables.Count() + 1,
                            ChiSonoIo = typeof(TreeViewTable).Name
                        };
                        lDbCls.Tables.Add(ltrvt);
                    }
                }
            }
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FAI QUALCOSA");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        private void tsmRemoveTable_Click(object sender, EventArgs e)
        {
            //var tbl = this.databases.Where(DB => DB.Tables.Where(TBL => TBL.Table == trvDB.SelectedNode.Text).Any());
            if (trvDB.SelectedNode != null)
            {
                string dbSel = NodoPrincipale1(trvDB.SelectedNode);
                var dba = this.databases.Where(DB => DB.DatabaseName == dbSel);
                if (dba.Any())
                {
                    var tbl = dba.First().Tables.Where(TBL => TBL.Table == trvDB.SelectedNode.Text);
                    if (tbl.Any())
                    {
                        if(MessageBox.Show("Rimuovere la tabella?","Attenzione",MessageBoxButtons.OKCancel,MessageBoxIcon.Question) == DialogResult.OK)
                        {
                            dba.First().Tables.Remove(tbl.First());
                            trvDB.SelectedNode.Remove();
                        }
                    }
                }
            }
            else
            {
                Console.ForegroundColor= ConsoleColor.Red;
                Console.WriteLine("FAI QUALCOSA");
                Console.ForegroundColor = ConsoleColor.White;
            }

        }
        #endregion
        #region ramo gestione campi campo
        private void tsmAddField_Click(object sender, EventArgs e)
        {
            // aggiungi campo
            if (trvDB.SelectedNode != null && trvDB.SelectedNode.Parent != null) 
            {
                Console.WriteLine("Text: {0} Nome: {1}", trvDB.SelectedNode.Parent.Text, trvDB.SelectedNode.Parent.Name);
                var dbTree = this.databases.Where(DB => DB.DatabaseName == trvDB.SelectedNode.Parent.Text);
                if (dbTree.Any())
                {
                    var tb = dbTree.First().Tables.Where(TB => TB.Table == trvDB.SelectedNode.Text);
                    if (tb.Any())
                    {
                        TreeViewTable tvt = tb.First() as TreeViewTable;
                        FrmAddField ftblName = new FrmAddField(tvt);
                        if (ftblName.ShowDialog()==DialogResult.OK)
                        {
                            TreeViewField treeViewField = new TreeViewField()
                            {
                                ChiSonoIo = typeof(TreeViewField).Name,
                                DataType = ftblName.TableField.DataType,
                                encryptedField = ftblName.TableField.EncryptedField,
                                FieldName = ftblName.TableField.FieldName,
                                Id = ftblName.TableField.Id,
                                Key = ftblName.TableField.Key,
                                registry = ftblName.TableField.Registry,
                                TableName = ftblName.TableField.TableName,
                                Value = ftblName.TableField.Value
                            };
                            trvDB.SelectedNode.Nodes.Insert(ftblName.TableField.Id, treeViewField.ChiSonoIo, ftblName.TableField.FieldName);
                            tvt.Fields.Add(treeViewField);
                        }

                    }
                }
            }
        }
        #endregion
        #region edit field
        private void tsmEditField_Click(object sender, EventArgs e)
        {
            if (trvDB.SelectedNode.Parent != null)
            {
                this.treePath.Clear();
                string dbSel = NodoPrincipale1(trvDB.SelectedNode);
                var dbTree = this.databases.Where(DB => DB.DatabaseName == dbSel);
                if (dbTree.Any())
                {
                    var tblTree = dbTree.First().Tables.Where(TBL => TBL.Table == trvDB.SelectedNode.Parent.Text);
                    if (tblTree.Any())
                    {
                        var fldTree = tblTree.First().Fields.Where(FLD => FLD.FieldName == trvDB.SelectedNode.Text);
                        if (fldTree.Any())
                        {
                            TreeViewField tvf = fldTree.First() as TreeViewField;
                            FrmAddField frmEditField = new FrmAddField(tvf);
                            if (frmEditField.ShowDialog() == DialogResult.OK) 
                            {
                                TreeViewField treeViewField = new TreeViewField()
                                {
                                    ChiSonoIo = typeof(TreeViewField).Name,
                                    DataType = frmEditField.TableField.DataType,
                                    encryptedField = frmEditField.TableField.EncryptedField,
                                    FieldName = frmEditField.TableField.FieldName,
                                    Id = frmEditField.TableField.Id,
                                    Key = frmEditField.TableField.Key,
                                    registry = frmEditField.TableField.Registry,
                                    TableName = frmEditField.TableField.TableName,
                                    Value = frmEditField.TableField.Value
                                };
                                tvf = treeViewField;
                                tblTree.First().Fields[tvf.Id] = tvf;
                            }
                        }
                        
                    }
                }
            }
        }
        private void tsmDelField_Click(object sender, EventArgs e)
        {
            if (trvDB.SelectedNode != null)
            {
                string dbSel = NodoPrincipale1(trvDB.SelectedNode);
                var dba = this.databases.Where(DB => DB.DatabaseName == dbSel);
                if (dba.Any())
                {
                    var tbl = dba.First().Tables.Where(TBL => TBL.Table == trvDB.SelectedNode.Parent.Text);
                    if (tbl.Any())
                    {
                        var fld = tbl.First().Fields.Where(FLD => FLD.FieldName == trvDB.SelectedNode.Text);
                        if (fld.Any())
                        {
                            if (MessageBox.Show("Rimuovere il campo?", "Attenzione", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                            {
                                dba.First().Fields.Remove(tbl.First());
                                trvDB.SelectedNode.Remove();
                            }
                        }
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FAI QUALCOSA");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        #endregion
        #endregion
        private string TreeViewMngSerialize(TreeviewDatabase treeviewDatabase)
        {
            string xmlRet = "";
            XmlSerializer xmls = new XmlSerializer(typeof(TreeviewDatabase));
            using (StringWriter sw = new StringWriter())
            {
                using (XmlWriter xmlw = XmlWriter.Create(sw))
                {
                    xmls.Serialize(xmlw, treeviewDatabase);
                }
                xmlRet = sw.ToString();
            }
            return xmlRet;
        }
        private TreeviewDatabase TreeViewMngDeserialize(XmlDocument xmlDocument)
        {
            TreeviewDatabase tvm = null;
            XmlSerializer xmls = new XmlSerializer(typeof(TreeviewDatabase));
            string xml = xmlDocument.InnerXml;
            using (TextReader textReader = new StringReader(xml))
            {
                XmlReader myXmlReader = XmlReader.Create(textReader);
                if (xmls.CanDeserialize(myXmlReader))
                {
                    tvm = (TreeviewDatabase)xmls.Deserialize(myXmlReader);
                }
            }
            return tvm;
        }
    }
}
 