// Importa gli spazi dei nomi necessari per la Dependency Injection e l'hosting.
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Importa i tuoi spazi dei nomi specifici.
using EvolutionarySystem.Core.Models;
using MasterLog;

// Spazi dei nomi per la configurazione e le utilità di Entity Framework e Windows Forms
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Windows.Forms;
using EvolutionarySystem.Data;

// Questo codice si basa sul pattern di un'applicazione Windows Forms con un host di servizio
// per la Dependency Injection.

namespace MIUtopologyApp
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Impostazioni standard per le applicazioni Windows Forms
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Inizializzazione del logger PRIMA di qualsiasi altra operazione.
            // In questo modo è sempre disponibile per loggare errori critici.
            Logger log = null;
            try
            {
                // Configura e inizializza il logger.
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                string debugLevelString = config.GetSection("AppSettings:DebugLev").Value;
                int debugLevel = 0;
                if (!string.IsNullOrEmpty(debugLevelString) && int.TryParse(debugLevelString, out debugLevel))
                {
                    // Livello di log letto correttamente.
                }

                log = new Logger(AppDomain.CurrentDomain.BaseDirectory, "MIUtopologyAppLog", 10);
                log.SwLogLevel = debugLevel;
                log.Log(LogLevel.INFO, $"Logger inizializzato con successo. Livello di debug impostato a {debugLevel}.");
            }
            catch (Exception ex)
            {
                // Fallback in caso di errore critico durante l'inizializzazione del logger stesso.
                Console.WriteLine($"Errore fatale durante l'inizializzazione del logger: {ex.Message}");
                MessageBox.Show("Errore fatale durante l'inizializzazione del logger.", "Errore di Avvio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Non ha senso continuare.
            }

            try
            {
                // Costruisci l'host che gestirà i servizi (la Dependency Injection)
                var host = CreateHostBuilder(args, log).Build();
                // Usa l'host per risolvere l'istanza della tua MainForm
                var form = host.Services.GetRequiredService<MainForm>();
                Application.Run(form);
            }
            catch (Exception ex)
            {
                // Usa il logger pre-inizializzato per registrare l'errore.
                if (log != null)
                {
                    log.Log(LogLevel.ERROR, $"Errore critico durante l'avvio dell'applicazione: {ex.Message}");
                    log.Log(LogLevel.ERROR, $"Stack Trace: {ex.StackTrace}");
                }

                // Mostra un messaggio di errore all'utente.
                MessageBox.Show(
                    "Si è verificato un errore critico durante l'avvio. Controlla il log degli errori per maggiori dettagli.",
                    "Errore di Avvio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, Logger log) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    // Aggiunge il logger pre-esistente al container DI come Singleton.
                    // Questo garantisce che esista una sola istanza del logger in tutta l'applicazione.
                    services.AddSingleton(log);

                    // Aggiunge il tuo DbContext come servizio, configurandolo per usare SQLite.
                    // Se la stringa di connessione è mancante, l'errore verrà gestito dal blocco try-catch in Main().
                    string connectionString = configuration.GetConnectionString("SqliteConnection");
                    services.AddDbContext<EvolutionarySystemContext>(options =>
                        options.UseSqlite(connectionString));

                    // Registra il tuo servizio di accesso ai dati.
                    services.AddTransient<IMIUDataService, MIUDataService>();

                    // Registra la tua Form principale in modo che il container di servizi possa costruirla.
                    services.AddTransient<MainForm>();
                });
    }
}
