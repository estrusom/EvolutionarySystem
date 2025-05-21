using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics; // Necessario per EventLog
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net; // Necessario per IPAddress, IPEndPoint
using System.Net.Sockets; // Necessario per Socket, SocketType, ProtocolType, SocketFlags
using System.Xml.Linq; // Necessario per lavorare con XElement/XDocument
using System.Collections.Concurrent; // Necessario per ConcurrentQueue, ConcurrentDictionary
using System.IO; // Necessario per File, Path

// Assicurati che questi namespace siano corretti e che i progetti siano referenziati
using EvolutiveSystem.SemanticData; // Contiene Database, Table, Field, SerializableDictionary
using EvolutiveSystem.Serialization; // Contiene DatabaseSerializer
using EvolutiveSystem.Communication;
using SocketManagerInfo; // Necessario per SocketMessageStructure e SocketMessageSerializer

// Aggiungi un namespace per la logica del tuo motore semantico (dovrai crearlo)
// using EvolutiveSystem.SemanticEngine;


namespace EvolutiveSystem.SemanticProcessor
{
    public partial class SemanticProcessorService : ServiceBase
    {
        // Componente EventLog per scrivere nel registro eventi di Windows
        private EventLog eventLog1;

        // CancellationTokenSource per gestire l'annullamento del duty cycle
        private CancellationTokenSource _cancellationTokenSource;

        // Task per il duty cycle del motore semantico
        private Task _semanticEngineTask;

        // Placeholder per l'istanza del motore semantico
        // private SemanticEngine _semanticEngine;

        // Coda thread-safe per i messaggi ricevuti dal socket server
        // Ogni elemento della coda è una Tuple contenente il messaggio deserializzato
        // e il socket del client che lo ha inviato.
        private ConcurrentQueue<Tuple<SocketMessageStructure, Socket>> _receivedMessageQueue;


        // --- Configurazione del Socket Server ---
        private SimpleSocketServer _socketServer;
        private const int SERVER_PORT = 12345;       // Porta su cui il server ascolta
        // I delimitatori sono ora gestiti centralmente nella classe SocketMessageSerializer.
        // private const string MSG_START_DELIMITER = "<MSG_START>"; // Delimitatore di inizio messaggio
        // private const string MSG_END_DELIMITER = "<MSG_END>";     // Delimitatore di fine messaggio

        // --- Gestione dei Database Semantici (In Memoria) ---
        // Questa lista rappresenta i database caricati e gestiti attivamente dal servizio.
        private List<Database> _loadedDatabases;

        // --- Gestione degli Endpoint UI Connessi (per notifiche Server -> UI - Connect-on-Demand) ---
        // La chiave è l'endpoint IP:Porta della UI (es. "192.168.1.100:12346"), il valore è la stessa stringa dell'endpoint.
        // Questo è il tuo 'concurrentData' discusso.
        private readonly ConcurrentDictionary<string, string> _connectedUiEndpoints;

        // --- Istanza del gestore comandi ---
        private CommandHandlers _commandHandlers;

        // --- Configurazione per il monitoraggio dei file di notifica ---
        // Il server cercherà un file XML di notifica in questo percorso.
        // Il contenuto del file sarà il BufferDati della notifica inviata alle UI.
        private const string NOTIFICATION_FILE_PATH = "C:\\Temp\\notification.xml"; // Percorso del file di notifica


        public SemanticProcessorService()
        {
            InitializeComponent(); // Metodo generato automaticamente dal designer

            // Inizializza EventLog
            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("SemanticProcessorSource"))
            {
                EventLog.CreateEventSource("SemanticProcessorSource", "SemanticProcessorLog");
            }
            eventLog1.Source = "SemanticProcessorSource";
            eventLog1.Log = "SemanticProcessorLog";

            // Inizializza la coda dei messaggi ricevuti
            _receivedMessageQueue = new ConcurrentQueue<Tuple<SocketMessageStructure, Socket>>();

