// File: C:\Progetti\EvolutiveSystem\MIU.Core\MIUStringHelper.cs
// Data di riferimento: 20 giugno 2025
// Descrizione: Classe di utilità per operazioni comuni sulle stringhe MIU.

using System;
using System.Linq; // Necessario per Count()

namespace MIU.Core
{
    /// <summary>
    /// Fornisce metodi helper per analizzare le caratteristiche delle stringhe MIU.
    /// </summary>
    public static class MIUStringHelper
    {
        /// <summary>
        /// Conta il numero di occorrenze del carattere 'I' in una stringa MIU.
        /// </summary>
        /// <param name="miuString">La stringa MIU da analizzare.</param>
        /// <returns>Il conteggio del carattere 'I'.</returns>
        public static int CountI(string miuString)
        {
            if (string.IsNullOrEmpty(miuString))
            {
                return 0;
            }
            return miuString.Count(c => c == 'I');
        }

        /// <summary>
        /// Conta il numero di occorrenze del carattere 'U' in una stringa MIU.
        /// </summary>
        /// <param name="miuString">La stringa MIU da analizzare.</param>
        /// <returns>Il conteggio del carattere 'U'.</returns>
        public static int CountU(string miuString)
        {
            if (string.IsNullOrEmpty(miuString))
            {
                return 0;
            }
            return miuString.Count(c => c == 'U');
        }

        // Potresti aggiungere altri metodi utili qui, come CountM, o per la validazione di stringhe MIU.
    }
}
