using EvolutiveSystem.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core.tester
{
    internal class Program
    {
        private const long passi = 1000000000;
        static void Main(string[] args)
        {
            //string[,] arrayString =
            //{
            //    {"MI", "MI", "MI", "MI", "MI", "MI", "MIIIIII","MII","MUI", "MI"},
            //    {"MIU","MII","MIIII","MUI","MUIU","MUIIU", "MUU", "MU","MIU","MIIIIIIIII"}
            //};
            string[,] arrayString =
            {
                {"M", "MI", "MIU"},
                {"MMMMMMMMMM","MIIIIIIIIII","MUIUIUIU"}
            };
            string databaseFilePath = @"C:\Progetti\EvolutiveSystem\xml\MIUProject.xml";
            int tipotest = 4;
            switch (tipotest)
            {
                case 1:
                    {
                        loadDatabase();
                    }
                    break;
                case 2:
                    {
                        ApplicazioneRegoleMIU();
                    }
                    break;
                case 3:
                    {
                        Database mioDatabase = null;
                        Console.Write("Caricare il database esistente da disco? (s/n): ");
                        string loadChoice = Console.ReadLine()?.ToLower();
                        if (loadChoice == "s")
                        {
                            Console.WriteLine($"Tentativo di caricare il database da: {databaseFilePath}");
                            mioDatabase = DatabaseSerializer.DeserializeFromXmlFile(databaseFilePath);
                            if (mioDatabase != null && mioDatabase.Tables.Any(t => t.TableName == "RegoleMIU"))
                            {
                                RegoleMIUManager.CaricaRegoleDaOggettoDatabase(mioDatabase);
                                Console.WriteLine("Regole MIU caricate dal database.");
                            }
                            else
                            {
                                mioDatabase = new Database(1, "MIUDatabase"); // Crea un nuovo database se il caricamento fallisce o non ci sono regole
                                Console.WriteLine("Nessun database valido trovato. Creato un nuovo database.");
                            }
                        }
                        else
                        {
                            // Se non si carica da disco, crea un nuovo database
                            mioDatabase = new Database(1, "MIUDatabase");
                            Console.WriteLine("Creato un nuovo database.");
                        }
                        bool bUscita = false;
                        while (!bUscita)
                        {
                            Console.WriteLine($"Numero passi: {passi}");
                            Console.WriteLine("Stringa di inizio");
                            string sInizio = Console.ReadLine();
                            Console.WriteLine("Stringa di fine");
                            string sfine = Console.ReadLine();
                            //int passi = 0;
                            //int.TryParse(Console.ReadLine(), out passi);
                            RicercaDiDerivazioneBFS(sInizio, sfine, passi, mioDatabase);
                            Console.WriteLine("Vuoi continuare (S/N)");
                            if (Console.ReadLine().ToUpper() == "N") bUscita=true;
                        }
                        Console.Write("\nSalvare il database su disco? (s/n): ");
                        string saveChoice = Console.ReadLine()?.ToLower();

                        if (saveChoice == "s")
                        {
                            Console.WriteLine($"Tentativo di salvare il database in: {mioDatabase}");
                            DatabaseSerializer.SerializeToXmlFile(mioDatabase, databaseFilePath);
                            Console.WriteLine("Database salvato su disco.");
                        }
                        else
                        {
                            Console.WriteLine("Il database non è stato salvato su disco.");
                        }
                    }
                    break;
                case 4:
                    {
                        Process myProc = new Process();
                        Console.WriteLine($"Process: {Process.GetCurrentProcess().Id} Premi un tastro per cominciare");
                        Console.ReadKey();
                        Database mioDatabase = mioDatabase = DatabaseSerializer.DeserializeFromXmlFile(databaseFilePath);
                        int cntDwn = 22;
                        while (cntDwn >= 1)
                        {
                            for (int y = 0; y < 3; y++) 
                            {
                                //for (int x = 0; x < 2; x++)
                                //{
                                //    Console.Write("[{0}]",arrayString[x,y]);
                                //}
                                //Console.WriteLine();
                                RicercaDiDerivazioneBFS(arrayString[0, y], arrayString[1, y], passi, mioDatabase);
                            }
                            cntDwn--;
                        }
                        DatabaseSerializer.SerializeToXmlFile(mioDatabase, databaseFilePath);
                        Console.WriteLine("premi un tatsto");
                        Console.ReadKey();
                    }
                    break;
            }
        }
        private static void RicercaDiDerivazioneBFS(string inizio, string fine, long passi, Database mioDatabase)
        {
            string miuProjectXmlPath = (Path.Combine(@"C:\Progetti\EvolutiveSystem\xml\", "MIUProject.xml"));
            MiuRulesLoader loader = new MiuRulesLoader();

            // Tenta di caricare le regole
            bool success = loader.LoadMiuRulesFromFile(miuProjectXmlPath);

            if (success)
            {
                List<string> miu = RegoleMIUManager.TrovaDerivazioneBFS(inizio, fine, passi, mioDatabase);
                if (miu != null)
                {
                    foreach (string item in miu)
                    {
                        Console.WriteLine(item);
                    }
                }
                else
                    Console.WriteLine("FAIL!");
            }
        } 
        private static void ApplicazioneRegoleMIU()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; // Per visualizzare correttamente i caratteri speciali

            // 1. Creazione di un database mock
            EvolutiveSystem.Core.Database mockDatabase = new EvolutiveSystem.Core.Database();

            // 2. Creazione della tabella "RegoleMIU"
            EvolutiveSystem.Core.Table regoleTable = new EvolutiveSystem.Core.Table { TableName = "RegoleMIU" };

            // 3. Aggiunta di record di regole alla tabella
            // Regola 1: M -> MU (se la stringa termina con M)
            regoleTable.DataRecords.Add(new EvolutiveSystem.Core.SerializableDictionary<string, object>
        {
            { "ID", "R1" },
            { "Nome", "Regola 1 (M->MU)" },
            { "Descrizione", "Aggiunge una U alla fine di una stringa che termina con M." },
            { "Pattern", "M$" }, // $ indica la fine della stringa
            { "Sostituzione", "MU" }
        });

            // Regola 2: IU -> U (se IU è presente)
            regoleTable.DataRecords.Add(new EvolutiveSystem.Core.SerializableDictionary<string, object>
        {
            { "ID", "R2" },
            { "Nome", "Regola 2 (IU->U)" },
            { "Descrizione", "Sostituisce IU con U." },
            { "Pattern", "IU" },
            { "Sostituzione", "U" }
        });

            // Regola 3: UU -> (vuoto) (se UU è presente)
            regoleTable.DataRecords.Add(new EvolutiveSystem.Core.SerializableDictionary<string, object>
        {
            { "ID", "R3" },
            { "Nome", "Regola 3 (UU->)" },
            { "Descrizione", "Rimuove le occorrenze di UU." },
            { "Pattern", "UU" },
            { "Sostituzione", "" } // Sostituzione con stringa vuota per rimuovere
        });

            // Regola 4: Qualsiasi stringa che inizia con 'I' -> 'MI'
            regoleTable.DataRecords.Add(new EvolutiveSystem.Core.SerializableDictionary<string, object>
        {
            { "ID", "R4" },
            { "Nome", "Regola 4 (I...->MI)" },
            { "Descrizione", "Aggiunge M all'inizio di una stringa che inizia con I." },
            { "Pattern", "^I" }, // ^ indica l'inizio della stringa
            { "Sostituzione", "MI" }
        });

            // Regola 5: Sostituisce 'X' con 'Y' (esempio di regola che non si applica sempre)
            regoleTable.DataRecords.Add(new EvolutiveSystem.Core.SerializableDictionary<string, object>
        {
            { "ID", "R5" },
            { "Nome", "Regola 5 (X->Y)" },
            { "Descrizione", "Sostituisce la lettera X con Y." },
            { "Pattern", "X" },
            { "Sostituzione", "Y" }
        });

            mockDatabase.Tables.Add(regoleTable);

            // 4. Caricamento delle regole tramite RegoleMIUManager
            Console.WriteLine("--- Caricamento delle regole MIU ---");
            MIU.Core.RegoleMIUManager.CaricaRegoleDaOggettoDatabase(mockDatabase);
            Console.WriteLine("------------------------------------");

            // 5. Test di applicazione delle regole
            Console.WriteLine("\n--- Test di applicazione delle regole ---");

            // Test Case 1: Stringa semplice che dovrebbe attivare R1
            string test1 = "MI";
            MIU.Core.RegoleMIUManager.ApplicaRegole(test1);
            Console.WriteLine("\n------------------------------------");

            // Test Case 2: Stringa che dovrebbe attivare R2 e poi R3
            string test2 = "MIIUUI";
            MIU.Core.RegoleMIUManager.ApplicaRegole(test2);
            Console.WriteLine("\n------------------------------------");

            // Test Case 3: Stringa che inizia con I e dovrebbe attivare R4
            string test3 = "I";
            MIU.Core.RegoleMIUManager.ApplicaRegole(test3);
            Console.WriteLine("\n------------------------------------");

            // Test Case 4: Stringa senza corrispondenze iniziali
            string test4 = "ABCM";
            MIU.Core.RegoleMIUManager.ApplicaRegole(test4);
            Console.WriteLine("\n------------------------------------");

            // Test Case 5: Stringa con pattern che non si applica
            string test5 = "MIU";
            MIU.Core.RegoleMIUManager.ApplicaRegole(test5);
            Console.WriteLine("\n------------------------------------");

            // Test Case 6: Stringa con una X
            string test6 = "MIX";
            MIU.Core.RegoleMIUManager.ApplicaRegole(test6);
            Console.WriteLine("\n------------------------------------");
        }
        private static void loadDatabase()
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
        }
    }
}
