using MasterLog;
using MessaggiErrore;
using SemanticProcessor.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SemanticProcessor
{
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
        private int logCountSrv = 0; //Timer for the issuance of the service notification is working
        private int swDebug = 0;
        string _path = "";
        private string ServiceVer = "";
#if DEBUG
        private bool firstInDebug = false;
#endif

        public SemanticProcessorService()
        {
            InitializeComponent();
            swDebug = Convert.ToInt32(ConfigurationManager.AppSettings["DebugLev"]); ;
            _path = ConfigurationManager.AppSettings["FolderLOG"];
            _logger = new Logger(_path, "SemanticProcessor");
            _logger.SwLogLevel = swDebug;
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
                this.myStart();
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
        private void myStart()
        {
            SchedularWD = new Timer(new TimerCallback(SchedularWatchdog));      // Create watchdog timer
        }
        private bool WatchDogStatus { get; set; }
    }
}
