using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutiveSystem.Common
{
    public class MIURuleApplication
    {
        public long SearchID { get; set; }
        public long ParentStateID { get; set; }
        public long NewStateID { get; set; }
        public long AppliedRuleID { get; set; }
        public int CurrentDepth { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
