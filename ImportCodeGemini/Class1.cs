// EvolutiveSystem.Taxonomy/MiuAbstractPattern.cs
// Data di riferimento: 29 luglio 2025
// Descrizione: Classe base astratta per tutti i pattern MIU che rappresentano antitesi.

using System;
using System.Collections.Generic;
using EvolutiveSystem.Common; // Necessario per RegolaMIU
using EvolutiveSystem.Abstractions; // NUOVO: Necessario per IRuleFailureDetail e IAntithesisWithDiagnosticDetails

namespace EvolutiveSystem.Taxonomy
{
    /// <summary>
    /// Classe base astratta per i pattern MIU che rappresentano antitesi
    /// (es. gap non coperti o inefficienze nel campo di esistenza MIU).
    /// </summary>
    public abstract class MiuAbstractPattern
    {
        public Guid ID { get; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public DateTime IdentifiedAt { get; }

        // --- PROPRIETÀ AGGIORNATE PER LA DIAGNOSI, USANO LE INTERFACCE DA ABSTRACTIONS ---
        /// <summary>
        /// Dettagli sulle regole MIU esistenti che sono state provate sugli stati problematici
        /// e sono fallite o non hanno portato al risultato atteso.
        /// Usa l'interfaccia IRuleFailureDetail per evitare dipendenze circolari.
        /// </summary>
        public List<IRuleFailureDetail> FailedRulesDetails { get; } // TIPO CAMBIATO

        /// <summary>
        /// Un campioncino di stringhe MIU correlate all'antitesi, per test mirati.
        /// </summary>
        public List<string> RelevantContextStrings { get; }
        // ---------------------------------------------------------------------------------

        protected MiuAbstractPattern(string name, string description,
                                     List<IRuleFailureDetail> failedRulesDetails = null, // TIPO CAMBIATO NEL COSTRUTTORE
                                     List<string> relevantContextStrings = null)
        {
            ID = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IdentifiedAt = DateTime.UtcNow;

            FailedRulesDetails = failedRulesDetails ?? new List<IRuleFailureDetail>();
            RelevantContextStrings = relevantContextStrings ?? new List<string>();
        }

        /// <summary>
        /// Restituisce un riepilogo diagnostico del pattern di antitesi.
        /// </summary>
        /// <returns>Una stringa che riassume l'antitesi.</returns>
        public abstract string GetDiagnosticSummary();
    }

    /// <summary>
    /// Rappresenta un gap (una stringa o uno stato non raggiungibile) nel campo di esistenza MIU.
    /// </summary>
    public class GapAntithesis : MiuAbstractPattern
    {
        public string SourceString { get; }
        public string TargetString { get; }

        public GapAntithesis(string sourceString, string targetString,
                             List<IRuleFailureDetail> failedRulesDetails = null, // TIPO CAMBIATO NEL COSTRUTTORE
                             List<string> relevantContextStrings = null)
            : base($"Gap: {sourceString} -> {targetString}",
                   $"Il tentativo di derivare '{sourceString}' in '{targetString}' è fallito.",
                   failedRulesDetails, relevantContextStrings)
        {
            SourceString = sourceString ?? throw new ArgumentNullException(nameof(sourceString));
            TargetString = targetString ?? throw new ArgumentNullException(nameof(targetString));
        }

        public override string GetDiagnosticSummary()
        {
            return $"GAP: '{SourceString}' non riesce a raggiungere '{TargetString}'.";
        }
    }

    /// <summary>
    /// Rappresenta un'inefficienza nel campo di esistenza MIU (es. derivazioni troppo lunghe, loop infiniti).
    /// </summary>
    public class InefficiencyAntithesis : MiuAbstractPattern
    {
        public string ProblematicSequence { get; }
        public string InefficiencyType { get; }

        public InefficiencyAntithesis(string problematicSequence, string inefficiencyType,
                                      List<IRuleFailureDetail> failedRulesDetails = null, // TIPO CAMBIATO NEL COSTRUTTORE
                                      List<string> relevantContextStrings = null)
            : base($"Inefficiency: {inefficiencyType} in '{problematicSequence}'",
                   $"Rilevata inefficienza di tipo '{inefficiencyType}' nella sequenza '{problematicSequence}'.",
                   failedRulesDetails, relevantContextStrings)
        {
            ProblematicSequence = problematicSequence ?? throw new ArgumentNullException(nameof(problematicSequence));
            InefficiencyType = inefficiencyType ?? throw new ArgumentNullException(nameof(inefficiencyType));
        }

        public override string GetDiagnosticSummary()
        {
            return $"INEFFICIENCY: Tipo '{InefficiencyType}' nella sequenza '{ProblematicSequence}'.";
        }
    }
}