using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MIU.Core
{
    public class MIUStringData
    {
        public string CurrentString { get; set; }
        public int CurrentStringLen { get; set; }
        public long DiscoveryTime_Int { get; set; }
        public string DiscoveryTime_Text { get; set; }
        public string Hash { get; set; }
    }
    public class MIUStringGenerator
    {
        public event EventHandler<GenerationProgressEventArgs> GenerationProgressChanged;

        private int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0) return m;
            if (m == 0) return n;

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                // Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        private char GetRandomMIUChar(Random random)
        {
            int r = random.Next(3);
            switch (r)
            {
                case 0: return 'M';
                case 1: return 'I';
                case 2: return 'U';
                default: return 'I'; // Should never happen
            }
        }

        string GetHash(string input)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
        public HashSet<string> GenerateChaoticStrings(int numberOfStrings, int minLength, int maxLength, int minLevenshtein)
        {
            HashSet<string> chaoticStrings = new HashSet<string>(); // Ora conterrà solo le stringhe MIU
            Random random = new Random();
#if DEBUG
            Console.WriteLine($"Inizio generazione di {numberOfStrings} stringhe caotiche...");
#endif
            while (chaoticStrings.Count < numberOfStrings)
            {
                // 1. Genera una stringa casuale
                int length = random.Next(minLength, maxLength + 1);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < length; i++)
                {
                    sb.Append(GetRandomMIUChar(random));
                }
                string newString = sb.ToString();

                // 2. Verifica l'unicità e aggiungi all'HashSet (ora di stringhe MIU)
                if (!chaoticStrings.Contains(newString))
                {
                    bool farEnough = true;
                    if (minLevenshtein > 0)
                    {
                        foreach (string existingString in chaoticStrings)
                        {
                            if (LevenshteinDistance(newString, existingString) < minLevenshtein)
                            {
                                farEnough = false;
                                break;
                            }
                        }
                    }

                    // 3. Aggiungi se sufficientemente distante
                    if (farEnough)
                    {
                        chaoticStrings.Add(newString); // Aggiungiamo la stringa MIU all'HashSet
                        GenerationProgressChanged?.Invoke(this, new GenerationProgressEventArgs(chaoticStrings.Count, numberOfStrings));
                        //Console.WriteLine($"Generata stringa caotica ({chaoticStrings.Count}/{numberOfStrings}): {newString}");
                    }
                    else
                    {
                        // Console.WriteLine($"Stringa '{newString}' troppo simile.");
                    }
                }
                else
                {
                    // Console.WriteLine($"Stringa '{newString}' già presente.");
                }

                // Aggiungiamo una sicurezza per evitare loop infiniti se non riusciamo a trovare stringhe adatte
                if (chaoticStrings.Count > 0 && chaoticStrings.Count % 100 == 0 && chaoticStrings.Count < numberOfStrings * 0.1)
                {
                    Console.WriteLine($"Avviso: Generazione lenta. Trovate {chaoticStrings.Count} stringhe su {numberOfStrings}.");
                }
            }
#if DEBUG
            Console.WriteLine($"Generazione di stringhe caotiche completata. Trovate {chaoticStrings.Count} stringhe.");
#endif

            // Ora creiamo la lista di stringhe XML da questo HashSet di stringhe MIU
            HashSet<string> xmlStringsToSave = new HashSet<string>();
            DateTimeOffset now = DateTimeOffset.UtcNow;
            foreach (string miuString in chaoticStrings)
            {
                XElement MIUstringDef = new XElement("MIUStringDef",
                    new XElement("CurrentString", miuString),
                    new XElement("CurrentStringLen", miuString.Length),
                    new XElement("DiscoveryTime_Int", now.ToUnixTimeSeconds()),
                    new XElement("DiscoveryTime_Text", DateTime.UtcNow.ToString("yyyy/MM/dd hh:mm:ss")),
                    new XElement("Hash", GetHash(miuString)));
                xmlStringsToSave.Add(MIUstringDef.ToString());
                now = now.AddSeconds(1); // Piccolo offset per tempi diversi (opzionale)
            }

            return xmlStringsToSave;
        }
        /*
        public HashSet<string> GenerateChaoticStrings(int numberOfStrings, int minLength, int maxLength, int minLevenshtein)
        {
            HashSet<string> chaoticStrings = new HashSet<string>();
            Random random = new Random();
#if DEBUG
            Console.WriteLine($"Inizio generazione di {numberOfStrings} stringhe caotiche...");
#endif
            while (chaoticStrings.Count < numberOfStrings)
            {
                // 1. Genera una stringa casuale
                int length = random.Next(minLength, maxLength + 1);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < length; i++)
                {
                    sb.Append(GetRandomMIUChar(random));
                }
                string newString = sb.ToString();

                // 2. Verifica l'unicità (HashSet lo fa automaticamente con Add)
                if (!chaoticStrings.Contains(newString))
                {
                    bool farEnough = true;
                    if (minLevenshtein > 0)
                    {
                        foreach (string existingString in chaoticStrings)
                        {
                            if (LevenshteinDistance(newString, existingString) < minLevenshtein)
                            {
                                farEnough = false;
                                break;
                            }
                        }
                    }

                    // 3. Aggiungi se sufficientemente distante
                    if (farEnough)
                    {
                        DateTimeOffset myDate = (DateTimeOffset)DateTime.UtcNow;
                        XElement MIUstringDef = new XElement("MIUStringDef",
                            new XElement("CurrentString", newString),
                            new XElement("CurrentStringLen", newString.ToString().Length),
                            new XElement("DiscoveryTime_Int", myDate.ToUnixTimeSeconds()),
                            new XElement("DiscoveryTime_Text", DateTime.UtcNow.ToString("yyyy/MM/dd hh:mm:ss")),
                            new XElement("Hash", GetHash(newString)));
                        chaoticStrings.Add(MIUstringDef.ToString());
                        GenerationProgressChanged?.Invoke(this, new GenerationProgressEventArgs(chaoticStrings.Count, numberOfStrings));
                        //Console.WriteLine($"Generata stringa caotica ({chaoticStrings.Count}/{numberOfStrings}): {newString}");
                    }
                    else
                    {
                        // Console.WriteLine($"Stringa '{newString}' troppo simile.");
                    }
                }
                else
                {
                    // Console.WriteLine($"Stringa '{newString}' già presente.");
                }

                // Aggiungiamo una sicurezza per evitare loop infiniti se non riusciamo a trovare stringhe adatte
                if (chaoticStrings.Count > 0 && chaoticStrings.Count % 100 == 0 && chaoticStrings.Count < numberOfStrings * 0.1)
                {
                    Console.WriteLine($"Avviso: Generazione lenta. Trovate {chaoticStrings.Count} stringhe su {numberOfStrings}.");
                }
            }
#if DEBUG
             Console.WriteLine($"Generazione di stringhe caotiche completata. Trovate {chaoticStrings.Count} stringhe.");
#endif
            return chaoticStrings;
        }
        */
    }

    public class GenerationProgressEventArgs : EventArgs
    {
        public int Current { get; }
        public int Total { get; }

        public GenerationProgressEventArgs(int current, int total)
        {
            Current = current;
            Total = total;
        }
    }
}
