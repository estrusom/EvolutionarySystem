using AsyncSocketServer;
using CommandHandlers.Properties;
using EvolutiveSystem.Core;
using MasterLog;
using MessaggiErrore;
using SocketManagerInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CommandHandlers
{
    public partial class ClsCommandHandlers
    {
        private const string prefisso = "<CommandHandlers>";
        private Logger _loger = null;
        private string lastCmdExec = "";
        private SocketCommand GdvCmd = new SocketCommand();//Global variable containing the communication status
        private SocketMessageStructure response = null;
        private readonly List<Database> _loadedDatabases = new List<Database>(); // Esempio: Gestione interna semplice
        public ClsCommandHandlers()
        {

        }
        public ClsCommandHandlers(Logger Log)
        {
            _loger = Log;
        }
        /// <summary>
        /// Controllo esistenze e connessione stabile col server socket
        /// </summary>
        /// <param name="DvCmd">comando da eseguire</param>
        /// <param name="Param">parametri da aggiungere al campo data della classse SocketMessageStructure</param>
        /// <param name="asl">Riferimenti al socket server</param>
        /// <exception cref="Exception"></exception>
        public void CmdSync(SocketCommand DvCmd, XElement Param, AsyncSocketListener asl)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                string command = checkCommand(DvCmd.CmdSync, DvCmd);
                response = new SocketMessageStructure
                {
                    Command = command.Substring(1, command.Length - 2),
                    SendingTime = DateTime.Now,
                    BufferDati = null,
                    Token = asl.TokenSocket.ToString(),
                    CRC = 0
                };
                string telegramGenerate = SocketMessageSerialize.SerializeUTF8(response);
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
                asl.Send(asl.Handler, txtSendData);
            }
            catch (Exception ex)
            {
                string msg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                if (ex.InnerException != null)
                {
                    throw new Exception(msg, ex.InnerException);
                }
                else
                {
                    throw new Exception(msg);
                }
            }
        }
        /// <summary>
        /// Apertura del database specificato nel campo data della classe SocketMessageStructure
        /// </summary>
        /// <param name="DvCmd"></param>
        /// <param name="Param"></param>
        /// <param name="asl"></param>
        /// <exception cref="Exception"></exception>
        public void CmdOpenDB(SocketCommand DvCmd, XElement Param, AsyncSocketListener asl)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                string command = checkCommand(DvCmd.CmdOpenDB, DvCmd);
                response = new SocketMessageStructure
                {
                    Command = command.Substring(1, command.Length - 2),
                    SendingTime = DateTime.Now,
                    BufferDati= new XElement("SyncDetails",
                                        new XElement("ServerTime", DateTime.UtcNow.ToString("o")), // Orario del server in formato ISO 8601
                                        new XElement("Status", "OK")), // Esempio di stato
                                        // Aggiungi qui altre informazioni di sincronizzazione se necessario
                    Token = asl.TokenSocket.ToString(),
                    CRC = 0
                };
                if (Param == null)
                {
                    throw (new Exception($"Comando CmdSaveDb ricevuto da {IPAddress.Parse(((IPEndPoint)asl.Handler.RemoteEndPoint).Address.ToString())} ma BufferDati mancante."));
                }
                XElement filePathElement = Param.Element("FilePath");
                if (filePathElement == null || string.IsNullOrWhiteSpace(filePathElement.Value))
                {
                    throw (new Exception("File non trovato"));
                }
                string dbFilePath = filePathElement.Value;
                Database loadedDb= DatabaseSerializer.DeserializeFromXmlFile(dbFilePath);
                if (filePathElement == null || string.IsNullOrWhiteSpace(filePathElement.Value))
                {
                    throw (new Exception($"Comando OpenDb ricevuto da {asl.CallerIpAddress} ma FilePath mancante o vuoto nel BufferDati."));
                }
                if (!File.Exists(dbFilePath))
                {
                    throw (new Exception($"il file{dbFilePath} ricevuto da  {asl.CallerIpAddress} no è sato trovato."));
                }
                _loadedDatabases.Add(loadedDb);

                XElement bufferContent = new XElement("DatabaseInfo",
                                            new XElement("FilePath", dbFilePath) // Utile per il client sapere da dove è stato caricato
                                            );
                string telegramGenerate = SocketMessageSerialize.SerializeUTF8(response);
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
                asl.Send(asl.Handler, txtSendData);

            }
            catch (FileNotFoundException ex)
            {
                string msg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                if (ex.InnerException != null)
                {
                    throw new Exception(msg, ex.InnerException);
                }
                else
                {
                    throw new Exception(msg);
                }
            }
            catch (Exception ex)
            {
                string msg = ClsMessaggiErrore.CustomMsg(ex, thisMethod);
                if (ex.InnerException != null)
                {
                    throw new Exception(msg, ex.InnerException);
                }
                else
                {
                    throw new Exception(msg);
                }
            }
        }
        /// <summary>
        /// salvataggio di un database
        /// </summary>
        /// <param name="DvCmd">comando da eseguire</param>
        /// <param name="Param">parametri da aggiungere al campo data della classse SocketMessageStructure</param>
        /// <param name="asl">Riferimenti al socket server</param>
        public void CmdSaveDB(SocketCommand DvCmd, XElement Param, AsyncSocketListener asl)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                string command = checkCommand(DvCmd.CmdSaveDB, DvCmd);
                response = new SocketMessageStructure
                {
                    Command = command.Substring(1, command.Length - 2),
                    SendingTime = DateTime.Now,
                    BufferDati = new XElement("SyncDetails",
                                        new XElement("ServerTime", DateTime.UtcNow.ToString("o")), // Orario del server in formato ISO 8601
                                        new XElement("Status", "OK")), // Esempio di stato
                                                                       // Aggiungi qui altre informazioni di sincronizzazione se necessario
                    Token = asl.TokenSocket.ToString(),
                    CRC = 0
                };
                if (Param == null)
                {
                    throw (new Exception($"Comando CmdSaveDb ricevuto da {IPAddress.Parse(((IPEndPoint)asl.Handler.RemoteEndPoint).Address.ToString())} ma BufferDati mancante."));
                }

                // Estrai l'identificatore del database e il percorso
                XElement dbIdentifierElement = Param.Element("DatabaseIdentifier");
                XElement filePathElement = Param.Element("FilePath");

                if (dbIdentifierElement == null || filePathElement == null)
                {
                    // _logger.LogWarning($"Comando CmdSaveDb: DatabaseIdentifier o FilePath mancante nel BufferDati.");
                    // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                    throw new Exception($"Comando CmdSaveDb ricevuto da {asl.CallerIpAddress} ma DatabaseIdentifier o FilePath mancante nel BufferDati.");
                }

                string identifierType = dbIdentifierElement.Attribute("Type")?.Value;
                string identifierValue = dbIdentifierElement.Value;
                string saveFilePath = filePathElement.Value;

                if (string.IsNullOrWhiteSpace(saveFilePath))
                {
                    // _logger.LogWarning($"Comando CmdSaveDb: Percorso file salvataggio mancante.");
                    // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                    throw new Exception($"Comando CmdSaveDb ricevuto da {asl.CallerIpAddress} ma percorso file salvataggio mancante.");
                }

                Database databaseToSave = null;
                if (identifierType?.Equals("Id", StringComparison.OrdinalIgnoreCase) == true && int.TryParse(identifierValue, out int dbId))
                {
                    databaseToSave = _loadedDatabases.FirstOrDefault(db => db.DatabaseId == dbId);
                }
                else if (identifierType?.Equals("Name", StringComparison.OrdinalIgnoreCase) == true && !string.IsNullOrWhiteSpace(identifierValue))
                {
                    databaseToSave = _loadedDatabases.FirstOrDefault(db => db.DatabaseName.Equals(identifierValue, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    // _logger.LogWarning($"Comando CmdSaveDb: Identificatore database non valido (Type='{identifierType}', Value='{identifierValue}').");
                    // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                    throw new Exception($"Comando CmdSaveDb ricevuto da {asl.CallerIpAddress} Identificatore database non valido (Type='{identifierType}', Value='{identifierValue}').");
                }

                if (databaseToSave == null)
                {
                    // _logger.LogWarning($"Comando CmdSaveDb: Database con identificatore '{identifierValue}' non trovato.");
                    // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                    throw new Exception($"Comando CmdSaveDb ricevuto da {asl.CallerIpAddress}. Database con identificatore '{identifierValue}' non trovato sul server.");
                }

                DatabaseSerializer.SerializeToXmlFile(databaseToSave, saveFilePath);

                response.BufferDati = new XElement("SyncDetails",
                                        new XElement("ServerTime", DateTime.UtcNow.ToString("o")), // Orario del server in formato ISO 8601
                                        new XElement("Database", $"Db {databaseToSave.DatabaseName}caricato"),
                                        new XElement("Status", "OK")); // Esempio di stato
                string telegramGenerate = SocketMessageSerialize.SerializeUTF8(response);
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";

                asl.Send(asl.Handler, txtSendData);
            }

            catch (UnauthorizedAccessException ex)
            {
                // _logger.LogError($"Errore di accesso al file per CmdSaveDb:", ex);
                // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                throw new Exception($"Errore: Accesso negato al percorso specificato '{ex.Message}'. Assicurarsi che il servizio abbia i permessi necessari.");
            }
            catch (PathTooLongException ex)
            {
                // _logger.LogError($"Percorso troppo lungo per CmdSaveDb:", ex);
                // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                throw new Exception($"Errore: Il percorso specificato è troppo lungo '{ex.Message}'.");
            }
            catch (DirectoryNotFoundException ex)
            {
                // _logger.LogError($"Directory non trovata per CmdSaveDb:", ex);
                // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                throw new Exception($"Errore: La directory specificata non esiste '{ex.Message}'.");
            }
            catch (IOException ex)
            {
                // Gestisce altri errori di I/O (es. file in uso)
                // _logger.LogError($"Errore di I/O per CmdSaveDb:", ex);
                // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                throw new Exception($"Errore di I/O durante il salvataggio del file: {ex.Message}");
            }
            catch (System.Xml.XmlException ex) // Potrebbe verificarsi durante la serializzazione se ci sono problemi con i dati
            {
                // _logger.LogError($"Errore di serializzazione XML per CmdSaveDb:", ex);
                // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                throw new Exception($"Errore durante la serializzazione del database in XML: {ex.Message}");
            }
            catch (InvalidOperationException ex) // Potrebbe verificarsi se ci sono problemi con i tipi noti nella serializzazione
            {
                // _logger.LogError($"Errore di operazione non valida per CmdSaveDb:", ex);
                // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                throw new Exception($"Errore interno di serializzazione: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Gestione generica degli errori per qualsiasi altra eccezione
                Console.WriteLine($"Errore generico durante la gestione del comando CmdSaveDb: {ex.Message}");
                // _logger.LogError($"Errore generico durante la gestione del comando CmdSaveDb:", ex);

                // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                throw new Exception($"Errore generico durante l'elaborazione del comando CmdSaveDb: {ex.Message}");
            }
        }
        /// <summary>
        /// Receiving a command, it determines the action to take, communicate on the serial or perform internal actions.
        /// For example: ask the device to cancel its display, retrieve the device's plate information or turn the communication on or off, pause the service
        /// </summary>
        /// <param name="cmd">Command to execute</param>
        /// <param name="DevCmd">List of available commands to search</param>
        /// <returns></returns>
        private string checkCommand(string cmd, SocketCommand DevCmd)
        {
            // SyncComMutex.WaitOne();
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            Type tyCmd = DevCmd.GetType();
            string ret = "";
            string msg = "";
            try
            {
                var getComCmd = tyCmd.GetProperties().Where(CMD => CMD.Name.ToUpper().Equals(cmd.ToUpper().Substring(1,cmd.Length-2)));
                if (getComCmd.Any())
                {
                    ret = getComCmd.First().GetValue(DevCmd).ToString();
                    // Console.WriteLine(getComCmd.First().CustomAttributes.First().NamedArguments[0].TypedValue.Value);
                    // I get the name of the command
                    var cName = ((System.Reflection.TypeInfo)tyCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("CommandName"));
                    if (cName.Any())
                    {
                        cName.First().SetValue(DevCmd, getComCmd.First().Name); // set command name
                    }
                    // I get the command code
                    var cCode = ((System.Reflection.TypeInfo)tyCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("CommandCode"));
                    if (cCode.Any())
                    {
                        cCode.First().SetValue(DevCmd, ret); // set command code
                    }
                    /*
                    //Check if the command to be activated is a signature type
                    var cStat = ((System.Reflection.TypeInfo)tyCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("IsSignCommand"));
                    if (cStat.Any())
                    {
                        var sic = getComCmd.First().CustomAttributes.First().NamedArguments.Where(A => A.MemberName.Equals("IsSignCommand"));
                        if (sic.Any())
                        {
                            cStat.First().SetValue(DevCmd, sic.First().TypedValue.Value); // Signature attribute assignment
                        }

                    }
                    */
                    var cWait = ((System.Reflection.TypeInfo)tyCmd).DeclaredProperties.Where(VAL => VAL.Name.Contains("WaitResponse"));
                    if (cWait.Any())
                    {
                        var wr = getComCmd.First().CustomAttributes.First().NamedArguments.Where(A => A.MemberName.Equals("WaitResponse"));
                        if (wr.Any())
                        {
                            cWait.First().SetValue(DevCmd, wr.First().TypedValue.Value);
                        }
                    }
                    this.lastCmdExec = DevCmd.CommandName;
                    GdvCmd = DevCmd; //To find out what happens and return the command that generates an error or a timeout, you need to assign a global instance of the DeviceComand class that carries around the last command executed
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                throw new Exception(ClsMessaggiErrore.CustomMsg(ex, thisMethod));
            }
            finally
            {
                if(_loger != null)
                    _loger.Log(LogLevel.DEBUG, string.Format("{0} command {1} errStat [{2}]", prefisso, cmd, msg));
                // SyncComMutex.ReleaseMutex();
            }
            return ret;
        }
    }
}
