// File: MiuSeederTool.Core/MiuStringHelper.cs
// Descrizione: Fornisce metodi per l'applicazione delle regole di derivazione MIU,
//              ora implementando l'applicazione delle regole tramite espressioni regolari (Regex)
//              caricate dai pattern del database.
// Target Framework: .NET Framework 4.8

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; // Necessario per Regex
using MasterLog; // Necessario per LogLevel

namespace MiuSeederTool.Core
{
    /// <summary>
    /// Fornisce metodi per l'applicazione delle regole di derivazione MIU.
    /// Questa classe ora utilizza le espressioni regolari definite nelle regole per la derivazione.
    /// </summary>
    public class MiuStringHelper
    {
        private readonly IEnumerable<SeederMiuRule> _rules;
        private readonly Logger _logger;
        // Dictionary per memorizzare le espressioni regolari pre-compilate per efficienza.
        private readonly Dictionary<long, Regex> _compiledRegexes;

        /// <summary>
        /// Inizializza una nuova istanza di MiuStringHelper.
        /// </summary>
        /// <param name="rules">Le regole MIU da utilizzare per la derivazione.</param>
        /// <param name="logger">The Logger instance for logging.</param>
        public MiuStringHelper(IEnumerable<SeederMiuRule> rules, Logger logger)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _compiledRegexes = new Dictionary<long, Regex>();

            // Pre-compila le espressioni regolari per ogni regola all'inizializzazione.
            // Questo migliora le prestazioni evitando la ricompilazione ad ogni applicazione della regola.
            foreach (var rule in _rules)
            {
                try
                {
                    // RegexOptions.Compiled migliora le prestazioni per usi ripetuti.
                    // RegexOptions.None è l'opzione predefinita, puoi aggiungere IgnoreCase, Multiline, ecc. se necessario.
                    _compiledRegexes[rule.RuleID] = new Regex(rule.Pattern, RegexOptions.Compiled);
                }
                catch (ArgumentException ex)
                {
                    _logger.Log(LogLevel.ERROR, $"[MiuStringHelper] ERRORE: Pattern Regex non valido per la regola ID {rule.RuleID} ('{rule.RuleName}'). Pattern: '{rule.Pattern}'. Dettagli: {ex.Message}");
                    // Potresti voler gestire questo errore in modo più robusto, ad esempio non aggiungendo la regola.
                }
            }
        }

        /// <summary>
        /// Applica tutte le regole MIU possibili a una data stringa e restituisce le stringhe derivate.
        /// </summary>
        /// <param name="currentString">La stringa MIU a cui applicare le regole.</param>
        /// <returns>Un IEnumerable di stringhe MIU derivate.</returns>
        public IEnumerable<string> ApplyAllRules(string currentString)
        {
            var derivedStrings = new HashSet<string>();
            _logger.Log(LogLevel.DEBUG, $"[MiuStringHelper] Tentativo di applicare regole a: '{currentString}'");

            foreach (var rule in _rules)
            {
                // Verifica se l'espressione regolare per questa regola è stata compilata con successo.
                if (_compiledRegexes.TryGetValue(rule.RuleID, out Regex compiledRegex))
                {
                    // ApplyRule ora utilizza il Regex pre-compilato.
                    var results = ApplyRule(currentString, rule, compiledRegex);
                    if (results.Any())
                    {
                        _logger.Log(LogLevel.DEBUG, $"[MiuStringHelper] Regola {rule.RuleID} ('{rule.RuleName}') applicata con successo. Derivate: {string.Join(", ", results.Take(5))}...");
                        foreach (var result in results)
                        {
                            derivedStrings.Add(result);
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.DEBUG, $"[MiuStringHelper] Regola {rule.RuleID} ('{rule.RuleName}') non applicabile a: '{currentString}'");
                    }
                }
                else
                {
                    _logger.Log(LogLevel.WARNING, $"[MiuStringHelper] Regola ID {rule.RuleID} ('{rule.RuleName}') non ha un Regex compilato valido. Saltata.");
                }
            }
            _logger.Log(LogLevel.DEBUG, $"[MiuStringHelper] Totale stringhe derivate da '{currentString}': {derivedStrings.Count}");
            return derivedStrings;
        }

        /// <summary>
        /// Applica una singola regola MIU a una data stringa utilizzando l'espressione regolare pre-compilata.
        /// </summary>
        /// <param name="inputString">La stringa MIU a cui applicare la regola.</param>
        /// <param name="rule">La regola MIU da applicare (contiene Pattern e Replacement).</param>
        /// <param name="compiledRegex">L'istanza Regex pre-compilata per questa regola.</param>
        /// <returns>Un IEnumerable di stringhe MIU derivate dall'applicazione della regola.</returns>
        private IEnumerable<string> ApplyRule(string inputString, SeederMiuRule rule, Regex compiledRegex)
        {
            var results = new List<string>();

            try
            {
                // Trova tutte le occorrenze del pattern nella stringa di input.
                MatchCollection matches = compiledRegex.Matches(inputString);

                if (matches.Count > 0)
                {
                    // Per ogni corrispondenza, applica la sostituzione.
                    // Nota: Regex.Replace di default sostituisce tutte le occorrenze.
                    // Se una regola deve applicarsi una sola volta per corrispondenza,
                    // la logica qui potrebbe dover essere più complessa (es. iterare e sostituire una per una).
                    // Per le regole MIU standard, ReplaceAll è spesso il comportamento desiderato.
                    string newString = compiledRegex.Replace(inputString, rule.Replacement);

                    // Aggiungi il risultato solo se è diverso dalla stringa originale e non è vuoto
                    if (!string.IsNullOrEmpty(newString) && newString != inputString)
                    {
                        results.Add(newString);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MiuStringHelper] ERRORE nell'applicazione della regola {rule.RuleID} ('{rule.RuleName}') a '{inputString}': {ex.Message}");
            }

            // Restituisce solo i risultati unici e validi.
            return results.Distinct();
        }
    }
}
