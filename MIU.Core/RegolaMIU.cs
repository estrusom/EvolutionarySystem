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
        public int ID { get; }
        public string Nome { get; }
        public string Descrizione { get; }
        public string Pattern { get; }
        public string Sostituzione { get; }

        public RegolaMIU(int id, string nome, string descrizione, string pattern, string sostituzione)
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
            //string standardInput = MIUStringConverter.DeflateMIUString(compressedInput);
            string standardInput = MIUStringConverter.InflateMIUString(compressedInput);

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
                        //compressedOutput = MIUStringConverter.InflateMIUString(standardOutput);
                        compressedOutput = MIUStringConverter.DeflateMIUString(standardOutput);
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
}
