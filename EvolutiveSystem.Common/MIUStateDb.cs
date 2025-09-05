namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Modello dati per la tabella MIU_States.
    /// Utilizzato esclusivamente per la persistenza e il recupero dati.
    /// </summary>
    public class MIUStateDb
    {
        public long StateID { get; set; }
        public string CurrentString { get; set; }
        public int StringLength { get; set; }
        public string DeflateString { get; set; }
        public string Hash { get; set; }
        public long DiscoveryTime_Int { get; set; }
        public string DiscoveryTime_Text { get; set; }
        public int SeedingType { get; set; }
    }
}