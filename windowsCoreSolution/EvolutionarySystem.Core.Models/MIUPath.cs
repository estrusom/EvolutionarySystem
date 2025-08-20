// File: MIUPath.cs
// Questa classe modella la struttura della tabella MIU_Paths.
// I nomi delle proprietà corrispondono ai nomi delle colonne del database.

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvolutionarySystem.Core.Models
{
    [Table("MIU_Paths")] // Aggiungere questo
    public class MIUPath
    {
        public int PathStepID { get; set; }
        public int SearchID { get; set; }
        public int StepNumber { get; set; }
        public int StateID { get; set; }
        public int? ParentStateID { get; set; } // Uso del tipo nullable int? per gestire il valore NULL
        public int? AppliedRuleID { get; set; } // Uso del tipo nullable int? per gestire il valore NULL
        public bool IsTarget { get; set; }
        public bool IsSuccess { get; set; }
        public int Depth { get; set; }
    }
}
