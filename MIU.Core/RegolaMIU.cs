using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MIU.Core
{
    //// <summary>
    /// Rappresenta una singola regola MIU.
    /// </summary>
    public class RegolaMIU
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Descrizione { get; set; }
        public string Pattern { get; set; }
        public string Sostituzione { get; set; }

        public RegolaMIU() { }
        public RegolaMIU(string id, string nome, string descrizione, string pattern, string sostituzione)
        {
            Id = id;
            Nome = nome;
            Descrizione = descrizione;
            Pattern = pattern;
            Sostituzione = sostituzione;
        }
        /// <summary>
        /// Applica questa specifica regola MIU a una stringa di input.
        /// </summary>
        /// <param name="inputString">La stringa a cui applicare la regola.</param>
        /// <param name="outputString">La stringa risultante dopo l'applicazione della regola, se applicata.</param>
        /// <returns>True se la regola è stata applicata e la stringa è cambiata, altrimenti false.</returns>
        public bool TryApply(string inputString, out string outputString)
        {
            outputString = inputString; // Inizializza con la stringa di input

            if (string.IsNullOrEmpty(Pattern))
            {
                // Se il pattern è vuoto, la regola non può essere applicata.
                return false;
            }

            try
            {
                // Applica l'espressione regolare.
                // Regex.Replace restituisce la stringa modificata se il pattern corrisponde,
                // altrimenti restituisce la stringa originale.
                string newString = Regex.Replace(inputString, Pattern, Sostituzione ?? "");

                if (newString != inputString)
                {
                    // La regola è stata applicata e la stringa è cambiata.
                    outputString = newString;
                    return true;
                }
                // La regola è stata applicata ma la stringa non è cambiata (es. pattern non trovato).
                return false;
            }
            catch (ArgumentException ex)
            {
                // Gestisce errori nel pattern dell'espressione regolare.
                Console.WriteLine($"Errore: Pattern '{Pattern}' non valido per la regola '{Nome}'. {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // Gestisce altri errori generici durante l'applicazione della regola.
                Console.WriteLine($"Errore generico nell'applicazione della regola '{Nome}': {ex.Message}");
                return false;
            }
        }
    }
}
