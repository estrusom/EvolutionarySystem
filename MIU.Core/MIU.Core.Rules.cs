using System;
using System.Linq;

namespace MIU.Core.Rules
{
    /// <summary>
    /// Classe base astratta per tutte le regole MIU.
    /// Ogni regola specifica erediterà da questa classe.
    /// </summary>
    public abstract class MIURule
    {
        public string Name { get; protected set; } // Nome della regola (es. "Rule 1")

        /// <summary>
        /// Determina se la regola è applicabile a una data stringa MIU.
        /// </summary>
        /// <param name="miuString">La stringa MIU su cui testare l'applicabilità.</param>
        /// <returns>True se la regola è applicabile, False altrimenti.</returns>
        public abstract bool IsApplicable(string miuString);

        /// <summary>
        /// Applica la regola a una data stringa MIU e restituisce la stringa risultante.
        /// È responsabilità dell'implementazione concreta gestire i casi in cui la regola non è applicabile.
        /// </summary>
        /// <param name="miuString">La stringa MIU su cui applicare la regola.</param>
        /// <returns>La nuova stringa MIU dopo l'applicazione della regola.</returns>
        public abstract string Apply(string miuString);
    }

    // --- Implementazioni delle Regole MIU Specifiche ---

    /// <summary>
    /// Regola 1: Se la stringa termina con 'I', puoi aggiungere 'U' alla fine.
    /// Es: MI -> MIU
    /// </summary>
    public class Rule1 : MIURule
    {
        public Rule1()
        {
            Name = "Rule 1: Add U if ends with I";
        }

        public override bool IsApplicable(string miuString)
        {
            return miuString != null && miuString.EndsWith("I");
        }

        public override string Apply(string miuString)
        {
            if (!IsApplicable(miuString))
            {
                // La regola non è applicabile, restituisce la stringa originale o lancia un'eccezione
                return miuString;
            }
            return miuString + "U";
        }
    }

    /// <summary>
    /// Regola 2: Raddoppia la stringa dopo la 'M' iniziale.
    /// Es: MI -> MII
    /// </summary>
    public class Rule2 : MIURule
    {
        public Rule2()
        {
            Name = "Rule 2: Double string after M";
        }

        public override bool IsApplicable(string miuString)
        {
            // La regola è applicabile a qualsiasi stringa che inizia con 'M'
            // e ha almeno un carattere dopo la 'M'.
            return miuString != null && miuString.StartsWith("M") && miuString.Length > 1;
        }

        public override string Apply(string miuString)
        {
            if (!IsApplicable(miuString))
            {
                return miuString;
            }
            // Prende la parte della stringa dopo la 'M' iniziale
            string suffix = miuString.Substring(1);
            return "M" + suffix + suffix;
        }
    }

    /// <summary>
    /// Regola 3: Se 'III' appare in qualsiasi punto, puoi sostituirlo con 'U'.
    /// Es: MIII -> MU
    /// </summary>
    public class Rule3 : MIURule
    {
        public Rule3()
        {
            Name = "Rule 3: Replace III with U";
        }

        public override bool IsApplicable(string miuString)
        {
            return miuString != null && miuString.Contains("III");
        }

        public override string Apply(string miuString)
        {
            if (!IsApplicable(miuString))
            {
                return miuString;
            }
            // Sostituisce tutte le occorrenze di "III" con "U"
            return miuString.Replace("III", "U");
        }
    }

    /// <summary>
    /// Regola 4: Se 'UU' appare in qualsiasi punto, puoi semplicemente rimuoverlo.
    /// Es: MUU -> M
    /// </summary>
    public class Rule4 : MIURule
    {
        public Rule4()
        {
            Name = "Rule 4: Remove UU";
        }

        public override bool IsApplicable(string miuString)
        {
            return miuString != null && miuString.Contains("UU");
        }

        public override string Apply(string miuString)
        {
            if (!IsApplicable(miuString))
            {
                return miuString;
            }
            // Sostituisce tutte le occorrenze di "UU" con una stringa vuota (rimuovendole)
            return miuString.Replace("UU", "");
        }
    }
}
