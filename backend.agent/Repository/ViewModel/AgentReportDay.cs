using System;

namespace Repository.ViewModel
{
    public class AgentReportDay
    {
        public int id { get; set; }
        public int agent_id { get; set; }
        public string name { get; set; }
        public int recharge { get; set; }
        public DateTime? day { get; set; }
        public int? circulate { get; set; }
        public int? sale { get; set; }
        public int? channel { get; set; }
        public decimal? profit { get; set; }
        public string remark { get; set; }
    }
}
