// File: EvolutiveSystem.Synthesis/RuleEvaluator.cs
// Data di riferimento: 25 luglio 2025
// Descrizione: Componente responsabile della valutazione delle regole MIU candidate.
//              Determina l'efficacia e la validità di una regola prima della sua integrazione.

using System;
using MasterLog;
using MIU.Core; // Per RegolaMIU, IMIUDataManager
using EvolutiveSystem.Common; // Per MiuAbstractPattern (se necessario per contesto)

namespace EvolutiveSystem.Synthesis
{
    /// <summary>
    /// Il RuleEvaluator valuta la qualità e l'utilità delle regole MIU generate.
    /// </summary>
    public class RuleEvaluator
    {
        private readonly Logger _logger;
        private readonly IMIUDataManager _dataManager; // Per accedere ai dati esistenti e validare le regole

        /// <summary>
        /// Costruttore di RuleEvaluator.
        /// </summary>
        /// <param name="logger">L'istanza del logger.</param>
        /// <param name="dataManager">L'istanza del gestore dati per l'interazione con il database MIU.</param>
        public RuleEvaluator(Logger logger, IMIUDataManager dataManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _logger.Log(LogLevel.DEBUG, "RuleEvaluator istanziato.");
        }

        /// <summary>
        /// Valuta una regola MIU candidata.
        /// Per ora, una valutazione semplice: la regola è valida se il suo pattern di input
        /// è diverso dal suo pattern di output (non è una regola identità) e se è un pattern MIU valido.
        /// In futuro, questa logica sarà molto più sofisticata (es. test su set di dati, simulazioni).
        /// </summary>
        /// <param name="rule">La RegolaMIU da valutare.</param>
        /// <returns>True se la regola è considerata valida e utile, altrimenti false.</returns>
        public bool EvaluateRule(RegolaMIU rule)
        {
            _logger.Log(LogLevel.INFO, $"[RuleEvaluator] Valutazione della regola: {rule.Nome} (ID: {rule.ID})");

            // Criterio 1: La regola deve produrre una trasformazione (non deve essere una regola identità)
            if (rule.Pattern == rule.Sostituzione)
            {
                _logger.Log(LogLevel.WARNING, $"[RuleEvaluator] Regola '{rule.Nome}' scartata: è una regola identità (Input = Output).");
                return false;
            }

            // Criterio 2: I pattern devono essere composti solo da caratteri MIU validi (M, I, U)
            // Questo metodo helper IsValidMiuString() è definito qui sotto.
            if (!IsValidMiuString(rule.Pattern) || !IsValidMiuString(rule.Sostituzione))
            {
                _logger.Log(LogLevel.WARNING, $"[RuleEvaluator] Regola '{rule.Nome}' scartata: contiene caratteri non MIU validi.");
                return false;
            }

            // Criterio 3: La regola non deve essere già presente nel sistema (controllo di duplicazione)
            // Questo richiederà un metodo in IMIUDataManager per verificare l'esistenza di una regola.
            // Per ora, lo lascio come TODO, ma è cruciale per evitare ridondanze.
            // if (_dataManager.RuleExists(rule.Pattern, rule.Sostituzione))
            // {
            //     _logger.Log(LogLevel.WARNING, $"[RuleEvaluator] Regola '{rule.Nome}' scartata: duplicato di una regola esistente.");
            //     return false;
            // }

            // Criterio 4 (più avanzato): La regola dovrebbe essere applicabile a qualche stringa esistente
            // o portare a un miglioramento in una simulazione. Questo è un TODO per il futuro.
            // Per ora, consideriamo che se supera i primi due criteri, è un candidato valido.

            _logger.Log(LogLevel.INFO, $"[RuleEvaluator] Regola '{rule.Nome}' considerata valida per l'integrazione.");
            return true;
        }

        /// <summary>
        /// Metodo helper per validare che una stringa contenga solo caratteri 'M', 'I', 'U'.
        /// Questo potrebbe essere spostato in una utility class in Common in futuro.
        /// </summary>
        private bool IsValidMiuString(string s)
        {
            if (string.IsNullOrEmpty(s)) return false; // Un pattern vuoto non è valido per le regole MIU
            foreach (char c in s)
            {
                if (c != 'M' && c != 'I' && c != 'U')
                {
                    return false;
                }
            }
            return true;
        }
    }
}
