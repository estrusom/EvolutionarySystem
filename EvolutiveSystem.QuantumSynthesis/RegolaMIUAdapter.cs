using EvolutiveSystem.Common;
using EvolutiveSystem.Logic;
using System;
using System.Text.RegularExpressions;

namespace EvolutiveSystem.QuantumSynthesis
{
    /// <summary>
    /// Classe Adapter che funge da ponte tra il tipo 'RegolaMIU' e l'interfaccia 'MIURule'.
    /// Permette al RuleCandidateProposer di lavorare con le regole esistenti
    /// senza dover modificare la classe MIURule del motore.
    /// </summary>
    public class RegolaMIUAdapter : MIURule
    {
        private readonly RegolaMIU _regola;

        public RegolaMIUAdapter(RegolaMIU regola)
        {
            _regola = regola ?? throw new ArgumentNullException(nameof(regola));
        }

        // Usiamo 'new' perché la classe base ha già un'implementazione.
        // In questo modo, nascondiamo la versione della classe base.
        public new long Id => _regola.ID;
        public new string Name => _regola.Nome;
        public new string Description => _regola.Descrizione;

        // I metodi sono correttamente sovrascritti con 'override'.
        public override bool IsApplicable(MIUState state)
        {
            if (state == null || string.IsNullOrEmpty(state.CurrentString))
            {
                return false;
            }
            return Regex.IsMatch(state.CurrentString, _regola.Pattern);
        }

        public override MIUState Apply(MIUState state)
        {
            if (!IsApplicable(state))
            {
                throw new InvalidOperationException($"La regola '{Name}' non è applicabile allo stato '{state.CurrentString}'.");
            }
            string newString = Regex.Replace(state.CurrentString, _regola.Pattern, _regola.Sostituzione);

            return new MIUState(newString);
        }
    }
}