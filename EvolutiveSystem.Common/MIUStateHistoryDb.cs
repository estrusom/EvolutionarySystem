namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Modello dati per la tabella MIU_States_History.
    /// Contiene i dati storici e le metriche di utilizzo/tassonomia.
    /// </summary>
    public class MIUStateHistoryDb
    {
        public long Id { get; set; }
        public string MIUString { get; set; }
        public string Hash { get; set; }
        public long FirstDiscoveredByRuleId { get; set; }
        public int Depth { get; set; }
        public int TimesFound { get; set; }
        public string Timestamp { get; set; }
        public int? UsageCount { get; set; }
        public string DetectedPatternHashes_SCSV { get; set; }
    }
}
