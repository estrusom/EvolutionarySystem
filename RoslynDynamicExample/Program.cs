using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace RoslynDynamicExample
{
    public interface IComponenteDinamico
    {
        string Nome { get; }
        string ProcessaDato(string inputData);
        void inputKey(); // <-- NUOVO
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("--- Applicazione Principale (Interagisce con Componenti Dinamici) ---");

                string dynamicSourceCode = @"
                using System;
                using RoslynDynamicExample;

                namespace ComponentiGenerati
                {
                    public class MioComponenteSpeciale : IComponenteDinamico
                    {
                        public string Nome => ""Componente Speciale v1.0"";

                        public string ProcessaDato(string inputData)
                        {
                            return $""Dato processato da '{Nome}': {inputData.ToUpper()} (lunghezza: {inputData.Length})"";
                        }
                        public void inputKey()
                        {
                            Console.WriteLine(""Premere un tasto per continuare"");
                            Console.ReadKey();
                        }
                    }
                }";
                Console.WriteLine("\nCodice sorgente C# dinamico da compilare:");
                Console.WriteLine(dynamicSourceCode);

                IComponenteDinamico dynamicComponent = CompileAndLoadDynamicComponent(dynamicSourceCode);

                if (dynamicComponent != null)
                {
                    Console.WriteLine("\n--- Componente Dinamico Caricato con Successo ---");
                    Console.WriteLine($"Nome del Componente: {dynamicComponent.Nome}");

                    string testData = "testo di prova da processare";
                    string result = dynamicComponent.ProcessaDato(testData);
                    Console.WriteLine($"Risultato del processamento per '{testData}': {result}");

                    string anotherTestData = "hello world";
                    string anotherResult = dynamicComponent.ProcessaDato(anotherTestData);
                    Console.WriteLine($"Risultato del processamento per '{anotherTestData}': {anotherResult}");
                    dynamicComponent.inputKey();
                }
                else
                {
                    Console.WriteLine("\nErrore durante la compilazione o il caricamento del componente dinamico.");
                }

                Console.WriteLine("\n--- Fine Applicazione ---");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Si è verificato un errore: {ex.Message}");
                // Potresti voler stampare anche lo stack trace completo per maggiori dettagli:
                // Console.WriteLine(ex.StackTrace);
            }
            Console.ReadKey(false);
        }

        private static IComponenteDinamico CompileAndLoadDynamicComponent(string sourceCode)
        {
            // 1. Parsing del codice sorgente
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            // 2. Definizione dell'assembly dinamico
            string assemblyName = Guid.NewGuid().ToString() + ".dll";

            // 3. Riferimenti NECESSARI per la compilazione Roslyn:
            //    Ora otteniamo i riferimenti in un modo più robusto per .NET Core / .NET 5+
            var references = new List<MetadataReference>();

            // Aggiungi tutti gli assembly già caricati nel dominio dell'applicazione corrente.
            // Questo è spesso sufficiente per le dipendenze di base.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                    {
                        references.Add(MetadataReference.CreateFromFile(assembly.Location));
                    }
                }
                catch (NotSupportedException)
                {
                    // Alcuni assembly dinamici o generati a runtime possono causare questa eccezione. Ignorali.
                }
            }

            // Assicurati che l'assembly corrente (che contiene IComponenteDinamico) sia incluso.
            references.Add(MetadataReference.CreateFromFile(typeof(IComponenteDinamico).GetTypeInfo().Assembly.Location));

            // Per alcune dipendenze comuni che potrebbero non essere caricate, puoi aggiungerle esplicitamente:
            // Esempio: Se il codice dinamico usa System.Net.Http, potresti voler aggiungere:
            // references.Add(MetadataReference.CreateFromFile(typeof(System.Net.Http.HttpClient).GetTypeInfo().Assembly.Location));
            // references.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Private.CoreLib").Location)); // Spesso incluso automaticamente da CurrentDomain.GetAssemblies()

            // 4. Creazione dell'oggetto di compilazione
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                // 5. Esecuzione della compilazione
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    Console.WriteLine("\nErrore di compilazione del codice dinamico:");
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine($"\t{diagnostic.Id}: {diagnostic.GetMessage()}");
                    }
                    return null;
                }

                ms.Seek(0, SeekOrigin.Begin); // Resetta il MemoryStream

                // 6. Caricamento dell'assembly compilato in memoria
                Assembly assembly = Assembly.Load(ms.ToArray());

                // 7. Ricerca del tipo specifico nell'assembly caricato
                Type dynamicComponentType = assembly.GetType("ComponentiGenerati.MioComponenteSpeciale");

                if (dynamicComponentType != null)
                {
                    // 8. Creazione di un'istanza del tipo trovato
                    object instance = Activator.CreateInstance(dynamicComponentType);

                    // 9. CAST ALL'INTERFACCIA CONOSCIUTA!
                    return (IComponenteDinamico)instance;
                }
                else
                {
                    Console.WriteLine($"Errore: Tipo 'ComponentiGenerati.MioComponenteSpeciale' non trovato nell'assembly dinamico.");
                    return null;
                }
            }
        }
    }
}


