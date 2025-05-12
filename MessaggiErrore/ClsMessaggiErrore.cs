using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessaggiErrore
{
    /// <summary>
    /// Gestione dei messaggi d'errore provenienti dalle eccezioni 
    /// </summary>
    public static class ClsMessaggiErrore 
    {
        /// <summary>
        /// Creazione del messaggio da visualizzare o da inserire nel log event
        /// </summary>
        /// <param name="Ex">Eccezione rilevata</param>
        /// <returns>Messaggio compilato</returns>
        public static string CustomMsg(Exception Ex)
        {
            StringBuilder myStr = new StringBuilder();
            myStr.Append(Ex.Message);
            myStr.Append("\t");
            if (Ex.InnerException != null)
            {
                myStr.Append(Ex.InnerException.Message);
                myStr.Append("\t");
            }
            return myStr.ToString();
        }
        /// <summary>
        /// Creazione del messaggio da visualizzare o da inserire nel log event
        /// </summary>
        /// <param name="Ex">Eccezione rilevata</param>
        /// <param name="ThisMethod">Metodo in cui è avvenuta l'eccezione</param>
        /// <returns>Messaggio compilato</returns>
        public static string CustomMsg(Exception Ex, MethodBase ThisMethod)
        {
            StringBuilder myStr = new StringBuilder();
            myStr.Append(Ex.Message);
            myStr.Append("\t");
            if (Ex.InnerException != null)
            {
                myStr.Append(Ex.InnerException.Message);
                myStr.Append("\t");
            }
            string procedura = string.Format("MODULO: {0} PROCEDURA: {1}", ThisMethod.DeclaringType.FullName, ThisMethod.Name);
            myStr.Append(procedura);
            return myStr.ToString();
        }
        /// <summary>
        /// Creazione del messaggio da visualizzare o da inserire nel log event con messaggio supplementare
        /// </summary>
        /// <param name="Ex">Eccezione rilevata</param>
        /// <param name="ThisMethod">Metodo in cui è avvenuta l'eccezione</param>
        /// <param name="CustomMessage">Messaggio supplementare</param>
        /// <returns></returns>
        public static string CustomMsg(Exception Ex, MethodBase ThisMethod, string CustomMessage)
        {
            StringBuilder myStr = new StringBuilder();
            myStr.Append(Ex.Message);
            myStr.Append("\t");
            if (Ex.InnerException != null)
            {
                myStr.Append(Ex.InnerException.Message);
                myStr.Append("\t");
            }
            string procedura = string.Format("MODULO: {0} PROCEDURA: {1} MESSAGGIO: {2}", ThisMethod.DeclaringType.FullName, ThisMethod.Name, CustomMessage);
            myStr.Append(procedura);
            return myStr.ToString();
        }
        /// <summary>
        /// estrae il codice d'errore dall'exception se il codice non esiste torna -1
        /// </summary>
        /// <param name="ErrCode"></param>
        /// <returns></returns>
        public static int ErrCode(Exception ErrCode)
        {
            int RetCodErr = 0;
            try
            {
                RetCodErr = ((System.ComponentModel.Win32Exception)ErrCode).NativeErrorCode;
            }
            catch (Exception ex)
            {
                RetCodErr = -1;
            }
            return RetCodErr;
        }
    }
}
