//creata 11.6.2025 1.23
using MIU.Core.Topology.Map;
using System.Collections.Generic;

namespace MIU.Core
{
    /// <summary>
    /// Interfaccia per la persistenza dello stato di apprendimento del sistema MIU.
    /// Definisce i metodi necessari per caricare e salvare le statistiche di regole e transizioni.
    /// Questa interfaccia si trova nel progetto MIU.Core.
    /// </summary>
    public interface ILearningStatePersistence
    {
        /*
        List<RuleStatistics> LoadRuleStatistics();
        void SaveRuleStatistics(List<RuleStatistics> ruleStats);
        List<TransitionStatistics> LoadTransitionStatistics();
        void SaveTransitionStatistics(List<TransitionStatistics> transitionStats);
        */
        /// <summary>
        /// Carica lo stato di apprendimento persistente (TopologicalMap).
        /// </summary>
        /// <returns>La TopologicalMap caricata, o null se non esiste uno stato salvato.</returns>
        TopologicalMap LoadLearningState();

        /// <summary>
        /// Salva lo stato di apprendimento corrente (TopologicalMap).
        /// </summary>
        /// <param name="map">La mappa topologica da salvare.</param>
        void SaveLearningState(TopologicalMap map);
    }
}
