// 25.09.04
using System;
using System.Collections.Generic;

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Definisce il contratto per i manager che gestiscono le statistiche di apprendimento.
    /// </summary>
    public interface ILearningStatisticsManager
    {
        /// <summary>
        /// Ottiene le statistiche di transizione aggregate dal data manager.
        /// </summary>
        Dictionary<Tuple<string, long>, TransitionStatistics> GetTransitionProbabilities();
    }
}

