using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ClEvSy
{
    public class DefField : System.Attribute
    {
        public string FieldName { get; set; }
        public string FieldType { get; set; } 
        public bool IsPrimary {  get; set; }
        public bool Mandatory { get; set; }
    }
    /// <summary>
    /// Struttura per creazione database dinamici
    /// </summary>
    public class EvSyDb
    {
        #region dichiarazioni variabili private
        private string _dataType = "";
        private string _fieldName = "";
        private string _key = "";
        private int _id = 0;
        private bool _isEncrypted = false;
        private long _registry = 0;
        private string _tableName = "";
        private object _value = null;
        #endregion
        public EvSyDb()
        {

        }
        #region public properties 
        /// <summary>
        /// Indice identificativo del field
        /// </summary>
        [DefField(IsPrimary = true, FieldType = "int", FieldName = "Indice campo", Mandatory = true)]
        public int Id { get { return this._id; } set { this._id = value; } }
        /// <summary>
        /// Nome della tabella a cui appartiene il campo
        /// </summary>
        [DefField(IsPrimary = true, FieldType = "string", FieldName = "Nome tabella", Mandatory = true)]
        public string TableName { get { return this._tableName; } set { this._tableName = value; } }
        /// <summary>
        /// Nome del campo
        /// </summary>
        [DefField(IsPrimary = false, FieldType = "string", FieldName = "Nome campo", Mandatory = true)]
        public string FieldName { get { return this._fieldName; } set { this._fieldName = value; } }
        /// <summary>
        /// Tipo del campo
        /// </summary>
        [DefField(IsPrimary = false, FieldType = "string", FieldName = "Tipo dato", Mandatory = true)]
        public string DataType { get { return this._dataType; } set { this._dataType = value; } }
        /// <summary>
        /// Chiave identificativa del campo
        /// </summary>
        [DefField(IsPrimary = true, FieldType = "string", FieldName = "Chiave", Mandatory = true)]
        public string Key { get { return this._key; } set { this._key = value; } }
        /// <summary>
        /// Definisce se il contenuto del campo è cifrato
        /// </summary>
        [DefField(IsPrimary = false, FieldType = "bool", FieldName = "Campo cifrato", Mandatory = true)]
        public bool EncryptedField { get { return this._isEncrypted; } set { this._isEncrypted = value; } }
        /// <summary>
        /// Definizione del comportamento del field in funzione dei bit attivi
        /// </summary>
        [DefField(IsPrimary = false, FieldType = "long", FieldName = "Registro", Mandatory = true)]
        public long Registry { get { return this._registry; } set { this._registry = value; } }
        /// <summary>
        /// Valore del campo
        /// </summary>
        [DefField(IsPrimary = false, FieldType = "object", FieldName = "Valore del campo", Mandatory = false)]
        public object Value { get { return this._value; } set { this._value = value; } }
        #endregion public properties 
    }
    /// <summary>
    /// Classe datatbase
    /// </summary>
    public class EvDbRec : IList<EvSyDb>
    {
        /*
        List<EvSyDb> _items = new List<EvSyDb>();
        private DateTime _created;
        private long _id;
        private string _tableName;
        public EvDbRec()
        {
            
        }

        public DateTime Created { get { return this._created; } }

        public EvDbRec this[int index] { get => ((IList<EvDbRec>)_items)[index]; set => ((IList<EvDbRec>)_items)[index] = value; }

        public int Count => ((ICollection<EvDbRec>)_items).Count;

        public bool IsReadOnly => ((ICollection<EvDbRec>)_items).IsReadOnly;

        public void Add(EvSyDb item)
        {
            ((ICollection<EvSyDb>)_items).Add(item);
        }

        public void Clear()
        {
            ((ICollection<EvDbRec>)_records).Clear();
        }

        public bool Contains(EvDbRec item)
        {
            return ((ICollection<EvDbRec>)_records).Contains(item);
        }

        public void CopyTo(EvDbRec[] array, int arrayIndex)
        {
            ((ICollection<EvDbRec>)_records).CopyTo(array, arrayIndex);
        }

        public IEnumerator<EvDbRec> GetEnumerator()
        {
            return ((IEnumerable<EvDbRec>)_items).GetEnumerator();
        }

        public int IndexOf(EvDbRec item)
        {
            return ((IList<EvDbRec>)_items).IndexOf(item);
        }

        public void Insert(int index, EvDbRec item)
        {
            ((IList<EvDbRec>)_items).Insert(index, item);
        }

        public bool Remove(EvDbRec item)
        {
            return ((ICollection<EvDbRec>)_items).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<EvDbRec>)_items).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }
    }
        */
        private List<EvSyDb> _items = new List<EvSyDb>();
        #region public properties
        public EvSyDb this[int index] { get => ((IList<EvSyDb>)this._items)[index]; set => ((IList<EvSyDb>)_items)[index] = value; }
        public int Count => ((ICollection<EvDbRec>)_items).Count;
        public bool IsReadOnly => ((ICollection<EvSyDb>)this._items).IsReadOnly;
        #endregion
        #region public methods
        /// <summary>
        /// Aggiungi un istanza della classe alla lista
        /// </summary>
        /// <param name="item"></param>
        public void Add(EvSyDb item)
        {
            this._items.Add(item);
        }
        /// <summary>
        /// azzera la lista delle istanze delle classi
        /// </summary>
        public void Clear()
        {
            ((ICollection<EvSyDb>)this._items).Clear();
        }
        /// <summary>
        /// Cerca un istanza di una classe nella lista in base a un sottoinsieme di un istanza
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(EvSyDb item)
        {
            return (this._items).Contains(item);
        }
        /// <summary>
        /// Copia la lista di istanze di classi in un array partendo da un indice
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(EvSyDb[] array, int arrayIndex)
        {
            EvSyDb[] _array = this._items.ToArray();
            _array.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// enu,eratore della classe per poter fare ricerche, raggruppamenti e selezioni
        /// </summary>
        /// <returns></returns>
        public IEnumerator<EvSyDb> GetEnumerator()
        {
            return ((IEnumerable<EvSyDb>)_items).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this._items).GetEnumerator();
        }
        /// <summary>
        /// Ricerca l'item specificato
        /// </summary>
        /// <param name="item"> item da ricercare</param>
        /// <returns></returns>
        public int IndexOf(EvSyDb item)
        {
            for (int i = 0; i < this._items.Count; i++)
            {
                if (this._items[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Ricerca l'item specificato partendo da un indice
        /// </summary>
        /// <param name="item">item da ricercare </param>
        /// <param name="start">posizione di partenza</param>
        /// <returns></returns>
        public int IndexOf(EvSyDb item, int start)
        {
            for (int i = start; i < this._items.Count; i++)
            {
                if (this._items[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        ///  Ricerca l'item specificato partendo da un indice per una determinata lunghezza
        /// </summary>
        /// <param name="item">item da ricercare</param>
        /// <param name="start">posizione di partenza</param>
        /// <param name="length">lunghezza della ricerca</param>
        /// <returns></returns>
        public int IndexOf(EvSyDb item, int start, int length)
        {
            if (length > this._items.Count) length = this._items.Count;
            for (int i = start; i < length; i++)
            {
                if (this._items[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }
        public void Insert(int index, EvSyDb item)
        {
            ((IList<EvSyDb>)this._items).Insert(index, item);
        }
        public bool Remove(EvSyDb item)
        {
            return ((ICollection<EvSyDb>)_items).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<EvSyDb>)this._items).RemoveAt(index);
        }
        #endregion
    }
    /// <summary>
    /// Classe statica per la serializzazione dati
    /// </summary>
    public static class EvSyDbFnc
    {
        private static List<string> _xmlDbList = new List<string>();
        /// <summary>
        /// Serializzazione record database dinamico
        /// </summary>
        /// <param name="EvSyDbInstance">istanza di cui serializzare il contenuto</param>
        /// <returns></returns>
        public static string SerializationEvSyDb(EvSyDb EvSyDbInstance)
        {
            string xmlRet = "";
            XmlSerializer xmls = new XmlSerializer(typeof(EvSyDb));
            using (StringWriter sw = new StringWriter())
            {
                using (XmlWriter xmlw = XmlWriter.Create(sw))
                {
                    xmls.Serialize(xmlw, EvSyDbInstance);
                }
                xmlRet = sw.ToString();
            }
            return xmlRet;
        }
        /// <summary>
        /// Serializazione dell calasse EvDbRec che raccoglie i records dinamici EvSyDb 
        /// </summary>
        /// <param name="EvDbRecInstance">Lista da serializzare</param>
        /// <returns></returns>
        public static string SerializzationEvDbRec(EvDbRec EvDbRecInstance)
        {
            string xmlRet = "";
            XmlSerializer xmls = new XmlSerializer(typeof(EvDbRec));
            using (StringWriter sw = new StringWriter())
            {
                using(XmlWriter xmlw = XmlWriter.Create(sw))
                {
                    xmls.Serialize(xmlw, EvDbRecInstance);
                }
                xmlRet = sw.ToString();
            }
            return xmlRet;
        }
        public static EvSyDb DeserializeEvSyDb(XmlDocument evSyDbXml)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            EvSyDb evSyDb = null;
            try
            {
                XmlSerializer xmls = new XmlSerializer(typeof(EvSyDb));
                string xml = evSyDbXml.InnerXml;
                using (TextReader textReader = new StringReader(xml))
                {
                    XmlReader myXmlReader = XmlReader.Create(textReader);
                    if (xmls.CanDeserialize(myXmlReader))
                    {
                        evSyDb = (EvSyDb)xmls.Deserialize(myXmlReader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return evSyDb;
        }
        public static EvDbRec DeserializeEvDbRec(XmlDocument evSyDbsXml)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            EvDbRec evSyDbs = null;
            try
            {
                XmlSerializer xmls = new XmlSerializer(typeof(EvDbRec));
                string xml = evSyDbsXml.InnerXml;
                using (TextReader TextReader = new StringReader(xml)) 
                {
                    XmlReader myXmlReader = XmlReader.Create(TextReader);
                    if (xmls.CanDeserialize(myXmlReader))
                    {
                        evSyDbs = (EvDbRec)xmls.Deserialize(myXmlReader);
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            return evSyDbs;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<string> GetXmlList()
        {
            return _xmlDbList;
        }
        public static void AddXml(string xmlString)
        {
            _xmlDbList.Add(xmlString);
        }
    }
}
