using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserAgent_GetAgentTaxLogTotal
    {
        public long totalTaxAmount { get; set; }
        public long totalTaxBonus { get; set; }
        public int temp_bpeople { get; set; }
    }
}
