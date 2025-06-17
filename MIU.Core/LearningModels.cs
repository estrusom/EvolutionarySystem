//creta 9.6.2025 0.35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{
    /// <summary>
    /// Modello per la persistenza delle statistiche aggregate di una singola regola MIU.
    /// Utilizzato da EmergingProcesses e dal livello di persistenza.
    /// </summary>
    public class RuleStatistics
    {
        //2025.06.12 Modificata integralmente
        // Potresti voler aggiungere anche un costruttore vuoto o altri costruttori a seconda delle tue esigenze
        public RuleStatistics() { }

        //public Guid RuleId { get; set; } // Assumo che l'ID della regola sia un Guid
        public int RuleId { get; set; } // <--- CAMBIATO DA Guid A int
        public int ApplicationCount { get; set; }
        public int SuccessCount { get; set; }
        public double EffectivenessScore { get; set; }
        // NUOVA PROPRIETÀ: Timestamp dell'ultima applicazione della regola
        public long LastApplicationTimestamp { get; set; } // Usa 'long' per i secondi Unix

        public RuleStatistics(int ruleId)
        {
            RuleId = ruleId;
            ApplicationCount = 0;
            SuccessCount = 0;
            EffectivenessScore = 0.0;
            //LastApplicationTimestamp = 0; // Inizializza a zero o un valore predefinito
            //2025.06.12
            LastApplicationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Inizializza al momento della creazione
        }
        public void RecalculateEffectiveness()
        {
            if (ApplicationCount > 0)
            {
                EffectivenessScore = (double)SuccessCount / ApplicationCount;
            }
            else
            {
                EffectivenessScore = 0.0;
            }
        }
    }
    /*
    /// <summary>
    /// Modello per la persistenza delle statistiche aggregate delle transizioni.
    /// Utilizzato da EmergingProcesses e dal livello di persistenza.
    /// </summary>
    public class TransitionStatistics
    {

        public string ParentStringCompressed { get; set; }
        public int AppliedRuleID { get; set; }
        public int ApplicationCount { get; set; } // Quante volte questa transizione è stata applicata
        public int SuccessfulCount { get; set; } // Quante volte questa transizione ha fatto parte di un percorso di successo
        public string LastUpdated { get; set; } // Aggiunto per tracciare l'ultimo aggiornamento nel DB
    }
    */
    /// <summary>
    /// 2025.06.12 sostituzione integrale
    /// Rappresenta le statistiche di apprendimento per una specifica transizione tra due stringhe MIU
    /// e l'applicazione di una Regola MIU, allineate con le colonne del database.
    /// </summary>
    public class TransitionStatistics
    {
        /// <summary>
        /// La stringa MIU compressa dello stato genitore (corrisponde a ParentStringCompressed nel DB).
        /// </summary>
        public string ParentStringCompressed { get; set; } // <--- Ripristinato questo nome

        /// <summary>
        /// L'ID della regola MIU applicata durante questa transizione.
        /// </summary>
        public int AppliedRuleID { get; set; } // <--- Ripristinato questo nome

        /// <summary>
        /// Numero totale di volte in cui questa specifica transizione è stata tentata.
        /// </summary>
        public int ApplicationCount { get; set; }

        /// <summary>
        /// Numero di volte in cui questa transizione ha fatto parte di un percorso di successo (corrisponde a SuccessfulCount nel DB).
        /// </summary>
        public int SuccessfulCount { get; set; } // <--- Ripristinato questo nome

        /// <summary>
        /// Il tasso di successo per questa transizione (SuccessfulCount / ApplicationCount).
        /// Questa è una proprietà calcolata in memoria.
        /// </summary>
        public double SuccessRate
        {
            get
            {
                if (ApplicationCount > 0)
                {
                    return (double)SuccessfulCount / ApplicationCount;
                }
                return 0.0;
            }
            // Non ha un set esplicito perché è calcolata
        }

        /// <summary>
        /// Il costo medio di applicazione della regola per questa transizione.
        /// </summary>
        public double AverageCost { get; set; }

        /// <summary>
        /// Data e ora dell'ultimo aggiornamento di queste statistiche (corrisponde a LastUpdated nel DB).
        /// Salvata come stringa in formato ISO 8601 (ToString("o")).
        /// </summary>
        public string LastUpdated { get; set; } // <--- Ripristinato questo nome

        // Nota: LastApplicationTimestamp non è nel tuo DB per TransitionStatistics, ma è utile per l'aging.
        // Lo manterrei per la logica interna di EmergingProcesses e potremmo mapparlo a LastUpdated in DB se necessario.
        // Per ora, lo rimuovo per ridurre la confusione con i nomi del DB.
        // Se EmergingProcesses ha bisogno di un timestamp separato per l'aging, lo gestirà internamente.


        /// <summary>
        /// Costruttore per TransitionStatistics.
        /// </summary>
        /// <param name="parentStringCompressed">La stringa MIU compressa del genitore.</param>
        /// <param name="appliedRuleId">L'ID della regola applicata.</param>
        public TransitionStatistics(string parentStringCompressed, int appliedRuleId)
        {
            ParentStringCompressed = parentStringCompressed;
            AppliedRuleID = appliedRuleId;
            ApplicationCount = 0;
            SuccessfulCount = 0;
            AverageCost = 0.0;
            LastUpdated = DateTime.UtcNow.ToString("o"); // Inizializza al momento della creazione
        }

        /// <summary>
        /// Costruttore vuoto, necessario per la deserializzazione o la creazione da query di database.
        /// </summary>
        public TransitionStatistics() { }
    }
}
