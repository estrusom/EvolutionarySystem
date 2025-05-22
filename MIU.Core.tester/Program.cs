using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core.tester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string miuProjectXmlPath = (Path.Combine(@"C:\Progetti\EvolutiveSystem\xml\", "MIUProject.xml"));
            MiuRulesLoader loader = new MiuRulesLoader();

            // Tenta di caricare le regole
            bool success = loader.LoadMiuRulesFromFile(miuProjectXmlPath);

            if (success)
            {
                Console.WriteLine("\n--- Regole MIU Caricate ---");
                foreach (var rule in RegoleMIUManager.Regole)
                {
                    Console.WriteLine($"ID: {rule.Id}, Nome: {rule.Nome}, Pattern: {rule.Pattern}");
                    // Puoi stampare tutti i dettagli che desideri
                }
            }
            else
            {
                Console.WriteLine("\nErrore: Impossibile caricare le regole MIU.");
            }

            Console.WriteLine("\nPremi un tasto per uscire...");
            Console.ReadKey();
        }
    }
}
