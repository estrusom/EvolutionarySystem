// MODIFICHE 
// TE 22.07.20

// TE 04.03.2021 elenco IP address
// 12.04.2021 l 'indirizzo IP è stato fissato così, non è più acquisito dal sistema
// 20.04.2021 isConnect determines the state of the socket, if true socket connected false disconnected. (SocketManager)
// 03.12.2021 siccome il servizio può essere chiamato da un host l'ipadress eda questo momento è preso dalla proprietà LocalEndPoint della classe DeviceCommand
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;
using MasterLog;
using SocketManager.Properties;
using System.Diagnostics;
using System.Threading;
using SocketManagerInfo;
using System.Reflection;
using MessaggiErrore;
using System.Net.NetworkInformation;

namespace SocketManager
{
    /// <summary>
    /// Synchronous client for communication management with socket server
    /// </summary>
    public class SocketClient
    {
        private const string Prefisso = "<SOCKET_CLIENT>"; //TE 22.07.20
        private int indexIP = 0;
        private string socketaddress = "";
        private UInt16 socketPort = 0;
        private Socket client = null;
        private Logger log; //TE 22.07.20
        private int swDebug = 0;

        /// <summary>
        /// Costruttore aggiunto 
        /// TE 22.07.20
        /// </summary>
        /// <param name="Log"></param>
        public SocketClient(Logger Log)
        {
            this.log = Log;
        }
        /// <summary>
        /// Costruttore che prevede l'impostazione dei livelli di log
        /// </summary>
        /// <param name="Log"></param>
        /// <param name="SwDebug"></param>
        public SocketClient(Logger Log, int SwDebug)
        {
            this.log = Log;
            this.swDebug = SwDebug;
        }

