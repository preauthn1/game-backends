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
        public decimal TotalTaxAmountCash
        {
            get
            {
                return Math.Round(((decimal)TotalTaxAmount / 100), 2);
            }
        }
        public decimal ConvertAmountCash
        {
            get
            {
                return Math.Round(((decimal)ConvertAmount / 100),2);
            }
        }
        public decimal PerformedAmountCash
        {
            get
            {
                return Math.Round(((decimal)PerformedAmount / 100), 2);
            }
        }
    }
}
