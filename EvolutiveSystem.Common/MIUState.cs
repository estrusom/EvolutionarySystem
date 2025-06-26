// EvolutiveSystem.Common/MIUState.cs
// Data di riferimento: 26 giugno 2025
// Descrizione: Rappresenta un singolo stato nel sistema MIU,
//              basato sulla stringa corrente.

using System;
// Non è necessario System.Collections.Generic per questa versione di MIUState.

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta un singolo stato nel sistema MIU, definito da una stringa.
    /// Gli stati sono considerati uguali se le loro stringhe rappresentative sono identiche.
    /// Implementa IEquatable<T> per un confronto efficiente.
    /// </summary>
    public class MIUState : IEquatable<MIUState>
    {
        /// <summary>
        /// Ottiene la stringa corrente che definisce questo stato MIU.
        /// Questo risolve l'errore 'MIUState' non contiene una definizione di 'CurrentString'.
        /// </summary>
        public string CurrentString { get; }

        /// <summary>
        /// Costruttore per creare un nuovo stato MIU.
        /// </summary>
        /// <param name="stateString">La stringa che rappresenta lo stato MIU.</param>
        public MIUState(string stateString)
        {
            // Validazione per assicurare che la stringa non sia nulla o vuota
            if (string.IsNullOrWhiteSpace(stateString))
            {
                throw new ArgumentException("La stringa dello stato non può essere nulla o vuota.", nameof(stateString));
            }
            CurrentString = stateString;
        }

        /// <summary>
        /// Fornisce una rappresentazione stringa dello stato, che è la sua CurrentString.
        /// </summary>
        /// <returns>La stringa dello stato MIU.</returns>
        public override string ToString()
        {
            return CurrentString;
        }

        /// <summary>
        /// Indica se l'oggetto corrente è uguale a un altro oggetto dello stesso tipo (MIUState).
        /// Necessario per i confronti di stato nell'esploratore.
        /// </summary>
        /// <param name="other">Un oggetto MIUState da confrontare con questo oggetto.</param>
        /// <returns>true se l'oggetto corrente è uguale al parametro <paramref name="other"/>; in caso contrario, false.</returns>
        public bool Equals(MIUState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CurrentString == other.CurrentString;
        }

        /// <summary>
        /// Determina se l'oggetto specificato è uguale all'oggetto corrente.
        /// Questo override è necessario per una corretta interazione con collezioni non generiche.
        /// </summary>
        /// <param name="obj">L'oggetto da confrontare con l'oggetto corrente.</param>
        /// <returns>true se l'oggetto specificato è uguale all'oggetto corrente; in caso contrario, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            // Se l'oggetto non è del tipo MIUState o un suo derivato, non sono uguali.
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MIUState)obj);
        }

        /// <summary>
        /// Funge da funzione hash predefinita. Essenziale per l'uso in HashSet<T> e Dictionary<K,V>.
        /// L'implementazione si basa sull'hash della stringa CurrentString.
        /// </summary>
        /// <returns>Un codice hash per l'oggetto corrente.</returns>
        public override int GetHashCode()
        {
            // Se CurrentString è null (cosa che non dovrebbe accadere per via del costruttore),
            // GetHashCode() causerebbe un'eccezione. Il null-conditional operator (??) gestisce questo caso.
            return CurrentString?.GetHashCode() ?? 0;
        }
    }
}
