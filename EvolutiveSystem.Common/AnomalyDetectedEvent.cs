// File: EvolutiveSystem.Common/AnomalyDetectedEvent.cs
using System;
using EvolutiveSystem.Common; // Per AnomalyType (assicurati che AnomalyType sia definito in EvolutiveSystem.Common)

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Evento scatenato quando l'AnomalyDetectionManager rileva un'anomalia.
    /// Contiene i dettagli dell'anomalia rilevata.
    /// </summary>
    public class AnomalyDetectedEvent
    {
        public long AnomalyId { get; } // ID dell'anomalia se persistita nel database
        public AnomalyType Type { get; } // Tipo di anomalia (es. CicloInfinito, IneffectiveRuleUsage)
        public long? RuleId { get; } // ID della regola coinvolta, se applicabile
        public int? ContextPatternHash { get; } // Hash del pattern di contesto, se applicabile
        public string Description { get; } // Breve descrizione dell'anomalia
        public DateTime Timestamp { get; } // Momento in cui l'anomalia è stata rilevata

        /// <summary>
        /// Costruttore per AnomalyDetectedEvent.
        /// </summary>
        /// <param name="anomalyId">L'ID univoco dell'anomalia (se già persistita).</param>
        /// <param name="type">Il tipo specifico di anomalia.</param>
        /// <param name="ruleId">L'ID della regola MIU associata all'anomalia (nullable).</param>
        /// <param name="contextPatternHash">L'hash del pattern di contesto associato (nullable).</param>
        /// <param name="description">Una descrizione testuale dell'anomalia.</param>
        public AnomalyDetectedEvent(long anomalyId, AnomalyType type, long? ruleId, int? contextPatternHash, string description)
        {
            AnomalyId = anomalyId;
            Type = type;
            RuleId = ruleId;
            ContextPatternHash = contextPatternHash;
            Description = description;
            Timestamp = DateTime.UtcNow; // Registra il momento dell'evento
        }
    }
}
