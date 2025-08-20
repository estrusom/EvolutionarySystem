// File: EvolutionarySystemContext.cs
// Questo file definisce il DbContext che si connette a un database
// esistente. Il suo ruolo è gestire le query e le operazioni di
// salvataggio, non la creazione delle tabelle.

using Microsoft.EntityFrameworkCore;
using EvolutionarySystem.Core.Models;
using MasterLog;
using Microsoft.Extensions.Logging;

namespace EvolutionarySystem.Core.Models
{
    // Il DbContext è il ponte per interagire con il database.
    // Eredita da DbContext e gestisce la sessione del database.
    public class EvolutionarySystemContext : DbContext
    {
        private readonly Logger _logMutex;
        // Questo costruttore riceve le opzioni di configurazione
        // (es. la stringa di connessione al database) tramite
        // Dependency Injection, che è la pratica standard in .NET.
        public EvolutionarySystemContext(DbContextOptions<EvolutionarySystemContext> options, Logger logMutex)
            : base(options)
        {
        }

        // Queste proprietà DbSet rappresentano le tabelle del tuo database.
        // I nomi delle proprietà (es. ExplorationAnomalies) verranno mappati
        // in automatico ai nomi delle tabelle (che devono essere le stesse).
        public DbSet<ExplorationAnomaly> ExplorationAnomalies { get; set; }

        // Ora il tuo DbContext conosce anche la tabella MIUPaths.
        public DbSet<MIUPath> MIUPaths { get; set; }
        public DbSet<MIURuleApplication> MIURuleApplications { get; set; }

        // È un buon posto per configurare manualmente il mapping se
        // i nomi delle tabelle o delle colonne non coincidono.
        // Ad esempio: modelBuilder.Entity<ExplorationAnomaly>().ToTable("Anomalies");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configura MIUPath per usare PathStepID come chiave primaria
            modelBuilder.Entity<MIUPath>().HasKey(p => p.PathStepID);

            // Se ApplicationID è la chiave primaria per MIURuleApplication,
            // aggiungi anche questa configurazione
            modelBuilder.Entity<MIURuleApplication>().HasKey(ra => ra.ApplicationID);
        }
    }
}
