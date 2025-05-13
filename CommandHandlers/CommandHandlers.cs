using AsyncSocketServer;
using CommandHandlers.Properties;
using EvolutiveSystem.Core;
using MasterLog;
using MessaggiErrore;
using SocketManagerInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public void CmdSync(SocketCommand DvCmd, string Param, AsyncSocketListener asl)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                string command = checkCommand(DvCmd.CmdSync, DvCmd);
                response = new SocketMessageStructure
                {
                    Command = command.Substring(1, command.Length - 2),
                    SendingTime = DateTime.Now,
                    Data = "",
                    Token = asl.TokenSocket,
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
        public void CmdOpenDB(SocketCommand DvCmd, string Param, AsyncSocketListener asl)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                string command = checkCommand(DvCmd.CmdOpenDB, DvCmd);
                response = new SocketMessageStructure
                {
                    Command = command.Substring(1, command.Length - 2),
                    SendingTime = DateTime.Now,
                    Data = Param,
                    Token = asl.TokenSocket,
                    CRC = 0
                };
                if (string.IsNullOrWhiteSpace(response.Data)) 
                {
                    throw (new Exception("Percorso file database mancante nel messaggio."));
                }
                if (!File.Exists(response.Data))
                {
                    throw (new Exception("File non trovato"));
                }
                Database loadedDb = DatabaseSerializer.DeserializeFromXmlFile(response.Data);
                if (_loadedDatabases.Any(db => db.DatabaseId == loadedDb.DatabaseId || db.DatabaseName.Equals(loadedDb.DatabaseName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw (new Exception(string.Format("Il database {0} è già caricato", loadedDb.DatabaseName)));
                }
                _loadedDatabases.Add(loadedDb);

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
        public void CmdSaveDB(SocketCommand DvCmd, string Param, AsyncSocketListener asl)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                string command = checkCommand(DvCmd.CmdSaveDB, DvCmd);
                response = new SocketMessageStructure
                {
                    Command = command.Substring(1, command.Length - 2),
                    SendingTime = DateTime.Now,
                    Data = Param,
                    Token = asl.TokenSocket,
                    CRC = 0
                };
                if (response.Data == null) 
                {
                    throw (new Exception($"Comando CmdSaveDb ricevuto da {IPAddress.Parse(((IPEndPoint)asl.Handler.RemoteEndPoint).Address.ToString())} ma BufferDati mancante."));
                }
                if (string.IsNullOrWhiteSpace(response.Data))
                {
                    throw (new Exception("Percorso file database mancante nel messaggio."));
                }
                if (!File.Exists(response.Data))
                {
                    throw (new Exception("File non trovato"));
                }

            }
            catch (Exception ex)
            {

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
