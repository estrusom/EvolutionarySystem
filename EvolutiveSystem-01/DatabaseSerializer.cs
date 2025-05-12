using System;
using System.Collections.Generic;
using System.IO; // Necessario per FileStream, StringWriter, ecc.
using System.Xml.Serialization; // Necessario per XmlSerializer
using System.Text; // Necessario per StringBuilder
using System.Xml;
using EvolutiveSystem.Core; // Necessario per XmlReader, XmlWriter

namespace EvolutiveSystem.Serialization
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
}