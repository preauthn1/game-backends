using System;

namespace Repository.Entity
{
    public class agent_money_user
    {
        public int id { get; set; }
        public int agent_id { get; set; }
        public byte type { get; set; }
        public int user_id { get; set; }
        public int money { get; set; }
        public int new_money { get; set; }
        public DateTime? created_at { get; set; }
        public string ip { get; set; }
        public string area { get; set; }
        public byte status { get; set; }
        public string remark { get; set; }
    }
}
