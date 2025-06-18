// Data di riferimento: 4 giugno 2025 (questo file non esisteva prima)
// creato 15/6/2025 02:13
// sostituito 15/6/2025 02:23
// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\IMIUDataManager.cs
// Data di riferimento: 4 giugno 2025 (questo file non esisteva prima)
// Questa interfaccia definisce il contratto per la gestione dei dati MIU.
// Questa interfaccia definisce i metodi che devono essere implementati da qualsiasi gestore
// di persistenza per il sistema MIU, garantendo un'astrazione dal meccanismo di storage sottostante.
// Aggiornato 19.06.2025: Allineamento definitivo dei tipi per AppliedRuleID e ParentStateID a long/long?.
// NUOVA MODIFICA 19.6.25 19.21: Aggiunta delle firme dei metodi per la persistenza delle statistiche di apprendimento.
// Data di riferimento: 20 giugno 2025 (Correzione definitiva tipi Dictionary a long)
// Questa interfaccia definisce il contratto per la gestione dei dati MIU.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{
    /// <summary>
    /// Interfaccia che definisce le operazioni di gestione dei dati per il sistema MIU.
    /// Questo separa la logica di business MIU (nel Repository) dai dettagli di persistenza.
    /// </summary>
    public interface IMIUDataManager
    {
        // Operazioni per MIU_Searches
        long InsertSearch(string initialString, string targetString, string searchAlgorithm);
        void UpdateSearch(long searchId, bool success, double flightTimeMs, int stepsTaken, int nodesExplored, int maxDepthReached);

        // Operazioni per MIU_States
        long UpsertMIUState(string miuString);

        // Operazioni per MIU_RuleApplications
        void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long appliedRuleID, int currentDepth);

        // Operazioni per MIU_Paths
        void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, long? appliedRuleID, bool isTarget, bool isSuccess, int depth);

        // Operazioni per RegoleMIU
        List<RegolaMIU> LoadRegoleMIU();
        void UpsertRegoleMIU(List<RegolaMIU> regole);

        // Operazioni per MIUParameterConfigurator
        Dictionary<string, string> LoadMIUParameterConfigurator();
        void SaveMIUParameterConfigurator(Dictionary<string, string> config);

        // Metodi per le statistiche di apprendimento (Chiave Dictionary ora 'long' e Tuple.Item2 a 'long')
        Dictionary<long, RuleStatistics> LoadRuleStatistics();
        void SaveRuleStatistics(Dictionary<long, RuleStatistics> ruleStats);
        Dictionary<Tuple<string, long>, TransitionStatistics> LoadTransitionStatistics();
        void SaveTransitionStatistics(Dictionary<Tuple<string, long>, TransitionStatistics> transitionStats);
    }
}
