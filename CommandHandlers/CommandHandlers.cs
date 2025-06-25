/*
 * 2025.05.16 Aggiunto: Proprietà per memorizzare il percorso del file associato a questo database *** VER 1.0.0.1 - 25.05.16.9
 * 2025.05.16 adesso il percorso del file lo vado a prendere dalla classe Database campo dalla classe FilePath
 * 2025.05.16 Aggiunta handler di gestione del comando richiesta sato del database 
 * 2025.05.19 Aggiunta la comunicazione di IP Address e IP Port per il client asincrono
 */
using AsyncSocketServer;
using CommandHandlers.Properties;
using EvolutiveSystem.SQL.Core;
using MasterLog;
using MessaggiErrore;
using SocketManager;
using SocketManagerInfo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
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
        // private readonly ConcurrentDictionary<string, string> _connectedUiEndpoints;
        private const string prefisso = "<CommandHandlers>";
        private Logger _loger = null;
        private string lastCmdExec = "";
        private SocketCommand GdvCmd = new SocketCommand();//Global variable containing the communication status
        private SocketMessageStructure response = null;
        private readonly List<Database> _loadedDatabases = new List<Database>(); // Esempio: Gestione interna semplice
        private static SQLiteConnection dbConnection;
        private readonly MIU.Core.IMIURepository _miuRepository; // Nuovo campo per IMIURepository
        

        public ClsCommandHandlers()
        {

        }
        public ClsCommandHandlers(Logger Log)
        {
            _loger = Log;
        }
        public  ClsCommandHandlers(Logger Log, ConcurrentDictionary<string, string> connectedUiEndpoints)
        {
            _loger = Log;
            //_connectedUiEndpoints = connectedUiEndpoints;
        }
        /// <summary>
        /// 2025.05.19 Aggiunta la comunicazione di IP Address e IP Port per il client asincrono
        /// Controllo esistenze e connessione stabile col server socket
        /// </summary>
        /// <param name="DvCmd">comando da eseguire</param>
        /// <param name="Param">parametri da aggiungere al campo data della classse SocketMessageStructure</param>
        /// <param name="asl">Riferimenti al socket server</param>
        /// <exception cref="Exception"></exception>
        public void CmdSync(SocketCommand DvCmd, XElement Param, AsyncSocketListener asl, ConcurrentDictionary<string, SemanticClientSocket> connectedUiEndpoints)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            string uiIpAddress = null;
            int uiPort = 0;
            string uiEndpointKey = "N/A:N/A"; // Valore di default in caso di errore di parsing
            try
            {
                if (Param == null)
                {
                    throw new Exception($"Comando CmdSync ricevuto da {IPAddress.Parse(((IPEndPoint)asl.Handler.RemoteEndPoint).Address.ToString())} ma BufferDati mancante.");
                }
                XElement UiIpAddressElement = Param.Element("UiIpAddress");
                if (UiIpAddressElement == null || string.IsNullOrWhiteSpace(UiIpAddressElement.Value))
                {
                    throw new Exception("IP server socket non trovato");
                }
                XElement UiPortElement = Param.Element("UiPort");
                if (UiPortElement== null || string.IsNullOrWhiteSpace(UiIpAddressElement.Value))
                {
                    throw new Exception("IP port socket non trovato");
                }


                uiIpAddress = UiIpAddressElement.Value;
                if (!int.TryParse(UiPortElement.Value, out uiPort)) // Usa TryParse
                {
                    Console.WriteLine($"Server Error: Formato porta UI non valido nel BufferDati di CmdSync ricevuto: {UiPortElement.Value}");
                    // eventLog1.WriteEntry($"Formato porta UI non valido nel BufferDati di CmdSync ricevuto: {UiPortElement.Value}", EventLogEntryType.Warning);
                    throw new Exception($"Formato porta non valido: {UiPortElement.Value}"); // Messaggio di errore più specifico
                }
                string command = checkCommand(DvCmd.CmdSync, DvCmd);

                uiEndpointKey = $"{uiIpAddress}:{uiPort}";
                SemanticClientSocket clientSocket = new SemanticClientSocket(uiIpAddress, uiPort, this._loger);
                bool connected = Task.Run(() => clientSocket.ConnectAsync()).Result; // Tenta la connessione asincrona
                string addedstatus = "";
                if (connected)
                {
                    bool added = connectedUiEndpoints.TryAdd(uiEndpointKey, clientSocket); // Chiave e Valore sono la stringa dell'endpoint
                    addedstatus = "";
                    if (added)
                    {
                        // L'aggiunta ha avuto successo: la chiave "IP:Porta" non esisteva.
                        // Questo è un nuovo endpoint UI che si sta sincronizzando e registrando per le notifiche.
                        // addedstatus= string.Format($"Server: Endpoint UI '{uiEndpointKey}' added successfully to dictionary for Connect-on-Demand notifications.");
                        addedstatus = "Chiave aggiunta";
                        // eventLog1.WriteEntry($"Endpoint UI '{uiEndpointKey}' aggiunto a dictionary.", EventLogEntryType.Information);

                        // *** NOTA: Per il modello Connect-on-Demand, NON creiamo un SemanticClientSocket qui. ***
                        // *** La creazione del SemanticClientSocket avverrà al momento dell'invio della notifica. ***

                    }
                    else
                    {
                        // L'aggiunta è fallita: la chiave "IP:Porta" esisteva già nella dictionary.
                        // Questo significa che il server ha già registrato un client UI con questo endpoint per le notifiche.
                        // Assumiamo che sia la stessa UI che si sta risincronizzando o che sia già nota.
                        //addedstatus = string.Format($"Server: Endpoint UI '{uiEndpointKey}' already exists in the dictionary for Connect-on-Demand notifications. No new entry added.");
                        string ping = asl.Ping(asl.CallerIpAddress);
                        addedstatus = "Chiave già presente";
                        // eventLog1.WriteEntry($"Endpoint UI '{uiEndpointKey}' già registrato.", EventLogEntryType.Information);

                        // Non c'è un'istanza di client socket da smaltire in questo modello, perché non l'abbiamo creata qui.
                        //clientSocket.Disconnect();
                        connectedUiEndpoints.TryRemove(uiEndpointKey, out SemanticClientSocket tryRemoveSocket);
                        added = connectedUiEndpoints.TryAdd(uiEndpointKey, clientSocket);
                    }
                }
                else
                {
                    clientSocket.Disconnect();
                }
                response = new SocketMessageStructure
                {
                    Command = command.Substring(1, command.Length - 2),
                    SendingTime = DateTime.Now,
                    BufferDati = new XElement("BufferDati",
                    new XElement("TCPIP",
                    new XElement("ipAddress", uiIpAddress),
                    new XElement("ipPort", uiPort.ToString()),
                    new XElement("Stato", addedstatus))),
                    Token = asl.TokenSocket.ToString(),
                    CRC = 0
                };

                string telegramGenerate = SocketMessageSerializer.SerializeUTF8(response);
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                //string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
                string txtSendData = SocketMessageSerializer.Base64Start + Convert.ToBase64String(bytes, 0, bytes.Length) + SocketMessageSerializer.Base64End;
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
        public void CmdOpenDB(SocketCommand DvCmd, XElement Param, AsyncSocketListener asl, out SQLiteConnection DbConnection)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                string command = checkCommand(DvCmd.CmdOpenDB, DvCmd);
                string dbFilePath = "";
                string dbStatus = "CLOSE";
                if (Param == null)
                {
                    throw new Exception($"Comando CmdSaveDb ricevuto da {IPAddress.Parse(((IPEndPoint)asl.Handler.RemoteEndPoint).Address.ToString())} ma BufferDati mancante.");
                }
                XElement filePathElement = Param.Element("FilePath");
                if (filePathElement == null || string.IsNullOrWhiteSpace(filePathElement.Value))
                {
                    throw new Exception("File non trovato");
                }
                if (dbConnection == null)
                {
                    dbFilePath = $"Data Source={filePathElement.Value};Version=3;";
                    dbConnection = new SQLiteConnection(dbFilePath);
                    dbConnection.Open();
                    dbStatus = "DB Open";
                }
                else
                {
                    if (dbConnection.State != System.Data.ConnectionState.Open)
                    {
                        dbFilePath = $"Data Source={filePathElement.Value};Version=3;";
                        dbConnection = new SQLiteConnection(dbFilePath);
                        dbConnection.Open();
                        dbStatus = "DB Open";
                    }
                    else
                    {
                        dbStatus = "DB Already open";
                        DbConnection = dbConnection;
                    }
                }
                StringBuilder stringBuilder = new StringBuilder();
                DbConnection = dbConnection;
                response = new SocketMessageStructure
                {
                    Command = command.Substring(1, command.Length - 2),
                    SendingTime = DateTime.Now,
                    BufferDati = new XElement("BufferDati",
                    new XElement("DATABASE",
                    new XElement("ConnectionString", dbFilePath),
                    new XElement("STATUS", dbStatus),
                    new XElement("MIUParameterConfigurator",stringBuilder.ToString()))),
                    Token = asl.TokenSocket.ToString(),
                    CRC = 0
                };
                string telegramGenerate = SocketMessageSerializer.SerializeUTF8(response);
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                //string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
                string txtSendData = SocketMessageSerializer.Base64Start + Convert.ToBase64String(bytes, 0, bytes.Length) + SocketMessageSerializer.Base64End;
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
        /// funzione che chiude il database in uso e iun futuro sospenda la procedura di derivazione delle stringhe MIU
        /// </summary>
        /// <param name="DvCmd"></param>
        /// <param name="Param"></param>
        /// <param name="asl"></param>
        public void CmdCloseDB(SocketCommand DvCmd, XElement Param, AsyncSocketListener asl, SQLiteConnection dbConnection)
        {
            string sendMessage = "";
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                string command = checkCommand(DvCmd.CmdOpenDB, DvCmd);
                if (Param == null)
                {
                    throw new Exception($"Comando CmdSaveDb ricevuto da {IPAddress.Parse(((IPEndPoint)asl.Handler.RemoteEndPoint).Address.ToString())} ma BufferDati mancante.");
                }
                if (dbConnection == null)
                {
                    response = new SocketMessageStructure
                    {
                        SendingTime = DateTime.Now,
                        BufferDati = new XElement("BufferDati",
                            new XElement("DATABASE",
                            new XElement("ConnectionString"),
                            new XElement("STATUS", "Non aperto"))),
                        Token = asl.TokenSocket.ToString(),
                        CRC = 0
                    };
                }
                else
                {
                    if (dbConnection.State == System.Data.ConnectionState.Closed)
                    {
                        string s = dbConnection.ConnectionString;
                        response = new SocketMessageStructure
                        {
                            SendingTime = DateTime.Now,
                            BufferDati = new XElement("BufferDati",
                            new XElement("DATABASE",
                            new XElement("ConnectionString", dbConnection.ConnectionString),
                            new XElement("STATUS", "DB già chiuso"))),
                            Token = asl.TokenSocket.ToString(),
                            CRC = 0
                        };
                    }
                    else
                    {
                        dbConnection.Close();
                        string s = dbConnection.ConnectionString;
                        response = new SocketMessageStructure
                        {
                            SendingTime = DateTime.Now,
                            BufferDati = new XElement("BufferDati",
                            new XElement("DATABASE",
                            new XElement("ConnectionString", dbConnection.ConnectionString),
                            new XElement("STATUS", "chiuso"))),
                            Token = asl.TokenSocket.ToString(),
                            CRC = 0
                        };
                    }
                }
                string telegramGenerate = SocketMessageSerializer.SerializeUTF8(response);
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                //string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
                string txtSendData = SocketMessageSerializer.Base64Start + Convert.ToBase64String(bytes, 0, bytes.Length) + SocketMessageSerializer.Base64End;
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
        /// Funzione di acquisizione dei parametri di configurazione dl sistema MIU dalla tabella MIUParameterConfigurator
        /// </summary>
        /// <param name="DvCmd"></param>
        /// <param name="Param"></param>
        /// <param name="asl"></param>
        public void CmdConfig(SocketCommand DvCmd, XElement Param, AsyncSocketListener asl, MIU.Core.IMIURepository miuRepositoryInstance, out Dictionary<string, string> configuration)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            Dictionary<string, string> configParam;
            try
            {
                
                string command = checkCommand(DvCmd.CmdConfig, DvCmd);

                if (Param == null)
                {
                    throw new Exception($"Comando CmdSaveDb ricevuto da {IPAddress.Parse(((IPEndPoint)asl.Handler.RemoteEndPoint).Address.ToString())} ma BufferDati mancante.");
                }
                configParam = miuRepositoryInstance.LoadMIUParameterConfigurator();
                configuration = configParam;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in configParam)
                {
                    _loger.Log(LogLevel.DEBUG, item.Value);
                    stringBuilder.Append(string.Format($"[{item.Key}, {item.Value}]"));
                }
                response = new SocketMessageStructure
                {
                    Command = command.Substring(1, command.Length - 2),
                    SendingTime = DateTime.Now,
                    BufferDati = new XElement("BufferDati",
                    new XElement("CONFIG", stringBuilder.ToString())),
                    Token = asl.TokenSocket.ToString(),
                    CRC = 0
                };
                string telegramGenerate = SocketMessageSerializer.SerializeUTF8(response);
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                //string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
                string txtSendData = SocketMessageSerializer.Base64Start + Convert.ToBase64String(bytes, 0, bytes.Length) + SocketMessageSerializer.Base64End;
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
        /// funzione di salvataggio dei parametri di funzionamento del sistema MIU
        /// </summary>
        /// <param name="DvCmd"></param>
        /// <param name="Param"></param>
        /// <param name="asl"></param>
        /// <param name="miuRepositoryInstance"></param>
        /// <param name="ConfigParam"></param>
        /// <exception cref="Exception"></exception>
        public void CmdSaveConfig(SocketCommand DvCmd, XElement Param, AsyncSocketListener asl, MIU.Core.IMIURepository miuRepositoryInstance, Dictionary<string,string> ConfigParam)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                string command = checkCommand(DvCmd.CmdConfig, DvCmd);
                if (Param == null)
                {
                    throw new Exception($"Comando CmdSaveDb ricevuto da {IPAddress.Parse(((IPEndPoint)asl.Handler.RemoteEndPoint).Address.ToString())} ma BufferDati mancante.");
                }
                else
                {

                }
                if (ConfigParam.Count() > 0)
                {
                    miuRepositoryInstance.SaveMIUParameterConfigurator(ConfigParam);
                }
                else
                {
                    throw new Exception($"Lista di configurazione vuota");
                }
                response = new SocketMessageStructure
                {
                    Command = command.Substring(1, command.Length - 2),
                    SendingTime = DateTime.Now,
                    BufferDati = new XElement("BufferDati",
                    new XElement("CONFIG", "PARAMETRI SALVATI")),
                    Token = asl.TokenSocket.ToString(),
                    CRC = 0
                };
                string telegramGenerate = SocketMessageSerializer.SerializeUTF8(response);
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                //string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
                string txtSendData = SocketMessageSerializer.Base64Start + Convert.ToBase64String(bytes, 0, bytes.Length) + SocketMessageSerializer.Base64End;
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
                    throw new Exception($"Comando CmdSaveDb ricevuto da {IPAddress.Parse(((IPEndPoint)asl.Handler.RemoteEndPoint).Address.ToString())} ma BufferDati mancante.");
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
                /*2025.05.16 adesso il percorso del file lo vado a prendere dalla classe Database campo dalla classe FilePath
                string saveFilePath = filePathElement.Value;
                */
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
                string saveFilePath = databaseToSave.FilePath;

                if (string.IsNullOrWhiteSpace(saveFilePath))
                {
                    Console.WriteLine($"Comando CmdSaveDb ricevuto da {asl.CallerIpAddress} per database '{databaseToSave.DatabaseName}', ma percorso file non memorizzato.");
                    // _logger.LogWarning($"Comando CmdSaveDb: Percorso file non memorizzato per database '{databaseToSave.DatabaseName}'.");
                    // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                    throw new Exception($"Comando CmdSaveDb ricevuto da {asl.CallerIpAddress} per database '{databaseToSave.DatabaseName}', ma percorso file non memorizzato.database. Caricare prima il database da file.");
                }

                _loger.Log(LogLevel.INFO, $"Salvataggio database '{databaseToSave.DatabaseName}' in corso su: {saveFilePath}");
                DatabaseSerializer.SerializeToXmlFile(databaseToSave, saveFilePath);
                _loger.Log(LogLevel.INFO, $"Database '{databaseToSave.DatabaseName}' salvato con successo.");

                if (string.IsNullOrWhiteSpace(saveFilePath))
                {
                    // _logger.LogWarning($"Comando CmdSaveDb: Percorso file salvataggio mancante.");
                    // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                    throw new Exception($"Comando CmdSaveDb ricevuto da {asl.CallerIpAddress} ma percorso file salvataggio mancante.");
                }

                DatabaseSerializer.SerializeToXmlFile(databaseToSave, saveFilePath);

                response.BufferDati = new XElement("SyncDetails",
                                        new XElement("ServerTime", DateTime.UtcNow.ToString("o")), // Orario del server in formato ISO 8601
                                        new XElement("Database", $"Db {databaseToSave.DatabaseName}caricato"),
                                        new XElement("Status", "OK")); // Esempio di stato
                string telegramGenerate = SocketMessageSerializer.SerializeUTF8(response);
                _loger.Log(LogLevel.DEBUG, telegramGenerate);
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
                //string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
                string txtSendData = SocketMessageSerializer.Base64Start + Convert.ToBase64String(bytes, 0, bytes.Length) + SocketMessageSerializer.Base64End;
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
        /// Handler di gestione del comando richiesta sato del database 2025.05.16
        /// </summary>
        /// <param name="DvCmd"></param>
        /// <param name="Param"></param>
        /// <param name="asl"></param>
        public void CmdStructDb(SocketCommand DvCmd,XElement Param, AsyncSocketListener asl)
        {
            MethodBase thisMethod = MethodBase.GetCurrentMethod();
            try
            {
                string command = checkCommand(DvCmd.CmdStructDb, DvCmd);
                response = new SocketMessageStructure
                {
                    Command = command.Substring(1, command.Length - 2),
                    SendingTime = DateTime.Now,
                    BufferDati = new XElement("BufferDati"),
                    Token = asl.TokenSocket.ToString(),
                    CRC = 0
                };
                if (Param == null)
                {
                    throw new Exception($"Comando CmdSaveDb ricevuto da {IPAddress.Parse(((IPEndPoint)asl.Handler.RemoteEndPoint).Address.ToString())} ma BufferDati mancante.");
                }
                XElement dbIdentifierElement = Param.Element("DatabaseIdentifier");
                XElement requestDetailsElement = Param.Element("RequestDetails"); // Opzionale

                string identifierType = dbIdentifierElement?.Attribute("Type")?.Value;
                string identifierValue = dbIdentifierElement?.Value;
                string requestDetails = requestDetailsElement?.Value ?? "Full"; // Default a "Full" se non specificato

                // --- Gestione della richiesta "ListOnly" ---
                // Se non viene fornito un identificatore specifico E la richiesta è "ListOnly",
                // restituiamo una lista di tutti i database caricati.
                if (dbIdentifierElement == null && requestDetails.Equals("ListOnly", StringComparison.OrdinalIgnoreCase))
                {
                    _loger.Log (LogLevel.INFO, $"Comando CmdRequestDbState ricevuto da {asl.CallerIpAddress}. Richiesta lista database caricati.");
                    // *** Usa il metodo helper normalizzato per creare la risposta della lista ***
                    response.BufferDati = new XElement("BufferDati", CreateDatabaseListResponse(_loadedDatabases));
                }
                // Se la richiesta non è "ListOnly" ma manca l'identificatore, è un errore.
                else if (dbIdentifierElement == null && !requestDetails.Equals("ListOnly", StringComparison.OrdinalIgnoreCase))
                {
                    // _logger.LogWarning($"Comando CmdRequestDbState: Identificatore database mancante per richiesta dettagliata.");
                    // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                    throw new Exception($"Comando CmdRequestDbState ricevuto da {asl.CallerIpAddress} ma Identificatore database mancante per richiesta dettagliata. ");
                }
                Database databaseToReport = null;
                if (identifierType?.Equals("Id", StringComparison.OrdinalIgnoreCase) == true && int.TryParse(identifierValue, out int dbId))
                {
                    databaseToReport = _loadedDatabases.FirstOrDefault(db => db.DatabaseId == dbId);
                }
                else if (identifierType?.Equals("Name", StringComparison.OrdinalIgnoreCase) == true && !string.IsNullOrWhiteSpace(identifierValue))
                {
                    databaseToReport = _loadedDatabases.FirstOrDefault(db => db.DatabaseName.Equals(identifierValue, StringComparison.OrdinalIgnoreCase));
                }
                else if (!requestDetails.Equals("ListOnly", StringComparison.OrdinalIgnoreCase))
                {
                    // _logger.LogWarning($"Comando CmdRequestDbState: Identificatore database non valido (Type='{identifierType}', Value='{identifierValue}').");
                    // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                    throw new Exception($"Comando CmdRequestDbState ricevuto da {asl.CallerIpAddress}. Identificatore database non valido o tipo sconosciuto Type='{identifierType}', Value='{identifierValue}'");
                }
                if (databaseToReport == null && (!requestDetails.Equals("ListOnly", StringComparison.OrdinalIgnoreCase)))
                {
                    // _logger.LogWarning($"Comando CmdRequestDbState: Database con identificatore '{identifierValue}' non trovato.");
                    // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                    throw new Exception ($"Comando CmdRequestDbState ricevuto da {asl.CallerIpAddress}.Database con identificatore '{identifierValue}' non trovato sul server.");
                }
                XElement responseBufferContent = null; // Contenuto per il BufferDati della risposta
                if (requestDetails.Equals("Full", StringComparison.OrdinalIgnoreCase) || requestDetails.Equals("StructureOnly", StringComparison.OrdinalIgnoreCase))
                {
                    Database dbToSerialize = CloneDatabaseForSerialization(databaseToReport, requestDetails.Equals("StructureOnly", StringComparison.OrdinalIgnoreCase));
                    string dbXmlContent = DatabaseSerializer.SerializeToXmlString(dbToSerialize);
                    try
                    {
                        // Il risultato della serializzazione di un Database è un elemento <Database>
                        XElement dbContentElement = XElement.Parse(dbXmlContent);
                        // Creiamo un wrapper per il BufferDati della risposta
                        responseBufferContent = new XElement("DatabaseStateDetails", dbContentElement);

                        //response.BufferDati.Add(CreateDatabaseListResponse(_loadedDatabases));
                        response.BufferDati.Add(responseBufferContent);
                    }
                    catch (Exception parseEx)
                    {
                        // _logger.LogError("Errore nel parsing XML del database serializzato per risposta:", parseEx);
                        // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                        throw new Exception ($"Errore interno nella serializzazione del database per la risposta: {parseEx.Message}");
                    }
                }
                else if (!requestDetails.Equals("ListOnly", StringComparison.OrdinalIgnoreCase))
                {
                    // Tipo di richiesta dettagliata non supportato
                    // _logger.LogWarning($"Comando CmdRequestDbState: Tipo di richiesta dettagliata '{requestDetails}' non supportato.");
                    // *** Usa il metodo helper normalizzato per creare la risposta di errore ***
                    throw new Exception($"Comando CmdRequestDbState ricevuto da {asl.CallerIpAddress}. Tipo di richiesta dettagliata '{requestDetails}' non supportato per CmdRequestDbState.");
                }
            }
            catch (Exception ex)
            {
                // Gestione generica degli errori
                throw new Exception ($"Errore durante la gestione del comando CmdRequestDbState: {ex.Message}");
                // _logger.LogError($"Errore durante la gestione del comando CmdRequestDbState:", ex);
            }
            string telegramGenerate = SocketMessageSerializer.SerializeUTF8(response);
            _loger.Log(LogLevel.DEBUG, telegramGenerate);
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bytes = Encoding.UTF8.GetBytes(telegramGenerate);
            //string txtSendData = "<SocketMessageStructure>" + Convert.ToBase64String(bytes, 0, bytes.Length) + "</SocketMessageStructure>";
            string txtSendData = SocketMessageSerializer.Base64Start + Convert.ToBase64String(bytes, 0, bytes.Length) + SocketMessageSerializer.Base64End;
            asl.Send(asl.Handler, txtSendData);
        }
        #region private methods
        private Database CloneDatabaseForSerialization(Database originalDb, bool structureOnly)
        {
            if (originalDb == null) return null;

            // Serializza e deserializza per creare un clone "profondo" e rimuovere i riferimenti Parent
            // Questo metodo sfrutta il DatabaseSerializer per creare una copia disconnessa.
            // È un modo semplice per clonare, ma potrebbe non essere il più performante per oggetti molto grandi.
            try
            {
                // Serializza l'originale in una stringa XML
                string originalXml = DatabaseSerializer.SerializeToXmlString(originalDb);

                // Deserializza la stringa XML in un nuovo oggetto Database
                Database clonedDb = DatabaseSerializer.DeserializeFromXmlString(originalXml);

                // Se richiesto solo la struttura, rimuovi i DataRecords dal clone
                if (structureOnly && clonedDb != null && clonedDb.Tables != null)
                {
                    foreach (var table in clonedDb.Tables)
                    {
                        // Sostituisci la lista DataRecords con una nuova lista vuota
                        table.DataRecords = new System.Collections.Generic.List<SerializableDictionary<string, object>>();  // <- errore cs1061
                    }
                }

                return clonedDb;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la clonazione del database per serializzazione: {ex.Message}");
                // _logger.LogError("Errore durante la clonazione del database per serializzazione:", ex);
                // Potresti voler lanciare di nuovo l'eccezione o restituire null a seconda della gestione degli errori desiderata.
                throw new InvalidOperationException($"Impossibile clonare il database per serializzazione: {ex.Message}", ex);
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
        /// <summary>
        /// Crea un messaggio di risposta contenente la lista dei database caricati.
        /// Questo è un helper specifico per la risposta DbListResponse.
        /// </summary>
        /// <param name="originalToken">Il token del messaggio originale.</param>
        /// <param name="databaseList">La lista dei database caricati.</param>
        /// <returns>Un messaggio di risposta con la lista dei database.</returns>
        private XElement CreateDatabaseListResponse(System.Collections.Generic.List<Database> databaseList)
        {
            // Prepara il contenuto specifico per il BufferDati della risposta
            XElement dbListElement = new XElement("LoadedDatabases");
            foreach (var db in databaseList)
            {
                dbListElement.Add(new XElement("DatabaseInfo",
                                    new XElement("DatabaseId", db.DatabaseId),
                                    new XElement("DatabaseName", db.DatabaseName)));
            }

            // *** Usa il metodo helper normalizzato per creare la risposta di successo ***
            return dbListElement;
        }
        #endregion
    }
}
