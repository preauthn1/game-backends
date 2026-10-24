using System;

namespace Repository.Entity
{
    public class agent_recharge_total
    {
        public int id { get; set; }
        public int manage_id { get; set; }
        public int total_agent_id { get; set; }
        public int num { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public int old_money { get; set; }
        public int new_money { get; set; }
        public string ip { get; set; }
        public string area { get; set; }
        public string device { get; set; }
        public string remark { get; set; }
    }
}
