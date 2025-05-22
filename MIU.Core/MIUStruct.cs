using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization; // Necessario per la serializzazione XML

    // ====================================================================================
    // CLASSI DEL MODELLO DI DATABASE (VERSIONI SEMPLIFICATE PER L'ESEMPIO)
    // Queste classi sono necessarie per la deserializzazione del file XML
    // e dovrebbero corrispondere alla struttura del tuo MIUProject.xml.
    // ====================================================================================

    /// <summary>
    /// Rappresenta la radice del database XML.
    /// </summary>
    [XmlRoot("Database")]
    public class Database
    {
        public int DatabaseId { get; set; }
        public string DatabaseName { get; set; }

        [XmlArray("Tables")]
        [XmlArrayItem("Table")]
        public List<Table> Tables { get; set; } = new List<Table>();
    }
    /// <summary>
    /// Rappresenta una tabella all'interno del database.
    /// </summary>
    public class Table
    {
        /// <summary>
        /// Ottiene o imposta l'ID univoco della tabella.
        /// </summary>
        public int TableId { get; set; }
        /// <summary>
        /// Ottiene o imposta il nome della tabella.
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Ottiene o imposta la lista dei campi della tabella.
        /// Questo array sarà serializzato come "Fields" con elementi "Field".
        /// </summary>
        [XmlArray("Fields")]
        [XmlArrayItem("Field")]
        public List<Field> Fields { get; set; } = new List<Field>();
        /// <summary>
        /// Ottiene o imposta la lista dei record di dati della tabella.
        /// Ogni record è un dizionario serializzabile di stringhe e oggetti.
        /// Questo array sarà serializzato come "DataRecords" con elementi "SerializableDictionaryOfStringObject".
        /// </summary>
        [XmlArray("DataRecords")]
        [XmlArrayItem("SerializableDictionaryOfStringObject")]
        public List<SerializableDictionaryOfStringObject> DataRecords { get; set; } = new List<SerializableDictionaryOfStringObject>();
    }
    /// <summary>
    /// Rappresenta un campo di una tabella.
    /// </summary>
    public class Field
    {
        public int Id { get; set; }
        public string FieldName { get; set; }
        public bool Key { get; set; }
        public string TableName { get; set; }
        public string DataType { get; set; }
        public bool EncryptedField { get; set; }
        public int Registry { get; set; }
        public string ElementType { get; set; }
    }
    /// <summary>
    /// Rappresenta l'elemento <SerializableDictionaryOfStringObject> che contiene gli item.
    /// </summary>
    public class SerializableDictionaryOfStringObject
    {
        [XmlElement("item")]
        public List<SerializableDictionaryItem> Items { get; set; } = new List<SerializableDictionaryItem>();

        /// <summary>
        /// Metodo helper per ottenere un valore dato il nome della chiave.
        /// </summary>
        /// <param name="keyName">Il nome della chiave (es. "ID", "Nome").</param>
        /// <returns>Il valore della chiave come stringa, o null se non trovato.</returns>
        public string GetValue(string keyName)
        {
            var item = Items.FirstOrDefault(i => i.Key?.Value == keyName);
            return item?.Value?.TypedValue; // Accede al valore effettivo tramite TypedValue
        }

    }
    /// <summary>
    /// Rappresenta un singolo <item> all'interno di SerializableDictionaryOfStringObject.
    /// </summary>
    public class SerializableDictionaryItem
    {
        [XmlElement("key")]
        public KeyWrapper Key { get; set; }

        [XmlElement("value")]
        public ValueWrapper Value { get; set; }
    }

    /// <summary>
    /// Rappresenta l'elemento <key> con il suo valore <string>.
    /// </summary>
    public class KeyWrapper
    {
        [XmlElement("string")]
        public string Value { get; set; }
    }
    /// <summary>
    /// Rappresenta l'elemento <value> che può contenere un <anyType>.
    /// </summary>
    public class ValueWrapper
    {
        [XmlAnyElement]
        public XmlElement AnyTypeValue { get; set; }

        /// <summary>
        /// Restituisce il valore effettivo contenuto nell'elemento anyType.
        /// </summary>
        public string TypedValue
        {
            get
            {
                if (AnyTypeValue != null)
                {
                    return AnyTypeValue.InnerText;
                }
                return null;
            }
        }
    }
}