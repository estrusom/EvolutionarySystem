using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MIU.Core
{
    // <summary>
    /// Rappresenta una singola regola MIU.
    /// </summary>
    public class RegolaMIU
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Descrizione { get; set; }
        public string Pattern { get; set; }
        public string Sostituzione { get; set; }

        public RegolaMIU() { }
        public RegolaMIU(string id, string nome, string descrizione, string pattern, string sostituzione)
        {
            Id = id;
            Nome = nome;
            Descrizione = descrizione;
            Pattern = pattern;
            Sostituzione = sostituzione;
        }
    }
}
