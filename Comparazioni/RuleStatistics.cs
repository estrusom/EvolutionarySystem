// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\RuleStatistics.cs
// Data di riferimento: 20 giugno 2025
// Contiene la definizione della classe RuleStatistics per tracciare le performance di ogni regola MIU.
// Implementa Laplace Smoothing e un fattore di recenza per il calcolo dell'EffectivenessScore.
// La proprietà SuccessRate è stata inglobata e raffinata nel calcolo di EffectivenessScore.

using System;

namespace MIU.Core
{
    /// <summary>
    /// Classe per memorizzare le statistiche di apprendimento per una specifica regola MIU.
    /// Include un calcolo raffinato dell'EffectivenessScore per la prioritizzazione.
    /// </summary>
    public class RuleStatistics
    {
        public long RuleID { get; set; } // L'ID della regola a cui si riferiscono queste statistiche
        public int ApplicationCount { get; set; } // Il numero di volte che la regola è stata applicata
        public int SuccessfulCount { get; set; } // Il numero di volte che la regola ha contribuito a un percorso di successo
        public double EffectivenessScore { get; set; } // Il punteggio di efficacia calcolato per la regola (include SuccessRate e Recency)
        public DateTime LastApplicationTimestamp { get; set; } // L'ultimo timestamp di quando questa regola è stata applicata

        public RuleStatistics()
        {
            ApplicationCount = 0;
            SuccessfulCount = 0;
            EffectivenessScore = 0.0;
            LastApplicationTimestamp = DateTime.MinValue; // Inizializza al valore minimo
        }

        /// <summary>
        /// Ricalcola l'EffectivenessScore della regola utilizzando la levigatura di Laplace
        /// e un fattore di recenza.
        /// </summary>
        public void RecalculateEffectiveness()
        {
            // 1. Laplace Smoothing:
            // Aggiunge un "successo virtuale" (+1) e due "applicazioni virtuali" (+2) per:
            // - Evitare divisioni per zero (se ApplicationCount è 0).
            // - Dare una stima più robusta per le regole con poche applicazioni (evita che 1/1 dia 100% di efficacia).
            // Formula: (SuccessfulCount + 1) / (ApplicationCount + 2)
            double smoothedScore = (double)(SuccessfulCount + 1) / (ApplicationCount + 2);

            // 2. Fattore di Recenza:
            // Diamo maggiore importanza alle applicazioni più recenti.
            // Calcoliamo la differenza di tempo dall'ultima applicazione.
            // Più è piccola la differenza (più recente), maggiore sarà il bonus.
            TimeSpan timeSinceLastApplication = DateTime.Now - LastApplicationTimestamp;

            double recencyFactor = 1.0; // Fattore neutro (nessun bonus/penalità)
            
            // Applica un bonus se la regola è stata applicata molto di recente
            // Controlla che LastApplicationTimestamp non sia il valore predefinito DateTime.MinValue,
            // altrimenti il calcolo timeSinceLastApplication potrebbe dare un valore molto grande.
            if (LastApplicationTimestamp != DateTime.MinValue) 
            {
                if (timeSinceLastApplication.TotalHours < 1)
                {
                    recencyFactor = 1.1; // Bonus del 10% se usata nell'ultima ora
                }
                else if (timeSinceLastApplication.TotalDays < 7)
                {
                    recencyFactor = 1.05; // Bonus del 5% se usata nell'ultima settimana
                }
                // Si potrebbero aggiungere altre soglie o una penalità per regole molto vecchie,
                // ma per ora manteniamo un focus sul bonus di recenza.
            }
            
            // Il punteggio finale è il punteggio levigato moltiplicato per il fattore di recenza
            EffectivenessScore = smoothedScore * recencyFactor;

            // Assicurati che il punteggio rimanga nel range 0-1, tipico per un punteggio di efficacia/probabilità.
            EffectivenessScore = Math.Min(EffectivenessScore, 1.0); 
            EffectivenessScore = Math.Max(EffectivenessScore, 0.0); // Assicurati che non scenda sotto zero
        }
    }
}
