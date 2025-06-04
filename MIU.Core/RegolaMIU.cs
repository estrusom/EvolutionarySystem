using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MIU.Core
{
    // -------------------------------------------------------------------
    // 3. Classe RegolaMIU (Assunzione di struttura e modifica TryApply)
    // -------------------------------------------------------------------
    public class RegolaMIU
    {
        public string ID { get; }
        public string Nome { get; }
        public string Descrizione { get; }
        public string Pattern { get; }
        public string Sostituzione { get; }

        public RegolaMIU(string id, string nome, string descrizione, string pattern, string sostituzione)
        {
            ID = id;
            Nome = nome;
            Descrizione = descrizione;
            Pattern = pattern;
            Sostituzione = sostituzione;
        }

        /// <summary>
        /// Tenta di applicare la regola a una stringa MIU compressa.
        /// Decomprime la stringa, applica la regola, e ricomprime il risultato.
        /// </summary>
        /// <param name="compressedInput">La stringa MIU compressa a cui applicare la regola.</param>
        /// <param name="compressedOutput">La stringa MIU compressa risultante se la regola è applicata.</param>
        /// <returns>True se la regola è stata applicata con successo, altrimenti false.</returns>
        public bool TryApply(string compressedInput, out string compressedOutput)
        {
            compressedOutput = compressedInput; // Inizializza con l'input in caso di fallimento

            // 1. Decomprimi la stringa compressa per applicare la regola
            string standardInput = MIUStringConverter.DeflateMIUString(compressedInput);

            // Applica la logica specifica della regola (pattern matching e sostituzione)
            // Questo è un esempio generico, la tua implementazione reale di TryApply
            // dovrebbe gestire i pattern specifici delle regole MIU (R1, R2, R3, R4).
            // Per esempio, se Pattern è una regex e Sostituzione è la stringa di rimpiazzo.
            try
            {
                // Esempio generico di applicazione di una regola basata su Regex
                // Questo deve essere adattato alla tua implementazione specifica di RegolaMIU
                // che gestisce R1, R2, R3, R4.
                // Per semplicità, qui simulo l'applicazione di una regola base.
                // Idealmente, ogni regola MIU avrebbe la sua logica specifica qui.

                // Esempio semplificato: se il pattern è una regex
                if (!string.IsNullOrEmpty(Pattern))
                {
                    Regex regex = new Regex(Pattern);
                    if (regex.IsMatch(standardInput))
                    {
                        string standardOutput = regex.Replace(standardInput, Sostituzione ?? ""); // Se Sostituzione è null, rimpiazza con vuoto
                        compressedOutput = MIUStringConverter.InflateMIUString(standardOutput);
                        return true;
                    }
                }
                // Se la regola non ha un pattern o non si applica, ritorna false
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nell'applicazione della regola '{Nome}': {ex.Message}");
                return false;
            }
        }
    }
    /*
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
                //string sost = Sostituzione == null ? "NULL" : Sostituzione;
                //Console.WriteLine($"inputString: {inputString} Pattern: {Pattern} Sostituzione: {sost}");
                //Console.WriteLine($"inputString: {inputString} newString: {newString}");
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
    */
}
