// MODIFICHE 
// 22.07.20
// 04.03.2021 elenco IP address
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
using System.Threading.Tasks;
using System.Xml;

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
            catch (SocketException sex)
            {
                throw new Exception(sex.Message);
            }
            catch (Exception ex)
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
                confd.Close(); // 2025.05.21
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
    /// <summary>
    /// SocketMessageSerialize
    /// 2025.05.19
    /// Definisci una classe per trasportare i dati dell'evento di messaggio ricevuto
    /// *** Modificato: Ora trasporta la stringa raw ricevuta e il socket ***
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs // Rinominato per essere più generico
    {
        /// <summary>
        /// La stringa raw ricevuta (es. Base64 con delimitatori)
        /// </summary>
        public string ReceivedString { get; } // 
        /// <summary>
        /// Il socket del client che ha inviato il messaggio
        /// </summary>
        public Socket ClientSocket { get; } // 
        /// <summary>
        /// Costruttore della classe MessageReceivedEventArgs 
        /// </summary>
        /// <param name="receivedString">Stringa di risposta dal server socket interrogato</param>
        /// <param name="clientSocket">socket che ha generato la richiesata al server</param>
        public MessageReceivedEventArgs(string receivedString, Socket clientSocket)
        {
            ReceivedString = receivedString;
            ClientSocket = clientSocket;
        }
    }
    /// <summary>
    /// Classe per trasportare i dati dell'evento di stato connessione
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Stato della connessione col server (truec -> connesso)
        /// </summary>
        public bool IsConnected { get; }
        /// <summary>
        /// Contiene la descrizione dello stato corrente della connessione
        /// </summary>
        public string StatusMessage { get; }
        /// <summary>
        /// Costruttore sella classe
        /// </summary>
        /// <param name="isConnected">stato regisatrato della connessione (true/fals) </param>
        /// <param name="statusMessage">descrizione dello stato attuale della connession</param>
        public ConnectionStatusChangedEventArgs(bool isConnected, string statusMessage)
        {
            IsConnected = isConnected;
            StatusMessage = statusMessage;
        }
    }
    /// <summary>
    /// Fornisce funzionalità di client socket per la comunicazione asincrona.
    /// Utilizzato dal SemanticProcessorService per inviare notifiche alle UI.
    /// Gestisce una connessione persistente e solleva eventi per l'esito dell'invio.
    /// </summary>
    public class SemanticClientSocket : IDisposable
    {
        Logger _logger = null;
        //private Socket _socket;
        private IPEndPoint _remoteEndPoint;
        private bool _isConnected;
        private readonly string _ipAddress;
        private readonly int _port;
        private readonly StringBuilder _receiveBuffer = new StringBuilder();
        private Socket _clientSocket;

        // TaskCompletionSource per gestire l'attesa del completamento dell'operazione SendAsync
        // Questo permette di convertire l'evento Completed di SocketAsyncEventArgs in un Task<bool>
        private TaskCompletionSource<bool> _sendCompletionSource;

        // Eventi per notificare l'esito dell'invio di un messaggio
        /// <summary>
        /// evento di avvenuta ricezione dei dati dal client
        /// </summary>
        public event EventHandler<MessageSentEventArgs> MessageSentSuccess;
        /// <summary>
        /// evento di errorre durante kla comunicazione al client
        /// </summary>
        public event EventHandler<MessageSentEventArgs> MessageSentFailed;
        /// <summary>
        /// Indica se il client socket è attualmente connesso.
        /// </summary>
        public bool IsConnected => _isConnected && (_clientSocket?.Connected ?? false);

        /// <summary>
        /// Ottiene l'endpoint remoto a cui il socket è connesso.
        /// </summary>
        public string RemoteEndpoint => _remoteEndPoint?.ToString();

        /// <summary>
        /// Costruttore per SemanticClientSocket.
        /// </summary>
        /// <param name="ipAddress">L'indirizzo IP del server (UI) a cui connettersi.</param>
        /// <param name="port">La porta del server (UI) a cui connettersi.</param>
        public SemanticClientSocket(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            //_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public SemanticClientSocket(string ipAddress, int port, Logger Log)
        {
            _ipAddress = ipAddress;
            _port = port;
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            //_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._logger = Log;
        }
        /// <summary>
        /// Tenta di connettersi al server (UI) in modo asincrono.
        /// </summary>
        /// <returns>True se la connessione ha successo, altrimenti false.</returns>
        public async Task<bool> ConnectAsync()
        {
            //if (IsConnected)
            //{
            //    // Già connesso o in fase di connessione
            //    return true;
            //}
            _clientSocket.Close();
            try
            {
                // Se il socket è stato chiuso o smaltito, creane uno nuovo
                if (_clientSocket == null || !_clientSocket.IsBound || !_clientSocket.Connected) // Aggiunto controllo Connected
                {
                    _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }

                // Utilizza Socket.ConnectAsync con SocketAsyncEventArgs per la connessione asincrona
                var connectArgs = new SocketAsyncEventArgs();
                connectArgs.RemoteEndPoint = _remoteEndPoint;

                // TaskCompletionSource per attendere il completamento della connessione
                var connectCompletionSource = new TaskCompletionSource<bool>();
                connectArgs.Completed += (s, e) =>
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        connectCompletionSource.SetResult(true);
                    }
                    else
                    {
                        connectCompletionSource.SetException(new SocketException((int)e.SocketError));
                    }
                };

                // Avvia l'operazione di connessione asincrona
                if (!_clientSocket.ConnectAsync(connectArgs))
                {
                    // Se l'operazione è completata sincronicamente (raro), gestisci il risultato
                    if (connectArgs.SocketError == SocketError.Success)
                    {
                        connectCompletionSource.SetResult(true);
                    }
                    else
                    {
                        connectCompletionSource.SetException(new SocketException((int)connectArgs.SocketError));
                    }
                }

                // Attendi il completamento della connessione
                await connectCompletionSource.Task;
                _isConnected = true;
                return true;
            }
            catch (SocketException se)
            {
                Console.WriteLine($"Errore Socket durante la connessione a {_ipAddress}:{_port}: {se.Message} (Codice: {se.SocketErrorCode})");
                _isConnected = false;
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore generico durante la connessione a {_ipAddress}:{_port}: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Disconnette il socket.
        /// </summary>
        public void Disconnect()
        {
            if (_clientSocket != null)
            {
                try
                {
                    if (_clientSocket.Connected)
                    {
                        _clientSocket.Shutdown(SocketShutdown.Both);
                        _clientSocket.Disconnect(false);
                    }
                }
                catch (SocketException se)
                {
                    Console.WriteLine($"Errore Socket durante la disconnessione da {_ipAddress}:{_port}: {se.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore generico durante la disconnessione da {_ipAddress}:{_port}: {ex.Message}");
                }
                finally
                {
                    _clientSocket.Close();
                    _clientSocket.Dispose();
                    _clientSocket = null; // Rilascia il riferimento
                    _isConnected = false;
                }
            }
        }

        /// <summary>
        /// Invia un messaggio strutturato al server in modo asincrono.
        /// </summary>
        /// <param name="message">Il messaggio SocketMessageStructure da inviare.</param>
        /// <returns>True se l'invio ha successo, altrimenti false.</returns>
        public async Task<bool> SendMessageAsync(SocketMessageStructure message)
        {
            //2025.05.21 indipendenetemente dallo stato della connessione provo ad aprirla
            //if (_clientSocket == null || !_clientSocket.Connected)
            //{
            //    if (this._logger != null)
            //        this._logger.Log(LogLevel.ERROR, "Errore: Socket non connesso o non inizializzato.");
            //    // (Se hai la tua gestione degli errori, usala qui)
            //    return false;
            //}
            if (!await ConnectAsync())
            {
                this._logger.Log(LogLevel.ERROR, "Fallita la riconnessione. Impossibile inviare.");
                return false;
            }
            try
            {
                // 1. Serializza il messaggio in XML e poi in Base64
                string messageXml = SocketMessageSerializer.Serialize(message);
                byte[] xmlBytes = Encoding.UTF8.GetBytes(messageXml);
                string messageBase64 = Convert.ToBase64String(xmlBytes);

                // 2. Aggiungi i delimitatori Base64 e codifica il tutto in byte
                string finalMessageWithDelimiters = SocketMessageSerializer.Base64Start + messageBase64 + SocketMessageSerializer.Base64End;
                byte[] dataToSend = Encoding.UTF8.GetBytes(finalMessageWithDelimiters);

                // 3. Prepara SocketAsyncEventArgs per l'operazione di invio asincrona
                var sendArgs = new SocketAsyncEventArgs();
                sendArgs.SetBuffer(dataToSend, 0, dataToSend.Length);

                var tcs = new TaskCompletionSource<bool>();

                sendArgs.Completed += (sender, e) =>
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        tcs.SetResult(true);
                        // (Se hai la tua gestione del successo, usala qui)
                    }
                    else
                    {
                        tcs.SetResult(false);
                        if (this._logger != null) 
                            this._logger.Log(LogLevel.ERROR, $"Errore Socket durante l'invio: {e.SocketError}");
                        // (Se hai la tua gestione degli errori, usala qui)
                    }
                };

                bool willRaiseEvent = _clientSocket.SendAsync(sendArgs);

                if (!willRaiseEvent)
                {
                    if (sendArgs.SocketError == SocketError.Success)
                    {
                        tcs.SetResult(true);
                        // (Se hai la tua gestione del successo, usala qui)
                    }
                    else
                    {
                        tcs.SetResult(false);
                        if (this._logger != null) 
                            this._logger.Log(LogLevel.ERROR, $"Errore Socket durante l'invio: {sendArgs.SocketError}");
                        // (Se hai la tua gestione degli errori, usala qui)
                    }
                }

                return await tcs.Task;
            }
            catch (SocketException ex)
            {
                if (this._logger != null)
                    this._logger.Log(LogLevel.ERROR, $"Errore Socket durante l'invio: {ex.Message} (Code: {ex.ErrorCode})");
                // (Se hai la tua gestione degli errori, usala qui)
                return false;
            }
            catch (Exception ex)
            {
                if (this._logger != null)
                    this._logger.Log(LogLevel.ERROR, $"Errore generico durante l'invio: {ex.Message}");
                // (Se hai la tua gestione degli errori, usala qui)
                return false;
            }
        }

        // (Il tuo metodo SendMessageAsync aggiornato con SocketAsyncEventArgs qui)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>
        public async Task<SocketMessageStructure> ReceiveMessageAsync(int timeoutMs = 0)
        {
            if (_clientSocket == null || !_clientSocket.Connected)
            {
                if (this._logger != null)
                    this._logger.Log(LogLevel.ERROR, "Errore: Client non connesso per la ricezione.");
                return null;
            }

            if (timeoutMs > 0)
            {
                _clientSocket.ReceiveTimeout = timeoutMs;
            }
            else
            {
                _clientSocket.ReceiveTimeout = System.Threading.Timeout.Infinite;
            }
            bool willRaiseEvent = false;
            var receiveArgs = new SocketAsyncEventArgs();
            byte[] buffer = new byte[4096];
            receiveArgs.SetBuffer(buffer, 0, buffer.Length);
            var tcs = new TaskCompletionSource<SocketMessageStructure>();

            EventHandler<SocketAsyncEventArgs> completedHandler = null;
            completedHandler = (sender, e) =>
            {
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    string receivedChunk = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                    _receiveBuffer.Append(receivedChunk);

                    string fullMessage = _receiveBuffer.ToString();
                    int start = fullMessage.IndexOf(SocketMessageSerializer.Base64Start);
                    int end = fullMessage.IndexOf(SocketMessageSerializer.Base64End, start + SocketMessageSerializer.Base64Start.Length);

                    if (start != -1 && end != -1)
                    {
                        string base64Content = fullMessage.Substring(start + SocketMessageSerializer.Base64Start.Length, end - (start + SocketMessageSerializer.Base64Start.Length));
                        _receiveBuffer.Clear(); // Pulisci il buffer per messaggi futuri

                        try
                        {
                            byte[] xmlBytes = Convert.FromBase64String(base64Content);
                            string xmlString = Encoding.UTF8.GetString(xmlBytes);
                            SocketMessageStructure message = SocketMessageSerializer.DeserializeUTF8(xmlString);
                            if(this._logger != null)
                            {
                                this._logger.Log(LogLevel.DEBUG, message.MessageType);
                                this._logger.Log(LogLevel.DEBUG, message.Token);
                                this._logger.Log(LogLevel.DEBUG, message.SendingTime.ToString("gg/MM/yyyy HH:mm:ss"));
                                this._logger.Log(LogLevel.DEBUG, message.BufferDati.ToString());
                            }
                            tcs.SetResult(message);
                        }
                        catch (FormatException ex)
                        {
                            if (this._logger != null)
                                this._logger.Log(LogLevel.ERROR, $"Errore formato Base64 durante la ricezione: {ex.Message}");
                            tcs.SetResult(null);
                        }
                        catch (XmlException ex)
                        {
                            if (this._logger != null)
                                this._logger.Log(LogLevel.ERROR, $"Errore XML durante la deserializzazione: {ex.Message}");
                            tcs.SetResult(null);
                        }
                        catch (Exception ex)
                        {
                            if (this._logger != null)
                                this._logger.Log(LogLevel.ERROR, $"Errore generico durante la deserializzazione: {ex.Message}");
                            tcs.SetResult(null);
                        }
                    }
                    else
                    {
                        // Continua a ricevere se il messaggio completo non è ancora arrivato
                        willRaiseEvent = _clientSocket.ReceiveAsync(receiveArgs);
                        if (!willRaiseEvent)
                        {
                            completedHandler(sender, receiveArgs); // Elabora sincrono
                        }
                    }
                }
                else if (e.SocketError != SocketError.Success)
                {
                    if (this._logger != null)
                        this._logger.Log(LogLevel.ERROR, $"Errore durante la ricezione: {e.SocketError}");
                    tcs.SetResult(null);
                }
                else // e.BytesTransferred == 0 significa che la connessione è stata chiusa
                {
                    if (this._logger != null)
                        this._logger.Log(LogLevel.ERROR, "Connessione chiusa dal server durante la ricezione.");
                    tcs.SetResult(null);
                }

                // IMPORTANTE: Una volta che abbiamo un risultato (successo o fallimento),
                // dobbiamo rimuovere l'handler per evitare che venga chiamato di nuovo
                // per la stessa operazione asincrona.
                receiveArgs.Completed -= completedHandler;
            };

            receiveArgs.Completed += completedHandler;

            willRaiseEvent = _clientSocket.ReceiveAsync(receiveArgs);
            if (!willRaiseEvent)
            {
                completedHandler(_clientSocket, receiveArgs); // Elabora sincrono
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        /// <summary>
        /// Handler per l'evento Completed di SocketAsyncEventArgs per l'invio.
        /// </summary>
        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            HandleSendCompleted(e);
        }

        /// <summary>
        /// Logica per gestire il completamento dell'operazione di invio.
        /// </summary>
        private void HandleSendCompleted(SocketAsyncEventArgs e)
        {
            var state = e.UserToken as SendOperationState;
            if (state == null)
            {
                _sendCompletionSource.TrySetException(new InvalidOperationException("UserToken non valido in SocketAsyncEventArgs."));
                return;
            }

            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                OnMessageSentSuccess(state.MessageToken, state.MessageType, state.TargetEndpoint);
                _sendCompletionSource.TrySetResult(true);
            }
            else
            {
                string errorMessage = $"Invio fallito. SocketError: {e.SocketError}, BytesTransferred: {e.BytesTransferred}";
                if (e.SocketError != SocketError.Success)
                {
                    errorMessage = $"Invio fallito. Errore Socket: {new SocketException((int)e.SocketError).Message} (Codice: {e.SocketError})";
                }
                else if (e.BytesTransferred == 0)
                {
                    errorMessage = "Invio fallito: 0 byte trasferiti.";
                }

                OnMessageSentFailed(state.MessageToken, state.MessageType, state.TargetEndpoint, errorMessage);
                _sendCompletionSource.TrySetResult(false);
                _isConnected = false; // Presumi che la connessione sia persa in caso di errore di invio
            }

            // Rilascia le risorse di SocketAsyncEventArgs se non riutilizzate
            e.Dispose();
        }

        // Classe interna per mantenere lo stato dell'operazione di invio
        private class SendOperationState
        {
            public string MessageToken { get; set; }
            public string MessageType { get; set; }
            public string TargetEndpoint { get; set; }
        }

        /// <summary>
        /// Metodo per sollevare l'evento di ricezione da client per la chiusura del ciclo di trasmissione
        /// </summary>
        /// <param name="token"></param>
        /// <param name="messageType"></param>
        /// <param name="targetEndpoint"></param>
        protected virtual void OnMessageSentSuccess(string token, string messageType, string targetEndpoint)
        {
            MessageSentSuccess?.Invoke(this, new MessageSentEventArgs(token, messageType, targetEndpoint));
        }
        /// <summary>
        /// Metodo per sollevare l'evento di trasmissione fallita
        /// </summary>
        /// <param name="token"></param>
        /// <param name="messageType"></param>
        /// <param name="targetEndpoint"></param>
        /// <param name="errorMessage"></param>
        protected virtual void OnMessageSentFailed(string token, string messageType, string targetEndpoint, string errorMessage)
        {
            MessageSentFailed?.Invoke(this, new MessageSentEventArgs(messageType, token, targetEndpoint));
        }
        /// <summary>
        /// Classe EventArgs per gli eventi di invio
        /// </summary>
        public class MessageSentEventArgs : EventArgs
        {
            public string MessageType { get; }
            public string Token { get; }
            public string Endpoint { get; }

            public MessageSentEventArgs(string messageType, string token, string endpoint)
            {
                MessageType = messageType;
                Token = token;
                Endpoint = endpoint;
            }
        }
        public class MessageSentFailedEventArgs : MessageSentEventArgs
        {
            public string ErrorMessage { get; }

            public MessageSentFailedEventArgs(string messageType, string token, string endpoint, string errorMessage)
                : base(messageType, token, endpoint)
            {
                ErrorMessage = errorMessage;
            }
        }
        // Implementazione di IDisposable per garantire la pulizia delle risorse
        public void Dispose()
        {
            Disconnect(); // Assicurati che il socket sia chiuso e smaltito
            GC.SuppressFinalize(this);
        }
    }
}
