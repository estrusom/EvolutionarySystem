// File: MiuSeederTool.Core/SeederMiuRule.cs

namespace MiuSeederTool.Core
{
    /// <summary>
    /// Represents a single MIU derivation rule.
    /// </summary>
    public class SeederMiuRule
    {
        public int RuleID { get; set; }
        public string RuleName { get; set; } // Property name corrected to RuleName
        public string Pattern { get; set; }
        public string Replacement { get; set; }
    }
}
