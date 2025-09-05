using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using EvolutiveSystem.Common; // Aggiungi il riferimento all'interfaccia

namespace EvolutiveSystem.SQL.Core
{
    /// <summary>
    /// Classe per gestire la serializzazione e deserializzazione
    /// degli oggetti Database in formato XML.
    /// </summary>
    public class DatabaseSerializer
    {
        private readonly IKnownTypeProvider _knownTypeProvider;

        /// <summary>
        /// Costruttore che accetta un provider di tipi noti.
        /// Questo applica il principio di Iniezione delle Dipendenze.
        /// </summary>
        /// <param name="knownTypeProvider">L'implementazione dell'interfaccia IKnownTypeProvider.</param>
        public DatabaseSerializer(IKnownTypeProvider knownTypeProvider)
        {
            _knownTypeProvider = knownTypeProvider;
        }

        // Il metodo GetKnownTypes non esiste più, ora usiamo l'interfaccia
        public Type[] GetKnownTypes()
        {
            return _knownTypeProvider.GetKnownTypes();
        }

        /// <summary>
        /// Serializza un oggetto Database in un file XML.
        /// </summary>
        /// <param name="database">L'oggetto Database da serializzare.</param>
        /// <param name="filePath">Il percorso del file in cui salvare l'XML.</param>
        public void SerializeToXmlFile(Database database, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Database), GetKnownTypes());
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(fs, settings))
                {
                    serializer.Serialize(writer, database);
                }
            }
        }

        /// <summary>
        /// Serializza un oggetto Database in una stringa XML.
        /// </summary>
        /// <param name="database">L'oggetto Database da serializzare.</param>
        /// <returns>Una stringa contenente la rappresentazione XML del Database.</returns>
        public string SerializeToXmlString(Database database)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Database), GetKnownTypes());
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, database);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Deserializza un oggetto Database da un file XML.
        /// </summary>
        /// <param name="filePath">Il percorso del file XML da cui leggere.</param>
        /// <returns>L'oggetto Database deserializzato.</returns>
        public Database DeserializeFromXmlFile(string filePath)
        {
            Database loadedDb;
            XmlSerializer serializer = new XmlSerializer(typeof(Database), GetKnownTypes());
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(fs))
                {
                    loadedDb = (Database)serializer.Deserialize(reader);
                }
            }
            if (loadedDb != null)
            {
                RestoreParentReferences(loadedDb);
            }

            return loadedDb;
        }

        /// <summary>
        /// Deserializza un oggetto Database da una stringa XML.
        /// </summary>
        /// <param name="xmlString">La stringa contenente l'XML.</param>
        /// <returns>L'oggetto Database deserializzato.</returns>
        public Database DeserializeFromXmlString(string xmlString)
        {
            Database loadedDb;
            XmlSerializer serializer = new XmlSerializer(typeof(Database), GetKnownTypes());
            using (StringReader reader = new StringReader(xmlString))
            {
                loadedDb = (Database)serializer.Deserialize(reader);
            }
            if (loadedDb != null)
            {
                RestoreParentReferences(loadedDb);
            }

            return loadedDb;
        }

        /// <summary>
        /// Metodo helper per ripristinare i riferimenti ParentDatabase e ParentTable
        /// dopo la deserializzazione.
        /// </summary>
        /// <param name="database">Il database deserializzato.</param>
        public static void RestoreParentReferences(Database database)
        {
            if (database == null) return;

            if (database.Tables != null)
            {
                foreach (var table in database.Tables)
                {
                    table.ParentDatabase = database;
                    if (table.Fields != null)
                    {
                        foreach (var field in table.Fields)
                        {
                            field.ParentTable = table;
                            field.TableName = table.TableName;
                        }
                    }
                }
            }
        }
    }
    /* 25.09.03 Pesante ristrutturazione per garantire la separazione dei compiti
    /// <summary>
    /// Classe helper per serializzare Dictionary<TKey, TValue> con XmlSerializer.
    /// Eredita da Dictionary e implementa IXmlSerializable.
    /// </summary>
    /// <typeparam name="TKey">Tipo della chiave del dizionario.</typeparam>
    /// <typeparam name="TValue">Tipo del valore del dizionario.</typeparam>
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Legge i dati XML e popola il dizionario.
        /// </summary>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            // Qui la chiamata a DatabaseSerializer.GetKnownTypes() causerà un errore di compilazione
            // perché non è più un metodo statico.
            // Il problema è che SerializableDictionary non sa dove trovare i tipi noti.
            // La soluzione è che la classe che usa SerializableDictionary si occupi di passare i tipi noti.
            // Per il momento lascio la riga, la correggeremo quando saremo pronti.
            // La dipendenza è forte
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), new EvolutiveSystem.SQL.Core.DatabaseSerializer(null).GetKnownTypes());

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();
                this.Add(key, value);
                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        /// <summary>
        /// Scrive i dati del dizionario in formato XML.
        /// </summary>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            // Anche qui, l'accesso statico a DatabaseSerializer.GetKnownTypes() non funziona più.
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), new EvolutiveSystem.SQL.Core.DatabaseSerializer(null).GetKnownTypes());

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                valueSerializer.Serialize(writer, this[key]);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }
    */
}