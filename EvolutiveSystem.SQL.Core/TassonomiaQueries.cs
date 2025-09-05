// File: TassonomiaQueries.cs
// Questo file contiene le stringhe di query SQL statiche e riutilizzabili.
// È la soluzione standard per .NET Framework 4.8, dove i membri statici
// non sono consentiti nelle interfacce.

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Classe statica per centralizzare tutte le query SQL relative alla tassonomia.
    /// </summary>
    public static class TassonomiaQueries
    {
        /// <summary>
        /// Query SQL per l'analisi della tassonomia del sistema MIU.
        /// Questa query unisce dati da diverse tabelle per fornire un report dettagliato
        /// sull'evoluzione degli stati e sulle regole applicate.
        /// </summary>
        public const string TassonomiaQuery = @"
            SELECT
                A.NewStateID AS IdStato,
                H.MIUString AS ContenutoStato,
                GROUP_CONCAT(R.Descrizione) AS RegoleApplicate,
                P.PathStepID,
                P.StepNumber,
                P.ParentStateID,
                P.AppliedRuleID,
                P.IsTarget,
                P.IsSuccess
            FROM MIU_RuleApplications AS A
            JOIN RegoleMIU AS R ON A.AppliedRuleID = R.ID
            JOIN MIU_States_History AS H ON A.NewStateID = H.Id
            LEFT JOIN MIU_Paths AS P ON A.NewStateID = P.StateID
            GROUP BY
                A.NewStateID, P.PathStepID
            ORDER BY
                P.StepNumber;
        ";
    }
}
