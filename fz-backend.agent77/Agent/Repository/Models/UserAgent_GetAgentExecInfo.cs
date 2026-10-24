using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserAgent_GetAgentExecInfo
    {
        public long AgentAmount { get; set; }
        public long AgentUserAmount { get; set; }
        public int AgentLevel { get; set; }
        public long AgentTaxAmount { get; set; }
        public long AgentTaxBonus { get; set; }
        public long AgentTotalUser { get; set; }
        public long AgentAgentUser { get; set; }
        public long WithDrawAmount { get; set; }

        public long Rate
        {
            get
            {
                if (AgentAmount == 0)
                    return 0;

                return AgentUserAmount / AgentAmount * 100;
            }
        }
    }
}
