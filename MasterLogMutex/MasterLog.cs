// 29.04.2021 aggiunta la proprietà SeparatoreCampi, definisce il carattere che è messo come separatori nel messaggio di log
// 04/07/2025 aggiunto overload all metodo log per impostare la lunghezza massima del log
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace MasterLog
{
    /// <summary>
    /// Indica il livello della riga di log, verra scritto nel file
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
    /// Classe per la determinazione dei livelli di log
    /// </summary>
    public class LogLevels : System.Attribute
    {
        /// <summary>
        /// Nome del livello da visualizzare
        /// </summary>
        public string LevelName { get; set; }
        /// <summary>
        /// peso in bit del livello di logs
        /// </summary>
        public int LogLevelToShow { get; set; }
        /// <summary>
        /// indice enum del log da visualizzare
        /// </summary>
        public LogLevel LogIndex { get; set; }
    }
    /// <summary>
    /// Classe per la gestione dei essaggi di log nei programmi
    /// </summary>
    public class Logger
    {
        private short applicationCategoryId = 0;
        private int applicationEventId = 0;
        private EventLog evLog;
        private bool isAdmin = false;
        private EventLogEntryType logEventEntryType;
        private List<LogLevels> LogLevelList;
        private byte[] rawData = new byte[4];
        private int swLogLevel = 0;
        private string separatoreCampi = "-";
        private DateTime toDay = DateTime.Now;

        /// <summary>
        ///  bit 0 LOG_INFO
        /// </summary>
        [LogLevels(LevelName = "INFO", LogLevelToShow = 1, LogIndex = (LogLevel)0)]
        public int LOG_INFO { get { return 1; } }
        /// <summary>
        /// bit 1 LOG_DEBUG 
        /// </summary>
        [LogLevels(LevelName = "DEBUG", LogLevelToShow = 2, LogIndex = (LogLevel)3)]
        public int LOG_DEBUG { get { return 2; } }
        /// <summary>
        /// bit 2 LOG_SOCKET 
        /// </summary>
        [LogLevels(LevelName = "SOCKET", LogLevelToShow = 4, LogIndex = (LogLevel)4)]
        public int LOG_SOCKET { get { return 4; } }
        /// bit log 6 LOG_WARNING
        /// </summary>
        [LogLevels(LevelName = "WARNING", LogLevelToShow = 0x40, LogIndex = (LogLevel)1)]
        public int LOG_WARNING { get { return 0x40; } }
        /// <summary>
        /// bit 7 LOG_SERVICE
        /// </summary>
        [LogLevels(LevelName = "SERVICE", LogLevelToShow = 0x80, LogIndex = (LogLevel)5)]
        public int LOG_SERVICE { get { return 0x80; } }
        /// <summary>
        /// bit 8 LOG_SERVICE_EVENT
        /// </summary>
        [LogLevels(LevelName = "SERVICE_EVENT", LogLevelToShow = 0x100, LogIndex = (LogLevel)6)]
        public int LOG_SERVICE_EVENT { get { return 0x100; } }
        /// <summary>
        /// bit 10 LOG_ENANCED_DEBUG
        /// </summary>
        [LogLevels(LevelName = "ENANCED_DEBUG", LogLevelToShow = 0x400, LogIndex = (LogLevel)7)]
        public int LOG_ENANCED_DEBUG { get { return 0x400; } }
        /// <summary>
        /// bit log 11 LOG_ERROR
        /// </summary>
        [LogLevels(LevelName = "ERROR", LogLevelToShow = 0x800, LogIndex = (LogLevel)2)]
        public int LOG_ERROR { get { return 0x800; } }
        /// <summary>
        /// bit log 14 INTERNAL_TEST 
        /// 16.04.2021 Log per test interni
        /// </summary>
        [LogLevels(LevelName = "INTERNAL_TEST", LogLevelToShow = 0x2000, LogIndex = (LogLevel)8)]
        public int LOG_INTERNAL_TEST { get { return 0x2000; } }
        /// <summary>
        /// Determina il livello dei log da visualizzare
        /// </summary>
        public int SwLogLevel { set { this.swLogLevel = value; } get { return this.swLogLevel; } }

        #region Costanti
        private const string _ESTENSIONE_FILE = "txt";
        private const string _DATE_TIME_NOME_FILE = "yyyyMMdd";
        #endregion

        #region Variabili
        Mutex syncLogMutex;
        bool mtxEnabled = false;
        /// <summary>
        /// Percorso dove scrivere i file
        /// </summary>
        private string _Percorso = string.Empty;

        /// <summary>
        /// Nome del file di log passato al costruttore
        /// </summary>
        private string _Nome = string.Empty;

        /// <summary>
        /// Nome del file di log comprensivo di data ed estensione
        /// </summary>
        private string _NomeLog = string.Empty;

        /// <summary>
        /// Indica quanti file di log conservare, gli altri verranno eliminati;
        /// Se maggiore di Zero
        /// </summary>
        private int _NumeroStorico = 0;

        /// <summary>
        /// Recupera il nome dell'eseguibile del software, se non riesce a recuperarlo allora assegna il nome del file di log.
        /// </summary>
        private string _NomeEseguibile = string.Empty;
        #endregion
        #region Construttori
        /// <summary>
        /// Crea un nuovo file di log
        /// </summary>
        /// <param name="percorso">E' il percorso dove scrivere il file di log (indifferente se termina con il carattere di slash o no)</param>
        /// <param name="nome">Nome del file di log: senza estensione, e senza data ed ora, perchè verranno aggiunte in automatico</param>
        /// <param name="numeroStorico">Se maggiore di Zero, indica quanti file di log conservare, gli altri verranno eliminati; altrimenti se Zero non eliminerà nessun file</param>
        public Logger(string percorso, string nome, int numeroStorico = 0, string ext = "")
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
            if (ext.Length == 0)
                _NomeLog = string.Concat(nome.Trim(), "_", DateTime.Now.ToString(_DATE_TIME_NOME_FILE), ".", _ESTENSIONE_FILE);
            else
                _NomeLog = string.Concat(nome.Trim(), "_", DateTime.Now.ToString(_DATE_TIME_NOME_FILE), ".", ext);
            #endregion

            _NumeroStorico = numeroStorico;

            #region LogInfoLocali
            try
            {
                LogInfoLocali();
            }
            catch (Exception ex)
            {
                throw new Exception("Errore durante la scrittura delle Informazioni locali: " + ex.Message);
            }
            #endregion

            #region Pulizia file di vecchi
            try
            {
                DeleteOldFile();
            }
            catch (Exception ex)
            {
                //Non solleva l'eccezione perchè non è bloccante per l'esecuzione del chiamante
                Log(LogLevel.WARNING, "Errore durante l'eliminazione dei file di log più vecchi: " + ex.Message);
            }
            #endregion
            // this.mtxEnabled = false;
            Type tyLogger = this.GetType();
            LogLevelList = new List<LogLevels>();
            PropertyInfo[] pInfoLogger = tyLogger.GetProperties();
            foreach (PropertyInfo pInfo in pInfoLogger)
            {
                var v = pInfo.GetCustomAttributes(false);
                if (v.Length > 0)
                {
                    if (v[0].GetType().Name == "LogLevels")
                    {
                        LogLevelList.Add(v[0] as LogLevels);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="percorso"></param>
        /// <param name="nome"></param>
        /// <param name="SyncLogMutex"></param>
        /// <param name="numeroStorico"></param>
        /// QUA
        public Logger(string percorso, string nome, Mutex SyncLogMutex, int numeroStorico = 0)
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
            _NomeLog = string.Concat(nome.Trim(), "_", DateTime.Now.ToString(_DATE_TIME_NOME_FILE), ".", _ESTENSIONE_FILE);
            #endregion

            _NumeroStorico = numeroStorico;

            #region LogInfoLocali
            try
            {
                LogInfoLocali();
            }
            catch (Exception ex)
            {
                throw new Exception("Errore durante la scrittura delle Informazioni locali: " + ex.Message);
            }
            #endregion

            #region Pulizia file di vecchi
            try
            {
                DeleteOldFile();
            }
            catch (Exception ex)
            {
                //Non solleva l'eccezione perchè non è bloccante per l'esecuzione del chiamante
                Log(LogLevel.WARNING, "Errore durante l'eliminazione dei file di log più vecchi: " + ex.Message);
            }
            #endregion
            this.syncLogMutex = SyncLogMutex;
            this.mtxEnabled = true;
            Type tyLogger = this.GetType();
            LogLevelList = new List<LogLevels>();
            PropertyInfo[] pInfoLogger = tyLogger.GetProperties();
            foreach (PropertyInfo pInfo in pInfoLogger)
            {
                var v = pInfo.GetCustomAttributes(false);
                if (v.Length > 0)
                {
                    if (v[0].GetType().Name == "LogLevels")
                    {
                        LogLevelList.Add(v[0] as LogLevels);
                    }
                }
            }
        }
        #endregion

        #region Private Helper Method
        /// <summary>
        /// Restituisce lo stream del file
        /// </summary>
        private StreamWriter getStream()
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
        /// Cancella i log più vecchi
        /// </summary>
        private void DeleteOldFile()
        {
            if (_NumeroStorico <= 0)
                return;

            try
            {
                string[] fileList = Directory.GetFiles(_Percorso, string.Format("{0}*", _Nome.Replace("..", "")), SearchOption.TopDirectoryOnly);

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

        #region "* * *  PROPERTIES  * * * 
        /// <summary>
        /// determina il tipo di separatore da usare nella costruzione della stringa di log, il valore predefinito è '-'
        /// </summary>
        public string SeparatoreCampi { get { return this.separatoreCampi; } set { this.separatoreCampi = value; } }
        public bool IsAdmin { get { return this.isAdmin; } set { this.isAdmin = value; } }
        public EventLogEntryType LogEventEntryType { get { return this.logEventEntryType; } set { this.logEventEntryType = value; } }
        public int AplicationEventID { get { return this.applicationEventId; } set { this.applicationEventId = value; } }
        public short ApplicationCategoryId { get { return this.applicationCategoryId; } set { this.applicationCategoryId = value; } }
        public byte[] RawData { get { return this.rawData; } set { this.rawData = value; } }
        public EventLog EvLog { get { return this.evLog; } set { this.evLog = value; } }
        #endregion

        #region Public
        /// <summary>
        /// Scrive nel file di log le seguenti informazioni:
        /// 1) Il nome della macchina dal quale viene eseguito il software
        /// 2) L'IP della macchina dal quale viene eseguito il software
        /// 3) Il percorso del file Exe del software (se si tratta di un applicativo Web non scriverà nulla)
        /// </summary>
        private void LogInfoLocali()
        {
            try
            {
                using (StreamWriter writer = getStream())
                {
                    int i;
                    int n;
                    string localHost = string.Empty;
                    System.Net.IPAddress[] localIps = null;
                    string localPath = string.Empty;

                    writer.WriteLine("Informazioni di sistema".ToUpper());
                    writer.WriteLine("sistema partito il: ".ToUpper() + DateTime.Now.ToString());
                    #region Host
                    try
                    {
                        localHost = System.Net.Dns.GetHostName();
                        if (!string.IsNullOrEmpty(localHost))
                        {
                            writer.WriteLine(string.Format("Host: {0}", localHost));
                        }
                    }
                    catch (Exception ex)
                    {
                        writer.WriteLine(string.Format("Host: Errore durante il recupero: {0}", ex.Message));
                    }
                    #endregion Host

                    #region IP
                    try
                    {
                        localIps = System.Net.Dns.GetHostAddresses(localHost);
                        n = localIps.Length;
                        for (i = 0; i < n; i++)
                        {
                            if (!string.IsNullOrEmpty(localIps[i].ToString()))
                            {
                                writer.WriteLine(string.Format("IP: {0}", localIps[i].ToString()));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        writer.WriteLine(string.Format("IP: Errore durante il recupero: {0}", ex.Message));
                    }
                    #endregion IP

                    #region Path
                    try
                    {
                        localPath = System.Reflection.Assembly.GetEntryAssembly().Location;

                        if (!string.IsNullOrEmpty(localPath))
                        {
                            writer.WriteLine(string.Format("Percorso: {0}", localPath));

                            try
                            {
                                string[] localPathSplit = localPath.Split('\\');
                                _NomeEseguibile = localPathSplit[localPathSplit.Length - 1];
                                _NomeEseguibile = _NomeEseguibile.Substring(0, _NomeEseguibile.LastIndexOf("."));
                            }
                            catch
                            {
                                _NomeEseguibile = _Nome;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        writer.WriteLine(string.Format("Percorso: Errore durante il recupero: {0}", ex.Message));
                        _NomeEseguibile = _Nome;
                    }
                    #endregion Path

                    //Riga vuota
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
        /// Restituisce il percorso compreso il nome del file.
        /// </summary>
        /// <returns></returns>
        public string GetPercorsoCompleto()
        {
            return Path.Combine(_Percorso, _NomeLog);
        }

        /// <summary>
        /// Restituisce il percorso senza il nome del file.
        /// </summary>
        /// <returns></returns>
        public string GetPercorso()
        {
            return _Percorso;
        }

        /// <summary>
        /// Restituisce solo il nome del file.
        /// </summary>
        /// <returns></returns>
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
        /// Cancellazione dei file per data
        /// Si cancellano tutti i file precedenti al numero di giorni indicato in NumDay
        /// </summary>
        /// <param name="FolderSpec">Directory da cui cancellare i file più vecchi</param>
        /// <param name="PatternSearc">Sequenza di caratteri di cui cercare i file (esmpio: "WinTTab*.*") </param>
        /// <param name="NumDay">Numero di giorni a dietro da cancellare</param>
        public void DeleteFileByDate(string FolderSpec, string PatternSearc, int NumDay)
        {
            string[] listFiles = { };
            listFiles = Directory.GetFiles(FolderSpec, PatternSearc);
            foreach (string s in listFiles)
            {
                if (File.GetCreationTime(s) < DateTime.Now.AddDays(-NumDay))
                    File.Delete(s);
            }
        }
        /// <summary>
        /// Scrive la riga nel file di log. Questa è la firma "classica" per mantenere la compatibilità.
        /// Non tronca automaticamente il messaggio.
        /// </summary>
        /// <param name="livello">Livello del messaggio di log</param>
        /// <param name="message">Contenuto da scrivere</param>
        public void Log(LogLevel livello, string message)
        {
            // Chiama l'overload più completo, impostando truncateLongStrings a false di default.
            Log(livello, message, false);
        }
        /// <summary>
        /// Scrive la riga nel file di log
        /// </summary>
        /// <param name="livello">Livello del messaggio di log</param>
        /// <param name="message">Contenuto da scrivere</param>
        public void Log(LogLevel livello, string message, bool truncateLongStrings, int maxLength = 100)
        {
            if (!truncateLongStrings) maxLength = 100000;
            DateTime myDate = DateTime.Now;
            // myDate = myDate.AddDays(1);
            //int i = (int) livello;
            if (toDay.Day != myDate.Day)
            {
                _NomeLog = string.Concat(_Nome.Trim(), "_", myDate.ToString(_DATE_TIME_NOME_FILE), ".", _ESTENSIONE_FILE);
                toDay = myDate;
            }
            var v = LogLevelList.Find(A => A.LogIndex == livello);
            if (((this.swLogLevel & v.LogLevelToShow) == v.LogLevelToShow) || (livello == LogLevel.ERROR) || livello == LogLevel.WARNING)
            {
                string finalMessage = message;
                if (truncateLongStrings)
                {
                    finalMessage = TruncateForLogInternal(message, maxLength);
                }
                string logEntry = string.Format("{0} {1} {2} {3} {4}",
                                                DateTime.Now.ToString("HH:mm:ss.fffffff"),
                                                this.separatoreCampi,
                                                livello.ToString(),
                                                this.separatoreCampi,
                                                finalMessage);
                if (this.mtxEnabled && this.syncLogMutex != null) // Aggiunto controllo null per sicurezza
                {
                    try
                    {
                        this.syncLogMutex.WaitOne();
                        using (StreamWriter writer = getStream())
                        {
                            writer.WriteLine(logEntry);
                        }
                    }
                    finally
                    {
                        this.syncLogMutex.ReleaseMutex();
                    }
                }
                else // Nessun mutex abilitato o mutex è null
                {
                    using (StreamWriter writer = getStream())
                    {
                        writer.WriteLine(logEntry);
                    }
                }
                // Log per EventLog di Windows se isAdmin è true
                if (this.isAdmin)
                {
                    string myMessage = string.Format("{0} {1} {2} {3} {4}", DateTime.Now.ToString("HH:mm:ss.fffffff"), this.separatoreCampi, livello.ToString(), this.separatoreCampi, message);
                    this.evLog.WriteEntry(myMessage, this.LogEventEntryType, this.applicationEventId, this.applicationCategoryId, this.RawData);
                }

                /*
                if (this.mtxEnabled)
                {
                    this.syncLogMutex.WaitOne();
                    using (StreamWriter writer = getStream())
                        writer.WriteLine(string.Format("{0} {1} {2} {3} {4}", DateTime.Now.ToString("HH:mm:ss.fffffff"), this.separatoreCampi, livello.ToString(), this.separatoreCampi, message));
                    this.syncLogMutex.ReleaseMutex();
                }
                else
                {
                    using (StreamWriter writer = getStream())
                        writer.WriteLine(string.Format("{0} {1} {2} {3} {4}", DateTime.Now.ToString("HH:mm:ss.fffffff"), this.separatoreCampi, livello.ToString(), this.separatoreCampi, message));
                }
                if (this.isAdmin)
                {
                    string myMessage = string.Format("{0} {1} {2} {3} {4}", DateTime.Now.ToString("HH:mm:ss.fffffff"), this.separatoreCampi, livello.ToString(), this.separatoreCampi, message);
                    this.evLog.WriteEntry(myMessage, this.LogEventEntryType, this.applicationEventId, this.applicationCategoryId, this.RawData);
                }
                */
            }
        }
        #endregion
    }
}