using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserAgent_GetWithdrawListTotal
    {
        public long TotalTaxAmount { get; set; }
        public long ConvertAmount { get; set; }
        public long PerformedAmount { get; set; }
    }
}
