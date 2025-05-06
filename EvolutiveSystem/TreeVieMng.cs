using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EvolutiveSystem
{
    //public class TreeViewMng: TreeviewDatabase
    //{
    //    List<TreeViewTable> tables = new List<TreeViewTable>();
    //    public TreeViewMng()
    //    {

    //    }
    //    public TreeViewMng(int databaseId, string datbaseName)
    //    {
    //        this.DatabaseId = databaseId;
    //        this.DatabaseName = datbaseName;
    //    }

    //    // public List<TreeViewTable> Tables { get { return this.tables; } set { this.tables = value; } }
    //}
    public interface ITreeViewField
    {
        int Id { get; set; }
        string FieldName { get; set; }
        bool Key {  get; set; }
        string TableName {  get; set; }
        string DataType { get; set; }
        object Value { get; set; }
        bool EncryptedField { get; set; }
        ulong Registry {  get; set; }
        string ChiSonoIo {  get; set; }
    }
    public class TreeviewDatabase: TreeViewTable
    {
        private int dataBaseId = 0;
        private string dataBaseName = "";
        private List<TreeViewTable> tables = new List<TreeViewTable>();
        public int DatabaseId { get { return this.dataBaseId; } set { this.dataBaseId = value; } }
        public string DatabaseName { get { return this.dataBaseName; } set { this.dataBaseName = value; } }
        public List<TreeViewTable> Tables { get { return this.tables; } set { this.tables = value; } }
        public override string ChiSonoIo { get { return this.chiSonoIo; } set { this.chiSonoIo = value; } }
    }
    public class TreeViewTable: TreeViewField
    {
        private List<TreeViewField> fields = new List<TreeViewField>();
        private string table = "";
        private int tableId = 0;
        /// <summary>
        /// Indice identificativo della tabella
        /// </summary>
        public int TableId { get { return this.tableId; } set { this.tableId = value; } }
        /// <summary>
        /// Nome della tabella
        /// </summary>
        public string Table { get { return this.table; } set { this.table = value; } }
        /// <summary>
        /// Lista campi della tabella
        /// </summary>
        public List<TreeViewField> Fields { get { return this.fields; } set { this.fields = value; } }
        /// <summary>
        /// Identificazione di appartenenza della tabella
        /// </summary>
        public override string ChiSonoIo { get; set; }
    }
    public class TreeViewField: ITreeViewField
    {
        private int id = 0;
        private string tableName = "";
        private bool key = false;
        private string fieldName = "";
        private string dataType = "";
        public object value = null;
        public bool encryptedField = false;
        public ulong registry = ulong.MaxValue;
        public string chiSonoIo = "";
        /// <summary>
        /// Indice indentificatore del campo
        /// </summary>
        public int Id { get {return this.id; } set { this.id = value; } }
        /// <summary>
        /// Nome del campo
        /// </summary>
        public string FieldName { get { return this.fieldName; } set { this.fieldName = value; } }
        /// <summary>
        /// Flag di identificazione se è chiave primaria
        /// </summary>
        public bool Key { get { return this.key; } set { this.key = value; } }
        /// <summary>
        /// Tabella a cui appartiene il campo
        /// </summary>
        public string TableName { get { return this.tableName; } set { this.tableName = value; } }
        /// <summary>
        /// Tipo dato
        /// </summary>
        public string DataType { get { return this.dataType; } set { this.dataType = value; } }
        /// <summary>
        /// Valore contenuto nel campo
        /// </summary>
        public object Value { get { return this.value; } set { this.value = value; } }
        /// <summary>
        /// Identificazione di cifratura del campo
        /// </summary>
        public bool EncryptedField { get { return this.encryptedField; } set { this.encryptedField = value; } }
        /// <summary>
        /// Valore per uso generico nel campo
        /// </summary>
        public ulong Registry { get { return this.registry; } set { this.registry = value; } }
        /// <summary>
        /// Identificazione d'appartenenza del campo
        /// </summary>
        public virtual string ChiSonoIo { get { return this.chiSonoIo; } set { this.chiSonoIo = value; } }

    }
    public class TreeRoot
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string IoChiSono { get; set; }
    }
}
