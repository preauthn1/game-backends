using System;

namespace Repository.Entity
{
    public class agent_login_total
    {
        public int id { get; set; }
        public int agent_id { get; set; }
        public string remark { get; set; }
        public DateTime? created_at { get; set; }
        public string ip { get; set; }
        public string area { get; set; }
    }
}
