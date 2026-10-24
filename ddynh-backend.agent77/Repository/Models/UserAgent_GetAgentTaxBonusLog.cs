using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserAgent_GetAgentTaxBonusLog
    {
        public DateTime LogDate { get; set; }
        public int UserID { get; set; }
        public long Amount { get; set; }
        public decimal AmountCash
        {
            get
            {
                return Math.Round(((decimal)Amount / 100), 2);
            }
        }
    }
}