            // Inizializza la lista dei database caricati
            _loadedDatabases = new List<Database>();

            // Inizializza la dictionary per gli endpoint UI
            _connectedUiEndpoints = new ConcurrentDictionary<string, string>();

            // Inizializza il gestore comandi, passandogli la lista dei database caricati e la dictionary degli endpoint UI
            _commandHandlers = new CommandHandlers(_loadedDatabases, _connectedUiEndpoints);
        }

        /// <summary>
        /// Metodo chiamato all'avvio del servizio.
        /// </summary>
        /// <param name="args">Argomenti passati all'avvio del servizio.</param>
        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Semantic Processor Service - Avvio in corso.");

            // Inizializza e avvia il Socket Server
            try
            {
                // Usa i delimitatori centralizzati da SocketMessageSerializer
                _socketServer = new SimpleSocketServer(SERVER_PORT, SocketMessageSerializer.Base64StartDelimiter, SocketMessageSerializer.Base64EndDelimiter);
                _socketServer.MessageReceived += SocketServer_MessageReceived; // Sottoscrivi all'evento di ricezione messaggi
                _socketServer.ClientConnected += SocketServer_ClientConnected; // Sottoscrivi all'evento di connessione client
                _socketServer.ClientDisconnected += SocketServer_ClientDisconnected; // Sottoscrivi all'evento di disconnessione client
                _socketServer.ServerError += SocketServer_ServerError; // Sottoscrivi all'evento di errore del server

                _socketServer.Start();
                eventLog1.WriteEntry($"Socket Server avviato sulla porta {SERVER_PORT}. In attesa di connessioni.");
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry($"Errore critico nell'avvio del Socket Server: {ex.Message}", EventLogEntryType.Error);
                // Considera di fermare il servizio se il server non può avviarsi
                this.ExitCode = 1066; // Codice di errore generico per servizio specifico
                this.Stop();
                return; // Esci da OnStart
            }


            // Inizializza il CancellationTokenSource per il duty cycle
            _cancellationTokenSource = new CancellationTokenSource();

            // Avvia il Task per il duty cycle del motore semantico
            // Questo task girerà in background per processare i messaggi dalla coda
            _semanticEngineTask = Task.Run(() => SemanticEngineDutyCycle(_cancellationTokenSource.Token));

