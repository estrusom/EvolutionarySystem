using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{
    /// <summary>
    /// Classe che incapsula i risultati di un'operazione di ricerca nel sistema MIU.
    /// Contiene un flag per indicare il successo della ricerca e il percorso di stringhe generato.
    /// </summary>
    public class RisultatoRicerca
    {
        /// <summary>
        /// Ottiene o imposta un valore che indica se la ricerca ha avuto successo nel trovare la stringa di destinazione.
        /// </summary>
        public bool Successo { get; set; }

        /// <summary>
        /// Ottiene o imposta la lista delle stringhe che formano il percorso dalla stringa iniziale
        /// alla stringa di destinazione (se la ricerca ha avuto successo).
        /// </summary>
        public List<string> Percorso { get; set; } = new List<string>();

        // Puoi aggiungere altre proprietà qui se in futuro volessi includere
        // dettagli come il numero di nodi visitati, il tempo impiegato, ecc.
    }
}
