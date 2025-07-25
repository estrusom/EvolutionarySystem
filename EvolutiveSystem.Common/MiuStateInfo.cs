// File: EvolutiveSystem.Common/MiuStateInfo.cs
// Data di riferimento: 21 giugno 2025
// Descrizione: Classe modello per rappresentare un singolo stato MIU dal database.

using System;

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta un record della tabella MIU_States.
    /// </summary>
    public class MiuStateInfo
    {
        // Questo risolve l'errore CS1729.
        public MiuStateInfo() { } // Mantenere un costruttore senza argomenti per la deserializzazione/ORM
        public MiuStateInfo(string currentString)
        {
            if (string.IsNullOrWhiteSpace(currentString))
            {
                throw new ArgumentException("La stringa dello stato non può essere nulla o vuota.", nameof(currentString));
            }
            CurrentString = currentString;
            StringLength = currentString.Length; // Inizializza la lunghezza
            // Puoi anche inizializzare altre proprietà di default o calcolate qui, es:
            // DiscoveryTime_Int = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // DiscoveryTime_Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            // Hash = YourHashingUtility.ComputeHash(currentString); // Se hai una utility per l'hash
        }
        public long StateID { get; set; }
        public string CurrentString { get; set; } // La stringa MIU standard (non compressa)
        public int StringLength { get; set; }
        public string DeflateString { get; set; } // La stringa MIU compressa
        public string Hash { get; set; }
        public long DiscoveryTime_Int { get; set; }
        public string DiscoveryTime_Text { get; set; }
        public int UsageCount { get; set; }

        // AGGIUNTA: Implementazione di Equals per confrontare gli stati in base alla loro stringa.
        // Questo è cruciale per il corretto funzionamento di HashSet<MiuStateInfo> e per i confronti.
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            // Assicurati che l'oggetto sia dello stesso tipo o derivato
            if (obj.GetType() != this.GetType()) return false;
            return CurrentString.Equals(((MiuStateInfo)obj).CurrentString, StringComparison.Ordinal);
        }

        // AGGIUNTA: Implementazione di GetHashCode per l'uso in HashSet e Dictionary.
        // Deve essere coerente con Equals.
        public override int GetHashCode()
        {
            return CurrentString?.GetHashCode() ?? 0;
        }
    }
}
