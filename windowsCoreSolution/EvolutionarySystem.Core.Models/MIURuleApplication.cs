// File: MIURuleApplication.cs
// Questa classe modella la struttura della tabella MIU_RuleApplications.
// I nomi delle proprietà corrispondono ai nomi delle colonne del database.

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvolutionarySystem.Core.Models
{
    [Table("MIU_RuleApplications")] // Aggiungere questo
    public class MIURuleApplication
    {
        public int ApplicationID { get; set; }
        public int SearchID { get; set; }
        public int? ParentStateID { get; set; }
        public int NewStateID { get; set; }
        public int AppliedRuleID { get; set; }
        public int CurrentDepth { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
