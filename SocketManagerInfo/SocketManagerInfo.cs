using MessaggiErrore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
        public static string Serilaize(SocketMessageStructure messageStructure)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            string xmlRet = "";
            try
            {
                XmlSerializer xmls = new XmlSerializer(typeof(SocketMessageStructure));
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlWriter xmlw = XmlWriter.Create(sw))
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
    }

    /// <summary>
    /// Properties definition for socket comunication command (attribute extension for DeviceCommand class)
    /// </summary>
    public class SktProperty : System.Attribute
    {
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
    public class SocketCommand : IDisposable
    {
        private const string stx = "";// "<stx>";
        private const string etx = "";// "<etx>";
        private const string eof = "<eof>";
        private string cmd00 = stx + "CmdSync" + etx;   
        /// <summary>
        /// Sync command (watch dog) CMD = 00
        /// </summary>
        [SktProperty(Description = "Check comunication chanel", TockenManaging = 1)]
        public string CmdSync { get { return this.cmd00; } }
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
    public class SemanticServerInteractionCommands
    {

    }
}
