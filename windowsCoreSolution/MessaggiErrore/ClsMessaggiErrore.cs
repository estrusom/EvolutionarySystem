using System;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace MessaggiErrore
{
    /// <summary>
    /// Gestione dei messaggi di errore provenienti dalle eccezioni
    /// </summary>
    public static class ClsMessaggiErrore
    {
        /// <summary>
        /// Crea un messaggio completo per la visualizzazione o il logging da un'eccezione.
        /// </summary>
        /// <param name="ex">L'eccezione rilevata.</param>
        /// <param name="thisMethod">Il metodo in cui si è verificata l'eccezione (opzionale).</param>
        /// <param name="customMessage">Un messaggio supplementare da aggiungere (opzionale).</param>
        /// <returns>Il messaggio di errore compilato, incluso lo stack trace per il debug.</returns>
        public static string CustomMsg(Exception ex, MethodBase thisMethod = null, string customMessage = null)
        {
            // Usiamo StringBuilder per costruire in modo efficiente il messaggio.
            var myStr = new StringBuilder();

            // Aggiungiamo sempre il messaggio principale dell'eccezione.
            myStr.AppendLine($"MESSAGGIO: {ex.Message}");

            // Controlliamo e aggiungiamo il messaggio dell'eccezione interna, se presente.
            if (ex.InnerException != null)
            {
                myStr.AppendLine($"MESSAGGIO ECCEZIONE INTERNA: {ex.InnerException.Message}");
            }

            // Aggiungiamo le informazioni sul metodo e il messaggio personalizzato, se forniti.
            if (thisMethod != null)
            {
                myStr.AppendLine($"MODULO: {thisMethod.DeclaringType.FullName}");
                myStr.AppendLine($"PROCEDURA: {thisMethod.Name}");
            }

            // Aggiungiamo il messaggio personalizzato, se fornito.
            if (!string.IsNullOrEmpty(customMessage))
            {
                myStr.AppendLine($"MESSAGGIO AGGIUNTIVO: {customMessage}");
            }

            // Aggiungiamo lo stack trace, che è fondamentale per il debug.
            myStr.AppendLine($"STACK TRACE: {ex.StackTrace}");

            return myStr.ToString();
        }

        /// <summary>
        /// Estrae il codice di errore nativo da un'eccezione Win32.
        /// </summary>
        /// <param name="ex">L'eccezione da cui estrarre il codice.</param>
        /// <returns>Il codice di errore nativo se è un'eccezione Win32, altrimenti -1.</returns>
        public static int ErrCode(Exception ex)
        {
            // Verifichiamo il tipo dell'eccezione prima di tentare il cast,
            // per evitare di dover usare un blocco try-catch.
            if (ex is Win32Exception win32Ex)
            {
                return win32Ex.NativeErrorCode;
            }
            return -1;
        }
    }
}
