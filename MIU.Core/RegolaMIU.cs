// creata il 14.6.2025 22.30
// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\RegolaMIU.cs
// Data di riferimento: 4 giugno 2025
// Contiene la definizione della classe RegolaMIU per rappresentare le regole del sistema MIU.
// MODIFICA 17.6.25: Modificato il tipo della proprietà ID da string a long per allinearsi al database.
// NUOVA MODIFICA 17.6.25: Rimossa l'implementazione dell'interfaccia IRegolaMIU, poiché non esiste.

using System.Text.RegularExpressions;

namespace MIU.Core
{
    // L'interfaccia IRegolaMIU è stata rimossa dalla dichiarazione della classe,
    // poiché non è stata trovata in nessuna parte della soluzione.
    public class RegolaMIU // : IRegolaMIU <--- RIMOSSO: QUESTA INTERFACCIA NON ESISTE
    {
        // Proprietà pubblica per l'identificatore unico della regola (ORA long)
        public long ID { get; }

        // Proprietà pubbliche per il nome, descrizione, pattern e stringa di sostituzione della regola.
        public string Nome { get; }
        public string Descrizione { get; }
        public string Pattern { get; }
        public string Sostituzione { get; }

        // Campo privato per una versione compilata del pattern Regex, per ottimizzare le prestazioni.
        private readonly Regex _regex;

        /// <summary>
        /// Costruttore per creare una nuova istanza di RegolaMIU.
        /// </summary>
        /// <param name="id">L'identificatore unico della regola (ORA long).</param>
        /// <param name="nome">Il nome descrittivo della regola.</param>
        /// <param name="descrizione">Una descrizione dettagliata di ciò che fa la regola.</param>
        /// <param name="pattern">Il pattern Regex da cercare nella stringa MIU.</param>
        /// <param name="sostituzione">La stringa con cui sostituire le corrispondenze del pattern.</param>
        public RegolaMIU(long id, string nome, string descrizione, string pattern, string sostituzione)
        {
            ID = id;
            Nome = nome;
            Descrizione = descrizione;
            Pattern = pattern;
            Sostituzione = sostituzione;
            // Compila il Regex una sola volta nel costruttore per riutilizzarlo.
            // IgnoreCase per ricerca insensibile alla maiuscola/minuscola, Compiled per prestazioni.
            _regex = new Regex(Pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Tenta di applicare la regola a una data stringa.
        /// </summary>
        /// <param name="inputString">La stringa MIU a cui tentare di applicare la regola.</param>
        /// <param name="outputString">La stringa risultante dopo l'applicazione della regola, se applicata con successo.</param>
        /// <returns>True se la regola è stata applicata con successo, altrimenti false.</returns>
        public bool TryApply(string inputString, out string outputString)
        {
            // Verifica se il pattern corrisponde alla stringa di input.
            if (_regex.IsMatch(inputString))
            {
                // Applica la sostituzione e assegna la stringa risultante a outputString.
                outputString = _regex.Replace(inputString, Sostituzione);
                return true;
            }

            // Se il pattern non corrisponde, la regola non può essere applicata.
            outputString = inputString; // Nessuna modifica alla stringa di output.
            return false;
        }
    }
}
