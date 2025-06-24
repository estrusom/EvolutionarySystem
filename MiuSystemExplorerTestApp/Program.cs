// File: C:\Progetti\EvolutiveSystem\MiuSystemExplorerTestApp\Program.cs
// Data di riferimento: 23 giugno 2025
// Descrizione: Questo file simula un'applicazione client (es. SemanticService)
// che interagisce direttamente con un'istanza di MiuSystemManager (il wrapper di classe)
// senza l'uso di socket o un server di comunicazione separato.
// La gestione della persistenza e delle statistiche è incapsulata completamente
// all'interno del MiuSystemManager.
// CORREZIONI: Risolti errori di namespace e riferimenti a FormalSystemExplorer/Manager.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading; // Necessario per CancellationTokenSource
using System.Threading.Tasks;
using MasterLog; // Necessario per la tua classe Logger
using EvolutiveSystem.Common; // Per MiuNotificationEventArgs e altri tipi comuni
using MiuSystemWorker; // Per MiuSystemManager (e indirettamente MiuNotificationEventArgs se fosse stato definito lì)
using MIU.Core; // Per MIUStringConverter (se ancora usato qui per la compressione/decompressione delle stringhe per l'input al manager)

namespace MiuSystemExplorerTestApp // Assicurati che questo sia il namespace corretto del tuo progetto di test
{
    // Lasciata qui per compatibilità, ma non direttamente usata
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public SerializableDictionary() : base() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
    }

    internal class Program
    {
        private static Logger _logger; // Istanza del logger

        static async Task Main(string[] args) // Modificato a async Task Main
        {
            // Inizializzazione del Logger
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _logger = new Logger(logDirectory, "MIULog", 7); // Conserva gli ultimi 7 giorni di log
            _logger.SwLogLevel = _logger.LOG_INFO | _logger.LOG_DEBUG | _logger.LOG_ERROR | _logger.LOG_WARNING; // Imposta i livelli di log attivi
            _logger.Log(LogLevel.INFO, "Applicazione client di test avviata (integrazione diretta del wrapper MiuSystemManager).");

            string databaseFilePath = @"C:\Progetti\EvolutiveSystem\Database\miu_data.db";

            // --- ISTANZIAZIONE E INTEGRAZIONE DIRETTA DEL WRAPPER (MiuSystemManager) ---
            MiuSystemManager miuSystemManager = new MiuSystemManager(_logger);

            try
            {
                // Sottoscrivi alle notifiche del MiuSystemManager.
                miuSystemManager.OnMiuSystemNotification += HandleMiuSystemNotification;

                _logger.Log(LogLevel.INFO, "[Program] Inizializzazione del MiuSystemManager...");
                miuSystemManager.InitializeMiuSystem(databaseFilePath);
                _logger.Log(LogLevel.INFO, "[Program] MiuSystemManager inizializzato.");

                // --- Esecuzione di una ricerca MIU ---
                // Le stringhe di test vengono compresse prima di essere passate a MiuSystemManager.
                // Assumiamo che MIUStringConverter sia accessibile tramite MIU.Core o EvolutiveSystem.Common.
                string testStartStringStandard = "MUUIIIMMMMI";
                string testTargetStringStandard = "MUMMMMIUMUMMMMIU";

                _logger.Log(LogLevel.INFO, $"[Program] Avvio ricerca MIU per '{testStartStringStandard}' -> '{testTargetStringStandard}'.");

                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    await miuSystemManager.PerformMiuSearchTask(
                        MIUStringConverter.DeflateMIUString(testStartStringStandard),
                        MIUStringConverter.DeflateMIUString(testTargetStringStandard),
                        cancellationTokenSource.Token
                    );
                }

                _logger.Log(LogLevel.INFO, "[Program] Ricerca MIU completata (o annullata/errore).");

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[Program ERROR] Errore critico nell'applicazione client: {ex.Message}");
                Console.WriteLine($"[Client Error] Un errore si è verificato: {ex.Message}");
            }
            finally
            {
                _logger.Log(LogLevel.INFO, "[Program] Spegnimento del MiuSystemManager...");
                miuSystemManager.ShutdownMiuSystem();
                _logger.Log(LogLevel.INFO, "[Program] MiuSystemManager spento.");

                // Disiscrivi dalle notifiche per pulizia
                miuSystemManager.OnMiuSystemNotification -= HandleMiuSystemNotification;

                Console.WriteLine("Premere un tasto qualsiasi per uscire.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Gestisce le notifiche provenienti dal MiuSystemManager.
        /// Questo simula come il SemanticService riceverebbe feedback dal wrapper.
        /// </summary>
        /// <param name="sender">L'oggetto che ha scatenato l'evento.</param>
        /// <param name="e">Gli argomenti dell'evento contenenti il tipo e il messaggio della notifica.</param>
        private static void HandleMiuSystemNotification(object sender, MiuNotificationEventArgs e)
        {
            _logger.Log(LogLevel.INFO, $"[Program-Notification] Notifica da MIU System Manager: Tipo={e.NotificationType}, Messaggio='{e.Message}'");
            Console.WriteLine($"[Client Notification] {e.NotificationType}: {e.Message}");
        }
    }
}
