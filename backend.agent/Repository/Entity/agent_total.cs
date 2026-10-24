using System;

namespace Repository.Entity
{
    public class agent_total
    {
        public int id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string encrypt { get; set; }
        public int money { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public byte status { get; set; }
        public long telphone { get; set; }
        public byte telphone_bind { get; set; }
        public byte telphone_login { get; set; }
        public string ip { get; set; }
        public byte ip_bind { get; set; }
        public int total_turn_out { get; set; }
        public int total_recharge { get; set; }
    }
}
