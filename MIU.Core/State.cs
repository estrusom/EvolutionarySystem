// creata 10.6.2025 10.07
using System;
using System.Collections.Generic;
using MIU.Core; // Assicurati che questo namespace sia corretto per MIURuleSet e RegolaMIU
using MIU.Core.Rules; // Contiene IRegolaMIU
using MIU.Core.Topology.Map;

namespace MIU.Core.Topology.Map // Namespace per la classe State
{
    /// <summary>
    /// Rappresenta uno "stato" all'interno della mappa topologica, derivante dall'applicazione di regole.
    /// Ogni stato è identificato univocamente dalla sua stringa MIU (teorema) e contiene metadati
    /// relativi alla sua generazione e alle regole che possono essere applicate da esso.
    /// </summary>
    public class State
    {
        /// <summary>
        /// Identificatore unico dello stato, basato sulla stringa del teorema MIU.
        /// Questo è il Teorema correntemente rappresentato dallo stato.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// La stringa MIU (teorema) che questo stato rappresenta.
        /// </summary>
        public string MiuString { get; private set; }

        /// <summary>
        /// Il livello di profondità raggiunto nell'esplorazione dell'albero di derivazione.
        /// Indica quante regole sono state applicate per raggiungere questo stato dal Teorema Iniziale (MI).
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// L'ID dello stato precedente da cui questo stato è stato generato.
        /// Null se è lo stato iniziale.
        /// </summary>
        public string ParentStateId { get; private set; }

        /// <summary>
        /// L'ID della regola specifica che è stata applicata per generare questo stato dal ParentState.
        /// Null se è lo stato iniziale.
        /// </summary>
        public string RuleAppliedFromParentId { get; private set; }

        /// <summary>
        /// Lista delle regole MIU (R1, R2, R3, R4) che possono essere applicate **DA** questo stato.
        /// Ogni elemento è una coppia: <IRegolaMIU, TeoremaRisultante>.
        /// Questa lista rappresenta i potenziali "archi in uscita" da questo stato.
        /// </summary>
        // *** MODIFICA CRUCIALE QUI: ABBIAMO CAMBIATO RegolaMIU IN IRegolaMIU ***
        public List<(IRegolaMIU Rule, string ResultingTheorem)> PotentialOutgoingRules { get; private set; }

        /// <summary>
        /// Unix timestamp: quando questo stato è stato scoperto/generato.
        /// </summary>
        public long DiscoveredTimestamp { get; private set; }

        /// <summary>
        /// Indica se lo stato è un Teorema I.
        /// </summary>
        public bool IsTheoremI { get; private set; }

        /// <summary>
        /// Contatore che indica quante volte questo stato è stato visitato/processato.
        /// </summary>
        public int VisitCount { get; set; } // Aggiunto per tracciare le visite

        /// <summary>
        /// Etichetta descrittiva dello stato, utile per il debug o la visualizzazione.
        /// Per semplicità, usa la MiuString come etichetta.
        /// </summary>
        public string Label => MiuString; // Aggiunto come etichetta dello stato

        /// <summary>
        /// Costruttore per inizializzare un nuovo stato.
        /// </summary>
        /// <param name="miuString">La stringa MIU (teorema) rappresentata da questo stato.</param>
        /// <param name="depth">La profondità dello stato nell'albero di derivazione.</param>
        /// <param name="parentStateId">L'ID dello stato genitore (null per lo stato iniziale).</param>
        /// <param name="ruleAppliedFromParentId">L'ID della regola applicata dal genitore (null per lo stato iniziale).</param>
        public State(string miuString, int depth, string parentStateId = null, string ruleAppliedFromParentId = null)
        {
            if (string.IsNullOrEmpty(miuString))
                throw new ArgumentNullException(nameof(miuString), "La stringa MIU non può essere null o vuota.");

            Id = GenerateStateId(miuString); // L'ID è generato dalla stringa MIU
            MiuString = miuString;
            Depth = depth;
            ParentStateId = parentStateId;
            RuleAppliedFromParentId = ruleAppliedFromParentId;
            // *** MODIFICA QUI: LA LISTA VIENE INIZIALIZZATA CON IRegolaMIU ***
            PotentialOutgoingRules = new List<(IRegolaMIU Rule, string ResultingTheorem)>();
            DiscoveredTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            IsTheoremI = (miuString.EndsWith("I") && miuString != "MI"); // Un Teorema I è un teorema che finisce con 'I' ma non è 'MI'
        }

        /// <summary>
        /// Genera un ID univoco per lo stato basato sulla sua stringa MIU.
        /// Potrebbe essere un hash della stringa MIU per garantirne l'unicità.
        /// </summary>
        /// <param name="miuString">La stringa MIU dello stato.</param>
        /// <returns>L'ID univoco dello stato.</returns>
        private string GenerateStateId(string miuString)
        {
            // Potrebbe essere un hash SHA256 della stringa, ma per semplicità useremo la stringa stessa
            // o un hash semplice per evitare stringhe troppo lunghe come ID.
            // Per scopi di debug e tracciabilità, usare la stringa intera (o un suo hash) è preferibile.
            return miuString; // Per ora, usiamo la stringa stessa come ID
        }

        /// <summary>
        /// Calcola e aggiunge le regole MIU che possono essere applicate da questo stato.
        /// </summary>
        /// <param name="miuRuleSet">L'insieme di tutte le regole MIU disponibili (R1, R2, R3, R4).</param>
        public void CalculatePotentialOutgoingRules(MIURuleSet miuRuleSet)
        {
            if (miuRuleSet == null)
            {
                Console.WriteLine("Avviso: MIURuleSet è null. Impossibile calcolare le regole in uscita.");
                return;
            }

            foreach (var rule in miuRuleSet.Rules)
            {
                // Qui, 'rule' è del tipo degli elementi in miuRuleSet.Rules.
                // Se miuRuleSet.Rules è IEnumerable<IRegolaMIU>, allora 'rule' sarà IRegolaMIU.
                // Se miuRuleSet.Rules è IEnumerable<RegolaMIU>, allora 'rule' sarà RegolaMIU.
                // In entrambi i casi, la dichiarazione della lista come List<(IRegolaMIU Rule, string ResultingTheorem)>
                // è più flessibile e permette di aggiungere sia IRegolaMIU che RegolaMIU (se RegolaMIU implementa IRegolaMIU).
                if (rule.IsApplicable(MiuString))
                {
                    string result = rule.Apply(MiuString);
                    // Ora, l'aggiunta della tupla è consistente con la dichiarazione della lista.
                    // Puoi usare la sintassi semplificata per le tuple:
                    PotentialOutgoingRules.Add((rule, result));
                    // O la sintassi esplicita se preferisci (ma non è più strettamente necessaria per questo errore):
                    // PotentialOutgoingRules.Add(new ValueTuple<IRegolaMIU, string>(rule, result));
                }
            }
        }

        // Sovrascrivi Equals e GetHashCode per consentire un confronto corretto
        // e l'uso in collezioni, basato sull'Id (che è la MiuString).
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            State other = (State)obj;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"Stato: {MiuString} (Profondità: {Depth})";
        }
    }
}
