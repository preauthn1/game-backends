using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserAgent_GetWithdrawList
    {
        public DateTime LogDate { get; set; }
        public string Status { get; set; }
        public long WithdrawAmount { get; set; }
        public decimal WithdrawAmountCash
        {
            get
            {
                return Math.Round(((decimal)WithdrawAmount / 100), 2);
            }
        }
    }
}
