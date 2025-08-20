using EvolutiveSystem.Common;
using EvolutiveSystem.Logic;
using EvolutiveSystem.QuantumSynthesis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Avvio del programma di test per AnalyzeFailures ---");

            var existingRegolaMIURules = GetExistingRulesFromDb();
            var testCases = GetTestStringsFromDb();

            var adaptedMIURules = existingRegolaMIURules.Select(r => new RegolaMIUAdapter(r)).ToList<MIURule>();
            var rulesEngine = new MIURulesEngine(adaptedMIURules);
            var proposer = new RuleCandidateProposer(rulesEngine);

            foreach (var testCase in testCases)
            {
                Console.WriteLine($"\nAnalisi del caso: source='{testCase.source}' target='{testCase.target}'");

                var failureDetails = proposer.AnalyzeFailures(testCase.source, testCase.target, existingRegolaMIURules);

                Console.WriteLine("--- Risultato dell'analisi ---");
                Console.WriteLine($"Motivo del fallimento (Mintermine): {failureDetails.FailureReasonPattern}");
                Console.WriteLine($"Stringa originale di partenza: {failureDetails.SourceString}");
                Console.WriteLine($"Stringa target: {failureDetails.TargetString}");

                if (string.IsNullOrEmpty(failureDetails.FailureReasonPattern) && failureDetails.SourceString == failureDetails.TargetString)
                {
                    Console.WriteLine("Risultato: Successo, la stringa target è stata raggiunta.");
                }
                else
                {
                    Console.WriteLine("Risultato: Fallimento, non è stato possibile raggiungere la stringa target.");
                    Console.WriteLine("Nessuna regola esistente ha potuto ridurre ulteriormente il mintermine.");
                }
            }

            Console.WriteLine("\n--- Fine del programma di test ---");
            Console.ReadKey();
        }

        /// <summary>
        /// Simula l'estrazione delle regole MIU corrette dal database.
        /// </summary>
        static List<RegolaMIU> GetExistingRulesFromDb()
        {
            return new List<RegolaMIU>
            {
                // Regola I: Corretto l'ordine dei parametri
                new RegolaMIU(1, "Regola I", "Sostituisce 'I' con 'IU' alla fine di una stringa", @"^(.*)I$", "$1IU"),
        
                // Regola II: Corretto l'ordine dei parametri
                new RegolaMIU(2, "Regola II", "Se la stringa è Mx, puoi formare Mxx", @"^M(.*)$", "M$1$1"),
        
                // Regola III: Corretto l'ordine dei parametri
                new RegolaMIU(3, "Regola III", "Se 'III' appare, può essere sostituito con 'U'", @"III", "U"),
        
                // Regola IV: Corretto l'ordine dei parametri
                new RegolaMIU(4, "Regola IV", "Rimuove due 'U' consecutive", @"UU", ""),
            };
        }

        /// <summary>
        /// Simula l'estrazione di coppie di stringhe di test basate sulle regole MIU.
        /// </summary>
        static List<(string source, string target)> GetTestStringsFromDb()
        {
            // Scenario 1: Il caso classico MI -> MU. Il sistema non dovrebbe trovare una soluzione
            // e dovrebbe identificare 'MI' come mintermine irriducibile.
            var scenario1 = ("MI", "MU");

            // Scenario 2: Un caso risolvibile. MI -> MII applicando la Regola II.
            var scenario2 = ("MI", "MII");

            // Scenario 3: Un caso complesso che richiede più passi.
            // MIIII -> MUI (applicando la Regola III)
            var scenario3 = ("MIIII", "MUI");

            // Scenario 4: Un caso non risolvibile ma con un mintermine più complesso.
            var scenario4 = ("MUIU", "MUIUUI");

            return new List<(string source, string target)> { scenario1, scenario2, scenario3, scenario4 };
        }
    }
}