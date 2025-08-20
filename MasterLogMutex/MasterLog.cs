// Modificato per .NET 8
// Rimossa la dipendenza da System.Diagnostics.EventLog per la compatibilità cross-platform.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Linq;

namespace MasterLog
{
    /// <summary>
    /// Indica il livello della riga di log, verra scritto nel file.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// bit 0 (1)
        /// </summary>
        INFO,
        /// <summary>
        /// bit 6 (64)
        /// </summary>
        WARNING,
        /// <summary>
        /// bit 11 (2048)
        /// </summary>
        ERROR,
        /// <summary>
        /// bit 1 (2)
        /// </summary>
        DEBUG,
        /// <summary>
        /// bit 2 (4)
        /// </summary>
        SOCKET,
        /// <summary>
        /// bit 7 (128)
        /// </summary>
        SERVICE,
        /// <summary>
        /// bit 8 (256)
        /// </summary>
        SERVICE_EVENT,
        /// <summary>
        /// bit 10 (1024)
        /// </summary>
        ENANCED_DEBUG,
        /// <summary>
        /// bit 14 (8192)
        /// </summary>
        INTERNAL_TEST
    }

    /// <summary>
    /// Classe per la determinazione dei livelli di log.
    /// </summary>
    public class LogLevels : System.Attribute
    {
        /// <summary>
        /// Nome del livello da visualizzare.
        /// </summary>
        public string LevelName { get; set; }
        /// <summary>
        /// peso in bit del livello di logs.
        /// </summary>
        public int LogLevelToShow { get; set; }
        /// <summary>
        /// indice enum del log da visualizzare.
        /// </summary>
        public LogLevel LogIndex { get; set; }
    }

    /// <summary>
    /// Classe per configurare i livelli di log tramite attributi di proprietà.
    /// </summary>
    public class LogConfiguration
    {
        /// <summary>
        /// bit 0 LOG_INFO
        /// </summary>
        [LogLevels(LevelName = "INFO", LogLevelToShow = 1, LogIndex = LogLevel.INFO)]
        public int LOG_INFO { get; } = 1;

        /// <summary>
        /// bit 1 LOG_DEBUG 
        /// </summary>
        [LogLevels(LevelName = "DEBUG", LogLevelToShow = 2, LogIndex = LogLevel.DEBUG)]
        public int LOG_DEBUG { get; } = 2;

        /// <summary>
        /// bit 2 LOG_SOCKET 
        /// </summary>
        [LogLevels(LevelName = "SOCKET", LogLevelToShow = 4, LogIndex = LogLevel.SOCKET)]
        public int LOG_SOCKET { get; } = 4;

        /// <summary>
        /// bit 6 LOG_WARNING
        /// </summary>
        [LogLevels(LevelName = "WARNING", LogLevelToShow = 0x40, LogIndex = LogLevel.WARNING)]
        public int LOG_WARNING { get; } = 0x40;

        /// <summary>
        /// bit 7 LOG_SERVICE
        /// </summary>
        [LogLevels(LevelName = "SERVICE", LogLevelToShow = 0x80, LogIndex = LogLevel.SERVICE)]
        public int LOG_SERVICE { get; } = 0x80;

        /// <summary>
        /// bit 8 LOG_SERVICE_EVENT
        /// </summary>
        [LogLevels(LevelName = "SERVICE_EVENT", LogLevelToShow = 0x100, LogIndex = LogLevel.SERVICE_EVENT)]
        public int LOG_SERVICE_EVENT { get; } = 0x100;

        /// <summary>
        /// bit 10 LOG_ENANCED_DEBUG
        /// </summary>
        [LogLevels(LevelName = "ENANCED_DEBUG", LogLevelToShow = 0x400, LogIndex = LogLevel.ENANCED_DEBUG)]
        public int LOG_ENANCED_DEBUG { get; } = 0x400;

        /// <summary>
        /// bit 11 LOG_ERROR
        /// </summary>
        [LogLevels(LevelName = "ERROR", LogLevelToShow = 0x800, LogIndex = LogLevel.ERROR)]
        public int LOG_ERROR { get; } = 0x800;

        /// <summary>
        /// bit 14 INTERNAL_TEST 
        /// </summary>
        [LogLevels(LevelName = "INTERNAL_TEST", LogLevelToShow = 0x4000, LogIndex = LogLevel.INTERNAL_TEST)]
        public int LOG_INTERNAL_TEST { get; } = 0x4000;
    }

    /// <summary>
    /// Classe per la gestione dei messaggi di log nei programmi.
    /// </summary>
    public class Logger
    {
        private int swLogLevel = 0;
        private string separatoreCampi = "-";
        private DateTime toDay = DateTime.Now;
        private List<LogLevels> LogLevelList;

        /// <summary>
        /// determina il tipo di separatore da usare nella costruzione della stringa di log, il valore predefinito è '-'.
        /// </summary>
        public string SeparatoreCampi { get { return this.separatoreCampi; } set { this.separatoreCampi = value; } }

        #region Costanti
        private const string _ESTENSIONE_FILE = "txt";
        private const string _DATE_TIME_NOME_FILE = "yyyyMMdd";
        #endregion

        #region Variabili
        private Mutex syncLogMutex;
        private bool mtxEnabled = false;

        /// <summary>
        /// Percorso dove scrivere i file.
        /// </summary>
        private string _Percorso = string.Empty;

        /// <summary>
        /// Nome del file di log passato al costruttore.
        /// </summary>
        private string _Nome = string.Empty;

        /// <summary>
        /// Nome del file di log comprensivo di data ed estensione.
        /// </summary>
        private string _NomeLog = string.Empty;

        /// <summary>
        /// Indica quanti file di log conservare, gli altri verranno eliminati;
        /// Se maggiore di Zero.
        /// </summary>
        private int _NumeroStorico = 0;

        /// <summary>
        /// Recupera il nome dell'eseguibile del software, se non riesce a recuperarlo allora assegna il nome del file di log.
        /// </summary>
        private string _NomeEseguibile = string.Empty;
        #endregion

        #region Construttori

        /// <summary>
        /// Crea un nuovo file di log con opzioni di base.
        /// </summary>
        /// <param name="percorso">E' il percorso dove scrivere il file di log (indifferente se termina con il carattere di slash o no).</param>
        /// <param name="nome">Nome del file di log: senza estensione, e senza data ed ora, perchè verranno aggiunte in automatico.</param>
        /// <param name="numeroStorico">Se maggiore di Zero, indica quanti file di log conservare, gli altri verranno eliminati; altrimenti se Zero non eliminerà nessun file.</param>
        /// <param name="ext">Estensione del file, predefinita "txt".</param>
        public Logger(string percorso, string nome, int numeroStorico = 0, string ext = _ESTENSIONE_FILE)
            : this(percorso, nome, null, numeroStorico, ext) // Chaining al costruttore più completo
        {
        }

        /// <summary>
        /// Crea un nuovo file di log con un Mutex per la sincronizzazione tra processi.
        /// </summary>
        /// <param name="percorso">Percorso dove scrivere il file di log.</param>
        /// <param name="nome">Nome del file di log.</param>
        /// <param name="SyncLogMutex">Mutex per la sincronizzazione tra processi.</param>
        /// <param name="numeroStorico">Numero di file di log da conservare.</param>
        /// <param name="ext">Estensione del file, predefinita "txt".</param>
        public Logger(string percorso, string nome, Mutex SyncLogMutex, int numeroStorico = 0, string ext = _ESTENSIONE_FILE)
        {
            #region percorso
            if (string.IsNullOrWhiteSpace(percorso))
                throw new IOException("Specificare un Percorso valido.");

            try
            {
                if (!Directory.Exists(percorso))
                    Directory.CreateDirectory(percorso);
            }
            catch (Exception ex)
            {
                throw new IOException("Errore durante la creazione della directory: " + ex.Message);
            }

            _Percorso = percorso;
            #endregion

            #region nome
            if (string.IsNullOrWhiteSpace(nome))
                throw new IOException("Specificare un Nome valido.");

            _Nome = nome;
            _NomeLog = string.Concat(nome.Trim(), "_", DateTime.Now.ToString(_DATE_TIME_NOME_FILE), ".", ext);
            #endregion

            _NumeroStorico = numeroStorico;

            // Inizializzazione della lista usando la reflection sulla classe LogConfiguration
            LogLevelList = new List<LogLevels>();
            var logConfigType = typeof(LogConfiguration);
            foreach (var prop in logConfigType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = (LogLevels)prop.GetCustomAttribute(typeof(LogLevels));
                if (attr != null)
                {
                    LogLevelList.Add(attr);
                }
            }

            #region LogInfoLocali
            try
            {
                LogInfoLocali();
            }
            catch (Exception ex)
            {
                // Registra l'errore senza bloccare l'esecuzione
                Log(LogLevel.ERROR, "Errore durante la scrittura delle Informazioni locali: " + ex.Message);
            }
            #endregion

            #region Pulizia file di vecchi
            try
            {
                DeleteOldFile();
            }
            catch (Exception ex)
            {
                Log(LogLevel.WARNING, "Errore durante l'eliminazione dei file di log più vecchi: " + ex.Message);
            }
            #endregion

            this.syncLogMutex = SyncLogMutex;
            this.mtxEnabled = this.syncLogMutex != null;
        }
        #endregion

        /// <summary>
        /// Determina il livello dei log da visualizzare.
        /// </summary>
        public int SwLogLevel { set { this.swLogLevel = value; } get { return this.swLogLevel; } }

        #region Private Helper Methods
        /// <summary>
        /// Restituisce lo stream del file.
        /// </summary>
        private StreamWriter GetStream()
        {
            try
            {
                return new StreamWriter(GetPercorsoCompleto(), true);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Cancella i log più vecchi in base al numero di file da conservare.
        /// </summary>
        private void DeleteOldFile()
        {
            if (_NumeroStorico <= 0)
                return;

            try
            {
                string[] fileList = Directory.GetFiles(_Percorso, string.Format("{0}*", _Nome.Trim()), SearchOption.TopDirectoryOnly);

                if (fileList.Length > 0 && fileList.Length > _NumeroStorico)
                {
                    List<FileInfo> fileInfoList = new List<FileInfo>();
                    for (int i = 0; i < fileList.Length; i++)
                        fileInfoList.Add(new FileInfo(fileList[i]));

                    fileInfoList.Sort((y, x) => DateTime.Compare(x.CreationTimeUtc, y.CreationTimeUtc));

                    for (int i = _NumeroStorico; i < fileInfoList.Count; i++)
                    {
                        try
                        {
                            File.Delete(fileInfoList[i].FullName);
                        }
                        catch
                        {
                            // Ignora gli errori di eliminazione, non sono bloccanti.
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Tronca una stringa per scopi di logging, aggiungendo "..." al centro se troppo lunga.
        /// Metodo interno, chiamato solo dal metodo Log.
        /// </summary>
        /// <param name="s">La stringa da troncare.</param>
        /// <param name="maxLength">La lunghezza massima desiderata per la stringa troncata.</param>
        /// <returns>La stringa troncata o la stringa originale se non supera maxLength.</returns>
        private string TruncateForLogInternal(string s, int maxLength)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= maxLength)
                return s;

            if (maxLength < 5) maxLength = 5; // Assicura che ci sia spazio per "a...b"

            int halfLength = (maxLength - 3) / 2; // -3 è per i puntini di sospensione "..."
            if (halfLength < 1) halfLength = 1; // Assicura almeno 1 carattere per lato

            return s.Substring(0, halfLength) + "..." + s.Substring(s.Length - halfLength);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Scrive nel file di log le informazioni di sistema locali.
        /// </summary>
        private void LogInfoLocali()
        {
            try
            {
                using (StreamWriter writer = GetStream())
                {
                    writer.WriteLine("--- INFORMAZIONI DI SISTEMA ---");
                    writer.WriteLine($"Sistema avviato il: {DateTime.Now}");

                    // Tentativo di ottenere il nome dell'eseguibile
                    try
                    {
                        var entryAssembly = Assembly.GetEntryAssembly();
                        if (entryAssembly != null)
                        {
                            _NomeEseguibile = Path.GetFileNameWithoutExtension(entryAssembly.Location);
                            writer.WriteLine($"Percorso Eseguibile: {entryAssembly.Location}");
                        }
                        else
                        {
                            _NomeEseguibile = _Nome;
                            writer.WriteLine("Percorso Eseguibile: non disponibile (ambiente non-desktop)");
                        }
                    }
                    catch (Exception ex)
                    {
                        writer.WriteLine($"Percorso Eseguibile: Errore durante il recupero: {ex.Message}");
                        _NomeEseguibile = _Nome;
                    }

                    // Aggiunge una riga vuota
                    writer.WriteLine("");
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Restituisce il percorso completo del file di log.
        /// </summary>
        public string GetPercorsoCompleto()
        {
            return Path.Combine(_Percorso, _NomeLog);
        }

        /// <summary>
        /// Restituisce il percorso della directory dei log.
        /// </summary>
        public string GetPercorso()
        {
            return _Percorso;
        }

        /// <summary>
        /// Restituisce solo il nome del file.
        /// </summary>
        public string GetNome()
        {
            return _NomeLog;
        }

        /// <summary>
        /// Restituisce il nome dell'eseguibile del software, se non riesce a recuperarlo allora assegna il nome del file di log.
        /// </summary>
        public string GetNomeEseguibile()
        {
            return _NomeEseguibile;
        }

        /// <summary>
        /// Cancella i file di log più vecchi in base al numero di giorni.
        /// </summary>
        /// <param name="folderSpec">Directory da cui cancellare i file più vecchi.</param>
        /// <param name="patternSearc">Sequenza di caratteri di cui cercare i file (es. "WinTTab*.*").</param>
        /// <param name="numDay">Numero di giorni a dietro da cancellare.</param>
        public void DeleteFileByDate(string folderSpec, string patternSearc, int numDay)
        {
            string[] listFiles = Directory.GetFiles(folderSpec, patternSearc);
            foreach (string s in listFiles)
            {
                if (File.GetCreationTime(s) < DateTime.Now.AddDays(-numDay))
                    File.Delete(s);
            }
        }

        /// <summary>
        /// Scrive la riga nel file di log. Questa è la firma "classica" per mantenere la compatibilità.
        /// Non tronca automaticamente il messaggio.
        /// </summary>
        /// <param name="livello">Livello del messaggio di log.</param>
        /// <param name="message">Contenuto da scrivere.</param>
        public void Log(LogLevel livello, string message)
        {
            Log(livello, message, false);
        }

        /// <summary>
        /// Scrive la riga nel file di log.
        /// </summary>
        /// <param name="livello">Livello del messaggio di log.</param>
        /// <param name="message">Contenuto da scrivere.</param>
        /// <param name="truncateLongStrings">Se true, tronca le stringhe lunghe.</param>
        /// <param name="maxLength">Lunghezza massima del messaggio di log.</param>
        public void Log(LogLevel livello, string message, bool truncateLongStrings, int maxLength = 100)
        {
            // Aggiorna il nome del file se il giorno è cambiato
            DateTime myDate = DateTime.Now;
            if (toDay.Day != myDate.Day)
            {
                _NomeLog = string.Concat(_Nome.Trim(), "_", myDate.ToString(_DATE_TIME_NOME_FILE), ".", _ESTENSIONE_FILE);
                toDay = myDate;
            }

            // Controlla se il livello di log è abilitato
            var v = LogLevelList.Find(A => A.LogIndex == livello);
            if (v != null && (((this.swLogLevel & v.LogLevelToShow) == v.LogLevelToShow) || (livello == LogLevel.ERROR) || livello == LogLevel.WARNING))
            {
                string finalMessage = truncateLongStrings ? TruncateForLogInternal(message, maxLength) : message;

                string logEntry = string.Format("{0}{1}{2}{3}{4}",
                    DateTime.Now.ToString("HH:mm:ss.fffffff"),
                    this.separatoreCampi,
                    livello.ToString(),
                    this.separatoreCampi,
                    finalMessage);

                // Scrittura sul file di log con o senza Mutex
                if (this.mtxEnabled && this.syncLogMutex != null)
                {
                    try
                    {
                        this.syncLogMutex.WaitOne();
                        File.AppendAllText(GetPercorsoCompleto(), logEntry + Environment.NewLine);
                    }
                    catch (Exception)
                    {
                        // In caso di errore con il mutex, scrivi direttamente
                        File.AppendAllText(GetPercorsoCompleto(), logEntry + Environment.NewLine);
                    }
                    finally
                    {
                        this.syncLogMutex.ReleaseMutex();
                    }
                }
                else
                {
                    File.AppendAllText(GetPercorsoCompleto(), logEntry + Environment.NewLine);
                }
            }
        }
        #endregion
    }
}
