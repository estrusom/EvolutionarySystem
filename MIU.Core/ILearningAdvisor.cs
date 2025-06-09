using System.Collections.Generic;
using MIU.Core; // Assicurati che RegolaMIU sia accessibile da qui

namespace MIU.Core.Learning.Interfaces
{
    /// <summary>
    /// Interfaccia per un consigliere di apprendimento che suggerisce l'ordine delle regole.
    /// </summary>
    public interface ILearningAdvisor
    {
        /// <summary>
        /// Inizializza il consigliere di apprendimento, caricando lo stato precedente.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Ottiene un elenco ordinato di regole preferite per una data stringa compressa e profondità.
        /// </summary>
        /// <param name="currentCompressedString">La stringa compressa attuale.</param>
        /// <param name="currentDepth">La profondità attuale nella ricerca.</param>
        /// <returns>Un elenco ordinato di oggetti RegolaMIU.</returns>
        List<RegolaMIU> GetPreferredRuleOrder(string currentCompressedString, int currentDepth);

        /// <summary>
        /// Notifica al consigliere di apprendimento che una regola è stata applicata.
        /// </summary>
        /// <param name="parentStringCompressed">La stringa compressa di partenza.</param>
        /// <param name="newStringCompressed">La stringa compressa risultante.</param>
        /// <param name="appliedRuleID">L'ID della regola applicata.</param>
        /// <param name="currentDepth">La profondità corrente.</param>
        /// <param name="isSuccessPath">Indica se questa applicazione fa parte di un percorso che ha portato a successo.</param>
        void NotifyRuleApplied(string parentStringCompressed, string newStringCompressed, int appliedRuleID, int currentDepth, bool isSuccessPath);

        // --- AGGIUNGI ESATTAMENTE QUESTO METODO QUI ALL'INTERFACCIA ---
        /// <summary>
        /// Determina se una specifica applicazione di regola dovrebbe essere persistita nel database
        /// per l'analisi dettagliata.
        /// </summary>
        /// <param name="parentString">La stringa compressa del genitore.</param>
        /// <param name="newString">La stringa compressa generata.</param>
        /// <param name="appliedRuleId">L'ID della regola applicata.</param>
        /// <param name="currentDepth">La profondità corrente dell'applicazione.</param>
        /// <returns>True se l'applicazione dovrebbe essere persistita, false altrimenti.</returns>
        bool ShouldPersistRuleApplication(string parentString, string newString, int appliedRuleId, int currentDepth);

        /// <summary>
        /// Deinizializza il consigliere di apprendimento, salvando lo stato corrente.
        /// </summary>
        void Deinitialize();

        /// <summary>
        /// Ottiene un riepilogo testuale delle statistiche di apprendimento.
        /// </summary>
        /// <returns>Una stringa riassuntiva delle statistiche.</returns>
        string GetStatisticsSummary();
    }
}
