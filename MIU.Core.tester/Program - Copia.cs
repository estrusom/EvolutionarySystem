using EvolutiveSystem.Core;
using EvolutiveSystem.SQL.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MIU.Core.tester
{
    internal class Program
    {
        private const long passi = 10;
        static void Main(string[] args)
        {
            string[,] arrayString =
            {
                {"MI", "MI", "MI", "MI", "MI", "MI", "MIIIIII","MII","MUI", "MI"},
                {"MIU","MII","MIIII","MUI","MUIU","MUIIU", "MUU", "MU","MIU","MIIIIIIIII"}
            };

            //string[,] arrayString =
            //{
            //    {"MI", "MI", "MI", "MII", "MII", "MUI", "MUI", "MI", "MIIII", "MIIII"},
            //    {"MIIU", "MIIIIU", "MUIU", "MIIIIIIII", "MUIUI", "MIUIU", "MUU", "MIIIIUU", "MUIIU", "MIIIIIIIIU"}
            //};

            //string[,] arrayString =
            //{
            //    {"M", "MI", "MIU"},
            //    {"MMMMMMMMMM","MIIIIIIIIII","MUIUIUIU"}
            //};

            //string[,] arrayString =
            //{
            //    {"MI"},
            //    {"MIIU"}
            //};

            string databaseFilePath = @"C:\Progetti\EvolutiveSystem\xml\MIUProject.xml";
            //Console.WriteLine($"Process: {Process.GetCurrentProcess().Id}");
            //Console.WriteLine("Test nr. (1÷5)");
            int tipotest = 4;// Convert.ToInt32(Console.ReadLine());
            Process myProc = new Process();
            switch (tipotest)
            {
                case 2:
                    {
                        ApplicazioneRegoleMIU();
                    }
                    break;
                case 3:
                    {
                        Random rnd = new Random();
                        databaseFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";
                        SQLiteSchemaLoader _schemaLoader = new SQLiteSchemaLoader(databaseFilePath);
                        List<string> regole = _schemaLoader.SQLiteSelect("SELECT ID, Nome, Pattern, Sostituzione, Descrizione FROM RegoleMIU;");
                        RegoleMIUManager.CaricaRegoleDaOggettoSQLite(regole);
                        RegoleMIUManager.OnRuleApplied += RegoleMIUManager_OnRuleApplied;
                        RegoleMIUManager.OnSolutionFound += RegoleMIUManager_OnSolutionFound;
                        List<string> MIUstringList = _schemaLoader.SQLiteSelect("SELECT StateID, CurrentString, StringLength, Hash, DiscoveryTime_Int, DiscoveryTime_Text, UsageCount FROM MIU_States;");
                        /*
                        int index = 0;
                        foreach (string s in MIUstringList)
                        {
                            index++;
                            string[] sArray = s.Split(';');
                            string deflateString = MIUStringConverter.DeflateMIUString(sArray[1]);
                            //string sqlUpdate = $"UPDATE MIU_States SET CurrentString = '{deflateString}', StringLength = '{deflateString.Length}', DeflateString='{sArray[1]}' WHERE StateID = '{index}' ";
                            //int i = _schemaLoader.SQLiteUpdate(sqlUpdate);
                            Console.WriteLine($"Source: {sArray[1]} target: {MIUStringConverter.DeflateMIUString(sArray[1])}");
                        }
                        */
                        if (RegoleMIUManager.Regole.Count > 0)
                        {
                            foreach (string s in MIUstringList)
                            {
                                string[] MIUstringsSource = s.Split(';');
                                int index = rnd.Next(0, MIUstringsSource.Length - 1);
                                string[] MIUstringDestination = MIUstringList[index].Split(';');
                                List<(string CompressedString, int? AppliedRuleID)> miu = RegoleMIUManager.TrovaDerivazioneBFS(MIUstringsSource[1], MIUstringDestination[1], passi);
                            }
                        }
                    }
                    break;
                case 4:
                    {
                        databaseFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";
                        SQLiteSchemaLoader _schemaLoader = new SQLiteSchemaLoader(databaseFilePath);
                        List<string> regole = _schemaLoader.SQLiteSelect("SELECT ID, Nome, Pattern, Sostituzione, Descrizione FROM RegoleMIU;");
                        RegoleMIUManager.CaricaRegoleDaOggettoSQLite(regole);
                        string StringIn = "M2U4MI";//"M2UM"; 
                        bool response = RegoleMIUManager.Regole[0].TryApply(StringIn, out string regola1);
                        Console.WriteLine($"String in: {StringIn} Regola 1: {regola1} response: {response}");
                        StringIn = "M2U";//"3IU"; 
                        response = RegoleMIUManager.Regole[1].TryApply(StringIn, out string regola2);
                        Console.WriteLine($"String in: {StringIn} Regola 2: {regola2} response: {response}");
                        StringIn = "M3IU3I2U";//"M2U4MI"; 
                        response = RegoleMIUManager.Regole[2].TryApply(StringIn, out string regola3);
                        Console.WriteLine($"String in: {StringIn} Regola 3: {regola3} response: {response}");
                        StringIn = "M3IU3I2U";//"3MIU3I"; 
                        response = RegoleMIUManager.Regole[3].TryApply(StringIn, out string regola4);
                        Console.WriteLine($"String in: {StringIn} Regola 4: {regola4} response: {response}");
                    }
                    break;
                case 5:
                    {

                        long maxProfondita = 10; // Imposta una profondità massima ragionevole
                        int cntDwn = 22;
                        while (cntDwn >= 1)
                        {
                            for (int y = 0; y < arrayString.GetLength(1); y++)
                            {
                                RicercaDiDerivazioneDFS(arrayString[0, y], arrayString[1, y], maxProfondita);
                            }
                            cntDwn--;
                        }

                    }
                    break;
                case 6:
                    {
                        Random r = new Random();
                        for (int i= 0; i < 10; i++)
                        {
                            Console.Write(r.Next(1, 3));
                            Console.WriteLine("Hit any key");
                            Console.ReadKey();
                        }
                        

                    }
                    break;
            }
            Console.WriteLine("premi un tasto");
            Console.ReadKey();
        }

        private static void RegoleMIUManager_OnSolutionFound(object sender, SolutionFoundEventArgs e)
        {
            Console.WriteLine($"ElapsedMilliseconds: {e.ElapsedMilliseconds} ElapsedTicks: {e.ElapsedTicks} InitialString: {e.InitialString} MaxDepthReached: {e.MaxDepthReached} NodesExplored: {e.NodesExplored} Path: {e.Path} {e.StepsTaken} Success: {e.Success} TargetString: {e.TargetString}");
        }

        private static void RegoleMIUManager_OnRuleApplied(object sender, RuleAppliedEventArgs e)
        {
            Console.WriteLine($"AppliedRuleID: {e.AppliedRuleID} AppliedRuleName: {e.AppliedRuleName} CurrentDepth: {e.CurrentDepth}  ");
        }

        private static void RicercaDiDerivazioneDFS(string startString, string targetString, long maxProfondita)
        {

            List<(string CompressedString, int? AppliedRuleID)> percorsoDFS = RegoleMIUManager.TrovaDerivazioneDFS(startString, targetString, maxProfondita);

            if (percorsoDFS != null)
            {
                Console.WriteLine("Percorso DFS trovato:");
                foreach (var s in percorsoDFS)
                {
                    Console.WriteLine(s);
                }
            }
            else
            {
                Console.WriteLine($"Nessuna derivazione trovata con DFS entro la profondità {maxProfondita}.");
            }
        }
        private static void RicercaDiDerivazioneBFS(string inizio, string fine, long passi)
        {
            //Console.WriteLine($"inizio: {inizio} fine: {fine} passi: {passi}");
            //List<(string CompressedString, int? AppliedRuleID)> miu = RegoleMIUManager.TrovaDerivazioneBFS(inizio, fine, passi);
            //if (miu != null)
            //{
            //    foreach (string item in miu)
            //    {
            //        Console.WriteLine(item);
            //    }
            //}
            //else
            //    Console.WriteLine("FAIL!");
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
            //MIU.Core.RegoleMIUManager.CaricaRegoleDaOggettoDatabase(mockDatabase);
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
        //private static void loadDatabase()
        //{
        //    string miuProjectXmlPath = (Path.Combine(@"C:\Progetti\EvolutiveSystem\xml\", "MIUProject.xml"));
        //    MiuRulesLoader loader = new MiuRulesLoader();

        //    // Tenta di caricare le regole
        //    bool success = loader.LoadMiuRulesFromFile(miuProjectXmlPath);

        //    if (success)
        //    {
        //        Console.WriteLine("\n--- Regole MIU Caricate ---");
        //        foreach (var rule in RegoleMIUManager.Regole)
        //        {
        //            Console.WriteLine($"ID: {rule.Id}, Nome: {rule.Nome}, Pattern: {rule.Pattern}");
        //            // Puoi stampare tutti i dettagli che desideri
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("\nErrore: Impossibile caricare le regole MIU.");
        //    }
        //}
    }
}
