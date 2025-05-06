using System;
using System.Collections.Generic;
using System.IO; // Necessario per FileStream, StringWriter, ecc.
using System.Xml.Serialization; // Necessario per XmlSerializer
using EvolutiveSystem.SemanticData; // Assicurati che questo namespace sia corretto

namespace EvolutiveSystem.Serialization
{
    public static class DatabaseSerializer
    {
        /// <summary>
        /// Serializza un oggetto Database in un file XML.
        /// </summary>
        /// <param name="database">L'oggetto Database da serializzare.</param>
        /// <param name="filePath">Il percorso del file in cui salvare l'XML.</param>
        public static void SerializeToXmlFile(Database database, string filePath)
        {
            // Crea un'istanza di XmlSerializer per il tipo Database.
            // Se le tue classi Field, Table, Database contengono proprietà di tipo object
            // o altri tipi complessi che non sono noti a compile-time, potresti aver bisogno di
            // specificare tipi aggiuntivi nel costruttore di XmlSerializer
            // (es. new XmlSerializer(typeof(Database), new Type[] { typeof(MyComplexType) }))
            // o usare attributi [XmlInclude].
            // Per la struttura attuale con object Value, potrebbe essere necessario specificare i tipi concreti che Value può assumere.
            // In alternativa, DataContractSerializer potrebbe essere più flessibile con i tipi noti.
            XmlSerializer serializer = new XmlSerializer(typeof(Database));

            // Usa un FileStream per scrivere l'XML su un file.
            // Usa using per assicurarti che lo stream venga chiuso correttamente.
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                // Opzionale: usa un XmlWriter per un maggiore controllo sulla formattazione,
                // ma XmlSerializer può scrivere direttamente su uno Stream.
                // XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                // using (XmlWriter writer = XmlWriter.Create(fs, settings))
                // {
                //     serializer.Serialize(writer, database);
                // }

                // Serializza l'oggetto Database nello stream.
                serializer.Serialize(fs, database);
            }
        }

        /// <summary>
        /// Serializza un oggetto Database in una stringa XML.
        /// </summary>
        /// <param name="database">L'oggetto Database da serializzare.</param>
        /// <returns>Una stringa contenente la rappresentazione XML del Database.</returns>
        public static string SerializeToXmlString(Database database)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Database));
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
            XmlSerializer serializer = new XmlSerializer(typeof(Database));
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                // Deserializza l'oggetto dallo stream.
                return (Database)serializer.Deserialize(fs);
            }
        }

        /// <summary>
        /// Deserializza un oggetto Database da una stringa XML.
        /// </summary>
        /// <param name="xmlString">La stringa contenente l'XML.</param>
        /// <returns>L'oggetto Database deserializzato.</returns>
        public static Database DeserializeFromXmlString(string xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Database));
            using (StringReader reader = new StringReader(xmlString))
            {
                return (Database)serializer.Deserialize(reader);
            }
        }

        // Esempio di utilizzo (potrebbe essere chiamato dalla tua UI o dal servizio)
        /*
        public static void Main(string[] args) // Solo per test
        {
            // Crea un database di esempio (usando il metodo dalla UI o creandone uno qui)
            Database myMuDb = new Database(1, "MU Game Semantics Example");
            Table rules = new Table(1, "Rules", myMuDb);
            rules.AddField(new Field(1, "Rule1", "string", false, false, 0, rules, "Regola 1 test"));
            myMuDb.AddTable(rules);


            string fileName = "MuGameSemantics.xml";

            // Serializza in file
            try
            {
                SerializeToXmlFile(myMuDb, fileName);
                Console.WriteLine($"Database serializzato con successo in {fileName}");

                // Deserializza da file
                Database loadedDb = DeserializeFromXmlFile(fileName);
                Console.WriteLine($"Database deserializzato con successo da {fileName}. Nome: {loadedDb.DatabaseName}");
                Console.WriteLine($"Numero tabelle: {loadedDb.Tables.Count}");
                 if(loadedDb.Tables.Any())
                 {
                     Console.WriteLine($"Prima tabella: {loadedDb.Tables[0].TableName}, Campi: {loadedDb.Tables[0].Fields.Count}");
                 }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante serializzazione/deserializzazione: {ex.Message}");
            }

             // Serializza in stringa
             try
             {
                 string xmlContent = SerializeToXmlString(myMuDb);
                 Console.WriteLine("\n--- XML String Output ---");
                 Console.WriteLine(xmlContent);
                 Console.WriteLine("-----------------------");

                 // Deserializza da stringa
                 Database loadedDbFromString = DeserializeFromXmlString(xmlContent);
                 Console.WriteLine($"Database deserializzato da stringa. Nome: {loadedDbFromString.DatabaseName}");

             }
              catch (Exception ex)
            {
                Console.WriteLine($"Errore durante serializzazione/deserializzazione da stringa: {ex.Message}");
            }
        }
        */
    }
}
