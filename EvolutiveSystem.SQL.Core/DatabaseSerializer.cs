using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace EvolutiveSystem.SQL.Core
{
    /// <summary>
    /// Classe statica per gestire la serializzazione e deserializzazione
    /// degli oggetti Database in formato XML.
    /// </summary>
    public static class DatabaseSerializer
    {
        /// <summary>
        /// Restituisce un array di tipi noti per la serializzazione.
        /// Questo è necessario per serializzare proprietà di tipo 'object' (come Field.Value)
        /// e per la classe SerializableDictionary che contiene object.
        /// </summary>
        public static Type[] GetKnownTypes() // Reso pubblico per essere accessibile da SerializableDictionary
        {
            return new Type[]
            {
                typeof(string), // Esempio: se Field.Value può essere una stringa
                typeof(bool),    // Esempio: se Field.Value può essere un booleano
                typeof(int),     // System.Int32
                typeof(short),   // System.Int16
                typeof(long),    // System.Int64
                typeof(uint),    // System.UInt32
                typeof(ulong),   // System.UInt64
                typeof(DateTime), // DateTime
                typeof(StringBuilder), // StringBuilder
                typeof(float), // StringBuilder
                typeof(double),
                typeof(decimal),
                // Aggiungi qui tutti gli altri tipi concreti che la proprietà 'Value'
                // nella tua classe Field o nei valori di SerializableDictionary
                // possono assumere.
                // Esempio: typeof(MyCustomClass), typeof(List<int>), ecc.

                // Dobbiamo informare XmlSerializer che potrebbe incontrare
                // istanze di SerializableDictionary<string, object>
                typeof(SerializableDictionary<string, object>)
            };
        }

        /// <summary>
        /// Serializza un oggetto Database in un file XML.
        /// </summary>
        /// <param name="database">L'oggetto Database da serializzare.</param>
        /// <param name="filePath">Il percorso del file in cui salvare l'XML.</param>
        public static void SerializeToXmlFile(Database database, string filePath)
        {
            // Usa i tipi noti per il serializzatore principale
            XmlSerializer serializer = new XmlSerializer(typeof(Database), GetKnownTypes());

            // Usa un FileStream per scrivere l'XML su un file.
            // Usa using per assicurarti che lo stream venga chiuso correttamente.
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                // Usa un XmlWriter per un maggiore controllo sulla formattazione (indentazione)
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
        /// QUA
        public static string SerializeToXmlString(Database database)
        {
            // Usa i tipi noti per il serializzatore principale
            XmlSerializer serializer = new XmlSerializer(typeof(Database), GetKnownTypes());
            // Usa StringWriter per scrivere l'XML in memoria (come stringa).
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
        public static Database DeserializeFromXmlFile(string filePath)
        {
            Database loadedDb;
            // Usa i tipi noti per il serializzatore principale
            XmlSerializer serializer = new XmlSerializer(typeof(Database), GetKnownTypes());
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(fs))
                {
                    // Deserializza l'oggetto dallo stream.
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
        public static Database DeserializeFromXmlString(string xmlString)
        {
            Database loadedDb;
            // Usa i tipi noti per il serializzatore principale
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
        public static void RestoreParentReferences(Database database) // Reso pubblico per essere chiamato esternamente se necessario
        {
            if (database == null) return;

            // Itera su tutte le tabelle nel database
            if (database.Tables != null)
            {
                foreach (var table in database.Tables)
                {
                    // Imposta il riferimento al database padre per la tabella
                    table.ParentDatabase = database;

                    // Itera su tutti i campi nella tabella
                    if (table.Fields != null)
                    {
                        foreach (var field in table.Fields)
                        {
                            // Imposta il riferimento alla tabella madre per il campo
                            field.ParentTable = table;
                            // Assicurati anche che il nome della tabella nel campo sia corretto (utile per DataPropertyName nel DataGridView)
                            field.TableName = table.TableName;
                        }
                    }
                }
            }
        }
    }
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
            return null; // Schema non necessario per questo esempio
        }

        /// <summary>
        /// Legge i dati XML e popola il dizionario.
        /// </summary>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            // Passa i tipi noti al serializzatore del valore per gestire 'object'
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), DatabaseSerializer.GetKnownTypes());

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item"); // Legge l'elemento <item>

                reader.ReadStartElement("key"); // Legge l'elemento <key>
                TKey key = (TKey)keySerializer.Deserialize(reader); // Deserializza la chiave
                reader.ReadEndElement(); // Legge l'elemento </key>

                reader.ReadStartElement("value"); // Legge l'elemento <value>
                TValue value = (TValue)valueSerializer.Deserialize(reader); // Deserializza il valore usando il serializzatore con tipi noti
                reader.ReadEndElement(); // Legge l'elemento </value>

                this.Add(key, value); // Aggiunge la coppia chiave-valore al dizionario

                reader.ReadEndElement(); // Legge l'elemento </item>
                reader.MoveToContent(); // Sposta il reader al prossimo nodo di contenuto
            }
            reader.ReadEndElement(); // Legge l'elemento </dictionary>
        }

        /// <summary>
        /// Scrive i dati del dizionario in formato XML.
        /// </summary>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            // Passa i tipi noti al serializzatore del valore per gestire 'object'
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), DatabaseSerializer.GetKnownTypes());

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item"); // Scrive l'elemento <item>

                writer.WriteStartElement("key"); // Scrive l'elemento <key>
                keySerializer.Serialize(writer, key); // Serializza la chiave
                writer.WriteEndElement(); // Scrive l'elemento </key>

                writer.WriteStartElement("value"); // Scrive l'elemento <value>
                valueSerializer.Serialize(writer, this[key]); // Serializza il valore usando il serializzatore con tipi noti
                writer.WriteEndElement(); // Scrive l'elemento </value>

                writer.WriteEndElement(); // Scrive l'elemento </item>
            }
        }
    }
}
