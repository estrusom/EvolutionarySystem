// Data di riferimento: 4 giugno 2025 (questo file non esisteva prima)
// creato 16.6.2025 11.00
// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\IMIUDataManager.cs
// Interfaccia per la gestione dei dati del sistema MIU.
// Questa interfaccia definisce i metodi che devono essere implementati da qualsiasi gestore
// di persistenza per il sistema MIU, garantendo un'astrazione dal meccanismo di storage sottostante.
// Aggiornato 19.06.2025: Allineamento definitivo dei tipi per AppliedRuleID e ParentStateID a long/long?.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{
    public interface IMIUDataManager
    {
        // Metodi per MIU_Searches
        long InsertSearch(string initialString, string targetString, string searchAlgorithm);
        void UpdateSearch(long searchId, bool success, double flightTimeMs, int stepsTaken, int nodesExplored, int maxDepthReached);

        // Metodi per MIU_States
        long UpsertMIUState(string miuString);

        // Metodi per MIU_RuleApplications
        // Il parametro appliedRuleID è ora di tipo long
        void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long appliedRuleID, int currentDepth);

        // Metodi per MIU_Paths
        // I parametri parentStateId e appliedRuleID sono ora di tipo long? (nullable long)
        void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, long? appliedRuleID, bool isTarget, bool isSuccess, int depth);

        // Metodi per RegoleMIU
        List<RegolaMIU> LoadRegoleMIU();
        void UpsertRegoleMIU(List<RegolaMIU> regole);

        // Metodi per MIUParameterConfigurator
        Dictionary<string, string> LoadMIUParameterConfigurator();
        void SaveMIUParameterConfigurator(Dictionary<string, string> config);

        // Metodi per Learning_RuleStatistics
        Dictionary<int, RuleStatistics> LoadRuleStatistics();
        void SaveRuleStatistics(Dictionary<int, RuleStatistics> ruleStats);

        // Metodi per Learning_TransitionStatistics
        Dictionary<Tuple<string, int>, TransitionStatistics> LoadTransitionStatistics();
        void SaveTransitionStatistics(Dictionary<Tuple<string, int>, TransitionStatistics> transitionStats);
    }
}
