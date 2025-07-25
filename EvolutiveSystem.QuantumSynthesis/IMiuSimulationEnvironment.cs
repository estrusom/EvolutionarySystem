// File: EvolutiveSystem.QuantumSynthesis/IMiuSimulationEnvironment.cs
// Data di riferimento: 25 luglio 2025
// Descrizione: Interfaccia per l'ambiente di simulazione MIU isolato,
//              utilizzato per testare l'impatto delle regole candidate.

using System; // Necessario per TimeSpan
using System.Collections.Generic;
using System.Threading.Tasks;
using EvolutiveSystem.Common; // Per RegolaMIU, MiuStateInfo
using EvolutiveSystem.Taxonomy; // Per MiuAbstractPattern (per metriche sui pattern)

namespace EvolutiveSystem.QuantumSynthesis
{
    /// <summary>
    /// Definisce il contratto per un ambiente di simulazione isolato del sistema MIU.
    /// Questo ambiente permette di testare l'impatto di un set di regole su un'esplorazione
    /// del paesaggio MIU, restituendo metriche sul "campo di esistenza" risultante.
    /// </summary>
    public interface IMiuSimulationEnvironment
    {
        /// <summary>
        /// Esegue una simulazione dell'esplorazione del paesaggio MIU con un dato set di regole.
        /// </summary>
        /// <param name="rulesToSimulate">La lista di RegolaMIU da utilizzare nella simulazione.</param>
        /// <param name="initialString">La stringa MIU iniziale per la simulazione (es. "MI").</param>
        /// <param name="maxSteps">Il numero massimo di passi di esplorazione nella simulazione.</param>
        /// <returns>Un oggetto contenente le metriche aggregate della simulazione.</returns>
        Task<SimulationResult> SimulateExplorationAsync(List<RegolaMIU> rulesToSimulate, string initialString, int maxSteps);
    }

    // File: EvolutiveSystem.QuantumSynthesis/IMiuSimulationEnvironment.cs
    // La classe seguente SOSTITUISCE la precedente definizione di SimulationResult.

    /// <summary>
    /// Rappresenta il risultato aggregato di una simulazione del campo di esistenza MIU,
    /// includendo metriche per la valutazione del "collasso della funzione d'onda".
    /// </summary>
    public class SimulationResult
    {
        // Metriche di Esplorazione e Ampiezza del Campo di Esistenza
        public int TotalStatesExplored { get; set; } // Numero totale di stati unici raggiunti
        public int MaxDepthReached { get; set; } // Massima profondità raggiunta nell'esplorazione
        public double AverageDepthOfDiscovery { get; set; } // Profondità media alla quale nuovi stati sono stati scoperti
        public List<MiuStateInfo> DiscoveredStates { get; set; } // Lista degli stati unici scoperti (per analisi dettagliata)

        // Metriche di Diversità dei Pattern (per catturare il concetto di "lenti polarizzate" e variazioni)
        public int UniquePatternCount { get; set; } // Numero di tipi di pattern astratti unici osservati
        public Dictionary<MiuAbstractPattern, int> PatternOccurrenceCounts { get; set; } // Conteggio delle occorrenze per ogni tipo di pattern
        public double PatternDiversityScore { get; set; } // Punteggio di diversità dei pattern (es. entropia di Shannon sulla distribuzione delle frequenze)
        public double AverageVariationDepth { get; set; } // Profondità media delle derivazioni incrementali dei pattern
        public int TotalVariationsGenerated { get; set; } // Numero totale di pattern considerati "variazioni"

        // Metriche di Bilanciamento dei Token (per catturare accumuli/diradamenti)
        public Dictionary<string, int> TokenCounts { get; set; } // Conteggio finale dei token (M, I, U) nelle stringhe generate
        public double M_Ratio { get; set; } // Rapporto M / (M+I+U)
        public double I_Ratio { get; set; } // Rapporto I / (M+I+U)
        public double U_Ratio { get; set; } // Rapporto U / (M+I+U)
        public double TokenBalanceScore { get; set; } // Punteggio complessivo di bilanciamento dei token (es. deviazione da un ideale)

        // Metriche di Efficienza
        public int TotalRuleApplications { get; set; } // Numero totale di applicazioni di regole nella simulazione
        public TimeSpan ElapsedTime { get; set; } // Tempo totale impiegato dalla simulazione

        // Metriche di Qualità Strutturale
        public double AverageStringLength { get; set; } // Lunghezza media delle stringhe generate
        public double StringLengthVariance { get; set; } // Varianza della lunghezza delle stringhe (per la diversità)

        // Metriche di Risoluzione dell'Antitesi (Il Filtro Primario - ora continuo)
        /// <summary>
        /// Indica il grado di risoluzione o mitigazione dell'antitesi target (valore tra 0.0 e 1.0).
        /// 0.0 = nessun progresso, 1.0 = antitesi completamente risolta.
        /// </summary>
        public double TargetAntithesisResolutionScore { get; set; }
        public string ResolutionDetails { get; set; } // Dettagli qualitativi sulla risoluzione dell'antitesi

        /// <summary>
        /// Costruttore di default. Inizializza le collezioni e i valori predefiniti.
        /// </summary>
        public SimulationResult()
        {
            DiscoveredStates = new List<MiuStateInfo>();
            PatternOccurrenceCounts = new Dictionary<MiuAbstractPattern, int>();
            TokenCounts = new Dictionary<string, int>();
            TargetAntithesisResolutionScore = 0.0; // Inizializza a 0.0
            PatternDiversityScore = 0.0;
            AverageVariationDepth = 0.0;
            TokenBalanceScore = 0.0;
            StringLengthVariance = 0.0;
        }
    }
}
