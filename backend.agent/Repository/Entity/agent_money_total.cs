using System;
using System.ComponentModel;

namespace Repository.Entity
{
    public class agent_money_total
    {
        public int id { get; set; }
        public int total_agent_id { get; set; }
        public transferType type { get; set; }
        public int user_agent_id { get; set; }
        public int money { get; set; }
        public int old_money { get; set; }
        public int new_money { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public string ip { get; set; }
        public string area { get; set; }
        public deviceType device { get; set; }
        public string remark { get; set; }
    }


    public enum deviceType : byte
    {
        [Description("PC")]
        PC = 1,
        [Description("手机")]
        Mobile = 2,
    }

    public enum transferType : byte
    {
        [Description("转入")]
        transferIn = 0,
        [Description("转出")]
        transferOut = 1,
        [Description("充值")]
        recharge = 2,
        [Description("回调")]
        callback = 4,
    }
}
