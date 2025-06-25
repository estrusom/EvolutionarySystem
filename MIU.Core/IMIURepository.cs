// File: MIU.Core/IMIURepository.cs
// Data di riferimento: 26 giugno 2025
// Descrizione: Interfaccia per le operazioni di repository di alto livello sui dati MIU.
// Questa interfaccia include tutti i metodi attualmente utilizzati dall'implementazione concreta
// di MIURepository nel sistema, per garantire la compilazione e la funzionalità immediata.

using System.Collections.Generic;
using System.Threading.Tasks; // Necessario per metodi asincroni
using EvolutiveSystem.Common; // Per MiuStateInfo, RuleStatistics, TransitionStatistics, RegolaMIU

namespace MIU.Core
{
    /// <summary>
    /// Interfaccia che definisce le operazioni di alto livello per l'accesso e la gestione dei dati MIU.
    /// Questa interfaccia riflette l'uso corrente dell'implementazione concreta di MIURepository
    /// all'interno del sistema (es. in Program.cs e MIUAutoExplorer), includendo sia la configurazione
    /// che altre entità di dati.
    /// </summary>
    public interface IMIURepository
    {
        // Metodi per i parametri di configurazione
        Dictionary<string, string> LoadMIUParameterConfigurator();
        void SaveMIUParameterConfigurator(Dictionary<string, string> config);

        // Metodi per le ricerche (tabella MIU_Searches)
        long InsertSearch(
            string initialString,
            string targetString,
            string searchAlgorithm,
            int initialStringLength,
            int targetStringLength,
            int initialIcount,
            int initialUcount,
            int targetIcount,
            int targetUcount
        );
        void UpdateSearch(
            long searchId,
            bool success,
            double flightTimeMs,
            int stepsTaken,
            int nodesExplored,
            int maxDepthReached
        );

        // Metodi per gli stati MIU (tabella MIU_States)
        long UpsertMIUState(string miuString);
        // LoadMIUStatesAsync non è qui se MIURepository non lo espone direttamente
        // e se viene chiamato direttamente su IMIUDataManager come nel MIUAutoExplorer.

        // Metodi per le applicazioni di regole (tabella MIU_RuleApplications)
        void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long appliedRuleID, int currentDepth);

        // Metodi per i percorsi di soluzione (tabella MIU_Paths)
        void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, long? appliedRuleID, bool isTarget, bool isSuccess, int depth);

        // Metodi per le regole MIU (tabella RegoleMIU)
        List<RegolaMIU> LoadRegoleMIU();
        void UpsertRegoleMIU(List<RegolaMIU> regole);

        // Metodi per le statistiche di apprendimento (tabelle Learning_RuleStatistics)
        Dictionary<long, RuleStatistics> LoadRuleStatistics();
        void SaveRuleStatistics(Dictionary<long, RuleStatistics> ruleStats);
    }
}
