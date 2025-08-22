using System; // Necessario per DateTime

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Enumera i tipi di anomalie di esplorazione che il sistema può rilevare.
    /// Questa è la base della tassonomia delle anomalie.
    /// </summary>
    public enum AnomalyType
    {
        None = 0,
        /// <summary>
        /// Indica che una regola ha generato una stringa che ha superato il limite massimo di lunghezza ed è stata scartata.
        /// </summary>
        ExcessiveLengthGeneration = 1,
        /// <summary>
        /// Indica che una regola specifica o una transizione ha un tasso di successo persistentemente basso nonostante numerosi tentativi.
        /// </summary>
        PersistentLowSuccessRate = 2,
        /// <summary>
        /// Indica che una regola o una transizione è stata applicata molto raramente o mai, suggerendo un'area inesplorata.
        /// </summary>
        UnderutilizedRuleOrTransition = 3,
        /// <summary>
        /// Indica che una stringa generata non è riuscita a innescare ulteriori derivazioni utili.
        /// </summary>
        DeadEndString = 4,
        /// <summary>
        /// Indica che una ricerca completa è fallita (e.g. per aver raggiunto i limiti di esplorazione).
        /// </summary>
        SearchFailure = 5,
        /// <summary>
        /// Indica che la ricerca ha superato il numero massimo di nodi da esplorare.
        /// </summary>
        MaxNodesExplored = 6,
        /// <summary>
        /// Indica che la ricerca ha raggiunto la massima profondità consentita, portando al "potamento" del ramo.
        /// </summary>
        MaxDepthReached = 7
    }

    /// <summary>
    /// Rappresenta un'anomalia di esplorazione rilevata dal sistema.
    /// Questi oggetti saranno persistiti nel database per costruire la tassonomia.
    /// </summary>
    public class ExplorationAnomaly
    {
        public long Id { get; set; } // ID del database (PRIMARY KEY)
        public AnomalyType Type { get; set; } // Tipo di anomalia (dal nostro enum)
        public long? RuleId { get; set; } // ID della regola coinvolta, nullable se non applicabile
        public int? ContextPatternHash { get; set; } // Hash della stringa contesto compressa, per identificare il contesto
        public string ContextPatternSample { get; set; } // Un piccolo campione della stringa contesto per debug/descrizione
        public int Count { get; set; } // Quante volte questo specifico fenomeno si è verificato
        public double AverageValue { get; set; } // Valore medio rilevante (es. lunghezza media, tasso di successo)
        public double AverageDepth { get; set; } // Profondità media a cui si è verificata l'anomalia (usiamo double per la media)
        public DateTime LastDetected { get; set; } // Ultima volta che l'anomalia è stata rilevata/aggiornata
        public string Description { get; set; } // Descrizione generata dell'anomalia
        public bool IsNewCategory { get; set; } // Indica se questa è una categoria di anomalia appena identificata
        public DateTime CreatedDate { get; set; } // Data di creazione della voce nel DB
                                                  // Nuove proprietà per le anomalie di tipo 'SearchFailure'
        public long? SearchID { get; set; }
        public string SourceString { get; set; }
        public string TargetString { get; set; }
        public string Severity { get; set; }

        /// <summary>
        /// Costruttore per facilitare la creazione di nuove istanze di anomalie in memoria.
        /// </summary>
        /// <param name="type">Il tipo di anomalia.</param>
        /// <param name="ruleId">L'ID della regola MIU coinvolta (nullable).</param>
        /// <param name="contextPatternHash">L'hash della stringa contesto compressa (nullable).</param>
        /// <param name="contextPatternSample">Un campione della stringa contesto (nullable).</param>
        /// <param name="description">Una descrizione leggibile dell'anomalia.</param>
        public ExplorationAnomaly(AnomalyType type, long? ruleId, int? contextPatternHash, string contextPatternSample, string description)
        {
            Type = type;
            RuleId = ruleId;
            ContextPatternHash = contextPatternHash;
            ContextPatternSample = contextPatternSample;
            Description = description;
            Count = 0; // Inizializza a 0, verrà incrementato da Update() o nel primo RecordAnomalyEvent
            AverageValue = 0;
            AverageDepth = 0;
            LastDetected = DateTime.UtcNow;
            IsNewCategory = true; // Presupponiamo che sia nuova finché non viene aggiornata dal manager
            CreatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Costruttore vuoto necessario per la deserializzazione da database (ORM).
        /// </summary>
        public ExplorationAnomaly() { }

        /// <summary>
        /// Aggiorna le statistiche di un'anomalia esistente in memoria.
        /// </summary>
        /// <param name="newValue">Il nuovo valore rilevante da incorporare nella media (es. lunghezza di una stringa scartata).</param>
        /// <param name="newDepth">La profondità a cui è stato rilevato l'evento.</param>
        public void Update(double newValue, int newDepth)
        {
            // Aggiorna la media in modo incrementale per AverageValue e AverageDepth
            AverageValue = (AverageValue * Count + newValue) / (Count + 1);
            AverageDepth = (AverageDepth * Count + newDepth) / (Count + 1);
            Count++;
            LastDetected = DateTime.UtcNow;
            IsNewCategory = false; // Non è più una categoria "nuova" una volta aggiornata
        }
    }
}
