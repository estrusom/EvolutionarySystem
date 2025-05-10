using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketManager
{
    /// <summary>
    /// Device data plate data structure
    /// </summary>
    public sealed class InfoDevice
    {
        public string UDID { get; set; }
        public string Producer { get; set; }
        public string ProductName { get; set; }
        public string Software_license { get; set; }
        public string VERSION_RELEASE { get; set; }
        public string VERSION_INCREMENTAL { get; set; }
        public string VERSION_SDK_NUMBER { get; set; }
        public string BOARD { get; set; }
        public string BOOTLOADER { get; set; }
        public string BRAND { get; set; }
        public string CPU_ABI { get; set; }
        public string CPU_ABI2 { get; set; }
        public string DISPLAY { get; set; }
        public string FINGERPRINT { get; set; }
        public string HARDWARE { get; set; }
        public string HOST { get; set; }
        public string ID { get; set; }
        public string MANUFACTURER { get; set; }
        public string MODEL { get; set; }
        public string PRODUCT { get; set; }
        public string SERIAL { get; set; }
        public string TAGS { get; set; }
        public string TIME { get; set; }
        public string TYPE { get; set; }
        public string UNKNOWN { get; set; }
        public string USER { get; set; }
        public string DENSITY { get; set; }
        public string DENSITY_DPI { get; set; }
        public string PIXEL_HEIGHT { get; set; }
        public string PIXEL_WIDTH { get; set; }
        public string SCALE_DENSITY { get; set; }
        public string DPI_X { get; set; }
        public string DPI_Y { get; set; }
        public string WIDTH_IN_DPI { get; set; }
        public string HEIGHT_IN_DPI { get; set; }
    }
    /// <summary>
    /// Message structure received from socket
    /// </summary>
    public partial class SocketMessageStructure
    {
        /// <summary>
        /// 
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime SendingTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Data { get; set; }

    }
    /// <summary>
    /// Properties definition for socket comunication command (attribute extension for DeviceCommand class)
    /// </summary>
    public class SktProperty : System.Attribute
    {
        /// <summary>
        /// Defines whether it is a signature request
        /// </summary>
        public byte IsSignatureRequest { get; set; }
        /// <summary>
        /// Defines the name of the command
        /// </summary>
        public string Description { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SocketCommand : IDisposable
    {
        private const string stx = "";// "<stx>";
        private const string etx = "";// "<etx>";
        private const string eof = "<eof>";
        private string cmd00 = stx + "cmdSync" + etx;
        private string cmd05 = stx + "CmdInfoTablet" + etx;
        private string cmd06 = stx + "CmdRxDataSign" + etx;
        private string cmd07 = stx + "CmdCancSignSurface" + etx;
        private string cmd08 = stx + "CmdSigningProcStarted" + etx;
        private string cmd09 = stx + "CmdSigningProcTermination" + etx;
        private string cmd0A = stx + "CmdHideSignatureForm " + etx;
        private string cmd1A = stx + "CmdActivationSignatureForm" + etx;
        private string cmd0B = stx + "CmdSendFormConfiguration[{0}]" + etx;
        private string sktSuspendedService = stx + "PAUSE" + etx;
        private string sktServiceAwakening = stx + "AWAKE" + etx;
        private string sktStopService = stx + "STOP" + etx;
        private string sktOpenComPort = stx + "OPEN" + etx;
        private string sktCloseComPort = stx + "CLOSE" + etx;
        private string sktSocketSrvPort = stx + "PORTINIT" + etx;
        /// <summary>
        /// Vediamo a cosa serve
        /// </summary>
        public string CommandSocket { get; set; }
        /// <summary>
        /// Sync command (watch dog) CMD = 00
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Check comunication chanel")]
        public string cmdSync { get { return this.cmd00; } }
        /// <summary>
        /// Device nameplate data CMD = 05
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Device nameplate data request")]
        public string CmdInfoTablet { get { return this.cmd05; } }
        /// <summary>
        /// Single signature frame reception CMD = 06
        /// </summary>
        [SktProperty(IsSignatureRequest = 2, Description = "Single signature frame reception")]
        public string CmdRxDataSign { get { return this.cmd06; } }
        /// <summary>
        /// Command to clearing signature surface area CMD = 07
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Command to clearing signature surface area")]
        public string CmdCancSignSurface { get { return this.cmd07; } }
        /// <summary>
        /// Start acquiring signature CMD = 08
        /// </summary>
        [SktProperty(IsSignatureRequest = 1, Description = "Start acquiring signature")]
        public string CmdSigningProcStarted { get { return this.cmd08; } }
        /// <summary>
        /// Signing process ended CMD = 09
        /// </summary>
        [SktProperty(IsSignatureRequest = 3, Description = "Signing process ended")]
        public string CmdSigningProcTermination { get { return this.cmd09; } }
        /// <summary>
        /// Hide signature form CMD = 0A
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Hide signature form")]
        public string CmdHideSignatureForm { get { return this.cmd0A; } }
        /// <summary>
        /// Show signature form CMD = 1A
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Show signature form")]
        public string CmdActivationSignatureForm { get { return this.cmd1A; } }
        /// <summary>
        /// Send the background configuration data to the device CMD = 0B
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Show signature form")]
        public string CmdSendFormConfiguration { get { return this.cmd0B; } }
        /// <summary>
        /// The service will be stopped
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "???")]
        public string StopService { get { return this.sktStopService; } }
        /// <summary>
        /// The service will be Awakening 
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "???")]
        public string ServiceAwakening { get { return this.sktServiceAwakening; } }
        /// <summary>
        /// The service will be suspended
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "???")]
        public string ServiceSuspended { get { return this.sktSuspendedService; } }
        /// <summary>
        /// Socket command for opening the communication port
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Comunication port opening")]
        public string OpenComPort { get { return this.sktOpenComPort; } }
        /// <summary>
        /// Socket command for closing the communication port
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Comunication port closing")]
        public string CloseComPort { get { return this.sktCloseComPort; } }
        /// <summary>
        /// Transmits the port number on which the socket server responds
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Transmits socket server the port number")]
        public string PortInit { get { return this.sktSocketSrvPort; } }
        /// <summary>
        /// Telegram start sequence
        /// </summary>
        public string StartTelegram { get { return stx; } }
        /// <summary>
        /// End of telegram sequence
        /// </summary>
        public string EndTelegramm { get { return etx; } }
        /// <summary>
        /// Final sequence to add to messages to be sent to the client
        /// </summary>
        public string Eof { get { return eof; } }
        #region IDisposable Support
        private bool disposedValue = false; // Per rilevare chiamate ridondanti
        /// <summary>
        /// disposing Class procedure
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminare lo stato gestito (oggetti gestiti).
                }

                // TODO: liberare risorse non gestite (oggetti non gestiti) ed eseguire sotto l'override di un finalizzatore.
                // TODO: impostare campi di grandi dimensioni su Null.

                disposedValue = true;
            }
        }

        // TODO: eseguire l'override di un finalizzatore solo se Dispose(bool disposing) include il codice per liberare risorse non gestite.
        // ~SocketCommand() {
        //   // Non modificare questo codice. Inserire il codice di pulizia in Dispose(bool disposing) sopra.
        //   Dispose(false);
        // }

        // Questo codice viene aggiunto per implementare in modo corretto il criterio Disposable.
        void IDisposable.Dispose()
        {
            // Non modificare questo codice. Inserire il codice di pulizia in Dispose(bool disposing) sopra.
            Dispose(true);
            // TODO: rimuovere il commento dalla riga seguente se è stato eseguito l'override del finalizzatore.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
    /// <summary>
    /// Vediamo se serve, non è detto 20.06.10
    /// </summary>
    public class SocketThread
    {
        SocketServer sktSrv;
        /// <summary>
        /// Costruttore del thread di gstione del server socket
        /// </summary>
        /// <param name="SktSrv"></param>
        public SocketThread(SocketServer SktSrv)
        {
            this.sktSrv = SktSrv;
        }
        /// <summary>
        /// This method handling the thread that manage the socket server
        /// </summary>
        /// <param name="obj"></param>
        public void Socket(object obj)
        {
            int interval = (int)obj;
            try
            {
                sktSrv.SocketServerDutyCicle();
            }
            catch (InvalidCastException)
            {
                interval = 500;
            }
            DateTime start = DateTime.Now;
            var sw = Stopwatch.StartNew();
#if DEBUG
            Console.WriteLine("Thread {0}: {1}, Priority {2}",
                              Thread.CurrentThread.ManagedThreadId,
                              Thread.CurrentThread.ThreadState,
                              Thread.CurrentThread.Priority);
#endif
            do
            {
#if DEBUG
                // Console.WriteLine("Thread {0}: Elapsed {1:N2} seconds", Thread.CurrentThread.ManagedThreadId, sw.ElapsedMilliseconds / 1000.0);
#endif
                Thread.Sleep(interval);
            } while (!StopThread); // (sw.ElapsedMilliseconds <= interval);
            sw.Stop();
        }
        /// <summary>
        /// If this property is true, the socket thread will be stoped
        /// </summary>
        public bool StopThread { get; set; }
    }
}
