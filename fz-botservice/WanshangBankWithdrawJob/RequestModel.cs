using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WanshangBankWithdrawJob
{
    public class RequestModel
    {
        public string return_code { get; set; }
        public string return_msg { get; set; }
        public string trade_state_desc { get; set; }
        public string mineSign { get; set; }
        public string transaction_id { get; set; }
    }
}
