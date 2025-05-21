//Change log
// 2025.05.20 lista per accessi concorrenti al clien socket asincrono
// 2025.05.20 gestione invio in modalità asincrona alle UI connesse
using AsyncSocketServer;
using CommandHandlers;
using MasterLog;
using MessaggiErrore;
using SemanticProcessor.Properties;
using SocketManagerInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using SemanticProcessor;
using System.Xml.Linq;
using System.Collections.Concurrent;
using SocketManager;
using EvolutiveSystem.Core;

namespace SemanticProcessor
{
    //94470marco
    /// <summary>
    /// Definition of service startup states
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// Service stoped
        /// </summary>
        SERVICE_STOPPED = 0x00000001,
        /// <summary>
        /// Service is pending start
        /// </summary>
        SERVICE_START_PENDING = 0x00000002,
        /// <summary>
        /// Service is pending stop
        /// </summary>
        SERVICE_STOP_PENDING = 0x00000003,
        /// <summary>
        /// Service is running
        /// </summary>
        SERVICE_RUNNING = 0x00000004,
        /// <summary>
        /// Service continuously waiting
        /// </summary>
        SERVICE_CONTINUE_PENDING = 0x00000005,
        /// <summary>
        /// Service is pending pause
        /// </summary>
        SERVICE_PAUSE_PENDING = 0x00000006,