//using System;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Collections.Generic; // Necessario per List
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.Emit;

//namespace RoslynDynamicExample
//{
//    // --- PARTE 1: L'INTERFACCIA CONOSCIUTA (nel tuo progetto principale) ---
//    // Questa interfaccia è definita nel tuo progetto principale (o in una libreria referenziata).
//    // È il "contratto" che sia il tuo codice principale che il codice dinamico conosceranno.
//    public interface IComponenteDinamico
//    {
//        string Nome { get; }
//        string ProcessaDato(string inputData);
//    }

//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            try
//            {
//                Console.WriteLine("--- Applicazione Principale (Interagisce con Componenti Dinamici) ---");

//                // --- PARTE 2: DEFINIZIONE DEL CODICE DINAMICO ---
//                // Supponiamo che questa stringa di codice provenga da una configurazione utente,
//                // un database, o un input generato da un'interfaccia utente.
//                // Nota che la classe implementa IComponenteDinamico.
//                string dynamicSourceCode = @"
//                using System;
//                using RoslynDynamicExample; // Namespace dell'interfaccia IComponenteDinamico

//                namespace ComponentiGenerati
//                {
//                    public class MioComponenteSpeciale : IComponenteDinamico
//                    {
//                        public string Nome => ""Componente Speciale v1.0"";

//                        public string ProcessaDato(string inputData)
//                        {
//                            // Logica di business dinamica
//                            return $""Dato processato da '{Nome}': {inputData.ToUpper()} (lunghezza: {inputData.Length})"";
//                        }
//                    }
//                }";

//                Console.WriteLine("\nCodice sorgente C# dinamico da compilare:");
//                Console.WriteLine(dynamicSourceCode);

//                // --- PARTE 3: COMPILAZIONE E CARICAMENTO CON ROSLYN ---
//                // Il processo di compilazione è incapsulato per chiarezza.
//                IComponenteDinamico dynamicComponent = CompileAndLoadDynamicComponent(dynamicSourceCode);

//                if (dynamicComponent != null)
//                {
//                    Console.WriteLine("\n--- Componente Dinamico Caricato con Successo ---");
//                    Console.WriteLine($"Nome del Componente: {dynamicComponent.Nome}");

//                    // --- PARTE 4: INTERAZIONE TRAMITE L'INTERFACCIA ---
//                    // Il tuo programma principale ora usa l'istanza del componente
//                    // attraverso l'interfaccia IComponenteDinamico. Non sa che tipo concreto sia!
//                    string testData = "testo di prova da processare";
//                    string result = dynamicComponent.ProcessaDato(testData);
//                    Console.WriteLine($"Risultato del processamento per '{testData}': {result}");

//                    string anotherTestData = "hello world";
//                    string anotherResult = dynamicComponent.ProcessaDato(anotherTestData);
//                    Console.WriteLine($"Risultato del processamento per '{anotherTestData}': {anotherResult}");
//                }
//                else
//                {
//                    Console.WriteLine("\nErrore durante la compilazione o il caricamento del componente dinamico.");
//                }

