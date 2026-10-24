using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunyunBankWithdrawJob
{
    [Serializable]
    public class RequestModel
    {
        public RequestModel()
        {
            payload = new RequestData();
        }
        public string method { get; set; }
        public RequestData payload { get; set; }
        public string module { get; set; }
        public string company_name { get; set; } 
    }

    [Serializable]
    public class RequestData
    {
        public string card_number { get; set; }
        public string amount { get; set; }
        public string trans_mode { get; set; }
        public string order_number { get; set; }
        public string real_name { get; set; }      
    }
}
