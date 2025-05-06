using System.Collections.Generic; // Necessario per List, Dictionary
using System; // Necessario per Type, object, ulong, DateTime
using System.Xml.Serialization; // Necessario per XmlSerializer, IXmlSerializable
using System.Xml; // Necessario per XmlReader, XmlWriter
using System.Text; // Necessario per StringBuilder


namespace EvolutiveSystem.SemanticData
{
    // L'interfaccia può essere utile se hai bisogno di un contratto comune
    // per i dati di un campo, ma non dovrebbe definire la gerarchia.
    // Ho rimosso ChiSonoIo dall'interfaccia perché è più legato al tipo di oggetto,
    // che si gestisce meglio con i tipi C# stessi.
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

    // Classe per rappresentare un singolo Campo (Field)
    // Implementa l'interfaccia se necessario per contratti comuni.
    // NON eredita da nulla per la gerarchia.
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

    // La classe TreeRoot potrebbe rappresentare un contenitore di più Database,
    // o potresti semplicemente usare una List<Database> come radice della tua struttura dati.
    // public class RootContainer
    // {
    //     public List<Database> Databases { get; set; } = new List<Database>();
    // }

    // Esempio di come potresti usare un Enum per i tipi logici, se necessario
    public enum SemanticElementType
    {
        Database,
        Table,
        Field,
        // Aggiungi altri tipi logici se necessario (es. PostulateTable, RuleTable, StringElement, OperatorElement)
    }

    // Potresti aggiungere una proprietà di questo tipo alle classi Database, Table, Field
    // public SemanticElementType ElementType { get; set; }
    // E poi usare uno switch su questo Enum invece che su una stringa "ChiSonoIo".
}