//                Console.WriteLine("\n--- Fine Applicazione ---");
//                Console.ReadKey(); // Per tenere la console aperta                
//            }catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//            Console.ReadKey(false);
//        }

//        // Funzione per compilare la stringa di codice e caricare il componente
//        private static IComponenteDinamico CompileAndLoadDynamicComponent(string sourceCode)
//        {
//            // 1. Parsing del codice sorgente
//            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

//            // 2. Definizione dell'assembly dinamico
//            string assemblyName = Guid.NewGuid().ToString() + ".dll";

//            // 3. Riferimenti NECESSARI per la compilazione Roslyn:
//            //    - System.Private.CoreLib (per tipi base come object, string, ecc.)
//            //    - System.Runtime (molte funzionalità base di .NET)
//            //    - L'assembly che contiene la nostra interfaccia IComponenteDinamico (questo stesso assembly!)
//            var references = new List<MetadataReference>
//            {
//                // Riferimento all'assembly base di .NET (typeof(object).GetTypeInfo().Assembly.Location)
//                // in .NET 6/7/8 spesso punta a System.Private.CoreLib
//                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
//                // Riferimento a System.Console (se il codice dinamico usa Console.WriteLine, ecc.)
//                MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location),
//                // Riferimento a System.Runtime (cruciale per molte funzionalità)
//                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
//                // **IL RIFERIMENTO FONDAMENTALE:** Riferimento all'assembly che contiene IComponenteDinamico.
//                // In questo caso, è l'assembly corrente del programma Main!
//                MetadataReference.CreateFromFile(typeof(IComponenteDinamico).GetTypeInfo().Assembly.Location)
//            };

//            // 4. Creazione dell'oggetto di compilazione
//            CSharpCompilation compilation = CSharpCompilation.Create(
//                assemblyName,
//                syntaxTrees: new[] { syntaxTree },
//                references: references,
//                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

//            using (var ms = new MemoryStream())
//            {
//                // 5. Esecuzione della compilazione
//                EmitResult result = compilation.Emit(ms);

//                if (!result.Success)
//                {
//                    Console.WriteLine("\nErrore di compilazione del codice dinamico:");
//                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
//                        diagnostic.IsWarningAsError ||
//                        diagnostic.Severity == DiagnosticSeverity.Error);

//                    foreach (Diagnostic diagnostic in failures)
//                    {
//                        Console.Error.WriteLine($"\t{diagnostic.Id}: {diagnostic.GetMessage()}");
//                    }
//                    return null;
//                }

//                ms.Seek(0, SeekOrigin.Begin); // Resetta il MemoryStream

//                // 6. Caricamento dell'assembly compilato in memoria
//                Assembly assembly = Assembly.Load(ms.ToArray());

//                // 7. Ricerca del tipo specifico nell'assembly caricato
//                // Utilizziamo "ComponentiGenerati.MioComponenteSpeciale" che è il Fully Qualified Name
//                // della classe definita nella stringa di codice dinamico.
//                Type dynamicComponentType = assembly.GetType("ComponentiGenerati.MioComponenteSpeciale");

//                if (dynamicComponentType != null)
//                {
//                    // 8. Creazione di un'istanza del tipo trovato
//                    object instance = Activator.CreateInstance(dynamicComponentType);

//                    // 9. CAST ALL'INTERFACCIA CONOSCIUTA!
//                    // Questo è il punto chiave: il programma principale può ora usare l'oggetto
//                    // senza conoscere il suo tipo concreto, ma solo la sua interfaccia.
//                    return (IComponenteDinamico)instance;
//                }
//                else
//                {
//                    Console.WriteLine($"Errore: Tipo 'ComponentiGenerati.MioComponenteSpeciale' non trovato nell'assembly dinamico.");
//                    return null;
//                }
//            }
//        }
//    }
//}
