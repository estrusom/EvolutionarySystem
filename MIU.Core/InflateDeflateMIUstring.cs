using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MIU.Core
{
    /// <summary>
    /// Fornisce metodi statici per la compressione e decompressione delle stringhe MIU
    /// secondo la notazione "conteggio-lettera" (es. MIIU -> M2IU).
    /// </summary>
    public static class MIUStringConverter
    {
        /// <summary>
        /// Comprime una stringa MIU standard (es. "MIIU") nella notazione "conteggio-lettera" (es. "M2IU").
        /// La regola è: il numero che indica la ripetizione precede la lettera.
        /// Il numero '1' è omesso se è seguito da un'altra lettera.
        /// </summary>
        /// <param name="input">La stringa MIU standard da comprimere.</param>
        /// <returns>La stringa MIU compressa.</returns>
        public static string InflateMIUString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            StringBuilder compressedString = new StringBuilder();
            char? currentChar = null;
            int count = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char letter = input[i];

                // Se è la prima lettera o la lettera corrente è diversa dalla precedente
                if (currentChar == null || letter != currentChar)
                {
                    // Se non è la prima iterazione, aggiungi il conteggio e la lettera precedente
                    if (currentChar != null)
                    {
                        // Se il conteggio è 1, lo omettiamo (es. 1M -> M)
                        if (count > 1)
                        {
                            compressedString.Append(count);
                        }
                        compressedString.Append(currentChar);
                    }

                    // Inizia il conteggio per la nuova lettera
                    currentChar = letter;
                    count = 1;
                }
                else
                {
                    // La lettera è la stessa, incrementa il conteggio
                    count++;
                }
            }

            // Aggiungi l'ultimo blocco di conteggio e lettera dopo il ciclo
            if (currentChar != null)
            {
                if (count > 1)
                {
                    compressedString.Append(count);
                }
                compressedString.Append(currentChar);
            }

            return compressedString.ToString();
        }

        /// <summary>
        /// Decomprime una stringa MIU nella notazione "conteggio-lettera" (es. "M2IU")
        /// in una stringa MIU standard (es. "MIIU").
        /// </summary>
        /// <param name="compressedInput">La stringa MIU compressa da decomprimere.</param>
        /// <returns>La stringa MIU standard decompressa.</returns>
        public static string DeflateMIUString(string compressedInput)
        {
            if (string.IsNullOrEmpty(compressedInput))
            {
                return string.Empty;
            }

            StringBuilder decompressedString = new StringBuilder();
            // Regex per trovare blocchi di (numero opzionale)(lettera)
            // Esempio: "2I" -> count=2, letter='I'
            // "M" -> count=1, letter='M' (se il numero è omesso, si assume 1)
            MatchCollection matches = Regex.Matches(compressedInput, @"(\d*)?([MIU])");

            foreach (Match match in matches)
            {
                string countStr = match.Groups[1].Value; // Il numero (es. "2" o "")
                char letter = match.Groups[2].Value[0]; // La lettera (es. 'I' o 'M')

                int count = 1; // Default a 1 se il numero è omesso
                if (!string.IsNullOrEmpty(countStr) && int.TryParse(countStr, out int parsedCount))
                {
                    count = parsedCount;
                }

                // Appendi la lettera ripetuta 'count' volte
                for (int i = 0; i < count; i++)
                {
                    decompressedString.Append(letter);
                }
            }

            return decompressedString.ToString();
        }
    }
}
