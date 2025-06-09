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
        List<RuleStatistics> LoadRuleStatistics();
        void SaveRuleStatistics(List<RuleStatistics> ruleStats);
        List<TransitionStatistics> LoadTransitionStatistics();
        void SaveTransitionStatistics(List<TransitionStatistics> transitionStats);
    }
}
