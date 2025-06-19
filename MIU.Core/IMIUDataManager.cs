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
        // Operazioni per MIU_Searches - MODIFICATI PER INCLUDERE NUOVI PARAMETRI
        long InsertSearch(
            string initialString,
            string targetString,
            string searchAlgorithm,
            int initialStringLength,    // NUOVO PARAMETRO
            int targetStringLength,     // NUOVO PARAMETRO
            int initialIcount,          // NUOVO PARAMETRO
            int initialUcount,          // NUOVO PARAMETRO
            int targetIcount,           // NUOVO PARAMETRO
            int targetUcount            // NUOVO PARAMETRO
        );
        void UpdateSearch(
            long searchId,
            bool success,
            double flightTimeMs,
            int stepsTaken,
            int nodesExplored,
            int maxDepthReached
        // N.B.: Non aggiungiamo qui i parametri di lunghezza/conteggio perché UpdateSearch li aggiorna solo se la ricerca ha successo,
        // e quei valori sono già stati forniti all'inserimento e non cambiano.
        // Se in futuro ci fosse necessità di aggiornare queste statistiche, andrebbe valutato un metodo separato.
        );

        // Operazioni per MIU_States (INVARIATE)
        long UpsertMIUState(string miuString);

        // Operazioni per MIU_RuleApplications (INVARIATE)
        void InsertRuleApplication(long searchId, long parentStateId, long newStateId, long appliedRuleID, int currentDepth);

        // Operazioni per MIU_Paths (INVARIATE)
        void InsertSolutionPathStep(long searchId, int stepNumber, long stateId, long? parentStateId, long? appliedRuleID, bool isTarget, bool isSuccess, int depth);

        // Operazioni per RegoleMIU (INVARIATE)
        List<RegolaMIU> LoadRegoleMIU();
        void UpsertRegoleMIU(List<RegolaMIU> regole);

        // Operazioni per MIUParameterConfigurator (INVARIATE)
        Dictionary<string, string> LoadMIUParameterConfigurator();
        void SaveMIUParameterConfigurator(Dictionary<string, string> config);

        // Metodi per le statistiche di apprendimento (Chiave Dictionary ora 'long' e Tuple.Item2 a 'long') (INVARIATE)
        Dictionary<long, RuleStatistics> LoadRuleStatistics();
        void SaveRuleStatistics(Dictionary<long, RuleStatistics> ruleStats);
        Dictionary<Tuple<string, long>, TransitionStatistics> LoadTransitionStatistics();
        void SaveTransitionStatistics(Dictionary<Tuple<string, long>, TransitionStatistics> transitionStats);
    }
}