            eventLog1.WriteEntry("Semantic Processor Service - Avviato con successo.");
        }

        /// <summary>
        /// Metodo chiamato all'arresto del servizio.
        /// </summary>
        protected override void OnStop()
        {
            eventLog1.WriteEntry("Semantic Processor Service - Arresto in corso.");

            // Segnala l'annullamento al duty cycle
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }

            // Attendi che il task del duty cycle termini (con un timeout opzionale)
            if (_semanticEngineTask != null)
            {
                try
                {
                    _semanticEngineTask.Wait(TimeSpan.FromSeconds(10)); // Attendi al massimo 10 secondi
                }
                catch (AggregateException ae)
                {
                    // Gestisci eccezioni dal task (es. OperationCanceledException è prevista)
                    foreach (var ex in ae.Flatten().InnerExceptions)
                    {
                        if (!(ex is OperationCanceledException))
                        {
                            eventLog1.WriteEntry($"Eccezione nel task del duty cycle durante l'arresto: {ex.Message}", EventLogEntryType.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    eventLog1.WriteEntry($"Errore durante l'attesa del task del duty cycle: {ex.Message}", EventLogEntryType.Warning);
                }
            }

            // Ferma il Socket Server
            if (_socketServer != null)
            {
                _socketServer.Stop();
                eventLog1.WriteEntry("Socket Server fermato.");
            }


            // TODO: Salva lo stato corrente dei database se necessario
            // Potresti voler salvare tutti i database caricati su disco qui.
            // Esempio concettuale:
            // foreach (var db in _loadedDatabases)
            // {
            //     try
            //     {
            //         string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{db.DatabaseName}_shutdown_backup.xml");
            //         DatabaseSerializer.SerializeToXmlFile(db, savePath);
            //         eventLog1.WriteEntry($"Database '{db.DatabaseName}' salvato come backup all'arresto.");
            //     }
            //     catch (Exception ex)
            //     {
            //         eventLog1.WriteEntry($"Errore nel salvataggio di backup del database '{db.DatabaseName}': {ex.Message}", EventLogEntryType.Warning);
            //     }
            // }
            // _loadedDatabases.Clear(); // Pulisci la lista in memoria


            eventLog1.WriteEntry("Semantic Processor Service - Fermato con successo.");
        }

        /// <summary>
        /// Metodo chiamato quando il servizio viene messo in pausa.
        /// </summary>
        protected override void OnPause()
        {
            eventLog1.WriteEntry("Semantic Processor Service - Messa in pausa.");
            // TODO: Implementa la logica per mettere in pausa il duty cycle o altre operazioni.
            // Potresti voler sospendere l'elaborazione dei messaggi dalla coda.
            // NOTA: La coda ConcurrentQueue non ha un metodo Pause/Resume diretto.
            // Dovresti implementare una logica di pausa all'interno del duty cycle
            // che controlla un flag di stato (es. _isPaused) prima di processare un messaggio.
        }

        /// <summary>
        /// Metodo chiamato quando il servizio riprende dalla pausa.
        /// </summary>
        protected override void OnContinue()
        {
            eventLog1.WriteEntry("Semantic Processor Service - Ripresa.");
            // TODO: Implementa la logica per riprendere il duty cycle o altre operazioni.
            // Resetta il flag di stato _isPaused.
        }

        /// <summary>
        /// Metodo chiamato quando il servizio riceve un comando personalizzato.
        /// </summary>
        /// <param name="command">Il comando personalizzato.</param>
        protected override void OnCustomCommand(int command)
        {
            // TODO: Implementa la gestione di comandi personalizzati.
            // Esempio: Potresti definire comandi per ricaricare configurazioni,
            // forzare un ciclo di evoluzione, ecc.
            // eventLog1.WriteEntry($"Semantic Processor Service - Ricevuto comando personalizzato: {command}");
        }


        /// <summary>
        /// Il duty cycle principale del motore semantico.
        /// Questo task gira in background e processa i messaggi dalla coda.
        /// </summary>
        /// <param name="cancellationToken">Token per segnalare l'annullamento.</param>
        private async Task SemanticEngineDutyCycle(CancellationToken cancellationToken)
        {
            eventLog1.WriteEntry("Semantic Engine Duty Cycle - Avviato.");

            // TODO: Inizializza il motore semantico qui se non l'hai fatto in OnStart
            // _semanticEngine = new SemanticEngine();
            // _semanticEngine.OnProcessUpdate += SemanticEngine_OnProcessUpdate; // Sottoscrivi agli eventi di aggiornamento
            // _semanticEngine.OnPhaseChange += SemanticEngine_OnPhaseChange; // Sottoscrivi agli eventi di cambio fase


            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Processa i messaggi ricevuti dai client
                    while (_receivedMessageQueue.TryDequeue(out var receivedItem))
                    {
                        SocketMessageStructure receivedMessage = receivedItem.Item1;
                        Socket clientSocket = receivedItem.Item2; // Ottieni il socket del client

                        eventLog1.WriteEntry($"Duty Cycle: Processando messaggio '{receivedMessage.Command}' dal client {clientSocket.RemoteEndPoint}.");

                        // Dispatch il comando al gestore appropriato e ottieni la risposta
                        SocketMessageStructure responseMessage = _commandHandlers.DispatchCommandAndGetResponse(receivedMessage, clientSocket);

                        // Se è stata generata una risposta, inviala al client
                        if (responseMessage != null)
                        {
                            SendSocketResponse(clientSocket, responseMessage);
                        }
                    }

                    // 2. *** Logica per il monitoraggio e l'invio di notifiche da file ***
                    // Controlla se il file di notifica esiste
                    if (File.Exists(NOTIFICATION_FILE_PATH))
                    {
                        eventLog1.WriteEntry($"Duty Cycle: Trovato file di notifica: {NOTIFICATION_FILE_PATH}.");
                        try
                        {
                            // Leggi il contenuto del file
                            string fileContent = await File.ReadAllTextAsync(NOTIFICATION_FILE_PATH, Encoding.UTF8, cancellationToken);
                            eventLog1.WriteEntry($"Duty Cycle: Contenuto del file di notifica letto ({fileContent.Length} caratteri).");

                            // Elimina il file dopo la lettura
                            File.Delete(NOTIFICATION_FILE_PATH);
                            eventLog1.WriteEntry($"Duty Cycle: File di notifica '{NOTIFICATION_FILE_PATH}' eliminato.");

                            // Crea un messaggio SocketMessageStructure dalla stringa letta
                            // Assumiamo che il contenuto del file sia un XML valido per il BufferDati.
                            // Potresti voler aggiungere un parsing più robusto qui.
                            XElement notificationBufferDati = XElement.Parse(fileContent);

                            SocketMessageStructure fileNotification = new SocketMessageStructure
                            {
                                Command = null, // Non è un comando UI -> Server
                                MessageType = "FileNotification", // Tipo di notifica personalizzato
                                SendingTime = DateTime.UtcNow,
                                BufferDati = notificationBufferDati,
                                Token = Guid.NewGuid().ToString(), // Nuovo token per questa notifica
                                CRC = 0
                            };

                            // Invia la notifica a tutte le UI registrate
                            await SendNotificationToUi(fileNotification);
                        }
                        catch (IOException ex)
                        {
                            // Gestisci errori di accesso al file (es. file in uso da un altro processo)
                            eventLog1.WriteEntry($"Errore di I/O durante la gestione del file di notifica '{NOTIFICATION_FILE_PATH}': {ex.Message}", EventLogEntryType.Warning);
                        }
                        catch (System.Xml.XmlException ex)
                        {
                            // Gestisci errori se il contenuto del file non è XML valido
                            eventLog1.WriteEntry($"Errore XML nel contenuto del file di notifica '{NOTIFICATION_FILE_PATH}': {ex.Message}", EventLogEntryType.Warning);
                        }
                        catch (Exception ex)
                        {
                            eventLog1.WriteEntry($"Errore generico durante la gestione del file di notifica '{NOTIFICATION_FILE_PATH}': {ex.Message}", EventLogEntryType.Error);
                        }
                    }


                    // TODO: Esegui il ciclo di evoluzione semantica qui (se non è basato su messaggi)
                    // Esempio:
                    // if (_semanticEngine != null && _semanticEngine.IsEvolutionActive && !_isPaused) // Assumendo flag IsEvolutionActive e _isPaused
                    // {
                    //     _semanticEngine.PerformEvolutionStep(); // Esegui un passo dell'evoluzione
                    // }


                    // Aggiungi un piccolo ritardo per evitare un loop troppo stretto
                    await Task.Delay(100, cancellationToken); // Ritardo di 100ms, rispettando il token di annullamento
                }
                catch (OperationCanceledException)
                {
                    // Questa eccezione è prevista quando il servizio viene fermato
                    eventLog1.WriteEntry("Semantic Engine Duty Cycle - Annullato.");
                    break; // Esci dal loop
                }
                catch (Exception ex)
                {
                    // Gestisci altre eccezioni non gestite nel duty cycle
                    eventLog1.WriteEntry($"Errore non gestito nel Semantic Engine Duty Cycle: {ex.Message}", EventLogEntryType.Error);
                    // Potresti voler aggiungere un ritardo maggiore qui per evitare loop di errore rapidi
                    await Task.Delay(1000, cancellationToken);
                }
            }

            eventLog1.WriteEntry("Semantic Engine Duty Cycle - Terminata esecuzione.");
        }


        /// <summary>
        /// Gestore evento per i messaggi ricevuti dal SimpleSocketServer.
        /// Questo metodo viene chiamato dal thread di ricezione del server.
        /// </summary>
        private void SocketServer_MessageReceived(object sender, Communication.MessageReceivedEventArgs e)
        {
            // *** Questo metodo viene chiamato dal thread di ricezione del server! ***
            // Non eseguire qui operazioni lunghe o bloccanti.
            // Accoda il messaggio per essere processato dal duty cycle.

            string rawMessage = e.ReceivedString; // La stringa raw ricevuta (Base64 con delimitatori)
            Socket clientSocket = e.ClientSocket; // Il socket del client che ha inviato il messaggio

            eventLog1.WriteEntry($"Socket Server: Ricevuto messaggio raw da {clientSocket.RemoteEndPoint} ({rawMessage.Length} bytes).");

            try
            {
                // 1. Rimuovi i delimitatori dalla stringa raw
                string messageWithoutDelimiters = rawMessage.Replace(SocketMessageSerializer.Base64StartDelimiter, "").Replace(SocketMessageSerializer.Base64EndDelimiter, "");

                // 2. Decodifica la stringa Base64
                byte[] xmlBytesDecoded = Convert.FromBase64String(messageWithoutDelimiters);
                string receivedXml = Encoding.UTF8.GetString(xmlBytesDecoded);

                //eventLog1.WriteEntry($"Socket Server: Decodificato XML:\n{receivedXml}"); // Evita di loggare XML molto grandi

                // 3. Deserializza l'XML in un oggetto SocketMessageStructure
                SocketMessageStructure receivedMessage = SocketMessageSerializer.Deserialize(receivedXml);

                eventLog1.WriteEntry($"Socket Server: Deserializzato messaggio: Comando='{receivedMessage.Command}', Token='{receivedMessage.Token}' da {clientSocket.RemoteEndPoint}.");

                // 4. Accoda il messaggio e il socket del client per il processamento nel duty cycle
                _receivedMessageQueue.Enqueue(new Tuple<SocketMessageStructure, Socket>(receivedMessage, clientSocket));

                eventLog1.WriteEntry($"Socket Server: Messaggio '{receivedMessage.Command}' accodato per il duty cycle.");

            }
            catch (FormatException ex)
            {
                // Gestisce errori se la stringa ricevuta non è un Base64 valido
                eventLog1.WriteEntry($"Socket Server Error: Errore di formato Base64 nel messaggio ricevuto da {clientSocket.RemoteEndPoint}: {ex.Message}", EventLogEntryType.Warning);
                // Potresti voler inviare un messaggio di errore al client qui
                SendErrorResponse(clientSocket, "Errore di formato nel messaggio ricevuto.");
            }
            catch (System.Xml.XmlException ex)
            {
                // Gestisce errori se l'XML decodificato non è valido
                eventLog1.WriteEntry($"Socket Server Error: Errore XML durante la deserializzazione messaggio da {clientSocket.RemoteEndPoint}: {ex.Message}", EventLogEntryType.Warning);
                // Potresti voler inviare un messaggio di errore al client qui
                SendErrorResponse(clientSocket, "Errore XML nel messaggio ricevuto.");
            }
            catch (Exception ex)
            {
                // Gestione generica degli errori nella ricezione/decodifica/deserializzazione
                eventLog1.WriteEntry($"Socket Server Error: Errore generico durante l'elaborazione messaggio ricevuto da {clientSocket.RemoteEndPoint}: {ex.Message}", EventLogEntryType.Error);
                // Potresti voler inviare un messaggio di errore al client qui
                SendErrorResponse(clientSocket, "Errore interno durante l'elaborazione del messaggio.");
            }
        }

        /// <summary>
        /// Gestore evento per le nuove connessioni client.
        /// </summary>
        private void SocketServer_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            eventLog1.WriteEntry($"Socket Server: Client connesso da {e.ClientSocket.RemoteEndPoint}.");
            // TODO: Potresti voler tenere traccia dei client connessi
        }

        /// <summary>
        /// Gestore evento per le disconnessioni client.
        /// </summary>
        private void SocketServer_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            eventLog1.WriteEntry($"Socket Server: Client disconnesso da {e.ClientSocket.RemoteEndPoint}.");
            // TODO: Rimuovere il client dalla lista dei client connessi se ne tieni traccia
        }

        /// <summary>
        /// Gestore evento per gli errori interni del Socket Server.
        /// </summary>
        private void SocketServer_ServerError(object sender, ServerErrorEventArgs e)
        {
            eventLog1.WriteEntry($"Socket Server Error: {e.ErrorMessage}", EventLogEntryType.Error);
            // TODO: Gestisci errori critici del server, potresti voler fermare il servizio o riavviare il server
        }


        /// <summary>
        /// Dispatch il comando ricevuto al gestore appropriato e restituisce la risposta.
        /// Questo metodo viene chiamato dal duty cycle.
        /// </summary>
        /// <param name="receivedMessage">Il messaggio ricevuto dal client.</param>
        /// <param name="clientSocket">Il socket del client che ha inviato il messaggio.</param>
        /// <returns>Un oggetto SocketMessageStructure di risposta, o null se non è richiesta una risposta.</returns>
        private SocketMessageStructure DispatchCommandAndGetResponse(SocketMessageStructure receivedMessage, Socket clientSocket)
        {
            // Il dispatching ora è delegato all'istanza di CommandHandlers
            return _commandHandlers.DispatchCommandAndGetResponse(receivedMessage, clientSocket);
        }

        /// <summary>
        /// Metodo helper per creare un messaggio di risposta di errore.
        /// </summary>
        /// <param name="requestToken">Il token della richiesta originale.</param>
        /// <param name="errorMessage">Il messaggio di errore.</param>
        /// <returns>Un oggetto SocketMessageStructure per la risposta di errore.</returns>
        private SocketMessageStructure CreateErrorResponse(string requestToken, string errorMessage)
        {
            XElement errorDetails = new XElement("ErrorDetails", new XElement("Message", errorMessage));

            return new SocketMessageStructure
            {
                Command = "ErrorResponse", // Comando standard per le risposte di errore
                SendingTime = DateTime.UtcNow,
                Token = requestToken, // Usa lo stesso token della richiesta originale per correlazione
                Crc = 0, // Placeholder CRC
                         // Stato = "Errore", // Stato del messaggio
                         // MessaggioStato = errorMessage // Messaggio di stato dettagliato
            };
        }

        /// <summary>
        /// Metodo helper per inviare direttamente un messaggio di errore a un client.
        /// Utile per errori che si verificano prima del dispatching del comando.
        /// </summary>
        /// <param name="clientSocket">Il socket del client a cui inviare l'errore.</param>
        /// <param name="errorMessage">Il messaggio di errore.</param>
        private void SendErrorResponse(Socket clientSocket, string errorMessage)
        {
            // Crea un messaggio di errore senza un token di richiesta specifico (o usa un token generico)
            SocketMessageStructure errorResponse = CreateErrorResponse("N/A", errorMessage); // Usa "N/A" o un altro placeholder per il token

            // Invia il messaggio di errore
            SendSocketResponse(clientSocket, errorResponse);
        }


        // --- Metodi Handler per i Comandi (Chiamati dal DispatchCommandAndGetResponse) ---
        // Questi metodi sono stati spostati o sono gestiti dalla classe CommandHandlers.
        // Li abbiamo mantenuti qui come placeholder per la tua reference, ma la loro logica
        // effettiva è ora in CommandHandlers.
        // Esempio:
        // private SocketMessageStructure HandleCmdOpenDb(SocketMessageStructure request, Socket clientSocket) { /* ... */ }
        // ...


        // --- Metodi per l'invio di Risposte e Notifiche ---

        /// <summary>
        /// Invia un messaggio di risposta a un client specifico tramite il suo socket.
        /// Gestisce la serializzazione XML, la codifica Base64 e l'aggiunta dei delimitatori.
        /// Questo metodo viene chiamato dal duty cycle.
        /// </summary>
        /// <param name="clientSocket">Il socket del client a cui inviare la risposta.</param>
        /// <param name="responseMessage">Il messaggio SocketMessageStructure di risposta.</param>
        private async void SendSocketResponse(Socket clientSocket, SocketMessageStructure responseMessage)
        {
            if (clientSocket == null || !clientSocket.Connected)
            {
                eventLog1.WriteEntry("Tentativo di inviare risposta a un socket non connesso.", EventLogEntryType.Warning);
                return;
            }

            try
            {
                // 1. Serializza l'oggetto messaggio di risposta in XML
                string responseXml = SocketMessageSerializer.Serialize(responseMessage);

                // eventLog1.WriteEntry($"Serializzata risposta (XML):\n{responseXml}"); // Evita di loggare XML molto grandi

                // 2. Codifica l'XML in Base64
                byte[] xmlBytes = Encoding.UTF8.GetBytes(responseXml);
                string responseBase64 = Convert.ToBase64String(xmlBytes);

                // eventLog1.WriteEntry($"Codificata risposta (Base64): {responseBase64}"); // Evita di loggare Base64 molto grandi

                // 3. Aggiungi i delimitatori di inizio e fine messaggio
                string finalMessage = SocketMessageSerializer.Base64StartDelimiter + responseBase64 + SocketMessageSerializer.Base64EndDelimiter;

                // 4. Converti la stringa finale in byte
                byte[] data = Encoding.UTF8.GetBytes(finalMessage);

                // 5. Invia i byte tramite il socket del client in modo ASINCRONO
                //    Utilizziamo il metodo SendAsync del Socket.
                //    Questo metodo restituisce un Task che rappresenta l'operazione asincrona.
                //    Usiamo 'await' per attendere il completamento dell'invio senza bloccare il thread del duty cycle.
                //    Nota: SendAsync con solo byte[] è una forma semplificata. Per alte prestazioni,
                //    si userebbe SocketAsyncEventArgs.
                int bytesSent = await clientSocket.SendAsync(data, SocketFlags.None);


                eventLog1.WriteEntry($"Risposta '{responseMessage.Command}' ({bytesSent} bytes) inviata al client {clientSocket.RemoteEndPoint}. Token: {responseMessage.Token}", EventLogEntryType.Information);

            }
            catch (SocketException ex)
            {
                // Gestisci errori specifici del socket (es. connessione persa)
                eventLog1.WriteEntry($"Socket Error durante l'invio della risposta '{responseMessage.Command}' al client {clientSocket.RemoteEndPoint}: {ex.Message} (Errore Code: {ex.ErrorCode})", EventLogEntryType.Warning);
                // La disconnessione del client dovrebbe essere gestita dall'infrastruttura del SimpleSocketServer
            }
            catch (Exception ex)
            {
                // Gestisci altri errori durante la serializzazione/codifica/invio
                eventLog1.WriteEntry($"Errore durante l'invio della risposta '{responseMessage.Command}' al client {clientSocket.RemoteEndPoint}: {ex.Message}", EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Invia una notifica a tutti gli endpoint UI registrati (Connect-on-Demand).
        /// Per ogni UI, crea un client socket, si connette, invia il messaggio e si disconnette.
        /// </summary>
        /// <param name="notificationMessage">Il messaggio SocketMessageStructure di notifica da inviare.</param>
        private async Task SendNotificationToUi(SocketMessageStructure notificationMessage)
        {
            eventLog1.WriteEntry($"Tentativo di inviare notifica '{notificationMessage.MessageType}' a {_connectedUiEndpoints.Count} UI registrate.");

            // Serializza la notifica una sola volta (XML -> Base64 con delimitatori)
            string notificationXml = SocketMessageSerializer.Serialize(notificationMessage);
            byte[] xmlBytes = Encoding.UTF8.GetBytes(notificationXml);
            string notificationBase64 = Convert.ToBase64String(xmlBytes);
            string finalNotificationMessage = SocketMessageSerializer.Base64StartDelimiter + notificationBase64 + SocketMessageSerializer.Base64EndDelimiter;

            // Converti la stringa finale in byte una sola volta
            byte[] notificationData = Encoding.UTF8.GetBytes(finalNotificationMessage);


            // Itera sugli endpoint UI registrati nella dictionary
            // Usa ToList() per evitare modifiche alla collezione durante l'iterazione
            foreach (var endpoint in _connectedUiEndpoints.Keys.ToList())
            {
                string[] parts = endpoint.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int uiPort))
                {
                    string uiIp = parts[0];
                    eventLog1.WriteEntry($"Invio notifica a UI: {uiIp}:{uiPort}");

                    // Crea un nuovo SemanticClientSocket per questo endpoint UI
                    // Questo SemanticClientSocket è il client che si connette al server socket della UI.
                    // I delimitatori vengono ora gestiti internamente dal SemanticClientSocket o sono predefiniti.
                    using (SemanticClientSocket client = new SemanticClientSocket(uiIp, uiPort))
                    {
                        try
                        {
                            // Tenta la connessione (con un timeout)
                            // Il ConnectAsync del SemanticClientSocket gestisce già il timeout interno.
                            // Qui usiamo un Task.Run per non bloccare il loop del duty cycle,
                            // anche se SendMessageAsync è già asincrono.
                            await Task.Run(async () =>
                            {
                                // Connessione e invio avvengono in modo asincrono
                                // Il SemanticClientSocket gestisce la connessione e l'invio
                                // e la chiusura della connessione nel suo metodo SendMessageAsync.
                                bool sent = await client.SendMessageAsync(notificationMessage); // Passa l'oggetto messaggio
                                if (sent)
                                {
                                    eventLog1.WriteEntry($"Notifica '{notificationMessage.MessageType}' inviata con successo a {endpoint}.");
                                }
                                else
                                {
                                    eventLog1.WriteEntry($"Fallito invio notifica '{notificationMessage.MessageType}' a {endpoint}.", EventLogEntryType.Warning);
                                    // Potresti voler rimuovere l'endpoint dalla dictionary se l'invio fallisce ripetutamente
                                    // string removedEndpoint;
                                    // _connectedUiEndpoints.TryRemove(endpoint, out removedEndpoint);
                                    // eventLog1.WriteEntry($"Endpoint {endpoint} rimosso dalla lista a causa di errori di invio.", EventLogEntryType.Warning);
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            // Gestisci errori di connessione o invio per questo endpoint specifico
                            eventLog1.WriteEntry($"Errore nell'invio notifica a {endpoint}: {ex.Message}", EventLogEntryType.Warning);
                            // Decidi se rimuovere l'endpoint dalla dictionary in caso di errori persistenti
                            // string removedEndpoint;
                            // if (_connectedUiEndpoints.TryRemove(endpoint, out removedEndpoint))
                            // {
                            //     eventLog1.WriteEntry($"Endpoint {endpoint} rimosso dalla lista a causa di errori di invio.", EventLogEntryType.Warning);
                            // }
                        }
                        // Il 'using' block assicura che il client socket venga smaltito (chiuso) dopo l'invio o l'errore.
                        // Il SemanticClientSocket.SendMessageAsync dovrebbe gestire la connessione/disconnessione per ogni messaggio.
                    }
                }
                else
                {
                    eventLog1.WriteEntry($"Endpoint '{endpoint}' in formato non valido nella dictionary degli endpoint UI.", EventLogEntryType.Warning);
                }
            }
            eventLog1.WriteEntry($"Completato tentativo di invio notifica a tutte le UI registrate.");
        }


        // TODO: Aggiungi altri gestori eventi per notifiche (es. OnNewTheoremFound)

    }
}
