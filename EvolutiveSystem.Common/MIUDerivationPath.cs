// EvolutiveSystem.Common/MIUDerivationPath.cs
// Data di riferimento: 25 giugno 2025 19.50
// Descrizione: Rappresenta un percorso di derivazione nel sistema MIU.
// Contiene la sequenza di stati e regole applicate, fungendo da "vettore" del percorso.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Rappresenta un percorso di derivazione completo all'interno del sistema MIU.
    /// Questo oggetto è immutabile dopo la creazione.
    /// Il percorso è una sequenza di MIUState e delle MIURule applicate per raggiungerli.
    /// Non contiene elementi grafici, ma i "vettori" che descrivono la derivazione.
    /// </summary>
    public sealed class MIUDerivationPath : IEquatable<MIUDerivationPath>
    {
        // Utilizziamo una lista di tuple o una classe interna Step per chiarezza e immutabilità.
        // Ogni Step rappresenta un passaggio nel percorso: (stato iniziale, regola applicata, stato finale)
        private readonly ReadOnlyCollection<DerivationStep> _steps;

        /// <summary>
        /// Ottiene il primo stato del percorso (lo stato iniziale della derivazione).
        /// </summary>
        public MIUState InitialState => _steps.FirstOrDefault()?.InitialState;

        /// <summary>
        /// Ottiene l'ultimo stato del percorso (lo stato finale della derivazione).
        /// </summary>
        public MIUState FinalState => _steps.LastOrDefault()?.ResultState;

        /// <summary>
        /// Ottiene una collezione di passi di derivazione (MIUState, MIURule, MIUState).
        /// </summary>
        public IReadOnlyList<DerivationStep> Steps => _steps;

        /// <summary>
        /// Inizializza una nuova istanza della classe MIUDerivationPath.
        /// </summary>
        /// <param name="initialState">Lo stato iniziale del percorso.</param>
        /// <param name="steps">Una collezione di DerivationStep che compongono il percorso.</param>
        /// <exception cref="ArgumentNullException">Lanciata se initialState o steps è null.</exception>
        /// <exception cref="ArgumentException">Lanciata se steps è vuoto o il primo passo non inizia con initialState.</exception>
        public MIUDerivationPath(MIUState initialState, IEnumerable<DerivationStep> steps)
        {
            if (initialState == null) throw new ArgumentNullException(nameof(initialState));
            if (steps == null) throw new ArgumentNullException(nameof(steps));

            var stepsList = steps.ToList();
            if (!stepsList.Any())
            {
                _steps = new ReadOnlyCollection<DerivationStep>(new List<DerivationStep> { new DerivationStep(initialState, null, initialState) });
                // If no steps are provided, it means the path is just the initial state itself.
                // We'll represent this as a "step" from initial state to itself with a null rule,
                // indicating no actual derivation took place but the path starts and ends there.
            }
            else
            {
                // Validate that the path is continuous and starts from the initial state
                if (!stepsList[0].InitialState.Equals(initialState))
                {
                    throw new ArgumentException("Il primo passo della derivazione deve iniziare dallo stato iniziale fornito.", nameof(steps));
                }

                for (int i = 0; i < stepsList.Count - 1; i++)
                {
                    if (!stepsList[i].ResultState.Equals(stepsList[i + 1].InitialState))
                    {
                        throw new ArgumentException("Il percorso di derivazione non è continuo. Lo stato finale di un passo deve corrispondere allo stato iniziale del passo successivo.", nameof(steps));
                    }
                }
                _steps = new ReadOnlyCollection<DerivationStep>(stepsList);
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza della classe MIUDerivationPath con un singolo stato iniziale.
        /// Questo crea un percorso di lunghezza zero, rappresentando solo lo stato di partenza.
        /// </summary>
        /// <param name="initialState">Lo stato iniziale del percorso.</param>
        public MIUDerivationPath(MIUState initialState)
            : this(initialState, Enumerable.Empty<DerivationStep>())
        {
        }


        /// <summary>
        /// Aggiunge un nuovo passo di derivazione al percorso esistente.
        /// Questo metodo restituisce una *nuova* istanza di MIUDerivationPath, mantenendo l'immutabilità.
        /// </summary>
        /// <param name="rule">La regola applicata.</param>
        /// <param name="resultState">Lo stato risultante dall'applicazione della regola.</param>
        /// <returns>Una nuova istanza di MIUDerivationPath che include il nuovo passo.</returns>
        /// <exception cref="ArgumentException">Lanciata se il InitialState del nuovo step non corrisponde al FinalState del percorso corrente.</exception>
        public MIUDerivationPath AppendStep(MIURule rule, MIUState resultState)
        {
            // Il nuovo passo deve partire dal FinalState corrente
            if (FinalState == null)
            {
                throw new InvalidOperationException("Impossibile aggiungere un passo a un percorso senza stato iniziale valido.");
            }

            var newStep = new DerivationStep(FinalState, rule, resultState);
            var newStepsList = _steps.ToList(); // Crea una lista modificabile
            newStepsList.Add(newStep);

            // Costruisci una nuova istanza con il primo stato originale e la nuova lista di passi
            // Nota: qui la validazione interna del costruttore verificherà la continuità.
            return new MIUDerivationPath(InitialState, newStepsList);
        }

        /// <summary>
        /// Determina se l'oggetto MIUDerivationPath corrente è uguale a un altro oggetto MIUDerivationPath.
        /// </summary>
        /// <param name="other">L'oggetto MIUDerivationPath da confrontare con l'oggetto corrente.</param>
        /// <returns>True se gli oggetti sono uguali; altrimenti, false.</returns>
        public bool Equals(MIUDerivationPath other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (_steps.Count != other._steps.Count) return false;

            for (int i = 0; i < _steps.Count; i++)
            {
                if (!_steps[i].Equals(other._steps[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determina se l'oggetto MIUDerivationPath corrente è uguale a un altro oggetto.
        /// </summary>
        /// <param name="obj">L'oggetto da confrontare con l'oggetto corrente.</param>
        /// <returns>True se gli oggetti sono uguali; altrimenti, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as MIUDerivationPath);
        }

        /// <summary>
        /// Serve come funzione hash predefinita.
        /// </summary>
        /// <returns>Un codice hash per l'oggetto corrente.</returns>
        public override int GetHashCode()
        {
            // Combina gli hash code di tutti i passi
            int hash = 17;
            foreach (var step in _steps)
            {
                hash = hash * 23 + step.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Restituisce una rappresentazione stringa del percorso di derivazione.
        /// </summary>
        /// <returns>Una stringa che rappresenta il percorso.</returns>
        public override string ToString()
        {
            if (!_steps.Any())
            {
                return $"Percorso: {InitialState} (Nessun passo)";
            }

            var pathString = new System.Text.StringBuilder();
            pathString.Append($"Percorso: {InitialState}");

            foreach (var step in _steps)
            {
                pathString.Append($" --[{step.AppliedRule.Name}]--> {step.ResultState}");
            }
            return pathString.ToString();
        }

        /// <summary>
        /// Rappresenta un singolo passo di derivazione all'interno di un percorso MIU.
        /// </summary>
        public sealed class DerivationStep : IEquatable<DerivationStep>
        {
            /// <summary>
            /// Ottiene lo stato MIU prima dell'applicazione della regola.
            /// </summary>
            public MIUState InitialState { get; }

            /// <summary>
            /// Ottiene la regola MIU applicata in questo passo.
            /// Può essere null solo per il primo "passo" di un percorso con lunghezza zero,
            /// dove lo stato iniziale è anche lo stato finale.
            /// </summary>
            public MIURule AppliedRule { get; }

            /// <summary>
            /// Ottiene lo stato MIU risultante dopo l'applicazione della regola.
            /// </summary>
            public MIUState ResultState { get; }

            /// <summary>
            /// Inizializza una nuova istanza della classe DerivationStep.
            /// </summary>
            /// <param name="initialState">Lo stato prima dell'applicazione della regola.</param>
            /// <param name="appliedRule">La regola applicata. Può essere null solo se InitialState == ResultState.</param>
            /// <param name="resultState">Lo stato dopo l'applicazione della regola.</param>
            /// <exception cref="ArgumentNullException">Lanciata se initialState o resultState sono null.</exception>
            /// <exception cref="ArgumentException">Lanciata se appliedRule è null e InitialState != ResultState, o viceversa.</exception>
            public DerivationStep(MIUState initialState, MIURule appliedRule, MIUState resultState)
            {
                if (initialState == null) throw new ArgumentNullException(nameof(initialState));
                if (resultState == null) throw new ArgumentNullException(nameof(resultState));

                // Validazione: Se la regola è null, gli stati devono essere identici (passo "vuoto")
                // Se la regola non è null, gli stati non dovrebbero essere necessariamente identici (regola applicata)
                if (appliedRule == null && !initialState.Equals(resultState))
                {
                    throw new ArgumentException("Se la regola applicata è null, lo stato iniziale e finale devono essere identici.");
                }
                if (appliedRule != null && initialState.Equals(resultState))
                {
                    // Questa è una condizione che potrebbe essere valida per alcune regole MIU (es. I -> IU -> I)
                    // ma solleva un flag se la regola non è null ma lo stato non cambia.
                    // Per ora la permettiamo ma è un buon punto per future validazioni più specifiche.
                }

                InitialState = initialState;
                AppliedRule = appliedRule;
                ResultState = resultState;
            }

            /// <summary>
            /// Determina se l'oggetto DerivationStep corrente è uguale a un altro oggetto DerivationStep.
            /// </summary>
            /// <param name="other">L'oggetto DerivationStep da confrontare con l'oggetto corrente.</param>
            /// <returns>True se gli oggetti sono uguali; altrimenti, false.</returns>
            public bool Equals(DerivationStep other)
            {
                if (ReferenceEquals(other, null)) return false;
                if (ReferenceEquals(this, other)) return true;

                return InitialState.Equals(other.InitialState) &&
                       (AppliedRule?.Equals(other.AppliedRule) ?? ReferenceEquals(other.AppliedRule, null)) && // Gestisce i null per AppliedRule
                       ResultState.Equals(other.ResultState);
            }

            /// <summary>
            /// Determina se l'oggetto DerivationStep corrente è uguale a un altro oggetto.
            /// </summary>
            /// <param name="obj">L'oggetto da confrontare con l'oggetto corrente.</param>
            /// <returns>True se gli oggetti sono uguali; altrimenti, false.</returns>
            public override bool Equals(object obj)
            {
                return Equals(obj as DerivationStep);
            }

            /// <summary>
            /// Serve come funzione hash predefinita.
            /// </summary>
            /// <returns>Un codice hash per l'oggetto corrente.</returns>
            public override int GetHashCode()
            {
                // Combinazione di hash codes
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + InitialState.GetHashCode();
                    hash = hash * 23 + (AppliedRule?.GetHashCode() ?? 0);
                    hash = hash * 23 + ResultState.GetHashCode();
                    return hash;
                }
            }

            /// <summary>
            /// Restituisce una rappresentazione stringa del passo di derivazione.
            /// </summary>
            /// <returns>Una stringa che rappresenta il passo.</returns>
            public override string ToString()
            {
                return $"{InitialState} --[{AppliedRule?.Name ?? "Nessuna Regola"}]--> {ResultState}";
            }
        }
    }
}
