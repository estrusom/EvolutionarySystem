// File: ExplorationAnomaly.cs
// Questa classe modella la struttura della tabella ExplorationAnomalies.
// I nomi delle proprietà corrispondono ai nomi delle colonne del database.

using System;

namespace EvolutionarySystem.Core.Models
{
    public class ExplorationAnomaly
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public int? RuleId { get; set; } // Uso del tipo nullable int? per gestire il valore NULL
        public int? ContextPatternHash { get; set; } // Uso del tipo nullable int? per gestire il valore NULL
        public string ContextPatternSample { get; set; }
        public int Count { get; set; }
        public double AverageValue { get; set; } // Uso di double per il tipo REAL
        public double AverageDepth { get; set; } // Uso di double per il tipo REAL
        public DateTime LastDetected { get; set; }
        public string Description { get; set; }
        public bool IsNewCategory { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
