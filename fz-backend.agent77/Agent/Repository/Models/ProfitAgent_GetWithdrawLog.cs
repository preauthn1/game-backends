using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ProfitAgent_GetWithdrawLog
    {     
        public int UserID { get; set; }
        public string CreateTime { get; set; }
        public long WithDrawAmount { get; set; }
        public string Status { get; set; }
        public string Memo { get; set; }
    }
}
