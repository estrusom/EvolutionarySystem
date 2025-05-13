using MessaggiErrore;
using SocketManagerInfo.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SocketManagerInfo
{
    public partial class SocketMessageStructure
    {
        /// <summary>
        /// Command to execute
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// Sending date
        /// </summary>
        public DateTime SendingTime { get; set; }
        /// <summary>
        /// Data buffer
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 02.04.2021 gestione accesso concorrente al socket
        /// </summary>
        public long Token { get; set; }
        /// <summary>
        /// hash check
        /// </summary>
        public int CRC { get; set; }
    }
    public static class SocketMessageSerialize
    {
        public static string SerializeUTF8(SocketMessageStructure messageStructure)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            string xmlRet = "";
            try
            {
                XmlSerializer xmls = new XmlSerializer(typeof(SocketMessageStructure));

                // Usiamo StringWriter per scrivere l'XML in memoria come stringa.
                using (StringWriter sw = new StringWriter())
                {
                    // Creiamo le impostazioni per l'XmlWriter per specificare l'encoding UTF-8.
                    XmlWriterSettings settings = new XmlWriterSettings()
                    {
                        Encoding = Encoding.UTF8, // *** Imposta esplicitamente l'encoding a UTF-8 ***
                        Indent = false,           // *** Imposta a false per una stringa compatta per la trasmissione ***
                                                  // Imposta a true se vuoi l'XML indentato per debugging/leggibilità
                        OmitXmlDeclaration = false // Assicurati che il prologo XML (con encoding="utf-8") sia incluso
                    };

                    // Creiamo un XmlWriter che scriverà nello StringWriter con le nostre impostazioni.
                    using (XmlWriter xmlw = XmlWriter.Create(sw, settings))
                    {
                        // Serializziamo l'oggetto usando l'XmlWriter.
                        xmls.Serialize(xmlw, messageStructure);
                    }

                    // Ora lo StringWriter contiene la stringa XML con prologo encoding="utf-8".
                    xmlRet = sw.ToString();
                }
            }
            catch (Exception ex)
            {
                // Gestione degli errori: Assicurati che ClsMessaggiErrore sia accessibile e gestisca il contesto.
                // Se ClsMessaggiErrore.CustomMsg richiede MethodBase, passalo.
                // string myMessage = ClsMessaggiErrore.CustomMsg(ex, thisMethod) + "\t";
                // throw new Exception(myMessage);
                // In alternativa, rilancia l'eccezione originale o loggala diversamente.
                string myMessage = ClsMessaggiErrore.CustomMsg(ex, thisMethod) + "\t";
                throw new Exception(myMessage);

            }
            return xmlRet;
        }
        /// <summary>
        /// Classe statica per gestire la serializzazione e deserializzazione
        /// degli oggetti SocketMessageStructure in formato XML con encoding UTF-8.
        /// </summary>
        /// <param name="messageStructure"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string Serialize(SocketMessageStructure messageStructure)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            string xmlRet = "";
            try
            {
                XmlSerializer xmls = new XmlSerializer(typeof(SocketMessageStructure));
                using (StringWriter sw = new StringWriter())
                {
                    XmlWriterSettings settings = new XmlWriterSettings()
                    {
                        Encoding = Encoding.UTF8,
                        Indent = true,
                        OmitXmlDeclaration = false
                    };
                    using (XmlWriter xmlw = XmlWriter.Create(sw, settings)) 
                    {
                        xmls.Serialize(xmlw, messageStructure);
                    }
                    xmlRet = sw.ToString();
                }
            }
            catch (Exception ex)
            {
                string myMessage = ClsMessaggiErrore.CustomMsg(ex, thisMethod) + "\t";
                throw new Exception(myMessage);
            }
            return xmlRet;
        }
        /// <summary>
        /// Deserializza un oggetto SocketMessageStructure da una stringa XML.
        /// Assume che la stringa XML contenga un singolo oggetto SocketMessageStructure
        /// e che il prologo XML dichiari l'encoding corretto (es. utf-8).
        /// </summary>
        /// <param name="xmlString">La stringa contenente l'XML del messaggio.</param>
        /// <returns>L'oggetto SocketMessageStructure deserializzato, o null in caso di errore.</returns>
        public static SocketMessageStructure DeserializeUTF8(string xmlString)
        {
            // Utilizzato per ottenere informazioni sul metodo corrente per il logging degli errori.
            MethodBase thisMethod = MethodBase.GetCurrentMethod();

            SocketMessageStructure socketMessageStructure = null;
            try
            {
                // Rimosso l'uso di XmlDocument - non necessario.
                // XmlDocument myXml = new XmlDocument();
                // myXml.InnerXml = xmlString; // Questo potrebbe causare problemi di encoding se l'InnerXml non è gestito correttamente

                XmlSerializer xmls = new XmlSerializer(typeof(SocketMessageStructure)); // *** Corretto: Deserializza un singolo oggetto ***

                // Usiamo StringReader per leggere l'XML dalla stringa in memoria.
                // XmlSerializer/XmlReader leggeranno automaticamente il prologo XML
                // per determinare l'encoding dei byte sottostanti (che C# gestisce internamente per la stringa).
                using (TextReader textReader = new StringReader(xmlString))
                {
                    // Deserializza l'oggetto dallo stream.
                    socketMessageStructure = (SocketMessageStructure)xmls.Deserialize(textReader);
                }
            }
            catch (Exception ex)
            {
                // Gestione degli errori: Utilizza ClsMessaggiErrore per creare un messaggio di log dettagliato.
                // Assicurati che ClsMessaggiErrore sia accessibile e gestisca il contesto statico.
                string myMessage = ClsMessaggiErrore.CustomMsg(ex, thisMethod) + "\t";
                throw new Exception(myMessage); // Rilancia una nuova eccezione con il messaggio personalizzato
                                                // In alternativa, se non vuoi avvolgere l'eccezione originale:
                                                // Console.WriteLine($"Errore di deserializzazione: {myMessage}"); // Logga l'errore
                                                // throw; // Rilancia l'eccezione originale
            }
            return socketMessageStructure;
        }
        public static SocketMessageStructure Deserialize(string Xml)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            SocketMessageStructure socketMessageStructure = null;
            try
            {
                XmlDocument myXml = new XmlDocument();
                myXml.InnerXml = Xml;
                XmlSerializer xmls = new XmlSerializer(typeof(List<SocketMessageStructure>));
                using (TextReader textReader = new StringReader(myXml.InnerXml))
                {
                    socketMessageStructure = (SocketMessageStructure)xmls.Deserialize(textReader);
                }
            }
            catch (Exception ex)
            {
                string myMessage = ClsMessaggiErrore.CustomMsg(ex, thisMethod) + "\t";
                throw new Exception(myMessage);
            }
            return socketMessageStructure;
        }

        /// <summary>
        /// Stringa iniziale per identificare una stringa base64 proveniente dal socket
        /// </summary>
        public static string Base64Start { get { return SocketManagerRes.StartB64string; } }
        /// <summary>
        /// Stringa di chiusura di un messaggio ricevuto via socket
        /// </summary>
        public static string Base64End { get { return SocketManagerRes.EndB64String; } }
    }

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
        /// <summary>
        /// If 0, Command requires token control.
        /// If 1, the command must acquire the token for managing the concurrency of the socket server.
        /// if 2, The command does not support tokens.
        /// </summary>
        public byte TockenManaging { get; set; } //02.04.2021 gestione accesso concorrente alla tavoletta 
        /// <summary>
        /// Sending data in packets.
        /// If true, send in packets13.04.2021
        /// </summary>
        public bool SendingDataPackets { get; set; }
    }
    /*
    public class SocketCommand : IDisposable
    {
        private const string stx = "";// "<stx>";
        private const string etx = "";// "<etx>";
        private const string eof = "<eof>";
        private string cmd00 = stx + "CmdSync" + etx;
        private string cmd01 = stx + "CmdOpenDB" + etx;
        /// <summary>
        /// Sync command (watch dog) CMD = 00
        /// </summary>
        [SktProperty(Description = "Check comunication chanel", TockenManaging = 1)]
        public string CmdSync { get { return this.cmd00; } }
        /// <summary>
        /// Opening the working db
        /// </summary>
        public string CmdOpenDB { get { return this.cmd01; } }
        /// <summary>
        /// Final sequence to add to messages to be sent to the client
        /// </summary>
        public string Eof { get { return eof; } }
        /// <summary>
        /// Stringa iniziale per identificare una stringa base64 proveniente dal socket
        /// </summary>
        public string Base64Start { get { return SocketManagerRes.StartB64string; } }
        /// <summary>
        /// Stringa di chiusura di un messaggio ricevuto via socket
        /// </summary>
        public string Base64End { get { return SocketManagerRes.EndB64String; } }
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
    */
    /// <summary>
    /// 
    /// </summary>
    public class SocketCommand : IDisposable
    {
        private const char stx = '\u0002';
        private const char etx = '\u0003';
        private const string eof = "<eof>";
        private string cmd00 = stx + "CmdSync" + etx;   // 29.03.2021 era cmdSync
        private string cmd05 = stx + "CmdOpenDB" + etx;
        private string cmd06 = stx + "CmdSaveDB" + etx;
        private string cmd07 = stx + "CmdCancSignSurface" + etx;
        private string cmd08 = stx + "CmdSigningProcStarted" + etx;
        private string cmd09 = stx + "CmdSigningProcTermination" + etx;
        private string cmd0A = stx + "CmdHideSignatureForm " + etx;
        private string cmd0B = stx + "CmdSendFormConfiguration[{0}]" + etx;
        private string cmd0R = stx + "CmdDeviceReset" + etx;
        private string cmd1A = stx + "CmdActivationSignatureForm" + etx;
        private string cmd1B = stx + "CmdDownloadLog" + etx;
        private string cmdEE = stx + "ErrUSBsconected" + etx;

        private string sktServiceSuspended = stx + "PAUSE" + etx;
        private string sktServiceAwakening = stx + "AWAKE" + etx;
        private string sktStopService = stx + "STOP" + etx;
        private string sktOpenComPort = stx + "OPEN" + etx;
        private string sktCloseComPort = stx + "CLOSE" + etx;
        private string sktSocketSrvPort = stx + "PORTINIT" + etx;
        private string sktClearScreen = stx + "CLEARSCREEN" + etx;
        private string sktCancelSignature = stx + "CANCEL" + etx;
        private string sktPaxRestart = stx + "PAXRESTART" + etx; // 31.03.2022 commant restarting PAX Airies 8
        /// <summary>
        /// Contenitore del comando proveniente dal client socket
        /// </summary>
        public string CommandSocket { get; set; }
        /// <summary>
        /// Sync command (watch dog) CMD = 00
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Check comunication chanel", TockenManaging = 1)]
        public string CmdSync { get { return this.cmd00; } } // 29.03.2021 era cmdSync
        /// <summary>
        /// Apertura del database
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Device nameplate data request", TockenManaging = 2)]
        public string CmdOpenDB { get { return this.cmd05; } }
        /// <summary>
        /// Salvataggio del database
        /// </summary>
        public string CmdSaveDB { get { return this.cmd06; } }
        /*
        /// <summary>
        /// Single signature frame reception CMD = 06
        /// </summary>
        [SktProperty(IsSignatureRequest = 2, Description = "Single signature frame reception", TockenManaging = 0)]
        public string CmdRxDataSign { get { return this.cmd06; } }
        /// <summary>
        /// Command to clearing signature surface area CMD = 07
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Command to clearing signature surface area", TockenManaging = 0)]
        public string CmdCancSignSurface { get { return this.cmd07; } }
        /// <summary>
        /// Start acquiring signature CMD = 08
        /// </summary>
        [SktProperty(IsSignatureRequest = 1, Description = "Start acquiring signature", TockenManaging = 0)]
        public string CmdSigningProcStarted { get { return this.cmd08; } }
        /// <summary>
        /// Signing process ended CMD = 09
        /// </summary>
        [SktProperty(IsSignatureRequest = 3, Description = "Signing process ended", TockenManaging = 0)]
        public string CmdSigningProcTermination { get { return this.cmd09; } }
        /// <summary>
        /// Hide signature form CMD = 0A
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Hide signature form", TockenManaging = 0)]
        public string CmdHideSignatureForm { get { return this.cmd0A; } }
        /// <summary>
        /// Show signature form CMD = 1A
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Show signature form", TockenManaging = 0)]
        public string CmdActivationSignatureForm { get { return this.cmd1A; } }
        /// <summary>
        /// Send the background configuration data to the device CMD = 0B
        /// Added attribute for packet sending 13.04.2021 
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Send the background configuration data to the device CMD = 0B", TockenManaging = 0, SendingDataPackets = true)]
        public string CmdSendFormConfiguration { get { return this.cmd0B; } }
        /// <summary>
        /// This command is device type, but behaves like a service command since it does not require a response. The response is sent to the client anyway
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "reset app on device")]
        public string CmdDeviceReset { get { return this.cmd0R; } }
        /// <summary>
        /// 18.05.2021 TockenManaging modificato da 1 a 2 per evitare il controllo del token durante il download
        /// Senza l'esclusione del controllo del token, all'orario pianificato per lo scarico dei log del tablet, accade l'errore di token mismatch
        /// Download and send the requested log file to the client CMD = 1B
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Download and send the requested log file to the client CMD = 1B", TockenManaging = 2)]
        public string CmdDownloadLog { get { return this.cmd1B; } }
        /// <summary>
        /// Response to client the usb cable sconected
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "Response message to client when USB cable is not conected", TockenManaging = 2)] //30.11.2021 prima era TockenManaging = 0
        public string ErrUSBsconected { get { return this.cmdEE; } }
        /// <summary>
        /// The service will be Awakening 
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "The service will be Awakening ")]
        public string ServiceAwakening { get { return this.sktServiceAwakening; } }
        /// <summary>
        /// Pax restart command
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "The tablet will be restarted", TockenManaging = 2)]
        public string PaxAries8Restart { get { return this.sktPaxRestart; } } // 31.03.2022 commant restarting PAX Airies 8
        /// <summary>
        /// The service will be stopped
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "The service will be stopped")]
        public string StopService { get { return this.sktStopService; } }
        /// <summary>
        /// The service will be suspended
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "The service will be suspended")]
        public string ServiceSuspended { get { return this.sktServiceSuspended; } }
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
        [SktProperty(IsSignatureRequest = 0, Description = "Transmits socket server the port number", TockenManaging = 2)]
        public string PortInit { get { return this.sktSocketSrvPort; } }
        /// <summary>
        /// It cleans the screen of the tablet, also of the background images
        /// </summary>
        [SktProperty(IsSignatureRequest = 0, Description = "It cleans the screen of the tablet, also of the background images")]
        public string ClearScreen { get { return this.sktClearScreen; } }
        /// <summary>
        /// Stops and cancels a signing process
        /// </summary>
        public string CancelSignature { get { return this.sktCancelSignature; } }
        */
        /// <summary>
        /// Telegram start sequence
        /// </summary>
        public char StartTelegram { get { return stx; } }
        /// <summary>
        /// End of telegram sequence
        /// </summary>
        public char EndTelegramm { get { return etx; } }
        /// <summary>
        /// Final sequence to add to messages to be sent to the client
        /// </summary>
        public string Eof { get { return eof; } }
        /// <summary>
        ///  Definition of the address where to send the graphometric data
        /// </summary>
        public string PortAddress { get; set; }
        /// <summary>
        /// Contains the name of the command sent in execution
        /// </summary>
        public string CommandName { get; set; }
        /// <summary>
        /// port a cui risponde il serber socket
        /// </summary>
        public int SocketPortSrv { get; set; }
        /// <summary>
        /// Indirizzo a cui risponde il server socket della firma
        /// </summary>
        public List<IPAddress> SocketAddressClient { get; set; }
        /// <summary>
        /// Oggetto socket usato per comunicare con il client chiamante
        /// </summary>
        public Socket SocketHandler { get; set; }
        /// <summary>
        /// port a cui risponde il server socket a cui inviare i dati biometrici
        /// </summary>
        public int SocketPortCli { get; set; }
        /// <summary>
        /// contiene eventuali risposte da trasferire al socket client
        /// </summary>
        public object Response { get; set; }
        /// <summary>
        /// 03.12.2021
        /// Stores the IP address and port number of the caller
        /// </summary>
        public string LocalEndPoint { get; set; }
        /// <summary>
        /// Valore del token per l'accesso alle funzioni della tavoletta 02.04.2021
        /// </summary>
        public int AccessToken { get; set; }
        /// <summary>
        /// Sending data in packets.
        /// If true, send in packets 13.04.2021
        /// </summary>
        public bool SendingDataPackets { get; set; }
        /// <summary>
        /// Stringa iniziale per identificare una stringa base64 proveniente dal socket
        /// </summary>
        public string Base64Start { get { return SocketManagerRes.StartB64string; } }
        /// <summary>
        /// Stringa di chiusura di un messaggio ricevuto via socket
        /// </summary>
        public string Base64End { get { return SocketManagerRes.EndB64String; } }
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
    public class SemanticServerInteractionCommands
    {

    }
}
