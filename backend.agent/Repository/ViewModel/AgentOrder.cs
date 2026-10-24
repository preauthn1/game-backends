
using System;

namespace Repository.ViewModel
{
    public class AgentOrder
    {
        public int id { get; set; }
        public int money { get; set; }
        public int agent_id { get; set; }
        public int user_id { get; set; }
        public int balance { get; set; }
        public DateTime? created_at { get; set; }
        public byte type { get; set; }
    }
}