        /// <summary>
        /// Connection to the socket server
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="PortNumber"></param>
        public void ConnectToServer(string Address, int PortNumber)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            byte[] data = new byte[256];
            IPHostEntry ipHostInfo = null;
            IPAddress ipAdress = null;
            IPEndPoint ipEndpoint = null;
            try
            {
                if (log != null)
                    log.Log(LogLevel.INFO, string.Format("{0} address: {1} port: {2}", Prefisso, Address, PortNumber));
                if (Address.Length > 0)
                {
                    if (log != null)
                    {
                        log.Log(LogLevel.DEBUG, string.Format("{0}, Host address: {1}", Prefisso, Address));
                    }
                    IPAddress[] ipAdr;
                    IPAddress[] ipv4Addresses = null; //04.03.2021 
                    if (Address.ToUpper() != "LOCALHOST")
                    {
                        IPAddress.TryParse(Address, out System.Net.IPAddress lAddress);
                        if (lAddress == null) throw new Exception(ResScktManager.errMsgIvalidAddress);
                        ipAdr = null;
                        ipAdr = Dns.GetHostAddresses(lAddress.ToString());
                        // 03.12.2021 ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                        //04.03.2021 elenco IP address
                        int c = 0;
                        foreach (IPAddress s in ipAdr)
                        {
                            log.Log(LogLevel.DEBUG, string.Format("{0} (1) Index: {1} IP: {2}", Prefisso, c, s));
                            c++;
                        }
                        c = 0;
                        //03.12.2021 controllo se sto rispondendo al pc locale o ad un host
                        if (ipv4Addresses != null)
                        {
                            foreach (IPAddress s in ipv4Addresses)
                            {
                                log.Log(LogLevel.DEBUG, string.Format("{0} (1) Index: {1} IPv4: {2}", Prefisso, c, s));
                                c++;
                            }
                            //-----------------------------
                            // 10.03.2021 Questo potrebbe causare errori perché vado a pescare da tutti gli indirizzi IP, mentreinvece devo prendere solo gli IPV4
                            //if (ipAdr.Count() > 0)
                            //    ipAdress = ipAdr[0];
                            //else
                            //    throw new Exception(ResScktManager.errMsgIvalidAddress);
                            if (ipv4Addresses.Count() > 0)
                                ipAdress = ipv4Addresses[0];
                            else
                                throw new Exception(ResScktManager.errMsgIvalidAddress);

                        }
                        else
                        {
                            //03.12.2021 se sto rispondendo a un host devo caricare l'indirizzo dell'host
                            ipAdress = ipAdr[0];
                            log.Log(LogLevel.INFO, string.Format("{0} Reply to the host through the address {1} and port number{2}", Prefisso, ipAdress.ToString(), PortNumber.ToString()));
                        }
                    }
                    else
                    {
                        ipAdr = null;
                        ipAdr = Dns.GetHostAddresses(Address);
                        //23.03.2021
                        var iPAddress = ipAdr.Where(AD => AD.AddressFamily == AddressFamily.InterNetwork);
                        if (iPAddress.Any())
                        {
                            ipAdress = iPAddress.First();
                        }
                        else
                        {
                            throw new Exception(ResScktManager.errMsgIvalidAddress);
                        }
                        /*23.03.2021
                        Type adrty = iPAddress.GetType();
                        //04.03.2021 
                        // ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                        //04.03.2021 elenco IP address
                        */
                        int c = 0;
                        foreach (IPAddress s in ipAdr)
                        {
                            log.Log(LogLevel.DEBUG, string.Format("{0} (2) Index: {1} IP: {2}", Prefisso, c, s));
                            c++;
                        }
                        /* 23.03.2021
                        c = 0;
                        foreach (IPAddress s in ipv4Addresses)
                        {
                            log.Log(LogLevel.DEBUG, string.Format("{0} (2) Index: {1} IPv4: {2}", Prefisso, c, s));
                            c++;
                        }
                        */
                        //-----------------------------
                        // 10.03.2021 Questo potrebbe causare errori perché vado a pescare da tutti gli indirizzi IP, mentreinvece devo prendere solo gli IPV4
                        //if (ipAdr.Count() > 0)
                        //    ipAdress = ipAdr[1];
                        //else
                        //    throw new Exception(ResScktManager.errMsgIvalidAddress);

                        /* 23.03.2021
                        if (ipv4Addresses.Count() > 0)
                            ipAdress = ipv4Addresses[0];
                        else
                            throw new Exception(ResScktManager.errMsgIvalidAddress);
                        */

                    }
                }
                else
                {
                    ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                    if (ipHostInfo.AddressList.Count() == 0)
                    {
                        throw new Exception(ResScktManager.errMsgSktSrvNotFound);
                    }
                    //04.03.2021 elenco IP address
                    int c = 0;
                    foreach (IPAddress s in ipHostInfo.AddressList)
                    {
                        log.Log(LogLevel.DEBUG, string.Format("{0} (3) Index: {1} IP: {2}", Prefisso, c, s));
                        c++;
                    }
                    IPAddress[] ipv4Addresses = null;
                    ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                    c = 0;
                    foreach (IPAddress s in ipv4Addresses)
                    {
                        log.Log(LogLevel.DEBUG, string.Format("{0} (3) Index: {1} IPv4: {2}", Prefisso, c, s));
                        c++;
                    }
                    //-----------------------------
                    if (this.indexIP == -1)
                        ipAdress = ipHostInfo.AddressList[0]; // ipHostInfo.AddressList.Count() - 1
                    else
                        ipAdress = ipHostInfo.AddressList[indexIP]; // ripristinare 0 25.06.2020

                }
                if (log != null)
                    log.Log(LogLevel.INFO, Prefisso + "Ckeck port available");

                // ipAdress = IPAddress.Parse("127.0.0.1"); //12.04.2021 l'indirizzo IP è stato fissato così, non è più acquisito dal sistema
                // 03.12.2021 siccome il servizio può essere chiamato da un host l'ipadress eda questo momento è preso dalla proprietà LocalEndPoint della classe DeviceCommand
                // 17.08.2020
                ipEndpoint = new IPEndPoint(ipAdress, PortNumber);
                if (log != null)
                    log.Log(LogLevel.DEBUG, string.Format("{0} ipEndpoint: {1} {2} {3}", Prefisso, ipEndpoint.AddressFamily, ipEndpoint.Address, ipEndpoint.Port));

                this.socketaddress = ipAdress.ToString();
                this.socketPort = (UInt16)PortNumber;

                client = new Socket(ipAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (log != null)
                    log.Log(LogLevel.DEBUG, Prefisso + "Client socket connection started");

                client.Connect(ipEndpoint);
                if (log != null)
                    log.Log(LogLevel.INFO, Prefisso + "Client socket connection created");

            }
            catch (Exception ex)
            {
                throw new Exception(ClsMessaggiErrore.CustomMsg(ex, thisMethod));
            }
        }
        /// <summary>
        /// Sending a message to the socket server in string format
        /// </summary>
        /// <param name="Message"></param>
        public void SendString(string Message)
        {
            try
            {
                byte[] sendmsg = Encoding.ASCII.GetBytes(Message);
                int n = client.Send(sendmsg);
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Sending a message to the socket server as a byte
        /// </summary>
        /// <param name="Message"></param>
        public void SendByte(byte[] Message)
        {
            try
            {
                //byte[] sendmsg = Encoding.ASCII.GetBytes(Message);
                int n = client.Send(Message);
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Receiving a message from socket server
        /// </summary>
        /// <returns></returns>
        public string ReceiveMessage()
        {
            try
            {
                byte[] data = new byte[65535];
                int m = client.Receive(data);
                return Encoding.ASCII.GetString(data);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// Closing socket communication
        /// </summary>
        public void CloseSocket()
        {
            try
            {
                if (this.client != null)
                {
                    this.client.Shutdown(SocketShutdown.Both);
                    this.client.Close();
                }
            }
            catch(SocketException sex)
            {
                throw new Exception(sex.Message);
            }
            catch(Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }
        #region "* * *  PRIVATE METHOD  * * * "
        // TE 22.07.20
        private bool isPortAvailable(int port)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            bool ret = false;
            try
            {
                var V = tcpConnInfoArray.Where(TCP => TCP.LocalEndPoint.Port == port);
                ret = !V.Any();
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0} {1} {2}", Prefisso, ClsMessaggiErrore.CustomMsg(ex, thisMethod), ((SocketException)ex).ErrorCode);
                //if (_log != null) _log.Log(LogLevel.ERROR, msg); else throw new Exception(msg, ex);
                throw new Exception(msg);
            }
            return ret;
        }
        // TE 22.07.20
        private string Ping(string Address)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            string data = "";
            string sRet = "";
            options.DontFragment = true;
            byte[] buffer = Encoding.ASCII.GetBytes(data.PadLeft(32, 'a'));
            int timeout = 120;
            PingReply reply = pingSender.Send(Address, timeout, buffer);
            if (reply.Status == IPStatus.Success)
                sRet = string.Format("Address: {0} Time (milli sec.): {1} Buffer size: {2}", reply.Address.ToString(), reply.RoundtripTime, reply.Buffer.Length);
            else
                sRet = string.Format("Address {0} not found", Address);
            return sRet;
        }
        // TE 22.07.20
        private string Ping(IPAddress Address)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            string data = "";
            string sRet = "";
            options.DontFragment = true;
            byte[] buffer = Encoding.ASCII.GetBytes(data.PadLeft(32, 'a'));
            int timeout = 120;
            PingReply reply = pingSender.Send(Address, timeout, buffer);
            if (reply.Status == IPStatus.Success)
                sRet = string.Format("Address: {0} Time (milli sec.): {1} Buffer size: {2}", reply.Address.ToString(), reply.RoundtripTime, reply.Buffer.Length);
            else
                sRet = string.Format("Address {0} not found", Address);
            return sRet;
        }
        #endregion /*          * * * That's all folks  * * *           */
        /// <summary>
        /// Selector of the ip address to use
        /// </summary>
        public int IndexIP
        {
            get { return this.indexIP; }
            set { this.indexIP = value; }
        }
        /// <summary>
        /// Contains the address of the socket server where the application is connected
        /// </summary>
        public string SocketAddress { get { return this.socketaddress; } }
        /// <summary>
        /// Contains the socket server port where the application is connected
        /// </summary>
        public UInt16 SocketPort { get { return this.socketPort; } }
        /// <summary>
        /// Determines the state of the socket, if true socket connected false disconnected. 20.04.2021
        /// </summary>
        public bool isConnect { get { return this.client.Connected; } }
    }
    /// <summary>
    /// Synchronous server for communication via socket with a client
    /// </summary>
    public class SocketServer
    {
        private byte[] buffer;
        private IPAddress ipAddress;
        private IPEndPoint localEndpoint;
        private Socket sock;
        private string sendMessage = "";
        private Socket confd = null;
        private Logger _logger;
        /// <summary>
        /// Event generated by receiving data via socket
        /// </summary>
        public event EventHandler<SocketMessageStructure> DataFromSocket;
        /// <summary>
        /// When an unknown message arrives from the client it must be answered with an error
        /// </summary>
        public event EventHandler<string> ErrorFromSocket;

        /// <summary>
        /// Constructor of the SocketServer class
        /// </summary>
        public SocketServer(int BufferSize, int PortNum, Logger logger)
        {
            try
            {
                this._logger = logger;
                this._logger.Log(LogLevel.INFO, ResScktManager.msgInfoStartServer);
                buffer = new byte[BufferSize];
                IPHostEntry iphostInfo = Dns.GetHostEntry(Dns.GetHostName());
                ipAddress = iphostInfo.AddressList[0];
                localEndpoint = new IPEndPoint(ipAddress, PortNum);
                this._logger.Log(LogLevel.INFO, string.Format(ResScktManager.msgInfoServerStarted, localEndpoint));
                sock = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //msg = Encoding.ASCII.GetBytes("From server\n");
                sock.Bind(localEndpoint);
                sock.Listen(5);
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Loop for managing messages received from the client
        /// </summary>
        public void SocketServerDutyCicle()
        {
            try
            {
                string sBuffer = "";
                //string data = null;
                confd = sock.Accept();

                while (!this.StopTheServer)
                {
                    int b = confd.Receive(buffer);
                    sBuffer += Encoding.ASCII.GetString(buffer, 0, b);
                    try
                    {
                        SocketMessageStructure sms = deserializedMessage(sBuffer);
                        DataFromSocket?.Invoke(this, sms);
                        //while (sendMessage.Contains("<eof>"))
                        //{
                        //    confd.Send(Encoding.ASCII.GetBytes(sendMessage.Substring(0, sendMessage.IndexOf("<eof>"))));
                        //    sendMessage = sBuffer = "";
                        //}
                        //while (MessageToClient.Contains("<eof>"))
                        //{
                        //    confd.Send(Encoding.ASCII.GetBytes(MessageToClient.Substring(0, MessageToClient.IndexOf("<eof>"))));
                        //    MessageToClient = sBuffer = "";
                        //}
                    }
                    catch (Exception ex)
                    {
                        ErrorFromSocket?.Invoke(this, ex.Message);
                        while (sendMessage.Contains("<eof>"))
                        {
                            confd.Send(Encoding.ASCII.GetBytes(sendMessage.Substring(0, sendMessage.IndexOf("<eof>"))));
                            sendMessage = sBuffer = "";
                        }
                        //while (MessageToClient.Contains("<eof>"))
                        //{
                        //    confd.Send(Encoding.ASCII.GetBytes(MessageToClient.Substring(0, MessageToClient.IndexOf("<eof>"))));
                        //    MessageToClient = sBuffer = "";
                        //}
                    }
                }
                confd.Close();
            }
            catch // (Exception ex)
            {
                
            }
        }
        /// <summary>
        /// Stops the socket server cycle
        /// </summary>
        public bool StopTheServer { get; set; }
        /// <summary>
        /// Message to be sent to the client
        /// </summary>
        // public string MessageToClient { get; set; }
        public void SetMessageToClient(string Message)
        {
            while (Message.Contains("<eof>"))
            {
                confd.Send(Encoding.ASCII.GetBytes(Message.Substring(0, Message.IndexOf("<eof>"))));
                Message = "";
            }

        }
        #region "* * *  PRIVATE METHODS  * * *"
        private SocketMessageStructure deserializedMessage(string content)
        {
            SocketMessageStructure Sms = null;
            try
            {
                XDocument xd = XDocument.Parse(content);
                var n = from N in xd.Elements() where N.Name.LocalName == "SocketMessageStructure" select N;
                if (n.Any())
                {
                    XmlSerializer xmls = new XmlSerializer(typeof(SocketMessageStructure));
                    using (TextReader textReader = new StringReader(content))
                    {
                        Sms = (SocketMessageStructure)xmls.Deserialize(textReader);
                    }
                }

            }
            catch
            {
                throw;
            }
            return Sms;
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
