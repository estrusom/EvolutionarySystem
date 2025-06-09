using System.Collections.Generic;

namespace MIU.Core
{
    /// <summary>
    /// Interfaccia per il Repository del sistema MIU.
    /// Definisce i contratti per le operazioni di persistenza principali
    /// (ricerche, stati, applicazioni di regole).
    /// Questa interfaccia si trova nel progetto MIU.Core.
    /// </summary>
    public interface IMIURepository
    {
        long InsertSearch(string initialString, string targetString, string searchAlgorithm);
        void UpdateSearch(long searchId, bool success, double flightTimeMs, int stepsTaken, int nodesExplored, int maxDepthReached);
        long UpsertMIUState(string miuString);
        void InsertRuleApplication(long searchId, long parentStateId, long newStateId, int appliedRuleId, int currentDepth);
        void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, int? appliedRuleId, bool isTarget, bool isSuccess, int depth);
        void UpsertRegoleMIU(List<RegolaMIU> regole);
        List<RegolaMIU> LoadRegoleMIU(); // Per caricare le regole iniziali se necessario
    }
}
