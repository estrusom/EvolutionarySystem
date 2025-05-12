using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace EvolutiveSystem.Core
{
    public interface IFieldData
    {
        int Id { get; set; }
        string FieldName { get; set; }
        bool Key { get; set; }
        string TableName { get; set; } // Utile per riferimento incrociato
        string DataType { get; set; }
        object Value { get; set; } // Attenzione qui: object richiede boxing/unboxing. Considera tipi specifici se possibile.
        bool EncryptedField { get; set; }
        ulong Registry { get; set; }
    }
    public class Field : IFieldData // Ho rinominato per chiarezza e generalità
    {
        // Proprietà specifiche di un Campo
        public int Id { get; set; }
        public string FieldName { get; set; }
        public bool Key { get; set; }
        public string TableName { get; set; } // Riferimento al nome della tabella
        public string DataType { get; set; }
        public object Value { get; set; }
        public bool EncryptedField { get; set; }
        public ulong Registry { get; set; }
        public SemanticElementType ElementType { get; set; }

        // *** Composizione/Riferimento per la Provenienza: Un Campo appartiene a una Tabella ***
        // Questo riferimento permette di risalire alla tabella madre direttamente.
        // [XmlIgnore] // Ignora questa proprietà durante la serializzazione XML per evitare riferimenti circolari
        [XmlIgnore]
        public Table ParentTable { get; set; } // Ho rinominato la classe Tabella

        // Costruttore vuoto per la serializzazione
        public Field() { }

        public Field(int id, string fieldName, string dataType, bool key, bool encryptedField, ulong registry, Table parentTable, object value)
        {
            Id = id;
            FieldName = fieldName;
            DataType = dataType;
            Key = key;
            EncryptedField = encryptedField;
            Registry = registry;
            ParentTable = parentTable;
            Value = value;
            TableName = parentTable?.TableName; // Imposta il nome della tabella di appartenenza
        }

        // Se hai bisogno di un identificatore logico (es. "PostulateField"),
        // potresti aggiungerlo qui, magari come Enum per maggiore sicurezza.
        // public string LogicalType { get; set; }
    }
    // Classe per rappresentare una singola Tabella (Table)
    // NON eredita da Field.
    public class Table
    {
        // Proprietà specifiche di una Tabella
        public int TableId { get; set; }
        public string TableName { get; set; } // Ho rinominato per chiarezza

        // *** Composizione: Una Tabella CONTIENE una lista di Campi ***
        public List<Field> Fields { get; set; } = new List<Field>(); // Inizializzata per evitare NullReferenceException

        // *** Composizione/Riferimento per la Provenienza: Una Tabella appartiene a un Database ***
        // Questo riferimento permette di risalire al database madre direttamente.
        // [XmlIgnore] // Ignora questa proprietà durante la serializzazione XML per evitare riferimenti circolari
        [XmlIgnore]
        public Database ParentDatabase { get; set; } // Ho rinominato la classe Database

        // *** Composizione: Una Tabella CONTIENE una lista di Record (dati) ***
        // Ogni record è un dizionario che mappa il nome del campo al suo valore.
        // Usiamo SerializableDictionary per permettere la serializzazione XML.
        public List<SerializableDictionary<string, object>> DataRecords { get; set; } = new List<SerializableDictionary<string, object>>();


        // Se hai bisogno di un identificatore logico (es. "PostulateTable", "RuleTable"),
        // potresti aggiungerlo qui, magari come Enum.
        // public string LogicalType { get; set; }

        // Costruttore vuoto per la serializzazione
        public Table() { }

        public Table(int tableId, string tableName, Database parentDatabase)
        {
            TableId = tableId;
            TableName = tableName;
            ParentDatabase = parentDatabase;
        }

        /// <summary>
        /// Aggiunge un campo alla tabella.
        /// </summary>
        /// <param name="field">Il campo da aggiungere.</param>
        public void AddField(Field field)
        {
            if (field != null)
            {
                Fields.Add(field);
                field.ParentTable = this; // Imposta il riferimento alla tabella madre nel campo
                field.TableName = this.TableName; // Imposta anche il nome della tabella nel campo
            }
        }
    }
    // Classe per rappresentare un singolo Database (Database)
    // NON eredita da Table o Field.
    public class Database
    {
        // Proprietà specifiche di un Database
        public int DatabaseId { get; set; }
        public string DatabaseName { get; set; }

        // *** Composizione: Un Database CONTIENE una lista di Tabelle ***
        public List<Table> Tables { get; set; } = new List<Table>(); // Inizializzata per evitare NullReferenceException

        // Se hai bisogno di un identificatore logico (es. "EuclideanSemantics"),
        // potresti aggiungerlo qui, magari come Enum.
        // public string LogicalType { get; set; }

        // Costruttore vuoto per la serializzazione
        public Database() { }

        public Database(int databaseId, string databaseName)
        {
            DatabaseId = databaseId;
            DatabaseName = databaseName;
        }

        /// <summary>
        /// Aggiunge una tabella al database.
        /// </summary>
        /// <param name="table">La tabella da aggiungere.</param>
        public void AddTable(Table table)
        {
            if (table != null)
            {
                Tables.Add(table);
                table.ParentDatabase = this; // Imposta il riferimento al database madre nella tabella
            }
        }
    }
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
        private static Type[] GetKnownTypes()
        {
            return new Type[]
            {
                typeof(string), // Esempio: se Field.Value può essere una stringa
                typeof(bool),   // Esempio: se Field.Value può essere un booleano
                typeof(int),    // System.Int32
                typeof(short),  // System.Int16
                typeof(long),   // System.Int64
                typeof(uint),   // System.UInt32
                typeof(ulong),  // System.UInt64
                typeof(DateTime), // DateTime
                typeof(StringBuilder), // StringBuilder
                // Aggiungi qui tutti gli altri tipi concreti che la proprietà 'Value'
                // nella tua classe Field può assumere.
                // Esempio: typeof(MyCustomClass), typeof(List<int>), ecc.

                // *** Aggiunto: Dobbiamo informare XmlSerializer che potrebbe incontrare
                // istanze di SerializableDictionary<string, object> ***
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
        private static void RestoreParentReferences(Database database)
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
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

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
                                                  // NOTA: Se TValue è object, qui potresti aver bisogno di un XmlSerializer
                                                  // che conosce i tipi noti, simile a quello usato per Database.
                                                  // Per semplicità, assumiamo che TValue sia serializzabile direttamente o un tipo noto.
                                                  // Se TValue è object, dovrai passare i knownTypes qui.
                                                  // Esempio con knownTypes (richiede un costruttore diverso per valueSerializer):
                                                  // XmlSerializer valueSerializerWithKnownTypes = new XmlSerializer(typeof(TValue), new Type[] { typeof(string), typeof(int), typeof(bool), ... });
                                                  // TValue value = (TValue)valueSerializerWithKnownTypes.Deserialize(reader);
                TValue value = (TValue)valueSerializer.Deserialize(reader); // Deserializza il valore
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
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item"); // Scrive l'elemento <item>

                writer.WriteStartElement("key"); // Scrive l'elemento <key>
                keySerializer.Serialize(writer, key); // Serializza la chiave
                writer.WriteEndElement(); // Scrive l'elemento </key>

                writer.WriteStartElement("value"); // Scrive l'elemento <value>
                                                   // NOTA: Se TValue è object, qui potresti aver bisogno di un XmlSerializer
                                                   // che conosce i tipi noti, simile a quello usato per Database.
                                                   // Per semplicità, assumiamo che TValue sia serializzabile direttamente o un tipo noto.
                                                   // Se TValue è object, dovrai passare i knownTypes qui.
                                                   // Esempio con knownTypes (richiede un costruttore diverso per valueSerializer):
                                                   // XmlSerializer valueSerializerWithKnownTypes = new XmlSerializer(typeof(TValue), new Type[] { typeof(string), typeof(int), typeof(bool), ... });
                                                   // valueSerializerWithKnownTypes.Serialize(writer, this[key]);
                valueSerializer.Serialize(writer, this[key]); // Serializza il valore
                writer.WriteEndElement(); // Scrive l'elemento </value>

                writer.WriteEndElement(); // Scrive l'elemento </item>
            }
        }
    }
    // Esempio di come potresti usare un Enum per i tipi logici, se necessario
    public enum SemanticElementType
    {
        Database,
        Table,
        Field,
        // Aggiungi altri tipi logici se necessario (es. PostulateTable, RuleTable, StringElement, OperatorElement)
    }
}
