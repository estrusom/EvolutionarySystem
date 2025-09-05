// File: EvolutiveSystem.Common/SerializableDictionary.cs
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Classe helper per serializzare Dictionary<TKey, TValue> con XmlSerializer.
    /// Eredita da Dictionary e implementa IXmlSerializable.
    /// </summary>
    /// <typeparam name="TKey">Tipo della chiave del dizionario.</typeparam>
    /// <typeparam name="TValue">Tipo del valore del dizionario.</typeparam>
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        // Variabile per conservare il provider di tipi noti iniettato.
        private IKnownTypeProvider _knownTypeProvider;

        // Costruttore vuoto necessario per XmlSerializer.
        public SerializableDictionary() { }

        // Costruttore per iniettare la dipendenza IKnownTypeProvider.
        // Questo costruttore non verrà chiamato da XmlSerializer, ma da noi.
        public SerializableDictionary(IKnownTypeProvider knownTypeProvider)
        {
            _knownTypeProvider = knownTypeProvider;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            // Ottiene i tipi noti dal provider iniettato.
            Type[] knownTypes = (_knownTypeProvider != null) ? _knownTypeProvider.GetKnownTypes() : new Type[0];

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), knownTypes);

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty) return;

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

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            // Ottiene i tipi noti dal provider iniettato.
            Type[] knownTypes = (_knownTypeProvider != null) ? _knownTypeProvider.GetKnownTypes() : new Type[0];

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), knownTypes);

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
}