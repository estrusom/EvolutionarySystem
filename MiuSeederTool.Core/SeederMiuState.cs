// File: MiuSeederTool.Core/SeederMiuState.cs
// Data di riferimento: 03 luglio 2025
// Descrizione: Definizione della classe che rappresenta uno stato MIU nel database.

namespace MiuSeederTool.Core
{
    /// <summary>
    /// Definisce il tipo di seeding per una stringa MIU, indicando come è stata generata o il suo scopo.
    /// </summary>
    public enum SeedingType
    {
        /// <summary>
        /// Stringa generata casualmente, senza garanzia di derivabilità o soluzione.
        /// </summary>
        Random = 0,
        /// <summary>
        /// Stringa generata con un percorso di derivazione noto (derivabile),
        /// ma non necessariamente mirata a una soluzione specifica.
        /// Corrisponde alle tue "150 stringhe derivabili ma probabilmente senza soluzione".
        /// </summary>
        Derivable = 1,
        /// <summary>
        /// Stringa generata come parte di un percorso che porta a una soluzione nota.
        /// Corrisponde alle tue "16 stringhe con soluzione certa".
        /// </summary>
        SolutionPath = 2
    }
    /// <summary>
    /// Represents a MIU state in the database.
    /// </summary>
    public class SeederMiuState
    {
        public long StateID { get; set; }
        public string CurrentString { get; set; }
        public int StringLength { get; set; }
        public string DeflateString { get; set; } // Compressed version to save space
        public string Hash { get; set; } // SHA256 hash of the string for uniqueness
        public long DiscoveryTime_Int { get; set; } // Unix timestamp
        public string DiscoveryTime_Text { get; set; } // Readable date/time
        public int UsageCount { get; set; } // Usage counter
        /// <summary>
        /// Indica il tipo di seeding della stringa (es. Random, Derivable, SolutionPath).
        /// </summary>
        public SeedingType SeedingType { get; set; } = SeedingType.Random; // Default a Random
    }
}
