using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ProfitAgent_GetWithdrawDetail
    {
        public string LogDate { get; set; }
        public int UserID { get; set; }
        public long ChargeAmount { get; set; }
        public long ConvertAmount { get; set; }
        public long StorageAmount { get; set; }
        public long BonusAmount { get; set; }
    }
}
