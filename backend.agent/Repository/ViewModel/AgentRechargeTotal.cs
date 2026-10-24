using System;

namespace Repository.ViewModel
{
    public class AgentRechargeTotal
    {
        public int id { get; set; }
        public int total_agent_id { get; set; }
        public int num { get; set; }
        public DateTime created_at { get; set; }
        public string remark { get; set; }
        public string username { get; set; }
    }
}
