using System;
using System.ComponentModel;

namespace Repository.ViewModel
{
    public class AgentMoneyUser
    {
        public int id { get; set; }
        public int agent_id { get; set; }
        public AgentMoneyType type { get; set; }
        public int user_id { get; set; }
        public int money { get; set; }
        public int new_money { get; set; }
        public DateTime? created_at { get; set; }
        public string ip { get; set; }
        public string area { get; set; }
        public byte status { get; set; }
        public string remark { get; set; }
    }


    public enum AgentMoneyType : byte
    {
        [Description("转出")]
        TransferOut = 0,
        [Description("转入")]
        transferIn = 1,
        [Description("购入")]
        purchase = 2,
        [Description("兑换")]
        exchange = 3,
        [Description("回调")]
        callback = 4,
    }
}
