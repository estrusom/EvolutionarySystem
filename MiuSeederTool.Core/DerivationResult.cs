// File: MiuSeederTool.Core/DerivationResult.cs

using System;
using System.Collections.Generic;

namespace MiuSeederTool.Core
{
    /// <summary>
    /// Rappresenta il risultato di un'operazione di derivazione interna al Seeder.
    /// </summary>
    public class DerivationResult
    {
        /// <summary>
        /// Indica se la derivazione ha avuto successo (true) o è fallita (false).
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Il percorso di derivazione come lista di stringhe MIU (formato standard).
        /// Sarà popolato solo se Success è true.
        /// </summary>
        public List<string> Path { get; set; }

        /// <summary>
        /// Il numero di passi (applicazioni di regole) compiuti per raggiungere la stringa target.
        /// </summary>
        public int StepsTaken { get; set; }

        /// <summary>
        /// La profondità massima raggiunta durante la ricerca.
        /// </summary>
        public int DepthReached { get; set; }

        /// <summary>
        /// La stringa MIU iniziale della ricerca.
        /// </summary>
        public string InitialString { get; set; }

        /// <summary>
        /// La stringa MIU target della ricerca.
        /// </summary>
        public string TargetString { get; set; }

        /// <summary>
        /// Un messaggio di errore, se la derivazione è fallita.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Costruttore per un risultato di derivazione.
        /// </summary>
        public DerivationResult(bool success, List<string> path, int stepsTaken, int depthReached, string initialString, string targetString, string errorMessage = null)
        {
            Success = success;
            Path = path ?? new List<string>(); // Assicurati che non sia null
            StepsTaken = stepsTaken;
            DepthReached = depthReached;
            InitialString = initialString;
            TargetString = targetString;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Costruttore statico per un risultato di successo.
        /// </summary>
        public static DerivationResult SuccessResult(List<string> path, int stepsTaken, int depthReached, string initialString, string targetString)
        {
            return new DerivationResult(true, path, stepsTaken, depthReached, initialString, targetString);
        }

        /// <summary>
        /// Costruttore statico per un risultato di fallimento.
        /// </summary>
        public static DerivationResult FailureResult(string initialString, string targetString, string errorMessage)
        {
            return new DerivationResult(false, null, 0, 0, initialString, targetString, errorMessage);
        }
    }
}
