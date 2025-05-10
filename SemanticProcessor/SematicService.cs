using AsyncSocketServer;
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
using WinTTab_01;

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
        private Timer SchedularWD;  //Watch dog scheduling time
        protected Logger _logger;
        private int TimerWatchDog = 0;
        private short StepStartingService = 0; //Steps for starting the service correctly
        private bool SemaforoDC = false; // (semaforo) serve per evitare l'over run durante l'avvio del servizio
        private int logCountSrv = 0; //Timer for the issuance of the service notification is working
        private int swDebug = 0;
        string _path = "";
        protected Mutex SyncMtxLogger = new Mutex();
        protected DateTime today = DateTime.MinValue;
        private string ServiceVer = "";
        #region Instance of the asynchronous socket server class
        private AsyncSocketListener asl;
        protected AsyncSocketThread scktThrd;
        protected Thread thSocket;
        private bool sysCmdRunnig = false; // lock dell'esecuzione di un comando se uno è già in corso
        private int CommandRunning = 0; // se true c'è un comando in corso 
        #endregion
        private string VersionDate = "07.05.2025";// Cambiare ad ogni release 
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
                swDebug = Convert.ToInt32(ConfigurationManager.AppSettings["DebugLev"]); ;
                _path = ConfigurationManager.AppSettings["FolderLOG"];
                _logger = new Logger(_path, "SemanticProcessor", SyncMtxLogger);
                _logger.SwLogLevel = swDebug;
                _logger.Log(LogLevel.INFO, string.Format("Log path:{0}", _logger.GetPercorsoCompleto()));
                today = DateTime.Now;
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
                StartThread();
            }catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, ClsMessaggiErrore.CustomMsg(ex, thisMethod));
            }
        }

        protected override void OnStart(string[] args)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                Assembly tyWinTTab = typeof(SemanticProcessor).Assembly;
                AssemblyName WinTTabVER = tyWinTTab.GetName();
                Version ver = WinTTabVER.Version;
                this.ServiceVer = ver.ToString();
                ServiceStatus serviceStatus = new ServiceStatus();
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
                serviceStatus.dwWaitHint = 30000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
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
        /// <summary>
        /// Service duty cycle
        /// </summary>
        public void MainCycleService()
        {

            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            int IntervalMilliSeconds = 0;
            try
            {
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
                _logger.Log(LogLevel.INFO, "Sun'mi sun't chi");
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
                switch (StepStartingService)
                {
                    case 0:
                        {
                            #region Avvio socket
                            if (!SemaforoDC)
                            {
                                SemaforoDC = true;
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
                                SemaforoDC = false;
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
                            if (!SemaforoDC)
                            {
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
                                _logger.Log(LogLevel.DEBUG, thSocket.IsAlive ? "Thread is alive" : "Thread is dead");
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
                this.myStart();
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
        /// 
        /// </summary>
        private void StartThread()
        {
            scktThrd = new AsyncSocketThread();
            scktThrd.Log = _logger;
            scktThrd.AsyncSocketListener = asl;
            scktThrd.Interval = 100;
            thSocket = new Thread(scktThrd.AsyncSocket);
        }
        private SocketCommand checkCommandFromSocket(string cmd, long? TokenSocket = 0)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            SocketCommand scktCmd = new SocketCommand();
            try
            {
#if DEBUG
                Console.WriteLine("********" + cmd + "**********");
#endif
                Type tySktCmd = scktCmd.GetType();
                var getSktCmd = tySktCmd.GetProperties().Where(CMD => CMD.Name.ToUpper().Equals(cmd.ToUpper()));
                if (getSktCmd.Any())
                {
                    PropertyInfo fInfo = getSktCmd.First();
                    var call = fInfo.CustomAttributes.First().NamedArguments.Where(CALL => CALL.MemberName == "TockenManaging");
                    if (call.Any())
                    {
                        switch ((byte)call.First().TypedValue.Value)
                        {
                            case 0: //Token validity check
                                {
                                    _logger.Log(LogLevel.INFO, string.Format("Check validity token for the command {0}. Received: {1} stored: {2}", cmd, TokenSocket, asl.TokenSocket));
                                    if (asl.TokenSocket == TokenSocket)
                                    {
                                        getSktCmd.First().GetValue(scktCmd).ToString();
                                        var val = ((System.Reflection.TypeInfo)tySktCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("CommandSocket")).First();
                                        val.SetValue(scktCmd, getSktCmd.First().Name);
                                    }
                                    else
                                    {
                                        getSktCmd.First().GetValue(scktCmd).ToString();
                                        var val = ((System.Reflection.TypeInfo)tySktCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("CommandSocket")).First();
                                        val.SetValue(scktCmd, "CmdTokenMismatch");
                                    }
                                }
                                break;
                            case 1: //Token assignment
                                {
                                    sysCmdRunnig = false; // when token is assigned the flag command runnig to be false
                                    if (TokenSocket != 0)
                                    {
                                        _logger.Log(LogLevel.INFO, string.Format(SemSerRes.logAssignToken, cmd, asl.TokenSocket, TokenSocket));
                                        asl.TokenSocket = (long)TokenSocket;
                                    }
                                    else
                                    {
                                        _logger.Log(LogLevel.INFO, string.Format("command: {0} token value: {1}. Token does not assigned", cmd, TokenSocket));
                                    }
                                }
                                break;
                            case 2: //commands for which the token should not be checked
                                {
                                    _logger.Log(LogLevel.INFO, string.Format("for the {0} command the token does not checked", cmd));
                                    getSktCmd.First().GetValue(scktCmd).ToString();
                                    var val = ((System.Reflection.TypeInfo)tySktCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("CommandSocket")).First();
                                    val.SetValue(scktCmd, getSktCmd.First().Name);
                                }
                                break;
                            default:
                                {
                                    string msg = string.Format("The command {0} does not support tokens. {1} Time: {2}", cmd, SemSerRes.logErrTockenMismatch, DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"));
                                    _logger.Log(LogLevel.WARNING, msg);
                                    asl.Send(asl.Handler, msg);
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
            return scktCmd;
        }
        #endregion
        #region Socket server events
        private void Asl_DataFromSocket(object sender, SocketManagerInfo.SocketMessageStructure e)
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
                cmdCom1 = checkCommandFromSocket(e.Command, e.Token);
                _logger.Log(LogLevel.DEBUG, "* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *");
                _logger.Log(LogLevel.DEBUG, string.Format("* * * *  {0} * * * * {1} * * * *", e.Command.ToUpper(), e.Token));
                _logger.Log(LogLevel.DEBUG, "* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *");
                if (cmdCom1 != null)
                {
                    while (sysCmdRunnig)
                    {
                        Thread.Sleep(20);
                        if (cnt == 10)
                            sysCmdRunnig = false;
                        else
                            cnt++;
                        _logger.Log(LogLevel.DEBUG, "- WAITING CYCLES: " + cnt.ToString());
                    }
                    _logger.Log(LogLevel.DEBUG, "At start command sysCmdRunnig set to true");
                    sysCmdRunnig = true; // 09.06.2021 interlock esecuzione comandi
                    CommandRunning = 1;
                    erCnt = 2;
                    ClsCustomBinder myCustomBinder = new ClsCustomBinder();
                    

                }
            }
            catch (Exception ex)
            {
                string message = ClsMessaggiErrore.CustomMsg(ex, thisMethod) + " erCnt = " + erCnt.ToString();
                _logger.Log(LogLevel.ERROR, message);
                asl.Send(Handler, string.Format(SemSerRes.sktErrRecive, message, cmdCom.Eof));
            }
            finally
            {
                _logger.Log(LogLevel.DEBUG, "finally sysCmdRunnig set to false");
                sysCmdRunnig = false; // 09.06.2021 interlock esecuzione comandi
                this.myStart();
            }
        }

        private void Asl_ErrorFromSocket(object sender, string e)
        {
            CommandRunning = 0;
            Socket Handler = sender as Socket;
            asl.Send(Handler, e);

        }
        #endregion
        private bool WatchDogStatus { get; set; }
    }
}
