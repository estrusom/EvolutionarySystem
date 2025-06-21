// Data di riferimento: 4 giugno 2025 (questo file non esisteva prima)
// creato 15/6/2025 02:13
// sostituito 15/6/2025 02:23
// sostituito 19/6/2025 11.51
// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\IMIUDataManager.cs
// Data di riferimento: 4 giugno 2025 (questo file non esisteva prima)
// Questa interfaccia definisce il contratto per la gestione dei dati MIU.
// Questa interfaccia definisce i metodi che devono essere implementati da qualsiasi gestore
// di persistenza per il sistema MIU, garantendo un'astrazione dal meccanismo di storage sottostante.
// Aggiornato 19.06.2025: Allineamento definitivo dei tipi per AppliedRuleID e ParentStateID a long/long?.
// NUOVA MODIFICA 19.6.25 19.21: Aggiunta delle firme dei metodi per la persistenza delle statistiche di apprendimento.
// Data di riferimento: 20 giugno 2025 (Correzione definitiva tipi Dictionary a long)
// Questa interfaccia definisce il contratto per la gestione dei dati MIU.
// NUOVA MODIFICA 21.6.25: Aggiunti nuovi parametri per le caratteristiche delle stringhe ai metodi InsertSearch e UpdateSearch esistenti.
// AGGIORNATO 20.06.2025: Aggiunta firma per il metodo GetTransitionProbabilities per l'aggregazione delle statistiche.
// AGGIORNATO 20.06.2025: Reintegrate le firme dei metodi Load/Save delle statistiche di apprendimento,
// necessarie per la delega da LearningStatisticsManager.
// AGGIORNATO 21.06.2025: Aggiunta firma per SetJournalMode per incapsulare PRAGMA in MIUDatabaseManager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvolutiveSystem.Common; // Per RegolaMIU, RuleStatistics, TransitionStatistics

namespace MIU.Core
{
    /// <summary>
    /// Interfaccia che definisce le operazioni di gestione dei dati per il sistema MIU.
    /// Questo separa la logica di business MIU (nel Repository) dai dettagli di persistenza.
    /// </summary>
    public interface IMIUDataManager
    {
        // Operazioni per MIU_Searches
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

        // Operazioni per MIU_States
        long UpsertMIUState(string miuString);
        System.Collections.Generic.List<EvolutiveSystem.Common.MiuStateInfo> LoadMIUStates();
        bool SearchExists(string initialString, string targetString);


        // Operazioni per MIU_RuleApplications
        void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long appliedRuleID, int currentDepth);

        // Operazioni per MIU_Paths
        void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, long? appliedRuleID, bool isTarget, bool isSuccess, int depth);

        // Operazioni per RegoleMIU
        System.Collections.Generic.List<EvolutiveSystem.Common.RegolaMIU> LoadRegoleMIU();
        void UpsertRegoleMIU(System.Collections.Generic.List<EvolutiveSystem.Common.RegolaMIU> regole);

        // Operazioni per MIUParameterConfigurator
        Dictionary<string, string> LoadMIUParameterConfigurator();
        void SaveMIUParameterConfigurator(Dictionary<string, string> config);

        // Metodi per le statistiche di apprendimento:
        System.Collections.Generic.Dictionary<long, EvolutiveSystem.Common.RuleStatistics> LoadRuleStatistics();
        void SaveRuleStatistics(System.Collections.Generic.Dictionary<long, EvolutiveSystem.Common.RuleStatistics> ruleStats);
        System.Collections.Generic.Dictionary<System.Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics> LoadTransitionStatistics();
        void SaveTransitionStatistics(System.Collections.Generic.Dictionary<System.Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics> transitionStats);
        System.Collections.Generic.Dictionary<Tuple<string, long>, EvolutiveSystem.Common.TransitionStatistics> GetTransitionProbabilities();

        // --- NUOVI METODI PER LA GESTIONE DEL CURSORE DI ESPLORAZIONE ---
        /// <summary>
        /// Carica lo stato del cursore di esplorazione dal database.
        /// </summary>
        /// <returns>Un oggetto MIUExplorerCursor se i parametri esistono, altrimenti un nuovo oggetto con valori di default.</returns>
        EvolutiveSystem.Common.MIUExplorerCursor LoadExplorerCursor();

        /// <summary>
        /// Salva lo stato corrente del cursore di esplorazione nel database.
        /// </summary>
        /// <param name="cursor">L'oggetto MIUExplorerCursor da salvare.</param>
        void SaveExplorerCursor(EvolutiveSystem.Common.MIUExplorerCursor cursor);
    }
}
