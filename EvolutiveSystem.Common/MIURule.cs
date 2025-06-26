// EvolutiveSystem.Common/MIURule.cs
// Data di riferimento: 26 giugno 2025
// Descrizione: Definizioni per le regole di derivazione del sistema MIU.
//              Questa classe astratta e le sue implementazioni concrete
//              descrivono come uno stato MIU può essere trasformato in un altro.

using System; // Necessario per InvalidOperationException e altre funzionalità base.

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Classe astratta base per tutte le regole di derivazione del sistema MIU.
    /// Ogni regola deve sapere se è applicabile a un dato stato e come applicarsi.
    /// Questo risolve gli errori 'MIURule' non contiene una definizione di 'IsApplicable' e 'Apply'.
    /// </summary>
    public abstract class MIURule
    {
        /// <summary>
        /// Ottiene il nome della regola.
        /// </summary>
        public string Name { get; protected set; } // 'protected set' permette alle classi derivate di impostarlo.

        /// <summary>
        /// Verifica se questa regola può essere applicata a un dato stato MIU.
        /// </summary>
        /// <param name="state">Lo stato MIU da controllare.</param>
        /// <returns>True se la regola è applicabile, altrimenti false.</returns>
        public abstract bool IsApplicable(MIUState state);

        /// <summary>
        /// Applica la regola a un dato stato MIU e restituisce il nuovo stato derivato.
        /// È responsabilità del chiamante verificare l'applicabilità prima di chiamare questo metodo.
        /// </summary>
        /// <param name="state">Lo stato MIU di partenza.</param>
        /// <returns>Il nuovo stato MIU risultante dall'applicazione della regola.</returns>
        /// <exception cref="InvalidOperationException">Lanciata se la regola non è applicabile allo stato fornito.</exception>
        public abstract MIUState Apply(MIUState state);

        /// <summary>
        /// Fornisce una rappresentazione stringa della regola (il suo nome).
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }

    // --- Implementazioni Concretre delle Quattro Regole MIU ---

    /// <summary>
    /// Regola I: Se una stringa termina con 'I', puoi aggiungere 'U' alla fine.
    /// Esempio: "MI" -> "MIU"
    /// </summary>
    public class RuleI : MIURule
    {
        public RuleI() { Name = "Regola I"; } // Il costruttore imposta il nome della regola.

        public override bool IsApplicable(MIUState state)
        {
            // La regola è applicabile se lo stato termina con 'I'.
            return state.CurrentString.EndsWith("I");
        }

        public override MIUState Apply(MIUState state)
        {
            // Verifica pre-condizione: la regola deve essere applicabile.
            if (!IsApplicable(state))
                throw new InvalidOperationException($"Regola I non applicabile allo stato '{state.CurrentString}'. Lo stato deve terminare con 'I'.");

            // Applica la regola aggiungendo 'U'.
            return new MIUState(state.CurrentString + "U");
        }
    }

    /// <summary>
    /// Regola II: Se hai 'Mx', puoi aggiungere 'Mxx'. (Doppia la parte dopo la 'M')
    /// Esempio: "MIU" -> "MIIUU"
    /// </summary>
    public class RuleII : MIURule
    {
        public RuleII() { Name = "Regola II"; }

        public override bool IsApplicable(MIUState state)
        {
            // La regola è applicabile se lo stato inizia con 'M' e ha almeno un carattere dopo la 'M'.
            return state.CurrentString.StartsWith("M") && state.CurrentString.Length > 1;
        }

        public override MIUState Apply(MIUState state)
        {
            // Verifica pre-condizione.
            if (!IsApplicable(state))
                throw new InvalidOperationException($"Regola II non applicabile allo stato '{state.CurrentString}'. Lo stato deve iniziare con 'M' e avere almeno un carattere dopo.");

            // Estrae la parte dopo la 'M' e la raddoppia.
            string afterM = state.CurrentString.Substring(1);
            return new MIUState("M" + afterM + afterM);
        }
    }

    /// <summary>
    /// Regola III: Se hai tre 'I' consecutive (III), puoi sostituirle con una 'U'.
    /// Esempio: "MIII" -> "MU"
    /// </summary>
    public class RuleIII : MIURule
    {
        public RuleIII() { Name = "Regola III"; }

        public override bool IsApplicable(MIUState state)
        {
            // La regola è applicabile se lo stato contiene la sequenza "III".
            return state.CurrentString.Contains("III");
        }

        public override MIUState Apply(MIUState state)
        {
            // Verifica pre-condizione.
            if (!IsApplicable(state))
                throw new InvalidOperationException($"Regola III non applicabile allo stato '{state.CurrentString}'. Lo stato deve contenere 'III'.");

            // Sostituisce la prima occorrenza di "III" con "U".
            return new MIUState(state.CurrentString.Replace("III", "U"));
        }
    }

    /// <summary>
    /// Regola IV: Se hai due 'U' consecutive (UU), puoi rimuoverle.
    /// Esempio: "MUU" -> "M"
    /// </summary>
    public class RuleIV : MIURule
    {
        public RuleIV() { Name = "Regola IV"; }

        public override bool IsApplicable(MIUState state)
        {
            // La regola è applicabile se lo stato contiene la sequenza "UU".
            return state.CurrentString.Contains("UU");
        }

        public override MIUState Apply(MIUState state)
        {
            // Verifica pre-condizione.
            if (!IsApplicable(state))
                throw new InvalidOperationException($"Regola IV non applicabile allo stato '{state.CurrentString}'. Lo stato deve contenere 'UU'.");

            // Sostituisce la prima occorrenza di "UU" con una stringa vuota (le rimuove).
            return new MIUState(state.CurrentString.Replace("UU", ""));
        }
    }
}
