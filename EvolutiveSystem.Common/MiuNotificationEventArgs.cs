// File: EvolutiveSystem.Common/MiuNotificationEventArgs.cs
// Data di riferimento: 23 giugno 2025
// Descrizione: EventArgs personalizzato per le notifiche all'interno del sistema MIU.
// Utilizzato per comunicare stati, progressi o risultati tra i componenti.

using System;

namespace EvolutiveSystem.Common // Namespace per le utilità comuni MIU-specifiche
{
    /// <summary>
    /// EventArgs personalizzato per le notifiche all'interno del sistema MIU.
    /// Contiene un tipo di notifica (stringa) e un messaggio descrittivo.
    /// </summary>
    public class MiuNotificationEventArgs : EventArgs
    {
        /// <summary>
        /// Il tipo di notifica (es. "EXPLORATION_PROGRESS", "MIU_SOLUTION_FOUND", "MIU_SEARCH_FAILED").
        /// </summary>
        public string NotificationType { get; }

        /// <summary>
        /// Il messaggio associato alla notifica.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Costruttore per MiuNotificationEventArgs.
        /// </summary>
        /// <param name="type">Il tipo della notifica.</param>
        /// <param name="message">Il messaggio descrittivo della notifica.</param>
        public MiuNotificationEventArgs(string type, string message)
        {
            NotificationType = type;
            Message = message;
        }
    }
    /// <summary>
    /// *** CLASSE EVENTARGS PER L'EVENTO OnNewStringDiscovered DEL MOTORE 
    /// </summary>
    public class NewMiuStringFoundEventArgs : EventArgs
    {
        public string NewMiuString { get; }
        public string DerivationPath { get; }

        public NewMiuStringFoundEventArgs(string newMiuString, string derivationPath)
        {
            NewMiuString = newMiuString;
            DerivationPath = derivationPath;
        }
    }
    public class NewMiuStringDiscoveredEventArgs : EventArgs
    {
        public long SearchID { get; set; } // L'ID della ricerca corrente
        public string DiscoveredString { get; set; } // La stringa MIU standard scoperta
        public bool IsTrulyNewToDatabase { get; set; } // Indica se è anche nuova al DB (risultato di UpsertMIUState)
        public long StateID { get; set; } // L'ID assegnato o recuperato dal DB
        public int Depth { get; set; } // 2025.07.18 Profondità a cui la stringa è stata scoperta
    }
}