        /// <summary>
        /// service is in pause
        /// </summary>
        SERVICE_PAUSED = 0x00000007,
    }
    /// <summary>
    /// Container structure of service states
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        /// <summary>
        /// 
        /// </summary>
        public int dwServiceType;
        /// <summary>
        /// Stores the current status of the service
        /// </summary>
        public ServiceState dwCurrentState;
        /// <summary>
        /// 
        /// </summary>
        public int dwControlsAccepted;
        /// <summary>
        /// 
        /// </summary>
        public int dwWin32ExitCode;
        /// <summary>
        /// 
        /// </summary>
        public int dwServiceSpecificExitCode;
        /// <summary>
        /// 
        /// </summary>
        public int dwCheckPoint;
        /// <summary>
        /// 
        /// </summary>
        public int dwWaitHint;
    };
    public partial class SemanticProcessorService : ServiceBase
    {
        private const int K_MILLI = 2500;
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
        // Componente EventLog per scrivere nel registro eventi di Windows
        private EventLog eventLog1;
        private Timer SchedularWD;  //Watch dog scheduling time
        protected Logger _logger;
        private int TimerWatchDog = 0;
        private short StepStartingService = 0; //Steps for starting the service correctly
        private bool SemaforoDC = false; // (semaforo) serve per evitare l'over run durante l'avvio del servizio
        private int logCountSrv = 0; //Timer for the issuance of the service notification is working
        private int swDebug = 0;
        private string _path = "";
        private long HandlerError = 0;

        private volatile bool _isRunning = false; // volatile per garantire visibilità tra thread
        // Flag per indicare se il processo semantico è in pausa
        private volatile bool _isPaused = false; // volatile per garantire visibilità tra thread

        private static string ServicePath = ""; //22.03.2022 Gestione riavvio tablet per timeout, nome della cartella ove risiede ilò servizio
        protected Mutex SyncMtxLogger = new Mutex();
        private ManualResetEvent _pauseEvent = new ManualResetEvent(true); // Inizializzato su 'true' (segnale) per partire non in pausa
        protected DateTime today = DateTime.MinValue;
        private string ServiceVer = "";
        private static ClsCommandHandlers commandHandlers;
        #region Instance of the asynchronous socket server class
        private AsyncSocketListener asl;
        protected AsyncSocketThread scktThrd;
        protected Thread thSocket;
        private bool sysCmdRunnig = false; // lock dell'esecuzione di un comando se uno è già in corso
        private int CommandRunning = 0; // se true c'è un comando in corso 
        #endregion
        private string VersionDate = "07.05.2025";// Cambiare ad ogni release 

        private List<Database> _loadedDatabases;
        private readonly ConcurrentDictionary<string, SemanticClientSocket> _connectedUiClients;
        private ClsCommandHandlers _commandHandlers;
        #region dichiarazioni per gestire la concorrenza d'accesso a canale socket
        private static ConcurrentDictionary<string, string> concurrentData = new ConcurrentDictionary<string, string>(); //2025.05.20 lista per accessi concorrenti al clien socket asincrono
        private ConcurrentQueue<Tuple<SocketMessageStructure, Socket>> _receivedMessageQueue;

        #endregion

#if DEBUG
        private bool firstInDebug = false;
#endif

        /// <summary>
        /// costrutt
        /// </summary>
        public SemanticProcessorService()
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                InitializeComponent();

#if !DEBUG

                eventLog1 = new EventLog();
                if (!EventLog.SourceExists("SemanticProcessorSource"))
                {
                    EventLog.CreateEventSource("SemanticProcessorSource", "SemanticProcessorLog");
                }
                eventLog1.Source = "SemanticProcessorSource";
                eventLog1.Log = "SemanticProcessorLog";
#endif

                this.CanPauseAndContinue = true; // *** Abilita la capacità di pausa/ripresa ***
                this.CanStop = true;
                this.CanShutdown = true; // Permette al servizio di rispondere allo shutdown del sistema
                this.CanHandleSessionChangeEvent = false; // Non necessario per ora
                this.CanHandlePowerEvent = false; // Non necessario per ora

                swDebug = Convert.ToInt32(ConfigurationManager.AppSettings["DebugLev"]); ;
                _path = ConfigurationManager.AppSettings["FolderLOG"];
                _logger = new Logger(_path, "SemanticProcessor", SyncMtxLogger);
                _logger.SwLogLevel = swDebug;
                _logger.Log(LogLevel.INFO, string.Format("Log path:{0}", _logger.GetPercorsoCompleto()));
                today = DateTime.Now;

                //_receivedMessageQueue = new ConcurrentQueue<Tuple<SocketMessageStructure, Socket>>();
                //_loadedDatabases = new List<Database>();
                _connectedUiClients = new ConcurrentDictionary<string, SocketManager.SemanticClientSocket>();

                commandHandlers = new ClsCommandHandlers(_logger, concurrentData);
                StartServerSocket();
                if (swDebug > 0)
                {
                    // List of activated log messages
                    _logger.Log(LogLevel.INFO, "Log level: " + swDebug.ToString());
                    ShowLogLevel();
                }
                else
                {
                    _logger.Log(LogLevel.WARNING, SemSerRes.logDisabled);
                }
                // accendo il timer per il duty cycle e lo disattivo subito per evitare l'errore nel duty cycle 13/5/2025
                this.myStart();
                swWatchDog(false, 0);
                StartThread();
            }catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, ClsMessaggiErrore.CustomMsg(ex, thisMethod));
            }
        }
        protected override void OnPause()
        {
            _logger.Log(LogLevel.INFO, SemSerRes.logMsgServicePaused);
            Console.WriteLine("Servizio SemanticProcessor: OnPause chiamato."); // *** Log aggiuntivo ***
            // Aggiungi logging alla pausa
            // _logger.LogInfo("Servizio SemanticProcessor in pausa...");
            Console.WriteLine("Servizio SemanticProcessor in pausa..."); // Log di fallback

            // Imposta il flag di pausa
            this._isPaused = true;
            // Resetta l'evento di pausa. Questo bloccherà il thread semantico nel suo loop.
            _pauseEvent.Reset(); // Imposta l'evento su 'non segnalato' (blocca)
            Console.WriteLine($"Servizio SemanticProcessor: OnPause completato. _isPaused = {_isPaused}"); // *** Log aggiuntivo ***
            // Segnala all'SCM che il servizio è in pausa
            // _logger.LogInfo("Servizio SemanticProcessor in pausa.");
            Console.WriteLine("Servizio SemanticProcessor in pausa."); // Log di fallback
            base.OnPause();
            this.StepStartingService = -1;
            swWatchDog(false, 0);
            
        }
        protected override void OnContinue()
        {
            base.OnContinue();
            // Imposta il flag di pausa a false
            this._isPaused = false;
            _logger.Log(LogLevel.INFO, SemSerRes.logMsgServiceRunning);
            // Segnala l'evento di pausa. Questo sbloccherà il thread semantico.
            _pauseEvent.Set(); // Imposta l'evento su 'segnalato' (sblocca)
            this.StepStartingService = 1;
            swWatchDog(true, 0);
        }
        protected override void OnStart(string[] args)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                Assembly tySematicProcessor = typeof(SemanticProcessor).Assembly;
                AssemblyName SematicProcessorVER = tySematicProcessor.GetName();
                Version ver = SematicProcessorVER.Version;
                this.ServiceVer = ver.ToString();
                ServiceStatus serviceStatus = new ServiceStatus();
                _logger.Log(LogLevel.INFO, "serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;");
                serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
                _logger.Log(LogLevel.INFO, "serviceStatus.dwWaitHint = 30000;");
                serviceStatus.dwWaitHint = 30000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                _logger.Log(LogLevel.INFO, "SetServiceStatus(this.ServiceHandle, ref serviceStatus);");
                _logger.Log(LogLevel.WARNING, "VER: " + this.ServiceVer);
                _logger.Log(LogLevel.WARNING, "VER DATE:" + this.VersionDate);
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                if (_logger != null)
                {
                    _logger.Log(LogLevel.INFO, SemSerRes.logMsgSrvStarted);
                    _logger.Log(LogLevel.INFO, string.Format("Current full name {0}", tySematicProcessor.FullName));
                    ServicePath = Path.GetDirectoryName(tySematicProcessor.Location);
                    _logger.Log(LogLevel.INFO, string.Format("Current Location name {0}", ServicePath));
                }
                else
                {
                    EventLog.WriteEntry(string.Format("No instances for the event log - {0}", SemSerRes.logMsgSrvStarted), EventLogEntryType.Error, 0x7f0f);
                }

                _isRunning = true; // Imposta il flag di esecuzione
                _isPaused = false; // Assicurati che non sia in pausa all'avvio
                _pauseEvent.Set(); // Assicurati che l'evento di pausa sia segnalato (non bloccato)
                //_cancellationTokenSource = new CancellationTokenSource(); // Crea un nuovo token di cancellazione

                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                this.myStart();
                int IntervalMilliSeconds = 1000;
                swWatchDog(true, IntervalMilliSeconds);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, string.Format("{0}, Pos={1}", ClsMessaggiErrore.CustomMsg(ex, thisMethod), 0));
                Process currentProcess = Process.GetCurrentProcess();
                currentProcess.Kill();
            }
        }
        protected override void OnStop()
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                _logger.Log(LogLevel.INFO, "STOP REQUEST");


                // Segnala al thread del motore semantico di fermarsi
                _isRunning = false; // Imposta il flag di esecuzione a false
                                    // Assicurati che il thread non sia bloccato in pausa prima di segnalare l'arresto
                _pauseEvent.Set(); // Sblocca il ManualResetEvent nel caso fosse in pausa
                                   // Segnala l'annullamento tramite CancellationTokenSource
                //if (_cancellationTokenSource != null)
                //{
                //    _cancellationTokenSource.Cancel();
                //}

                if (asl != null)
                {
                    asl.DataFromSocket -= Asl_DataFromSocket;
                    asl.CloseServerSocket(1000);
                    scktThrd.StopThread = true;
                }
                //if (_cancellationTokenSource != null)
                //{
                //    _cancellationTokenSource.Dispose();
                //    _cancellationTokenSource = null;
                //}

                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
                serviceStatus.dwWaitHint = 10000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                _logger.Log(LogLevel.INFO, "SERVICE STOPPED");
                // Update the service state to Stopped.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.ERROR, string.Format("{0} pos= {1}", ClsMessaggiErrore.CustomMsg(ex, thisMethod), 0));
            }
        }
        #region async send
        private async Task NotificaTutteLeUIs(SocketMessageStructure notifica)
        {
            foreach (var clientSocketPair in _connectedUiClients)
            {
                string clientId = clientSocketPair.Key;
                SemanticClientSocket clientSocket = clientSocketPair.Value;

                try
                {
                    clientSocket.MessageSentFailed += ClientSocket_MessageSentFailed;
                    // Non è necessario connettersi qui, dovremmo già essere connessi.
                    // Controlla lo stato della connessione se necessario.
                    // if (!clientSocket.IsConnected) { ... }

                    if (_logger == null)
                    {
                        eventLog1.WriteEntry($"Invio notifica '{notifica.MessageType}' (Token: {notifica.Token}) alla UI {clientId}.");
                    }
                    else
                    {
                        _logger.Log(LogLevel.INFO, $"Invio notifica '{notifica.MessageType}' (Token: {notifica.Token}) alla UI {clientId}.");
                    }
                    bool inviato = await clientSocket.SendMessageAsync(notifica);
                    if (inviato)
                    {
                        if (_logger == null)
                        {
                            eventLog1.WriteEntry($"Notifica inviata. Attendo ACK dalla UI {clientId}.");
                        }
                        else
                        {
                            _logger.Log(LogLevel.INFO, $"Notifica inviata. Attendo ACK dalla UI {clientId}.");
                        }

                        SocketMessageStructure ack = await clientSocket.ReceiveMessageAsync(timeoutMs: 30000); // Timeout di 3 secondi per l'ACK

                        if (ack != null && ack.MessageType == "INFO" && ack.Token == notifica.Token) 
                        {
                            if (_logger == null)
                            {
                                eventLog1.WriteEntry($"Ricevuto ACK dalla UI {clientId} per notifica '{notifica.MessageType}' (Token: {notifica.Token}).");
                            }
                            else
                            {
                                _logger.Log(LogLevel.INFO, $"Ricevuto ACK dalla UI {clientId} per notifica '{notifica.MessageType}' (Token: {notifica.Token})");
                            }
                                
                        }
                        else
                        {
                            string responseInfo = ack != null ? $"Comando='{ack.Command}', Token='{ack.Token}'" : "Nessuna risposta ricevuta (timeout).";
                            if (_logger == null)
                            {
                                eventLog1.WriteEntry($"Nessun ACK valido ricevuto dalla UI {clientId} per notifica '{notifica.MessageType}' (Token: {notifica.Token}). Risposta: {responseInfo}", EventLogEntryType.Warning);
                            }
                            else
                            {
                                _logger.Log(LogLevel.WARNING, $"Nessun ACK valido ricevuto dalla UI {clientId} per notifica '{notifica.MessageType}' (Token: {notifica.Token}). Risposta: {responseInfo}");
                            }
                                
                            // Qui potresti decidere di gestire la mancata ricezione dell'ACK
                        }
                    }
                    else
                    {
                        if (_logger == null)
                        {
                            eventLog1.WriteEntry($"Fallito l'invio della notifica alla UI {clientId}.", EventLogEntryType.Warning);
                        }
                        else
                        {
                            _logger.Log(LogLevel.WARNING, $"Fallito l'invio della notifica alla UI {clientId}.");
                        }
                            
                        // Gestisci il fallimento dell'invio a questo client
                    }
                }
                catch (Exception ex)
                {
                    if (_logger == null)
                    {
                        eventLog1.WriteEntry($"Errore durante la comunicazione con la UI {clientId}: {ex.Message}", EventLogEntryType.Error);
                    }
                    else
                    {
                        _logger.Log(LogLevel.ERROR, $"Errore durante la comunicazione con la UI {clientId}: {ex.Message}");
                    }

                    // Potresti voler gestire qui la disconnessione o il tentativo di riconnessione
                }
                finally
                {
                    clientSocket.MessageSentFailed -= ClientSocket_MessageSentFailed;
                }
                // Non chiudiamo la connessione qui, poiché sono connessioni persistenti gestite da _connectedUiClients.
            }
        }

        private void ClientSocket_MessageSentFailed(object sender, SemanticClientSocket.MessageSentEventArgs e)
        {
            SemanticClientSocket locaClient = sender as SemanticClientSocket;
            //locaClient.IsConnected = false;
        }

        public async Task EseguiNotificaUIs()
        {
            SocketMessageStructure notifica = new SocketMessageStructure
            {
                MessageType = "NuoviDatiDisponibili",
                Token = Guid.NewGuid().ToString(),
                BufferDati = new System.Xml.Linq.XElement("Data", $"Dati aggiornati alle {DateTime.Now}")
            };

            await NotificaTutteLeUIs(notifica);
        }
        #endregion
        /// <summary>
        /// Service duty cycle
        /// </summary>
        public void MainCycleService()
        {

            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            int IntervalMilliSeconds = 0;
            try
            {
                swWatchDog(false, IntervalMilliSeconds);
#if DEBUG
                if (!firstInDebug)
                {
                    Assembly tyWinTTab = typeof(SemanticProcessor).Assembly;
                    AssemblyName WinTTabVER = tyWinTTab.GetName();
                    Version ver = WinTTabVER.Version;
                    this.ServiceVer = ver.ToString();
                    Thread.Sleep(100);
                    this.myStart();
                    swWatchDog(true, IntervalMilliSeconds);
                }
#endif

#if DEBUG
                if (!firstInDebug)
                {
                    firstInDebug = true;
                }
#endif
                if (this.StepStartingService != -1)
                {
                    //_logger.Log(LogLevel.INFO, "Sun'mi sun't chi");
                    IntervalMilliSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalMilliSeconds"]);
                    IntervalMilliSeconds = IntervalMilliSeconds < K_MILLI ? K_MILLI : IntervalMilliSeconds;
                    Thread.Sleep(100);
                    if (scktThrd != null)
                        _logger.Log(LogLevel.SERVICE_EVENT, string.Format("THREAD SOCKET IS STARTED = {0} STEPS START = {1}", scktThrd.IsStarted, StepStartingService));
                    else
                    {
                        _logger.Log(LogLevel.ERROR, "THREAD SOCKET NOT STARTED ");
#if !DEBUG
                        ServiceController[] scServices;
                        scServices = ServiceController.GetServices();
                        var scs = scServices.Where(SC => SC.ServiceName == "WinTTabServiceCom-01");
                        if (scs.Any())
                        {
                            Process currentProcess = Process.GetCurrentProcess();
                            _logger.Log(LogLevel.WARNING, string.Format("Process {0} stopped", currentProcess.ProcessName));
                            currentProcess.Kill();
                        }
#endif
                    }
                    _logger.Log(LogLevel.SERVICE, string.Format("Step status: {0}", StepStartingService));
                }
                switch (StepStartingService)
                {
                    case 0:
                        {
                            #region Avvio socket
                            if (!this.SemaforoDC)
                            {
                                this.SemaforoDC = true;
                                IntervalMilliSeconds = 500;
                                if (!scktThrd.IsStarted)
                                {
                                    _logger.Log(LogLevel.INFO, "Open socket");
                                    thSocket.Start();
                                    this.myStart();
                                    swWatchDog(true, IntervalMilliSeconds);

                                }
                                else
                                {
                                    StepStartingService = 1;
                                }
                                this.SemaforoDC = false;
                            }
                            else
                            {
                                _logger.Log(LogLevel.DEBUG, SemSerRes.logMsgStartThread);
                            }
                            #endregion
                        }
                        break;
                    case 1:
                        {
                            if (!this.SemaforoDC)
                            {
                                this.SemaforoDC = true;
                                Socket soc = null;
                                if (asl.Listner != null)
                                {
                                    soc = new Socket(asl.Listner.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                                    if (asl.SocketConnected(soc))
                                    {
                                        _logger.Log(LogLevel.DEBUG, "The socket exists");
                                    }
                                    else
                                    {
                                        _logger.Log(LogLevel.WARNING, "Socket not found");
                                    }
                                    soc.Close();
                                    soc.Dispose();
                                    SemaforoDC = false;
                                }
#if !DEBUG
                                manageService();
#endif
                                string pathFileCtrl = Path.Combine(Directory.GetCurrentDirectory(), "CheckFile.txt");
                                if (File.Exists(pathFileCtrl))
                                {
                                    _logger.Log(LogLevel.INFO, $"Trovato file:{pathFileCtrl}");
                                    Random randomGenerator = new Random();
                                    int token = randomGenerator.Next(0, int.MaxValue);
                                    string msgFile = "";
                                    using (StreamReader sr = new StreamReader(pathFileCtrl))
                                    {
                                        msgFile = sr.ReadToEnd();
                                        _logger.Log(LogLevel.INFO, string.Format("Time: {0} Message: {1}", DateTime.Now, msgFile));
                                    }
                                    SocketMessageStructure msg = new SocketMessageStructure()
                                    {
                                        Command = null,
                                        SendingTime = DateTime.Now,
                                        Token = token.ToString(),
                                        MessageType = "INFO",
                                        BufferDati = new XElement("BufferDati",new XElement("message", msgFile))
                                    };
                                    //string telegramGenerate = SocketMessageSerializer.SerializeUTF8(msg);
                                    //ASCIIEncoding encoding = new ASCIIEncoding();
                                    //byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                                    ////string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
                                    //string txtSendData = SocketMessageSerializer.Base64Start + Convert.ToBase64String(bytes, 0, bytes.Length) + SocketMessageSerializer.Base64End;
                                    Task.Run(() => NotificaTutteLeUIs(msg));
                                    foreach (var clientSocketPair in _connectedUiClients)
                                    {
                                        _logger.Log(LogLevel.DEBUG, $"*** Is Connect = {clientSocketPair.Value.IsConnected} ***");
                                        
                                    }
                                    File.Delete(pathFileCtrl);  
                                }
                                _logger.Log(LogLevel.DEBUG, thSocket.IsAlive ? "Thread is alive" : "Thread is dead");
                                this.SemaforoDC = false;
                            }
                        }
                        break;
                       
                    default:
                        {

                        }
                        break;
                }
                //this.myStart();
                swWatchDog(true, IntervalMilliSeconds);
            }
            catch (Exception ex)
            {
                string msg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                _logger.Log(LogLevel.ERROR, msg);
                //this.myStart();
                swWatchDog(true, IntervalMilliSeconds);
            }
        }
        /// <summary>
        /// **** Service duty cycle
        /// </summary>
        /// <param name="e"></param>
        private void SchedularWatchdog(object e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
#if DEBUG
            // Console.WriteLine("Service is running comunication is {0}", paxAires8.ComIsOpen == true ? "OPEN" : "CLOSE");
#else
            int IntervalMilliSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalMilliSeconds"]);
            IntervalMilliSeconds = IntervalMilliSeconds < K_MILLI ? K_MILLI : IntervalMilliSeconds;

            //if ((this.swDebug & _logger.LOG_SERVICE_EVENT) == _logger.LOG_SERVICE_EVENT)
            {

                float count = (60000 / IntervalMilliSeconds) >= 1 ? 60000 : IntervalMilliSeconds / 60000;
                
                if (logCountSrv > count)
                {
                    _logger.Log(LogLevel.INFO, Resources.logMsgSrvRun);
                    logCountSrv = 0;
                }
                logCountSrv++;
            }
#endif
            try
            {
                TimerWatchDog++;
                //When the watchdog timer is triggered, the timer is started only by MainCycleService
                SchedularWD.Change(Timeout.Infinite, Timeout.Infinite);
                MainCycleService();
            }
            catch (Exception ex)
            {
                string msg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                //if ((this.swDebug & _logger.LOG_SERVICE_EVENT) == _logger.LOG_SERVICE_EVENT)
                _logger.Log(LogLevel.ERROR, msg);
            }
        }
        private void manageService()
        {
            _logger.Log(LogLevel.DEBUG, "manageService");
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();
            var scs = scServices.Where(SC => SC.ServiceName == "WinTTabServiceCom-01");
            if (scs.Any())
            {
                ServiceController sc = scs.First();
                //if ((swDebug & _logger.LOG_SERVICE) == _logger.LOG_SERVICE)
                {
                    _logger.Log(LogLevel.SERVICE, string.Format(SemSerRes.logMsgSrvcStatus, sc.Status));
                    _logger.Log(LogLevel.SERVICE, string.Format(SemSerRes.logMsgPauseCont, sc.CanPauseAndContinue));
                    _logger.Log(LogLevel.SERVICE, string.Format(SemSerRes.logMsgCanShutDwn, sc.CanShutdown));
                    _logger.Log(LogLevel.SERVICE, string.Format(SemSerRes.logMsgCanStop, sc.CanStop));
                }
#if DEBUG
                Console.WriteLine(string.Format("Status = {0}", sc.Status));
                Console.WriteLine(string.Format("Can Pause and Continue = {0}", sc.CanPauseAndContinue));
                Console.WriteLine(string.Format("Can ShutDown = {0}", sc.CanShutdown));
                Console.WriteLine(string.Format("Can Stop = {0}", sc.CanStop));
#endif
#if !DEBUG
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    // if ((swDebug & _logger.LOG_INFO) == _logger.LOG_INFO)
                    _logger.Log(LogLevel.INFO, SemSerRes.logMsgWaitStart);
                    Thread.Sleep(30000);
                    sc.Start();
                }
#endif
                sc.Dispose();
                scs.First().Dispose();
            }
        }
        /// <summary>
        /// Accende e spegne il temporizzatore del cane da guardia
        /// </summary>
        /// <param name="swWD">Accende spegne l'orologio del watchdog se true acceso se false spento</param>
        /// <param name="milliSec"></param>
        private void swWatchDog(bool swWD, int milliSec = 10000)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();

            if (swWD)
                SchedularWD.Change(milliSec, Timeout.Infinite);
            else
                SchedularWD.Change(Timeout.Infinite, Timeout.Infinite);
            WatchDogStatus = swWD;
        }
        /// <summary>
        /// Activation of the main duty cycle start programming clock
        /// </summary>
        private void myStart()
        {
            SchedularWD = new Timer(new TimerCallback(SchedularWatchdog));      // Create watchdog timer
        }
        private void ShowLogLevel()
        {
            Type tyLogLv = _logger.GetType();
            var pLogLv = tyLogLv.GetProperties();
            double index = 0;
            foreach (PropertyInfo pinfo in pLogLv)
            {
                long p = (Int64)Math.Pow(2, index);
                if ((swDebug & p) == p)
                {
                    _logger.Log(LogLevel.INFO, pinfo.Name);
                }
                index++;
            }
        }
        #region atctivation socket and managing socket comunication
        /// <summary>
        /// Enabling the server socket
        /// </summary>
        private void StartServerSocket()
        {
            asl = new AsyncSocketListener(ConfigurationManager.AppSettings["RemotePortList"], _logger);
            asl.Echo = false;
            asl.SwDebug = swDebug;
            asl.DataFromSocket += Asl_DataFromSocket;
            asl.ErrorFromSocket += Asl_ErrorFromSocket;
            asl.TokenSocket = 0x7FFFFFFF;
        }
        /// <summary>
        /// Avvio thread per server soc
        /// </summary>
        private void StartThread()
        {
            scktThrd = new AsyncSocketThread();
            scktThrd.Log = _logger;
            scktThrd.AsyncSocketListener = asl;
            scktThrd.Interval = 100;
            thSocket = new Thread(scktThrd.AsyncSocket);
        }
        /// <summary>
        /// looking if I got some command from the socket
        /// </summary>
        /// <returns></returns>
        private SocketCommand checkCommandFromSocket(string cmd, string TokenSocket = "")
        {
            // 02.04.2021 gestione del token
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            SocketCommand scktCmd = new SocketCommand();
            //string ret = "";
            try
            {
#if DEBUG
                Console.WriteLine("********" + cmd + "**********");
#endif
                Type tySktCmd = scktCmd.GetType();
                var getSktCmd = tySktCmd.GetProperties().Where(CMD => CMD.Name.ToUpper().Equals(cmd.ToUpper()));
                if (getSktCmd.Any())
                {
                    // 02.04.2021 gestione accesso concorrente alla tavoletta 
                    PropertyInfo fInfo = getSktCmd.First();
                    var call = fInfo.CustomAttributes.First().NamedArguments.Where(CALL => CALL.MemberName == "TockenManaging");
                    if (call.Any())
                    {
                        //var vToken = tySktCmd.GetProperties().Where(A => A.Name == "AccessToken");
                        //if (vToken.Any())
                        {
                            switch ((byte)call.First().TypedValue.Value)
                            {
                                case 0: // controllo validità del token 
                                    {
                                        _logger.Log(LogLevel.INFO, string.Format("Check validity token for the command {0}. Received: {1} stored: {2}", cmd, TokenSocket, asl.TokenSocket));
                                        if (asl.TokenSocket == Convert.ToInt32(TokenSocket)) 
                                        {
                                            //SocketCommand mysc = getSktCmd.First();
                                            getSktCmd.First().GetValue(scktCmd).ToString();
                                            var val = ((System.Reflection.TypeInfo)tySktCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("CommandSocket")).First();
                                            val.SetValue(scktCmd, getSktCmd.First().Name);
                                        }
                                        else
                                        {
                                            getSktCmd.First().GetValue(scktCmd).ToString();
                                            var val = ((System.Reflection.TypeInfo)tySktCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("CommandSocket")).First();
                                            val.SetValue(scktCmd, "CmdTokenMismatch");
                                            //_logger.Log(LogLevel.WARNING, msg);
                                            //HandlerError |= 0x400;  //System busy reporting event
                                            //asl.Send(asl.Handler, msg);
                                        }
                                        break;
                                    }
                                case 1: // assegnazione token
                                    {
                                        sysCmdRunnig = false; // 09.06.2021 when token is assigned the flag command runnig to be false
                                        if (Convert.ToUInt32(TokenSocket) != 0) 
                                        {
                                            _logger.Log(LogLevel.INFO, string.Format(SemSerRes.logAssignToken, cmd, asl.TokenSocket, TokenSocket));
                                            asl.TokenSocket = Convert.ToInt32(TokenSocket);
                                        }
                                        else
                                        {
                                            _logger.Log(LogLevel.INFO, string.Format("command: {0} token value: {1}. Token does not assigned", cmd, TokenSocket));
                                        }
                                        getSktCmd.First().GetValue(scktCmd).ToString();
                                        var val = ((System.Reflection.TypeInfo)tySktCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("CommandSocket")).First();
                                        val.SetValue(scktCmd, getSktCmd.First().Name);
                                        break;
                                    }
                                case 2: // comandi per cui il token non deve essere controllato
                                    {
                                        _logger.Log(LogLevel.INFO, string.Format("for the {0} command the token does not checked", cmd));
                                        getSktCmd.First().GetValue(scktCmd).ToString();
                                        var val = ((System.Reflection.TypeInfo)tySktCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("CommandSocket")).First();
                                        val.SetValue(scktCmd, getSktCmd.First().Name);
                                        break;
                                    }
                                default:
                                    {
                                        string msg = string.Format("The command {0} does not support tokens. {1} Time: {2}", cmd, SemSerRes.logErrTockenMismatch, DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"));
                                        _logger.Log(LogLevel.WARNING, msg);
                                        HandlerError |= 0x2000;  //Command does not support tokens
                                        asl.Send(asl.Handler, msg);
                                        break;
                                    }
                            }
                        }
                        var sp = fInfo.CustomAttributes.First().NamedArguments.Where(CALL => CALL.MemberName == "SendingDataPackets");
                        if (sp.Any())
                        {
                            getSktCmd.First().GetValue(scktCmd).ToString();
                            var val = ((System.Reflection.TypeInfo)tySktCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("SendingDataPackets")).First();
                            val.SetValue(scktCmd, sp.First().TypedValue.Value);
                        }
                    }
                }
                else
                {
                    throw new Exception(SemSerRes.logErrCmdNotFound);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ClsMessaggiErrore.CustomMsg(ex, thisMethod));
            }
            return scktCmd;
        }
        #endregion
        #region Socket server events
        private void Asl_DataFromSocket(object sender, SocketMessageStructure e)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            Socket Handler = sender as Socket;
            SocketCommand cmdCom = null;
            SocketCommand cmdCom1 = null;
            int erCnt = 0;
            int cnt = 0;
            try
            {
                erCnt = 1;
                swWatchDog(false); // condizione di break e.Command=="CmdSendFormConfiguration"
                    
                cmdCom1 = checkCommandFromSocket(e.Command, e.Token.ToString());

                _logger.Log(LogLevel.DEBUG, "* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *");
                _logger.Log(LogLevel.DEBUG, string.Format("* * * *  {0} * * * * {1} * * * *", e.Command.ToUpper(), e.Token));
                _logger.Log(LogLevel.DEBUG, "* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *");

                if (cmdCom1 != null)
                {
                    cmdCom = cmdCom1;
                    _logger.Log(LogLevel.ENANCED_DEBUG, "sysCmdRunnig status on received comand from soket: " + sysCmdRunnig.ToString());
                    //16.06.2021
                    while (sysCmdRunnig)
                    {
                        Thread.Sleep(20);
                        if (cnt == 10)
                            sysCmdRunnig = false;
                        else
                            cnt++;
                        _logger.Log(LogLevel.ENANCED_DEBUG, "- WAITING CYCLES: " + cnt.ToString());
                    }
                    _logger.Log(LogLevel.ENANCED_DEBUG, "At start command sysCmdRunnig set to true");
                    sysCmdRunnig = true; // 09.06.2021 interlock esecuzione comandi
                    CommandRunning = 1; 
                    erCnt = 2;
                    Type tyCommandHandlers = typeof(ClsCommandHandlers);
                    ClsCustomBinder myCustomBinder = new ClsCustomBinder();
                    SocketCommand myO = new SocketCommand();
                    myO.LocalEndPoint= Handler.RemoteEndPoint.ToString();
                    myO.PortAddress = asl.CallerIpAddress;
                    myO.SocketPortSrv = asl.SrvPort;
                    myO.SocketAddressClient = asl.SrvIpAddress;
                    myO.SocketHandler = Handler;
                    myO.SocketPortCli= Convert.ToInt32(ConfigurationManager.AppSettings["SocketPortClient"]);
                    myO.Response = new object();
                    erCnt = 4;
                    string metodo;
                    MethodInfo myMethod = null;
                    //MethodInfo myMethod = tyCommandHandlers.GetMethod(cmdCom.CommandSocket, BindingFlags.Public | BindingFlags.Instance, myCustomBinder, new Type[] { typeof(SocketCommand), typeof(string), typeof(AsyncSocketListener) }, null);
                    myMethod = tyCommandHandlers.GetMethod(
                        cmdCom.CommandSocket, // Nome del metodo
                        BindingFlags.Public | BindingFlags.Instance, // Cerca metodi pubblici d'istanza
                        myCustomBinder, // Usa il tuo binder per la risoluzione
                        new Type[] { typeof(SocketCommand), typeof(XElement), typeof(AsyncSocketListener) }, // Tipi dei parametri (verifica che siano corretti!)
                        null // Modificatori
                    );
                    if (myMethod != null)
                    {
                        metodo = myMethod.Name;
                        _logger.Log(LogLevel.INFO, "<START COMMAND>");
                        _logger.Log(LogLevel.DEBUG, string.Format("{0} {1} param DeviceCommand, string", SemSerRes.logMsgCmdRved, metodo));
                        erCnt = 5;
                        // Invoke the overload.
                        //tyCommandHandlers.InvokeMember(metodo, BindingFlags.InvokeMethod, myCustomBinder, commandHandlers, new Object[] { myO, e.Data, asl });
                        object result = myMethod.Invoke(commandHandlers, new Object[] { myO, e.BufferDati, asl });
                        // if (myO.CommandCode == myO.CmdSendFormConfiguration) _logger.Log(LogLevel.DEBUG,string.Format( "Sono uscito dal comando {0}", myO.CommandName));
                    }
                    else
                    {
                        myMethod = tyCommandHandlers.GetMethod(
                        cmdCom.CommandSocket, // Nome del metodo
                        BindingFlags.Public | BindingFlags.Instance, // Cerca metodi pubblici d'istanza
                        myCustomBinder, // Usa il tuo binder per la risoluzione
                        new Type[] { typeof(SocketCommand), typeof(XElement), typeof(AsyncSocketListener), typeof(ConcurrentDictionary<string, SemanticClientSocket>) }, // Tipi dei parametri (verifica che siano corretti!)
                        null
                        );// Modificatori
                        if (myMethod != null)
                        {
                            metodo = myMethod.Name;
                            _logger.Log(LogLevel.INFO, "<START COMMAND>");
                            _logger.Log(LogLevel.DEBUG, string.Format("{0} {1} param DeviceCommand, string", SemSerRes.logMsgCmdRved, metodo));
                            object result = myMethod.Invoke(commandHandlers, new Object[] { myO, e.BufferDati, asl, _connectedUiClients });
                        }
                        else
                        {
                            throw new Exception("Command not found");
                        }
                    }
                    /*
                    Type TyPaxAires8 = typeof(PAXAires8Com);
                    //ClsCustomBinder myCustomBinder = new ClsCustomBinder();
                    ClsCustomBinder myCustomBinder = new ClsCustomBinder();
                    DeviceCommand myO = new DeviceCommand();
                    erCnt = 3;
                    myO.LocalEndPoint = Handler.RemoteEndPoint.ToString(); // 03.12.2021
                    myO.PortAddress = asl.CallerIpAddress; //  ConfigurationManager.AppSettings["SocketAddressClient"]; 20.07.02
                    myO.SocketPortSrv = asl.SrvPort; // port a cui si deve connettere la libreria //Convert.ToInt32(ConfigurationManager.AppSettings["SocketPortClient"]);
                    myO.SocketAddressClient = asl.SrvIpAddress;
                    myO.SocketHandler = Handler;
                    myO.SocketPortCli = Convert.ToInt32(ConfigurationManager.AppSettings["SocketPortClient"]);
                    // 12.11.2020 Da oggi devo scegliere la porta da mandare la libreria myO.SocketPortCli = Convert.ToInt32(ConfigurationManager.AppSettings["SocketPortClient"]);
                    myO.Response = new object();
                    erCnt = 4;
                    string metodo;
                    MethodInfo myMethod = TyPaxAires8.GetMethod(cmdCom.CommandSocket, BindingFlags.Public | BindingFlags.Instance, myCustomBinder, new Type[] { typeof(DeviceCommand), typeof(string) }, null);
                    //17.08.2020 modifica per ritornare licenza
                    if (myMethod != null)
                    {
                        metodo = myMethod.Name;
                        //if ((this.swDebug & _logger.LOG_INFO) == _logger.LOG_INFO) 
                        _logger.Log(LogLevel.INFO, "<START COMMAND>");
                        if (logDelay != null)
                            logDelay.Log(LogLevel.INFO, string.Format("1) COMMAND {0}; STARTED AT; {1}", metodo, DateTime.Now.Ticks.ToString()));
                        //if ((this.swDebug & _logger.LOG_DEBUG) == _logger.LOG_DEBUG) 
                        _logger.Log(LogLevel.DEBUG, string.Format("{0} {1} param DeviceCommand, string", WinTTabRes.logMsgCmdRved, metodo));
                        erCnt = 5;
                        // Invoke the overload.
                        TyPaxAires8.InvokeMember(cmdCom.CommandSocket, BindingFlags.InvokeMethod, myCustomBinder, paxAires8, new Object[] { myO, e.Data });
                        // if (myO.CommandCode == myO.CmdSendFormConfiguration) _logger.Log(LogLevel.DEBUG,string.Format( "Sono uscito dal comando {0}", myO.CommandName));
                    }
                    else
                    {
                        //if ((this.swDebug & _logger.LOG_INFO) == _logger.LOG_INFO) 
                        myMethod = TyPaxAires8.GetMethod(cmdCom.CommandSocket, BindingFlags.Public | BindingFlags.Instance, myCustomBinder, new Type[] { typeof(DeviceCommand), typeof(string), typeof(string) }, null);
                        if (myMethod != null)
                        {
                            if (logDelay != null)
                                logDelay.Log(LogLevel.INFO, string.Format("2) COMMAND {0}; STARTED AT; {1}", myMethod, DateTime.Now.Ticks.ToString()));
                            metodo = myMethod.Name;
                            //if ((this.swDebug & _logger.LOG_DEBUG) == _logger.LOG_DEBUG)
                            _logger.Log(LogLevel.INFO, "<START COMMAND>");
                            _logger.Log(LogLevel.DEBUG, string.Format("{0} {1} param DeviceCommand, string, string", WinTTabRes.logMsgCmdRved, metodo));
                            erCnt = 51;
                            // Invoke the overload.
                            // 20.05.2021 TyPaxAires8.InvokeMember(cmdCom.CommandSocket, BindingFlags.InvokeMethod, myCustomBinder, paxAires8, new Object[] { myO, e.Data, string.Format("{0}, {1}", License.udid2, License.udid) });

                            // 06.06.2021
                            string sudid = "";
                            if (License.udid.GetType() == typeof(System.Xml.XmlNode[]))
                            {
                                sudid = ((System.Xml.XmlNode[])License.udid)[0].Value;
                            }
                            else
                            {
                                sudid = License.udid.ToString();
                            }
                            TyPaxAires8.InvokeMember(cmdCom.CommandSocket, BindingFlags.InvokeMethod, myCustomBinder, paxAires8, new Object[] { myO, e.Data, string.Format("{0}, {1}", License.udid2, sudid) });

                            // 06.06.2021 TyPaxAires8.InvokeMember(cmdCom.CommandSocket, BindingFlags.InvokeMethod, myCustomBinder, paxAires8, new Object[] { myO, e.Data, string.Format("{0}, {1}", License.udid2, ((System.Xml.XmlCharacterData)((System.Xml.XmlNode[])License.udid)[0]).InnerText) });

                        }
                        else
                        {
                            myMethod = TyPaxAires8.GetMethod(cmdCom.CommandSocket, BindingFlags.Public | BindingFlags.Instance, myCustomBinder, new Type[] { typeof(DeviceCommand), typeof(string), typeof(bool) }, null);
                            //13.04.2021 modifica per definire le trasmissioni a pacchetto
                            metodo = myMethod.Name;
                            //if ((this.swDebug & _logger.LOG_INFO) == _logger.LOG_INFO) 
                            _logger.Log(LogLevel.INFO, "<START COMMAND>");
                            if (logDelay != null)
                                logDelay.Log(LogLevel.INFO, string.Format("3) COMMAND {0}; STARTED AT; {1}", metodo, DateTime.Now.Ticks.ToString()));
                            _logger.Log(LogLevel.DEBUG, string.Format("{0} {1} param DeviceCommand, string", WinTTabRes.logMsgCmdRved, metodo));
                            erCnt = 5;
                            // Invoke the overload.
                            TyPaxAires8.InvokeMember(cmdCom.CommandSocket, BindingFlags.InvokeMethod, myCustomBinder, paxAires8, new Object[] { myO, e.Data, cmdCom.SendingDataPackets });
                            // if (myO.CommandCode == myO.CmdSendFormConfiguration) _logger.Log(LogLevel.DEBUG,string.Format( "Sono uscito dal comando {0}", myO.CommandName));
                        }
                    }
                    if (logDelay != null)
                        logDelay.Log(LogLevel.INFO, string.Format("0) COMMAND {0}; EXECUTED AT; {1}", cmdCom.CommandSocket, DateTime.Now.Ticks.ToString()));
                    //if ((this.swDebug & _logger.LOG_DEBUG) == _logger.LOG_DEBUG)
                    _logger.Log(LogLevel.INFO, string.Format(WinTTabRes.logMsgCmdExecWait, metodo));
                    //17.08.2020
                    erCnt = 6;
                    Type tymyO = myO.GetType();
                    // Molto probabilmente non serve controllare il comando dell'evento perché se le cose sono fatte bene in cmdCom dovrebbe essere correttamente assegnato dalla gestione comandi
                    //2.11.2020
                    // var getComCmd = this.USBSconnected ? tymyO.GetProperties().Where(CMD => CMD.Name.ToUpper().Equals(cmdCom.CommandSocket.ToUpper())) : tymyO.GetProperties().Where(CMD => CMD.Name.ToUpper().Equals(e.Command.ToUpper()));// I'm looking for the command in the table
                    var getComCmd = tymyO.GetProperties().Where(CMD => CMD.Name.ToUpper().Equals(cmdCom.CommandSocket.ToUpper()));
                    erCnt = 7;
                    if (getComCmd.Any())
                    {
                        erCnt = 8;
                        // Do I need to determine which command was executed? device type or service type from here
                        var trgt = getComCmd.First().CustomAttributes.First().NamedArguments.Where(A => A.MemberName == "CmdTarget");
                        erCnt = 9;
                        if (trgt.Any())
                        {
                            if ((Convert.ToInt32(trgt.First().TypedValue.Value) == Convert.ToInt32(CmdTargetID.Service)))
                            {
                                erCnt = 10;
                                string methods = getComCmd.First().CustomAttributes.First().NamedArguments[2].TypedValue.Value.ToString();
                                //it is only for control, it does not perform any function
                                MethodInfo myMethodOut;
                                myMethodOut = TyPaxAires8.GetMethod(methods, BindingFlags.Public | BindingFlags.Instance, myCustomBinder, new Type[] { typeof(int), typeof(string), typeof(AsyncSocketListener) }, null);
                                if (myMethodOut == null)
                                    myMethodOut = TyPaxAires8.GetMethod(methods, BindingFlags.Public | BindingFlags.Instance, myCustomBinder, new Type[] { typeof(string), typeof(string), typeof(AsyncSocketListener) }, null);
                                if (myMethodOut == null)
                                    myMethodOut = TyPaxAires8.GetMethod(methods, BindingFlags.Public | BindingFlags.Instance, myCustomBinder, new Type[] { typeof(DeviceCommand), typeof(string), typeof(AsyncSocketListener) }, null);
                                // to here
                                object ob = myO.Response;
                                erCnt = 11;
                                switch (ob.GetType().Name.ToUpper())
                                {
                                    case "INT32":
                                        {
                                            erCnt = 12;
                                            int I = (int)myO.Response;
                                            TyPaxAires8.InvokeMember(methods, BindingFlags.InvokeMethod, myCustomBinder, paxAires8, new Object[] { I, cmdCom.CommandSocket, asl });
                                            erCnt = 13;
                                            break;
                                        }
                                    case "STRING":
                                        {
                                            erCnt = 14;
                                            if (ob == null)
                                            {
                                                //if ((this.swDebug & _logger.LOG_DEBUG) == _logger.LOG_DEBUG)
                                                _logger.Log(LogLevel.DEBUG, "ob is null");
                                            }

                                            string S = ob.ToString();
                                            TyPaxAires8.InvokeMember(methods, BindingFlags.InvokeMethod, myCustomBinder, paxAires8, new Object[] { S, cmdCom.CommandSocket, asl });
                                            erCnt = 15;
                                            break;
                                        }
                                    case "BOOLEAN":
                                        {
                                            erCnt = 16;
                                            bool B = (bool)ob;
                                            TyPaxAires8.InvokeMember(methods, BindingFlags.InvokeMethod, myCustomBinder, paxAires8, new Object[] { B, cmdCom.CommandSocket, asl });
                                            erCnt = 17;
                                            break;
                                        }
                                    case "DEVICECOMMAND":
                                        {
                                            erCnt = 18;
                                            TyPaxAires8.InvokeMember(methods, BindingFlags.InvokeMethod, myCustomBinder, paxAires8, new Object[] { myO, cmdCom.CommandSocket, asl });
                                            erCnt = 19;
                                            break;
                                        }
                                    default:
                                        {
                                            _logger.Log(LogLevel.WARNING, WinTTabRes.logErrNoAnswerProvide);
                                            break;
                                        }
                                }
                                if (logDelay != null)
                                    logDelay.Log(LogLevel.INFO, string.Format("(SERVICES) COMMAND {0}; CLOSED AT; {1}", methods, DateTime.Now.Ticks.ToString()));
                                _logger.Log(LogLevel.INFO, string.Format("Command:{0} <COMMAND ENDED>", myMethod));
                                Type tycmdCom = cmdCom.GetType();
                                var cc = tycmdCom.GetProperties().Where(CC => CC.Name == cmdCom.CommandSocket);
                                if (cc.Any())
                                {
                                    var isr = cc.First().CustomAttributes.First().NamedArguments.Where(ISR => ISR.MemberName == "IsSignatureRequest");
                                    if (isr.Any())
                                    {
                                        if ((byte)isr.First().TypedValue.Value == 0) MainCycleService();
                                    }
                                }
                            }
                            else
                            {
#if DEBUG
                            _logger.Log(LogLevel.DEBUG, string.Format("Sender={0} Command={1}", sender.GetType(), e.Command));
                            Console.WriteLine("* * * *  " + trgt.First().TypedValue.Value + "  " + trgt.First().TypedValue.Value.GetType() + "  * * * *");
                            Console.WriteLine("CommandName: {0} {1}", myO.CommandName, myO.CommandCode);
#endif
                                var f = tymyO.GetRuntimeProperties().Where(F => F.Name == myO.CommandName);
                                if (f.Any())
                                {
                                    // fInfo.CustomAttributes.First().NamedArguments.Where(A => A.MemberName == "TypeOfResponse").First()
                                    PropertyInfo fInfo = f.First();
                                    var call = fInfo.CustomAttributes.First().NamedArguments.Where(CALL => CALL.MemberName == "TypeOfResponse");
                                    if (call.Any())
                                    {
                                        var NameMethos = call.First().TypedValue.Value;
                                        myMethod = TyPaxAires8.GetMethod(NameMethos.ToString(), BindingFlags.Public | BindingFlags.Instance, myCustomBinder, new Type[] { typeof(DeviceCommand), typeof(string), typeof(AsyncSocketListener) }, null);
                                        TyPaxAires8.InvokeMember(NameMethos.ToString(), BindingFlags.InvokeMethod, myCustomBinder, paxAires8, new Object[] { myO, myO.Response.ToString(), asl });
                                        if (logDelay != null)
                                            logDelay.Log(LogLevel.INFO, string.Format("(TABLET) COMMAND {0}; CLOSED AT; {1}", NameMethos.ToString(), DateTime.Now.Ticks.ToString()));
                                        _logger.Log(LogLevel.INFO, string.Format("Command:{0} <COMMAND ENDED>", myMethod));
                                    }
                                    // string NameMethos = fInfo.CustomAttributes.First().NamedArguments[2].TypedValue.Value.ToString();
                                }
                                if (myO.IsSignCommand == 0 || myO.IsSignCommand == 3)
                                    MainCycleService(); // after timeout event, i need to restart de duty cycle
                                else
                                {
#if DEBUG
                                Console.WriteLine("MainCycleService bloccato");
#endif
                                    // SchedularWD.Change(Timeout.Infinite, Timeout.Infinite);
                                    swWatchDog(false);
                                }
                                erCnt = 20;
                            }
                        }
                        myO.Dispose();
                        if (logDelay != null)
                        {
                            logDelay.Log(LogLevel.INFO, string.Format("0) COMMAND {0}; DISPOSED AT; {1}", e.Command, DateTime.Now.Ticks.ToString()));
                            logDelay.Log(LogLevel.DEBUG, "* * * * * * * * * * * * * * * * * * * * * * * * * * * *");
                        }
                    }
                    syncCount = 0;
                    CommandRunning = 2; // alla conclusione di un processo innescato da host spengo la flag di command is running, così torno a gfare il comando 99
                    //else
                    //{
                    //    // 09.06.2021 interlock esecuzione comandi
                    //    asl.Send(Handler, string.Format(WinTTabRes.msgCmdAlreadyInRun, cmdCom1.CommandSocket, cmdCom.CommandSocket));
                    //    _logger.Log(LogLevel.WARNING, string.Format(WinTTabRes.msgCmdAlreadyInRun, cmdCom1.CommandSocket, cmdCom.CommandSocket));
                    //}
                    */
                }
                else
                {
                    erCnt = 21;
                    string message = SemSerRes.logErrTlgrmInvalid;
                    throw new Exception(message);
                }
            }
            catch (Exception ex)
            {
                HandlerError |= 0x2000; //Error occurred in the handling of messages received by the socket serverù
                string errMsg = ClsMessaggiErrore.CustomMsg(ex);
                if (ex.InnerException != null)
                {
                    errMsg += " " + ClsMessaggiErrore.CustomMsg(ex.InnerException);
                }
                SocketMessageStructure response = new SocketMessageStructure
                {
                    Command = "Error",
                    SendingTime = DateTime.Now,
                    BufferDati = new XElement("BufferDati",
                                    new XElement("ErrorDetails", new XElement("Message", errMsg))),  
                    Token = asl.TokenSocket.ToString(),
                    CRC = 0
                };
                string telegramGenerate = SocketMessageSerializer.SerializeUTF8(response);
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                //string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
                string txtSendData = SocketMessageSerializer.Base64Start + Convert.ToBase64String(bytes, 0, bytes.Length) + SocketMessageSerializer.Base64End;
                _logger.Log(LogLevel.ERROR, errMsg);
                asl.Send(Handler, string.Format(SemSerRes.sktErrRecive, txtSendData, cmdCom.Eof));
            }
            finally
            {
                _logger.Log(LogLevel.ENANCED_DEBUG, "finally sysCmdRunnig set to false");
                sysCmdRunnig = false; // 09.06.2021 interlock esecuzione comandi
                swWatchDog(true); // condizione di break e.Command=="CmdSendFormConfiguration"
                this.myStart();
                // swWatchDog(true, IntervalMilliSeconds);
            }
        }

        private void Asl_ErrorFromSocket(object sender, string e)
        {
            CommandRunning = 0;
            Socket Handler = sender as Socket;
            // asl.Send(Handler, e); 2025.05.18 Secondo me non serve.
            _logger.Log(LogLevel.ERROR, e);

        }
        #endregion
        private bool WatchDogStatus { get; set; }
    }
}
