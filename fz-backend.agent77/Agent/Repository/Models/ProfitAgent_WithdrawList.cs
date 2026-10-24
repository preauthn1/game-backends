using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ProfitAgent_WithdrawList
    {
        public long ID { get; set; }
        public int UserID { get; set; }
        public long WithDrawAmount { get; set; }
        public int WithDrawRate { get; set; }
        public string  Status { get; set; }
        public int StatusNo { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? Updatetime { get; set; }
        public string Account { get; set; }
        public string LoginName { get; set; }
        public string UnionpayRealName { get; set; }
        public string UnionpayBank { get; set; }
        public string BankName { get; set; }
        public string UnionpayAccount { get; set; }
    }
}
