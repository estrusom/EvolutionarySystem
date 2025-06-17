using System;
using System.Collections.Generic;
using System.Linq; // Non strettamente necessario per questo codice, ma spesso utile

namespace MIU.Core
{
    public static class RicercaMIU
    {
        /// <summary>
        /// Gestisce l'esecuzione di un algoritmo di ricerca (es. DFS) per trovare un percorso dalla stringa di partenza alla stringa di destinazione
        /// applicando le regole del sistema MIU. Include la logica di gestione dello stato della ricerca.
        /// </summary>
        /// <param name="startString">La stringa di partenza (assioma 'MI').</param>
        /// <param name="targetString">La stringa di destinazione da raggiungere.</param>
        /// <param name="regole">La lista delle regole MIU disponibili.</param>
        /// <returns>Un oggetto RisultatoRicerca contenente il successo e il percorso trovato.</returns>
        public static RisultatoRicerca GestioneRicerca(string startString, string targetString, List<RegolaMIU> regole)
        {
            // Inizializza lo stato della ricerca
            var visited = new HashSet<string>(); // Per tenere traccia delle stringhe già visitate ed evitare cicli infiniti
            var path = new List<string>(); // Il percorso dalla stringa iniziale a quella corrente

            // Utilizziamo uno stack per l'implementazione DFS (LIFO)
            var stack = new Stack<(string currentString, List<string> currentPath)>();

            // Aggiungi la stringa di partenza allo stack
            stack.Push((startString, new List<string> { startString }));
            visited.Add(startString);

            Console.WriteLine($"RicercaMIU: Avvio gestione ricerca da '{startString}' a '{targetString}'.");

            while (stack.Count > 0)
            {
                var (current, currentPath) = stack.Pop();
                Console.WriteLine($"RicercaMIU: Esamino la stringa corrente: '{current}'");

                // Controlla se abbiamo raggiunto la stringa di destinazione
                if (current == targetString)
                {
                    Console.WriteLine($"RicercaMIU: Soluzione trovata! Percorso: {string.Join(" -> ", currentPath)}");
                    return new RisultatoRicerca
                    {
                        Successo = true,
                        Percorso = currentPath
                    };
                }

                // Applica ogni regola possibile alla stringa corrente
                foreach (var regola in regole)
                {
                    // Chiamata al metodo ApplicaRegola per generare nuove stringhe
                    var newStrings = ApplicaRegola(current, regola);
                    foreach (var newString in newStrings)
                    {
                        if (!visited.Contains(newString))
                        {
                            visited.Add(newString);
                            var newPath = new List<string>(currentPath) { newString };
                            stack.Push((newString, newPath));
                            Console.WriteLine($"RicercaMIU: Generata nuova stringa '{newString}' applicando la regola {regola.Nome} (ID: {regola.ID}).");
                        }
                    }
                }
            }

            // Se lo stack è vuoto e la destinazione non è stata raggiunta
            Console.WriteLine($"RicercaMIU: Nessuna soluzione trovata da '{startString}' a '{targetString}'.");
            return new RisultatoRicerca
            {
                Successo = false,
                Percorso = new List<string>() // Percorso vuoto o parziale se non trovata soluzione
            };
        }

        /// <summary>
        /// Metodo per applicare una specifica regola MIU a una stringa di input.
        /// Questo è il punto cruciale dove devi implementare la logica per ciascuna delle tue 4 regole MIU.
        /// </summary>
        /// <param name="inputString">La stringa a cui applicare la regola.</param>
        /// <param name="regola">L'oggetto RegolaMIU che descrive la regola da applicare.</param>
        /// <returns>Una lista di nuove stringhe generate dall'applicazione della regola. Potrebbe essere vuota se la regola non si applica.</returns>
        private static List<string> ApplicaRegola(string inputString, RegolaMIU regola)
        {
            var results = new List<string>();

            // *** QUI DEVI IMPLEMENTARE LA LOGICA PER LE TUE QUATTRO REGOLE MIU ***
            // Esempio:
            // switch (regola.Id)
            // {
            //     case 1:
            //         // Logica per la Regola 1: es. "Se la stringa finisce con I, aggiungi U alla fine"
            //         if (inputString.EndsWith("I"))
            //         {
            //             results.Add(inputString + "U");
            //         }
            //         break;
            //     case 2:
            //         // Logica per la Regola 2: es. "Se hai Mx, aggiungi Mxx" (dove x è un blocco di I o U)
            //         // Questa è più complessa e richiede espressioni regolari o manipolazione stringhe avanzata
            //         break;
            //     // ... e così via per le altre regole
            // }

            // Per ora, questo metodo restituisce sempre una lista vuota,
            // quindi la ricerca non genererà nuove stringhe e fallirà.
            // La prossima priorità è implementare la logica delle regole qui!

            return results;
        }
    }
}
