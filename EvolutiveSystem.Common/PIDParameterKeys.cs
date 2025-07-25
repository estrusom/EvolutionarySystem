// File: EvolutiveSystem.Common/PIDParameterKeys.cs
// Data di riferimento: 25 luglio 2025
// Descrizione: Definisce le chiavi costanti per i parametri del controller PID e Feedforward,
//              utilizzate per la configurazione e la persistenza nel MIUParameterConfigurator.

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Contiene le chiavi costanti utilizzate per identificare i parametri
    /// del controller PID e del termine di Feedforward nel sistema di configurazione.
    /// </summary>
    public static class PIDParameterKeys
    {
        // Parametri del controller PID
        public const string Kp = "PID_Kp"; // Guadagno Proporzionale
        public const string Ki = "PID_Ki"; // Guadagno Integrale
        public const string Kd = "PID_Kd"; // Guadagno Derivativo

        // Parametri del termine di Feedforward
        public const string FeedforwardWeight = "PID_FeedforwardWeight"; // Peso del termine Feedforward
        public const string FeedforwardLookaheadDepth = "PID_FeedforwardLookaheadDepth"; // Profondità di "previsione" per il Feedforward

        // Potresti aggiungere altre chiavi in futuro, ad esempio:
        // public const string MaxOutput = "PID_MaxOutput";
        // public const string MinOutput = "PID_MinOutput";
        // public const string TargetOptimizationMetric = "PID_TargetOptimizationMetric";
    }
}
