// Esempio di classe ClientInfo che potresti avere nel tuo dizionario
using SocketManager;
using SocketManagerInfo;
using System.Collections.Generic;

public class ClientInfo
{
    public string ServerIp { get; set; }
    public int ServerPort { get; set; }
    // Altre proprietà utili, es. IdClient
}

// ... nel tuo codice che gestisce i client
// ConcurrentDictionary<string, ClientInfo> _connectedUiClients; // Supponiamo sia definito altrove

// La lista dovrebbe essere usata per mantenere i client attivi
List<SemanticClientSocket> activeClients = new List<SemanticClientSocket>();

// Per la creazione iniziale o l'aggiunta di nuovi client
foreach (System.Collections.Generic.KeyValuePair<string, ClientInfo> entry in _connectedUiClients)
{
    ClientInfo clientData = entry.Value;
    SemanticClientSocket scs = new SemanticClientSocket(clientData.ServerIp, clientData.ServerPort, "<MSG_START>", "<MSG_END>");
    scs.MessageSentSuccess += Client_MessageSentSuccess;
    scs.MessageSentFailed += Client_MessageSentFailed;
    // Potresti voler chiamare scs.ConnectAsync() qui, magari in modo asincrono
    // scs.ConnectAsync(); // Attenzione a come gestisci l'await in un foreach
    activeClients.Add(scs);
}

// Se ricevi un comando CmdSyn e vuoi inviare un messaggio a TUTTI i client connessi:
// Non "ricaricare" la lista, ma itera sui client esistenti e invia il comando.
// Esempio di invio di un comando a tutti i client già connessi e gestiti:
// (Assumendo che l'invio sia asincrono e non blocchi il ciclo)

public async System.Threading.Tasks.Task SendCmdSynToAllClients()
{
    foreach (SemanticClientSocket client in activeClients)
    {
        if (client != null) // Assicurati che l'oggetto non sia null
        {
            // Crea il messaggio CmdSyn
            SocketMessageStructure cmdSynMessage = new SocketMessageStructure
            {
                Command = "CmdSyn",
                MessageType = "Command",
                SendingTime = System.DateTime.UtcNow,
                Token = System.Guid.NewGuid().ToString(),
                BufferDati = new System.Xml.Linq.XElement("Data", "SyncCommandPayload")
            };

            // Invia il messaggio in modo asincrono.
            // L'await qui potrebbe bloccare il ciclo se non sei in un contesto Task.WhenAll.
            // Se non vuoi bloccare, puoi semplicemente avviare il Task:
            // _ = client.SendMessageAsync(cmdSynMessage);
            await client.SendMessageAsync(cmdSynMessage); // Se vuoi attendere ogni invio
        }
    }
}

// Quando l'applicazione si chiude o un client non è più necessario, devi disporre i client:
public void DisposeAllClients()
{
    foreach (SemanticClientSocket client in activeClients)
    {
        client.Dispose();
    }
    activeClients.Clear()