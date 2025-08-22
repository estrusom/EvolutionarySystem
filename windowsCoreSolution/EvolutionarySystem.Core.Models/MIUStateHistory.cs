using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvolutionarySystem.Core.Models
{
    [Table("MIU_States_History")]
    public class MIUStateHistory
    {
        [Key] // AGGIUNGI QUESTO ATTRIBUTO
        public int Id { get; set; } // O il nome della tua chiave primaria
        public string? MIUString { get; set; } // RESO NULLABLE
        public string? Hash { get; set; } // RESO NULLABLE        public int FirstDiscoveredByRuleId { get; set; }
        public int Depth { get; set; }
        public int TimesFound { get; set; }
        public string Timestamp { get; set; }
        public int UsageCount { get; set; } // Uso del tipo nullable int?
        public string DetectedPatternHashes_SCSV { get; set; }
    }
}