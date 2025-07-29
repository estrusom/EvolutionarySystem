// File: EvolutiveSystem.Taxonomy/MiuPatternDefinitions.cs
// Data di riferimento: 18 Luglio 2025 (Aggiornato per .NET Framework 4.8)
// Descrizione: Definizioni delle classi per la rappresentazione e le statistiche dei pattern astratti delle stringhe MIU.

using System;
using System.Collections.Generic;
using System.Linq; // Lasciato per coerenza, non strettamente necessario solo per GetHashCode

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta un pattern astratto di una stringa MIU.
    /// Usato per categorizzare le stringhe in base a proprietà strutturali o comportamentali.
    /// </summary>
    public abstract class MiuAbstractPattern
    {
        /// <summary>
        /// Ottiene o imposta un identificatore unico per il pattern.
        /// Questo ID può essere utilizzato per riferimenti univoci del pattern.
        /// È un Guid per garantire unicità globale.
        /// </summary>
        public Guid ID { get; set; }
        public string Type { get; set; } // Esempio: "StringLength", "ICountParity", "ContainsMIU"
        public string Value { get; set; } // Esempio: "100" (per lunghezza), "Even" (per parità), "True" (per contiene)
        /// <summary>
        /// Ottiene o imposta il nome descrittivo del pattern.
        /// Questo campo è stato aggiunto per migliorare la leggibilità e l'identificazione
        /// dei pattern, specialmente in contesti di logging e analisi.
        /// </summary>
        public string Nome { get; set; } // <--- NUOVA PROPRIETÀ AGGIUNTA
        public MiuAbstractPattern(string type, string value, string nome)
        {
            ID = Guid.NewGuid(); // Genera un nuovo ID unico per ogni pattern
            Type = type;
            Value = value;
            Nome = nome;
        }

        // Sovrascrivi GetHashCode e Equals per usare MiuAbstractPattern come chiave in un Dictionary
        // Questo è cruciale per la corretta gestione dei pattern unici.
        public override bool Equals(object obj)
        {
            if (obj is MiuAbstractPattern other)
            {
                return Type == other.Type && Value == other.Value;
            }
            return false;
        }

        /// <summary>
        /// Genera un hash code per l'oggetto MiuAbstractPattern, compatibile con .NET Framework 4.8.
        /// Utilizza una combinazione basata su numeri primi, seguendo lo stesso pattern già presente
        /// nelle classi MIUDerivationPath e DerivationStep.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked // Permette l'overflow senza generare eccezioni
            {
                int hash = 17; // Un numero primo iniziale
                hash = hash * 23 + (Type != null ? Type.GetHashCode() : 0);
                hash = hash * 23 + (Value != null ? Value.GetHashCode() : 0);
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{Nome} ({Type}:{Value})"; // <--- AGGIORNATO ToString per includere Nome
        }
    }

    /// <summary>
    /// Statistiche aggregate per un MiuAbstractPattern.
    /// Queste statistiche vengono raccolte per informare il processo di identificazione delle Antitesi.
    /// </summary>
    public class MiuPatternStatistics
    {
        public MiuAbstractPattern Pattern { get; set; }
        public long DiscoveryCount { get; set; } // Quante volte questo pattern è stato "scoperto"
        public long SuccessCount { get; set; }   // Quante volte questo pattern è stato in un percorso di successo
        public double TotalDepth { get; set; }   // Somma delle profondità a cui è stato scoperto
        public double AverageDepth => DiscoveryCount > 0 ? TotalDepth / DiscoveryCount : 0;
        public DateTime LastUpdated { get; set; }

        public MiuPatternStatistics(MiuAbstractPattern pattern)
        {
            Pattern = pattern;
            DiscoveryCount = 0;
            SuccessCount = 0;
            TotalDepth = 0;
            LastUpdated = DateTime.UtcNow;
        }
    }
}
