using System.Collections.Generic; // Necessario per List
using System; // Necessario per Type, object, ulong

namespace EvolutiveSystem.SemanticData
{
    // Interfaccia per definire le proprietà comuni dei dati di un campo.
    // Utile se diverse implementazioni di campo avranno comportamenti simili.
    public interface IFieldData
    {
        int Id { get; set; }
        string FieldName { get; set; }
        bool Key { get; set; }
        string TableName { get; set; } // Riferimento al nome della tabella per comodità
        string DataType { get; set; }
        object Value { get; set; } // Attenzione qui: object richiede boxing/unboxing. Considera tipi specifici se possibile.
        bool EncryptedField { get; set; }
        ulong Registry { get; set; }
        // La proprietà ChiSonoIo non è qui, l'identificazione del tipo logico
        // è gestita meglio tramite un Enum o il tipo C# stesso.
    }

    // Enumerazione per identificare il tipo logico di un elemento semantico.
    // Può essere usata per distinguere tra diversi tipi di Database, Tabelle o Campi
    // (es. Tabella dei Postulati vs Tabella delle Regole).
    public enum SemanticElementType
    {
        Unknown,
        Database,
        Table,
        Field,
        PostulateTable, // Esempio di tipo logico specifico
        RuleTable,      // Esempio di tipo logico specifico
        StringElement,  // Esempio per il gioco MU
        SymbolElement   // Esempio per il gioco MU ('M', 'I', 'U')
        // Aggiungi altri tipi logici se necessario
    }

    // Classe per rappresentare un singolo Campo (Field) all'interno di una Tabella.
    // Implementa l'interfaccia IFieldData per il contratto dei dati.
    // NON eredita da Table o Database.
    public class Field : IFieldData
    {
        // Proprietà specifiche di un Campo, come definito nell'interfaccia.
        public int Id { get; set; }
        public string FieldName { get; set; }
        public bool Key { get; set; }
        public string TableName { get; set; } // Riferimento al nome della tabella
        public string DataType { get; set; }
        public object Value { get; set; }
        public bool EncryptedField { get; set; }
        public ulong Registry { get; set; }

        // *** Composizione/Riferimento per la Provenienza: Un Campo appartiene a una Tabella ***
        // Questo riferimento permette di risalire alla tabella madre direttamente.
        // È cruciale per la navigazione della struttura dati.
        public Table ParentTable { get; set; }

        // Proprietà per identificare il tipo logico di questo elemento semantico (Campo).
        // Utile per distinguere campi con ruoli diversi (es. un campo che contiene una stringa MU vs un campo che contiene un valore numerico).
        public SemanticElementType ElementType { get; set; } = SemanticElementType.Field; // Default a Field

        // Costruttore di esempio
        public Field(int id, string fieldName, string dataType, bool isKey, bool isEncrypted, ulong registryValue, Table parentTable, object value = null)
        {
            Id = id;
            FieldName = fieldName;
            DataType = dataType;
            Key = isKey;
            EncryptedField = isEncrypted;
            Registry = registryValue;
            ParentTable = parentTable; // Imposta il riferimento al genitore
            TableName = parentTable?.TableName; // Imposta il nome della tabella madre (utile per riferimento veloce)
            Value = value;
            ElementType = SemanticElementType.Field; // Assicurati che sia il tipo corretto
        }

        // Costruttore vuoto per la serializzazione
        public Field() { }
    }

    // Classe per rappresentare una singola Tabella (Table) all'interno di un Database.
    // NON eredita da Field.
    public class Table
    {
        // Proprietà specifiche di una Tabella.
        public int TableId { get; set; }
        public string TableName { get; set; }

        // *** Composizione: Una Tabella CONTIENE una lista di Campi ***
        // Questa lista rappresenta la struttura dei dati all'interno della tabella.
        public List<Field> Fields { get; set; } = new List<Field>(); // Inizializzata per evitare NullReferenceException

        // *** Composizione/Riferimento per la Provenienza: Una Tabella appartiene a un Database ***
        // Questo riferimento permette di risalire al database madre direttamente.
        public Database ParentDatabase { get; set; } // Riferimento al genitore

        // Proprietà per identificare il tipo logico di questo elemento semantico (Tabella).
        // Utile per distinguere tabelle con ruoli diversi (es. tabella dei postulati vs tabella delle regole).
        public SemanticElementType ElementType { get; set; } = SemanticElementType.Table; // Default a Table

        // Costruttore di esempio
        public Table(int tableId, string tableName, Database parentDatabase)
        {
            TableId = tableId;
            TableName = tableName;
            ParentDatabase = parentDatabase; // Imposta il riferimento al genitore
            ElementType = SemanticElementType.Table; // Assicurati che sia il tipo corretto
        }

        // Costruttore vuoto per la serializzazione
        public Table() { }

        /// <summary>
        /// Aggiunge un campo a questa tabella e imposta il suo riferimento ParentTable.
        /// </summary>
        public void AddField(Field field)
        {
            if (field != null)
            {
                field.ParentTable = this; // Imposta il riferimento corretto al genitore nel campo
                Fields.Add(field);
            }
        }
    }

    // Classe per rappresentare un singolo Database (Database) che contiene Tabelle.
    // NON eredita da Table o Field. Questa è la radice della gerarchia dati semantica.
    public class Database
    {
        // Proprietà specifiche di un Database.
        public int DatabaseId { get; set; }
        public string DatabaseName { get; set; }

        // *** Composizione: Un Database CONTIENE una lista di Tabelle ***
        // Questa lista rappresenta le diverse tabelle all'interno del database (semantica).
        public List<Table> Tables { get; set; } = new List<Table>(); // Inizializzata per evitare NullReferenceException

        // Proprietà per identificare il tipo logico di questo elemento semantico (Database).
        // Utile per distinguere diversi database (es. semantica Euclidea, semantica MU).
        public SemanticElementType ElementType { get; set; } = SemanticElementType.Database; // Default a Database

        // Costruttore di esempio
        public Database(int databaseId, string databaseName)
        {
            DatabaseId = databaseId;
            DatabaseName = databaseName;
            ElementType = SemanticElementType.Database; // Assicurati che sia il tipo corretto
        }

        // Costruttore vuoto per la serializzazione
        public Database() { }

        /// <summary>
        /// Aggiunge una tabella a questo database e imposta il suo riferimento ParentDatabase.
        /// </summary>
        public void AddTable(Table table)
        {
            if (table != null)
            {
                table.ParentDatabase = this; // Imposta il riferimento corretto al genitore nella tabella
                Tables.Add(table);
            }
        }
    }

    // La classe TreeRoot non è strettamente necessaria con questa struttura.
    // Puoi gestire una List<Database> come la collezione di tutte le semantiche disponibili.
    // public class RootContainer
    // {
    //     public List<Database> Databases { get; set; } = new List<Database>();
    // }
}
