// creata il 14.6.2025 22.30
// File: C:\Progetti\EvolutiveSystem_250604\MIU.Core\RegolaMIU.cs
// Data di riferimento: 4 giugno 2025
// Contiene la definizione della classe RegolaMIU per rappresentare le regole del sistema MIU.
// MODIFICA 17.6.25: Modificato il tipo della proprietà ID da string a long per allinearsi al database.
// NUOVA MODIFICA 17.6.25: Rimossa l'implementazione dell'interfaccia IRegolaMIU, poiché non esiste.

using System.Text.RegularExpressions;

namespace EvolutiveSystem.Common
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
        /// <summary>
        /// Stima della profondità media di derivazione (numero di passi) che questa regola tende a generare
        /// per raggiungere un obiettivo rilevante (es. un'antitesi).
        /// Viene aggiornata dinamicamente in base ai successi delle simulazioni.
        /// </summary>
        public double StimaProfonditaMedia { get; set; } // <<-- Questa è l'unica con 'set;' pubblico

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
        public RegolaMIU(long id, string nome, string descrizione, string pattern, string sostituzione, double stimaProfonditaMedia = 0.0)
        {
            ID = id;
            Nome = nome;
            Descrizione = descrizione;
            Pattern = pattern;
            Sostituzione = sostituzione;
            // Compila il Regex una sola volta nel costruttore per riutilizzarlo.
            // IgnoreCase per ricerca insensibile alla maiuscola/minuscola, Compiled per prestazioni.
            _regex = new Regex(Pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            StimaProfonditaMedia = stimaProfonditaMedia;
        }
        // ma in questo caso, poiché tutte le proprietà principali sono get-only,
        // l'ORM avrà bisogno di un modo per "costruire" l'oggetto e poi impostare StimaProfonditaMedia.
        // Se le altre proprietà (ID, Nome ecc.) non possono essere impostate dopo la costruzione,
        // l'ORM dovrà usare il costruttore parametrico. Questo dipende dall'ORM.
        // Per massima compatibilità con ORM come Dapper che usano i setter se disponibili,
        // o con JSON/XML deserialization che usano constructor + setters:
        public RegolaMIU()
        {
            // Inizializzazioni per evitare null reference se non si usa il costruttore parametrico.
            // Le proprietà { get; } rimarranno i loro valori di default o non inizializzati se non c'è un ORM che le popola.
            Nome = string.Empty; // Può essere inizializzato qui per evitare nulls
            Descrizione = string.Empty;
            Pattern = string.Empty;
            Sostituzione = string.Empty;
            StimaProfonditaMedia = 0.0; // Valore di default

            // NOTA: _regex non può essere inizializzato qui se 'Pattern' non è ancora impostato.
            // La gestione di _regex deve avvenire in TryApply o in un metodo separato dopo il caricamento.
            // Vedi la modifica qui sotto per TryApply.
        }
        /// <summary>
        /// Tenta di applicare la regola a una data stringa.
        /// </summary>
        /// <param name="inputString">La stringa MIU a cui tentare di applicare la regola.</param>
        /// <param name="outputString">La stringa risultante dopo l'applicazione della regola, se applicata con successo.</param>
        /// <returns>True se la regola è stata applicata con successo, altrimenti false.</returns>
        public bool TryApply(string inputString, out string outputString)
        {
            if (_regex == null && !string.IsNullOrEmpty(Pattern))
            {
                // Questo crea una nuova istanza Regex. Se la classe è IMMUTABILE (tranne StimaProfonditaMedia),
                // sarebbe meglio assicurarsi che _regex venga inizializzato una sola volta al caricamento.
                // Per un oggetto con proprietà { get; } caricate da DB, l'ORM potrebbe chiamare un costruttore
                // privato o usare reflection per impostare i campi, e _regex dovrebbe essere gestito lì.
                // Per ora, questa logica è un fallback.
                // Una soluzione più robusta potrebbe essere un campo Lazy<Regex> o un metodo Init() chiamato dall'ORM.
                // Data la struttura get-only, l'ORM IDEALMENTE dovrebbe chiamare il costruttore parametrico.
                outputString = inputString;
                return false; // Se Pattern è nullo o vuoto, non possiamo applicare.
            }
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
