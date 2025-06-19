// File: MIU.Core\IMIUDataManager.cs
// Aggiungi un metodo per caricare i parametri di configurazione.

using System.Collections.Generic; // Assicurati che questo using sia presente

namespace MIU.Core
{
    public interface IMIUDataManager
    {
        // ... (altri metodi esistenti) ...

        // Metodo per caricare le regole MIU
        List<RegolaMIU> LoadRegoleMIU();

        // Metodi per RuleStatistics
        Dictionary<long, RuleStatistics> LoadRuleStatistics();
        void SaveRuleStatistics(List<RuleStatistics> stats);
        
        // Metodi per TransitionStatistics
        List<TransitionStatistics> LoadTransitionStatistics(); // Assicurati sia presente
        void SaveTransitionStatistics(List<TransitionStatistics> stats); // Assicurati sia presente

        // Metodi per MIU States e Rule Applications
        long UpsertMIUState(string miuString);
        void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long ruleId, int depth);

        // Metodi per Search e Solution Path
        long InsertSearch(string initialString, string targetString, string algorithm);
        void UpdateSearch(long searchId, bool success, double elapsedMilliseconds, int stepsTaken, long nodesExplored, int maxDepthReached);
        void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, long? appliedRuleId, bool isTarget, bool success, int depth);

        // NEW: Metodo per caricare i parametri di configurazione
        Dictionary<string, string> LoadMIUParameterConfigurator();
    }
}
